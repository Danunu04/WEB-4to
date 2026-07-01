using System;
using BE;

namespace gymAppV2.DashBoard
{
    public partial class WebForm1 : BasePage
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            VerificarAcceso(PermisosSistema.Dashboard);
        }
    }
}
