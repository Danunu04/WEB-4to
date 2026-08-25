using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using BLL;
using BE;
using gymAppV2;
using Newtonsoft.Json;
using Servicios.Singleton;

namespace gymAppV2.Actividades
{
    public partial class actividades : BasePage
    {
        private BLLActividad bllActividad;

        /// <summary>
        /// Indica si el usuario logueado es un Cliente (rol 4).
        /// Expuesto al front-end para limitar acciones y filtrar contenido.
        /// </summary>
        protected bool EsCliente
        {
            get { return Singleton.Instancia.Usuario?.USUARIO_Rol == 4; }
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            VerificarAcceso(BE.PermisosSistema.ActividadesCalendario);

            bllActividad = new BLLActividad();

            if (!IsPostBack)
            {
                AplicarIdioma();

                // Los Clientes no pueden crear actividades.
                pnlNuevaActividad.Visible = !EsCliente;

                // Exponer al front-end si el usuario es Cliente para filtrar clases inscritas.
                hdnEsCliente.Value = EsCliente ? "1" : "0";

                CargarActividades();
            }
        }

        public override void OnIdiomaChanged(IdiomaApp idioma)
        {
            base.OnIdiomaChanged(idioma);
            AplicarIdioma();
        }

        private void AplicarIdioma()
        {
            litTitulo.Text      = T("actividades_titulo");
            litBtnNueva.Text    = T("actividades_btn_nueva");
            litClienteInfo.Text = T("actividades_cliente_info");
            litModalTitulo.Text = T("actividades_modal_titulo");
        }

        /// <summary>
        /// Carga las actividades desde la capa de negocio y las serializa al front-end.
        /// Para los clientes solo se muestran las actividades en las que están inscriptos
        /// sus alumnos asociados (filtro por Actividad_Alumno).
        /// </summary>
        private void CargarActividades()
        {
            try
            {
                List<Actividad> actividades;

                if (EsCliente)
                {
                    string usuarioActual = Singleton.Instancia.Usuario?.USUARIO_Usuario;
                    actividades = bllActividad.ListarActividadesPorCliente(usuarioActual);
                }
                else
                {
                    actividades = bllActividad.ListarActividades();
                }

                hdnActividadesJson.Value = GenerarActividadesJson(actividades);
            }
            catch (Exception ex)
            {
                hdnActividadesJson.Value = "{}";
                MostrarError("Error al cargar actividades: " + ex.Message);
            }
        }

        /// <summary>
        /// Convierte la lista de actividades en el formato esperado por el calendario del front-end.
        /// Como el esquema actual no almacena horarios ni días fijos para las actividades,
        /// se distribuyen de forma determinística para que el calendario las visualice.
        /// </summary>
        private string GenerarActividadesJson(List<Actividad> actividades)
        {
            var colores = new[] { "pink", "mint", "lavender", "peach", "sky" };
            var horarios = new[] { "08:00", "10:00", "12:00", "16:00", "18:00", "20:00" };

            var actividadesPorDia = new Dictionary<int, List<object>>();

            foreach (var actividad in actividades)
            {
                // Distribución determinística: día 1-28 según el código de actividad.
                int dia = (actividad.CodActividad % 28) + 1;
                string horario = horarios[actividad.CodActividad % horarios.Length];
                string color = colores[actividad.CodActividad % colores.Length];

                if (!actividadesPorDia.ContainsKey(dia))
                {
                    actividadesPorDia[dia] = new List<object>();
                }

                actividadesPorDia[dia].Add(new
                {
                    name = actividad.Descripcion,
                    time = horario,
                    color = color,
                    instructor = "Sin instructor"
                });
            }

            return JsonConvert.SerializeObject(actividadesPorDia);
        }

        private void MostrarInfo(string mensaje)
        {
            ScriptManager.RegisterStartupScript(this, GetType(), "info", $"if(window.showToast) showToast('{System.Security.SecurityElement.Escape(mensaje)}', 'info');", true);
        }
    }
}