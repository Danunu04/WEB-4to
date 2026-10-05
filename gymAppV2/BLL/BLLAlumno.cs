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
                    throw new Exception($"No existe un alumno con DNI {dni}");
                }

                BLLUsuario bllUsuario = new BLLUsuario();
                BE.Usuario usuarioBD = bllUsuario.ObtenerUsuario(usuario);
                if (usuarioBD == null)
                {
                    throw new Exception($"No existe el usuario '{usuario}'");
                }

                List<AlumnoUsuarioVinculo> vinculos = mppAlumno.ObtenerUsuariosDeAlumno(dni);
                if (vinculos.Any(v => v.Usuario.Equals(usuario, StringComparison.OrdinalIgnoreCase)))
                {
                    throw new Exception($"El usuario '{usuario}' ya está vinculado a este alumno");
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
                    throw new Exception($"No existe un alumno con DNI {dni}");
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