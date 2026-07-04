using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using gymAppV2;
using Servicios.Singleton;

namespace gymAppV2.Rutinas
{
    public partial class Rutinas : BasePage
    {
        /// <summary>
        /// Indica si el usuario logueado es un Cliente (rol 4).
        /// </summary>
        protected bool EsCliente
        {
            get { return Singleton.Instancia.Usuario?.USUARIO_Rol == 4; }
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            VerificarAcceso(BE.PermisosSistema.GestionRutinas);

            if (!IsPostBack)
            {
                // El Cliente solo debería ver rutinas de sus alumnos asociados.
                pnlCliente.Visible = EsCliente;
                pnlAdmin.Visible = !EsCliente;

                if (EsCliente)
                {
                    PrepararFiltroPorAlumnosAsociados();
                }
            }
        }

        /// <summary>
        /// Prepara el filtro de rutinas para el Cliente logueado.
        /// Cuando exista la capa de datos de Rutinas, aquí se cargarán solo las rutinas
        /// cuyo alumno esté asociado al usuario actual.
        /// </summary>
        private void PrepararFiltroPorAlumnosAsociados()
        {
            string usuarioActual = Singleton.Instancia.Usuario?.USUARIO_Usuario;
            if (string.IsNullOrEmpty(usuarioActual))
            {
                return;
            }

            // TODO: cuando exista BLLRutina, filtrar por alumnos cuyo Usuario == usuarioActual.
            // Por ahora solo se deja preparada la verificación de propiedad de datos.
        }
    }
}
