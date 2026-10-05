using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.UI;
using System.Web.UI.WebControls;
using BLL;
using BE;
using gymAppV2;
using Servicios.Singleton;

namespace gymAppV2.Entrenadores
{
    public partial class Entrenadores : BasePage
    {
        private BLLEntrenador bllEntrenador;
        private BLLUsuario bllUsuario;
        private BLLEvento bllEvento;
        // Propiedades respaldadas por ViewState: una propiedad de C# común no sobrevive
        // entre dos postbacks distintos (ej. click en la fila para seleccionar, y luego
        // click en "Eliminar"/"Guardar" en un postback aparte) porque cada postback crea
        // una instancia nueva de la página. ViewState sí viaja en el __VIEWSTATE oculto.
        private int? DniSeleccionado
        {
            get { return ViewState["DniSeleccionado"] as int?; }
            set { ViewState["DniSeleccionado"] = value; }
        }

        private bool EsModificacion
        {
            get { return ViewState["EsModificacion"] as bool? ?? false; }
            set { ViewState["EsModificacion"] = value; }
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            VerificarAcceso(BE.PermisosSistema.GestionEntrenadores);

            bllEntrenador = new BLLEntrenador();
            bllUsuario = new BLLUsuario();
            bllEvento = new BLLEvento();

            if (!IsPostBack)
            {
                AplicarIdioma();
                CargarEntrenadores();
            }
        }

        public override void OnIdiomaChanged(IdiomaApp idioma)
        {
            base.OnIdiomaChanged(idioma);
            AplicarIdioma();
            CargarEntrenadores();
        }

        private void AplicarIdioma()
        {
            litTitulo.Text         = T("entrenadores_titulo");
            litStatTotal.Text      = T("entrenadores_stat_total");
            litStatActivos.Text    = T("entrenadores_stat_activos");
            litStatConAlumnos.Text = T("entrenadores_stat_con_alumnos");
            litStatSinUsuario.Text = T("entrenadores_stat_sin_usuario");
            litListaTitulo.Text    = T("entrenadores_lista_titulo");
            litBtnCrear.Text       = T("entrenadores_btn_crear");
            litBtnModificar.Text   = T("entrenadores_btn_modificar");
            litBtnEliminar.Text    = T("entrenadores_btn_eliminar");
            litBtnCancelar.Text    = T("btn_cancelar");
            litBtnGuardar.Text     = T("btn_guardar");
            litConfirmarTitulo.Text = T("entrenadores_confirmar_elim_titulo");
            litConfirmarMsg.Text     = T("entrenadores_confirmar_elim_msg");
            litConfirmarAviso.Text   = T("entrenadores_confirmar_elim_aviso");
            litBtnCancelarEliminar.Text  = T("btn_cancelar");
            litBtnConfirmarEliminar.Text = T("entrenadores_btn_eliminar");

            ((TemplateField)gvEntrenadores.Columns[1]).HeaderText = T("entrenadores_col_entrenador");
            ((TemplateField)gvEntrenadores.Columns[6]).HeaderText = T("entrenadores_col_estado");
        }

        // ==================== MÉTODOS PRINCIPALES ====================

        private void CargarEntrenadores()
        {
            try
            {
                var entrenadores = bllEntrenador.ListarEntrenadores() ?? new List<Entrenador>();

                gvEntrenadores.DataSource = entrenadores;
                gvEntrenadores.DataBind();

                var stats = bllEntrenador.ObtenerEstadisticas();
                lblTotal.Text = stats["Total"].ToString();
                lblActivos.Text = stats["Activos"].ToString();
                lblConAlumnos.Text = stats["ConAlumnos"].ToString();
                lblSinUsuario.Text = stats["SinUsuario"].ToString();

                badgeCount.InnerText = lblTotal.Text;
                footerText.InnerText = $"Mostrando {entrenadores.Count} de {entrenadores.Count}";
            }
            catch (Exception)
            {
                MostrarError("Ocurrió un error inesperado. Intente nuevamente.");
                footerText.InnerText = "Mostrando 0 de 0";
            }
        }

        // ==================== EVENTOS DE GRIDVIEW ====================

        protected void gvEntrenadores_PageIndexChanging(object sender, GridViewPageEventArgs e)
        {
            gvEntrenadores.PageIndex = e.NewPageIndex;
            CargarEntrenadores();
        }

        protected void gvEntrenadores_RowCommand(object sender, GridViewCommandEventArgs e)
        {
            try
            {
                if (e.CommandName == "Select")
                {
                    GridViewRow row = (GridViewRow)((LinkButton)e.CommandSource).NamingContainer;
                    int? dni = gvEntrenadores.DataKeys[row.RowIndex]?.Value as int?;

                    if (dni.HasValue)
                    {
                        DniSeleccionado = dni.Value;
                        foreach (GridViewRow r in gvEntrenadores.Rows)
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

        protected void gvEntrenadores_RowDataBound(object sender, GridViewRowEventArgs e)
        {
            if (e.Row.RowType == DataControlRowType.DataRow)
            {
                if (e.Row.Cells.Count > 1)
                    e.Row.Cells[1].Attributes["data-label"] = "Teléfono";
                if (e.Row.Cells.Count > 2)
                    e.Row.Cells[2].Attributes["data-label"] = "Fecha Nacimiento";
                if (e.Row.Cells.Count > 3)
                    e.Row.Cells[3].Attributes["data-label"] = "Alumnos";
                if (e.Row.Cells.Count > 4)
                    e.Row.Cells[4].Attributes["data-label"] = "Usuario";
                if (e.Row.Cells.Count > 5)
                    e.Row.Cells[5].Attributes["data-label"] = "Estado";
            }
        }

        // ==================== EVENTOS DE ACCIONES ====================

        protected void btnCrear_Click(object sender, EventArgs e)
        {
            try
            {
                LimpiarFormulario();
                lblFormTitle.Text = T("entrenadores_form_nuevo");
                EsModificacion = false;
                DniSeleccionado = null;
                txtDNI.Enabled = true;
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
                if (!DniSeleccionado.HasValue)
                {
                    MostrarError("Debe seleccionar un entrenador de la lista.");
                    return;
                }

                var entrenador = bllEntrenador.ObtenerEntrenador(DniSeleccionado.Value);
                if (entrenador == null)
                {
                    MostrarError("El entrenador seleccionado ya no existe.");
                    return;
                }

                EsModificacion = true;

                txtDNI.Text = entrenador.DNI.ToString();
                txtDNI.Enabled = false;
                txtNombre.Text = entrenador.Nombre ?? "";
                txtApellido.Text = entrenador.Apellido ?? "";
                txtTelefono.Text = entrenador.Telefono ?? "";
                txtFechaNacimiento.Text = entrenador.FechaNacimiento?.ToString("yyyy-MM-dd") ?? "";
                chkActivo.Checked = entrenador.Activo;

                lblFormTitle.Text = T("entrenadores_form_modificar");
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
                if (!DniSeleccionado.HasValue)
                {
                    MostrarError("Debe seleccionar un entrenador para eliminar.");
                    return;
                }

                var entrenador = bllEntrenador.ObtenerEntrenador(DniSeleccionado.Value);
                if (entrenador == null)
                {
                    MostrarError("El entrenador seleccionado ya no existe.");
                    return;
                }

                lblEntrenadorAEliminar.Text = $"{entrenador.Apellido}, {entrenador.Nombre} (DNI: {entrenador.DNI})";
                hdnDniAEliminar.Value = entrenador.DNI.ToString();
                pnlConfirmarEliminar.Visible = true;
            }
            catch (Exception)
            {
                MostrarError("Ocurrió un error inesperado. Intente nuevamente.");
            }
        }

        protected void btnCancelar_Click(object sender, EventArgs e)
        {
            DniSeleccionado = null;
            CargarEntrenadores();
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
                if (string.IsNullOrEmpty(txtDNI.Text))
                {
                    MostrarError("El DNI es obligatorio.");
                    return;
                }

                if (!int.TryParse(txtDNI.Text, out int dni))
                {
                    MostrarError("El DNI ingresado no es válido.");
                    return;
                }

                if (string.IsNullOrEmpty(txtNombre.Text))
                {
                    MostrarError("El nombre es obligatorio.");
                    return;
                }

                if (string.IsNullOrEmpty(txtApellido.Text))
                {
                    MostrarError("El apellido es obligatorio.");
                    return;
                }

                if (string.IsNullOrEmpty(txtFechaNacimiento.Text))
                {
                    MostrarError("La fecha de nacimiento es obligatoria.");
                    return;
                }

                if (!DateTime.TryParse(txtFechaNacimiento.Text, out DateTime fechaNacimiento))
                {
                    MostrarError("La fecha de nacimiento ingresada no es válida.");
                    return;
                }

                if (fechaNacimiento > DateTime.Now)
                {
                    MostrarError("La fecha de nacimiento no puede ser futura.");
                    return;
                }

                string telefono = string.IsNullOrEmpty(txtTelefono.Text) ? null : txtTelefono.Text;

                if (EsModificacion)
                {
                    var entrenador = bllEntrenador.ObtenerEntrenador(DniSeleccionado.Value);
                    if (entrenador == null)
                    {
                        MostrarError("El entrenador seleccionado ya no existe.");
                        return;
                    }

                    entrenador.Activo = chkActivo.Checked;
                    bllEntrenador.ActualizarEntrenador(entrenador);

                    MostrarExito("Entrenador modificado correctamente.");
                }
                else
                {
                    if (bllEntrenador.ObtenerEntrenador(dni) != null)
                    {
                        MostrarError("Ya existe un entrenador con ese DNI.");
                        return;
                    }

                    if (bllUsuario.DniExiste(dni))
                    {
                        MostrarError("Ese DNI ya está registrado con otro tipo de usuario (Alumno, Administrador, etc.).");
                        return;
                    }

                    string usuarioName = $"entrenador_{dni}";
                    string contrasena = bllUsuario.GenerarContrasenaSegura();

                    bllUsuario.CrearUsuario(
                        usuarioName,
                        contrasena,
                        3, // Rol Entrenador
                        txtNombre.Text.Trim(),
                        txtApellido.Text.Trim(),
                        telefono,
                        null, // email
                        fechaNacimiento,
                        new Entrenador { DNI = dni }, // datosEntrenador
                        null  // dniAlumno
                    );

                    var entrenador = bllEntrenador.ObtenerEntrenador(dni);
                    if (entrenador != null)
                    {
                        entrenador.Activo = chkActivo.Checked;
                        bllEntrenador.ActualizarEntrenador(entrenador);
                    }

                    MostrarExito("Entrenador creado correctamente.");
                }

                pnlForm.Visible = false;
                CargarEntrenadores();
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
                if (!int.TryParse(hdnDniAEliminar.Value, out int dni))
                {
                    MostrarError("Ocurrió un error inesperado. Intente nuevamente.");
                    return;
                }

                bllEntrenador.EliminarEntrenador(dni);
                MostrarExito("Entrenador eliminado correctamente.");

                pnlConfirmarEliminar.Visible = false;
                CargarEntrenadores();
            }
            catch (InscripcionException ex)
            {
                // Es titular de algún turno
                pnlConfirmarEliminar.Visible = false;
                MostrarError(ex.Message);
            }
            catch (Exception)
            {
                MostrarError("Ocurrió un error inesperado. Intente nuevamente.");
            }
        }

        // ==================== MÉTODOS AUXILIARES ====================

        private void LimpiarFormulario()
        {
            txtDNI.Text = "";
            txtNombre.Text = "";
            txtApellido.Text = "";
            txtTelefono.Text = "";
            txtFechaNacimiento.Text = "";
            chkActivo.Checked = true;
        }

        // ==================== MÉTODOS PARA EL GRIDVIEW ====================

        protected string GetInitials(object nombre, object apellido)
        {
            string n = nombre?.ToString() ?? "";
            string a = apellido?.ToString() ?? "";
            if (string.IsNullOrEmpty(n) || string.IsNullOrEmpty(a))
                return "--";
            return (n[0].ToString() + a[0].ToString()).ToUpper();
        }

        protected string GetAvatarClass(int index)
        {
            string[] classes = { "av-lavender", "av-mint", "av-pink", "av-peach", "av-sky" };
            return classes[index % classes.Length];
        }

        protected string GetUsuarioClass(object usuario)
        {
            string u = usuario?.ToString() ?? "";
            return string.IsNullOrEmpty(u) ? "user-without" : "user-with";
        }

        protected string GetEstadoClass(object activo)
        {
            bool a = Convert.ToBoolean(activo);
            return a ? "pill-active" : "pill-inactive";
        }

        protected string GetEstadoText(object activo)
        {
            bool a = Convert.ToBoolean(activo);
            return a ? T("entrenadores_estado_activo") : T("entrenadores_estado_inactivo");
        }
    }
}
