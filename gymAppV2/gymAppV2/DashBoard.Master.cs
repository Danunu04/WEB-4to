using System;
using Servicios.Singleton;

namespace gymAppV2
{
    public partial class DashBoardMaster : System.Web.UI.MasterPage
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            if (!Singleton.Instancia.IsLogged())
            {
                Response.Redirect("~/LogIn/LogIn.aspx");
            }
        }

        protected void LnkLogout_Click(object sender, EventArgs e)
        {
            Singleton.Instancia.LogOut();
            System.Web.Security.FormsAuthentication.SignOut();
            Response.Redirect("~/Inicio/Default.aspx");
        }
    }
}