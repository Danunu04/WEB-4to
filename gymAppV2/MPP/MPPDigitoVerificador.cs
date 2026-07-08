using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Text;
using BE;
using DAL;
using SERVICIOS;

namespace MPP
{
    /// <summary>
    /// Acceso a datos para la tabla de control DigitoVerificador y verificación
    /// de integridad de todas las tablas del schema.
    /// </summary>
    public class MPPDigitoVerificador
    {
        private DalGeneral dal;
        private DigitoVerificadorManager dvManager;
        private CriptoManager criptoManager;

        public MPPDigitoVerificador()
        {
            dal = new DalGeneral();
            dvManager = new DigitoVerificadorManager();
            criptoManager = new CriptoManager();
        }

        #region Tabla de control

        /// <summary>
        /// Obtiene el registro de control de una tabla, o null si no existe.
        /// </summary>
        public DataRow ObtenerControlPorTabla(string nombreTabla)
        {
            try
            {
                string consulta = @"
                    SELECT idDigitoVerificador, nombreTabla, dvvTabla, cantidadFilas, fechaCalculo
                    FROM [GymApp].[dbo].[DigitoVerificador]
                    WHERE nombreTabla = @NombreTabla";

                List<SqlParameter> parametros = new List<SqlParameter>
                {
                    new SqlParameter("@NombreTabla", nombreTabla)
                };

                DataTable dt = dal._686DPConsultar(consulta, parametros);
                return dt.Rows.Count > 0 ? dt.Rows[0] : null;
            }
            catch (Exception ex)
            {
                throw new Exception($"Error al obtener control de {nombreTabla}: " + ex.Message, ex);
            }
        }

        /// <summary>
        /// Inserta o actualiza el registro de control de una tabla.
        /// dvvTabla = SHA256 de la concatenación de todos los dvh de las filas.
        /// cantidadFilas = cantidad de filas al momento del último recálculo.
        /// </summary>
        public void GuardarControl(string nombreTabla, string dvvTabla, int cantidadFilas)
        {
            try
            {
                string consulta = @"
                    IF EXISTS (SELECT 1 FROM [GymApp].[dbo].[DigitoVerificador] WHERE nombreTabla = @NombreTabla)
                        UPDATE [GymApp].[dbo].[DigitoVerificador]
                        SET dvvTabla = @DVVTabla,
                            cantidadFilas = @CantidadFilas,
                            fechaCalculo = GETDATE()
                        WHERE nombreTabla = @NombreTabla
                    ELSE
                        INSERT INTO [GymApp].[dbo].[DigitoVerificador] (nombreTabla, dvvTabla, cantidadFilas)
                        VALUES (@NombreTabla, @DVVTabla, @CantidadFilas)";

                List<SqlParameter> parametros = new List<SqlParameter>
                {
                    new SqlParameter("@NombreTabla", nombreTabla),
                    new SqlParameter("@DVVTabla", dvvTabla),
                    new SqlParameter("@CantidadFilas", cantidadFilas)
                };

                dal._686DPEscribir(consulta, parametros);
            }
            catch (Exception ex)
            {
                throw new Exception($"Error al guardar control de {nombreTabla}: " + ex.Message, ex);
            }
        }

        /// <summary>
        /// Recalcula dvvTabla y cantidadFilas para una tabla sin tocar los dvh de cada fila.
        /// Llamar después de cualquier INSERT legítimo en tablas con control de integridad.
        /// </summary>
        public void ActualizarControlTabla(string nombreTabla)
        {
            try
            {
                string dvvTabla = CalcularDvvTabla(nombreTabla);
                int cantidadFilas = ObtenerCantidadFilas(nombreTabla);
                GuardarControl(nombreTabla, dvvTabla, cantidadFilas);
            }
            catch (Exception ex)
            {
                throw new Exception($"Error al actualizar control de {nombreTabla}: " + ex.Message, ex);
            }
        }

        #endregion

        #region Verificación de integridad

        /// <summary>
        /// Verifica el hash de tabla comparando dvvTabla almacenado contra el recalculado
        /// a partir de los dvh de las filas actuales. También compara cantidadFilas para
        /// detectar inserciones o eliminaciones. Devuelve un único resultado con TipoAlteracion.
        /// </summary>
        public ResultadoVerificacionDV VerificarHashTabla(string nombreTabla)
        {
            try
            {
                DataRow control = ObtenerControlPorTabla(nombreTabla);
                if (control == null)
                {
                    return new ResultadoVerificacionDV
                    {
                        NombreTabla = nombreTabla,
                        ClaveFila = "-",
                        Campo = "-",
                        EsValido = false,
                        TipoAlteracion = TipoAlteracion.SinAlteracion,
                        Mensaje = "La tabla no tiene registro de control. Ejecutar recálculo masivo."
                    };
                }

                string dvvTablaAlmacenado = control["dvvTabla"].ToString();
                int cantidadAlmacenada = control["cantidadFilas"] != DBNull.Value
                    ? Convert.ToInt32(control["cantidadFilas"])
                    : -1;

                if (!TablaExisteEnDB(nombreTabla))
                {
                    return new ResultadoVerificacionDV
                    {
                        NombreTabla = nombreTabla,
                        ClaveFila = "-",
                        Campo = "-",
                        EsValido = false,
                        TipoAlteracion = TipoAlteracion.TablaEliminada,
                        Mensaje = cantidadAlmacenada > 0
                            ? $"La tabla fue eliminada del sistema. Tenía {cantidadAlmacenada} filas registradas."
                            : "La tabla fue eliminada del sistema."
                    };
                }

                int cantidadActual = ObtenerCantidadFilas(nombreTabla);
                string dvvTablaCalculado = CalcularDvvTabla(nombreTabla);

                bool dvvOk = dvvTablaAlmacenado.Equals(dvvTablaCalculado, StringComparison.OrdinalIgnoreCase);
                bool countOk = cantidadAlmacenada >= 0 && cantidadAlmacenada == cantidadActual;

                if (dvvOk && countOk)
                {
                    return new ResultadoVerificacionDV
                    {
                        NombreTabla = nombreTabla,
                        ClaveFila = "-",
                        Campo = "-",
                        EsValido = true,
                        TipoAlteracion = TipoAlteracion.SinAlteracion,
                        Mensaje = "Tabla verificada correctamente."
                    };
                }

                TipoAlteracion tipo;
                string mensaje;

                if (cantidadActual == 0 && cantidadAlmacenada > 0)
                {
                    tipo = TipoAlteracion.TablaVaciada;
                    mensaje = $"La tabla fue vaciada. Se esperaban {cantidadAlmacenada} filas, no hay ninguna.";
                }
                else if (cantidadAlmacenada >= 0 && cantidadActual < cantidadAlmacenada)
                {
                    tipo = TipoAlteracion.EliminacionFila;
                    mensaje = $"Se eliminaron filas. Se esperaban {cantidadAlmacenada}, hay {cantidadActual}.";
                }
                else if (cantidadAlmacenada >= 0 && cantidadActual > cantidadAlmacenada)
                {
                    tipo = TipoAlteracion.InsercionNoAutorizada;
                    mensaje = $"Se insertaron filas no registradas. Se esperaban {cantidadAlmacenada}, hay {cantidadActual}.";
                }
                else
                {
                    tipo = TipoAlteracion.EdicionDato;
                    mensaje = "Se modificaron datos de una o más filas. Hash de tabla no coincide.";
                }

                return new ResultadoVerificacionDV
                {
                    NombreTabla = nombreTabla,
                    ClaveFila = "-",
                    Campo = "-",
                    EsValido = false,
                    TipoAlteracion = tipo,
                    Mensaje = mensaje
                };
            }
            catch (Exception ex)
            {
                return new ResultadoVerificacionDV
                {
                    NombreTabla = nombreTabla,
                    ClaveFila = "-",
                    Campo = "-",
                    EsValido = false,
                    TipoAlteracion = TipoAlteracion.SinAlteracion,
                    Mensaje = "Error al verificar tabla: " + ex.Message
                };
            }
        }

        /// <summary>
        /// Verifica la integridad de todas las tablas registradas en DigitoVerificador.
        /// Primero hace el chequeo de hash de tabla; si falla, evalúa fila por fila.
        /// </summary>
        public List<ResultadoVerificacionDV> VerificarIntegridadGlobal()
        {
            var resultados = new List<ResultadoVerificacionDV>();

            try
            {
                List<string> tablas = ObtenerTablasConControl();

                foreach (string tabla in tablas)
                {
                    resultados.AddRange(VerificarIntegridadTabla(tabla));
                }
            }
            catch (Exception ex)
            {
                resultados.Add(new ResultadoVerificacionDV
                {
                    NombreTabla = "Sistema",
                    ClaveFila = "-",
                    Campo = "-",
                    EsValido = false,
                    Mensaje = "Error general al verificar integridad: " + ex.Message
                });
            }

            return resultados;
        }

        /// <summary>
        /// Verifica la integridad de una tabla genérica (no encriptada).
        /// Primero compara el hash de tabla; si falla, recorre fila por fila.
        /// </summary>
        public List<ResultadoVerificacionDV> VerificarIntegridadTabla(string nombreTabla)
        {
            var resultados = new List<ResultadoVerificacionDV>();

            var resultadoTabla = VerificarHashTabla(nombreTabla);
            resultados.Add(resultadoTabla);

            if (!resultadoTabla.EsValido && resultadoTabla.TipoAlteracion == TipoAlteracion.EdicionDato)
            {
                resultados.AddRange(VerificarFilasTabla(nombreTabla));
            }

            return resultados;
        }

        /// <summary>
        /// Recorre las filas de una tabla genérica y verifica su dvh.
        /// Solo se llama cuando el hash de tabla ya falló.
        /// </summary>
        public List<ResultadoVerificacionDV> VerificarFilasTabla(string nombreTabla)
        {
            var resultados = new List<ResultadoVerificacionDV>();

            try
            {
                DataTable dt = ObtenerFilasParaVerificacion(nombreTabla);
                if (dt == null || dt.Rows.Count == 0)
                    return resultados;

                List<DataColumn> columnasDatos = dt.Columns.Cast<DataColumn>()
                    .Where(c => !c.ColumnName.Equals("dvh", StringComparison.OrdinalIgnoreCase))
                    .ToList();

                string[] clavesPrimarias = ObtenerClavesPrimarias(nombreTabla);

                foreach (DataRow row in dt.Rows)
                {
                    string claveFila = ArmarClaveFila(row, clavesPrimarias);
                    string dvhAlmacenado = row["dvh"] != DBNull.Value ? row["dvh"].ToString() : string.Empty;

                    var valores = ArmarDiccionarioValores(row, columnasDatos);
                    string dvhCalculado = dvManager.CalcularDVH(valores);

                    bool dvhOk = !string.IsNullOrEmpty(dvhAlmacenado)
                        && dvhAlmacenado.Equals(dvhCalculado, StringComparison.OrdinalIgnoreCase);

                    if (dvhOk)
                        continue;

                    resultados.Add(new ResultadoVerificacionDV
                    {
                        NombreTabla = nombreTabla,
                        ClaveFila = claveFila,
                        Campo = "DVH (fila completa)",
                        EsValido = false,
                        TipoAlteracion = TipoAlteracion.EdicionDato,
                        Mensaje = "DVH de fila no coincide. Los datos fueron modificados.",
                        DVHAlmacenado = dvhAlmacenado,
                        DVHCalculado = dvhCalculado
                    });
                }
            }
            catch (Exception ex)
            {
                resultados.Add(new ResultadoVerificacionDV
                {
                    NombreTabla = nombreTabla,
                    ClaveFila = "-",
                    Campo = "-",
                    EsValido = false,
                    Mensaje = "Error al verificar filas: " + ex.Message
                });
            }

            return resultados;
        }

        /// <summary>
        /// Devuelve el listado de tablas registradas en DigitoVerificador.
        /// </summary>
        public List<string> ObtenerTablasConControl()
        {
            try
            {
                string consulta = @"
                    SELECT nombreTabla
                    FROM [GymApp].[dbo].[DigitoVerificador]
                    ORDER BY nombreTabla";

                DataTable dt = dal._686DPConsultar(consulta, new List<SqlParameter>());
                List<string> tablas = new List<string>();

                foreach (DataRow row in dt.Rows)
                {
                    tablas.Add(row["nombreTabla"].ToString());
                }

                return tablas;
            }
            catch (Exception ex)
            {
                throw new Exception("Error al listar tablas con control: " + ex.Message, ex);
            }
        }

        /// <summary>
        /// Devuelve las tablas del schema que tienen columna dvh
        /// pero aún no están registradas en DigitoVerificador.
        /// </summary>
        public List<string> ObtenerTablasSinControl()
        {
            try
            {
                string consulta = @"
                    SELECT t.TABLE_NAME
                    FROM INFORMATION_SCHEMA.TABLES t
                    WHERE t.TABLE_TYPE = 'BASE TABLE'
                      AND EXISTS (
                          SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS c
                          WHERE c.TABLE_NAME = t.TABLE_NAME
                            AND c.COLUMN_NAME = 'dvh')
                      AND NOT EXISTS (
                          SELECT 1 FROM [GymApp].[dbo].[DigitoVerificador] dv
                          WHERE dv.nombreTabla = t.TABLE_NAME)
                    ORDER BY t.TABLE_NAME";

                DataTable dt = dal._686DPConsultar(consulta, new List<SqlParameter>());
                List<string> tablas = new List<string>();

                foreach (DataRow row in dt.Rows)
                {
                    tablas.Add(row["TABLE_NAME"].ToString());
                }

                return tablas;
            }
            catch (Exception ex)
            {
                throw new Exception("Error al listar tablas sin control: " + ex.Message, ex);
            }
        }

        /// <summary>
        /// Devuelve un resumen del estado de control para todas las tablas con dvh.
        /// </summary>
        public List<EstadoControlDV> ObtenerEstadoControl()
        {
            try
            {
                string consultaControl = @"
                    SELECT
                        t.TABLE_NAME AS NombreTabla,
                        CASE WHEN dv.idDigitoVerificador IS NULL THEN 0 ELSE 1 END AS TieneControl,
                        dv.fechaCalculo AS FechaCalculo
                    FROM INFORMATION_SCHEMA.TABLES t
                    INNER JOIN INFORMATION_SCHEMA.COLUMNS cdvh
                        ON t.TABLE_NAME = cdvh.TABLE_NAME AND cdvh.COLUMN_NAME = 'dvh'
                    LEFT JOIN [GymApp].[dbo].[DigitoVerificador] dv
                        ON dv.nombreTabla = t.TABLE_NAME
                    WHERE t.TABLE_TYPE = 'BASE TABLE'
                    ORDER BY t.TABLE_NAME";

                DataTable dtControl = dal._686DPConsultar(consultaControl, new List<SqlParameter>());
                List<EstadoControlDV> estado = new List<EstadoControlDV>();

                foreach (DataRow row in dtControl.Rows)
                {
                    string nombreTabla = row["NombreTabla"] != DBNull.Value ? row["NombreTabla"].ToString() : string.Empty;
                    bool tieneControl = row["TieneControl"] != DBNull.Value && Convert.ToBoolean(row["TieneControl"]);
                    DateTime? fechaCalculo = row["FechaCalculo"] != DBNull.Value ? (DateTime?)Convert.ToDateTime(row["FechaCalculo"]) : null;

                    int totalFilas = 0;
                    int filasDVHVacio = 0;

                    try
                    {
                        string consultaConteo = $@"
                            SELECT
                                COUNT(*) AS TotalFilas,
                                SUM(CASE WHEN [dvh] = '' OR [dvh] IS NULL THEN 1 ELSE 0 END) AS FilasDVHVacio
                            FROM [GymApp].[dbo].[{nombreTabla}]";

                        DataTable dtConteo = dal._686DPConsultar(consultaConteo, new List<SqlParameter>());

                        if (dtConteo.Rows.Count > 0)
                        {
                            DataRow r = dtConteo.Rows[0];
                            totalFilas = r["TotalFilas"] != DBNull.Value ? Convert.ToInt32(r["TotalFilas"]) : 0;
                            filasDVHVacio = r["FilasDVHVacio"] != DBNull.Value ? Convert.ToInt32(r["FilasDVHVacio"]) : 0;
                        }
                    }
                    catch (Exception)
                    {
                        totalFilas = -1;
                        filasDVHVacio = -1;
                    }

                    estado.Add(new EstadoControlDV(nombreTabla, tieneControl, totalFilas, filasDVHVacio, fechaCalculo));
                }

                return estado;
            }
            catch (Exception ex)
            {
                throw new Exception("Error al obtener estado de control: " + ex.Message, ex);
            }
        }

        #endregion

        #region Recálculo

        /// <summary>
        /// Recalcula dvh de todas las filas de todas las tablas con control
        /// y actualiza dvvTabla y cantidadFilas en DigitoVerificador.
        /// </summary>
        public void RecalcularDigitosGlobal()
        {
            List<string> tablas = ObtenerTablasConControl();

            foreach (string tabla in tablas)
            {
                RecalcularDigitosTabla(tabla);
            }
        }

        /// <summary>
        /// Recalcula dvh de todas las filas de una tabla y actualiza su control.
        /// Cuando actualizarFilas=false, solo recalcula dvvTabla y cantidadFilas a partir
        /// de los dvh existentes (usado para tablas con datos encriptados donde el MPP
        /// especializado ya actualizó los dvh de cada fila).
        /// </summary>
        public void RecalcularDigitosTabla(string nombreTabla, bool actualizarFilas = true)
        {
            try
            {
                DataTable dt = ObtenerFilasParaVerificacion(nombreTabla);
                int cantidadFilas = dt?.Rows.Count ?? 0;

                if (dt == null || dt.Rows.Count == 0)
                {
                    GuardarControl(nombreTabla, criptoManager.GenerarHashSHA256(string.Empty), 0);
                    return;
                }

                List<DataColumn> columnasDatos = dt.Columns.Cast<DataColumn>()
                    .Where(c => !c.ColumnName.Equals("dvh", StringComparison.OrdinalIgnoreCase))
                    .ToList();

                string[] clavesPrimarias = ObtenerClavesPrimarias(nombreTabla);
                StringBuilder dvhConcat = new StringBuilder();

                foreach (DataRow row in dt.Rows)
                {
                    string dvh;

                    if (actualizarFilas)
                    {
                        var valores = ArmarDiccionarioValores(row, columnasDatos);
                        dvh = dvManager.CalcularDVH(valores);

                        string claveFila = ArmarClaveFila(row, clavesPrimarias);
                        ActualizarDigitosFila(nombreTabla, claveFila, dvh);
                    }
                    else
                    {
                        dvh = row["dvh"] != DBNull.Value ? row["dvh"].ToString() : string.Empty;
                    }

                    dvhConcat.Append(dvh);
                }

                string dvvTabla = criptoManager.GenerarHashSHA256(dvhConcat.ToString());
                GuardarControl(nombreTabla, dvvTabla, cantidadFilas);
            }
            catch (Exception ex)
            {
                throw new Exception($"Error al recalcular dígitos de {nombreTabla}: " + ex.Message, ex);
            }
        }

        /// <summary>
        /// Actualiza el dvh de una fila específica usando su clave primaria.
        /// </summary>
        private void ActualizarDigitosFila(string nombreTabla, string claveFila, string dvh)
        {
            try
            {
                string[] partes = claveFila.Split('|');
                string[] claves = ObtenerClavesPrimarias(nombreTabla);

                if (partes.Length != claves.Length)
                    return;

                StringBuilder where = new StringBuilder();
                List<SqlParameter> parametros = new List<SqlParameter>
                {
                    new SqlParameter("@DVH", dvh)
                };

                for (int i = 0; i < claves.Length; i++)
                {
                    if (i > 0) where.Append(" AND ");
                    string paramName = "@PK" + i;
                    where.Append($"[{claves[i]}] = {paramName}");
                    parametros.Add(new SqlParameter(paramName, partes[i]));
                }

                string consulta = $@"
                    UPDATE [GymApp].[dbo].[{nombreTabla}]
                    SET dvh = @DVH
                    WHERE {where}";

                dal._686DPEscribir(consulta, parametros);
            }
            catch (Exception ex)
            {
                throw new Exception($"Error al actualizar dvh de fila {claveFila} en {nombreTabla}: " + ex.Message, ex);
            }
        }

        #endregion

        #region Backup

        /// <summary>
        /// Realiza un backup completo de la base de datos en la ruta indicada.
        /// </summary>
        public void RealizarBackup(string rutaDestino)
        {
            if (string.IsNullOrWhiteSpace(rutaDestino))
                throw new ArgumentException("La ruta de destino no puede estar vacía.", nameof(rutaDestino));

            string extension = Path.GetExtension(rutaDestino);
            if (!string.Equals(extension, ".bak", StringComparison.OrdinalIgnoreCase))
                throw new ArgumentException("La ruta del backup debe tener extensión .bak.");

            string directorio = Path.GetDirectoryName(rutaDestino);
            if (string.IsNullOrWhiteSpace(directorio))
                throw new ArgumentException("La ruta de destino no es válida.");

            try
            {
                string nombreBase = ObtenerNombreBaseDatos();
                string consulta = $@"
                    BACKUP DATABASE [{nombreBase}]
                    TO DISK = @Ruta
                    WITH INIT;";

                using (SqlConnection connMaster = new SqlConnection(ObtenerCadenaConexionMaster()))
                {
                    connMaster.Open();

                    using (SqlCommand cmd = new SqlCommand(consulta, connMaster))
                    {
                        cmd.Parameters.AddWithValue("@Ruta", rutaDestino);
                        cmd.CommandTimeout = 300;
                        cmd.ExecuteNonQuery();
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Error al realizar el backup. Verifique permisos y ruta: " + ex.Message, ex);
            }
        }

        /// <summary>
        /// Restaura la base de datos desde un backup previo.
        /// </summary>
        public void RestaurarBackup(string rutaBackup)
        {
            if (string.IsNullOrWhiteSpace(rutaBackup))
                throw new ArgumentException("La ruta del backup no puede estar vacía.", nameof(rutaBackup));

            if (!File.Exists(rutaBackup))
                throw new ArgumentException("No se encontró el archivo de backup especificado.");

            try
            {
                string nombreBase = ObtenerNombreBaseDatos();

                using (SqlConnection connMaster = new SqlConnection(ObtenerCadenaConexionMaster()))
                {
                    connMaster.Open();

                    string logicalData;
                    string logicalLog;
                    using (SqlCommand cmd = new SqlCommand("RESTORE FILELISTONLY FROM DISK = @Ruta", connMaster))
                    {
                        cmd.Parameters.AddWithValue("@Ruta", rutaBackup);
                        DataTable dt = new DataTable();
                        using (SqlDataReader reader = cmd.ExecuteReader())
                        {
                            dt.Load(reader);
                        }

                        if (dt.Rows.Count < 2)
                            throw new Exception("El backup no contiene los archivos de datos y log esperados.");

                        logicalData = dt.Rows[0]["LogicalName"].ToString();
                        logicalLog = dt.Rows[1]["LogicalName"].ToString();
                    }

                    string mdfPath;
                    string ldfPath;
                    using (SqlCommand cmd = new SqlCommand(@"
                        SELECT SERVERPROPERTY('InstanceDefaultDataPath') AS DataPath,
                               SERVERPROPERTY('InstanceDefaultLogPath') AS LogPath;", connMaster))
                    {
                        DataTable dtPath = new DataTable();
                        using (SqlDataReader reader = cmd.ExecuteReader())
                        {
                            dtPath.Load(reader);
                        }

                        if (dtPath.Rows.Count == 0)
                            throw new Exception("No se pudieron obtener las rutas por defecto de la instancia SQL Server.");

                        string dataPath = dtPath.Rows[0]["DataPath"]?.ToString();
                        string logPath = dtPath.Rows[0]["LogPath"]?.ToString();

                        if (string.IsNullOrWhiteSpace(dataPath) || string.IsNullOrWhiteSpace(logPath))
                            throw new Exception("La instancia SQL Server no tiene configuradas las rutas por defecto de datos y/o log.");

                        mdfPath = Path.Combine(dataPath, $"{nombreBase}.mdf");
                        ldfPath = Path.Combine(logPath, $"{nombreBase}_log.ldf");
                    }

                    SqlConnection.ClearAllPools();

                    string consulta = $@"
                        ALTER DATABASE [{nombreBase}] SET SINGLE_USER WITH ROLLBACK IMMEDIATE;
                        RESTORE DATABASE [{nombreBase}]
                        FROM DISK = @Ruta
                        WITH
                            MOVE @LogicalData TO @Mdf,
                            MOVE @LogicalLog TO @Ldf,
                            REPLACE,
                            STATS = 5;
                        ALTER DATABASE [{nombreBase}] SET MULTI_USER;";

                    using (SqlCommand cmd = new SqlCommand(consulta, connMaster))
                    {
                        cmd.Parameters.AddWithValue("@Ruta", rutaBackup);
                        cmd.Parameters.AddWithValue("@LogicalData", logicalData);
                        cmd.Parameters.AddWithValue("@LogicalLog", logicalLog);
                        cmd.Parameters.AddWithValue("@Mdf", mdfPath);
                        cmd.Parameters.AddWithValue("@Ldf", ldfPath);
                        cmd.CommandTimeout = 300;
                        cmd.ExecuteNonQuery();
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Error al restaurar el backup. Verifique permisos y ruta: " + ex.Message, ex);
            }
        }

        /// <summary>
        /// Exporta la base de datos a un archivo .bacpac usando SqlPackage.exe.
        /// </summary>
        public void ExportarBacpac(string rutaDestino)
        {
            if (string.IsNullOrWhiteSpace(rutaDestino))
                throw new ArgumentException("La ruta de destino no puede estar vacía.", nameof(rutaDestino));

            string extension = Path.GetExtension(rutaDestino);
            if (!string.Equals(extension, ".bacpac", StringComparison.OrdinalIgnoreCase))
                throw new ArgumentException("La ruta del archivo debe tener extensión .bacpac.");

            try
            {
                string sqlPackage = BuscarSqlPackage();
                string connSrc = ObtenerCadenaConexionSqlPackage();
                string args = $"/Action:Export " +
                              $"/SourceConnectionString:\"{connSrc}\" " +
                              $"/TargetFile:\"{rutaDestino}\"";
                EjecutarSqlPackage(sqlPackage, args);
            }
            catch (Exception ex)
            {
                throw new Exception("Error al exportar el archivo .bacpac. Verifique que SqlPackage.exe esté instalado: " + ex.Message, ex);
            }
        }

        /// <summary>
        /// Importa un archivo .bacpac, reemplazando la base de datos existente.
        /// </summary>
        public void ImportarBacpac(string rutaBacpac)
        {
            if (string.IsNullOrWhiteSpace(rutaBacpac))
                throw new ArgumentException("La ruta del archivo no puede estar vacía.", nameof(rutaBacpac));

            if (!File.Exists(rutaBacpac))
                throw new ArgumentException("No se encontró el archivo .bacpac especificado.");

            try
            {
                string nombreBase = ObtenerNombreBaseDatos();

                SqlConnection.ClearAllPools();
                using (SqlConnection connMaster = new SqlConnection(ObtenerCadenaConexionMaster()))
                {
                    connMaster.Open();
                    using (SqlCommand cmd = new SqlCommand(
                        $"IF DB_ID('{nombreBase.Replace("'", "''")}') IS NOT NULL " +
                        $"BEGIN ALTER DATABASE [{nombreBase}] SET SINGLE_USER WITH ROLLBACK IMMEDIATE; " +
                        $"DROP DATABASE [{nombreBase}]; END", connMaster))
                    {
                        cmd.CommandTimeout = 60;
                        cmd.ExecuteNonQuery();
                    }
                }

                string sqlPackage = BuscarSqlPackage();
                string args = $"/Action:Import " +
                              $"/TargetServerName:. " +
                              $"/TargetDatabaseName:{nombreBase} " +
                              $"/SourceFile:\"{rutaBacpac}\" " +
                              $"/TargetTrustServerCertificate:True";
                EjecutarSqlPackage(sqlPackage, args);
            }
            catch (Exception ex)
            {
                throw new Exception("Error al importar el archivo .bacpac. Verifique que SqlPackage.exe esté instalado: " + ex.Message, ex);
            }
        }

        private static string BuscarSqlPackage()
        {
            var candidatos = new List<string>();

            string pf64 = Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles);
            string pf86 = Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86);
            string[] basesDirs = string.Equals(pf64, pf86, StringComparison.OrdinalIgnoreCase)
                ? new[] { pf64 }
                : new[] { pf64, pf86 };

            string[] versiones = { "170", "160", "150", "140", "130", "120", "110" };
            foreach (var dir in basesDirs)
            {
                if (string.IsNullOrEmpty(dir)) continue;
                foreach (var ver in versiones)
                    candidatos.Add(Path.Combine(dir, "Microsoft SQL Server", ver, "DAC", "bin", "SqlPackage.exe"));
            }

            string userProfile = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
            candidatos.Add(Path.Combine(userProfile, ".dotnet", "tools", "SqlPackage.exe"));

            string[] aniosVS = { "2022", "2019", "2017" };
            string[] edicionesVS = { "Enterprise", "Professional", "Community", "BuildTools" };
            foreach (var dir in basesDirs)
            {
                if (string.IsNullOrEmpty(dir)) continue;
                foreach (var anio in aniosVS)
                    foreach (var ed in edicionesVS)
                        candidatos.Add(Path.Combine(dir, "Microsoft Visual Studio", anio, ed,
                            "Common7", "IDE", "Extensions", "Microsoft", "SQLDB", "DAC", "SqlPackage.exe"));
            }

            foreach (var ruta in candidatos)
            {
                if (File.Exists(ruta)) return ruta;
            }

            try
            {
                var psi = new System.Diagnostics.ProcessStartInfo
                {
                    FileName = "where.exe",
                    Arguments = "sqlpackage.exe",
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    CreateNoWindow = true,
                };
                using (var proc = System.Diagnostics.Process.Start(psi))
                {
                    if (proc != null)
                    {
                        string linea = proc.StandardOutput.ReadLine()?.Trim();
                        proc.WaitForExit(3000);
                        if (!string.IsNullOrEmpty(linea) && File.Exists(linea))
                            return linea;
                    }
                }
            }
            catch { }

            throw new Exception(
                "No se encontró SqlPackage.exe en el servidor. " +
                "Para instalarlo ejecute en una consola con permisos de administrador: " +
                "dotnet tool install -g microsoft.sqlpackage  " +
                "O instale 'SQL Server Data Tools' (SSDT) desde el instalador de Visual Studio.");
        }

        private static void EjecutarSqlPackage(string exe, string args)
        {
            var psi = new System.Diagnostics.ProcessStartInfo
            {
                FileName = exe,
                Arguments = args,
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                CreateNoWindow = true,
            };

            using (var proc = System.Diagnostics.Process.Start(psi))
            {
                if (proc == null)
                    throw new Exception(
                        "No se pudo iniciar SqlPackage.exe. " +
                        "Instale SQL Server Data Tools en el servidor.");

                string output = proc.StandardOutput.ReadToEnd();
                string error = proc.StandardError.ReadToEnd();
                bool termino = proc.WaitForExit(300000);

                if (!termino)
                {
                    proc.Kill();
                    throw new Exception("La operación excedió el tiempo máximo permitido (5 minutos).");
                }

                if (proc.ExitCode != 0)
                {
                    string detalle = !string.IsNullOrWhiteSpace(error) ? error
                                   : !string.IsNullOrWhiteSpace(output) ? output
                                   : "(sin detalle)";
                    throw new Exception(
                        $"SqlPackage.exe finalizó con error (código {proc.ExitCode}): {detalle.Trim()}");
                }
            }
        }

        private string ObtenerCadenaConexionSqlPackage()
        {
            var settings = ConfigurationManager.ConnectionStrings["GymAppConnection"];
            if (settings == null || string.IsNullOrEmpty(settings.ConnectionString))
                throw new Exception("No se encontró la cadena de conexión 'GymAppConnection' en Web.config.");

            SqlConnectionStringBuilder builder = new SqlConnectionStringBuilder(settings.ConnectionString);
            builder.Remove("Network Library");
            builder["TrustServerCertificate"] = true;
            return builder.ConnectionString;
        }

        private string ObtenerNombreBaseDatos()
        {
            var settings = ConfigurationManager.ConnectionStrings["GymAppConnection"];
            if (settings == null || string.IsNullOrEmpty(settings.ConnectionString))
                throw new Exception("No se encontró la cadena de conexión 'GymAppConnection' en Web.config.");

            SqlConnectionStringBuilder builder = new SqlConnectionStringBuilder(settings.ConnectionString);
            string nombreBase = builder.InitialCatalog;

            if (string.IsNullOrWhiteSpace(nombreBase))
                throw new Exception("La cadena de conexión no especifica un Initial Catalog.");

            return nombreBase;
        }

        private string ObtenerCadenaConexionMaster()
        {
            var settings = ConfigurationManager.ConnectionStrings["GymAppConnection"];
            if (settings == null || string.IsNullOrEmpty(settings.ConnectionString))
                throw new Exception("No se encontró la cadena de conexión 'GymAppConnection' en Web.config.");

            SqlConnectionStringBuilder builder = new SqlConnectionStringBuilder(settings.ConnectionString)
            {
                InitialCatalog = "master"
            };

            return builder.ConnectionString;
        }

        #endregion

        #region Utilidades privadas

        /// <summary>
        /// Calcula dvvTabla = SHA256 de la concatenación de todos los dvh de las filas actuales.
        /// </summary>
        private string CalcularDvvTabla(string nombreTabla)
        {
            try
            {
                string consulta = $"SELECT dvh FROM [GymApp].[dbo].[{nombreTabla}]";
                DataTable dt = dal._686DPConsultar(consulta, new List<SqlParameter>());

                if (dt == null || dt.Rows.Count == 0)
                    return criptoManager.GenerarHashSHA256(string.Empty);

                StringBuilder concat = new StringBuilder();
                foreach (DataRow row in dt.Rows)
                {
                    concat.Append(row["dvh"] != DBNull.Value ? row["dvh"].ToString() : string.Empty);
                }

                return criptoManager.GenerarHashSHA256(concat.ToString());
            }
            catch (Exception ex)
            {
                throw new Exception($"Error al calcular dvvTabla de {nombreTabla}: " + ex.Message, ex);
            }
        }

        /// <summary>
        /// Devuelve el COUNT(*) actual de una tabla.
        /// </summary>
        private int ObtenerCantidadFilas(string nombreTabla)
        {
            string consulta = $"SELECT COUNT(*) FROM [GymApp].[dbo].[{nombreTabla}]";
            object resultado = dal._686DPEscalar(consulta, new List<SqlParameter>());
            return resultado != null && resultado != DBNull.Value ? Convert.ToInt32(resultado) : 0;
        }

        /// <summary>
        /// Obtiene todas las filas de una tabla incluyendo dvh.
        /// </summary>
        private DataTable ObtenerFilasParaVerificacion(string nombreTabla)
        {
            try
            {
                string consulta = $"SELECT * FROM [GymApp].[dbo].[{nombreTabla}]";
                return dal._686DPConsultar(consulta, new List<SqlParameter>());
            }
            catch (Exception ex)
            {
                throw new Exception($"Error al leer filas de {nombreTabla}: " + ex.Message, ex);
            }
        }

        /// <summary>
        /// Obtiene las columnas que forman la clave primaria de una tabla.
        /// Fallback: devuelve la primera columna.
        /// </summary>
        private string[] ObtenerClavesPrimarias(string nombreTabla)
        {
            try
            {
                string consulta = @"
                    SELECT COLUMN_NAME
                    FROM INFORMATION_SCHEMA.KEY_COLUMN_USAGE
                    WHERE OBJECTPROPERTY(OBJECT_ID(CONSTRAINT_SCHEMA + '.' + CONSTRAINT_NAME), 'IsPrimaryKey') = 1
                      AND TABLE_NAME = @NombreTabla
                    ORDER BY ORDINAL_POSITION";

                List<SqlParameter> parametros = new List<SqlParameter>
                {
                    new SqlParameter("@NombreTabla", nombreTabla)
                };

                DataTable dt = dal._686DPConsultar(consulta, parametros);

                if (dt.Rows.Count > 0)
                {
                    return dt.Rows.Cast<DataRow>().Select(r => r["COLUMN_NAME"].ToString()).ToArray();
                }

                string columnas = $@"
                    SELECT TOP 1 COLUMN_NAME
                    FROM INFORMATION_SCHEMA.COLUMNS
                    WHERE TABLE_NAME = @NombreTabla
                    ORDER BY ORDINAL_POSITION";

                DataTable dtColumnas = dal._686DPConsultar(columnas, parametros);
                if (dtColumnas.Rows.Count > 0)
                    return new[] { dtColumnas.Rows[0]["COLUMN_NAME"].ToString() };

                return new[] { "?" };
            }
            catch (Exception ex)
            {
                throw new Exception($"Error al obtener PK de {nombreTabla}: " + ex.Message, ex);
            }
        }

        private string ArmarClaveFila(DataRow row, string[] clavesPrimarias)
        {
            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < clavesPrimarias.Length; i++)
            {
                if (i > 0) sb.Append("|");
                sb.Append(row[clavesPrimarias[i]] != DBNull.Value ? row[clavesPrimarias[i]].ToString() : "NULL");
            }
            return sb.ToString();
        }

        private bool TablaExisteEnDB(string nombreTabla)
        {
            try
            {
                string consulta = "SELECT COUNT(*) FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = @NombreTabla AND TABLE_TYPE = 'BASE TABLE'";
                object resultado = dal._686DPEscalar(consulta, new List<SqlParameter> { new SqlParameter("@NombreTabla", nombreTabla) });
                return resultado != null && Convert.ToInt32(resultado) > 0;
            }
            catch
            {
                return false;
            }
        }

        private Dictionary<string, object> ArmarDiccionarioValores(DataRow row, List<DataColumn> columnasDatos)
        {
            var valores = new Dictionary<string, object>();

            foreach (DataColumn col in columnasDatos)
            {
                valores[col.ColumnName] = row[col];
            }

            return valores;
        }

        #endregion
    }
}
