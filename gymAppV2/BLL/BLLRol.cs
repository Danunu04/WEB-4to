using System;
using BE;
using MPP;
using Servicios.Singleton;

namespace BLL
{
    public class BLLRol
    {
        private MPPRol mppRol;
        private BLLEvento bllEvento;

        public BLLRol()
        {
            mppRol = new MPPRol();
            bllEvento = new BLLEvento();
        }

        public int ObtenerRol(string usuario)
        {
            try
            {
                return mppRol.ObtenerRol(usuario);
            }
            catch (Exception ex)
            {
                throw new Exception("Error al obtener el rol: " + ex.Message, ex);
            }
        }

        public void ActualizarRol(string usuario, int rol)
        {
            try
            {
                mppRol.ActualizarRol(usuario, rol);
                bllEvento.RegistrarCambioRol(usuario, rol);
            }
            catch (Exception ex)
            {
                throw new Exception("Error al actualizar el rol: " + ex.Message, ex);
            }
        }

        /// <summary>
        /// Devuelve true si el rol tiene acceso al módulo indicado.
        /// 1=Administrador | 2=Recepcionista | 3=Entrenador | 4=Cliente
        /// </summary>
        public bool TieneAccesoAModulo(int rol, string modulo)
        {
            switch (modulo)
            {
                case "Dashboard":
                    return true;

                case "GestionUsuarios":
                case "GestionEntrenadores":
                case "Bitacora":
                case "PreciosCuota":
                    return rol <= 2;

                case "GestionAlumnos":
                    return rol <= 2 || rol == 4;

                case "ActividadesCalendario":
                case "Pagos":
                    return rol != 3;

                case "GestionRutinas":
                    return rol <= 3;

                case "Perfil":
                    return rol == 4;

                case "VerificacionDV":
                    return rol == 1;

                default:
                    return false;
            }
        }

        /// <summary>
        /// Verifica si el usuario logueado actualmente tiene acceso al módulo indicado.
        /// </summary>
        public bool UsuarioActualTieneAcceso(string modulo)
        {
            var usuario = Singleton.Instancia.Usuario;
            if (usuario == null)
                return false;

            return TieneAccesoAModulo(usuario.USUARIO_Rol, modulo);
        }
    }
}