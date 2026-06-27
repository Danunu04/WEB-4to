using System;
using System.Web.UI;
using BE;
using BLL;
using gymAppV2;
using Servicios.Singleton;

namespace gymAppV2.Perfil
{
    public partial class Perfil : BasePage
    {
        private BLLUsuario bllUsuario;
        private BLLEvento bllEvento;

        /// <summary>
        /// Usuario logueado actualmente. Se usa para asegurar que solo se modifiquen datos propios.
        /// </summary>
        private Usuario UsuarioActual
        {
            get { return Singleton.Instancia.Usuario; }
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            VerificarAcceso("Perfil");

            bllUsuario = new BLLUsuario();
            bllEvento = new BLLEvento();

            if (!IsPostBack)
            {
                CargarDatosUsuario();
            }
        }

        /// <summary>
        /// Carga los datos personales del usuario logueado en los controles del formulario.
        /// </summary>
        private void CargarDatosUsuario()
        {
            if (UsuarioActual == null)
            {
                Response.Redirect("~/LogIn/LogIn.aspx");
                return;
            }

            txtUsuario.Text = UsuarioActual.USUARIO_Usuario;
            txtDNI.Text = UsuarioActual.USUARIO_DNI.ToString();
            txtNombre.Text = UsuarioActual.Nombre ?? string.Empty;
            txtApellido.Text = UsuarioActual.Apellido ?? string.Empty;
            txtTelefono.Text = UsuarioActual.Telefono ?? string.Empty;
            txtEmail.Text = UsuarioActual.Email ?? string.Empty;
        }

        protected void btnGuardar_Click(object sender, EventArgs e)
        {
            try
            {
                if (UsuarioActual == null)
                {
                    Response.Redirect("~/LogIn/LogIn.aspx");
                    return;
                }

                // Validaciones básicas de los campos editables.
                string nombre = txtNombre.Text.Trim();
                string apellido = txtApellido.Text.Trim();
                string telefono = txtTelefono.Text.Trim();
                string email = txtEmail.Text.Trim();

                if (string.IsNullOrEmpty(nombre))
                {
                    MostrarMensaje("El nombre es obligatorio.", false);
                    return;
                }

                if (string.IsNullOrEmpty(apellido))
                {
                    MostrarMensaje("El apellido es obligatorio.", false);
                    return;
                }

                // Asegurar que el usuario solo pueda modificar su propio perfil.
                string usuarioActual = UsuarioActual.USUARIO_Usuario;

                bllUsuario.ModificarUsuario(
                    usuarioActual,
                    usuarioActual,
                    nombre,
                    apellido,
                    telefono,
                    email,
                    UsuarioActual.FechaNacimiento,
                    UsuarioActual.USUARIO_Rol,
                    UsuarioActual.USUARIO_Activo,
                    UsuarioActual.USUARIO_DNI);

                // Refrescar la sesión con los datos actualizados.
                Singleton.Instancia.LogIn(bllUsuario.ObtenerUsuario(usuarioActual));

                bllEvento.RegistrarCambioDatosUsuario(usuarioActual, "datos personales");

                MostrarMensaje("Tus datos fueron actualizados correctamente.", true);
            }
            catch (Exception ex)
            {
                MostrarMensaje("Error al guardar los cambios: " + ex.Message, false);
            }
        }

        protected void btnCancelar_Click(object sender, EventArgs e)
        {
            CargarDatosUsuario();
            pnlMensaje.Visible = false;
        }

        private void MostrarMensaje(string mensaje, bool exito)
        {
            pnlMensaje.CssClass = exito ? "perfil-message success" : "perfil-message error";
            pnlMensaje.Controls.Clear();
            pnlMensaje.Controls.Add(new System.Web.UI.LiteralControl(mensaje));
            pnlMensaje.Visible = true;
        }
    }
}
