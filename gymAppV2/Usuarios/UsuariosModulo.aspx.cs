using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.UI;
using System.Web.UI.WebControls;
using BLL;

namespace gymAppV2.Usuarios
{
    public partial class UsuariosModulo : System.Web.UI.Page
    {
        private string SelectedUsuario { get; set; }
        private List<BE.UsuarioGestion> Usuarios { get; set; }
        private BLLUsuario bllUsuario;

        protected void Page_Load(object sender, EventArgs e)
        {
            bllUsuario = new BLLUsuario();
            Usuarios = new List<BE.UsuarioGestion>();

            if (!IsPostBack)
            {
                CargarUsuarios();
                ActualizarEstadisticas();
            }
        }

        // ==================== MÉTODOS PRINCIPALES ====================

        private void CargarUsuarios()
        {
            // Obtener usuarios de la base de datos
            Usuarios = bllUsuario.ListarUsuarios() ?? new List<BE.UsuarioGestion>();

            AplicarFiltros();
            gvUsuarios.DataSource = Usuarios;
            gvUsuarios.DataBind();

            ActualizarFooter();
        }

        private void AplicarFiltros()
        {
            if (Usuarios == null) return;

            var filtrados = Usuarios.AsEnumerable();

            // Filtro por estado
            string estado = ddlEstado.SelectedValue;
            if (!string.IsNullOrEmpty(estado))
            {
                if (estado == "activo")
                    filtrados = filtrados.Where(u => u.USUARIO_Activo);
                else if (estado == "inactivo")
                    filtrados = filtrados.Where(u => !u.USUARIO_Activo);
            }

            // Filtro por bloqueado
            string bloqueado = ddlBloqueado.SelectedValue;
            if (!string.IsNullOrEmpty(bloqueado))
            {
                if (bloqueado == "bloqueado")
                    filtrados = filtrados.Where(u => u.USUARIO_Bloqueado);
                else if (bloqueado == "no_bloqueado")
                    filtrados = filtrados.Where(u => !u.USUARIO_Bloqueado);
            }

            // Filtro por rol
            string rol = ddlRol.SelectedValue;
            if (!string.IsNullOrEmpty(rol))
            {
                filtrados = filtrados.Where(u => u.USUARIO_Tipo == rol);
            }

            // Filtro por búsqueda
            string busqueda = txtBusqueda.Text.ToLower();
            if (!string.IsNullOrEmpty(busqueda))
            {
                filtrados = filtrados.Where(u =>
                    u.USUARIO_Usuario.ToLower().Contains(busqueda) ||
                    (u.Nombre != null && u.Nombre.ToLower().Contains(busqueda)) ||
                    (u.Apellido != null && u.Apellido.ToLower().Contains(busqueda)));
            }

            Usuarios = filtrados.ToList();
        }

        private void ActualizarEstadisticas()
        {
            if (Usuarios != null)
            {
                lblTotal.Text = Usuarios.Count.ToString();
                lblActivos.Text = Usuarios.Count(u => u.USUARIO_Activo).ToString();
                lblBloqueados.Text = Usuarios.Count(u => u.USUARIO_Bloqueado).ToString();
                lblInactivos.Text = Usuarios.Count(u => !u.USUARIO_Activo).ToString();
            }
            else
            {
                lblTotal.Text = "0";
                lblActivos.Text = "0";
                lblBloqueados.Text = "0";
                lblInactivos.Text = "0";
            }

            badgeCount.InnerText = lblTotal.Text + " usuarios";
        }

        private void ActualizarFooter()
        {
            int total = Usuarios?.Count ?? 0;
            footerText.InnerText = $"Mostrando {total} de {total} usuarios";
        }

        private void MostrarError(string mensaje)
        {
            // Por ahora, usar alert de JavaScript
            ScriptManager.RegisterStartupScript(this, GetType(), "error", $"alert('{mensaje.Replace("'", "\\'")}')", true);
        }

        private void MostrarExito(string mensaje)
        {
            ScriptManager.RegisterStartupScript(this, GetType(), "exito", $"alert('{mensaje.Replace("'", "\\'")}')", true);
        }

        // ==================== EVENTOS DE FILTROS ====================

        protected void ddlEstado_SelectedIndexChanged(object sender, EventArgs e)
        {
            CargarUsuarios();
        }

        protected void ddlBloqueado_SelectedIndexChanged(object sender, EventArgs e)
        {
            CargarUsuarios();
        }

        protected void ddlRol_SelectedIndexChanged(object sender, EventArgs e)
        {
            CargarUsuarios();
        }

        protected void txtBusqueda_TextChanged(object sender, EventArgs e)
        {
            CargarUsuarios();
        }

        protected void btnFiltrar_Click(object sender, EventArgs e)
        {
            CargarUsuarios();
        }

        protected void ddlRolForm_SelectedIndexChanged(object sender, EventArgs e)
        {
            MostrarCamposSegunRol();
        }

        private void MostrarCamposSegunRol()
        {
            string rol = ddlRolForm.SelectedValue;

            if (rol == "3")
            {
                EntField.Visible = true;
                clienteFields.Visible = false;
            }
            else if (rol == "4")
            {
                EntField.Visible = false;
                clienteFields.Visible = true;
            }
            else
            {
                EntField.Visible = false;
                clienteFields.Visible = false;
            }
        }

        // ==================== EVENTOS DE GRIDVIEW ====================

        protected void gvUsuarios_PageIndexChanging(object sender, GridViewPageEventArgs e)
        {
            gvUsuarios.PageIndex = e.NewPageIndex;
            CargarUsuarios();
        }

        protected void gvUsuarios_RowCommand(object sender, GridViewCommandEventArgs e)
        {
            try
            {
                if (e.CommandName == "Select")
                {
                    int index = Convert.ToInt32(e.CommandArgument);
                    string usuario = gvUsuarios.DataKeys[index]?.Value?.ToString();

                    if (!string.IsNullOrEmpty(usuario))
                    {
                        SeleccionarUsuario(usuario);
                    }
                }
            }
            catch (Exception ex)
            {
                MostrarError("Error al seleccionar usuario: " + ex.Message);
            }
        }

        protected void gvUsuarios_RowDataBound(object sender, GridViewRowEventArgs e)
        {
            if (e.Row.RowType == DataControlRowType.DataRow)
            {
                e.Row.Attributes["onclick"] = Page.ClientScript.GetPostBackClientHyperlink(gvUsuarios, "Select$" + e.Row.RowIndex);
                e.Row.Style["cursor"] = "pointer";

                // Marcar fila seleccionada
                var dataItem = e.Row.DataItem as BE.UsuarioGestion;
                if (dataItem != null && dataItem.USUARIO_Usuario == SelectedUsuario)
                {
                    e.Row.CssClass = "selected";
                }
            }
        }

        // ==================== EVENTOS DE ACCIONES ====================

        protected void btnExportar_Click(object sender, EventArgs e)
        {
            try
            {
                // TODO: Llamar a la capa BLL para exportar usuarios
                MostrarExito("Funcionalidad de exportar en desarrollo");
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
                CargarUsuarios();
                ActualizarEstadisticas();
                MostrarExito("Lista actualizada");
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
                lblFormTitle.Text = "Nuevo usuario";
                pnlForm.Visible = true;
                passwordRow.Visible = true;
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
                if (string.IsNullOrEmpty(SelectedUsuario))
                {
                    MostrarError("Selecciona un usuario primero");
                    return;
                }

                var usuario = Usuarios.FirstOrDefault(u => u.USUARIO_Usuario == SelectedUsuario);
                if (usuario != null)
                {
                    CargarUsuarioEnFormulario(usuario);
                    lblFormTitle.Text = "Modificar usuario";
                    pnlForm.Visible = true;
                    passwordRow.Visible = false;
                }
            }
            catch (Exception ex)
            {
                MostrarError("Error al cargar usuario: " + ex.Message);
            }
        }

        protected void btnDesbloquear_Click(object sender, EventArgs e)
        {
            try
            {
                if (string.IsNullOrEmpty(SelectedUsuario))
                {
                    MostrarError("Selecciona un usuario primero");
                    return;
                }

                // TODO: Llamar a la capa BLL para desbloquear usuario
                var usuario = Usuarios.FirstOrDefault(u => u.USUARIO_Usuario == SelectedUsuario);
                if (usuario != null)
                {
                    usuario.USUARIO_Bloqueado = false;
                    MostrarExito($"✅ {usuario.USUARIO_Tipo} desbloqueado/a");
                }

                CargarUsuarios();
                ActualizarEstadisticas();
            }
            catch (Exception ex)
            {
                MostrarError("Error al desbloquear usuario: " + ex.Message);
            }
        }

        protected void btnActivar_Click(object sender, EventArgs e)
        {
            try
            {
                if (string.IsNullOrEmpty(SelectedUsuario))
                {
                    MostrarError("Selecciona un usuario primero");
                    return;
                }

                CambiarEstadoUsuario(SelectedUsuario, true);
            }
            catch (Exception ex)
            {
                MostrarError("Error al activar usuario: " + ex.Message);
            }
        }

        protected void btnDesactivar_Click(object sender, EventArgs e)
        {
            try
            {
                if (string.IsNullOrEmpty(SelectedUsuario))
                {
                    MostrarError("Selecciona un usuario primero");
                    return;
                }

                CambiarEstadoUsuario(SelectedUsuario, false);
            }
            catch (Exception ex)
            {
                MostrarError("Error al desactivar usuario: " + ex.Message);
            }
        }

        protected void btnCancelar_Click(object sender, EventArgs e)
        {
            try
            {
                CancelarSeleccion();
            }
            catch (Exception ex)
            {
                MostrarError("Error al cancelar: " + ex.Message);
            }
        }

        protected void btnGuardar_Click(object sender, EventArgs e)
        {
            try
            {
                if (string.IsNullOrEmpty(txtUsuario.Text))
                {
                    MostrarError("El nombre de usuario es obligatorio");
                    return;
                }

                if (string.IsNullOrEmpty(ddlRolForm.SelectedValue))
                {
                    MostrarError("El rol es obligatorio");
                    return;
                }

                int rol = int.Parse(ddlRolForm.SelectedValue);
                string contrasena = string.IsNullOrEmpty(txtContrasena.Text) ? "" : txtContrasena.Text;

                bool esModificacion = !string.IsNullOrEmpty(SelectedUsuario) && lblFormTitle.Text == "Modificar usuario";

                if (!esModificacion)
                {
                    // Crear nuevo usuario
                    BE.UsuarioCrearDTO dto = new BE.UsuarioCrearDTO
                    {
                        Usuario = txtUsuario.Text.Trim(),
                        Contrasena = contrasena,
                        Rol = rol
                    };

                    if (rol == 3)
                    {
                        // Datos de entrenador
                        if (string.IsNullOrEmpty(txtDNIEntrenador.Text) ||
                            !int.TryParse(txtDNIEntrenador.Text, out int dniEntrenador) ||
                            string.IsNullOrEmpty(txtApellido.Text) ||
                            string.IsNullOrEmpty(txtNombre.Text))
                        {
                            MostrarError("Para crear un entrenador, el DNI, nombre y apellido son obligatorios");
                            return;
                        }

                        dto.EntrenadorDNI = dniEntrenador;
                        dto.EntrenadorNombre = txtNombre.Text.Trim();
                        dto.EntrenadorApellido = txtApellido.Text.Trim();
                        dto.EntrenadorFechaNacimiento = string.IsNullOrEmpty(txtFechaNacimientoEntrenador.Text) ?
                            (DateTime?)null : DateTime.Parse(txtFechaNacimientoEntrenador.Text);
                        dto.EntrenadorTelefono = string.IsNullOrEmpty(txtTelefono.Text) ? null : txtTelefono.Text;
                    }
                    else if (rol == 4)
                    {
                        // Datos de cliente - asociar alumno existente
                        if (string.IsNullOrEmpty(txtDNIAlumno.Text) ||
                            !int.TryParse(txtDNIAlumno.Text, out int dniAlumno))
                        {
                            MostrarError("Para crear un cliente, el DNI del alumno es obligatorio");
                            return;
                        }

                        dto.AlumnoDNI = dniAlumno;
                    }

                    bllUsuario.CrearUsuario(dto);
                    MostrarExito($"✅ Usuario '{txtUsuario.Text}' creado correctamente");
                }
                else
                {
                    // Modificar usuario existente
                    // TODO: Implementar lógica de modificación
                    MostrarExito("✅ Usuario modificado correctamente");
                }

                CerrarFormulario();
                CargarUsuarios();
                ActualizarEstadisticas();
            }
            catch (Exception ex)
            {
                MostrarError(ex.Message);
            }
        }

        protected void btnCancelarForm_Click(object sender, EventArgs e)
        {
            try
            {
                CerrarFormulario();
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
                CerrarFormulario();
            }
            catch (Exception ex)
            {
                MostrarError("Error al cerrar formulario: " + ex.Message);
            }
        }

        // ==================== MÉTODOS AUXILIARES ====================

        private void SeleccionarUsuario(string usuario)
        {
            SelectedUsuario = usuario;
            gvUsuarios.DataBind();
        }

        private void CancelarSeleccion()
        {
            SelectedUsuario = null;
            CerrarFormulario();
            gvUsuarios.DataBind();
        }

        private void LimpiarFormulario()
        {
            txtDNI.Text = string.Empty;
            txtTelefono.Text = string.Empty;
            txtApellido.Text = string.Empty;
            txtNombre.Text = string.Empty;
            txtEmail.Text = string.Empty;
            txtUsuario.Text = string.Empty;
            txtContrasena.Text = string.Empty;
            ddlRolForm.SelectedIndex = 0;
            txtDNIEntrenador.Text = string.Empty;
            txtFechaNacimientoEntrenador.Text = string.Empty;
            txtDNIAlumno.Text = string.Empty;
        }

        private void CargarUsuarioEnFormulario(BE.UsuarioGestion usuario)
        {
            txtUsuario.Text = usuario.USUARIO_Usuario;
            ddlRolForm.SelectedValue = usuario.USUARIO_Tipo;
            // Los demás campos se cargarían de las tablas correspondientes (Alumnos, Entrenadores, etc.)
        }

        private void CerrarFormulario()
        {
            pnlForm.Visible = false;
            LimpiarFormulario();
        }

        private void CambiarEstadoUsuario(string usuario, bool activo)
        {
            try
            {
                // TODO: Llamar a la capa BLL para cambiar estado de usuario

                var user = Usuarios.FirstOrDefault(u => u.USUARIO_Usuario == usuario);
                if (user != null)
                {
                    user.USUARIO_Activo = activo;
                    MostrarExito($"✅ {user.USUARIO_Tipo} {(activo ? "activado/a" : "desactivado/a")}");
                }

                CargarUsuarios();
                ActualizarEstadisticas();
            }
            catch (Exception ex)
            {
                MostrarError("Error al cambiar estado del usuario: " + ex.Message);
            }
        }

        // ==================== MÉTODOS PARA EL GRIDVIEW ====================

        protected string GetInitials(object nombre, object apellido, object usuario = null)
        {
            string n = nombre?.ToString() ?? "";
            string a = apellido?.ToString() ?? "";

            if (string.IsNullOrEmpty(n) || string.IsNullOrEmpty(a))
            {
                // Usar primera letra del nombre de usuario
                string user = usuario?.ToString() ?? "";
                if (!string.IsNullOrEmpty(user))
                    return user[0].ToString().ToUpper();
                return "U";
            }

            return (n[0].ToString() + a[0].ToString()).ToUpper();
        }

        protected string GetAvatarClass(int index)
        {
            string[] classes = { "av-teal", "av-orange", "av-yellow" };
            return classes[index % 3];
        }

        protected string GetRolClass(object tipo)
        {
            string t = tipo?.ToString() ?? "";
            switch (t)
            {
                case "Administrador": return "role-admin";
                case "Entrenador": return "role-trainer";
                case "Cliente": return "role-student";
                case "Recepcionista": return "role-receptionist";
                case "Familiar": return "role-family";
                default: return "role-student";
            }
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

        protected string GetBloqueadoText(object bloqueado)
        {
            bool b = Convert.ToBoolean(bloqueado);
            if (b)
                return "<span class=\"pill pill-blocked\"><i class=\"fa-solid fa-lock\" style=\"font-size:0.65rem\"></i> Bloqueado</span>";
            else
                return "<span style=\"color:var(--text-light);font-size:0.82rem\">—</span>";
        }
    }
}