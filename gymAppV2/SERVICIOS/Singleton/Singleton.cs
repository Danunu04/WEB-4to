using System;
using System.Web;

namespace Servicios.Singleton
{
    public class Singleton
    {
        private const string SESSION_KEY = "SesionUsuario_Instancia";

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
                catch (HttpException ex)
                {
                    // Session puede haber expirado o no estar disponible
                    throw new Exception("Error al acceder a la sesión HTTP: " + ex.Message, ex);
                }
                catch (Exception ex)
                {
                    throw new Exception("Error inesperado al obtener instancia de sesión: " + ex.Message, ex);
                }
            }
        }
    }
}