using System;

namespace BE
{
    /// <summary>
    /// Entrenador - solo datos específicos del rol.
    /// Los datos personales (Nombre, Apellido, DNI, Telefono, Email, FechaNacimiento) están en USUARIOS.
    /// </summary>
    public class Entrenador
    {
        // Clave foránea a USUARIOS.dni
        public int DNI { get; set; }

        // Datos específicos del rol Entrenador
        public int AlumnosCount { get; set; }  // Count of students assigned via Rutinas
        public bool Activo { get; set; }

        // Campos de verificación
        public string DVV { get; set; }
        public string DVH { get; set; }

        // Referencia al usuario (solo el usr, no datos personales)
        public string Usuario { get; set; }

        // Propiedades de solo lectura para visualización (se llenan desde USUARIOS via JOIN)
        public string Nombre { get; set; }
        public string Apellido { get; set; }
        public string Telefono { get; set; }
        public DateTime? FechaNacimiento { get; set; }

        public Entrenador()
        {
        }

        public Entrenador(int dni, int alumnosCount, bool activo, string dvv, string dvh)
        {
            DNI = dni;
            AlumnosCount = alumnosCount;
            Activo = activo;
            DVV = dvv;
            DVH = dvh;
        }

        /// <summary>
        /// Constructor completo que incluye la referencia al usuario
        /// </summary>
        public Entrenador(int dni, int alumnosCount, bool activo, string dvv, string dvh, string usuario)
        {
            DNI = dni;
            AlumnosCount = alumnosCount;
            Activo = activo;
            DVV = dvv;
            DVH = dvh;
            Usuario = usuario;
        }
    }
}