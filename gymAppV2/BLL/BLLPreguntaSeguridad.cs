using System;
using System.Collections.Generic;
using System.Linq;
using BE;
using MPP;

namespace BLL
{
    /// <summary>
    /// Gestiona las preguntas de seguridad para recuperación y desbloqueo de cuentas.
    /// </summary>
    public class BLLPreguntaSeguridad
    {
        private MPPPreguntaSeguridad mppPreguntaSeguridad;
        private MPPUsuario mppUsuario;
        private BLLAlumno bllAlumno;

        public BLLPreguntaSeguridad()
        {
            mppPreguntaSeguridad = new MPPPreguntaSeguridad();
            mppUsuario = new MPPUsuario();
            bllAlumno = new BLLAlumno();
        }

        /// <summary>
        /// Obtiene la pregunta de seguridad almacenada para el usuario.
        /// </summary>
        public PreguntaSeguridad ObtenerPreguntaPorUsuario(string usuario)
        {
            try
            {
                return mppPreguntaSeguridad.ObtenerPreguntaPorUsuario(usuario);
            }
            catch (Exception ex)
            {
                throw new Exception("Error al obtener pregunta de seguridad: " + ex.Message, ex);
            }
        }

        /// <summary>
        /// Guarda o actualiza la pregunta de seguridad del usuario.
        /// </summary>
        public void GuardarPregunta(PreguntaSeguridad pregunta)
        {
            try
            {
                if (pregunta == null)
                {
                    throw new ArgumentException("La pregunta de seguridad no puede ser nula");
                }

                mppPreguntaSeguridad.GuardarPregunta(pregunta);
            }
            catch (Exception ex)
            {
                throw new Exception("Error al guardar pregunta de seguridad: " + ex.Message, ex);
            }
        }

        /// <summary>
        /// Valida la respuesta de seguridad de un usuario, ignorando mayúsculas/minúsculas y espacios.
        /// </summary>

        public string ObtenerRespuestaPorUsuario(string usuario)
        {
            try
            {
                return mppPreguntaSeguridad.ObtenerRespuestaPorUsuario(usuario);
            }
            catch (Exception ex)
            {
                throw new Exception("Error al obtener respuesta de seguridad: " + ex.Message, ex);
            }
        }

        public bool ValidarRespuesta(string usuario, string respuesta)
        {
            try
            {
                if (string.IsNullOrEmpty(usuario) || string.IsNullOrEmpty(respuesta))
                {
                    return false;
                }

                return mppPreguntaSeguridad.ValidarRespuesta(usuario, respuesta);
            }
            catch (Exception ex)
            {
                throw new Exception("Error al validar respuesta: " + ex.Message, ex);
            }
        }

        /// <summary>
        /// Genera la pregunta de seguridad predeterminada basada en el año de nacimiento del usuario.
        /// La respuesta esperada es el año completo (ejemplo: 1990).
        /// </summary>
        public PreguntaSeguridad GenerarPreguntaSeguridad(string usuario)
        {
            try
            {
                if (string.IsNullOrEmpty(usuario))
                {
                    throw new ArgumentException("El usuario no puede ser nulo o vacío");
                }

                DateTime? fechaNacimiento = mppUsuario.ObtenerFechaNacimiento(usuario);
                if (!fechaNacimiento.HasValue)
                {
                    throw new Exception("No se pudo obtener la fecha de nacimiento del usuario");
                }

                int anioNacimiento = fechaNacimiento.Value.Year;
                return new PreguntaSeguridad
                {
                    Usuario = usuario,
                    Pregunta = $"¿En qué año naciste?",
                    Respuesta = anioNacimiento.ToString(),
                    Tipo = TipoPreguntaSeguridad.FechaNacimiento
                };
            }
            catch (Exception ex)
            {
                throw new Exception("Error al generar pregunta de seguridad: " + ex.Message, ex);
            }
        }

        /// <summary>
        /// Sobrecarga que recibe el año directamente. Se mantiene por compatibilidad.
        /// </summary>
        public string GenerarPreguntaSeguridad(string usuario, int anioNacimiento)
        {
            return $"¿En qué año naciste?";
        }

        /// <summary>
        /// Genera la segunda pregunta de seguridad cuando el usuario tiene más de un alumno asociado.
        /// Mezcla el nombre real de uno de sus alumnos con nombres aleatorios para evitar adivinación.
        /// La respuesta esperada es el nombre completo del alumno real.
        /// </summary>
        public PreguntaSeguridad GenerarPreguntaSeguridadAlumno(string usuario)
        {
            try
            {
                if (string.IsNullOrEmpty(usuario))
                {
                    throw new ArgumentException("El usuario no puede ser nulo o vacío");
                }

                // La segunda pregunta solo aplica cuando hay más de un alumno asociado.
                int cantidad = bllAlumno.CantidadAlumnosAsociados(usuario);
                if (cantidad <= 1)
                {
                    throw new Exception("El usuario debe tener más de un alumno asociado para generar esta pregunta de seguridad");
                }

                List<Alumno> alumnos = bllAlumno.ListarAlumnos()
                    .Where(a => !string.IsNullOrEmpty(a.Usuario) &&
                                a.Usuario.Equals(usuario, StringComparison.OrdinalIgnoreCase))
                    .ToList();

                if (alumnos.Count == 0)
                {
                    throw new Exception("El usuario no tiene alumnos asociados");
                }

                // Elegir un alumno real aleatoriamente.
                Random random = new Random();
                Alumno alumnoReal = alumnos[random.Next(alumnos.Count)];
                string nombreReal = $"{alumnoReal.Nombre} {alumnoReal.Apellido}".Trim();

                // Nombres aleatorios para confundir.
                string[] nombresAleatorios = new[]
                {
                    "Carlos López",
                    "María Fernández",
                    "Juan Pérez",
                    "Ana García",
                    "Diego Martínez",
                    "Laura Rodríguez"
                };

                List<string> opciones = new List<string> { nombreReal };
                opciones.AddRange(nombresAleatorios
                    .Where(n => !n.Equals(nombreReal, StringComparison.OrdinalIgnoreCase))
                    .Take(3));

                // Mezclar opciones.
                opciones = opciones.OrderBy(x => Guid.NewGuid()).ToList();

                string opcionesTexto = string.Join(", ", opciones);

                return new PreguntaSeguridad
                {
                    Usuario = usuario,
                    Pregunta = $"¿Conoce a {opcionesTexto}?",
                    Respuesta = nombreReal,
                    Tipo = TipoPreguntaSeguridad.AlumnoAsociado
                };
            }
            catch (Exception ex)
            {
                throw new Exception("Error al generar pregunta de seguridad por alumno: " + ex.Message, ex);
            }
        }

        /// <summary>
        /// Crea automáticamente la pregunta de seguridad para un usuario nuevo.
        /// Usa la pregunta de fecha de nacimiento por defecto.
        /// </summary>
        public void CrearPreguntaSeguridadPorDefecto(string usuario)
        {
            try
            {
                PreguntaSeguridad pregunta = GenerarPreguntaSeguridad(usuario);
                GuardarPregunta(pregunta);
            }
            catch (Exception ex)
            {
                throw new Exception("Error al crear pregunta de seguridad por defecto: " + ex.Message, ex);
            }
        }
    }
}
