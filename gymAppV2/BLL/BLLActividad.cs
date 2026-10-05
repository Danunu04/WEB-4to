using System;
using System.Collections.Generic;
using System.Linq;
using BE;
using MPP;
using Servicios.Singleton;

namespace BLL
{
    /// <summary>
    /// Lógica de negocio para la gestión de actividades y sus turnos semanales
    /// (cada uno con aula, titular y auxiliar). Las inscripciones de alumnos están en BLLInscripcion.
    /// </summary>
    public class BLLActividad
    {
        private MPPActividad mppActividad;
        private BLLEvento bllEvento;

        public BLLActividad()
        {
            mppActividad = new MPPActividad();
            bllEvento = new BLLEvento();
        }

        /// <summary>
        /// Lista todas las actividades activas del gimnasio.
        /// </summary>
        public List<Actividad> ListarActividades()
        {
            try
            {
                return mppActividad.ListarActividades();
            }
            catch (Exception ex)
            {
                throw new Exception("Error al listar actividades: " + ex.Message, ex);
            }
        }

        /// <summary>
        /// Lista todas las actividades (activas e inactivas) con sus instructores, para la gestión.
        /// </summary>
        public List<Actividad> ListarTodasLasActividades()
        {
            try
            {
                return mppActividad.ListarTodasLasActividades();
            }
            catch (Exception ex)
            {
                throw new Exception("Error al listar actividades: " + ex.Message, ex);
            }
        }

        public Actividad ObtenerActividad(int codActividad)
        {
            try
            {
                return mppActividad.ObtenerActividad(codActividad);
            }
            catch (Exception ex)
            {
                throw new Exception("Error al obtener la actividad: " + ex.Message, ex);
            }
        }

        /// <summary>
        /// Horarios semanales de todas las actividades activas.
        /// </summary>
        public List<ActividadHorario> ListarHorariosActivos()
        {
            try
            {
                return mppActividad.ListarHorariosActivos();
            }
            catch (Exception ex)
            {
                throw new Exception("Error al listar los horarios: " + ex.Message, ex);
            }
        }

        /// <summary>
        /// Horarios de la semana actual (lunes a domingo) a los que va alguno de los alumnos
        /// vinculados a la cuenta, según sus inscripciones fijas y puntuales.
        /// </summary>
        public List<ActividadHorario> ListarHorariosPorCliente(string usuario)
        {
            try
            {
                if (string.IsNullOrEmpty(usuario))
                    return new List<ActividadHorario>();

                var bllInscripcion = new BLLInscripcion();
                List<BLLInscripcion.Agenda> agendas = new BLLAlumno().ObtenerAlumnosDeUsuario(usuario)
                    .Select(a => bllInscripcion.ObtenerAgenda(a.DNI))
                    .ToList();

                DateTime lunes = DateTime.Today.AddDays(1 - ActividadHorario.DiaSemanaDesde(DateTime.Today.DayOfWeek));

                return mppActividad.ListarHorariosActivos()
                    .Where(h => agendas.Any(ag => ag.Estado(h.CodHorario, lunes.AddDays(h.DiaSemana - 1)) != EstadoClase.NoAnotado))
                    .ToList();
            }
            catch (Exception ex)
            {
                throw new Exception("Error al listar los horarios del cliente: " + ex.Message, ex);
            }
        }

        /// <summary>
        /// Crea o modifica una actividad junto con sus turnos (cada uno con aula y profesores).
        /// Si actividad.CodActividad es 0 se trata de un alta. Devuelve el código de la actividad.
        /// </summary>
        public int GuardarActividad(Actividad actividad, List<ActividadHorario> horarios)
        {
            // Fuera del try: los errores de validación llegan a la pantalla con su mensaje
            ValidarActividad(actividad, horarios);

            try
            {
                actividad.Descripcion = actividad.Descripcion.Trim();
                actividad.CantXSemana = horarios.Count;

                bool esAlta = actividad.CodActividad == 0;
                if (esAlta)
                {
                    actividad.CodActividad = mppActividad.CrearActividad(actividad);
                }
                else
                {
                    mppActividad.ActualizarActividad(actividad);
                }

                mppActividad.GuardarHorarios(actividad.CodActividad, horarios);

                RegistrarEvento(esAlta
                    ? $"Alta de actividad: {actividad.Descripcion}"
                    : $"Modificación de actividad: {actividad.Descripcion}");

                return actividad.CodActividad;
            }
            catch (Exception ex)
            {
                throw new Exception("Error al guardar la actividad: " + ex.Message, ex);
            }
        }

        /// <summary>
        /// Valida la actividad y sus turnos. Lanza excepción con un mensaje apto para mostrar al usuario.
        /// Además de los datos, verifica que ningún profesor, aula ni alumno quede en dos clases que se
        /// superponen con las demás actividades activas, y que el cupo no quede por debajo de los anotados.
        /// Se valida aunque la actividad quede inactiva: si no, se podría guardar un horario que después
        /// impide reactivarla.
        /// </summary>
        public void ValidarActividad(Actividad actividad, List<ActividadHorario> horarios)
        {
            Dictionary<int, Aula> aulas = ValidarDatos(actividad, horarios);

            string descripcion = actividad.Descripcion.Trim();
            List<ActividadHorario> turnos = horarios.Select(h => new ActividadHorario
            {
                CodHorario = h.CodHorario,
                CodActividad = actividad.CodActividad,
                DiaSemana = h.DiaSemana,
                HoraInicio = h.HoraInicio,
                DuracionMin = h.DuracionMin,
                CodAula = h.CodAula,
                DniTitular = h.DniTitular,
                DniAuxiliar = h.DniAuxiliar,
                DescripcionActividad = descripcion,
                NombreAula = aulas[h.CodAula].Nombre,
                CupoAula = aulas[h.CodAula].Cupo,
                Cupo = actividad.CupoMaximo.HasValue && actividad.CupoMaximo.Value < aulas[h.CodAula].Cupo
                    ? actividad.CupoMaximo.Value
                    : aulas[h.CodAula].Cupo
            }).ToList();

            List<ActividadHorario> otrasActividades = mppActividad.ListarHorariosActivos()
                .Where(h => h.CodActividad != actividad.CodActividad)
                .ToList();

            ValidarTurnosContraOtrasActividades(turnos, otrasActividades);

            if (actividad.CodActividad == 0)
                return;

            // Turnos que ya existían: alumnos anotados
            Actividad anterior = mppActividad.ObtenerActividad(actividad.CodActividad);
            Dictionary<int, ActividadHorario> previos = (anterior?.Horarios ?? new List<ActividadHorario>()).ToDictionary(h => h.CodHorario);
            bool estabaInactiva = anterior != null && !anterior.Activo;
            var bllInscripcion = new BLLInscripcion();

            foreach (ActividadHorario turno in turnos.Where(t => previos.ContainsKey(t.CodHorario)))
            {
                ActividadHorario previo = previos[turno.CodHorario];

                // Si baja el cupo (aula más chica o tope menor), ninguna clase futura puede tener más anotados
                if (turno.Cupo < previo.Cupo)
                {
                    KeyValuePair<DateTime, int> maximo = bllInscripcion.MaximoAnotadosDesde(previo, DateTime.Today);
                    if (maximo.Value > turno.Cupo)
                        throw new CupoCompletoException(
                            $"{descripcion} ({BLLInscripcion.DescribirHorario(previo)}) tiene {maximo.Value} alumnos anotados " +
                            $"el {maximo.Key:dd/MM/yyyy}: el cupo de esa clase no puede quedar en {turno.Cupo}.");
                }

                // Si cambia el horario (o estaba inactiva y no se controló), los alumnos no pueden quedar superpuestos
                bool cambia = previo.DiaSemana != turno.DiaSemana || previo.HoraInicio != turno.HoraInicio || previo.DuracionMin != turno.DuracionMin;
                if (cambia || estabaInactiva)
                    bllInscripcion.ValidarAlumnosSinSuperposicion(turno, otrasActividades);
            }
        }

        /// <summary>
        /// Validaciones de los datos cargados. Devuelve las aulas (código → aula) para no volver a consultarlas.
        /// </summary>
        private Dictionary<int, Aula> ValidarDatos(Actividad actividad, List<ActividadHorario> horarios)
        {
            if (string.IsNullOrWhiteSpace(actividad.Descripcion))
                throw new Exception("La descripción es obligatoria.");

            if (actividad.Descripcion.Trim().Length > 200)
                throw new Exception("La descripción no puede superar los 200 caracteres.");

            if (actividad.CostoInterno < 0 || actividad.PrecioAlumno < 0)
                throw new Exception("El costo y el precio no pueden ser negativos.");

            if (actividad.CupoMaximo.HasValue && (actividad.CupoMaximo.Value < 1 || actividad.CupoMaximo.Value > 1000))
                throw new Exception("El cupo máximo debe estar entre 1 y 1000 alumnos por clase (o vacío para usar el del aula).");

            if (mppActividad.ExisteDescripcion(actividad.Descripcion.Trim(), actividad.CodActividad))
                throw new Exception("Ya existe una actividad con esa descripción.");

            if (horarios == null || horarios.Count == 0)
                throw new Exception("Cargá al menos un horario.");

            Dictionary<int, Aula> aulas = new MPPAula().ListarAulas(false).ToDictionary(a => a.CodAula);
            HashSet<int> profesores = new HashSet<int>(new BLLEntrenador().ListarEntrenadores().Select(e => e.DNI));

            foreach (ActividadHorario horario in horarios)
            {
                if (horario.DiaSemana < 1 || horario.DiaSemana > 7)
                    throw new Exception("Día de la semana inválido.");

                if (horario.DuracionMin < 1 || horario.DuracionMin > 600)
                    throw new Exception("La duración debe estar entre 1 y 600 minutos.");

                if (horario.HoraFin > TimeSpan.FromHours(24))
                    throw new Exception("Un horario no puede terminar después de la medianoche.");

                if (!aulas.TryGetValue(horario.CodAula, out Aula aula))
                    throw new Exception("Elegí un aula para cada horario.");

                if (!aula.Activo)
                    throw new Exception($"El aula {aula.Nombre} está desactivada: elegí otra.");

                if (!profesores.Contains(horario.DniTitular))
                    throw new Exception("Elegí un profesor titular para cada horario.");

                if (horario.DniAuxiliar.HasValue && !profesores.Contains(horario.DniAuxiliar.Value))
                    throw new Exception("El profesor auxiliar elegido no existe.");

                if (horario.DniAuxiliar == horario.DniTitular)
                    throw new Exception("El auxiliar no puede ser el mismo profesor que el titular.");
            }

            // Dos turnos de la misma actividad no pueden superponerse el mismo día
            foreach (var dia in horarios.GroupBy(h => h.DiaSemana))
            {
                var ordenados = dia.OrderBy(h => h.HoraInicio).ToList();
                for (int i = 1; i < ordenados.Count; i++)
                {
                    if (ordenados[i].HoraInicio < ordenados[i - 1].HoraFin)
                        throw new Exception("Hay horarios que se superponen el mismo día.");
                }
            }

            return aulas;
        }

        /// <summary>
        /// Un aula no puede tener dos clases a la vez, y un profesor (titular o auxiliar) no puede estar
        /// en dos clases que se superponen. Compara los turnos con los de las demás actividades activas.
        /// </summary>
        private void ValidarTurnosContraOtrasActividades(List<ActividadHorario> turnos, List<ActividadHorario> otrasActividades)
        {
            foreach (ActividadHorario turno in turnos)
            {
                foreach (ActividadHorario otro in otrasActividades.Where(o => BLLInscripcion.SeSuperponen(o, turno)))
                {
                    if (otro.CodAula == turno.CodAula)
                        throw new ConflictoHorarioException(
                            $"El aula {turno.NombreAula} ya está ocupada por {otro.DescripcionActividad} ({BLLInscripcion.DescribirHorario(otro)}), " +
                            $"que se superpone con {turno.DescripcionActividad} ({BLLInscripcion.DescribirHorario(turno)}).");

                    int dniComun = turno.Profesores.Intersect(otro.Profesores).FirstOrDefault();
                    if (dniComun != 0)
                        throw new ConflictoHorarioException(
                            $"{NombreEntrenador(dniComun)} ya está en {otro.DescripcionActividad} ({BLLInscripcion.DescribirHorario(otro)}), " +
                            $"que se superpone con {turno.DescripcionActividad} ({BLLInscripcion.DescribirHorario(turno)}).");
                }
            }
        }

        private static string NombreEntrenador(int dni)
        {
            Entrenador entrenador = new BLLEntrenador().ListarEntrenadores().FirstOrDefault(e => e.DNI == dni);
            return entrenador != null ? $"{entrenador.Apellido}, {entrenador.Nombre}" : $"El profesor DNI {dni}";
        }

        /// <summary>
        /// Activa o desactiva (baja lógica) una actividad.
        /// </summary>
        public void CambiarEstado(int codActividad, bool activo)
        {
            Actividad actividad = mppActividad.ObtenerActividad(codActividad);
            if (actividad == null)
                throw new Exception("La actividad no existe.");

            // Al reactivar, sus turnos vuelven a contar: no pueden pisar un aula, a un profesor ni a un alumno
            if (activo && !actividad.Activo)
            {
                Dictionary<int, Aula> aulas = new MPPAula().ListarAulas(false).ToDictionary(a => a.CodAula);
                Aula inactiva = actividad.Horarios
                    .Select(h => aulas.TryGetValue(h.CodAula, out Aula aula) ? aula : null)
                    .FirstOrDefault(a => a != null && !a.Activo);
                if (inactiva != null)
                    throw new ConflictoHorarioException($"El aula {inactiva.Nombre} está desactivada: cambiá el aula de la actividad antes de reactivarla.");

                List<ActividadHorario> otrasActividades = mppActividad.ListarHorariosActivos()
                    .Where(h => h.CodActividad != codActividad)
                    .ToList();

                ValidarTurnosContraOtrasActividades(actividad.Horarios, otrasActividades);

                var bllInscripcion = new BLLInscripcion();
                foreach (ActividadHorario turno in actividad.Horarios)
                    bllInscripcion.ValidarAlumnosSinSuperposicion(turno, otrasActividades);
            }

            try
            {
                mppActividad.CambiarEstado(codActividad, activo);

                RegistrarEvento((activo ? "Activación" : "Baja") + $" de actividad: {actividad.Descripcion}");
            }
            catch (Exception ex)
            {
                throw new Exception("Error al cambiar el estado de la actividad: " + ex.Message, ex);
            }
        }

        private void RegistrarEvento(string accion)
        {
            try
            {
                string usuario = Singleton.Instancia?.Usuario?.USUARIO_Usuario;
                if (string.IsNullOrEmpty(usuario))
                    return;

                bllEvento.RegistrarEvento(BLLEvento.EVENTO_CONFIGURACION, usuario, accion, 3, "Actividades");
            }
            catch
            {
                // No impedir la operación principal si falla el log
            }
        }
    }
}
