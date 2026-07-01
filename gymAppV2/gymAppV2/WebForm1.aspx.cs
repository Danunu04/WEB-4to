using System;
using System.Web.UI;

namespace gymAppV2
{
    public partial class WebForm1 : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                // Página de prueba no utilizada; redirigir al inicio público.
                Response.Redirect("~/Inicio/Default.aspx", false);
                Context.ApplicationInstance.CompleteRequest();
            }
        }
    }
}
