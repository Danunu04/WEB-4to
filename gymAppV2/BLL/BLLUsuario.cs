using System;
using System.Collections.Generic;
using BE;
using MPP;
using Servicios;
using Servicios.Singleton;
using SERVICIOS;

namespace BLL
{
    public class BLLUsuario
    {
        private MPPUsuario mppUsuario;
        private CriptoManager criptoManager;
        private BLLEntrenador bllEntrenador;
        private BLLAlumno bllAlumno;
        private BLLEvento bllEvento;

        public BLLUsuario()
        {
            mppUsuario = new MPPUsuario();
            criptoManager = new CriptoManager();
            bllEntrenador = new BLLEntrenador();
            bllAlumno = new BLLAlumno();
            bllEvento = new BLLEvento();
        }

        private void RegistrarEvento(string tipo, string accion)
        {
            try
            {
                var usuario = System.Web.HttpContext.Current?.Session["UsuarioLogueado"] as Usuario;
                string usr = usuario?.USUARIO_Usuario ?? "sistema";
                bllEvento.RegistrarEvento(tipo, usr, accion);
            }
            catch
            {
                // No impedir la operación principal si falla el log
            }
        }

        public bool ValidarLogin(string usuario, string contrasena)
        {
            bool ok = false;
            try
            {
                // Validar campos no vacíos
                if (string.IsNullOrEmpty(usuario) || string.IsNullOrEmpty(contrasena))
                {
                    throw new ExcepcionesLogIn(ResultadosLogIn.InvalidUsername);
                }

                Usuario usuarioBD = mppUsuario.ObtenerUsuario(usuario);

                if (usuarioBD == null)
                {
                    throw new ExcepcionesLogIn(ResultadosLogIn.InvalidUsername);
                }

                if (!usuarioBD.USUARIO_Activo)
                {
                    throw new Exception("El usuario está desactivado o no esta registrado");
                }

                // Si cuenta bloqueada → lanzar excepción específica para redirigir a preguntas de seguridad
                if (mppUsuario.UsuarioEstaBloqueado(usuario))
                {
                    throw new ExcepcionesLogIn(ResultadosLogIn.AccountLocked);
                }

                string contrasenaHash = criptoManager._686DPGetSHA256(contrasena);
                string contrasenaBD = mppUsuario.ObtenerContrasena(usuario);

                if (contrasenaHash == contrasenaBD)
                {
                    ok = true;
                    // Login exitoso: reestablecer intentos
                    ReestablecerIntentos(usuario);
                    return ok;
                }
                else
                {
                    // Contraseña incorrecta: registrar intento fallido
                    RegistrarIntentoFallido(usuario);
                    return ok;
                }
            }
            catch (ExcepcionesLogIn)
            {
                throw;
            }
            catch (Exception ex)
            {
                throw new Exception("Error al validar el login: " + ex.Message, ex);
            }
        }

        public void RegistrarIntentoFallido(string usuario)
        {
            try
            {
                mppUsuario.AgregarIntento(usuario);

                // Verificar si se bloquea por llegar a 3 intentos
                int intentos = mppUsuario.ObtenerIntentos(usuario);
                if (intentos >= 3)
                {
                    RegistrarEvento("bloqueo_usuario", $"Usuario '{usuario}' bloqueado por exceso de intentos fallidos");
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Error al registrar el intento fallido: " + ex.Message, ex);
            }
        }

        public int ObtenerIntentosRestantes(string usuario)
        {
            try
            {
                int intentos = mppUsuario.ObtenerIntentos(usuario);
                return 3 - intentos;
            }
            catch (Exception ex)
            {
                throw new Exception("Error al obtener los intentos restantes: " + ex.Message, ex);
            }
        }

        public void ReestablecerIntentos(string usuario)
        {
            try
            {
                // Verificar si estaba bloqueado antes de reestablecer
                bool estabaBloqueado = mppUsuario.UsuarioEstaBloqueado(usuario);

                mppUsuario.ReestablecerIntentos(usuario);

                // Registrar evento de desbloqueo si corresponde
                if (estabaBloqueado)
                {
                    RegistrarEvento("desbloqueo_usuario", $"Usuario '{usuario}' desbloqueado");
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Error al reestablecer los intentos: " + ex.Message, ex);
            }
        }

        public Usuario ObtenerUsuario(string usuario)
        {
            try
            {
                return mppUsuario.ObtenerUsuario(usuario);
            }
            catch (Exception ex)
            {
                throw new Exception("Error al obtener el usuario: " + ex.Message, ex);
            }
        }

        public void LogearUsuario(Usuario usuario)
        {
            try
            {
                Singleton.Instancia.LogIn(usuario);
            }
            catch (Exception ex)
            {
                throw new Exception("Error al iniciar sesión: " + ex.Message, ex);
            }
        }

        public void DeslogearUsuario()
        {
            try
            {
                Singleton.Instancia.LogOut();
            }
            catch (Exception ex)
            {
                throw new Exception("Error al cerrar sesión: " + ex.Message, ex);
            }
        }

        public bool UsuarioEstaLogueado()
        {
            try
            {
                return Singleton.Instancia.IsLogged();
            }
            catch
            {
                return false;
            }
        }

        public void ValidarRequisitosContrasena(string contrasena)
        {
            if (string.IsNullOrEmpty(contrasena))
            {
                throw new Exception("La contraseña no puede estar vacía");
            }

            if (contrasena.Length < 6)
            {
                throw new Exception("La contraseña debe tener al menos 6 caracteres");
            }

            bool tieneMayuscula = false;
            bool tieneCaracterEspecial = false;

            foreach (char c in contrasena)
            {
                if (char.IsUpper(c))
                {
                    tieneMayuscula = true;
                }

                if (!char.IsLetterOrDigit(c))
                {
                    tieneCaracterEspecial = true;
                }
            }

            if (!tieneMayuscula)
            {
                throw new Exception("La contraseña debe tener al menos una letra mayúscula");
            }

            if (!tieneCaracterEspecial)
            {
                throw new Exception("La contraseña debe tener al menos un carácter especial");
            }
        }

        public void CambiarContrasena(string usuario, string nuevaContrasena)
        {
            try
            {
                ValidarRequisitosContrasena(nuevaContrasena);
                string nuevaContrasenaHash = criptoManager._686DPGetSHA256(nuevaContrasena);

                if (mppUsuario.ContrasenaFueUtilizada(usuario, nuevaContrasenaHash))
                {
                    throw new Exception("No puedes reutilizar una contraseña anterior");
                }

                mppUsuario.GuardarContrasenaEnHistorial(usuario, nuevaContrasenaHash);
                mppUsuario.ActualizarContrasena(usuario, nuevaContrasenaHash);
                mppUsuario.ReestablecerIntentos(usuario);

                RegistrarEvento("cambio_contrasena", $"Contraseña cambiada para usuario '{usuario}'");
            }
            catch (Exception ex)
            {
                throw new Exception("Error al cambiar contraseña: " + ex.Message, ex);
            }
        }

        public bool RequierePreguntaSeguridad(string usuario)
        {
            try
            {
                int intentos = mppUsuario.ObtenerIntentos(usuario);
                return intentos >= 3;
            }
            catch (Exception ex)
            {
                throw new Exception("Error al verificar si requiere pregunta de seguridad: " + ex.Message, ex);
            }
        }

        public List<UsuarioGestion> ListarUsuarios()
        {
            try
            {
                return mppUsuario.ListarUsuarios();
            }
            catch (Exception ex)
            {
                throw new Exception("Error al listar usuarios: " + ex.Message, ex);
            }
        }

        public void CrearUsuario(string usuario, string contrasena, int rol, Entrenador datosEntrenador = null, int? dniAlumno = null, string confirmarContrasena = null)
        {
            try
            {
                // Validar username no vacío
                if (string.IsNullOrEmpty(usuario))
                {
                    throw new Exception("El nombre de usuario no puede estar vacío");
                }

                // Validar username único (antes del INSERT para mensaje amigable)
                if (mppUsuario.UsuarioExiste(usuario))
                {
                    throw new Exception("El nombre de usuario ya existe. Por favor, elija otro.");
                }

                // Validar requisitos de contraseña
                //ValidarRequisitosContrasena(contrasena);

                // Validar confirmación de contraseña
                if (!string.IsNullOrEmpty(confirmarContrasena) && contrasena != confirmarContrasena)
                {
                    throw new Exception("Las contraseñas no coinciden");
                }

                string contrasenaHash = criptoManager._686DPGetSHA256(contrasena);
                Usuario nuevoUsuario = new Usuario(usuario, contrasenaHash, true, 0, rol, "", "");

                mppUsuario.CrearUsuario(nuevoUsuario);
                mppUsuario.CrearUsuarioIntentos(usuario);

                if (rol == 3)
                {
                    if (datosEntrenador == null)
                    {
                        throw new Exception("Para crear un usuario de tipo Entrenador, se deben proporcionar los datos del entrenador");
                    }

                    datosEntrenador.Usuario = usuario;
                    datosEntrenador.Activo = true;
                    bllEntrenador.CrearEntrenador(datosEntrenador);
                }
                else if (rol == 4)
                {
                    if (dniAlumno == null || !dniAlumno.HasValue)
                    {
                        throw new Exception("Para crear un usuario de tipo Cliente, se debe proporcionar el DNI del alumno asociado");
                    }

                    if (!bllAlumno.AlumnoExiste(dniAlumno.Value))
                    {
                        throw new Exception($"No existe un alumno con DNI {dniAlumno.Value}");
                    }

                    Alumno alumno = bllAlumno.ObtenerAlumno(dniAlumno.Value);

                    if (!string.IsNullOrEmpty(alumno.Usuario))
                    {
                        throw new Exception($"El alumno con DNI {dniAlumno.Value} ya tiene un usuario asociado");
                    }

                    alumno.Usuario = usuario;
                    bllAlumno.ActualizarAlumno(alumno);
                }

                RegistrarEvento("alta_usuario", $"Usuario '{usuario}' creado con rol {rol}");
            }
            catch (Exception ex)
            {
                throw new Exception("Error al crear usuario: " + ex.Message, ex);
            }
        }

        public void CrearUsuario(UsuarioCrearDTO dto)
        {
            try
            {
                if (dto == null)
                {
                    throw new ArgumentException("Los datos del usuario son requeridos");
                }

                if (string.IsNullOrEmpty(dto.Contrasena))
                {
                    dto.Contrasena = GenerarContrasenaAutomatica(dto);
                }

                dto.Validar();

                if (mppUsuario.UsuarioExiste(dto.Usuario))
                {
                    throw new Exception("El nombre de usuario ya existe. Por favor, elija otro.");
                }

                ValidarRequisitosContrasena(dto.Contrasena);
                string contrasenaHash = criptoManager._686DPGetSHA256(dto.Contrasena);
                Usuario nuevoUsuario = new Usuario(dto.Usuario, contrasenaHash, true, 0, dto.Rol, "", "");

                mppUsuario.CrearUsuario(nuevoUsuario);
                mppUsuario.CrearUsuarioIntentos(dto.Usuario);

                if (dto.Rol == 3)
                {
                    Entrenador entrenador = new Entrenador
                    {
                        DNI = dto.EntrenadorDNI.Value,
                        Nombre = dto.EntrenadorNombre,
                        Apellido = dto.EntrenadorApellido,
                        FechaNacimiento = dto.EntrenadorFechaNacimiento.Value,
                        Usuario = dto.Usuario,
                        Activo = true
                    };

                    bllEntrenador.CrearEntrenador(entrenador);
                }
                else if (dto.Rol == 4)
                {
                    if (!bllAlumno.AlumnoExiste(dto.AlumnoDNI.Value))
                    {
                        throw new Exception($"No existe un alumno con DNI {dto.AlumnoDNI.Value}");
                    }

                    Alumno alumno = bllAlumno.ObtenerAlumno(dto.AlumnoDNI.Value);

                    if (!string.IsNullOrEmpty(alumno.Usuario))
                    {
                        throw new Exception($"El alumno con DNI {dto.AlumnoDNI.Value} ya tiene un usuario asociado");
                    }

                    alumno.Usuario = dto.Usuario;
                    bllAlumno.ActualizarAlumno(alumno);
                }

                RegistrarEvento("alta_usuario", $"Usuario '{dto.Usuario}' creado con rol {dto.Rol}");
            }
            catch (ArgumentException ex)
            {
                throw ex;
            }
            catch (Exception ex)
            {
                throw new Exception("Error al crear usuario: " + ex.Message, ex);
            }
        }

        private string GenerarContrasenaAutomatica(UsuarioCrearDTO dto)
        {
            string baseContrasena = string.Empty;

            if (dto.Rol == 3 && !string.IsNullOrEmpty(dto.EntrenadorApellido) && dto.EntrenadorDNI.HasValue)
            {
                baseContrasena = $"{dto.EntrenadorApellido}{dto.EntrenadorDNI.Value}";
            }
            else if (dto.Rol == 4 && dto.AlumnoDNI.HasValue)
            {
                Alumno alumno = bllAlumno.ObtenerAlumno(dto.AlumnoDNI.Value);
                if (alumno != null && !string.IsNullOrEmpty(alumno.Apellido))
                {
                    baseContrasena = $"{alumno.Apellido}{alumno.DNI}";
                }
                else
                {
                    baseContrasena = $"Alumno{dto.AlumnoDNI.Value}";
                }
            }
            else
            {
                baseContrasena = $"{dto.Usuario}123";
            }

            if (baseContrasena.Length < 6)
            {
                baseContrasena += "2024";
            }

            if (!System.Text.RegularExpressions.Regex.IsMatch(baseContrasena, @"[A-Z]"))
            {
                baseContrasena = char.ToUpper(baseContrasena[0]) + baseContrasena.Substring(1);
            }

            if (!System.Text.RegularExpressions.Regex.IsMatch(baseContrasena, @"[^a-zA-Z0-9]"))
            {
                baseContrasena += "!";
            }

            return baseContrasena;
        }

        public List<BE.UsuarioGestion> ListarUsuariosClientesDisponibles()
        {
            try
            {
                return mppUsuario.ListarUsuariosClientesSinAlumno();
            }
            catch (Exception ex)
            {
                throw new Exception("Error al listar usuarios clientes disponibles: " + ex.Message, ex);
            }
        }

        public void ActivarUsuario(string usuario)
        {
            try
            {
                mppUsuario.ActualizarEstado(usuario, true);
                RegistrarEvento("activar_usuario", $"Usuario '{usuario}' activado");
            }
            catch (Exception ex)
            {
                throw new Exception("Error al activar usuario: " + ex.Message, ex);
            }
        }

        public void DesactivarUsuario(string usuario)
        {
            try
            {
                mppUsuario.ActualizarEstado(usuario, false);
                RegistrarEvento("desactivar_usuario", $"Usuario '{usuario}' desactivado");
            }
            catch (Exception ex)
            {
                throw new Exception("Error al desactivar usuario: " + ex.Message, ex);
            }
        }

        public void DesbloquearUsuario(string usuario)
        {
            try
            {
                ReestablecerIntentos(usuario);
                RegistrarEvento("desbloqueo_usuario", $"Usuario '{usuario}' desbloqueado");
            }
            catch (Exception ex)
            {
                throw new Exception("Error al desbloquear usuario: " + ex.Message, ex);
            }
        }
    }
}