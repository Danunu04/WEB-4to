<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="WebForm1.aspx.cs" Inherits="gymAppV2.DashBoard.WebForm1" MasterPageFile="~/DashBoard.Master" Title="Dashboard" %>

<asp:Content ID="Content1" ContentPlaceHolderID="HeadContent" runat="server">
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="MainContent" runat="server">
    <div class="animate-fade-in">
        <div class="mb-6">
            <h1 class="font-display text-2xl font-bold"><asp:Literal ID="litTitulo" runat="server" Text="Panel de Administración" /></h1>
            <p class="text-muted text-sm"><asp:Literal ID="litSubtitulo" runat="server" Text="Bienvenido al panel de gestión" /></p>
        </div>

        <!-- KPI Cards: solo para Administrador / Recepcionista / WebMaster -->
        <asp:Panel ID="pnlKpis" runat="server" CssClass="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-4 gap-4 mb-6">
            <div class="kpi-card card-hover">
                <div class="kpi-card-header">
                    <div class="kpi-card-icon bg-pink-light">
                        <div class="kpi-card-icon-dot bg-pink"></div>
                    </div>
                </div>
                <p class="kpi-card-label"><asp:Literal ID="litKpiMiembros" runat="server" Text="Miembros Activos" /></p>
                <p class="kpi-card-value tabular-nums"><asp:Literal ID="litValorMiembros" runat="server" Text="—" /></p>
            </div>

            <div class="kpi-card card-hover">
                <div class="kpi-card-header">
                    <div class="kpi-card-icon bg-mint-light">
                        <div class="kpi-card-icon-dot bg-mint"></div>
                    </div>
                </div>
                <p class="kpi-card-label"><asp:Literal ID="litKpiClases" runat="server" Text="Clases este Mes" /></p>
                <p class="kpi-card-value tabular-nums"><asp:Literal ID="litValorClases" runat="server" Text="—" /></p>
            </div>

            <div class="kpi-card card-hover">
                <div class="kpi-card-header">
                    <div class="kpi-card-icon bg-lavender-light">
                        <div class="kpi-card-icon-dot bg-lavender"></div>
                    </div>
                    <!-- Variación contra el mes anterior (solo si hay datos de ambos meses) -->
                    <asp:Panel ID="pnlTendenciaIngresos" runat="server" Visible="false">
                        <i id="iconTendencia" runat="server" class="bi bi-arrow-up-short"></i>
                        <asp:Literal ID="litTendenciaIngresos" runat="server" />
                    </asp:Panel>
                </div>
                <p class="kpi-card-label"><asp:Literal ID="litKpiIngresos" runat="server" Text="Ingresos (Mes)" /></p>
                <p class="kpi-card-value tabular-nums"><asp:Literal ID="litValorIngresos" runat="server" Text="—" /></p>
            </div>

            <div class="kpi-card card-hover">
                <div class="kpi-card-header">
                    <div class="kpi-card-icon bg-peach-light">
                        <div class="kpi-card-icon-dot bg-peach"></div>
                    </div>
                </div>
                <p class="kpi-card-label"><asp:Literal ID="litKpiAlDia" runat="server" Text="Cuotas al día" /></p>
                <p class="kpi-card-value tabular-nums"><asp:Literal ID="litValorAlDia" runat="server" Text="—" /></p>
            </div>
        </asp:Panel>

        <!-- Alertas de check-in rechazado por abono vencido (administración y recepción) -->
        <asp:Panel ID="pnlAlertasCheckin" runat="server" Visible="false" CssClass="card mb-6 alertas-checkin">
            <div class="flex items-center justify-between mb-4 alertas-checkin-header">
                <h3 class="card-title"><i class="bi bi-exclamation-triangle"></i> <asp:Literal ID="litAlertasTitulo" runat="server" Text="Check-in rechazados" /></h3>
                <asp:LinkButton ID="btnAlertasTodas" runat="server" CssClass="btn-alerta" OnClick="btnAlertasTodas_Click" CausesValidation="false">
                    <asp:Literal ID="litAlertasTodas" runat="server" Text="Marcar todas como vistas" />
                </asp:LinkButton>
            </div>
            <div class="table-container">
                <asp:Repeater ID="rptAlertas" runat="server" OnItemCommand="rptAlertas_ItemCommand">
                    <HeaderTemplate>
                        <table class="table tabla-tarjetas">
                            <thead>
                                <tr>
                                    <th><%# Texto("dash_alertas_col_fecha", "Fecha y hora") %></th>
                                    <th><%# Texto("dash_alertas_col_alumno", "Alumno") %></th>
                                    <th>DNI</th>
                                    <th><%# Texto("dash_alertas_col_situacion", "Situación") %></th>
                                    <th></th>
                                </tr>
                            </thead>
                            <tbody>
                    </HeaderTemplate>
                    <ItemTemplate>
                                <tr>
                                    <td class="tabular-nums"><%#: Eval("Fecha") %></td>
                                    <td><%#: Eval("Alumno") %></td>
                                    <td class="tabular-nums"><%# Eval("Dni") %></td>
                                    <td><span class="badge badge-peach"><%#: Eval("Situacion") %></span></td>
                                    <td>
                                        <asp:LinkButton ID="btnAlertaVista" runat="server" CssClass="btn-alerta btn-alerta-sm"
                                            CommandName="Vista" CommandArgument='<%# Eval("CodAlerta") %>' Text='<%# TextoBotonVista %>' />
                                    </td>
                                </tr>
                    </ItemTemplate>
                    <FooterTemplate>
                            </tbody>
                        </table>
                    </FooterTemplate>
                </asp:Repeater>
            </div>
        </asp:Panel>

        <!-- Abono de los alumnos del cliente (alumno o familiar) -->
        <asp:Panel ID="pnlAbonoCliente" runat="server" Visible="false" CssClass="mb-6">
            <div class="flex items-center justify-between mb-4">
                <h3 class="card-title"><asp:Literal ID="litAbonoTitulo" runat="server" Text="Mi abono" /></h3>
                <span class="text-muted text-sm"><asp:Literal ID="litAbonoHoy" runat="server" /></span>
            </div>
            <div class="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-4 gap-4">
                <asp:Repeater ID="rptAbonoCliente" runat="server">
                    <ItemTemplate>
                        <div class="kpi-card card-hover">
                            <div class="kpi-card-header">
                                <p class="kpi-card-label"><%#: Eval("Alumno") %></p>
                                <span class="badge <%# Eval("BadgeClase") %>"><%#: Eval("Estado") %></span>
                            </div>
                            <p class="kpi-card-value tabular-nums"><%#: Eval("Dias") %></p>
                            <p class="text-muted text-sm"><%#: Eval("Detalle") %></p>
                        </div>
                    </ItemTemplate>
                </asp:Repeater>
            </div>
        </asp:Panel>

        <!-- Horarios semanales de las actividades -->
        <div class="card">
            <h3 class="card-title"><asp:Literal ID="litSemanaTitulo" runat="server" Text="Actividades de la Semana" /></h3>
            <div class="table-container">
                <table class="table tabla-tarjetas">
                    <thead>
                        <tr>
                            <th><asp:Literal ID="litColActividad" runat="server" Text="Actividad" /></th>
                            <th><asp:Literal ID="litColInstructor" runat="server" Text="Instructor" /></th>
                            <th><asp:Literal ID="litColDia" runat="server" Text="Día" /></th>
                            <th><asp:Literal ID="litColHorario" runat="server" Text="Horario" /></th>
                            <th><asp:Literal ID="litColDuracion" runat="server" Text="Duración" /></th>
                            <th><asp:Literal ID="litColEstado" runat="server" Text="Estado" /></th>
                        </tr>
                    </thead>
                    <tbody>
                        <asp:Repeater ID="rptSemana" runat="server">
                            <ItemTemplate>
                                <tr>
                                    <td>
                                        <div class="flex items-center gap-3">
                                            <div class="avatar avatar-sm <%# Eval("Color") %>"><%#: Eval("Inicial") %></div>
                                            <span class="font-medium"><%#: Eval("Actividad") %></span>
                                        </div>
                                    </td>
                                    <td><%#: Eval("Instructor") %></td>
                                    <td><%#: Eval("Dia") %></td>
                                    <td class="font-mono tabular-nums"><%#: Eval("Horario") %></td>
                                    <td class="text-muted"><%#: Eval("Duracion") %></td>
                                    <td><span class="badge <%# Eval("BadgeClase") %>"><%#: Eval("Estado") %></span></td>
                                </tr>
                            </ItemTemplate>
                        </asp:Repeater>
                        <asp:PlaceHolder ID="phSinHorarios" runat="server" Visible="false">
                            <tr>
                                <td colspan="6" class="text-muted"><asp:Literal ID="litSinHorarios" runat="server" Text="No hay actividades con horarios cargados." /></td>
                            </tr>
                        </asp:PlaceHolder>
                    </tbody>
                </table>
            </div>
        </div>
    </div>
</asp:Content>
