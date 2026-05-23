using System;
using BE;
using MPP;

namespace BLL
{
    public class BLLPreguntaSeguridad
    {
        private MPPPreguntaSeguridad mppPreguntaSeguridad;

        public BLLPreguntaSeguridad()
        {
            mppPreguntaSeguridad = new MPPPreguntaSeguridad();
        }

        public PreguntaSeguridad ObtenerPreguntaPorUsuario(string usuario)
        {
            try
            {
                return mppPreguntaSeguridad.ObtenerPreguntaPorUsuario(usuario);
            }
            catch (Exception ex)
            {
                throw new Exception("Error al obtener pregunta de seguridad: " + ex.Message, ex);
            }
        }

        public void GuardarPregunta(PreguntaSeguridad pregunta)
        {
            try
            {
                mppPreguntaSeguridad.GuardarPregunta(pregunta);
            }
            catch (Exception ex)
            {
                throw new Exception("Error al guardar pregunta de seguridad: " + ex.Message, ex);
            }
        }

        public bool ValidarRespuesta(string usuario, string respuesta)
        {
            try
            {
                return mppPreguntaSeguridad.ValidarRespuesta(usuario, respuesta);
            }
            catch (Exception ex)
            {
                throw new Exception("Error al validar respuesta: " + ex.Message, ex);
            }
        }

        public string GenerarPreguntaSeguridad(string usuario, int anioNacimiento)
        {
            // Generate security question based on birth year
            return $"¿En qué año naciste?";
        }
    }
}