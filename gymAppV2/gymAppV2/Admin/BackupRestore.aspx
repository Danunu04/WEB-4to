<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="BackupRestore.aspx.cs" Inherits="gymAppV2.Admin.BackupRestore" MasterPageFile="~/DashBoard.Master" %>

<asp:Content ID="HeadContent" ContentPlaceHolderID="HeadContent" runat="server">
    <title>Gestión de Respaldo - GymApp</title>
    <link href="<%= ResolveUrl("~/Admin/BackupRestore.css?v=4") %>" rel="stylesheet" type="text/css" />
</asp:Content>

<asp:Content ID="MainContent" ContentPlaceHolderID="MainContent" runat="server">
    <div class="backuprestore-container">
        <header class="backuprestore-header">
            <h1 class="backuprestore-title">Gestión de Respaldo</h1>
            <p class="backuprestore-subtitle">Realice backups de seguridad o restaure la base de datos desde un archivo .bak.</p>
        </header>

        <asp:Panel ID="pnlBackup" runat="server" CssClass="backuprestore-section">
            <div class="section-header">
                <i class="bi bi-cloud-arrow-up section-icon"></i>
                <h2 class="section-title">Backup</h2>
            </div>
            <p class="backuprestore-ayuda">Genere una copia de seguridad completa de la base de datos en una ruta del servidor.</p>

            <div class="backuprestore-form">
                <span class="form-label">Formato</span>
                <asp:RadioButtonList ID="rblFormatoBackup" runat="server"
                    RepeatLayout="Flow" RepeatDirection="Horizontal"
                    CssClass="formato-radios">
                    <asp:ListItem Value="bak" Selected="True">.bak &mdash; nativo SQL Server (misma versión)</asp:ListItem>
                    <asp:ListItem Value="bacpac">.bacpac &mdash; portable (compatible con versiones anteriores)</asp:ListItem>
                </asp:RadioButtonList>

                <asp:Label ID="lblRutaBackup" runat="server" CssClass="form-label" AssociatedControlID="txtRutaBackup">Ruta destino</asp:Label>
                <div class="input-row">
                    <asp:TextBox ID="txtRutaBackup" runat="server" CssClass="form-control" placeholder="C:\Backups\GymApp_20250701_120000.bak" />
                    <asp:Button ID="btnGenerarNombre" runat="server" Text="Generar nombre" CssClass="btn-secondary" OnClick="btnGenerarNombre_Click" />
                </div>
                <asp:Button ID="btnRealizarBackup" runat="server" Text="Realizar backup" CssClass="btn-primary" OnClick="btnRealizarBackup_Click"
                    OnClientClick="return iniciarBackup(this);" />
            </div>
        </asp:Panel>

        <asp:Panel ID="pnlRestore" runat="server" CssClass="backuprestore-section">
            <div class="section-header">
                <i class="bi bi-cloud-arrow-down section-icon"></i>
                <h2 class="section-title">Restore</h2>
            </div>
            <div class="advertencia-box">
                <strong>Advertencia:</strong> esta operación reemplazará la base de datos actual. Todos los cambios posteriores al backup seleccionado se perderán.
            </div>

            <div class="backuprestore-form">
                <span class="form-label">Formato</span>
                <asp:RadioButtonList ID="rblFormatoRestore" runat="server"
                    RepeatLayout="Flow" RepeatDirection="Horizontal"
                    CssClass="formato-radios">
                    <asp:ListItem Value="bak" Selected="True">.bak &mdash; nativo SQL Server</asp:ListItem>
                    <asp:ListItem Value="bacpac">.bacpac &mdash; portable</asp:ListItem>
                </asp:RadioButtonList>

                <asp:Label runat="server" CssClass="form-label">Backups disponibles en C:\GymApp\</asp:Label>
                <div class="input-row">
                    <asp:DropDownList ID="ddlBackupsRestore" runat="server" CssClass="form-control"
                        AutoPostBack="true" OnSelectedIndexChanged="ddlBackupsRestore_SelectedIndexChanged" />
                    <asp:Button ID="btnActualizarListaRestore" runat="server" Text="↻ Actualizar" CssClass="btn-secondary" OnClick="btnActualizarListaRestore_Click" />
                </div>

                <asp:Label ID="lblRutaRestore" runat="server" CssClass="form-label" AssociatedControlID="txtRutaRestore">Ruta del archivo a restaurar</asp:Label>
                <asp:TextBox ID="txtRutaRestore" runat="server" CssClass="form-control" placeholder="C:\GymApp\GymApp_20250701_120000.bak" />
                <asp:Button ID="btnRealizarRestore" runat="server" Text="Restaurar backup" CssClass="btn-danger" OnClick="btnRealizarRestore_Click"
                    OnClientClick="return confirmarRestore(this);" />
            </div>
        </asp:Panel>
    </div>

    <!-- Confirmación del restore (reemplaza el confirm() del navegador) -->
    <div id="modalRestore" class="confirm-overlay" hidden>
        <div class="confirm-box" role="dialog" aria-modal="true" aria-labelledby="modalRestoreTitulo">
            <h3 id="modalRestoreTitulo">¿Restaurar este backup?</h3>
            <p>Se van a reemplazar todos los datos actuales por los del archivo elegido. Los cambios hechos después de ese backup se pierden.</p>
            <p class="confirm-archivo" id="modalRestoreArchivo"></p>
            <div class="confirm-acciones">
                <button type="button" class="btn-secondary" id="btnRestoreCancelar">Cancelar</button>
                <button type="button" class="btn-danger" id="btnRestoreConfirmar">Sí, restaurar</button>
            </div>
        </div>
    </div>

    <!-- Pantalla de carga mientras corre el backup o el restore: no deja usar ni salir de la página -->
    <div id="cargaOperacion" class="carga-overlay" hidden tabindex="-1" role="alertdialog" aria-modal="true"
        aria-labelledby="cargaTitulo" aria-describedby="cargaTexto">
        <div class="carga-box">
            <div class="carga-spinner" aria-hidden="true"></div>
            <h3 id="cargaTitulo"></h3>
            <p id="cargaTexto">Esto puede tardar varios minutos según el tamaño de la base. No cierres ni salgas de esta pantalla hasta que termine.</p>
            <p class="carga-tiempo">Tiempo transcurrido: <span id="cargaTiempo">0:00</span></p>
        </div>
    </div>

    <script>
        (function () {
            function actualizarExtension(formato, txtId) {
                var txt = document.getElementById(txtId);
                if (!txt || !txt.value) return;
                var ruta = txt.value;
                var extNueva = formato === 'bacpac' ? '.bacpac' : '.bak';
                var extVieja = formato === 'bacpac' ? '.bak' : '.bacpac';

                if (ruta.toLowerCase().endsWith(extVieja.toLowerCase())) {
                    txt.value = ruta.slice(0, -extVieja.length) + extNueva;
                } else {
                    // Reemplazar cualquier otra extensión existente
                    var sep = Math.max(ruta.lastIndexOf('\\'), ruta.lastIndexOf('/'));
                    var lastDot = ruta.lastIndexOf('.');
                    if (lastDot > sep) {
                        txt.value = ruta.substring(0, lastDot) + extNueva;
                    }
                }
            }

            function adjuntar(rblId, txtId) {
                var rbl = document.getElementById(rblId);
                if (!rbl) return;
                rbl.querySelectorAll('input[type="radio"]').forEach(function (radio) {
                    radio.addEventListener('change', function () {
                        actualizarExtension(this.value, txtId);
                    });
                });
            }

            adjuntar('<%= rblFormatoBackup.ClientID %>', '<%= txtRutaBackup.ClientID %>');
            adjuntar('<%= rblFormatoRestore.ClientID %>', '<%= txtRutaRestore.ClientID %>');

            // Confirmación del restore: el primer clic abre el cartel; "Sí, restaurar" hace el postback del botón.
            var modal = document.getElementById('modalRestore');
            var botonPendiente = null;

            function cerrarModal() {
                modal.hidden = true;
                botonPendiente = null;
            }

            window.confirmarRestore = function (boton) {
                var ruta = document.getElementById('<%= txtRutaRestore.ClientID %>');
                document.getElementById('modalRestoreArchivo').textContent = ruta && ruta.value ? ruta.value : '';
                botonPendiente = boton;
                modal.hidden = false;
                document.getElementById('btnRestoreCancelar').focus();
                return false;
            };

            document.getElementById('btnRestoreCancelar').addEventListener('click', cerrarModal);
            modal.addEventListener('click', function (e) { if (e.target === modal) cerrarModal(); });
            document.addEventListener('keydown', function (e) { if (e.key === 'Escape' && !modal.hidden) cerrarModal(); });
            document.getElementById('btnRestoreConfirmar').addEventListener('click', function () {
                var nombre = botonPendiente ? botonPendiente.name : null;
                cerrarModal();
                if (!nombre) return;
                var formato = formatoElegido('<%= rblFormatoRestore.ClientID %>');
                mostrarCarga('Restaurando la base desde ' + formato + '...');
                __doPostBack(nombre, '');
            });

            // ===== Pantalla de carga =====
            // El backup y el restore son un postback que tarda: mientras el servidor trabaja se muestra
            // el cartel, se bloquea el resto de la página y se avisa si el usuario intenta salir.
            // Cuando llega la respuesta la página se recarga y el cartel desaparece solo.
            var carga = document.getElementById('cargaOperacion');
            var operacionEnCurso = false;
            var saltarAvisoPropio = false;

            function formatoElegido(rblId) {
                var marcado = document.querySelector('#' + rblId + ' input[type="radio"]:checked');
                return marcado && marcado.value === 'bacpac' ? '.bacpac' : '.bak';
            }

            function mostrarCarga(titulo) {
                document.getElementById('cargaTitulo').textContent = titulo;

                // Se pasa al final del body para poder dejar inerte todo lo demás (menú, links, botones)
                document.body.appendChild(carga);
                Array.prototype.forEach.call(document.body.children, function (el) {
                    if (el !== carga) el.inert = true;
                });
                carga.hidden = false;
                carga.focus();

                operacionEnCurso = true;
                // El propio postback dispara un beforeunload: ese no tiene que mostrar el aviso
                saltarAvisoPropio = true;
                setTimeout(function () { saltarAvisoPropio = false; }, 1000);

                var inicio = Date.now();
                var reloj = document.getElementById('cargaTiempo');
                setInterval(function () {
                    var seg = Math.floor((Date.now() - inicio) / 1000);
                    reloj.textContent = Math.floor(seg / 60) + ':' + ('0' + (seg % 60)).slice(-2);
                }, 1000);
            }

            window.iniciarBackup = function (boton) {
                var formato = formatoElegido('<%= rblFormatoBackup.ClientID %>');
                mostrarCarga('Generando backup ' + formato + '...');
                __doPostBack(boton.name, '');
                return false;
            };

            // Cerrar la pestaña, recargar o ir atrás mientras corre la operación: el navegador pide confirmar
            window.addEventListener('beforeunload', function (e) {
                if (!operacionEnCurso) return;
                if (saltarAvisoPropio) {
                    saltarAvisoPropio = false;
                    return;
                }
                e.preventDefault();
                e.returnValue = '';
            });
        })();
    </script>
</asp:Content>
