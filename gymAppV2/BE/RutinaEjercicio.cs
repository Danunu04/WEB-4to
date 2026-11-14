namespace BE
{
    /// <summary>
    /// Un ejercicio dentro de una rutina (tabla RutinaEjercicio, CUN008): series, repeticiones,
    /// peso opcional, descanso entre series y observaciones del entrenador.
    /// </summary>
    public class RutinaEjercicio
    {
        public int CodRutinaEjercicio { get; set; }
        public int CodRutina { get; set; }
        public int CodEjercicio { get; set; }
        public int Orden { get; set; }
        public int Series { get; set; }
        public int Repeticiones { get; set; }
        public decimal? PesoLevantado { get; set; }
        public int? DescansoSegundos { get; set; }
        public string Observaciones { get; set; }
        public string DVH { get; set; }

        public string EjercicioNombre { get; set; }
        public string GrupoMuscular { get; set; }

        /// <summary>
        /// Qué porcentaje de la última 1RM registrada del alumno en ese ejercicio representa el
        /// peso indicado. Se calcula al mostrar (no se guarda); null si falta el peso o la 1RM.
        /// </summary>
        public decimal? PorcentajeRM { get; set; }
    }
}
