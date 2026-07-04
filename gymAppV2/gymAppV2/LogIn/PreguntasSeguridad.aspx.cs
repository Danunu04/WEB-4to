using System;
using System.Threading;
using System.Web.Security;
using System.Web.UI;
using BE;
using BLL;
using Servicios.Singleton;

namespace gymAppV2.LogIn
{
    public partial class PreguntasSeguridad : System.Web.UI.Page
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
        /// Usuario cuya pregunta de seguridad se está respondiendo.
        /// Se guarda en ViewState para no depender del campo editable en el paso 2.
        /// </summary>
        private string UsuarioPregunta
        {
            get { return ViewState["UsuarioPregunta"] as string; }
            set { ViewState["UsuarioPregunta"] = value; }
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                pnlUsuario.Visible = true;
                pnlPregunta.Visible = false;

                string usuario = Request.QueryString["usuario"];
                if (!string.IsNullOrWhiteSpace(usuario))
                {
                    txtUsuario.Text = usuario.Trim();
                }
            }
        }

        /// <summary>
        /// Carga la pregunta de seguridad del usuario ingresado.
        /// Solo muestra el paso 2 si el usuario existe y tiene pregunta configurada.
        /// </summary>
        protected void btnContinuar_Click(object sender, EventArgs e)
        {
            Page.Validate("PreguntaGroup");
            if (!Page.IsValid)
            {
                MostrarMensaje("Ingrese un usuario.", false);
                return;
            }

            string usuario = txtUsuario.Text.Trim();

            if (string.IsNullOrWhiteSpace(usuario))
            {
                MostrarMensaje("Ingrese un usuario.", false);
                return;
            }

            try
            {
                if (!BllUsuario.UsuarioExiste(usuario))
                {
                    // Mensaje genérico para no permitir enumeración de usuarios.
                    MostrarMensaje("No se pudo recuperar el acceso. Verifique los datos ingresados.", false);
                    return;
                }

                PreguntaSeguridad pregunta = BllPreguntaSeguridad.ObtenerPreguntaPorUsuario(usuario);

                if (pregunta == null || string.IsNullOrEmpty(pregunta.Pregunta))
                {
                    MostrarMensaje("El usuario no tiene configurada una pregunta de seguridad. Contacte al administrador.", false);
                    return;
                }

                UsuarioPregunta = usuario;
                lblPregunta.Text = pregunta.Pregunta;
                pnlUsuario.Visible = false;
                pnlPregunta.Visible = true;
            }
            catch (ThreadAbortException)
            {
                // ThreadAbortException es esperado al usar Response.Redirect.
            }
            catch (Exception ex)
            {
                MostrarMensaje("Error al cargar la pregunta de seguridad: " + ex.Message, false);
            }
        }

        /// <summary>
        /// Valida la respuesta de seguridad. Si es correcta, redirige al cambio de contraseña.
        /// </summary>
        protected void btnVerificar_Click(object sender, EventArgs e)
        {
            Page.Validate("RespuestaGroup");
            if (!Page.IsValid)
            {
                MostrarMensaje("Ingrese una respuesta.", false);
                return;
            }

            string usuario = UsuarioPregunta;
            string respuesta = txtRespuesta.Text.Trim();

            if (string.IsNullOrWhiteSpace(usuario))
            {
                MostrarMensaje("Sesión de recuperación inválida. Vuelva a iniciar el proceso.", false);
                return;
            }

            try
            {
                bool valida = BllPreguntaSeguridad.ValidarRespuesta(usuario, respuesta);

                if (valida)
                {
                    try
                    {
                        BllEvento.RegistrarEvento(BLLEvento.EVENTO_CAMBIO_CONTRASENA, usuario, "Respuesta de seguridad correcta - inicia sesión de recuperación", 1, "Autenticación");
                    }
                    catch
                    {
                        // No bloquear el flujo si falla el log.
                    }

                    // Autenticar al usuario de forma temporal mediante la pregunta de seguridad.
                    Usuario usuarioBD = BllUsuario.ObtenerUsuario(usuario);
                    if (usuarioBD != null)
                    {
                        BllUsuario.LogearUsuario(usuarioBD);
                        FormsAuthentication.SetAuthCookie(usuarioBD.USUARIO_Usuario, false);
                    }

                    // Generar token de un solo uso para el flujo de recuperación.
                    string token = Guid.NewGuid().ToString("N");
                    Session["Recuperacion_" + usuario] = token;

                    Redirigir("~/CambiarContra/Cambiar-contra.aspx?usuario=" + Server.UrlEncode(usuario) + "&modo=recuperacion&token=" + Server.UrlEncode(token));
                }
                else
                {
                    try
                    {
                        if (BllUsuario.UsuarioExiste(usuario))
                        {
                            BllEvento.RegistrarRespuestaSeguridadIncorrecta(usuario);
                        }
                    }
                    catch
                    {
                        // No bloquear el flujo si falla el log.
                    }

                    try
                    {
                        if (BllUsuario.UsuarioExiste(usuario))
                        {
                            BllUsuario.BloquearUsuario(usuario);
                        }
                    }
                    catch
                    {
                        // No bloquear el flujo UI si falla el bloqueo.
                    }

                    MostrarMensaje("Respuesta incorrecta. Su cuenta ha sido bloqueada. Contacte al administrador de Sportio.", false);
                }
            }
            catch (ThreadAbortException)
            {
                // ThreadAbortException es esperado al usar Response.Redirect.
            }
            catch (Exception ex)
            {
                MostrarMensaje("Error al validar la respuesta: " + ex.Message, false);
            }
        }

        protected void btnVolver_Click(object sender, EventArgs e)
        {
            Redirigir("~/LogIn/LogIn.aspx");
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
    }
}
