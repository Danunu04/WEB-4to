using System;
using System.Collections.Generic;

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

        /// <summary>
        /// Tope opcional de alumnos por clase. El cupo de cada clase es el del aula del turno,
        /// o este tope si es menor. Null = se usa el del aula.
        /// </summary>
        public int? CupoMaximo { get; set; }

        // Campo de verificación de integridad
        public string DVH { get; set; }

        // Relaciones (se completan solo cuando se pide el detalle de la actividad)
        public List<ActividadHorario> Horarios { get; set; } = new List<ActividadHorario>();

        // Visualización: nombres de los profesores de sus turnos, separados por coma
        public string Instructores { get; set; }

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
