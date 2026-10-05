using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Web;
using System.Web.UI.WebControls;
using BLL;
using BE;
using gymAppV2;
using Newtonsoft.Json;
using Servicios.Singleton;

namespace gymAppV2.Actividades
{
    public partial class actividades : BasePage
    {
        private BLLActividad bllActividad;
        private BLLInscripcion bllInscripcion;

        private static readonly string[] Colores = { "pink", "mint", "lavender", "peach", "sky" };

        // Claves de traducción de los días (1 = Lunes ... 7 = Domingo)
        private static readonly string[] ClavesDias =
        {
            "actividades_dia_lun", "actividades_dia_mar", "actividades_dia_mie", "actividades_dia_jue",
            "actividades_dia_vie", "actividades_dia_sab", "actividades_dia_dom"
        };

        private const string FormatoFecha = "yyyy-MM-dd";

        /// <summary>
        /// Cliente o Familiar: se anota (o anota a sus alumnos) a las clases.
        /// </summary>
        protected bool EsCliente
        {
            get { return Singleton.Instancia.Usuario?.USUARIO_Rol == 4 || Singleton.Instancia.Usuario?.USUARIO_Rol == 6; }
        }

        private bool PuedeGestionar
        {
            get { return BllRol.UsuarioActualTieneAcceso(PermisosSistema.GestionActividades); }
        }

        private string UsuarioActual
        {
            get { return Singleton.Instancia.Usuario?.USUARIO_Usuario; }
        }

        // Actividad en edición (0 = alta).
        private int CodActividadEdicion
        {
            get { return ViewState["CodActividadEdicion"] as int? ?? 0; }
            set { ViewState["CodActividadEdicion"] = value; }
        }

        // Turno cuyas inscripciones fijas se están editando (ventana "Alumnos").
        private int CodHorarioInscripcion
        {
            get { return ViewState["CodHorarioInscripcion"] as int? ?? 0; }
            set { ViewState["CodHorarioInscripcion"] = value; }
        }

        /// <summary>Alumno (DNI) para el que el Cliente/Familiar está viendo sus inscripciones.</summary>
        private int DniMisActividades
        {
            get { return ViewState["DniMisActividades"] as int? ?? 0; }
            set { ViewState["DniMisActividades"] = value; }
        }

        private int CodActividadBaja
        {
            get { return ViewState["CodActividadBaja"] as int? ?? 0; }
            set { ViewState["CodActividadBaja"] = value; }
        }

        // Clase abierta en la ventana de clase (turno + fecha).
        private int ClaseCodHorario
        {
            get { return ViewState["ClaseCodHorario"] as int? ?? 0; }
            set { ViewState["ClaseCodHorario"] = value; }
        }

        private DateTime ClaseFecha
        {
            get { return ViewState["ClaseFecha"] as DateTime? ?? DateTime.MinValue; }
            set { ViewState["ClaseFecha"] = value; }
        }

        /// <summary>Fila del editor de horarios (valores tal como se muestran en el formulario).</summary>
        public class FilaHorario
        {
            public int CodHorario { get; set; }
            public int Dia { get; set; }
            public string Hora { get; set; }
            public string Duracion { get; set; }
            public int CodAula { get; set; }
            public int DniTitular { get; set; }
            public int? DniAuxiliar { get; set; }
        }

        // Opciones de los combos del editor de horarios (se cargan una vez por request)
        private List<Aula> aulasFormulario;
        private List<Entrenador> profesoresFormulario;

        /// <summary>Traducción escapada para usarla dentro de un string de JavaScript.</summary>
        protected string JsT(string clave)
        {
            return HttpUtility.JavaScriptStringEncode(T(clave));
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            VerificarAcceso(PermisosSistema.ActividadesCalendario);

            bllActividad = new BLLActividad();
            bllInscripcion = new BLLInscripcion();

            btnNuevaActividad.Visible = PuedeGestionar;
            pnlGestion.Visible = PuedeGestionar;
            pnlMisActividades.Visible = EsCliente;

            if (!IsPostBack)
            {
                AplicarIdioma();
                hdnEsCliente.Value = EsCliente ? "1" : "0";
                hdnPuedeGestionar.Value = PuedeGestionar ? "1" : "0";
                CargarDatos();
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
            litTitulo.Text      = T("actividades_titulo");
            litBtnNueva.Text    = T("actividades_btn_nueva");
            litClienteInfo.Text = T("actividades_cliente_info");
            litGestionTitulo.Text = T("actividades_gestion_titulo");
            btnAgregarHorario.Text = "+ " + T("actividades_btn_agregar_horario");
            btnCancelarForm.Text = T("actividades_btn_cancelar");
            btnGuardarActividad.Text = T("btn_guardar");
            btnCancelarInscripciones.Text = T("actividades_btn_cancelar");
            btnGuardarInscripciones.Text = T("btn_guardar");
            btnCancelarBaja.Text = T("actividades_btn_cancelar");
            btnConfirmarBaja.Text = T("actividades_btn_desactivar");
            litMisActividadesTitulo.Text = T("actividades_mis_titulo");
            litMisActividadesAyuda.Text = T("actividades_mis_ayuda");
            litInscripcionesAyuda.Text = T("actividades_inscripciones_ayuda");
            litSinAlumnos.Text = "<span class=\"texto-vacio\">" + Server.HtmlEncode(T("actividades_sin_alumnos")) + "</span>";

            btnClaseAnotar.Text = T("actividades_btn_anotarme");
            btnClaseDesanotar.Text = T("actividades_btn_desanotarme");
            btnClaseAgregar.Text = T("actividades_btn_anotar_alumno");
            btnCerrarClase.Text = T("actividades_btn_cerrar");
            btnAnotarSeleccion.Text = T("actividades_btn_anotar_seleccion");
            btnBajaSeleccion.Text = T("actividades_btn_baja_seleccion");
            litClasePasada.Text = T("actividades_clase_pasada");

            LlenarAlcancesAlta(rblAlcanceAlta);
            LlenarAlcancesAlta(rblAlcanceAltaGestor);
            LlenarAlcancesBaja(rblAlcanceBaja);
        }

        private void LlenarAlcancesAlta(RadioButtonList lista)
        {
            string seleccionado = lista.SelectedValue;
            lista.Items.Clear();
            lista.Items.Add(new ListItem(T("actividades_alcance_una"), ((int)AlcanceInscripcion.SoloEstaClase).ToString()));
            lista.Items.Add(new ListItem(T("actividades_alcance_mes"), ((int)AlcanceInscripcion.RestoDelMes).ToString()));
            lista.Items.Add(new ListItem(T("actividades_alcance_4semanas"), ((int)AlcanceInscripcion.ProximasCuatroSemanas).ToString()));
            lista.Items.Add(new ListItem(T("actividades_alcance_todas"), ((int)AlcanceInscripcion.Todas).ToString()));
            lista.SelectedValue = string.IsNullOrEmpty(seleccionado) ? ((int)AlcanceInscripcion.SoloEstaClase).ToString() : seleccionado;
        }

        private void LlenarAlcancesBaja(RadioButtonList lista)
        {
            string seleccionado = lista.SelectedValue;
            lista.Items.Clear();
            lista.Items.Add(new ListItem(T("actividades_baja_una"), ((int)AlcanceBaja.SoloEstaClase).ToString()));
            lista.Items.Add(new ListItem(T("actividades_baja_desde"), ((int)AlcanceBaja.DesdeEstaClase).ToString()));
            lista.SelectedValue = string.IsNullOrEmpty(seleccionado) ? ((int)AlcanceBaja.SoloEstaClase).ToString() : seleccionado;
        }

        private string DescribirTurno(ActividadHorario horario)
        {
            return $"{T(ClavesDias[horario.DiaSemana - 1])} {horario.HoraInicio:hh\\:mm}–{horario.HoraFin:hh\\:mm}";
        }

        // ==================== CARGA ====================

        private void CargarDatos()
        {
            // Primero "Mis actividades": define el alumno cuyo estado se pinta en el calendario
            if (EsCliente)
            {
                CargarMisActividades();
            }
            CargarCalendario();
            if (PuedeGestionar)
            {
                CargarListaGestion();
            }
        }

        /// <summary>
        /// Serializa los horarios al front-end agrupados por día de la semana (1 = Lunes ... 7 = Domingo).
        /// El calendario repite cada horario en todas las fechas de ese día. Para Cliente/Familiar
        /// también se envían las inscripciones del alumno elegido, para marcar a qué clases va.
        /// </summary>
        private void CargarCalendario()
        {
            hdnHoy.Value = DateTime.Today.ToString(FormatoFecha, CultureInfo.InvariantCulture);

            try
            {
                var porDia = bllActividad.ListarHorariosActivos()
                    .GroupBy(h => h.DiaSemana)
                    .ToDictionary(
                        g => g.Key,
                        g => g.Select(h => new
                        {
                            h = h.CodHorario,
                            name = h.DescripcionActividad,
                            time = h.HoraInicio.ToString(@"hh\:mm"),
                            end = h.HoraFin.ToString(@"hh\:mm"),
                            color = Colores[h.CodActividad % Colores.Length],
                            cupo = h.Cupo,
                            aula = h.NombreAula,
                            instructor = string.IsNullOrEmpty(h.Instructores) ? T("actividades_sin_instructor") : h.Instructores
                        }).ToList());

                hdnActividadesJson.Value = JsonConvert.SerializeObject(porDia);
                hdnOcupacionJson.Value = SerializarOcupacion(bllInscripcion.ObtenerOcupacionDesde(DateTime.Today));
                hdnAgendaJson.Value = EsCliente && DniMisActividades != 0
                    ? SerializarAgenda(bllInscripcion.ObtenerAgenda(DniMisActividades))
                    : "{}";
            }
            catch (Exception)
            {
                hdnActividadesJson.Value = "{}";
                hdnAgendaJson.Value = "{}";
                hdnOcupacionJson.Value = "{}";
                MostrarError(T("msg_error_generico"));
            }
        }

        /// <summary>
        /// Ocupación por turno para que el calendario sepa qué clases están completas, sin datos de los alumnos:
        /// f = períodos de inscripciones fijas [desde, hasta], a/b = cantidad de altas/bajas puntuales por fecha.
        /// </summary>
        private static string SerializarOcupacion(Dictionary<int, BLLInscripcion.OcupacionTurno> ocupacion)
        {
            Func<DateTime, string> iso = f => f.ToString(FormatoFecha, CultureInfo.InvariantCulture);

            return JsonConvert.SerializeObject(ocupacion.ToDictionary(
                par => par.Key.ToString(CultureInfo.InvariantCulture),
                par => new
                {
                    f = par.Value.Fijas.Select(p => new[] { iso(p.Key), p.Value.HasValue ? iso(p.Value.Value) : null }),
                    a = par.Value.Altas.ToDictionary(p => iso(p.Key), p => p.Value),
                    b = par.Value.Bajas.ToDictionary(p => iso(p.Key), p => p.Value)
                }));
        }

        private static string SerializarAgenda(BLLInscripcion.Agenda agenda)
        {
            Func<DateTime, string> iso = f => f.ToString(FormatoFecha, CultureInfo.InvariantCulture);

            return JsonConvert.SerializeObject(new
            {
                fijas = agenda.Fijas.Select(f => new
                {
                    h = f.CodHorario,
                    d = iso(f.FechaDesde),
                    u = f.FechaHasta.HasValue ? iso(f.FechaHasta.Value) : null
                }),
                altas = agenda.Clases.Where(c => c.Tipo == InscripcionClase.TipoAlta).Select(c => c.CodHorario + "|" + iso(c.Fecha)),
                bajas = agenda.Clases.Where(c => c.Tipo == InscripcionClase.TipoBaja).Select(c => c.CodHorario + "|" + iso(c.Fecha))
            });
        }

        private void CargarListaGestion()
        {
            try
            {
                List<Actividad> actividades = bllActividad.ListarTodasLasActividades();
                Dictionary<int, List<ActividadHorario>> horariosPorActividad = new Dictionary<int, List<ActividadHorario>>();

                foreach (Actividad actividad in actividades)
                {
                    horariosPorActividad[actividad.CodActividad] = bllActividad.ObtenerActividad(actividad.CodActividad)?.Horarios
                        ?? new List<ActividadHorario>();
                }

                rptActividades.DataSource = actividades.Select(a => new
                {
                    a.CodActividad,
                    a.Descripcion,
                    a.Activo,
                    Instructores = string.IsNullOrEmpty(a.Instructores) ? "—" : a.Instructores,
                    Precio = a.PrecioAlumno.ToString("C0", CultureInfo.CurrentCulture),
                    Cupo = a.CupoMaximo.HasValue ? a.CupoMaximo.Value.ToString(CultureInfo.CurrentCulture) : T("actividades_cupo_segun_aula"),
                    EstadoTexto = a.Activo ? T("actividades_estado_activa") : T("actividades_estado_inactiva"),
                    ResumenHorarios = ResumirHorarios(horariosPorActividad[a.CodActividad])
                }).ToList();
                rptActividades.DataBind();

                phSinActividades.Visible = actividades.Count == 0;
            }
            catch (Exception)
            {
                MostrarError(T("msg_error_generico"));
            }
        }

        private string ResumirHorarios(List<ActividadHorario> horarios)
        {
            if (horarios.Count == 0)
                return "—";

            return string.Join(", ", horarios.Select(h => $"{T(ClavesDias[h.DiaSemana - 1])} {h.HoraInicio:hh\\:mm}"));
        }

        // ==================== ALTA / MODIFICACIÓN ====================

        protected void btnNuevaActividad_Click(object sender, EventArgs e)
        {
            if (!PuedeGestionar) return;

            CodActividadEdicion = 0;
            litModalTitulo.Text = T("actividades_form_nueva");
            txtDescripcion.Text = string.Empty;
            txtCostoInterno.Text = "0";
            txtPrecioAlumno.Text = "0";
            txtCupo.Text = string.Empty;
            chkActiva.Checked = true;

            BindHorarios(new List<FilaHorario> { new FilaHorario { Dia = 1, Hora = "08:00", Duracion = "60" } });

            pnlForm.Visible = true;
        }

        private void AbrirEdicion(int codActividad)
        {
            Actividad actividad = bllActividad.ObtenerActividad(codActividad);
            if (actividad == null)
            {
                MostrarError(T("msg_error_generico"));
                return;
            }

            CodActividadEdicion = codActividad;
            litModalTitulo.Text = T("actividades_form_editar");
            txtDescripcion.Text = actividad.Descripcion;
            txtCostoInterno.Text = actividad.CostoInterno.ToString("0.##", CultureInfo.InvariantCulture);
            txtPrecioAlumno.Text = actividad.PrecioAlumno.ToString("0.##", CultureInfo.InvariantCulture);
            txtCupo.Text = actividad.CupoMaximo.HasValue ? actividad.CupoMaximo.Value.ToString(CultureInfo.InvariantCulture) : string.Empty;
            chkActiva.Checked = actividad.Activo;

            BindHorarios(actividad.Horarios.Select(h => new FilaHorario
            {
                CodHorario = h.CodHorario,
                Dia = h.DiaSemana,
                Hora = h.HoraInicio.ToString(@"hh\:mm"),
                Duracion = h.DuracionMin.ToString(),
                CodAula = h.CodAula,
                DniTitular = h.DniTitular,
                DniAuxiliar = h.DniAuxiliar
            }).ToList());

            pnlForm.Visible = true;
        }

        protected void btnAgregarHorario_Click(object sender, EventArgs e)
        {
            List<FilaHorario> filas = LeerFilasHorario();
            filas.Add(new FilaHorario { Dia = 1, Hora = "08:00", Duracion = "60" });
            BindHorarios(filas);
        }

        protected void rptHorarios_ItemCommand(object source, RepeaterCommandEventArgs e)
        {
            if (e.CommandName != "Quitar") return;

            List<FilaHorario> filas = LeerFilasHorario();
            int indice = Convert.ToInt32(e.CommandArgument);
            if (indice >= 0 && indice < filas.Count)
            {
                filas.RemoveAt(indice);
            }
            BindHorarios(filas);
        }

        protected void rptHorarios_ItemDataBound(object sender, RepeaterItemEventArgs e)
        {
            if (e.Item.ItemType != ListItemType.Item && e.Item.ItemType != ListItemType.AlternatingItem) return;

            var ddlDia = (DropDownList)e.Item.FindControl("ddlDia");
            for (int dia = 1; dia <= 7; dia++)
            {
                ddlDia.Items.Add(new ListItem(T(ClavesDias[dia - 1]), dia.ToString()));
            }
            var fila = (FilaHorario)e.Item.DataItem;
            ddlDia.SelectedValue = fila.Dia.ToString();

            // Aulas activas (más la actual si se desactivó, para que se vea qué tenía)
            if (aulasFormulario == null)
                aulasFormulario = new BLLAula().ListarAulas();
            var ddlAula = (DropDownList)e.Item.FindControl("ddlAula");
            ddlAula.Items.Add(new ListItem(T("actividades_elegir"), "0"));
            foreach (Aula aula in aulasFormulario.Where(a => a.Activo || a.CodAula == fila.CodAula))
                ddlAula.Items.Add(new ListItem($"{aula.Nombre} ({aula.Cupo})", aula.CodAula.ToString()));
            SeleccionarSiExiste(ddlAula, fila.CodAula.ToString());

            // Profesores activos (más los asignados, aunque estén inactivos)
            if (profesoresFormulario == null)
                profesoresFormulario = new BLLEntrenador().ListarEntrenadores()
                    .OrderBy(p => p.Apellido).ThenBy(p => p.Nombre).ToList();
            var ddlTitular = (DropDownList)e.Item.FindControl("ddlTitular");
            var ddlAuxiliar = (DropDownList)e.Item.FindControl("ddlAuxiliar");
            ddlTitular.Items.Add(new ListItem(T("actividades_elegir"), "0"));
            ddlAuxiliar.Items.Add(new ListItem(T("actividades_sin_auxiliar"), "0"));
            foreach (Entrenador profesor in profesoresFormulario.Where(p => p.Activo || p.DNI == fila.DniTitular || p.DNI == fila.DniAuxiliar))
            {
                string nombre = $"{profesor.Apellido}, {profesor.Nombre}";
                ddlTitular.Items.Add(new ListItem(nombre, profesor.DNI.ToString()));
                ddlAuxiliar.Items.Add(new ListItem(nombre, profesor.DNI.ToString()));
            }
            SeleccionarSiExiste(ddlTitular, fila.DniTitular.ToString());
            SeleccionarSiExiste(ddlAuxiliar, (fila.DniAuxiliar ?? 0).ToString());
        }

        private static void SeleccionarSiExiste(DropDownList lista, string valor)
        {
            if (lista.Items.FindByValue(valor) != null)
                lista.SelectedValue = valor;
        }

        private static int ValorCombo(RepeaterItem item, string id)
        {
            return int.TryParse(((DropDownList)item.FindControl(id)).SelectedValue, out int valor) ? valor : 0;
        }

        private void BindHorarios(List<FilaHorario> filas)
        {
            rptHorarios.DataSource = filas;
            rptHorarios.DataBind();
        }

        /// <summary>
        /// Lee los valores actuales del editor de horarios (lo que el usuario escribió).
        /// </summary>
        private List<FilaHorario> LeerFilasHorario()
        {
            var filas = new List<FilaHorario>();
            foreach (RepeaterItem item in rptHorarios.Items)
            {
                filas.Add(new FilaHorario
                {
                    CodHorario = int.TryParse(((HiddenField)item.FindControl("hdnCodHorario")).Value, out int cod) ? cod : 0,
                    Dia = int.TryParse(((DropDownList)item.FindControl("ddlDia")).SelectedValue, out int dia) ? dia : 1,
                    Hora = ((TextBox)item.FindControl("txtHora")).Text,
                    Duracion = ((TextBox)item.FindControl("txtDuracion")).Text,
                    CodAula = ValorCombo(item, "ddlAula"),
                    DniTitular = ValorCombo(item, "ddlTitular"),
                    DniAuxiliar = ValorCombo(item, "ddlAuxiliar") > 0 ? ValorCombo(item, "ddlAuxiliar") : (int?)null
                });
            }
            return filas;
        }

        protected void btnGuardarActividad_Click(object sender, EventArgs e)
        {
            if (!PuedeGestionar) return;

            try
            {
                if (!TryParseImporte(txtCostoInterno.Text, out decimal costo) || !TryParseImporte(txtPrecioAlumno.Text, out decimal precio))
                {
                    MostrarError(T("actividades_msg_importe_invalido"));
                    return;
                }

                // Tope de cupo opcional: vacío = se usa el cupo del aula
                int? cupoMaximo = null;
                string textoCupo = (txtCupo.Text ?? string.Empty).Trim();
                if (textoCupo.Length > 0)
                {
                    if (!int.TryParse(textoCupo, NumberStyles.Integer, CultureInfo.InvariantCulture, out int cupo))
                    {
                        MostrarError(T("actividades_msg_cupo_invalido"));
                        return;
                    }
                    cupoMaximo = cupo;
                }

                var horarios = new List<ActividadHorario>();
                foreach (FilaHorario fila in LeerFilasHorario())
                {
                    if (!TimeSpan.TryParse(fila.Hora, CultureInfo.InvariantCulture, out TimeSpan hora) || !int.TryParse(fila.Duracion, out int duracion))
                    {
                        MostrarError(T("actividades_msg_horario_invalido"));
                        return;
                    }
                    horarios.Add(new ActividadHorario
                    {
                        CodHorario = fila.CodHorario,
                        DiaSemana = fila.Dia,
                        HoraInicio = hora,
                        DuracionMin = duracion,
                        CodAula = fila.CodAula,
                        DniTitular = fila.DniTitular,
                        DniAuxiliar = fila.DniAuxiliar
                    });
                }

                var actividad = new Actividad
                {
                    CodActividad = CodActividadEdicion,
                    Descripcion = txtDescripcion.Text,
                    CostoInterno = costo,
                    PrecioAlumno = precio,
                    CupoMaximo = cupoMaximo,
                    Activo = chkActiva.Checked
                };

                // Validación primero, para mostrar el mensaje puntual al usuario
                try
                {
                    bllActividad.ValidarActividad(actividad, horarios);
                }
                catch (Exception ex)
                {
                    MostrarError(ex.Message);
                    return;
                }

                bllActividad.GuardarActividad(actividad, horarios);

                pnlForm.Visible = false;
                MostrarExito(T("actividades_msg_guardada"));
                CargarDatos();
            }
            catch (Exception)
            {
                MostrarError(T("msg_error_generico"));
            }
        }

        private static bool TryParseImporte(string texto, out decimal valor)
        {
            // input type="number" siempre envía punto decimal
            return decimal.TryParse((texto ?? string.Empty).Trim(), NumberStyles.Number, CultureInfo.InvariantCulture, out valor);
        }

        protected void btnCerrarForm_Click(object sender, EventArgs e)
        {
            pnlForm.Visible = false;
        }

        // ==================== ACCIONES DE LA LISTA ====================

        protected void rptActividades_ItemCommand(object source, RepeaterCommandEventArgs e)
        {
            if (!PuedeGestionar) return;

            try
            {
                int codActividad = Convert.ToInt32(e.CommandArgument);

                switch (e.CommandName)
                {
                    case "Editar":
                        AbrirEdicion(codActividad);
                        break;

                    case "Alumnos":
                        AbrirInscripciones(codActividad);
                        break;

                    case "Desactivar":
                        Actividad actividad = bllActividad.ObtenerActividad(codActividad);
                        if (actividad == null) return;
                        CodActividadBaja = codActividad;
                        litConfirmarBaja.Text = Server.HtmlEncode(string.Format(T("actividades_confirmar_baja_msg_fmt"), actividad.Descripcion));
                        pnlConfirmarBaja.Visible = true;
                        break;

                    case "Activar":
                        bllActividad.CambiarEstado(codActividad, true);
                        MostrarExito(T("actividades_msg_estado"));
                        CargarDatos();
                        break;
                }
            }
            catch (InscripcionException ex)
            {
                // Al reactivar: un profesor o un alumno quedaría en dos clases a la misma hora
                MostrarError(ex.Message);
            }
            catch (Exception)
            {
                MostrarError(T("msg_error_generico"));
            }
        }

        protected void btnConfirmarBaja_Click(object sender, EventArgs e)
        {
            if (!PuedeGestionar || CodActividadBaja == 0) return;

            try
            {
                bllActividad.CambiarEstado(CodActividadBaja, false);
                pnlConfirmarBaja.Visible = false;
                CodActividadBaja = 0;
                MostrarExito(T("actividades_msg_estado"));
                CargarDatos();
            }
            catch (Exception)
            {
                MostrarError(T("msg_error_generico"));
            }
        }

        protected void btnCancelarBaja_Click(object sender, EventArgs e)
        {
            pnlConfirmarBaja.Visible = false;
            CodActividadBaja = 0;
        }

        // ==================== INSCRIPCIÓN FIJA POR TURNO (GESTIÓN) ====================

        private void AbrirInscripciones(int codActividad)
        {
            Actividad actividad = bllActividad.ObtenerActividad(codActividad);
            if (actividad == null)
            {
                MostrarError(T("msg_error_generico"));
                return;
            }

            litInscripcionesTitulo.Text = Server.HtmlEncode(string.Format(T("actividades_inscripciones_titulo_fmt"), actividad.Descripcion));

            ddlTurnoInscripciones.Items.Clear();
            foreach (ActividadHorario horario in actividad.Horarios)
            {
                ddlTurnoInscripciones.Items.Add(new ListItem($"{DescribirTurno(horario)} · {horario.NombreAula}", horario.CodHorario.ToString()));
            }

            CodHorarioInscripcion = actividad.Horarios.Count > 0 ? actividad.Horarios[0].CodHorario : 0;
            CargarFijasDelTurno();
            pnlInscripciones.Visible = true;
        }

        private void CargarFijasDelTurno()
        {
            cblAlumnos.Items.Clear();
            btnGuardarInscripciones.Visible = CodHorarioInscripcion != 0;

            if (CodHorarioInscripcion == 0)
            {
                litSinAlumnos.Text = "<span class=\"texto-vacio\">" + Server.HtmlEncode(T("actividades_sin_turnos")) + "</span>";
                litSinAlumnos.Visible = true;
                return;
            }

            ddlTurnoInscripciones.SelectedValue = CodHorarioInscripcion.ToString();

            HashSet<int> inscriptos = new HashSet<int>(bllInscripcion.ListarAlumnosConFija(CodHorarioInscripcion));
            var alumnos = (new BLLAlumno().ListarAlumnos() ?? new List<Alumno>())
                .Where(a => a.Activo || inscriptos.Contains(a.DNI))
                .OrderBy(a => a.Apellido).ThenBy(a => a.Nombre)
                .ToList();

            foreach (Alumno alumno in alumnos)
            {
                var item = new ListItem($"{alumno.Apellido}, {alumno.Nombre} (DNI {alumno.DNI})", alumno.DNI.ToString());
                item.Selected = inscriptos.Contains(alumno.DNI);
                cblAlumnos.Items.Add(item);
            }

            litSinAlumnos.Text = "<span class=\"texto-vacio\">" + Server.HtmlEncode(T("actividades_sin_alumnos")) + "</span>";
            litSinAlumnos.Visible = alumnos.Count == 0;
        }

        protected void ddlTurnoInscripciones_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (!PuedeGestionar) return;

            try
            {
                CodHorarioInscripcion = int.TryParse(ddlTurnoInscripciones.SelectedValue, out int cod) ? cod : 0;
                CargarFijasDelTurno();
            }
            catch (Exception)
            {
                MostrarError(T("msg_error_generico"));
            }
        }

        protected void btnGuardarInscripciones_Click(object sender, EventArgs e)
        {
            if (!PuedeGestionar || CodHorarioInscripcion == 0) return;

            try
            {
                List<int> seleccionados = cblAlumnos.Items.Cast<ListItem>()
                    .Where(i => i.Selected)
                    .Select(i => int.Parse(i.Value))
                    .ToList();

                bllInscripcion.GuardarFijasDeHorario(CodHorarioInscripcion, seleccionados);

                pnlInscripciones.Visible = false;
                CodHorarioInscripcion = 0;
                MostrarExito(T("actividades_msg_inscripciones_guardadas"));
                CargarDatos();
            }
            catch (InscripcionException ex)
            {
                MostrarError(ex.Message);
            }
            catch (Exception)
            {
                MostrarError(T("msg_error_generico"));
            }
        }

        protected void btnCerrarInscripciones_Click(object sender, EventArgs e)
        {
            pnlInscripciones.Visible = false;
            CodHorarioInscripcion = 0;
        }

        // ==================== MIS ACTIVIDADES (CLIENTE / FAMILIAR) ====================

        /// <summary>
        /// Lista los turnos de las actividades activas con la inscripción fija del alumno elegido.
        /// Si la cuenta tiene más de un alumno vinculado (Familiar) se muestra un selector.
        /// </summary>
        private void CargarMisActividades()
        {
            try
            {
                List<Alumno> misAlumnos = new BLLAlumno().ObtenerAlumnosDeUsuario(UsuarioActual);

                if (misAlumnos.Count == 0)
                {
                    DniMisActividades = 0;
                    phSelectorAlumno.Visible = false;
                    rptMisActividades.DataSource = null;
                    rptMisActividades.DataBind();
                    litSinMisActividades.Text = Server.HtmlEncode(T("actividades_sin_alumnos_vinculados"));
                    phSinMisActividades.Visible = true;
                    return;
                }

                if (!misAlumnos.Any(a => a.DNI == DniMisActividades))
                {
                    DniMisActividades = misAlumnos[0].DNI;
                }

                phSelectorAlumno.Visible = misAlumnos.Count > 1;
                ddlMisAlumnos.Items.Clear();
                foreach (Alumno alumno in misAlumnos)
                {
                    ddlMisAlumnos.Items.Add(new ListItem($"{alumno.Apellido}, {alumno.Nombre}", alumno.DNI.ToString()));
                }
                ddlMisAlumnos.SelectedValue = DniMisActividades.ToString();

                BLLInscripcion.Agenda agenda = bllInscripcion.ObtenerAgenda(DniMisActividades);
                Dictionary<int, Actividad> actividades = bllActividad.ListarTodasLasActividades()
                    .Where(a => a.Activo)
                    .ToDictionary(a => a.CodActividad);
                List<ActividadHorario> turnos = bllActividad.ListarHorariosActivos()
                    .Where(h => actividades.ContainsKey(h.CodActividad))
                    .OrderBy(h => h.DescripcionActividad).ThenBy(h => h.DiaSemana).ThenBy(h => h.HoraInicio)
                    .ToList();

                rptMisActividades.DataSource = turnos.Select(h =>
                {
                    bool fija = agenda.TieneFijaAbierta(h.CodHorario, BLLInscripcion.ProximaFecha(h.DiaSemana, DateTime.Today));
                    return new
                    {
                        h.CodHorario,
                        Descripcion = h.DescripcionActividad,
                        Turno = $"{DescribirTurno(h)} · {h.NombreAula}",
                        Instructores = string.IsNullOrEmpty(h.Instructores) ? "—" : h.Instructores,
                        Precio = actividades[h.CodActividad].PrecioAlumno.ToString("C0", CultureInfo.CurrentCulture),
                        Fija = fija,
                        EstadoTexto = fija ? T("actividades_estado_fija") : T("actividades_estado_no_anotado")
                    };
                }).ToList();
                rptMisActividades.DataBind();

                litSinMisActividades.Text = Server.HtmlEncode(T("actividades_sin_actividades"));
                phSinMisActividades.Visible = turnos.Count == 0;
            }
            catch (Exception)
            {
                MostrarError(T("msg_error_generico"));
            }
        }

        protected void ddlMisAlumnos_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (!EsCliente) return;

            DniMisActividades = int.TryParse(ddlMisAlumnos.SelectedValue, out int dni) ? dni : 0;
            hdnSeleccion.Value = string.Empty; // la selección era sobre el estado del otro alumno
            CargarDatos();
        }

        // ==================== SELECCIÓN DE VARIAS CLASES (CLIENTE / FAMILIAR) ====================

        /// <summary>
        /// Clases elegidas en el calendario, como "turno|yyyy-MM-dd" separados por coma.
        /// Se ignoran los valores que no tienen ese formato.
        /// </summary>
        private List<KeyValuePair<int, DateTime>> LeerSeleccion()
        {
            var clases = new List<KeyValuePair<int, DateTime>>();
            foreach (string item in (hdnSeleccion.Value ?? string.Empty).Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries))
            {
                string[] partes = item.Split('|');
                if (partes.Length == 2
                    && int.TryParse(partes[0], out int codHorario)
                    && DateTime.TryParseExact(partes[1], FormatoFecha, CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime fecha))
                {
                    clases.Add(new KeyValuePair<int, DateTime>(codHorario, fecha));
                }
            }
            return clases;
        }

        protected void btnAnotarSeleccion_Click(object sender, EventArgs e)
        {
            AplicarSeleccion(anotar: true);
        }

        protected void btnBajaSeleccion_Click(object sender, EventArgs e)
        {
            AplicarSeleccion(anotar: false);
        }

        private void AplicarSeleccion(bool anotar)
        {
            if (!EsCliente || DniMisActividades == 0) return;

            try
            {
                List<KeyValuePair<int, DateTime>> clases = LeerSeleccion();
                int cantidad = anotar
                    ? bllInscripcion.AnotarPropioVarias(UsuarioActual, DniMisActividades, clases)
                    : bllInscripcion.DesanotarPropioVarias(UsuarioActual, DniMisActividades, clases);

                hdnSeleccion.Value = string.Empty;
                MostrarExito(string.Format(T(anotar ? "actividades_msg_anotado_varias_fmt" : "actividades_msg_desanotado_varias_fmt"), cantidad));
            }
            catch (InscripcionException ex)
            {
                MostrarError(ex.Message);
            }
            catch (Exception)
            {
                MostrarError(T("msg_error_generico"));
            }

            CargarDatos();
        }

        protected void rptMisActividades_ItemCommand(object source, RepeaterCommandEventArgs e)
        {
            if (!EsCliente || DniMisActividades == 0) return;
            if (e.CommandName != "AnotarFija" && e.CommandName != "BajaFija") return;

            try
            {
                int codHorario = Convert.ToInt32(e.CommandArgument);
                ActividadHorario horario = bllInscripcion.ObtenerHorario(codHorario);
                if (horario == null)
                {
                    MostrarError(T("msg_error_generico"));
                    return;
                }

                DateTime proxima = BLLInscripcion.ProximaFecha(horario.DiaSemana, DateTime.Today);
                if (e.CommandName == "AnotarFija")
                {
                    bllInscripcion.AnotarPropio(UsuarioActual, DniMisActividades, codHorario, proxima, AlcanceInscripcion.Todas);
                    MostrarExito(T("actividades_msg_anotado"));
                }
                else
                {
                    bllInscripcion.DesanotarPropio(UsuarioActual, DniMisActividades, codHorario, proxima, AlcanceBaja.DesdeEstaClase);
                    MostrarExito(T("actividades_msg_desanotado"));
                }

                CargarDatos();
            }
            catch (InscripcionException ex)
            {
                MostrarError(ex.Message);
                CargarDatos();
            }
            catch (Exception)
            {
                MostrarError(T("msg_error_generico"));
                CargarDatos();
            }
        }

        // ==================== VENTANA DE CLASE (TURNO + FECHA) ====================

        /// <summary>
        /// El calendario guarda turno y fecha en campos ocultos y dispara este botón oculto.
        /// </summary>
        protected void btnAbrirClase_Click(object sender, EventArgs e)
        {
            if (!EsCliente && !PuedeGestionar) return;

            if (!int.TryParse(hdnClaseHorario.Value, out int codHorario)
                || !DateTime.TryParseExact(hdnClaseFecha.Value, FormatoFecha, CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime fecha))
            {
                MostrarError(T("msg_error_generico"));
                return;
            }

            AbrirClase(codHorario, fecha);
        }

        private void AbrirClase(int codHorario, DateTime fecha)
        {
            try
            {
                ActividadHorario horario = bllInscripcion.ObtenerHorario(codHorario);
                if (horario == null || ActividadHorario.DiaSemanaDesde(fecha.DayOfWeek) != horario.DiaSemana)
                {
                    MostrarError(T("msg_error_generico"));
                    return;
                }

                ClaseCodHorario = codHorario;
                ClaseFecha = fecha.Date;
                bool pasada = fecha.Date < DateTime.Today;

                litClaseTitulo.Text = Server.HtmlEncode(horario.DescripcionActividad);
                litClaseDetalle.Text = Server.HtmlEncode(
                    $"{T(ClavesDias[horario.DiaSemana - 1])} {fecha:d} · {horario.HoraInicio:hh\\:mm}–{horario.HoraFin:hh\\:mm} · {horario.NombreAula} · " +
                    (string.IsNullOrEmpty(horario.Instructores) ? T("actividades_sin_instructor") : horario.Instructores));
                phClasePasada.Visible = pasada;

                int anotados = bllInscripcion.ContarAnotados(codHorario, fecha);
                bool completa = horario.Cupo > 0 && anotados >= horario.Cupo;
                litClaseCupo.Text = Server.HtmlEncode(string.Format(T("actividades_cupo_fmt"), anotados, horario.Cupo));
                phClaseCompleta.Visible = completa && !pasada;

                CargarClaseCliente(pasada, completa);
                CargarClaseGestor(pasada, completa);

                pnlClase.Visible = true;
            }
            catch (Exception)
            {
                MostrarError(T("msg_error_generico"));
            }
        }

        private void CargarClaseCliente(bool pasada, bool completa)
        {
            phClaseCliente.Visible = EsCliente && DniMisActividades != 0;
            if (!phClaseCliente.Visible) return;

            EstadoClase estado = bllInscripcion.ObtenerAgenda(DniMisActividades).Estado(ClaseCodHorario, ClaseFecha);

            string textoEstado;
            switch (estado)
            {
                case EstadoClase.Fija: textoEstado = T("actividades_clase_estado_fija"); break;
                case EstadoClase.Puntual: textoEstado = T("actividades_clase_estado_puntual"); break;
                default: textoEstado = T("actividades_clase_estado_no"); break;
            }

            // Familiar con varios alumnos: aclarar de quién es el estado
            if (ddlMisAlumnos.Items.Count > 1 && ddlMisAlumnos.SelectedItem != null)
            {
                textoEstado = ddlMisAlumnos.SelectedItem.Text + ": " + textoEstado;
            }

            litClaseEstado.Text = Server.HtmlEncode(textoEstado);
            phClaseAnotar.Visible = !pasada && !completa && estado == EstadoClase.NoAnotado;
            phClaseDesanotar.Visible = !pasada && estado != EstadoClase.NoAnotado;
        }

        private void CargarClaseGestor(bool pasada, bool completa)
        {
            phClaseGestor.Visible = PuedeGestionar;
            if (!PuedeGestionar) return;

            Dictionary<int, EstadoClase> asistentes = bllInscripcion.ListarAsistentes(ClaseCodHorario, ClaseFecha);
            List<Alumno> alumnos = new BLLAlumno().ListarAlumnos() ?? new List<Alumno>();
            Dictionary<int, Alumno> porDni = alumnos.ToDictionary(a => a.DNI);

            rptAsistentes.DataSource = asistentes
                .Select(par => new
                {
                    Dni = par.Key,
                    Nombre = porDni.TryGetValue(par.Key, out Alumno a) ? $"{a.Apellido}, {a.Nombre}" : $"DNI {par.Key}",
                    TipoTexto = par.Value == EstadoClase.Fija ? T("actividades_tipo_fija") : T("actividades_tipo_puntual"),
                    PuedeQuitar = !pasada
                })
                .OrderBy(a => a.Nombre)
                .ToList();
            rptAsistentes.DataBind();

            litAsistentesTitulo.Text = Server.HtmlEncode(string.Format(T("actividades_clase_asistentes_fmt"), asistentes.Count));
            phSinAsistentes.Visible = asistentes.Count == 0;

            phAgregarAsistente.Visible = !pasada && !completa;
            ddlAgregarAlumno.Items.Clear();
            foreach (Alumno alumno in alumnos.Where(a => a.Activo && !asistentes.ContainsKey(a.DNI)).OrderBy(a => a.Apellido).ThenBy(a => a.Nombre))
            {
                ddlAgregarAlumno.Items.Add(new ListItem($"{alumno.Apellido}, {alumno.Nombre} (DNI {alumno.DNI})", alumno.DNI.ToString()));
            }
            btnClaseAgregar.Enabled = ddlAgregarAlumno.Items.Count > 0;
        }

        protected void btnClaseAnotar_Click(object sender, EventArgs e)
        {
            if (!EsCliente || DniMisActividades == 0 || ClaseCodHorario == 0) return;

            EjecutarEnClase(() =>
            {
                var alcance = (AlcanceInscripcion)int.Parse(rblAlcanceAlta.SelectedValue);
                bllInscripcion.AnotarPropio(UsuarioActual, DniMisActividades, ClaseCodHorario, ClaseFecha, alcance);
                MostrarExito(T("actividades_msg_anotado"));
            }, cerrar: true);
        }

        protected void btnClaseDesanotar_Click(object sender, EventArgs e)
        {
            if (!EsCliente || DniMisActividades == 0 || ClaseCodHorario == 0) return;

            EjecutarEnClase(() =>
            {
                var alcance = (AlcanceBaja)int.Parse(rblAlcanceBaja.SelectedValue);
                bllInscripcion.DesanotarPropio(UsuarioActual, DniMisActividades, ClaseCodHorario, ClaseFecha, alcance);
                MostrarExito(T("actividades_msg_desanotado"));
            }, cerrar: true);
        }

        protected void btnClaseAgregar_Click(object sender, EventArgs e)
        {
            if (!PuedeGestionar || ClaseCodHorario == 0 || !int.TryParse(ddlAgregarAlumno.SelectedValue, out int dni)) return;

            EjecutarEnClase(() =>
            {
                var alcance = (AlcanceInscripcion)int.Parse(rblAlcanceAltaGestor.SelectedValue);
                bllInscripcion.AnotarComoGestor(dni, ClaseCodHorario, ClaseFecha, alcance);
                MostrarExito(T("actividades_msg_alumno_anotado"));
            }, cerrar: false);
        }

        protected void rptAsistentes_ItemCommand(object source, RepeaterCommandEventArgs e)
        {
            if (!PuedeGestionar || ClaseCodHorario == 0) return;
            if (e.CommandName != "QuitarUna" && e.CommandName != "QuitarDesde") return;

            int dni = Convert.ToInt32(e.CommandArgument);
            AlcanceBaja alcance = e.CommandName == "QuitarUna" ? AlcanceBaja.SoloEstaClase : AlcanceBaja.DesdeEstaClase;

            EjecutarEnClase(() =>
            {
                bllInscripcion.DesanotarComoGestor(dni, ClaseCodHorario, ClaseFecha, alcance);
                MostrarExito(T("actividades_msg_alumno_quitado"));
            }, cerrar: false);
        }

        /// <summary>
        /// Ejecuta una acción de la ventana de clase, recarga la página y, si no se cierra, vuelve a mostrar la clase actualizada.
        /// </summary>
        private void EjecutarEnClase(Action accion, bool cerrar)
        {
            try
            {
                accion();
                pnlClase.Visible = !cerrar;
            }
            catch (InscripcionException ex)
            {
                MostrarError(ex.Message);
            }
            catch (Exception)
            {
                MostrarError(T("msg_error_generico"));
            }

            CargarDatos();
            if (pnlClase.Visible)
            {
                AbrirClase(ClaseCodHorario, ClaseFecha);
            }
        }

        protected void btnCerrarClase_Click(object sender, EventArgs e)
        {
            pnlClase.Visible = false;
            ClaseCodHorario = 0;
        }
    }
}
