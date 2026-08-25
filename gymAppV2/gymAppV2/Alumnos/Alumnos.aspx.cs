using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.UI;
using System.Web.UI.WebControls;
using BLL;
using BE;
using gymAppV2;
using Servicios.Singleton;

namespace gymAppV2.Alumnos
{
    public partial class Alumnos : BasePage
    {
        private BLLAlumno bllAlumno;
        private BLLUsuario bllUsuario;
        private BLLEvento bllEvento;
        private int? DniSeleccionado { get; set; }
        private bool EsModificacion { get; set; }

        private bool EsSoloLectura
        {
            get { return Singleton.Instancia.Usuario?.USUARIO_Rol == 4; }
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            VerificarAcceso(BE.PermisosSistema.GestionAlumnos);

            bllAlumno = new BLLAlumno();
            bllUsuario = new BLLUsuario();
            bllEvento = new BLLEvento();

            if (!IsPostBack)
            {
                AplicarIdioma();
                CargarAlumnos();
                CargarUsuariosDropdown();
                ConfigurarModoSoloLectura();
            }
        }

        public override void OnIdiomaChanged(IdiomaApp idioma)
        {
            base.OnIdiomaChanged(idioma);
            AplicarIdioma();
            CargarAlumnos();
        }

        private void AplicarIdioma()
        {
            litTitulo.Text          = T("alumnos_titulo");
            litStatTotal.Text       = T("alumnos_stat_total");
            litStatActivos.Text     = T("alumnos_stat_activos");
            litStatConRutinas.Text  = T("alumnos_stat_con_rutinas");
            litStatSinUsuario.Text  = T("alumnos_stat_sin_usuario");
            litListaTitulo.Text     = T("alumnos_lista_titulo");
            litBtnCrear.Text        = T("alumnos_btn_crear");
            litBtnModificar.Text    = T("alumnos_btn_modificar");
            litBtnEliminar.Text     = T("alumnos_btn_eliminar");
            litBtnAsociar.Text      = T("alumnos_btn_asociar");
            litBtnCancelar.Text     = T("btn_cancelar");
            litBtnGuardar.Text      = T("btn_guardar");
            lblFormTitle.Text       = T("alumnos_form_titulo");
            litConfirmarTitulo.Text = T("alumnos_confirmar_elim_titulo");
            litConfirmarMsg.Text    = T("alumnos_confirmar_elim_msg");
            litConfirmarAviso.Text  = T("alumnos_confirmar_elim_aviso");
            litBtnCancelarEliminar.Text  = T("btn_cancelar");
            litBtnConfirmarEliminar.Text = T("alumnos_btn_eliminar");

            ((TemplateField)gvAlumnos.Columns[1]).HeaderText = T("alumnos_col_alumno");
            ((TemplateField)gvAlumnos.Columns[6]).HeaderText = T("alumnos_col_estado");

            // Opciones de dropdowns de filtros
            ddlEstado.Items[0].Text = T("alumnos_filtro_todos");
            ddlEstado.Items[1].Text = T("alumnos_filtro_activos");
            ddlEstado.Items[2].Text = T("alumnos_filtro_inactivos");

            // ddlUsuario se carga en CargarUsuariosDropdown con T() para el primer load.
            // En OnIdiomaChanged los items ya existen: traducimos sus textos directamente.
            if (ddlUsuario.Items.Count >= 3)
            {
                ddlUsuario.Items[0].Text = T("alumnos_filtro_todos");
                ddlUsuario.Items[1].Text = T("alumnos_filtro_con_usuario");
                ddlUsuario.Items[2].Text = T("alumnos_filtro_sin_usuario");
            }

            txtBusqueda.Attributes["placeholder"] = T("alumnos_buscar_placeholder");
        }

        private void ConfigurarModoSoloLectura()
        {
            if (EsSoloLectura)
            {
                btnCrear.Visible = false;
                btnModificar.Visible = false;
                btnEliminar.Visible = false;
                btnAsociarUsuario.Visible = false;
                pnlForm.Visible = false;
                pnlConfirmarEliminar.Visible = false;
            }
        }

        // ==================== MÉTODOS PRINCIPALES ====================

        private void CargarAlumnos()
        {
            try
            {
                var alumnos = bllAlumno.ListarAlumnos() ?? new List<Alumno>();

                // Un Cliente solo ve los alumnos asociados a su usuario
                if (EsSoloLectura)
                {
                    string usuarioActual = Singleton.Instancia.Usuario?.USUARIO_Usuario;
                    alumnos = alumnos.Where(a => !string.IsNullOrEmpty(a.Usuario)
                        && a.Usuario.Equals(usuarioActual, StringComparison.OrdinalIgnoreCase)).ToList();
                }

                // Aplicar filtros
                if (!string.IsNullOrEmpty(ddlEstado.SelectedValue))
                {
                    bool activo = ddlEstado.SelectedValue == "activo";
                    alumnos = alumnos.Where(a => a.Activo == activo).ToList();
                }

                if (!string.IsNullOrEmpty(ddlUsuario.SelectedValue) && !EsSoloLectura)
                {
                    string filtro = ddlUsuario.SelectedValue;
                    if (filtro == "con_usuario")
                        alumnos = alumnos.Where(a => !string.IsNullOrEmpty(a.Usuario)).ToList();
                    else if (filtro == "sin_usuario")
                        alumnos = alumnos.Where(a => string.IsNullOrEmpty(a.Usuario)).ToList();
                }

                if (!string.IsNullOrEmpty(txtBusqueda.Text))
                {
                    string busqueda = txtBusqueda.Text.ToLower();
                    alumnos = alumnos.Where(a =>
                        a.DNI.ToString().Contains(busqueda) ||
                        a.Nombre.ToLower().Contains(busqueda) ||
                        a.Apellido.ToLower().Contains(busqueda)
                    ).ToList();
                }

                gvAlumnos.DataSource = alumnos;
                gvAlumnos.DataBind();

                lblTotal.Text = alumnos.Count.ToString();
                lblActivos.Text = alumnos.Count(a => a.Activo).ToString();
                lblConRutinas.Text = alumnos.Count(a => a.TieneRutinas).ToString();
                lblSinUsuario.Text = alumnos.Count(a => string.IsNullOrEmpty(a.Usuario)).ToString();

                badgeCount.InnerText = lblTotal.Text;
                footerText.InnerText = string.Format(T("msg_mostrando_fmt"), alumnos.Count, alumnos.Count);
            }
            catch (Exception)
            {
                MostrarError(T("msg_error_generico"));
                footerText.InnerText = string.Format(T("msg_mostrando_fmt"), 0, 0);
            }
        }

        private void CargarUsuariosDropdown()
        {
            try
            {
                ddlUsuario.Items.Clear();
                ddlUsuario.Items.Add(new ListItem(T("alumnos_filtro_todos"),       ""));
                ddlUsuario.Items.Add(new ListItem(T("alumnos_filtro_con_usuario"), "con_usuario"));
                ddlUsuario.Items.Add(new ListItem(T("alumnos_filtro_sin_usuario"), "sin_usuario"));
            }
            catch (Exception)
            {
                // Silencioso
            }
        }

        private void CargarUsuariosDisponibles()
        {
            try
            {
                var usuarios = bllUsuario.ListarUsuariosClientesDisponibles();
                ddlUsuarioAsociar.Items.Clear();
                ddlUsuarioAsociar.Items.Add(new ListItem(T("alumnos_sin_asociar"), ""));

                foreach (var usuario in usuarios)
                {
                    string texto = $"{usuario.USUARIO_Usuario} ({usuario.Apellido}, {usuario.Nombre})";
                    ddlUsuarioAsociar.Items.Add(new ListItem(texto, usuario.USUARIO_Usuario));
                }
            }
            catch (Exception)
            {
                // Silencioso - el dropdown queda vacío
            }
        }

        // ==================== EVENTOS DE FILTROS ====================

        protected void ddlEstado_SelectedIndexChanged(object sender, EventArgs e)
        {
            CargarAlumnos();
        }

        protected void ddlUsuario_SelectedIndexChanged(object sender, EventArgs e)
        {
            CargarAlumnos();
        }

        protected void txtBusqueda_TextChanged(object sender, EventArgs e)
        {
            CargarAlumnos();
        }

        protected void btnFiltrar_Click(object sender, EventArgs e)
        {
            CargarAlumnos();
        }

        // ==================== EVENTOS DE GRIDVIEW ====================

        protected void gvAlumnos_PageIndexChanging(object sender, GridViewPageEventArgs e)
        {
            gvAlumnos.PageIndex = e.NewPageIndex;
            CargarAlumnos();
        }

        protected void gvAlumnos_RowCommand(object sender, GridViewCommandEventArgs e)
        {
            try
            {
                if (e.CommandName == "Select")
                {
                    GridViewRow row = (GridViewRow)((LinkButton)e.CommandSource).NamingContainer;
                    int? dni = gvAlumnos.DataKeys[row.RowIndex]?.Value as int?;

                    if (dni.HasValue)
                    {
                        DniSeleccionado = dni.Value;
                        foreach (GridViewRow r in gvAlumnos.Rows)
                            r.CssClass = "gridview-row";
                        row.CssClass = "selected-row";
                    }
                }
            }
            catch (Exception)
            {
                MostrarError(T("msg_error_generico"));
            }
        }

        protected void gvAlumnos_RowDataBound(object sender, GridViewRowEventArgs e)
        {
            if (e.Row.RowType == DataControlRowType.DataRow)
            {
                if (e.Row.Cells.Count > 1)
                    e.Row.Cells[1].Attributes["data-label"] = "Teléfono";
                if (e.Row.Cells.Count > 2)
                    e.Row.Cells[2].Attributes["data-label"] = "Fecha Nacimiento";
                if (e.Row.Cells.Count > 3)
                    e.Row.Cells[3].Attributes["data-label"] = "Peso";
                if (e.Row.Cells.Count > 4)
                    e.Row.Cells[4].Attributes["data-label"] = "Usuario";
                if (e.Row.Cells.Count > 5)
                    e.Row.Cells[5].Attributes["data-label"] = "Estado";
            }
        }

        // ==================== EVENTOS DE ACCIONES ====================

        protected void btnExportar_Click(object sender, EventArgs e)
        {
            MostrarAdvertencia("Funcionalidad de exportar en desarrollo");
        }

        protected void btnActualizar_Click(object sender, EventArgs e)
        {
            try
            {
                CargarAlumnos();
                MostrarExito(T("alumnos_msg_actualizado"));
            }
            catch (Exception)
            {
                MostrarError(T("msg_error_generico"));
            }
        }

        protected void btnCrear_Click(object sender, EventArgs e)
        {
            try
            {
                LimpiarFormulario();
                lblFormTitle.Text = T("alumnos_form_nuevo");
                EsModificacion = false;
                DniSeleccionado = null;
                txtDNI.Enabled = true;
                CargarUsuariosDisponibles();
                pnlForm.Visible = true;
            }
            catch (Exception)
            {
                MostrarError(T("msg_error_generico"));
            }
        }

        protected void btnModificar_Click(object sender, EventArgs e)
        {
            try
            {
                if (!DniSeleccionado.HasValue)
                {
                    MostrarError(T("alumnos_msg_sel_requerido"));
                    return;
                }

                var alumno = bllAlumno.ObtenerAlumno(DniSeleccionado.Value);
                if (alumno == null)
                {
                    MostrarError(T("alumnos_msg_no_existe"));
                    return;
                }

                EsModificacion = true;

                txtDNI.Text = alumno.DNI.ToString();
                txtDNI.Enabled = false;
                txtNombre.Text = alumno.Nombre ?? "";
                txtApellido.Text = alumno.Apellido ?? "";
                txtTelefono.Text = alumno.Telefono ?? "";
                txtFechaNacimiento.Text = alumno.FechaNacimiento?.ToString("yyyy-MM-dd") ?? "";
                txtPeso.Text = alumno.Peso?.ToString("F2") ?? "";
                chkActivo.Checked = alumno.Activo;

                CargarUsuariosDisponibles();

                if (!string.IsNullOrEmpty(alumno.Usuario))
                    ddlUsuarioAsociar.SelectedValue = alumno.Usuario;

                lblFormTitle.Text = T("alumnos_form_modificar");
                pnlForm.Visible = true;
            }
            catch (Exception)
            {
                MostrarError(T("msg_error_generico"));
            }
        }

        protected void btnEliminar_Click(object sender, EventArgs e)
        {
            try
            {
                if (!DniSeleccionado.HasValue)
                {
                    MostrarError(T("alumnos_msg_sel_eliminar"));
                    return;
                }

                var alumno = bllAlumno.ObtenerAlumno(DniSeleccionado.Value);
                if (alumno == null)
                {
                    MostrarError(T("alumnos_msg_no_existe"));
                    return;
                }

                lblAlumnoAEliminar.Text = $"{alumno.Apellido}, {alumno.Nombre} (DNI: {alumno.DNI})";
                hdnDniAEliminar.Value = alumno.DNI.ToString();
                pnlConfirmarEliminar.Visible = true;
            }
            catch (Exception)
            {
                MostrarError(T("msg_error_generico"));
            }
        }

        protected void btnAsociarUsuario_Click(object sender, EventArgs e)
        {
            try
            {
                if (!DniSeleccionado.HasValue)
                {
                    MostrarError(T("alumnos_msg_sel_asociar"));
                    return;
                }

                var alumno = bllAlumno.ObtenerAlumno(DniSeleccionado.Value);
                if (alumno == null)
                {
                    MostrarError(T("alumnos_msg_no_existe"));
                    return;
                }

                if (!string.IsNullOrEmpty(alumno.Usuario))
                {
                    MostrarAdvertencia(T("alumnos_msg_ya_asociado"));
                    return;
                }

                EsModificacion = true;
                DniSeleccionado = alumno.DNI;

                txtDNI.Text = alumno.DNI.ToString();
                txtDNI.Enabled = false;
                txtNombre.Text = alumno.Nombre ?? "";
                txtApellido.Text = alumno.Apellido ?? "";
                txtTelefono.Text = alumno.Telefono ?? "";
                txtFechaNacimiento.Text = alumno.FechaNacimiento?.ToString("yyyy-MM-dd") ?? "";
                txtPeso.Text = alumno.Peso?.ToString("F2") ?? "";
                chkActivo.Checked = alumno.Activo;

                CargarUsuariosDisponibles();
                lblFormTitle.Text = T("alumnos_btn_asociar") + " - " + alumno.Apellido + ", " + alumno.Nombre;
                pnlForm.Visible = true;
            }
            catch (Exception)
            {
                MostrarError(T("msg_error_generico"));
            }
        }

        protected void btnCancelar_Click(object sender, EventArgs e)
        {
            DniSeleccionado = null;
            CargarAlumnos();
        }

        protected void btnCloseForm_Click(object sender, EventArgs e)
        {
            pnlForm.Visible = false;
        }

        protected void btnCancelarForm_Click(object sender, EventArgs e)
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
                    MostrarError(T("alumnos_msg_dni_obligatorio"));
                    return;
                }

                if (!int.TryParse(txtDNI.Text, out int dni))
                {
                    MostrarError(T("alumnos_msg_dni_invalido"));
                    return;
                }

                if (string.IsNullOrEmpty(txtNombre.Text))
                {
                    MostrarError(T("alumnos_msg_nombre_oblig"));
                    return;
                }

                if (string.IsNullOrEmpty(txtApellido.Text))
                {
                    MostrarError(T("alumnos_msg_apellido_oblig"));
                    return;
                }

                if (string.IsNullOrEmpty(txtFechaNacimiento.Text))
                {
                    MostrarError(T("alumnos_msg_fecha_oblig"));
                    return;
                }

                if (!DateTime.TryParse(txtFechaNacimiento.Text, out DateTime fechaNacimiento))
                {
                    MostrarError(T("alumnos_msg_fecha_invalida"));
                    return;
                }

                if (fechaNacimiento > DateTime.Now)
                {
                    MostrarError(T("alumnos_msg_fecha_futura"));
                    return;
                }

                decimal? peso = null;
                if (!string.IsNullOrEmpty(txtPeso.Text) && decimal.TryParse(txtPeso.Text, out decimal p))
                {
                    if (p <= 0 || p >= 500)
                    {
                        MostrarError(T("alumnos_msg_peso_invalido"));
                        return;
                    }
                    peso = p;
                }

                string telefono = string.IsNullOrEmpty(txtTelefono.Text) ? null : txtTelefono.Text;

                if (EsModificacion)
                {
                    var alumno = bllAlumno.ObtenerAlumno(DniSeleccionado.Value);
                    if (alumno == null)
                    {
                        MostrarError(T("alumnos_msg_no_existe"));
                        return;
                    }

                    alumno.Peso = peso;
                    alumno.Activo = chkActivo.Checked;

                    bllAlumno.ActualizarAlumno(alumno);

                    string usuarioSeleccionado = ddlUsuarioAsociar.SelectedValue;
                    if (!string.IsNullOrEmpty(usuarioSeleccionado) && string.IsNullOrEmpty(alumno.Usuario))
                    {
                        bllAlumno.AsociarUsuario(alumno.DNI, usuarioSeleccionado);
                        bllEvento.RegistrarAsociarUsuario(ObtenerUsuarioActual(), alumno.DNI);
                    }
                    else if (string.IsNullOrEmpty(usuarioSeleccionado) && !string.IsNullOrEmpty(alumno.Usuario))
                    {
                        bllAlumno.DesasociarUsuario(alumno.DNI);
                        bllEvento.RegistrarDesasociarUsuario(ObtenerUsuarioActual(), alumno.DNI);
                    }

                    bllEvento.RegistrarModificacionAlumno(ObtenerUsuarioActual(), alumno.DNI);
                    bllEvento.RegistrarCambioDatosAlumno(ObtenerUsuarioActual(), alumno.DNI, "datos alumno");

                    MostrarExito(T("alumnos_msg_modificado"));
                }
                else
                {
                    if (bllAlumno.AlumnoExiste(dni))
                    {
                        MostrarError(T("alumnos_msg_ya_existe"));
                        return;
                    }

                    string usuarioName = $"cliente_{dni}";

                    var bllUsuarioLocal = new BLL.BLLUsuario();
                    string contrasena = bllUsuarioLocal.GenerarContrasenaSegura();
                    bllUsuarioLocal.CrearUsuario(
                        usuarioName,
                        contrasena,
                        4, // Rol Cliente
                        txtNombre.Text.Trim(),
                        txtApellido.Text.Trim(),
                        telefono,
                        null, // email
                        fechaNacimiento,
                        null, // datosEntrenador
                        dni   // dniAlumno
                    );

                    var alumno = bllAlumno.ObtenerAlumno(dni);
                    if (alumno != null)
                    {
                        alumno.Peso = peso;
                        alumno.Activo = chkActivo.Checked;
                        bllAlumno.ActualizarAlumno(alumno);
                    }

                    MostrarExito(T("alumnos_msg_creado"));
                    bllEvento.RegistrarAltaAlumno(ObtenerUsuarioActual(), dni);
                }

                pnlForm.Visible = false;
                CargarAlumnos();
            }
            catch (Exception)
            {
                MostrarError(T("msg_error_generico"));
            }
        }

        protected void btnConfirmarEliminar_Click(object sender, EventArgs e)
        {
            try
            {
                if (!int.TryParse(hdnDniAEliminar.Value, out int dni))
                {
                    MostrarError(T("msg_error_generico"));
                    return;
                }

                bllAlumno.EliminarAlumno(dni);
                MostrarExito(T("alumnos_msg_eliminado"));
                bllEvento.RegistrarBajaAlumno(ObtenerUsuarioActual(), dni);

                pnlConfirmarEliminar.Visible = false;
                CargarAlumnos();
            }
            catch (Exception)
            {
                MostrarError(T("msg_error_generico"));
            }
        }

        // ==================== MÉTODOS AUXILIARES ====================

        private string ObtenerUsuarioActual()
        {
            return Singleton.Instancia.Usuario?.USUARIO_Usuario ?? string.Empty;
        }

        private void LimpiarFormulario()
        {
            txtDNI.Text = "";
            txtNombre.Text = "";
            txtApellido.Text = "";
            txtTelefono.Text = "";
            txtFechaNacimiento.Text = "";
            txtPeso.Text = "";
            chkActivo.Checked = true;
            ddlUsuarioAsociar.Items.Clear();
            ddlUsuarioAsociar.Items.Add(new ListItem(T("alumnos_sin_asociar"), ""));
        }

        private void MostrarInfo(string mensaje)
        {
            ScriptManager.RegisterStartupScript(this, GetType(), "info", $"if(window.showToast) showToast('{System.Security.SecurityElement.Escape(mensaje)}', 'info');", true);
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
            string[] classes = { "av-pink", "av-mint", "av-lavender", "av-peach", "av-sky" };
            return classes[index % classes.Length];
        }

        protected string GetUsuarioClass(object usuario)
        {
            string u = usuario?.ToString() ?? "";
            if (string.IsNullOrEmpty(u))
                return "user-without";
            return "user-with";
        }

        protected string GetEstadoClass(object activo)
        {
            bool a = Convert.ToBoolean(activo);
            return a ? "pill-active" : "pill-inactive";
        }

        protected string GetEstadoText(object activo)
        {
            bool a = Convert.ToBoolean(activo);
            return a ? T("alumnos_estado_activo") : T("alumnos_estado_inactivo");
        }
    }
}
