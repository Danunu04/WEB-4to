using System;
using BE;
using MPP;

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

        private void RegistrarEvento(string tipo, string accion)
        {
            try
            {
                var usuario = System.Web.HttpContext.Current?.Session["UsuarioLogueado"] as Usuario;
                string usr = usuario?.USUARIO_Usuario ?? "sistema";
                bllEvento.RegistrarEvento(tipo, usr, accion);
            }
            catch
            {
                // No impedir la operación principal si falla el log
            }
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
                RegistrarEvento("cambio_rol", $"Rol del usuario '{usuario}' actualizado a {rol}");
            }
            catch (Exception ex)
            {
                throw new Exception("Error al actualizar el rol: " + ex.Message, ex);
            }
        }

        public bool TieneAccesoAModulo(int rol, string modulo)
        {
            // 1=Administrador | 2=Recepcionista | 3=Entrenador | 4=Cliente
            switch (modulo)
            {
                case "Dashboard":
                    return true;

                case "GestionAlumnos":
                case "GestionUsuarios":
                case "GestionEntrenadores":
                case "CrearFamiliaPerfil":
                case "Bitacora":
                case "PreciosCuota":
                    return rol <= 2;

                case "ActividadesCalendario":
                case "Pagos":
                    return rol != 3;

                case "GestionRutinas":
                    return rol <= 3;

                case "GestionClases":
                    return true;

                case "Perfil":
                    return rol == 4;

                default:
                    return false;
            }
        }
    }
}