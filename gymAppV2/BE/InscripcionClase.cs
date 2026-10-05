using System;

namespace BE
{
    /// <summary>
    /// Inscripción fija a un turno (tabla InscripcionFija): el alumno va a todas las clases
    /// del turno desde FechaDesde y, si tiene FechaHasta, hasta esa fecha inclusive.
    /// </summary>
    public class InscripcionFija
    {
        public int CodInscripcion { get; set; }
        public int DNI { get; set; }
        public int CodHorario { get; set; }
        public DateTime FechaDesde { get; set; }
        public DateTime? FechaHasta { get; set; }
        public string DVH { get; set; }

        public bool Cubre(DateTime fecha)
        {
            return FechaDesde <= fecha.Date && (!FechaHasta.HasValue || FechaHasta.Value >= fecha.Date);
        }
    }

    /// <summary>
    /// Excepción puntual sobre una fecha concreta de un turno (tabla InscripcionClase).
    /// </summary>
    public class InscripcionClase
    {
        public const string TipoAlta = "A";
        public const string TipoBaja = "B";

        public int CodInscripcionClase { get; set; }
        public int DNI { get; set; }
        public int CodHorario { get; set; }
        public DateTime Fecha { get; set; }

        /// <summary>"A" = va a esa clase sin inscripción fija; "B" = tiene fija pero esa fecha no va.</summary>
        public string Tipo { get; set; }

        public string DVH { get; set; }
    }

    /// <summary>
    /// A qué clases se anota el alumno a partir de la clase elegida.
    /// </summary>
    public enum AlcanceInscripcion
    {
        SoloEstaClase = 1,
        RestoDelMes = 2,
        ProximasCuatroSemanas = 3,
        Todas = 4
    }

    /// <summary>
    /// De qué clases se da de baja el alumno a partir de la clase elegida.
    /// </summary>
    public enum AlcanceBaja
    {
        SoloEstaClase = 1,
        DesdeEstaClase = 2
    }

    /// <summary>
    /// Situación de un alumno respecto de una clase concreta (turno + fecha).
    /// </summary>
    public enum EstadoClase
    {
        NoAnotado = 0,
        Fija = 1,
        Puntual = 2
    }
}
