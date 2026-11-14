using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using BE;
using MPP;

namespace BLL
{
    /// <summary>
    /// Lógica de negocio para la gestión de rutinas de entrenamiento.
    /// </summary>
    public class BLLRutina
    {
        // Rangos válidos de cada ejercicio de la rutina
        public const int SERIES_MAXIMAS = 20;
        public const int REPETICIONES_MAXIMAS = 100;
        public const decimal PESO_MAXIMO = 500m;
        public const int DESCANSO_MAXIMO_SEGUNDOS = 900;
        public const int OBSERVACIONES_MAXIMO = 300;

        private MPPRutina mppRutina;
        private MPPRutinaEjercicio mppRutinaEjercicio;
        private BLLEvento bllEvento;

        public BLLRutina()
        {
            mppRutina = new MPPRutina();
            mppRutinaEjercicio = new MPPRutinaEjercicio();
            bllEvento = new BLLEvento();
        }

        private void ValidarRutina(Rutina rutina)
        {
            if (string.IsNullOrWhiteSpace(rutina.Descripcion))
            {
                throw new Exception("La descripción de la rutina es obligatoria");
            }

            if (rutina.Fecha == DateTime.MinValue)
            {
                throw new Exception("La fecha de la rutina es obligatoria");
            }

            if (rutina.DniAlumno <= 0)
            {
                throw new Exception("Debe seleccionar un alumno");
            }

            if (rutina.DniEntrenador <= 0)
            {
                throw new Exception("Debe seleccionar un entrenador");
            }

            if (rutina.CodActividad <= 0)
            {
                throw new Exception("Debe seleccionar una actividad");
            }
        }

        public List<Rutina> ListarRutinas()
        {
            try
            {
                return mppRutina.ListarRutinas();
            }
            catch (Exception ex)
            {
                throw new Exception("Error al listar rutinas: " + ex.Message, ex);
            }
        }

        /// <summary>
        /// Lista las rutinas de los alumnos asociados al usuario Cliente logueado.
        /// </summary>
        public List<Rutina> ListarRutinasPorCliente(string usuario)
        {
            try
            {
                if (string.IsNullOrEmpty(usuario))
                {
                    return new List<Rutina>();
                }

                return mppRutina.ListarRutinasPorCliente(usuario);
            }
            catch (Exception ex)
            {
                throw new Exception("Error al listar rutinas del cliente: " + ex.Message, ex);
            }
        }

        public Rutina ObtenerRutina(int codRutina)
        {
            try
            {
                return mppRutina.ObtenerRutina(codRutina);
            }
            catch (Exception ex)
            {
                throw new Exception("Error al obtener rutina: " + ex.Message, ex);
            }
        }

        public void CrearRutina(Rutina rutina)
        {
            try
            {
                ValidarRutina(rutina);

                int codRutina = mppRutina.CrearRutina(rutina);
                rutina.CodRutina = codRutina;

                var usuario = HttpContext.Current?.Session["UsuarioLogueado"] as Usuario;
                if (usuario != null)
                {
                    bllEvento.RegistrarRutinaAlta(usuario.USUARIO_Usuario, rutina.DniAlumno);
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Error al crear rutina: " + ex.Message, ex);
            }
        }

        public void ActualizarRutina(Rutina rutina)
        {
            try
            {
                ValidarRutina(rutina);

                mppRutina.ActualizarRutina(rutina);

                var usuario = HttpContext.Current?.Session["UsuarioLogueado"] as Usuario;
                if (usuario != null)
                {
                    bllEvento.RegistrarRutinaModificacion(usuario.USUARIO_Usuario, rutina.DniAlumno);
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Error al actualizar rutina: " + ex.Message, ex);
            }
        }

        public void EliminarRutina(int codRutina)
        {
            try
            {
                var rutina = mppRutina.ObtenerRutina(codRutina);

                mppRutina.EliminarRutina(codRutina);

                var usuario = HttpContext.Current?.Session["UsuarioLogueado"] as Usuario;
                if (usuario != null && rutina != null)
                {
                    bllEvento.RegistrarRutinaBaja(usuario.USUARIO_Usuario, rutina.DniAlumno);
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Error al eliminar rutina: " + ex.Message, ex);
            }
        }

        // ==================== EJERCICIOS DE LA RUTINA (CUN008) ====================

        /// <summary>
        /// Ejercicios de una rutina con el porcentaje de la 1RM que representa cada peso (si el
        /// alumno tiene una 1RM registrada en ese ejercicio). Un Cliente o Familiar solo puede ver
        /// las rutinas de los alumnos asociados a su cuenta.
        /// </summary>
        public List<RutinaEjercicio> ListarEjercicios(int codRutina, Usuario usuario)
        {
            Rutina rutina = ValidarAccesoRutina(codRutina, usuario, paraModificar: false);

            try
            {
                List<RutinaEjercicio> ejercicios = mppRutinaEjercicio.ListarPorRutina(codRutina);
                CompletarPorcentajesRM(ejercicios, rutina.DniAlumno);
                return ejercicios;
            }
            catch (Exception ex)
            {
                throw new Exception("Error al listar los ejercicios de la rutina: " + ex.Message, ex);
            }
        }

        public void AgregarEjercicio(RutinaEjercicio ejercicio, Usuario usuario)
        {
            Rutina rutina = ValidarAccesoRutina(ejercicio.CodRutina, usuario, paraModificar: true);
            ValidarEjercicio(ejercicio);

            try
            {
                ejercicio.Observaciones = string.IsNullOrWhiteSpace(ejercicio.Observaciones) ? null : ejercicio.Observaciones.Trim();
                mppRutinaEjercicio.Crear(ejercicio);
                bllEvento.RegistrarRutinaModificacion(usuario.USUARIO_Usuario, rutina.DniAlumno);
            }
            catch (Exception ex)
            {
                throw new Exception("Error al agregar el ejercicio: " + ex.Message, ex);
            }
        }

        public void QuitarEjercicio(int codRutinaEjercicio, Usuario usuario)
        {
            RutinaEjercicio ejercicio = mppRutinaEjercicio.Obtener(codRutinaEjercicio);
            if (ejercicio == null)
            {
                throw new ReglaNegocioException("rutinas_ej_err_no_existe", "El ejercicio ya no está en la rutina.");
            }

            Rutina rutina = ValidarAccesoRutina(ejercicio.CodRutina, usuario, paraModificar: true);

            try
            {
                mppRutinaEjercicio.Eliminar(codRutinaEjercicio);
                bllEvento.RegistrarRutinaModificacion(usuario.USUARIO_Usuario, rutina.DniAlumno);
            }
            catch (Exception ex)
            {
                throw new Exception("Error al quitar el ejercicio: " + ex.Message, ex);
            }
        }

        private void ValidarEjercicio(RutinaEjercicio ejercicio)
        {
            if (ejercicio.CodEjercicio <= 0)
                throw new ReglaNegocioException("rutinas_ej_err_ejercicio", "Elegí un ejercicio.");

            if (ejercicio.Series < 1 || ejercicio.Series > SERIES_MAXIMAS)
                throw new ReglaNegocioException("rutinas_ej_err_series",
                    $"Las series deben ser un número entero entre 1 y {SERIES_MAXIMAS}.", SERIES_MAXIMAS);

            if (ejercicio.Repeticiones < 1 || ejercicio.Repeticiones > REPETICIONES_MAXIMAS)
                throw new ReglaNegocioException("rutinas_ej_err_reps",
                    $"Las repeticiones deben ser un número entero entre 1 y {REPETICIONES_MAXIMAS}.", REPETICIONES_MAXIMAS);

            if (ejercicio.PesoLevantado.HasValue && (ejercicio.PesoLevantado <= 0 || ejercicio.PesoLevantado > PESO_MAXIMO))
                throw new ReglaNegocioException("rutinas_ej_err_peso",
                    $"El peso debe ser mayor que 0 y hasta {PESO_MAXIMO:0} kg.", PESO_MAXIMO.ToString("0"));

            if (ejercicio.DescansoSegundos.HasValue && (ejercicio.DescansoSegundos < 0 || ejercicio.DescansoSegundos > DESCANSO_MAXIMO_SEGUNDOS))
                throw new ReglaNegocioException("rutinas_ej_err_descanso",
                    $"El descanso debe estar entre 0 y {DESCANSO_MAXIMO_SEGUNDOS} segundos.", DESCANSO_MAXIMO_SEGUNDOS);

            if (!string.IsNullOrEmpty(ejercicio.Observaciones) && ejercicio.Observaciones.Trim().Length > OBSERVACIONES_MAXIMO)
                throw new ReglaNegocioException("rutinas_ej_err_observaciones",
                    $"Las observaciones no pueden superar los {OBSERVACIONES_MAXIMO} caracteres.", OBSERVACIONES_MAXIMO);
        }

        /// <summary>
        /// La rutina tiene que existir. Un Cliente o Familiar solo puede verla si es de un alumno
        /// suyo, y nunca modificarla.
        /// </summary>
        private Rutina ValidarAccesoRutina(int codRutina, Usuario usuario, bool paraModificar)
        {
            Rutina rutina = mppRutina.ObtenerRutina(codRutina);
            if (rutina == null || usuario == null)
            {
                throw new ReglaNegocioException("rutinas_ej_err_rutina", "La rutina ya no existe.");
            }

            bool esCliente = usuario.USUARIO_Rol == PerfilesSistema.RolCliente || usuario.USUARIO_Rol == PerfilesSistema.RolFamiliar;
            if (!esCliente)
            {
                return rutina;
            }

            if (paraModificar)
            {
                throw new ReglaNegocioException("rutinas_ej_err_sin_permiso", "No podés modificar las rutinas.");
            }

            bool esSuya = (new BLLAlumno().ObtenerAlumnosDeUsuario(usuario.USUARIO_Usuario) ?? new List<Alumno>())
                .Any(a => a.DNI == rutina.DniAlumno);
            if (!esSuya)
            {
                throw new ReglaNegocioException("rutinas_ej_err_rutina", "La rutina ya no existe.");
            }

            return rutina;
        }

        /// <summary>
        /// Porcentaje de la última 1RM del alumno en cada ejercicio con peso. Si no hay historial
        /// de 1RM (o la tabla todavía no existe), los porcentajes quedan vacíos.
        /// </summary>
        private static void CompletarPorcentajesRM(List<RutinaEjercicio> ejercicios, int dniAlumno)
        {
            if (!ejercicios.Any(e => e.PesoLevantado.HasValue))
                return;

            List<RegistroRM> historial;
            try
            {
                historial = new MPPCalculoRM().ListarRegistrosPorAlumno(dniAlumno);
            }
            catch
            {
                return;
            }

            foreach (RutinaEjercicio ejercicio in ejercicios.Where(e => e.PesoLevantado.HasValue))
            {
                // El historial viene del más reciente al más viejo
                RegistroRM ultimo = historial.FirstOrDefault(r => r.CodEjercicio == ejercicio.CodEjercicio);
                if (ultimo != null && ultimo.PesoRM > 0)
                {
                    ejercicio.PorcentajeRM = Math.Round(ejercicio.PesoLevantado.Value / ultimo.PesoRM * 100m, 0);
                }
            }
        }
    }
}
