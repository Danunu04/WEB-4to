using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.UI;
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
        /// Carga los resultados en el grid y actualiza los contadores.
        /// </summary>
        private void CargarResultados(List<ResultadoVerificacionDV> resultados)
        {
            gvResultados.DataSource = resultados;
            gvResultados.DataBind();

            var tablas = resultados.Select(r => r.NombreTabla).Distinct().ToList();
            int errores = resultados.Count(r => !r.EsValido);
            int ok = resultados.Count(r => r.EsValido && r.NombreTabla != "Sistema" && !r.Mensaje.Contains("no tiene registro de control"));

            lblTotalTablas.Text = tablas.Count.ToString();
            lblTotalErrores.Text = errores.ToString();
            lblTablasOk.Text = ok.ToString();
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
            pnlRestaurar.Visible = true;
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
                if (estado != null && (!estado.TieneControl || estado.FilasDVHVacio > 0 || estado.FilasDVVVacio > 0))
                {
                    e.Row.CssClass = "dv-grid-row dv-fila-alerta";
                }
            }
        }

        protected void btnSalir_Click(object sender, EventArgs e)
        {
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
