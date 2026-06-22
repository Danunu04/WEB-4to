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
                    CargarUsuario(usuarioQuery.Trim(), true);
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
        /// Carga la pregunta de seguridad del usuario y precarga el campo usuario.
        /// </summary>
        private void CargarUsuario(string usuario, bool soloLectura)
        {
            txtUsuario.Text = usuario;
            txtUsuario.ReadOnly = soloLectura;

            try
            {
                PreguntaSeguridad pregunta = BllPreguntaSeguridad.ObtenerPreguntaPorUsuario(usuario);

                if (pregunta != null && !string.IsNullOrEmpty(pregunta.Pregunta))
                {
                    lblPregunta.Text = pregunta.Pregunta;
                }
                else
                {
                    // Si no existe pregunta, generarla automáticamente a partir de la fecha de nacimiento.
                    try
                    {
                        pregunta = BllPreguntaSeguridad.GenerarPreguntaSeguridad(usuario);
                        BllPreguntaSeguridad.GuardarPregunta(pregunta);
                        lblPregunta.Text = pregunta.Pregunta;
                    }
                    catch (Exception exGenerar)
                    {
                        MostrarMensaje("No se pudo generar la pregunta de seguridad: " + exGenerar.Message, false);
                    }
                }
            }
            catch (Exception ex)
            {
                MostrarMensaje("Error al cargar la pregunta de seguridad: " + ex.Message, false);
            }
        }

        /// <summary>
        /// Valida la respuesta de seguridad. Si es correcta, finaliza el primer login
        /// (o guarda la pregunta en modo normal) y redirige al dashboard.
        /// </summary>
        protected void btnGuardar_Click(object sender, EventArgs e)
        {
            Page.Validate("ConfigurarPreguntaGroup");
            if (!Page.IsValid)
            {
                return;
            }

            string usuario = txtUsuario.Text.Trim();
            string respuesta = txtRespuesta.Text.Trim();

            if (string.IsNullOrWhiteSpace(usuario))
            {
                MostrarMensaje("Usuario inválido.", false);
                return;
            }

            try
            {
                // Validar que la respuesta coincida con la respuesta esperada.
                bool valida = BllPreguntaSeguridad.ValidarRespuesta(usuario, respuesta);

                if (!valida)
                {
                    MostrarMensaje("La respuesta no coincide con la información registrada.", false);
                    return;
                }

                // Refrescar la pregunta de seguridad (por si el usuario la editara en una futura versión).
                PreguntaSeguridad pregunta = BllPreguntaSeguridad.ObtenerPreguntaPorUsuario(usuario);
                if (pregunta != null)
                {
                    BllPreguntaSeguridad.GuardarPregunta(pregunta);
                }

                // En modo primer login, finalizar el proceso y redirigir al dashboard.
                if (ModoPrimerLogin)
                {
                    try
                    {
                        BllUsuario.FinalizarPrimerLogin(usuario);
                    }
                    catch (Exception exFinalizar)
                    {
                        MostrarMensaje("Respuesta correcta, pero no se pudo finalizar la configuración: " + exFinalizar.Message, false);
                        return;
                    }

                    // Iniciar sesión automáticamente si no está logueado.
                    if (!BllUsuario.UsuarioEstaLogueado())
                    {
                        Usuario usuarioBD = BllUsuario.ObtenerUsuario(usuario);
                        if (usuarioBD != null)
                        {
                            BllUsuario.LogearUsuario(usuarioBD);
                        }
                    }

                    MostrarMensaje("Preguntas de seguridad configuradas correctamente.", true);
                    MostrarToast("Preguntas de seguridad configuradas correctamente.", "success");
                    RedirigirConDelay("~/DashBoard/WebForm1.aspx", 1500);
                }
                else
                {
                    MostrarMensaje("Preguntas de seguridad actualizadas correctamente.", true);
                    MostrarToast("Preguntas de seguridad actualizadas correctamente.", "success");
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
                        BllEvento.RegistrarError(usuario, $"Error al configurar preguntas de seguridad: {ex.Message}");
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
            lblMensaje.CssClass = exito ? "lblMensaje" : "lblMensaje";
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
