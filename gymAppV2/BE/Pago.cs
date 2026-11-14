using System;

namespace BE
{
    /// <summary>
    /// Pago de la cuota mensual de un alumno, según su modalidad (PrecioModalidad).
    /// Cada pago es un abono de 30 días: de FechaInicio a FechaVencimiento, ambas incluidas.
    /// Periodo es el mes abonado (primer día del mes de FechaInicio).
    /// </summary>
    public class Pago
    {
        public int CodPago { get; set; }
        public int Dni { get; set; }
        public int ModalidadId { get; set; }
        public DateTime Periodo { get; set; }
        public DateTime FechaInicio { get; set; }
        public DateTime FechaVencimiento { get; set; }
        public decimal Monto { get; set; }
        public DateTime FechaPago { get; set; }
        public string MetodoPago { get; set; }
        public string UsuarioRegistro { get; set; }

        // Campo de verificación de integridad
        public string DVH { get; set; }

        // Propiedades de solo lectura para visualización (se llenan vía JOIN)
        public string AlumnoNombre { get; set; }
        public string AlumnoApellido { get; set; }
        public string ModalidadDescripcion { get; set; }

        public Pago()
        {
        }
    }
}
