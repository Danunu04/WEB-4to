using System;
using System.Collections.Generic;
using BE;
using MPP;

namespace BLL
{
    public class BLLEvento
    {
        public const string EVENTO_LOGIN = "login";
        public const string EVENTO_LOGOUT = "logout";
        public const string EVENTO_CHECKIN = "checkin";
        public const string EVENTO_PAGO = "pago";
        public const string EVENTO_CAMBIO_DATOS_ALUMNO = "cambio_datos_alumno";
        public const string EVENTO_CAMBIO_DATOS_USUARIO = "cambio_datos_usuario";
        public const string EVENTO_MODIFICACION_PRECIO = "modificacion_precio";
        public const string EVENTO_ALTA_ALUMNO = "alta_alumno";
        public const string EVENTO_BAJA_ALUMNO = "baja_alumno";
        public const string EVENTO_ERROR = "error";
        public const string EVENTO_CAMBIO_CONTRASENA = "cambio_contrasena";
        public const string EVENTO_BLOQUEO_USUARIO = "bloqueo_usuario";
        public const string EVENTO_DESBLOQUEO_USUARIO = "desbloqueo_usuario";

        private MPPEvento mppEvento;

        public BLLEvento()
        {
            mppEvento = new MPPEvento();
        }

        public int RegistrarEvento(string tipo, string usuario, string accion, int criticidad = 1)
        {
            try
            {
                // Validar que la criticidad esté entre 1 y 4
                if (criticidad < 1 || criticidad > 4)
                {
                    throw new Exception("La criticidad debe estar entre 1 (Info) y 4 (Crítico)");
                }

                // Validar que el usuario corresponda al de la sesión activa
                var usuarioSesion = System.Web.HttpContext.Current?.Session["UsuarioLogueado"] as BE.Usuario;
                string usuarioEsperado = usuarioSesion?.USUARIO_Usuario ?? "sistema";

                // Permitir "sistema" para eventos automáticos, pero validar que coincida en otros casos
                if (usuario != "sistema" && usuario != usuarioEsperado)
                {
                    throw new Exception($"El usuario '{usuario}' no corresponde al usuario de la sesión activa ('{usuarioEsperado}')");
                }

                Evento evento = new Evento
                {
                    EVENTO_Tipo = tipo,
                    EVENTO_Usuario = usuario,
                    EVENTO_Accion = accion,
                    EVENTO_Timestamp = DateTime.Now,
                    EVENTO_DVH = criticidad // Usamos el campo DVH para almacenar la criticidad
                };

                return mppEvento.RegistrarEvento(evento);
            }
            catch (Exception ex)
            {
                throw new Exception("Error al registrar el evento: " + ex.Message, ex);
            }
        }

        public List<Evento> ObtenerEventos(string filtro, string busqueda)
        {
            try
            {
                return mppEvento.ObtenerEventos(filtro, busqueda);
            }
            catch (Exception ex)
            {
                throw new Exception("Error al obtener los eventos: " + ex.Message, ex);
            }
        }

        public Dictionary<string, int> ObtenerEstadisticas()
        {
            try
            {
                return mppEvento.ObtenerEstadisticas();
            }
            catch (Exception ex)
            {
                throw new Exception("Error al obtener las estadísticas: " + ex.Message, ex);
            }
        }

        public int RegistrarLogin(string usuario)
        {
            return RegistrarEvento(EVENTO_LOGIN, usuario, "Inicio de sesión exitoso");
        }

        public int RegistrarLogout(string usuario)
        {
            return RegistrarEvento(EVENTO_LOGOUT, usuario, "Cierre de sesión");
        }

        public int RegistrarCheckin(string usuario, int dniAlumno)
        {
            return RegistrarEvento(EVENTO_CHECKIN, usuario, $"Check-in registrado para alumno DNI: {dniAlumno}");
        }

        public int RegistrarPago(string usuario, int dniAlumno, decimal monto, string medioPago)
        {
            return RegistrarEvento(EVENTO_PAGO, usuario, $"Pago registrado - Alumno DNI: {dniAlumno}, Monto: ${monto}, Medio: {medioPago}");
        }

        public int RegistrarCambioDatosAlumno(string usuario, int dniAlumno, string campo)
        {
            return RegistrarEvento(EVENTO_CAMBIO_DATOS_ALUMNO, usuario, $"Cambio de datos - Alumno DNI: {dniAlumno}, Campo: {campo}");
        }

        public int RegistrarCambioDatosUsuario(string usuario, string campo)
        {
            return RegistrarEvento(EVENTO_CAMBIO_DATOS_USUARIO, usuario, $"Cambio de datos - Usuario: {usuario}, Campo: {campo}");
        }

        public int RegistrarModificacionPrecio(string usuario, string actividad, decimal precioAnterior, decimal precioNuevo, int criticidad = 2)
        {
            return RegistrarEvento(EVENTO_MODIFICACION_PRECIO, usuario,
                $"Modificación de precio - Actividad: {actividad}, Anterior: ${precioAnterior}, Nuevo: ${precioNuevo}",
                criticidad);
        }

        public int RegistrarAltaAlumno(string usuario, int dniAlumno)
        {
            return RegistrarEvento(EVENTO_ALTA_ALUMNO, usuario, $"Alta de alumno - DNI: {dniAlumno}");
        }

        public int RegistrarBajaAlumno(string usuario, int dniAlumno)
        {
            return RegistrarEvento(EVENTO_BAJA_ALUMNO, usuario, $"Baja de alumno - DNI: {dniAlumno}");
        }

        public int RegistrarError(string usuario, string mensajeError)
        {
            return RegistrarEvento(EVENTO_ERROR, usuario, $"Error: {mensajeError}");
        }

        public int RegistrarCambioContrasena(string usuario)
        {
            return RegistrarEvento(EVENTO_CAMBIO_CONTRASENA, usuario, "Cambio de contraseña");
        }

        public int RegistrarBloqueoUsuario(string usuario)
        {
            return RegistrarEvento(EVENTO_BLOQUEO_USUARIO, usuario, "Usuario bloqueado por exceso de intentos fallidos");
        }

        public int RegistrarDesbloqueoUsuario(string usuario)
        {
            return RegistrarEvento(EVENTO_DESBLOQUEO_USUARIO, usuario, "Usuario desbloqueado");
        }
    }
}