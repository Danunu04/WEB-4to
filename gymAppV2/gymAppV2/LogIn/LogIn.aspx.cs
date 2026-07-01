using System;
using System.Threading;
using System.Web;
using System.Web.Security;
using System.Web.UI;
using Servicios.Singleton;
using BE;
using BLL;
using SERVICIOS;

namespace gymAppV2.LogIn
{
    public partial class LogIn : System.Web.UI.Page
    {
        private BLLUsuario _bllUsuario;
        private BLLEvento _bllEvento;
        private BLLDigitoVerificador _bllDV;

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

        private BLLDigitoVerificador BllDV
        {
            get
            {
                if (_bllDV == null)
                {
                    _bllDV = new BLLDigitoVerificador();
                }
                return _bllDV;
            }
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                if (BllUsuario.UsuarioEstaLogueado())
                {
                    RedirigirSeguro("~/DashBoard/WebForm1.aspx");
                }
            }
        }

        protected void btnLogIn_Click(object sender, EventArgs e)
        {
            Page.Validate("LoginGroup");

            // Verificar validacion del lado del servidor primero
            if (!Page.IsValid)
            {
                MostrarToast("Complete todos los campos requeridos.", "warning");
                return;
            }

            string usuario = txtUsuario.Text.Trim();
            string contrasena = txtContrasena.Text.Trim();

            try
            {
                bool resultado = BllUsuario.ValidarLogin(usuario, contrasena);

                if (resultado)
                {
                    Usuario userBD = BllUsuario.ObtenerUsuario(usuario);

                    // Verificación de integridad de dígitos verificadores (DVH/DVV).
                    // Si hay inconsistencias, solo el administrador puede ingresar para reparar el sistema.
                    if (BllDV.ExisteErrorIntegridad())
                    {
                        if (userBD.USUARIO_Rol != PerfilesSistema.RolAdministrador)
                        {
                            try
                            {
                                BllEvento.RegistrarEvento(
                                    "error_integridad",
                                    userBD.USUARIO_Usuario,
                                    "Intento de login bloqueado por error de integridad.",
                                    4,
                                    "Seguridad");
                            }
                            catch
                            {
                                // No impedir el flujo si falla el registro del evento.
                            }

                            MostrarToast("Error de integridad en la base de datos. Contacte al administrador.", "error");
                            return;
                        }

                        // Administrador: permite el ingreso y redirige al panel de reparación.
                        BllUsuario.LogearUsuario(userBD);
                        FormsAuthentication.SetAuthCookie(userBD.USUARIO_Usuario, false);
                        BllEvento.RegistrarEvento(
                            "error_integridad",
                            userBD.USUARIO_Usuario,
                            "El sistema ingresó en modo de reparación por error de integridad.",
                            4,
                            "Seguridad");
                        BllEvento.RegistrarLogin(userBD.USUARIO_Usuario);

                        RedirigirConToast(
                            "Se detectó un error de integridad. Redirigiendo al panel de reparación.",
                            "~/VerificacioDV/VerificacioDV.aspx",
                            "warning");
                        return;
                    }

                    if (BllUsuario.UsuarioEstaLogueado())
                    {
                        MostrarToast("Ya hay una sesion activa.", "warning");
                        return;
                    }

                    BllUsuario.ReestablecerIntentos(usuario);
                    BllUsuario.LogearUsuario(userBD);

                    FormsAuthentication.SetAuthCookie(userBD.USUARIO_Usuario, false);

                    BllEvento.RegistrarLogin(userBD.USUARIO_Usuario);

                    // Si es el primer login, forzar cambio de contraseña y configuración de preguntas de seguridad.
                    if (userBD.USUARIO_PrimerLogin)
                    {
                        RedirigirConToast("Es su primer inicio de sesión. Debe cambiar su contraseña y configurar sus preguntas de seguridad.",
                            $"~/CambiarContra/Cambiar-contra.aspx?usuario={Server.UrlEncode(userBD.USUARIO_Usuario)}&modo=primerLogin");
                        return;
                    }

                    // Mostrar toast de exito y redirigir desde el cliente
                    RedirigirConToast("Inicio de sesion exitoso!", "~/DashBoard/WebForm1.aspx");
                }
            }
            catch (ExcepcionesLogIn ex)
            {
                // Si la cuenta está bloqueada, redirigir a cambio de contraseña en modo recuperación.
                if (ex.Result == ResultadosLogIn.AccountLocked)
                {
                    try
                    {
                        if (!string.IsNullOrEmpty(usuario) && BllUsuario.UsuarioExiste(usuario))
                        {
                            BllEvento.RegistrarBloqueoUsuario(usuario);
                        }
                    }
                    catch
                    {
                        // No impedir el flujo si falla el registro del evento.
                    }

                    RedirigirConToast("Usuario bloqueado. Responda su pregunta de seguridad para recuperar el acceso.",
                        $"~/LogIn/PreguntasSeguridad.aspx?usuario={Server.UrlEncode(usuario)}",
                        "error");
                    return;
                }

                string mensaje;
                switch (ex.Result)
                {
                    case ResultadosLogIn.InvalidPassword:
                        mensaje = "Contraseña incorrecta.";
                        break;
                    case ResultadosLogIn.InvalidUsername:
                        mensaje = "Usuario incorrecto.";
                        break;
                    default:
                        mensaje = "Credenciales inválidas.";
                        break;
                }

                MostrarToast(mensaje, "error");

                // Registrar error solo si el usuario existe en la BD
                try
                {
                    if (!string.IsNullOrEmpty(usuario) && BllUsuario.UsuarioExiste(usuario))
                    {
                        BllEvento.RegistrarError(usuario, $"Intento fallido de login - {ex.Result}");
                    }
                }
                catch
                {
                    // No impedir el flujo si falla el registro del evento
                }
            }
            catch (ThreadAbortException)
            {
                // ThreadAbortException es esperado al usar Response.Redirect
                // No hacer nada, dejar que el proceso continúe
            }
            catch (Exception ex)
            {
                MostrarToast("Error al conectar con el servidor: " + ex.Message, "error");
                // Registrar error solo si el usuario existe en la BD
                try
                {
                    if (!string.IsNullOrEmpty(usuario) && BllUsuario.UsuarioExiste(usuario))
                    {
                        BllEvento.RegistrarError(usuario, $"Error en login: {ex.Message}");
                    }
                }
                catch
                {
                    // No impedir el flujo si falla el registro del evento
                }
            }
        }

        protected void Button1_Click(object sender, EventArgs e)
        {
            try
            {
                Response.Redirect("/Inicio/Default.aspx", false);
                Context.ApplicationInstance.CompleteRequest();
            }
            catch (ThreadAbortException)
            {
                // ThreadAbortException es esperado al usar Response.Redirect
                // No hacer nada, dejar que el proceso continúe
            }
            catch (Exception)
            {
                // Ignorar errores menores de redirección.
            }
        }

        /// <summary>
        /// Redirige a la URL indicada terminando la request de forma segura.
        /// </summary>
        private void Redirigir(string url)
        {
            RedirigirSeguro(url);
        }

        /// <summary>
        /// Redirige de forma segura evitando ThreadAbortException.
        /// </summary>
        private void RedirigirSeguro(string url)
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
            catch (HttpException)
            {
                // Contexto no disponible.
            }
            catch (Exception)
            {
                // Ignorar errores menores de redirección.
            }
        }

        private void MostrarToast(string mensaje, string tipo)
        {
            string mensajeEscapado = mensaje
                .Replace("\\", "\\\\")
                .Replace("'", "\\'")
                .Replace("\"", "\\\"")
                .Replace("\r", "")
                .Replace("\n", "\\n");

            string script = "<script>(function(){" +
                "var container=document.getElementById('toastContainer');" +
                "if(!container){container=document.createElement('div');container.id='toastContainer';container.style.cssText='position:fixed;top:1.5rem;right:1.5rem;z-index:9999;display:flex;flex-direction:column;gap:0.75rem;max-width:400px;';document.body.appendChild(container);}" +
                "var toast=document.createElement('div');" +
                "toast.className='toast toast-" + tipo + "';" +
                "var icons={success:'bi-check-circle-fill',error:'bi-exclamation-circle-fill',warning:'bi-exclamation-triangle-fill',info:'bi-info-circle-fill'};" +
                "toast.innerHTML='<div class=\"toast-icon\"><i class=\"bi '+icons['" + tipo + "']+'\"></i></div><div class=\"toast-content\"><div class=\"toast-message\">" + mensajeEscapado + "</div></div><button class=\"toast-close\" onclick=\"this.parentElement.remove()\"><i class=\"bi bi-x\"></i></button>';" +
                "container.appendChild(toast);" +
                "setTimeout(function(){toast.classList.add('show');},10);" +
                "setTimeout(function(){toast.classList.add('hiding');setTimeout(function(){toast.remove();},300);},4000);" +
                "})();</script>";

            string key = "toast_" + DateTime.Now.Ticks.ToString() + "_" + new Random().Next(1000);
            ClientScript.RegisterStartupScript(this.GetType(), key, script);
        }

        private void RedirigirConToast(string mensaje, string url, string tipo = "success")
        {
            string mensajeEscapado = System.Security.SecurityElement.Escape(mensaje);
            string urlResuelta = ResolveUrl(url);
            string icono = tipo == "success" ? "bi-check-circle-fill" :
                           tipo == "error" ? "bi-exclamation-circle-fill" :
                           tipo == "warning" ? "bi-exclamation-triangle-fill" : "bi-info-circle-fill";
            string script = "<script>(function(){" +
                "var container=document.getElementById('toastContainer');" +
                "if(!container)return;" +
                "var toast=document.createElement('div');" +
                "toast.className='toast toast-" + tipo + "';" +
                "toast.innerHTML='<div class=\"toast-icon\"><i class=\"bi "+icono+"\"></i></div><div class=\"toast-content\"><div class=\"toast-message\">"+mensajeEscapado+"</div></div><button class=\"toast-close\" onclick=\"this.parentElement.remove()\"><i class=\"bi bi-x\"></i></button>';" +
                "container.appendChild(toast);" +
                "setTimeout(function(){toast.classList.add('show');},10);" +
                "setTimeout(function(){window.location.href='" + urlResuelta + "';},1500);" +
                "})();</script>";
            ClientScript.RegisterStartupScript(this.GetType(), "redirect_" + DateTime.Now.Ticks, script);
        }
    }
}



