using System;
using System.Threading;
using System.Web.UI;
using BE;
using BLL;
using Servicios.Singleton;

namespace gymAppV2.LogIn
{
    public partial class ConfigurarPreguntas : System.Web.UI.Page
    {
        private BLLUsuario _bllUsuario;
        private BLLPreguntaSeguridad _bllPreguntaSeguridad;
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

        private BLLPreguntaSeguridad BllPreguntaSeguridad
        {
            get
            {
                if (_bllPreguntaSeguridad == null)
                {
                    _bllPreguntaSeguridad = new BLLPreguntaSeguridad();
                }
                return _bllPreguntaSeguridad;
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
        /// Indica si el usuario llegó desde el flujo de primer login.
        /// En ese modo se permite acceder sin sesión activa.
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

                // Solo se permite acceder con modo=primerLogin o con sesión activa.
                if (!string.IsNullOrWhiteSpace(usuarioQuery) &&
                    string.Equals(modoQuery, "primerLogin", StringComparison.OrdinalIgnoreCase))
                {
                    string usuarioTrimmed = usuarioQuery.Trim();
                    // El usuario de la URL debe coincidir con el de la sesión activa para evitar
                    // que un usuario configure la pregunta de seguridad de otra cuenta.
                    if (!BllUsuario.UsuarioEstaLogueado())
                    {
                        Redirigir("~/LogIn/LogIn.aspx");
                        return;
                    }
                    var usuarioSesion = Singleton.Instancia?.Usuario;
                    if (usuarioSesion == null ||
                        !string.Equals(usuarioSesion.USUARIO_Usuario, usuarioTrimmed, StringComparison.OrdinalIgnoreCase))
                    {
                        Redirigir("~/LogIn/LogIn.aspx");
                        return;
                    }
                    CargarUsuario(usuarioTrimmed, true);
                    ModoPrimerLogin = true;
                }
                else if (BllUsuario.UsuarioEstaLogueado())
                {
                    Usuario usuarioLogueado = Singleton.Instancia.Usuario;
                    if (usuarioLogueado == null)
                    {
                        Redirigir("~/LogIn/LogIn.aspx");
                        return;
                    }

                    CargarUsuario(usuarioLogueado.USUARIO_Usuario, false);
                    ModoPrimerLogin = false;
                }
                else
                {
                    Redirigir("~/LogIn/LogIn.aspx");
                    return;
                }
            }
        }

        /// <summary>
        /// Precarga el campo usuario en el formulario.
        /// </summary>
        private void CargarUsuario(string usuario, bool soloLectura)
        {
            txtUsuario.Text = usuario;
            txtUsuario.ReadOnly = soloLectura;
        }

        /// <summary>
        /// Guarda la pregunta y respuesta que el usuario ingresó, encriptadas, y finaliza el primer login.
        /// </summary>
        protected void btnGuardar_Click(object sender, EventArgs e)
        {
            Page.Validate("ConfigurarPreguntaGroup");
            if (!Page.IsValid)
            {
                return;
            }

            string usuario = txtUsuario.Text.Trim();
            string preguntaTexto = txtPregunta.Text.Trim();
            string respuesta = txtRespuesta.Text.Trim();

            if (string.IsNullOrWhiteSpace(usuario))
            {
                MostrarMensaje("Usuario inválido.", false);
                return;
            }

            if (string.IsNullOrWhiteSpace(preguntaTexto))
            {
                MostrarMensaje("Ingrese una pregunta de seguridad.", false);
                return;
            }

            if (string.IsNullOrWhiteSpace(respuesta))
            {
                MostrarMensaje("Ingrese la respuesta.", false);
                return;
            }

            try
            {
                var pregunta = new PreguntaSeguridad(0, preguntaTexto, respuesta, usuario, string.Empty, string.Empty);
                BllPreguntaSeguridad.GuardarPregunta(pregunta);

                if (ModoPrimerLogin)
                {
                    try
                    {
                        BllUsuario.FinalizarPrimerLogin(usuario);
                    }
                    catch (Exception exFinalizar)
                    {
                        MostrarMensaje("Pregunta guardada, pero no se pudo finalizar la configuración: " + exFinalizar.Message, false);
                        return;
                    }

                    if (!BllUsuario.UsuarioEstaLogueado())
                    {
                        Usuario usuarioBD = BllUsuario.ObtenerUsuario(usuario);
                        if (usuarioBD != null)
                        {
                            BllUsuario.LogearUsuario(usuarioBD);
                        }
                    }

                    MostrarMensaje("Pregunta de seguridad configurada correctamente.", true);
                    MostrarToast("Pregunta de seguridad configurada correctamente.", "success");
                    RedirigirConDelay("~/DashBoard/WebForm1.aspx", 1500);
                }
                else
                {
                    MostrarMensaje("Pregunta de seguridad actualizada correctamente.", true);
                    MostrarToast("Pregunta de seguridad actualizada correctamente.", "success");
                    RedirigirConDelay("~/DashBoard/WebForm1.aspx", 1500);
                }
            }
            catch (ThreadAbortException)
            {
                // ThreadAbortException es esperado al usar Response.Redirect.
            }
            catch (Exception ex)
            {
                MostrarMensaje("Error al guardar la pregunta de seguridad: " + ex.Message, false);

                try
                {
                    if (!string.IsNullOrEmpty(usuario))
                    {
                        BllEvento.RegistrarError(usuario, $"Error al configurar pregunta de seguridad: {ex.Message}");
                    }
                }
                catch
                {
                    // No bloquear el flujo si falla el log.
                }
            }
        }

        protected void btnIrInicio_Click(object sender, EventArgs e)
        {
            Redirigir("/Inicio/Default.aspx");
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
        /// Muestra un mensaje en el label de estado.
        /// </summary>
        private void MostrarMensaje(string mensaje, bool exito)
        {
            lblMensaje.Text = mensaje;
            lblMensaje.CssClass = exito ? "lblMensaje lblMensaje-success" : "lblMensaje";
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
