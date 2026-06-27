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
            VerificarAcceso("GestionAlumnos");

            bllAlumno = new BLLAlumno();
            bllUsuario = new BLLUsuario();
            bllEvento = new BLLEvento();

            if (!IsPostBack)
            {
                CargarAlumnos();
                CargarUsuariosDropdown();
                ConfigurarModoSoloLectura();
            }
        }

        /// <summary>
        /// Para los clientes, el módulo de alumnos es solo lectura de sus alumnos asociados.
        /// </summary>
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
                ddlUsuarioAsociar.Items.Add(new ListItem("-- Sin asociar --", ""));

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
                txtNombre.Text = alumno.Nombre ?? "";
                txtApellido.Text = alumno.Apellido ?? "";
                txtTelefono.Text = alumno.Telefono ?? "";
                txtFechaNacimiento.Text = alumno.FechaNacimiento?.ToString("yyyy-MM-dd") ?? "";
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
                txtNombre.Text = alumno.Nombre ?? "";
                txtApellido.Text = alumno.Apellido ?? "";
                txtTelefono.Text = alumno.Telefono ?? "";
                txtFechaNacimiento.Text = alumno.FechaNacimiento?.ToString("yyyy-MM-dd") ?? "";
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

                string telefono = string.IsNullOrEmpty(txtTelefono.Text) ? null : txtTelefono.Text;

                if (EsModificacion)
                {
                    // Modificar alumno existente - solo campos específicos de ALUMNOS
                    var alumno = bllAlumno.ObtenerAlumno(DniSeleccionado.Value);
                    if (alumno == null)
                    {
                        MostrarError("El alumno no existe");
                        return;
                    }

                    // Los datos personales (Nombre, Apellido, Telefono, FechaNacimiento) están en USUARIOS
                    // Aquí solo actualizamos Peso y Activo que son específicos de ALUMNOS
                    alumno.Peso = peso;
                    alumno.Activo = chkActivo.Checked;

                    bllAlumno.ActualizarAlumno(alumno);

                    // Manejar asociación de usuario
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

                    MostrarExito($"Alumno modificado correctamente");
                }
                else
                {
                    // Crear nuevo alumno - en esquema normalizado se crea USUARIOS primero con datos personales
                    if (bllAlumno.AlumnoExiste(dni))
                    {
                        MostrarError($"Ya existe un alumno con DNI {dni}");
                        return;
                    }

                    // Los datos personales se guardan en USUARIOS, no en ALUMNOS
                    // Se usa BLLUsuario.CrearUsuario con rol=4 para crear Cliente (crea USUARIOS + ALUMNOS)
                    string usuarioName = $"cliente_{dni}";

                    var bllUsuario = new BLL.BLLUsuario();
                    string contrasena = bllUsuario.GenerarContrasenaSegura();
                    bllUsuario.CrearUsuario(
                        usuarioName,
                        contrasena,
                        4, // Rol Cliente
                        txtNombre.Text.Trim(),
                        txtApellido.Text.Trim(),
                        telefono,
                        null, // email
                        fechaNacimiento,
                        null, // datosEntrenador
                        dni // dniAlumno
                    );

                    // Actualizar el peso en ALUMNOS (ya creado por CrearUsuario)
                    var alumno = bllAlumno.ObtenerAlumno(dni);
                    if (alumno != null)
                    {
                        alumno.Peso = peso;
                        alumno.Activo = chkActivo.Checked;
                        bllAlumno.ActualizarAlumno(alumno);
                    }

                    MostrarExito($"Alumno creado correctamente");

                    bllEvento.RegistrarAltaAlumno(ObtenerUsuarioActual(), dni);
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
                bllEvento.RegistrarBajaAlumno(ObtenerUsuarioActual(), dni);

                pnlConfirmarEliminar.Visible = false;
                CargarAlumnos();
            }
            catch (Exception ex)
            {
                MostrarError("Error al eliminar alumno: " + ex.Message);
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