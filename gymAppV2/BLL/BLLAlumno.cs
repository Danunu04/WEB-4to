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

        private void RegistrarEvento(string tipo, string accion)
        {
            try
            {
                var usuario = HttpContext.Current?.Session["UsuarioLogueado"] as Usuario;
                string usr = usuario?.USUARIO_Usuario ?? "sistema";
                bllEvento.RegistrarEvento(tipo, usr, accion);
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

                // Validar Nombre (solo letras y espacios)
                ValidarNombreApellido(alumno.Nombre, "Nombre");

                // Validar Apellido (solo letras y espacios)
                ValidarNombreApellido(alumno.Apellido, "Apellido");

                // Validar Teléfono (opcional, formato válido)
                ValidarTelefono(alumno.Telefono?.ToString());

                // Validar Fecha de Nacimiento (no futura, rango razonable)
                ValidarFechaNacimiento(alumno.FechaNacimiento);

                // Validar Peso (0-500 kg)
                ValidarPeso(alumno.Peso);

                mppAlumno.CrearAlumno(alumno);
                RegistrarEvento("alta_alumno", $"Alumno DNI {alumno.DNI} creado");
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

                // Validar Nombre (solo letras y espacios)
                ValidarNombreApellido(alumno.Nombre, "Nombre");

                // Validar Apellido (solo letras y espacios)
                ValidarNombreApellido(alumno.Apellido, "Apellido");

                // Validar Teléfono (opcional, formato válido)
                ValidarTelefono(alumno.Telefono?.ToString());

                // Validar Fecha de Nacimiento (no futura, rango razonable)
                ValidarFechaNacimiento(alumno.FechaNacimiento);

                // Validar Peso (0-500 kg)
                ValidarPeso(alumno.Peso);

                mppAlumno.ActualizarAlumno(alumno);
                RegistrarEvento("modificacion_alumno", $"Alumno DNI {alumno.DNI} modificado");
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
                RegistrarEvento("baja_alumno", $"Alumno DNI {dni} eliminado (con rutinas asociadas)");
            }
            catch (Exception ex)
            {
                throw new Exception("Error al eliminar alumno: " + ex.Message, ex);
            }
        }

        public void AsociarUsuario(int dni, string usuario)
        {
            try
            {
                // Validar que el alumno exista
                if (!AlumnoExiste(dni))
                {
                    throw new Exception($"No existe un alumno con DNI {dni}");
                }

                // Validar que el usuario exista
                BLLUsuario bllUsuario = new BLLUsuario();
                BE.Usuario usuarioBD = bllUsuario.ObtenerUsuario(usuario);

                if (usuarioBD == null)
                {
                    throw new Exception($"No existe el usuario '{usuario}'");
                }

                // Validar que el usuario sea Cliente (Rol 4)
                if (usuarioBD.USUARIO_Rol != 4)
                {
                    throw new Exception($"El usuario '{usuario}' no es de tipo Cliente (Rol {usuarioBD.USUARIO_Rol})");
                }

                // Validar que el alumno no tenga ya un usuario
                Alumno alumno = ObtenerAlumno(dni);
                if (!string.IsNullOrEmpty(alumno.Usuario))
                {
                    throw new Exception($"El alumno con DNI {dni} ya tiene un usuario asociado: {alumno.Usuario}");
                }

                // Validar que el usuario no tenga ya un alumno asociado
                List<BE.UsuarioGestion> usuarios = bllUsuario.ListarUsuarios();
                var usuarioConAlumno = usuarios.FirstOrDefault(u => u.USUARIO_Usuario == usuario && u.DNI.HasValue);

                if (usuarioConAlumno != null)
                {
                    throw new Exception($"El usuario '{usuario}' ya tiene un alumno asociado (DNI: {usuarioConAlumno.DNI})");
                }

                mppAlumno.AsociarUsuario(dni, usuario);
                RegistrarEvento("asociar_usuario", $"Usuario '{usuario}' asociado a alumno DNI {dni}");
            }
            catch (Exception ex)
            {
                throw new Exception("Error al asociar usuario: " + ex.Message, ex);
            }
        }

        public void DesasociarUsuario(int dni)
        {
            try
            {
                if (!AlumnoExiste(dni))
                {
                    throw new Exception($"No existe un alumno con DNI {dni}");
                }

                Alumno alumno = ObtenerAlumno(dni);
                if (string.IsNullOrEmpty(alumno.Usuario))
                {
                    throw new Exception($"El alumno con DNI {dni} no tiene un usuario asociado");
                }

                mppAlumno.AsociarUsuario(dni, null);
                RegistrarEvento("desasociar_usuario", $"Usuario desasociado de alumno DNI {dni}");
            }
            catch (Exception ex)
            {
                throw new Exception("Error al desasociar usuario: " + ex.Message, ex);
            }
        }

        public List<Alumno> ListarAlumnosSinUsuario()
        {
            try
            {
                return mppAlumno.ListarAlumnosSinUsuario();
            }
            catch (Exception ex)
            {
                throw new Exception("Error al listar alumnos sin usuario: " + ex.Message, ex);
            }
        }
    }
}