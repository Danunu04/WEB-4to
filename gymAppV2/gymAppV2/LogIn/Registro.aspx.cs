using System;
using System.Globalization;
using System.Linq;
using System.Web.UI.WebControls;
using BE;
using BLL;
using Servicios.Singleton;

namespace gymAppV2.LogIn
{
    /// <summary>
    /// Registro de un cliente nuevo (RFN1 - Proceso de asociación, CUS002) en tres pasos: la
    /// cuenta, los datos del alumno y el pago del primer mes. Las validaciones y el alta las hace
    /// BLLRegistro; acá solo se arma el formulario.
    /// </summary>
    public partial class Registro : System.Web.UI.Page
    {
        // La contraseña no puede viajar en el ViewState (se ve en el navegador): entre el paso 1
        // y el pago queda en la sesión del servidor.
        private const string CLAVE_CONTRASENA = "Registro_Contrasena";

        private BLLRegistro bllRegistro;

        protected void Page_Load(object sender, EventArgs e)
        {
            bllRegistro = new BLLRegistro();

            if (!IsPostBack)
            {
                if (Singleton.Instancia.IsLogged())
                {
                    Redirigir("~/DashBoard/WebForm1.aspx");
                    return;
                }

                CargarModalidades();
                MostrarPaso(1);
            }
        }

        // ==================== PASOS ====================

        private void MostrarPaso(int paso)
        {
            pnlPaso1.Visible = paso == 1;
            pnlPaso2.Visible = paso == 2;
            pnlPaso3.Visible = paso == 3;
            pnlExito.Visible = paso == 4;
            lblPaso.Text = paso <= 3 ? $"Paso {paso} de 3" : string.Empty;
            lblPaso.Visible = paso <= 3;
        }

        protected void btnSiguiente1_Click(object sender, EventArgs e)
        {
            try
            {
                DatosRegistro datos = LeerDatos();
                datos.Contrasena = txtContrasena.Text;
                datos.ConfirmarContrasena = txtConfirmar.Text;

                bllRegistro.ValidarCuenta(datos);

                Session[CLAVE_CONTRASENA] = txtContrasena.Text;
                OcultarMensaje();
                MostrarPaso(2);
            }
            catch (Exception ex)
            {
                MostrarError(ex);
            }
        }

        protected void btnSiguiente2_Click(object sender, EventArgs e)
        {
            try
            {
                bllRegistro.ValidarAlumno(LeerDatos());

                OcultarMensaje();
                ActualizarMonto();
                MostrarPaso(3);
            }
            catch (Exception ex)
            {
                MostrarError(ex);
            }
        }

        protected void btnVolver2_Click(object sender, EventArgs e)
        {
            OcultarMensaje();
            MostrarPaso(1);
        }

        protected void btnVolver3_Click(object sender, EventArgs e)
        {
            // Los datos de la tarjeta no se conservan al salir del paso de pago
            LimpiarTarjeta();
            OcultarMensaje();
            MostrarPaso(2);
        }

        protected void ddlModalidad_SelectedIndexChanged(object sender, EventArgs e)
        {
            ActualizarMonto();
        }

        protected void rblMetodo_SelectedIndexChanged(object sender, EventArgs e)
        {
            bool esTarjeta = rblMetodo.SelectedValue == "Tarjeta";
            pnlTarjeta.Visible = esTarjeta;
            pnlTransferencia.Visible = !esTarjeta;
            if (!esTarjeta)
                LimpiarTarjeta();
        }

        protected void btnPagar_Click(object sender, EventArgs e)
        {
            string contrasena = Session[CLAVE_CONTRASENA] as string;
            if (string.IsNullOrEmpty(contrasena))
            {
                // La sesión venció entre pasos: hay que volver a elegir la contraseña
                MostrarPaso(1);
                MostrarMensaje("Por seguridad, volvé a ingresar la contraseña.");
                return;
            }

            try
            {
                DatosRegistro datos = LeerDatos();
                datos.Contrasena = contrasena;
                datos.ConfirmarContrasena = contrasena;

                Pago pago = bllRegistro.Registrar(datos);

                Session.Remove(CLAVE_CONTRASENA);
                LimpiarTarjeta();
                OcultarMensaje();
                litExito.Text = Server.HtmlEncode(
                    $"Tu usuario es {datos.Usuario}. El abono de {datos.Nombre} {datos.Apellido} está pago " +
                    $"del {pago.FechaInicio:dd/MM/yyyy} al {pago.FechaVencimiento:dd/MM/yyyy}.");
                MostrarPaso(4);
            }
            catch (Exception ex)
            {
                MostrarError(ex);
            }
        }

        protected void btnIrLogin_Click(object sender, EventArgs e)
        {
            Session.Remove(CLAVE_CONTRASENA);
            Redirigir("~/LogIn/LogIn.aspx");
        }

        // ==================== AUXILIARES ====================

        private void CargarModalidades()
        {
            try
            {
                ddlModalidad.Items.Clear();
                foreach (PrecioModalidad modalidad in new BLLPrecioModalidad().ListarModalidades().Where(m => m.Activo))
                {
                    ddlModalidad.Items.Add(new ListItem(
                        $"{modalidad.ObtenerDescripcion()} — {modalidad.Precio.ToString("C")}",
                        modalidad.Id.ToString()));
                }
            }
            catch (Exception ex)
            {
                MostrarError(ex);
            }
        }

        private void ActualizarMonto()
        {
            try
            {
                if (!int.TryParse(ddlModalidad.SelectedValue, out int id))
                {
                    litMonto.Text = "—";
                    litCobertura.Text = string.Empty;
                    return;
                }

                PrecioModalidad modalidad = new BLLPrecioModalidad().ObtenerModalidad(id);
                DateTime hoy = DateTime.Today;
                litMonto.Text = Server.HtmlEncode(modalidad?.Precio.ToString("C") ?? "—");
                litCobertura.Text = Server.HtmlEncode(
                    $"Cubre del {hoy:dd/MM/yyyy} al {hoy.AddDays(BLLPago.DIAS_ABONO - 1):dd/MM/yyyy}");
            }
            catch (Exception ex)
            {
                MostrarError(ex);
            }
        }

        private DatosRegistro LeerDatos()
        {
            int.TryParse((txtDni.Text ?? string.Empty).Trim(), out int dni);
            int.TryParse(ddlModalidad.SelectedValue, out int modalidadId);

            DateTime? fechaNacimiento = null;
            if (DateTime.TryParseExact((txtFechaNacimiento.Text ?? string.Empty).Trim(), "yyyy-MM-dd",
                    CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime fecha))
            {
                fechaNacimiento = fecha;
            }

            return new DatosRegistro
            {
                Usuario = (txtUsuario.Text ?? string.Empty).Trim(),
                Dni = dni,
                Nombre = (txtNombre.Text ?? string.Empty).Trim(),
                Apellido = (txtApellido.Text ?? string.Empty).Trim(),
                Telefono = (txtTelefono.Text ?? string.Empty).Trim(),
                Email = (txtEmail.Text ?? string.Empty).Trim(),
                FechaNacimiento = fechaNacimiento,
                ModalidadId = modalidadId,
                MetodoPago = rblMetodo.SelectedValue,
                TarjetaNumero = txtTarjetaNumero.Text,
                TarjetaTitular = txtTarjetaTitular.Text,
                TarjetaVence = txtTarjetaVence.Text,
                TarjetaCvv = txtTarjetaCvv.Text
            };
        }

        private void LimpiarTarjeta()
        {
            txtTarjetaNumero.Text = string.Empty;
            txtTarjetaTitular.Text = string.Empty;
            txtTarjetaVence.Text = string.Empty;
            txtTarjetaCvv.Text = string.Empty;
        }

        /// <summary>
        /// Las reglas de negocio se muestran tal cual; cualquier otro error, con un mensaje genérico.
        /// </summary>
        private void MostrarError(Exception ex)
        {
            for (Exception e = ex; e != null; e = e.InnerException)
            {
                if (e is ReglaNegocioException regla)
                {
                    MostrarMensaje(regla.Message);
                    return;
                }
            }

            System.Diagnostics.Trace.WriteLine($"[Registro] {ex}");
            MostrarMensaje("Ocurrió un error inesperado. Intentá de nuevo en unos minutos.");
        }

        private void MostrarMensaje(string mensaje)
        {
            lblMensaje.Text = Server.HtmlEncode(mensaje);
            lblMensaje.Visible = true;
        }

        private void OcultarMensaje()
        {
            lblMensaje.Visible = false;
        }

        private void Redirigir(string url)
        {
            Response.Redirect(ResolveUrl(url), false);
            Context.ApplicationInstance.CompleteRequest();
        }
    }
}
