using System;
using Servicios.Singleton;
using BLL;

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
            var usuario = Singleton.Instancia.Usuario;
            string usuarioNombre = usuario?.USUARIO_Usuario ?? "desconocido";

            // Registrar evento de logout antes de cerrar sesión
            try
            {
                var bllEvento = new BLLEvento();
                bllEvento.RegistrarLogout(usuarioNombre);
            }
            catch
            {
                // No impedir el logout si falla el log
            }

            Singleton.Instancia.LogOut();
            System.Web.Security.FormsAuthentication.SignOut();
            Response.Redirect("~/LogIn/LogIn.aspx", false);
            Context.ApplicationInstance.CompleteRequest();
        }
    }
}