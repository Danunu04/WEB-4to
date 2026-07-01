using System;
using System.Web.UI;
using BLL;
using Servicios.Singleton;
using System.Web;
using BE;

namespace gymAppV2
{
    /// <summary>
    /// Página base para todas las pantallas protegidas del sistema.
    /// Centraliza la verificación de login, autorización por perfil, control de integridad
    /// de datos y utilidades de UI.
    /// </summary>
    public class BasePage : System.Web.UI.Page
    {
        protected BLLRol BllRol { get; private set; }
        protected BLLDigitoVerificador BllDV { get; private set; }
        protected BLLEvento BllEvento { get; private set; }

        /// <summary>
        /// Nombre de la página de verificación de integridad para evitar redirecciones en bucle.
        /// </summary>
        private const string PAGINA_VERIFICACION_DV = "VerificacioDV/VerificacioDV.aspx";

        protected override void OnInit(EventArgs e)
        {
            base.OnInit(e);
            BllRol = new BLLRol();
            BllDV = new BLLDigitoVerificador();
            BllEvento = new BLLEvento();

            if (!Singleton.Instancia.IsLogged())
            {
                RedirigirSeguro("~/LogIn/LogIn.aspx");
                return;
            }

            VerificarIntegridadSiAplica();
        }

        /// <summary>
        /// Verifica la integridad de los datos. Si existe un error y el usuario no es
        /// administrador, redirige a la página de verificación para pausar el sistema.
        /// La página de verificación misma se excluye para evitar bucles.
        /// Cualquier fallo en la verificación se registra en bitácora.
        /// </summary>
        private void VerificarIntegridadSiAplica()
        {
            string paginaActual = Request.AppRelativeCurrentExecutionFilePath;
            if (!string.IsNullOrEmpty(paginaActual)
                && paginaActual.Replace("~/", "").Equals(PAGINA_VERIFICACION_DV, StringComparison.OrdinalIgnoreCase))
            {
                return;
            }

            try
            {
                if (BllDV.ExisteErrorIntegridad() && !BllDV.UsuarioActualEsAdmin())
                {
                    RegistrarErrorIntegridad();
                    RedirigirSeguro("~/VerificacioDV/VerificacioDV.aspx");
                }
            }
            catch (Exception ex)
            {
                // Si no se puede verificar, se asume error crítico para no continuar con datos dudosos.
                try
                {
                    BllEvento.RegistrarError(
                        Singleton.Instancia.Usuario?.USUARIO_Usuario ?? "sistema",
                        $"Fallo al verificar integridad: {ex.Message}");
                }
                catch
                {
                    // Último recurso: trace para diagnóstico.
                    System.Diagnostics.Trace.WriteLine($"[INTEGRIDAD] Error de verificación: {ex}");
                }

                if (!BllDV.UsuarioActualEsAdmin())
                {
                    RedirigirSeguro("~/VerificacioDV/VerificacioDV.aspx");
                }
            }
        }

        /// <summary>
        /// Registra en bitácora un evento crítico de error de integridad.
        /// </summary>
        private void RegistrarErrorIntegridad()
        {
            try
            {
                var usuario = Singleton.Instancia.Usuario;
                BllEvento.RegistrarEvento(
                    "error_integridad",
                    usuario?.USUARIO_Usuario ?? "sistema",
                    "Se detectó un error de integridad; el sistema fue pausado para usuarios no administradores.",
                    4, // Crítico
                    "Seguridad");
            }
            catch
            {
                // No detener la redirección de pausa si falla la bitácora.
            }
        }

        /// <summary>
        /// Verifica que el usuario logueado tenga acceso al módulo indicado.
        /// Si no tiene permisos, redirige a la página de acceso denegado.
        /// </summary>
        protected void VerificarAcceso(string modulo)
        {
            if (!BllRol.UsuarioActualTieneAcceso(modulo))
            {
                RedirigirSeguro("~/AccesoDenegado.aspx");
            }
        }

        /// <summary>
        /// Redirige de forma segura terminando la request correctamente.
        /// Evita ThreadAbortException por Response.Redirect.
        /// </summary>
        protected void RedirigirSeguro(string url)
        {
            try
            {
                Response.Redirect(ResolveUrl(url), false);
                Context.ApplicationInstance.CompleteRequest();
            }
            catch (HttpException)
            {
                // Contexto no disponible; no se puede redirigir.
            }
            catch (Exception)
            {
                // Ignorar errores menores de redirección.
            }
        }

        /// <summary>
        /// Muestra un toast global usando la función showToast definida en DashBoard.Master.
        /// </summary>
        protected void MostrarToast(string mensaje, string tipo = "info")
        {
            ScriptManager.RegisterStartupScript(
                this,
                GetType(),
                Guid.NewGuid().ToString(),
                $"if(window.showToast) window.showToast('{System.Security.SecurityElement.Escape(mensaje)}','{tipo}');",
                true);
        }

        /// <summary>
        /// Muestra un mensaje de error amigable usando el toast del dashboard.
        /// </summary>
        protected void MostrarError(string mensaje)
        {
            MostrarToast(mensaje, "error");
        }

        /// <summary>
        /// Muestra un mensaje de éxito usando el toast del dashboard.
        /// </summary>
        protected void MostrarExito(string mensaje)
        {
            MostrarToast(mensaje, "success");
        }

        /// <summary>
        /// Muestra una advertencia usando el toast del dashboard.
        /// </summary>
        protected void MostrarAdvertencia(string mensaje)
        {
            MostrarToast(mensaje, "warning");
        }
    }
}
