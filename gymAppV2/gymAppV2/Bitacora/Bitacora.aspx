<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="Bitacora.aspx.cs" Inherits="gymAppV2.Bitacora.Bitacora" MasterPageFile="~/DashBoard.Master" %>

<asp:Content ID="HeadContent" ContentPlaceHolderID="HeadContent" runat="server">
    <title>Bitácora - GymApp</title>
    <link href="<%= ResolveUrl("~/Bitacora/Bitacora.css") %>" rel="stylesheet" type="text/css" />
</asp:Content>

<asp:Content ID="MainContent" ContentPlaceHolderID="MainContent" runat="server">
    <div class="bitacora-container">
        <header class="bitacora-header">
            <div class="bitacora-header-content">
                <h1 class="bitacora-title">Bitácora de Eventos</h1>
                <p class="bitacora-subtitle">Monitor de actividad del sistema</p>
            </div>
            <div class="bitacora-status">
                <div class="status-indicator">
                    <span class="status-dot animate-pulse"></span>
                    <span class="status-text">Sistema Online</span>
                </div>
                <div class="bitacora-clock" id="bitacoraClock"></div>
            </div>
        </header>

        <div class="bitacora-main">
            <asp:Label ID="lblError" runat="server" Visible="false" CssClass="error-message"></asp:Label>

            <asp:Panel ID="pnlLoading" runat="server" CssClass="loading-container">
                <div class="skeleton skeleton-card"></div>
                <div class="skeleton skeleton-card"></div>
                <div class="skeleton skeleton-card"></div>
                <div class="skeleton skeleton-card"></div>
                <div class="skeleton skeleton-card"></div>
            </asp:Panel>

            <asp:Panel ID="pnlContent" runat="server" Visible="false">
                <div class="stats-grid">
                    <div class="stat-card">
                        <div class="stat-value"><asp:Label ID="lblTotal" runat="server" Text="0"></asp:Label></div>
                        <div class="stat-label">Total</div>
                    </div>
                    <div class="stat-card">
                        <div class="stat-value"><asp:Label ID="lblLogins" runat="server" Text="0"></asp:Label></div>
                        <div class="stat-label">Logins</div>
                    </div>
                    <div class="stat-card">
                        <div class="stat-value"><asp:Label ID="lblUsuariosNuevos" runat="server" Text="0"></asp:Label></div>
                        <div class="stat-label">Usuarios Nuevos</div>
                    </div>
                    <div class="stat-card">
                        <div class="stat-value"><asp:Label ID="lblErrores" runat="server" Text="0"></asp:Label></div>
                        <div class="stat-label">Errores</div>
                    </div>
                </div>

                <div class="filter-bar">
                    <asp:TextBox ID="txtBusqueda" runat="server" CssClass="search-input" placeholder="Buscar por usuario o acción..." AutoPostBack="true" OnTextChanged="txtBusqueda_TextChanged"></asp:TextBox>
                    <asp:DropDownList ID="ddlCriticidad" runat="server" CssClass="filter-select" AutoPostBack="true" OnSelectedIndexChanged="ddlCriticidad_SelectedIndexChanged">
                        <asp:ListItem Value="" Text="Todas las criticidades" />
                        <asp:ListItem Value="1" Text="Alta (1)" />
                        <asp:ListItem Value="2" Text="Media Alta (2)" />
                        <asp:ListItem Value="3" Text="Media Baja (3)" />
                        <asp:ListItem Value="4" Text="Baja (4)" />
                    </asp:DropDownList>
                    <asp:DropDownList ID="ddlModulo" runat="server" CssClass="filter-select" AutoPostBack="true" OnSelectedIndexChanged="ddlModulo_SelectedIndexChanged">
                    </asp:DropDownList>
                </div>

                <div class="filter-buttons">
                    <asp:Button ID="btnTodos" runat="server" Text="Todos" CssClass="filter-btn active" CommandArgument="all" OnClick="btnFiltro_Click" />
                    <asp:Button ID="btnLogin" runat="server" Text="Login" CssClass="filter-btn" CommandArgument="login" OnClick="btnFiltro_Click" />
                    <asp:Button ID="btnLogout" runat="server" Text="Logout" CssClass="filter-btn" CommandArgument="logout" OnClick="btnFiltro_Click" />
                    <asp:Button ID="btnBackup" runat="server" Text="Backup" CssClass="filter-btn" CommandArgument="backup" OnClick="btnFiltro_Click" />
                    <asp:Button ID="btnUsuarioNuevo" runat="server" Text="Usuario Nuevo" CssClass="filter-btn" CommandArgument="new_user" OnClick="btnFiltro_Click" />
                    <asp:Button ID="btnActualizacion" runat="server" Text="Actualización" CssClass="filter-btn" CommandArgument="update" OnClick="btnFiltro_Click" />
                    <asp:Button ID="btnError" runat="server" Text="Error" CssClass="filter-btn" CommandArgument="error" OnClick="btnFiltro_Click" />
                </div>

                <asp:Panel ID="pnlEventos" runat="server" CssClass="events-list">
                    <asp:Repeater ID="rptEventos" runat="server" OnItemCommand="rptEventos_ItemCommand">
                        <ItemTemplate>
                            <div class="event-card event-card-<%# Eval("EVENTO_Tipo") %>">
                                <div class="event-card-header">
                                    <div class="event-card-left">
                                        <div class="event-icon">
                                            <%# GetIconForType(Eval("EVENTO_Tipo").ToString()) %>
                                        </div>
                                        <div class="event-info">
                                            <div class="event-type">
                                                <%# GetLabelForType(Eval("EVENTO_Tipo").ToString()) %>
                                                <span class="criticidad-badge criticidad-<%# Eval("EVENTO_Criticidad") %>">
                                                    <%# GetCriticidadLabel(Eval("EVENTO_Criticidad")) %>
                                                </span>
                                            </div>
                                            <div class="event-user"><%# Eval("EVENTO_Usuario") %></div>
                                            <%# !string.IsNullOrEmpty(Eval("EVENTO_Modulo").ToString()) ? "<div class=\"event-modulo\">Módulo: " + Eval("EVENTO_Modulo") + "</div>" : "" %>
                                        </div>
                                    </div>
                                    <div class="event-card-right">
                                        <div class="event-time"><%# Eval("EVENTO_Timestamp", "{0:HH:mm}") %></div>
                                        <div class="event-date"><%# Eval("EVENTO_Timestamp", "{0:dd MMM}") %></div>
                                    </div>
                                </div>
                                <asp:Panel ID="pnlDetails" runat="server" Visible='<%# Convert.ToBoolean(Eval("Expandido")) %>' CssClass="event-details">
                                    <p class="event-action"><%# Eval("EVENTO_Accion") %></p>
                                    <p class="event-id">ID: EVT-<%# Eval("EVENTO_Id", "{0:D5}") %></p>
                                </asp:Panel>
                                <asp:LinkButton ID="lnkToggle" runat="server" CommandName="Toggle" CommandArgument='<%# Eval("EVENTO_Id") %>' CssClass="event-toggle"></asp:LinkButton>
                            </div>
                        </ItemTemplate>
                    </asp:Repeater>
                </asp:Panel>

                <asp:Panel ID="pnlNoEventos" runat="server" Visible="false" CssClass="no-events">
                    <div class="no-events-icon"></div>
                    <p>No se encontraron eventos</p>
                </asp:Panel>
            </asp:Panel>
        </div>
    </div>

    <script>
        (function () {
            function updateClock() {
                var now = new Date();
                var timeString = now.toLocaleTimeString('es-AR', { hour: '2-digit', minute: '2-digit' });
                var clockElement = document.getElementById('bitacoraClock');
                if (clockElement) {
                    clockElement.textContent = timeString;
                }
            }

            updateClock();
            setInterval(updateClock, 1000);
        })();
    </script>
</asp:Content>