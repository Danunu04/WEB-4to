using System;

namespace BE
{
    /// <summary>
    /// Representa las 4 modalidades de precio de cuota mensual
    /// </summary>
    public class PrecioModalidad
    {
        public int Id { get; set; }
        public int DiasPorSemana { get; set; }  // 1, 2, 3, o 0 para "Diario"
        public bool EsDiario { get; set; }      // true si es modalidad "Diario (todos los días)"
        public decimal Precio { get; set; }
        public bool Activo { get; set; }
        public DateTime FechaModificacion { get; set; }
        public string DVV { get; set; }
        public string DVH { get; set; }

        public PrecioModalidad()
        {
        }

        public PrecioModalidad(int id, int diasPorSemana, bool esDiario, decimal precio, bool activo, DateTime fechaModificacion)
        {
            Id = id;
            DiasPorSemana = diasPorSemana;
            EsDiario = esDiario;
            Precio = precio;
            Activo = activo;
            FechaModificacion = fechaModificacion;
        }

        /// <summary>
        /// Obtiene la descripción de la modalidad
        /// </summary>
        public string ObtenerDescripcion()
        {
            if (EsDiario)
                return "Diario (todos los días)";
            return $"{DiasPorSemana} día{(DiasPorSemana > 1 ? "s" : "")} por semana";
        }
    }
}
