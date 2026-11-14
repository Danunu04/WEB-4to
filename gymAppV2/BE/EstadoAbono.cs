using System;

namespace BE
{
    public enum SituacionAbono
    {
        SinAbono,
        Vigente,
        PorVencer,
        Vencido
    }

    /// <summary>
    /// Situación del abono mensual de un alumno a una fecha dada. Se calcula a partir de sus
    /// pagos: como los abonos se encadenan, el último vencimiento es hasta cuándo está cubierto.
    /// </summary>
    public class EstadoAbono
    {
        public int Dni { get; set; }
        public string Nombre { get; set; }
        public string Apellido { get; set; }
        public bool AlumnoActivo { get; set; }

        public SituacionAbono Situacion { get; set; }

        /// <summary>Último día cubierto (null si nunca pagó).</summary>
        public DateTime? FechaVencimiento { get; set; }

        /// <summary>Días que quedan contando hoy (0 si está vencido o sin abono).</summary>
        public int DiasRestantes { get; set; }

        /// <summary>Modalidad del último pago, para proponerla al renovar.</summary>
        public int? ModalidadId { get; set; }

        /// <summary>Precio vigente de esa modalidad (lo que tiene que pagar para renovar).</summary>
        public decimal? MontoARenovar { get; set; }

        /// <summary>Ya tiene un abono pago que todavía no empezó (no puede pagar otro).</summary>
        public bool TieneAbonoAdelantado { get; set; }

        public bool EstaAlDia
        {
            get { return Situacion == SituacionAbono.Vigente || Situacion == SituacionAbono.PorVencer; }
        }

        public string NombreCompleto
        {
            get { return $"{Apellido}, {Nombre}"; }
        }
    }
}
