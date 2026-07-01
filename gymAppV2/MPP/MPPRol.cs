using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using BE;
using DAL;

namespace MPP
{
    public class MPPRol
    {
        private DalGeneral dal;

        public MPPRol()
        {
            dal = new DalGeneral();
        }

        public int ObtenerRol(string usuario)
        {
            try
            {
                string consulta = @"
                    SELECT rol
                    FROM [GymApp].[dbo].[USUARIOS]
                    WHERE usr = @Usuario";

                List<SqlParameter> parametros = new List<SqlParameter>
                {
                    new SqlParameter("@Usuario", usuario)
                };

                object resultado = dal._686DPEscalar(consulta, parametros);

                if (resultado != null && resultado != DBNull.Value)
                {
                    int rolValue = Convert.ToInt32(resultado);
                    return rolValue;
                }

                return 1; // Default role
            }
            catch (Exception ex)
            {
                throw new Exception("Error al obtener el rol: " + ex.Message, ex);
            }
        }

        public void ActualizarRol(string usuario, int rol)
        {
            try
            {
                string consulta = @"
                    UPDATE [GymApp].[dbo].[USUARIOS]
                    SET rol = @Rol
                    WHERE usr = @Usuario";

                List<SqlParameter> parametros = new List<SqlParameter>
                {
                    new SqlParameter("@Usuario", usuario),
                    new SqlParameter("@Rol", (int)rol)
                };

                dal._686DPEscribir(consulta, parametros);
            }
            catch (Exception ex)
            {
                throw new Exception("Error al actualizar el rol: " + ex.Message, ex);
            }
        }
    }
}