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
            ISNULL(us.rol, 1) AS rol,
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
                    return new Usuario(
                        row["usr"] != DBNull.Value ? row["usr"].ToString() : string.Empty,
                        row["contra"] != DBNull.Value ? row["contra"].ToString() : string.Empty,
                        Convert.ToBoolean(row["activo"]),
                        row["intentos"] != DBNull.Value ? Convert.ToInt32(row["intentos"]) : 0,
                        row["rol"] != DBNull.Value ? Convert.ToInt32(row["rol"]) : 1,
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

        public void CrearUsuario(Usuario usuario)
        {
            try
            {
                string consulta = @"
            INSERT INTO [GymApp].[dbo].[USUARIOS]
            (usr, contra, activo, dvv, dvh, rol)
            VALUES
            (@Usuario, @Contrasena, @Activo, '', '', @Rol)";

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

        public void CrearUsuarioIntentos(string usuario)
        {
            try
            {
                string consulta = @"
            INSERT INTO [GymApp].[dbo].[USUARIO_Intentos]
            (usr, intentos, dvv, dvh)
            VALUES
            (@Usuario, 0, '', '')";

                ArrayList parametros = new ArrayList
                {
                    new SqlParameter("@Usuario", usuario)
                };

                dal._686DPEscribir(consulta, parametros);
            }
            catch (Exception ex)
            {
                throw new Exception("Error al crear registro de intentos: " + ex.Message, ex);
            }
        }

        public List<BE.UsuarioGestion> ListarUsuarios()
        {
            try
            {
                string consulta = @"
            SELECT
                u.usr AS USUARIO_Usuario,
                CASE u.rol
                    WHEN 1 THEN 'Administrador'
                    WHEN 2 THEN 'Recepcionista'
                    WHEN 3 THEN 'Entrenador'
                    WHEN 4 THEN 'Cliente'
                    ELSE 'Desconocido'
                END AS USUARIO_Tipo,
                u.activo AS USUARIO_Activo,
                CASE WHEN ui.intentos >= 3 THEN 1 ELSE 0 END AS USUARIO_Bloqueado,
                ISNULL(ui.intentos, 0) AS USUARIO_Intentos,
                u.dvv AS USUARIO_DVV,
                u.dvh AS USUARIO_DVH,
                CASE u.rol
                    WHEN 3 THEN COALESCE(e.nombre, '')
                    WHEN 4 THEN COALESCE(a.nombre, '')
                    ELSE ''
                END AS Nombre,
                CASE u.rol
                    WHEN 3 THEN COALESCE(e.apellido, '')
                    WHEN 4 THEN COALESCE(a.apellido, '')
                    ELSE ''
                END AS Apellido,
                CASE u.rol
                    WHEN 3 THEN COALESCE(CAST(e.dni AS VARCHAR), '')
                    WHEN 4 THEN COALESCE(CAST(a.dni AS VARCHAR), '')
                    ELSE ''
                END AS DNI,
                CASE u.rol
                    WHEN 3 THEN COALESCE(e.telefono, '')
                    WHEN 4 THEN COALESCE(a.telefono, '')
                    ELSE ''
                END AS Telefono,
                '' AS Email
            FROM [GymApp].[dbo].[USUARIOS] u
            LEFT JOIN [GymApp].[dbo].[USUARIO_Intentos] ui ON u.usr = ui.usr
            LEFT JOIN [GymApp].[dbo].[Entrenadores] e ON u.usr = e.usr
            LEFT JOIN [GymApp].[dbo].[Alumnos] a ON u.usr = a.usr
            ORDER BY u.rol, u.usr";

                ArrayList parametros = new ArrayList();

                DataTable dt = dal._686DPConsultar(consulta, parametros);
                List<BE.UsuarioGestion> usuarios = new List<BE.UsuarioGestion>();

                foreach (DataRow row in dt.Rows)
                {
                    usuarios.Add(new BE.UsuarioGestion(
                        row["USUARIO_Usuario"] != DBNull.Value ? row["USUARIO_Usuario"].ToString() : string.Empty,
                        string.Empty, // Contra no se expone
                        row["USUARIO_Tipo"] != DBNull.Value ? row["USUARIO_Tipo"].ToString() : string.Empty,
                        Convert.ToBoolean(row["USUARIO_Activo"]),
                        Convert.ToBoolean(row["USUARIO_Bloqueado"]),
                        row["USUARIO_Intentos"] != DBNull.Value ? Convert.ToInt32(row["USUARIO_Intentos"]) : 0,
                        row["USUARIO_DVV"] != DBNull.Value ? row["USUARIO_DVV"].ToString() : string.Empty,
                        row["USUARIO_DVH"] != DBNull.Value ? row["USUARIO_DVH"].ToString() : string.Empty
                    )
                    {
                        Nombre = row["Nombre"] != DBNull.Value ? row["Nombre"].ToString() : null,
                        Apellido = row["Apellido"] != DBNull.Value ? row["Apellido"].ToString() : null,
                        DNI = row["DNI"] != DBNull.Value && int.TryParse(row["DNI"].ToString(), out int dni) ? (int?)dni : null,
                        Telefono = row["Telefono"] != DBNull.Value ? row["Telefono"].ToString() : null,
                        Email = row["Email"] != DBNull.Value ? row["Email"].ToString() : null
                    });
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
                    SELECT COUNT(*)
                    FROM [GymApp].[dbo].[USUARIOS]
                    WHERE usr = @Usuario";

                ArrayList parametros = new ArrayList
                {
                    new SqlParameter("@Usuario", usuario)
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
                throw new Exception("Error al verificar si existe el usuario: " + ex.Message, ex);
            }
        }

        public List<BE.UsuarioGestion> ListarUsuariosClientesSinAlumno()
        {
            try
            {
                string consulta = @"
                    SELECT
                        u.usr AS USUARIO_Usuario,
                        'Cliente' AS USUARIO_Tipo,
                        u.activo AS USUARIO_Activo,
                        CASE WHEN ui.intentos >= 3 THEN 1 ELSE 0 END AS USUARIO_Bloqueado,
                        ISNULL(ui.intentos, 0) AS USUARIO_Intentos,
                        u.dvv AS USUARIO_DVV,
                        u.dvh AS USUARIO_DVH,
                        COALESCE(a.nombre, '') AS Nombre,
                        COALESCE(a.apellido, '') AS Apellido,
                        CAST(a.dni AS VARCHAR) AS DNI,
                        COALESCE(a.telefono, '') AS Telefono,
                        '' AS Email
                    FROM [GymApp].[dbo].[USUARIOS] u
                    LEFT JOIN [GymApp].[dbo].[USUARIO_Intentos] ui ON u.usr = ui.usr
                    LEFT JOIN [GymApp].[dbo].[Alumnos] a ON u.usr = a.usr
                    WHERE u.rol = 4
                      AND u.activo = 1
                      AND (a.dni IS NULL OR a.usr IS NULL OR a.usr = '')
                    ORDER BY u.usr";

                ArrayList parametros = new ArrayList();
                DataTable dt = dal._686DPConsultar(consulta, parametros);
                List<BE.UsuarioGestion> usuarios = new List<BE.UsuarioGestion>();

                foreach (DataRow row in dt.Rows)
                {
                    usuarios.Add(new BE.UsuarioGestion(
                        row["USUARIO_Usuario"] != DBNull.Value ? row["USUARIO_Usuario"].ToString() : string.Empty,
                        string.Empty,
                        row["USUARIO_Tipo"] != DBNull.Value ? row["USUARIO_Tipo"].ToString() : string.Empty,
                        Convert.ToBoolean(row["USUARIO_Activo"]),
                        Convert.ToBoolean(row["USUARIO_Bloqueado"]),
                        row["USUARIO_Intentos"] != DBNull.Value ? Convert.ToInt32(row["USUARIO_Intentos"]) : 0,
                        row["USUARIO_DVV"] != DBNull.Value ? row["USUARIO_DVV"].ToString() : string.Empty,
                        row["USUARIO_DVH"] != DBNull.Value ? row["USUARIO_DVH"].ToString() : string.Empty
                    )
                    {
                        Nombre = row["Nombre"] != DBNull.Value ? row["Nombre"].ToString() : null,
                        Apellido = row["Apellido"] != DBNull.Value ? row["Apellido"].ToString() : null,
                        DNI = row["DNI"] != DBNull.Value && int.TryParse(row["DNI"].ToString(), out int dni) ? (int?)dni : null,
                        Telefono = row["Telefono"] != DBNull.Value ? row["Telefono"].ToString() : null,
                        Email = row["Email"] != DBNull.Value ? row["Email"].ToString() : null
                    });
                }

                return usuarios;
            }
            catch (Exception ex)
            {
                throw new Exception("Error al listar usuarios clientes sin alumno: " + ex.Message, ex);
            }
        }
    }
}