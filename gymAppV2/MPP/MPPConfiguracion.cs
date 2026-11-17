using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using DAL;

namespace MPP
{
    /// <summary>
    /// Acceso a datos de la configuración del gimnasio (tabla ConfiguracionGimnasio: clave/valor).
    /// </summary>
    public class MPPConfiguracion
    {
        private DalGeneral dal;

        public MPPConfiguracion()
        {
            dal = new DalGeneral();
        }

        /// <summary>Valor guardado para la clave, o null si no existe.</summary>
        public string Obtener(string clave)
        {
            try
            {
                DataTable dt = dal._686DPConsultar(
                    "SELECT valor FROM [GymApp].[dbo].[ConfiguracionGimnasio] WHERE clave = @Clave",
                    new List<SqlParameter> { new SqlParameter("@Clave", clave) });

                return dt.Rows.Count > 0 && dt.Rows[0]["valor"] != DBNull.Value ? dt.Rows[0]["valor"].ToString() : null;
            }
            catch (Exception ex)
            {
                throw new Exception("Error al leer la configuración: " + ex.Message, ex);
            }
        }

        /// <summary>Crea o actualiza el valor de la clave y recalcula su dígito verificador.</summary>
        public void Guardar(string clave, string valor, string usuario)
        {
            try
            {
                string consulta = @"
                    IF EXISTS (SELECT 1 FROM [GymApp].[dbo].[ConfiguracionGimnasio] WHERE clave = @Clave)
                        UPDATE [GymApp].[dbo].[ConfiguracionGimnasio]
                        SET valor = @Valor, usuarioModifico = @Usuario, fechaModificacion = GETDATE(), dvh = ''
                        WHERE clave = @Clave;
                    ELSE
                        INSERT INTO [GymApp].[dbo].[ConfiguracionGimnasio] (clave, valor, usuarioModifico, fechaModificacion, dvh)
                        VALUES (@Clave, @Valor, @Usuario, GETDATE(), '');";

                dal._686DPEscribir(consulta, new List<SqlParameter>
                {
                    new SqlParameter("@Clave", clave),
                    new SqlParameter("@Valor", valor),
                    new SqlParameter("@Usuario", (object)usuario ?? DBNull.Value)
                });

                MPPDigitoVerificador.RecalcularDvhFilas("ConfiguracionGimnasio", "clave = @Clave", new SqlParameter("@Clave", clave));
            }
            catch (Exception ex)
            {
                throw new Exception("Error al guardar la configuración: " + ex.Message, ex);
            }
        }
    }
}
