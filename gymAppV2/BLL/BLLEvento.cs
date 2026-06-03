using System;
using System.Collections.Generic;
using BE;
using MPP;

namespace BLL
{
    public class BLLEvento
    {
        // ================================
        // CRITICIDAD 1 - ALTA (Usuarios y Seguridad)
        // ================================
        public const string EVENTO_LOGIN = "login";
        public const string EVENTO_LOGOUT = "logout";
        public const string EVENTO_ALTA_USUARIO = "alta_usuario";
        public const string EVENTO_MODIFICACION_USUARIO = "modificacion_usuario";
        public const string EVENTO_BAJA_USUARIO = "baja_usuario";
        public const string EVENTO_CAMBIO_ROL = "cambio_rol";
        public const string EVENTO_BLOQUEO_USUARIO = "bloqueo_usuario";
        public const string EVENTO_DESBLOQUEO_USUARIO = "desbloqueo_usuario";
        public const string EVENTO_ACTIVAR_USUARIO = "activar_usuario";
        public const string EVENTO_DESACTIVAR_USUARIO = "desactivar_usuario";
        public const string EVENTO_CAMBIO_CONTRASENA = "cambio_contrasena";

        // ================================
        // CRITICIDAD 2 - MEDIA ALTA (Negocio)
        // ================================
        public const string EVENTO_CHECKIN = "checkin";
        public const string EVENTO_PAGO = "pago";
        public const string EVENTO_MODIFICACION_PRECIO = "modificacion_precio";
        public const string EVENTO_INSCRIPCION = "inscripcion";
        public const string EVENTO_RUTINA_ALTA = "rutina_alta";
        public const string EVENTO_RUTINA_MODIFICACION = "rutina_modificacion";
        public const string EVENTO_RUTINA_BAJA = "rutina_baja";
        public const string EVENTO_ASOCIAR_USUARIO = "asociar_usuario";
        public const string EVENTO_DESASOCIAR_USUARIO = "desasociar_usuario";

        // ================================
        // CRITICIDAD 3 - MEDIA BAJA (ABM Básico)
        // ================================
        public const string EVENTO_ALTA_ALUMNO = "alta_alumno";
        public const string EVENTO_BAJA_ALUMNO = "baja_alumno";
        public const string EVENTO_MODIFICACION_ALUMNO = "modificacion_alumno";
        public const string EVENTO_ALTA_ENTRENADOR = "alta_entrenador";
        public const string EVENTO_BAJA_ENTRENADOR = "baja_entrenador";
        public const string EVENTO_MODIFICACION_ENTRENADOR = "modificacion_entrenador";
        public const string EVENTO_CAMBIO_DATOS_ALUMNO = "cambio_datos_alumno";
        public const string EVENTO_CAMBIO_DATOS_USUARIO = "cambio_datos_usuario";

        // ================================
        // CRITICIDAD 4 - BAJA (Básicos/Configuración)
        // ================================
        public const string EVENTO_ERROR = "error";
        public const string EVENTO_CAMBIO_IDIOMA = "cambio_idioma";
        public const string EVENTO_CONFIGURACION = "configuracion";
        public const string EVENTO_BACKUP = "backup";
        public const string EVENTO_ACTUALIZACION = "update";
        public const string EVENTO_NUEVO_USUARIO = "new_user";

        private MPPEvento mppEvento;

        public BLLEvento()
        {
            mppEvento = new MPPEvento();
        }

        /// <summary>
        /// Registra un evento en la bitácora
        /// </summary>
        /// <param name="tipo">Tipo de evento</param>
        /// <param name="usuario">Usuario que realiza la acción</param>
        /// <param name="accion">Descripción de la acción</param>
        /// <param name="criticidad">1=Alta, 2=Media Alta, 3=Media Baja, 4=Baja</param>
        /// <param name="modulo">Módulo del sistema</param>
        public int RegistrarEvento(string tipo, string usuario, string accion, int criticidad = 4, string modulo = "")
        {
            try
            {
                // Validar que la criticidad esté entre 1 y 4
                if (criticidad < 1 || criticidad > 4)
                {
                    throw new Exception("La criticidad debe estar entre 1 (Alta) y 4 (Baja)");
                }

                // Permitir eventos de login/error sin validación de sesión (ocurren antes de autenticar)
                bool esEventoPreAuth = tipo == EVENTO_LOGIN || tipo == EVENTO_ERROR || tipo == "bloqueo_usuario";

                if (!esEventoPreAuth)
                {
                    // Validar que el usuario corresponda al de la sesión activa
                    var usuarioSesion = System.Web.HttpContext.Current?.Session["UsuarioLogueado"] as BE.Usuario;
                    string usuarioEsperado = usuarioSesion?.USUARIO_Usuario ?? "sistema";

                    // Permitir "sistema" para eventos automáticos, pero validar que coincida en otros casos
                    if (usuario != "sistema" && usuario != usuarioEsperado)
                    {
                        throw new Exception($"El usuario '{usuario}' no corresponde al usuario de la sesión activa ('{usuarioEsperado}')");
                    }
                }

                Evento evento = new Evento
                {
                    EVENTO_Tipo = tipo,
                    EVENTO_Usuario = usuario,
                    EVENTO_Accion = accion,
                    EVENTO_Timestamp = DateTime.Now,
                    EVENTO_Criticidad = criticidad,
                    EVENTO_Modulo = modulo
                };

                return mppEvento.RegistrarEvento(evento, criticidad);
            }
            catch (Exception ex)
            {
                throw new Exception("Error al registrar el evento: " + ex.Message, ex);
            }
        }

        public List<Evento> ObtenerEventos(string filtro, string busqueda, int? filtroCriticidad = null, string filtroModulo = null)
        {
            try
            {
                return mppEvento.ObtenerEventos(filtro, busqueda, filtroCriticidad, filtroModulo);
            }
            catch (Exception ex)
            {
                throw new Exception("Error al obtener los eventos: " + ex.Message, ex);
            }
        }

        public List<string> ObtenerModulos()
        {
            try
            {
                return mppEvento.ObtenerModulos();
            }
            catch (Exception ex)
            {
                throw new Exception("Error al obtener los módulos: " + ex.Message, ex);
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

        // ================================
        // MÉTODOS DE REGISTRO - CRITICIDAD 1 (ALTA)
        // ================================
        public int RegistrarLogin(string usuario)
        {
            return RegistrarEvento(EVENTO_LOGIN, usuario, "Inicio de sesión exitoso", 1, "Autenticación");
        }

        public int RegistrarLogout(string usuario)
        {
            return RegistrarEvento(EVENTO_LOGOUT, usuario, "Cierre de sesión", 1, "Autenticación");
        }

        public int RegistrarAltaUsuario(string usuario, int rol)
        {
            return RegistrarEvento(EVENTO_ALTA_USUARIO, usuario, $"Usuario creado con rol {rol}", 1, "Usuarios");
        }

        public int RegistrarModificacionUsuario(string usuarioOriginal, string nuevoUsuario)
        {
            return RegistrarEvento(EVENTO_MODIFICACION_USUARIO, usuarioOriginal, $"Usuario modificado de '{usuarioOriginal}' a '{nuevoUsuario}'", 1, "Usuarios");
        }

        public int RegistrarCambioRol(string usuario, int nuevoRol)
        {
            return RegistrarEvento(EVENTO_CAMBIO_ROL, usuario, $"Rol del usuario cambiado a {nuevoRol}", 1, "Usuarios");
        }

        public int RegistrarBloqueoUsuario(string usuario)
        {
            return RegistrarEvento(EVENTO_BLOQUEO_USUARIO, usuario, "Usuario bloqueado por exceso de intentos fallidos", 1, "Autenticación");
        }

        public int RegistrarDesbloqueoUsuario(string usuario)
        {
            return RegistrarEvento(EVENTO_DESBLOQUEO_USUARIO, usuario, "Usuario desbloqueado", 1, "Autenticación");
        }

        public int RegistrarActivarUsuario(string usuario)
        {
            return RegistrarEvento(EVENTO_ACTIVAR_USUARIO, usuario, "Usuario activado", 1, "Usuarios");
        }

        public int RegistrarDesactivarUsuario(string usuario)
        {
            return RegistrarEvento(EVENTO_DESACTIVAR_USUARIO, usuario, "Usuario desactivado", 1, "Usuarios");
        }

        public int RegistrarCambioContrasena(string usuario)
        {
            return RegistrarEvento(EVENTO_CAMBIO_CONTRASENA, usuario, "Cambio de contraseña", 1, "Autenticación");
        }

        // ================================
        // MÉTODOS DE REGISTRO - CRITICIDAD 2 (MEDIA ALTA - NEGOCIO)
        // ================================
        public int RegistrarCheckin(string usuario, int dniAlumno)
        {
            return RegistrarEvento(EVENTO_CHECKIN, usuario, $"Check-in registrado para alumno DNI: {dniAlumno}", 2, "Alumnos");
        }

        public int RegistrarPago(string usuario, int dniAlumno, decimal monto, string medioPago)
        {
            return RegistrarEvento(EVENTO_PAGO, usuario, $"Pago registrado - Alumno DNI: {dniAlumno}, Monto: ${monto}, Medio: {medioPago}", 2, "Pagos");
        }

        public int RegistrarModificacionPrecio(string usuario, string actividad, decimal precioAnterior, decimal precioNuevo)
        {
            return RegistrarEvento(EVENTO_MODIFICACION_PRECIO, usuario,
                $"Modificación de precio - Actividad: {actividad}, Anterior: ${precioAnterior}, Nuevo: ${precioNuevo}",
                2, "Actividades");
        }

        public int RegistrarInscripcion(string usuario, int dniAlumno, string actividad)
        {
            return RegistrarEvento(EVENTO_INSCRIPCION, usuario, $"Inscripción registrada - Alumno DNI: {dniAlumno}, Actividad: {actividad}", 2, "Inscripciones");
        }

        public int RegistrarRutinaAlta(string usuario, int dniAlumno)
        {
            return RegistrarEvento(EVENTO_RUTINA_ALTA, usuario, $"Rutina creada para alumno DNI: {dniAlumno}", 2, "Rutinas");
        }

        public int RegistrarRutinaModificacion(string usuario, int dniAlumno)
        {
            return RegistrarEvento(EVENTO_RUTINA_MODIFICACION, usuario, $"Rutina modificada para alumno DNI: {dniAlumno}", 2, "Rutinas");
        }

        public int RegistrarRutinaBaja(string usuario, int dniAlumno)
        {
            return RegistrarEvento(EVENTO_RUTINA_BAJA, usuario, $"Rutina eliminada para alumno DNI: {dniAlumno}", 2, "Rutinas");
        }

        public int RegistrarAsociarUsuario(string usuario, int dniAlumno)
        {
            return RegistrarEvento(EVENTO_ASOCIAR_USUARIO, usuario, $"Usuario '{usuario}' asociado a alumno DNI {dniAlumno}", 2, "Alumnos");
        }

        public int RegistrarDesasociarUsuario(string usuario, int dniAlumno)
        {
            return RegistrarEvento(EVENTO_DESASOCIAR_USUARIO, usuario, $"Usuario desasociado de alumno DNI {dniAlumno}", 2, "Alumnos");
        }

        // ================================
        // MÉTODOS DE REGISTRO - CRITICIDAD 3 (MEDIA BAJA - ABM BÁSICO)
        // ================================
        public int RegistrarAltaAlumno(string usuario, int dniAlumno)
        {
            return RegistrarEvento(EVENTO_ALTA_ALUMNO, usuario, $"Alta de alumno - DNI: {dniAlumno}", 3, "Alumnos");
        }

        public int RegistrarBajaAlumno(string usuario, int dniAlumno)
        {
            return RegistrarEvento(EVENTO_BAJA_ALUMNO, usuario, $"Baja de alumno - DNI: {dniAlumno}", 3, "Alumnos");
        }

        public int RegistrarModificacionAlumno(string usuario, int dniAlumno)
        {
            return RegistrarEvento(EVENTO_MODIFICACION_ALUMNO, usuario, $"Modificación de alumno - DNI: {dniAlumno}", 3, "Alumnos");
        }

        public int RegistrarAltaEntrenador(string usuario, int dniEntrenador)
        {
            return RegistrarEvento(EVENTO_ALTA_ENTRENADOR, usuario, $"Alta de entrenador - DNI: {dniEntrenador}", 3, "Entrenadores");
        }

        public int RegistrarBajaEntrenador(string usuario, int dniEntrenador)
        {
            return RegistrarEvento(EVENTO_BAJA_ENTRENADOR, usuario, $"Baja de entrenador - DNI: {dniEntrenador}", 3, "Entrenadores");
        }

        public int RegistrarModificacionEntrenador(string usuario, int dniEntrenador)
        {
            return RegistrarEvento(EVENTO_MODIFICACION_ENTRENADOR, usuario, $"Modificación de entrenador - DNI: {dniEntrenador}", 3, "Entrenadores");
        }

        public int RegistrarCambioDatosAlumno(string usuario, int dniAlumno, string campo)
        {
            return RegistrarEvento(EVENTO_CAMBIO_DATOS_ALUMNO, usuario, $"Cambio de datos - Alumno DNI: {dniAlumno}, Campo: {campo}", 3, "Alumnos");
        }

        public int RegistrarCambioDatosUsuario(string usuario, string campo)
        {
            return RegistrarEvento(EVENTO_CAMBIO_DATOS_USUARIO, usuario, $"Cambio de datos - Usuario: {usuario}, Campo: {campo}", 3, "Usuarios");
        }

        // ================================
        // MÉTODOS DE REGISTRO - CRITICIDAD 4 (BAJA - BÁSICOS/CONFIGURACIÓN)
        // ================================
        public int RegistrarError(string usuario, string mensajeError)
        {
            return RegistrarEvento(EVENTO_ERROR, usuario, $"Error: {mensajeError}", 4, "Sistema");
        }

        public int RegistrarCambioIdioma(string usuario, string idioma)
        {
            return RegistrarEvento(EVENTO_CAMBIO_IDIOMA, usuario, $"Cambio de idioma a {idioma}", 4, "Configuración");
        }

        public int RegistrarConfiguracion(string usuario, string configuracion)
        {
            return RegistrarEvento(EVENTO_CONFIGURACION, usuario, $"Cambio de configuración: {configuracion}", 4, "Configuración");
        }

        public int RegistrarBackup(string usuario)
        {
            return RegistrarEvento(EVENTO_BACKUP, usuario, "Backup realizado", 4, "Sistema");
        }

        public int RegistrarActualizacion(string usuario, string accion)
        {
            return RegistrarEvento(EVENTO_ACTUALIZACION, usuario, accion, 4, "Sistema");
        }
    }
}