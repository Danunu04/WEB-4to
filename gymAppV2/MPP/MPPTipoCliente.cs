using System;
using System.Collections;
using System.Data;
using System.Data.SqlClient;
using BE;
using DAL;

namespace MPP
{
    public class MPPTipoCliente
    {
        private DalGeneral dal;

        public MPPTipoCliente()
        {
            dal = new DalGeneral();
        }

        public TipoCliente DeterminarTipoCliente(string usuario)
        {
            try
            {
                // Check if user has associated alumnos
                string consulta = @"
                    SELECT COUNT(DISTINCT a.dni) as TotalAlumnos,
                           SUM(CASE WHEN a.dni = u.dniUsuario THEN 1 ELSE 0 END) as DNICoincidente
                    FROM [GymApp].[dbo].[Alumnos] a
                    CROSS JOIN [GymApp].[dbo].[USUARIOS] u
                    WHERE u.usr = @Usuario AND a.usr = u.usr";

                ArrayList parametros = new ArrayList
                {
                    new SqlParameter("@Usuario", usuario)
                };

                DataTable dt = dal._686DPConsultar(consulta, parametros);

                if (dt.Rows.Count > 0)
                {
                    DataRow row = dt.Rows[0];
                    int totalAlumnos = Convert.ToInt32(row["TotalAlumnos"]);
                    int dniCoincidente = Convert.ToInt32(row["DNICoincidente"]);

                    // Alumno: single alumno with matching DNI
                    if (totalAlumnos == 1 && dniCoincidente == 1)
                    {
                        return TipoCliente.Alumno;
                    }
                    // Familiar: multiple alumnos OR at least one with different DNI
                    else if (totalAlumnos > 0)
                    {
                        return TipoCliente.Familiar;
                    }
                }

                return TipoCliente.Alumno; // Default
            }
            catch (Exception ex)
            {
                throw new Exception("Error al determinar tipo de cliente: " + ex.Message, ex);
            }
        }
    }
}