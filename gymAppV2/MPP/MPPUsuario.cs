using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using BE;
using DAL;
using SERVICIOS;
using static BE.ConstantesSeguridad;

namespace MPP
{
    public class MPPUsuario
    {
        private DalGeneral dal;
        private CriptoManager criptoManager;

        public MPPUsuario()
        {
            dal = new DalGeneral();
            criptoManager = new CriptoManager();
        }

        /// <summary>
        /// Encripta un campo personal con AES-256 si tiene valor; de lo contrario devuelve null.
        /// </summary>
        private string EncriptarCampoPersonal(string valor)
        {
            return string.IsNullOrEmpty(valor) ? null : criptoManager.EncriptarAES256(valor);
        }

        /// <summary>
        /// Desencripta un campo personal con AES-256 si tiene valor; de lo contrario devuelve el valor original o null.
        /// </summary>
        private string DesencriptarCampoPersonal(string valor)
        {
            if (string.IsNullOrEmpty(valor))
                return null;

            try
            {
                return criptoManager.DesencriptarAES256(valor);
            }
            catch
            {
                // Si no se puede desencriptar, asumimos que el valor aún no está encriptado (migración gradual).
                return valor;
            }
        }

        /// <summary>
        /// Desencripta una fecha encriptada con AES-256. Si falla, intenta parsearla como texto plano (migración gradual).
        /// </summary>
        private DateTime? DesencriptarFechaPersonal(object valorBD)
        {
            if (valorBD == null || valorBD == DBNull.Value)
                return null;

            string textoEncriptado = valorBD.ToString();
            if (string.IsNullOrEmpty(textoEncriptado))
                return null;

            try
            {
                string textoPlano = criptoManager.DesencriptarAES256(textoEncriptado);
                if (DateTime.TryParse(textoPlano, out DateTime fecha))
                    return fecha;
            }
            catch
            {
                if (DateTime.TryParse(textoEncriptado, out DateTime fechaPlana))
                    return fechaPlana;
            }

            return null;
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
                us.bloqueado,
                us.intentos,
                us.rol,
                us.tipo,
                us.dni,
                us.nombre,
                us.apellido,
                us.telefono,
                us.email,
                us.fechaNacimiento,
                us.primerLogin,
                us.dvv,
                us.dvh
            FROM [GymApp].[dbo].[USUARIOS] as us
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
                        row["bloqueado"] != DBNull.Value && Convert.ToBoolean(row["bloqueado"]),
                        row["intentos"] != DBNull.Value ? Convert.ToInt32(row["intentos"]) : 0,
                        row["rol"] != DBNull.Value ? Convert.ToInt32(row["rol"]) : 1,
                        row["tipo"] != DBNull.Value ? row["tipo"].ToString() : string.Empty,
                        row["dni"] != DBNull.Value ? Convert.ToInt32(row["dni"]) : 0,
                        DesencriptarCampoPersonal(row["nombre"] != DBNull.Value ? row["nombre"].ToString() : string.Empty),
                        DesencriptarCampoPersonal(row["apellido"] != DBNull.Value ? row["apellido"].ToString() : string.Empty),
                        DesencriptarCampoPersonal(row["telefono"] != DBNull.Value ? row["telefono"].ToString() : string.Empty),
                        DesencriptarCampoPersonal(row["email"] != DBNull.Value ? row["email"].ToString() : string.Empty),
                        DesencriptarFechaPersonal(row["fechaNacimiento"]),
                        row["dvv"] != DBNull.Value ? row["dvv"].ToString() : string.Empty,
                        row["dvh"] != DBNull.Value ? row["dvh"].ToString() : string.Empty,
                        row["primerLogin"] != DBNull.Value && Convert.ToBoolean(row["primerLogin"])
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
                    FROM [GymApp].[dbo].[USUARIOS]
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
                string consulta = $@"
                    UPDATE [GymApp].[dbo].[USUARIOS]
                    SET intentos = intentos + 1,
                        bloqueado = CASE WHEN intentos + 1 >= {MAX_INTENTOS_LOGIN} THEN 1 ELSE 0 END
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
                    UPDATE [GymApp].[dbo].[USUARIOS]
                    SET intentos = 0,
                        bloqueado = 0
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
                    SELECT bloqueado
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
                // Insertar la nueva contraseña en el historial.
                string insertar = @"
            INSERT INTO [GymApp].[dbo].[USUARIO_Contras] (usr, contra, dvv, dvh)
            VALUES (@Usuario, @Contrasena, '', '')";

                ArrayList parametrosInsertar = new ArrayList
                {
                    new SqlParameter("@Usuario", usuario),
                    new SqlParameter("@Contrasena", contrasenaHash)
                };

                dal._686DPEscribir(insertar, parametrosInsertar);

                // Mantener solo las últimas N contraseñas históricas para evitar acumulación infinita.
                string limpiar = @"
            WITH ContrasOrdenadas AS (
                SELECT TOP 1000000 [usr], [contra], [dvv], [dvh],
                    ROW_NUMBER() OVER (PARTITION BY [usr] ORDER BY (SELECT NULL)) AS rn
                FROM [GymApp].[dbo].[USUARIO_Contras]
                WHERE [usr] = @Usuario
            )
            DELETE FROM ContrasOrdenadas
            WHERE rn > @MaxHistorial";

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

        public void FinalizarPrimerLogin(string usuario)
        {
            try
            {
                string consulta = @"
            UPDATE [GymApp].[dbo].[USUARIOS]
            SET primerLogin = 0
            WHERE usr = @Usuario";

                ArrayList parametros = new ArrayList
                {
                    new SqlParameter("@Usuario", usuario)
                };

                dal._686DPEscribir(consulta, parametros);
            }
            catch (Exception ex)
            {
                throw new Exception("Error al finalizar el primer login: " + ex.Message, ex);
            }
        }

        public void CrearUsuario(Usuario usuario)
        {
            try
            {
                string consulta = @"
            INSERT INTO [GymApp].[dbo].[USUARIOS]
            (usr, contra, activo, bloqueado, intentos, rol, primerLogin, tipo, dni, nombre, apellido, telefono, email, fechaNacimiento, dvv, dvh)
            VALUES
            (@Usuario, @Contrasena, @Activo, @Bloqueado, @Intentos, @Rol, @PrimerLogin, @Tipo, @DNI, @Nombre, @Apellido, @Telefono, @Email, @FechaNacimiento, '', '')";

                ArrayList parametros = new ArrayList
                {
                    new SqlParameter("@Usuario", usuario.USUARIO_Usuario),
                    new SqlParameter("@Contrasena", usuario.USUARIO_Contras),
                    new SqlParameter("@Activo", usuario.USUARIO_Activo),
                    new SqlParameter("@Bloqueado", usuario.USUARIO_Bloqueado),
                    new SqlParameter("@Intentos", usuario.USUARIO_Intentos),
                    new SqlParameter("@Rol", usuario.USUARIO_Rol),
                    new SqlParameter("@PrimerLogin", usuario.USUARIO_PrimerLogin),
                    new SqlParameter("@Tipo", usuario.USUARIO_Tipo),
                    new SqlParameter("@DNI", usuario.USUARIO_DNI),
                    new SqlParameter("@Nombre", EncriptarCampoPersonal(usuario.Nombre) ?? (object)DBNull.Value),
                    new SqlParameter("@Apellido", EncriptarCampoPersonal(usuario.Apellido) ?? (object)DBNull.Value),
                    new SqlParameter("@Telefono", EncriptarCampoPersonal(usuario.Telefono) ?? (object)DBNull.Value),
                    new SqlParameter("@Email", EncriptarCampoPersonal(usuario.Email) ?? (object)DBNull.Value),
                    new SqlParameter("@FechaNacimiento", EncriptarCampoPersonal(usuario.FechaNacimiento?.ToString("yyyy-MM-dd")) ?? (object)DBNull.Value)
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
            SELECT
                u.usr AS USUARIO_Usuario,
                u.tipo AS USUARIO_Tipo,
                u.activo AS USUARIO_Activo,
                u.bloqueado AS USUARIO_Bloqueado,
                u.intentos AS USUARIO_Intentos,
                u.dvv AS USUARIO_DVV,
                u.dvh AS USUARIO_DVH,
                u.nombre AS Nombre,
                u.apellido AS Apellido,
                u.dni AS DNI,
                u.telefono AS Telefono,
                u.email AS Email,
                u.fechaNacimiento AS FechaNacimiento
            FROM [GymApp].[dbo].[USUARIOS] u
            ORDER BY u.tipo, u.usr";

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
                        Nombre = DesencriptarCampoPersonal(row["Nombre"] != DBNull.Value ? row["Nombre"].ToString() : string.Empty),
                        Apellido = DesencriptarCampoPersonal(row["Apellido"] != DBNull.Value ? row["Apellido"].ToString() : string.Empty),
                        DNI = row["DNI"] != DBNull.Value && int.TryParse(row["DNI"].ToString(), out int dni) ? (int?)dni : null,
                        Telefono = DesencriptarCampoPersonal(row["Telefono"] != DBNull.Value ? row["Telefono"].ToString() : string.Empty),
                        Email = DesencriptarCampoPersonal(row["Email"] != DBNull.Value ? row["Email"].ToString() : string.Empty),
                        FechaNacimiento = DesencriptarFechaPersonal(row["FechaNacimiento"])
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
                // En el esquema normalizado, los clientes (tipo='Cliente') que no tienen registro en ALUMNOS
                string consulta = @"
                    SELECT
                        u.usr AS USUARIO_Usuario,
                        u.tipo AS USUARIO_Tipo,
                        u.activo AS USUARIO_Activo,
                        u.bloqueado AS USUARIO_Bloqueado,
                        u.intentos AS USUARIO_Intentos,
                        u.dvv AS USUARIO_DVV,
                        u.dvh AS USUARIO_DVH,
                        u.nombre AS Nombre,
                        u.apellido AS Apellido,
                        u.dni AS DNI,
                        u.telefono AS Telefono,
                        u.email AS Email,
                        u.fechaNacimiento AS FechaNacimiento
                    FROM [GymApp].[dbo].[USUARIOS] u
                    LEFT JOIN [GymApp].[dbo].[ALUMNOS] a ON u.dni = a.dni
                    WHERE u.tipo = 'Cliente'
                      AND u.activo = 1
                      AND a.dni IS NULL
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
                        Nombre = DesencriptarCampoPersonal(row["Nombre"] != DBNull.Value ? row["Nombre"].ToString() : string.Empty),
                        Apellido = DesencriptarCampoPersonal(row["Apellido"] != DBNull.Value ? row["Apellido"].ToString() : string.Empty),
                        DNI = row["DNI"] != DBNull.Value && int.TryParse(row["DNI"].ToString(), out int dni) ? (int?)dni : null,
                        Telefono = DesencriptarCampoPersonal(row["Telefono"] != DBNull.Value ? row["Telefono"].ToString() : string.Empty),
                        Email = DesencriptarCampoPersonal(row["Email"] != DBNull.Value ? row["Email"].ToString() : string.Empty),
                        FechaNacimiento = DesencriptarFechaPersonal(row["FechaNacimiento"])
                    });
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
                    UPDATE [GymApp].[dbo].[USUARIOS]
                    SET activo = @Activo
                    WHERE usr = @Usuario";

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
            try
            {
                string consulta = @"
                    SELECT fechaNacimiento
                    FROM [GymApp].[dbo].[USUARIOS]
                    WHERE usr = @Usuario";

                ArrayList parametros = new ArrayList
                {
                    new SqlParameter("@Usuario", usuario)
                };

                object resultado = dal._686DPEscalar(consulta, parametros);

                return DesencriptarFechaPersonal(resultado);
            }
            catch (Exception ex)
            {
                throw new Exception("Error al obtener la fecha de nacimiento: " + ex.Message, ex);
            }
        }

        public void ActualizarUsuario(Usuario usuario)
        {
            try
            {
                string consulta = @"
                    UPDATE [GymApp].[dbo].[USUARIOS]
                    SET
                        tipo = @Tipo,
                        dni = @DNI,
                        nombre = @Nombre,
                        apellido = @Apellido,
                        telefono = @Telefono,
                        email = @Email,
                        fechaNacimiento = @FechaNacimiento,
                        rol = @Rol,
                        primerLogin = @PrimerLogin
                    WHERE usr = @Usuario";

                ArrayList parametros = new ArrayList
                {
                    new SqlParameter("@Usuario", usuario.USUARIO_Usuario),
                    new SqlParameter("@Tipo", usuario.USUARIO_Tipo),
                    new SqlParameter("@DNI", usuario.USUARIO_DNI),
                    new SqlParameter("@Nombre", EncriptarCampoPersonal(usuario.Nombre) ?? (object)DBNull.Value),
                    new SqlParameter("@Apellido", EncriptarCampoPersonal(usuario.Apellido) ?? (object)DBNull.Value),
                    new SqlParameter("@Telefono", EncriptarCampoPersonal(usuario.Telefono) ?? (object)DBNull.Value),
                    new SqlParameter("@Email", EncriptarCampoPersonal(usuario.Email) ?? (object)DBNull.Value),
                    new SqlParameter("@FechaNacimiento", EncriptarCampoPersonal(usuario.FechaNacimiento?.ToString("yyyy-MM-dd")) ?? (object)DBNull.Value),
                    new SqlParameter("@Rol", usuario.USUARIO_Rol),
                    new SqlParameter("@PrimerLogin", usuario.USUARIO_PrimerLogin)
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