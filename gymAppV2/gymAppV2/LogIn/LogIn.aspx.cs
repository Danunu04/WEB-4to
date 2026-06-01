using System;
using System.Threading;
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
        private BLLUsuario bllUsuario;
        private BLLEvento bllEvento;

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                bllUsuario = new BLLUsuario();
                bllEvento = new BLLEvento();

                if (bllUsuario.UsuarioEstaLogueado())
                {
                    Response.Redirect("~/DashBoard/WebForm1.aspx");
                }
            }
        }

        protected void btnLogIn_Click(object sender, EventArgs e)
        {
            string usuario = txtUsuario.Text.Trim();
            string contrasena = txtContrasena.Text.Trim();
            CriptoManager cm = new CriptoManager();
            string contraHash = cm._686DPGetSHA256(contrasena);

            if (string.IsNullOrEmpty(usuario) || string.IsNullOrEmpty(contrasena))
            {
                MostrarToast("Complete usuario y contrasena.", "warning");
                return;
            }

            try
            {
                bllUsuario = new BLLUsuario();
                bllEvento = new BLLEvento();

                bool resultado = bllUsuario.ValidarLogin(usuario, contrasena);

                if (resultado)
                {
                    Usuario userBD = bllUsuario.ObtenerUsuario(usuario);

                    if (bllUsuario.UsuarioEstaLogueado())
                    {
                        MostrarToast("Ya hay una sesion activa.", "warning");
                        return;
                    }

                    bllUsuario.ReestablecerIntentos(usuario);
                    bllUsuario.LogearUsuario(userBD);

                    FormsAuthentication.SetAuthCookie(userBD.USUARIO_Usuario, false);

                    bllEvento.RegistrarEvento("login", userBD.USUARIO_Usuario, "Inicio de sesión exitoso");

                    // Mostrar toast de exito y redirigir desde el cliente
                    RedirigirConToast("Inicio de sesion exitoso!", "~/DashBoard/WebForm1.aspx");
                }
            }
            catch (ExcepcionesLogIn ex)
            {
                bllUsuario.RegistrarIntentoFallido(usuario);
                int intentosRestantes = bllUsuario.ObtenerIntentosRestantes(usuario);

                if (intentosRestantes <= 0)
                {
                    MostrarToast("El usuario ha sido bloqueado por demasiados intentos fallidos.", "error");
                    btnLogIn.Enabled = false;
                }
                else
                {
                    string mensaje = "";
                    switch (ex.Result)
                    {
                        case ResultadosLogIn.InvalidUsername:
                            mensaje = "Usuario no encontrado. Intentos restantes: " + intentosRestantes;
                            break;
                        case ResultadosLogIn.InvalidPassword:
                            mensaje = "Contrasena incorrecta. Intentos restantes: " + intentosRestantes;
                            break;
                        default:
                            mensaje = "Error de autenticacion. Intentos restantes: " + intentosRestantes;
                            break;
                    }
                    MostrarToast(mensaje, "error");
                }

                bllEvento.RegistrarEvento("error", usuario, $"Intento fallido de login - {ex.Result}");
            }
            catch (ThreadAbortException)
            {
                // ThreadAbortException es esperado al usar Response.Redirect
                // No hacer nada, dejar que el proceso continúe
            }
            catch (Exception ex)
            {
                MostrarToast("Error al conectar con el servidor: " + ex.Message, "error");
                bllEvento.RegistrarEvento("error", usuario, $"Error en login: {ex.Message}");
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
            catch (Exception ex)
            {
                lblMensaje.Text = "Error al redirigir: " + ex.Message;
            }
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
                "toast.innerHTML='<div class=\"toast-icon\"><i class=\"bi '+icons['" + tipo + "']+'\"></i></div><div class=\"toast-content\"><div class=\"toast-message\">"+mensajeEscapado+"</div></div><button class=\"toast-close\" onclick=\"this.parentElement.remove()\"><i class=\"bi bi-x\"></i></button>';" +
                "container.appendChild(toast);" +
                "setTimeout(function(){toast.classList.add('show');},10);" +
                "setTimeout(function(){toast.classList.add('hiding');setTimeout(function(){toast.remove();},300);},4000);" +
                "})();</script>";
            ClientScript.RegisterStartupScript(this.GetType(), "toast_" + DateTime.Now.Ticks, script);
        }

        private void RedirigirConToast(string mensaje, string url)
        {
            string mensajeEscapado = System.Security.SecurityElement.Escape(mensaje);
            string urlResuelta = ResolveUrl(url);
            string script = "<script>(function(){" +
                "var container=document.getElementById('toastContainer');" +
                "if(!container)return;" +
                "var toast=document.createElement('div');" +
                "toast.className='toast toast-success';" +
                "toast.innerHTML='<div class=\"toast-icon\"><i class=\"bi bi-check-circle-fill\"></i></div><div class=\"toast-content\"><div class=\"toast-message\">"+mensajeEscapado+"</div></div><button class=\"toast-close\" onclick=\"this.parentElement.remove()\"><i class=\"bi bi-x\"></i></button>';" +
                "container.appendChild(toast);" +
                "setTimeout(function(){toast.classList.add('show');},10);" +
                "setTimeout(function(){window.location.href='" + urlResuelta + "';},1500);" +
                "})();</script>";
            ClientScript.RegisterStartupScript(this.GetType(), "redirect_" + DateTime.Now.Ticks, script);
        }
    }
}



