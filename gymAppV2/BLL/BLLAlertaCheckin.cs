using System;
using System.Collections.Generic;
using BE;
using MPP;

namespace BLL
{
    /// <summary>
    /// Alerta automática al recepcionista cuando un alumno intenta hacer check-in con el abono
    /// vencido o sin abono pago (G05 - Notificaciones y CUN007, paso 5.3). Las alertas se ven en
    /// el Dashboard de administración hasta que alguien las marca como vistas.
    /// </summary>
    public class BLLAlertaCheckin
    {
        private MPPAlertaCheckin mppAlertaCheckin;

        public BLLAlertaCheckin()
        {
            mppAlertaCheckin = new MPPAlertaCheckin();
        }

        /// <summary>
        /// Registra la alerta de un check-in rechazado por el abono.
        /// </summary>
        public void Registrar(int dni, DateTime? fechaVencimiento)
        {
            mppAlertaCheckin.Crear(new AlertaCheckin
            {
                Dni = dni,
                Situacion = fechaVencimiento.HasValue ? AlertaCheckin.SITUACION_VENCIDO : AlertaCheckin.SITUACION_SIN_ABONO,
                FechaVencimiento = fechaVencimiento,
                Fecha = DateTime.Now
            });
        }

        public List<AlertaCheckin> ListarPendientes()
        {
            try
            {
                return mppAlertaCheckin.ListarPendientes();
            }
            catch (Exception ex)
            {
                throw new Exception("Error al listar las alertas de check-in: " + ex.Message, ex);
            }
        }

        public void MarcarVista(int codAlerta, string usuario)
        {
            mppAlertaCheckin.MarcarVista(codAlerta, usuario);
        }

        public void MarcarTodasVistas(string usuario)
        {
            foreach (AlertaCheckin alerta in mppAlertaCheckin.ListarPendientes())
            {
                mppAlertaCheckin.MarcarVista(alerta.CodAlerta, usuario);
            }
        }
    }
}
