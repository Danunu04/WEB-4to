using System;
using System.Collections.Generic;
using System.Linq;
using BE;
using MPP;
using Servicios.Singleton;

namespace BLL
{
    /// <summary>
    /// Inscripción de alumnos a clases, por turno y por fecha.
    ///
    /// Un alumno va a la clase del turno H en la fecha F si tiene una inscripción fija de H
    /// que cubre F y no tiene una Baja puntual de H en F, o si tiene un Alta puntual de H en F.
    ///
    /// Lo puede hacer el propio Cliente/Familiar (solo con sus alumnos vinculados) o quien
    /// tenga el permiso GestionActividades. No se modifican clases que ya pasaron.
    /// </summary>
    public class BLLInscripcion
    {
        private MPPInscripcion mppInscripcion;
        private MPPActividad mppActividad;
        private BLLEvento bllEvento;

        public BLLInscripcion()
        {
            mppInscripcion = new MPPInscripcion();
            mppActividad = new MPPActividad();
            bllEvento = new BLLEvento();
        }

        /// <summary>
        /// Inscripciones fijas y puntuales de un alumno, para consultar muchas fechas sin ir a la base cada vez.
        /// </summary>
        public class Agenda
        {
            public List<InscripcionFija> Fijas { get; set; } = new List<InscripcionFija>();
            public List<InscripcionClase> Clases { get; set; } = new List<InscripcionClase>();

            public bool TieneFija(int codHorario, DateTime fecha)
            {
                return Fijas.Any(f => f.CodHorario == codHorario && f.Cubre(fecha));
            }

            /// <summary>
            /// True si tiene una inscripción fija al turno que sigue vigente sin fecha de fin desde la fecha indicada.
            /// </summary>
            public bool TieneFijaAbierta(int codHorario, DateTime desde)
            {
                return Fijas.Any(f => f.CodHorario == codHorario && !f.FechaHasta.HasValue && f.FechaDesde <= desde.Date);
            }

            public InscripcionClase Excepcion(int codHorario, DateTime fecha)
            {
                return Clases.FirstOrDefault(c => c.CodHorario == codHorario && c.Fecha == fecha.Date);
            }

            public EstadoClase Estado(int codHorario, DateTime fecha)
            {
                InscripcionClase excepcion = Excepcion(codHorario, fecha);
                if (excepcion != null && excepcion.Tipo == InscripcionClase.TipoAlta)
                    return EstadoClase.Puntual;

                if (TieneFija(codHorario, fecha) && (excepcion == null || excepcion.Tipo != InscripcionClase.TipoBaja))
                    return EstadoClase.Fija;

                return EstadoClase.NoAnotado;
            }
        }

        #region Fechas

        /// <summary>
        /// Primera fecha igual o posterior a "desde" que cae en el día de la semana indicado (1 = Lunes ... 7 = Domingo).
        /// </summary>
        public static DateTime ProximaFecha(int diaSemana, DateTime desde)
        {
            DateTime fecha = desde.Date;
            while (ActividadHorario.DiaSemanaDesde(fecha.DayOfWeek) != diaSemana)
            {
                fecha = fecha.AddDays(1);
            }
            return fecha;
        }

        /// <summary>
        /// Fechas a las que se anota el alumno según el alcance, empezando por la clase elegida.
        /// Para "Todas" devuelve solo la primera fecha (es una inscripción fija, sin fin).
        /// </summary>
        public static List<DateTime> FechasDelAlcance(DateTime fecha, AlcanceInscripcion alcance)
        {
            var fechas = new List<DateTime> { fecha.Date };

            switch (alcance)
            {
                case AlcanceInscripcion.RestoDelMes:
                    for (DateTime f = fecha.Date.AddDays(7); f.Month == fecha.Month; f = f.AddDays(7))
                        fechas.Add(f);
                    break;

                case AlcanceInscripcion.ProximasCuatroSemanas:
                    for (int semana = 1; semana < 4; semana++)
                        fechas.Add(fecha.Date.AddDays(7 * semana));
                    break;
            }

            return fechas;
        }

        #endregion

        #region Consultas

        public Agenda ObtenerAgenda(int dni)
        {
            try
            {
                return new Agenda
                {
                    Fijas = mppInscripcion.ListarFijasDeAlumno(dni),
                    Clases = mppInscripcion.ListarClasesDeAlumno(dni)
                };
            }
            catch (Exception ex)
            {
                throw new Exception("Error al obtener las inscripciones del alumno: " + ex.Message, ex);
            }
        }

        public ActividadHorario ObtenerHorario(int codHorario)
        {
            try
            {
                return mppActividad.ObtenerHorario(codHorario);
            }
            catch (Exception ex)
            {
                throw new Exception("Error al obtener el turno: " + ex.Message, ex);
            }
        }

        /// <summary>
        /// Alumnos que van a la clase del turno en la fecha, con el tipo de inscripción de cada uno.
        /// </summary>
        public Dictionary<int, EstadoClase> ListarAsistentes(int codHorario, DateTime fecha)
        {
            try
            {
                var asistentes = new Dictionary<int, EstadoClase>();
                List<InscripcionClase> excepciones = mppInscripcion.ListarClasesDeHorario(codHorario, fecha);

                foreach (InscripcionFija fija in mppInscripcion.ListarFijasDeHorario(codHorario, fecha))
                {
                    if (!excepciones.Any(e => e.DNI == fija.DNI && e.Tipo == InscripcionClase.TipoBaja))
                        asistentes[fija.DNI] = EstadoClase.Fija;
                }

                foreach (InscripcionClase alta in excepciones.Where(e => e.Tipo == InscripcionClase.TipoAlta))
                {
                    asistentes[alta.DNI] = EstadoClase.Puntual;
                }

                return asistentes;
            }
            catch (Exception ex)
            {
                throw new Exception("Error al listar los alumnos de la clase: " + ex.Message, ex);
            }
        }

        /// <summary>
        /// Alumnos con inscripción fija al turno vigente desde la próxima clase y sin fecha de fin.
        /// </summary>
        public List<int> ListarAlumnosConFija(int codHorario)
        {
            try
            {
                ActividadHorario horario = mppActividad.ObtenerHorario(codHorario);
                if (horario == null)
                    return new List<int>();

                DateTime proxima = ProximaFecha(horario.DiaSemana, DateTime.Today);
                return mppInscripcion.ListarFijasDeHorario(codHorario, proxima)
                    .Where(f => !f.FechaHasta.HasValue)
                    .Select(f => f.DNI)
                    .Distinct()
                    .ToList();
            }
            catch (Exception ex)
            {
                throw new Exception("Error al listar las inscripciones fijas: " + ex.Message, ex);
            }
        }

        #endregion

        #region Cliente / Familiar (sus propios alumnos)

        public void AnotarPropio(string usuario, int dni, int codHorario, DateTime fecha, AlcanceInscripcion alcance)
        {
            ValidarAlumnoPropio(usuario, dni);
            Anotar(dni, codHorario, fecha, alcance);
        }

        public void DesanotarPropio(string usuario, int dni, int codHorario, DateTime fecha, AlcanceBaja alcance)
        {
            ValidarAlumnoPropio(usuario, dni);
            Desanotar(dni, codHorario, fecha, alcance);
        }

        /// <summary>
        /// Anota al alumno en varias clases sueltas (turno + fecha) de una vez.
        /// Valida todas antes de guardar, para no dejar la operación a medias por un error de validación.
        /// Las clases a las que ya va se ignoran. Devuelve cuántas clases se anotaron.
        /// </summary>
        public int AnotarPropioVarias(string usuario, int dni, IEnumerable<KeyValuePair<int, DateTime>> clases)
        {
            ValidarAlumnoPropio(usuario, dni);

            Agenda agenda = ObtenerAgenda(dni);
            List<KeyValuePair<int, DateTime>> pendientes = clases
                .Select(c => new KeyValuePair<int, DateTime>(c.Key, c.Value.Date))
                .Distinct()
                .Where(c => agenda.Estado(c.Key, c.Value) == EstadoClase.NoAnotado)
                .ToList();

            List<ActividadHorario> activos = mppActividad.ListarHorariosActivos();
            var elegidas = new List<KeyValuePair<ActividadHorario, DateTime>>();
            foreach (var clase in pendientes)
            {
                ActividadHorario horario = ValidarAnotacion(dni, clase.Key, clase.Value);
                ValidarSinSuperposicion(horario, clase.Value, AlcanceInscripcion.SoloEstaClase, agenda, activos);
                ValidarCupo(horario, clase.Value, AlcanceInscripcion.SoloEstaClase, agenda);
                elegidas.Add(new KeyValuePair<ActividadHorario, DateTime>(horario, clase.Value));
            }

            // Dos clases elegidas el mismo día que se pisan entre sí
            for (int i = 0; i < elegidas.Count; i++)
            {
                for (int j = i + 1; j < elegidas.Count; j++)
                {
                    if (elegidas[i].Value == elegidas[j].Value && SeSuperponen(elegidas[i].Key, elegidas[j].Key))
                        throw new ConflictoHorarioException(
                            $"Elegiste dos clases que se superponen el {elegidas[i].Value:dd/MM/yyyy}: " +
                            $"{elegidas[i].Key.DescripcionActividad} ({DescribirHorario(elegidas[i].Key)}) y " +
                            $"{elegidas[j].Key.DescripcionActividad} ({DescribirHorario(elegidas[j].Key)}).");
                }
            }

            foreach (var clase in pendientes)
                Anotar(dni, clase.Key, clase.Value, AlcanceInscripcion.SoloEstaClase);

            return pendientes.Count;
        }

        /// <summary>
        /// Da de baja al alumno de varias clases sueltas (solo esas fechas). Las clases a las que no va se ignoran.
        /// Devuelve cuántas clases se dieron de baja.
        /// </summary>
        public int DesanotarPropioVarias(string usuario, int dni, IEnumerable<KeyValuePair<int, DateTime>> clases)
        {
            ValidarAlumnoPropio(usuario, dni);

            Agenda agenda = ObtenerAgenda(dni);
            List<KeyValuePair<int, DateTime>> pendientes = clases
                .Select(c => new KeyValuePair<int, DateTime>(c.Key, c.Value.Date))
                .Distinct()
                .Where(c => agenda.Estado(c.Key, c.Value) != EstadoClase.NoAnotado)
                .ToList();

            foreach (var clase in pendientes)
                ValidarClase(clase.Key, clase.Value);

            foreach (var clase in pendientes)
                Desanotar(dni, clase.Key, clase.Value, AlcanceBaja.SoloEstaClase);

            return pendientes.Count;
        }

        private static void ValidarAlumnoPropio(string usuario, int dni)
        {
            if (string.IsNullOrEmpty(usuario) || !new BLLAlumno().ObtenerAlumnosDeUsuario(usuario).Any(a => a.DNI == dni))
                throw new InscripcionException("El alumno no está vinculado a tu cuenta.");
        }

        #endregion

        #region Gestión (permiso GestionActividades)

        public void AnotarComoGestor(int dni, int codHorario, DateTime fecha, AlcanceInscripcion alcance)
        {
            ValidarPermisoGestion();
            Anotar(dni, codHorario, fecha, alcance);
        }

        public void DesanotarComoGestor(int dni, int codHorario, DateTime fecha, AlcanceBaja alcance)
        {
            ValidarPermisoGestion();
            Desanotar(dni, codHorario, fecha, alcance);
        }

        /// <summary>
        /// Deja el turno con exactamente esos alumnos inscriptos en forma fija, a partir de la próxima clase.
        /// A los que se sacan se les termina la inscripción fija y se les borran las clases puntuales futuras.
        /// </summary>
        public void GuardarFijasDeHorario(int codHorario, List<int> dniAlumnos)
        {
            ValidarPermisoGestion();

            ActividadHorario horario = mppActividad.ObtenerHorario(codHorario);
            if (horario == null)
                throw new InscripcionException("El turno no existe.");

            DateTime proxima = ProximaFecha(horario.DiaSemana, DateTime.Today);
            HashSet<int> actuales = new HashSet<int>(ListarAlumnosConFija(codHorario));
            HashSet<int> nuevos = new HashSet<int>(dniAlumnos ?? new List<int>());

            List<int> agregar = nuevos.Except(actuales).ToList();
            List<int> quitar = actuales.Except(nuevos).ToList();
            List<ActividadHorario> activos = mppActividad.ListarHorariosActivos();
            var agendas = new Dictionary<int, Agenda>();
            foreach (int dni in agregar)
            {
                ValidarAnotacion(dni, codHorario, proxima);
                agendas[dni] = ObtenerAgenda(dni);
                ValidarSinSuperposicion(horario, proxima, AlcanceInscripcion.Todas, agendas[dni], activos, NombreAlumno(dni));
            }

            // Cupo: en cada clase desde la próxima, los que quedan más los que se suman
            if (horario.Cupo > 0 && agregar.Count > 0)
            {
                foreach (DateTime dia in FechasCriticas(horario, proxima))
                {
                    int total = ContarAnotados(codHorario, dia, quitar)
                        + agregar.Count(dni => agendas[dni].Estado(codHorario, dia) == EstadoClase.NoAnotado);
                    if (total > horario.Cupo)
                        throw new CupoCompletoException(
                            $"{horario.DescripcionActividad} ({DescribirHorario(horario)}) tendría {total} alumnos el {dia:dd/MM/yyyy} " +
                            $"y el cupo es {horario.Cupo}.");
                }
            }

            // Primero las bajas, así las altas encuentran el lugar libre
            foreach (int dni in quitar)
                Desanotar(dni, codHorario, proxima, AlcanceBaja.DesdeEstaClase);

            foreach (int dni in agregar)
                Anotar(dni, codHorario, proxima, AlcanceInscripcion.Todas);
        }

        private static void ValidarPermisoGestion()
        {
            if (!new BLLRol().UsuarioActualTieneAcceso(PermisosSistema.GestionActividades))
                throw new InscripcionException("No tenés permiso para gestionar inscripciones.");
        }

        #endregion

        #region Lógica común

        private ActividadHorario ValidarClase(int codHorario, DateTime fecha)
        {
            ActividadHorario horario = mppActividad.ObtenerHorario(codHorario);
            if (horario == null)
                throw new InscripcionException("El turno no existe.");

            if (ActividadHorario.DiaSemanaDesde(fecha.DayOfWeek) != horario.DiaSemana)
                throw new InscripcionException("La fecha no corresponde al día del turno.");

            if (fecha.Date < DateTime.Today)
                throw new InscripcionException("No se puede modificar una clase que ya pasó.");

            return horario;
        }

        /// <summary>
        /// Validaciones para anotar: clase válida y futura, actividad activa y alumno activo.
        /// </summary>
        private ActividadHorario ValidarAnotacion(int dni, int codHorario, DateTime fecha)
        {
            ActividadHorario horario = ValidarClase(codHorario, fecha);

            Actividad actividad = mppActividad.ObtenerActividad(horario.CodActividad);
            if (actividad == null || !actividad.Activo)
                throw new InscripcionException("La actividad no está activa.");

            Alumno alumno = new BLLAlumno().ObtenerAlumno(dni);
            if (alumno == null || !alumno.Activo)
                throw new InscripcionException("El alumno no está activo.");

            return horario;
        }

        private void Anotar(int dni, int codHorario, DateTime fecha, AlcanceInscripcion alcance)
        {
            ActividadHorario horario = ValidarAnotacion(dni, codHorario, fecha);
            Agenda agenda = ObtenerAgenda(dni);
            ValidarSinSuperposicion(horario, fecha, alcance, agenda, mppActividad.ListarHorariosActivos());
            ValidarCupo(horario, fecha, alcance, agenda);

            try
            {
                if (alcance == AlcanceInscripcion.Todas)
                {
                    // Si ya tiene una fija abierta que cubre la fecha, solo se limpian las excepciones;
                    // si no, se cierra lo que hubiera desde esa fecha y se crea una fija nueva.
                    if (!agenda.TieneFijaAbierta(codHorario, fecha))
                    {
                        mppInscripcion.CerrarFijasDesde(dni, codHorario, fecha);
                        mppInscripcion.CrearFija(dni, codHorario, fecha);
                    }
                    mppInscripcion.BorrarClasesDesde(dni, codHorario, fecha);
                }
                else
                {
                    foreach (DateTime dia in FechasDelAlcance(fecha, alcance))
                    {
                        InscripcionClase excepcion = agenda.Excepcion(codHorario, dia);

                        if (agenda.TieneFija(codHorario, dia))
                        {
                            if (excepcion != null)
                                mppInscripcion.BorrarClase(dni, codHorario, dia);
                        }
                        else if (excepcion == null || excepcion.Tipo != InscripcionClase.TipoAlta)
                        {
                            mppInscripcion.GuardarClase(dni, codHorario, dia, InscripcionClase.TipoAlta);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Error al anotar al alumno: " + ex.Message, ex);
            }

            RegistrarInscripcion(dni, $"{horario.DescripcionActividad} ({DescribirTurno(horario)}) - {DescribirAlcance(alcance)} desde {fecha:dd/MM/yyyy}");
        }

        private void Desanotar(int dni, int codHorario, DateTime fecha, AlcanceBaja alcance)
        {
            ActividadHorario horario = ValidarClase(codHorario, fecha);

            try
            {
                if (alcance == AlcanceBaja.DesdeEstaClase)
                {
                    mppInscripcion.CerrarFijasDesde(dni, codHorario, fecha);
                    mppInscripcion.BorrarClasesDesde(dni, codHorario, fecha);
                }
                else
                {
                    Agenda agenda = ObtenerAgenda(dni);
                    if (agenda.TieneFija(codHorario, fecha))
                        mppInscripcion.GuardarClase(dni, codHorario, fecha, InscripcionClase.TipoBaja);
                    else if (agenda.Excepcion(codHorario, fecha) != null)
                        mppInscripcion.BorrarClase(dni, codHorario, fecha);
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Error al dar de baja al alumno: " + ex.Message, ex);
            }

            string detalle = alcance == AlcanceBaja.DesdeEstaClase ? "desde" : "solo la clase del";
            RegistrarEvento($"Baja de inscripción de DNI {dni} en {horario.DescripcionActividad} ({DescribirTurno(horario)}): {detalle} {fecha:dd/MM/yyyy}");
        }

        #endregion

        #region Superposición de horarios

        /// <summary>
        /// True si los dos turnos caen el mismo día de la semana y sus horarios se pisan
        /// (18:00–19:00 se pisa con 18:30–19:30, pero no con 19:00–20:00).
        /// </summary>
        public static bool SeSuperponen(ActividadHorario a, ActividadHorario b)
        {
            return a.DiaSemana == b.DiaSemana && a.HoraInicio < b.HoraFin && b.HoraInicio < a.HoraFin;
        }

        /// <summary>
        /// Verifica que, al anotarse en el turno con ese alcance, el alumno no quede en dos clases
        /// que se superponen. Para "Todas" alcanza con que vaya a la otra clase alguna vez desde esa fecha.
        /// </summary>
        private static void ValidarSinSuperposicion(ActividadHorario horario, DateTime fecha, AlcanceInscripcion alcance,
            Agenda agenda, List<ActividadHorario> activos, string nombreAlumno = null)
        {
            string quien = string.IsNullOrEmpty(nombreAlumno) ? "El alumno" : nombreAlumno;

            foreach (ActividadHorario otro in activos.Where(o => o.CodHorario != horario.CodHorario && SeSuperponen(o, horario)))
            {
                if (alcance == AlcanceInscripcion.Todas)
                {
                    if (VaAlTurnoDesde(agenda, otro, fecha))
                        throw new ConflictoHorarioException(
                            $"{quien} ya está anotado en {otro.DescripcionActividad} ({DescribirHorario(otro)}), que se superpone con este horario.");
                    continue;
                }

                foreach (DateTime dia in FechasDelAlcance(fecha, alcance))
                {
                    if (agenda.Estado(otro.CodHorario, dia) != EstadoClase.NoAnotado)
                        throw new ConflictoHorarioException(
                            $"{quien} ya está anotado en {otro.DescripcionActividad} ({DescribirHorario(otro)}) el {dia:dd/MM/yyyy}, que se superpone con esta clase.");
                }
            }
        }

        /// <summary>
        /// True si el alumno va (o puede ir) al turno en alguna fecha desde la indicada:
        /// inscripción fija vigente o alta puntual futura en el día de ese turno.
        /// </summary>
        private static bool VaAlTurnoDesde(Agenda agenda, ActividadHorario turno, DateTime desde)
        {
            return agenda.Fijas.Any(f => f.CodHorario == turno.CodHorario && (!f.FechaHasta.HasValue || f.FechaHasta.Value >= desde.Date))
                || agenda.Clases.Any(c => c.CodHorario == turno.CodHorario && c.Tipo == InscripcionClase.TipoAlta && c.Fecha >= desde.Date
                                          && ActividadHorario.DiaSemanaDesde(c.Fecha.DayOfWeek) == turno.DiaSemana);
        }

        /// <summary>
        /// True si hay alguna fecha (desde la indicada) en la que el alumno vaya a los dos turnos.
        /// Se usa con el turno ya modificado: las altas puntuales que no caen en su nuevo día no cuentan
        /// (al guardar el cambio de día esas altas se borran).
        /// </summary>
        private static bool CoincidenDesde(Agenda agenda, ActividadHorario turno, ActividadHorario otro, DateTime desde)
        {
            DateTime fin = DateTime.MaxValue.Date;
            var fijasTurno = agenda.Fijas.Where(f => f.CodHorario == turno.CodHorario).ToList();
            var fijasOtro = agenda.Fijas.Where(f => f.CodHorario == otro.CodHorario).ToList();

            // Dos inscripciones fijas cuyos períodos se cruzan
            if (fijasTurno.Any(a => fijasOtro.Any(b =>
                    new[] { a.FechaDesde, b.FechaDesde, desde.Date }.Max() <= new[] { a.FechaHasta ?? fin, b.FechaHasta ?? fin }.Min())))
                return true;

            // Un alta puntual en uno de los turnos, en una fecha en la que también va al otro
            foreach (InscripcionClase alta in agenda.Clases.Where(c => c.Tipo == InscripcionClase.TipoAlta && c.Fecha >= desde.Date))
            {
                ActividadHorario turnoDeAlta = alta.CodHorario == turno.CodHorario ? turno : alta.CodHorario == otro.CodHorario ? otro : null;
                if (turnoDeAlta == null || ActividadHorario.DiaSemanaDesde(alta.Fecha.DayOfWeek) != turnoDeAlta.DiaSemana)
                    continue;

                ActividadHorario contrario = turnoDeAlta == turno ? otro : turno;
                if (agenda.Estado(contrario.CodHorario, alta.Fecha) != EstadoClase.NoAnotado)
                    return true;
            }

            return false;
        }

        /// <summary>
        /// Para un cambio de día/horario de un turno o la reactivación de su actividad: verifica que
        /// ningún alumno que va al turno desde hoy quede superpuesto con otra clase a la que también va.
        /// </summary>
        /// <param name="turno">El turno con los valores nuevos (día, hora y duración) y su código actual.</param>
        /// <param name="otros">Turnos de las demás actividades activas.</param>
        public void ValidarAlumnosSinSuperposicion(ActividadHorario turno, IEnumerable<ActividadHorario> otros)
        {
            if (turno.CodHorario == 0)
                return;

            List<ActividadHorario> candidatos = otros.Where(o => o.CodHorario != turno.CodHorario && SeSuperponen(o, turno)).ToList();
            if (candidatos.Count == 0)
                return;

            DateTime hoy = DateTime.Today;
            foreach (int dni in mppInscripcion.ListarAlumnosConInscripcionDesde(turno.CodHorario, hoy))
            {
                Agenda agenda = ObtenerAgenda(dni);
                ActividadHorario conflicto = candidatos.FirstOrDefault(o => CoincidenDesde(agenda, turno, o, hoy));
                if (conflicto != null)
                    throw new ConflictoHorarioException(
                        $"{NombreAlumno(dni)} va a {turno.DescripcionActividad} ({DescribirHorario(turno)}) y a " +
                        $"{conflicto.DescripcionActividad} ({DescribirHorario(conflicto)}): los horarios se superpondrían.");
            }
        }

        private static string NombreAlumno(int dni)
        {
            Alumno alumno = new BLLAlumno().ObtenerAlumno(dni);
            return alumno != null ? $"{alumno.Apellido}, {alumno.Nombre}" : $"El alumno DNI {dni}";
        }

        /// <summary>"Lun 18:00–19:00".</summary>
        public static string DescribirHorario(ActividadHorario horario)
        {
            return $"{DescribirTurno(horario)}–{horario.HoraFin:hh\\:mm}";
        }

        #endregion

        #region Cupo

        /// <summary>
        /// Cantidad de alumnos que van a la clase del turno en la fecha (sin contar a los excluidos).
        /// </summary>
        public int ContarAnotados(int codHorario, DateTime fecha, ICollection<int> excluir = null)
        {
            return ListarAsistentes(codHorario, fecha).Keys.Count(dni => excluir == null || !excluir.Contains(dni));
        }

        /// <summary>
        /// Fechas a revisar desde una fecha en adelante: la primera clase y cada clase en la que puede
        /// subir la cantidad de anotados (empieza una inscripción fija o hay un alta puntual).
        /// Entre esas fechas la cantidad solo puede bajar, así que alcanza con revisar estas.
        /// </summary>
        private List<DateTime> FechasCriticas(ActividadHorario horario, DateTime desde)
        {
            DateTime primera = ProximaFecha(horario.DiaSemana, desde);
            return new[] { primera }
                .Concat(mppInscripcion.ListarFechasDeAltaDesde(horario.CodHorario, primera).Select(f => ProximaFecha(horario.DiaSemana, f)))
                .Distinct()
                .OrderBy(f => f)
                .ToList();
        }

        /// <summary>
        /// Verifica que haya lugar para el alumno en cada clase a la que se anotaría.
        /// Las clases a las que ya va no cuentan (ya ocupa su lugar).
        /// </summary>
        private void ValidarCupo(ActividadHorario horario, DateTime fecha, AlcanceInscripcion alcance, Agenda agenda)
        {
            if (horario.Cupo <= 0)
                return;

            IEnumerable<DateTime> fechas = alcance == AlcanceInscripcion.Todas
                ? FechasCriticas(horario, fecha)
                : FechasDelAlcance(fecha, alcance);

            foreach (DateTime dia in fechas)
            {
                if (agenda.Estado(horario.CodHorario, dia) != EstadoClase.NoAnotado)
                    continue;

                int anotados = ContarAnotados(horario.CodHorario, dia);
                if (anotados >= horario.Cupo)
                    throw new CupoCompletoException(
                        $"La clase de {horario.DescripcionActividad} del {dia:dd/MM/yyyy} ({DescribirHorario(horario)}) " +
                        $"tiene el cupo completo ({anotados}/{horario.Cupo}).");
            }
        }

        /// <summary>
        /// La clase con más anotados del turno desde la fecha indicada (fecha y cantidad).
        /// </summary>
        public KeyValuePair<DateTime, int> MaximoAnotadosDesde(ActividadHorario horario, DateTime desde)
        {
            return FechasCriticas(horario, desde)
                .Select(dia => new KeyValuePair<DateTime, int>(dia, ContarAnotados(horario.CodHorario, dia)))
                .OrderByDescending(par => par.Value)
                .First();
        }

        /// <summary>
        /// Datos para calcular cuántos van a cada clase de un turno sin exponer quiénes son:
        /// períodos de las inscripciones fijas y cantidad de altas y bajas puntuales por fecha.
        /// </summary>
        public class OcupacionTurno
        {
            public List<KeyValuePair<DateTime, DateTime?>> Fijas { get; } = new List<KeyValuePair<DateTime, DateTime?>>();
            public Dictionary<DateTime, int> Altas { get; } = new Dictionary<DateTime, int>();
            public Dictionary<DateTime, int> Bajas { get; } = new Dictionary<DateTime, int>();
        }

        /// <summary>
        /// Ocupación de todos los turnos desde la fecha indicada (código de turno → datos).
        /// </summary>
        public Dictionary<int, OcupacionTurno> ObtenerOcupacionDesde(DateTime desde)
        {
            try
            {
                var resultado = new Dictionary<int, OcupacionTurno>();
                Func<int, OcupacionTurno> turno = cod => resultado.TryGetValue(cod, out OcupacionTurno o) ? o : (resultado[cod] = new OcupacionTurno());

                foreach (InscripcionFija fija in mppInscripcion.ListarFijasVigentesDesde(desde))
                    turno(fija.CodHorario).Fijas.Add(new KeyValuePair<DateTime, DateTime?>(fija.FechaDesde, fija.FechaHasta));

                foreach (InscripcionClase clase in mppInscripcion.ListarClasesDesde(desde))
                {
                    Dictionary<DateTime, int> destino = clase.Tipo == InscripcionClase.TipoAlta ? turno(clase.CodHorario).Altas : turno(clase.CodHorario).Bajas;
                    destino[clase.Fecha] = destino.TryGetValue(clase.Fecha, out int n) ? n + 1 : 1;
                }

                return resultado;
            }
            catch (Exception ex)
            {
                throw new Exception("Error al obtener la ocupación de las clases: " + ex.Message, ex);
            }
        }

        #endregion

        #region Textos

        private static string DescribirTurno(ActividadHorario horario)
        {
            string[] dias = { "Lun", "Mar", "Mié", "Jue", "Vie", "Sáb", "Dom" };
            return $"{dias[horario.DiaSemana - 1]} {horario.HoraInicio:hh\\:mm}";
        }

        private static string DescribirAlcance(AlcanceInscripcion alcance)
        {
            switch (alcance)
            {
                case AlcanceInscripcion.RestoDelMes: return "resto del mes";
                case AlcanceInscripcion.ProximasCuatroSemanas: return "próximas 4 semanas";
                case AlcanceInscripcion.Todas: return "todas las semanas";
                default: return "solo una clase";
            }
        }

        private void RegistrarInscripcion(int dni, string detalle)
        {
            try
            {
                string usuario = Singleton.Instancia?.Usuario?.USUARIO_Usuario;
                if (!string.IsNullOrEmpty(usuario))
                    bllEvento.RegistrarInscripcion(usuario, dni, detalle);
            }
            catch
            {
                // No impedir la operación principal si falla el log
            }
        }

        private void RegistrarEvento(string accion)
        {
            try
            {
                string usuario = Singleton.Instancia?.Usuario?.USUARIO_Usuario;
                if (!string.IsNullOrEmpty(usuario))
                    bllEvento.RegistrarEvento(BLLEvento.EVENTO_CONFIGURACION, usuario, accion, 3, "Actividades");
            }
            catch
            {
                // No impedir la operación principal si falla el log
            }
        }

        #endregion
    }

    /// <summary>
    /// Error de validación de una inscripción, con un mensaje apto para mostrar al usuario.
    /// </summary>
    public class InscripcionException : Exception
    {
        public InscripcionException(string mensaje) : base(mensaje) { }
    }

    /// <summary>
    /// Un profesor o un alumno quedaría en dos clases que se superponen.
    /// </summary>
    public class ConflictoHorarioException : InscripcionException
    {
        public ConflictoHorarioException(string mensaje) : base(mensaje) { }
    }

    /// <summary>
    /// La clase ya tiene tantos alumnos como su cupo.
    /// </summary>
    public class CupoCompletoException : InscripcionException
    {
        public CupoCompletoException(string mensaje) : base(mensaje) { }
    }
}
