using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace gymAppV2.DashBoard
{
    public partial class WebForm1 : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            HtmlLink cssLink = new HtmlLink();
            cssLink.Href = ResolveUrl("~/Content/dashboard.css");
            cssLink.Attributes["rel"] = "stylesheet";
            cssLink.Attributes["type"] = "text/css";
            Page.Header.Controls.Add(cssLink);
        }
    }
}