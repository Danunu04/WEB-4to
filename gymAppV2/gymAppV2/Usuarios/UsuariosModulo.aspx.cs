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
            VerificarAcceso(BE.PermisosSistema.GestionUsuarios);

            bllUsuario = new BLLUsuario();

            txtFechaNacimiento.Attributes["max"] = DateTime.Today.ToString("yyyy-MM-dd");

            if (!IsPostBack)
            {
                AplicarIdioma();
                CargarUsuarios();
                ActualizarEstadisticas();
            }
        }

        public override void OnIdiomaChanged(IdiomaApp idioma)
        {
            base.OnIdiomaChanged(idioma);
            AplicarIdioma();
            ActualizarEstadisticas();
            // Rebind para que GetEstadoText/GetBloqueadoText devuelvan el nuevo idioma
            if (Usuarios != null)
            {
                gvUsuarios.DataSource = Usuarios;
                gvUsuarios.DataBind();
            }
        }

        private void AplicarIdioma()
        {
            litTitulo.Text          = T("usuarios_titulo");
            litStatTotal.Text       = T("usuarios_stat_total");
            litStatActivos.Text     = T("usuarios_stat_activos");
            litStatBloqueados.Text  = T("usuarios_stat_bloqueados");
            litStatInactivos.Text   = T("usuarios_stat_inactivos");
            litListaTitulo.Text     = T("usuarios_lista_titulo");
            litBtnCrear.Text        = T("usuarios_btn_crear");
            litBtnModificar.Text    = T("usuarios_btn_modificar");
            litBtnDesbloquear.Text  = T("usuarios_btn_desbloquear");
            litBtnBlanquear.Text    = T("usuarios_btn_blanquear");
            litBtnActivar.Text      = T("usuarios_btn_activar");
            litBtnDesactivar.Text   = T("usuarios_btn_desactivar");
            litBtnCancelar.Text     = T("btn_cancelar");
            litBtnGuardar.Text      = T("btn_guardar");
            litBtnCancelarForm.Text = T("btn_cancelar");
            lblFormTitle.Text       = T("usuarios_form_titulo");

            // Columnas del GridView (col 0: Usuario, 4: Estado, 5: Bloqueado)
            ((System.Web.UI.WebControls.TemplateField)gvUsuarios.Columns[0]).HeaderText = T("usuarios_col_usuario");
            ((System.Web.UI.WebControls.TemplateField)gvUsuarios.Columns[4]).HeaderText = T("usuarios_col_estado");
            ((System.Web.UI.WebControls.TemplateField)gvUsuarios.Columns[5]).HeaderText = T("usuarios_col_bloqueado");

            // Opciones de dropdowns de filtros
            ddlEstado.Items[0].Text   = T("usuarios_filtro_todos");
            ddlEstado.Items[1].Text   = T("usuarios_filtro_activados");
            ddlEstado.Items[2].Text   = T("usuarios_filtro_desactivados");

            ddlBloqueado.Items[0].Text = T("usuarios_filtro_todos");
            ddlBloqueado.Items[1].Text = T("usuarios_filtro_bloqueados");
            ddlBloqueado.Items[2].Text = T("usuarios_filtro_no_bloqueados");

            ddlRol.Items[0].Text = T("usuarios_filtro_todos_roles");

            txtBusqueda.Attributes["placeholder"] = T("usuarios_buscar_placeholder");
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

            badgeCount.InnerText = lblTotal.Text;
        }

        private void ActualizarFooter()
        {
            int total = Usuarios?.Count ?? 0;
            footerText.InnerText = string.Format(T("msg_mostrando_fmt"), total, total);
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
            lblFormTitle.Text = T("usuarios_form_nuevo");
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
                lblFormTitle.Text = T("usuarios_form_modificar");
                pnlForm.Visible = true;
                passwordRow.Visible = false;
            }
        }

        protected void btnDesbloquear_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(SelectedUsuario))
            {
                MostrarError(T("usuarios_msg_sel_desbloquear"));
                return;
            }

            try
            {
                bllUsuario.DesbloquearUsuario(SelectedUsuario);
                MostrarExito(T("usuarios_msg_desbloqueado"));
            }
            catch (Exception)
            {
                MostrarError(T("msg_error_generico"));
            }

            CargarUsuarios();
            ActualizarEstadisticas();
        }

        protected void btnBlanquearContrasena_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(SelectedUsuario))
            {
                MostrarError(T("usuarios_msg_sel_blanquear"));
                return;
            }

            try
            {
                bllUsuario.BlanquearContrasena(SelectedUsuario);
                MostrarExito(T("usuarios_msg_blanqueado"));
            }
            catch (Exception)
            {
                MostrarError(T("msg_error_generico"));
            }

            CargarUsuarios();
        }

        protected void btnActivar_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(SelectedUsuario))
            {
                MostrarError(T("usuarios_msg_sel_activar"));
                return;
            }

            CambiarEstadoUsuario(SelectedUsuario, true);
        }

        protected void btnDesactivar_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(SelectedUsuario))
            {
                MostrarError(T("usuarios_msg_sel_desactivar"));
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
            if (!int.TryParse(txtDNI.Text, out int dni))
            {
                MostrarError(T("msg_dni_invalido"));
                return;
            }
            string telefono = txtTelefono.Text.Trim();
            string usuario = txtUsuario.Text;
            string contrasenia = string.IsNullOrEmpty(txtContrasena.Text) ? bllUsuario.GenerarContrasenaSegura() : txtContrasena.Text;
            string email = txtEmail.Text;
            DateTime? fechaNacimiento = null;
            bool activo = ddlEstadoForm.SelectedValue == "1";

            // Validar fecha de nacimiento para Entrenador o Cliente
            string rolSeleccionado = ddlRolForm.SelectedValue;
            if ((rolSeleccionado == "3" || rolSeleccionado == "4") && string.IsNullOrEmpty(txtFechaNacimiento.Text))
            {
                MostrarError(T("alumnos_msg_fecha_oblig"));
                return;
            }

            if (!string.IsNullOrEmpty(txtFechaNacimiento.Text))
            {
                fechaNacimiento = Convert.ToDateTime(txtFechaNacimiento.Text);
            }

            try
            {
                bool esModificacion = !string.IsNullOrEmpty(SelectedUsuario) && lblFormTitle.Text == T("usuarios_form_modificar");

                if (esModificacion)
                {
                    int rol = int.Parse(ddlRolForm.SelectedValue);
                    bllUsuario.ModificarUsuario(SelectedUsuario, usuario, nombre, apellido,
                        telefono, email, fechaNacimiento, rol, activo, dni);
                    MostrarExito(T("usuarios_msg_modificado"));
                }
                else
                {
                    switch (ddlRolForm.SelectedValue)
                    {
                        case "5":
                            bllUsuario.CrearUsuario(usuario, contrasenia, 5, nombre, apellido, telefono, email, fechaNacimiento, null, dni, null, activo);
                            MostrarExito(T("usuarios_msg_creado"));
                            break;
                        case "1":
                            bllUsuario.CrearUsuario(usuario, contrasenia, 1, nombre, apellido, telefono, email, fechaNacimiento, null, dni, null, activo);
                            MostrarExito(T("usuarios_msg_creado"));
                            break;
                        case "2":
                            bllUsuario.CrearUsuario(usuario, contrasenia, 2, nombre, apellido, telefono, email, fechaNacimiento, null, dni, null, activo);
                            MostrarExito(T("usuarios_msg_creado"));
                            break;
                        case "3":
                            Entrenador ent = new Entrenador(dni, 0, activo, "", usuario);
                            bllUsuario.CrearUsuario(usuario, contrasenia, 3, nombre, apellido, telefono, email, fechaNacimiento, ent, null, null, activo);
                            MostrarExito(T("usuarios_msg_creado"));
                            break;
                        case "4":
                            bllUsuario.CrearUsuario(usuario, contrasenia, 4, nombre, apellido, telefono, email, fechaNacimiento, null, dni, null, activo);
                            MostrarExito(T("usuarios_msg_creado"));
                            break;
                        default:
                            MostrarError(T("usuarios_msg_rol_invalido"));
                            return;
                    }
                }
            }
            catch (Exception)
            {
                MostrarError(T("msg_error_generico"));
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
            txtUsuario.ReadOnly = false;
            txtUsuario.Attributes.Remove("style");
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
            txtUsuario.ReadOnly = true;
            txtUsuario.Attributes["style"] = "background-color:#e9ecef;cursor:not-allowed;opacity:0.8;";
            txtDNI.Text = usuario.DNI?.ToString() ?? "";
            txtNombre.Text = usuario.Nombre ?? "";
            txtApellido.Text = usuario.Apellido ?? "";
            txtTelefono.Text = usuario.Telefono ?? "";
            txtEmail.Text = usuario.Email ?? "";
            txtFechaNacimiento.Text = usuario.FechaNacimiento?.ToString("yyyy-MM-dd") ?? "";
            ddlEstadoForm.SelectedValue = usuario.USUARIO_Activo ? "1" : "0";
            ddlRolForm.SelectedValue = usuario.USUARIO_Rol.ToString();

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
                    MostrarExito(T("usuarios_msg_activado"));
                }
                else
                {
                    bllUsuario.DesactivarUsuario(usuario);
                    MostrarExito(T("usuarios_msg_desactivado"));
                }
            }
            catch (Exception)
            {
                MostrarError(T("msg_error_generico"));
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
                case "WebMaster": return "role-webmaster";
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
            return a ? T("usuarios_estado_activo") : T("usuarios_estado_inactivo");
        }

        protected string GetBloqueadoText(object bloqueado)
        {
            bool b = Convert.ToBoolean(bloqueado);
            if (b)
                return $"<span class=\"pill pill-blocked\"><i class=\"fa-solid fa-lock\" style=\"font-size:0.65rem\"></i> {T("usuarios_bloqueado_si")}</span>";
            else
                return "<span style=\"color:var(--text-light);font-size:0.82rem\">—</span>";
        }
    }
}
