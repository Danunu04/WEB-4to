using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using BE;
using DAL;

namespace MPP
{
    /// <summary>
    /// Acceso a datos para la entidad Actividad y sus relaciones con Alumnos y Entrenadores.
    /// </summary>
    public class MPPActividad
    {
        private DalGeneral dal;

        public MPPActividad()
        {
            dal = new DalGeneral();
        }

        /// <summary>
        /// Lista todas las actividades activas del gimnasio.
        /// </summary>
        public List<Actividad> ListarActividades()
        {
            try
            {
                string consulta = @"
                    SELECT
                        codActividad,
                        descripcion,
                        cantXSemana,
                        costoInterno,
                        precioAlumno,
                        activo,
                        dvh
                    FROM [GymApp].[dbo].[Actividades]
                    WHERE activo = 1
                    ORDER BY descripcion";

                DataTable dt = dal._686DPConsultar(consulta, new List<SqlParameter>());
                return MapearActividades(dt);
            }
            catch (Exception ex)
            {
                throw new Exception("Error al listar actividades: " + ex.Message, ex);
            }
        }

        /// <summary>
        /// Lista las actividades en las que están inscriptos los alumnos asociados a un usuario cliente.
        /// </summary>
        /// <param name="usuario">Nombre de usuario (usr) del cliente logueado.</param>
        public List<Actividad> ListarActividadesPorCliente(string usuario)
        {
            try
            {
                string consulta = @"
                    SELECT DISTINCT
                        a.codActividad,
                        a.descripcion,
                        a.cantXSemana,
                        a.costoInterno,
                        a.precioAlumno,
                        a.activo,
                        a.dvh
                    FROM [GymApp].[dbo].[Actividades] a
                    INNER JOIN [GymApp].[dbo].[Actividad_Alumno] aa ON a.codActividad = aa.codActividad
                    INNER JOIN [GymApp].[dbo].[Alumnos] al ON aa.dni = al.dni
                    WHERE al.usr = @Usuario
                      AND a.activo = 1
                    ORDER BY a.descripcion";

                List<SqlParameter> parametros = new List<SqlParameter>
                {
                    new SqlParameter("@Usuario", usuario)
                };

                DataTable dt = dal._686DPConsultar(consulta, parametros);
                return MapearActividades(dt);
            }
            catch (Exception ex)
            {
                throw new Exception("Error al listar actividades del cliente: " + ex.Message, ex);
            }
        }

        /// <summary>
        /// Mapea un DataTable con columnas de Actividades a una lista de entidades.
        /// </summary>
        private List<Actividad> MapearActividades(DataTable dt)
        {
            List<Actividad> actividades = new List<Actividad>();

            foreach (DataRow row in dt.Rows)
            {
                Actividad actividad = new Actividad(
                    Convert.ToInt32(row["codActividad"]),
                    row["descripcion"] != DBNull.Value ? row["descripcion"].ToString() : string.Empty,
                    Convert.ToInt32(row["cantXSemana"]),
                    Convert.ToDecimal(row["costoInterno"]),
                    Convert.ToDecimal(row["precioAlumno"]),
                    Convert.ToBoolean(row["activo"]),
                    row["dvh"] != DBNull.Value ? row["dvh"].ToString() : string.Empty
                );

                actividades.Add(actividad);
            }

            return actividades;
        }
    }
}
