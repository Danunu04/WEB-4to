using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web.UI.WebControls;
using BLL;
using BE;
using gymAppV2;
using Servicios.Singleton;

namespace gymAppV2.Pagos
{
    /// <summary>
    /// Abono mensual. El cliente (alumno o familiar) ve el estado del abono de sus alumnos y lo
    /// paga desde acá (pago simulado). Administración busca alumnos, ve su estado y cobra.
    /// </summary>
    public partial class PagosCliente : BasePage
    {
        private BLLPago bllPago;
        private BLLAlumno bllAlumno;
        private BLLPrecioModalidad bllPrecioModalidad;

        protected bool EsCliente
        {
            get
            {
                int? rol = Singleton.Instancia.Usuario?.USUARIO_Rol;
                return rol == PerfilesSistema.RolCliente || rol == PerfilesSistema.RolFamiliar;
            }
        }

        private string UsuarioActual
        {
            get { return Singleton.Instancia.Usuario?.USUARIO_Usuario; }
        }

        /// <summary>Texto buscado en la vista de administración (se conserva al paginar).</summary>
        private string Busqueda
        {
            get { return ViewState["Busqueda"] as string; }
            set { ViewState["Busqueda"] = value; }
        }

        /// <summary>Transferencia que se está por rechazar (ventana de confirmación).</summary>
        private int? TransferenciaARechazar
        {
            get { return ViewState["TransferenciaARechazar"] as int?; }
            set { ViewState["TransferenciaARechazar"] = value; }
        }

        /// <summary>Alumno cuyo abono está pagando el cliente en la ventana de pago.</summary>
        private int? DniPagoCliente
        {
            get { return ViewState["PagoDni"] as int?; }
            set { ViewState["PagoDni"] = value; }
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            VerificarAcceso(PermisosSistema.Pagos);

            bllPago = new BLLPago();
            bllAlumno = new BLLAlumno();
            bllPrecioModalidad = new BLLPrecioModalidad();

            if (!IsPostBack)
            {
                pnlCliente.Visible = EsCliente;
                pnlAdmin.Visible = !EsCliente;

                AplicarIdioma();
                CargarDatos();

                // Viene del alta de un alumno (Alumnos -> "Cobrar primer mes"): abrir el cobro ya elegido
                if (!EsCliente && int.TryParse(Request.QueryString["cobrar"], out int dniCobro))
                {
                    AbrirFormulario(dniCobro);
                }
            }
        }

        public override void OnIdiomaChanged(IdiomaApp idioma)
        {
            base.OnIdiomaChanged(idioma);
            AplicarIdioma();
            CargarDatos();
        }

        private void AplicarIdioma()
        {
            litTitulo.Text = T("pagos_titulo");
            litSubtitulo.Text = T("pagos_subtitulo");

            // Cliente
            litClienteMsg.Text = Texto("pagos_cliente_estado_msg", "Estado del abono de tus alumnos.");
            litSinAlumnos.Text = Texto("pagos_cliente_sin_alumnos", "No tenés alumnos asociados a tu cuenta.");
            litListaClienteTitulo.Text = T("pagos_lista_cliente_titulo");
            litPagoClienteTitulo.Text = Texto("pagos_btn_pagar", "Pagar abono");
            litClienteCampoAlumno.Text = T("pagos_campo_alumno");
            litClienteCampoModalidad.Text = T("pagos_campo_modalidad");
            litClienteCampoMonto.Text = T("pagos_campo_monto");
            litClienteCampoMetodo.Text = T("pagos_campo_metodo");
            litTarjetaNumero.Text = Texto("pagos_tarjeta_numero", "Número de tarjeta");
            litTarjetaTitular.Text = Texto("pagos_tarjeta_titular", "Titular");
            litTarjetaVence.Text = Texto("pagos_tarjeta_vence", "Vencimiento (MM/AA)");
            litTarjetaCvv.Text = Texto("pagos_tarjeta_cvv", "Código de seguridad");
            litTransferenciaAliasTitulo.Text = Texto("pagos_transferencia_alias", "Alias del gimnasio");
            litTransferenciaAlias.Text = Server.HtmlEncode(new BLLTransferencia().ObtenerAlias());
            litTransferenciaDatos.Text = Texto("pagos_transferencia_info",
                "Transferí el monto a ese alias y después tocá \"Informar transferencia\". Recepción verifica que llegó y recién ahí se activa el abono.");
            litAvisoDemo.Text = Texto("pagos_aviso_demo",
                "Pago de demostración: no se hace ningún cobro real y los datos de la tarjeta no se guardan.");
            litBtnConfirmarPago.Text = Texto("pagos_btn_confirmar", "Confirmar pago");
            rblClienteMetodo.Items.FindByValue("Tarjeta").Text = Texto("pagos_medio_tarjeta", "Tarjeta");
            rblClienteMetodo.Items.FindByValue("Transferencia").Text = Texto("pagos_medio_transferencia", "Transferencia");

            // Administración
            litTransferenciasTitulo.Text = Texto("pagos_transf_titulo", "Transferencias a confirmar");
            litTransferenciasAyuda.Text = Texto("pagos_transf_ayuda",
                "Comprobá en la cuenta del gimnasio que la transferencia llegó antes de confirmarla. Al confirmarla se activa el abono.");
            litAliasTitulo.Text = Texto("pagos_alias_titulo", "Alias que ven los clientes para transferir");
            litBtnGuardarAlias.Text = Texto("pagos_alias_guardar", "Guardar alias");
            litAliasAyuda.Text = Texto("pagos_alias_ayuda", "De 6 a 20 caracteres: letras sin tilde, números, puntos o guiones.");
            litRechazarTitulo.Text = Texto("pagos_transf_rechazar_titulo", "Rechazar transferencia");
            litBtnConfirmarRechazo.Text = Texto("pagos_transf_rechazar_si", "Sí, rechazar");
            TitularColumnas(gvTransferencias, "pagos_transf_col_fecha", "pagos_col_alumno", "pagos_col_dni", "pagos_col_modalidad", "pagos_col_monto", "pagos_transf_col_informo", null);

            litEstadosTitulo.Text = Texto("pagos_estados_titulo", "Estado de abonos");
            txtBuscar.Attributes["placeholder"] = Texto("pagos_buscar_placeholder", "Buscar por nombre, apellido o DNI");
            btnBuscar.Text = Texto("pagos_btn_buscar", "Buscar");
            litListaTitulo.Text = T("pagos_lista_titulo");
            litBtnRegistrar.Text = T("pagos_btn_registrar");
            litFormTitulo.Text = T("pagos_form_titulo");
            litCampoAlumno.Text = T("pagos_campo_alumno");
            litCampoModalidad.Text = T("pagos_campo_modalidad");
            litCampoMetodo.Text = T("pagos_campo_metodo");
            litCampoMonto.Text = T("pagos_campo_monto");
            litBtnGuardar.Text = T("btn_guardar");

            TitularColumnas(gvEstados, "pagos_col_alumno", "pagos_col_dni", "pagos_col_vence", "pagos_col_dias", "pagos_col_estado", null);

            string[] columnasHistorial = { "pagos_col_alumno", "pagos_col_desde", "pagos_col_hasta", "pagos_col_modalidad",
                                           "pagos_col_monto", "pagos_col_fecha_pago", "pagos_col_metodo" };
            TitularColumnas(gvPagos, columnasHistorial);
            TitularColumnas(gvPagosCliente, columnasHistorial);
        }

        /// <summary>Encabezados traducidos de una grilla, en el orden de sus columnas (null deja el actual).</summary>
        private void TitularColumnas(GridView grilla, params string[] claves)
        {
            for (int i = 0; i < claves.Length && i < grilla.Columns.Count; i++)
            {
                if (claves[i] != null)
                {
                    string texto = T(claves[i]);
                    if (texto != claves[i])
                    {
                        grilla.Columns[i].HeaderText = texto;
                    }
                }
            }
        }

        private void CargarDatos()
        {
            if (EsCliente)
            {
                CargarVistaCliente();
            }
            else
            {
                CargarTransferencias();
                CargarEstados();
                CargarPagos();
            }
        }

        // ==================== VISTA CLIENTE ====================

        private void CargarVistaCliente()
        {
            try
            {
                List<EstadoAbono> estados = bllPago.ListarEstadosAbonoCliente(UsuarioActual, DateTime.Now);
                rptAbonosCliente.DataSource = estados;
                rptAbonosCliente.DataBind();
                pnlSinAlumnos.Visible = estados.Count == 0;

                gvPagosCliente.DataSource = bllPago.ListarPagosPorCliente(UsuarioActual) ?? new List<Pago>();
                gvPagosCliente.DataBind();
            }
            catch (Exception ex)
            {
                MostrarError(MensajeError(ex));
            }
        }

        protected void rptAbonosCliente_ItemCommand(object source, RepeaterCommandEventArgs e)
        {
            if (e.CommandName != "Pagar" || !int.TryParse(Convert.ToString(e.CommandArgument), out int dni))
                return;

            try
            {
                EstadoAbono estado = bllPago.ListarEstadosAbonoCliente(UsuarioActual, DateTime.Now)
                    .FirstOrDefault(a => a.Dni == dni);
                if (estado == null)
                {
                    MostrarError(Texto("pagos_err_no_asociado", "Ese alumno no está asociado a tu cuenta."));
                    return;
                }

                DniPagoCliente = dni;
                lblClienteAlumno.Text = Server.HtmlEncode(estado.NombreCompleto);
                CargarModalidades(ddlClienteModalidad, estado.ModalidadId);
                rblClienteMetodo.SelectedValue = "Tarjeta";
                LimpiarTarjeta();
                MostrarMedioCliente();
                ActualizarPreviewCliente();
                pnlPagoCliente.Visible = true;
            }
            catch (Exception ex)
            {
                MostrarError(MensajeError(ex));
            }
        }

        protected void ddlClienteModalidad_SelectedIndexChanged(object sender, EventArgs e)
        {
            ActualizarPreviewCliente();
        }

        protected void btnCerrarPagoCliente_Click(object sender, EventArgs e)
        {
            CerrarPagoCliente();
        }

        protected void btnConfirmarPagoCliente_Click(object sender, EventArgs e)
        {
            try
            {
                if (!DniPagoCliente.HasValue)
                {
                    CerrarPagoCliente();
                    return;
                }

                if (!int.TryParse(ddlClienteModalidad.SelectedValue, out int modalidadId))
                {
                    MostrarError(Texto("pagos_err_modalidad", "Debe seleccionar una modalidad válida."));
                    return;
                }

                string metodo = rblClienteMetodo.SelectedValue;
                if (metodo == "Transferencia")
                {
                    // La transferencia no activa el abono: queda pendiente hasta que recepción la confirma
                    new BLLTransferencia().Informar(UsuarioActual, DniPagoCliente.Value, modalidadId);
                    CerrarPagoCliente();
                    MostrarExito(Texto("pagos_transf_informada",
                        "Recibimos el aviso de tu transferencia. El abono se activa cuando recepción confirme que llegó."));
                    CargarVistaCliente();
                    return;
                }

                if (metodo == "Tarjeta")
                {
                    string errorTarjeta = ValidarTarjeta();
                    if (errorTarjeta != null)
                    {
                        MostrarError(errorTarjeta);
                        return;
                    }
                }

                Pago pago = bllPago.RegistrarPagoCliente(UsuarioActual, DniPagoCliente.Value, modalidadId, metodo);

                CerrarPagoCliente();
                MostrarExito(Texto("pagos_ok", "Pago registrado. El abono cubre del {0} al {1}.",
                    pago.FechaInicio.ToString("dd/MM/yyyy"), pago.FechaVencimiento.ToString("dd/MM/yyyy")));
                CargarVistaCliente();
            }
            catch (Exception ex)
            {
                MostrarError(MensajeError(ex));
            }
        }

        private void CerrarPagoCliente()
        {
            LimpiarTarjeta();
            DniPagoCliente = null;
            pnlPagoCliente.Visible = false;
        }

        /// <summary>
        /// Muestra los datos de la tarjeta o los de la transferencia según el medio elegido. Los dos
        /// paneles se envían siempre: al cambiar de medio, el script de la página los alterna sin
        /// recargar; acá se deja el estado correcto en cada respuesta del servidor.
        /// </summary>
        private void MostrarMedioCliente()
        {
            bool tarjeta = rblClienteMetodo.SelectedValue != "Transferencia";
            MostrarSegun(pnlTarjeta, tarjeta);
            MostrarSegun(pnlTransferencia, !tarjeta);
            litBtnConfirmarPago.Text = tarjeta ? TextoBotonTarjeta : TextoBotonTransferencia;
        }

        private static void MostrarSegun(Panel panel, bool visible)
        {
            if (visible)
                panel.Style.Remove("display");
            else
                panel.Style["display"] = "none";
        }

        protected string TextoBotonTarjeta
        {
            get { return System.Web.HttpUtility.HtmlAttributeEncode(Texto("pagos_btn_confirmar", "Confirmar pago")); }
        }

        protected string TextoBotonTransferencia
        {
            get { return System.Web.HttpUtility.HtmlAttributeEncode(Texto("pagos_btn_informar_transf", "Informar transferencia")); }
        }

        protected override void OnPreRender(EventArgs e)
        {
            base.OnPreRender(e);
            if (pnlPagoCliente.Visible)
                MostrarMedioCliente();
        }

        private void ActualizarPreviewCliente()
        {
            int? modalidadId = int.TryParse(ddlClienteModalidad.SelectedValue, out int id) ? id : (int?)null;
            ActualizarPreview(DniPagoCliente, modalidadId, lblClienteMonto, lblClienteCobertura);
        }

        private void LimpiarTarjeta()
        {
            txtTarjetaNumero.Text = string.Empty;
            txtTarjetaTitular.Text = string.Empty;
            txtTarjetaVence.Text = string.Empty;
            txtTarjetaCvv.Text = string.Empty;
        }

        /// <summary>
        /// Validación de formato de la tarjeta (el pago es simulado: los datos no se guardan ni se
        /// envían a ningún lado). Devuelve el mensaje de error o null si está bien.
        /// </summary>
        private string ValidarTarjeta()
        {
            txtTarjetaNumero.Text = ValidadorTarjeta.Normalizar(txtTarjetaNumero.Text) ?? txtTarjetaNumero.Text;
            txtTarjetaVence.Text = ValidadorTarjeta.NormalizarVencimiento(txtTarjetaVence.Text) ?? txtTarjetaVence.Text;

            string clave = ValidadorTarjeta.Validar(txtTarjetaNumero.Text, txtTarjetaTitular.Text,
                txtTarjetaVence.Text, txtTarjetaCvv.Text, DateTime.Today);

            return clave == null ? null : Texto(clave, ValidadorTarjeta.MensajeRespaldo(clave));
        }

        // ==================== VISTA ADMIN ====================

        protected string TextoBotonConfirmarTransferencia
        {
            get { return Texto("pagos_transf_confirmar", "Confirmar"); }
        }

        protected string TextoBotonRechazarTransferencia
        {
            get { return Texto("pagos_transf_rechazar", "Rechazar"); }
        }

        /// <summary>Transferencias informadas sin confirmar. La sección se muestra siempre, aunque esté vacía.</summary>
        private void CargarTransferencias()
        {
            try
            {
                var bllTransferencia = new BLLTransferencia();
                gvTransferencias.DataSource = bllTransferencia.ListarPendientes();
                gvTransferencias.DataBind();
                txtAlias.Text = bllTransferencia.ObtenerAlias();
                pnlTransferencias.Visible = true;
            }
            catch (Exception)
            {
                // Si la tabla todavía no existe (falta el script), la sección no se muestra
                pnlTransferencias.Visible = false;
            }
        }

        protected void btnGuardarAlias_Click(object sender, EventArgs e)
        {
            try
            {
                new BLLTransferencia().ActualizarAlias(txtAlias.Text);
                MostrarExito(Texto("pagos_alias_ok", "Alias actualizado. Los clientes ya ven el nuevo."));
            }
            catch (Exception ex)
            {
                MostrarError(MensajeError(ex));
            }

            txtAlias.Text = new BLLTransferencia().ObtenerAlias();
        }

        protected void gvTransferencias_RowCommand(object sender, GridViewCommandEventArgs e)
        {
            if (!int.TryParse(Convert.ToString(e.CommandArgument), out int codTransferencia))
                return;

            if (e.CommandName == "Confirmar")
            {
                try
                {
                    Pago pago = new BLLTransferencia().Confirmar(codTransferencia, UsuarioActual);
                    MostrarExito(Texto("pagos_transf_confirmada", "Transferencia confirmada. El abono cubre del {0} al {1}.",
                        pago.FechaInicio.ToString("dd/MM/yyyy"), pago.FechaVencimiento.ToString("dd/MM/yyyy")));
                }
                catch (Exception ex)
                {
                    MostrarError(MensajeError(ex));
                }

                CargarTransferencias();
                CargarEstados();
                CargarPagos();
            }
            else if (e.CommandName == "Rechazar")
            {
                TransferenciaPendiente transferencia = null;
                try
                {
                    transferencia = new BLLTransferencia().ListarPendientes().FirstOrDefault(t => t.CodTransferencia == codTransferencia);
                }
                catch (Exception ex)
                {
                    MostrarError(MensajeError(ex));
                    return;
                }

                if (transferencia == null)
                {
                    CargarTransferencias();
                    return;
                }

                TransferenciaARechazar = codTransferencia;
                litRechazarMensaje.Text = Server.HtmlEncode(Texto("pagos_transf_rechazar_msg",
                    "¿Rechazar la transferencia de {0} por {1}? El abono no se activa y el cliente tiene que volver a pagar.",
                    transferencia.AlumnoNombreCompleto, transferencia.Monto.ToString("C")));
                pnlRechazarTransferencia.Visible = true;
            }
        }

        protected void btnCancelarRechazo_Click(object sender, EventArgs e)
        {
            TransferenciaARechazar = null;
            pnlRechazarTransferencia.Visible = false;
        }

        protected void btnConfirmarRechazo_Click(object sender, EventArgs e)
        {
            pnlRechazarTransferencia.Visible = false;
            if (!TransferenciaARechazar.HasValue)
                return;

            try
            {
                new BLLTransferencia().Rechazar(TransferenciaARechazar.Value, UsuarioActual);
                MostrarExito(Texto("pagos_transf_rechazada", "Transferencia rechazada."));
            }
            catch (Exception ex)
            {
                MostrarError(MensajeError(ex));
            }

            TransferenciaARechazar = null;
            CargarTransferencias();
            CargarEstados();
        }

        private void CargarEstados()
        {
            try
            {
                gvEstados.DataSource = bllPago.ListarEstadosAbono(DateTime.Now, Busqueda);
                gvEstados.DataBind();
            }
            catch (Exception ex)
            {
                MostrarError(MensajeError(ex));
            }
        }

        private void CargarPagos()
        {
            try
            {
                var pagos = bllPago.ListarPagos() ?? new List<Pago>();

                gvPagos.DataSource = pagos;
                gvPagos.DataBind();

                footerText.InnerText = $"Mostrando {pagos.Count} de {pagos.Count}";
            }
            catch (Exception ex)
            {
                MostrarError(MensajeError(ex));
                footerText.InnerText = "Mostrando 0 de 0";
            }
        }

        protected void btnBuscar_Click(object sender, EventArgs e)
        {
            Busqueda = txtBuscar.Text.Trim();
            gvEstados.PageIndex = 0;
            CargarEstados();
        }

        protected void btnLimpiarBusqueda_Click(object sender, EventArgs e)
        {
            txtBuscar.Text = string.Empty;
            Busqueda = null;
            gvEstados.PageIndex = 0;
            CargarEstados();
        }

        protected void gvEstados_PageIndexChanging(object sender, GridViewPageEventArgs e)
        {
            gvEstados.PageIndex = e.NewPageIndex;
            CargarEstados();
        }

        protected void gvEstados_RowCommand(object sender, GridViewCommandEventArgs e)
        {
            if (e.CommandName == "Cobrar" && int.TryParse(Convert.ToString(e.CommandArgument), out int dni))
            {
                AbrirFormulario(dni);
            }
        }

        protected void gvPagos_PageIndexChanging(object sender, GridViewPageEventArgs e)
        {
            gvPagos.PageIndex = e.NewPageIndex;
            CargarPagos();
        }

        protected void btnRegistrar_Click(object sender, EventArgs e)
        {
            AbrirFormulario(null);
        }

        /// <summary>Abre el formulario de cobro, con el alumno ya elegido si viene de "Cobrar".</summary>
        private void AbrirFormulario(int? dni)
        {
            try
            {
                ddlAlumno.Items.Clear();
                ddlAlumno.Items.Add(new ListItem("—", string.Empty));
                foreach (var alumno in (bllAlumno.ListarAlumnos() ?? new List<Alumno>()).Where(a => a.Activo))
                {
                    ddlAlumno.Items.Add(new ListItem($"{alumno.Apellido}, {alumno.Nombre} (DNI: {alumno.DNI})", alumno.DNI.ToString()));
                }

                int? modalidadPropuesta = null;
                if (dni.HasValue && ddlAlumno.Items.FindByValue(dni.Value.ToString()) != null)
                {
                    ddlAlumno.SelectedValue = dni.Value.ToString();
                    modalidadPropuesta = bllPago.ObtenerEstadoAbono(dni.Value, DateTime.Now)?.ModalidadId;
                }

                CargarModalidades(ddlModalidad, modalidadPropuesta);
                ddlMetodoPago.SelectedIndex = 0;
                ActualizarPreviewAdmin();
                pnlForm.Visible = true;
            }
            catch (Exception ex)
            {
                MostrarError(MensajeError(ex));
            }
        }

        protected void btnCloseForm_Click(object sender, EventArgs e)
        {
            pnlForm.Visible = false;
        }

        protected void ddlAlumno_SelectedIndexChanged(object sender, EventArgs e)
        {
            ActualizarPreviewAdmin();
        }

        protected void ddlModalidad_SelectedIndexChanged(object sender, EventArgs e)
        {
            ActualizarPreviewAdmin();
        }

        private void ActualizarPreviewAdmin()
        {
            int? dni = int.TryParse(ddlAlumno.SelectedValue, out int d) ? d : (int?)null;
            int? modalidadId = int.TryParse(ddlModalidad.SelectedValue, out int m) ? m : (int?)null;
            ActualizarPreview(dni, modalidadId, lblMontoPreview, lblCobertura);
        }

        protected void btnGuardar_Click(object sender, EventArgs e)
        {
            try
            {
                if (!int.TryParse(ddlAlumno.SelectedValue, out int dni))
                {
                    MostrarError(Texto("pagos_err_alumno", "Debe seleccionar un alumno válido."));
                    return;
                }

                if (!int.TryParse(ddlModalidad.SelectedValue, out int modalidadId))
                {
                    MostrarError(Texto("pagos_err_modalidad", "Debe seleccionar una modalidad válida."));
                    return;
                }

                Pago pago = bllPago.RegistrarPago(dni, modalidadId, ddlMetodoPago.SelectedValue, UsuarioActual);

                pnlForm.Visible = false;
                MostrarExito(Texto("pagos_ok", "Pago registrado. El abono cubre del {0} al {1}.",
                    pago.FechaInicio.ToString("dd/MM/yyyy"), pago.FechaVencimiento.ToString("dd/MM/yyyy")));
                CargarEstados();
                CargarPagos();
            }
            catch (Exception ex)
            {
                MostrarError(MensajeError(ex));
            }
        }

        // ==================== COMPARTIDO ====================

        private void CargarModalidades(DropDownList lista, int? seleccionada)
        {
            lista.Items.Clear();
            foreach (var modalidad in (bllPrecioModalidad.ListarModalidades() ?? new List<PrecioModalidad>()).Where(m => m.Activo))
            {
                lista.Items.Add(new ListItem($"{modalidad.ObtenerDescripcion()} — {modalidad.Precio:C}", modalidad.Id.ToString()));
            }

            if (seleccionada.HasValue && lista.Items.FindByValue(seleccionada.Value.ToString()) != null)
            {
                lista.SelectedValue = seleccionada.Value.ToString();
            }
        }

        /// <summary>
        /// Muestra el monto y las fechas que cubriría el abono. Si no se puede pagar
        /// (ej. ya tiene uno adelantado), muestra el motivo en lugar de las fechas.
        /// </summary>
        private void ActualizarPreview(int? dni, int? modalidadId, Label lblMonto, Label lblCoberturaPreview)
        {
            lblMonto.Text = "—";
            lblCoberturaPreview.Text = string.Empty;
            lblCoberturaPreview.CssClass = "abono-cobertura";

            if (!dni.HasValue || !modalidadId.HasValue)
                return;

            try
            {
                Pago abono = bllPago.PrevisualizarAbono(dni.Value, modalidadId.Value, DateTime.Now);
                lblMonto.Text = abono.Monto.ToString("C");
                lblCoberturaPreview.Text = Texto("pagos_cobertura", "El abono cubre del {0} al {1}.",
                    abono.FechaInicio.ToString("dd/MM/yyyy"), abono.FechaVencimiento.ToString("dd/MM/yyyy"));
            }
            catch (Exception ex)
            {
                lblCoberturaPreview.Text = Server.HtmlEncode(MensajeError(ex));
                lblCoberturaPreview.CssClass = "abono-cobertura abono-cobertura-error";
            }
        }

        private string MensajeError(Exception ex)
        {
            return MensajeDePermisos(ex)
                ?? Texto("msg_error_generico", "Ocurrió un error inesperado. Intente nuevamente.");
        }

        // ==================== AYUDAS PARA EL MARKUP ====================

        protected string BadgeEstado(object item)
        {
            var estado = (EstadoAbono)item;

            string clase, texto;
            if (!estado.AlumnoActivo)
            {
                clase = "estado-inactivo";
                texto = Texto("pagos_estado_inactivo", "Dado de baja");
            }
            else if (estado.TieneTransferenciaPendiente && !estado.EstaAlDia)
            {
                clase = "estado-transferencia";
                texto = Texto("pagos_estado_transf_pendiente", "Transferencia a confirmar");
            }
            else
            {
                switch (estado.Situacion)
                {
                    case SituacionAbono.Vigente:
                        clase = "estado-vigente";
                        texto = Texto("pagos_estado_vigente", "Al día");
                        break;
                    case SituacionAbono.PorVencer:
                        clase = "estado-por-vencer";
                        texto = Texto("pagos_estado_por_vencer", "Por vencer");
                        break;
                    case SituacionAbono.Vencido:
                        clase = "estado-vencido";
                        texto = Texto("pagos_estado_vencido", "Vencido");
                        break;
                    default:
                        clase = "estado-sin-abono";
                        texto = Texto("pagos_estado_sin_abono", "Sin abono");
                        break;
                }
            }

            return $"<span class=\"estado-badge {clase}\">{Server.HtmlEncode(texto)}</span>";
        }

        protected string FormatearFecha(object fecha)
        {
            return fecha is DateTime d ? d.ToString("dd/MM/yyyy") : "—";
        }

        protected string FormatearMonto(object monto)
        {
            return monto is decimal m ? m.ToString("C") : "—";
        }

        protected bool PuedePagar(object item)
        {
            var estado = (EstadoAbono)item;
            return estado.AlumnoActivo && !estado.TieneAbonoAdelantado && !estado.TieneTransferenciaPendiente;
        }

        protected bool MostrarMontoAPagar(object item)
        {
            var estado = (EstadoAbono)item;
            return estado.MontoARenovar.HasValue && PuedePagar(item) && estado.Situacion != SituacionAbono.Vigente;
        }

        protected string AvisoNoPuedePagar(object item)
        {
            var estado = (EstadoAbono)item;
            string aviso = null;

            if (!estado.AlumnoActivo)
                aviso = Texto("pagos_aviso_inactivo", "El alumno está dado de baja. Consultá en recepción.");
            else if (estado.TieneAbonoAdelantado)
                aviso = Texto("pagos_aviso_adelantado", "El próximo abono ya está pago.");
            else if (estado.TieneTransferenciaPendiente)
                aviso = Texto("pagos_aviso_transf_pendiente", "Transferencia informada: el abono se activa cuando recepción la confirme.");

            return aviso == null ? string.Empty : $"<p class=\"abono-card-aviso\">{Server.HtmlEncode(aviso)}</p>";
        }
    }
}
