<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="Permisos.aspx.cs" Inherits="gymAppV2.Permisos.Permisos" MasterPageFile="~/DashBoard.Master" Title="Permisos" %>

<asp:Content ID="HeadContent" ContentPlaceHolderID="HeadContent" runat="server">
    <link href="<%= ResolveUrl("~/Permisos/Permisos.css?v=2") %>" rel="stylesheet" type="text/css" />
    <link rel="stylesheet" href="https://cdnjs.cloudflare.com/ajax/libs/font-awesome/6.5.0/css/all.min.css" />
</asp:Content>

<asp:Content ID="MainContent" ContentPlaceHolderID="MainContent" runat="server">
    <div class="permisos-container">
        <div class="permisos-header">
            <i class="fa-solid fa-shield-halved"></i>
            <div class="permisos-title-text">
                <h1><asp:Literal ID="litTitulo" runat="server" Text="Permisos" /></h1>
                <p><asp:Literal ID="litSubtitulo" runat="server" Text="Gestión de roles y permisos del sistema" /></p>
            </div>
        </div>

        <!-- Se muestra si todavía no se corrió scripts/old/crear-tabla-rol-permiso.sql -->
        <asp:Panel ID="pnlSinTabla" runat="server" Visible="false" CssClass="permisos-empty">
            <i class="fa-solid fa-database"></i>
            <h3><asp:Literal ID="litSinTablaTitulo" runat="server" /></h3>
            <p><asp:Literal ID="litSinTablaMsg" runat="server" /></p>
        </asp:Panel>

        <asp:Panel ID="pnlMatriz" runat="server" CssClass="permisos-card">
            <p class="permisos-ayuda"><asp:Literal ID="litAyuda" runat="server" /></p>

            <div class="permisos-tabla-wrap">
                <table class="permisos-tabla">
                    <thead>
                        <tr>
                            <th class="col-permiso"><asp:Literal ID="litColPermiso" runat="server" Text="Permiso" /></th>
                            <th>WebMaster</th>
                            <th>Administrador</th>
                            <th>Recepcionista</th>
                            <th>Entrenador</th>
                            <th>Cliente</th>
                            <th>Familiar</th>
                        </tr>
                    </thead>
                    <tbody>
                        <asp:Repeater ID="rptPermisos" runat="server" OnItemDataBound="rptPermisos_ItemDataBound">
                            <ItemTemplate>
                                <tr>
                                    <td class="col-permiso">
                                        <span class="permiso-nombre"><%#: Eval("Nombre") %></span>
                                        <span class="permiso-clave"><%#: Eval("Clave") %></span>
                                        <asp:HiddenField ID="hdnPermiso" runat="server" Value='<%# Eval("Clave") %>' />
                                    </td>
                                    <!-- WebMaster: siempre tiene todos los permisos -->
                                    <td><input type="checkbox" checked="checked" disabled="disabled" /></td>
                                    <td><asp:CheckBox ID="chkRol1" runat="server" /></td>
                                    <td><asp:CheckBox ID="chkRol2" runat="server" /></td>
                                    <td><asp:CheckBox ID="chkRol3" runat="server" /></td>
                                    <td><asp:CheckBox ID="chkRol4" runat="server" /></td>
                                    <td><asp:CheckBox ID="chkRol6" runat="server" /></td>
                                </tr>
                            </ItemTemplate>
                        </asp:Repeater>
                    </tbody>
                </table>
            </div>

            <div class="permisos-acciones">
                <asp:Button ID="btnRestablecer" runat="server" CssClass="btn-permisos secundario" OnClick="btnRestablecer_Click" CausesValidation="false" />
                <asp:Button ID="btnGuardar" runat="server" CssClass="btn-permisos" OnClick="btnGuardar_Click" CausesValidation="false" />
            </div>
        </asp:Panel>
    </div>
</asp:Content>
