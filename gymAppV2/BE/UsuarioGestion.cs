using System;

namespace BE
{
    [Serializable]
    public class UsuarioGestion
    {
        public string USUARIO_Usuario { get; set; }
        public string USUARIO_Contras { get; set; }
        public string USUARIO_Tipo { get; set; }
        public int USUARIO_Rol { get; set; }
        public bool USUARIO_Activo { get; set; }
        public bool USUARIO_Bloqueado { get; set; }
        public int USUARIO_Intentos { get; set; }
        public string USUARIO_DVV { get; set; }
        public string USUARIO_DVH { get; set; }

        // Campos adicionales para gestión (no en tabla USUARIOS)
        public int? DNI { get; set; }
        public string Nombre { get; set; }
        public string Apellido { get; set; }
        public string Telefono { get; set; }
        public string Email { get; set; }
        public DateTime? FechaNacimiento { get; set; }

        public UsuarioGestion()
        {
        }

        public UsuarioGestion(string usuario, string contras, string tipo, bool activo, bool bloqueado, int intentos, string dvv, string dvh)
        {
            USUARIO_Usuario = usuario;
            USUARIO_Contras = contras;
            USUARIO_Tipo = tipo;
            USUARIO_Activo = activo;
            USUARIO_Bloqueado = bloqueado;
            USUARIO_Intentos = intentos;
            USUARIO_DVV = dvv;
            USUARIO_DVH = dvh;
        }
    }
}