using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using gymAppV2;

namespace gymAppV2.Entrenadores
{
    public partial class Entrenadores : BasePage
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            VerificarAcceso(BE.PermisosSistema.GestionEntrenadores);
        }

        private void MostrarInfo(string mensaje)
        {
            ScriptManager.RegisterStartupScript(this, GetType(), "info", $"if(window.showToast) showToast('{System.Security.SecurityElement.Escape(mensaje)}', 'info');", true);
        }
    }
}