using System;

namespace BE
{
    /// <summary>
    /// Resumen del estado de control de DVH/DVV para una tabla.
    /// </summary>
    public class EstadoControlDV
    {
        /// <summary>
        /// Nombre de la tabla.
        /// </summary>
        public string NombreTabla { get; set; }

        /// <summary>
        /// Indica si la tabla tiene un registro en DigitoVerificador.
        /// </summary>
        public bool TieneControl { get; set; }

        /// <summary>
        /// Cantidad total de filas de la tabla.
        /// </summary>
        public int TotalFilas { get; set; }

        /// <summary>
        /// Cantidad de filas que tienen dvh vacío.
        /// </summary>
        public int FilasDVHVacio { get; set; }

        /// <summary>
        /// Cantidad de filas que tienen dvv vacío.
        /// </summary>
        public int FilasDVVVacio { get; set; }

        /// <summary>
        /// Fecha del último cálculo del control de tabla.
        /// </summary>
        public DateTime? FechaCalculo { get; set; }

        public EstadoControlDV()
        {
        }

        public EstadoControlDV(string nombreTabla, bool tieneControl, int totalFilas, int filasDVHVacio, int filasDVVVacio, DateTime? fechaCalculo)
        {
            NombreTabla = nombreTabla;
            TieneControl = tieneControl;
            TotalFilas = totalFilas;
            FilasDVHVacio = filasDVHVacio;
            FilasDVVVacio = filasDVVVacio;
            FechaCalculo = fechaCalculo;
        }
    }
}
