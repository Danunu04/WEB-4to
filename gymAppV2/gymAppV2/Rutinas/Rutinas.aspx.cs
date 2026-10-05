using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.UI;
using System.Web.UI.WebControls;
using BLL;
using BE;
using gymAppV2;
using Servicios.Singleton;

namespace gymAppV2.Rutinas
{
    public partial class Rutinas : BasePage
    {
        private BLLRutina bllRutina;
        private BLLAlumno bllAlumno;
        private BLLEntrenador bllEntrenador;
        private BLLActividad bllActividad;

        // Propiedades respaldadas por ViewState: una propiedad de C# común no sobrevive
        // entre dos postbacks distintos (ej. click en la fila para seleccionar, y luego
        // click en "Eliminar"/"Guardar" en un postback aparte) porque cada postback crea
        // una instancia nueva de la página. ViewState sí viaja en el __VIEWSTATE oculto.
        private int? CodRutinaSeleccionado
        {
            get { return ViewState["CodRutinaSeleccionado"] as int?; }
            set { ViewState["CodRutinaSeleccionado"] = value; }
        }

        private bool EsModificacion
        {
            get { return ViewState["EsModificacion"] as bool? ?? false; }
            set { ViewState["EsModificacion"] = value; }
        }

        /// <summary>
        /// Indica si el usuario logueado es un Cliente (rol 4).
        /// </summary>
        protected bool EsCliente
        {
            get { return Singleton.Instancia.Usuario?.USUARIO_Rol == 4 || Singleton.Instancia.Usuario?.USUARIO_Rol == 6; }
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            VerificarAcceso(BE.PermisosSistema.GestionRutinas);

            bllRutina = new BLLRutina();
            bllAlumno = new BLLAlumno();
            bllEntrenador = new BLLEntrenador();
            bllActividad = new BLLActividad();

            if (!IsPostBack)
            {
                AplicarIdioma();

                pnlCliente.Visible = EsCliente;
                pnlAdmin.Visible = !EsCliente;

                if (EsCliente)
                {
                    CargarRutinasCliente();
                }
                else
                {
                    CargarRutinas();
                }
            }
        }

        public override void OnIdiomaChanged(IdiomaApp idioma)
        {
            base.OnIdiomaChanged(idioma);
            AplicarIdioma();
        }

        private void AplicarIdioma()
        {
            litTitulo.Text     = T("rutinas_titulo");
            litSubtitulo.Text  = T("rutinas_subtitulo");
            litClienteMsg.Text = T("rutinas_cliente_msg");

            litListaClienteTitulo.Text = T("rutinas_lista_cliente_titulo");
            litListaTitulo.Text        = T("rutinas_lista_titulo");
            litBtnCrear.Text           = T("rutinas_btn_crear");
            litBtnModificar.Text       = T("rutinas_btn_modificar");
            litBtnEliminar.Text        = T("rutinas_btn_eliminar");
            litBtnCancelar.Text        = T("btn_cancelar");
            litBtnGuardar.Text         = T("btn_guardar");
            litConfirmarTitulo.Text        = T("rutinas_confirmar_elim_titulo");
            litConfirmarMsg.Text           = T("rutinas_confirmar_elim_msg");
            litBtnCancelarEliminar.Text     = T("btn_cancelar");
            litBtnConfirmarEliminar.Text    = T("rutinas_btn_eliminar");
        }

        // ==================== VISTA CLIENTE ====================

        private void CargarRutinasCliente()
        {
            try
            {
                string usuarioActual = Singleton.Instancia.Usuario?.USUARIO_Usuario;
                var rutinas = bllRutina.ListarRutinasPorCliente(usuarioActual) ?? new List<Rutina>();

                gvRutinasCliente.DataSource = rutinas;
                gvRutinasCliente.DataBind();
            }
            catch (Exception)
            {
                MostrarError("Ocurrió un error inesperado. Intente nuevamente.");
            }
        }

        // ==================== VISTA ADMIN ====================

        private void CargarRutinas()
        {
            try
            {
                var rutinas = bllRutina.ListarRutinas() ?? new List<Rutina>();

                gvRutinas.DataSource = rutinas;
                gvRutinas.DataBind();

                footerText.InnerText = $"Mostrando {rutinas.Count} de {rutinas.Count}";
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

                ddlEntrenador.Items.Clear();
                foreach (var entrenador in bllEntrenador.ListarEntrenadores() ?? new List<Entrenador>())
                {
                    ddlEntrenador.Items.Add(new ListItem($"{entrenador.Apellido}, {entrenador.Nombre} (DNI: {entrenador.DNI})", entrenador.DNI.ToString()));
                }

                ddlActividad.Items.Clear();
                foreach (var actividad in bllActividad.ListarActividades() ?? new List<Actividad>())
                {
                    ddlActividad.Items.Add(new ListItem(actividad.Descripcion, actividad.CodActividad.ToString()));
                }
            }
            catch (Exception)
            {
                // Silencioso - los dropdowns quedan vacíos
            }
        }

        // ==================== EVENTOS DE GRIDVIEW ====================

        protected void gvRutinas_PageIndexChanging(object sender, GridViewPageEventArgs e)
        {
            gvRutinas.PageIndex = e.NewPageIndex;
            CargarRutinas();
        }

        protected void gvRutinas_RowCommand(object sender, GridViewCommandEventArgs e)
        {
            try
            {
                if (e.CommandName == "Select")
                {
                    GridViewRow row = (GridViewRow)((LinkButton)e.CommandSource).NamingContainer;
                    int? codRutina = gvRutinas.DataKeys[row.RowIndex]?.Value as int?;

                    if (codRutina.HasValue)
                    {
                        CodRutinaSeleccionado = codRutina.Value;
                        foreach (GridViewRow r in gvRutinas.Rows)
                            r.CssClass = "";
                        row.CssClass = "selected-row";
                    }
                }
            }
            catch (Exception)
            {
                MostrarError("Ocurrió un error inesperado. Intente nuevamente.");
            }
        }

        protected void gvRutinas_RowDataBound(object sender, GridViewRowEventArgs e)
        {
            if (e.Row.RowType == DataControlRowType.DataRow)
            {
                if (e.Row.Cells.Count > 1)
                    e.Row.Cells[1].Attributes["data-label"] = "Fecha";
                if (e.Row.Cells.Count > 2)
                    e.Row.Cells[2].Attributes["data-label"] = "Alumno";
                if (e.Row.Cells.Count > 3)
                    e.Row.Cells[3].Attributes["data-label"] = "Entrenador";
                if (e.Row.Cells.Count > 4)
                    e.Row.Cells[4].Attributes["data-label"] = "Actividad";
                if (e.Row.Cells.Count > 5)
                    e.Row.Cells[5].Attributes["data-label"] = "Descripción";
            }
        }

        // ==================== EVENTOS DE ACCIONES ====================

        protected void btnCrear_Click(object sender, EventArgs e)
        {
            try
            {
                LimpiarFormulario();
                CargarDropdowns();
                lblFormTitle.Text = T("rutinas_form_nuevo");
                EsModificacion = false;
                CodRutinaSeleccionado = null;
                pnlForm.Visible = true;
            }
            catch (Exception)
            {
                MostrarError("Ocurrió un error inesperado. Intente nuevamente.");
            }
        }

        protected void btnModificar_Click(object sender, EventArgs e)
        {
            try
            {
                if (!CodRutinaSeleccionado.HasValue)
                {
                    MostrarError("Debe seleccionar una rutina de la lista.");
                    return;
                }

                var rutina = bllRutina.ObtenerRutina(CodRutinaSeleccionado.Value);
                if (rutina == null)
                {
                    MostrarError("La rutina seleccionada ya no existe.");
                    return;
                }

                EsModificacion = true;
                CargarDropdowns();

                ddlAlumno.SelectedValue = rutina.DniAlumno.ToString();
                ddlEntrenador.SelectedValue = rutina.DniEntrenador.ToString();
                ddlActividad.SelectedValue = rutina.CodActividad.ToString();
                txtFecha.Text = rutina.Fecha.ToString("yyyy-MM-dd");
                txtDescripcion.Text = rutina.Descripcion;

                lblFormTitle.Text = T("rutinas_form_modificar");
                pnlForm.Visible = true;
            }
            catch (Exception)
            {
                MostrarError("Ocurrió un error inesperado. Intente nuevamente.");
            }
        }

        protected void btnEliminar_Click(object sender, EventArgs e)
        {
            try
            {
                if (!CodRutinaSeleccionado.HasValue)
                {
                    MostrarError("Debe seleccionar una rutina para eliminar.");
                    return;
                }

                var rutina = bllRutina.ObtenerRutina(CodRutinaSeleccionado.Value);
                if (rutina == null)
                {
                    MostrarError("La rutina seleccionada ya no existe.");
                    return;
                }

                lblRutinaAEliminar.Text = $"{rutina.ActividadDescripcion} - {rutina.AlumnoApellido}, {rutina.AlumnoNombre} ({rutina.Fecha:dd/MM/yyyy})";
                hdnCodRutinaAEliminar.Value = rutina.CodRutina.ToString();
                pnlConfirmarEliminar.Visible = true;
            }
            catch (Exception)
            {
                MostrarError("Ocurrió un error inesperado. Intente nuevamente.");
            }
        }

        protected void btnCancelar_Click(object sender, EventArgs e)
        {
            CodRutinaSeleccionado = null;
            CargarRutinas();
        }

        protected void btnCloseForm_Click(object sender, EventArgs e)
        {
            pnlForm.Visible = false;
        }

        protected void btnCloseConfirm_Click(object sender, EventArgs e)
        {
            pnlConfirmarEliminar.Visible = false;
        }

        protected void btnCancelarEliminar_Click(object sender, EventArgs e)
        {
            pnlConfirmarEliminar.Visible = false;
        }

        protected void btnGuardar_Click(object sender, EventArgs e)
        {
            try
            {
                if (ddlAlumno.SelectedIndex < 0 || !int.TryParse(ddlAlumno.SelectedValue, out int dniAlumno))
                {
                    MostrarError("Debe seleccionar un alumno.");
                    return;
                }

                if (ddlEntrenador.SelectedIndex < 0 || !int.TryParse(ddlEntrenador.SelectedValue, out int dniEntrenador))
                {
                    MostrarError("Debe seleccionar un entrenador.");
                    return;
                }

                if (ddlActividad.SelectedIndex < 0 || !int.TryParse(ddlActividad.SelectedValue, out int codActividad))
                {
                    MostrarError("Debe seleccionar una actividad.");
                    return;
                }

                if (string.IsNullOrEmpty(txtFecha.Text) || !DateTime.TryParse(txtFecha.Text, out DateTime fecha))
                {
                    MostrarError("La fecha es obligatoria y debe ser válida.");
                    return;
                }

                if (string.IsNullOrWhiteSpace(txtDescripcion.Text))
                {
                    MostrarError("La descripción es obligatoria.");
                    return;
                }

                if (EsModificacion)
                {
                    var rutina = bllRutina.ObtenerRutina(CodRutinaSeleccionado.Value);
                    if (rutina == null)
                    {
                        MostrarError("La rutina seleccionada ya no existe.");
                        return;
                    }

                    rutina.DniAlumno = dniAlumno;
                    rutina.DniEntrenador = dniEntrenador;
                    rutina.CodActividad = codActividad;
                    rutina.Fecha = fecha;
                    rutina.Descripcion = txtDescripcion.Text.Trim();

                    bllRutina.ActualizarRutina(rutina);
                    MostrarExito("Rutina modificada correctamente.");
                }
                else
                {
                    var rutina = new Rutina
                    {
                        DniAlumno = dniAlumno,
                        DniEntrenador = dniEntrenador,
                        CodActividad = codActividad,
                        Fecha = fecha,
                        Descripcion = txtDescripcion.Text.Trim()
                    };

                    bllRutina.CrearRutina(rutina);
                    MostrarExito("Rutina creada correctamente.");
                }

                pnlForm.Visible = false;
                CargarRutinas();
            }
            catch (Exception)
            {
                MostrarError("Ocurrió un error inesperado. Intente nuevamente.");
            }
        }

        protected void btnConfirmarEliminar_Click(object sender, EventArgs e)
        {
            try
            {
                if (!int.TryParse(hdnCodRutinaAEliminar.Value, out int codRutina))
                {
                    MostrarError("Ocurrió un error inesperado. Intente nuevamente.");
                    return;
                }

                bllRutina.EliminarRutina(codRutina);
                MostrarExito("Rutina eliminada correctamente.");

                pnlConfirmarEliminar.Visible = false;
                CargarRutinas();
            }
            catch (Exception)
            {
                MostrarError("Ocurrió un error inesperado. Intente nuevamente.");
            }
        }

        // ==================== MÉTODOS AUXILIARES ====================

        private void LimpiarFormulario()
        {
            txtFecha.Text = "";
            txtDescripcion.Text = "";
        }
    }
}
