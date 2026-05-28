using System;
using System.Collections.Generic;
using System.Web.UI;
using System.Web.UI.WebControls;
using BE;
using BLL;

namespace gymAppV2.Bitacora
{
    public partial class Bitacora : System.Web.UI.Page
    {
        private string filtroActual = "all";
        private string busquedaActual = "";
        private BLLEvento bllEvento;

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                filtroActual = "all";
                CargarBitacora();
            }
            else
            {
                filtroActual = ViewState["filtro"] as string ?? "all";
                busquedaActual = ViewState["busqueda"] as string ?? "";
            }
        }

        private void CargarBitacora()
        {
            try
            {
                pnlLoading.Visible = true;
                pnlContent.Visible = false;

                bllEvento = new BLLEvento();

                List<BE.Evento> eventos = bllEvento.ObtenerEventos(filtroActual, busquedaActual);
                Dictionary<string, int> stats = bllEvento.ObtenerEstadisticas();

                lblTotal.Text = stats["Total"].ToString();
                lblLogins.Text = stats["Logins"].ToString();
                lblUsuariosNuevos.Text = stats["UsuariosNuevos"].ToString();
                lblErrores.Text = stats["Errores"].ToString();

                ActualizarBotonesFiltro(stats);

                if (eventos.Count == 0)
                {
                    pnlNoEventos.Visible = true;
                    pnlEventos.Visible = false;
                }
                else
                {
                    rptEventos.DataSource = eventos;
                    rptEventos.DataBind();
                    pnlNoEventos.Visible = false;
                    pnlEventos.Visible = true;
                }

                pnlLoading.Visible = false;
                pnlContent.Visible = true;
            }
            catch (Exception ex)
            {
                lblError.Text = "Error al cargar la bitácora. Intente nuevamente.";
                lblError.Visible = true;
                pnlLoading.Visible = false;
                pnlContent.Visible = false;
            }
        }

        protected void btnFiltro_Click(object sender, EventArgs e)
        {
            Button btn = (Button)sender;
            filtroActual = btn.CommandArgument;

            ViewState["filtro"] = filtroActual;
            ViewState["busqueda"] = busquedaActual;

            CargarBitacora();
        }

        protected void txtBusqueda_TextChanged(object sender, EventArgs e)
        {
            busquedaActual = txtBusqueda.Text.Trim();

            ViewState["filtro"] = filtroActual;
            ViewState["busqueda"] = busquedaActual;

            CargarBitacora();
        }

        protected void rptEventos_ItemCommand(object source, RepeaterCommandEventArgs e)
        {
            if (e.CommandName == "Toggle")
            {
                int eventoId = Convert.ToInt32(e.CommandArgument);
            }
        }

        private void ActualizarBotonesFiltro(Dictionary<string, int> stats)
        {
            btnLogin.Text = "Login (" + stats["Logins"] + ")";
            btnLogout.Text = "Logout";
            btnBackup.Text = "Backup";
            btnUsuarioNuevo.Text = "Usuario Nuevo (" + stats["UsuariosNuevos"] + ")";
            btnActualizacion.Text = "Actualización";
            btnError.Text = "Error (" + stats["Errores"] + ")";
            btnTodos.Text = "Todos (" + stats["Total"] + ")";
        }

        protected string GetLabelForType(string tipo)
        {
            switch (tipo)
            {
                case "login": return "Login";
                case "logout": return "Logout";
                case "backup": return "Backup";
                case "new_user": return "Usuario Nuevo";
                case "update": return "Actualización";
                case "error": return "Error";
                default: return tipo;
            }
        }

        protected string GetIconForType(string tipo)
        {
            switch (tipo)
            {
                case "login":
                    return "<svg viewBox=\"0 0 24 24\" fill=\"none\" stroke=\"currentColor\" stroke-width=\"2\"><path d=\"M15 3h4a2 2 0 0 1 2 2v14a2 2 0 0 1-2 2h-4\"/><polyline points=\"10 17 15 12 10 7\"/><line x1=\"15\" y1=\"12\" x2=\"3\" y2=\"12\"/></svg>";
                case "logout":
                    return "<svg viewBox=\"0 0 24 24\" fill=\"none\" stroke=\"currentColor\" stroke-width=\"2\"><path d=\"M9 21H5a2 2 0 0 1-2-2V5a2 2 0 0 1 2-2h4\"/><polyline points=\"16 17 21 12 16 7\"/><line x1=\"21\" y1=\"12\" x2=\"9\" y2=\"12\"/></svg>";
                case "backup":
                    return "<svg viewBox=\"0 0 24 24\" fill=\"none\" stroke=\"currentColor\" stroke-width=\"2\"><path d=\"M19 21H5a2 2 0 0 1-2-2V5a2 2 0 0 1 2-2h11l5 5v11a2 2 0 0 1-2 2z\"/><polyline points=\"17 21 17 13 7 13 7 21\"/><polyline points=\"7 3 7 8 15 8\"/></svg>";
                case "new_user":
                    return "<svg viewBox=\"0 0 24 24\" fill=\"none\" stroke=\"currentColor\" stroke-width=\"2\"><path d=\"M20 21v-2a4 4 0 0 0-4-4H8a4 4 0 0 0-4 4v2\"/><circle cx=\"12\" cy=\"7\" r=\"4\"/><line x1=\"12\" y1=\"11\" x2=\"12\" y2=\"11\"/><line x1=\"12\" y1=\"15\" x2=\"12\" y2=\"23\"/><line x1=\"8\" y1=\"15\" x2=\"8\" y2=\"23\"/><line x1=\"16\" y1=\"15\" x2=\"16\" y2=\"23\"/></svg>";
                case "update":
                    return "<svg viewBox=\"0 0 24 24\" fill=\"none\" stroke=\"currentColor\" stroke-width=\"2\"><path d=\"M21.5 2v6h-6M2.5 22v-6h6M2 11.5a10 10 0 0 1 18.8-4.3M22 12.5a10 10 0 0 1-18.8 4.2\"/></svg>";
                case "error":
                    return "<svg viewBox=\"0 0 24 24\" fill=\"none\" stroke=\"currentColor\" stroke-width=\"2\"><circle cx=\"12\" cy=\"12\" r=\"10\"/><line x1=\"12\" y1=\"8\" x2=\"12\" y2=\"12\"/><line x1=\"12\" y1=\"16\" x2=\"12.01\" y2=\"16\"/></svg>";
                default:
                    return "<svg viewBox=\"0 0 24 24\" fill=\"none\" stroke=\"currentColor\" stroke-width=\"2\"><circle cx=\"12\" cy=\"12\" r=\"10\"/><path d=\"M12 6v6l4 2\"/></svg>";
            }
        }

        private void MostrarError(string mensaje)
        {
            ScriptManager.RegisterStartupScript(this, GetType(), "error", $"if(window.showToast) showToast('{System.Security.SecurityElement.Escape(mensaje)}', 'error');", true);
        }

        private void MostrarExito(string mensaje)
        {
            ScriptManager.RegisterStartupScript(this, GetType(), "exito", $"if(window.showToast) showToast('{System.Security.SecurityElement.Escape(mensaje)}', 'success');", true);
        }

        private void MostrarAdvertencia(string mensaje)
        {
            ScriptManager.RegisterStartupScript(this, GetType(), "advertencia", $"if(window.showToast) showToast('{System.Security.SecurityElement.Escape(mensaje)}', 'warning');", true);
        }

        private void MostrarInfo(string mensaje)
        {
            ScriptManager.RegisterStartupScript(this, GetType(), "info", $"if(window.showToast) showToast('{System.Security.SecurityElement.Escape(mensaje)}', 'info');", true);
        }
    }
}