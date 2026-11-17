<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="Paginador.ascx.cs" Inherits="gymAppV2.Controles.Paginador" %>
<asp:Panel ID="pnlPaginador" runat="server" CssClass="paginador" Visible="false">
    <asp:Repeater ID="rptPaginas" runat="server" OnItemCommand="rptPaginas_ItemCommand">
        <ItemTemplate>
            <asp:LinkButton ID="lnkPagina" runat="server" CommandName="Pagina" CommandArgument='<%# Eval("Indice") %>'
                CausesValidation="false" Visible='<%# !(bool)Eval("Actual") %>'><%# Eval("Texto") %></asp:LinkButton>
            <asp:Label ID="lblPaginaActual" runat="server" Visible='<%# (bool)Eval("Actual") %>'><%# Eval("Texto") %></asp:Label>
        </ItemTemplate>
    </asp:Repeater>
</asp:Panel>
