using System;
using System.Collections.Generic;
using System.Web.Services;
using System.Web.Services.Protocols;
using BE;
using BLL;

namespace gymAppV2.WebServices
{
    /// <summary>
    /// Web service de Alumnos: se ubica entre la página y la base de datos.
    /// Recibe el pedido, identifica al usuario logueado por la cookie de Forms Authentication
    /// que reenvía la página, verifica su rol, consulta la base a través de BLLAlumno
    /// (BLL -> MPP -> DAL) y devuelve los datos a quien lo llamó.
    ///
    /// No usa la Session (EnableSession=false a propósito): la página que lo llama tiene
    /// tomado el lock de su sesión mientras espera la respuesta, así que si el servicio
    /// pidiera la misma sesión se bloquearían mutuamente.
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

            if (!new BLLRol().TieneAccesoAModulo(usuario.USUARIO_Rol, PermisosSistema.GestionAlumnos))
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
