<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="WebForm1.aspx.cs" Inherits="gymAppV2.DashBoard.WebForm1" MasterPageFile="~/DashBoard.Master" %>

<asp:Content ID="Content1" ContentPlaceHolderID="HeadContent" runat="server">
    <title>Dashboard - Sportio</title>
    <link href="<%= ResolveUrl("~/Content/dashboard.css?v=3") %>" rel="stylesheet" type="text/css" />
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="MainContent" runat="server">
    <div class="animate-fade-in">
        <div class="mb-6">
            <h1 class="font-display text-2xl font-bold"><asp:Literal ID="litTitulo" runat="server" Text="Panel de Administración" /></h1>
    <p>&nbsp;</p>
            <p class="text-muted text-sm"><asp:Literal ID="litSubtitulo" runat="server" Text="Bienvenido al panel de gestión" /></p>
        </div>

        <!-- KPI Cards -->
        <div class="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-4 gap-4 mb-6">
            <div class="kpi-card card-hover">
                <div class="kpi-card-header">
                    <div class="kpi-card-icon bg-pink-light">
                        <div class="kpi-card-icon-dot bg-pink"></div>
                    </div>
                    <div class="kpi-card-trend up">
                        <i class="bi bi-arrow-up-short"></i>
                        12.5%
                    </div>
                    <div class="kpi-card-trend up">&nbsp;</div>
                </div>
                <p class="kpi-card-label"><asp:Literal ID="litKpiMiembros" runat="server" Text="Miembros Activos" /></p>
                <p class="kpi-card-value tabular-nums">1,247</p>
            </div>

            <div class="kpi-card card-hover">
                <div class="kpi-card-header">
                    <div class="kpi-card-icon bg-mint-light">
                        <div class="kpi-card-icon-dot bg-mint"></div>
                    </div>
                    <div class="kpi-card-trend up">
                        <i class="bi bi-arrow-up-short"></i>
                        8.2%
                    </div>
                </div>
                <p class="kpi-card-label"><asp:Literal ID="litKpiClases" runat="server" Text="Clases este Mes" /></p>
                <p class="kpi-card-value tabular-nums">342</p>
            </div>

            <div class="kpi-card card-hover">
                <div class="kpi-card-header">
                    <div class="kpi-card-icon bg-lavender-light">
                        <div class="kpi-card-icon-dot bg-lavender"></div>
                    </div>
                    <div class="kpi-card-trend down">
                        <i class="bi bi-arrow-down-short"></i>
                        2.1%
                    </div>
                </div>
                <p class="kpi-card-label"><asp:Literal ID="litKpiIngresos" runat="server" Text="Ingresos (Mes)" /></p>
                <p class="kpi-card-value tabular-nums">$48.2K</p>
            </div>

            <div class="kpi-card card-hover">
                <div class="kpi-card-header">
                    <div class="kpi-card-icon bg-peach-light">
                        <div class="kpi-card-icon-dot bg-peach"></div>
                    </div>
                    <div class="kpi-card-trend up">
                        <i class="bi bi-arrow-up-short"></i>
                        4.3%
                    </div>
                </div>
                <p class="kpi-card-label"><asp:Literal ID="litKpiRetencion" runat="server" Text="Tasa Retención" /></p>
                <p class="kpi-card-value tabular-nums">87%</p>
            </div>
        </div>

        <!-- Tabla única de Actividades Semanales -->
        <div class="card">
            <h3 class="card-title"><asp:Literal ID="litSemanaTitulo" runat="server" Text="Actividades de la Semana" /></h3>
            <div class="table-container">
                <table class="table">
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
                        <tr>
                            <td>
                                <div class="flex items-center gap-3">
                                    <div class="avatar avatar-sm bg-pink">Y</div>
                                    <span class="font-medium">Yoga Flow</span>
                                </div>
                            </td>
                            <td>Ana García</td>
                            <td>Lunes</td>
                            <td class="font-mono tabular-nums">08:00</td>
                            <td class="text-muted">60 min</td>
                            <td><span class="badge badge-mint"><%= T("dash_badge_completada") %></span></td>
                        </tr>
                        <tr>
                            <td>
                                <div class="flex items-center gap-3">
                                    <div class="avatar avatar-sm bg-lavender">C</div>
                                    <span class="font-medium">CrossFit Intensivo</span>
                                </div>
                            </td>
                            <td>Carlos Ruiz</td>
                            <td>Martes</td>
                            <td class="font-mono tabular-nums">09:30</td>
                            <td class="text-muted">45 min</td>
                            <td><span class="badge badge-mint"><%= T("dash_badge_completada") %></span></td>
                        </tr>
                        <tr>
                            <td>
                                <div class="flex items-center gap-3">
                                    <div class="avatar avatar-sm bg-mint">P</div>
                                    <span class="font-medium">Pilates Core</span>
                                </div>
                            </td>
                            <td>María López</td>
                            <td>Miércoles</td>
                            <td class="font-mono tabular-nums">11:00</td>
                            <td class="text-muted">50 min</td>
                            <td><span class="badge badge-lavender"><%= T("dash_badge_pendiente") %></span></td>
                        </tr>
                        <tr>
                            <td>
                                <div class="flex items-center gap-3">
                                    <div class="avatar avatar-sm bg-peach">H</div>
                                    <span class="font-medium">HIIT Cardio</span>
                                </div>
                            </td>
                            <td>David Chen</td>
                            <td>Jueves</td>
                            <td class="font-mono tabular-nums">17:00</td>
                            <td class="text-muted">30 min</td>
                            <td><span class="badge badge-lavender"><%= T("dash_badge_pendiente") %></span></td>
                        </tr>
                        <tr>
                            <td>
                                <div class="flex items-center gap-3">
                                    <div class="avatar avatar-sm bg-sky">Z</div>
                                    <span class="font-medium">Zumba Fitness</span>
                                </div>
                            </td>
                            <td>Sofía Martín</td>
                            <td>Viernes</td>
                            <td class="font-mono tabular-nums">19:00</td>
                            <td class="text-muted">55 min</td>
                            <td><span class="badge badge-lavender"><%= T("dash_badge_pendiente") %></span></td>
                        </tr>
                        <tr>
                            <td>
                                <div class="flex items-center gap-3">
                                    <div class="avatar avatar-sm bg-pink">Y</div>
                                    <span class="font-medium">Yoga Relax</span>
                                </div>
                            </td>
                            <td>Ana García</td>
                            <td>Sábado</td>
                            <td class="font-mono tabular-nums">10:00</td>
                            <td class="text-muted">45 min</td>
                            <td><span class="badge badge-peach"><%= T("dash_badge_programada") %></span></td>
                        </tr>
                    </tbody>
                </table>
            </div>
        </div>
    </div>
</asp:Content>