using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;

namespace DAL
{
    public class DalTraduccion
    {
        // Tablas del esquema Traducciones que se incluyen en el diccionario global.
        // El orden no importa; al agregar una nueva pantalla basta con añadirla aquí.
        public static readonly string[] TABLAS = {
            "Pantalla_Login",
            "Pantalla_Idioma",
            "Pantalla_DashBoard",
            "Pantalla_DashboardContent",
            "Pantalla_Usuarios",
            "Pantalla_Alumnos",
            "Pantalla_Actividades",
            "Pantalla_Rutinas",
            "Pantalla_CambiarContra",
            "Comunes_Botones",
            "Comunes_Mensajes",
        };

        private DalGeneral dal;

        public DalTraduccion()
        {
            dal = new DalGeneral();
        }

        /// <summary>
        /// Devuelve un diccionario tag → texto leyendo todas las tablas del esquema
        /// Traducciones para el IdiomaID dado.
        /// Los nombres de columna (excepto TraduccionID e IdiomaID) se usan como tags.
        /// Las tablas vienen de una lista estática; no hay riesgo de inyección SQL.
        /// </summary>
        public Dictionary<string, string> ObtenerDiccionario(int idiomaId)
        {
            var resultado = new Dictionary<string, string>();
            var parametros = new List<SqlParameter>
            {
                new SqlParameter("@IdiomaID", SqlDbType.Int) { Value = idiomaId }
            };

            foreach (string tabla in TABLAS)
            {
                string sql = $"SELECT * FROM [Traducciones].[{tabla}] WHERE [IdiomaID] = @IdiomaID";
                DataTable dt = dal._686DPConsultar(sql, parametros);
                if (dt.Rows.Count == 0) continue;

                DataRow fila = dt.Rows[0];
                foreach (DataColumn col in dt.Columns)
                {
                    if (col.ColumnName == "TraduccionID" || col.ColumnName == "IdiomaID") continue;
                    resultado[col.ColumnName] = fila[col]?.ToString() ?? string.Empty;
                }
            }

            return resultado;
        }
    }
}
