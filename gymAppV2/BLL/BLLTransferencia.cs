using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using BE;
using MPP;
using Servicios.Singleton;

namespace BLL
{
    /// <summary>
    /// Pago del abono por transferencia. El cliente la informa (desde Pagos o al registrarse) y
    /// queda pendiente: el abono no se activa hasta que administración o recepción comprueba que
    /// el dinero llegó y la confirma. Al confirmarla se registra el pago con las fechas del día.
    /// </summary>
    public class BLLTransferencia
    {
        /// <summary>Alias que se muestra si todavía no se configuró uno (o no se puede leer).</summary>
        public const string ALIAS_POR_DEFECTO = "placeholder_alias";

        /// <summary>Clave del alias en la tabla ConfiguracionGimnasio.</summary>
        public const string CLAVE_ALIAS = "AliasTransferencia";

        public const int ALIAS_LARGO_MINIMO = 6;
        public const int ALIAS_LARGO_MAXIMO = 20;

        public const string EVENTO_TRANSFERENCIA = "transferencia";

        private MPPTransferencia mppTransferencia;
        private MPPConfiguracion mppConfiguracion;
        private BLLPago bllPago;
        private BLLAlumno bllAlumno;
        private BLLEvento bllEvento;

        public BLLTransferencia()
        {
            mppTransferencia = new MPPTransferencia();
            mppConfiguracion = new MPPConfiguracion();
            bllPago = new BLLPago();
            bllAlumno = new BLLAlumno();
            bllEvento = new BLLEvento();
        }

        // ==================== ALIAS DEL GIMNASIO ====================

        /// <summary>
        /// Alias de la cuenta del gimnasio que se le muestra al cliente para transferir. Si no se
        /// configuró o no se puede leer (ej. falta el script), devuelve el de por defecto.
        /// </summary>
        public string ObtenerAlias()
        {
            try
            {
                string alias = mppConfiguracion.Obtener(CLAVE_ALIAS);
                return string.IsNullOrWhiteSpace(alias) ? ALIAS_POR_DEFECTO : alias;
            }
            catch (Exception)
            {
                return ALIAS_POR_DEFECTO;
            }
        }

        /// <summary>
        /// Cambia el alias (solo administración, recepción o WebMaster). Como los alias bancarios:
        /// de 6 a 20 caracteres, solo letras sin tilde, números, puntos y guiones.
        /// </summary>
        public void ActualizarAlias(string alias)
        {
            int? rol = Singleton.Instancia?.Usuario?.USUARIO_Rol;
            if (rol != PerfilesSistema.RolAdministrador && rol != PerfilesSistema.RolRecepcionista && rol != PerfilesSistema.RolWebMaster)
                throw new ReglaNegocioException("pagos_err_alias_permiso", "Solo administración o recepción puede cambiar el alias.");

            alias = (alias ?? string.Empty).Trim();
            if (!Regex.IsMatch(alias, $@"^[A-Za-z0-9.\-]{{{ALIAS_LARGO_MINIMO},{ALIAS_LARGO_MAXIMO}}}$"))
                throw new ReglaNegocioException("pagos_err_alias",
                    $"El alias tiene que tener entre {ALIAS_LARGO_MINIMO} y {ALIAS_LARGO_MAXIMO} caracteres: letras sin tilde, números, puntos o guiones.",
                    ALIAS_LARGO_MINIMO, ALIAS_LARGO_MAXIMO);

            string anterior = ObtenerAlias();
            if (string.Equals(anterior, alias, StringComparison.Ordinal))
                throw new ReglaNegocioException("pagos_err_alias_igual", "Ese ya es el alias actual.");

            string usuario = Singleton.Instancia.Usuario.USUARIO_Usuario;
            mppConfiguracion.Guardar(CLAVE_ALIAS, alias, usuario);

            bllEvento.RegistrarEvento(EVENTO_TRANSFERENCIA, usuario,
                $"Alias para transferencias cambiado - Anterior: {anterior}, Nuevo: {alias}", 2, "Pagos");
        }

        // ==================== TRANSFERENCIAS ====================

        /// <summary>
        /// El cliente informa que transfirió el abono de uno de sus alumnos. Valida lo mismo que un
        /// pago (alumno activo, modalidad vigente, sin abono adelantado) y que no tenga otra pendiente.
        /// </summary>
        public TransferenciaPendiente Informar(string usuarioCliente, int dni, int modalidadId)
        {
            bool esSuyo = !string.IsNullOrEmpty(usuarioCliente)
                && (bllAlumno.ObtenerAlumnosDeUsuario(usuarioCliente) ?? new List<Alumno>()).Any(a => a.DNI == dni);
            if (!esSuyo)
                throw new ReglaNegocioException("pagos_err_no_asociado", "Ese alumno no está asociado a tu cuenta.");

            if (mppTransferencia.ListarDnisPendientes().Contains(dni))
                throw new ReglaNegocioException("pagos_err_transf_pendiente",
                    "Ya hay una transferencia informada para este alumno. Recepción la está verificando.");

            Pago abono = bllPago.PrevisualizarAbono(dni, modalidadId, DateTime.Now);

            var transferencia = new TransferenciaPendiente
            {
                Dni = dni,
                ModalidadId = modalidadId,
                Monto = abono.Monto,
                FechaSolicitud = DateTime.Now,
                UsuarioSolicita = usuarioCliente,
                Estado = TransferenciaPendiente.ESTADO_PENDIENTE
            };
            transferencia.CodTransferencia = mppTransferencia.Crear(transferencia);

            bllEvento.RegistrarEvento(EVENTO_TRANSFERENCIA, usuarioCliente,
                $"Transferencia informada - Alumno DNI: {dni}, Monto: ${abono.Monto}. Pendiente de confirmación", 3, "Pagos");
            return transferencia;
        }

        public List<TransferenciaPendiente> ListarPendientes()
        {
            try
            {
                return mppTransferencia.ListarPendientes();
            }
            catch (Exception ex)
            {
                throw new Exception("Error al listar las transferencias pendientes: " + ex.Message, ex);
            }
        }

        /// <summary>
        /// Administración comprobó que la transferencia llegó: se registra el pago (activa el abono)
        /// y la transferencia queda aprobada.
        /// </summary>
        public Pago Confirmar(int codTransferencia, string usuario)
        {
            TransferenciaPendiente transferencia = ObtenerPendiente(codTransferencia);

            Pago pago = bllPago.RegistrarPago(transferencia.Dni, transferencia.ModalidadId, "Transferencia", usuario);
            mppTransferencia.Resolver(codTransferencia, TransferenciaPendiente.ESTADO_APROBADA, usuario, pago.CodPago);

            bllEvento.RegistrarEvento(EVENTO_TRANSFERENCIA, usuario,
                $"Transferencia confirmada - Alumno DNI: {transferencia.Dni}, Pago: {pago.CodPago}", 2, "Pagos");
            return pago;
        }

        /// <summary>La transferencia no llegó: queda rechazada y el abono sigue sin activarse.</summary>
        public void Rechazar(int codTransferencia, string usuario)
        {
            TransferenciaPendiente transferencia = ObtenerPendiente(codTransferencia);

            if (!mppTransferencia.Resolver(codTransferencia, TransferenciaPendiente.ESTADO_RECHAZADA, usuario, null))
                throw YaResuelta();

            bllEvento.RegistrarEvento(EVENTO_TRANSFERENCIA, usuario,
                $"Transferencia rechazada - Alumno DNI: {transferencia.Dni}, Monto: ${transferencia.Monto}", 2, "Pagos");
        }

        private TransferenciaPendiente ObtenerPendiente(int codTransferencia)
        {
            TransferenciaPendiente transferencia = mppTransferencia.Obtener(codTransferencia);
            if (transferencia == null || transferencia.Estado != TransferenciaPendiente.ESTADO_PENDIENTE)
                throw YaResuelta();
            return transferencia;
        }

        private static ReglaNegocioException YaResuelta()
        {
            return new ReglaNegocioException("pagos_err_transf_resuelta",
                "Esa transferencia ya fue confirmada o rechazada. Actualizá la página.");
        }
    }
}
