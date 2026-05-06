using System;

namespace BE
{
    public class Usuario
    {
        public int USUARIO_Id { get; set; }
        public string USUARIO_Nombre { get; set; }
        public string USUARIO_Contras { get; set; }
        public int USUARIO_Intentos { get; set; }
        public bool USUARIO_Baja { get; set; }
        public int USUARIO_DVH { get; set; }
    }
}