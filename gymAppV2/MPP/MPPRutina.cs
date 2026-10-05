using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using BE;
using DAL;
using SERVICIOS;

namespace MPP
{
    public class MPPRutina
    {
        private DalGeneral dal;
        private DigitoVerificadorManager dvManager;
        private CriptoManager criptoManager;

        public MPPRutina()
        {
            dal = new DalGeneral();
            dvManager = new DigitoVerificadorManager();
            criptoManager = new CriptoManager();
        }

        private const string SELECT_BASE = @"
            SELECT
                r.codRutina,
                r.descripcion,
                r.fecha,
                r.dniAlumno,
                r.dniEntrenador,
                r.codActividad,
                r.dvh,
                alu_u.nombre AS alumnoNombre,
                alu_u.apellido AS alumnoApellido,
                ent_u.nombre AS entrenadorNombre,
                ent_u.apellido AS entrenadorApellido,
                a.descripcion AS actividadDescripcion
            FROM [GymApp].[dbo].[Rutinas] r
            INNER JOIN [GymApp].[dbo].[Alumnos] al ON r.dniAlumno = al.dni
            LEFT JOIN [GymApp].[dbo].[USUARIOS] alu_u ON al.dni = alu_u.dni
            INNER JOIN [GymApp].[dbo].[Entrenadores] ent ON r.dniEntrenador = ent.dni
            LEFT JOIN [GymApp].[dbo].[USUARIOS] ent_u ON ent.dni = ent_u.dni
            INNER JOIN [GymApp].[dbo].[Actividades] a ON r.codActividad = a.codActividad";

        /// <summary>
        /// Arma el diccionario de valores usado para calcular el DVH. No incluye codRutina
        /// porque es autogenerado por la base de datos y no se conoce en el momento del INSERT.
        /// </summary>
        private Dictionary<string, object> ArmarValoresDV(Rutina rutina)
        {
            return new Dictionary<string, object>
            {
                { "descripcion", rutina.Descripcion },
                { "fecha", rutina.Fecha.Date },
                { "dniAlumno", rutina.DniAlumno },
                { "dniEntrenador", rutina.DniEntrenador },
                { "codActividad", rutina.CodActividad }
            };
        }

        private string CalcularDigitosRutina(Rutina rutina)
        {
            return dvManager.CalcularDVH(ArmarValoresDV(rutina));
        }

        private Rutina MapearFila(DataRow row)
        {
            return new Rutina
            {
                CodRutina = Convert.ToInt32(row["codRutina"]),
                Descripcion = row["descripcion"] != DBNull.Value ? row["descripcion"].ToString() : string.Empty,
                Fecha = Convert.ToDateTime(row["fecha"]),
                DniAlumno = Convert.ToInt32(row["dniAlumno"]),
                DniEntrenador = Convert.ToInt32(row["dniEntrenador"]),
                CodActividad = Convert.ToInt32(row["codActividad"]),
                DVH = row["dvh"] != DBNull.Value ? row["dvh"].ToString() : string.Empty,
                AlumnoNombre = row["alumnoNombre"] != DBNull.Value ? criptoManager.DesencriptarCampoPersonal(row["alumnoNombre"].ToString()) : null,
                AlumnoApellido = row["alumnoApellido"] != DBNull.Value ? criptoManager.DesencriptarCampoPersonal(row["alumnoApellido"].ToString()) : null,
                EntrenadorNombre = row["entrenadorNombre"] != DBNull.Value ? criptoManager.DesencriptarCampoPersonal(row["entrenadorNombre"].ToString()) : null,
                EntrenadorApellido = row["entrenadorApellido"] != DBNull.Value ? criptoManager.DesencriptarCampoPersonal(row["entrenadorApellido"].ToString()) : null,
                ActividadDescripcion = row["actividadDescripcion"] != DBNull.Value ? row["actividadDescripcion"].ToString() : null
            };
        }

        public List<Rutina> ListarRutinas()
        {
            try
            {
                string consulta = SELECT_BASE + " ORDER BY r.fecha DESC";

                DataTable dt = dal._686DPConsultar(consulta, new List<SqlParameter>());
                List<Rutina> rutinas = new List<Rutina>();

                foreach (DataRow row in dt.Rows)
                {
                    rutinas.Add(MapearFila(row));
                }

                return rutinas;
            }
            catch (Exception ex)
            {
                throw new Exception("Error al listar rutinas: " + ex.Message, ex);
            }
        }

        /// <summary>
        /// Lista las rutinas de los alumnos asociados al usuario Cliente indicado.
        /// </summary>
        public List<Rutina> ListarRutinasPorCliente(string usuario)
        {
            try
            {
                string consulta = SELECT_BASE + " WHERE EXISTS (SELECT 1 FROM [GymApp].[dbo].[ALUMNOS_USUARIOS] au WHERE au.dniAlumno = al.dni AND au.usr = @Usuario) ORDER BY r.fecha DESC";

                List<SqlParameter> parametros = new List<SqlParameter>
                {
                    new SqlParameter("@Usuario", usuario)
                };

                DataTable dt = dal._686DPConsultar(consulta, parametros);
                List<Rutina> rutinas = new List<Rutina>();

                foreach (DataRow row in dt.Rows)
                {
                    rutinas.Add(MapearFila(row));
                }

                return rutinas;
            }
            catch (Exception ex)
            {
                throw new Exception("Error al listar rutinas del cliente: " + ex.Message, ex);
            }
        }

        public Rutina ObtenerRutina(int codRutina)
        {
            try
            {
                string consulta = SELECT_BASE + " WHERE r.codRutina = @CodRutina";

                List<SqlParameter> parametros = new List<SqlParameter>
                {
                    new SqlParameter("@CodRutina", codRutina)
                };

                DataTable dt = dal._686DPConsultar(consulta, parametros);

                return dt.Rows.Count > 0 ? MapearFila(dt.Rows[0]) : null;
            }
            catch (Exception ex)
            {
                throw new Exception("Error al obtener rutina: " + ex.Message, ex);
            }
        }

        public int CrearRutina(Rutina rutina)
        {
            try
            {
                string dvh = CalcularDigitosRutina(rutina);

                string consulta = @"
                    INSERT INTO [GymApp].[dbo].[Rutinas]
                    (descripcion, fecha, dniAlumno, dniEntrenador, codActividad, dvh)
                    VALUES
                    (@Descripcion, @Fecha, @DniAlumno, @DniEntrenador, @CodActividad, @DVH);

                    SELECT CAST(SCOPE_IDENTITY() AS INT);";

                List<SqlParameter> parametros = new List<SqlParameter>
                {
                    new SqlParameter("@Descripcion", rutina.Descripcion),
                    new SqlParameter("@Fecha", rutina.Fecha.Date),
                    new SqlParameter("@DniAlumno", rutina.DniAlumno),
                    new SqlParameter("@DniEntrenador", rutina.DniEntrenador),
                    new SqlParameter("@CodActividad", rutina.CodActividad),
                    new SqlParameter("@DVH", dvh)
                };

                object resultado = dal._686DPEscalar(consulta, parametros);
                MPPDigitoVerificador.SincronizarControl("Rutinas");
                return (resultado != null && resultado != DBNull.Value) ? Convert.ToInt32(resultado) : 0;
            }
            catch (Exception ex)
            {
                throw new Exception("Error al crear rutina: " + ex.Message, ex);
            }
        }

        public void ActualizarRutina(Rutina rutina)
        {
            try
            {
                string dvh = CalcularDigitosRutina(rutina);

                string consulta = @"
                    UPDATE [GymApp].[dbo].[Rutinas]
                    SET descripcion = @Descripcion,
                        fecha = @Fecha,
                        dniAlumno = @DniAlumno,
                        dniEntrenador = @DniEntrenador,
                        codActividad = @CodActividad,
                        dvh = @DVH
                    WHERE codRutina = @CodRutina";

                List<SqlParameter> parametros = new List<SqlParameter>
                {
                    new SqlParameter("@CodRutina", rutina.CodRutina),
                    new SqlParameter("@Descripcion", rutina.Descripcion),
                    new SqlParameter("@Fecha", rutina.Fecha.Date),
                    new SqlParameter("@DniAlumno", rutina.DniAlumno),
                    new SqlParameter("@DniEntrenador", rutina.DniEntrenador),
                    new SqlParameter("@CodActividad", rutina.CodActividad),
                    new SqlParameter("@DVH", dvh)
                };

                dal._686DPEscribir(consulta, parametros);
                MPPDigitoVerificador.SincronizarControl("Rutinas");
            }
            catch (Exception ex)
            {
                throw new Exception("Error al actualizar rutina: " + ex.Message, ex);
            }
        }

        public void EliminarRutina(int codRutina)
        {
            try
            {
                string consulta = @"
                    DELETE FROM [GymApp].[dbo].[Rutinas]
                    WHERE codRutina = @CodRutina";

                List<SqlParameter> parametros = new List<SqlParameter>
                {
                    new SqlParameter("@CodRutina", codRutina)
                };

                dal._686DPEscribir(consulta, parametros);
                MPPDigitoVerificador.SincronizarControl("Rutinas");
            }
            catch (Exception ex)
            {
                throw new Exception("Error al eliminar rutina: " + ex.Message, ex);
            }
        }
    }
}
