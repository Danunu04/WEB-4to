using System;
using System.Web.UI;

namespace gymAppV2
{
    public partial class Default : Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                // La página raíz ya no se usa; redirigir al inicio público.
                Response.Redirect("~/Inicio/Default.aspx", false);
                Context.ApplicationInstance.CompleteRequest();
            }
        }
    }
}
