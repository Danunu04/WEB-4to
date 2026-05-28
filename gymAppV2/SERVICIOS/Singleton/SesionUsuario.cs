using System;
using System.Web;
using BE;

namespace Servicios.Singleton
{
    public class SesionUsuario
    {
        private const string SESSION_KEY = "UsuarioLogueado";

        public Usuario Usuario
        {
            get
            {
                return HttpContext.Current.Session[SESSION_KEY] as Usuario;
            }
        }

        public void LogIn(Usuario usuario)
        {
            try
            {
                HttpContext.Current.Session[SESSION_KEY] = usuario;
            }
            catch (HttpException ex)
            {
                throw new Exception("Error al iniciar sesión HTTP: " + ex.Message, ex);
            }
            catch (Exception ex)
            {
                throw new Exception("Error inesperado al guardar usuario en sesión: " + ex.Message, ex);
            }
        }

        public void LogOut()
        {
            try
            {
                HttpContext.Current.Session[SESSION_KEY] = null;
                HttpContext.Current.Session.Abandon();
            }
            catch (HttpException ex)
            {
                throw new Exception("Error al cerrar sesión HTTP: " + ex.Message, ex);
            }
            catch (Exception ex)
            {
                throw new Exception("Error inesperado al cerrar sesión: " + ex.Message, ex);
            }
        }

        public bool IsLogged()
        {
            try
            {
                return HttpContext.Current.Session[SESSION_KEY] != null;
            }
            catch (HttpException)
            {
                // Session no disponible
                return false;
            }
            catch (Exception)
            {
                return false;
            }
        }
    }
}