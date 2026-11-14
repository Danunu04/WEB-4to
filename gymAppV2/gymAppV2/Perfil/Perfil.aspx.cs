using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Web.UI;
using System.Web.UI.WebControls;
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
        private BLLPesoCorporal bllPesoCorporal;

        /// <summary>
        /// Usuario logueado actualmente. Se usa para asegurar que solo se modifiquen datos propios.
        /// </summary>
        private Usuario UsuarioActual
        {
            get { return Singleton.Instancia.Usuario; }
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            VerificarAcceso(BE.PermisosSistema.Perfil);

            bllUsuario = new BLLUsuario();
            bllEvento = new BLLEvento();
            bllPesoCorporal = new BLLPesoCorporal();

            if (!IsPostBack)
            {
                CargarDatosUsuario();
                CargarPesos();
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

        // ==================== PESO CORPORAL (CUN001) ====================

        /// <summary>Texto del botón de cada alumno (se evalúa al enlazar el Repeater).</summary>
        protected string TextoBotonPeso
        {
            get { return Texto("perfil_btn_registrar_peso", "Registrar peso"); }
        }

        /// <summary>
        /// Una fila por alumno activo de la cuenta, con su peso actual y los últimos registros.
        /// La tarjeta no se muestra si la cuenta no tiene alumnos.
        /// </summary>
        private void CargarPesos()
        {
            try
            {
                litPesoTitulo.Text = Texto("perfil_peso_titulo", "Peso corporal");
                litPesoSubtitulo.Text = Texto("perfil_peso_subtitulo", "Registrá el peso de tus alumnos para seguir su evolución.");

                List<Alumno> alumnos = UsuarioActual != null
                    ? bllPesoCorporal.ListarAlumnosDeUsuario(UsuarioActual.USUARIO_Usuario)
                    : new List<Alumno>();

                pnlPeso.Visible = alumnos.Count > 0;

                rptPesos.DataSource = alumnos.Select(a => new
                {
                    Dni = a.DNI,
                    NombreCompleto = $"{a.Nombre} {a.Apellido}",
                    TextoActual = a.Peso.HasValue
                        ? Texto("perfil_peso_actual", "Peso actual: {0} kg", a.Peso.Value.ToString("0.##"))
                        : Texto("perfil_peso_sin_dato", "Todavía no tiene peso registrado."),
                    TextoHistorial = TextoHistorial(bllPesoCorporal.ListarUltimos(a.DNI))
                }).ToList();
                rptPesos.DataBind();
            }
            catch (Exception ex)
            {
                MostrarMensaje(MensajeDePermisos(ex) ?? Texto("msg_error_generico", "Ocurrió un error inesperado. Intente nuevamente."), false);
            }
        }

        private string TextoHistorial(List<RegistroPeso> registros)
        {
            if (registros == null || registros.Count == 0)
                return string.Empty;

            string lista = string.Join(" · ", registros.Select(r => $"{r.Peso:0.##} kg ({r.Fecha:dd/MM/yyyy})"));
            return Texto("perfil_peso_historial", "Últimos registros: {0}", lista);
        }

        protected void rptPesos_ItemCommand(object source, RepeaterCommandEventArgs e)
        {
            if (e.CommandName != "Registrar" || UsuarioActual == null)
                return;

            try
            {
                int dni = Convert.ToInt32(e.CommandArgument);
                var txtPeso = (TextBox)e.Item.FindControl("txtPesoNuevo");
                string texto = (txtPeso.Text ?? string.Empty).Trim().Replace(',', '.');

                if (!decimal.TryParse(texto, NumberStyles.Number, CultureInfo.InvariantCulture, out decimal peso))
                {
                    MostrarMensaje(Texto("perfil_err_peso", "El peso debe ser un número mayor que 0 y menor que 500 kg."), false);
                    return;
                }

                bllPesoCorporal.RegistrarPeso(UsuarioActual.USUARIO_Usuario, dni, peso);

                MostrarMensaje(Texto("perfil_msg_peso", "Peso registrado: {0} kg.", peso.ToString("0.##")), true);
                CargarPesos();
            }
            catch (Exception ex)
            {
                MostrarMensaje(MensajeDePermisos(ex) ?? Texto("msg_error_generico", "Ocurrió un error inesperado. Intente nuevamente."), false);
            }
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
