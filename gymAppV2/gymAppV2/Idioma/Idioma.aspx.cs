using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.Script.Serialization;
using BE;
using BLL;
using Servicios.Singleton;
using SERVICIOS.Observer;

namespace gymAppV2.Idioma
{
    public partial class IdiomaPage : BasePage
    {
        private const string VS_MODO         = "GI_Modo";
        private const string VS_TARGET_ID    = "GI_TargetId";
        private const string VS_NOMBRE_NUEVO = "GI_NombreNuevo";
        private const string VS_CODIGO_NUEVO = "GI_CodigoNuevo";

        // Valores temporales del formulario cuando el guardado falla por campos vacíos.
        // Se usa en OnPreRender para repopular el editor sin perder lo que el usuario escribió.
        private Dictionary<string, Dictionary<string, string>> _valoresTemporales;

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                ActualizarVista();
                InicializarGestion();

                var msg = Session["IdiomaCambiadoMsg"] as string;
                if (msg != null)
                {
                    MostrarExito(msg);
                    Session.Remove("IdiomaCambiadoMsg");
                }
            }
        }

        protected override void OnPreRender(EventArgs e)
        {
            base.OnPreRender(e);
            // litTablas no usa ViewState (es un Literal), por lo que debe regenerarse
            // en cada postback cuando el editor está visible.
            if (pnlEditor.Visible)
                CargarEditor();
        }

        public override void OnIdiomaChanged(IdiomaApp idioma)
        {
            base.OnIdiomaChanged(idioma);
            litTitulo.Text    = T("idioma_titulo");
            litSubtitulo.Text = T("idioma_subtitulo");
            ActualizarBadges(idioma);
        }

        // ─── Selector de idioma activo ───────────────────────────────────────────

        private void ActualizarVista()
        {
            litTitulo.Text    = T("idioma_titulo");
            litSubtitulo.Text = T("idioma_subtitulo");
            ActualizarBadges(GestorIdioma.IdiomaActual);
        }

        private void ActualizarBadges(IdiomaApp idioma)
        {
            string textoActivo = T("idioma_activo");
            litActivoES.Text = textoActivo;
            litActivoEN.Text = textoActivo;
            litActivoPT.Text = textoActivo;
            litActivoFR.Text = textoActivo;
            litActivoJA.Text = textoActivo;

            badgeES.Visible = (idioma == IdiomaApp.ES);
            badgeEN.Visible = (idioma == IdiomaApp.EN);
            badgePT.Visible = (idioma == IdiomaApp.PT);
            badgeFR.Visible = (idioma == IdiomaApp.FR);
            badgeJA.Visible = (idioma == IdiomaApp.JA);

            btnES.CssClass = "idioma-card" + (idioma == IdiomaApp.ES ? " idioma-card-activa" : "");
            btnEN.CssClass = "idioma-card" + (idioma == IdiomaApp.EN ? " idioma-card-activa" : "");
            btnPT.CssClass = "idioma-card" + (idioma == IdiomaApp.PT ? " idioma-card-activa" : "");
            btnFR.CssClass = "idioma-card" + (idioma == IdiomaApp.FR ? " idioma-card-activa" : "");
            btnJA.CssClass = "idioma-card" + (idioma == IdiomaApp.JA ? " idioma-card-activa" : "");
        }

        protected void BtnES_Click(object sender, EventArgs e) => CambiarY(IdiomaApp.ES);
        protected void BtnEN_Click(object sender, EventArgs e) => CambiarY(IdiomaApp.EN);
        protected void BtnPT_Click(object sender, EventArgs e) => CambiarY(IdiomaApp.PT);
        protected void BtnFR_Click(object sender, EventArgs e) => CambiarY(IdiomaApp.FR);
        protected void BtnJA_Click(object sender, EventArgs e) => CambiarY(IdiomaApp.JA);

        private void CambiarY(IdiomaApp idioma)
        {
            GestorIdioma.CambiarIdioma(idioma);

            try
            {
                string usr = Singleton.Instancia.Usuario?.USUARIO_Usuario;
                if (!string.IsNullOrEmpty(usr))
                    new BLLUsuario().GuardarIdioma(usr, idioma.ToString());
            }
            catch { }

            Session["IdiomaCambiadoMsg"] = T("idioma_guardado");
            RedirigirSeguro(Request.RawUrl);
        }

        // ─── Gestión de idiomas ──────────────────────────────────────────────────

        private void InicializarGestion()
        {
            if (!BllRol.UsuarioActualEsAdmin())
            {
                pnlGestion.Visible = false;
                return;
            }
            pnlGestion.Visible = true;
            CargarDropdownEditar();
        }

        private void CargarDropdownEditar()
        {
            var idiomas = new BLLGestionIdiomas().ObtenerIdiomas();
            ddlEditarIdioma.DataSource     = idiomas;
            ddlEditarIdioma.DataTextField  = "NombreIdioma";
            ddlEditarIdioma.DataValueField = "IdiomaID";
            ddlEditarIdioma.DataBind();
        }

        protected void BtnIniciarNuevo_Click(object sender, EventArgs e)
        {
            string nombre = txtNombreIdioma.Text.Trim();
            string codigo = txtCodigoIdioma.Text.Trim().ToUpper();

            if (string.IsNullOrEmpty(nombre) || string.IsNullOrEmpty(codigo))
            {
                MostrarError("Ingresá nombre y código del idioma.");
                return;
            }
            if (new BLLGestionIdiomas().ExisteIdioma(codigo, nombre))
            {
                MostrarError("Ya existe un idioma con ese nombre o código.");
                return;
            }

            ViewState[VS_MODO]         = "nuevo";
            ViewState[VS_NOMBRE_NUEVO] = nombre;
            ViewState[VS_CODIGO_NUEVO] = codigo;
            ViewState[VS_TARGET_ID]    = -1;
            pnlEditor.Visible = true;
        }

        protected void BtnEditarIdioma_Click(object sender, EventArgs e)
        {
            if (!int.TryParse(ddlEditarIdioma.SelectedValue, out int id) || id <= 0)
            {
                MostrarError("Seleccioná un idioma para editar.");
                return;
            }
            ViewState[VS_MODO]      = "editar";
            ViewState[VS_TARGET_ID] = id;
            pnlEditor.Visible = true;
        }

        protected void BtnGuardarIdioma_Click(object sender, EventArgs e)
        {
            var bll            = new BLLGestionIdiomas();
            var valoresPorTabla = bll.RecolectarValoresFormulario(Request.Form);

            bool hayVacios = valoresPorTabla.Values
                .Any(tabla => tabla.Values.Any(v => string.IsNullOrWhiteSpace(v)));

            if (hayVacios)
            {
                MostrarError("Hay campos sin completar. Revisá todas las secciones antes de guardar.");
                _valoresTemporales = valoresPorTabla;
                return;
            }

            string modo = ViewState[VS_MODO] as string;
            try
            {
                if (modo == "nuevo")
                {
                    string nombre = ViewState[VS_NOMBRE_NUEVO] as string;
                    string codigo = ViewState[VS_CODIGO_NUEVO] as string;
                    bll.CrearIdiomaConTraducciones(codigo, nombre, valoresPorTabla);
                    MostrarExito($"Idioma '{nombre}' creado correctamente.");

                    ViewState[VS_MODO]      = null;
                    ViewState[VS_TARGET_ID] = null;
                    pnlEditor.Visible = false;
                    CargarDropdownEditar();
                }
                else
                {
                    int targetId = (int)(ViewState[VS_TARGET_ID] ?? 0);
                    bll.ActualizarTraducciones(targetId, valoresPorTabla);
                    MostrarExito("Traducciones actualizadas correctamente.");
                }
            }
            catch (Exception ex)
            {
                MostrarError("Error al guardar: " + ex.Message);
            }
        }

        protected void BtnCancelarEditor_Click(object sender, EventArgs e)
        {
            pnlEditor.Visible  = false;
            ViewState[VS_MODO] = null;
        }

        // ─── Generación del editor ───────────────────────────────────────────────

        private void CargarEditor()
        {
            string modo     = ViewState[VS_MODO] as string;
            int    targetId = ViewState[VS_TARGET_ID] != null ? (int)ViewState[VS_TARGET_ID] : -1;

            var bll    = new BLLGestionIdiomas();
            var idiomas = bll.ObtenerIdiomas();

            // Determinar valores del idioma destino
            Dictionary<string, string> targetValues;
            if (_valoresTemporales != null)
            {
                targetValues = _valoresTemporales
                    .SelectMany(kv => kv.Value)
                    .ToDictionary(kv => kv.Key, kv => kv.Value);
            }
            else if (targetId > 0)
            {
                targetValues = bll.ObtenerTodosLosValores(targetId);
            }
            else
            {
                targetValues = new Dictionary<string, string>();
            }

            // Secciones con sus columnas
            var secciones = bll.ObtenerSecciones();
            foreach (var sec in secciones)
                foreach (var tag in sec.Tags)
                    tag.ValorTarget = targetValues.TryGetValue(tag.Tag, out var v) ? v : string.Empty;

            // Etiqueta del editor
            if (modo == "nuevo")
            {
                string n = HttpUtility.HtmlEncode(ViewState[VS_NOMBRE_NUEVO] as string);
                string c = HttpUtility.HtmlEncode(ViewState[VS_CODIGO_NUEVO] as string);
                litEditorNombre.Text = $"Nuevo idioma: <strong>{n}</strong> ({c})";
            }
            else
            {
                var info = idiomas.FirstOrDefault(i => i.IdiomaID == targetId);
                string n = HttpUtility.HtmlEncode(info?.NombreIdioma ?? targetId.ToString());
                litEditorNombre.Text = $"Editando: <strong>{n}</strong>";
            }

            // Opciones del selector de referencia
            var refSb = new StringBuilder();
            foreach (var idioma in idiomas)
                refSb.AppendFormat("<option value=\"{0}\">{1}</option>",
                    idioma.IdiomaID, HttpUtility.HtmlEncode(idioma.NombreIdioma));
            litRefOptions.Text = refSb.ToString();

            // HTML del acordeón
            var sb = new StringBuilder();
            sb.Append("<div class=\"gi-accordion\">");

            for (int i = 0; i < secciones.Count; i++)
            {
                var sec      = secciones[i];
                int completos = sec.Tags.Count(t => !string.IsNullOrEmpty(t.ValorTarget));
                int total     = sec.Tags.Count;
                string pClass = completos == total && total > 0 ? "gi-prog-ok"
                              : completos > 0                   ? "gi-prog-partial"
                                                                 : "gi-prog-empty";

                sb.AppendFormat("<div class=\"gi-section\" id=\"gi-sec-{0}\">", i);
                sb.AppendFormat("<div class=\"gi-sec-header\" onclick=\"giToggle({0})\">", i);
                sb.AppendFormat("<span class=\"gi-sec-nombre\">{0}</span>",
                    HttpUtility.HtmlEncode(FormatearNombreTabla(sec.NombreTabla)));
                sb.AppendFormat("<span class=\"gi-sec-progress {0}\" id=\"gi-prog-{1}\">{2}/{3}</span>",
                    pClass, i, completos, total);
                sb.Append("<span class=\"gi-sec-chevron\">&#9654;</span>");
                sb.Append("</div>");

                // Si hay errores de validación, abrir la primera sección con vacíos
                bool tieneVacios  = sec.Tags.Any(t => string.IsNullOrEmpty(t.ValorTarget)) && _valoresTemporales != null;
                string bodyDisplay = tieneVacios ? "block" : "none";

                sb.AppendFormat("<div class=\"gi-sec-body\" id=\"gi-body-{0}\" style=\"display:{1}\">", i, bodyDisplay);
                sb.Append("<table class=\"gi-table\">");
                sb.Append("<thead><tr><th>Tag</th><th>Referencia</th><th>Traducción</th></tr></thead><tbody>");

                foreach (var tag in sec.Tags)
                {
                    string safeTag = HttpUtility.HtmlEncode(tag.Tag);
                    string safeVal = HttpUtility.HtmlAttributeEncode(tag.ValorTarget ?? string.Empty);
                    bool   isEmpty = string.IsNullOrEmpty(tag.ValorTarget);
                    string iClass  = isEmpty && _valoresTemporales != null
                                   ? "gi-input gi-input-error"
                                   : "gi-input";

                    sb.Append("<tr>");
                    sb.AppendFormat("<td class=\"gi-tag\">{0}</td>", safeTag);
                    sb.AppendFormat("<td class=\"gi-ref\" data-tag=\"{0}\"></td>",
                        HttpUtility.HtmlAttributeEncode(tag.Tag));
                    sb.AppendFormat(
                        "<td><input class=\"{0}\" type=\"text\" name=\"gft_{1}\" value=\"{2}\" data-sec=\"{3}\" oninput=\"giUpdateProgress(this)\" /></td>",
                        iClass, safeTag, safeVal, i);
                    sb.Append("</tr>");
                }

                sb.Append("</tbody></table></div></div>");
            }

            sb.Append("</div>");
            litTablas.Text = sb.ToString();

            // JSON con todos los idiomas existentes para el selector de referencia client-side.
            // Claves string porque JavaScriptSerializer requiere keys string en diccionarios.
            Dictionary<string, Dictionary<string, string>> allData = bll.ObtenerDatosTodosIdiomas();
            var serializer = new JavaScriptSerializer { MaxJsonLength = int.MaxValue };
            string json    = serializer.Serialize(allData);

            litRefData.Text = $@"<script>
var giRefData = {json};
(function() {{
    giCambiarRef();
    giUpdateAllProgress();
}})();
</script>";
        }

        private static string FormatearNombreTabla(string tabla)
        {
            switch (tabla)
            {
                case "Pantalla_Login":            return "Login";
                case "Pantalla_Idioma":           return "Idioma";
                case "Pantalla_DashBoard":        return "Dashboard — Menú";
                case "Pantalla_DashboardContent": return "Dashboard — Contenido";
                case "Pantalla_Usuarios":         return "Usuarios";
                case "Pantalla_Alumnos":          return "Alumnos";
                case "Pantalla_Actividades":      return "Actividades";
                case "Pantalla_Rutinas":          return "Rutinas";
                case "Pantalla_CambiarContra":    return "Cambiar Contraseña";
                case "Comunes_Botones":           return "Comunes — Botones";
                case "Comunes_Mensajes":          return "Comunes — Mensajes";
                default:
                    return tabla.Replace("Pantalla_", "").Replace("Comunes_", "Comunes — ").Replace("_", " ");
            }
        }
    }
}
