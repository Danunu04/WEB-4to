using System;
using System.Collections.Generic;
using System.Threading;
using System.Web.UI;
using BE;
using BLL;
using Servicios.Singleton;
using SERVICIOS.Observer;

namespace gymAppV2.CambiarContra
{
    public partial class Cambiar_contra : System.Web.UI.Page
    {
        private BLLUsuario _bllUsuario;
        private BLLEvento _bllEvento;
        private Dictionary<string, string> _dict;

        private BLLUsuario BllUsuario
        {
            get
            {
                if (_bllUsuario == null) _bllUsuario = new BLLUsuario();
                return _bllUsuario;
            }
        }

        private BLLEvento BllEvento
        {
            get
            {
                if (_bllEvento == null) _bllEvento = new BLLEvento();
                return _bllEvento;
            }
        }

        // Helper de traducción: carga el diccionario una vez por request.
        protected string T(string tag)
        {
            if (_dict == null)
            {
                try { _dict = new BLLTraduccion().ObtenerDiccionario(GestorIdioma.IdiomaActual); }
                catch { _dict = new Dictionary<string, string>(); }
            }
            return _dict.TryGetValue(tag, out var val) ? val : tag;
        }

        private bool ModoRecuperacion
        {
            get { return ViewState["ModoRecuperacion"] as bool? ?? false; }
            set { ViewState["ModoRecuperacion"] = value; }
        }

        private bool ModoPrimerLogin
        {
            get { return ViewState["ModoPrimerLogin"] as bool? ?? false; }
            set { ViewState["ModoPrimerLogin"] = value; }
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            if (SistemaEnPausa() && !UsuarioActualEsAdmin())
            {
                Redirigir("~/VerificacioDV/VerificacioDV.aspx");
                return;
            }

            // Aplicar traducciones en cada carga (initial + PostBack para validators).
            AplicarIdioma();

            if (!IsPostBack)
            {
                string usuarioQuery = Request.QueryString["usuario"];
                string modoQuery = Request.QueryString["modo"];
                bool sesionActiva = BllUsuario.UsuarioEstaLogueado();

                if (!string.IsNullOrWhiteSpace(usuarioQuery) &&
                    string.Equals(modoQuery, "primerLogin", StringComparison.OrdinalIgnoreCase))
                {
                    if (!sesionActiva)
                    {
                        Redirigir("~/LogIn/LogIn.aspx");
                        return;
                    }

                    string usuario = usuarioQuery.Trim();
                    if (!EsUsuarioEnSesion(usuario))
                    {
                        Redirigir("~/LogIn/LogIn.aspx");
                        return;
                    }

                    Usuario usuarioBD = BllUsuario.ObtenerUsuario(usuario);
                    if (usuarioBD == null || !usuarioBD.USUARIO_PrimerLogin)
                    {
                        Redirigir("~/LogIn/LogIn.aspx");
                        return;
                    }

                    ConfigurarModo(usuario, true, false);
                }
                else if (!string.IsNullOrWhiteSpace(usuarioQuery) &&
                         string.Equals(modoQuery, "recuperacion", StringComparison.OrdinalIgnoreCase))
                {
                    string usuario = usuarioQuery.Trim();

                    if (sesionActiva && !EsUsuarioEnSesion(usuario))
                    {
                        Redirigir("~/LogIn/LogIn.aspx");
                        return;
                    }

                    if (!ValidarFlujoRecuperacion(usuario))
                    {
                        Redirigir("~/LogIn/LogIn.aspx");
                        return;
                    }

                    ConfigurarModo(usuario, false, true);
                }
                else if (sesionActiva)
                {
                    ConfigurarModoNormal();
                }
                else
                {
                    Redirigir("~/LogIn/LogIn.aspx");
                    return;
                }

                ConfigurarVisibilidadContrasenaActual();
            }
        }

        private void AplicarIdioma()
        {
            // Botones server-side
            btnGuardar.Text  = T("cambio_btn_guardar");
            btnCancelar.Text = T("cambio_btn_cancelar");
            lnkVolverLogin.Text = T("cambio_link_volver");

            // Mensajes de error de los validadores ASP.NET
            rfvContrasenaActual.ErrorMessage  = T("cambio_val_contra_actual");
            rfvNuevaContrasena.ErrorMessage   = T("cambio_val_nueva_contra");
            revNuevaContrasena.ErrorMessage   = T("cambio_val_contra_regex");
            rfvConfirmarContrasena.ErrorMessage = T("cambio_val_confirmar");
            cvConfirmarContrasena.ErrorMessage  = T("cambio_val_no_coinciden");
        }

        private bool EsUsuarioEnSesion(string usuario)
        {
            try
            {
                var sesion = Singleton.Instancia;
                return sesion != null && sesion.Usuario != null
                    && string.Equals(sesion.Usuario.USUARIO_Usuario, usuario, StringComparison.OrdinalIgnoreCase);
            }
            catch { return false; }
        }

        private void ConfigurarModoNormal()
        {
            Usuario usuarioLogueado = Singleton.Instancia.Usuario;
            if (usuarioLogueado == null)
            {
                Redirigir("~/LogIn/LogIn.aspx");
                return;
            }

            ConfigurarModo(usuarioLogueado.USUARIO_Usuario, false, false);
        }

        private void ConfigurarModo(string usuario, bool primerLogin, bool recuperacion)
        {
            txtUsuario.Text = usuario;
            txtUsuario.ReadOnly = true;
            txtUsuario.CssClass += " bg-surface-2";
            ModoPrimerLogin = primerLogin;
            ModoRecuperacion = recuperacion;
        }

        private bool ValidarFlujoRecuperacion(string usuario)
        {
            string token = Request.QueryString["token"];
            if (string.IsNullOrWhiteSpace(token))
                return false;

            string expectedToken = Session["Recuperacion_" + usuario] as string;
            if (!string.Equals(token, expectedToken, StringComparison.Ordinal))
                return false;

            try
            {
                Usuario usuarioBD = BllUsuario.ObtenerUsuario(usuario);
                if (usuarioBD == null || !usuarioBD.USUARIO_Activo)
                    return false;

                return true;
            }
            catch { return false; }
        }

        protected void btnGuardar_Click(object sender, EventArgs e)
        {
            if (!Page.IsValid) return;

            string usuario = txtUsuario.Text.Trim();
            string contrasenaActual = txtContrasenaActual.Text;
            string nuevaContrasena = txtNuevaContrasena.Text;
            string confirmarContrasena = txtConfirmarContrasena.Text;

            if (nuevaContrasena != confirmarContrasena)
            {
                MostrarMensaje(T("cambio_msg_no_coinciden"), "danger");
                return;
            }

            try
            {
                if (!ModoRecuperacion)
                {
                    bool loginValido = BllUsuario.ValidarLogin(usuario, contrasenaActual);
                    if (!loginValido)
                    {
                        MostrarMensaje(T("cambio_msg_contra_incorrecta"), "danger");
                        return;
                    }
                }

                BllUsuario.CambiarContrasena(usuario, nuevaContrasena);

                if (ModoRecuperacion)
                {
                    try
                    {
                        BllUsuario.DesbloquearUsuario(usuario);
                        BllUsuario.ReestablecerIntentos(usuario);
                        Session.Remove("Recuperacion_" + usuario);
                    }
                    catch (Exception exRestablecer)
                    {
                        MostrarMensaje(T("cambio_msg_ok") + " — " + exRestablecer.Message, "danger");
                        return;
                    }
                }

                if (ModoRecuperacion)
                {
                    System.Web.Security.FormsAuthentication.SignOut();
                    try { Singleton.Instancia.LogOut(); } catch { }
                    string msg = T("cambio_msg_ok_recuperacion");
                    MostrarMensaje(msg, "success");
                    MostrarToast(msg, "success");
                    RedirigirConDelay("~/LogIn/LogIn.aspx", 2000);
                }
                else if (ModoPrimerLogin)
                {
                    string msg = T("cambio_msg_ok_primer_login");
                    MostrarMensaje(msg, "success");
                    MostrarToast(msg, "success");
                    RedirigirConDelay($"~/LogIn/ConfigurarPreguntas.aspx?usuario={Server.UrlEncode(usuario)}&modo=primerLogin", 1500);
                }
                else
                {
                    string msg = T("cambio_msg_ok");
                    MostrarMensaje(msg, "success");
                    MostrarToast(msg, "success");
                    RedirigirConDelay("~/DashBoard/WebForm1.aspx", 1500);
                }
            }
            catch (ThreadAbortException) { }
            catch (Exception ex)
            {
                string mensaje = ex.Message;

                if (mensaje.Contains("reutilizar"))
                    mensaje = T("cambio_msg_reutilizar");

                MostrarMensaje(mensaje, "danger");

                try
                {
                    if (!string.IsNullOrEmpty(usuario))
                        BllEvento.RegistrarError(usuario, $"Error al cambiar contraseña: {ex.Message}");
                }
                catch { }
            }
        }

        protected void btnCancelar_Click(object sender, EventArgs e)
        {
            if (ModoRecuperacion)
            {
                System.Web.Security.FormsAuthentication.SignOut();
                try { Singleton.Instancia.LogOut(); } catch { }
                Redirigir("~/LogIn/LogIn.aspx");
            }
            else if (BllUsuario.UsuarioEstaLogueado())
            {
                Redirigir("~/DashBoard/WebForm1.aspx");
            }
            else
            {
                Redirigir("~/LogIn/LogIn.aspx");
            }
        }

        protected void lnkVolverLogin_Click(object sender, EventArgs e)
        {
            System.Web.Security.FormsAuthentication.SignOut();
            try { Singleton.Instancia.LogOut(); } catch { }
            Redirigir("~/LogIn/LogIn.aspx");
        }

        protected void btnIrInicio_Click(object sender, EventArgs e)
        {
            Redirigir("/Inicio/Default.aspx");
        }

        private void ConfigurarVisibilidadContrasenaActual()
        {
            bool requiereContrasenaActual = !ModoRecuperacion;

            txtContrasenaActual.Visible = requiereContrasenaActual;
            grupoContrasenaActual.Visible = requiereContrasenaActual;
            rfvContrasenaActual.Enabled = requiereContrasenaActual;

            lblMensaje.Visible = false;
        }

        private void MostrarMensaje(string mensaje, string tipo = "danger")
        {
            lblMensaje.Text = mensaje;
            lblMensaje.CssClass = tipo == "success"
                ? "lblMensaje lblMensaje-success"
                : "lblMensaje";
            lblMensaje.Visible = true;
        }

        private void MostrarToast(string mensaje, string tipo)
        {
            string mensajeEscapado = System.Security.SecurityElement.Escape(mensaje);
            string script = "<script>(function(){" +
                "var container=document.getElementById('toastContainer');" +
                "if(!container)return;" +
                "var toast=document.createElement('div');" +
                "toast.className='toast toast-" + tipo + "';" +
                "var icons={success:'bi-check-circle-fill',error:'bi-exclamation-circle-fill',warning:'bi-exclamation-triangle-fill',info:'bi-info-circle-fill'};" +
                "toast.innerHTML='<div class=\"toast-icon\"><i class=\"bi '+icons['" + tipo + "']+'\"></i></div><div class=\"toast-content\"><div class=\"toast-message\">" + mensajeEscapado + "</div></div><button class=\"toast-close\" onclick=\"this.parentElement.remove()\"><i class=\"bi bi-x\"></i></button>';" +
                "container.appendChild(toast);" +
                "setTimeout(function(){toast.classList.add('show');},10);" +
                "setTimeout(function(){toast.classList.add('hiding');setTimeout(function(){toast.remove();},300);},4000);" +
                "})();</script>";
            ClientScript.RegisterStartupScript(this.GetType(), "toast_" + DateTime.Now.Ticks, script);
        }

        private void Redirigir(string url)
        {
            try
            {
                Response.Redirect(ResolveUrl(url), false);
                Context.ApplicationInstance.CompleteRequest();
            }
            catch (ThreadAbortException) { }
        }

        private void RedirigirConDelay(string url, int delayMs)
        {
            string urlResuelta = ResolveUrl(url);
            string script = "<script>(function(){" +
                "setTimeout(function(){window.location.href='" + urlResuelta + "';}," + delayMs + ");" +
                "})();</script>";
            ClientScript.RegisterStartupScript(this.GetType(), "redirect_" + DateTime.Now.Ticks, script);
        }

        private bool SistemaEnPausa()
        {
            try { return new BLLDigitoVerificador().ExisteErrorIntegridad(); }
            catch { return true; }
        }

        private bool UsuarioActualEsAdmin()
        {
            try
            {
                var sesion = Singleton.Instancia;
                if (sesion == null || sesion.Usuario == null) return false;
                return new BLLRol().UsuarioActualEsAdmin();
            }
            catch { return false; }
        }
    }
}
