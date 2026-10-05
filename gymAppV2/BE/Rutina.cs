using System;

namespace BE
{
    /// <summary>
    /// Rutina de entrenamiento: relaciona un alumno, un entrenador y una actividad en una fecha.
    /// </summary>
    public class Rutina
    {
        public int CodRutina { get; set; }
        public string Descripcion { get; set; }
        public DateTime Fecha { get; set; }
        public int DniAlumno { get; set; }
        public int DniEntrenador { get; set; }
        public int CodActividad { get; set; }

        // Campo de verificación de integridad
        public string DVH { get; set; }

        // Propiedades de solo lectura para visualización (se llenan vía JOIN)
        public string AlumnoNombre { get; set; }
        public string AlumnoApellido { get; set; }
        public string EntrenadorNombre { get; set; }
        public string EntrenadorApellido { get; set; }
        public string ActividadDescripcion { get; set; }

        public Rutina()
        {
        }

        public Rutina(int codRutina, string descripcion, DateTime fecha, int dniAlumno, int dniEntrenador, int codActividad, string dvh)
        {
            CodRutina = codRutina;
            Descripcion = descripcion;
            Fecha = fecha;
            DniAlumno = dniAlumno;
            DniEntrenador = dniEntrenador;
            CodActividad = codActividad;
            DVH = dvh;
        }
    }
}
