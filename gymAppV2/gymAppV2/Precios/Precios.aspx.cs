using System;
using System.Globalization;
using System.Linq;
using System.Web.UI.WebControls;
using BLL;
using BE;
using gymAppV2;
using Servicios.Singleton;

namespace gymAppV2.Precios
{
    /// <summary>
    /// Precios de cuota: administración ve el precio mensual de cada modalidad y lo modifica.
    /// Cada cambio avisa a los usuarios que no son entrenadores con un banner y un email
    /// (lo hace BLLPrecioModalidad).
    /// </summary>
    public partial class Precios : BasePage
    {
        private BLLPrecioModalidad bllPrecioModalidad;

        protected void Page_Load(object sender, EventArgs e)
        {
            VerificarAcceso(PermisosSistema.PreciosCuota);

            bllPrecioModalidad = new BLLPrecioModalidad();

            if (!IsPostBack)
            {
                AplicarIdioma();
                CargarPrecios();
            }
        }

        public override void OnIdiomaChanged(IdiomaApp idioma)
        {
            base.OnIdiomaChanged(idioma);
            AplicarIdioma();
        }

        private void AplicarIdioma()
        {
            litTitulo.Text = Texto("precios_titulo", "Precios de cuota");
            litSubtitulo.Text = Texto("precios_subtitulo", "Precio mensual de cada modalidad de asistencia.");
            litAviso.Text = Texto("precios_aviso",
                "Al cambiar un precio se avisa a todos los usuarios que no son entrenadores con un aviso en la app y un email.");
        }

        /// <summary>Texto del botón de cada fila (se evalúa al enlazar el Repeater).</summary>
        protected string TextoBotonGuardar
        {
            get { return Texto("precios_btn_guardar", "Guardar"); }
        }

        private void CargarPrecios()
        {
            try
            {
                rptPrecios.DataSource = bllPrecioModalidad.ListarModalidades()
                    .Where(m => m.Activo)
                    .Select(m => new
                    {
                        m.Id,
                        Descripcion = m.ObtenerDescripcion(),
                        m.Precio,
                        m.FechaModificacion
                    })
                    .ToList();
                rptPrecios.DataBind();
            }
            catch (Exception ex)
            {
                MostrarError(MensajeError(ex));
            }
        }

        protected void rptPrecios_ItemCommand(object source, RepeaterCommandEventArgs e)
        {
            if (e.CommandName != "Guardar")
                return;

            try
            {
                int id = Convert.ToInt32(e.CommandArgument);
                var txtNuevoPrecio = (TextBox)e.Item.FindControl("txtNuevoPrecio");
                string texto = (txtNuevoPrecio.Text ?? string.Empty).Trim().Replace(',', '.');

                if (!decimal.TryParse(texto, NumberStyles.Number, CultureInfo.InvariantCulture, out decimal nuevoPrecio) || nuevoPrecio <= 0)
                {
                    MostrarError(Texto("precios_err_precio", "El precio debe ser mayor a 0."));
                    return;
                }

                string usuario = Singleton.Instancia.Usuario?.USUARIO_Usuario;
                bllPrecioModalidad.ModificarPrecio(id, Math.Round(nuevoPrecio, 2), usuario);

                MostrarExito(Texto("precios_msg_guardado", "Precio actualizado. Se avisó a los usuarios."));
                CargarPrecios();
            }
            catch (Exception ex)
            {
                MostrarError(MensajeError(ex));
            }
        }

        private string MensajeError(Exception ex)
        {
            return MensajeDePermisos(ex)
                ?? Texto("msg_error_generico", "Ocurrió un error inesperado. Intente nuevamente.");
        }
    }
}
