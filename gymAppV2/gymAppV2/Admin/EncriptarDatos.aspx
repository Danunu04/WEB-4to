<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="EncriptarDatos.aspx.cs" Inherits="gymAppV2.Admin.EncriptarDatos" MasterPageFile="~/DashBoard.Master" %>

<asp:Content ID="HeadContent" ContentPlaceHolderID="HeadContent" runat="server">
    <title>Migrar Encriptación - GymApp</title>
    <link href="<%= ResolveUrl("~/Admin/EncriptarDatos.css") %>" rel="stylesheet" type="text/css" />
</asp:Content>

<asp:Content ID="MainContent" ContentPlaceHolderID="MainContent" runat="server">
    <div class="encriptar-container">
        <header class="encriptar-header">
            <h1 class="encriptar-title">Migrar datos a encriptación reversible</h1>
            <p class="encriptar-subtitle">
                Encripta AES-256 todos los datos personales existientes en la base de datos.
                Los valores ya encriptados con IV aleatorio se saltan. Los valores encriptados
                con el formato anterior se migran automáticamente al nuevo formato.
            </p>
        </header>

        <asp:Panel ID="pnlAdvertencia" runat="server" CssClass="advertencia-box">
            <strong>Importante:</strong> esta operación modifica la base de datos. Asegúrese de tener un backup antes de continuar.
        </asp:Panel>

        <div class="encriptar-actions">
            <asp:Button ID="btnEncriptarTodo" runat="server" Text="Encriptar todos los datos personales" CssClass="btn-primary" OnClick="btnEncriptarTodo_Click" />
        </div>

        <asp:Panel ID="pnlResultado" runat="server" Visible="false" CssClass="resultado-panel">
            <h2>Resultado de la migración</h2>

            <asp:GridView ID="gvResultados" runat="server" AutoGenerateColumns="false" CssClass="resultado-grid"
                EmptyDataText="No se encontraron tablas para migrar.">
                <Columns>
                    <asp:BoundField DataField="Tabla" HeaderText="Tabla" />
                    <asp:BoundField DataField="Campo" HeaderText="Campo" />
                    <asp:BoundField DataField="TotalFilas" HeaderText="Total filas" />
                    <asp:BoundField DataField="Encriptadas" HeaderText="Encriptadas ahora" />
                    <asp:BoundField DataField="YaEncriptadas" HeaderText="Ya encriptadas" />
                    <asp:BoundField DataField="LegacyReencriptadas" HeaderText="Formato anterior migrado" />
                    <asp:BoundField DataField="Errores" HeaderText="Errores" />
                    <asp:BoundField DataField="MensajeError" HeaderText="Observaciones" />
                </Columns>
            </asp:GridView>
        </asp:Panel>

        <asp:Panel ID="pnlError" runat="server" Visible="false" CssClass="error-box">
            <asp:Label ID="lblError" runat="server" />
        </asp:Panel>
    </div>
</asp:Content>
