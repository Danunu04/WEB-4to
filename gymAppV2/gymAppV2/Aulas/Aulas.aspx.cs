using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Web.UI.WebControls;
using BE;
using BLL;
using gymAppV2;

namespace gymAppV2.Aulas
{
    /// <summary>
    /// Aulas del gimnasio: cada gimnasio carga las suyas con su cupo de personas.
    /// </summary>
    public partial class Aulas : BasePage
    {
        private BLLAula bllAula;

        // Aula en edición (0 = alta)
        private int CodAulaEdicion
        {
            get { return ViewState["CodAulaEdicion"] as int? ?? 0; }
            set { ViewState["CodAulaEdicion"] = value; }
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            VerificarAcceso(PermisosSistema.GestionAulas);

            bllAula = new BLLAula();

            if (!IsPostBack)
            {
                AplicarIdioma();
                CargarAulas();
            }
        }

        public override void OnIdiomaChanged(IdiomaApp idioma)
        {
            base.OnIdiomaChanged(idioma);
            AplicarIdioma();
            CargarAulas();
        }

        private void AplicarIdioma()
        {
            litTitulo.Text = T("aulas_titulo");
            litSubtitulo.Text = T("aulas_subtitulo");
            litBtnNueva.Text = T("aulas_btn_nueva");
            btnCancelar.Text = T("actividades_btn_cancelar");
            btnGuardar.Text = T("btn_guardar");
        }

        private void CargarAulas()
        {
            try
            {
                List<Aula> aulas = bllAula.ListarAulas();
                var turnosPorAula = new BLLActividad().ListarHorariosActivos()
                    .GroupBy(h => h.CodAula)
                    .ToDictionary(g => g.Key, g => g.Count());

                rptAulas.DataSource = aulas.Select(a => new
                {
                    a.CodAula,
                    a.Nombre,
                    a.Cupo,
                    a.Activo,
                    Turnos = turnosPorAula.TryGetValue(a.CodAula, out int cantidad) ? cantidad : 0,
                    EstadoTexto = a.Activo ? T("actividades_estado_activa") : T("actividades_estado_inactiva")
                }).ToList();
                rptAulas.DataBind();

                phSinAulas.Visible = aulas.Count == 0;
            }
            catch (Exception)
            {
                MostrarError(T("msg_error_generico"));
            }
        }

        protected void btnNuevaAula_Click(object sender, EventArgs e)
        {
            CodAulaEdicion = 0;
            litFormTitulo.Text = T("aulas_form_nueva");
            txtNombre.Text = string.Empty;
            txtCupo.Text = string.Empty;
            pnlForm.Visible = true;
        }

        protected void rptAulas_ItemCommand(object source, RepeaterCommandEventArgs e)
        {
            try
            {
                int codAula = Convert.ToInt32(e.CommandArgument);

                switch (e.CommandName)
                {
                    case "Editar":
                        Aula aula = bllAula.ObtenerAula(codAula);
                        if (aula == null)
                        {
                            MostrarError(T("msg_error_generico"));
                            return;
                        }
                        CodAulaEdicion = codAula;
                        litFormTitulo.Text = T("aulas_form_editar");
                        txtNombre.Text = aula.Nombre;
                        txtCupo.Text = aula.Cupo.ToString(CultureInfo.InvariantCulture);
                        pnlForm.Visible = true;
                        break;

                    case "Desactivar":
                    case "Activar":
                        bllAula.CambiarEstado(codAula, e.CommandName == "Activar");
                        MostrarExito(T("aulas_msg_estado"));
                        CargarAulas();
                        break;
                }
            }
            catch (InscripcionException ex)
            {
                MostrarError(ex.Message);
            }
            catch (Exception)
            {
                MostrarError(T("msg_error_generico"));
            }
        }

        protected void btnGuardar_Click(object sender, EventArgs e)
        {
            try
            {
                if (!int.TryParse((txtCupo.Text ?? string.Empty).Trim(), NumberStyles.Integer, CultureInfo.InvariantCulture, out int cupo))
                {
                    MostrarError(T("aulas_msg_cupo_invalido"));
                    return;
                }

                Aula anterior = CodAulaEdicion != 0 ? bllAula.ObtenerAula(CodAulaEdicion) : null;
                var aula = new Aula
                {
                    CodAula = CodAulaEdicion,
                    Nombre = txtNombre.Text,
                    Cupo = cupo,
                    Activo = anterior?.Activo ?? true
                };

                bllAula.GuardarAula(aula);

                pnlForm.Visible = false;
                MostrarExito(T("aulas_msg_guardada"));
                CargarAulas();
            }
            catch (InscripcionException ex)
            {
                MostrarError(ex.Message);
            }
            catch (Exception)
            {
                MostrarError(T("msg_error_generico"));
            }
        }

        protected void btnCancelar_Click(object sender, EventArgs e)
        {
            pnlForm.Visible = false;
        }
    }
}
