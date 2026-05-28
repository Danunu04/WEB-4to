using System;

namespace BE
{
    public class Alumno
    {
        public int DNI { get; set; }
        public string Nombre { get; set; }
        public string Apellido { get; set; }
        public long? Telefono { get; set; }
        public DateTime FechaNacimiento { get; set; }
        public decimal? Peso { get; set; }
        public string Usuario { get; set; }
        public bool Activo { get; set; }
        public bool TieneRutinas { get; set; }
        //public TipoCliente TipoCliente { get; set; }
        public string DVV { get; set; }
        public string DVH { get; set; }

        public Alumno()
        {
        }

        public Alumno(int dni, string nombre, string apellido, long? telefono, DateTime fechaNacimiento, decimal? peso, string usuario, bool activo, bool tieneRutinas)
        {
            DNI = dni;
            Nombre = nombre;
            Apellido = apellido;
            Telefono = telefono;
            FechaNacimiento = fechaNacimiento;
            Peso = peso;
            Usuario = usuario;
            Activo = activo;
            TieneRutinas = tieneRutinas;
        }
    }
}