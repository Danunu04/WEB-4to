using System;
using BE;

namespace gymAppV2.DashBoard
{
    public partial class WebForm1 : BasePage
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            VerificarAcceso(PermisosSistema.Dashboard);

            if (!IsPostBack)
                AplicarIdioma();
        }

        public override void OnIdiomaChanged(IdiomaApp idioma)
        {
            base.OnIdiomaChanged(idioma);
            AplicarIdioma();
        }

        private void AplicarIdioma()
        {
            litTitulo.Text       = T("dash_titulo");
            litSubtitulo.Text    = T("dash_bienvenido");
            litKpiMiembros.Text  = T("dash_kpi_miembros");
            litKpiClases.Text    = T("dash_kpi_clases");
            litKpiIngresos.Text  = T("dash_kpi_ingresos");
            litKpiRetencion.Text = T("dash_kpi_retencion");
            litSemanaTitulo.Text = T("dash_semana_titulo");
            litColActividad.Text = T("dash_col_actividad");
            litColInstructor.Text = T("dash_col_instructor");
            litColDia.Text       = T("dash_col_dia");
            litColHorario.Text   = T("dash_col_horario");
            litColDuracion.Text  = T("dash_col_duracion");
            litColEstado.Text    = T("dash_col_estado");
        }
    }
}
