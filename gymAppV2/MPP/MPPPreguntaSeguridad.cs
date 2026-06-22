using System;
using System.Collections;
using System.Data;
using System.Data.SqlClient;
using BE;
using DAL;
using SERVICIOS;

namespace MPP
{
    public class MPPPreguntaSeguridad
    {
        private DalGeneral dal;
        private CriptoManager criptoManager;

        public MPPPreguntaSeguridad()
        {
            dal = new DalGeneral();
            criptoManager = new CriptoManager();
        }

        /// <summary>
        /// Encripta un campo de pregunta/respuesta con AES-256 si tiene valor.
        /// </summary>
        private string EncriptarCampo(string valor)
        {
            return string.IsNullOrEmpty(valor) ? valor : criptoManager.EncriptarAES256(valor);
        }

        /// <summary>
        /// Desencripta un campo de pregunta/respuesta con AES-256 si tiene valor.
        /// Si el valor no se puede desencriptar, se asume que aún está en texto plano (migración gradual).
        /// </summary>
        private string DesencriptarCampo(string valor)
        {
            if (string.IsNullOrEmpty(valor))
                return valor;

            try
            {
                return criptoManager.DesencriptarAES256(valor);
            }
            catch
            {
                return valor;
            }
        }

        public PreguntaSeguridad ObtenerPreguntaPorUsuario(string usuario)
        {
            try
            {
                string consulta = @"
                    SELECT codPregunta, pregunta, respuesta, usr, dvv, dvh
                    FROM [GymApp].[dbo].[PreguntasSeguridad]
                    WHERE usr = @Usuario";

                ArrayList parametros = new ArrayList
                {
                    new SqlParameter("@Usuario", usuario)
                };

                DataTable dt = dal._686DPConsultar(consulta, parametros);

                if (dt.Rows.Count > 0)
                {
                    DataRow row = dt.Rows[0];
                    return new PreguntaSeguridad(
                        Convert.ToInt32(row["codPregunta"]),
                        DesencriptarCampo(row["pregunta"] != DBNull.Value ? row["pregunta"].ToString() : string.Empty),
                        DesencriptarCampo(row["respuesta"] != DBNull.Value ? row["respuesta"].ToString() : string.Empty),
                        row["usr"] != DBNull.Value ? row["usr"].ToString() : string.Empty,
                        row["dvv"] != DBNull.Value ? row["dvv"].ToString() : string.Empty,
                        row["dvh"] != DBNull.Value ? row["dvh"].ToString() : string.Empty
                    );
                }

                return null;
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
                string consulta = @"
                    IF EXISTS (SELECT 1 FROM [GymApp].[dbo].[PreguntasSeguridad] WHERE usr = @Usuario)
                        UPDATE [GymApp].[dbo].[PreguntasSeguridad]
                        SET pregunta = @Pregunta, respuesta = @Respuesta
                        WHERE usr = @Usuario
                    ELSE
                        INSERT INTO [GymApp].[dbo].[PreguntasSeguridad] (pregunta, respuesta, usr, dvv, dvh)
                        VALUES (@Pregunta, @Respuesta, @Usuario, '', '')";

                ArrayList parametros = new ArrayList
                {
                    new SqlParameter("@Usuario", pregunta.Usuario),
                    new SqlParameter("@Pregunta", EncriptarCampo(pregunta.Pregunta)),
                    new SqlParameter("@Respuesta", EncriptarCampo(pregunta.Respuesta))
                };

                dal._686DPEscribir(consulta, parametros);
            }
            catch (Exception ex)
            {
                throw new Exception("Error al guardar pregunta de seguridad: " + ex.Message, ex);
            }
        }

        public string ObtenerRespuestaPorUsuario(string usuario)
        {
            try
            {
                string consulta = @"
                    SELECT respuesta
                    FROM [GymApp].[dbo].[PreguntasSeguridad]
                    WHERE usr = @Usuario";

                ArrayList parametros = new ArrayList
                {
                    new SqlParameter("@Usuario", usuario)
                };

                object resultado = dal._686DPEscalar(consulta, parametros);

                if (resultado != null && resultado != DBNull.Value)
                {
                    return DesencriptarCampo(resultado.ToString());
                }

                return string.Empty;
            }
            catch (Exception ex)
            {
                throw new Exception("Error al obtener respuesta de seguridad: " + ex.Message, ex);
            }
        }

        /// <summary>
        /// Normaliza una respuesta quitando espacios extra para la comparación.
        /// </summary>
        private string NormalizarRespuesta(string valor)
        {
            if (string.IsNullOrWhiteSpace(valor))
            {
                return string.Empty;
            }

            return System.Text.RegularExpressions.Regex.Replace(valor.Trim(), @"\s+", " ");
        }

        public bool ValidarRespuesta(string usuario, string respuesta)
        {
            try
            {
                string consulta = @"
                    SELECT respuesta
                    FROM [GymApp].[dbo].[PreguntasSeguridad]
                    WHERE usr = @Usuario";

                ArrayList parametros = new ArrayList
                {
                    new SqlParameter("@Usuario", usuario)
                };

                object resultado = dal._686DPEscalar(consulta, parametros);

                if (resultado != null && resultado != DBNull.Value)
                {
                    string respuestaAlmacenada = NormalizarRespuesta(DesencriptarCampo(resultado.ToString()));
                    string respuestaIngresada = NormalizarRespuesta(respuesta);
                    return respuestaAlmacenada.Equals(respuestaIngresada, StringComparison.OrdinalIgnoreCase);
                }

                return false;
            }
            catch (Exception ex)
            {
                throw new Exception("Error al validar respuesta: " + ex.Message, ex);
            }
        }
    }
}