using System;

namespace BE
{
    /// <summary>
    /// Datos que recibe el web service para dar de alta o modificar un alumno (CUN011.2 / CUN011.3).
    ///
    /// Alta: si ya existe una persona con ese DNI (ej. un familiar con cuenta), solo se crea el
    /// alumno y se lo vincula. Si no existe, se crea también su cuenta de Cliente: Usuario y
    /// Contrasena son opcionales (por defecto cliente_DNI y una contraseña generada).
    /// UsuarioVinculo/Parentesco: para un menor, la cuenta del tutor a la que queda vinculado.
    /// </summary>
    public class SolicitudAlumno
    {
        public int Dni { get; set; }
        public string Nombre { get; set; }
        public string Apellido { get; set; }
        public string Telefono { get; set; }
        public DateTime? FechaNacimiento { get; set; }
        public decimal? Peso { get; set; }
        public bool Activo { get; set; } = true;

        public string Usuario { get; set; }
        public string Contrasena { get; set; }

        public string UsuarioVinculo { get; set; }
        public string Parentesco { get; set; }
    }
}
