using System;
using System.Collections.Generic;
using System.Linq;
using BE;
using MPP;
using Servicios;
using Servicios.Singleton;
using SERVICIOS;
using static BE.ConstantesSeguridad;

namespace BLL
{
    public class BLLUsuario
    {
        private MPPUsuario mppUsuario;
        private CriptoManager criptoManager;
        private BLLEntrenador bllEntrenador;
        private BLLAlumno bllAlumno;
        private BLLEvento bllEvento;
        private BLLPreguntaSeguridad bllPreguntaSeguridad;

        public BLLUsuario()
        {
            mppUsuario = new MPPUsuario();
            criptoManager = new CriptoManager();
            bllEntrenador = new BLLEntrenador();
            bllAlumno = new BLLAlumno();
            bllEvento = new BLLEvento();
            bllPreguntaSeguridad = new BLLPreguntaSeguridad();
        }

        private void RegistrarEvento(string tipo, string accion, int criticidad = 1)
        {
            try
            {
                var usuario = System.Web.HttpContext.Current?.Session["UsuarioLogueado"] as Usuario;
                if (usuario == null)
                {
                    // No registrar evento si no hay usuario válido
                    return;
                }
                bllEvento.RegistrarEvento(tipo, usuario.USUARIO_Usuario, accion, criticidad);
            }
            catch (Exception ex)
            {
                // Fallback: no impedir la operación principal si falla el log,
                // pero dejar traza en el diagnostic trace para poder diagnosticar.
                System.Diagnostics.Trace.WriteLine($"[AUDITORIA FALLBACK] Tipo={tipo}, Accion={accion}, Error={ex.Message}");
            }
        }

        /// <summary>
        /// Valida las credenciales de un usuario sin exponer si el usuario existe.
        /// Trata usuarios inactivos como credenciales inválidas para evitar enumeración.
        /// Bloquea la cuenta tras el máximo de intentos fallidos.
        /// </summary>
        public bool ValidarLogin(string usuario, string contrasena)
        {
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
                    // Tratar usuarios inactivos como credenciales inválidas para evitar enumeración.
                    throw new ExcepcionesLogIn(ResultadosLogIn.InvalidUsername);
                }

                // Si cuenta bloqueada → lanzar excepción específica para redirigir a preguntas de seguridad
                if (mppUsuario.UsuarioEstaBloqueado(usuario))
                {
                    bllEvento.RegistrarBloqueoUsuario(usuario);
                    throw new ExcepcionesLogIn(ResultadosLogIn.AccountLocked);
                }

                string contrasenaHash = criptoManager.GenerarHashSHA256(contrasena);
                string contrasenaBD = mppUsuario.ObtenerContrasena(usuario);

                if (contrasenaHash == contrasenaBD)
                {
                    // Login exitoso: reestablecer intentos
                    ReestablecerIntentos(usuario);
                    return true;
                }
                else
                {
                    // Contraseña incorrecta: registrar intento fallido y lanzar excepción
                    RegistrarIntentoFallido(usuario);
                    throw new ExcepcionesLogIn(ResultadosLogIn.InvalidPassword);
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

        /// <summary>
        /// Incrementa el contador de intentos fallidos de login.
        /// Si se alcanza el límite, bloquea la cuenta y registra el evento.
        /// </summary>
        public void RegistrarIntentoFallido(string usuario)
        {
            try
            {
                mppUsuario.AgregarIntento(usuario);

                // Verificar si se bloquea por llegar a 3 intentos
                int intentos = mppUsuario.ObtenerIntentos(usuario);
                if (intentos >= MAX_INTENTOS_LOGIN)
                {
                    bllEvento.RegistrarBloqueoUsuario(usuario);
                    throw new ExcepcionesLogIn(ResultadosLogIn.AccountLocked);
                }
            }
            catch (ExcepcionesLogIn)
            {
                throw;
            }
            catch (Exception ex)
            {
                throw new Exception("Error al registrar el intento fallido: " + ex.Message, ex);
            }
        }

        /// <summary>
        /// Devuelve la cantidad de intentos de login restantes antes del bloqueo.
        /// </summary>
        public int ObtenerIntentosRestantes(string usuario)
        {
            try
            {
                int intentos = mppUsuario.ObtenerIntentos(usuario);
                return MAX_INTENTOS_LOGIN - intentos;
            }
            catch (Exception ex)
            {
                throw new Exception("Error al obtener los intentos restantes: " + ex.Message, ex);
            }
        }

        /// <summary>
        /// Reinicia el contador de intentos fallidos tras un login exitoso o desbloqueo.
        /// </summary>
        public void ReestablecerIntentos(string usuario)
        {
            try
            {
                mppUsuario.ReestablecerIntentos(usuario);
            }
            catch (Exception ex)
            {
                throw new Exception("Error al reestablecer los intentos: " + ex.Message, ex);
            }
        }

        public DateTime? ObtenerFechaNacimiento(string usuario)
        {
            try
            {
                return mppUsuario.ObtenerFechaNacimiento(usuario);
            }
            catch (Exception ex)
            {
                throw new Exception("Error al obtener la fecha de nacimiento: " + ex.Message, ex);
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

        /// <summary>
        /// Carga el usuario en el singleton de sesión tras una autenticación exitosa.
        /// </summary>
        public void LogearUsuario(Usuario usuario)
        {
            try
            {
                var sesion = Singleton.Instancia;
                if (sesion == null)
                    throw new Exception("No se pudo acceder a la sesión HTTP.");

                sesion.LogIn(usuario);
            }
            catch (Exception ex)
            {
                throw new Exception("Error al iniciar sesión: " + ex.Message, ex);
            }
        }

        /// <summary>
        /// Limpia el singleton de sesión. No invalida la cookie de forms; usar FormsAuthentication.SignOut por separado.
        /// </summary>
        public void DeslogearUsuario()
        {
            try
            {
                var sesion = Singleton.Instancia;
                if (sesion == null)
                    throw new Exception("No se pudo acceder a la sesión HTTP.");

                sesion.LogOut();
            }
            catch (Exception ex)
            {
                throw new Exception("Error al cerrar sesión: " + ex.Message, ex);
            }
        }

        /// <summary>
        /// Indica si existe una sesión activa en el singleton. Es tolerante a fallos de sesión HTTP.
        /// </summary>
        public bool UsuarioEstaLogueado()
        {
            try
            {
                var sesion = Singleton.Instancia;
                return sesion != null && sesion.IsLogged();
            }
            catch
            {
                return false;
            }
        }

        public bool UsuarioExiste(string usuario)
        {
            try
            {
                return mppUsuario.UsuarioExiste(usuario);
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Verifica que la contraseña cumpla los requisitos mínimos de seguridad.
        /// </summary>
        public void ValidarRequisitosContrasena(string contrasena)
        {
            if (string.IsNullOrEmpty(contrasena))
            {
                throw new Exception("La contraseña no puede estar vacía");
            }

            if (contrasena.Length < CONTRASENA_MIN_LENGTH)
            {
                throw new Exception($"La contraseña debe tener al menos {CONTRASENA_MIN_LENGTH} caracteres");
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

        /// <summary>
        /// Cambia la contraseña de un usuario validando complejidad y evitando reutilización.
        /// Guarda el nuevo hash en el historial de contraseñas.
        /// </summary>
        public void CambiarContrasena(string usuario, string nuevaContrasena)
        {
            try
            {
                ValidarRequisitosContrasena(nuevaContrasena);
                string nuevaContrasenaHash = criptoManager.GenerarHashSHA256(nuevaContrasena);

                if (mppUsuario.ContrasenaFueUtilizada(usuario, nuevaContrasenaHash))
                {
                    throw new Exception("No puedes reutilizar una contraseña anterior");
                }

                mppUsuario.GuardarContrasenaEnHistorial(usuario, nuevaContrasenaHash);
                mppUsuario.ActualizarContrasena(usuario, nuevaContrasenaHash);
                mppUsuario.ReestablecerIntentos(usuario);

                bllEvento.RegistrarCambioContrasena(usuario);
            }
            catch (Exception ex)
            {
                throw new Exception("Error al cambiar contraseña: " + ex.Message, ex);
            }
        }

        /// <summary>
        /// Marca el primer login como finalizado para el usuario.
        /// Se llama después de que el usuario configuró sus preguntas de seguridad.
        /// </summary>
        public void FinalizarPrimerLogin(string usuario)
        {
            try
            {
                if (string.IsNullOrEmpty(usuario))
                {
                    throw new ArgumentException("El usuario no puede ser nulo o vacío");
                }

                mppUsuario.FinalizarPrimerLogin(usuario);
                bllEvento.RegistrarEvento("configuracion_preguntas_seguridad", usuario, "Preguntas de seguridad configuradas", 1, "Autenticación");
            }
            catch (Exception ex)
            {
                throw new Exception("Error al finalizar el primer login: " + ex.Message, ex);
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

        public void CrearUsuario(string usuario, string contrasena, int rol,
            string nombre = null, string apellido = null, string telefono = null,
            string email = null, DateTime? fechaNacimiento = null,
            Entrenador datosEntrenador = null, int? dniAlumno = null, string confirmarContrasena = null, bool activo = true)
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

                // Validar confirmación de contraseña
                if (!string.IsNullOrEmpty(confirmarContrasena) && contrasena != confirmarContrasena)
                {
                    throw new Exception("Las contraseñas no coinciden");
                }

                string contrasenaHash = criptoManager.GenerarHashSHA256(contrasena);

                // Determinar tipo y datos personales según el rol
                string tipo = "";
                int dni = 0;
                DateTime? fechaNac = null;

                if (rol == 3) // Entrenador
                {
                    if (datosEntrenador == null)
                    {
                        throw new Exception("Para crear un usuario de tipo Entrenador, se deben proporcionar los datos del entrenador");
                    }
                    if (!fechaNacimiento.HasValue)
                    {
                        throw new Exception("Para crear un usuario de tipo Entrenador, se debe proporcionar la fecha de nacimiento");
                    }
                    tipo = "Entrenador";
                    dni = datosEntrenador.DNI;
                    fechaNac = fechaNacimiento;

                    // Crear usuario primero con datos personales; primerLogin = 1 fuerza cambio de contraseña.
                    Usuario nuevoUsuario = new Usuario(usuario, contrasenaHash, activo, false, 0, rol, tipo, dni, nombre, apellido, telefono, email, fechaNac, "", "", true);
                    mppUsuario.CrearUsuario(nuevoUsuario);

                    // Luego crear registro en ENTRENADORES (solo datos específicos del rol)
                    datosEntrenador.Usuario = usuario;
                    datosEntrenador.Activo = activo;
                    bllEntrenador.CrearEntrenador(datosEntrenador);
                }
                else if (rol == 4) // Cliente
                {
                    if (dniAlumno == null || !dniAlumno.HasValue)
                    {
                        throw new Exception("Para crear un usuario de tipo Cliente, se debe proporcionar el DNI del alumno");
                    }
                    if (!fechaNacimiento.HasValue)
                    {
                        throw new Exception("Para crear un usuario de tipo Cliente, se debe proporcionar la fecha de nacimiento");
                    }

                    // En esquema normalizado: primero creamos USUARIOS, luego ALUMNOS
                    tipo = "Cliente";
                    dni = dniAlumno.Value;
                    fechaNac = fechaNacimiento;

                    // Crear usuario con datos personales; primerLogin = 1 fuerza cambio de contraseña.
                    Usuario nuevoUsuario = new Usuario(usuario, contrasenaHash, activo, false, 0, rol, tipo, dni, nombre, apellido, telefono, email, fechaNac, "", "", true);
                    mppUsuario.CrearUsuario(nuevoUsuario);

                    // Luego crear registro en ALUMNOS (solo datos específicos del rol)
                    Alumno nuevoAlumno = new Alumno(dni, null, activo, true, "", "", usuario);
                    bllAlumno.CrearAlumno(nuevoAlumno);
                }
                else // Empleado (Admin/Recepcionista)
                {
                    tipo = "Empleado";
                    dni = 999999990 + rol; // DNI placeholder
                    nombre = nombre ?? "Empleado";
                    apellido = apellido ?? usuario;
                    fechaNac = fechaNacimiento ?? DateTime.Parse("1990-01-01");
                    telefono = telefono ?? "0000-0000";

                    // primerLogin = 1 fuerza cambio de contraseña en el primer login.
                    Usuario nuevoUsuario = new Usuario(usuario, contrasenaHash, activo, false, 0, rol, tipo, dni, nombre, apellido, telefono, email, fechaNac, "", "", true);
                    mppUsuario.CrearUsuario(nuevoUsuario);
                }

                // Guardar contraseña inicial en el historial para evitar reutilización.
                try
                {
                    mppUsuario.GuardarContrasenaEnHistorial(usuario, contrasenaHash);
                }
                catch
                {
                    // No impedir la creación del usuario si falla el historial.
                }

                bllEvento.RegistrarAltaUsuario(usuario, rol);

                // Crear pregunta de seguridad por defecto basada en fecha de nacimiento.
                // Si falla, se propaga el error para que el operador lo corrija; el usuario ya fue creado.
                bllPreguntaSeguridad.CrearPreguntaSeguridadPorDefecto(usuario);
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
                string contrasenaHash = criptoManager.GenerarHashSHA256(dto.Contrasena);

                // Determinar tipo y datos personales según el rol
                string tipo = "";
                int dni = 0;
                string nombre = "";
                string apellido = "";
                string telefono = "";
                string email = "";
                DateTime? fechaNacimiento = null;

                if (dto.Rol == 3) // Entrenador
                {
                    tipo = "Entrenador";
                    dni = dto.EntrenadorDNI.Value;
                    nombre = dto.EntrenadorNombre;
                    apellido = dto.EntrenadorApellido;
                    fechaNacimiento = dto.EntrenadorFechaNacimiento;

                    // Crear usuario primero con datos personales; primerLogin = 1 fuerza cambio de contraseña.
                    Usuario nuevoUsuario = new Usuario(dto.Usuario, contrasenaHash, true, false, 0, dto.Rol, tipo, dni, nombre, apellido, telefono, email, fechaNacimiento, "", "", true);
                    mppUsuario.CrearUsuario(nuevoUsuario);

                    // Luego crear registro en ENTRENADORES
                    Entrenador entrenador = new Entrenador(dni, 0, true, "", "", dto.Usuario);
                    bllEntrenador.CrearEntrenador(entrenador);
                }
                else if (dto.Rol == 4) // Cliente
                {
                    tipo = "Cliente";
                    dni = dto.AlumnoDNI.Value;
                    nombre = dto.AlumnoNombre;
                    apellido = dto.AlumnoApellido;
                    telefono = dto.AlumnoTelefono;
                    email = dto.AlumnoEmail;
                    fechaNacimiento = dto.AlumnoFechaNacimiento;

                    // Crear usuario primero; primerLogin = 1 fuerza cambio de contraseña.
                    Usuario nuevoUsuario = new Usuario(dto.Usuario, contrasenaHash, true, false, 0, dto.Rol, tipo, dni, nombre, apellido, telefono, email, fechaNacimiento, "", "", true);
                    mppUsuario.CrearUsuario(nuevoUsuario);

                    // Luego crear registro en ALUMNOS
                    Alumno alumno = new Alumno(dni, null, false, true, "", "", dto.Usuario);
                    bllAlumno.CrearAlumno(alumno);
                }
                else // Empleado
                {
                    tipo = "Empleado";
                    dni = 999999990 + dto.Rol;
                    nombre = "Empleado";
                    apellido = dto.Usuario;
                    fechaNacimiento = DateTime.Parse("1990-01-01");

                    // primerLogin = 1 fuerza cambio de contraseña en el primer login.
                    Usuario nuevoUsuario = new Usuario(dto.Usuario, contrasenaHash, true, false, 0, dto.Rol, tipo, dni, nombre, apellido, telefono, email, fechaNacimiento, "", "", true);
                    mppUsuario.CrearUsuario(nuevoUsuario);
                }

                // Guardar contraseña inicial en el historial para evitar reutilización.
                try
                {
                    mppUsuario.GuardarContrasenaEnHistorial(dto.Usuario, contrasenaHash);
                }
                catch
                {
                    // No impedir la creación del usuario si falla el historial.
                }

                bllEvento.RegistrarAltaUsuario(dto.Usuario, dto.Rol);

                // Crear pregunta de seguridad por defecto basada en fecha de nacimiento.
                // Si falla, se propaga el error para que el operador lo corrija; el usuario ya fue creado.
                bllPreguntaSeguridad.CrearPreguntaSeguridadPorDefecto(dto.Usuario);
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

        /// <summary>
        /// Genera una contraseña aleatoria segura que cumple con los requisitos del modelo.
        /// Longitud mínima 10 caracteres, incluye mayúscula, minúscula, número y carácter especial.
        /// </summary>
        public string GenerarContrasenaSegura()
        {
            const string mayusculas = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
            const string minusculas = "abcdefghijklmnopqrstuvwxyz";
            const string numeros = "0123456789";
            const string especiales = "!@#$%^&*()_+-=[]{}|;:,.<>?";
            const string todos = mayusculas + minusculas + numeros + especiales;

            var random = new Random();
            var caracteres = new List<char>
            {
                mayusculas[random.Next(mayusculas.Length)],
                minusculas[random.Next(minusculas.Length)],
                numeros[random.Next(numeros.Length)],
                especiales[random.Next(especiales.Length)]
            };

            while (caracteres.Count < 12)
            {
                caracteres.Add(todos[random.Next(todos.Length)]);
            }

            // Mezclar para que no siempre aparezcan en el mismo orden.
            return new string(caracteres.OrderBy(c => random.Next()).ToArray());
        }

        private string GenerarContrasenaAutomatica(UsuarioCrearDTO dto)
        {
            return GenerarContrasenaSegura();
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
                bllEvento.RegistrarActivarUsuario(usuario);
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
                bllEvento.RegistrarDesactivarUsuario(usuario);
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
                bllEvento.RegistrarDesbloqueoUsuario(usuario);
            }
            catch (Exception ex)
            {
                throw new Exception("Error al desbloquear usuario: " + ex.Message, ex);
            }
        }

        public void ModificarUsuario(string usuarioOriginal, string nuevoUsuario, string nombre, string apellido,
            string telefono, string email, DateTime? fechaNacimiento, int rol, bool activo, int nuevoDNI)
        {
            try
            {
                // Validar que el usuario existe
                Usuario usuarioExistente = mppUsuario.ObtenerUsuario(usuarioOriginal);
                if (usuarioExistente == null)
                {
                    throw new Exception("El usuario no existe");
                }

                // Si cambió el nombre de usuario, validar que el nuevo no esté en uso
                if (usuarioOriginal != nuevoUsuario && mppUsuario.UsuarioExiste(nuevoUsuario))
                {
                    throw new Exception("El nombre de usuario ya existe. Por favor, elija otro.");
                }

                // Determinar tipo según rol
                string tipo = rol == 3 ? "Entrenador" : rol == 4 ? "Cliente" : "Empleado";

                // Si es entrenador o cliente y cambió el DNI, primero eliminar el registro relacionado
                // para evitar violación de clave foránea al actualizar USUARIOS
                bool dniCambio = usuarioExistente.USUARIO_DNI != nuevoDNI;

                if (rol == 3 && dniCambio)
                {
                    Entrenador entrenadorViejo = bllEntrenador.ObtenerEntrenador(usuarioExistente.USUARIO_DNI);
                    if (entrenadorViejo != null)
                    {
                        bllEntrenador.EliminarEntrenador(usuarioExistente.USUARIO_DNI);
                    }
                }
                else if (rol == 4 && dniCambio)
                {
                    Alumno alumnoViejo = bllAlumno.ObtenerAlumno(usuarioExistente.USUARIO_DNI);
                    if (alumnoViejo != null)
                    {
                        bllAlumno.EliminarAlumno(usuarioExistente.USUARIO_DNI);
                    }
                }

                // Actualizar datos principales en USUARIOS
                Usuario usuarioActualizado = new Usuario(
                    nuevoUsuario,
                    usuarioExistente.USUARIO_Contras, // Mantener contraseña existente
                    activo,
                    usuarioExistente.USUARIO_Bloqueado, // Mantener estado de bloqueo
                    usuarioExistente.USUARIO_Intentos, // Mantener intentos
                    rol,
                    tipo,
                    nuevoDNI, // DNI actualizado
                    nombre,
                    apellido,
                    telefono,
                    email,
                    fechaNacimiento,
                    usuarioExistente.USUARIO_DVV,
                    usuarioExistente.USUARIO_DVH,
                    usuarioExistente.USUARIO_PrimerLogin // Mantener estado de primer login
                );

                mppUsuario.ActualizarUsuario(usuarioActualizado, usuarioOriginal);

                // Si es entrenador, crear/actualizar registro específico
                if (rol == 3)
                {
                    if (dniCambio)
                    {
                        // Verificar si ya existe un entrenador con el nuevo DNI
                        Entrenador entrenadorExistente = bllEntrenador.ObtenerEntrenador(nuevoDNI);
                        if (entrenadorExistente == null)
                        {
                            Entrenador entrenadorNuevo = new Entrenador(nuevoDNI, 0, activo, "", "", nuevoUsuario);
                            bllEntrenador.CrearEntrenador(entrenadorNuevo);
                        }
                        else
                        {
                            entrenadorExistente.Usuario = nuevoUsuario;
                            entrenadorExistente.Activo = activo;
                            bllEntrenador.ActualizarEntrenador(entrenadorExistente);
                        }
                    }
                    else
                    {
                        // DNI no cambió, solo actualizar usuario
                        Entrenador entrenador = bllEntrenador.ObtenerEntrenador(usuarioExistente.USUARIO_DNI);
                        if (entrenador != null)
                        {
                            entrenador.Usuario = nuevoUsuario;
                            bllEntrenador.ActualizarEntrenador(entrenador);
                        }
                    }
                }
                // Si es cliente, crear/actualizar registro específico
                else if (rol == 4)
                {
                    if (dniCambio)
                    {
                        // Verificar si ya existe un alumno con el nuevo DNI
                        Alumno alumnoExistente = bllAlumno.ObtenerAlumno(nuevoDNI);
                        if (alumnoExistente == null)
                        {
                            Alumno alumnoNuevo = new Alumno(nuevoDNI, null, activo, true, "", "", nuevoUsuario);
                            bllAlumno.CrearAlumno(alumnoNuevo);
                        }
                        else
                        {
                            alumnoExistente.Usuario = nuevoUsuario;
                            alumnoExistente.Activo = activo;
                            bllAlumno.ActualizarAlumno(alumnoExistente);
                        }
                    }
                    else
                    {
                        // DNI no cambió, solo actualizar usuario
                        Alumno alumno = bllAlumno.ObtenerAlumno(usuarioExistente.USUARIO_DNI);
                        if (alumno != null)
                        {
                            alumno.Usuario = nuevoUsuario;
                            bllAlumno.ActualizarAlumno(alumno);
                        }
                    }
                }

                bllEvento.RegistrarModificacionUsuario(usuarioOriginal, nuevoUsuario);
            }
            catch (Exception ex)
            {
                throw new Exception("Error al modificar usuario: " + ex.Message, ex);
            }
        }
    }
}