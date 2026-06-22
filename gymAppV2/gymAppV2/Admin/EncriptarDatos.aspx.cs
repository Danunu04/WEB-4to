using System;
using System.Collections.Generic;
using System.Web.UI;
using BLL;
using gymAppV2;
using SERVICIOS;

namespace gymAppV2.Admin
{
    public partial class EncriptarDatos : BasePage
    {
        private BLLCriptoMigracion _bllMigracion;

        private BLLCriptoMigracion BllMigracion
        {
            get
            {
                if (_bllMigracion == null)
                    _bllMigracion = new BLLCriptoMigracion();
                return _bllMigracion;
            }
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            // Solo administradores pueden ejecutar migración de encriptación
            VerificarAcceso("GestionUsuarios");

            if (!IsPostBack)
            {
                pnlResultado.Visible = false;
                pnlError.Visible = false;
            }
        }

        protected void btnEncriptarTodo_Click(object sender, EventArgs e)
        {
            try
            {
                List<CriptoMigracion.ResultadoMigracion> resultados = BllMigracion.EncriptarTodo();

                gvResultados.DataSource = resultados;
                gvResultados.DataBind();

                pnlResultado.Visible = true;
                pnlError.Visible = false;
            }
            catch (Exception ex)
            {
                lblError.Text = "Error al ejecutar la migración: " + ex.Message;
                pnlError.Visible = true;
                pnlResultado.Visible = false;
            }
        }
    }
}
