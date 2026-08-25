using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Linq;

namespace DAL
{
    public class DalGestionIdiomas
    {
        private readonly DalGeneral dal = new DalGeneral();

        // Devuelve DataTable con columnas IdiomaID, Codigo, NombreIdioma.
        // La conversión a IdiomaInfo ocurre en MPP para respetar la separación de capas.
        public DataTable ObtenerIdiomas()
        {
            return dal._686DPConsultar(
                "SELECT [IdiomaID],[Codigo],[NombreIdioma] FROM [Traducciones].[Idiomas] ORDER BY [IdiomaID]",
                null);
        }

        public bool ExisteCodigoONombre(string codigo, string nombre)
        {
            var p = new List<SqlParameter> {
                new SqlParameter("@c", SqlDbType.NVarChar) { Value = codigo },
                new SqlParameter("@n", SqlDbType.NVarChar) { Value = nombre }
            };
            object r = dal._686DPEscalar(
                "SELECT COUNT(*) FROM [Traducciones].[Idiomas] WHERE [Codigo]=@c OR [NombreIdioma]=@n", p);
            return Convert.ToInt32(r) > 0;
        }

        // Devuelve {tableName: [col1, col2, ...]} consultando INFORMATION_SCHEMA.
        // Los nombres de tabla vienen de DalTraduccion.TABLAS (estático), no del usuario.
        public Dictionary<string, List<string>> ObtenerTodasLasColumnas()
        {
            var tablas = string.Join("','", DalTraduccion.TABLAS);
            string sql = "SELECT TABLE_NAME, COLUMN_NAME FROM INFORMATION_SCHEMA.COLUMNS " +
                         $"WHERE TABLE_SCHEMA='Traducciones' AND TABLE_NAME IN ('{tablas}') " +
                         "AND COLUMN_NAME NOT IN ('TraduccionID','IdiomaID') " +
                         "ORDER BY TABLE_NAME, ORDINAL_POSITION";
            var dt     = dal._686DPConsultar(sql, null);
            var result = new Dictionary<string, List<string>>();
            foreach (DataRow row in dt.Rows)
            {
                string tabla = row["TABLE_NAME"].ToString();
                string col   = row["COLUMN_NAME"].ToString();
                if (!result.ContainsKey(tabla))
                    result[tabla] = new List<string>();
                result[tabla].Add(col);
            }
            return result;
        }

        // Crea idioma + filas en todas las tablas de traducciones dentro de una transacción.
        // Devuelve el nuevo IdiomaID.
        public int CrearIdiomaConTraducciones(
            string codigo,
            string nombre,
            Dictionary<string, Dictionary<string, string>> valoresPorTabla)
        {
            string connStr = ConfigurationManager.ConnectionStrings["GymAppConnection"].ConnectionString;
            using (var conn = new SqlConnection(connStr))
            {
                conn.Open();
                using (var tx = conn.BeginTransaction())
                {
                    try
                    {
                        int newId;
                        using (var cmd = new SqlCommand(
                            "INSERT INTO [Traducciones].[Idiomas]([Codigo],[NombreIdioma]) " +
                            "VALUES(@c,@n); SELECT SCOPE_IDENTITY();", conn, tx))
                        {
                            cmd.Parameters.AddWithValue("@c", codigo);
                            cmd.Parameters.AddWithValue("@n", nombre);
                            newId = Convert.ToInt32(cmd.ExecuteScalar());
                        }

                        foreach (string tabla in DalTraduccion.TABLAS)
                        {
                            if (!valoresPorTabla.TryGetValue(tabla, out var vals) || vals.Count == 0)
                                continue;

                            var cols      = vals.Keys.ToList();
                            var colList   = string.Join(", ", cols.Select(c => $"[{c}]"));
                            var paramList = string.Join(", ", Enumerable.Range(0, cols.Count).Select(i => $"@p{i}"));

                            using (var cmd = new SqlCommand(
                                $"INSERT INTO [Traducciones].[{tabla}] ([IdiomaID],{colList}) " +
                                $"VALUES(@idiomaId,{paramList})", conn, tx))
                            {
                                cmd.Parameters.AddWithValue("@idiomaId", newId);
                                for (int i = 0; i < cols.Count; i++)
                                    cmd.Parameters.AddWithValue($"@p{i}", vals[cols[i]]);
                                cmd.ExecuteNonQuery();
                            }
                        }

                        tx.Commit();
                        return newId;
                    }
                    catch
                    {
                        tx.Rollback();
                        throw;
                    }
                }
            }
        }

        // Actualiza todas las tablas de traducciones para un idioma existente.
        public void ActualizarTraducciones(
            int idiomaId,
            Dictionary<string, Dictionary<string, string>> valoresPorTabla)
        {
            string connStr = ConfigurationManager.ConnectionStrings["GymAppConnection"].ConnectionString;
            using (var conn = new SqlConnection(connStr))
            {
                conn.Open();
                using (var tx = conn.BeginTransaction())
                {
                    try
                    {
                        foreach (string tabla in DalTraduccion.TABLAS)
                        {
                            if (!valoresPorTabla.TryGetValue(tabla, out var vals) || vals.Count == 0)
                                continue;

                            var cols    = vals.Keys.ToList();
                            var setList = string.Join(", ",
                                Enumerable.Range(0, cols.Count).Select(i => $"[{cols[i]}]=@p{i}"));

                            using (var cmd = new SqlCommand(
                                $"UPDATE [Traducciones].[{tabla}] SET {setList} WHERE [IdiomaID]=@idiomaId",
                                conn, tx))
                            {
                                cmd.Parameters.AddWithValue("@idiomaId", idiomaId);
                                for (int i = 0; i < cols.Count; i++)
                                    cmd.Parameters.AddWithValue($"@p{i}", vals[cols[i]]);
                                cmd.ExecuteNonQuery();
                            }
                        }
                        tx.Commit();
                    }
                    catch
                    {
                        tx.Rollback();
                        throw;
                    }
                }
            }
        }
    }
}
