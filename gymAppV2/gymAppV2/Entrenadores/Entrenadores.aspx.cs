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
        private int? DniSeleccionado { get; set; }
        private bool EsModificacion { get; set; }

        protected void Page_Load(object sender, EventArgs e)
        {
            VerificarAcceso("GestionEntrenadores");

            bllEntrenador = new BLLEntrenador();
            bllUsuario = new BLLUsuario();
            bllEvento = new BLLEvento();

            if (!IsPostBack)
            {
                CargarEntrenadores();
                ActualizarEstadisticas();
            }
        }

        // ==================== CARGA DE DATOS ====================

        private void CargarEntrenadores()
        {
            try
            {
                var entrenadores = bllEntrenador.ListarEntrenadores() ?? new List<Entrenador>();

                if (!string.IsNullOrEmpty(ddlEstado.SelectedValue))
                {
                    bool activo = ddlEstado.SelectedValue == "activo";
                    entrenadores = entrenadores.Where(e => e.Activo == activo).ToList();
                }

                if (!string.IsNullOrEmpty(txtBusqueda.Text))
                {
                    string busqueda = txtBusqueda.Text.ToLower();
                    entrenadores = entrenadores.Where(e =>
                        e.DNI.ToString().Contains(busqueda) ||
                        (e.Nombre ?? "").ToLower().Contains(busqueda) ||
                        (e.Apellido ?? "").ToLower().Contains(busqueda)
                    ).ToList();
                }

                gvEntrenadores.DataSource = entrenadores;
                gvEntrenadores.DataBind();

                badgeCount.InnerText = entrenadores.Count + " entrenadores";
                footerText.InnerText = $"Mostrando {entrenadores.Count} de {entrenadores.Count} entrenadores";
            }
            catch (Exception ex)
            {
                MostrarError("Error al cargar entrenadores: " + ex.Message);
            }
        }

        private void ActualizarEstadisticas()
        {
            try
            {
                var stats = bllEntrenador.ObtenerEstadisticas();
                lblTotal.Text = stats.ContainsKey("Total") ? stats["Total"].ToString() : "0";
                lblActivos.Text = stats.ContainsKey("Activos") ? stats["Activos"].ToString() : "0";
                lblConAlumnos.Text = stats.ContainsKey("ConAlumnos") ? stats["ConAlumnos"].ToString() : "0";
                lblSinUsuario.Text = stats.ContainsKey("SinUsuario") ? stats["SinUsuario"].ToString() : "0";
            }
            catch
            {
                lblTotal.Text = lblActivos.Text = lblConAlumnos.Text = lblSinUsuario.Text = "0";
            }
        }

        // ==================== EVENTOS DE FILTROS ====================

        protected void ddlEstado_SelectedIndexChanged(object sender, EventArgs e)
        {
            CargarEntrenadores();
        }

        protected void txtBusqueda_TextChanged(object sender, EventArgs e)
        {
            CargarEntrenadores();
        }

        protected void btnFiltrar_Click(object sender, EventArgs e)
        {
            CargarEntrenadores();
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
                            r.CssClass = "gridview-row";
                        row.CssClass = "selected-row";
                    }
                }
            }
            catch (Exception ex)
            {
                MostrarError("Error al seleccionar entrenador: " + ex.Message);
            }
        }

        protected void gvEntrenadores_RowDataBound(object sender, GridViewRowEventArgs e)
        {
            if (e.Row.RowType == DataControlRowType.DataRow)
            {
                if (e.Row.Cells.Count > 1) e.Row.Cells[1].Attributes["data-label"] = "Teléfono";
                if (e.Row.Cells.Count > 2) e.Row.Cells[2].Attributes["data-label"] = "Fecha Nacimiento";
                if (e.Row.Cells.Count > 3) e.Row.Cells[3].Attributes["data-label"] = "Alumnos";
                if (e.Row.Cells.Count > 4) e.Row.Cells[4].Attributes["data-label"] = "Estado";
            }
        }

        // ==================== EVENTOS DE ACCIONES ====================

        protected void btnCrear_Click(object sender, EventArgs e)
        {
            try
            {
                LimpiarFormulario();
                lblFormTitle.Text = "Nuevo Entrenador";
                EsModificacion = false;
                DniSeleccionado = null;
                txtDNI.Enabled = true;
                txtNombre.Enabled = true;
                txtApellido.Enabled = true;
                txtTelefono.Enabled = true;
                txtFechaNacimiento.Enabled = true;
                pnlReadonlyNote.Visible = false;
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
                    MostrarError("Seleccione un entrenador de la lista");
                    return;
                }

                var entrenador = bllEntrenador.ObtenerEntrenador(DniSeleccionado.Value);
                if (entrenador == null)
                {
                    MostrarError("El entrenador no existe");
                    return;
                }

                EsModificacion = true;

                txtDNI.Text = entrenador.DNI.ToString();
                txtDNI.Enabled = false;
                txtNombre.Text = entrenador.Nombre ?? "";
                txtNombre.Enabled = false;
                txtApellido.Text = entrenador.Apellido ?? "";
                txtApellido.Enabled = false;
                txtTelefono.Text = entrenador.Telefono ?? "";
                txtTelefono.Enabled = false;
                txtFechaNacimiento.Text = entrenador.FechaNacimiento?.ToString("yyyy-MM-dd") ?? "";
                txtFechaNacimiento.Enabled = false;
                chkActivo.Checked = entrenador.Activo;
                pnlReadonlyNote.Visible = true;

                lblFormTitle.Text = "Modificar Entrenador";
                pnlForm.Visible = true;
            }
            catch (Exception ex)
            {
                MostrarError("Error al cargar entrenador: " + ex.Message);
            }
        }

        protected void btnEliminar_Click(object sender, EventArgs e)
        {
            try
            {
                if (!DniSeleccionado.HasValue)
                {
                    MostrarError("Seleccione un entrenador de la lista para eliminar");
                    return;
                }

                var entrenador = bllEntrenador.ObtenerEntrenador(DniSeleccionado.Value);
                if (entrenador == null)
                {
                    MostrarError("El entrenador no existe");
                    return;
                }

                lblEntrenadorAEliminar.Text = $"{entrenador.Apellido}, {entrenador.Nombre} (DNI: {entrenador.DNI})";
                hdnDniAEliminar.Value = entrenador.DNI.ToString();
                pnlConfirmarEliminar.Visible = true;
            }
            catch (Exception ex)
            {
                MostrarError("Error al preparar eliminación: " + ex.Message);
            }
        }

        protected void btnCancelar_Click(object sender, EventArgs e)
        {
            try
            {
                DniSeleccionado = null;
                CargarEntrenadores();
            }
            catch (Exception ex)
            {
                MostrarError("Error al cancelar: " + ex.Message);
            }
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
                    MostrarError("El DNI es obligatorio");
                    return;
                }

                if (!int.TryParse(txtDNI.Text, out int dni))
                {
                    MostrarError("El DNI debe ser un número válido");
                    return;
                }

                if (EsModificacion)
                {
                    var entrenador = bllEntrenador.ObtenerEntrenador(DniSeleccionado.Value);
                    if (entrenador == null)
                    {
                        MostrarError("El entrenador no existe");
                        return;
                    }

                    entrenador.Activo = chkActivo.Checked;
                    bllEntrenador.ActualizarEntrenador(entrenador);

                    MostrarExito("Entrenador modificado correctamente");
                }
                else
                {
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

                    if (bllEntrenador.ObtenerEntrenador(dni) != null)
                    {
                        MostrarError($"Ya existe un entrenador con DNI {dni}");
                        return;
                    }

                    string telefono = string.IsNullOrEmpty(txtTelefono.Text) ? null : txtTelefono.Text.Trim();
                    string usuarioName = $"entrenador_{dni}";
                    string contrasena = bllUsuario.GenerarContrasenaSegura();

                    var datosEntrenador = new Entrenador
                    {
                        DNI = dni,
                        AlumnosCount = 0,
                        Activo = chkActivo.Checked
                    };

                    bllUsuario.CrearUsuario(
                        usuarioName,
                        contrasena,
                        3,
                        txtNombre.Text.Trim(),
                        txtApellido.Text.Trim(),
                        telefono,
                        null,
                        fechaNacimiento,
                        datosEntrenador
                    );

                    MostrarInfo($"Entrenador creado. Usuario: <strong>{usuarioName}</strong> — La contraseña temporal fue generada. El entrenador deberá cambiarla al primer inicio de sesión.");
                }

                pnlForm.Visible = false;
                CargarEntrenadores();
                ActualizarEstadisticas();
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

                bllEntrenador.EliminarEntrenador(dni);
                MostrarExito("Entrenador eliminado correctamente");

                pnlConfirmarEliminar.Visible = false;
                CargarEntrenadores();
                ActualizarEstadisticas();
            }
            catch (Exception ex)
            {
                MostrarError("Error al eliminar entrenador: " + ex.Message);
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
            chkActivo.Checked = true;
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
            string[] classes = { "av-teal", "av-mint", "av-lavender", "av-peach", "av-sky" };
            return classes[index % classes.Length];
        }

        protected string GetAlumnosClass(object alumnosCount)
        {
            int count = Convert.ToInt32(alumnosCount);
            return count > 0 ? "alumnos-con" : "alumnos-sin";
        }

        protected string GetEstadoClass(object activo)
        {
            return Convert.ToBoolean(activo) ? "pill-active" : "pill-inactive";
        }

        protected string GetEstadoText(object activo)
        {
            return Convert.ToBoolean(activo) ? "Activo" : "Inactivo";
        }
    }
}
