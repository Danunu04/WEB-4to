using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.UI;
using System.Web.UI.WebControls;
using BE;
using BLL;
using gymAppV2;
using SERVICIOS;

namespace gymAppV2.Usuarios
{
    public partial class UsuariosModulo : BasePage
    {
        private string SelectedUsuario
        {
            get => ViewState["SelectedUsuario"] as string;
            set => ViewState["SelectedUsuario"] = value;
        }

        // Lista completa de usuarios (sin filtrar)
        private List<BE.UsuarioGestion> _todosUsuarios
        {
            get => ViewState["_todosUsuarios"] as List<BE.UsuarioGestion>;
            set => ViewState["_todosUsuarios"] = value;
        }

        // Lista filtrada actual para mostrar en el grid
        private List<BE.UsuarioGestion> Usuarios
        {
            get => ViewState["Usuarios"] as List<BE.UsuarioGestion>;
            set => ViewState["Usuarios"] = value;
        }

        private BLLUsuario bllUsuario;

        protected void Page_Load(object sender, EventArgs e)
        {
            VerificarAcceso("GestionUsuarios");

            bllUsuario = new BLLUsuario();

            if (!IsPostBack)
            {
                CargarUsuarios();
                ActualizarEstadisticas();
            }
        }

        // ==================== MÉTODOS PRINCIPALES ====================

        private void CargarUsuarios()
        {
            // Obtener usuarios de la base de datos - guardar lista completa
            _todosUsuarios = bllUsuario.ListarUsuarios() ?? new List<UsuarioGestion>();

            AplicarFiltros();
            gvUsuarios.DataSource = Usuarios;
            gvUsuarios.DataBind();

            ActualizarFooter();
        }

        private void AplicarFiltros()
        {
            // TODO: Llamar a la capa BLL para filtrar usuarios
            // Ejemplo: Usuarios = BLL.UsuarioBLL.FiltrarUsuarios(filtros);

            if (_todosUsuarios == null)
            {
                Usuarios = new List<UsuarioGestion>();
                return;
            }

            var filtrados = _todosUsuarios.AsEnumerable();

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
            // TODO: Llamar a la capa BLL para obtener estadísticas
            // Ejemplo: var stats = BLL.UsuarioBLL.ObtenerEstadisticas();

            // Por ahora, cálculos locales para visualización
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

        /// <summary>
        /// Escapa un texto para usarlo de forma segura dentro de una cadena JavaScript entre comillas simples.
        /// </summary>
        private string EscaparParaJs(string mensaje)
        {
            if (string.IsNullOrEmpty(mensaje))
            {
                return string.Empty;
            }

            return mensaje
                .Replace("\\", "\\\\")
                .Replace("'", "\\'")
                .Replace("\r", "\\r")
                .Replace("\n", "\\n");
        }

        private void MostrarMensaje(string mensaje, string tipo, string cssClass)
        {
            // Toast (si la página provee la función showToast)
            string script = $"if(typeof window.showToast==='function'){{window.showToast('{EscaparParaJs(mensaje)}','{EscaparParaJs(tipo)}');}}";
            ClientScript.RegisterStartupScript(this.GetType(), Guid.NewGuid().ToString(), script, true);

            // Fallback visual dentro del formulario
            if (lblMensajeForm != null)
            {
                lblMensajeForm.Text = mensaje;
                lblMensajeForm.CssClass = cssClass;
                lblMensajeForm.Visible = true;
            }
        }

        private void MostrarError(string mensaje)
        {
            MostrarMensaje(mensaje, "error", "mensaje-error");
        }

        private void MostrarExito(string mensaje)
        {
            MostrarMensaje(mensaje, "success", "mensaje-exito");
        }

        private void MostrarAdvertencia(string mensaje)
        {
            MostrarMensaje(mensaje, "warning", "mensaje-advertencia");
        }

        private void MostrarInfo(string mensaje)
        {
            MostrarMensaje(mensaje, "info", "mensaje-info");
        }

        /// <summary>
        /// Recolecta los mensajes de validación de ASP.NET que no pasaron y los muestra en el label fallback.
        /// </summary>
        private void MostrarErroresValidacion()
        {
            var sb = new StringBuilder();
            foreach (IValidator validator in Validators)
            {
                if (!validator.IsValid)
                {
                    sb.AppendLine("• " + validator.ErrorMessage);
                }
            }

            if (sb.Length > 0)
            {
                MostrarError(sb.ToString().Trim());
            }
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

        protected void ddlRolForm_SelectedIndexChanged(object sender, EventArgs e)
        {
            MostrarCamposSegunRol();
        }

        private void MostrarCamposSegunRol()
        {
            string rol = ddlRolForm.SelectedValue;

            // Mostrar/ocultar validator de fecha de nacimiento segun el rol
            RequiredFieldValidator8.Enabled = (rol == "3" || rol == "4"); // Entrenador o Cliente

            if (rol == "3")
            {
                // Entrenador - mostrar campos de entrenador
                EntField.Visible = true;
                clienteFields.Visible = false;
            }
            else if (rol == "4")
            {
                // Cliente - mostrar campo para asociar alumno
                EntField.Visible = false;
                clienteFields.Visible = true;
            }
            else
            {
                // Otros roles - no mostrar campos adicionales
                EntField.Visible = false;
                clienteFields.Visible = false;
            }
        }

        protected void txtBusqueda_TextChanged(object sender, EventArgs e)
        {
            CargarUsuarios();
        }

        protected void btnFiltrar_Click(object sender, EventArgs e)
        {
            CargarUsuarios();
        }

        // ==================== EVENTOS DE GRIDVIEW ====================

        protected void gvUsuarios_PageIndexChanging(object sender, GridViewPageEventArgs e)
        {
            gvUsuarios.PageIndex = e.NewPageIndex;
            CargarUsuarios();
        }

        protected void gvUsuarios_RowCommand(object sender, GridViewCommandEventArgs e)
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
            // TODO: Llamar a la capa BLL para exportar usuarios
            // Ejemplo: BLL.UsuarioBLL.ExportarUsuarios();
        }

        protected void btnActualizar_Click(object sender, EventArgs e)
        {
            CargarUsuarios();
            ActualizarEstadisticas();
        }

        protected void btnCrear_Click(object sender, EventArgs e)
        {
            LimpiarFormulario();
            lblFormTitle.Text = "Nuevo usuario";
            pnlForm.Visible = true;
            passwordRow.Visible = true;
        }

        protected void btnModificar_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(SelectedUsuario))
            {
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

        protected void btnDesbloquear_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(SelectedUsuario))
            {
                MostrarError("Seleccione un usuario para desbloquear");
                return;
            }

            try
            {
                bllUsuario.DesbloquearUsuario(SelectedUsuario);
                MostrarExito($"Usuario '{SelectedUsuario}' desbloqueado correctamente");
            }
            catch (Exception ex)
            {
                MostrarError("Error al desbloquear: " + ex.Message);
            }

            CargarUsuarios();
            ActualizarEstadisticas();
        }

        protected void btnActivar_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(SelectedUsuario))
            {
                MostrarError("Seleccione un usuario para activar");
                return;
            }

            CambiarEstadoUsuario(SelectedUsuario, true);
        }

        protected void btnDesactivar_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(SelectedUsuario))
            {
                MostrarError("Seleccione un usuario para desactivar");
                return;
            }

            CambiarEstadoUsuario(SelectedUsuario, false);
        }

        protected void btnCancelar_Click(object sender, EventArgs e)
        {
            CancelarSeleccion();
        }

        protected void btnGuardar_Click(object sender, EventArgs e)
        {
            if (!IsValid)
            {
                MostrarErroresValidacion();
                return;
            }

            string nombre = txtNombre.Text;
            string apellido = txtApellido.Text;
            if(!int.TryParse(txtDNI.Text, out int dni))
            {
                MostrarError("El DNI debe ser un número válido");
                return;
            }
            if(!int.TryParse(txtTelefono.Text, out int telefono))
            {
                MostrarError("El Teléfono debe ser un número válido");
                return;
            }
            string usuario = txtUsuario.Text;
            string contrasenia = string.IsNullOrEmpty(txtContrasena.Text) ? bllUsuario.GenerarContrasenaSegura() : txtContrasena.Text;
            string email = txtEmail.Text;
            DateTime? fechaNacimiento = null;
            bool activo = ddlEstadoForm.SelectedValue == "1";

            // Validar fecha de nacimiento para Entrenador o Cliente
            string rolSeleccionado = ddlRolForm.SelectedValue;
            if ((rolSeleccionado == "3" || rolSeleccionado == "4") && string.IsNullOrEmpty(txtFechaNacimiento.Text))
            {
                MostrarError("La fecha de nacimiento es obligatoria");
                return;
            }

            if (!string.IsNullOrEmpty(txtFechaNacimiento.Text))
            {
                fechaNacimiento = Convert.ToDateTime(txtFechaNacimiento.Text);
            }

            try
            {
                bool esModificacion = !string.IsNullOrEmpty(SelectedUsuario) && lblFormTitle.Text == "Modificar usuario";

                if (esModificacion)
                {
                    // Modificar usuario existente
                    int rol = int.Parse(ddlRolForm.SelectedValue);
                    bllUsuario.ModificarUsuario(SelectedUsuario, usuario, nombre, apellido,
                        telefono.ToString(), email, fechaNacimiento, rol, activo, dni);
                    MostrarExito("Usuario modificado correctamente");
                }
                else
                {
                    // Crear nuevo usuario
                    switch (ddlRolForm.SelectedValue)
                    {
                        case "1": // Administrador
                            bllUsuario.CrearUsuario(usuario, contrasenia, 1, nombre, apellido, telefono.ToString(), email, fechaNacimiento, null, null, null, activo);
                            MostrarExito("Administrador creado correctamente");
                            break;
                        case "2": // Recepcionista
                            bllUsuario.CrearUsuario(usuario, contrasenia, 2, nombre, apellido, telefono.ToString(), email, fechaNacimiento, null, null, null, activo);
                            MostrarExito("Recepcionista creado correctamente");
                            break;
                        case "3": // Entrenador
                            Entrenador ent = new Entrenador(dni, 0, activo, "", "", usuario);
                            bllUsuario.CrearUsuario(usuario, contrasenia, 3, nombre, apellido, telefono.ToString(), email, fechaNacimiento, ent, null, null, activo);
                            MostrarExito("Entrenador creado correctamente");
                            break;
                        case "4": // Cliente
                            // En esquema normalizado: se crea USUARIOS + ALUMNOS con el mismo DNI
                            bllUsuario.CrearUsuario(usuario, contrasenia, 4, nombre, apellido, telefono.ToString(), email, fechaNacimiento, null, dni, null, activo);
                            MostrarExito("Cliente creado correctamente");
                            break;
                        default:
                            MostrarError("Seleccione un rol válido");
                            return;
                    }
                }
            }
            catch (Exception ex)
            {
                MostrarError("Error al guardar usuario: " + ex.Message);
                return;
            }

            CerrarFormulario();
            CargarUsuarios();
            ActualizarEstadisticas();
        }

        protected void btnCancelarForm_Click(object sender, EventArgs e)
        {
            CerrarFormulario();
        }

        protected void btnCloseForm_Click(object sender, EventArgs e)
        {
            CerrarFormulario();
        }

        // ==================== MÉTODOS AUXILIARES ====================

        private void SeleccionarUsuario(string usuario)
        {
            SelectedUsuario = usuario;
        }

        private void CancelarSeleccion()
        {
            SelectedUsuario = null;
            //pnlSelectedPreview.Visible = false;
            CerrarFormulario();
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
            ddlEstadoForm.SelectedValue = "1";
            EntField.Visible = false;
            clienteFields.Visible = false;

            if (lblMensajeForm != null)
            {
                lblMensajeForm.Text = string.Empty;
                lblMensajeForm.Visible = false;
            }
        }

        private void CargarUsuarioEnFormulario(BE.UsuarioGestion usuario)
        {
            txtUsuario.Text = usuario.USUARIO_Usuario;
            txtDNI.Text = usuario.DNI?.ToString() ?? "";
            txtNombre.Text = usuario.Nombre ?? "";
            txtApellido.Text = usuario.Apellido ?? "";
            txtTelefono.Text = usuario.Telefono ?? "";
            txtEmail.Text = usuario.Email ?? "";
            txtFechaNacimiento.Text = usuario.FechaNacimiento?.ToString("yyyy-MM-dd") ?? "";
            ddlEstadoForm.SelectedValue = usuario.USUARIO_Activo ? "1" : "0";

            // Buscar el ListItem por texto y seleccionar su valor
            ListItem rolItem = ddlRolForm.Items.FindByText(usuario.USUARIO_Tipo);
            if (rolItem != null)
            {
                ddlRolForm.SelectedValue = rolItem.Value;
            }

            // Mostrar campos específicos según el rol
            MostrarCamposSegunRol();
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
                if (activo)
                {
                    bllUsuario.ActivarUsuario(usuario);
                    MostrarExito($"Usuario '{usuario}' activado correctamente");
                }
                else
                {
                    bllUsuario.DesactivarUsuario(usuario);
                    MostrarExito($"Usuario '{usuario}' desactivado correctamente");
                }
            }
            catch (Exception ex)
            {
                MostrarError("Error al cambiar estado: " + ex.Message);
            }

            CargarUsuarios();
            ActualizarEstadisticas();
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
                case "Alumno": return "role-student";
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
