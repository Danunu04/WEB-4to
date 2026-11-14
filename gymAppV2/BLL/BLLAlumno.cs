using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using BE;
using MPP;
using Servicios.Singleton;

namespace BLL
{
    public class BLLAlumno
    {
        private MPPAlumno mppAlumno;
        private BLLEvento bllEvento;

        public BLLAlumno()
        {
            mppAlumno = new MPPAlumno();
            bllEvento = new BLLEvento();
        }

        /// <summary>
        /// Valida que el DNI sea solo números, de 7-8 dígitos
        /// </summary>
        public void ValidarDNI(string dniStr)
        {
            if (string.IsNullOrEmpty(dniStr))
            {
                throw new Exception("El DNI no puede estar vacío");
            }

            // Solo números
            foreach (char c in dniStr)
            {
                if (!char.IsDigit(c))
                {
                    throw new Exception("El DNI debe contener solo números");
                }
            }

            // Longitud válida (7-8 dígitos)
            if (dniStr.Length < 7 || dniStr.Length > 8)
            {
                throw new Exception("El DNI debe tener 7 u 8 dígitos");
            }
        }

        /// <summary>
        /// Valida que el nombre/apellido sean solo letras y espacios
        /// </summary>
        public void ValidarNombreApellido(string valor, string campo)
        {
            if (string.IsNullOrEmpty(valor))
            {
                throw new Exception($"El {campo} no puede estar vacío");
            }

            // Solo letras y espacios
            foreach (char c in valor)
            {
                if (!char.IsLetter(c) && !char.IsWhiteSpace(c))
                {
                    throw new Exception($"El {campo} debe contener solo letras y espacios");
                }
            }
        }

        /// <summary>
        /// Valida que el teléfono sea formato válido (opcional)
        /// </summary>
        public void ValidarTelefono(string telefono)
        {
            if (string.IsNullOrEmpty(telefono))
            {
                return; // Campo opcional
            }

            // Solo números y caracteres válidos (+, -, espacio)
            foreach (char c in telefono)
            {
                if (!char.IsDigit(c) && c != '+' && c != '-' && c != ' ')
                {
                    throw new Exception("El teléfono debe contener solo números y caracteres válidos (+, -, espacio)");
                }
            }

            if (telefono.Replace("+", "").Replace("-", "").Replace(" ", "").Length < 7)
            {
                throw new Exception("El teléfono debe tener al menos 7 dígitos");
            }
        }

        /// <summary>
        /// Valida que la fecha de nacimiento no sea futura y esté en rango razonable
        /// </summary>
        public void ValidarFechaNacimiento(DateTime fechaNacimiento)
        {
            DateTime ahora = DateTime.Now;

            // No puede ser futura
            if (fechaNacimiento > ahora)
            {
                throw new Exception("La fecha de nacimiento no puede ser futura");
            }

            // No más de 100 años atrás (rango razonable)
            if (fechaNacimiento.Year < ahora.Year - 100)
            {
                throw new Exception("La fecha de nacimiento debe ser dentro de los últimos 100 años");
            }
        }

        /// <summary>
        /// Valida que el peso esté en rango válido (0-500 kg)
        /// </summary>
        public void ValidarPeso(decimal? peso)
        {
            if (!peso.HasValue || peso <= 0)
            {
                throw new Exception("El peso debe ser mayor a 0");
            }

            if (peso >= 500)
            {
                throw new Exception("El peso debe ser menor a 500 kg");
            }
        }

        private void RegistrarEvento(string tipo, string accion, int criticidad = 3)
        {
            try
            {
                var usuario = HttpContext.Current?.Session["UsuarioLogueado"] as Usuario;
                if (usuario == null)
                {
                    // No registrar evento si no hay usuario válido
                    return;
                }
                bllEvento.RegistrarEvento(tipo, usuario.USUARIO_Usuario, accion, criticidad);
            }
            catch
            {
                // No impedir la operación principal si falla el log
            }
        }

        public void CrearAlumno(Alumno alumno)
        {
            try
            {
                // Validar DNI (solo números, 7-8 dígitos)
                ValidarDNI(alumno.DNI.ToString());

                // Validar que no exista duplicado
                if (mppAlumno.AlumnoExiste(alumno.DNI))
                {
                    throw new Exception("Ya existe un alumno con ese DNI");
                }

                // ALUMNOS.dni es FK a USUARIOS.dni: la identidad (Usuario) debe existir primero.
                if (!new BLLUsuario().DniExiste(alumno.DNI))
                {
                    throw new Exception($"No existe un Usuario con DNI {alumno.DNI}. Debe crearse primero la identidad de Usuario.");
                }

                // Validar Peso (0-500 kg) - único campo específico de Alumno
                if (alumno.Peso.HasValue)
                {
                    ValidarPeso(alumno.Peso);
                }

                mppAlumno.CrearAlumno(alumno);

                var usuario = HttpContext.Current?.Session["UsuarioLogueado"] as Usuario;
                if (usuario != null)
                {
                    bllEvento.RegistrarAltaAlumno(usuario.USUARIO_Usuario, alumno.DNI);
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Error al crear alumno: " + ex.Message, ex);
            }
        }

        public Alumno ObtenerAlumno(int dni)
        {
            try
            {
                return mppAlumno.ObtenerAlumno(dni);
            }
            catch (Exception ex)
            {
                throw new Exception("Error al obtener alumno: " + ex.Message, ex);
            }
        }

        public void ActualizarAlumno(Alumno alumno)
        {
            try
            {
                // Validar DNI (solo números, 7-8 dígitos)
                ValidarDNI(alumno.DNI.ToString());

                // Validar Peso (0-500 kg) - único campo específico de Alumno
                if (alumno.Peso.HasValue)
                {
                    ValidarPeso(alumno.Peso);
                }

                mppAlumno.ActualizarAlumno(alumno);

                var usuario = HttpContext.Current?.Session["UsuarioLogueado"] as Usuario;
                if (usuario != null)
                {
                    bllEvento.RegistrarModificacionAlumno(usuario.USUARIO_Usuario, alumno.DNI);
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Error al actualizar alumno: " + ex.Message, ex);
            }
        }

        public bool AlumnoExiste(int dni)
        {
            try
            {
                return mppAlumno.AlumnoExiste(dni);
            }
            catch (Exception ex)
            {
                throw new Exception("Error al verificar si existe el alumno: " + ex.Message, ex);
            }
        }

        public List<Alumno> ListarAlumnos()
        {
            try
            {
                return mppAlumno.ListarAlumnos();
            }
            catch (Exception ex)
            {
                throw new Exception("Error al listar alumnos: " + ex.Message, ex);
            }
        }

        /// <summary>
        /// Alta de un alumno (CUN011.2), la usa el web service. Si ya hay una persona con ese DNI
        /// (ej. un familiar con cuenta), se crea solo el alumno y se lo vincula a esa cuenta o a la
        /// del tutor. Si no existe, se crea también su cuenta de Cliente con los datos personales
        /// (encriptados) y, si se informa un tutor, el alumno queda vinculado a él.
        /// </summary>
        public ResultadoAltaAlumno RegistrarAlumno(SolicitudAlumno solicitud)
        {
            if (solicitud == null)
                throw new ReglaNegocioException("alumnos_err_datos", "Faltan los datos del alumno.");

            int dni = solicitud.Dni;
            ComoRegla(() => ValidarDNI(dni.ToString()), "alumnos_msg_dni_invalido");
            if (solicitud.Peso.HasValue)
                ComoRegla(() => ValidarPeso(solicitud.Peso), "alumnos_msg_peso_invalido");

            if (AlumnoExiste(dni))
                throw new ReglaNegocioException("alumnos_err_ya_existe", $"Ya existe un alumno con DNI {dni}.", dni);

            bool conTutor = !string.IsNullOrWhiteSpace(solicitud.UsuarioVinculo);
            var bllUsuario = new BLLUsuario();
            var resultado = new ResultadoAltaAlumno { Dni = dni };

            try
            {
                Usuario persona = bllUsuario.ObtenerUsuarioPorDni(dni);
                if (persona != null)
                {
                    // La persona ya tiene cuenta: se crea el alumno y se vincula
                    CrearAlumno(new Alumno(dni, solicitud.Peso, false, solicitud.Activo, ""));
                    AsociarFamiliar(dni,
                        conTutor ? solicitud.UsuarioVinculo : persona.USUARIO_Usuario,
                        conTutor ? solicitud.Parentesco : "Titular");
                    return resultado;
                }

                // Persona nueva: datos personales obligatorios y cuenta de Cliente
                ComoRegla(() => ValidarNombreApellido(solicitud.Nombre, "nombre"), "alumnos_msg_nombre_oblig");
                ComoRegla(() => ValidarNombreApellido(solicitud.Apellido, "apellido"), "alumnos_msg_nombre_oblig");
                ComoRegla(() => ValidarTelefono(solicitud.Telefono), "alumnos_msg_telefono_invalido");
                if (!solicitud.FechaNacimiento.HasValue)
                    throw new ReglaNegocioException("alumnos_msg_fecha_invalida", "La fecha de nacimiento es obligatoria.");
                ComoRegla(() => ValidarFechaNacimiento(solicitud.FechaNacimiento.Value), "alumnos_msg_fecha_invalida");

                string usuario = string.IsNullOrWhiteSpace(solicitud.Usuario) ? $"cliente_{dni}" : solicitud.Usuario.Trim();
                if (bllUsuario.UsuarioExiste(usuario))
                    throw new ReglaNegocioException("alumnos_asistente_error_usuario_existe", $"El usuario '{usuario}' ya existe.", usuario);

                string contrasena = solicitud.Contrasena;
                if (string.IsNullOrEmpty(contrasena))
                {
                    contrasena = bllUsuario.GenerarContrasenaSegura();
                    resultado.ContrasenaGenerada = contrasena;
                }
                else
                {
                    ComoRegla(() => bllUsuario.ValidarRequisitosContrasena(contrasena), "registro_err_contrasena");
                }

                bllUsuario.CrearUsuario(usuario, contrasena, PerfilesSistema.RolCliente,
                    solicitud.Nombre.Trim(), solicitud.Apellido.Trim(),
                    string.IsNullOrWhiteSpace(solicitud.Telefono) ? null : solicitud.Telefono.Trim(),
                    null, solicitud.FechaNacimiento, null, dni);
                resultado.UsuarioCreado = usuario;

                Alumno creado = mppAlumno.ObtenerAlumno(dni);
                if (creado != null)
                {
                    creado.Peso = solicitud.Peso;
                    creado.Activo = solicitud.Activo;
                    mppAlumno.ActualizarAlumno(creado);
                }

                if (conTutor)
                {
                    // CrearUsuario vinculó al propio titular: se reemplaza por el tutor
                    DesasociarFamiliar(dni, usuario);
                    AsociarFamiliar(dni, solicitud.UsuarioVinculo, solicitud.Parentesco);
                }

                return resultado;
            }
            catch (ReglaNegocioException)
            {
                throw;
            }
            catch (Exception ex)
            {
                throw new Exception("Error al registrar el alumno: " + ex.Message, ex);
            }
        }

        /// <summary>
        /// Los validadores de esta clase lanzan Exception con un mensaje para el usuario: se pasa a
        /// ReglaNegocioException para que el web service y las pantallas lo muestren tal cual.
        /// </summary>
        private static void ComoRegla(Action validacion, string clave)
        {
            try
            {
                validacion();
            }
            catch (ReglaNegocioException)
            {
                throw;
            }
            catch (Exception ex)
            {
                throw new ReglaNegocioException(clave, ex.Message);
            }
        }

        /// <summary>
        /// Baja lógica (CUN011.1): el alumno queda inactivo y se conservan sus pagos, rutinas,
        /// inscripciones y 1RM. Las cuentas de Cliente o Familiar vinculadas se desactivan si no
        /// les queda otro alumno activo (un familiar con otro hijo activo sigue entrando).
        /// Devuelve los usuarios que se desactivaron.
        /// </summary>
        public List<string> DarDeBajaAlumno(int dni)
        {
            try
            {
                Alumno alumno = mppAlumno.ObtenerAlumno(dni);
                if (alumno == null)
                {
                    throw new ReglaNegocioException("alumnos_err_no_existe", $"No existe un alumno con DNI {dni}.", dni);
                }

                if (alumno.Activo)
                {
                    alumno.Activo = false;
                    mppAlumno.ActualizarAlumno(alumno);
                }

                var bllUsuario = new BLLUsuario();
                var desactivados = new List<string>();

                foreach (AlumnoUsuarioVinculo vinculo in mppAlumno.ObtenerUsuariosDeAlumno(dni))
                {
                    Usuario cuenta = bllUsuario.ObtenerUsuario(vinculo.Usuario);
                    if (cuenta == null || !cuenta.USUARIO_Activo)
                        continue;

                    if (cuenta.USUARIO_Rol != PerfilesSistema.RolCliente && cuenta.USUARIO_Rol != PerfilesSistema.RolFamiliar)
                        continue;

                    bool tieneOtroActivo = (mppAlumno.ObtenerAlumnosDeUsuario(cuenta.USUARIO_Usuario) ?? new List<Alumno>())
                        .Any(a => a.DNI != dni && a.Activo);

                    if (!tieneOtroActivo)
                    {
                        bllUsuario.DesactivarUsuario(cuenta.USUARIO_Usuario);
                        desactivados.Add(cuenta.USUARIO_Usuario);
                    }
                }

                var usuario = HttpContext.Current?.Session["UsuarioLogueado"] as Usuario;
                if (usuario != null)
                {
                    bllEvento.RegistrarBajaAlumno(usuario.USUARIO_Usuario, dni);
                }

                return desactivados;
            }
            catch (Exception ex)
            {
                throw new Exception("Error al dar de baja al alumno: " + ex.Message, ex);
            }
        }

        /// <summary>
        /// Borrado físico del alumno, sus rutinas y sus vínculos. Solo lo usa BLLUsuario cuando a
        /// un cliente se le cambia el DNI (hay que liberar la FK). Para dar de baja a un alumno
        /// desde la pantalla se usa DarDeBajaAlumno.
        /// </summary>
        public void EliminarAlumno(int dni)
        {
            try
            {
                if (!AlumnoExiste(dni))
                {
                    throw new Exception($"No existe un alumno con DNI {dni}");
                }

                mppAlumno.EliminarAlumno(dni);

                var usuario = HttpContext.Current?.Session["UsuarioLogueado"] as Usuario;
                if (usuario != null)
                {
                    bllEvento.RegistrarBajaAlumno(usuario.USUARIO_Usuario, dni);
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Error al eliminar alumno: " + ex.Message, ex);
            }
        }

        /// <summary>
        /// Vincula una cuenta de usuario (titular o familiar: madre/padre/tutor) a un alumno.
        /// A diferencia del viejo esquema 1:1, un alumno puede tener varios usuarios vinculados
        /// y un usuario puede estar vinculado a varios alumnos (ej. varios hijos).
        /// </summary>
        public void AsociarFamiliar(int dni, string usuario, string parentesco = null)
        {
            try
            {
                if (!AlumnoExiste(dni))
                {
                    throw new ReglaNegocioException("alumnos_err_no_existe", $"No existe un alumno con DNI {dni}.", dni);
                }

                BLLUsuario bllUsuario = new BLLUsuario();
                BE.Usuario usuarioBD = bllUsuario.ObtenerUsuario(usuario);
                if (usuarioBD == null)
                {
                    throw new ReglaNegocioException("alumnos_err_usuario_no_existe", $"No existe el usuario '{usuario}'.", usuario);
                }

                List<AlumnoUsuarioVinculo> vinculos = mppAlumno.ObtenerUsuariosDeAlumno(dni);
                if (vinculos.Any(v => v.Usuario.Equals(usuario, StringComparison.OrdinalIgnoreCase)))
                {
                    throw new ReglaNegocioException("alumnos_err_ya_vinculado", $"El usuario '{usuario}' ya está vinculado a este alumno.", usuario);
                }

                mppAlumno.AsociarFamiliar(dni, usuario, parentesco);
                bllEvento.RegistrarAsociarUsuario(usuario, dni);
            }
            catch (Exception ex)
            {
                throw new Exception("Error al asociar familiar: " + ex.Message, ex);
            }
        }

        public void DesasociarFamiliar(int dni, string usuario)
        {
            try
            {
                if (!AlumnoExiste(dni))
                {
                    throw new ReglaNegocioException("alumnos_err_no_existe", $"No existe un alumno con DNI {dni}.", dni);
                }

                mppAlumno.DesasociarFamiliar(dni, usuario);
                bllEvento.RegistrarDesasociarUsuario(usuario, dni);
            }
            catch (Exception ex)
            {
                throw new Exception("Error al desasociar familiar: " + ex.Message, ex);
            }
        }

        public List<AlumnoUsuarioVinculo> ObtenerUsuariosDeAlumno(int dni)
        {
            try
            {
                return mppAlumno.ObtenerUsuariosDeAlumno(dni);
            }
            catch (Exception ex)
            {
                throw new Exception("Error al obtener los usuarios vinculados al alumno: " + ex.Message, ex);
            }
        }

        /// <summary>
        /// Alumnos vinculados a una cuenta (el propio titular y/o los alumnos a su cargo como familiar).
        /// </summary>
        public List<Alumno> ObtenerAlumnosDeUsuario(string usuario)
        {
            try
            {
                if (string.IsNullOrEmpty(usuario))
                {
                    return new List<Alumno>();
                }

                return mppAlumno.ObtenerAlumnosDeUsuario(usuario);
            }
            catch (Exception ex)
            {
                throw new Exception("Error al obtener los alumnos del usuario: " + ex.Message, ex);
            }
        }

        public int CantidadAlumnosAsociados(string usuario)
        {
            try
            {
                if (string.IsNullOrEmpty(usuario))
                {
                    return 0;
                }

                return mppAlumno.CantidadAlumnosAsociados(usuario);
            }
            catch (Exception ex)
            {
                throw new Exception("Error al obtener la cantidad de alumnos asociados: " + ex.Message, ex);
            }
        }

        /// <summary>
        /// Actualiza los vínculos ALUMNOS_USUARIOS cuando se renombra una cuenta de usuario.
        /// </summary>
        public void RenombrarUsuarioEnVinculos(string usuarioOriginal, string usuarioNuevo)
        {
            try
            {
                mppAlumno.RenombrarUsuarioEnVinculos(usuarioOriginal, usuarioNuevo);
            }
            catch (Exception ex)
            {
                throw new Exception("Error al renombrar el usuario en los vínculos de alumnos: " + ex.Message, ex);
            }
        }
    }
}