using System;
using System.Web;
using BE;

namespace Servicios.Singleton
{
    /// <summary>
    /// Wrapper seguro de la sesión del usuario logueado.
    /// Centraliza el acceso a Session["UsuarioLogueado"] y provee métodos de login/logout.
    /// </summary>
    public class SesionUsuario
    {
        private const string SESSION_KEY = "UsuarioLogueado";

        /// <summary>
        /// Devuelve el usuario logueado o null si no hay sesión.
        /// </summary>
        public Usuario Usuario
        {
            get
            {
                if (HttpContext.Current?.Session == null)
                    return null;

                return HttpContext.Current.Session[SESSION_KEY] as Usuario;
            }
        }

        /// <summary>
        /// Guarda el usuario en sesión, invalidando cualquier sesión previa.
        /// </summary>
        public void LogIn(Usuario usuario)
        {
            if (usuario == null)
                throw new ArgumentNullException(nameof(usuario), "El usuario no puede ser nulo");

            try
            {
                HttpContext.Current.Session[SESSION_KEY] = usuario;
            }
            catch (HttpException ex)
            {
                throw new Exception("Error al iniciar sesión: la sesión HTTP no está disponible.", ex);
            }
            catch (Exception ex)
            {
                throw new Exception("Error inesperado al guardar el usuario en sesión.", ex);
            }
        }

        /// <summary>
        /// Cierra la sesión actual y destruye la cookie de forms.
        /// </summary>
        public void LogOut()
        {
            try
            {
                HttpContext.Current.Session[SESSION_KEY] = null;
                HttpContext.Current.Session.Abandon();
            }
            catch (HttpException ex)
            {
                throw new Exception("Error al cerrar sesión: la sesión HTTP no está disponible.", ex);
            }
            catch (Exception ex)
            {
                throw new Exception("Error inesperado al cerrar la sesión.", ex);
            }
        }

        /// <summary>
        /// Indica si hay un usuario logueado en la sesión actual.
        /// </summary>
        public bool IsLogged()
        {
            try
            {
                return HttpContext.Current?.Session != null
                    && HttpContext.Current.Session[SESSION_KEY] != null;
            }
            catch (HttpException)
            {
                return false;
            }
            catch (Exception)
            {
                return false;
            }
        }
    }
}
