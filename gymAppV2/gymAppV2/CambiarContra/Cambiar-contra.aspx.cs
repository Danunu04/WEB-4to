using System;
using System.Threading;
using System.Web.UI;
using BE;
using BLL;
using Servicios.Singleton;

namespace gymAppV2.CambiarContra
{
    public partial class Cambiar_contra : System.Web.UI.Page
    {
        private BLLUsuario _bllUsuario;
        private BLLEvento _bllEvento;

        private BLLUsuario BllUsuario
        {
            get
            {
                if (_bllUsuario == null)
                {
                    _bllUsuario = new BLLUsuario();
                }
                return _bllUsuario;
            }
        }

        private BLLEvento BllEvento
        {
            get
            {
                if (_bllEvento == null)
                {
                    _bllEvento = new BLLEvento();
                }
                return _bllEvento;
            }
        }

        /// <summary>
        /// Indica si el usuario llegó desde el flujo de recuperación por pregunta de seguridad
        /// o porque la cuenta fue bloqueada. En ese modo no exigimos la contraseña actual.
        /// </summary>
        private bool ModoRecuperacion
        {
            get { return ViewState["ModoRecuperacion"] as bool? ?? false; }
            set { ViewState["ModoRecuperacion"] = value; }
        }

        /// <summary>
        /// Indica si el usuario llegó desde el primer login y debe configurar preguntas de seguridad
        /// después de cambiar la contraseña.
        /// </summary>
        private bool ModoPrimerLogin
        {
            get { return ViewState["ModoPrimerLogin"] as bool? ?? false; }
            set { ViewState["ModoPrimerLogin"] = value; }
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                string usuarioQuery = Request.QueryString["usuario"];
                string modoQuery = Request.QueryString["modo"];
                bool sesionActiva = BllUsuario.UsuarioEstaLogueado();

                if (!string.IsNullOrWhiteSpace(usuarioQuery) &&
                    string.Equals(modoQuery, "primerLogin", StringComparison.OrdinalIgnoreCase))
                {
                    // El modo primer login solo es válido si el usuario acaba de iniciar sesión
                    // y la marca primerLogin está activa en la base de datos.
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
                    // Modo recuperación: solo válido si se presenta el token generado por
                    // PreguntasSeguridad.aspx, la cuenta está bloqueada y el usuario autenticado
                    // coincide con el que inició el flujo.
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
                    // Sin parámetros de recuperación/primerLogin y sin sesión: no se permite el cambio de contraseña.
                    Redirigir("~/LogIn/LogIn.aspx");
                    return;
                }

                ConfigurarVisibilidadContrasenaActual();
            }
        }

        /// <summary>
        /// Devuelve true si el usuario indicado coincide con el usuario logueado en el singleton.
        /// </summary>
        private bool EsUsuarioEnSesion(string usuario)
        {
            try
            {
                var sesion = Singleton.Instancia;
                return sesion != null && sesion.Usuario != null
                    && string.Equals(sesion.Usuario.USUARIO_Usuario, usuario, StringComparison.OrdinalIgnoreCase);
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Configura la pantalla en modo cambio normal para el usuario logueado.
        /// </summary>
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

        /// <summary>
        /// Configura los campos para el modo indicado.
        /// </summary>
        private void ConfigurarModo(string usuario, bool primerLogin, bool recuperacion)
        {
            txtUsuario.Text = usuario;
            txtUsuario.ReadOnly = true;
            txtUsuario.CssClass += " bg-surface-2";
            ModoPrimerLogin = primerLogin;
            ModoRecuperacion = recuperacion;
        }

        /// <summary>
        /// Valida que el flujo de recuperación provenga realmente de PreguntasSeguridad.aspx
        /// verificando el token de un solo uso y que la cuenta esté bloqueada.
        /// </summary>
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

                return usuarioBD.USUARIO_Bloqueado || usuarioBD.USUARIO_PrimerLogin;
            }
            catch
            {
                return false;
            }
        }

        protected void btnGuardar_Click(object sender, EventArgs e)
        {
            if (!Page.IsValid)
            {
                return;
            }

            string usuario = txtUsuario.Text.Trim();
            string contrasenaActual = txtContrasenaActual.Text;
            string nuevaContrasena = txtNuevaContrasena.Text;
            string confirmarContrasena = txtConfirmarContrasena.Text;

            // Validación server-side de coincidencia (refuerzo del CompareValidator).
            if (nuevaContrasena != confirmarContrasena)
            {
                MostrarMensaje("Las contraseñas nuevas no coinciden.", "danger");
                return;
            }

            try
            {
                // En modo recuperación (login bloqueado o pregunta de seguridad correcta)
                // no se verifica ni se toma en cuenta la contraseña actual.
                if (!ModoRecuperacion)
                {
                    bool loginValido = BllUsuario.ValidarLogin(usuario, contrasenaActual);
                    if (!loginValido)
                    {
                        MostrarMensaje("La contraseña actual es incorrecta.", "danger");
                        return;
                    }
                }

                BllUsuario.CambiarContrasena(usuario, nuevaContrasena);

                // En modo recuperación, asegurar que se desbloquee el usuario y se reinicien los intentos.
                if (ModoRecuperacion)
                {
                    try
                    {
                        BllUsuario.DesbloquearUsuario(usuario);
                        BllUsuario.ReestablecerIntentos(usuario);
                        // Invalidar el token de recuperación para evitar reuso.
                        Session.Remove("Recuperacion_" + usuario);
                    }
                    catch (Exception exRestablecer)
                    {
                        MostrarMensaje("Contraseña actualizada, pero no se pudo desbloquear el usuario: " + exRestablecer.Message, "danger");
                        return;
                    }
                }

                // Si hay sesión activa, mantenerla; si es recuperación, redirigir al login;
                // si es primer login, redirigir a la configuración de preguntas de seguridad.
                if (ModoRecuperacion)
                {
                    MostrarMensaje("Contraseña actualizada. Inicie sesión con su nueva contraseña.", "success");
                    MostrarToast("Contraseña actualizada. Inicie sesión con su nueva contraseña.", "success");
                    RedirigirConDelay("~/LogIn/LogIn.aspx", 2000);
                }
                else if (ModoPrimerLogin)
                {
                    MostrarMensaje("Contraseña actualizada. Ahora configure sus preguntas de seguridad.", "success");
                    MostrarToast("Contraseña actualizada. Ahora configure sus preguntas de seguridad.", "success");
                    RedirigirConDelay($"~/LogIn/ConfigurarPreguntas.aspx?usuario={Server.UrlEncode(usuario)}&modo=primerLogin", 1500);
                }
                else
                {
                    MostrarMensaje("Contraseña actualizada correctamente.", "success");
                    MostrarToast("Contraseña actualizada correctamente.", "success");
                    RedirigirConDelay("~/DashBoard/WebForm1.aspx", 1500);
                }
            }
            catch (ThreadAbortException)
            {
                // ThreadAbortException es esperado al usar Response.Redirect.
            }
            catch (Exception ex)
            {
                string mensaje = ex.Message;

                // Mensaje amigable para contraseña ya utilizada.
                if (mensaje.Contains("reutilizar"))
                {
                    mensaje = "No puedes reutilizar una contraseña anterior.";
                }

                MostrarMensaje(mensaje, "danger");

                try
                {
                    if (!string.IsNullOrEmpty(usuario))
                    {
                        BllEvento.RegistrarError(usuario, $"Error al cambiar contraseña: {ex.Message}");
                    }
                }
                catch
                {
                    // No bloquear el flujo si falla el registro del evento.
                }
            }
        }

        protected void btnCancelar_Click(object sender, EventArgs e)
        {
            if (BllUsuario.UsuarioEstaLogueado())
            {
                Redirigir("~/DashBoard/WebForm1.aspx");
            }
            else
            {
                Redirigir("~/LogIn/LogIn.aspx");
            }
        }

        protected void btnIrInicio_Click(object sender, EventArgs e)
        {
            Redirigir("/Inicio/Default.aspx");
        }

        /// <summary>
        /// Muestra u oculta el campo de contraseña actual según el modo de acceso.
        /// Solo se exige cuando el usuario tiene sesión activa (está logueado).
        /// </summary>
        private void ConfigurarVisibilidadContrasenaActual()
        {
            bool requiereContrasenaActual = !ModoRecuperacion;

            txtContrasenaActual.Visible = requiereContrasenaActual;
            grupoContrasenaActual.Visible = requiereContrasenaActual;
            rfvContrasenaActual.Enabled = requiereContrasenaActual;

            lblMensaje.Visible = false;
        }

        /// <summary>
        /// Muestra un mensaje en el label de estado con la estética del login.
        /// </summary>
        private void MostrarMensaje(string mensaje, string tipo = "danger")
        {
            lblMensaje.Text = mensaje;
            lblMensaje.CssClass = tipo == "success"
                ? "lblMensaje lblMensaje-success"
                : "lblMensaje";
            lblMensaje.Visible = true;
        }

        /// <summary>
        /// Muestra un toast de notificación usando el mismo estilo del dashboard.
        /// </summary>
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

        /// <summary>
        /// Redirige a la URL indicada terminando la request de forma segura.
        /// </summary>
        private void Redirigir(string url)
        {
            try
            {
                Response.Redirect(ResolveUrl(url), false);
                Context.ApplicationInstance.CompleteRequest();
            }
            catch (ThreadAbortException)
            {
                // ThreadAbortException es esperado al usar Response.Redirect.
            }
        }

        /// <summary>
        /// Muestra un toast y redirige tras un delay.
        /// </summary>
        private void RedirigirConDelay(string url, int delayMs)
        {
            string urlResuelta = ResolveUrl(url);
            string script = "<script>(function(){" +
                "setTimeout(function(){window.location.href='" + urlResuelta + "';}," + delayMs + ");" +
                "})();</script>";
            ClientScript.RegisterStartupScript(this.GetType(), "redirect_" + DateTime.Now.Ticks, script);
        }
    }
}
