using System;
using System.Collections;
using System.Data;
using System.Data.SqlClient;
using BE;
using DAL;

namespace MPP
{
    public class MPPPreguntaSeguridad
    {
        private DalGeneral dal;

        public MPPPreguntaSeguridad()
        {
            dal = new DalGeneral();
        }

        public PreguntaSeguridad ObtenerPreguntaPorUsuario(string usuario)
        {
            try
            {
                string consulta = @"
                    SELECT id, pregunta, respuesta, usr, dvv, dvh
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
                        Convert.ToInt32(row["id"]),
                        row["pregunta"] != DBNull.Value ? row["pregunta"].ToString() : string.Empty,
                        row["respuesta"] != DBNull.Value ? row["respuesta"].ToString() : string.Empty,
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
                    new SqlParameter("@Pregunta", pregunta.Pregunta),
                    new SqlParameter("@Respuesta", pregunta.Respuesta)
                };

                dal._686DPEscribir(consulta, parametros);
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
                    return resultado.ToString().Equals(respuesta, StringComparison.OrdinalIgnoreCase);
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