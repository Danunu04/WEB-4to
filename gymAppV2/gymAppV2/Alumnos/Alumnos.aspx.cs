using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.UI;
using System.Web.UI.WebControls;
using BLL;
using BE;

namespace gymAppV2.Alumnos
{
    public partial class Alumnos : System.Web.UI.Page
    {
        private BLLAlumno bllAlumno;
        private BLLUsuario bllUsuario;
        private int? DniSeleccionado { get; set; }
        private bool EsModificacion { get; set; }

        protected void Page_Load(object sender, EventArgs e)
        {
            bllAlumno = new BLLAlumno();
            bllUsuario = new BLLUsuario();

            if (!IsPostBack)
            {
                CargarAlumnos();
                CargarUsuariosDropdown();
            }
        }

        // ==================== MÉTODOS PRINCIPALES ====================

        private void CargarAlumnos()
        {
            try
            {
                var alumnos = bllAlumno.ListarAlumnos() ?? new List<Alumno>();

                // Aplicar filtros
                if (!string.IsNullOrEmpty(ddlEstado.SelectedValue))
                {
                    bool activo = ddlEstado.SelectedValue == "activo";
                    alumnos = alumnos.Where(a => a.Activo == activo).ToList();
                }

                if (!string.IsNullOrEmpty(ddlUsuario.SelectedValue))
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

                // Actualizar estadísticas
                lblTotal.Text = alumnos.Count.ToString();
                lblActivos.Text = alumnos.Count(a => a.Activo).ToString();
                lblConRutinas.Text = alumnos.Count(a => a.TieneRutinas).ToString();
                lblSinUsuario.Text = alumnos.Count(a => string.IsNullOrEmpty(a.Usuario)).ToString();

                badgeCount.InnerText = lblTotal.Text + " alumnos";
                footerText.InnerText = $"Mostrando {alumnos.Count} de {alumnos.Count} alumnos";
            }
            catch (Exception ex)
            {
                MostrarError("Error al cargar alumnos: " + ex.Message);
            }
        }

        private void CargarUsuariosDropdown()
        {
            try
            {
                ddlUsuario.Items.Clear();
                ddlUsuario.Items.Add(new ListItem("Todos", ""));
                ddlUsuario.Items.Add(new ListItem("Con usuario", "con_usuario"));
                ddlUsuario.Items.Add(new ListItem("Sin usuario", "sin_usuario"));
            }
            catch (Exception ex)
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
                ddlUsuarioAsociar.Items.Add(new ListItem("-- Sin asociar --", ""));

                foreach (var usuario in usuarios)
                {
                    string texto = $"{usuario.USUARIO_Usuario} ({usuario.Apellido}, {usuario.Nombre})";
                    ddlUsuarioAsociar.Items.Add(new ListItem(texto, usuario.USUARIO_Usuario));
                }
            }
            catch (Exception ex)
            {
                // Silencioso - el dropdown queda vacío
            }
        }

        private void ActualizarEstadisticas()
        {
            // Se actualiza desde CargarAlumnos
        }

        private void ActualizarFooter()
        {
            // Se actualiza desde CargarAlumnos
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
                // Selection via LinkButton click
                if (e.CommandName == "Select")
                {
                    GridViewRow row = (GridViewRow)((LinkButton)e.CommandSource).NamingContainer;
                    int? dni = gvAlumnos.DataKeys[row.RowIndex]?.Value as int?;

                    if (dni.HasValue)
                    {
                        DniSeleccionado = dni.Value;
                        // Clear other selections
                        foreach (GridViewRow r in gvAlumnos.Rows)
                        {
                            r.CssClass = "gridview-row";
                        }
                        row.CssClass = "selected-row";
                    }
                }
            }
            catch (Exception ex)
            {
                MostrarError("Error al seleccionar alumno: " + ex.Message);
            }
        }

        protected void gvAlumnos_RowDataBound(object sender, GridViewRowEventArgs e)
        {
            if (e.Row.RowType == DataControlRowType.DataRow)
            {
                // Agregar atributos data-label para vista móvil
                if (e.Row.Cells.Count > 0)
                {
                    // Teléfono
                    if (e.Row.Cells.Count > 1)
                        e.Row.Cells[1].Attributes["data-label"] = "Teléfono";
                    // Fecha Nacimiento
                    if (e.Row.Cells.Count > 2)
                        e.Row.Cells[2].Attributes["data-label"] = "Fecha Nacimiento";
                    // Peso
                    if (e.Row.Cells.Count > 3)
                        e.Row.Cells[3].Attributes["data-label"] = "Peso";
                    // Usuario
                    if (e.Row.Cells.Count > 4)
                        e.Row.Cells[4].Attributes["data-label"] = "Usuario";
                    // Estado
                    if (e.Row.Cells.Count > 5)
                        e.Row.Cells[5].Attributes["data-label"] = "Estado";
                }
            }
        }

        // ==================== EVENTOS DE ACCIONES ====================

        protected void btnExportar_Click(object sender, EventArgs e)
        {
            try
            {
                MostrarAdvertencia("Funcionalidad de exportar en desarrollo");
            }
            catch (Exception ex)
            {
                MostrarError("Error al exportar: " + ex.Message);
            }
        }

        protected void btnActualizar_Click(object sender, EventArgs e)
        {
            try
            {
                CargarAlumnos();
                MostrarExito("Lista de alumnos actualizada");
            }
            catch (Exception ex)
            {
                MostrarError("Error al actualizar lista: " + ex.Message);
            }
        }

        protected void btnCrear_Click(object sender, EventArgs e)
        {
            try
            {
                LimpiarFormulario();
                lblFormTitle.Text = "Nuevo Alumno";
                EsModificacion = false;
                DniSeleccionado = null;
                txtDNI.Enabled = true;
                CargarUsuariosDisponibles();
                pnlForm.Visible = true;
            }
            catch (Exception ex)
            {
                MostrarError("Error al preparar formulario: " + ex.Message);
            }
        }

        protected void btnModificar_Click(object sender, EventArgs e)
        {
            try
            {
                if (!DniSeleccionado.HasValue)
                {
                    MostrarError("Seleccione un alumno de la lista");
                    return;
                }

                var alumno = bllAlumno.ObtenerAlumno(DniSeleccionado.Value);
                if (alumno == null)
                {
                    MostrarError("El alumno no existe");
                    return;
                }

                EsModificacion = true;

                txtDNI.Text = alumno.DNI.ToString();
                txtDNI.Enabled = false;
                txtNombre.Text = alumno.Nombre;
                txtApellido.Text = alumno.Apellido;
                txtTelefono.Text = alumno.Telefono?.ToString() ?? "";
                txtFechaNacimiento.Text = alumno.FechaNacimiento.ToString("yyyy-MM-dd");
                txtPeso.Text = alumno.Peso?.ToString("F2") ?? "";
                chkActivo.Checked = alumno.Activo;

                CargarUsuariosDisponibles();

                if (!string.IsNullOrEmpty(alumno.Usuario))
                {
                    ddlUsuarioAsociar.SelectedValue = alumno.Usuario;
                }

                lblFormTitle.Text = "Modificar Alumno";
                pnlForm.Visible = true;
            }
            catch (Exception ex)
            {
                MostrarError("Error al cargar alumno: " + ex.Message);
            }
        }

        protected void btnEliminar_Click(object sender, EventArgs e)
        {
            try
            {
                if (!DniSeleccionado.HasValue)
                {
                    MostrarError("Seleccione un alumno de la lista para eliminar");
                    return;
                }

                var alumno = bllAlumno.ObtenerAlumno(DniSeleccionado.Value);
                if (alumno == null)
                {
                    MostrarError("El alumno no existe");
                    return;
                }

                lblAlumnoAEliminar.Text = $"{alumno.Apellido}, {alumno.Nombre} (DNI: {alumno.DNI})";
                hdnDniAEliminar.Value = alumno.DNI.ToString();
                pnlConfirmarEliminar.Visible = true;
            }
            catch (Exception ex)
            {
                MostrarError("Error al preparar eliminación: " + ex.Message);
            }
        }

        protected void btnAsociarUsuario_Click(object sender, EventArgs e)
        {
            try
            {
                if (!DniSeleccionado.HasValue)
                {
                    MostrarError("Seleccione un alumno de la lista para asociar usuario");
                    return;
                }

                var alumno = bllAlumno.ObtenerAlumno(DniSeleccionado.Value);
                if (alumno == null)
                {
                    MostrarError("El alumno seleccionado no existe");
                    return;
                }

                if (!string.IsNullOrEmpty(alumno.Usuario))
                {
                    MostrarAdvertencia($"El alumno ya tiene un usuario asociado: {alumno.Usuario}");
                    return;
                }

                EsModificacion = true;
                DniSeleccionado = alumno.DNI;

                txtDNI.Text = alumno.DNI.ToString();
                txtDNI.Enabled = false;
                txtNombre.Text = alumno.Nombre;
                txtApellido.Text = alumno.Apellido;
                txtTelefono.Text = alumno.Telefono?.ToString() ?? "";
                txtFechaNacimiento.Text = alumno.FechaNacimiento.ToString("yyyy-MM-dd");
                txtPeso.Text = alumno.Peso?.ToString("F2") ?? "";
                chkActivo.Checked = alumno.Activo;

                CargarUsuariosDisponibles();
                lblFormTitle.Text = "Asociar Usuario - " + alumno.Apellido + ", " + alumno.Nombre;
                pnlForm.Visible = true;
            }
            catch (Exception ex)
            {
                MostrarError("Error al preparar asociación: " + ex.Message);
            }
        }

        protected void btnCancelar_Click(object sender, EventArgs e)
        {
            try
            {
                DniSeleccionado = null;
                CargarAlumnos();
            }
            catch (Exception ex)
            {
                MostrarError("Error al cancelar: " + ex.Message);
            }
        }

        protected void btnCloseForm_Click(object sender, EventArgs e)
        {
            try
            {
                pnlForm.Visible = false;
            }
            catch (Exception ex)
            {
                MostrarError("Error al cerrar formulario: " + ex.Message);
            }
        }

        protected void btnCancelarForm_Click(object sender, EventArgs e)
        {
            try
            {
                pnlForm.Visible = false;
            }
            catch (Exception ex)
            {
                MostrarError("Error al cancelar: " + ex.Message);
            }
        }

        protected void btnCloseConfirm_Click(object sender, EventArgs e)
        {
            try
            {
                pnlConfirmarEliminar.Visible = false;
            }
            catch (Exception ex)
            {
                MostrarError("Error al cerrar confirmación: " + ex.Message);
            }
        }

        protected void btnCancelarEliminar_Click(object sender, EventArgs e)
        {
            try
            {
                pnlConfirmarEliminar.Visible = false;
            }
            catch (Exception ex)
            {
                MostrarError("Error al cancelar eliminación: " + ex.Message);
            }
        }

        protected void btnGuardar_Click(object sender, EventArgs e)
        {
            try
            {
                // Validaciones
                if (string.IsNullOrEmpty(txtDNI.Text))
                {
                    MostrarError("El DNI es obligatorio");
                    return;
                }

                if (!int.TryParse(txtDNI.Text, out int dni))
                {
                    MostrarError("El DNI debe ser un número válido");
                    return;
                }

                if (string.IsNullOrEmpty(txtNombre.Text))
                {
                    MostrarError("El nombre es obligatorio");
                    return;
                }

                if (string.IsNullOrEmpty(txtApellido.Text))
                {
                    MostrarError("El apellido es obligatorio");
                    return;
                }

                if (string.IsNullOrEmpty(txtFechaNacimiento.Text))
                {
                    MostrarError("La fecha de nacimiento es obligatoria");
                    return;
                }

                if (!DateTime.TryParse(txtFechaNacimiento.Text, out DateTime fechaNacimiento))
                {
                    MostrarError("La fecha de nacimiento no es válida");
                    return;
                }

                if (fechaNacimiento > DateTime.Now)
                {
                    MostrarError("La fecha de nacimiento no puede ser futura");
                    return;
                }

                decimal? peso = null;
                if (!string.IsNullOrEmpty(txtPeso.Text) && decimal.TryParse(txtPeso.Text, out decimal p))
                {
                    if (p <= 0 || p >= 500)
                    {
                        MostrarError("El peso debe estar entre 0 y 500 kg");
                        return;
                    }
                    peso = p;
                }

                long? telefono = null;
                if (!string.IsNullOrEmpty(txtTelefono.Text) && long.TryParse(txtTelefono.Text, out long t))
                {
                    telefono = t;
                }

                if (EsModificacion)
                {
                    // Modificar alumno existente
                    var alumno = bllAlumno.ObtenerAlumno(DniSeleccionado.Value);
                    if (alumno == null)
                    {
                        MostrarError("El alumno no existe");
                        return;
                    }

                    alumno.Nombre = txtNombre.Text.Trim();
                    alumno.Apellido = txtApellido.Text.Trim();
                    alumno.Telefono = telefono;
                    alumno.FechaNacimiento = fechaNacimiento;
                    alumno.Peso = peso;
                    alumno.Activo = chkActivo.Checked;

                    bllAlumno.ActualizarAlumno(alumno);

                    // Manejar asociación de usuario
                    string usuarioSeleccionado = ddlUsuarioAsociar.SelectedValue;
                    if (!string.IsNullOrEmpty(usuarioSeleccionado) && string.IsNullOrEmpty(alumno.Usuario))
                    {
                        bllAlumno.AsociarUsuario(alumno.DNI, usuarioSeleccionado);
                    }
                    else if (string.IsNullOrEmpty(usuarioSeleccionado) && !string.IsNullOrEmpty(alumno.Usuario))
                    {
                        bllAlumno.DesasociarUsuario(alumno.DNI);
                    }

                    MostrarExito($"Alumno modificado correctamente");
                }
                else
                {
                    // Crear nuevo alumno
                    if (bllAlumno.AlumnoExiste(dni))
                    {
                        MostrarError($"Ya existe un alumno con DNI {dni}");
                        return;
                    }

                    var nuevoAlumno = new Alumno
                    {
                        DNI = dni,
                        Nombre = txtNombre.Text.Trim(),
                        Apellido = txtApellido.Text.Trim(),
                        Telefono = telefono,
                        FechaNacimiento = fechaNacimiento,
                        Peso = peso,
                        Activo = chkActivo.Checked,
                        Usuario = string.IsNullOrEmpty(ddlUsuarioAsociar.SelectedValue)
                            ? null
                            : ddlUsuarioAsociar.SelectedValue
                    };

                    bllAlumno.CrearAlumno(nuevoAlumno);

                    // Si se seleccionó usuario, asociarlo
                    if (!string.IsNullOrEmpty(ddlUsuarioAsociar.SelectedValue))
                    {
                        bllAlumno.AsociarUsuario(dni, ddlUsuarioAsociar.SelectedValue);
                    }

                    MostrarExito($"Alumno creado correctamente");
                }

                pnlForm.Visible = false;
                CargarAlumnos();
            }
            catch (Exception ex)
            {
                MostrarError(ex.Message);
            }
        }

        protected void btnConfirmarEliminar_Click(object sender, EventArgs e)
        {
            try
            {
                if (!int.TryParse(hdnDniAEliminar.Value, out int dni))
                {
                    MostrarError("DNI inválido");
                    return;
                }

                bllAlumno.EliminarAlumno(dni);
                MostrarExito($"Alumno eliminado correctamente");

                pnlConfirmarEliminar.Visible = false;
                CargarAlumnos();
            }
            catch (Exception ex)
            {
                MostrarError("Error al eliminar alumno: " + ex.Message);
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
            txtPeso.Text = "";
            chkActivo.Checked = true;
            ddlUsuarioAsociar.Items.Clear();
            ddlUsuarioAsociar.Items.Add(new ListItem("-- Sin asociar --", ""));
        }

        private void MostrarError(string mensaje)
        {
            ScriptManager.RegisterStartupScript(this, GetType(), "error", $"if(window.showToast) showToast('{System.Security.SecurityElement.Escape(mensaje)}', 'error');", true);
        }

        private void MostrarExito(string mensaje)
        {
            ScriptManager.RegisterStartupScript(this, GetType(), "exito", $"if(window.showToast) showToast('{System.Security.SecurityElement.Escape(mensaje)}', 'success');", true);
        }

        private void MostrarAdvertencia(string mensaje)
        {
            ScriptManager.RegisterStartupScript(this, GetType(), "advertencia", $"if(window.showToast) showToast('{System.Security.SecurityElement.Escape(mensaje)}', 'warning');", true);
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
            return a ? "Activo" : "Inactivo";
        }
    }
}