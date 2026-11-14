using System;
using System.Collections.Generic;
using System.Linq;
using BE;
using MPP;

namespace BLL
{
    /// <summary>
    /// Cálculo de la repetición máxima (1RM): el peso más alto que un alumno puede levantar
    /// una sola vez en un ejercicio. Se estima con la fórmula de Epley a partir de una serie
    /// (peso y repeticiones) y se guarda en el historial del alumno.
    ///
    /// El personal (administración, recepción, entrenadores) trabaja con cualquier alumno
    /// activo; un Cliente o Familiar, solo con los alumnos asociados a su cuenta.
    /// </summary>
    public class BLLCalculoRM
    {
        /// <summary>Hasta cuántas repeticiones la fórmula de Epley da una estimación confiable.</summary>
        public const int REPETICIONES_MAXIMAS = 12;

        /// <summary>Tope del peso levantado en una serie, para descartar errores de carga.</summary>
        public const decimal PESO_MAXIMO = 500m;

        /// <summary>Porcentajes de la 1RM que se muestran en la tabla de cargas.</summary>
        private static readonly int[] PORCENTAJES_CARGA = { 100, 95, 90, 85, 80, 75, 70, 65, 60, 55, 50 };

        private MPPCalculoRM mppCalculoRM;
        private BLLAlumno bllAlumno;
        private BLLEvento bllEvento;

        public BLLCalculoRM()
        {
            mppCalculoRM = new MPPCalculoRM();
            bllAlumno = new BLLAlumno();
            bllEvento = new BLLEvento();
        }

        // ==================== FÓRMULA ====================

        /// <summary>
        /// Fórmula de Epley: 1RM = peso × (1 + repeticiones / 30). Con una sola repetición la
        /// 1RM es el mismo peso. Se redondea a 0,1 kg.
        /// </summary>
        public decimal CalcularRM(decimal pesoLevantado, int repeticiones)
        {
            ValidarSerie(pesoLevantado, repeticiones);

            if (repeticiones == 1)
            {
                return Math.Round(pesoLevantado, 1, MidpointRounding.AwayFromZero);
            }

            return Math.Round(pesoLevantado * (1m + repeticiones / 30m), 1, MidpointRounding.AwayFromZero);
        }

        /// <summary>
        /// Tabla de cargas para una 1RM: por cada porcentaje, el peso redondeado a 0,5 kg (lo que
        /// se puede armar con discos; el 100% es la 1RM tal cual) y las repeticiones aproximadas, despejando la misma
        /// fórmula de Epley: repeticiones = 30 × (100 / porcentaje − 1).
        /// </summary>
        public List<CargaRM> CalcularCargas(decimal rm)
        {
            return PORCENTAJES_CARGA
                .Select(porcentaje => new CargaRM
                {
                    Porcentaje = porcentaje,
                    Peso = porcentaje == 100
                        ? rm
                        : Math.Round(rm * porcentaje / 100m * 2m, MidpointRounding.AwayFromZero) / 2m,
                    RepeticionesAprox = porcentaje == 100
                        ? 1
                        : (int)Math.Round(30m * (100m / porcentaje - 1m), MidpointRounding.AwayFromZero)
                })
                .ToList();
        }

        private void ValidarSerie(decimal pesoLevantado, int repeticiones)
        {
            if (pesoLevantado <= 0 || pesoLevantado > PESO_MAXIMO)
            {
                throw new ReglaNegocioException("rm_err_peso",
                    $"El peso debe ser mayor que 0 y hasta {PESO_MAXIMO:0} kg.", PESO_MAXIMO.ToString("0"));
            }

            if (repeticiones < 1 || repeticiones > REPETICIONES_MAXIMAS)
            {
                throw new ReglaNegocioException("rm_err_reps",
                    $"Las repeticiones deben ser un número entero entre 1 y {REPETICIONES_MAXIMAS}.", REPETICIONES_MAXIMAS);
            }
        }

        // ==================== CONSULTAS ====================

        public List<Ejercicio> ListarEjercicios()
        {
            try
            {
                return mppCalculoRM.ListarEjercicios();
            }
            catch (Exception ex)
            {
                throw new Exception("Error al listar ejercicios: " + ex.Message, ex);
            }
        }

        /// <summary>
        /// Alumnos con los que puede trabajar el usuario: todos los activos para el personal,
        /// o los asociados a su cuenta para un Cliente o Familiar.
        /// </summary>
        public List<Alumno> ListarAlumnosDisponibles(Usuario usuario)
        {
            try
            {
                if (usuario == null)
                {
                    return new List<Alumno>();
                }

                List<Alumno> alumnos = EsCliente(usuario)
                    ? bllAlumno.ObtenerAlumnosDeUsuario(usuario.USUARIO_Usuario)
                    : bllAlumno.ListarAlumnos();

                return (alumnos ?? new List<Alumno>())
                    .Where(a => a.Activo)
                    .OrderBy(a => a.Apellido)
                    .ThenBy(a => a.Nombre)
                    .ToList();
            }
            catch (Exception ex)
            {
                throw new Exception("Error al listar los alumnos: " + ex.Message, ex);
            }
        }

        /// <summary>
        /// Historial de 1RM de un alumno, del más reciente al más viejo.
        /// </summary>
        public List<RegistroRM> ListarHistorial(int dni, Usuario usuario)
        {
            ValidarAccesoAlumno(dni, usuario, exigirActivo: false);

            try
            {
                return mppCalculoRM.ListarRegistrosPorAlumno(dni);
            }
            catch (Exception ex)
            {
                throw new Exception("Error al listar el historial de 1RM: " + ex.Message, ex);
            }
        }

        // ==================== REGISTRO ====================

        /// <summary>
        /// Calcula la 1RM de la serie y la guarda en el historial del alumno con la fecha de hoy.
        /// </summary>
        public RegistroRM RegistrarRM(int dni, int codEjercicio, decimal pesoLevantado, int repeticiones, Usuario usuario)
        {
            ValidarAccesoAlumno(dni, usuario, exigirActivo: true);

            Ejercicio ejercicio = mppCalculoRM.ObtenerEjercicio(codEjercicio);
            if (ejercicio == null)
            {
                throw new ReglaNegocioException("rm_err_ejercicio", "Debe seleccionar un ejercicio válido.");
            }

            decimal rm = CalcularRM(pesoLevantado, repeticiones);

            try
            {
                var registro = new RegistroRM
                {
                    Dni = dni,
                    CodEjercicio = codEjercicio,
                    PesoRM = rm,
                    FechaRM = DateTime.Today,
                    PesoLevantado = pesoLevantado,
                    Repeticiones = repeticiones,
                    UsuarioRegistro = usuario.USUARIO_Usuario,
                    EjercicioNombre = ejercicio.Nombre,
                    GrupoMuscular = ejercicio.GrupoMuscular
                };

                registro.CodRM = mppCalculoRM.CrearRegistro(registro);

                bllEvento.RegistrarRMAlta(usuario.USUARIO_Usuario, dni, ejercicio.Nombre, rm);

                return registro;
            }
            catch (Exception ex)
            {
                throw new Exception("Error al guardar el 1RM: " + ex.Message, ex);
            }
        }

        // ==================== ACCESO ====================

        private static bool EsCliente(Usuario usuario)
        {
            return usuario.USUARIO_Rol == PerfilesSistema.RolCliente
                || usuario.USUARIO_Rol == PerfilesSistema.RolFamiliar;
        }

        /// <summary>
        /// El alumno tiene que existir (y estar activo, para registrar), y un Cliente o Familiar
        /// solo puede ver o cargar la 1RM de los alumnos asociados a su cuenta.
        /// </summary>
        private void ValidarAccesoAlumno(int dni, Usuario usuario, bool exigirActivo)
        {
            if (usuario == null)
            {
                throw new ReglaNegocioException("rm_err_alumno", "Debe seleccionar un alumno válido.");
            }

            Alumno alumno = bllAlumno.ObtenerAlumno(dni);
            if (alumno == null)
            {
                throw new ReglaNegocioException("rm_err_alumno", "Debe seleccionar un alumno válido.");
            }

            if (exigirActivo && !alumno.Activo)
            {
                throw new ReglaNegocioException("rm_err_alumno_inactivo", "El alumno está dado de baja.");
            }

            if (EsCliente(usuario))
            {
                bool esSuyo = (bllAlumno.ObtenerAlumnosDeUsuario(usuario.USUARIO_Usuario) ?? new List<Alumno>())
                    .Any(a => a.DNI == dni);

                if (!esSuyo)
                {
                    throw new ReglaNegocioException("rm_err_no_asociado", "Ese alumno no está asociado a tu cuenta.");
                }
            }
        }
    }
}
