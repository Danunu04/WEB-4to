using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using BE;
using DAL;

namespace MPP
{
    /// <summary>
    /// Acceso a datos de los ejercicios de cada rutina (tabla RutinaEjercicio).
    /// </summary>
    public class MPPRutinaEjercicio
    {
        private DalGeneral dal;

        public MPPRutinaEjercicio()
        {
            dal = new DalGeneral();
        }

        public List<RutinaEjercicio> ListarPorRutina(int codRutina)
        {
            try
            {
                string consulta = @"
                    SELECT re.codRutinaEjercicio, re.codRutina, re.codEjercicio, re.orden, re.series,
                           re.repeticiones, re.pesoLevantado, re.descansoSegundos, re.observaciones, re.dvh,
                           e.nombre AS ejercicioNombre, e.grupoMuscular
                    FROM [GymApp].[dbo].[RutinaEjercicio] re
                    INNER JOIN [GymApp].[dbo].[Ejercicio] e ON re.codEjercicio = e.codEjercicio
                    WHERE re.codRutina = @CodRutina
                    ORDER BY re.orden, re.codRutinaEjercicio";

                List<SqlParameter> parametros = new List<SqlParameter>
                {
                    new SqlParameter("@CodRutina", codRutina)
                };

                DataTable dt = dal._686DPConsultar(consulta, parametros);
                List<RutinaEjercicio> ejercicios = new List<RutinaEjercicio>();

                foreach (DataRow row in dt.Rows)
                {
                    ejercicios.Add(new RutinaEjercicio
                    {
                        CodRutinaEjercicio = Convert.ToInt32(row["codRutinaEjercicio"]),
                        CodRutina = Convert.ToInt32(row["codRutina"]),
                        CodEjercicio = Convert.ToInt32(row["codEjercicio"]),
                        Orden = row["orden"] != DBNull.Value ? Convert.ToInt32(row["orden"]) : 0,
                        Series = Convert.ToInt32(row["series"]),
                        Repeticiones = Convert.ToInt32(row["repeticiones"]),
                        PesoLevantado = row["pesoLevantado"] != DBNull.Value ? Convert.ToDecimal(row["pesoLevantado"]) : (decimal?)null,
                        DescansoSegundos = row["descansoSegundos"] != DBNull.Value ? Convert.ToInt32(row["descansoSegundos"]) : (int?)null,
                        Observaciones = row["observaciones"] != DBNull.Value ? row["observaciones"].ToString() : null,
                        DVH = row["dvh"] != DBNull.Value ? row["dvh"].ToString() : string.Empty,
                        EjercicioNombre = row["ejercicioNombre"].ToString(),
                        GrupoMuscular = row["grupoMuscular"] != DBNull.Value ? row["grupoMuscular"].ToString() : string.Empty
                    });
                }

                return ejercicios;
            }
            catch (Exception ex)
            {
                throw new Exception("Error al listar los ejercicios de la rutina: " + ex.Message, ex);
            }
        }

        public RutinaEjercicio Obtener(int codRutinaEjercicio)
        {
            try
            {
                string consulta = @"
                    SELECT codRutinaEjercicio, codRutina
                    FROM [GymApp].[dbo].[RutinaEjercicio]
                    WHERE codRutinaEjercicio = @Cod";

                DataTable dt = dal._686DPConsultar(consulta, new List<SqlParameter> { new SqlParameter("@Cod", codRutinaEjercicio) });
                if (dt.Rows.Count == 0)
                    return null;

                return new RutinaEjercicio
                {
                    CodRutinaEjercicio = Convert.ToInt32(dt.Rows[0]["codRutinaEjercicio"]),
                    CodRutina = Convert.ToInt32(dt.Rows[0]["codRutina"])
                };
            }
            catch (Exception ex)
            {
                throw new Exception("Error al obtener el ejercicio de la rutina: " + ex.Message, ex);
            }
        }

        /// <summary>
        /// Agrega un ejercicio al final de la rutina. Se inserta con dvh vacío y se calcula después
        /// con la fila real (la clave es IDENTITY), igual que Pagos.
        /// </summary>
        public int Crear(RutinaEjercicio ejercicio)
        {
            try
            {
                string consulta = @"
                    INSERT INTO [GymApp].[dbo].[RutinaEjercicio]
                    (codRutina, codEjercicio, orden, series, repeticiones, pesoLevantado, descansoSegundos, observaciones, dvh)
                    VALUES
                    (@CodRutina, @CodEjercicio,
                     (SELECT ISNULL(MAX(orden), 0) + 1 FROM [GymApp].[dbo].[RutinaEjercicio] WHERE codRutina = @CodRutina),
                     @Series, @Repeticiones, @Peso, @Descanso, @Observaciones, '');

                    SELECT CAST(SCOPE_IDENTITY() AS INT);";

                List<SqlParameter> parametros = new List<SqlParameter>
                {
                    new SqlParameter("@CodRutina", ejercicio.CodRutina),
                    new SqlParameter("@CodEjercicio", ejercicio.CodEjercicio),
                    new SqlParameter("@Series", ejercicio.Series),
                    new SqlParameter("@Repeticiones", ejercicio.Repeticiones),
                    new SqlParameter("@Peso", (object)ejercicio.PesoLevantado ?? DBNull.Value),
                    new SqlParameter("@Descanso", (object)ejercicio.DescansoSegundos ?? DBNull.Value),
                    new SqlParameter("@Observaciones", (object)ejercicio.Observaciones ?? DBNull.Value)
                };

                object resultado = dal._686DPEscalar(consulta, parametros);
                int cod = (resultado != null && resultado != DBNull.Value) ? Convert.ToInt32(resultado) : 0;

                MPPDigitoVerificador.RecalcularDvhFilas("RutinaEjercicio", "codRutinaEjercicio = @Cod", new SqlParameter("@Cod", cod));
                return cod;
            }
            catch (Exception ex)
            {
                throw new Exception("Error al agregar el ejercicio a la rutina: " + ex.Message, ex);
            }
        }

        public void Eliminar(int codRutinaEjercicio)
        {
            try
            {
                dal._686DPEscribir(
                    "DELETE FROM [GymApp].[dbo].[RutinaEjercicio] WHERE codRutinaEjercicio = @Cod",
                    new List<SqlParameter> { new SqlParameter("@Cod", codRutinaEjercicio) });
                MPPDigitoVerificador.SincronizarControl("RutinaEjercicio");
            }
            catch (Exception ex)
            {
                throw new Exception("Error al quitar el ejercicio de la rutina: " + ex.Message, ex);
            }
        }
    }
}
