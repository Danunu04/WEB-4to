using System;
using BE;
using MPP;

namespace BLL
{
    public class BLLRol
    {
        private MPPRol mppRol;

        public BLLRol()
        {
            mppRol = new MPPRol();
        }

        public Rol ObtenerRol(string usuario)
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

        public void ActualizarRol(string usuario, Rol rol)
        {
            try
            {
                mppRol.ActualizarRol(usuario, rol);
            }
            catch (Exception ex)
            {
                throw new Exception("Error al actualizar el rol: " + ex.Message, ex);
            }
        }

        public bool TieneAccesoAModulo(Rol rol, string modulo)
        {
            // Implement access control based on LogicaNegocio.txt Section 3
            switch (modulo)
            {
                case "Dashboard":
                    return true; // All roles have access

                case "GestionAlumnos":
                case "GestionUsuarios":
                case "GestionEntrenadores":
                case "CrearFamiliaPerfil":
                case "Bitacora":
                case "PreciosCuota":
                    return rol == Rol.Administrador || rol == Rol.Recepcionista;

                case "ActividadesCalendario":
                    return rol == Rol.Administrador || rol == Rol.Recepcionista || rol == Rol.Cliente;

                case "GestionRutinas":
                    return rol == Rol.Administrador || rol == Rol.Recepcionista || rol == Rol.Entrenador;

                case "GestionClases":
                    return rol == Rol.Administrador || rol == Rol.Recepcionista;

                case "Pagos":
                    return rol == Rol.Administrador || rol == Rol.Recepcionista || rol == Rol.Cliente;

                case "Perfil":
                    return rol == Rol.Cliente;

                default:
                    return false;
            }
        }
    }
}