using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using BE;
using DAL;
using static BE.ConstantesSeguridad;

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
                    SELECT us.usr, us.contra, us.activo, us.rol, us.dvv, us.dvh,
                           ISNULL(ui.intentos, 0) AS intentos
                    FROM [GymApp].[dbo].[USUARIOS] us
                    LEFT JOIN [GymApp].[dbo].[USUARIO_Intentos] ui ON us.usr = ui.usr
                    WHERE us.usr = @Usuario";

                ArrayList parametros = new ArrayList
                {
                    new SqlParameter("@Usuario", usuario)
                };

                DataTable dt = dal._686DPConsultar(consulta, parametros);

                if (dt.Rows.Count > 0)
                {
                    DataRow row = dt.Rows[0];
                    int intentos = Convert.ToInt32(row["intentos"]);
                    bool bloqueado = intentos >= MAX_INTENTOS_LOGIN;

                    return new Usuario(
                        row["usr"].ToString(),
                        row["contra"].ToString(),
                        Convert.ToBoolean(row["activo"]),
                        bloqueado,
                        intentos,
                        Convert.ToInt32(row["rol"]),
                        string.Empty,
                        0,
                        string.Empty,
                        string.Empty,
                        string.Empty,
                        string.Empty,
                        null,
                        row["dvv"] != DBNull.Value ? row["dvv"].ToString() : string.Empty,
                        row["dvh"] != DBNull.Value ? row["dvh"].ToString() : string.Empty,
                        false
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
                    SELECT ISNULL(intentos, 0)
                    FROM [GymApp].[dbo].[USUARIO_Intentos]
                    WHERE usr = @Usuario";

                ArrayList parametros = new ArrayList
                {
                    new SqlParameter("@Usuario", usuario)
                };

                object resultado = dal._686DPEscalar(consulta, parametros);
                return resultado != null && resultado != DBNull.Value ? Convert.ToInt32(resultado) : 0;
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
                // MERGE: si ya existe la fila en USUARIO_Intentos la actualiza, si no la crea
                string consulta = @"
                    MERGE INTO [GymApp].[dbo].[USUARIO_Intentos] AS target
                    USING (VALUES (@Usuario)) AS source (usr) ON target.usr = source.usr
                    WHEN MATCHED THEN
                        UPDATE SET intentos = intentos + 1
                    WHEN NOT MATCHED THEN
                        INSERT (usr, intentos, dvv, dvh) VALUES (@Usuario, 1, '', '');";

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
                int intentos = ObtenerIntentos(usuario);
                return intentos >= MAX_INTENTOS_LOGIN;
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
                    SELECT activo FROM [GymApp].[dbo].[USUARIOS] WHERE usr = @Usuario";

                ArrayList parametros = new ArrayList
                {
                    new SqlParameter("@Usuario", usuario)
                };

                object resultado = dal._686DPEscalar(consulta, parametros);
                return resultado != null && resultado != DBNull.Value && Convert.ToBoolean(resultado);
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
                    SELECT contra FROM [GymApp].[dbo].[USUARIOS] WHERE usr = @Usuario";

                ArrayList parametros = new ArrayList
                {
                    new SqlParameter("@Usuario", usuario)
                };

                object resultado = dal._686DPEscalar(consulta, parametros);
                return resultado != null && resultado != DBNull.Value ? resultado.ToString() : string.Empty;
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
                    SELECT COUNT(*) FROM [GymApp].[dbo].[USUARIO_Contras]
                    WHERE usr = @Usuario AND contra = @Contrasena";

                ArrayList parametros = new ArrayList
                {
                    new SqlParameter("@Usuario", usuario),
                    new SqlParameter("@Contrasena", contrasenaHash)
                };

                object resultado = dal._686DPEscalar(consulta, parametros);
                return resultado != null && resultado != DBNull.Value && Convert.ToInt32(resultado) > 0;
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
                string insertar = @"
                    INSERT INTO [GymApp].[dbo].[USUARIO_Contras] (usr, contra, dvv, dvh)
                    VALUES (@Usuario, @Contrasena, '', '')";

                ArrayList parametrosInsertar = new ArrayList
                {
                    new SqlParameter("@Usuario", usuario),
                    new SqlParameter("@Contrasena", contrasenaHash)
                };

                dal._686DPEscribir(insertar, parametrosInsertar);

                string limpiar = @"
                    WITH ContrasOrdenadas AS (
                        SELECT TOP 1000000 [usr], [contra], [dvv], [dvh],
                            ROW_NUMBER() OVER (PARTITION BY [usr] ORDER BY (SELECT NULL)) AS rn
                        FROM [GymApp].[dbo].[USUARIO_Contras]
                        WHERE [usr] = @Usuario
                    )
                    DELETE FROM ContrasOrdenadas WHERE rn > @MaxHistorial";

                ArrayList parametrosLimpiar = new ArrayList
                {
                    new SqlParameter("@Usuario", usuario),
                    new SqlParameter("@MaxHistorial", MAX_HISTORIAL_CONTRASENAS)
                };

                dal._686DPEscribir(limpiar, parametrosLimpiar);
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
                    SET contra = @Contrasena WHERE usr = @Usuario";

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

        public void FinalizarPrimerLogin(string usuario)
        {
            // La columna primerLogin no existe en el schema actual — no-op
        }

        public void CrearUsuario(Usuario usuario)
        {
            try
            {
                string consulta = @"
                    INSERT INTO [GymApp].[dbo].[USUARIOS] (usr, contra, activo, rol, dvv, dvh)
                    VALUES (@Usuario, @Contrasena, @Activo, @Rol, '', '')";

                ArrayList parametros = new ArrayList
                {
                    new SqlParameter("@Usuario", usuario.USUARIO_Usuario),
                    new SqlParameter("@Contrasena", usuario.USUARIO_Contras),
                    new SqlParameter("@Activo", usuario.USUARIO_Activo),
                    new SqlParameter("@Rol", usuario.USUARIO_Rol)
                };

                dal._686DPEscribir(consulta, parametros);
            }
            catch (Exception ex)
            {
                throw new Exception("Error al crear usuario: " + ex.Message, ex);
            }
        }

        public List<BE.UsuarioGestion> ListarUsuarios()
        {
            try
            {
                string consulta = @"
                    SELECT u.usr, u.activo, u.rol, u.dvv, u.dvh,
                           ISNULL(ui.intentos, 0) AS intentos
                    FROM [GymApp].[dbo].[USUARIOS] u
                    LEFT JOIN [GymApp].[dbo].[USUARIO_Intentos] ui ON u.usr = ui.usr
                    ORDER BY u.usr";

                DataTable dt = dal._686DPConsultar(consulta, new ArrayList());
                List<BE.UsuarioGestion> usuarios = new List<BE.UsuarioGestion>();

                foreach (DataRow row in dt.Rows)
                {
                    int intentos = Convert.ToInt32(row["intentos"]);
                    usuarios.Add(new BE.UsuarioGestion(
                        row["usr"].ToString(),
                        string.Empty,
                        string.Empty,
                        Convert.ToBoolean(row["activo"]),
                        intentos >= MAX_INTENTOS_LOGIN,
                        intentos,
                        row["dvv"] != DBNull.Value ? row["dvv"].ToString() : string.Empty,
                        row["dvh"] != DBNull.Value ? row["dvh"].ToString() : string.Empty
                    ));
                }

                return usuarios;
            }
            catch (Exception ex)
            {
                throw new Exception("Error al listar usuarios: " + ex.Message, ex);
            }
        }

        public bool UsuarioExiste(string usuario)
        {
            try
            {
                string consulta = @"
                    SELECT COUNT(*) FROM [GymApp].[dbo].[USUARIOS] WHERE usr = @Usuario";

                ArrayList parametros = new ArrayList
                {
                    new SqlParameter("@Usuario", usuario)
                };

                object resultado = dal._686DPEscalar(consulta, parametros);
                return resultado != null && resultado != DBNull.Value && Convert.ToInt32(resultado) > 0;
            }
            catch (Exception ex)
            {
                throw new Exception("Error al verificar si existe el usuario: " + ex.Message, ex);
            }
        }

        public List<BE.UsuarioGestion> ListarUsuariosClientesSinAlumno()
        {
            try
            {
                // En el schema actual, clientes son rol=4. Busca los que no tienen alumno asociado en Alumnos.usr
                string consulta = @"
                    SELECT u.usr, u.activo, u.rol, u.dvv, u.dvh
                    FROM [GymApp].[dbo].[USUARIOS] u
                    LEFT JOIN [GymApp].[dbo].[Alumnos] a ON u.usr = a.usr
                    WHERE u.rol = 4 AND u.activo = 1 AND a.usr IS NULL
                    ORDER BY u.usr";

                DataTable dt = dal._686DPConsultar(consulta, new ArrayList());
                List<BE.UsuarioGestion> usuarios = new List<BE.UsuarioGestion>();

                foreach (DataRow row in dt.Rows)
                {
                    usuarios.Add(new BE.UsuarioGestion(
                        row["usr"].ToString(),
                        string.Empty,
                        string.Empty,
                        Convert.ToBoolean(row["activo"]),
                        false,
                        0,
                        row["dvv"] != DBNull.Value ? row["dvv"].ToString() : string.Empty,
                        row["dvh"] != DBNull.Value ? row["dvh"].ToString() : string.Empty
                    ));
                }

                return usuarios;
            }
            catch (Exception ex)
            {
                throw new Exception("Error al listar usuarios clientes sin alumno: " + ex.Message, ex);
            }
        }

        public void ActualizarEstado(string usuario, bool activo)
        {
            try
            {
                string consulta = @"
                    UPDATE [GymApp].[dbo].[USUARIOS] SET activo = @Activo WHERE usr = @Usuario";

                ArrayList parametros = new ArrayList
                {
                    new SqlParameter("@Usuario", usuario),
                    new SqlParameter("@Activo", activo)
                };

                dal._686DPEscribir(consulta, parametros);
            }
            catch (Exception ex)
            {
                throw new Exception("Error al actualizar estado del usuario: " + ex.Message, ex);
            }
        }

        public DateTime? ObtenerFechaNacimiento(string usuario)
        {
            // La columna fechaNacimiento no existe en el schema actual
            return null;
        }

        public void ActualizarUsuario(Usuario usuario)
        {
            try
            {
                // Solo actualizamos columnas que existen en el schema actual
                string consulta = @"
                    UPDATE [GymApp].[dbo].[USUARIOS]
                    SET activo = @Activo, rol = @Rol
                    WHERE usr = @Usuario";

                ArrayList parametros = new ArrayList
                {
                    new SqlParameter("@Usuario", usuario.USUARIO_Usuario),
                    new SqlParameter("@Activo", usuario.USUARIO_Activo),
                    new SqlParameter("@Rol", usuario.USUARIO_Rol)
                };

                dal._686DPEscribir(consulta, parametros);
            }
            catch (Exception ex)
            {
                throw new Exception("Error al actualizar usuario: " + ex.Message, ex);
            }
        }
    }
}
