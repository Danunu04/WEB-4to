using System;

namespace BE
{
    /// <summary>
    /// Lo que completa una persona al registrarse desde el login (CUS002): su cuenta, los datos
    /// del alumno y el pago del primer mes. Los datos de la tarjeta solo se validan (pago
    /// simulado): no se guardan.
    /// </summary>
    public class DatosRegistro
    {
        // Cuenta
        public string Usuario { get; set; }
        public string Contrasena { get; set; }
        public string ConfirmarContrasena { get; set; }

        // Alumno
        public int Dni { get; set; }
        public string Nombre { get; set; }
        public string Apellido { get; set; }
        public string Telefono { get; set; }
        public string Email { get; set; }
        public DateTime? FechaNacimiento { get; set; }

        // Primer mes
        public int ModalidadId { get; set; }
        public string MetodoPago { get; set; }
        public string TarjetaNumero { get; set; }
        public string TarjetaTitular { get; set; }
        public string TarjetaVence { get; set; }
        public string TarjetaCvv { get; set; }
    }
}
