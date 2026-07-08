using System;

namespace BE
{
    /// <summary>
    /// Representa una actividad/clase ofrecida por el gimnasio.
    /// Mapea directamente la tabla Actividades del esquema de base de datos.
    /// </summary>
    public class Actividad
    {
        public int CodActividad { get; set; }
        public string Descripcion { get; set; }
        public int CantXSemana { get; set; }
        public decimal CostoInterno { get; set; }
        public decimal PrecioAlumno { get; set; }
        public bool Activo { get; set; }

        // Campo de verificación de integridad
        public string DVH { get; set; }

        public Actividad()
        {
        }

        public Actividad(int codActividad, string descripcion, int cantXSemana,
                         decimal costoInterno, decimal precioAlumno, bool activo,
                         string dvh)
        {
            CodActividad = codActividad;
            Descripcion = descripcion;
            CantXSemana = cantXSemana;
            CostoInterno = costoInterno;
            PrecioAlumno = precioAlumno;
            Activo = activo;
            DVH = dvh;
        }
    }
}
