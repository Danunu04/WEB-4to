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
            HttpContext.Current.Session[SESSION_KEY] = usuario;
        }

        public void LogOut()
        {
            HttpContext.Current.Session[SESSION_KEY] = null;
            HttpContext.Current.Session.Abandon();
        }

        public bool IsLogged()
        {
            return HttpContext.Current.Session[SESSION_KEY] != null;
        }
    }
}