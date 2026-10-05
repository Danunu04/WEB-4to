using System;
using System.Collections.Generic;
using System.Linq;

namespace BE
{
    /// <summary>
    /// Alumno - solo datos específicos del rol.
    /// Los datos personales (Nombre, Apellido, Telefono, FechaNacimiento) están en USUARIOS.
    /// Las propiedades Nombre, Apellido, Telefono, FechaNacimiento se usan solo para visualización (JOIN con USUARIOS).
    /// </summary>
    public class Alumno
    {
        // Clave foránea a USUARIOS.dni
        public int DNI { get; set; }

        // Datos específicos del rol Alumno
        public decimal? Peso { get; set; }
        public bool TieneRutinas { get; set; }
        public bool Activo { get; set; }

        // Campo de verificación
        public string DVH { get; set; }

        // Usuarios vinculados (tabla intermedia ALUMNOS_USUARIOS): el propio titular
        // y/o uno o más familiares (madre/padre/tutor). Reemplaza a la vieja columna
        // única ALUMNOS.usr, que solo soportaba un usuario por alumno.
        public List<AlumnoUsuarioVinculo> Familiares { get; set; } = new List<AlumnoUsuarioVinculo>();

        // Conveniencia para la grilla: primer usuario vinculado, o vacío si no tiene ninguno.
        public string Usuario => Familiares.FirstOrDefault()?.Usuario ?? string.Empty;

        // Propiedades de solo lectura para visualización (se llenan desde USUARIOS via JOIN)
        // Se usan en la UI para mostrar/buscar alumnos pero no se persisten en ALUMNOS
        public string Nombre { get; set; }
        public string Apellido { get; set; }
        public string Telefono { get; set; }
        public DateTime? FechaNacimiento { get; set; }

        public Alumno()
        {
        }

        public Alumno(int dni, decimal? peso, bool tieneRutinas, bool activo, string dvh)
        {
            DNI = dni;
            Peso = peso;
            TieneRutinas = tieneRutinas;
            Activo = activo;
            DVH = dvh;
        }
    }
}