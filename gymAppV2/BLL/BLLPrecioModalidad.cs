using System;
using System.Collections.Generic;
using System.Linq;
using BE;
using MPP;

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
                throw new Exception("El precio debe ser mayor a 0");
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

                // Actualizar el precio
                mppPrecioModalidad.ActualizarPrecio(id, nuevoPrecio);

                // Registrar evento de modificación de precio (criticidad 2 = Media Alta - Negocio)
                bllEvento.RegistrarModificacionPrecio(
                    usuarioModificador,
                    modalidadAnterior.ObtenerDescripcion(),
                    precioAnterior,
                    nuevoPrecio
                );

                // Disparar notificación a todos los usuarios que no sean Entrenadores (rol != 3)
                NotificarCambioDePrecio(id, precioAnterior, nuevoPrecio, usuarioModificador);
            }
            catch (Exception ex)
            {
                throw new Exception("Error al modificar precio: " + ex.Message, ex);
            }
        }

        /// <summary>
        /// Notifica el cambio de precio a todos los usuarios que no sean Entrenadores
        /// Según LogicaNegocio.txt: alerta interna + email
        /// </summary>
        private void NotificarCambioDePrecio(int id, decimal precioAnterior, decimal precioNuevo, string usuarioModificador = null)
        {
            // Si no hay usuario válido, no registrar eventos de notificación
            // (las notificaciones son eventos secundarios, el evento principal ya se registró)
            if (string.IsNullOrEmpty(usuarioModificador))
            {
                return;
            }

            try
            {
                // Obtener todos los usuarios excepto Entrenadores (rol != 3)
                List<UsuarioGestion> usuarios = bllUsuario.ListarUsuarios()
                    .Where(u => u.USUARIO_Tipo != "Entrenador")
                    .ToList();

                // Registrar evento de notificación masiva
                string detalleNotificacion = $"Precio modificado para {ObtenerModalidad(id)?.ObtenerDescripcion()}: ${precioAnterior} -> ${precioNuevo}";

                foreach (var usuario in usuarios)
                {
                    // Registrar evento individual de notificación (criticidad 4 = Baja)
                    bllEvento.RegistrarEvento(
                        "notificacion_cambio_precio",
                        usuarioModificador,
                        $"Notificación enviada a {usuario.USUARIO_Usuario}: {detalleNotificacion}",
                        4
                    );

                    // TODO: Enviar email al correo registrado del usuario
                    // Esto requiere implementar un servicio de email
                    // EnviarEmail(usuario.Email, "Cambio de Precio - Sportio", detalleNotificacion);
                }

                // Registrar evento consolidado (criticidad 4 = Baja)
                bllEvento.RegistrarEvento(
                    "notificacion_masiva_precio",
                    usuarioModificador,
                    $"Notificación de cambio de precio enviada a {usuarios.Count} usuarios no-entrenadores",
                    4
                );
            }
            catch (Exception ex)
            {
                // No impedir la modificación si falla la notificación (criticidad 4 = Baja)
                // Solo registrar si hay usuario válido
                if (!string.IsNullOrEmpty(usuarioModificador))
                {
                    bllEvento.RegistrarEvento(
                        "error_notificacion_precio",
                        usuarioModificador,
                        $"Error al notificar cambio de precio: {ex.Message}",
                        4
                    );
                }
            }
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
