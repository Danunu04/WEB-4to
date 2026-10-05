<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="Aulas.aspx.cs" Inherits="gymAppV2.Aulas.Aulas" MasterPageFile="~/DashBoard.Master" Title="Aulas" %>

<asp:Content ID="Content1" ContentPlaceHolderID="HeadContent" runat="server">
    <style>
        .aulas-container {
            --color-surface: #FFFFFF;
            --color-surface-2: #FDF6EC;
            --color-text: #2D2D2D;
            --color-muted: #6B6B6B;
            --color-border: #E8E0D5;
            --color-accent: #C7B5FF;
            --color-accent-light: #F0EBFF;
            --color-mint-light: #E5F9F0;
            --radius-lg: 0.75rem;
            --radius-xl: 1rem;
            padding: 1.5rem;
        }
        .aulas-header { display: flex; align-items: center; justify-content: space-between; flex-wrap: wrap; gap: 0.75rem; margin-bottom: 1.25rem; }
        .aulas-header h2 { font-family: 'Fraunces', serif; font-size: 1.5rem; font-weight: 700; color: var(--color-text); margin: 0; }
        .aulas-header p { color: var(--color-muted); margin: 0.25rem 0 0 0; font-size: 0.875rem; }
        .btn-nueva { display: inline-flex; align-items: center; gap: 0.5rem; padding: 0.5rem 1rem; background: var(--color-accent); color: white; border: none; border-radius: var(--radius-lg); font-weight: 500; cursor: pointer; text-decoration: none; }
        .btn-nueva:hover { color: white; transform: translateY(-0.125rem); }
        .aulas-card { background: var(--color-surface); border: 0.0625rem solid var(--color-border); border-radius: var(--radius-xl); overflow-x: auto; }
        .aulas-tabla { width: 100%; border-collapse: collapse; font-size: 0.875rem; }
        .aulas-tabla th { text-align: left; padding: 0.75rem 1rem; background: var(--color-surface-2); color: var(--color-muted); font-weight: 500; border-bottom: 0.0625rem solid var(--color-border); }
        .aulas-tabla td { padding: 0.75rem 1rem; border-bottom: 0.0625rem solid var(--color-border); color: var(--color-text); vertical-align: middle; }
        .aulas-tabla tr:last-child td { border-bottom: none; }
        .aulas-tabla .numero { font-family: 'JetBrains Mono', monospace; }
        .estado-pill { display: inline-block; padding: 0.125rem 0.5rem; border-radius: 62.5rem; font-size: 0.75rem; font-weight: 600; }
        .estado-pill.activa { background: var(--color-mint-light); color: #0F6E56; }
        .estado-pill.inactiva { background: #F1EFE8; color: #5F5E5A; }
        .acciones-fila { display: flex; gap: 0.75rem; flex-wrap: wrap; }
        .btn-link-accion { background: none; border: none; padding: 0; font-size: 0.875rem; font-weight: 500; color: #6B4FD8; cursor: pointer; text-decoration: underline; }
        .btn-link-accion.peligro { color: #C0392B; }
        .texto-vacio { color: var(--color-muted); font-size: 0.875rem; }

        .modal-overlay { position: fixed; inset: 0; background: rgba(0, 0, 0, 0.5); display: flex; align-items: center; justify-content: center; z-index: 1000; padding: 1rem; }
        .aulas-container .modal-content { background: var(--color-surface); border: 0.0625rem solid var(--color-border); border-radius: var(--radius-xl); padding: 1.5rem; width: 100%; max-width: 28rem; }
        .aulas-container .modal-title { font-family: 'Fraunces', serif; font-size: 1.25rem; font-weight: 700; color: var(--color-text); margin: 0 0 1rem 0; }
        .form-group { margin-bottom: 1rem; }
        .form-group label { display: block; font-size: 0.875rem; font-weight: 500; color: var(--color-text); margin-bottom: 0.25rem; }
        .form-group input[type="text"], .form-group input[type="number"] { width: 100%; padding: 0.5rem 0.75rem; border-radius: var(--radius-lg); border: 0.0625rem solid var(--color-border); }
        .form-group small { color: var(--color-muted); font-size: 0.75rem; }
        .modal-actions { display: flex; gap: 0.5rem; padding-top: 1rem; border-top: 0.0625rem solid var(--color-border); }
        .btn-secondary, .btn-submit { flex: 1; padding: 0.5rem 1rem; border-radius: var(--radius-lg); font-weight: 500; cursor: pointer; }
        .btn-secondary { border: 0.0625rem solid var(--color-border); background: var(--color-surface); color: var(--color-muted); }
        .btn-submit { border: none; background: var(--color-accent); color: white; }

        .sidebar-menu li a[href*="Aulas"] { background: var(--color-accent, #C7B5FF); color: white; }
        .sidebar-menu li a[href*="Aulas"] i, .sidebar-menu li a[href*="Aulas"] .menu-text { color: white; }
    </style>
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="MainContent" runat="server">
    <div class="aulas-container">
        <div class="aulas-header">
            <div>
                <h2><asp:Literal ID="litTitulo" runat="server" Text="Aulas" /></h2>
                <p><asp:Literal ID="litSubtitulo" runat="server" /></p>
            </div>
            <asp:LinkButton ID="btnNuevaAula" runat="server" CssClass="btn-nueva" OnClick="btnNuevaAula_Click" CausesValidation="false">
                <i class="bi bi-plus-lg"></i> <asp:Literal ID="litBtnNueva" runat="server" Text="Nueva aula" />
            </asp:LinkButton>
        </div>

        <div class="aulas-card">
            <table class="aulas-tabla">
                <thead>
                    <tr>
                        <th><%= T("aulas_col_nombre") %></th>
                        <th><%= T("aulas_col_cupo") %></th>
                        <th><%= T("aulas_col_turnos") %></th>
                        <th><%= T("dash_col_estado") %></th>
                        <th></th>
                    </tr>
                </thead>
                <tbody>
                    <asp:Repeater ID="rptAulas" runat="server" OnItemCommand="rptAulas_ItemCommand">
                        <ItemTemplate>
                            <tr>
                                <td><%#: Eval("Nombre") %></td>
                                <td class="numero"><%#: Eval("Cupo") %></td>
                                <td class="numero"><%#: Eval("Turnos") %></td>
                                <td><span class="estado-pill <%# (bool)Eval("Activo") ? "activa" : "inactiva" %>"><%#: Eval("EstadoTexto") %></span></td>
                                <td>
                                    <div class="acciones-fila">
                                        <asp:LinkButton runat="server" CssClass="btn-link-accion" CommandName="Editar" CommandArgument='<%# Eval("CodAula") %>' CausesValidation="false"><%# T("actividades_btn_editar") %></asp:LinkButton>
                                        <asp:LinkButton runat="server" CssClass="btn-link-accion peligro" CommandName="Desactivar" CommandArgument='<%# Eval("CodAula") %>' CausesValidation="false" Visible='<%# (bool)Eval("Activo") %>'><%# T("actividades_btn_desactivar") %></asp:LinkButton>
                                        <asp:LinkButton runat="server" CssClass="btn-link-accion" CommandName="Activar" CommandArgument='<%# Eval("CodAula") %>' CausesValidation="false" Visible='<%# !(bool)Eval("Activo") %>'><%# T("actividades_btn_activar") %></asp:LinkButton>
                                    </div>
                                </td>
                            </tr>
                        </ItemTemplate>
                    </asp:Repeater>
                    <asp:PlaceHolder ID="phSinAulas" runat="server" Visible="false">
                        <tr><td colspan="5" class="texto-vacio"><%= T("aulas_sin_aulas") %></td></tr>
                    </asp:PlaceHolder>
                </tbody>
            </table>
        </div>

        <!-- Alta / modificación de aula -->
        <asp:Panel ID="pnlForm" runat="server" Visible="false" CssClass="modal-overlay">
            <div class="modal-content">
                <h3 class="modal-title"><asp:Literal ID="litFormTitulo" runat="server" /></h3>
                <div class="form-group">
                    <label for="<%= txtNombre.ClientID %>"><%= T("aulas_col_nombre") %></label>
                    <asp:TextBox ID="txtNombre" runat="server" MaxLength="100" />
                </div>
                <div class="form-group">
                    <label for="<%= txtCupo.ClientID %>"><%= T("aulas_campo_cupo") %></label>
                    <asp:TextBox ID="txtCupo" runat="server" TextMode="Number" step="1" min="1" max="1000" />
                    <small><%= T("aulas_campo_cupo_ayuda") %></small>
                </div>
                <div class="modal-actions">
                    <asp:Button ID="btnCancelar" runat="server" CssClass="btn-secondary" OnClick="btnCancelar_Click" CausesValidation="false" />
                    <asp:Button ID="btnGuardar" runat="server" CssClass="btn-submit" OnClick="btnGuardar_Click" CausesValidation="false" />
                </div>
            </div>
        </asp:Panel>
    </div>
</asp:Content>
