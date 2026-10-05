using System;
using System.Collections.Generic;
using System.Configuration;
using System.Net;
using System.Web;
using System.Web.Security;
using System.Web.Services;
using System.Web.Services.Protocols;
using BE;

namespace gymAppV2.WebServices
{
    /// <summary>
    /// Proxy SOAP para consumir AlumnosWS.asmx por HTTP (equivalente a lo que genera
    /// "Agregar referencia web" / wsdl.exe, pero reutilizando las clases de BE).
    /// Las páginas usan esta clase en lugar de llamar directo a BLLAlumno.
    /// </summary>
    [WebServiceBinding(Name = "AlumnosWSSoap", Namespace = AlumnosWS.NAMESPACE)]
    public class AlumnosWSCliente : SoapHttpClientProtocol
    {
        /// <param name="url">URL absoluta de AlumnosWS.asmx.</param>
        public AlumnosWSCliente(string url)
        {
            Url = url;
            CookieContainer = new CookieContainer();
        }

        [SoapDocumentMethod(AlumnosWS.NAMESPACE + "/ListarAlumnos",
            RequestNamespace = AlumnosWS.NAMESPACE, ResponseNamespace = AlumnosWS.NAMESPACE)]
        public List<Alumno> ListarAlumnos()
        {
            object[] resultado = Invoke("ListarAlumnos", new object[0]);
            return (List<Alumno>)resultado[0];
        }

        [SoapDocumentMethod(AlumnosWS.NAMESPACE + "/ObtenerMisAlumnos",
            RequestNamespace = AlumnosWS.NAMESPACE, ResponseNamespace = AlumnosWS.NAMESPACE)]
        public List<Alumno> ObtenerMisAlumnos()
        {
            object[] resultado = Invoke("ObtenerMisAlumnos", new object[0]);
            return (List<Alumno>)resultado[0];
        }

        /// <summary>
        /// Crea el proxy apuntando a la URL de appSettings["AlumnosWS_Url"] o, si no está
        /// configurada, al AlumnosWS.asmx de este mismo sitio. Reenvía la cookie de login
        /// (Forms Authentication) del usuario actual para que el servicio sepa quién consulta.
        /// Solo se reenvía esa cookie, no la de Session (ver comentario en AlumnosWS).
        /// </summary>
        public static AlumnosWSCliente Crear(System.Web.UI.Page pagina)
        {
            string url = ConfigurationManager.AppSettings["AlumnosWS_Url"];
            if (string.IsNullOrEmpty(url))
            {
                Uri baseSitio = new Uri(pagina.Request.Url.GetLeftPart(UriPartial.Authority));
                url = new Uri(baseSitio, pagina.ResolveUrl("~/WebServices/AlumnosWS.asmx")).ToString();
            }

            var cliente = new AlumnosWSCliente(url);

            HttpCookie cookieLogin = pagina.Request.Cookies[FormsAuthentication.FormsCookieName];
            if (cookieLogin != null)
            {
                cliente.CookieContainer.Add(new Cookie(cookieLogin.Name, cookieLogin.Value, "/", new Uri(url).Host));
            }

            return cliente;
        }
    }
}
