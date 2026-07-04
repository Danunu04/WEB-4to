using System;
using System.Web;

namespace Servicios.Singleton
{
    /// <summary>
    /// Singleton de sesión para ASP.NET Web Forms.
    /// Garantiza una única instancia de SesionUsuario por sesión HTTP.
    /// </summary>
    public class Singleton
    {
        private const string SESSION_KEY = "SesionUsuario_Instancia";

        /// <summary>
        /// Devuelve la instancia de SesionUsuario asociada a la sesión HTTP actual.
        /// Si no hay sesión disponible, devuelve null en lugar de propagar una excepción técnica.
        /// </summary>
        public static SesionUsuario Instancia
        {
            get
            {
                try
                {
                    if (HttpContext.Current?.Session == null)
                        return null;

                    if (HttpContext.Current.Session[SESSION_KEY] == null)
                        HttpContext.Current.Session[SESSION_KEY] = new SesionUsuario();

                    return (SesionUsuario)HttpContext.Current.Session[SESSION_KEY];
                }
                catch (HttpException)
                {
                    // Session no disponible (expirada o contexto inválido).
                    return null;
                }
                catch (Exception)
                {
                    // Cualquier otro error inesperado: no exponer al usuario.
                    return null;
                }
            }
        }
    }
}
