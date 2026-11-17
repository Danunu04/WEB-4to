using System;

namespace BE
{
    /// <summary>
    /// Pago de un abono por transferencia que informa el cliente (desde Pagos o al registrarse).
    /// El abono recién se activa cuando administración o recepción comprueba que la transferencia
    /// llegó y la confirma (tabla TransferenciaPendiente). Al confirmarla se registra el pago.
    /// </summary>
    public class TransferenciaPendiente
    {
        public int CodTransferencia { get; set; }
        public int Dni { get; set; }
        public int ModalidadId { get; set; }

        /// <summary>Monto que el cliente transfirió (precio de la modalidad al informarla).</summary>
        public decimal Monto { get; set; }

        public DateTime FechaSolicitud { get; set; }
        public string UsuarioSolicita { get; set; }

        /// <summary>Pendiente, Aprobada o Rechazada.</summary>
        public string Estado { get; set; }

        public string UsuarioResuelve { get; set; }
        public DateTime? FechaResuelve { get; set; }

        /// <summary>Pago registrado al confirmarla (null mientras está pendiente o si se rechazó).</summary>
        public int? CodPago { get; set; }

        public string DVH { get; set; }

        public string AlumnoNombre { get; set; }
        public string AlumnoApellido { get; set; }
        public string ModalidadDescripcion { get; set; }

        public string AlumnoNombreCompleto
        {
            get { return string.IsNullOrEmpty(AlumnoApellido) ? "—" : $"{AlumnoApellido}, {AlumnoNombre}"; }
        }

        public const string ESTADO_PENDIENTE = "Pendiente";
        public const string ESTADO_APROBADA = "Aprobada";
        public const string ESTADO_RECHAZADA = "Rechazada";
    }
}
