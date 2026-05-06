using System;
using System.Web.Security;
using System.Web.UI;
using Servicios.Singleton;
using BE;

namespace gymAppV2.LogIn
{
    public partial class LogIn : System.Web.UI.Page
    {
        // Propiedad que persiste el contador de intentos fallidos entre postbacks usando ViewState
        private int Intentos
        {
            get { return ViewState["Intentos"] != null ? (int)ViewState["Intentos"] : 0; }
            set { ViewState["Intentos"] = value; }
        }

        // Evento que se ejecuta al cargar la página
        protected void Page_Load(object sender, EventArgs e)
        {
            // Sin lógica inicial por ahora
        }

        // Evento del botón de login - valida credenciales y maneja autenticación
        protected void btnLogIn_Click(object sender, EventArgs e)
        {
            // Obtener y limpiar valores de los campos
            string usuario = txtUsuario.Text.Trim();
            string contrasena = txtContrasena.Text.Trim();

            // Validar que ambos campos tengan datos
            if (string.IsNullOrEmpty(usuario) || string.IsNullOrEmpty(contrasena))
            {
                lblMensaje.Text = "Complete usuario y contraseña.";
                return;
            }

            // Incrementar contador de intentos
            Intentos++;

            try
            {
                // Validar credenciales contra la base de datos
                BE.Usuario user = ValidarCredenciales(usuario, contrasena);

                if (user != null)
                {
                    // Verificar si ya hay una sesión activa
                    if (Singleton.Instancia != null && Singleton.Instancia.IsLogged())
                    {
                        lblMensaje.Text = "Sesion activa.";
                        return;
                    }

                    // Login exitoso - resetear contador de intentos
                    Intentos = 0;

                    // Guardar usuario en el singleton para acceso global
                    Singleton.Instancia.LogIn(user);

                    // Crear cookie de autenticación de ASP.NET
                    FormsAuthentication.SetAuthCookie(user.USUARIO_Nombre, false);

                    // Redirigir al dashboard
                    Response.Redirect("~/DashBoard/WebForm1.aspx");
                }
                else
                {
                    // Login fallido - verificar si se alcanzó el límite de intentos
                    if (Intentos >= 3)
                    {
                        lblMensaje.Text = "Demasiados intentos. Intente más tarde.";
                        btnLogIn.Enabled = false;
                    }
                    else
                    {
                        lblMensaje.Text = string.Format("Usuario o contraseña incorrectos. Intento {0} de 3.", Intentos);
                    }
                }
            }
            catch (ExcepcionesLogIn ex)
            {
                // Manejar excepciones específicas de login con mensajes personalizados
                switch (ex.Result)
                {
                    case ResultadosLogIn.InvalidUsername:
                        lblMensaje.Text = "Usuario no encontrado.";
                        break;
                    case ResultadosLogIn.InvalidPassword:
                        lblMensaje.Text = string.Format("Contraseña incorrecta. Intento {0} de 3.", Intentos);
                        break;
                    default:
                        lblMensaje.Text = "Error de autenticación.";
                        break;
                }
            }
            catch (Exception)
            {
                // Manejar errores generales (ej. conexión a BD)
                lblMensaje.Text = "Error al conectar con el servidor.";
            }
        }

        // Valida las credenciales del usuario contra la base de datos
        // Retorna el objeto Usuario si las credenciales son válidas, null si no
        private BE.Usuario ValidarCredenciales(string usuario, string contrasena)
        {
            // TODO: Reemplazar con consulta a DAL cuando esté implementado
            // Por ahora, lógica de prueba para que funcione el flujo
            if (usuario == "admin" && contrasena == "admin")
            {
                return new BE.Usuario
                {
                    USUARIO_Id = 1,
                    USUARIO_Nombre = "admin",
                    USUARIO_Contras = "",
                    USUARIO_Intentos = 0,
                    USUARIO_Baja = false
                };
            }

            // Si el usuario no es "admin", lanzar excepción de usuario inválido
            if (usuario != "admin")
            {
                throw new ExcepcionesLogIn(ResultadosLogIn.InvalidUsername);
            }

            // Si el usuario es "admin" pero la contraseña no coincide, lanzar excepción de contraseña inválida
            throw new ExcepcionesLogIn(ResultadosLogIn.InvalidPassword);
        }
    }
}




