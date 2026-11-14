<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="Precios.aspx.cs" Inherits="gymAppV2.Precios.Precios" MasterPageFile="~/DashBoard.Master" Title="Precios de cuota" %>

<asp:Content ID="HeadContent" ContentPlaceHolderID="HeadContent" runat="server">
    <link href="<%= ResolveUrl("~/Precios/Precios.css?v=1") %>" rel="stylesheet" type="text/css" />
    <link rel="stylesheet" href="https://cdnjs.cloudflare.com/ajax/libs/font-awesome/6.5.0/css/all.min.css" />
</asp:Content>

<asp:Content ID="MainContent" ContentPlaceHolderID="MainContent" runat="server">
    <div class="precios-container">
        <div class="precios-header">
            <div class="precios-title">
                <i class="fa-solid fa-tags"></i>
                <asp:Literal ID="litTitulo" runat="server" Text="Precios de cuota" />
            </div>
            <p><asp:Literal ID="litSubtitulo" runat="server" Text="Precio mensual de cada modalidad de asistencia." /></p>
        </div>

        <p class="precios-aviso"><i class="fa-solid fa-bullhorn"></i> <asp:Literal ID="litAviso" runat="server" /></p>

        <div class="precios-card">
            <asp:Repeater ID="rptPrecios" runat="server" OnItemCommand="rptPrecios_ItemCommand">
                <HeaderTemplate>
                    <table class="precios-tabla tabla-tarjetas">
                        <thead>
                            <tr>
                                <th><%# Texto("precios_col_modalidad", "Modalidad") %></th>
                                <th><%# Texto("precios_col_actual", "Precio actual") %></th>
                                <th><%# Texto("precios_col_modificado", "Última modificación") %></th>
                                <th><%# Texto("precios_col_nuevo", "Nuevo precio") %></th>
                            </tr>
                        </thead>
                        <tbody>
                </HeaderTemplate>
                <ItemTemplate>
                            <tr>
                                <td><strong><%#: Eval("Descripcion") %></strong></td>
                                <td><%# Eval("Precio", "{0:C}") %></td>
                                <td><%# Eval("FechaModificacion", "{0:dd/MM/yyyy HH:mm}") %></td>
                                <td>
                                    <div class="precios-carga">
                                        <asp:TextBox ID="txtNuevoPrecio" runat="server" TextMode="Number" step="0.01" min="0.01" placeholder="0" />
                                        <asp:Button ID="btnGuardar" runat="server" CssClass="btn-guardar"
                                            CommandName="Guardar" CommandArgument='<%# Eval("Id") %>' Text='<%# TextoBotonGuardar %>' />
                                    </div>
                                </td>
                            </tr>
                </ItemTemplate>
                <FooterTemplate>
                        </tbody>
                    </table>
                </FooterTemplate>
            </asp:Repeater>
        </div>
    </div>
</asp:Content>
