using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using BE;
using BLL;
using Servicios.Singleton;

namespace gymAppV2.VerificacioDV
{
    public partial class VerificacioDV : BasePage
    {
        private BLLDigitoVerificador bllDV;

        protected void Page_Load(object sender, EventArgs e)
        {
            bllDV = new BLLDigitoVerificador();

            if (!IsPostBack)
            {
                ConfigurarVistaSegunRol();
            }
        }

        /// <summary>
        /// Si el usuario es administrador muestra el panel de control.
        /// Si no lo es, muestra la pantalla de pausa/bloqueo.
        /// </summary>
        private void ConfigurarVistaSegunRol()
        {
            bool esAdmin = bllDV.UsuarioActualEsAdmin();

            pnlAdmin.Visible = esAdmin;
            pnlNoAdmin.Visible = !esAdmin;

            if (esAdmin)
            {
                try
                {
                    CargarEstadoControl();

                    var tablasSinControl = bllDV.ObtenerTablasSinControl();
                    pnlInicializar.Visible = tablasSinControl.Count > 0;

                    CargarResultados(bllDV.VerificarIntegridad());
                }
                catch (Exception ex)
                {
                    MostrarMensaje("No se pudo verificar la integridad: " + ex.Message, "error");
                }
            }
        }

        /// <summary>
        /// Carga el resumen de estado de control (tablas registradas y filas con DV vacíos).
        /// </summary>
        private void CargarEstadoControl()
        {
            try
            {
                var estado = bllDV.ObtenerEstadoControl();
                gvEstadoControl.DataSource = estado;
                gvEstadoControl.DataBind();
            }
            catch (Exception ex)
            {
                MostrarMensaje("No se pudo cargar el estado de control: " + ex.Message, "error");
            }
        }

        /// <summary>
        /// Carga los resultados agrupados por tabla: errores arriba expandidos, OK en desplegable cerrado.
        /// </summary>
        private void CargarResultados(List<ResultadoVerificacionDV> resultados)
        {
            // Agrupar por tabla
            var grupos = resultados
                .GroupBy(r => r.NombreTabla)
                .Select(g => new { Tabla = g.Key, Items = g.ToList(), TieneError = g.Any(r => !r.EsValido) })
                .OrderBy(g => g.Tabla)
                .ToList();

            var conError = grupos.Where(g => g.TieneError).ToList();
            var sinError = grupos.Where(g => !g.TieneError).ToList();

            lblTotalTablas.Text = grupos.Count.ToString();
            lblTotalErrores.Text = conError.Count.ToString();
            lblTablasOk.Text = sinError.Count.ToString();

            if (resultados.Count == 0)
            {
                litResultados.Text = "<p class=\"dv-sin-resultados\">No hay resultados. Presione \"Verificar ahora\".</p>";
                return;
            }

            var sb = new StringBuilder();

            // ── Tablas con error (expandidas) ──────────────────────────────
            if (conError.Count > 0)
            {
                sb.Append("<div class=\"dv-grupo-errores\">");
                sb.Append($"<h4 class=\"dv-grupo-titulo dv-grupo-titulo-error\"><i class=\"fa-solid fa-circle-xmark\"></i> Tablas con errores ({conError.Count})</h4>");

                foreach (var g in conError)
                {
                    sb.Append("<details class=\"dv-tabla-grupo dv-tabla-error\" open>");
                    sb.Append($"<summary class=\"dv-tabla-summary\"><i class=\"fa-solid fa-table\"></i> {HttpUtility.HtmlEncode(g.Tabla)}</summary>");
                    sb.Append(RenderTablaResultados(g.Items));
                    sb.Append("</details>");
                }

                sb.Append("</div>");
            }

            // ── Tablas correctas (colapsadas) ──────────────────────────────
            if (sinError.Count > 0)
            {
                sb.Append($"<details class=\"dv-seccion-ok\">");
                sb.Append($"<summary class=\"dv-grupo-titulo dv-grupo-titulo-ok\"><i class=\"fa-solid fa-circle-check\"></i> Tablas correctas ({sinError.Count})</summary>");

                foreach (var g in sinError)
                {
                    sb.Append("<details class=\"dv-tabla-grupo dv-tabla-ok\">");
                    sb.Append($"<summary class=\"dv-tabla-summary\"><i class=\"fa-solid fa-table\"></i> {HttpUtility.HtmlEncode(g.Tabla)}</summary>");
                    sb.Append(RenderTablaResultados(g.Items));
                    sb.Append("</details>");
                }

                sb.Append("</details>");
            }

            litResultados.Text = sb.ToString();
        }

        private string RenderTablaResultados(List<ResultadoVerificacionDV> items)
        {
            var sb = new StringBuilder();
            sb.Append("<table class=\"dv-grid dv-grid-detalle\"><thead><tr>");
            sb.Append("<th>Clave</th><th>Campo</th><th>Estado</th><th>Tipo de alteración</th><th>Mensaje</th>");
            sb.Append("</tr></thead><tbody>");

            foreach (var r in items)
            {
                string claseFila = r.EsValido ? "dv-fila-ok" : "dv-fila-error";
                string estadoHtml = r.EsValido
                    ? "<span class=\"dv-estado-ok\">OK</span>"
                    : "<span class=\"dv-estado-error\">ERROR</span>";
                sb.Append($"<tr class=\"{claseFila}\">");
                sb.Append($"<td>{HttpUtility.HtmlEncode(r.ClaveFila)}</td>");
                sb.Append($"<td>{HttpUtility.HtmlEncode(r.Campo)}</td>");
                sb.Append($"<td>{estadoHtml}</td>");
                sb.Append($"<td>{HttpUtility.HtmlEncode(r.TipoAlteracionTexto ?? "")}</td>");
                sb.Append($"<td>{HttpUtility.HtmlEncode(r.Mensaje)}</td>");
                sb.Append("</tr>");
            }

            sb.Append("</tbody></table>");
            return sb.ToString();
        }

        protected void btnVerificar_Click(object sender, EventArgs e)
        {
            try
            {
                CargarEstadoControl();

                var resultados = bllDV.VerificarIntegridad();
                CargarResultados(resultados);

                if (resultados.Any(r => !r.EsValido))
                    MostrarMensaje("Se detectaron errores de integridad. Revise el detalle.", "error");
                else
                    MostrarMensaje("No se detectaron errores de integridad.", "ok");
            }
            catch (Exception ex)
            {
                MostrarMensaje("Error al verificar: " + ex.Message, "error");
            }
        }

        protected void btnRecalcular_Click(object sender, EventArgs e)
        {
            try
            {
                bllDV.RecalcularDigitos();
                CargarEstadoControl();

                var resultados = bllDV.VerificarIntegridad();
                CargarResultados(resultados);

                if (resultados.Any(r => !r.EsValido))
                    MostrarMensaje("Dígitos recalculados, pero aún se detectan errores. Revise el detalle.", "error");
                else
                    MostrarMensaje("Dígitos verificadores recalculados correctamente.", "ok");
            }
            catch (Exception ex)
            {
                MostrarMensaje("Error al recalcular: " + ex.Message, "error");
            }
        }

        protected void btnRestaurar_Click(object sender, EventArgs e)
        {
            CargarListaBackups();
            pnlRestaurar.Visible = true;
        }

        protected void btnRefrescarBackups_Click(object sender, EventArgs e)
        {
            CargarListaBackups();
        }

        protected void ddlBackups_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (!string.IsNullOrEmpty(ddlBackups.SelectedValue))
                txtRutaBackup.Text = ddlBackups.SelectedValue;
        }

        private void CargarListaBackups()
        {
            ddlBackups.Items.Clear();
            ddlBackups.Items.Add(new ListItem("— Seleccionar backup —", ""));

            string carpeta = @"C:\GymApp";
            if (Directory.Exists(carpeta))
            {
                var archivos = new List<string>();
                archivos.AddRange(Directory.GetFiles(carpeta, "*.bak"));
                archivos.AddRange(Directory.GetFiles(carpeta, "*.bacpac"));
                archivos.Sort();
                archivos.Reverse();

                foreach (string a in archivos)
                    ddlBackups.Items.Add(new ListItem(Path.GetFileName(a), a));

                if (archivos.Count == 0)
                    ddlBackups.Items.Add(new ListItem("(No se encontraron backups en C:\\GymApp\\)", ""));
            }
            else
            {
                ddlBackups.Items.Add(new ListItem("(La carpeta C:\\GymApp\\ no existe)", ""));
            }
        }

        protected void btnCancelarRestaurar_Click(object sender, EventArgs e)
        {
            pnlRestaurar.Visible = false;
            txtRutaBackup.Text = string.Empty;
        }

        protected void btnConfirmarRestaurar_Click(object sender, EventArgs e)
        {
            try
            {
                string ruta = txtRutaBackup.Text.Trim();
                if (string.IsNullOrEmpty(ruta))
                {
                    MostrarMensaje("Debe ingresar la ruta del backup.", "error");
                    return;
                }

                bllDV.RestaurarBackup(ruta);
                MostrarMensaje("Backup restaurado correctamente. Reinicie la aplicación.", "ok");
            }
            catch (Exception ex)
            {
                MostrarMensaje("Error al restaurar backup: " + ex.Message, "error");
            }
            finally
            {
                pnlRestaurar.Visible = false;
            }
        }

        protected void btnInicializar_Click(object sender, EventArgs e)
        {
            try
            {
                bllDV.InicializarControl();
                CargarEstadoControl();

                var tablasSinControl = bllDV.ObtenerTablasSinControl();
                pnlInicializar.Visible = tablasSinControl.Count > 0;

                var resultados = bllDV.VerificarIntegridad();
                CargarResultados(resultados);
                MostrarMensaje("Control de integridad inicializado correctamente.", "ok");
            }
            catch (Exception ex)
            {
                MostrarMensaje("Error al inicializar control: " + ex.Message, "error");
            }
        }

        protected void gvEstadoControl_RowDataBound(object sender, System.Web.UI.WebControls.GridViewRowEventArgs e)
        {
            if (e.Row.RowType == System.Web.UI.WebControls.DataControlRowType.DataRow)
            {
                EstadoControlDV estado = e.Row.DataItem as EstadoControlDV;
                if (estado != null && (!estado.TieneControl || estado.FilasDVHVacio > 0))
                {
                    e.Row.CssClass = "dv-grid-row dv-fila-alerta";
                }
            }
        }

        protected void btnSalir_Click(object sender, EventArgs e)
        {
            try
            {
                if (bllDV.ExisteErrorIntegridad())
                {
                    MostrarMensaje("Debe resolver el problema de integridad antes de salir. Recalcule los dígitos verificadores o restaure un backup.", "error");
                    return;
                }
            }
            catch
            {
                // Si la verificación falla (ej. tras un restore que reinició la BD),
                // se permite el logout ya que el estado no es determinable.
            }

            try
            {
                var sesion = Singleton.Instancia;
                if (sesion != null && sesion.IsLogged())
                    sesion.LogOut();
            }
            catch
            {
                // No impedir la redirección si el logout falla.
            }

            RedirigirSeguro("~/LogIn/LogIn.aspx");
        }

        protected void btnLogoutBloqueo_Click(object sender, EventArgs e)
        {
            try
            {
                var sesion = Singleton.Instancia;
                if (sesion != null)
                    sesion.LogOut();
            }
            catch
            {
                // No impedir la redirección si el logout falla.
            }

            RedirigirSeguro("~/LogIn/LogIn.aspx");
        }

        /// <summary>
        /// Muestra un mensaje en el panel de administrador.
        /// </summary>
        private void MostrarMensaje(string mensaje, string tipo)
        {
            lblMensaje.Text = mensaje;
            pnlMensaje.CssClass = "dv-mensaje dv-mensaje-" + tipo;
            pnlMensaje.Visible = true;
        }
    }
}
