using System;

namespace BE
{
    /// <summary>
    /// Una repetición máxima (1RM) guardada en el historial de un alumno (tabla AlumnoRM).
    /// PesoLevantado y Repeticiones son la serie con la que se estimó; pueden faltar en
    /// registros cargados antes de que existiera la calculadora.
    /// </summary>
    public class RegistroRM
    {
        public int CodRM { get; set; }
        public int Dni { get; set; }
        public int CodEjercicio { get; set; }
        public decimal PesoRM { get; set; }
        public DateTime FechaRM { get; set; }
        public decimal? PesoLevantado { get; set; }
        public int? Repeticiones { get; set; }
        public string UsuarioRegistro { get; set; }
        public string DVH { get; set; }

        public string EjercicioNombre { get; set; }
        public string GrupoMuscular { get; set; }
    }
}
