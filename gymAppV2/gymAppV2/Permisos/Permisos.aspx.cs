using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.UI.WebControls;
using BE;
using BLL;
using gymAppV2;

namespace gymAppV2.Permisos
{
    public partial class Permisos : BasePage
    {
        // Columnas de la matriz: rol -> ID del CheckBox en cada fila
        private static readonly Dictionary<int, string> CheckPorRol = new Dictionary<int, string>
        {
            { PerfilesSistema.RolAdministrador, "chkRol1" },
            { PerfilesSistema.RolRecepcionista, "chkRol2" },
            { PerfilesSistema.RolEntrenador, "chkRol3" },
            { PerfilesSistema.RolCliente, "chkRol4" },
            { PerfilesSistema.RolFamiliar, "chkRol6" }
        };

        // Matriz usada en el DataBind de las filas
        private Dictionary<int, HashSet<string>> matrizActual;

        protected void Page_Load(object sender, EventArgs e)
        {
            VerificarAcceso(PermisosSistema.GestionPermisos);

            if (!IsPostBack)
            {
                AplicarIdioma();
                CargarMatriz(null);
            }
        }

        public override void OnIdiomaChanged(IdiomaApp idioma)
        {
            base.OnIdiomaChanged(idioma);
            AplicarIdioma();
            CargarMatriz(null);
        }

        private void AplicarIdioma()
        {
            litTitulo.Text = T("permisos_titulo");
            litSubtitulo.Text = T("permisos_subtitulo");
            litAyuda.Text = Server.HtmlEncode(T("permisos_ayuda"));
            litColPermiso.Text = T("permisos_col_permiso");
            litSinTablaTitulo.Text = T("permisos_sin_tabla_titulo");
            litSinTablaMsg.Text = T("permisos_sin_tabla_msg");
            btnGuardar.Text = T("btn_guardar");
            btnRestablecer.Text = T("permisos_btn_restablecer");
        }

        /// <summary>
        /// Muestra la matriz guardada (o la indicada, ej. los valores por defecto al restablecer).
        /// </summary>
        private void CargarMatriz(Dictionary<int, HashSet<string>> matriz)
        {
            var bllRol = new BLLRol();

            bool tablaDisponible;
            try
            {
                tablaDisponible = bllRol.TablaPermisosDisponible();
            }
            catch (Exception)
            {
                MostrarError(T("msg_error_generico"));
                return;
            }

            pnlSinTabla.Visible = !tablaDisponible;
            pnlMatriz.Visible = tablaDisponible;
            if (!tablaDisponible)
                return;

            try
            {
                matrizActual = matriz ?? bllRol.ObtenerMatrizPermisos();

                rptPermisos.DataSource = PermisosSistema.Todos.Select(p => new
                {
                    Clave = p,
                    Nombre = T("permisos_nombre_" + p.ToLowerInvariant())
                }).ToList();
                rptPermisos.DataBind();
            }
            catch (Exception)
            {
                MostrarError(T("msg_error_generico"));
            }
        }

        protected void rptPermisos_ItemDataBound(object sender, RepeaterItemEventArgs e)
        {
            if (e.Item.ItemType != ListItemType.Item && e.Item.ItemType != ListItemType.AlternatingItem) return;

            string permiso = ((HiddenField)e.Item.FindControl("hdnPermiso")).Value;

            foreach (var columna in CheckPorRol)
            {
                var chk = (CheckBox)e.Item.FindControl(columna.Value);
                chk.Checked = matrizActual.TryGetValue(columna.Key, out HashSet<string> permisos) && permisos.Contains(permiso);

                // El Administrador no puede perder estos permisos (evita quedar sin acceso a esta pantalla)
                if (columna.Key == PerfilesSistema.RolAdministrador && BLLRol.PermisosFijosAdministrador.Contains(permiso))
                {
                    chk.Checked = true;
                    chk.Enabled = false;
                }
            }
        }

        protected void btnGuardar_Click(object sender, EventArgs e)
        {
            try
            {
                var matriz = BLLRol.RolesEditables.ToDictionary(r => r, r => new HashSet<string>(StringComparer.OrdinalIgnoreCase));

                foreach (RepeaterItem item in rptPermisos.Items)
                {
                    string permiso = ((HiddenField)item.FindControl("hdnPermiso")).Value;
                    foreach (var columna in CheckPorRol)
                    {
                        if (((CheckBox)item.FindControl(columna.Value)).Checked)
                            matriz[columna.Key].Add(permiso);
                    }
                }

                new BLLRol().GuardarMatrizPermisos(matriz);

                MostrarExito(T("permisos_msg_guardado"));
                CargarMatriz(null);
            }
            catch (Exception)
            {
                MostrarError(T("msg_error_generico"));
            }
        }

        /// <summary>
        /// Carga en pantalla los permisos por defecto. No se guardan hasta apretar Guardar.
        /// </summary>
        protected void btnRestablecer_Click(object sender, EventArgs e)
        {
            var porDefecto = BLLRol.RolesEditables.ToDictionary(
                rol => rol,
                rol => new HashSet<string>(PermisosSistema.Todos.Where(p => BLLRol.PermisoPorDefecto(rol, p)), StringComparer.OrdinalIgnoreCase));

            CargarMatriz(porDefecto);
            MostrarInfo(T("permisos_msg_restablecido"));
        }

        private void MostrarInfo(string mensaje)
        {
            MostrarToast(mensaje, "info");
        }
    }
}
