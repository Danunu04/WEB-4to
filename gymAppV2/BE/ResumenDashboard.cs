namespace BE
{
    /// <summary>
    /// Indicadores del panel principal, calculados con datos reales de la base.
    /// Un valor null significa que no se pudo calcular (ej. la tabla todavía no existe).
    /// </summary>
    public class ResumenDashboard
    {
        /// <summary>Alumnos con activo = 1.</summary>
        public int? MiembrosActivos { get; set; }

        /// <summary>Turnos de actividades activas que caen en el mes actual.</summary>
        public int? ClasesMes { get; set; }

        /// <summary>Suma de los pagos cuyo período es el mes actual.</summary>
        public decimal? IngresosMes { get; set; }

        /// <summary>Suma de los pagos cuyo período es el mes anterior.</summary>
        public decimal? IngresosMesAnterior { get; set; }

        /// <summary>Alumnos activos con la cuota del mes actual pagada.</summary>
        public int? AlumnosAlDia { get; set; }

        /// <summary>Porcentaje de alumnos activos con la cuota al día (0-100).</summary>
        public int? PorcentajeAlDia
        {
            get
            {
                if (!AlumnosAlDia.HasValue || !MiembrosActivos.HasValue || MiembrosActivos.Value == 0)
                    return null;
                return (int)System.Math.Round(AlumnosAlDia.Value * 100m / MiembrosActivos.Value);
            }
        }

        /// <summary>Variación porcentual de ingresos contra el mes anterior.</summary>
        public decimal? VariacionIngresos
        {
            get
            {
                if (!IngresosMes.HasValue || !IngresosMesAnterior.HasValue || IngresosMesAnterior.Value == 0)
                    return null;
                return System.Math.Round((IngresosMes.Value - IngresosMesAnterior.Value) * 100m / IngresosMesAnterior.Value, 1);
            }
        }
    }
}
