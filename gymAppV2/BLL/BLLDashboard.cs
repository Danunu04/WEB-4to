using System;
using System.Collections.Generic;
using System.Linq;
using BE;
using MPP;

namespace BLL
{
    /// <summary>
    /// Indicadores del panel principal y grilla semanal de actividades.
    /// </summary>
    public class BLLDashboard
    {
        private MPPDashboard mppDashboard;
        private BLLActividad bllActividad;

        public BLLDashboard()
        {
            mppDashboard = new MPPDashboard();
            bllActividad = new BLLActividad();
        }

        /// <summary>
        /// Calcula los indicadores del mes actual. Cada indicador se calcula por separado:
        /// si uno falla (ej. falta una tabla) queda en null y los demás se muestran igual.
        /// </summary>
        public ResumenDashboard ObtenerResumen(DateTime hoy)
        {
            DateTime periodoActual = new DateTime(hoy.Year, hoy.Month, 1);
            DateTime periodoAnterior = periodoActual.AddMonths(-1);

            var resumen = new ResumenDashboard
            {
                MiembrosActivos = Intentar(() => mppDashboard.ContarAlumnosActivos()),
                IngresosMes = Intentar(() => mppDashboard.SumarIngresosPeriodo(periodoActual)),
                IngresosMesAnterior = Intentar(() => mppDashboard.SumarIngresosPeriodo(periodoAnterior)),
                AlumnosAlDia = Intentar(() => mppDashboard.ContarAlumnosAlDia(periodoActual))
            };

            resumen.ClasesMes = Intentar<int?>(() =>
            {
                if (!mppDashboard.TablaExiste("ActividadHorario"))
                    return null;
                return ContarClasesDelMes(bllActividad.ListarHorariosActivos(), hoy.Year, hoy.Month);
            });

            return resumen;
        }

        /// <summary>
        /// Horarios de la semana. Devuelve una lista vacía si la tabla de horarios todavía no existe.
        /// </summary>
        public List<ActividadHorario> ListarHorariosSemana(string usuarioCliente = null)
        {
            if (!mppDashboard.TablaExiste("ActividadHorario"))
                return new List<ActividadHorario>();

            return string.IsNullOrEmpty(usuarioCliente)
                ? bllActividad.ListarHorariosActivos()
                : bllActividad.ListarHorariosPorCliente(usuarioCliente);
        }

        /// <summary>
        /// Cantidad de turnos que caen en el mes: cada horario semanal se repite
        /// tantas veces como su día de la semana aparece en el mes.
        /// </summary>
        public static int ContarClasesDelMes(List<ActividadHorario> horarios, int anio, int mes)
        {
            int diasEnMes = DateTime.DaysInMonth(anio, mes);
            var apariciones = new Dictionary<int, int>();

            for (int dia = 1; dia <= diasEnMes; dia++)
            {
                int diaSemana = ActividadHorario.DiaSemanaDesde(new DateTime(anio, mes, dia).DayOfWeek);
                apariciones[diaSemana] = apariciones.TryGetValue(diaSemana, out int n) ? n + 1 : 1;
            }

            return horarios.Sum(h => apariciones.TryGetValue(h.DiaSemana, out int n) ? n : 0);
        }

        private static T Intentar<T>(Func<T> calculo)
        {
            try
            {
                return calculo();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Trace.WriteLine("[Dashboard] " + ex.Message);
                return default(T);
            }
        }
    }
}
