using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using BE;
using DAL;
using System.Configuration;

namespace MPP
{
    public class MPPEntrenador
    {
        private DalGeneral dal;

        public MPPEntrenador()
        {
            dal = new DalGeneral();
        }

        public List<Entrenador> ListarEntrenadores()
        {
            try
            {
                string consulta = @"
                    SELECT
                        e.dni,
                        e.nombre,
                        e.apellido,
                        e.fechaNacimiento,
                        e.usr,
                        ISNULL(u.USUARIO_Activo, 0) AS activo,
                        ISNULL((SELECT COUNT(*) FROM [GymApp].[dbo].[Rutinas] r WHERE r.dniEntrenador = e.dni), 0) AS alumnosCount,
                        e.dvv,
                        e.dvh
                    FROM [GymApp].[dbo].[Entrenadores] e
                    LEFT JOIN [GymApp].[dbo].[USUARIOS] u ON e.usr = u.USUARIO_Usuario
                    ORDER BY e.apellido, e.nombre";

                ArrayList parametros = new ArrayList();

                DataTable dt = dal._686DPConsultar(consulta, parametros);
                List<Entrenador> entrenadores = new List<Entrenador>();

                foreach (DataRow row in dt.Rows)
                {
                    entrenadores.Add(new Entrenador(
                        Convert.ToInt32(row["dni"]),
                        row["nombre"].ToString(),
                        row["apellido"].ToString(),
                        Convert.ToDateTime(row["fechaNacimiento"]),
                        row["usr"] != DBNull.Value ? row["usr"].ToString() : string.Empty,
                        Convert.ToBoolean(row["activo"]),
                        row["alumnosCount"] != DBNull.Value ? Convert.ToInt32(row["alumnosCount"]) : 0
                    ));
                }

                return entrenadores;
            }
            catch (Exception ex)
            {
                throw new Exception("Error al listar entrenadores: " + ex.Message, ex);
            }
        }

        public void CrearEntrenador(Entrenador entrenador)
        {
            try
            {
                string consulta = @"
                    INSERT INTO [GymApp].[dbo].[Entrenadores]
                    (dni, nombre, apellido, fechaNacimiento, activo, usr, dvv, dvh)
                    VALUES
                    (@DNI, @Nombre, @Apellido, @FechaNacimiento, @Activo, @Usuario, '', '')";

                ArrayList parametros = new ArrayList
                {
                    new SqlParameter("@DNI", entrenador.DNI),
                    new SqlParameter("@Nombre", entrenador.Nombre),
                    new SqlParameter("@Apellido", entrenador.Apellido),
                    new SqlParameter("@FechaNacimiento", entrenador.FechaNacimiento),
                    new SqlParameter("@Activo", entrenador.Activo),
                    new SqlParameter("@Usuario", entrenador.Usuario ?? (object)DBNull.Value)
                };

                dal._686DPEscribir(consulta, parametros);
            }
            catch (Exception ex)
            {
                throw new Exception("Error al crear entrenador: " + ex.Message, ex);
            }
        }

        public bool EntrenadorExiste(int dni)
        {
            try
            {
                string consulta = @"
                    SELECT COUNT(*)
                    FROM [GymApp].[dbo].[Entrenadores]
                    WHERE dni = @DNI";

                ArrayList parametros = new ArrayList
                {
                    new SqlParameter("@DNI", dni)
                };

                object resultado = dal._686DPEscalar(consulta, parametros);

                if (resultado != null && resultado != DBNull.Value)
                {
                    return Convert.ToInt32(resultado) > 0;
                }

                return false;
            }
            catch (Exception ex)
            {
                throw new Exception("Error al verificar si existe el entrenador: " + ex.Message, ex);
            }
        }

        public Entrenador ObtenerEntrenador(int dni)
        {
            try
            {
                string consulta = @"
                    SELECT
                        e.dni,
                        e.nombre,
                        e.apellido,
                        e.fechaNacimiento,
                        e.usr,
                        ISNULL(u.USUARIO_Activo, 0) AS activo,
                        ISNULL((SELECT COUNT(*) FROM [GymApp].[dbo].[Rutinas] r WHERE r.dniEntrenador = e.dni), 0) AS alumnosCount,
                        e.dvv,
                        e.dvh
                    FROM [GymApp].[dbo].[Entrenadores] e
                    LEFT JOIN [GymApp].[dbo].[USUARIOS] u ON e.usr = u.USUARIO_Usuario
                    WHERE e.dni = @DNI";

                ArrayList parametros = new ArrayList
                {
                    new SqlParameter("@DNI", dni)
                };

                DataTable dt = dal._686DPConsultar(consulta, parametros);

                if (dt.Rows.Count > 0)
                {
                    DataRow row = dt.Rows[0];
                    return new Entrenador(
                        Convert.ToInt32(row["dni"]),
                        row["nombre"].ToString(),
                        row["apellido"].ToString(),
                        Convert.ToDateTime(row["fechaNacimiento"]),
                        row["usr"] != DBNull.Value ? row["usr"].ToString() : string.Empty,
                        Convert.ToBoolean(row["activo"]),
                        row["alumnosCount"] != DBNull.Value ? Convert.ToInt32(row["alumnosCount"]) : 0
                    );
                }

                return null;
            }
            catch (Exception ex)
            {
                throw new Exception("Error al obtener entrenador: " + ex.Message, ex);
            }
        }

        public void ActualizarEntrenador(Entrenador entrenador)
        {
            try
            {
                string consulta = @"
                    UPDATE [GymApp].[dbo].[Entrenadores]
                    SET nombre = @Nombre,
                        apellido = @Apellido,
                        fechaNacimiento = @FechaNacimiento,
                        usr = @Usuario
                    WHERE dni = @DNI";

                ArrayList parametros = new ArrayList
                {
                    new SqlParameter("@DNI", entrenador.DNI),
                    new SqlParameter("@Nombre", entrenador.Nombre),
                    new SqlParameter("@Apellido", entrenador.Apellido),
                    new SqlParameter("@FechaNacimiento", entrenador.FechaNacimiento),
                    new SqlParameter("@Usuario", entrenador.Usuario ?? (object)DBNull.Value)
                };

                dal._686DPEscribir(consulta, parametros);
            }
            catch (Exception ex)
            {
                throw new Exception("Error al actualizar entrenador: " + ex.Message, ex);
            }
        }

        public void EliminarEntrenador(int dni)
        {
            try
            {
                // Note: Using direct SqlConnection for transaction support
                // because DalGeneral doesn't provide transaction capability
                string connectionString = ConfigurationManager.ConnectionStrings["GymAppConnection"].ConnectionString;

                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open();

                    using (SqlTransaction transaction = connection.BeginTransaction())
                    {
                        try
                        {
                            // Delete from Actividad_Entrenador first
                            string deleteActividadEntrenador = @"
                                DELETE FROM [GymApp].[dbo].[Actividad_Entrenador]
                                WHERE dniEntrenador = @DNI";

                            using (SqlCommand cmd = new SqlCommand(deleteActividadEntrenador, connection, transaction))
                            {
                                cmd.Parameters.AddWithValue("@DNI", dni);
                                cmd.ExecuteNonQuery();
                            }

                            // Delete from Rutinas
                            string deleteRutinas = @"
                                DELETE FROM [GymApp].[dbo].[Rutinas]
                                WHERE dniEntrenador = @DNI";

                            using (SqlCommand cmd = new SqlCommand(deleteRutinas, connection, transaction))
                            {
                                cmd.Parameters.AddWithValue("@DNI", dni);
                                cmd.ExecuteNonQuery();
                            }

                            // Delete the Entrenador record
                            string deleteEntrenador = @"
                                DELETE FROM [GymApp].[dbo].[Entrenadores]
                                WHERE dni = @DNI";

                            using (SqlCommand cmd = new SqlCommand(deleteEntrenador, connection, transaction))
                            {
                                cmd.Parameters.AddWithValue("@DNI", dni);
                                cmd.ExecuteNonQuery();
                            }

                            transaction.Commit();
                        }
                        catch (Exception)
                        {
                            transaction.Rollback();
                            throw;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Error al eliminar entrenador: " + ex.Message, ex);
            }
        }

        public Dictionary<string, int> ObtenerEstadisticas()
        {
            try
            {
                string consulta = @"
                    SELECT
                        COUNT(*) as Total,
                        SUM(CASE WHEN u.USUARIO_Activo = 1 THEN 1 ELSE 0 END) as Activos,
                        SUM(CASE WHEN EXISTS (SELECT 1 FROM [GymApp].[dbo].[Rutinas] r WHERE r.dniEntrenador = e.dni) THEN 1 ELSE 0 END) as ConAlumnos,
                        SUM(CASE WHEN e.usr IS NULL OR e.usr = '' THEN 1 ELSE 0 END) as SinUsuario
                    FROM [GymApp].[dbo].[Entrenadores] e
                    LEFT JOIN [GymApp].[dbo].[USUARIOS] u ON e.usr = u.USUARIO_Usuario";

                ArrayList parametros = new ArrayList();

                DataTable dt = dal._686DPConsultar(consulta, parametros);

                if (dt.Rows.Count > 0)
                {
                    DataRow row = dt.Rows[0];
                    return new Dictionary<string, int>
                    {
                        { "Total", Convert.IsDBNull(row["Total"]) ? 0 : Convert.ToInt32(row["Total"]) },
                        { "Activos", Convert.IsDBNull(row["Activos"]) ? 0 : Convert.ToInt32(row["Activos"]) },
                        { "ConAlumnos", Convert.IsDBNull(row["ConAlumnos"]) ? 0 : Convert.ToInt32(row["ConAlumnos"]) },
                        { "SinUsuario", Convert.IsDBNull(row["SinUsuario"]) ? 0 : Convert.ToInt32(row["SinUsuario"]) }
                    };
                }

                return new Dictionary<string, int>
                {
                    { "Total", 0 },
                    { "Activos", 0 },
                    { "ConAlumnos", 0 },
                    { "SinUsuario", 0 }
                };
            }
            catch (Exception ex)
            {
                throw new Exception("Error al obtener estadísticas: " + ex.Message, ex);
            }
        }
    }
}