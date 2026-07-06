<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="BackupRestore.aspx.cs" Inherits="gymAppV2.Admin.BackupRestore" MasterPageFile="~/DashBoard.Master" %>

<asp:Content ID="HeadContent" ContentPlaceHolderID="HeadContent" runat="server">
    <title>Gestión de Respaldo - GymApp</title>
    <link href="<%= ResolveUrl("~/Admin/BackupRestore.css") %>" rel="stylesheet" type="text/css" />
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
                <asp:Button ID="btnRealizarBackup" runat="server" Text="Realizar backup" CssClass="btn-primary" OnClick="btnRealizarBackup_Click" />
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

                <asp:Label ID="lblRutaRestore" runat="server" CssClass="form-label" AssociatedControlID="txtRutaRestore">Ruta del archivo a restaurar</asp:Label>
                <asp:TextBox ID="txtRutaRestore" runat="server" CssClass="form-control" placeholder="C:\Backups\GymApp_20250701_120000.bak" />
                <asp:Button ID="btnRealizarRestore" runat="server" Text="Restaurar backup" CssClass="btn-danger" OnClick="btnRealizarRestore_Click"
                    OnClientClick="return confirm('¿Confirma que desea restaurar? Esta operación reemplaza todos los datos actuales.');" />
            </div>
        </asp:Panel>
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
        })();
    </script>
</asp:Content>
