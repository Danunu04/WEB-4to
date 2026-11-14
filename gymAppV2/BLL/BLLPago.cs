using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using BE;
using MPP;

namespace BLL
{
    /// <summary>
    /// Lógica de negocio del abono mensual: registro de pagos y estado del abono de cada alumno.
    ///
    /// Cada pago es un abono de DIAS_ABONO días. Si el alumno todavía tiene un abono vigente,
    /// el nuevo empieza el día siguiente a su vencimiento (se encadenan, no se pierden días);
    /// si está vencido o nunca pagó, empieza el día del pago. Se permite un solo abono pagado
    /// por adelantado. El abono vence por fecha: no se cuentan las clases usadas.
    /// </summary>
    public class BLLPago
    {
        private MPPPago mppPago;
        private BLLAlumno bllAlumno;
        private BLLPrecioModalidad bllPrecioModalidad;
        private BLLEvento bllEvento;

        public const int DIAS_ABONO = 30;

        /// <summary>Con esta cantidad de días restantes o menos, el abono figura "Por vencer".</summary>
        public const int DIAS_AVISO_VENCIMIENTO = 5;

        public static readonly string[] MEDIOS_PAGO_VALIDOS = { "Efectivo", "Transferencia", "Tarjeta" };

        /// <summary>Medios con los que el cliente puede pagar desde la app (el efectivo es solo en recepción).</summary>
        public static readonly string[] MEDIOS_PAGO_ONLINE = { "Tarjeta", "Transferencia" };

        public BLLPago()
        {
            mppPago = new MPPPago();
            bllAlumno = new BLLAlumno();
            bllPrecioModalidad = new BLLPrecioModalidad();
            bllEvento = new BLLEvento();
        }

        public List<Pago> ListarPagos()
        {
            try
            {
                return mppPago.ListarPagos();
            }
            catch (Exception ex)
            {
                throw new Exception("Error al listar pagos: " + ex.Message, ex);
            }
        }

        /// <summary>
        /// Lista el historial de pagos de los alumnos asociados al usuario Cliente logueado.
        /// </summary>
        public List<Pago> ListarPagosPorCliente(string usuario)
        {
            try
            {
                if (string.IsNullOrEmpty(usuario))
                {
                    return new List<Pago>();
                }

                return mppPago.ListarPagosPorCliente(usuario);
            }
            catch (Exception ex)
            {
                throw new Exception("Error al listar pagos del cliente: " + ex.Message, ex);
            }
        }

        // ==================== ESTADO DEL ABONO ====================

        /// <summary>
        /// Estado del abono de un alumno. Null si el alumno no existe.
        /// </summary>
        public EstadoAbono ObtenerEstadoAbono(int dni, DateTime hoy)
        {
            try
            {
                Alumno alumno = bllAlumno.ObtenerAlumno(dni);
                if (alumno == null)
                {
                    return null;
                }

                Pago ultimo = mppPago.ListarUltimosAbonos(dni).FirstOrDefault();
                return ArmarEstado(alumno, ultimo, hoy, ObtenerModalidades());
            }
            catch (Exception ex)
            {
                throw new Exception("Error al obtener el estado del abono: " + ex.Message, ex);
            }
        }

        /// <summary>
        /// Estado del abono de todos los alumnos (vista de recepción). El filtro busca en
        /// nombre, apellido y DNI, sin distinguir mayúsculas ni tildes.
        /// </summary>
        public List<EstadoAbono> ListarEstadosAbono(DateTime hoy, string filtro = null)
        {
            try
            {
                IEnumerable<Alumno> alumnos = bllAlumno.ListarAlumnos() ?? new List<Alumno>();

                if (!string.IsNullOrWhiteSpace(filtro))
                {
                    string texto = filtro.Trim();
                    alumnos = alumnos.Where(a => Contiene(a.Nombre, texto)
                                              || Contiene(a.Apellido, texto)
                                              || a.DNI.ToString().Contains(texto));
                }

                return ArmarEstados(alumnos, hoy);
            }
            catch (Exception ex)
            {
                throw new Exception("Error al listar el estado de los abonos: " + ex.Message, ex);
            }
        }

        /// <summary>
        /// Estado del abono de los alumnos asociados a la cuenta de un cliente (titular o familiar).
        /// </summary>
        public List<EstadoAbono> ListarEstadosAbonoCliente(string usuario, DateTime hoy)
        {
            try
            {
                if (string.IsNullOrEmpty(usuario))
                {
                    return new List<EstadoAbono>();
                }

                return ArmarEstados(bllAlumno.ObtenerAlumnosDeUsuario(usuario) ?? new List<Alumno>(), hoy);
            }
            catch (Exception ex)
            {
                throw new Exception("Error al listar los abonos del cliente: " + ex.Message, ex);
            }
        }

        private List<EstadoAbono> ArmarEstados(IEnumerable<Alumno> alumnos, DateTime hoy)
        {
            Dictionary<int, Pago> ultimos = mppPago.ListarUltimosAbonos().ToDictionary(p => p.Dni);
            Dictionary<int, PrecioModalidad> modalidades = ObtenerModalidades();

            return alumnos
                .Select(a => ArmarEstado(a, ultimos.TryGetValue(a.DNI, out Pago p) ? p : null, hoy, modalidades))
                .OrderBy(e => e.Apellido)
                .ThenBy(e => e.Nombre)
                .ToList();
        }

        private EstadoAbono ArmarEstado(Alumno alumno, Pago ultimo, DateTime hoy, Dictionary<int, PrecioModalidad> modalidades)
        {
            DateTime dia = hoy.Date;

            var estado = new EstadoAbono
            {
                Dni = alumno.DNI,
                Nombre = alumno.Nombre,
                Apellido = alumno.Apellido,
                AlumnoActivo = alumno.Activo,
                Situacion = SituacionAbono.SinAbono
            };

            if (ultimo != null)
            {
                DateTime vencimiento = ultimo.FechaVencimiento.Date;
                estado.FechaVencimiento = vencimiento;
                estado.ModalidadId = ultimo.ModalidadId;
                estado.TieneAbonoAdelantado = ultimo.FechaInicio.Date > dia;

                if (vencimiento < dia)
                {
                    estado.Situacion = SituacionAbono.Vencido;
                }
                else
                {
                    estado.DiasRestantes = (vencimiento - dia).Days + 1;
                    estado.Situacion = estado.DiasRestantes <= DIAS_AVISO_VENCIMIENTO
                        ? SituacionAbono.PorVencer
                        : SituacionAbono.Vigente;
                }

                if (modalidades.TryGetValue(ultimo.ModalidadId, out PrecioModalidad modalidad) && modalidad.Activo)
                {
                    estado.MontoARenovar = modalidad.Precio;
                }
            }

            return estado;
        }

        private Dictionary<int, PrecioModalidad> ObtenerModalidades()
        {
            return (bllPrecioModalidad.ListarModalidades() ?? new List<PrecioModalidad>()).ToDictionary(m => m.Id);
        }

        private static bool Contiene(string valor, string texto)
        {
            return !string.IsNullOrEmpty(valor)
                && CultureInfo.InvariantCulture.CompareInfo.IndexOf(valor, texto,
                       CompareOptions.IgnoreCase | CompareOptions.IgnoreNonSpace) >= 0;
        }

        // ==================== REGISTRO DE PAGOS ====================

        /// <summary>
        /// Arma (sin guardar) el abono que se registraría hoy: fechas de cobertura y monto.
        /// Lanza ReglaNegocioException si no se puede pagar (ej. ya tiene un abono adelantado).
        /// </summary>
        public Pago PrevisualizarAbono(int dni, int modalidadId, DateTime hoy)
        {
            Alumno alumno = bllAlumno.ObtenerAlumno(dni);
            if (alumno == null)
            {
                throw new ReglaNegocioException("pagos_err_alumno", "Debe seleccionar un alumno válido.");
            }

            if (!alumno.Activo)
            {
                throw new ReglaNegocioException("pagos_err_alumno_inactivo",
                    "El alumno está dado de baja: no se le puede cobrar el abono.");
            }

            PrecioModalidad modalidad = bllPrecioModalidad.ObtenerModalidad(modalidadId);
            if (modalidad == null || !modalidad.Activo)
            {
                throw new ReglaNegocioException("pagos_err_modalidad", "Debe seleccionar una modalidad válida.");
            }

            DateTime inicio = CalcularInicioProximoAbono(dni, hoy);

            return new Pago
            {
                Dni = dni,
                ModalidadId = modalidadId,
                FechaInicio = inicio,
                FechaVencimiento = inicio.AddDays(DIAS_ABONO - 1),
                Periodo = new DateTime(inicio.Year, inicio.Month, 1),
                Monto = modalidad.Precio,
                AlumnoNombre = alumno.Nombre,
                AlumnoApellido = alumno.Apellido,
                ModalidadDescripcion = modalidad.ObtenerDescripcion()
            };
        }

        /// <summary>
        /// Primer día del próximo abono: el día siguiente al vencimiento si el actual sigue vigente,
        /// o hoy si está vencido o nunca pagó.
        /// </summary>
        private DateTime CalcularInicioProximoAbono(int dni, DateTime hoy)
        {
            DateTime dia = hoy.Date;
            Pago ultimo = mppPago.ListarUltimosAbonos(dni).FirstOrDefault();

            if (ultimo == null || ultimo.FechaVencimiento.Date < dia)
            {
                return dia;
            }

            if (ultimo.FechaInicio.Date > dia)
            {
                string desde = ultimo.FechaInicio.ToString("dd/MM/yyyy");
                throw new ReglaNegocioException("pagos_err_adelantado",
                    $"El alumno ya tiene un abono pagado por adelantado (desde el {desde}).", desde);
            }

            return ultimo.FechaVencimiento.Date.AddDays(1);
        }

        /// <summary>
        /// Registra el pago de un abono desde recepción (Administrador / Recepcionista).
        /// Devuelve el pago guardado con sus fechas de cobertura.
        /// </summary>
        public Pago RegistrarPago(int dni, int modalidadId, string metodoPago, string usuarioRegistro)
        {
            return Registrar(dni, modalidadId, metodoPago, usuarioRegistro, MEDIOS_PAGO_VALIDOS);
        }

        /// <summary>
        /// Registra el pago que hace un cliente desde la app. Solo puede pagar el abono de
        /// alumnos asociados a su cuenta, y con un medio de pago online.
        /// </summary>
        public Pago RegistrarPagoCliente(string usuarioCliente, int dni, int modalidadId, string metodoPago)
        {
            bool esSuyo = !string.IsNullOrEmpty(usuarioCliente)
                && (bllAlumno.ObtenerAlumnosDeUsuario(usuarioCliente) ?? new List<Alumno>()).Any(a => a.DNI == dni);

            if (!esSuyo)
            {
                throw new ReglaNegocioException("pagos_err_no_asociado", "Ese alumno no está asociado a tu cuenta.");
            }

            return Registrar(dni, modalidadId, metodoPago, usuarioCliente, MEDIOS_PAGO_ONLINE);
        }

        private Pago Registrar(int dni, int modalidadId, string metodoPago, string usuarioRegistro, string[] mediosPermitidos)
        {
            try
            {
                if (Array.IndexOf(mediosPermitidos, metodoPago) < 0)
                {
                    throw new ReglaNegocioException("pagos_err_metodo", "Debe seleccionar un medio de pago válido.");
                }

                Pago pago = PrevisualizarAbono(dni, modalidadId, DateTime.Now);
                pago.FechaPago = DateTime.Now;
                pago.MetodoPago = metodoPago;
                pago.UsuarioRegistro = usuarioRegistro;

                pago.CodPago = mppPago.CrearPago(pago);

                bllEvento.RegistrarPago(usuarioRegistro, dni, pago.Monto, metodoPago);
                return pago;
            }
            catch (ReglaNegocioException)
            {
                throw;
            }
            catch (Exception ex)
            {
                throw new Exception("Error al registrar pago: " + ex.Message, ex);
            }
        }
    }
}
