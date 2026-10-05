using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using BE;
using BLL;
using Servicios.Singleton;

namespace gymAppV2.DashBoard
{
    public partial class WebForm1 : BasePage
    {
        private static readonly string[] Colores = { "bg-pink", "bg-mint", "bg-lavender", "bg-peach", "bg-sky" };

        // Claves de traducción de los días (1 = Lunes ... 7 = Domingo), compartidas con Actividades
        private static readonly string[] ClavesDias =
        {
            "actividades_dia_lun", "actividades_dia_mar", "actividades_dia_mie", "actividades_dia_jue",
            "actividades_dia_vie", "actividades_dia_sab", "actividades_dia_dom"
        };

        /// <summary>
        /// Los indicadores (ingresos, cuotas, etc.) son información de gestión:
        /// solo los ven Administrador, Recepcionista y WebMaster.
        /// </summary>
        private bool VeIndicadores
        {
            get
            {
                int? rol = Singleton.Instancia.Usuario?.USUARIO_Rol;
                return rol == PerfilesSistema.RolAdministrador
                    || rol == PerfilesSistema.RolRecepcionista
                    || rol == PerfilesSistema.RolWebMaster;
            }
        }

        private bool EsCliente
        {
            get
            {
                int? rol = Singleton.Instancia.Usuario?.USUARIO_Rol;
                return rol == PerfilesSistema.RolCliente || rol == PerfilesSistema.RolFamiliar;
            }
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            VerificarAcceso(PermisosSistema.Dashboard);

            if (!IsPostBack)
            {
                AplicarIdioma();
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
            litTitulo.Text       = T("dash_titulo");
            litSubtitulo.Text    = T("dash_bienvenido");
            litKpiMiembros.Text  = T("dash_kpi_miembros");
            litKpiClases.Text    = T("dash_kpi_clases");
            litKpiIngresos.Text  = T("dash_kpi_ingresos");
            litKpiAlDia.Text     = T("dash_kpi_al_dia");
            litSemanaTitulo.Text = T("dash_semana_titulo");
            litColActividad.Text = T("dash_col_actividad");
            litColInstructor.Text = T("dash_col_instructor");
            litColDia.Text       = T("dash_col_dia");
            litColHorario.Text   = T("dash_col_horario");
            litColDuracion.Text  = T("dash_col_duracion");
            litColEstado.Text    = T("dash_col_estado");
            litSinHorarios.Text  = T("dash_sin_horarios");
        }

        private void CargarDatos()
        {
            var bllDashboard = new BLLDashboard();
            DateTime ahora = DateTime.Now;

            pnlKpis.Visible = VeIndicadores;
            if (VeIndicadores)
            {
                CargarIndicadores(bllDashboard.ObtenerResumen(ahora));
            }

            try
            {
                string usuarioCliente = EsCliente ? Singleton.Instancia.Usuario?.USUARIO_Usuario : null;
                CargarSemana(bllDashboard.ListarHorariosSemana(usuarioCliente), ahora);
            }
            catch (Exception)
            {
                CargarSemana(new List<ActividadHorario>(), ahora);
                MostrarError(T("msg_error_generico"));
            }
        }

        private void CargarIndicadores(ResumenDashboard resumen)
        {
            CultureInfo cultura = CultureInfo.CurrentCulture;

            litValorMiembros.Text = resumen.MiembrosActivos?.ToString("N0", cultura) ?? "—";
            litValorClases.Text   = resumen.ClasesMes?.ToString("N0", cultura) ?? "—";
            litValorIngresos.Text = resumen.IngresosMes?.ToString("C0", cultura) ?? "—";
            litValorAlDia.Text    = resumen.PorcentajeAlDia.HasValue ? resumen.PorcentajeAlDia + "%" : "—";

            decimal? variacion = resumen.VariacionIngresos;
            pnlTendenciaIngresos.Visible = variacion.HasValue;
            if (variacion.HasValue)
            {
                bool sube = variacion.Value >= 0;
                pnlTendenciaIngresos.CssClass = "kpi-card-trend " + (sube ? "up" : "down");
                iconTendencia.Attributes["class"] = sube ? "bi bi-arrow-up-short" : "bi bi-arrow-down-short";
                litTendenciaIngresos.Text = Math.Abs(variacion.Value).ToString("0.#", cultura) + "%";
            }
        }

        /// <summary>
        /// Arma la grilla de la semana actual. El estado de cada turno se calcula contra la hora actual:
        /// ya terminó (Completada), es hoy y todavía no terminó (Pendiente), o es más adelante (Programada).
        /// </summary>
        private void CargarSemana(List<ActividadHorario> horarios, DateTime ahora)
        {
            int hoy = ActividadHorario.DiaSemanaDesde(ahora.DayOfWeek);
            TimeSpan horaActual = ahora.TimeOfDay;

            var filas = horarios.Select(h =>
            {
                string estado, badge;
                if (h.DiaSemana < hoy || (h.DiaSemana == hoy && h.HoraFin <= horaActual))
                {
                    estado = T("dash_badge_completada");
                    badge = "badge-mint";
                }
                else if (h.DiaSemana == hoy)
                {
                    estado = T("dash_badge_pendiente");
                    badge = "badge-lavender";
                }
                else
                {
                    estado = T("dash_badge_programada");
                    badge = "badge-peach";
                }

                return new
                {
                    Actividad = h.DescripcionActividad,
                    Inicial = string.IsNullOrEmpty(h.DescripcionActividad) ? "?" : h.DescripcionActividad.Substring(0, 1).ToUpper(),
                    Color = Colores[h.CodActividad % Colores.Length],
                    Instructor = string.IsNullOrEmpty(h.Instructores) ? "—" : h.Instructores,
                    Dia = T(ClavesDias[h.DiaSemana - 1]),
                    Horario = $"{h.HoraInicio:hh\\:mm} - {h.HoraFin:hh\\:mm}",
                    Duracion = h.DuracionMin + " min",
                    Estado = estado,
                    BadgeClase = badge
                };
            }).ToList();

            rptSemana.DataSource = filas;
            rptSemana.DataBind();
            phSinHorarios.Visible = filas.Count == 0;
        }
    }
}
