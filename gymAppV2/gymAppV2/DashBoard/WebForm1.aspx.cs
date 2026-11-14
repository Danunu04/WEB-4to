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

            CargarAlertasCheckin();

            pnlAbonoCliente.Visible = EsCliente;
            if (EsCliente)
            {
                CargarAbonoCliente(ahora);
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

        // ==================== ALERTAS DE CHECK-IN ====================

        /// <summary>Texto del botón de cada alerta (se evalúa al enlazar el Repeater).</summary>
        protected string TextoBotonVista
        {
            get { return Texto("dash_alertas_vista", "Marcar como vista"); }
        }

        /// <summary>
        /// Check-in rechazados por abono vencido o sin pagar que nadie marcó como vistos
        /// (CUN007, paso 5.3). Solo para administración y recepción; si no hay, no se muestra.
        /// </summary>
        private void CargarAlertasCheckin()
        {
            pnlAlertasCheckin.Visible = false;
            if (!VeIndicadores)
                return;

            try
            {
                List<AlertaCheckin> alertas = new BLLAlertaCheckin().ListarPendientes();
                pnlAlertasCheckin.Visible = alertas.Count > 0;
                if (alertas.Count == 0)
                    return;

                litAlertasTitulo.Text = Texto("dash_alertas_titulo", "Check-in rechazados por el abono ({0})", alertas.Count);
                litAlertasTodas.Text = Texto("dash_alertas_todas", "Marcar todas como vistas");

                rptAlertas.DataSource = alertas.Select(a => new
                {
                    a.CodAlerta,
                    Fecha = a.Fecha.ToString("dd/MM/yyyy HH:mm"),
                    Alumno = string.IsNullOrEmpty(a.Apellido) ? "—" : $"{a.Apellido}, {a.Nombre}",
                    a.Dni,
                    Situacion = a.Situacion == AlertaCheckin.SITUACION_VENCIDO && a.FechaVencimiento.HasValue
                        ? Texto("dash_alertas_vencido", "Abono vencido el {0}", a.FechaVencimiento.Value.ToString("dd/MM/yyyy"))
                        : Texto("dash_alertas_sin_abono", "Sin abono pago")
                }).ToList();
                rptAlertas.DataBind();
            }
            catch
            {
                // Si la tabla todavía no existe (falta el script), el panel no se muestra
                pnlAlertasCheckin.Visible = false;
            }
        }

        protected void rptAlertas_ItemCommand(object source, System.Web.UI.WebControls.RepeaterCommandEventArgs e)
        {
            if (e.CommandName != "Vista")
                return;

            try
            {
                new BLLAlertaCheckin().MarcarVista(Convert.ToInt32(e.CommandArgument), Singleton.Instancia.Usuario?.USUARIO_Usuario);
            }
            catch (Exception)
            {
                MostrarError(T("msg_error_generico"));
            }

            CargarAlertasCheckin();
        }

        protected void btnAlertasTodas_Click(object sender, EventArgs e)
        {
            try
            {
                new BLLAlertaCheckin().MarcarTodasVistas(Singleton.Instancia.Usuario?.USUARIO_Usuario);
            }
            catch (Exception)
            {
                MostrarError(T("msg_error_generico"));
            }

            CargarAlertasCheckin();
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
        /// Una tarjeta por alumno del cliente: días restantes, vencimiento y estado del abono.
        /// </summary>
        private void CargarAbonoCliente(DateTime ahora)
        {
            litAbonoTitulo.Text = Texto("dash_abono_titulo", "Mi abono");
            litAbonoHoy.Text = Texto("dash_abono_hoy", "Hoy: {0}", ahora.ToString("dd/MM/yyyy"));

            try
            {
                var estados = new BLLPago().ListarEstadosAbonoCliente(Singleton.Instancia.Usuario?.USUARIO_Usuario, ahora);

                rptAbonoCliente.DataSource = estados.Select(a =>
                {
                    string estado, badge;
                    switch (a.Situacion)
                    {
                        case SituacionAbono.Vigente:
                            estado = Texto("pagos_estado_vigente", "Al día");
                            badge = "badge-mint";
                            break;
                        case SituacionAbono.PorVencer:
                            estado = Texto("pagos_estado_por_vencer", "Por vencer");
                            badge = "badge-peach";
                            break;
                        case SituacionAbono.Vencido:
                            estado = Texto("pagos_estado_vencido", "Vencido");
                            badge = "badge-pink";
                            break;
                        default:
                            estado = Texto("pagos_estado_sin_abono", "Sin abono");
                            badge = "badge-lavender";
                            break;
                    }

                    string detalle = a.FechaVencimiento.HasValue
                        ? Texto(a.Situacion == SituacionAbono.Vencido ? "dash_abono_vencio" : "dash_abono_vence",
                                a.Situacion == SituacionAbono.Vencido ? "Venció el {0}" : "Vence el {0}",
                                a.FechaVencimiento.Value.ToString("dd/MM/yyyy"))
                        : Texto("dash_abono_sin_pagos", "Todavía no hay pagos registrados");

                    return new
                    {
                        Alumno = a.NombreCompleto,
                        Estado = estado,
                        BadgeClase = badge,
                        Dias = Texto("dash_abono_dias", "{0} días restantes", a.DiasRestantes),
                        Detalle = detalle
                    };
                }).ToList();
                rptAbonoCliente.DataBind();
            }
            catch (Exception)
            {
                pnlAbonoCliente.Visible = false;
                MostrarError(T("msg_error_generico"));
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
