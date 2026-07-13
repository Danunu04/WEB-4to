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
        private DigitoVerificadorManager dvManager;

        public MPPEvento()
        {
            dal = new DalGeneral();
            criptoManager = new CriptoManager();
            dvManager = new DigitoVerificadorManager();
        }

        /// <summary>
        /// Trunca la fecha a segundos para evitar diferencias de milisegundos
        /// entre el valor insertado y el valor leído de SQL Server.
        /// </summary>
        private DateTime TruncarFecha(DateTime fecha)
        {
            return new DateTime(fecha.Year, fecha.Month, fecha.Day, fecha.Hour, fecha.Minute, fecha.Second);
        }

        /// <summary>
        /// Arma el diccionario de valores usado para calcular DVH/DVV de un evento.
        /// No incluye codEvento porque es autogenerado por la base de datos y no se conoce
        /// en el momento del INSERT. Al no formar parte del cálculo, la verificación es
        /// coherente entre inserción y lectura.
        /// </summary>
        private Dictionary<string, object> ArmarValoresDV(Evento evento, int criticidad)
        {
            var valores = new Dictionary<string, object>
            {
                { "tipo", evento.EVENTO_Tipo },
                { "usr", evento.EVENTO_Usuario },
                { "descripcion", evento.EVENTO_Accion },
                { "fecha", TruncarFecha(evento.EVENTO_Timestamp) },
                { "criticidad", criticidad },
                { "modulo", evento.EVENTO_Modulo }
            };

            return valores;
        }

        /// <summary>
        /// Calcula DVH y DVV de un evento a partir de sus valores de persistencia.
        /// La fecha se trunca a segundos para coincidir con lo almacenado en SQL Server.
        /// </summary>
        private string CalcularDigitosEvento(Evento evento, int criticidad)
        {
            var valores = ArmarValoresDV(evento, criticidad);
            return dvManager.CalcularDVH(valores);
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

                DateTime fechaTruncada = TruncarFecha(evento.EVENTO_Timestamp);
                string dvh = CalcularDigitosEvento(evento, criticidad);

                string consulta = @"
                    INSERT INTO [GymApp].[dbo].[Evento]
                    (tipo, usr, descripcion, fecha, criticidad, modulo, dvh)
                    VALUES (@Tipo, @Usuario, @Accion, @Timestamp, @Criticidad, @Modulo, @DVH);

                    SELECT CAST(SCOPE_IDENTITY() AS INT);";

                List<SqlParameter> parametros = new List<SqlParameter>
                {
                    new SqlParameter("@Tipo", evento.EVENTO_Tipo),
                    new SqlParameter("@Usuario", evento.EVENTO_Usuario),
                    new SqlParameter("@Accion", evento.EVENTO_Accion),
                    new SqlParameter("@Timestamp", fechaTruncada),
                    new SqlParameter("@Criticidad", criticidad),
                    new SqlParameter("@Modulo", string.IsNullOrEmpty(evento.EVENTO_Modulo) ? (object)DBNull.Value : evento.EVENTO_Modulo),
                    new SqlParameter("@DVH", dvh)
                };

                object resultado = dal._686DPEscalar(consulta, parametros);

                int codEvento = (resultado != null && resultado != DBNull.Value)
                    ? Convert.ToInt32(resultado)
                    : 0;

                // Mantener DigitoVerificador sincronizado: cada INSERT legítimo actualiza
                // dvvTabla y cantidadFilas para que la verificación no lo tome como intrusión.
                try
                {
                    new MPPDigitoVerificador().ActualizarControlTabla("Evento");
                }
                catch { /* no romper el flujo si falla la actualización del control */ }

                return codEvento;
            }
            catch (Exception ex)
            {
                throw new Exception("Error al registrar el evento: " + ex.Message, ex);
            }
        }

        /// <summary>
        /// Verifica la integridad de la tabla Evento.
        /// Se lee fila por fila con codEvento y se comparan DVH/DVV.
        /// </summary>
        public List<ResultadoVerificacionDV> VerificarIntegridadEventos()
        {
            var resultados = new List<ResultadoVerificacionDV>();

            try
            {
                string consulta = @"
                    SELECT codEvento, tipo, usr, descripcion, fecha, criticidad, modulo, dvh
                    FROM [GymApp].[dbo].[Evento]";

                DataTable dt = dal._686DPConsultar(consulta, new List<SqlParameter>());

                foreach (DataRow row in dt.Rows)
                {
                    int codEvento = Convert.ToInt32(row["codEvento"]);
                    string dvhAlmacenado = row["dvh"] != DBNull.Value ? row["dvh"].ToString() : string.Empty;

                    Evento evento = new Evento
                    {
                        EVENTO_Id = codEvento,
                        EVENTO_Tipo = row["tipo"] != DBNull.Value ? row["tipo"].ToString() : string.Empty,
                        EVENTO_Usuario = row["usr"] != DBNull.Value ? row["usr"].ToString() : string.Empty,
                        EVENTO_Accion = row["descripcion"] != DBNull.Value ? row["descripcion"].ToString() : string.Empty,
                        EVENTO_Timestamp = row["fecha"] != DBNull.Value ? Convert.ToDateTime(row["fecha"]) : DateTime.MinValue,
                        EVENTO_Criticidad = row["criticidad"] != DBNull.Value ? Convert.ToInt32(row["criticidad"]) : 1,
                        EVENTO_Modulo = row["modulo"] != DBNull.Value ? row["modulo"].ToString() : string.Empty
                    };

                    string dvhCalculado = CalcularDigitosEvento(evento, evento.EVENTO_Criticidad);

                    bool dvhOk = !string.IsNullOrEmpty(dvhAlmacenado)
                        && dvhAlmacenado.Equals(dvhCalculado, StringComparison.OrdinalIgnoreCase);

                    if (dvhOk)
                        continue;

                    resultados.Add(new ResultadoVerificacionDV
                    {
                        NombreTabla = "Evento",
                        ClaveFila = codEvento.ToString(),
                        Campo = "DVH (fila completa)",
                        EsValido = false,
                        TipoAlteracion = TipoAlteracion.EdicionDato,
                        Mensaje = "DVH del evento no coincide. Los datos fueron modificados.",
                        DVHAlmacenado = dvhAlmacenado,
                        DVHCalculado = dvhCalculado
                    });
                }

                if (resultados.Count == 0)
                {
                    resultados.Add(new ResultadoVerificacionDV
                    {
                        NombreTabla = "Evento",
                        ClaveFila = "-",
                        Campo = "-",
                        EsValido = true,
                        Mensaje = "Filas de Evento verificadas correctamente."
                    });
                }
            }
            catch (Exception ex)
            {
                resultados.Add(new ResultadoVerificacionDV
                {
                    NombreTabla = "Evento",
                    ClaveFila = "-",
                    Campo = "-",
                    EsValido = false,
                    Mensaje = "Error al verificar Evento: " + ex.Message
                });
            }

            return resultados;
        }

        /// <summary>
        /// Recalcula y actualiza los dígitos verificadores de todos los eventos.
        /// </summary>
        public void RecalcularDigitosTodosEventos()
        {
            try
            {
                string consulta = @"
                    SELECT codEvento, tipo, usr, descripcion, fecha, criticidad, modulo
                    FROM [GymApp].[dbo].[Evento]";

                DataTable dt = dal._686DPConsultar(consulta, new List<SqlParameter>());

                foreach (DataRow row in dt.Rows)
                {
                    Evento evento = new Evento
                    {
                        EVENTO_Id = Convert.ToInt32(row["codEvento"]),
                        EVENTO_Tipo = row["tipo"] != DBNull.Value ? row["tipo"].ToString() : string.Empty,
                        EVENTO_Usuario = row["usr"] != DBNull.Value ? row["usr"].ToString() : string.Empty,
                        EVENTO_Accion = row["descripcion"] != DBNull.Value ? row["descripcion"].ToString() : string.Empty,
                        EVENTO_Timestamp = row["fecha"] != DBNull.Value ? Convert.ToDateTime(row["fecha"]) : DateTime.MinValue,
                        EVENTO_Criticidad = row["criticidad"] != DBNull.Value ? Convert.ToInt32(row["criticidad"]) : 1,
                        EVENTO_Modulo = row["modulo"] != DBNull.Value ? row["modulo"].ToString() : string.Empty
                    };

                    string dvh = CalcularDigitosEvento(evento, evento.EVENTO_Criticidad);
                    ActualizarDigitosEvento(evento.EVENTO_Id, dvh);
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Error al recalcular dígitos de todos los eventos: " + ex.Message, ex);
            }
        }

        /// <summary>
        /// Actualiza los dígitos verificadores de un evento por su codEvento.
        /// </summary>
        private void ActualizarDigitosEvento(int codEvento, string dvh)
        {
            string consulta = @"
                UPDATE [GymApp].[dbo].[Evento]
                SET dvh = @DVH
                WHERE codEvento = @CodEvento";

            List<SqlParameter> parametros = new List<SqlParameter>
            {
                new SqlParameter("@CodEvento", codEvento),
                new SqlParameter("@DVH", dvh)
            };

            dal._686DPEscribir(consulta, parametros);
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

                List<SqlParameter> parametros = new List<SqlParameter>
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
                    SUM(CASE WHEN tipo = 'logout' THEN 1 ELSE 0 END) as Logouts,
                    SUM(CASE WHEN tipo = 'bloqueo_usuario' THEN 1 ELSE 0 END) as Bloqueos,
                    SUM(CASE WHEN tipo = 'desbloqueo_usuario' THEN 1 ELSE 0 END) as Desbloqueos,
                    SUM(CASE WHEN tipo = 'cambio_contrasena' THEN 1 ELSE 0 END) as CambioContrasenas,
                    SUM(CASE WHEN tipo = 'backup' THEN 1 ELSE 0 END) as Backups,
                    SUM(CASE WHEN tipo = 'new_user' THEN 1 ELSE 0 END) as UsuariosNuevos,
                    SUM(CASE WHEN tipo = 'update' THEN 1 ELSE 0 END) as Actualizaciones,
                    SUM(CASE WHEN tipo = 'error' THEN 1 ELSE 0 END) as Errores,
                    SUM(CASE WHEN criticidad = 1 THEN 1 ELSE 0 END) as Alta,
                    SUM(CASE WHEN criticidad = 2 THEN 1 ELSE 0 END) as MediaAlta,
                    SUM(CASE WHEN criticidad = 3 THEN 1 ELSE 0 END) as MediaBaja,
                    SUM(CASE WHEN criticidad = 4 THEN 1 ELSE 0 END) as Baja
                    FROM [GymApp].[dbo].[Evento]";

                List<SqlParameter> parametros = new List<SqlParameter>();

                DataTable dt = dal._686DPConsultar(consulta, parametros);

                if (dt.Rows.Count > 0)
                {
                    DataRow row = dt.Rows[0];
                    return new Dictionary<string, int>
                    {
                        { "Total", Convert.IsDBNull(row["Total"]) ? 0 : Convert.ToInt32(row["Total"]) },
                        { "Logins", Convert.IsDBNull(row["Logins"]) ? 0 : Convert.ToInt32(row["Logins"]) },
                        { "Logouts", Convert.IsDBNull(row["Logouts"]) ? 0 : Convert.ToInt32(row["Logouts"]) },
                        { "Bloqueos", Convert.IsDBNull(row["Bloqueos"]) ? 0 : Convert.ToInt32(row["Bloqueos"]) },
                        { "Desbloqueos", Convert.IsDBNull(row["Desbloqueos"]) ? 0 : Convert.ToInt32(row["Desbloqueos"]) },
                        { "CambioContrasenas", Convert.IsDBNull(row["CambioContrasenas"]) ? 0 : Convert.ToInt32(row["CambioContrasenas"]) },
                        { "Backups", Convert.IsDBNull(row["Backups"]) ? 0 : Convert.ToInt32(row["Backups"]) },
                        { "UsuariosNuevos", Convert.IsDBNull(row["UsuariosNuevos"]) ? 0 : Convert.ToInt32(row["UsuariosNuevos"]) },
                        { "Actualizaciones", Convert.IsDBNull(row["Actualizaciones"]) ? 0 : Convert.ToInt32(row["Actualizaciones"]) },
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
                    { "Logouts", 0 },
                    { "Bloqueos", 0 },
                    { "Desbloqueos", 0 },
                    { "CambioContrasenas", 0 },
                    { "Backups", 0 },
                    { "UsuariosNuevos", 0 },
                    { "Actualizaciones", 0 },
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

                List<SqlParameter> parametros = new List<SqlParameter>();
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
