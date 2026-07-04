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
                <asp:Label ID="lblRutaBackup" runat="server" CssClass="form-label" AssociatedControlID="txtRutaBackup">Ruta destino (.bak)</asp:Label>
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
                <asp:Label ID="lblRutaRestore" runat="server" CssClass="form-label" AssociatedControlID="txtRutaRestore">Ruta del backup a restaurar (.bak)</asp:Label>
                <asp:TextBox ID="txtRutaRestore" runat="server" CssClass="form-control" placeholder="C:\Backups\GymApp_20250701_120000.bak" />
                <asp:Button ID="btnRealizarRestore" runat="server" Text="Restaurar backup" CssClass="btn-danger" OnClick="btnRealizarRestore_Click" />
            </div>
        </asp:Panel>
    </div>
</asp:Content>
