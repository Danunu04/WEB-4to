using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using BE;
using DAL;

namespace MPP
{
    public class MPPUsuario
    {
        private DalGeneral dal;

        public MPPUsuario()
        {
            dal = new DalGeneral();
        }

        public Usuario ObtenerUsuario(string usuario)
        {
            try
            {
                string consulta = @"
            SELECT
                us.usr,
                us.contra,
                us.activo,
                ISNULL(ui.intentos, 0) AS intentos,
                ISNULL(us.rol, 4) AS rol,
                us.dvv,
                us.dvh
            FROM [GymApp].[dbo].[USUARIOS] as us
            LEFT JOIN [GymApp].[dbo].[USUARIO_Intentos] as ui
                ON us.usr = ui.usr
            WHERE us.usr = @Usuario";

                ArrayList parametros = new ArrayList
                {
                    new SqlParameter("@Usuario", usuario)
                };

                DataTable dt = dal._686DPConsultar(consulta, parametros);

                if (dt.Rows.Count > 0)
                {
                    DataRow row = dt.Rows[0];
                    int rolValue = row["rol"] != DBNull.Value ? Convert.ToInt32(row["rol"]) : 4;
                    return new Usuario(
                        row["usr"] != DBNull.Value ? row["usr"].ToString() : string.Empty,
                        row["contra"] != DBNull.Value ? row["contra"].ToString() : string.Empty,
                        Convert.ToBoolean(row["activo"]),
                        row["intentos"] != DBNull.Value ? Convert.ToInt32(row["intentos"]) : 0,
                        (Rol)rolValue,
                        row["dvv"] != DBNull.Value ? row["dvv"].ToString() : string.Empty,
                        row["dvh"] != DBNull.Value ? row["dvh"].ToString() : string.Empty
                    );
                }

                return null;
            }
            catch (Exception ex)
            {
                throw new Exception("Error al obtener el usuario: " + ex.Message, ex);
            }
        }

        public int ObtenerIntentos(string usuario)
        {
            try
            {
                string consulta = @"
                    SELECT intentos
                    FROM [GymApp].[dbo].[USUARIO_Intentos]
                    WHERE usr = @Usuario";

                ArrayList parametros = new ArrayList
                {
                    new SqlParameter("@Usuario", usuario)
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
                throw new Exception("Error al obtener los intentos: " + ex.Message, ex);
            }
        }

        public void AgregarIntento(string usuario)
        {
            try
            {
                string consulta = @"
                    UPDATE [GymApp].[dbo].[USUARIO_Intentos]
                    SET intentos = ISNULL(intentos, 0) + 1
                    WHERE usr = @Usuario";

                ArrayList parametros = new ArrayList
                {
                    new SqlParameter("@Usuario", usuario)
                };

                dal._686DPEscribir(consulta, parametros);
            }
            catch (Exception ex)
            {
                throw new Exception("Error al agregar intento: " + ex.Message, ex);
            }
        }

        public void ReestablecerIntentos(string usuario)
        {
            try
            {
                string consulta = @"
                    UPDATE [GymApp].[dbo].[USUARIO_Intentos]
                    SET intentos = 0
                    WHERE usr = @Usuario";

                ArrayList parametros = new ArrayList
                {
                    new SqlParameter("@Usuario", usuario)
                };

                dal._686DPEscribir(consulta, parametros);
            }
            catch (Exception ex)
            {
                throw new Exception("Error al reestablecer intentos: " + ex.Message, ex);
            }
        }

        public bool UsuarioEstaBloqueado(string usuario)
        {
            try
            {
                string consulta = @"
                    SELECT intentos
                    FROM [GymApp].[dbo].[USUARIO_Intentos]
                    WHERE usr = @Usuario";

                ArrayList parametros = new ArrayList
                {
                    new SqlParameter("@Usuario", usuario)
                };

                object resultado = dal._686DPEscalar(consulta, parametros);

                if (resultado != null && resultado != DBNull.Value)
                {
                    int intentos = Convert.ToInt32(resultado);
                    return intentos >= 3;
                }

                return false;
            }
            catch (Exception ex)
            {
                throw new Exception("Error al verificar si el usuario está bloqueado: " + ex.Message, ex);
            }
        }

        public bool UsuarioEstaActivo(string usuario)
        {
            try
            {
                string consulta = @"
                    SELECT activo
                    FROM [GymApp].[dbo].[USUARIOS]
                    WHERE usr = @Usuario";

                ArrayList parametros = new ArrayList
                {
                    new SqlParameter("@Usuario", usuario)
                };

                object resultado = dal._686DPEscalar(consulta, parametros);

                if (resultado != null && resultado != DBNull.Value)
                {
                    return Convert.ToBoolean(resultado);
                }

                return false;
            }
            catch (Exception ex)
            {
                throw new Exception("Error al verificar si el usuario está activo: " + ex.Message, ex);
            }
        }

        public string ObtenerContrasena(string usuario)
        {
            try
            {
                string consulta = @"
                    SELECT contra
                    FROM [GymApp].[dbo].[USUARIOS]
                    WHERE usr = @Usuario";

                ArrayList parametros = new ArrayList
                {
                    new SqlParameter("@Usuario", usuario)
                };

                object resultado = dal._686DPEscalar(consulta, parametros);

                if (resultado != null && resultado != DBNull.Value)
                {
                    return resultado.ToString();
                }

                return string.Empty;
            }
            catch (Exception ex)
            {
                throw new Exception("Error al obtener la contraseña: " + ex.Message, ex);
            }
        }

        public bool ContrasenaFueUtilizada(string usuario, string contrasenaHash)
        {
            try
            {
                string consulta = @"
            SELECT COUNT(*) as count
            FROM [GymApp].[dbo].[USUARIO_Contras]
            WHERE usr = @Usuario AND contra = @Contrasena";

                ArrayList parametros = new ArrayList
                {
                    new SqlParameter("@Usuario", usuario),
                    new SqlParameter("@Contrasena", contrasenaHash)
                };

                object resultado = dal._686DPEscalar(consulta, parametros);

                if (resultado != null && resultado != DBNull.Value)
                {
                    return Convert.ToInt32(resultado) > 0;
                }

                return false;
            }
            catch (Exception ex)
            {
                throw new Exception("Error al verificar historial de contraseñas: " + ex.Message, ex);
            }
        }

        public void GuardarContrasenaEnHistorial(string usuario, string contrasenaHash)
        {
            try
            {
                string consulta = @"
            INSERT INTO [GymApp].[dbo].[USUARIO_Contras] (usr, contra, dvv, dvh)
            VALUES (@Usuario, @Contrasena, '', '')";

                ArrayList parametros = new ArrayList
                {
                    new SqlParameter("@Usuario", usuario),
                    new SqlParameter("@Contrasena", contrasenaHash)
                };

                dal._686DPEscribir(consulta, parametros);
            }
            catch (Exception ex)
            {
                throw new Exception("Error al guardar contraseña en historial: " + ex.Message, ex);
            }
        }

        public void ActualizarContrasena(string usuario, string nuevaContrasenaHash)
        {
            try
            {
                string consulta = @"
            UPDATE [GymApp].[dbo].[USUARIOS]
            SET contra = @Contrasena
            WHERE usr = @Usuario";

                ArrayList parametros = new ArrayList
                {
                    new SqlParameter("@Usuario", usuario),
                    new SqlParameter("@Contrasena", nuevaContrasenaHash)
                };

                dal._686DPEscribir(consulta, parametros);
            }
            catch (Exception ex)
            {
                throw new Exception("Error al actualizar contraseña: " + ex.Message, ex);
            }
        }
    }
}