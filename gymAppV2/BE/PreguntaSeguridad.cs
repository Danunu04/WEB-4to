using System;

namespace BE
{
    public class PreguntaSeguridad
    {
        public int Id { get; set; }
        public string Pregunta { get; set; }
        public string Respuesta { get; set; }
        public string Usuario { get; set; }
        public string DVV { get; set; }
        public string DVH { get; set; }

        public PreguntaSeguridad()
        {
        }

        public PreguntaSeguridad(int id, string pregunta, string respuesta, string usuario, string dvv, string dvh)
        {
            Id = id;
            Pregunta = pregunta;
            Respuesta = respuesta;
            Usuario = usuario;
            DVV = dvv;
            DVH = dvh;
        }
    }
}