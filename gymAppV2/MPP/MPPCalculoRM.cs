using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using BE;
using DAL;

namespace MPP
{
    /// <summary>
    /// Acceso a datos del cálculo de RM: catálogo de ejercicios (tabla Ejercicio) e historial
    /// de 1RM de cada alumno (tabla AlumnoRM).
    /// </summary>
    public class MPPCalculoRM
    {
        private DalGeneral dal;

        public MPPCalculoRM()
        {
            dal = new DalGeneral();
        }

        private Ejercicio MapearEjercicio(DataRow row)
        {
            return new Ejercicio
            {
                CodEjercicio = Convert.ToInt32(row["codEjercicio"]),
                Nombre = row["nombre"].ToString(),
                GrupoMuscular = row["grupoMuscular"] != DBNull.Value ? row["grupoMuscular"].ToString() : string.Empty,
                Descripcion = row["descripcion"] != DBNull.Value ? row["descripcion"].ToString() : null,
                DVH = row["dvh"] != DBNull.Value ? row["dvh"].ToString() : string.Empty
            };
        }

        private RegistroRM MapearRegistro(DataRow row)
        {
            return new RegistroRM
            {
                CodRM = Convert.ToInt32(row["codRM"]),
                Dni = Convert.ToInt32(row["dni"]),
                CodEjercicio = Convert.ToInt32(row["codEjercicio"]),
                PesoRM = Convert.ToDecimal(row["pesoRM"]),
                FechaRM = Convert.ToDateTime(row["fechaRM"]),
                PesoLevantado = row["pesoLevantado"] != DBNull.Value ? Convert.ToDecimal(row["pesoLevantado"]) : (decimal?)null,
                Repeticiones = row["repeticiones"] != DBNull.Value ? Convert.ToInt32(row["repeticiones"]) : (int?)null,
                UsuarioRegistro = row["usuarioRegistro"] != DBNull.Value ? row["usuarioRegistro"].ToString() : null,
                DVH = row["dvh"] != DBNull.Value ? row["dvh"].ToString() : string.Empty,
                EjercicioNombre = row["ejercicioNombre"].ToString(),
                GrupoMuscular = row["grupoMuscular"] != DBNull.Value ? row["grupoMuscular"].ToString() : string.Empty
            };
        }

        public List<Ejercicio> ListarEjercicios()
        {
            try
            {
                string consulta = @"
                    SELECT codEjercicio, nombre, grupoMuscular, descripcion, dvh
                    FROM [GymApp].[dbo].[Ejercicio]
                    ORDER BY nombre";

                DataTable dt = dal._686DPConsultar(consulta, new List<SqlParameter>());
                List<Ejercicio> ejercicios = new List<Ejercicio>();

                foreach (DataRow row in dt.Rows)
                {
                    ejercicios.Add(MapearEjercicio(row));
                }

                return ejercicios;
            }
            catch (Exception ex)
            {
                throw new Exception("Error al listar ejercicios: " + ex.Message, ex);
            }
        }

        public Ejercicio ObtenerEjercicio(int codEjercicio)
        {
            try
            {
                string consulta = @"
                    SELECT codEjercicio, nombre, grupoMuscular, descripcion, dvh
                    FROM [GymApp].[dbo].[Ejercicio]
                    WHERE codEjercicio = @CodEjercicio";

                List<SqlParameter> parametros = new List<SqlParameter>
                {
                    new SqlParameter("@CodEjercicio", codEjercicio)
                };

                DataTable dt = dal._686DPConsultar(consulta, parametros);
                return dt.Rows.Count > 0 ? MapearEjercicio(dt.Rows[0]) : null;
            }
            catch (Exception ex)
            {
                throw new Exception("Error al obtener ejercicio: " + ex.Message, ex);
            }
        }

        /// <summary>
        /// Historial de 1RM de un alumno, del más reciente al más viejo.
        /// </summary>
        public List<RegistroRM> ListarRegistrosPorAlumno(int dni)
        {
            try
            {
                string consulta = @"
                    SELECT rm.codRM, rm.dni, rm.codEjercicio, rm.pesoRM, rm.fechaRM,
                           rm.pesoLevantado, rm.repeticiones, rm.usuarioRegistro, rm.dvh,
                           e.nombre AS ejercicioNombre, e.grupoMuscular
                    FROM [GymApp].[dbo].[AlumnoRM] rm
                    INNER JOIN [GymApp].[dbo].[Ejercicio] e ON rm.codEjercicio = e.codEjercicio
                    WHERE rm.dni = @Dni
                    ORDER BY rm.fechaRM DESC, rm.codRM DESC";

                List<SqlParameter> parametros = new List<SqlParameter>
                {
                    new SqlParameter("@Dni", dni)
                };

                DataTable dt = dal._686DPConsultar(consulta, parametros);
                List<RegistroRM> registros = new List<RegistroRM>();

                foreach (DataRow row in dt.Rows)
                {
                    registros.Add(MapearRegistro(row));
                }

                return registros;
            }
            catch (Exception ex)
            {
                throw new Exception("Error al listar el historial de 1RM: " + ex.Message, ex);
            }
        }

        /// <summary>
        /// Guarda un 1RM. Se inserta con dvh vacío y se calcula después con la fila real
        /// (codRM es IDENTITY y no se conoce antes del INSERT), igual que Pagos.
        /// </summary>
        public int CrearRegistro(RegistroRM registro)
        {
            try
            {
                string consulta = @"
                    INSERT INTO [GymApp].[dbo].[AlumnoRM]
                    (dni, codEjercicio, pesoRM, fechaRM, pesoLevantado, repeticiones, usuarioRegistro, dvh)
                    VALUES
                    (@Dni, @CodEjercicio, @PesoRM, @FechaRM, @PesoLevantado, @Repeticiones, @UsuarioRegistro, '');

                    SELECT CAST(SCOPE_IDENTITY() AS INT);";

                List<SqlParameter> parametros = new List<SqlParameter>
                {
                    new SqlParameter("@Dni", registro.Dni),
                    new SqlParameter("@CodEjercicio", registro.CodEjercicio),
                    new SqlParameter("@PesoRM", registro.PesoRM),
                    new SqlParameter("@FechaRM", registro.FechaRM.Date),
                    new SqlParameter("@PesoLevantado", (object)registro.PesoLevantado ?? DBNull.Value),
                    new SqlParameter("@Repeticiones", (object)registro.Repeticiones ?? DBNull.Value),
                    new SqlParameter("@UsuarioRegistro", (object)registro.UsuarioRegistro ?? DBNull.Value)
                };

                object resultado = dal._686DPEscalar(consulta, parametros);
                int codRM = (resultado != null && resultado != DBNull.Value) ? Convert.ToInt32(resultado) : 0;

                MPPDigitoVerificador.RecalcularDvhFilas("AlumnoRM", "codRM = @Cod", new SqlParameter("@Cod", codRM));
                return codRM;
            }
            catch (Exception ex)
            {
                throw new Exception("Error al guardar el 1RM: " + ex.Message, ex);
            }
        }
    }
}
