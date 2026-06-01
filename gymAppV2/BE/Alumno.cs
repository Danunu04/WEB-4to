using System;

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

        // Campos de verificación
        public string DVV { get; set; }
        public string DVH { get; set; }

        // Referencia al usuario (solo el usr, no datos personales)
        public string Usuario { get; set; }

        // Propiedades de solo lectura para visualización (se llenan desde USUARIOS via JOIN)
        // Se usan en la UI para mostrar/buscar alumnos pero no se persisten en ALUMNOS
        public string Nombre { get; set; }
        public string Apellido { get; set; }
        public string Telefono { get; set; }
        public DateTime? FechaNacimiento { get; set; }

        public Alumno()
        {
        }

        public Alumno(int dni, decimal? peso, bool tieneRutinas, bool activo, string dvv, string dvh)
        {
            DNI = dni;
            Peso = peso;
            TieneRutinas = tieneRutinas;
            Activo = activo;
            DVV = dvv;
            DVH = dvh;
        }

        /// <summary>
        /// Constructor completo que incluye la referencia al usuario
        /// </summary>
        public Alumno(int dni, decimal? peso, bool tieneRutinas, bool activo, string dvv, string dvh, string usuario)
        {
            DNI = dni;
            Peso = peso;
            TieneRutinas = tieneRutinas;
            Activo = activo;
            DVV = dvv;
            DVH = dvh;
            Usuario = usuario;
        }
    }
}