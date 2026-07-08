using System;

namespace BE
{
    /// <summary>
    /// Tipo de alteración detectada en la verificación de integridad de una tabla.
    /// </summary>
    public enum TipoAlteracion
    {
        SinAlteracion = 0,
        EdicionDato = 1,
        EliminacionFila = 2,
        InsercionNoAutorizada = 3,
        TablaVaciada = 4,
        TablaEliminada = 5
    }

    /// <summary>
    /// Representa el resultado de verificar integridad de una fila/campo de una tabla.
    /// </summary>
    public class ResultadoVerificacionDV
    {
        public string NombreTabla { get; set; }
        public string ClaveFila { get; set; }
        public string Campo { get; set; }
        public bool EsValido { get; set; }
        public string Mensaje { get; set; }
        public TipoAlteracion TipoAlteracion { get; set; }

        /// <summary>
        /// Descripción legible del tipo de alteración, para mostrar en grids.
        /// </summary>
        public string TipoAlteracionTexto
        {
            get
            {
                switch (TipoAlteracion)
                {
                    case TipoAlteracion.EdicionDato:            return "Edición de dato";
                    case TipoAlteracion.EliminacionFila:        return "Eliminación de fila";
                    case TipoAlteracion.InsercionNoAutorizada:  return "Inserción no autorizada";
                    case TipoAlteracion.TablaVaciada:           return "Tabla vaciada";
                    case TipoAlteracion.TablaEliminada:         return "Tabla eliminada";
                    default:                                     return "—";
                }
            }
        }

        public string DVHAlmacenado { get; set; }
        public string DVHCalculado { get; set; }

        public ResultadoVerificacionDV()
        {
        }

        public ResultadoVerificacionDV(string nombreTabla, string claveFila, string campo, bool esValido, string mensaje)
        {
            NombreTabla = nombreTabla;
            ClaveFila = claveFila;
            Campo = campo;
            EsValido = esValido;
            Mensaje = mensaje;
        }
    }
}
