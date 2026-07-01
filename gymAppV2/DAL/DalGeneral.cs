using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DAL
{
    /// <summary>
    /// Capa de acceso a datos genérica para SQL Server.
    /// Implementa IDisposable para garantizar el cierre correcto de conexiones.
    /// Todos los métodos públicos usan List&lt;SqlParameter&gt; y cierran la conexión
    /// automáticamente tras cada operación.
    /// </summary>
    public class DalGeneral : IDisposable
    {
        private string cadenaConexion;
        private SqlConnection conn;
        private bool disposed = false;

        /// <summary>
        /// Nombre de la cadena de conexión en &lt;connectionStrings&gt; de Web.config.
        /// </summary>
        private const string NOMBRE_CONNECTION_STRING = "GymAppConnection";

        public DalGeneral()
        {
            try
            {
                // La cadena de conexión se lee desde Web.config para evitar valores hardcodeados.
                var settings = ConfigurationManager.ConnectionStrings[NOMBRE_CONNECTION_STRING];
                if (settings == null || string.IsNullOrEmpty(settings.ConnectionString))
                {
                    throw new ConfigurationErrorsException($"La cadena de conexión '{NOMBRE_CONNECTION_STRING}' no está configurada en Web.config.");
                }

                cadenaConexion = settings.ConnectionString;
                conn = new SqlConnection(cadenaConexion);
            }
            catch (SqlException ex)
            {
                throw new Exception("Error al inicializar la conexión con la base de datos. Contacte al administrador.", ex);
            }
            catch (Exception ex)
            {
                throw new Exception("Error inesperado al inicializar el acceso a datos. Contacte al administrador.", ex);
            }
        }

        /// <summary>
        /// Agrega parámetros a un comando SQL.
        /// </summary>
        private void AgregarParametros(SqlCommand cmd, List<SqlParameter> parametros)
        {
            if (parametros == null)
                return;

            foreach (SqlParameter dato in parametros)
            {
                if (dato == null)
                    continue;

                cmd.Parameters.AddWithValue(dato.ParameterName, dato.Value ?? DBNull.Value);
            }
        }

        /// <summary>
        /// Abre la conexión si no está abierta.
        /// </summary>
        private void AbrirConexion()
        {
            if (conn != null && conn.State != ConnectionState.Open)
            {
                conn.Open();
            }
        }

        /// <summary>
        /// Cierra la conexión si está abierta.
        /// </summary>
        private void CerrarConexion()
        {
            if (conn != null && conn.State == ConnectionState.Open)
            {
                conn.Close();
            }
        }

        /// <summary>
        /// Devuelve un mensaje genérico para el usuario final sin exponer detalles de SQL.
        /// El error original se conserva en InnerException para depuración.
        /// </summary>
        private Exception CrearExcepcionSegura(SqlException ex, string contexto)
        {
            // Códigos de error comunes de SQL Server que el usuario puede entender de forma abstracta.
            string mensajeUsuario;
            switch (ex.Number)
            {
                case 547: // FOREIGN KEY constraint conflict
                    mensajeUsuario = "No se puede realizar la operación porque el registro está relacionado con otros datos.";
                    break;
                case 2601: // Cannot insert duplicate key row (índice único)
                case 2627: // Violation of PRIMARY KEY constraint
                    mensajeUsuario = "Ya existe un registro con los mismos datos. Verifique la información ingresada.";
                    break;
                case 4060: // Cannot open database
                case 18456: // Login failed
                    mensajeUsuario = "No se pudo conectar con la base de datos. Contacte al administrador.";
                    break;
                case -2: // Timeout
                    mensajeUsuario = "La operación tardó demasiado. Intente nuevamente en unos momentos.";
                    break;
                default:
                    mensajeUsuario = "Ocurrió un error al acceder a la base de datos. Intente nuevamente o contacte al administrador.";
                    break;
            }

            return new Exception(mensajeUsuario, ex);
        }

        /// <summary>
        /// ADO Desconectado: ejecuta una consulta y devuelve un DataTable.
        /// La conexión se abre solo durante el Fill y se cierra inmediatamente.
        /// </summary>
        public DataTable _686DPConsultar(string consulta, List<SqlParameter> parametros)
        {
            DataTable dt = new DataTable();

            try
            {
                using (SqlCommand cmd = new SqlCommand(consulta, conn))
                {
                    cmd.CommandType = CommandType.Text;
                    AgregarParametros(cmd, parametros);

                    AbrirConexion();

                    using (SqlDataAdapter DA = new SqlDataAdapter(cmd))
                    {
                        DA.Fill(dt);
                    }
                }
            }
            catch (SqlException ex)
            {
                throw CrearExcepcionSegura(ex, "consulta");
            }
            catch (Exception ex)
            {
                throw new Exception("Error inesperado al consultar la información. Contacte al administrador.", ex);
            }
            finally
            {
                CerrarConexion();
            }

            return dt;
        }

        /// <summary>
        /// ADO Desconectado: ejecuta un stored procedure de lectura y devuelve un DataTable.
        /// La conexión se abre solo durante el Fill y se cierra inmediatamente.
        /// </summary>
        public DataTable _686DPConsultarSP(string nombreSP, List<SqlParameter> parametros)
        {
            DataTable dt = new DataTable();

            try
            {
                using (SqlCommand cmd = new SqlCommand(nombreSP, conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    AgregarParametros(cmd, parametros);

                    AbrirConexion();

                    using (SqlDataAdapter DA = new SqlDataAdapter(cmd))
                    {
                        DA.Fill(dt);
                    }
                }
            }
            catch (SqlException ex)
            {
                throw CrearExcepcionSegura(ex, "stored procedure");
            }
            catch (Exception ex)
            {
                throw new Exception("Error inesperado al ejecutar el proceso en base de datos. Contacte al administrador.", ex);
            }
            finally
            {
                CerrarConexion();
            }

            return dt;
        }

        /// <summary>
        /// ADO Conectado: ejecuta un stored procedure que modifica datos.
        /// Usa ExecuteNonQuery para INSERT/UPDATE/DELETE y control transaccional.
        /// </summary>
        public void _686DPEjecutar(string nombreSP, List<SqlParameter> parametros)
        {
            try
            {
                using (SqlCommand cmd = new SqlCommand(nombreSP, conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    AgregarParametros(cmd, parametros);

                    AbrirConexion();
                    cmd.ExecuteNonQuery();
                }
            }
            catch (SqlException ex)
            {
                throw CrearExcepcionSegura(ex, "stored procedure");
            }
            catch (Exception ex)
            {
                throw new Exception("Error inesperado al ejecutar el proceso en base de datos. Contacte al administrador.", ex);
            }
            finally
            {
                CerrarConexion();
            }
        }

        /// <summary>
        /// ADO Conectado: ejecuta una consulta escalar (COUNT, MAX, etc.).
        /// Usa ExecuteScalar para valores únicos atómicos.
        /// </summary>
        public object _686DPEscalar(string consulta, List<SqlParameter> parametros)
        {
            try
            {
                using (SqlCommand cmd = new SqlCommand(consulta, conn))
                {
                    cmd.CommandType = CommandType.Text;
                    AgregarParametros(cmd, parametros);

                    AbrirConexion();
                    return cmd.ExecuteScalar();
                }
            }
            catch (SqlException ex)
            {
                throw CrearExcepcionSegura(ex, "consulta escalar");
            }
            catch (Exception ex)
            {
                throw new Exception("Error inesperado al obtener el valor. Contacte al administrador.", ex);
            }
            finally
            {
                CerrarConexion();
            }
        }

        /// <summary>
        /// ADO Conectado: ejecuta una sentencia SQL que modifica datos.
        /// Usa ExecuteNonQuery para INSERT/UPDATE/DELETE directos.
        /// </summary>
        public void _686DPEscribir(string consulta, List<SqlParameter> parametros)
        {
            try
            {
                using (SqlCommand cmd = new SqlCommand(consulta, conn))
                {
                    cmd.CommandType = CommandType.Text;
                    AgregarParametros(cmd, parametros);

                    AbrirConexion();
                    cmd.ExecuteNonQuery();
                }
            }
            catch (SqlException ex)
            {
                throw CrearExcepcionSegura(ex, "escritura");
            }
            catch (Exception ex)
            {
                throw new Exception("Error inesperado al guardar la información. Contacte al administrador.", ex);
            }
            finally
            {
                CerrarConexion();
            }
        }

        /// <summary>
        /// Libera los recursos no administrados, cerrando la conexión si está abierta.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposed)
                return;

            if (disposing)
            {
                CerrarConexion();
                conn?.Dispose();
            }

            disposed = true;
        }

        ~DalGeneral()
        {
            Dispose(false);
        }
    }
}
