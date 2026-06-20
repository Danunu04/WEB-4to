using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using BE;
using DAL;
using SERVICIOS;

namespace MPP
{
    public class MPPEvento
    {
        private DalGeneral dal;
        private CriptoManager criptoManager;

        public MPPEvento()
        {
            dal = new DalGeneral();
            criptoManager = new CriptoManager();
        }

        public int RegistrarEvento(Evento evento, int criticidad = 1)
        {
            try
            {
                // Validar que el usuario no sea vacío o "sistema"
                // Todos los eventos deben estar atados a un usuario válido
                if (string.IsNullOrEmpty(evento.EVENTO_Usuario) || evento.EVENTO_Usuario == "sistema")
                {
                    throw new Exception("No se puede registrar un evento sin usuario válido");
                }

                string consulta = @"
                    INSERT INTO [GymApp].[dbo].[Evento]
                    (tipo, usr, descripcion, fecha, criticidad, modulo, dvv, dvh)
                    VALUES (@Tipo, @Usuario, @Accion, @Timestamp, @Criticidad, @Modulo, @DVV, @DVH);

                    SELECT CAST(SCOPE_IDENTITY() AS INT);";

                ArrayList parametros = new ArrayList
                {
                    new SqlParameter("@Tipo", evento.EVENTO_Tipo),
                    new SqlParameter("@Usuario", evento.EVENTO_Usuario),
                    new SqlParameter("@Accion", evento.EVENTO_Accion),
                    new SqlParameter("@Timestamp", evento.EVENTO_Timestamp),
                    new SqlParameter("@Criticidad", criticidad),
                    new SqlParameter("@Modulo", string.IsNullOrEmpty(evento.EVENTO_Modulo) ? (object)DBNull.Value : evento.EVENTO_Modulo),
                    new SqlParameter("@DVV", ""),
                    new SqlParameter("@DVH", "")
                };

                object resultado = dal._686DPEscalar(consulta, parametros);

                if (resultado != null && resultado != DBNull.Value)
                {
                    return Convert.ToInt32(resultado);
                }

                return 0;
            }
            catch (Exception ex)
            {
                throw new Exception("Error al registrar el evento: " + ex.Message, ex);
            }
        }

        public List<Evento> ObtenerEventos(string filtro, string busqueda, int? filtroCriticidad = null, string filtroModulo = null)
        {
            try
            {
                string consulta = @"
                    SELECT codEvento, tipo, usr, descripcion, fecha, criticidad, modulo
                    FROM [GymApp].[dbo].[Evento]
                    WHERE (@Filtro = 'all' OR tipo = @Filtro)
                    AND (@Busqueda = '' OR usr LIKE '%' + @Busqueda + '%' OR descripcion LIKE '%' + @Busqueda + '%')
                    AND (@FiltroCriticidad IS NULL OR criticidad = @FiltroCriticidad)
                    AND (@FiltroModulo IS NULL OR modulo = @FiltroModulo OR @FiltroModulo = 'all')
                    ORDER BY fecha DESC";

                ArrayList parametros = new ArrayList
                {
                    new SqlParameter("@Filtro", filtro ?? "all"),
                    new SqlParameter("@Busqueda", busqueda ?? ""),
                    new SqlParameter("@FiltroCriticidad", (object)filtroCriticidad ?? DBNull.Value),
                    new SqlParameter("@FiltroModulo", (object)filtroModulo ?? "all")
                };

                DataTable dt = dal._686DPConsultar(consulta, parametros);
                List<Evento> eventos = new List<Evento>();

                foreach (DataRow row in dt.Rows)
                {
                    Evento evento = new Evento(
                        Convert.ToInt32(row["codEvento"]),
                        row["tipo"].ToString(),
                        row["usr"].ToString(),
                        row["descripcion"].ToString(),
                        Convert.ToDateTime(row["fecha"]),
                        Convert.IsDBNull(row["criticidad"]) ? 1 : Convert.ToInt32(row["criticidad"]),
                        Convert.IsDBNull(row["modulo"]) ? "" : row["modulo"].ToString()
                    );
                    evento.Expandido = false;
                    eventos.Add(evento);
                }

                return eventos;
            }
            catch (Exception ex)
            {
                throw new Exception("Error al obtener los eventos: " + ex.Message, ex);
            }
        }

        public Dictionary<string, int> ObtenerEstadisticas()
        {
            try
            {
                string consulta = @"
                    SELECT
                    COUNT(*) as Total,
                    SUM(CASE WHEN tipo = 'login' THEN 1 ELSE 0 END) as Logins,
                    SUM(CASE WHEN tipo = 'new_user' THEN 1 ELSE 0 END) as UsuariosNuevos,
                    SUM(CASE WHEN tipo = 'error' THEN 1 ELSE 0 END) as Errores,
                    SUM(CASE WHEN criticidad = 1 THEN 1 ELSE 0 END) as Alta,
                    SUM(CASE WHEN criticidad = 2 THEN 1 ELSE 0 END) as MediaAlta,
                    SUM(CASE WHEN criticidad = 3 THEN 1 ELSE 0 END) as MediaBaja,
                    SUM(CASE WHEN criticidad = 4 THEN 1 ELSE 0 END) as Baja
                    FROM [GymApp].[dbo].[Evento]";

                ArrayList parametros = new ArrayList();

                DataTable dt = dal._686DPConsultar(consulta, parametros);

                if (dt.Rows.Count > 0)
                {
                    DataRow row = dt.Rows[0];
                    return new Dictionary<string, int>
                    {
                        { "Total", Convert.IsDBNull(row["Total"]) ? 0 : Convert.ToInt32(row["Total"]) },
                        { "Logins", Convert.IsDBNull(row["Logins"]) ? 0 : Convert.ToInt32(row["Logins"]) },
                        { "UsuariosNuevos", Convert.IsDBNull(row["UsuariosNuevos"]) ? 0 : Convert.ToInt32(row["UsuariosNuevos"]) },
                        { "Errores", Convert.IsDBNull(row["Errores"]) ? 0 : Convert.ToInt32(row["Errores"]) },
                        { "Alta", Convert.IsDBNull(row["Alta"]) ? 0 : Convert.ToInt32(row["Alta"]) },
                        { "MediaAlta", Convert.IsDBNull(row["MediaAlta"]) ? 0 : Convert.ToInt32(row["MediaAlta"]) },
                        { "MediaBaja", Convert.IsDBNull(row["MediaBaja"]) ? 0 : Convert.ToInt32(row["MediaBaja"]) },
                        { "Baja", Convert.IsDBNull(row["Baja"]) ? 0 : Convert.ToInt32(row["Baja"]) }
                    };
                }

                return new Dictionary<string, int>
                {
                    { "Total", 0 },
                    { "Logins", 0 },
                    { "UsuariosNuevos", 0 },
                    { "Errores", 0 },
                    { "Alta", 0 },
                    { "MediaAlta", 0 },
                    { "MediaBaja", 0 },
                    { "Baja", 0 }
                };
            }
            catch (Exception ex)
            {
                throw new Exception("Error al obtener las estadísticas: " + ex.Message, ex);
            }
        }

        public List<string> ObtenerModulos()
        {
            try
            {
                string consulta = @"
                    SELECT DISTINCT modulo
                    FROM [GymApp].[dbo].[Evento]
                    WHERE modulo IS NOT NULL AND modulo <> ''
                    ORDER BY modulo";

                ArrayList parametros = new ArrayList();
                DataTable dt = dal._686DPConsultar(consulta, parametros);
                List<string> modulos = new List<string>();

                foreach (DataRow row in dt.Rows)
                {
                    modulos.Add(row["modulo"].ToString());
                }

                return modulos;
            }
            catch (Exception ex)
            {
                throw new Exception("Error al obtener los módulos: " + ex.Message, ex);
            }
        }
    }
}