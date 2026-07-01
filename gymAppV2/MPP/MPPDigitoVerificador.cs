using System;
using System.Collections;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Globalization;
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
                    SELECT idDigitoVerificador, nombreTabla, dvhTabla, dvvTabla, fechaCalculo
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
        /// </summary>
        public void GuardarControl(string nombreTabla, string dvhTabla, string dvvTabla)
        {
            try
            {
                string consulta = @"
                    IF EXISTS (SELECT 1 FROM [GymApp].[dbo].[DigitoVerificador] WHERE nombreTabla = @NombreTabla)
                        UPDATE [GymApp].[dbo].[DigitoVerificador]
                        SET dvhTabla = @DVHTabla,
                            dvvTabla = @DVVTabla,
                            fechaCalculo = GETDATE()
                        WHERE nombreTabla = @NombreTabla
                    ELSE
                        INSERT INTO [GymApp].[dbo].[DigitoVerificador] (nombreTabla, dvhTabla, dvvTabla)
                        VALUES (@NombreTabla, @DVHTabla, @DVVTabla)";

                List<SqlParameter> parametros = new List<SqlParameter>
                {
                    new SqlParameter("@NombreTabla", nombreTabla),
                    new SqlParameter("@DVHTabla", dvhTabla),
                    new SqlParameter("@DVVTabla", dvvTabla)
                };

                dal._686DPEscribir(consulta, parametros);
            }
            catch (Exception ex)
            {
                throw new Exception($"Error al guardar control de {nombreTabla}: " + ex.Message, ex);
            }
        }

        #endregion

        #region Verificación de integridad

        /// <summary>
        /// Verifica la integridad de todas las tablas registradas en DigitoVerificador.
        /// Si una tabla no tiene control, se reporta como advertencia.
        /// Si el control no coincide, se evalúa fila por fila.
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
        /// Verifica la integridad de una tabla específica. Primero compara el hash de tabla;
        /// si falla, recorre fila por fila.
        /// </summary>
        public List<ResultadoVerificacionDV> VerificarIntegridadTabla(string nombreTabla)
        {
            var resultados = new List<ResultadoVerificacionDV>();

            try
            {
                DataRow control = ObtenerControlPorTabla(nombreTabla);
                if (control == null)
                {
                    resultados.Add(new ResultadoVerificacionDV
                    {
                        NombreTabla = nombreTabla,
                        ClaveFila = "-",
                        Campo = "-",
                        EsValido = false,
                        Mensaje = "La tabla no tiene registro de control. Ejecutar recálculo masivo."
                    });
                    return resultados;
                }

                string dvhTablaAlmacenado = control["dvhTabla"].ToString();
                string dvvTablaAlmacenado = control["dvvTabla"].ToString();

                CalcularHashTabla(nombreTabla, out string dvhTablaCalculado, out string dvvTablaCalculado);

                bool dvhOk = dvhTablaAlmacenado.Equals(dvhTablaCalculado, StringComparison.OrdinalIgnoreCase);
                bool dvvOk = dvvTablaAlmacenado.Equals(dvvTablaCalculado, StringComparison.OrdinalIgnoreCase);

                if (dvhOk && dvvOk)
                {
                    resultados.Add(new ResultadoVerificacionDV
                    {
                        NombreTabla = nombreTabla,
                        ClaveFila = "-",
                        Campo = "-",
                        EsValido = true,
                        Mensaje = "Tabla verificada correctamente."
                    });
                    return resultados;
                }

                // Falló el hash de tabla: evaluar fila por fila.
                resultados.Add(new ResultadoVerificacionDV
                {
                    NombreTabla = nombreTabla,
                    ClaveFila = "-",
                    Campo = "-",
                    EsValido = false,
                    Mensaje = $"Hash de tabla no coincide. DVH esperado: {dvhTablaAlmacenado}, calculado: {dvhTablaCalculado}; DVV esperado: {dvvTablaAlmacenado}, calculado: {dvvTablaCalculado}."
                });

                resultados.AddRange(VerificarFilasTabla(nombreTabla));
            }
            catch (Exception ex)
            {
                resultados.Add(new ResultadoVerificacionDV
                {
                    NombreTabla = nombreTabla,
                    ClaveFila = "-",
                    Campo = "-",
                    EsValido = false,
                    Mensaje = "Error al verificar tabla: " + ex.Message
                });
            }

            return resultados;
        }

        /// <summary>
        /// Recorre las filas de una tabla y verifica su DVH/DVV.
        /// </summary>
        private List<ResultadoVerificacionDV> VerificarFilasTabla(string nombreTabla)
        {
            var resultados = new List<ResultadoVerificacionDV>();

            try
            {
                DataTable dt = ObtenerFilasParaVerificacion(nombreTabla);
                if (dt == null || dt.Rows.Count == 0)
                    return resultados;

                List<DataColumn> columnasDatos = dt.Columns.Cast<DataColumn>()
                    .Where(c => !c.ColumnName.Equals("dvv", StringComparison.OrdinalIgnoreCase)
                             && !c.ColumnName.Equals("dvh", StringComparison.OrdinalIgnoreCase))
                    .ToList();

                string[] clavesPrimarias = ObtenerClavesPrimarias(nombreTabla);

                foreach (DataRow row in dt.Rows)
                {
                    string claveFila = ArmarClaveFila(row, clavesPrimarias);

                    string dvhAlmacenado = row["dvh"] != DBNull.Value ? row["dvh"].ToString() : string.Empty;
                    string dvvAlmacenado = row["dvv"] != DBNull.Value ? row["dvv"].ToString() : string.Empty;

                    var valores = ArmarDiccionarioValores(row, columnasDatos);

                    dvManager.CalcularAmbos(valores, out string dvhCalculado, out string dvvCalculado);

                    bool dvhOk = !string.IsNullOrEmpty(dvhAlmacenado) && dvhAlmacenado.Equals(dvhCalculado, StringComparison.OrdinalIgnoreCase);
                    bool dvvOk = !string.IsNullOrEmpty(dvvAlmacenado) && dvvAlmacenado.Equals(dvvCalculado, StringComparison.OrdinalIgnoreCase);

                    if (dvhOk && dvvOk)
                        continue;

                    // Fila corrupta: intentar identificar campo problemático.
                    if (!dvhOk)
                    {
                        resultados.Add(new ResultadoVerificacionDV
                        {
                            NombreTabla = nombreTabla,
                            ClaveFila = claveFila,
                            Campo = "DVH (fila completa)",
                            EsValido = false,
                            Mensaje = "DVH de fila no coincide.",
                            DVHAlmacenado = dvhAlmacenado,
                            DVHCalculado = dvhCalculado,
                            DVVAlmacenado = dvvAlmacenado,
                            DVVCalculado = dvvCalculado
                        });
                    }

                    if (!dvvOk)
                    {
                        resultados.Add(new ResultadoVerificacionDV
                        {
                            NombreTabla = nombreTabla,
                            ClaveFila = claveFila,
                            Campo = "DVV (fila completa)",
                            EsValido = false,
                            Mensaje = "DVV de fila no coincide.",
                            DVHAlmacenado = dvhAlmacenado,
                            DVHCalculado = dvhCalculado,
                            DVVAlmacenado = dvvAlmacenado,
                            DVVCalculado = dvvCalculado
                        });
                    }

                    // Identificar campo específico comparando hash individual.
                    foreach (DataColumn col in columnasDatos)
                    {
                        string valorCampo = dvManager.NormalizarValor(row[col]);
                        string hashCampoActual = criptoManager.GenerarHashSHA256(valorCampo);

                        // Si el DVV actual no se forma con este hash, el campo es sospechoso.
                        // Para detectar cuál cambió comparamos contra un DVV recalculado sin este campo.
                        var valoresSinCampo = new Dictionary<string, object>(valores);
                        valoresSinCampo.Remove(col.ColumnName);
                        dvManager.CalcularDVV(valoresSinCampo); // no lo usamos directamente

                        // Calculamos el hash de campo esperado original a partir del DVV almacenado:
                        // DVV_alm = SHA256( hash(c1) + hash(c2) + ... + hash(cN) )
                        // Si cambió un solo campo, el hash esperado para ese campo sería:
                        // hash_esperado_campo = SHA256(DVV_alm) ??? no es reversible.
                        // Estrategia alternativa: marcar el campo si su valor actual es NULL/empty
                        // o si tiene caracteres sospechosos; es una heurística.
                        // Implementación simple: reportamos todos los campos no-clave de la fila
                        // como "revisar", con el hash actual del campo.

                        resultados.Add(new ResultadoVerificacionDV
                        {
                            NombreTabla = nombreTabla,
                            ClaveFila = claveFila,
                            Campo = col.ColumnName,
                            EsValido = false,
                            Mensaje = $"Campo sospechoso (hash actual: {hashCampoActual}). Revisar valor."
                        });
                    }
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
        /// Devuelve las tablas del schema que tienen columnas dvv y dvh
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
                            AND c.COLUMN_NAME = 'dvv')
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
        /// Devuelve un resumen del estado de control y de filas con DV vacíos
        /// para todas las tablas que tienen columnas dvv/dvh.
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
                    INNER JOIN INFORMATION_SCHEMA.COLUMNS cdvv
                        ON t.TABLE_NAME = cdvv.TABLE_NAME AND cdvv.COLUMN_NAME = 'dvv'
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
                    int filasDVVVacio = 0;

                    try
                    {
                        string consultaConteo = $@"
                            SELECT
                                COUNT(*) AS TotalFilas,
                                SUM(CASE WHEN [dvh] = '' OR [dvh] IS NULL THEN 1 ELSE 0 END) AS FilasDVHVacio,
                                SUM(CASE WHEN [dvv] = '' OR [dvv] IS NULL THEN 1 ELSE 0 END) AS FilasDVVVacio
                            FROM [GymApp].[dbo].[{nombreTabla}]";

                        DataTable dtConteo = dal._686DPConsultar(consultaConteo, new List<SqlParameter>());

                        if (dtConteo.Rows.Count > 0)
                        {
                            DataRow r = dtConteo.Rows[0];
                            totalFilas = r["TotalFilas"] != DBNull.Value ? Convert.ToInt32(r["TotalFilas"]) : 0;
                            filasDVHVacio = r["FilasDVHVacio"] != DBNull.Value ? Convert.ToInt32(r["FilasDVHVacio"]) : 0;
                            filasDVVVacio = r["FilasDVVVacio"] != DBNull.Value ? Convert.ToInt32(r["FilasDVVVacio"]) : 0;
                        }
                    }
                    catch (Exception)
                    {
                        // Si no se puede contar una tabla en particular, se reporta con valores -1.
                        totalFilas = -1;
                        filasDVHVacio = -1;
                        filasDVVVacio = -1;
                    }

                    estado.Add(new EstadoControlDV(nombreTabla, tieneControl, totalFilas, filasDVHVacio, filasDVVVacio, fechaCalculo));
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
        /// Recalcula DVH/DVV de todas las filas de todas las tablas con control
        /// y actualiza la tabla DigitoVerificador.
        /// Solo actualiza los dígitos; no restaura valores de datos.
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
        /// Recalcula DVH/DVV de todas las filas de una tabla y actualiza su control.
        /// Para tablas con datos encriptados se puede indicar que solo se actualice
        /// el registro de control, dejando los dígitos de fila calculados por el
        /// MPP especializado correspondiente.
        /// </summary>
        public void RecalcularDigitosTabla(string nombreTabla, bool actualizarFilas = true)
        {
            try
            {
                DataTable dt = ObtenerFilasParaVerificacion(nombreTabla);
                if (dt == null || dt.Rows.Count == 0)
                {
                    GuardarControl(nombreTabla,
                        criptoManager.GenerarHashSHA256(string.Empty),
                        criptoManager.GenerarHashSHA256(string.Empty));
                    return;
                }

                List<DataColumn> columnasDatos = dt.Columns.Cast<DataColumn>()
                    .Where(c => !c.ColumnName.Equals("dvv", StringComparison.OrdinalIgnoreCase)
                             && !c.ColumnName.Equals("dvh", StringComparison.OrdinalIgnoreCase))
                    .ToList();

                StringBuilder dvhConcat = new StringBuilder();
                StringBuilder dvvConcat = new StringBuilder();

                foreach (DataRow row in dt.Rows)
                {
                    string dvh;
                    string dvv;

                    if (actualizarFilas)
                    {
                        var valores = ArmarDiccionarioValores(row, columnasDatos);
                        dvManager.CalcularAmbos(valores, out dvh, out dvv);

                        string claveFila = ArmarClaveFila(row, ObtenerClavesPrimarias(nombreTabla));
                        ActualizarDigitosFila(nombreTabla, claveFila, dvh, dvv);
                    }
                    else
                    {
                        dvh = row["dvh"] != DBNull.Value ? row["dvh"].ToString() : string.Empty;
                        dvv = row["dvv"] != DBNull.Value ? row["dvv"].ToString() : string.Empty;
                    }

                    dvhConcat.Append(dvh);
                    dvvConcat.Append(dvv);
                }

                string dvhTabla = criptoManager.GenerarHashSHA256(dvhConcat.ToString());
                string dvvTabla = criptoManager.GenerarHashSHA256(dvvConcat.ToString());

                GuardarControl(nombreTabla, dvhTabla, dvvTabla);
            }
            catch (Exception ex)
            {
                throw new Exception($"Error al recalcular dígitos de {nombreTabla}: " + ex.Message, ex);
            }
        }

        /// <summary>
        /// Actualiza dvv/dvh de una fila específica usando su clave primaria.
        /// </summary>
        private void ActualizarDigitosFila(string nombreTabla, string claveFila, string dvh, string dvv)
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
                    new SqlParameter("@DVH", dvh),
                    new SqlParameter("@DVV", dvv)
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
                    SET dvh = @DVH, dvv = @DVV
                    WHERE {where}";

                dal._686DPEscribir(consulta, parametros);
            }
            catch (Exception ex)
            {
                throw new Exception($"Error al actualizar dígitos de fila {claveFila} en {nombreTabla}: " + ex.Message, ex);
            }
        }

        #endregion

        #region Backup

        /// <summary>
        /// Restaura la base de datos desde un backup previo.
        /// ADVERTENCIA: operación destructiva. Requiere permisos de sysadmin o dbcreator.
        /// La restauración se ejecuta conectado a la base master para poder reemplazar GymApp.
        /// </summary>
        public void RestaurarBackup(string rutaBackup)
        {
            if (string.IsNullOrWhiteSpace(rutaBackup))
                throw new ArgumentException("La ruta del backup no puede estar vacía.", nameof(rutaBackup));

            try
            {
                string consulta = @"
                    ALTER DATABASE [GymApp] SET SINGLE_USER WITH ROLLBACK IMMEDIATE;
                    RESTORE DATABASE [GymApp] FROM DISK = @RutaBackup WITH REPLACE;
                    ALTER DATABASE [GymApp] SET MULTI_USER;";

                var settings = ConfigurationManager.ConnectionStrings["GymAppConnection"];
                if (settings == null || string.IsNullOrEmpty(settings.ConnectionString))
                    throw new Exception("No se encontró la cadena de conexión 'GymAppConnection' en Web.config.");

                SqlConnectionStringBuilder builder = new SqlConnectionStringBuilder(settings.ConnectionString)
                {
                    InitialCatalog = "master"
                };

                using (SqlConnection connMaster = new SqlConnection(builder.ConnectionString))
                {
                    connMaster.Open();

                    using (SqlCommand cmd = new SqlCommand(consulta, connMaster))
                    {
                        cmd.Parameters.AddWithValue("@RutaBackup", rutaBackup);
                        cmd.ExecuteNonQuery();
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Error al restaurar el backup. Verifique permisos y ruta: " + ex.Message, ex);
            }
        }

        #endregion

        #region Utilidades privadas

        /// <summary>
        /// Calcula el hash agregado de una tabla a partir de los dvh/dvv de sus filas.
        /// </summary>
        private void CalcularHashTabla(string nombreTabla, out string dvhTabla, out string dvvTabla)
        {
            try
            {
                DataTable dt = ObtenerFilasParaVerificacion(nombreTabla);

                if (dt == null || dt.Rows.Count == 0)
                {
                    dvhTabla = criptoManager.GenerarHashSHA256(string.Empty);
                    dvvTabla = criptoManager.GenerarHashSHA256(string.Empty);
                    return;
                }

                StringBuilder dvhConcat = new StringBuilder();
                StringBuilder dvvConcat = new StringBuilder();

                foreach (DataRow row in dt.Rows)
                {
                    string dvh = row["dvh"] != DBNull.Value ? row["dvh"].ToString() : string.Empty;
                    string dvv = row["dvv"] != DBNull.Value ? row["dvv"].ToString() : string.Empty;

                    dvhConcat.Append(dvh);
                    dvvConcat.Append(dvv);
                }

                dvhTabla = criptoManager.GenerarHashSHA256(dvhConcat.ToString());
                dvvTabla = criptoManager.GenerarHashSHA256(dvvConcat.ToString());
            }
            catch (Exception ex)
            {
                throw new Exception($"Error al calcular hash de tabla {nombreTabla}: " + ex.Message, ex);
            }
        }

        /// <summary>
        /// Obtiene todas las filas de una tabla incluyendo dvv y dvh.
        /// </summary>
        private DataTable ObtenerFilasParaVerificacion(string nombreTabla)
        {
            try
            {
                string consulta = $@"
                    SELECT *
                    FROM [GymApp].[dbo].[{nombreTabla}]";

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

                // Fallback: primera columna de la tabla.
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

        /// <summary>
        /// Arma la clave primaria de una fila concatenando valores de las PK.
        /// </summary>
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

        /// <summary>
        /// Arma el diccionario de valores de una fila para el cálculo de DVH/DVV.
        /// </summary>
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
