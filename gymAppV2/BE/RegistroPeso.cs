using System;

namespace BE
{
    /// <summary>
    /// Un peso corporal registrado en el historial de un alumno (tabla PesoHistorial).
    /// </summary>
    public class RegistroPeso
    {
        public int CodPeso { get; set; }
        public int Dni { get; set; }
        public decimal Peso { get; set; }
        public DateTime Fecha { get; set; }
        public string DVH { get; set; }
    }
}
