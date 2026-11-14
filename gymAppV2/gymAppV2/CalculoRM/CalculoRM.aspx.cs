using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Web.UI.WebControls;
using BLL;
using BE;
using gymAppV2;
using Servicios.Singleton;

namespace gymAppV2.CalculoRM
{
    /// <summary>
    /// Cálculo de la repetición máxima (1RM). Con el peso y las repeticiones de una serie estima
    /// la 1RM (fórmula de Epley), muestra la tabla de cargas por porcentaje y permite guardarla
    /// en el historial del alumno. El personal elige entre todos los alumnos activos; un Cliente
    /// o Familiar, solo entre los suyos (lo controla BLLCalculoRM).
    /// </summary>
    public partial class CalculoRM : BasePage
    {
        private BLLCalculoRM bllCalculoRM;

        private Usuario UsuarioActual
        {
            get { return Singleton.Instancia.Usuario; }
        }

        protected bool EsCliente
        {
            get
            {
                int? rol = UsuarioActual?.USUARIO_Rol;
                return rol == PerfilesSistema.RolCliente || rol == PerfilesSistema.RolFamiliar;
            }
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            VerificarAcceso(PermisosSistema.CalculoRM);

            bllCalculoRM = new BLLCalculoRM();

            if (!IsPostBack)
            {
                AplicarIdioma();
                CargarCombos();
                CargarHistorial();
            }
        }

        public override void OnIdiomaChanged(IdiomaApp idioma)
        {
            base.OnIdiomaChanged(idioma);
            AplicarIdioma();
        }

        private void AplicarIdioma()
        {
            litTitulo.Text         = Texto("rm_titulo", "Cálculo de RM");
            litSubtitulo.Text      = Texto("rm_subtitulo", "Estimá la repetición máxima (1RM) con el peso y las repeticiones de una serie.");
            litSinAlumnos.Text     = Texto("rm_sin_alumnos", "No tenés alumnos asociados a tu cuenta.");
            litCalcularTitulo.Text = Texto("rm_calcular_titulo", "Calcular 1RM");
            litCampoAlumno.Text    = Texto("rm_campo_alumno", "Alumno");
            litCampoEjercicio.Text = Texto("rm_campo_ejercicio", "Ejercicio");
            litCampoPeso.Text      = Texto("rm_campo_peso", "Peso levantado (kg)");
            litCampoReps.Text      = Texto("rm_campo_reps", "Repeticiones");
            litAyudaFormula.Text   = Texto("rm_ayuda_formula",
                "Fórmula de Epley: 1RM = peso × (1 + repeticiones / 30). Es confiable hasta {0} repeticiones.",
                BLLCalculoRM.REPETICIONES_MAXIMAS);
            litBtnCalcular.Text    = Texto("rm_btn_calcular", "Calcular");
            litBtnGuardar.Text     = Texto("rm_btn_guardar", "Guardar en el historial");
            litResultadoLabel.Text = Texto("rm_resultado_label", "1RM estimada");
            litTablaTitulo.Text    = Texto("rm_tabla_titulo", "Cargas de entrenamiento");
            litHistorialTitulo.Text = Texto("rm_historial_titulo", "Historial de 1RM");

            gvHistorial.Columns[0].HeaderText = Texto("rm_col_fecha", "Fecha");
            gvHistorial.Columns[1].HeaderText = Texto("rm_campo_ejercicio", "Ejercicio");
            gvHistorial.Columns[2].HeaderText = Texto("rm_col_serie", "Serie");
            gvHistorial.Columns[3].HeaderText = Texto("rm_col_rm", "1RM");
            gvHistorial.Columns[4].HeaderText = Texto("rm_col_registro", "Registró");
            gvHistorial.EmptyDataText = Texto("rm_sin_historial", "Todavía no hay registros de 1RM para este alumno.");

            string elegir = Texto("rm_elegir", "— Elegí —");
            foreach (DropDownList ddl in new[] { ddlAlumno, ddlEjercicio })
            {
                if (ddl.Items.Count > 0 && ddl.Items[0].Value == string.Empty)
                    ddl.Items[0].Text = elegir;
            }
        }

        // ==================== CARGA ====================

        private void CargarCombos()
        {
            try
            {
                string elegir = Texto("rm_elegir", "— Elegí —");

                List<Alumno> alumnos = bllCalculoRM.ListarAlumnosDisponibles(UsuarioActual);

                if (EsCliente && alumnos.Count == 0)
                {
                    pnlSinAlumnos.Visible = true;
                    pnlContenido.Visible = false;
                    return;
                }

                ddlAlumno.Items.Clear();
                // Un cliente con un solo alumno lo tiene elegido de entrada
                if (!(EsCliente && alumnos.Count == 1))
                {
                    ddlAlumno.Items.Add(new ListItem(elegir, string.Empty));
                }
                foreach (Alumno alumno in alumnos)
                {
                    ddlAlumno.Items.Add(new ListItem($"{alumno.Apellido}, {alumno.Nombre} (DNI: {alumno.DNI})", alumno.DNI.ToString()));
                }

                ddlEjercicio.Items.Clear();
                ddlEjercicio.Items.Add(new ListItem(elegir, string.Empty));
                foreach (Ejercicio ejercicio in bllCalculoRM.ListarEjercicios())
                {
                    string texto = string.IsNullOrEmpty(ejercicio.GrupoMuscular)
                        ? ejercicio.Nombre
                        : $"{ejercicio.Nombre} ({ejercicio.GrupoMuscular})";
                    ddlEjercicio.Items.Add(new ListItem(texto, ejercicio.CodEjercicio.ToString()));
                }
            }
            catch (Exception ex)
            {
                MostrarError(MensajeError(ex));
            }
        }

        private int? DniSeleccionado
        {
            get { return int.TryParse(ddlAlumno.SelectedValue, out int dni) ? dni : (int?)null; }
        }

        private int? EjercicioSeleccionado
        {
            get { return int.TryParse(ddlEjercicio.SelectedValue, out int cod) ? cod : (int?)null; }
        }

        private List<RegistroRM> ObtenerHistorial()
        {
            return DniSeleccionado.HasValue
                ? bllCalculoRM.ListarHistorial(DniSeleccionado.Value, UsuarioActual)
                : new List<RegistroRM>();
        }

        private void CargarHistorial()
        {
            try
            {
                lblHistorialAlumno.Text = DniSeleccionado.HasValue ? Server.HtmlEncode(ddlAlumno.SelectedItem.Text) : string.Empty;
                gvHistorial.DataSource = ObtenerHistorial();
                gvHistorial.DataBind();
            }
            catch (Exception ex)
            {
                gvHistorial.DataSource = new List<RegistroRM>();
                gvHistorial.DataBind();
                MostrarError(MensajeError(ex));
            }
        }

        // ==================== EVENTOS ====================

        protected void ddlAlumno_SelectedIndexChanged(object sender, EventArgs e)
        {
            gvHistorial.PageIndex = 0;
            pnlResultado.Visible = false;
            CargarHistorial();
        }

        protected void gvHistorial_PageIndexChanging(object sender, GridViewPageEventArgs e)
        {
            gvHistorial.PageIndex = e.NewPageIndex;
            CargarHistorial();
        }

        /// <summary>
        /// Calcula sin guardar. Alcanza con el peso y las repeticiones; si además hay alumno y
        /// ejercicio elegidos, compara con su último registro.
        /// </summary>
        protected void btnCalcular_Click(object sender, EventArgs e)
        {
            try
            {
                if (!LeerSerie(out decimal peso, out int repeticiones))
                    return;

                decimal rm = bllCalculoRM.CalcularRM(peso, repeticiones);
                MostrarResultado(rm, peso, repeticiones, UltimoRegistroDelEjercicio());
            }
            catch (Exception ex)
            {
                pnlResultado.Visible = false;
                MostrarError(MensajeError(ex));
            }
        }

        protected void btnGuardar_Click(object sender, EventArgs e)
        {
            try
            {
                if (!DniSeleccionado.HasValue)
                {
                    MostrarError(Texto("rm_err_alumno", "Debe seleccionar un alumno válido."));
                    return;
                }

                if (!EjercicioSeleccionado.HasValue)
                {
                    MostrarError(Texto("rm_err_ejercicio", "Debe seleccionar un ejercicio válido."));
                    return;
                }

                if (!LeerSerie(out decimal peso, out int repeticiones))
                    return;

                // El "anterior" es el último registro antes de guardar este
                RegistroRM anterior = UltimoRegistroDelEjercicio();

                RegistroRM registro = bllCalculoRM.RegistrarRM(
                    DniSeleccionado.Value, EjercicioSeleccionado.Value, peso, repeticiones, UsuarioActual);

                MostrarResultado(registro.PesoRM, peso, repeticiones, anterior);
                MostrarExito(Texto("rm_msg_guardado", "1RM guardada: {0} kg en {1}.",
                    FormatoNumero(registro.PesoRM), registro.EjercicioNombre));

                gvHistorial.PageIndex = 0;
                CargarHistorial();
            }
            catch (Exception ex)
            {
                MostrarError(MensajeError(ex));
            }
        }

        // ==================== AUXILIARES ====================

        /// <summary>
        /// Lee peso y repeticiones del formulario. Acepta coma o punto como separador decimal.
        /// Los rangos los valida BLLCalculoRM; acá solo que sean números.
        /// </summary>
        private bool LeerSerie(out decimal peso, out int repeticiones)
        {
            repeticiones = 0;
            string textoPeso = (txtPeso.Text ?? string.Empty).Trim().Replace(',', '.');

            if (!decimal.TryParse(textoPeso, NumberStyles.Number, CultureInfo.InvariantCulture, out peso))
            {
                MostrarError(Texto("rm_err_peso", "El peso debe ser mayor que 0 y hasta {0} kg.",
                    BLLCalculoRM.PESO_MAXIMO.ToString("0")));
                return false;
            }

            if (!int.TryParse((txtRepeticiones.Text ?? string.Empty).Trim(), out repeticiones))
            {
                MostrarError(Texto("rm_err_reps", "Las repeticiones deben ser un número entero entre 1 y {0}.",
                    BLLCalculoRM.REPETICIONES_MAXIMAS));
                return false;
            }

            return true;
        }

        /// <summary>Último 1RM guardado del alumno y ejercicio elegidos, o null.</summary>
        private RegistroRM UltimoRegistroDelEjercicio()
        {
            if (!DniSeleccionado.HasValue || !EjercicioSeleccionado.HasValue)
                return null;

            return ObtenerHistorial().FirstOrDefault(r => r.CodEjercicio == EjercicioSeleccionado.Value);
        }

        private void MostrarResultado(decimal rm, decimal peso, int repeticiones, RegistroRM anterior)
        {
            lblRM.Text = FormatoKg(rm);

            string serie = $"{FormatoNumero(peso)} kg × {repeticiones}";
            if (EjercicioSeleccionado.HasValue)
                serie += " · " + ddlEjercicio.SelectedItem.Text;
            lblSerie.Text = Server.HtmlEncode(serie);

            if (anterior != null)
            {
                decimal diferencia = rm - anterior.PesoRM;
                string signo = diferencia > 0 ? "+" : string.Empty;
                lblAnterior.Text = Server.HtmlEncode(Texto("rm_anterior", "Último registro: {0} kg el {1} ({2} kg)",
                    FormatoNumero(anterior.PesoRM), anterior.FechaRM.ToString("dd/MM/yyyy"), signo + FormatoNumero(diferencia)));
                lblAnterior.Visible = true;
            }
            else if (DniSeleccionado.HasValue && EjercicioSeleccionado.HasValue)
            {
                lblAnterior.Text = Server.HtmlEncode(Texto("rm_primera", "Todavía no hay registros de este ejercicio."));
                lblAnterior.Visible = true;
            }
            else
            {
                lblAnterior.Visible = false;
            }

            rptCargas.DataSource = bllCalculoRM.CalcularCargas(rm);
            rptCargas.DataBind();
            pnlResultado.Visible = true;
        }

        private string MensajeError(Exception ex)
        {
            return MensajeDePermisos(ex)
                ?? Texto("msg_error_generico", "Ocurrió un error inesperado. Intente nuevamente.");
        }

        // ==================== AYUDAS PARA EL MARKUP ====================

        private static string FormatoNumero(decimal valor)
        {
            return valor.ToString("0.#");
        }

        protected string FormatoKg(object valor)
        {
            return valor == null ? "—" : FormatoNumero(Convert.ToDecimal(valor)) + " kg";
        }

        protected string FormatoSerie(object item)
        {
            var registro = item as RegistroRM;
            if (registro?.PesoLevantado == null || registro.Repeticiones == null)
                return "—";

            return $"{FormatoNumero(registro.PesoLevantado.Value)} kg × {registro.Repeticiones.Value}";
        }
    }
}
