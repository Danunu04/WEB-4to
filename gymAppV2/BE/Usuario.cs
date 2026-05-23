using System;

namespace BE
{
    public class Usuario
    {
        public string USUARIO_Usuario { get; set; }
        public string USUARIO_Contras { get; set; }
        public bool USUARIO_Activo { get; set; }
        public int USUARIO_Intentos { get; set; }
        public Rol USUARIO_Rol { get; set; }
        public string USUARIO_DVV { get; set; }
        public string USUARIO_DVH { get; set; }

        public Usuario()
        {
        }

        public Usuario(string usuario, string contras, bool activo, int intentos, Rol rol, string dvv, string dvh)
        {
            USUARIO_Usuario = usuario;
            USUARIO_Contras = contras;
            USUARIO_Activo = activo;
            USUARIO_Intentos = intentos;
            USUARIO_Rol = rol;
            USUARIO_DVV = dvv;
            USUARIO_DVH = dvh;
        }
    }
}