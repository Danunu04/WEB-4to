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

        // FriendlyUrls (RedirectMode.Permanent) elimina la extensión .aspx de las URLs,
        // por lo que Request.AppRelativeCurrentExecutionFilePath puede devolver la ruta
        // sin extensión. Todas las listas usan rutas SIN .aspx para que el match sea
        // consistente independientemente de si la URL tiene extensión o no.

        private static readonly string[] PAGINAS_EXENTAS_VERIFICACION =
        {
            "VerificacioDV/VerificacioDV",
            "Admin/BackupRestore",
        };

        private static readonly string[] PAGINAS_SOLO_CON_ERROR_INTEGRIDAD =
        {
            "VerificacioDV/VerificacioDV",
        };

        private static readonly string[] PAGINAS_MANTENIMIENTO_SISTEMA =
        {
            "Admin/EncriptarDatos",
        };

        protected override void OnInit(EventArgs e)
        {
            base.OnInit(e);
            BllRol = new BLLRol();
            BllDV = new BLLDigitoVerificador();
            BllEvento = new BLLEvento();

            var sesion = Singleton.Instancia;
            if (sesion == null || !sesion.IsLogged())
            {
                RedirigirSeguro("~/LogIn/LogIn.aspx");
                return;
            }

            VerificarIntegridadSiAplica();
            VerificarPaginasSoloErrorIntegridad();
            VerificarPaginasMantenimientoSistema();
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
                && EsPaginaEnLista(NormalizarPagina(paginaActual), PAGINAS_EXENTAS_VERIFICACION))
            {
                return;
            }

            try
            {
                if (BllDV.ExisteErrorIntegridad())
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

                RedirigirSeguro("~/VerificacioDV/VerificacioDV.aspx");
            }
        }

        /// <summary>
        /// Páginas como VerificacioDV solo tienen sentido cuando hay un error de integridad.
        /// Si no hay error, redirige al dashboard.
        /// </summary>
        private void VerificarPaginasSoloErrorIntegridad()
        {
            // En PostBack la página ya está cargada; no redirigir para que puedan
            // renderizarse mensajes de éxito tras recalcular o restaurar.
            if (IsPostBack) return;

            string paginaActual = Request.AppRelativeCurrentExecutionFilePath;
            if (string.IsNullOrEmpty(paginaActual))
                return;

            string pagina = NormalizarPagina(paginaActual);
            if (!EsPaginaEnLista(pagina, PAGINAS_SOLO_CON_ERROR_INTEGRIDAD))
                return;

            try
            {
                if (!BllDV.ExisteErrorIntegridad())
                {
                    RedirigirSeguro("~/DashBoard/WebForm1.aspx");
                }
            }
            catch
            {
                // Si no se puede verificar, se permite el acceso para no bloquear al admin.
            }
        }

        /// <summary>
        /// Páginas de mantenimiento del sistema (ej. encriptación masiva) no se muestran
        /// en el menú y no deben accederse en navegación normal. Se redirige al dashboard.
        /// </summary>
        private void VerificarPaginasMantenimientoSistema()
        {
            string paginaActual = Request.AppRelativeCurrentExecutionFilePath;
            if (string.IsNullOrEmpty(paginaActual))
                return;

            string pagina = NormalizarPagina(paginaActual);
            if (EsPaginaEnLista(pagina, PAGINAS_MANTENIMIENTO_SISTEMA))
            {
                RedirigirSeguro("~/DashBoard/WebForm1.aspx");
            }
        }

        /// <summary>
        /// Normaliza el path de página: quita el prefijo ~/ y la extensión .aspx.
        /// Necesario porque FriendlyUrls (RedirectMode.Permanent) sirve las páginas
        /// sin extensión, haciendo que AppRelativeCurrentExecutionFilePath devuelva
        /// la ruta sin .aspx.
        /// </summary>
        private static string NormalizarPagina(string paginaActual)
        {
            if (string.IsNullOrEmpty(paginaActual)) return paginaActual;
            string result = paginaActual.Replace("~/", "");
            if (result.EndsWith(".aspx", StringComparison.OrdinalIgnoreCase))
                result = result.Substring(0, result.Length - 5);
            return result;
        }

        /// <summary>
        /// Indica si la página actual coincide con alguna de la lista.
        /// </summary>
        private bool EsPaginaEnLista(string paginaActual, string[] paginas)
        {
            foreach (string pagina in paginas)
            {
                if (paginaActual.Equals(pagina, StringComparison.OrdinalIgnoreCase))
                    return true;
            }
            return false;
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
                    1,
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
