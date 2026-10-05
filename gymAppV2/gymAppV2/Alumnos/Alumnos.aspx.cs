using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.UI;
using System.Web.UI.WebControls;
using BLL;
using BE;
using gymAppV2;
using gymAppV2.WebServices;
using Servicios.Singleton;

namespace gymAppV2.Alumnos
{
    public partial class Alumnos : BasePage
    {
        private BLLAlumno bllAlumno;
        private BLLUsuario bllUsuario;
        private BLLEvento bllEvento;

        /// <summary>
        /// Pasos del asistente de alta/vínculo. Un único panel modal (pnlAsistente) cambia
        /// de contenido según el paso actual, guardado en ViewState para sobrevivir postbacks.
        /// </summary>
        private enum PasoAsistente
        {
            Ninguno = 0,
            PreguntarTutor = 1,             // menor de edad: ¿asociar con un familiar?
            BuscarUsuarioVinculo = 2,       // pedir DNI (+ parentesco) del usuario a vincular
            ConfirmarVinculoExistente = 3,  // el usuario buscado ya existe -> confirmar vínculo
            CompletarDatosVinculo = 4,      // el usuario buscado no existe -> crearlo como Familiar
            ConfirmarAlumnoExistente = 5,   // (alta principal) la persona ya existe -> confirmar alta como Alumno
            CompletarDatosAlumno = 6        // (alta principal) la persona no existe -> crear Usuario+Alumno
        }

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

        private bool EsSoloLectura
        {
            get { return Singleton.Instancia.Usuario?.USUARIO_Rol == 4 || Singleton.Instancia.Usuario?.USUARIO_Rol == 6; }
        }

        private PasoAsistente Paso
        {
            get { return (PasoAsistente)(ViewState["PasoAsistente"] ?? PasoAsistente.Ninguno); }
            set { ViewState["PasoAsistente"] = value; }
        }

        // DNI de la persona que se está evaluando actualmente en el asistente
        // (el propio alumno, o el usuario/familiar que se está buscando para vincular).
        private int? DniEvaluado
        {
            get { return ViewState["DniEvaluado"] as int?; }
            set { ViewState["DniEvaluado"] = value; }
        }

        // DNI del alumno (nuevo o ya existente) sobre el que se está trabajando en el asistente.
        private int? DniAlumnoPendiente
        {
            get { return ViewState["DniAlumnoPendiente"] as int?; }
            set { ViewState["DniAlumnoPendiente"] = value; }
        }

        // true cuando el asistente se abrió desde "Vincular familiar" sobre un alumno ya existente
        // en la grilla (en vez de venir del flujo de alta de un alumno nuevo).
        private bool VincularAAlumnoExistente
        {
            get { return ViewState["VincularAAlumnoExistente"] as bool? ?? false; }
            set { ViewState["VincularAAlumnoExistente"] = value; }
        }

        // Usuario/parentesco al que hay que vincular el alumno una vez resuelto.
        // Null = auto-vincular a su propio titular (caso mayor de edad sin tutor).
        private string VinculoUsuarioObjetivo
        {
            get { return ViewState["VinculoUsuarioObjetivo"] as string; }
            set { ViewState["VinculoUsuarioObjetivo"] = value; }
        }

        private string VinculoParentescoObjetivo
        {
            get { return ViewState["VinculoParentescoObjetivo"] as string; }
            set { ViewState["VinculoParentescoObjetivo"] = value; }
        }

        // Credenciales autogeneradas durante el flujo, para mostrarlas una única vez al terminar.
        private List<string> CredencialesGeneradas
        {
            get { return ViewState["CredencialesGeneradas"] as List<string> ?? new List<string>(); }
            set { ViewState["CredencialesGeneradas"] = value; }
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

            if (ddlUsuario.Items.Count >= 3)
            {
                ddlUsuario.Items[0].Text = T("alumnos_filtro_todos");
                ddlUsuario.Items[1].Text = T("alumnos_filtro_con_usuario");
                ddlUsuario.Items[2].Text = T("alumnos_filtro_sin_usuario");
            }

            txtBusqueda.Attributes["placeholder"] = T("alumnos_buscar_placeholder");
            litFamiliaresVacio.Text = T("alumnos_familiares_vacio");
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
                List<Alumno> alumnos;

                // La consulta a la base la hace el web service (WebServices/AlumnosWS.asmx):
                // la página le pide los alumnos por SOAP y solo se encarga de mostrarlos.
                using (var ws = AlumnosWSCliente.Crear(this))
                {
                    // Un Cliente/Familiar solo ve los alumnos vinculados a su cuenta
                    // (el servicio toma el usuario de la cookie de login)
                    if (EsSoloLectura)
                    {
                        alumnos = ws.ObtenerMisAlumnos() ?? new List<Alumno>();
                    }
                    else
                    {
                        alumnos = ws.ListarAlumnos() ?? new List<Alumno>();
                    }
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
                        alumnos = alumnos.Where(a => a.Familiares.Count > 0).ToList();
                    else if (filtro == "sin_usuario")
                        alumnos = alumnos.Where(a => a.Familiares.Count == 0).ToList();
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
                lblSinUsuario.Text = alumnos.Count(a => a.Familiares.Count == 0).ToString();

                badgeCount.InnerText = lblTotal.Text;
                footerText.InnerText = string.Format(T("msg_mostrando_fmt"), alumnos.Count, alumnos.Count);
            }
            catch (Exception)
            {
                MostrarError(T("msg_error_generico"));
                footerText.InnerText = string.Format(T("msg_mostrando_fmt"), 0, 0);
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
                btnGuardar.Visible = false;
                btnContinuarAlta.Visible = true;
                pnlFamiliares.Visible = false;
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

                CargarFamiliares(alumno);

                lblFormTitle.Text = T("alumnos_form_modificar");
                btnGuardar.Visible = true;
                btnContinuarAlta.Visible = false;
                pnlFamiliares.Visible = true;
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

        /// <summary>
        /// Botón "Vincular familiar" de la barra de acciones: abre el asistente para vincular
        /// una cuenta de usuario (titular o familiar) a un alumno ya existente en la grilla.
        /// </summary>
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

                DniAlumnoPendiente = alumno.DNI;
                VincularAAlumnoExistente = true;
                VinculoUsuarioObjetivo = null;
                VinculoParentescoObjetivo = null;
                Paso = PasoAsistente.BuscarUsuarioVinculo;
                txtAsistenteDniVinculo.Text = "";
                ddlAsistenteParentesco.SelectedIndex = 0;
                MostrarAsistente();
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

        // ==================== ASISTENTE DE ALTA (edad, tutor, familiares) ====================

        /// <summary>
        /// Botón "Confirmar" del formulario de alta. Valida DNI y fecha de nacimiento,
        /// calcula la edad y arranca el flujo correspondiente (mayor: buscar por DNI /
        /// menor: preguntar por un familiar).
        /// </summary>
        protected void btnContinuarAlta_Click(object sender, EventArgs e)
        {
            try
            {
                if (EsModificacion) return;

                if (string.IsNullOrWhiteSpace(txtDNI.Text))
                {
                    MostrarError(T("alumnos_msg_dni_obligatorio"));
                    return;
                }
                if (!int.TryParse(txtDNI.Text.Trim(), out int dni) || txtDNI.Text.Trim().Length < 7 || txtDNI.Text.Trim().Length > 8)
                {
                    MostrarError(T("alumnos_msg_dni_invalido"));
                    return;
                }
                if (string.IsNullOrWhiteSpace(txtFechaNacimiento.Text))
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

                if (bllAlumno.AlumnoExiste(dni))
                {
                    MostrarError(T("alumnos_msg_ya_existe"));
                    return;
                }

                VincularAAlumnoExistente = false;
                int edad = BLLUsuario.CalcularEdad(fechaNacimiento);

                if (edad >= 18)
                {
                    ResolverAlumnoPrincipal(dni, vincularA: null, parentesco: null);
                }
                else
                {
                    DniAlumnoPendiente = dni;
                    Paso = PasoAsistente.PreguntarTutor;
                    MostrarAsistente();
                }
            }
            catch (Exception)
            {
                MostrarError(T("msg_error_generico"));
            }
        }

        /// <summary>
        /// Busca a la persona (alumno principal) por DNI y decide el siguiente paso:
        /// si ya existe como Usuario, pide confirmar el alta como Alumno; si no, pide completar datos.
        /// </summary>
        private void ResolverAlumnoPrincipal(int dni, string vincularA, string parentesco)
        {
            DniEvaluado = dni;
            DniAlumnoPendiente = dni;
            VinculoUsuarioObjetivo = vincularA;
            VinculoParentescoObjetivo = parentesco;

            var usuarioExistente = bllUsuario.ObtenerUsuarioPorDni(dni);
            if (usuarioExistente == null)
            {
                Paso = PasoAsistente.CompletarDatosAlumno;
                txtAsistenteUsuarioSugerido.Text = $"cliente_{dni}";
                txtAsistenteContrasena.Text = "";
            }
            else
            {
                Paso = PasoAsistente.ConfirmarAlumnoExistente;
                lblAsistenteMensaje.Text = string.Format(T("alumnos_asistente_confirmar_alumno_fmt"),
                    usuarioExistente.Apellido, usuarioExistente.Nombre, dni, usuarioExistente.USUARIO_Tipo);
            }
            MostrarAsistente();
        }

        protected void btnAsistenteTutorSi_Click(object sender, EventArgs e)
        {
            Paso = PasoAsistente.BuscarUsuarioVinculo;
            txtAsistenteDniVinculo.Text = "";
            ddlAsistenteParentesco.SelectedIndex = 0;
            MostrarAsistente();
        }

        protected void btnAsistenteTutorNo_Click(object sender, EventArgs e)
        {
            if (!DniAlumnoPendiente.HasValue) { CerrarAsistente(); return; }
            ResolverAlumnoPrincipal(DniAlumnoPendiente.Value, vincularA: null, parentesco: null);
        }

        protected void btnAsistenteBuscarVinculo_Click(object sender, EventArgs e)
        {
            try
            {
                if (!int.TryParse(txtAsistenteDniVinculo.Text, out int dniVinculo))
                {
                    MostrarError(T("alumnos_msg_dni_invalido"));
                    return;
                }

                if (DniAlumnoPendiente.HasValue && dniVinculo == DniAlumnoPendiente.Value)
                {
                    MostrarError(T("alumnos_asistente_error_mismo_dni"));
                    return;
                }

                DniEvaluado = dniVinculo;
                VinculoParentescoObjetivo = ddlAsistenteParentesco.SelectedValue;

                var usuarioExistente = bllUsuario.ObtenerUsuarioPorDni(dniVinculo);
                if (usuarioExistente == null)
                {
                    Paso = PasoAsistente.CompletarDatosVinculo;
                    txtAsistenteVinculoNombre.Text = "";
                    txtAsistenteVinculoApellido.Text = "";
                    txtAsistenteVinculoTelefono.Text = "";
                    txtAsistenteVinculoEmail.Text = "";
                    txtAsistenteVinculoFechaNac.Text = "";
                    txtAsistenteUsuarioSugerido.Text = $"familiar_{dniVinculo}";
                    txtAsistenteContrasena.Text = "";
                }
                else
                {
                    if (BLLUsuario.CalcularEdad(usuarioExistente.FechaNacimiento ?? DateTime.Now) < 18)
                    {
                        MostrarError(T("alumnos_asistente_error_tutor_menor"));
                        return;
                    }
                    Paso = PasoAsistente.ConfirmarVinculoExistente;
                    lblAsistenteMensaje.Text = string.Format(T("alumnos_asistente_confirmar_vinculo_fmt"),
                        usuarioExistente.Apellido, usuarioExistente.Nombre, dniVinculo);
                }
                MostrarAsistente();
            }
            catch (Exception)
            {
                MostrarError(T("msg_error_generico"));
            }
        }

        protected void btnAsistenteConfirmarVinculoSi_Click(object sender, EventArgs e)
        {
            try
            {
                var usuario = bllUsuario.ObtenerUsuarioPorDni(DniEvaluado.Value);
                if (usuario == null)
                {
                    MostrarError(T("alumnos_msg_no_existe"));
                    return;
                }
                FinalizarVinculo(usuario.USUARIO_Usuario, VinculoParentescoObjetivo);
            }
            catch (Exception ex)
            {
                MostrarError(ex.Message);
            }
        }

        protected void btnAsistenteCrearVinculo_Click(object sender, EventArgs e)
        {
            try
            {
                if (string.IsNullOrEmpty(txtAsistenteVinculoNombre.Text) || string.IsNullOrEmpty(txtAsistenteVinculoApellido.Text))
                {
                    MostrarError(T("alumnos_msg_nombre_oblig"));
                    return;
                }
                if (!DateTime.TryParse(txtAsistenteVinculoFechaNac.Text, out DateTime fechaNac))
                {
                    MostrarError(T("alumnos_msg_fecha_invalida"));
                    return;
                }
                if (BLLUsuario.CalcularEdad(fechaNac) < 18)
                {
                    MostrarError(T("alumnos_asistente_error_tutor_menor"));
                    return;
                }

                string usuarioFamiliar = string.IsNullOrEmpty(txtAsistenteUsuarioSugerido.Text)
                    ? $"familiar_{DniEvaluado}"
                    : txtAsistenteUsuarioSugerido.Text.Trim();
                string contrasena = txtAsistenteContrasena.Text;
                bool autogenerada = string.IsNullOrEmpty(contrasena);
                if (autogenerada) contrasena = bllUsuario.GenerarContrasenaSegura();

                bllUsuario.CrearUsuario(
                    usuarioFamiliar, contrasena, 6, // Rol Familiar
                    txtAsistenteVinculoNombre.Text.Trim(), txtAsistenteVinculoApellido.Text.Trim(),
                    string.IsNullOrEmpty(txtAsistenteVinculoTelefono.Text) ? null : txtAsistenteVinculoTelefono.Text,
                    string.IsNullOrEmpty(txtAsistenteVinculoEmail.Text) ? null : txtAsistenteVinculoEmail.Text,
                    fechaNac, null, DniEvaluado.Value);

                if (autogenerada) RegistrarCredencialGenerada(usuarioFamiliar, contrasena, T("alumnos_rol_familiar"));

                FinalizarVinculo(usuarioFamiliar, VinculoParentescoObjetivo);
            }
            catch (Exception ex)
            {
                MostrarError(ex.Message);
            }
        }

        /// <summary>
        /// Una vez resuelto (creado o encontrado) el usuario a vincular, decide si ya se puede
        /// cerrar (vínculo sobre un alumno existente) o si todavía falta crear/confirmar al
        /// propio alumno (menor con tutor recién resuelto).
        /// </summary>
        private void FinalizarVinculo(string usuario, string parentesco)
        {
            if (VincularAAlumnoExistente)
            {
                bllAlumno.AsociarFamiliar(DniAlumnoPendiente.Value, usuario, parentesco);
                MostrarExito(T("alumnos_msg_familiar_vinculado"));
                CerrarAsistente();
                MostrarCredencialesFinal();
                CargarAlumnos();
                DniSeleccionado = DniAlumnoPendiente;
                return;
            }

            ResolverAlumnoPrincipal(DniAlumnoPendiente.Value, vincularA: usuario, parentesco: parentesco);
        }

        protected void btnAsistenteConfirmarAlumnoSi_Click(object sender, EventArgs e)
        {
            try
            {
                int dni = DniEvaluado.Value;
                decimal? peso = null;
                if (!string.IsNullOrEmpty(txtPeso.Text) && decimal.TryParse(txtPeso.Text, out decimal p))
                    peso = p;

                var alumno = new Alumno(dni, peso, false, chkActivo.Checked, "");
                bllAlumno.CrearAlumno(alumno);

                var usuarioExistente = bllUsuario.ObtenerUsuarioPorDni(dni);
                string usuarioVinculo = VinculoUsuarioObjetivo ?? usuarioExistente.USUARIO_Usuario;
                string parentesco = VinculoUsuarioObjetivo != null ? VinculoParentescoObjetivo : "Titular";
                bllAlumno.AsociarFamiliar(dni, usuarioVinculo, parentesco);

                bllEvento.RegistrarAltaAlumno(ObtenerUsuarioActual(), dni);
                MostrarExito(T("alumnos_msg_creado"));
                CerrarAsistente();
                MostrarCredencialesFinal();
                CargarAlumnos();
                DniSeleccionado = dni;
                pnlForm.Visible = false;
            }
            catch (Exception ex)
            {
                MostrarError(ex.Message);
            }
        }

        protected void btnAsistenteConfirmarAlumnoNo_Click(object sender, EventArgs e)
        {
            CerrarAsistente();
        }

        protected void btnAsistenteCrearAlumno_Click(object sender, EventArgs e)
        {
            try
            {
                int dni = DniEvaluado.Value;
                string usuarioNuevo = string.IsNullOrEmpty(txtAsistenteUsuarioSugerido.Text)
                    ? $"cliente_{dni}"
                    : txtAsistenteUsuarioSugerido.Text.Trim();

                if (bllUsuario.UsuarioExiste(usuarioNuevo))
                {
                    MostrarError(T("alumnos_asistente_error_usuario_existe"));
                    return;
                }

                string contrasena = txtAsistenteContrasena.Text;
                bool autogenerada = string.IsNullOrEmpty(contrasena);
                if (autogenerada) contrasena = bllUsuario.GenerarContrasenaSegura();

                decimal? peso = null;
                if (!string.IsNullOrEmpty(txtPeso.Text) && decimal.TryParse(txtPeso.Text, out decimal p))
                    peso = p;

                bllUsuario.CrearUsuario(
                    usuarioNuevo, contrasena, 4, // Cliente
                    txtNombre.Text.Trim(), txtApellido.Text.Trim(),
                    string.IsNullOrEmpty(txtTelefono.Text) ? null : txtTelefono.Text,
                    null,
                    DateTime.Parse(txtFechaNacimiento.Text),
                    null, dni);

                var alumno = bllAlumno.ObtenerAlumno(dni);
                if (alumno != null)
                {
                    alumno.Peso = peso;
                    alumno.Activo = chkActivo.Checked;
                    bllAlumno.ActualizarAlumno(alumno);
                }

                if (autogenerada) RegistrarCredencialGenerada(usuarioNuevo, contrasena, T("alumnos_rol_titular"));

                if (!string.IsNullOrEmpty(VinculoUsuarioObjetivo))
                {
                    // CrearUsuario ya auto-vinculó al propio titular; si viene de un flujo de
                    // tutor, hay que reemplazar ese vínculo por el del familiar correspondiente.
                    bllAlumno.DesasociarFamiliar(dni, usuarioNuevo);
                    bllAlumno.AsociarFamiliar(dni, VinculoUsuarioObjetivo, VinculoParentescoObjetivo);
                }

                bllEvento.RegistrarAltaAlumno(ObtenerUsuarioActual(), dni);
                MostrarExito(T("alumnos_msg_creado"));
                CerrarAsistente();
                MostrarCredencialesFinal();
                CargarAlumnos();
                DniSeleccionado = dni;
                pnlForm.Visible = false;
            }
            catch (Exception ex)
            {
                MostrarError(ex.Message);
            }
        }

        protected void btnAsistenteCerrar_Click(object sender, EventArgs e)
        {
            CerrarAsistente();
        }

        protected void btnCerrarCredenciales_Click(object sender, EventArgs e)
        {
            pnlCredenciales.Visible = false;
        }

        private void MostrarAsistente()
        {
            pnlAsistente.Visible = true;
            pnlAsistentePreguntarTutor.Visible = Paso == PasoAsistente.PreguntarTutor;
            pnlAsistenteBuscarVinculo.Visible = Paso == PasoAsistente.BuscarUsuarioVinculo;
            pnlAsistenteConfirmarAlumno.Visible = Paso == PasoAsistente.ConfirmarAlumnoExistente;
            pnlAsistenteConfirmarVinculo.Visible = Paso == PasoAsistente.ConfirmarVinculoExistente;
            pnlAsistenteDatosVinculo.Visible = Paso == PasoAsistente.CompletarDatosVinculo;
            pnlAsistenteCredenciales.Visible = Paso == PasoAsistente.CompletarDatosAlumno || Paso == PasoAsistente.CompletarDatosVinculo;
            btnAsistenteCrearAlumno.Visible = Paso == PasoAsistente.CompletarDatosAlumno;
            btnAsistenteCrearVinculo.Visible = Paso == PasoAsistente.CompletarDatosVinculo;

            switch (Paso)
            {
                case PasoAsistente.PreguntarTutor:
                    lblAsistenteTitulo.Text = T("alumnos_asistente_titulo_tutor");
                    lblAsistenteMensaje.Text = T("alumnos_asistente_msg_tutor");
                    break;
                case PasoAsistente.BuscarUsuarioVinculo:
                    lblAsistenteTitulo.Text = VincularAAlumnoExistente
                        ? T("alumnos_asistente_titulo_vincular")
                        : T("alumnos_asistente_titulo_tutor_dni");
                    break;
                case PasoAsistente.ConfirmarAlumnoExistente:
                case PasoAsistente.ConfirmarVinculoExistente:
                    lblAsistenteTitulo.Text = T("alumnos_asistente_titulo_confirmar");
                    break;
                case PasoAsistente.CompletarDatosAlumno:
                    lblAsistenteTitulo.Text = T("alumnos_asistente_titulo_datos");
                    break;
                case PasoAsistente.CompletarDatosVinculo:
                    lblAsistenteTitulo.Text = T("alumnos_asistente_titulo_datos_familiar");
                    break;
            }
        }

        private void CerrarAsistente()
        {
            Paso = PasoAsistente.Ninguno;
            DniEvaluado = null;
            DniAlumnoPendiente = null;
            VinculoUsuarioObjetivo = null;
            VinculoParentescoObjetivo = null;
            VincularAAlumnoExistente = false;
            pnlAsistente.Visible = false;
        }

        private void RegistrarCredencialGenerada(string usuario, string contrasena, string rolLabel)
        {
            var lista = CredencialesGeneradas;
            lista.Add($"{rolLabel}: {usuario} / {contrasena}");
            CredencialesGeneradas = lista;
        }

        private void MostrarCredencialesFinal()
        {
            var lista = CredencialesGeneradas;
            if (lista.Count > 0)
            {
                litCredenciales.Text = string.Join("<br/>", lista.Select(System.Web.HttpUtility.HtmlEncode));
                pnlCredenciales.Visible = true;
                CredencialesGeneradas = new List<string>();
            }
        }

        // ==================== FAMILIARES (modificar alumno existente) ====================

        private void CargarFamiliares(Alumno alumno)
        {
            rptFamiliares.DataSource = alumno.Familiares;
            rptFamiliares.DataBind();
            litFamiliaresVacio.Visible = alumno.Familiares.Count == 0;
        }

        protected void rptFamiliares_ItemCommand(object source, RepeaterCommandEventArgs e)
        {
            try
            {
                if (e.CommandName == "Quitar" && DniSeleccionado.HasValue)
                {
                    string usuario = e.CommandArgument.ToString();
                    bllAlumno.DesasociarFamiliar(DniSeleccionado.Value, usuario);
                    MostrarExito(T("alumnos_msg_familiar_desvinculado"));

                    var alumno = bllAlumno.ObtenerAlumno(DniSeleccionado.Value);
                    if (alumno != null) CargarFamiliares(alumno);
                    CargarAlumnos();
                }
            }
            catch (Exception)
            {
                MostrarError(T("msg_error_generico"));
            }
        }

        // ==================== GUARDAR (solo modificación: peso/activo) ====================

        protected void btnGuardar_Click(object sender, EventArgs e)
        {
            try
            {
                if (!EsModificacion || !DniSeleccionado.HasValue)
                {
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

                var alumno = bllAlumno.ObtenerAlumno(DniSeleccionado.Value);
                if (alumno == null)
                {
                    MostrarError(T("alumnos_msg_no_existe"));
                    return;
                }

                alumno.Peso = peso;
                alumno.Activo = chkActivo.Checked;
                bllAlumno.ActualizarAlumno(alumno);

                bllEvento.RegistrarModificacionAlumno(ObtenerUsuarioActual(), alumno.DNI);
                bllEvento.RegistrarCambioDatosAlumno(ObtenerUsuarioActual(), alumno.DNI, "datos alumno");

                MostrarExito(T("alumnos_msg_modificado"));
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
