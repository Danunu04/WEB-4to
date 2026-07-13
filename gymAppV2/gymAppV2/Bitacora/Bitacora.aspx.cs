using System;
using System.Collections.Generic;
using System.Web.UI;
using System.Web.UI.WebControls;
using BE;
using BLL;
using gymAppV2;

namespace gymAppV2.Bitacora
{
    public partial class Bitacora : BasePage
    {
        private string filtroActual = "all";
        private string busquedaActual = "";
        private int? filtroCriticidadActual = null;
        private string filtroModuloActual = null;
        private BLLEvento bllEvento;

        private HashSet<int> EventosExpandidos
        {
            get
            {
                var ids = ViewState["EventosExpandidos"] as HashSet<int>;
                if (ids == null)
                {
                    ids = new HashSet<int>();
                    ViewState["EventosExpandidos"] = ids;
                }
                return ids;
            }
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            VerificarAcceso(BE.PermisosSistema.Bitacora);

            if (!IsPostBack)
            {
                filtroActual = "all";
                filtroCriticidadActual = null;
                filtroModuloActual = null;
                CargarFiltrosModulos();
                CargarBitacora();
            }
            else
            {
                filtroActual = ViewState["filtro"] as string ?? "all";
                busquedaActual = ViewState["busqueda"] as string ?? "";
                filtroCriticidadActual = ViewState["filtroCriticidad"] as int?;
                filtroModuloActual = ViewState["filtroModulo"] as string;
            }
        }

        private void CargarFiltrosModulos()
        {
            try
            {
                bllEvento = new BLLEvento();
                List<string> modulos = bllEvento.ObtenerModulos();

                ddlModulo.DataSource = modulos;
                ddlModulo.DataBind();
                ddlModulo.Items.Insert(0, new ListItem("Todos los módulos", "all"));
            }
            catch (Exception)
            {
                // Si no hay módulos, dejar el dropdown vacío
                ddlModulo.Items.Insert(0, new ListItem("Todos los módulos", "all"));
            }
        }

        private void CargarBitacora()
        {
            try
            {
                pnlLoading.Visible = true;
                pnlContent.Visible = false;

                bllEvento = new BLLEvento();

                List<BE.Evento> eventos = bllEvento.ObtenerEventos(filtroActual, busquedaActual, filtroCriticidadActual, filtroModuloActual);
                var expandidos = EventosExpandidos;
                foreach (var evento in eventos)
                {
                    evento.Expandido = expandidos.Contains(evento.EVENTO_Id);
                }

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
            catch (Exception)
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
            ViewState["filtroCriticidad"] = filtroCriticidadActual;
            ViewState["filtroModulo"] = filtroModuloActual;

            CargarBitacora();
        }

        protected void btnFiltrar_Click(object sender, EventArgs e)
        {
            busquedaActual = txtBusqueda.Text.Trim();

            string criticidad = ddlCriticidad.SelectedValue;
            filtroCriticidadActual = string.IsNullOrEmpty(criticidad) ? null : (int?)Convert.ToInt32(criticidad);

            string modulo = ddlModulo.SelectedValue;
            filtroModuloActual = modulo == "all" ? null : modulo;

            ViewState["filtro"] = filtroActual;
            ViewState["busqueda"] = busquedaActual;
            ViewState["filtroCriticidad"] = filtroCriticidadActual;
            ViewState["filtroModulo"] = filtroModuloActual;

            CargarBitacora();
        }

        protected void rptEventos_ItemCommand(object source, RepeaterCommandEventArgs e)
        {
            if (e.CommandName == "Toggle")
            {
                int eventoId = Convert.ToInt32(e.CommandArgument);
                var expandidos = EventosExpandidos;

                if (expandidos.Contains(eventoId))
                    expandidos.Remove(eventoId);
                else
                    expandidos.Add(eventoId);

                ViewState["EventosExpandidos"] = expandidos;
                CargarBitacora();
            }
        }

        private void ActualizarBotonesFiltro(Dictionary<string, int> stats)
        {
            AsignarTextoYVisibilidad(btnTodos, "Todos", stats["Total"]);
            AsignarTextoYVisibilidad(btnLogin, "Login", stats["Logins"]);
            AsignarTextoYVisibilidad(btnLogout, "Logout", stats["Logouts"]);
            AsignarTextoYVisibilidad(btnBloqueo, "Bloqueos", stats["Bloqueos"]);
            AsignarTextoYVisibilidad(btnDesbloqueo, "Desbloqueos", stats["Desbloqueos"]);
            AsignarTextoYVisibilidad(btnCambioContrasena, "Cambio Contraseña", stats["CambioContrasenas"]);
            AsignarTextoYVisibilidad(btnBackup, "Backup", stats["Backups"]);
            AsignarTextoYVisibilidad(btnUsuarioNuevo, "Usuario Nuevo", stats["UsuariosNuevos"]);
            AsignarTextoYVisibilidad(btnActualizacion, "Actualización", stats["Actualizaciones"]);
            AsignarTextoYVisibilidad(btnError, "Error", stats["Errores"]);

            // Resaltar visualmente el filtro activo
            foreach (var btn in new[] { btnTodos, btnLogin, btnLogout, btnBloqueo, btnDesbloqueo, btnCambioContrasena, btnBackup, btnUsuarioNuevo, btnActualizacion, btnError })
            {
                if (btn.CommandArgument == filtroActual)
                    btn.CssClass = "filter-btn active";
                else
                    btn.CssClass = "filter-btn";
            }
        }

        private void AsignarTextoYVisibilidad(Button btn, string textoBase, int cantidad)
        {
            btn.Text = $"{textoBase} ({cantidad})";
            btn.Visible = cantidad > 0;
        }

        protected string GetLabelForType(string tipo)
        {
            switch (tipo)
            {
                case "login": return "Login";
                case "logout": return "Logout";
                case "bloqueo_usuario": return "Bloqueo de usuario";
                case "desbloqueo_usuario": return "Desbloqueo de usuario";
                case "cambio_contrasena": return "Cambio de contraseña";
                case "backup": return "Backup";
                case "new_user": return "Usuario Nuevo";
                case "update": return "Actualización";
                case "error": return "Error";
                default: return tipo;
            }
        }

        protected string GetCriticidadLabel(object criticidadObj)
        {
            int criticidad = Convert.ToInt32(criticidadObj);
            switch (criticidad)
            {
                case 1: return "Alta";
                case 2: return "Media Alta";
                case 3: return "Media Baja";
                case 4: return "Baja";
                default: return "";
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
                case "bloqueo_usuario":
                    return "<svg viewBox=\"0 0 24 24\" fill=\"none\" stroke=\"currentColor\" stroke-width=\"2\"><rect x=\"3\" y=\"11\" width=\"18\" height=\"11\" rx=\"2\" ry=\"2\"/><path d=\"M7 11V7a5 5 0 0 1 10 0v4\"/></svg>";
                case "desbloqueo_usuario":
                    return "<svg viewBox=\"0 0 24 24\" fill=\"none\" stroke=\"currentColor\" stroke-width=\"2\"><rect x=\"3\" y=\"11\" width=\"18\" height=\"11\" rx=\"2\" ry=\"2\"/><path d=\"M7 11V7a5 5 0 0 1 9.9-1\"/><circle cx=\"12\" cy=\"16\" r=\"1\"/></svg>";
                case "cambio_contrasena":
                    return "<svg viewBox=\"0 0 24 24\" fill=\"none\" stroke=\"currentColor\" stroke-width=\"2\"><rect x=\"3\" y=\"11\" width=\"18\" height=\"11\" rx=\"2\" ry=\"2\"/><path d=\"M7 11V7a5 5 0 0 1 10 0v4\"/><circle cx=\"12\" cy=\"16\" r=\"1\"/></svg>";
                default:
                    return "<svg viewBox=\"0 0 24 24\" fill=\"none\" stroke=\"currentColor\" stroke-width=\"2\"><circle cx=\"12\" cy=\"12\" r=\"10\"/><path d=\"M12 6v6l4 2\"/></svg>";
            }
        }

        private void MostrarInfo(string mensaje)
        {
            ScriptManager.RegisterStartupScript(this, GetType(), "info", $"if(window.showToast) showToast('{System.Security.SecurityElement.Escape(mensaje)}', 'info');", true);
        }
    }
}