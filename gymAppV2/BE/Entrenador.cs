using System;

namespace BE
{
    public class Entrenador
    {
        public int DNI { get; set; }
        public string Nombre { get; set; }
        public string Apellido { get; set; }
        public DateTime FechaNacimiento { get; set; }
        public string Usuario { get; set; }
        public bool Activo { get; set; }  // Derived from USUARIO_Activo
        public int AlumnosCount { get; set; }  // Count of students assigned via Rutinas
        public string DVV { get; set; }
        public string DVH { get; set; }

        public Entrenador()
        {
        }

        public Entrenador(int dni, string nombre, string apellido, DateTime fechaNacimiento, string usuario, bool activo, int alumnosCount, string dvv, string dvh)
        {
            DNI = dni;
            Nombre = nombre;
            Apellido = apellido;
            FechaNacimiento = fechaNacimiento;
            Usuario = usuario;
            Activo = activo;
            AlumnosCount = alumnosCount;
            DVV = dvv;
            DVH = dvh;
        }
    }
}