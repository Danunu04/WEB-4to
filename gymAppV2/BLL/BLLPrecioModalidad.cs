using System;
using System.Collections.Generic;
using System.Linq;
using System.Globalization;
using System.Net;
using BE;
using MPP;
using SERVICIOS;

namespace BLL
{
    public class BLLPrecioModalidad
    {
        private MPPPrecioModalidad mppPrecioModalidad;
        private BLLEvento bllEvento;
        private BLLUsuario bllUsuario;

        // Las 4 modalidades válidas según lógica de negocio
        private static readonly int[] DIAS_POR_SEMANA_VALIDOS = { 1, 2, 3 };
        private const int DIARIO_ID = 4;

        public BLLPrecioModalidad()
        {
            mppPrecioModalidad = new MPPPrecioModalidad();
            bllEvento = new BLLEvento();
            bllUsuario = new BLLUsuario();
        }

        /// <summary>
        /// Valida que la modalidad sea una de las 4 permitidas
        /// </summary>
        public void ValidarModalidad(int diasPorSemana, bool esDiario)
        {
            // Validar que sea una de las 4 modalidades permitidas
            if (esDiario)
            {
                // Modalidad "Diario" - válida
                return;
            }

            if (!DIAS_POR_SEMANA_VALIDOS.Contains(diasPorSemana))
            {
                throw new Exception("Solo se permiten 4 modalidades: 1, 2, 3 días por semana, o Diario");
            }
        }

        /// <summary>
        /// Valida que el precio sea mayor a 0
        /// </summary>
        public void ValidarPrecio(decimal precio)
        {
            if (precio <= 0)
            {
                throw new ReglaNegocioException("precios_err_precio", "El precio debe ser mayor a 0.");
            }
        }

        /// <summary>
        /// Obtiene todas las modalidades de precio
        /// </summary>
        public List<PrecioModalidad> ListarModalidades()
        {
            try
            {
                return mppPrecioModalidad.ListarModalidades();
            }
            catch (Exception ex)
            {
                throw new Exception("Error al listar modalidades: " + ex.Message, ex);
            }
        }

        /// <summary>
        /// Obtiene una modalidad específica por ID
        /// </summary>
        public PrecioModalidad ObtenerModalidad(int id)
        {
            try
            {
                return mppPrecioModalidad.ObtenerModalidad(id);
            }
            catch (Exception ex)
            {
                throw new Exception("Error al obtener modalidad: " + ex.Message, ex);
            }
        }

        /// <summary>
        /// Modifica el precio de una modalidad y dispara notificación a usuarios no-entrenadores
        /// </summary>
        public void ModificarPrecio(int id, decimal nuevoPrecio, string usuarioModificador)
        {
            try
            {
                // Validar precio
                ValidarPrecio(nuevoPrecio);

                // Obtener precio anterior para el evento
                PrecioModalidad modalidadAnterior = ObtenerModalidad(id);
                if (modalidadAnterior == null)
                {
                    throw new Exception($"No existe una modalidad con ID {id}");
                }

                decimal precioAnterior = modalidadAnterior.Precio;

                // Sin cambio real no se guarda ni se avisa a nadie
                if (nuevoPrecio == precioAnterior)
                {
                    throw new ReglaNegocioException("precios_err_sin_cambio", "El precio nuevo es igual al actual.");
                }

                // Actualizar el precio
                mppPrecioModalidad.ActualizarPrecio(id, nuevoPrecio);

                // Registrar evento de modificación de precio (criticidad 2 = Media Alta - Negocio)
                bllEvento.RegistrarModificacionPrecio(
                    usuarioModificador,
                    modalidadAnterior.ObtenerDescripcion(),
                    precioAnterior,
                    nuevoPrecio
                );

                // Alerta interna (banner) y email a todos los usuarios que no son Entrenadores
                NotificarCambioDePrecio(modalidadAnterior, precioAnterior, nuevoPrecio, usuarioModificador);
            }
            catch (Exception ex)
            {
                throw new Exception("Error al modificar precio: " + ex.Message, ex);
            }
        }

        /// <summary>
        /// Notifica el cambio de precio a todos los usuarios activos que no son Entrenadores
        /// (G05 - Notificaciones): crea el aviso que ven como banner y les manda un email.
        /// Si algo falla se registra en la bitácora, pero el precio ya quedó modificado.
        /// </summary>
        private void NotificarCambioDePrecio(PrecioModalidad modalidad, decimal precioAnterior, decimal precioNuevo, string usuarioModificador)
        {
            if (string.IsNullOrEmpty(usuarioModificador))
            {
                return;
            }

            string descripcion = modalidad.ObtenerDescripcion();

            // 1. Alerta interna (banner)
            try
            {
                new BLLAvisoPrecio().Crear(new AvisoPrecio
                {
                    ModalidadId = modalidad.Id,
                    Descripcion = descripcion,
                    PrecioAnterior = precioAnterior,
                    PrecioNuevo = precioNuevo,
                    Fecha = DateTime.Now,
                    UsuarioRegistro = usuarioModificador
                });
            }
            catch (Exception ex)
            {
                bllEvento.RegistrarEvento("error_notificacion_precio", usuarioModificador,
                    $"No se pudo crear el aviso interno de cambio de precio: {ex.Message}", 4, "Precios");
            }

            // 2. Email a cada usuario con email cargado
            int enviados = 0, sinEmail = 0, conError = 0;
            var email = new EmailManager();
            string asunto = "Sportio - Cambio en el precio de la cuota";

            try
            {
                List<UsuarioGestion> destinatarios = bllUsuario.ListarUsuarios()
                    .Where(u => u.USUARIO_Activo && u.USUARIO_Rol != PerfilesSistema.RolEntrenador)
                    .ToList();

                foreach (UsuarioGestion usuario in destinatarios)
                {
                    if (string.IsNullOrWhiteSpace(usuario.Email))
                    {
                        sinEmail++;
                        continue;
                    }

                    try
                    {
                        email.Enviar(usuario.Email, asunto, ArmarCuerpoEmail(usuario, descripcion, precioAnterior, precioNuevo));
                        enviados++;
                    }
                    catch
                    {
                        conError++;
                    }
                }

                bllEvento.RegistrarEvento("notificacion_masiva_precio", usuarioModificador,
                    $"Aviso de cambio de precio ({descripcion}: ${precioAnterior:0.##} -> ${precioNuevo:0.##}). " +
                    $"Emails enviados: {enviados}, sin email: {sinEmail}, con error: {conError}.",
                    4, "Precios");
            }
            catch (Exception ex)
            {
                bllEvento.RegistrarEvento("error_notificacion_precio", usuarioModificador,
                    $"Error al enviar los emails de cambio de precio: {ex.Message}", 4, "Precios");
            }
        }

        private static string ArmarCuerpoEmail(UsuarioGestion usuario, string descripcion, decimal precioAnterior, decimal precioNuevo)
        {
            CultureInfo cultura = CultureInfo.GetCultureInfo("es-AR");
            string nombre = WebUtility.HtmlEncode(string.IsNullOrWhiteSpace(usuario.Nombre) ? usuario.USUARIO_Usuario : usuario.Nombre);

            return $@"<div style=""font-family:Arial,sans-serif;font-size:14px;color:#2D2D2D"">
<p>Hola {nombre}:</p>
<p>Te avisamos que cambió el precio de la cuota mensual <strong>{WebUtility.HtmlEncode(descripcion)}</strong>:</p>
<p>Antes: <strong>{precioAnterior.ToString("C", cultura)}</strong><br/>
Ahora: <strong>{precioNuevo.ToString("C", cultura)}</strong></p>
<p>El nuevo precio se aplica a partir del próximo pago del abono.</p>
<p>Saludos,<br/>Sportio</p>
</div>";
        }

        /// <summary>
        /// Obtiene el precio según los días por semana (0 = diario)
        /// </summary>
        public decimal ObtenerPrecio(int diasPorSemana)
        {
            try
            {
                return mppPrecioModalidad.ObtenerPrecioPorDias(diasPorSemana);
            }
            catch (Exception ex)
            {
                throw new Exception("Error al obtener precio: " + ex.Message, ex);
            }
        }
    }
}
