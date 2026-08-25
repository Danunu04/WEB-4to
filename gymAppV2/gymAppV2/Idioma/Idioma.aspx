<%@ Page Title="Idioma" Language="C#" MasterPageFile="~/DashBoard.Master" AutoEventWireup="true" CodeBehind="Idioma.aspx.cs" Inherits="gymAppV2.Idioma.IdiomaPage" %>

<asp:Content ID="HeadContent" ContentPlaceHolderID="HeadContent" runat="server">
    <link href="<%= ResolveUrl("~/Idioma/Idioma.css") %>" rel="stylesheet" type="text/css" />
</asp:Content>

<asp:Content ID="MainContent" ContentPlaceHolderID="MainContent" runat="server">
    <script>
    function giMostrarNuevo() {
        document.getElementById('giNuevoPanel').style.display = 'block';
    }
    function giOcultarNuevo() {
        document.getElementById('giNuevoPanel').style.display = 'none';
    }

    function giToggle(i) {
        var body    = document.getElementById('gi-body-' + i);
        var chevron = document.querySelector('#gi-sec-' + i + ' .gi-sec-chevron');
        var hdr     = document.querySelector('#gi-sec-' + i + ' .gi-sec-header');
        var isOpen  = body.style.display !== 'none';
        body.style.display = isOpen ? 'none' : 'block';
        chevron.innerHTML  = isOpen ? '&#9654;' : '&#9660;';
        if (hdr) hdr.classList.toggle('gi-sec-header-open', !isOpen);
    }

    function giCambiarRef() {
        var sel  = document.getElementById('giDdlRef');
        if (!sel || typeof giRefData === 'undefined') return;
        var data = giRefData[sel.value] || {};
        document.querySelectorAll('.gi-ref[data-tag]').forEach(function(td) {
            td.textContent = data[td.getAttribute('data-tag')] || '';
        });
    }

    function giUpdateProgress(inp) {
        if (!inp) return;
        var idx = inp.getAttribute('data-sec');
        var sec = document.getElementById('gi-sec-' + idx);
        if (!sec) return;
        var inputs = sec.querySelectorAll('.gi-input');
        var filled = 0;
        inputs.forEach(function(i) { if (i.value.trim() !== '') filled++; });
        var prog = document.getElementById('gi-prog-' + idx);
        if (prog) {
            prog.textContent = filled + '/' + inputs.length;
            prog.className   = 'gi-sec-progress ' + (filled === inputs.length ? 'gi-prog-ok'
                             : filled > 0 ? 'gi-prog-partial' : 'gi-prog-empty');
        }
        inp.classList.toggle('gi-input-error', inp.value.trim() === '');
        giUpdateTotalProgress();
    }

    function giUpdateAllProgress() {
        document.querySelectorAll('.gi-section').forEach(function(sec, i) {
            var inputs = sec.querySelectorAll('.gi-input');
            var filled = 0;
            inputs.forEach(function(inp) { if (inp.value.trim() !== '') filled++; });
            var prog = document.getElementById('gi-prog-' + i);
            if (prog) {
                prog.textContent = filled + '/' + inputs.length;
                prog.className   = 'gi-sec-progress ' + (filled === inputs.length ? 'gi-prog-ok'
                                 : filled > 0 ? 'gi-prog-partial' : 'gi-prog-empty');
            }
        });
        giUpdateTotalProgress();
    }

    function giUpdateTotalProgress() {
        var all    = document.querySelectorAll('.gi-input');
        var filled = 0;
        all.forEach(function(i) { if (i.value.trim() !== '') filled++; });
        var el = document.getElementById('giProgressText');
        if (el) el.textContent = filled + '/' + all.length + ' campos completados';
    }

    function giValidar() {
        var inputs     = document.querySelectorAll('.gi-input');
        var primerVacio = null;
        inputs.forEach(function(inp) {
            var vacio = inp.value.trim() === '';
            inp.classList.toggle('gi-input-error', vacio);
            if (vacio && !primerVacio) primerVacio = inp;
        });
        if (primerVacio) {
            var sec = primerVacio.closest('.gi-section');
            if (sec) {
                var idx  = sec.id.replace('gi-sec-', '');
                var body = document.getElementById('gi-body-' + idx);
                if (body && body.style.display === 'none') giToggle(idx);
            }
            primerVacio.focus();
            primerVacio.scrollIntoView({ behavior: 'smooth', block: 'center' });
            return false;
        }
        return true;
    }
    </script>

    <div class="idioma-page">

        <%-- Selector de idioma activo --%>
        <div class="idioma-header">
            <h1 class="idioma-titulo"><asp:Literal ID="litTitulo" runat="server" Text="Seleccionar idioma" /></h1>
            <p class="idioma-subtitulo"><asp:Literal ID="litSubtitulo" runat="server" Text="Elegí el idioma en que querés usar la aplicación." /></p>
        </div>

        <div class="idioma-grid">
            <asp:LinkButton ID="btnES" runat="server" CssClass="idioma-card" OnClick="BtnES_Click">
                <div class="idioma-flag">🇦🇷</div>
                <div class="idioma-nombre">Español</div>
                <div class="idioma-nativo">Español</div>
                <div class="idioma-activo-badge" id="badgeES" runat="server"><asp:Literal ID="litActivoES" runat="server" Text="Activo" /></div>
            </asp:LinkButton>

            <asp:LinkButton ID="btnEN" runat="server" CssClass="idioma-card" OnClick="BtnEN_Click">
                <div class="idioma-flag">🇺🇸</div>
                <div class="idioma-nombre">English</div>
                <div class="idioma-nativo">English</div>
                <div class="idioma-activo-badge" id="badgeEN" runat="server"><asp:Literal ID="litActivoEN" runat="server" Text="Active" /></div>
            </asp:LinkButton>

            <asp:LinkButton ID="btnPT" runat="server" CssClass="idioma-card" OnClick="BtnPT_Click">
                <div class="idioma-flag">🇧🇷</div>
                <div class="idioma-nombre">Português</div>
                <div class="idioma-nativo">Português</div>
                <div class="idioma-activo-badge" id="badgePT" runat="server"><asp:Literal ID="litActivoPT" runat="server" Text="Ativo" /></div>
            </asp:LinkButton>

            <asp:LinkButton ID="btnFR" runat="server" CssClass="idioma-card" OnClick="BtnFR_Click">
                <div class="idioma-flag">🇫🇷</div>
                <div class="idioma-nombre">Français</div>
                <div class="idioma-nativo">Français</div>
                <div class="idioma-activo-badge" id="badgeFR" runat="server"><asp:Literal ID="litActivoFR" runat="server" Text="Actif" /></div>
            </asp:LinkButton>

            <asp:LinkButton ID="btnJA" runat="server" CssClass="idioma-card" OnClick="BtnJA_Click">
                <div class="idioma-flag">🇯🇵</div>
                <div class="idioma-nombre">日本語</div>
                <div class="idioma-nativo">日本語</div>
                <div class="idioma-activo-badge" id="badgeJA" runat="server"><asp:Literal ID="litActivoJA" runat="server" Text="有効" /></div>
            </asp:LinkButton>
        </div>

        <%-- Gestión de idiomas (solo admins) --%>
        <asp:Panel ID="pnlGestion" runat="server" Visible="false" CssClass="gi-outer">

            <div class="gi-section-divider"></div>

            <div class="gi-header">
                <h2 class="gi-titulo">Gestión de Idiomas</h2>
                <div class="gi-actions">
                    <button type="button" class="gi-btn-nuevo" onclick="giMostrarNuevo()">+ Nuevo idioma</button>
                    <div class="gi-editar-row">
                        <asp:DropDownList ID="ddlEditarIdioma" runat="server" CssClass="gi-ddl" />
                        <asp:Button ID="btnEditarIdioma" runat="server" Text="Editar" CssClass="gi-btn-accion" OnClick="BtnEditarIdioma_Click" CausesValidation="false" />
                    </div>
                </div>
            </div>

            <%-- Formulario nuevo idioma (mostrado/ocultado por JS) --%>
            <div class="gi-nuevo-panel" id="giNuevoPanel" style="display:none">
                <div class="gi-nuevo-campos">
                    <div class="gi-campo">
                        <label class="gi-label">Nombre del idioma</label>
                        <asp:TextBox ID="txtNombreIdioma" runat="server" CssClass="gi-input-field" placeholder="Ej: Ruso" />
                    </div>
                    <div class="gi-campo">
                        <label class="gi-label">Código (2–5 letras)</label>
                        <asp:TextBox ID="txtCodigoIdioma" runat="server" CssClass="gi-input-field gi-input-code" placeholder="Ej: RU" MaxLength="5" />
                    </div>
                    <div class="gi-nuevo-btns">
                        <asp:Button ID="btnIniciarNuevo" runat="server" Text="Iniciar traducción" CssClass="gi-btn-iniciar" OnClick="BtnIniciarNuevo_Click" CausesValidation="false" />
                        <button type="button" class="gi-btn-cancel-nuevo" onclick="giOcultarNuevo()">Cancelar</button>
                    </div>
                </div>
            </div>

            <%-- Editor de traducciones --%>
            <asp:Panel ID="pnlEditor" runat="server" Visible="false" CssClass="gi-editor">

                <div class="gi-editor-header">
                    <span class="gi-editor-info"><asp:Literal ID="litEditorNombre" runat="server" /></span>
                    <div class="gi-ref-selector">
                        <label class="gi-label">Referencia:</label>
                        <select id="giDdlRef" class="gi-ddl" onchange="giCambiarRef()">
                            <asp:Literal ID="litRefOptions" runat="server" />
                        </select>
                    </div>
                </div>

                <div class="gi-progress-total">
                    <span id="giProgressText">Calculando...</span>
                </div>

                <%-- Contenido del acordeón generado server-side --%>
                <asp:Literal ID="litTablas" runat="server" />

                <%-- JSON de referencia + inicialización JS --%>
                <asp:Literal ID="litRefData" runat="server" />

                <div class="gi-save-bar">
                    <asp:Button ID="btnGuardarIdioma" runat="server" Text="Guardar idioma"
                                CssClass="gi-btn-guardar"
                                OnClientClick="return giValidar();"
                                OnClick="BtnGuardarIdioma_Click"
                                CausesValidation="false" />
                    <asp:Button ID="btnCancelarEditor" runat="server" Text="Cancelar"
                                CssClass="gi-btn-cancelar-editor"
                                OnClick="BtnCancelarEditor_Click"
                                CausesValidation="false" />
                </div>
            </asp:Panel>

        </asp:Panel>

    </div>

</asp:Content>
