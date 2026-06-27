using System;
using System.Collections;
using System.Collections.Generic;
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
        private DigitoVerificadorManager dvManager;

        public MPPPreguntaSeguridad()
        {
            dal = new DalGeneral();
            criptoManager = new CriptoManager();
            dvManager = new DigitoVerificadorManager();
        }

        /// <summary>
        /// Calcula DVH y DVV de una pregunta de seguridad a partir de valores en texto plano.
        /// </summary>
        private void CalcularDigitosPregunta(PreguntaSeguridad pregunta, out string dvh, out string dvv)
        {
            var valores = new Dictionary<string, object>
            {
                { "usr", pregunta.Usuario },
                { "pregunta", pregunta.Pregunta },
                { "respuesta", pregunta.Respuesta }
            };

            dvManager.CalcularAmbos(valores, out dvh, out dvv);
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
                CalcularDigitosPregunta(pregunta, out string dvh, out string dvv);

                string consulta = @"
                    IF EXISTS (SELECT 1 FROM [GymApp].[dbo].[PreguntasSeguridad] WHERE usr = @Usuario)
                        UPDATE [GymApp].[dbo].[PreguntasSeguridad]
                        SET pregunta = @Pregunta,
                            respuesta = @Respuesta,
                            dvv = @DVV,
                            dvh = @DVH
                        WHERE usr = @Usuario
                    ELSE
                        INSERT INTO [GymApp].[dbo].[PreguntasSeguridad] (pregunta, respuesta, usr, dvv, dvh)
                        VALUES (@Pregunta, @Respuesta, @Usuario, @DVV, @DVH)";

                ArrayList parametros = new ArrayList
                {
                    new SqlParameter("@Usuario", pregunta.Usuario),
                    new SqlParameter("@Pregunta", EncriptarCampo(pregunta.Pregunta)),
                    new SqlParameter("@Respuesta", EncriptarCampo(pregunta.Respuesta)),
                    new SqlParameter("@DVV", dvv),
                    new SqlParameter("@DVH", dvh)
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

        /// <summary>
        /// Recalcula los dígitos verificadores de todas las preguntas de seguridad.
        /// Necesario para la inicialización del control de integridad, ya que la
        /// pregunta y respuesta se almacenan encriptadas.
        /// </summary>
        public void RecalcularDigitosTodasPreguntas()
        {
            try
            {
                string consulta = @"
                    SELECT codPregunta, usr, pregunta, respuesta
                    FROM [GymApp].[dbo].[PreguntasSeguridad]";

                DataTable dt = dal._686DPConsultar(consulta, new ArrayList());

                foreach (DataRow row in dt.Rows)
                {
                    PreguntaSeguridad pregunta = new PreguntaSeguridad(
                        Convert.ToInt32(row["codPregunta"]),
                        DesencriptarCampo(row["pregunta"] != DBNull.Value ? row["pregunta"].ToString() : string.Empty),
                        DesencriptarCampo(row["respuesta"] != DBNull.Value ? row["respuesta"].ToString() : string.Empty),
                        row["usr"] != DBNull.Value ? row["usr"].ToString() : string.Empty,
                        string.Empty,
                        string.Empty
                    );

                    CalcularDigitosPregunta(pregunta, out string dvh, out string dvv);

                    string actualizar = @"
                        UPDATE [GymApp].[dbo].[PreguntasSeguridad]
                        SET dvv = @DVV, dvh = @DVH
                        WHERE codPregunta = @IdPregunta";

                    ArrayList parametros = new ArrayList
                    {
                        new SqlParameter("@IdPregunta", pregunta.Id),
                        new SqlParameter("@DVV", dvv),
                        new SqlParameter("@DVH", dvh)
                    };

                    dal._686DPEscribir(actualizar, parametros);
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Error al recalcular dígitos de preguntas de seguridad: " + ex.Message, ex);
            }
        }

        /// <summary>
        /// Verifica la integridad de la tabla PreguntasSeguridad desencriptando
        /// la pregunta y respuesta antes de comparar DVH/DVV.
        /// </summary>
        public List<ResultadoVerificacionDV> VerificarIntegridadPreguntas()
        {
            var resultados = new List<ResultadoVerificacionDV>();

            try
            {
                string consulta = @"
                    SELECT codPregunta, usr, pregunta, respuesta, dvv, dvh
                    FROM [GymApp].[dbo].[PreguntasSeguridad]";

                DataTable dt = dal._686DPConsultar(consulta, new ArrayList());

                foreach (DataRow row in dt.Rows)
                {
                    int codPregunta = Convert.ToInt32(row["codPregunta"]);
                    string usuario = row["usr"] != DBNull.Value ? row["usr"].ToString() : string.Empty;
                    string dvvAlmacenado = row["dvv"] != DBNull.Value ? row["dvv"].ToString() : string.Empty;
                    string dvhAlmacenado = row["dvh"] != DBNull.Value ? row["dvh"].ToString() : string.Empty;

                    PreguntaSeguridad pregunta = new PreguntaSeguridad(
                        codPregunta,
                        DesencriptarCampo(row["pregunta"] != DBNull.Value ? row["pregunta"].ToString() : string.Empty),
                        DesencriptarCampo(row["respuesta"] != DBNull.Value ? row["respuesta"].ToString() : string.Empty),
                        usuario,
                        string.Empty,
                        string.Empty
                    );

                    CalcularDigitosPregunta(pregunta, out string dvhCalculado, out string dvvCalculado);

                    bool dvhOk = !string.IsNullOrEmpty(dvhAlmacenado) && dvhAlmacenado.Equals(dvhCalculado, StringComparison.OrdinalIgnoreCase);
                    bool dvvOk = !string.IsNullOrEmpty(dvvAlmacenado) && dvvAlmacenado.Equals(dvvCalculado, StringComparison.OrdinalIgnoreCase);

                    if (dvhOk && dvvOk)
                        continue;

                    resultados.Add(new ResultadoVerificacionDV
                    {
                        NombreTabla = "PreguntasSeguridad",
                        ClaveFila = codPregunta.ToString(),
                        Campo = "DVH/DVV (fila completa)",
                        EsValido = false,
                        Mensaje = $"Los dígitos verificadores de la pregunta del usuario {usuario} no coinciden.",
                        DVHAlmacenado = dvhAlmacenado,
                        DVHCalculado = dvhCalculado,
                        DVVAlmacenado = dvvAlmacenado,
                        DVVCalculado = dvvCalculado
                    });
                }

                if (resultados.Count == 0)
                {
                    resultados.Add(new ResultadoVerificacionDV
                    {
                        NombreTabla = "PreguntasSeguridad",
                        ClaveFila = "-",
                        Campo = "-",
                        EsValido = true,
                        Mensaje = "Tabla PreguntasSeguridad verificada correctamente."
                    });
                }
            }
            catch (Exception ex)
            {
                resultados.Add(new ResultadoVerificacionDV
                {
                    NombreTabla = "PreguntasSeguridad",
                    ClaveFila = "-",
                    Campo = "-",
                    EsValido = false,
                    Mensaje = "Error al verificar PreguntasSeguridad: " + ex.Message
                });
            }

            return resultados;
        }
    }
}
