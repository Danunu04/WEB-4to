using System;

namespace BE
{
    /// <summary>
    /// Alerta para recepción cuando un alumno intenta hacer check-in con el abono vencido o sin
    /// abono pago (tabla AlertaCheckin). Queda pendiente hasta que alguien de recepción la marca
    /// como vista.
    /// </summary>
    public class AlertaCheckin
    {
        public int CodAlerta { get; set; }
        public int Dni { get; set; }

        /// <summary>Vencido o SinAbono.</summary>
        public string Situacion { get; set; }

        /// <summary>Último día cubierto por el abono (null si nunca pagó).</summary>
        public DateTime? FechaVencimiento { get; set; }

        public DateTime Fecha { get; set; }
        public bool Vista { get; set; }
        public string UsuarioVista { get; set; }
        public DateTime? FechaVista { get; set; }
        public string DVH { get; set; }

        public string Nombre { get; set; }
        public string Apellido { get; set; }

        public const string SITUACION_VENCIDO = "Vencido";
        public const string SITUACION_SIN_ABONO = "SinAbono";
    }
}
