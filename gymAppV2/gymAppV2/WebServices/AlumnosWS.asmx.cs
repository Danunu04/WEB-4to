using System;
using System.Collections.Generic;
using System.Web.Services;
using System.Web.Services.Protocols;
using System.Linq;
using BE;
using BLL;
using Servicios.Singleton;

namespace gymAppV2.WebServices
{
    /// <summary>
    /// Web service de Alumnos: se ubica entre la página y la base de datos.
    /// Recibe el pedido, identifica al usuario logueado por la cookie de Forms Authentication
    /// que reenvía la página, verifica su rol, consulta la base a través de BLLAlumno
    /// (BLL -> MPP -> DAL) y devuelve los datos a quien lo llamó.
    ///
    /// Las consultas no usan la Session (EnableSession=false a propósito): la página que lo llama
    /// tiene tomado el lock de su sesión mientras espera la respuesta, así que si el servicio
    /// pidiera la misma sesión se bloquearían mutuamente.
    ///
    /// Las operaciones de alta, baja, modificación y asociación sí habilitan la sesión, pero
    /// el proxy solo reenvía la cookie de login (no la de sesión): cada llamada recibe una
    /// sesión nueva y propia, sin riesgo de bloqueo. Ahí se pone al operador autenticado mientras
    /// dura la operación, para que la bitácora registre las acciones a su nombre, y al terminar
    /// se descarta (ver EjecutarOperacion).
    /// </summary>
    [WebService(Namespace = AlumnosWS.NAMESPACE)]
    [WebServiceBinding(ConformsTo = WsiProfiles.BasicProfile1_1)]
    [System.ComponentModel.ToolboxItem(false)]
    public class AlumnosWS : WebService
    {
        // Debe coincidir con el namespace usado por el proxy (AlumnosWSCliente)
        public const string NAMESPACE = "http://gymappv2.local/ws/alumnos";

        [WebMethod(Description = "Devuelve todos los alumnos registrados (solo Administrador/Recepcionista).")]
        public List<Alumno> ListarAlumnos()
        {
            Usuario usuario = ObtenerUsuarioAutenticado();
            if (EsSoloLectura(usuario))
            {
                throw new SoapException("No tiene permiso para listar todos los alumnos.", SoapException.ClientFaultCode);
            }

            return Ejecutar(() => new BLLAlumno().ListarAlumnos() ?? new List<Alumno>());
        }

        [WebMethod(Description = "Devuelve los alumnos vinculados a la cuenta del usuario logueado (titular o familiar).")]
        public List<Alumno> ObtenerMisAlumnos()
        {
            // El usuario sale de la cookie, no de un parámetro: así nadie puede pedir los alumnos de otra cuenta.
            Usuario usuario = ObtenerUsuarioAutenticado();
            return Ejecutar(() => new BLLAlumno().ObtenerAlumnosDeUsuario(usuario.USUARIO_Usuario));
        }

        // ==================== ABM (CUN011.1 a CUN011.4) ====================

        [WebMethod(EnableSession = true, Description = "Da de alta un alumno (y su cuenta de Cliente si no tiene). Solo Administrador/Recepcionista.")]
        public RespuestaAlumnoWS RegistrarAlumno(SolicitudAlumno solicitud)
        {
            return EjecutarOperacion(respuesta =>
            {
                ResultadoAltaAlumno alta = new BLLAlumno().RegistrarAlumno(solicitud);
                respuesta.UsuarioCreado = alta.UsuarioCreado;
                respuesta.ContrasenaGenerada = alta.ContrasenaGenerada;
                respuesta.Mensaje = "Alumno registrado.";
            });
        }

        [WebMethod(EnableSession = true, Description = "Modifica el peso y el estado de un alumno. Solo Administrador/Recepcionista.")]
        public RespuestaAlumnoWS ModificarAlumno(SolicitudAlumno solicitud)
        {
            return EjecutarOperacion(respuesta =>
            {
                var bllAlumno = new BLLAlumno();
                Alumno alumno = bllAlumno.ObtenerAlumno(solicitud.Dni);
                if (alumno == null)
                {
                    throw new ReglaNegocioException("alumnos_msg_no_existe", $"No existe un alumno con DNI {solicitud.Dni}.");
                }

                alumno.Peso = solicitud.Peso;
                alumno.Activo = solicitud.Activo;
                bllAlumno.ActualizarAlumno(alumno);
                respuesta.Mensaje = "Alumno modificado.";
            });
        }

        [WebMethod(EnableSession = true, Description = "Baja lógica de un alumno y, si corresponde, de su cuenta. Solo Administrador/Recepcionista.")]
        public RespuestaAlumnoWS DarDeBajaAlumno(int dni)
        {
            return EjecutarOperacion(respuesta =>
            {
                respuesta.CuentasDesactivadas = new BLLAlumno().DarDeBajaAlumno(dni);
                respuesta.Mensaje = "Alumno dado de baja.";
            });
        }

        [WebMethod(EnableSession = true, Description = "Vincula un alumno a una cuenta de usuario (titular o familiar). Solo Administrador/Recepcionista.")]
        public RespuestaAlumnoWS AsociarAlumnoAUsuario(int dni, string usuario, string parentesco)
        {
            return EjecutarOperacion(respuesta =>
            {
                new BLLAlumno().AsociarFamiliar(dni, usuario, parentesco);
                respuesta.Mensaje = "Alumno vinculado.";
            });
        }

        [WebMethod(EnableSession = true, Description = "Quita el vínculo entre un alumno y una cuenta de usuario. Solo Administrador/Recepcionista.")]
        public RespuestaAlumnoWS DesasociarAlumnoDeUsuario(int dni, string usuario)
        {
            return EjecutarOperacion(respuesta =>
            {
                new BLLAlumno().DesasociarFamiliar(dni, usuario);
                respuesta.Mensaje = "Vínculo quitado.";
            });
        }

        /// <summary>
        /// Valida al operador (sesión iniciada, cuenta habilitada, acceso a Gestión de Alumnos y no
        /// ser Cliente/Familiar), lo pone en la sesión propia de esta llamada para la bitácora y
        /// ejecuta la operación. Siempre devuelve una respuesta con código y mensaje: las reglas
        /// de negocio salen con su mensaje y cualquier otro error con uno genérico.
        /// </summary>
        private RespuestaAlumnoWS EjecutarOperacion(Action<RespuestaAlumnoWS> operacion)
        {
            var respuesta = new RespuestaAlumnoWS { Codigo = RespuestaAlumnoWS.OK };

            Usuario usuario;
            try
            {
                usuario = ObtenerUsuarioAutenticado();
            }
            catch (SoapException ex)
            {
                respuesta.Codigo = User != null && User.Identity.IsAuthenticated
                    ? RespuestaAlumnoWS.SIN_PERMISO
                    : RespuestaAlumnoWS.NO_AUTENTICADO;
                respuesta.Mensaje = ex.Message;
                return respuesta;
            }

            if (EsSoloLectura(usuario))
            {
                respuesta.Codigo = RespuestaAlumnoWS.SIN_PERMISO;
                respuesta.Mensaje = "No tiene permiso para modificar alumnos.";
                return respuesta;
            }

            try
            {
                Singleton.Instancia.LogIn(usuario);
                operacion(respuesta);
            }
            catch (Exception ex)
            {
                CompletarError(respuesta, ex);
            }
            finally
            {
                try { Session?.Abandon(); } catch { }
            }

            return respuesta;
        }

        private static void CompletarError(RespuestaAlumnoWS respuesta, Exception ex)
        {
            for (Exception e = ex; e != null; e = e.InnerException)
            {
                if (e is ReglaNegocioException regla)
                {
                    respuesta.Codigo = RespuestaAlumnoWS.DATOS_INVALIDOS;
                    respuesta.Mensaje = regla.Message;
                    respuesta.Clave = regla.Clave;
                    respuesta.Argumentos = regla.Args.Select(a => Convert.ToString(a)).ToList();
                    return;
                }

                if (e is PermisoException permiso)
                {
                    respuesta.Codigo = RespuestaAlumnoWS.SIN_PERMISO;
                    respuesta.Mensaje = permiso.Message;
                    respuesta.Clave = permiso.Clave;
                    return;
                }
            }

            System.Diagnostics.Trace.WriteLine($"[AlumnosWS] {ex}");
            respuesta.Codigo = RespuestaAlumnoWS.ERROR_INTERNO;
            respuesta.Mensaje = "No se pudo completar la operación.";
            respuesta.Clave = "msg_error_generico";
        }

        /// <summary>
        /// Identifica al usuario a partir de la cookie de Forms Authentication y valida contra USUARIOS
        /// que la cuenta siga activa, no esté bloqueada y su rol tenga acceso a Gestión de Alumnos.
        /// </summary>
        private Usuario ObtenerUsuarioAutenticado()
        {
            if (User == null || !User.Identity.IsAuthenticated)
            {
                throw new SoapException("Debe iniciar sesión para usar el servicio.", SoapException.ClientFaultCode);
            }

            Usuario usuario;
            try
            {
                usuario = new BLLUsuario().ObtenerUsuario(User.Identity.Name);
            }
            catch (Exception)
            {
                throw new SoapException("No se pudo validar el usuario.", SoapException.ServerFaultCode);
            }

            if (usuario == null || !usuario.USUARIO_Activo || usuario.USUARIO_Bloqueado)
            {
                throw new SoapException("Usuario no habilitado.", SoapException.ClientFaultCode);
            }

            if (!new BLLRol().UsuarioTieneAcceso(usuario.USUARIO_Usuario, PermisosSistema.GestionAlumnos))
            {
                throw new SoapException("No tiene permiso para acceder a los alumnos.", SoapException.ClientFaultCode);
            }

            return usuario;
        }

        // Cliente y Familiar solo pueden ver sus propios alumnos (mismo criterio que Alumnos.aspx)
        private static bool EsSoloLectura(Usuario usuario)
        {
            return usuario.USUARIO_Rol == PerfilesSistema.RolCliente || usuario.USUARIO_Rol == PerfilesSistema.RolFamiliar;
        }

        /// <summary>
        /// Ejecuta la consulta y convierte cualquier error en un SoapFault genérico,
        /// para no exponer detalles internos (SQL, stack trace) al cliente.
        /// </summary>
        private static T Ejecutar<T>(Func<T> consulta)
        {
            try
            {
                return consulta();
            }
            catch (Exception)
            {
                throw new SoapException("Error al consultar los alumnos.", SoapException.ServerFaultCode);
            }
        }
    }
}
