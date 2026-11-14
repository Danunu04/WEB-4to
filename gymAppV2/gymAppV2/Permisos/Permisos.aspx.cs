using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.UI.WebControls;
using BE;
using BLL;
using SERVICIOS.Composite;
using Servicios.Singleton;
using PerfilAcceso = SERVICIOS.Composite.Perfil;

namespace gymAppV2.Permisos
{
    /// <summary>
    /// Permisos con el patrón Composite: perfiles, familias de permisos y el perfil de cada usuario.
    /// El editor arma el perfil o la familia en memoria y avisa apenas algo no se puede agregar
    /// (permiso repetido, ciclo); las reglas se vuelven a validar en la BLL al guardar.
    /// </summary>
    public partial class Permisos : BasePage
    {
        private const string TAB_PERFILES = "perfiles";
        private const string TAB_FAMILIAS = "familias";
        private const string TAB_USUARIOS = "usuarios";

        private const string EDITOR_PERFIL = "perfil";
        private const string EDITOR_FAMILIA = "familia";

        private BLLPermiso bllPermiso;

        // Datos para armar los desplegables de la pestaña de usuarios (rptUsuarios_ItemDataBound)
        private List<PerfilAcceso> perfilesParaAsignar;
        private bool actorEsWebMaster;

        /// <summary>
        /// Fila de la pestaña de usuarios.
        /// </summary>
        private class FilaUsuario
        {
            public string Usuario { get; set; }
            public string NombreCompleto { get; set; }
            public string Tipo { get; set; }
            public bool Activo { get; set; }
            public string Estado { get; set; }
            public int? IdPerfil { get; set; }
        }

        private BLLPermiso BllPermiso
        {
            get { return bllPermiso ?? (bllPermiso = new BLLPermiso()); }
        }

        private string Pestania
        {
            get { return ViewState["Pestania"] as string ?? TAB_PERFILES; }
            set { ViewState["Pestania"] = value; }
        }

        private string EditorTipo
        {
            get { return ViewState["EditorTipo"] as string ?? EDITOR_PERFIL; }
            set { ViewState["EditorTipo"] = value; }
        }

        // Id del perfil o familia en edición (0 = alta)
        private int EditorId
        {
            get { return ViewState["EditorId"] as int? ?? 0; }
            set { ViewState["EditorId"] = value; }
        }

        private List<int> EditorPermisos
        {
            get { return ViewState["EditorPermisos"] as List<int> ?? new List<int>(); }
            set { ViewState["EditorPermisos"] = value; }
        }

        private List<int> EditorFamilias
        {
            get { return ViewState["EditorFamilias"] as List<int> ?? new List<int>(); }
            set { ViewState["EditorFamilias"] = value; }
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            VerificarAcceso(PermisosSistema.GestionPermisos);
            if (!BllRol.UsuarioActualTieneAcceso(PermisosSistema.GestionPermisos))
            {
                // VerificarAcceso ya redirige: no armar nada de la pantalla
                pnlContenido.Visible = false;
                pnlEditor.Visible = false;
                return;
            }

            if (!IsPostBack)
            {
                AplicarIdioma();
                CargarPantalla();
            }
        }

        public override void OnIdiomaChanged(IdiomaApp idioma)
        {
            base.OnIdiomaChanged(idioma);
            if (!BllRol.UsuarioActualTieneAcceso(PermisosSistema.GestionPermisos))
                return;

            AplicarIdioma();
            CargarPantalla();

            if (pnlEditor.Visible)
            {
                ActualizarTituloEditor();
                RefrescarEditor();
            }
        }

        private void AplicarIdioma()
        {
            litTitulo.Text = T("permisos_titulo");
            litSubtitulo.Text = T("permisos_subtitulo");
            litSinTablasTitulo.Text = T("permisos_sin_tabla_titulo");
            litSinTablasMsg.Text = HttpUtility.HtmlEncode(T("permisos_sin_tabla_msg"));
            // El texto va en el Text del LinkButton y no en un Literal hijo: en un PostBack el
            // LinkButton descarta sus controles hijos si su ViewState cambió (CssClass de la pestaña activa).
            btnTabPerfiles.Text = TextoPestania("fa-id-badge", T("permisos_tab_perfiles"));
            btnTabFamilias.Text = TextoPestania("fa-folder-tree", T("permisos_tab_familias"));
            btnTabUsuarios.Text = TextoPestania("fa-users", T("permisos_tab_usuarios"));
            litAyudaPerfiles.Text = HttpUtility.HtmlEncode(T("permisos_ayuda_perfiles"));
            litAyudaFamilias.Text = HttpUtility.HtmlEncode(T("permisos_ayuda_familias"));
            litAyudaUsuarios.Text = HttpUtility.HtmlEncode(T("permisos_ayuda_usuarios"));
            litBtnNuevoPerfil.Text = T("permisos_btn_nuevo_perfil");
            litBtnNuevaFamilia.Text = T("permisos_btn_nueva_familia");
            txtBuscarUsuario.Attributes["placeholder"] = T("permisos_buscar_usuario");
            btnBuscarUsuario.ToolTip = T("permisos_buscar_usuario");
            btnBuscarUsuario.Attributes["aria-label"] = T("permisos_buscar_usuario");
            btnCancelarEditor.Text = T("permisos_btn_cancelar");
            btnGuardarEditor.Text = T("btn_guardar");
        }

        #region Carga de la pantalla

        private void CargarPantalla()
        {
            bool tablasDisponibles;
            try
            {
                tablasDisponibles = BllPermiso.TablasDisponibles();
            }
            catch (Exception)
            {
                MostrarError(T("msg_error_generico"));
                tablasDisponibles = false;
            }

            pnlSinTablas.Visible = !tablasDisponibles;
            pnlContenido.Visible = tablasDisponibles;
            if (!tablasDisponibles)
            {
                pnlEditor.Visible = false;
                return;
            }

            MostrarPestania(Pestania);
            CargarPerfiles();
            CargarFamilias();
            CargarUsuarios();
        }

        private void CargarPerfiles()
        {
            try
            {
                List<PerfilAcceso> perfiles = BllPermiso.ListarPerfiles();
                Dictionary<string, string> nombres = new BLLUsuario().ListarUsuarios()
                    .GroupBy(u => u.USUARIO_Usuario, StringComparer.OrdinalIgnoreCase)
                    .ToDictionary(g => g.Key, g => $"{g.First().Nombre} {g.First().Apellido}".Trim(), StringComparer.OrdinalIgnoreCase);
                Dictionary<int, List<string>> usuariosPorPerfil = BllPermiso.ObtenerPerfilesDeUsuarios()
                    .GroupBy(u => u.Value)
                    .ToDictionary(g => g.Key, g => g.Select(u => u.Key).OrderBy(u => u, StringComparer.OrdinalIgnoreCase).ToList());

                rptPerfiles.DataSource = perfiles.Select(p => new
                {
                    p.Id,
                    Nombre = NombreVisible(p),
                    EsSistema = p.Codigo != null,
                    EtiquetaUsuarios = RenderEtiquetaUsuarios(usuariosPorPerfil.TryGetValue(p.Id, out List<string> usuarios) ? usuarios : new List<string>(), nombres),
                    Editable = true,
                    Eliminable = p.Codigo == null,
                    Arbol = RenderArbol(p.ObtenerComponentes())
                }).ToList();
                rptPerfiles.DataBind();

                phSinPerfiles.Visible = perfiles.Count == 0;
            }
            catch (Exception)
            {
                MostrarError(T("msg_error_generico"));
            }
        }

        private void CargarFamilias()
        {
            try
            {
                List<Familia> familias = BllPermiso.ListarFamilias();

                rptFamilias.DataSource = familias.Select(f =>
                {
                    List<object> usos = BllPermiso.ObtenerUsosDeFamilia(f.Id);
                    return new
                    {
                        f.Id,
                        Nombre = NombreVisible(f),
                        EsSistema = f.Codigo != null,
                        TextoUsos = usos.Count == 0 ? T("permisos_sin_uso") : Formato("permisos_usada_en", NombreVisible(usos)),
                        Eliminable = true,
                        Arbol = RenderArbol(f.ObtenerHijos())
                    };
                }).ToList();
                rptFamilias.DataBind();

                phSinFamilias.Visible = familias.Count == 0;
            }
            catch (Exception)
            {
                MostrarError(T("msg_error_generico"));
            }
        }

        private void CargarUsuarios()
        {
            try
            {
                perfilesParaAsignar = BllPermiso.ListarPerfiles();
                actorEsWebMaster = BllPermiso.UsuarioEsWebMaster(Singleton.Instancia?.Usuario?.USUARIO_Usuario);
                Dictionary<string, int> perfilDeUsuario = BllPermiso.ObtenerPerfilesDeUsuarios();

                string filtro = (txtBuscarUsuario.Text ?? string.Empty).Trim();

                List<FilaUsuario> filas = new BLLUsuario().ListarUsuarios()
                    .Select(u => new FilaUsuario
                    {
                        Usuario = u.USUARIO_Usuario,
                        NombreCompleto = $"{u.Nombre} {u.Apellido}".Trim(),
                        Tipo = Formato("permisos_tipo_" + u.USUARIO_Rol),
                        Activo = u.USUARIO_Activo,
                        Estado = u.USUARIO_Activo ? T("permisos_estado_activo") : T("permisos_estado_inactivo"),
                        IdPerfil = perfilDeUsuario.TryGetValue(u.USUARIO_Usuario, out int idPerfil) ? idPerfil : (int?)null
                    })
                    .Where(f => filtro.Length == 0
                        || f.Usuario.IndexOf(filtro, StringComparison.OrdinalIgnoreCase) >= 0
                        || f.NombreCompleto.IndexOf(filtro, StringComparison.OrdinalIgnoreCase) >= 0)
                    .OrderBy(f => f.Usuario, StringComparer.OrdinalIgnoreCase)
                    .ToList();

                rptUsuarios.DataSource = filas;
                rptUsuarios.DataBind();

                phSinUsuarios.Visible = filas.Count == 0;
            }
            catch (Exception)
            {
                MostrarError(T("msg_error_generico"));
            }
        }

        protected void rptUsuarios_ItemDataBound(object sender, RepeaterItemEventArgs e)
        {
            if (e.Item.ItemType != ListItemType.Item && e.Item.ItemType != ListItemType.AlternatingItem) return;

            var fila = (FilaUsuario)e.Item.DataItem;
            var ddl = (DropDownList)e.Item.FindControl("ddlPerfil");
            var btnAsignar = (LinkButton)e.Item.FindControl("btnAsignar");
            ddl.Attributes["aria-label"] = T("permisos_col_perfil") + " " + fila.Usuario;

            PerfilAcceso actual = fila.IdPerfil.HasValue ? perfilesParaAsignar.FirstOrDefault(p => p.Id == fila.IdPerfil.Value) : null;

            if (actual == null)
                ddl.Items.Add(new ListItem(T("permisos_sin_perfil"), string.Empty));

            foreach (PerfilAcceso perfil in perfilesParaAsignar)
            {
                // El perfil WebMaster solo lo ofrece a otro WebMaster
                if (perfil.EsWebMaster && !actorEsWebMaster && perfil != actual)
                    continue;

                ddl.Items.Add(new ListItem(NombreVisible(perfil), perfil.Id.ToString()));
            }

            ddl.SelectedValue = actual != null ? actual.Id.ToString() : string.Empty;

            // A un WebMaster solo lo cambia otro WebMaster
            if (actual != null && actual.EsWebMaster && !actorEsWebMaster)
            {
                ddl.Enabled = false;
                btnAsignar.Visible = false;
            }
        }

        #endregion

        #region Pestañas

        protected void btnTabPerfiles_Click(object sender, EventArgs e)
        {
            MostrarPestania(TAB_PERFILES);
        }

        protected void btnTabFamilias_Click(object sender, EventArgs e)
        {
            MostrarPestania(TAB_FAMILIAS);
        }

        protected void btnTabUsuarios_Click(object sender, EventArgs e)
        {
            MostrarPestania(TAB_USUARIOS);
        }

        private void MostrarPestania(string pestania)
        {
            Pestania = pestania;

            pnlPerfiles.Visible = pestania == TAB_PERFILES;
            pnlFamilias.Visible = pestania == TAB_FAMILIAS;
            pnlUsuarios.Visible = pestania == TAB_USUARIOS;

            MarcarPestania(btnTabPerfiles, pestania == TAB_PERFILES);
            MarcarPestania(btnTabFamilias, pestania == TAB_FAMILIAS);
            MarcarPestania(btnTabUsuarios, pestania == TAB_USUARIOS);
        }

        private static string TextoPestania(string icono, string texto)
        {
            return "<i class=\"fa-solid " + icono + "\"></i> " + HttpUtility.HtmlEncode(texto);
        }

        private static void MarcarPestania(LinkButton boton, bool activa)
        {
            boton.CssClass = activa ? "perm-tab activo" : "perm-tab";
            boton.Attributes["aria-selected"] = activa ? "true" : "false";
        }

        #endregion

        #region Perfiles y familias

        protected void btnNuevoPerfil_Click(object sender, EventArgs e)
        {
            AbrirEditor(EDITOR_PERFIL, 0, string.Empty, false, new List<int>(), new List<int>());
        }

        protected void btnNuevaFamilia_Click(object sender, EventArgs e)
        {
            AbrirEditor(EDITOR_FAMILIA, 0, string.Empty, false, new List<int>(), new List<int>());
        }

        protected void rptPerfiles_ItemCommand(object source, RepeaterCommandEventArgs e)
        {
            try
            {
                int idPerfil = Convert.ToInt32(e.CommandArgument);

                switch (e.CommandName)
                {
                    case "Editar":
                        PerfilAcceso perfil = BllPermiso.ObtenerPerfil(idPerfil);
                        if (perfil == null)
                        {
                            MostrarError(T("msg_error_generico"));
                            return;
                        }

                        List<IComponente> componentes = perfil.ObtenerComponentes();
                        AbrirEditor(EDITOR_PERFIL, idPerfil,
                            perfil.Codigo != null ? NombreVisible(perfil) : perfil.Nombre,
                            perfil.Codigo != null,
                            componentes.OfType<PermisoSimple>().Select(p => p.Id).ToList(),
                            componentes.OfType<Familia>().Select(f => f.Id).ToList());
                        break;

                    case "Eliminar":
                        BllPermiso.EliminarPerfil(idPerfil);
                        MostrarExito(T("permisos_msg_perfil_eliminado"));
                        CargarPerfiles();
                        CargarUsuarios();
                        break;
                }
            }
            catch (Exception ex)
            {
                MostrarError(MensajeDePermisos(ex) ?? T("msg_error_generico"));
            }
        }

        protected void rptFamilias_ItemCommand(object source, RepeaterCommandEventArgs e)
        {
            try
            {
                int idFamilia = Convert.ToInt32(e.CommandArgument);

                switch (e.CommandName)
                {
                    case "Editar":
                        Familia familia = BllPermiso.ObtenerFamilia(idFamilia);
                        if (familia == null)
                        {
                            MostrarError(T("msg_error_generico"));
                            return;
                        }

                        List<IComponente> hijos = familia.ObtenerHijos();
                        AbrirEditor(EDITOR_FAMILIA, idFamilia,
                            familia.Codigo != null ? NombreVisible(familia) : familia.Nombre,
                            familia.Codigo != null,
                            hijos.OfType<PermisoSimple>().Select(p => p.Id).ToList(),
                            hijos.OfType<Familia>().Select(f => f.Id).ToList());
                        break;

                    case "Eliminar":
                        BllPermiso.EliminarFamilia(idFamilia);
                        MostrarExito(T("permisos_msg_familia_eliminada"));
                        CargarFamilias();
                        break;
                }
            }
            catch (Exception ex)
            {
                MostrarError(MensajeDePermisos(ex) ?? T("msg_error_generico"));
            }
        }

        protected void rptUsuarios_ItemCommand(object source, RepeaterCommandEventArgs e)
        {
            if (e.CommandName != "Asignar") return;

            try
            {
                string usuario = ((HiddenField)e.Item.FindControl("hdnUsuario")).Value;
                string valor = ((DropDownList)e.Item.FindControl("ddlPerfil")).SelectedValue;

                if (string.IsNullOrEmpty(valor))
                {
                    MostrarAdvertencia(T("permisos_elegir_perfil"));
                    return;
                }

                BllPermiso.AsignarPerfil(usuario, Convert.ToInt32(valor));
                MostrarExito(Formato("permisos_msg_perfil_asignado", usuario));
                CargarPerfiles();
                CargarUsuarios();
            }
            catch (Exception ex)
            {
                MostrarError(MensajeDePermisos(ex) ?? T("msg_error_generico"));
                CargarUsuarios();
            }
        }

        protected void btnBuscarUsuario_Click(object sender, EventArgs e)
        {
            CargarUsuarios();
        }

        #endregion

        #region Editor

        private void AbrirEditor(string tipo, int id, string nombre, bool esDelSistema, List<int> permisos, List<int> familias)
        {
            EditorTipo = tipo;
            EditorId = id;
            EditorPermisos = permisos;
            EditorFamilias = familias;

            txtNombreEditor.Text = nombre;
            txtNombreEditor.Enabled = !esDelSistema;
            phNombreBloqueado.Visible = esDelSistema;

            ActualizarTituloEditor();
            pnlEditor.Visible = true;
            RefrescarEditor();
        }

        private void ActualizarTituloEditor()
        {
            bool esAlta = EditorId == 0;
            litEditorTitulo.Text = EditorTipo == EDITOR_PERFIL
                ? T(esAlta ? "permisos_editor_nuevo_perfil" : "permisos_editor_editar_perfil")
                : T(esAlta ? "permisos_editor_nueva_familia" : "permisos_editor_editar_familia");
        }

        private void CerrarEditor()
        {
            pnlEditor.Visible = false;
            ViewState.Remove("EditorPermisos");
            ViewState.Remove("EditorFamilias");
        }

        /// <summary>
        /// Vuelve a dibujar las dos columnas del editor: lo disponible (marcando lo que ya está
        /// incluido) y el contenido actual con su árbol.
        /// </summary>
        private void RefrescarEditor()
        {
            try
            {
                List<PermisoSimple> todosLosPermisos = BllPermiso.ListarPermisos();
                List<Familia> todasLasFamilias = BllPermiso.ListarFamilias();
                Dictionary<int, PermisoSimple> permisosPorId = todosLosPermisos.ToDictionary(p => p.Id);
                Dictionary<int, Familia> familiasPorId = todasLasFamilias.ToDictionary(f => f.Id);

                // Lo que se haya borrado mientras tanto se descarta
                List<int> idsPermisos = EditorPermisos.Where(permisosPorId.ContainsKey).ToList();
                List<int> idsFamilias = EditorFamilias.Where(familiasPorId.ContainsKey).ToList();
                EditorPermisos = idsPermisos;
                EditorFamilias = idsFamilias;

                var contenido = new List<IComponente>();
                contenido.AddRange(idsPermisos.Select(id => permisosPorId[id]));
                contenido.AddRange(idsFamilias.Select(id => familiasPorId[id]));

                var permisosIncluidos = new HashSet<int>(contenido.SelectMany(Componentes.Aplanar).Select(p => p.Id));
                var familiasIncluidas = new HashSet<int>(idsFamilias);
                foreach (int id in idsFamilias)
                    familiasIncluidas.UnionWith(Subfamilias(familiasPorId[id]));

                rptDisponiblesPermisos.DataSource = todosLosPermisos.Select(p => new
                {
                    p.Id,
                    Nombre = NombreVisible(p),
                    Incluido = permisosIncluidos.Contains(p.Id)
                }).ToList();
                rptDisponiblesPermisos.DataBind();

                // Una familia no se ofrece a sí misma
                rptDisponiblesFamilias.DataSource = todasLasFamilias
                    .Where(f => !(EditorTipo == EDITOR_FAMILIA && f.Id == EditorId))
                    .Select(f => new
                    {
                        f.Id,
                        Nombre = NombreVisible(f),
                        Incluido = familiasIncluidas.Contains(f.Id),
                        Arbol = RenderArbol(f.ObtenerHijos())
                    }).ToList();
                rptDisponiblesFamilias.DataBind();

                rptContenido.DataSource = contenido.Select(c => new
                {
                    c.Id,
                    Nombre = NombreVisible(c),
                    EsFamilia = c is Familia,
                    Arbol = c is Familia familia ? RenderArbol(familia.ObtenerHijos()) : string.Empty
                }).ToList();
                rptContenido.DataBind();

                phContenidoVacio.Visible = contenido.Count == 0;
            }
            catch (Exception)
            {
                MostrarError(T("msg_error_generico"));
            }
        }

        protected void rptDisponibles_ItemCommand(object source, RepeaterCommandEventArgs e)
        {
            int id = Convert.ToInt32(e.CommandArgument);
            List<int> permisos = EditorPermisos;
            List<int> familias = EditorFamilias;

            if (e.CommandName == "AgregarPermiso")
                permisos.Add(id);
            else if (e.CommandName == "AgregarFamilia")
                familias.Add(id);
            else
                return;

            try
            {
                // Se arma en memoria con el Composite: si el permiso ya llega por otro lado,
                // o la familia formaría un ciclo, se avisa y no se agrega
                if (EditorTipo == EDITOR_PERFIL)
                    BllPermiso.ArmarPerfil(EditorId, txtNombreEditor.Text, permisos, familias);
                else
                    BllPermiso.ArmarFamilia(EditorId, txtNombreEditor.Text, permisos, familias);

                EditorPermisos = permisos;
                EditorFamilias = familias;
            }
            catch (Exception ex)
            {
                MostrarError(MensajeDePermisos(ex) ?? T("msg_error_generico"));
            }

            RefrescarEditor();
        }

        protected void rptContenido_ItemCommand(object source, RepeaterCommandEventArgs e)
        {
            int id = Convert.ToInt32(e.CommandArgument);

            if (e.CommandName == "QuitarPermiso")
                EditorPermisos = EditorPermisos.Where(p => p != id).ToList();
            else if (e.CommandName == "QuitarFamilia")
                EditorFamilias = EditorFamilias.Where(f => f != id).ToList();

            RefrescarEditor();
        }

        protected void btnGuardarEditor_Click(object sender, EventArgs e)
        {
            try
            {
                if (EditorTipo == EDITOR_PERFIL)
                {
                    BllPermiso.GuardarPerfil(EditorId, txtNombreEditor.Text, EditorPermisos, EditorFamilias);
                    MostrarExito(T("permisos_msg_perfil_guardado"));
                }
                else
                {
                    BllPermiso.GuardarFamilia(EditorId, txtNombreEditor.Text, EditorPermisos, EditorFamilias);
                    MostrarExito(T("permisos_msg_familia_guardada"));
                }

                CerrarEditor();
                CargarPerfiles();
                CargarFamilias();
                CargarUsuarios();
            }
            catch (Exception ex)
            {
                MostrarError(MensajeDePermisos(ex) ?? T("msg_error_generico"));
            }
        }

        protected void btnCancelarEditor_Click(object sender, EventArgs e)
        {
            CerrarEditor();
        }

        #endregion

        #region Auxiliares

        /// <summary>
        /// Árbol del Composite como listas anidadas. Los nombres se codifican.
        /// </summary>
        /// <summary>
        /// Etiqueta "N usuario(s)" del perfil. Si tiene usuarios, al pasar el mouse (o al hacer
        /// clic / tab) se despliega la lista con usuario y nombre, sin salir de la pantalla.
        /// </summary>
        private string RenderEtiquetaUsuarios(List<string> usuarios, Dictionary<string, string> nombres)
        {
            string texto = HttpUtility.HtmlEncode(Formato("permisos_usuarios_cantidad", usuarios.Count));
            if (usuarios.Count == 0)
                return "<span class=\"perm-etiqueta\">" + texto + "</span>";

            var html = new StringBuilder();
            html.Append("<span class=\"perm-usuarios\" tabindex=\"0\">");
            html.Append("<span class=\"perm-etiqueta perm-etiqueta-usuarios\">").Append(texto)
                .Append(" <i class=\"fa-solid fa-chevron-down\" aria-hidden=\"true\"></i></span>");
            html.Append("<span class=\"perm-usuarios-lista\" role=\"tooltip\">");
            foreach (string usuario in usuarios)
            {
                html.Append("<span class=\"perm-usuarios-item\"><span class=\"perm-usuarios-usuario\">")
                    .Append(HttpUtility.HtmlEncode(usuario)).Append("</span>");
                if (nombres.TryGetValue(usuario, out string nombre) && nombre.Length > 0)
                    html.Append("<span class=\"perm-usuarios-nombre\">").Append(HttpUtility.HtmlEncode(nombre)).Append("</span>");
                html.Append("</span>");
            }
            html.Append("</span></span>");
            return html.ToString();
        }

        private string RenderArbol(IEnumerable<IComponente> componentes)
        {
            List<IComponente> lista = componentes.ToList();
            if (lista.Count == 0)
                return string.Empty;

            var html = new StringBuilder("<ul class=\"perm-arbol\">");
            foreach (IComponente componente in lista)
            {
                bool esFamilia = componente is Familia;
                html.Append("<li class=\"").Append(esFamilia ? "familia" : "permiso").Append("\">")
                    .Append("<i class=\"fa-solid ").Append(esFamilia ? "fa-folder" : "fa-key").Append("\" aria-hidden=\"true\"></i> ")
                    .Append(HttpUtility.HtmlEncode(NombreVisible(componente)));

                if (componente is Familia familia)
                    html.Append(RenderArbol(familia.ObtenerHijos()));

                html.Append("</li>");
            }
            return html.Append("</ul>").ToString();
        }

        private static IEnumerable<int> Subfamilias(Familia familia)
        {
            foreach (Familia hija in familia.ObtenerHijos().OfType<Familia>())
            {
                yield return hija.Id;
                foreach (int id in Subfamilias(hija))
                    yield return id;
            }
        }

        private string Formato(string clave, params object[] args)
        {
            string plantilla = T(clave);
            try
            {
                return string.Format(plantilla, args);
            }
            catch (FormatException)
            {
                return plantilla;
            }
        }

        /// <summary>
        /// Script del botón Eliminar: pide confirmación antes del postback.
        /// </summary>
        protected string ConfirmarEliminar()
        {
            return "return confirm(" + HttpUtility.JavaScriptStringEncode(T("permisos_confirmar_eliminar"), true) + ");";
        }

        #endregion
    }
}
