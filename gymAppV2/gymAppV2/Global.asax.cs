using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Optimization;
using System.Web.Routing;
using System.Web.Security;
using System.Web.SessionState;
using BLL;
using Servicios.Singleton;

namespace gymAppV2
{
    public class Global : HttpApplication
    {
        /// <summary>
        /// Se ejecuta una sola vez al iniciar la aplicación.
        /// Registra rutas/bundles y ejecuta un health check de seguridad sin bloquear el arranque.
        /// </summary>
        void Application_Start(object sender, EventArgs e)
        {
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            BundleConfig.RegisterBundles(BundleTable.Bundles);

            EjecutarHealthCheckInicial();
        }

        /// <summary>
        /// Verifica que la tabla de control de dígitos verificadores exista y tenga registros.
        /// Si falla, se deja una traza en el trace para diagnóstico; no se detiene el arranque
        /// para no impedir el primer despliegue, pero se alerta al administrador vía bitácora.
        /// </summary>
        private void EjecutarHealthCheckInicial()
        {
            try
            {
                var bllDV = new BLLDigitoVerificador();
                var estado = bllDV.ObtenerEstadoControl();

                if (estado == null || estado.Count == 0)
                {
                    RegistrarEventoSeguridad(
                        "HealthCheck",
                        "sistema",
                        "No se encontraron tablas registradas en DigitoVerificador. Ejecutar InicializarControl.",
                        2);
                }
            }
            catch (Exception ex)
            {
                RegistrarEventoSeguridad(
                    "HealthCheck",
                    "sistema",
                    $"Error en health check de integridad: {ex.Message}",
                    3);
            }
        }

        /// <summary>
        /// Captura errores no controlados a nivel de aplicación.
        /// Registra el error en bitácora y redirige a una página amigable según el tipo de error,
        /// sin exponer detalles técnicos al usuario final.
        /// </summary>
        void Application_Error(object sender, EventArgs e)
        {
            Exception ex = Server.GetLastError();
            if (ex == null)
                return;

            // No exponer mensajes técnicos. Registrar en bitácora si es posible.
            try
            {
                string usuario = "sistema";
                try
                {
                    var sesion = Singleton.Instancia;
                    if (sesion != null && sesion.Usuario != null)
                        usuario = sesion.Usuario.USUARIO_Usuario;
                }
                catch
                {
                    // Si la sesión falla, seguimos con "sistema".
                }

                var bllEvento = new BLLEvento();
                bllEvento.RegistrarError(usuario, $"Error no controlado: {ex.Message}");
            }
            catch
            {
                // Fallback a trace si la bitácora también falla.
                System.Diagnostics.Trace.WriteLine($"[ERROR GLOBAL] {ex}");
            }

            // Limpiar el error para evitar que ASP.NET muestre la página amarilla.
            Server.ClearError();

            // Redirigir a página amigable si el contexto lo permite.
            try
            {
                if (Response != null && !Response.HeadersWritten)
                {
                    Response.Redirect("~/AccesoDenegado.aspx", false);
                    Context.ApplicationInstance.CompleteRequest();
                }
            }
            catch
            {
                // Si la redirección falla, dejar que ASP.NET use customErrors.
            }
        }

        /// <summary>
        /// Intenta registrar un evento de seguridad sin romper el flujo.
        /// </summary>
        private void RegistrarEventoSeguridad(string tipo, string usuario, string accion, int criticidad)
        {
            try
            {
                var bllEvento = new BLLEvento();
                bllEvento.RegistrarEvento(tipo, usuario, accion, criticidad, "Seguridad");
            }
            catch
            {
                System.Diagnostics.Trace.WriteLine($"[SEGURIDAD] {tipo}: {accion}");
            }
        }
    }
}
