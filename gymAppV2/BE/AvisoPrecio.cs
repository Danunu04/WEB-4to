using System;

namespace BE
{
    /// <summary>
    /// Aviso interno de cambio de precio de cuota (tabla AvisoPrecio). Se muestra como banner a
    /// los usuarios que no son entrenadores hasta que lo marcan como visto.
    /// </summary>
    public class AvisoPrecio
    {
        public int CodAviso { get; set; }
        public int ModalidadId { get; set; }
        public string Descripcion { get; set; }
        public decimal PrecioAnterior { get; set; }
        public decimal PrecioNuevo { get; set; }
        public DateTime Fecha { get; set; }
        public string UsuarioRegistro { get; set; }
        public string DVH { get; set; }
    }
}
