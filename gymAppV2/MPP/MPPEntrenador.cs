using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using BE;
using DAL;
using System.Configuration;
using SERVICIOS;

namespace MPP
{
    public class MPPEntrenador
    {
        private DalGeneral dal;
        private DigitoVerificadorManager dvManager;
        private CriptoManager criptoManager;

        public MPPEntrenador()
        {
            dal = new DalGeneral();
            dvManager = new DigitoVerificadorManager();
            criptoManager = new CriptoManager();
        }

        /// <summary>
        /// Pobla los datos personales de visualización (Nombre, Apellido, Teléfono,
        /// FechaNacimiento) leídos vía JOIN con USUARIOS, desencriptándolos. Esos campos
        /// se guardan encriptados desde MPPUsuario, por eso no se pueden leer crudos.
        /// </summary>
        private void PoblarDatosPersonales(Entrenador entrenador, DataRow row)
        {
            entrenador.Nombre = row["nombre"] != DBNull.Value ? criptoManager.DesencriptarCampoPersonal(row["nombre"].ToString()) : null;
            entrenador.Apellido = row["apellido"] != DBNull.Value ? criptoManager.DesencriptarCampoPersonal(row["apellido"].ToString()) : null;
            entrenador.Telefono = row["telefono"] != DBNull.Value ? criptoManager.DesencriptarCampoPersonal(row["telefono"].ToString()) : null;
            entrenador.FechaNacimiento = criptoManager.DesencriptarFechaPersonal(row["fechaNacimiento"]);
        }

        /// <summary>
        /// Calcula DVH y DVV de un entrenador a partir de sus valores de persistencia.
        /// </summary>
        private string CalcularDigitosEntrenador(Entrenador entrenador)
        {
            var valores = new Dictionary<string, object>
            {
                { "dni", entrenador.DNI },
                { "alumnosCount", entrenador.AlumnosCount },
                { "activo", entrenador.Activo },
                { "usr", entrenador.Usuario }
            };

            return dvManager.CalcularDVH(valores);
        }

        public List<Entrenador> ListarEntrenadores()
        {
            try
            {
                // En esquema normalizado, los datos personales están en USUARIOS
                string consulta = @"
                    SELECT
                        e.dni,
                        e.activo,
                        e.alumnosCount,
                        e.usr,
                        e.dvh,
                        u.nombre,
                        u.apellido,
                        u.telefono,
                        u.fechaNacimiento
                    FROM [GymApp].[dbo].[Entrenadores] e
                    LEFT JOIN [GymApp].[dbo].[USUARIOS] u ON e.dni = u.dni
                    ORDER BY u.apellido, u.nombre";

                List<SqlParameter> parametros = new List<SqlParameter>();

                DataTable dt = dal._686DPConsultar(consulta, parametros);
                List<Entrenador> entrenadores = new List<Entrenador>();

                foreach (DataRow row in dt.Rows)
                {
                    Entrenador entrenador = new Entrenador(
                        Convert.ToInt32(row["dni"]),
                        row["alumnosCount"] != DBNull.Value ? Convert.ToInt32(row["alumnosCount"]) : 0,
                        Convert.ToBoolean(row["activo"]),
                        row["dvh"] != DBNull.Value ? row["dvh"].ToString() : string.Empty,
                        row["usr"] != DBNull.Value ? row["usr"].ToString() : string.Empty
                    );
                    // Poblar datos personales desde USUARIOS (para visualización)
                    PoblarDatosPersonales(entrenador, row);
                    entrenadores.Add(entrenador);
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
                string dvh = CalcularDigitosEntrenador(entrenador);

                string consulta = @"
                    INSERT INTO [GymApp].[dbo].[Entrenadores]
                    (dni, alumnosCount, activo, usr, dvh)
                    VALUES
                    (@DNI, @AlumnosCount, @Activo, @Usuario, @DVH)";

                List<SqlParameter> parametros = new List<SqlParameter>
                {
                    new SqlParameter("@DNI", entrenador.DNI),
                    new SqlParameter("@AlumnosCount", entrenador.AlumnosCount),
                    new SqlParameter("@Activo", entrenador.Activo),
                    new SqlParameter("@Usuario", entrenador.Usuario ?? (object)DBNull.Value),
                    new SqlParameter("@DVH", dvh)
                };

                dal._686DPEscribir(consulta, parametros);
                MPPDigitoVerificador.SincronizarControl("ENTRENADORES");
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

                List<SqlParameter> parametros = new List<SqlParameter>
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
                // En esquema normalizado, los datos personales están en USUARIOS
                string consulta = @"
                    SELECT
                        e.dni,
                        e.activo,
                        e.alumnosCount,
                        e.usr,
                        e.dvh,
                        u.nombre,
                        u.apellido,
                        u.telefono,
                        u.fechaNacimiento
                    FROM [GymApp].[dbo].[Entrenadores] e
                    LEFT JOIN [GymApp].[dbo].[USUARIOS] u ON e.dni = u.dni
                    WHERE e.dni = @DNI";

                List<SqlParameter> parametros = new List<SqlParameter>
                {
                    new SqlParameter("@DNI", dni)
                };

                DataTable dt = dal._686DPConsultar(consulta, parametros);

                if (dt.Rows.Count > 0)
                {
                    DataRow row = dt.Rows[0];
                    Entrenador entrenador = new Entrenador(
                        Convert.ToInt32(row["dni"]),
                        row["alumnosCount"] != DBNull.Value ? Convert.ToInt32(row["alumnosCount"]) : 0,
                        Convert.ToBoolean(row["activo"]),
                        row["dvh"] != DBNull.Value ? row["dvh"].ToString() : string.Empty,
                        row["usr"] != DBNull.Value ? row["usr"].ToString() : string.Empty
                    );
                    // Poblar datos personales desde USUARIOS (para visualización)
                    PoblarDatosPersonales(entrenador, row);
                    return entrenador;
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
                string dvh = CalcularDigitosEntrenador(entrenador);

                string consulta = @"
                    UPDATE [GymApp].[dbo].[Entrenadores]
                    SET alumnosCount = @AlumnosCount,
                        activo = @Activo,
                        usr = @Usuario,
                        dvh = @DVH
                    WHERE dni = @DNI";

                List<SqlParameter> parametros = new List<SqlParameter>
                {
                    new SqlParameter("@DNI", entrenador.DNI),
                    new SqlParameter("@AlumnosCount", entrenador.AlumnosCount),
                    new SqlParameter("@Activo", entrenador.Activo),
                    new SqlParameter("@Usuario", entrenador.Usuario ?? (object)DBNull.Value),
                    new SqlParameter("@DVH", dvh)
                };

                dal._686DPEscribir(consulta, parametros);
                MPPDigitoVerificador.SincronizarControl("ENTRENADORES");
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

                var turnosSinAuxiliar = new List<int>();

                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open();

                    using (SqlTransaction transaction = connection.BeginTransaction())
                    {
                        try
                        {
                            // Delete from Actividad_Entrenador first (tabla vieja, ya sin uso)
                            string deleteActividadEntrenador = @"
                                DELETE FROM [GymApp].[dbo].[Actividad_Entrenador]
                                WHERE dniEntrenador = @DNI";

                            using (SqlCommand cmd = new SqlCommand(deleteActividadEntrenador, connection, transaction))
                            {
                                cmd.Parameters.AddWithValue("@DNI", dni);
                                cmd.ExecuteNonQuery();
                            }

                            // Turnos donde era auxiliar: quedan sin auxiliar (si es titular, BLL ya bloqueó la baja)
                            string quitarAuxiliar = @"
                                UPDATE [GymApp].[dbo].[ActividadHorario]
                                SET dniAuxiliar = NULL
                                OUTPUT inserted.codHorario
                                WHERE dniAuxiliar = @DNI";

                            using (SqlCommand cmd = new SqlCommand(quitarAuxiliar, connection, transaction))
                            {
                                cmd.Parameters.AddWithValue("@DNI", dni);
                                using (SqlDataReader reader = cmd.ExecuteReader())
                                {
                                    while (reader.Read())
                                        turnosSinAuxiliar.Add(reader.GetInt32(0));
                                }
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
                            MPPDigitoVerificador.SincronizarControl("Actividad_Entrenador", "Rutinas", "ENTRENADORES");
                            foreach (int codHorario in turnosSinAuxiliar)
                                MPPDigitoVerificador.RecalcularDvhFilas("ActividadHorario", "codHorario = @Horario", new SqlParameter("@Horario", codHorario));
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
                // En esquema normalizado, la FK es por dni
                string consulta = @"
                    SELECT
                        COUNT(*) as Total,
                        SUM(CASE WHEN u.activo = 1 THEN 1 ELSE 0 END) as Activos,
                        SUM(CASE WHEN EXISTS (SELECT 1 FROM [GymApp].[dbo].[Rutinas] r WHERE r.dniEntrenador = e.dni) THEN 1 ELSE 0 END) as ConAlumnos,
                        SUM(CASE WHEN e.usr IS NULL OR e.usr = '' THEN 1 ELSE 0 END) as SinUsuario
                    FROM [GymApp].[dbo].[Entrenadores] e
                    LEFT JOIN [GymApp].[dbo].[USUARIOS] u ON e.dni = u.dni";

                List<SqlParameter> parametros = new List<SqlParameter>();

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