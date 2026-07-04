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
        private string SelectedUsuario
        {
            get { return ViewState["SelectedUsuario"] as string; }
            set { ViewState["SelectedUsuario"] = value; }
        }
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

                bllUsuario.DesbloquearUsuario(SelectedUsuario);

                var usuario = Usuarios.FirstOrDefault(u => u.USUARIO_Usuario == SelectedUsuario);
                if (usuario != null)
                {
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

                // Datos personales comunes
                if (string.IsNullOrEmpty(txtDNI.Text) || !int.TryParse(txtDNI.Text, out int dni))
                {
                    MostrarError("El DNI es obligatorio y debe ser numérico");
                    return;
                }

                if (string.IsNullOrEmpty(txtNombre.Text) ||
                    string.IsNullOrEmpty(txtApellido.Text) ||
                    string.IsNullOrEmpty(txtFechaNacimiento.Text) ||
                    !DateTime.TryParse(txtFechaNacimiento.Text, out DateTime fechaNacimiento))
                {
                    MostrarError("El nombre, apellido y fecha de nacimiento son obligatorios");
                    return;
                }

                string nombre = txtNombre.Text.Trim();
                string apellido = txtApellido.Text.Trim();
                string telefono = string.IsNullOrEmpty(txtTelefono.Text) ? null : txtTelefono.Text.Trim();
                string email = string.IsNullOrEmpty(txtEmail.Text) ? null : txtEmail.Text.Trim();

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
                        dto.EntrenadorDNI = dni;
                        dto.EntrenadorNombre = nombre;
                        dto.EntrenadorApellido = apellido;
                        dto.EntrenadorFechaNacimiento = fechaNacimiento;
                        dto.EntrenadorTelefono = telefono;
                    }
                    else if (rol == 4)
                    {
                        dto.AlumnoDNI = dni;
                        dto.AlumnoNombre = nombre;
                        dto.AlumnoApellido = apellido;
                        dto.AlumnoFechaNacimiento = fechaNacimiento;
                        dto.AlumnoTelefono = telefono;
                        dto.AlumnoEmail = email;
                    }

                    bllUsuario.CrearUsuario(dto);
                    MostrarExito($"✅ Usuario '{txtUsuario.Text}' creado correctamente");
                }
                else
                {
                    // Modificar usuario existente
                    bool activo = ddlEstadoForm.SelectedValue == "1";
                    bllUsuario.ModificarUsuario(
                        SelectedUsuario,
                        txtUsuario.Text.Trim(),
                        nombre,
                        apellido,
                        telefono,
                        email,
                        fechaNacimiento,
                        rol,
                        activo,
                        dni);

                    MostrarExito($"✅ Usuario '{txtUsuario.Text}' modificado correctamente");
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
            txtFechaNacimiento.Text = string.Empty;
            ddlRolForm.SelectedIndex = 0;
            ddlEstadoForm.SelectedIndex = 0;
        }

        private void CargarUsuarioEnFormulario(BE.UsuarioGestion usuario)
        {
            txtUsuario.Text = usuario.USUARIO_Usuario;
            ddlRolForm.SelectedValue = ObtenerRolValue(usuario.USUARIO_Tipo);
            ddlEstadoForm.SelectedValue = usuario.USUARIO_Activo ? "1" : "0";

            txtDNI.Text = usuario.DNI.HasValue ? usuario.DNI.Value.ToString() : string.Empty;
            txtNombre.Text = usuario.Nombre ?? string.Empty;
            txtApellido.Text = usuario.Apellido ?? string.Empty;
            txtTelefono.Text = usuario.Telefono ?? string.Empty;
            txtEmail.Text = usuario.Email ?? string.Empty;
            txtFechaNacimiento.Text = usuario.FechaNacimiento.HasValue
                ? usuario.FechaNacimiento.Value.ToString("yyyy-MM-dd")
                : string.Empty;
        }

        private void CerrarFormulario()
        {
            pnlForm.Visible = false;
            LimpiarFormulario();
            SelectedUsuario = null;
        }

        private void CambiarEstadoUsuario(string usuario, bool activo)
        {
            try
            {
                if (activo)
                    bllUsuario.ActivarUsuario(usuario);
                else
                    bllUsuario.DesactivarUsuario(usuario);

                var user = Usuarios.FirstOrDefault(u => u.USUARIO_Usuario == usuario);
                if (user != null)
                {
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

        private string ObtenerRolValue(string tipo)
        {
            switch (tipo)
            {
                case "Administrador": return "1";
                case "Recepcionista": return "2";
                case "Entrenador": return "3";
                case "Cliente": return "4";
                default: return "";
            }
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