using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
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
        private DigitoVerificadorManager dvManager;

        public MPPUsuario()
        {
            dal = new DalGeneral();
            criptoManager = new CriptoManager();
            dvManager = new DigitoVerificadorManager();
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

        /// <summary>
        /// Calcula DVH y DVV de un usuario a partir de sus valores en texto plano.
        /// Los campos personales se normalizan al formato que se persistiría para asegurar
        /// que el cálculo coincida al leer y al escribir.
        /// </summary>
        private void CalcularDigitosUsuario(Usuario usuario, out string dvh, out string dvv)
        {
            var valores = new Dictionary<string, object>
            {
                { "usr", usuario.USUARIO_Usuario },
                { "contra", usuario.USUARIO_Contras },
                { "activo", usuario.USUARIO_Activo },
                { "bloqueado", usuario.USUARIO_Bloqueado },
                { "intentos", usuario.USUARIO_Intentos },
                { "tipo", usuario.USUARIO_Tipo },
                { "dni", usuario.USUARIO_DNI },
                { "nombre", usuario.Nombre },
                { "apellido", usuario.Apellido },
                { "telefono", usuario.Telefono },
                { "email", usuario.Email },
                { "fechaNacimiento", usuario.FechaNacimiento.HasValue ? usuario.FechaNacimiento.Value.ToString("yyyy-MM-dd") : null },
                { "rol", usuario.USUARIO_Rol },
                { "primerLogin", usuario.USUARIO_PrimerLogin }
            };

            dvManager.CalcularAmbos(valores, out dvh, out dvv);
        }

        /// <summary>
        /// Vuelve a calcular y actualizar dvv/dvh de un usuario existente.
        /// Se usa después de UPDATEs parciales (intentos, bloqueo, contraseña, etc.).
        /// </summary>
        private void RecalcularDigitosUsuario(string usuario)
        {
            Usuario u = ObtenerUsuario(usuario);
            if (u == null) return;

            CalcularDigitosUsuario(u, out string dvh, out string dvv);
            ActualizarDigitosUsuario(usuario, dvh, dvv);
        }

        /// <summary>
        /// Actualiza los dígitos verificadores de un usuario por su nombre de usuario.
        /// </summary>
        private void ActualizarDigitosUsuario(string usuario, string dvh, string dvv)
        {
            string consulta = @"
                UPDATE [GymApp].[dbo].[USUARIOS]
                SET dvv = @DVV, dvh = @DVH
                WHERE usr = @Usuario";

            List<SqlParameter> parametros = new List<SqlParameter>
            {
                new SqlParameter("@Usuario", usuario),
                new SqlParameter("@DVV", dvv),
                new SqlParameter("@DVH", dvh)
            };

            dal._686DPEscribir(consulta, parametros);
        }

        /// <summary>
        /// Recalcula los dígitos verificadores de todos los usuarios existentes.
        /// Necesario para la inicialización del control de integridad, ya que los
        /// campos personales se almacenan encriptados y el cálculo requiere el
        /// texto plano.
        /// </summary>
        public void RecalcularDigitosTodosUsuarios()
        {
            try
            {
                string consulta = @"
                    SELECT usr
                    FROM [GymApp].[dbo].[USUARIOS]";

                DataTable dt = dal._686DPConsultar(consulta, new List<SqlParameter>());

                foreach (DataRow row in dt.Rows)
                {
                    string usuario = row["usr"] != DBNull.Value ? row["usr"].ToString() : string.Empty;
                    if (string.IsNullOrEmpty(usuario)) continue;

                    Usuario u = ObtenerUsuario(usuario);
                    if (u == null) continue;

                    CalcularDigitosUsuario(u, out string dvh, out string dvv);
                    ActualizarDigitosUsuario(usuario, dvh, dvv);
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Error al recalcular dígitos de todos los usuarios: " + ex.Message, ex);
            }
        }

        /// <summary>
        /// Verifica la integridad de la tabla USUARIOS desencriptando los campos
        /// personales antes de comparar DVH/DVV.
        /// </summary>
        public List<ResultadoVerificacionDV> VerificarIntegridadUsuarios()
        {
            var resultados = new List<ResultadoVerificacionDV>();

            try
            {
                string consulta = @"
                    SELECT usr, dvv, dvh
                    FROM [GymApp].[dbo].[USUARIOS]";

                DataTable dt = dal._686DPConsultar(consulta, new List<SqlParameter>());

                foreach (DataRow row in dt.Rows)
                {
                    string usuario = row["usr"] != DBNull.Value ? row["usr"].ToString() : string.Empty;
                    if (string.IsNullOrEmpty(usuario)) continue;

                    string dvvAlmacenado = row["dvv"] != DBNull.Value ? row["dvv"].ToString() : string.Empty;
                    string dvhAlmacenado = row["dvh"] != DBNull.Value ? row["dvh"].ToString() : string.Empty;

                    Usuario u = ObtenerUsuario(usuario);
                    if (u == null) continue;

                    CalcularDigitosUsuario(u, out string dvhCalculado, out string dvvCalculado);

                    bool dvhOk = !string.IsNullOrEmpty(dvhAlmacenado) && dvhAlmacenado.Equals(dvhCalculado, StringComparison.OrdinalIgnoreCase);
                    bool dvvOk = !string.IsNullOrEmpty(dvvAlmacenado) && dvvAlmacenado.Equals(dvvCalculado, StringComparison.OrdinalIgnoreCase);

                    if (dvhOk && dvvOk)
                        continue;

                    resultados.Add(new ResultadoVerificacionDV
                    {
                        NombreTabla = "USUARIOS",
                        ClaveFila = usuario,
                        Campo = "DVH/DVV (fila completa)",
                        EsValido = false,
                        Mensaje = "Los dígitos verificadores del usuario no coinciden.",
                        DVHAlmacenado = dvhAlmacenado,
                        DVHCalculado = dvhCalculado,
                        DVVAlmacenado = dvvAlmacenado,
                        DVVCalculado = dvvCalculado
                    });
                }

                if (resultados.Count == 0)
                {
                    resultados.Add(new ResultadoVerificacionDV
                    {
                        NombreTabla = "USUARIOS",
                        ClaveFila = "-",
                        Campo = "-",
                        EsValido = true,
                        Mensaje = "Tabla USUARIOS verificada correctamente."
                    });
                }
            }
            catch (Exception ex)
            {
                resultados.Add(new ResultadoVerificacionDV
                {
                    NombreTabla = "USUARIOS",
                    ClaveFila = "-",
                    Campo = "-",
                    EsValido = false,
                    Mensaje = "Error al verificar USUARIOS: " + ex.Message
                });
            }

            return resultados;
        }

        public List<ResultadoVerificacionDV> VerificarIntegridadHistorialContrasenas()
        {
            var resultados = new List<ResultadoVerificacionDV>();
            try
            {
                string consulta = "SELECT usr, contra, dvv, dvh FROM [GymApp].[dbo].[USUARIO_Contras]";
                DataTable dt = dal._686DPConsultar(consulta, new List<SqlParameter>());

                foreach (DataRow row in dt.Rows)
                {
                    string usr = row["usr"] != DBNull.Value ? row["usr"].ToString() : string.Empty;
                    string contra = row["contra"] != DBNull.Value ? row["contra"].ToString() : string.Empty;
                    string dvhAlmacenado = row["dvh"] != DBNull.Value ? row["dvh"].ToString() : string.Empty;
                    string dvvAlmacenado = row["dvv"] != DBNull.Value ? row["dvv"].ToString() : string.Empty;

                    var valores = new Dictionary<string, object>
                    {
                        { "usr", usr },
                        { "contra", contra }
                    };
                    dvManager.CalcularAmbos(valores, out string dvhCalculado, out string dvvCalculado);

                    bool dvhOk = !string.IsNullOrEmpty(dvhAlmacenado)
                        && dvhAlmacenado.Equals(dvhCalculado, StringComparison.OrdinalIgnoreCase);
                    bool dvvOk = !string.IsNullOrEmpty(dvvAlmacenado)
                        && dvvAlmacenado.Equals(dvvCalculado, StringComparison.OrdinalIgnoreCase);

                    if (dvhOk && dvvOk) continue;

                    resultados.Add(new ResultadoVerificacionDV
                    {
                        NombreTabla = "USUARIO_Contras",
                        ClaveFila = usr,
                        Campo = "DVH/DVV (fila completa)",
                        EsValido = false,
                        Mensaje = "Los dígitos verificadores del historial de contraseñas no coinciden.",
                        DVHAlmacenado = dvhAlmacenado,
                        DVHCalculado = dvhCalculado,
                        DVVAlmacenado = dvvAlmacenado,
                        DVVCalculado = dvvCalculado
                    });
                }

                if (resultados.Count == 0)
                {
                    resultados.Add(new ResultadoVerificacionDV
                    {
                        NombreTabla = "USUARIO_Contras",
                        ClaveFila = "-",
                        Campo = "-",
                        EsValido = true,
                        Mensaje = "Historial de contraseñas verificado correctamente."
                    });
                }
            }
            catch (Exception ex)
            {
                resultados.Add(new ResultadoVerificacionDV
                {
                    NombreTabla = "USUARIO_Contras",
                    ClaveFila = "-",
                    Campo = "-",
                    EsValido = false,
                    Mensaje = "Error al verificar historial de contraseñas: " + ex.Message
                });
            }
            return resultados;
        }

        public void RecalcularDigitosHistorialContrasenas()
        {
            try
            {
                string consulta = "SELECT usr, contra FROM [GymApp].[dbo].[USUARIO_Contras]";
                DataTable dt = dal._686DPConsultar(consulta, new List<SqlParameter>());

                foreach (DataRow row in dt.Rows)
                {
                    string usr = row["usr"] != DBNull.Value ? row["usr"].ToString() : string.Empty;
                    string contra = row["contra"] != DBNull.Value ? row["contra"].ToString() : string.Empty;

                    var valores = new Dictionary<string, object>
                    {
                        { "usr", usr },
                        { "contra", contra }
                    };
                    dvManager.CalcularAmbos(valores, out string dvh, out string dvv);

                    string update = @"
                        UPDATE [GymApp].[dbo].[USUARIO_Contras]
                        SET dvv = @DVV, dvh = @DVH
                        WHERE usr = @Usuario AND contra = @Contra";

                    dal._686DPEscribir(update, new List<SqlParameter>
                    {
                        new SqlParameter("@DVV", dvv),
                        new SqlParameter("@DVH", dvh),
                        new SqlParameter("@Usuario", usr),
                        new SqlParameter("@Contra", contra)
                    });
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Error al recalcular dígitos del historial de contraseñas: " + ex.Message, ex);
            }
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

                List<SqlParameter> parametros = new List<SqlParameter>
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

                List<SqlParameter> parametros = new List<SqlParameter>
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

                List<SqlParameter> parametros = new List<SqlParameter>
                {
                    new SqlParameter("@Usuario", usuario)
                };

                dal._686DPEscribir(consulta, parametros);
                RecalcularDigitosUsuario(usuario);
            }
            catch (Exception ex)
            {
                throw new Exception("Error al agregar intento: " + ex.Message, ex);
            }
        }

        public void BloquearUsuario(string usuario)
        {
            try
            {
                string consulta = @"
                    UPDATE [GymApp].[dbo].[USUARIOS]
                    SET bloqueado = 1
                    WHERE usr = @Usuario";

                List<SqlParameter> parametros = new List<SqlParameter>
                {
                    new SqlParameter("@Usuario", usuario)
                };

                dal._686DPEscribir(consulta, parametros);
                RecalcularDigitosUsuario(usuario);
            }
            catch (Exception ex)
            {
                throw new Exception("Error al bloquear usuario: " + ex.Message, ex);
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

                List<SqlParameter> parametros = new List<SqlParameter>
                {
                    new SqlParameter("@Usuario", usuario)
                };

                dal._686DPEscribir(consulta, parametros);
                RecalcularDigitosUsuario(usuario);
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

                List<SqlParameter> parametros = new List<SqlParameter>
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

                List<SqlParameter> parametros = new List<SqlParameter>
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

                List<SqlParameter> parametros = new List<SqlParameter>
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

                List<SqlParameter> parametros = new List<SqlParameter>
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
                var valoresContras = new Dictionary<string, object>
                {
                    { "usr", usuario },
                    { "contra", contrasenaHash }
                };
                dvManager.CalcularAmbos(valoresContras, out string dvhContra, out string dvvContra);

                // Insertar la nueva contraseña en el historial.
                string insertar = @"
            INSERT INTO [GymApp].[dbo].[USUARIO_Contras] (usr, contra, dvv, dvh)
            VALUES (@Usuario, @Contrasena, @DVV, @DVH)";

                List<SqlParameter> parametrosInsertar = new List<SqlParameter>
                {
                    new SqlParameter("@Usuario", usuario),
                    new SqlParameter("@Contrasena", contrasenaHash),
                    new SqlParameter("@DVV", dvvContra),
                    new SqlParameter("@DVH", dvhContra)
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

                List<SqlParameter> parametrosLimpiar = new List<SqlParameter>
                {
                    new SqlParameter("@Usuario", usuario),
                    new SqlParameter("@MaxHistorial", MAX_HISTORIAL_CONTRASENAS)
                };

                dal._686DPEscribir(limpiar, parametrosLimpiar);

                // Mantener tabla-hash de DigitoVerificador sincronizado para USUARIO_Contras.
                try
                {
                    var mppDV = new MPPDigitoVerificador();
                    if (mppDV.ObtenerControlPorTabla("USUARIO_Contras") != null)
                        mppDV.RecalcularDigitosTabla("USUARIO_Contras", actualizarFilas: false);
                }
                catch
                {
                    // No bloquear el flujo si falla la sincronización del hash de tabla.
                }
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

                List<SqlParameter> parametros = new List<SqlParameter>
                {
                    new SqlParameter("@Usuario", usuario),
                    new SqlParameter("@Contrasena", nuevaContrasenaHash)
                };

                dal._686DPEscribir(consulta, parametros);
                RecalcularDigitosUsuario(usuario);
            }
            catch (Exception ex)
            {
                throw new Exception("Error al actualizar contraseña: " + ex.Message, ex);
            }
        }

        public void BlanquearContrasena(string usuario)
        {
            try
            {
                string consulta = @"
            UPDATE [GymApp].[dbo].[USUARIOS]
            SET primerLogin = 1
            WHERE usr = @Usuario";

                List<SqlParameter> parametros = new List<SqlParameter>
                {
                    new SqlParameter("@Usuario", usuario)
                };

                dal._686DPEscribir(consulta, parametros);
                RecalcularDigitosUsuario(usuario);
            }
            catch (Exception ex)
            {
                throw new Exception("Error al blanquear contraseña: " + ex.Message, ex);
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

                List<SqlParameter> parametros = new List<SqlParameter>
                {
                    new SqlParameter("@Usuario", usuario)
                };

                dal._686DPEscribir(consulta, parametros);
                RecalcularDigitosUsuario(usuario);
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
                CalcularDigitosUsuario(usuario, out string dvh, out string dvv);

                string consulta = @"
            INSERT INTO [GymApp].[dbo].[USUARIOS]
            (usr, contra, activo, bloqueado, intentos, rol, primerLogin, tipo, dni, nombre, apellido, telefono, email, fechaNacimiento, dvv, dvh)
            VALUES
            (@Usuario, @Contrasena, @Activo, @Bloqueado, @Intentos, @Rol, @PrimerLogin, @Tipo, @DNI, @Nombre, @Apellido, @Telefono, @Email, @FechaNacimiento, @DVV, @DVH)";

                List<SqlParameter> parametros = new List<SqlParameter>
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
                    new SqlParameter("@FechaNacimiento", EncriptarCampoPersonal(usuario.FechaNacimiento?.ToString("yyyy-MM-dd")) ?? (object)DBNull.Value),
                    new SqlParameter("@DVV", dvv),
                    new SqlParameter("@DVH", dvh)
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
                u.rol AS USUARIO_Rol_DB,
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

                List<SqlParameter> parametros = new List<SqlParameter>();

                DataTable dt = dal._686DPConsultar(consulta, parametros);
                List<BE.UsuarioGestion> usuarios = new List<BE.UsuarioGestion>();

                foreach (DataRow row in dt.Rows)
                {
                    int rolInt = row["USUARIO_Rol_DB"] != DBNull.Value ? Convert.ToInt32(row["USUARIO_Rol_DB"]) : 0;
                    string tipoStr = rolInt == 5 ? "WebMaster"
                                   : rolInt == 1 ? "Administrador"
                                   : rolInt == 2 ? "Recepcionista"
                                   : rolInt == 3 ? "Entrenador"
                                   : rolInt == 4 ? "Cliente"
                                   : (row["USUARIO_Tipo"] != DBNull.Value ? row["USUARIO_Tipo"].ToString() : string.Empty);
                    usuarios.Add(new BE.UsuarioGestion(
                        row["USUARIO_Usuario"] != DBNull.Value ? row["USUARIO_Usuario"].ToString() : string.Empty,
                        string.Empty,
                        tipoStr,
                        Convert.ToBoolean(row["USUARIO_Activo"]),
                        Convert.ToBoolean(row["USUARIO_Bloqueado"]),
                        row["USUARIO_Intentos"] != DBNull.Value ? Convert.ToInt32(row["USUARIO_Intentos"]) : 0,
                        row["USUARIO_DVV"] != DBNull.Value ? row["USUARIO_DVV"].ToString() : string.Empty,
                        row["USUARIO_DVH"] != DBNull.Value ? row["USUARIO_DVH"].ToString() : string.Empty
                    )
                    {
                        USUARIO_Rol = rolInt,
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

                List<SqlParameter> parametros = new List<SqlParameter>
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

                List<SqlParameter> parametros = new List<SqlParameter>();
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
                        USUARIO_Rol = 4,
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

                List<SqlParameter> parametros = new List<SqlParameter>
                {
                    new SqlParameter("@Usuario", usuario),
                    new SqlParameter("@Activo", activo)
                };

                dal._686DPEscribir(consulta, parametros);
                RecalcularDigitosUsuario(usuario);
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

                List<SqlParameter> parametros = new List<SqlParameter>
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

        public void ActualizarUsuario(Usuario usuario, string usuarioOriginal = null)
        {
            try
            {
                CalcularDigitosUsuario(usuario, out string dvh, out string dvv);

                string consulta = @"
                    UPDATE [GymApp].[dbo].[USUARIOS]
                    SET
                        usr = @Usuario,
                        activo = @Activo,
                        tipo = @Tipo,
                        dni = @DNI,
                        nombre = @Nombre,
                        apellido = @Apellido,
                        telefono = @Telefono,
                        email = @Email,
                        fechaNacimiento = @FechaNacimiento,
                        rol = @Rol,
                        primerLogin = @PrimerLogin,
                        dvv = @DVV,
                        dvh = @DVH
                    WHERE usr = @UsuarioOriginal";

                List<SqlParameter> parametros = new List<SqlParameter>
                {
                    new SqlParameter("@Usuario", usuario.USUARIO_Usuario),
                    new SqlParameter("@UsuarioOriginal", usuarioOriginal ?? usuario.USUARIO_Usuario),
                    new SqlParameter("@Activo", usuario.USUARIO_Activo),
                    new SqlParameter("@Tipo", usuario.USUARIO_Tipo),
                    new SqlParameter("@DNI", usuario.USUARIO_DNI),
                    new SqlParameter("@Nombre", EncriptarCampoPersonal(usuario.Nombre) ?? (object)DBNull.Value),
                    new SqlParameter("@Apellido", EncriptarCampoPersonal(usuario.Apellido) ?? (object)DBNull.Value),
                    new SqlParameter("@Telefono", EncriptarCampoPersonal(usuario.Telefono) ?? (object)DBNull.Value),
                    new SqlParameter("@Email", EncriptarCampoPersonal(usuario.Email) ?? (object)DBNull.Value),
                    new SqlParameter("@FechaNacimiento", EncriptarCampoPersonal(usuario.FechaNacimiento?.ToString("yyyy-MM-dd")) ?? (object)DBNull.Value),
                    new SqlParameter("@Rol", usuario.USUARIO_Rol),
                    new SqlParameter("@PrimerLogin", usuario.USUARIO_PrimerLogin),
                    new SqlParameter("@DVV", dvv),
                    new SqlParameter("@DVH", dvh)
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