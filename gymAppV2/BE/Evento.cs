using System;

namespace BE
{
    public class Evento
    {
        public int EVENTO_Id { get; set; }
        public string EVENTO_Tipo { get; set; }
        public string EVENTO_Usuario { get; set; }
        public string EVENTO_Accion { get; set; }
        public DateTime EVENTO_Timestamp { get; set; }
        public string EVENTO_DVV { get; set; }
        public string EVENTO_DVH { get; set; }
        public bool Expandido { get; set; }
        public int EVENTO_Criticidad { get; set; }
        public string EVENTO_Modulo { get; set; }

        public Evento()
        {
        }

        public Evento(int id, string tipo, string usuario, string accion, DateTime timestamp, int criticidad = 1, string modulo = "")
        {
            EVENTO_Id = id;
            EVENTO_Tipo = tipo;
            EVENTO_Usuario = usuario;
            EVENTO_Accion = accion;
            EVENTO_Timestamp = timestamp;
            EVENTO_Criticidad = criticidad;
            EVENTO_Modulo = modulo;
            Expandido = false;
        }
    }
}