using System;
using System.Net.Mail;
using BE;
using Servicios.Singleton;

namespace BLL
{
    /// <summary>
    /// Registro de un cliente nuevo desde el login (RFN1 - Proceso de asociación, CUS002): la
    /// persona crea su cuenta, carga los datos del alumno y paga el primer mes. Recién con el
    /// pago queda registrado: todo se valida antes de crear nada.
    ///
    /// Los errores de validación se lanzan como ReglaNegocioException con el mensaje en español
    /// (las pantallas del login no están traducidas).
    /// </summary>
    public class BLLRegistro
    {
        private BLLUsuario bllUsuario;
        private BLLAlumno bllAlumno;
        private BLLPago bllPago;
        private BLLPrecioModalidad bllPrecioModalidad;

        public BLLRegistro()
        {
            bllUsuario = new BLLUsuario();
            bllAlumno = new BLLAlumno();
            bllPago = new BLLPago();
            bllPrecioModalidad = new BLLPrecioModalidad();
        }

        // ==================== VALIDACIONES POR PASO ====================

        /// <summary>Paso 1: usuario libre, contraseña con los requisitos y bien confirmada.</summary>
        public void ValidarCuenta(DatosRegistro datos)
        {
            if (string.IsNullOrWhiteSpace(datos.Usuario))
                throw Regla("registro_err_usuario", "Ingresá un nombre de usuario.");

            if (datos.Usuario.Trim().Length > 50)
                throw Regla("registro_err_usuario_largo", "El nombre de usuario no puede superar los 50 caracteres.");

            if (bllUsuario.UsuarioExiste(datos.Usuario.Trim()))
                throw Regla("registro_err_usuario_existe", "Ese nombre de usuario ya existe. Elegí otro.");

            ConvertirError(() => bllUsuario.ValidarRequisitosContrasena(datos.Contrasena), "registro_err_contrasena");

            if (datos.Contrasena != datos.ConfirmarContrasena)
                throw Regla("registro_err_confirmacion", "Las contraseñas no coinciden.");
        }

        /// <summary>Paso 2: datos del alumno completos y válidos, y DNI sin registrar.</summary>
        public void ValidarAlumno(DatosRegistro datos)
        {
            ConvertirError(() => bllAlumno.ValidarDNI(datos.Dni.ToString()), "registro_err_dni");
            ConvertirError(() => bllAlumno.ValidarNombreApellido(datos.Nombre, "nombre"), "registro_err_nombre");
            ConvertirError(() => bllAlumno.ValidarNombreApellido(datos.Apellido, "apellido"), "registro_err_apellido");
            if (string.IsNullOrWhiteSpace(datos.Telefono))
                throw Regla("registro_err_telefono", "Ingresá un teléfono.");
            ConvertirError(() => bllAlumno.ValidarTelefono(datos.Telefono.Trim()), "registro_err_telefono");

            if (!datos.FechaNacimiento.HasValue)
                throw Regla("registro_err_fecha", "Ingresá la fecha de nacimiento.");
            ConvertirError(() => bllAlumno.ValidarFechaNacimiento(datos.FechaNacimiento.Value), "registro_err_fecha");

            if (!EmailValido(datos.Email))
                throw Regla("registro_err_email", "Ingresá un email válido.");

            if (bllUsuario.DniExiste(datos.Dni) || bllAlumno.AlumnoExiste(datos.Dni))
                throw Regla("registro_err_dni_existe",
                    "Ese DNI ya está registrado. Si ya sos alumno, pedí tu usuario en recepción.");
        }

        /// <summary>Paso 3: modalidad vigente y medio de pago online válido.</summary>
        public PrecioModalidad ValidarPago(DatosRegistro datos)
        {
            PrecioModalidad modalidad = bllPrecioModalidad.ObtenerModalidad(datos.ModalidadId);
            if (modalidad == null || !modalidad.Activo)
                throw Regla("registro_err_modalidad", "Elegí una modalidad.");

            if (datos.MetodoPago == "Tarjeta")
            {
                string error = ValidadorTarjeta.Validar(datos.TarjetaNumero, datos.TarjetaTitular,
                    datos.TarjetaVence, datos.TarjetaCvv, DateTime.Today);
                if (error != null)
                    throw Regla(error, ValidadorTarjeta.MensajeRespaldo(error));
            }
            else if (datos.MetodoPago != "Transferencia")
            {
                throw Regla("registro_err_metodo", "Elegí un medio de pago.");
            }

            return modalidad;
        }

        // ==================== REGISTRO ====================

        /// <summary>
        /// Valida todo otra vez, crea la cuenta (rol Cliente) con su alumno y registra el pago del
        /// primer mes. Devuelve el pago con las fechas del abono. Si eligió transferencia, la deja
        /// informada (pendiente de que la confirme recepción) y devuelve null: el abono se activa
        /// recién cuando se confirma.
        /// </summary>
        public Pago Registrar(DatosRegistro datos)
        {
            datos.Usuario = (datos.Usuario ?? string.Empty).Trim();
            ValidarCuenta(datos);
            ValidarAlumno(datos);
            ValidarPago(datos);

            // Mientras se crea la cuenta, la sesión es la de la persona que se registra: así la
            // bitácora registra el alta, el alumno y el pago a su nombre (es ella quien opera).
            // Al terminar se cierra siempre; después inicia sesión normalmente desde el login.
            var sesion = Singleton.Instancia;
            sesion.LogIn(new Usuario
            {
                USUARIO_Usuario = datos.Usuario,
                USUARIO_Rol = PerfilesSistema.RolCliente,
                USUARIO_Activo = true,
                USUARIO_DNI = datos.Dni
            });

            try
            {
                bllUsuario.CrearUsuario(new UsuarioCrearDTO
                {
                    Usuario = datos.Usuario,
                    Contrasena = datos.Contrasena,
                    Rol = PerfilesSistema.RolCliente,
                    AlumnoDNI = datos.Dni,
                    AlumnoNombre = datos.Nombre.Trim(),
                    AlumnoApellido = datos.Apellido.Trim(),
                    AlumnoTelefono = (datos.Telefono ?? string.Empty).Trim(),
                    AlumnoEmail = datos.Email.Trim(),
                    AlumnoFechaNacimiento = datos.FechaNacimiento
                });

                try
                {
                    if (datos.MetodoPago == "Transferencia")
                    {
                        new BLLTransferencia().Informar(datos.Usuario, datos.Dni, datos.ModalidadId);
                        return null;
                    }

                    return bllPago.RegistrarPagoCliente(datos.Usuario, datos.Dni, datos.ModalidadId, datos.MetodoPago);
                }
                catch (Exception)
                {
                    // Sin pago no hay asociación: la cuenta y el alumno quedan dados de baja
                    try { bllAlumno.DarDeBajaAlumno(datos.Dni); } catch { }
                    throw Regla("registro_err_pago",
                        "No se pudo registrar el pago. Acercate a recepción para completar el alta.");
                }
            }
            finally
            {
                try { sesion.LogOut(); } catch { }
            }
        }

        // ==================== AUXILIARES ====================

        private static bool EmailValido(string email)
        {
            if (string.IsNullOrWhiteSpace(email))
                return false;

            try
            {
                return new MailAddress(email.Trim()).Address == email.Trim();
            }
            catch (FormatException)
            {
                return false;
            }
        }

        private static ReglaNegocioException Regla(string clave, string mensaje)
        {
            return new ReglaNegocioException(clave, mensaje);
        }

        /// <summary>
        /// Los validadores de BLLAlumno y BLLUsuario lanzan Exception con un mensaje para el usuario:
        /// se pasa a ReglaNegocioException para que la pantalla lo muestre tal cual.
        /// </summary>
        private static void ConvertirError(Action validacion, string clave)
        {
            try
            {
                validacion();
            }
            catch (ReglaNegocioException)
            {
                throw;
            }
            catch (Exception ex)
            {
                throw new ReglaNegocioException(clave, ex.Message);
            }
        }
    }
}
