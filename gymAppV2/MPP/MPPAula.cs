using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using BE;
using DAL;

namespace MPP
{
    /// <summary>
    /// Acceso a datos de las aulas del gimnasio (tabla Aulas).
    /// Las escrituras recalculan el dvh desde la fila real (MPPDigitoVerificador.RecalcularDvhFilas).
    /// </summary>
    public class MPPAula
    {
        private DalGeneral dal;

        public MPPAula()
        {
            dal = new DalGeneral();
        }

        public List<Aula> ListarAulas(bool soloActivas)
        {
            try
            {
                DataTable dt = dal._686DPConsultar(@"
                    SELECT codAula, nombre, cupo, activo, dvh
                    FROM [GymApp].[dbo].[Aulas]" + (soloActivas ? " WHERE activo = 1" : string.Empty) + @"
                    ORDER BY activo DESC, nombre",
                    new List<SqlParameter>());

                return Mapear(dt);
            }
            catch (Exception ex)
            {
                throw new Exception("Error al listar las aulas: " + ex.Message, ex);
            }
        }

        public Aula ObtenerAula(int codAula)
        {
            try
            {
                DataTable dt = dal._686DPConsultar(@"
                    SELECT codAula, nombre, cupo, activo, dvh
                    FROM [GymApp].[dbo].[Aulas]
                    WHERE codAula = @Cod",
                    new List<SqlParameter> { new SqlParameter("@Cod", codAula) });

                return Mapear(dt).FirstOrDefault();
            }
            catch (Exception ex)
            {
                throw new Exception("Error al obtener el aula: " + ex.Message, ex);
            }
        }

        public bool ExisteNombre(string nombre, int codAulaExcluir)
        {
            try
            {
                object resultado = dal._686DPEscalar(@"
                    SELECT COUNT(*)
                    FROM [GymApp].[dbo].[Aulas]
                    WHERE nombre = @Nombre AND codAula <> @Cod",
                    new List<SqlParameter>
                    {
                        new SqlParameter("@Nombre", nombre),
                        new SqlParameter("@Cod", codAulaExcluir)
                    });

                return resultado != null && resultado != DBNull.Value && Convert.ToInt32(resultado) > 0;
            }
            catch (Exception ex)
            {
                throw new Exception("Error al verificar el nombre del aula: " + ex.Message, ex);
            }
        }

        public int CrearAula(Aula aula)
        {
            try
            {
                object resultado = dal._686DPEscalar(@"
                    INSERT INTO [GymApp].[dbo].[Aulas] (nombre, cupo, activo, dvh)
                    VALUES (@Nombre, @Cupo, @Activo, '');

                    SELECT CAST(SCOPE_IDENTITY() AS INT);",
                    new List<SqlParameter>
                    {
                        new SqlParameter("@Nombre", aula.Nombre),
                        new SqlParameter("@Cupo", aula.Cupo),
                        new SqlParameter("@Activo", aula.Activo)
                    });

                int codAula = Convert.ToInt32(resultado);
                MPPDigitoVerificador.RecalcularDvhFilas("Aulas", "codAula = @Cod", new SqlParameter("@Cod", codAula));
                return codAula;
            }
            catch (Exception ex)
            {
                throw new Exception("Error al crear el aula: " + ex.Message, ex);
            }
        }

        public void ActualizarAula(Aula aula)
        {
            try
            {
                dal._686DPEscribir(@"
                    UPDATE [GymApp].[dbo].[Aulas]
                    SET nombre = @Nombre, cupo = @Cupo, activo = @Activo
                    WHERE codAula = @Cod",
                    new List<SqlParameter>
                    {
                        new SqlParameter("@Cod", aula.CodAula),
                        new SqlParameter("@Nombre", aula.Nombre),
                        new SqlParameter("@Cupo", aula.Cupo),
                        new SqlParameter("@Activo", aula.Activo)
                    });

                MPPDigitoVerificador.RecalcularDvhFilas("Aulas", "codAula = @Cod", new SqlParameter("@Cod", aula.CodAula));
            }
            catch (Exception ex)
            {
                throw new Exception("Error al actualizar el aula: " + ex.Message, ex);
            }
        }

        private static List<Aula> Mapear(DataTable dt)
        {
            return dt.Rows.Cast<DataRow>().Select(row => new Aula
            {
                CodAula = Convert.ToInt32(row["codAula"]),
                Nombre = row["nombre"].ToString(),
                Cupo = Convert.ToInt32(row["cupo"]),
                Activo = Convert.ToBoolean(row["activo"]),
                DVH = row["dvh"] != DBNull.Value ? row["dvh"].ToString() : string.Empty
            }).ToList();
        }
    }
}
