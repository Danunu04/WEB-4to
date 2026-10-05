using System;
using System.Collections.Generic;
using System.Web.UI.WebControls;
using BLL;
using BE;
using gymAppV2;
using Servicios.Singleton;

namespace gymAppV2.Pagos
{
    public partial class PagosCliente : BasePage
    {
        private BLLPago bllPago;
        private BLLAlumno bllAlumno;
        private BLLPrecioModalidad bllPrecioModalidad;

        /// <summary>
        /// Indica si el usuario logueado es un Cliente (rol 4).
        /// </summary>
        protected bool EsCliente
        {
            get { return Singleton.Instancia.Usuario?.USUARIO_Rol == 4 || Singleton.Instancia.Usuario?.USUARIO_Rol == 6; }
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            VerificarAcceso(BE.PermisosSistema.Pagos);

            bllPago = new BLLPago();
            bllAlumno = new BLLAlumno();
            bllPrecioModalidad = new BLLPrecioModalidad();

            if (!IsPostBack)
            {
                AplicarIdioma();

                pnlCliente.Visible = EsCliente;
                pnlAdmin.Visible = !EsCliente;

                if (EsCliente)
                {
                    CargarPagosCliente();
                }
                else
                {
                    CargarPagos();
                }
            }
        }

        public override void OnIdiomaChanged(BE.IdiomaApp idioma)
        {
            base.OnIdiomaChanged(idioma);
            AplicarIdioma();
        }

        private void AplicarIdioma()
        {
            litTitulo.Text = T("pagos_titulo");
            litSubtitulo.Text = T("pagos_subtitulo");

            litClienteMsg.Text = T("pagos_cliente_msg");
            litListaClienteTitulo.Text = T("pagos_lista_cliente_titulo");

            litListaTitulo.Text = T("pagos_lista_titulo");
            litBtnRegistrar.Text = T("pagos_btn_registrar");
            litFormTitulo.Text = T("pagos_form_titulo");
            litCampoAlumno.Text = T("pagos_campo_alumno");
            litCampoModalidad.Text = T("pagos_campo_modalidad");
            litCampoPeriodo.Text = T("pagos_campo_periodo");
            litCampoPeriodoHint.Text = T("pagos_campo_periodo_hint");
            litCampoMetodo.Text = T("pagos_campo_metodo");
            litCampoMonto.Text = T("pagos_campo_monto");
            litBtnGuardar.Text = T("btn_guardar");
        }

        // ==================== VISTA CLIENTE ====================

        private void CargarPagosCliente()
        {
            try
            {
                string usuarioActual = Singleton.Instancia.Usuario?.USUARIO_Usuario;
                var pagos = bllPago.ListarPagosPorCliente(usuarioActual) ?? new List<Pago>();

                gvPagosCliente.DataSource = pagos;
                gvPagosCliente.DataBind();
            }
            catch (Exception)
            {
                MostrarError("Ocurrió un error inesperado. Intente nuevamente.");
            }
        }

        // ==================== VISTA ADMIN ====================

        private void CargarPagos()
        {
            try
            {
                var pagos = bllPago.ListarPagos() ?? new List<Pago>();

                gvPagos.DataSource = pagos;
                gvPagos.DataBind();

                footerText.InnerText = $"Mostrando {pagos.Count} de {pagos.Count}";
            }
            catch (Exception)
            {
                MostrarError("Ocurrió un error inesperado. Intente nuevamente.");
                footerText.InnerText = "Mostrando 0 de 0";
            }
        }

        private void CargarDropdowns()
        {
            try
            {
                ddlAlumno.Items.Clear();
                foreach (var alumno in bllAlumno.ListarAlumnos() ?? new List<Alumno>())
                {
                    ddlAlumno.Items.Add(new ListItem($"{alumno.Apellido}, {alumno.Nombre} (DNI: {alumno.DNI})", alumno.DNI.ToString()));
                }

                ddlModalidad.Items.Clear();
                foreach (var modalidad in bllPrecioModalidad.ListarModalidades() ?? new List<PrecioModalidad>())
                {
                    if (!modalidad.Activo) continue;
                    ddlModalidad.Items.Add(new ListItem($"{modalidad.ObtenerDescripcion()} — {modalidad.Precio:C}", modalidad.Id.ToString()));
                }
            }
            catch (Exception)
            {
                // Silencioso - los dropdowns quedan vacíos
            }

            ActualizarMontoPreview();
        }

        private void ActualizarMontoPreview()
        {
            try
            {
                if (ddlModalidad.SelectedIndex < 0 || !int.TryParse(ddlModalidad.SelectedValue, out int modalidadId))
                {
                    lblMontoPreview.Text = "—";
                    return;
                }

                var modalidad = bllPrecioModalidad.ObtenerModalidad(modalidadId);
                lblMontoPreview.Text = modalidad != null ? modalidad.Precio.ToString("C") : "—";
            }
            catch (Exception)
            {
                lblMontoPreview.Text = "—";
            }
        }

        // ==================== EVENTOS ====================

        protected void gvPagos_PageIndexChanging(object sender, GridViewPageEventArgs e)
        {
            gvPagos.PageIndex = e.NewPageIndex;
            CargarPagos();
        }

        protected void gvPagos_RowDataBound(object sender, GridViewRowEventArgs e)
        {
            if (e.Row.RowType == DataControlRowType.DataRow)
            {
                if (e.Row.Cells.Count > 1)
                    e.Row.Cells[1].Attributes["data-label"] = "Período";
                if (e.Row.Cells.Count > 2)
                    e.Row.Cells[2].Attributes["data-label"] = "Modalidad";
                if (e.Row.Cells.Count > 3)
                    e.Row.Cells[3].Attributes["data-label"] = "Monto";
                if (e.Row.Cells.Count > 4)
                    e.Row.Cells[4].Attributes["data-label"] = "Fecha de pago";
                if (e.Row.Cells.Count > 5)
                    e.Row.Cells[5].Attributes["data-label"] = "Método";
            }
        }

        protected void btnRegistrar_Click(object sender, EventArgs e)
        {
            try
            {
                LimpiarFormulario();
                CargarDropdowns();
                pnlForm.Visible = true;
            }
            catch (Exception)
            {
                MostrarError("Ocurrió un error inesperado. Intente nuevamente.");
            }
        }

        protected void btnCloseForm_Click(object sender, EventArgs e)
        {
            pnlForm.Visible = false;
        }

        protected void ddlModalidad_SelectedIndexChanged(object sender, EventArgs e)
        {
            ActualizarMontoPreview();
        }

        protected void btnGuardar_Click(object sender, EventArgs e)
        {
            try
            {
                if (ddlAlumno.SelectedIndex < 0 || !int.TryParse(ddlAlumno.SelectedValue, out int dni))
                {
                    MostrarError("Debe seleccionar un alumno.");
                    return;
                }

                if (ddlModalidad.SelectedIndex < 0 || !int.TryParse(ddlModalidad.SelectedValue, out int modalidadId))
                {
                    MostrarError("Debe seleccionar una modalidad.");
                    return;
                }

                if (string.IsNullOrEmpty(txtPeriodo.Text) || !DateTime.TryParse(txtPeriodo.Text, out DateTime periodo))
                {
                    MostrarError("El período es obligatorio y debe ser una fecha válida.");
                    return;
                }

                if (ddlMetodoPago.SelectedIndex < 0)
                {
                    MostrarError("Debe seleccionar un método de pago.");
                    return;
                }

                if (bllPago.ExistePagoEnPeriodo(dni, periodo))
                {
                    MostrarError("Ya existe un pago registrado para este alumno en este período.");
                    return;
                }

                string usuarioActual = Singleton.Instancia.Usuario?.USUARIO_Usuario;
                bllPago.RegistrarPago(dni, modalidadId, periodo, ddlMetodoPago.SelectedValue, usuarioActual);

                MostrarExito("Pago registrado correctamente.");

                pnlForm.Visible = false;
                CargarPagos();
            }
            catch (Exception)
            {
                MostrarError("Ocurrió un error inesperado. Intente nuevamente.");
            }
        }

        // ==================== MÉTODOS AUXILIARES ====================

        private void LimpiarFormulario()
        {
            txtPeriodo.Text = "";
            lblMontoPreview.Text = "—";
        }
    }
}
