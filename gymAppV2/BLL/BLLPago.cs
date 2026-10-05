using System;
using System.Collections.Generic;
using BE;
using MPP;

namespace BLL
{
    /// <summary>
    /// Lógica de negocio para el registro y consulta de pagos de cuota mensual.
    /// </summary>
    public class BLLPago
    {
        private MPPPago mppPago;
        private BLLAlumno bllAlumno;
        private BLLPrecioModalidad bllPrecioModalidad;
        private BLLEvento bllEvento;

        public static readonly string[] MEDIOS_PAGO_VALIDOS = { "Efectivo", "Transferencia", "Tarjeta" };

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

        /// <summary>
        /// Indica si ya existe un pago registrado para el alumno en el período (mes) indicado.
        /// Útil para validar en la UI antes de intentar registrar el pago.
        /// </summary>
        public bool ExistePagoEnPeriodo(int dni, DateTime periodo)
        {
            try
            {
                DateTime periodoNormalizado = new DateTime(periodo.Year, periodo.Month, 1);
                return mppPago.ExistePagoEnPeriodo(dni, periodoNormalizado);
            }
            catch (Exception ex)
            {
                throw new Exception("Error al verificar pago existente: " + ex.Message, ex);
            }
        }

        /// <summary>
        /// Registra el pago de la cuota de un alumno para un período (mes) determinado.
        /// El monto se toma del precio vigente de la modalidad indicada.
        /// </summary>
        public void RegistrarPago(int dni, int modalidadId, DateTime periodo, string metodoPago, string usuarioRegistro)
        {
            try
            {
                if (dni <= 0 || !bllAlumno.AlumnoExiste(dni))
                {
                    throw new Exception("Debe seleccionar un alumno válido");
                }

                if (Array.IndexOf(MEDIOS_PAGO_VALIDOS, metodoPago) < 0)
                {
                    throw new Exception("El método de pago debe ser Efectivo, Transferencia o Tarjeta");
                }

                var modalidad = bllPrecioModalidad.ObtenerModalidad(modalidadId);
                if (modalidad == null || !modalidad.Activo)
                {
                    throw new Exception("Debe seleccionar una modalidad de cuota válida");
                }

                DateTime periodoNormalizado = new DateTime(periodo.Year, periodo.Month, 1);

                if (mppPago.ExistePagoEnPeriodo(dni, periodoNormalizado))
                {
                    throw new Exception("Ya existe un pago registrado para este alumno en este período");
                }

                var pago = new Pago
                {
                    Dni = dni,
                    ModalidadId = modalidadId,
                    Periodo = periodoNormalizado,
                    Monto = modalidad.Precio,
                    FechaPago = DateTime.Now,
                    MetodoPago = metodoPago,
                    UsuarioRegistro = usuarioRegistro
                };

                mppPago.CrearPago(pago);

                bllEvento.RegistrarPago(usuarioRegistro, dni, pago.Monto, metodoPago);
            }
            catch (Exception ex)
            {
                throw new Exception("Error al registrar pago: " + ex.Message, ex);
            }
        }
    }
}
