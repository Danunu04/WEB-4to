using System;

namespace BE
{
    public class Usuario
    {
        // Campos principales de USUARIOS
        public string USUARIO_Usuario { get; set; }
        public string USUARIO_Contras { get; set; }
        public bool USUARIO_Activo { get; set; }
        public bool USUARIO_Bloqueado { get; set; }
        public int USUARIO_Intentos { get; set; }
        public int USUARIO_Rol { get; set; }
        public bool USUARIO_PrimerLogin { get; set; }

        // Campos normalizados (datos personales centralizados)
        public string USUARIO_Tipo { get; set; }  // 'Empleado', 'Entrenador', 'Cliente'
        public int USUARIO_DNI { get; set; }
        public string Nombre { get; set; }
        public string Apellido { get; set; }
        public string Telefono { get; set; }
        public string Email { get; set; }
        public DateTime? FechaNacimiento { get; set; }

        // Campo de verificación horizontal por fila
        public string USUARIO_DVH { get; set; }

        // Idioma preferido del usuario ('ES', 'EN', 'PT', 'FR', 'JA').
        // No forma parte del cálculo DVH — es un campo operativo.
        public string USUARIO_Idioma { get; set; }

        public Usuario()
        {
        }

        public Usuario(string usuario, string contras, bool activo, bool bloqueado, int intentos, int rol,
                       string tipo, int dni, string nombre, string apellido, string telefono,
                       string email, DateTime? fechaNacimiento, string dvh, bool primerLogin = true,
                       string idioma = "ES")
        {
            USUARIO_Usuario = usuario;
            USUARIO_Contras = contras;
            USUARIO_Activo = activo;
            USUARIO_Bloqueado = bloqueado;
            USUARIO_Intentos = intentos;
            USUARIO_Rol = rol;
            USUARIO_PrimerLogin = primerLogin;
            USUARIO_Tipo = tipo;
            USUARIO_DNI = dni;
            Nombre = nombre;
            Apellido = apellido;
            Telefono = telefono;
            Email = email;
            FechaNacimiento = fechaNacimiento;
            USUARIO_DVH = dvh;
            USUARIO_Idioma = idioma;
        }
    }
}