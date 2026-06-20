using System;

namespace BE
{
    /// <summary>
    /// Representa una pregunta de seguridad asociada a un usuario.
    /// </summary>
    public class PreguntaSeguridad
    {
        public int Id { get; set; }
        public string Pregunta { get; set; }
        public string Respuesta { get; set; }
        public string Usuario { get; set; }
        public string DVV { get; set; }
        public string DVH { get; set; }

        /// <summary>
        /// Tipo de pregunta de seguridad. Afecta el origen de la respuesta esperada.
        /// </summary>
        public TipoPreguntaSeguridad Tipo { get; set; }

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

    /// <summary>
    /// Tipos de pregunta de seguridad soportados.
    /// </summary>
    public enum TipoPreguntaSeguridad
    {
        /// <summary>Pregunta basada en la fecha de nacimiento del usuario.</summary>
        FechaNacimiento = 1,

        /// <summary>Pregunta basada en un alumno asociado al usuario.</summary>
        AlumnoAsociado = 2
    }
}