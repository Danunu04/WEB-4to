using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DAL
{
    public class DalGeneral
    {
        private string cadenaConexion;

        public SqlConnection conn;
        public SqlCommand cmd;

        public DalGeneral()
        {
            try
            {
                //string server = Environment.GetEnvironmentVariable("DB_SERVER");
                //string database = Environment.GetEnvironmentVariable("DB_DATABASE");
                //string user = Environment.GetEnvironmentVariable("DB_USER");
                //string password = Environment.GetEnvironmentVariable("DB_PASSWORD");
                //string auth = Environment.GetEnvironmentVariable("DB_AUTH");

                //cadenaConexion = ConstruirConnectionString(server, database, user, password, auth);

                cadenaConexion = "Data Source=.;Initial Catalog=GymApp;Integrated Security=True";

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

        private string ConstruirConnectionString(string server, string database, string user, string password, string auth)
        {
            bool windowsAuth = !string.IsNullOrEmpty(auth) && auth == "1";

            return $"Data Source={server};Initial Catalog={database};Integrated Security=True;";
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

        public DataTable _686DPConsultar(string consulta, ArrayList parametros)
        {
            DataTable dt = new DataTable();

            try
            {
                using (SqlCommand cmd = new SqlCommand(consulta, conn))
                {
                    cmd.CommandType = CommandType.Text;

                    if (parametros != null)
                    {
                        foreach (SqlParameter dato in parametros)
                        {
                            cmd.Parameters.AddWithValue(dato.ParameterName, dato.Value ?? DBNull.Value);
                        }
                    }

                    if (conn.State != ConnectionState.Open)
                    {
                        conn.Open();
                    }

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
                if (conn.State == ConnectionState.Open)
                {
                    conn.Close();
                }
            }

            return dt;
        }

        public DataTable _686DPConsultarSP(string nombreSP, ArrayList parametros)
        {
            DataTable dt = new DataTable();

            try
            {
                using (SqlCommand cmd = new SqlCommand(nombreSP, conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;

                    if (parametros != null)
                    {
                        foreach (SqlParameter dato in parametros)
                        {
                            cmd.Parameters.AddWithValue(dato.ParameterName, dato.Value ?? DBNull.Value);
                        }
                    }

                    if (conn.State != ConnectionState.Open)
                    {
                        conn.Open();
                    }

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
                if (conn.State == ConnectionState.Open)
                {
                    conn.Close();
                }
            }

            return dt;
        }

        public void _686DPEjecutar(string nombreSP, ArrayList parametros)
        {
            try
            {
                using (SqlCommand cmd = new SqlCommand(nombreSP, conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;

                    if (parametros != null)
                    {
                        foreach (SqlParameter dato in parametros)
                        {
                            cmd.Parameters.AddWithValue(dato.ParameterName, dato.Value ?? DBNull.Value);
                        }
                    }

                    if (conn.State != ConnectionState.Open)
                    {
                        conn.Open();
                    }

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
                if (conn.State == ConnectionState.Open)
                {
                    conn.Close();
                }
            }
        }

        public object _686DPEscalar(string consulta, ArrayList parametros)
        {
            try
            {
                using (SqlCommand cmd = new SqlCommand(consulta, conn))
                {
                    cmd.CommandType = CommandType.Text;

                    if (parametros != null)
                    {
                        foreach (SqlParameter dato in parametros)
                        {
                            cmd.Parameters.AddWithValue(dato.ParameterName, dato.Value ?? DBNull.Value);
                        }
                    }

                    if (conn.State != ConnectionState.Open)
                    {
                        conn.Open();
                    }

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
                if (conn.State == ConnectionState.Open)
                {
                    conn.Close();
                }
            }
        }

        public void _686DPEscribir(string consulta, ArrayList parametros)
        {
            try
            {
                using (SqlCommand cmd = new SqlCommand(consulta, conn))
                {
                    cmd.CommandType = CommandType.Text;

                    if (parametros != null)
                    {
                        foreach (SqlParameter dato in parametros)
                        {
                            cmd.Parameters.AddWithValue(dato.ParameterName, dato.Value ?? DBNull.Value);
                        }
                    }

                    if (conn.State != ConnectionState.Open)
                    {
                        conn.Open();
                    }

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
                if (conn.State == ConnectionState.Open)
                {
                    conn.Close();
                }
            }
        }

    }
}
