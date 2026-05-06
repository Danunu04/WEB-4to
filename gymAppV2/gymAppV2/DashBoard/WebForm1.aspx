<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="WebForm1.aspx.cs" Inherits="gymAppV2.DashBoard.WebForm1" MasterPageFile="~/DashBoard.Master" %>

<asp:Content ID="Content1" ContentPlaceHolderID="HeadContent" runat="server">
    <title>Dashboard - Sportio</title>
    <link href="<%= ResolveUrl("~/Content/dashboard.css") %>" rel="stylesheet" type="text/css" />
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="MainContent" runat="server">
    <div class="animate-fade-in">
        <div class="mb-6">
            <h1 class="font-display text-2xl font-bold">Panel de Administración</h1>
    <p>&nbsp;</p>
            <p class="text-muted text-sm">Bienvenido al panel de gestión</p>
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
                <p class="kpi-card-label">Miembros Activos</p>
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
                <p class="kpi-card-label">Clases este Mes</p>
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
                <p class="kpi-card-label">Ingresos (Mes)</p>
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
                <p class="kpi-card-label">Tasa Retención</p>
                <p class="kpi-card-value tabular-nums">87%</p>
            </div>
        </div>

        <!-- Two Column Layout -->
        <div class="grid grid-cols-1 lg:grid-cols-2 gap-6">
            <!-- Upcoming Classes -->
            <div class="card">
                <h3 class="card-title">Próximas Clases</h3>
                <div class="space-y-3">
                    <div class="flex items-center gap-4 p-3 rounded-lg hover:bg-surface-2 transition-colors">
                        <div class="avatar bg-pink">Y</div>
                        <div class="flex-1">
                            <h4 class="font-medium">Yoga Flow</h4>
                            <p class="text-sm text-muted">Ana García</p>
                        </div>
                        <div class="text-right">
                            <p class="text-sm font-medium font-mono tabular-nums">08:00</p>
                            <p class="text-xs text-muted">60 min</p>
                        </div>
                    </div>

                    <div class="flex items-center gap-4 p-3 rounded-lg hover:bg-surface-2 transition-colors">
                        <div class="avatar bg-lavender">C</div>
                        <div class="flex-1">
                            <h4 class="font-medium">CrossFit</h4>
                            <p class="text-sm text-muted">Carlos Ruiz</p>
                        </div>
                        <div class="text-right">
                            <p class="text-sm font-medium font-mono tabular-nums">09:30</p>
                            <p class="text-xs text-muted">45 min</p>
                        </div>
                    </div>

                    <div class="flex items-center gap-4 p-3 rounded-lg hover:bg-surface-2 transition-colors">
                        <div class="avatar bg-mint">P</div>
                        <div class="flex-1">
                            <h4 class="font-medium">Pilates</h4>
                            <p class="text-sm text-muted">María López</p>
                        </div>
                        <div class="text-right">
                            <p class="text-sm font-medium font-mono tabular-nums">11:00</p>
                            <p class="text-xs text-muted">50 min</p>
                        </div>
                    </div>

                    <div class="flex items-center gap-4 p-3 rounded-lg hover:bg-surface-2 transition-colors">
                        <div class="avatar bg-peach">H</div>
                        <div class="flex-1">
                            <h4 class="font-medium">HIIT</h4>
                            <p class="text-sm text-muted">David Chen</p>
                        </div>
                        <div class="text-right">
                            <p class="text-sm font-medium font-mono tabular-nums">17:00</p>
                            <p class="text-xs text-muted">30 min</p>
                        </div>
                    </div>

                    <div class="flex items-center gap-4 p-3 rounded-lg hover:bg-surface-2 transition-colors">
                        <div class="avatar bg-sky">Z</div>
                        <div class="flex-1">
                            <h4 class="font-medium">Zumba</h4>
                            <p class="text-sm text-muted">Sofía Martín</p>
                        </div>
                        <div class="text-right">
                            <p class="text-sm font-medium font-mono tabular-nums">19:00</p>
                            <p class="text-xs text-muted">55 min</p>
                        </div>
                    </div>
                </div>
            </div>

            <!-- Weekly Activity -->
            <div class="card">
                <h3 class="card-title">Actividad Semanal</h3>
                <div class="flex items-end justify-between h-40 gap-2">
                    <div class="flex-1 flex flex-col items-center gap-2">
                        <div class="w-full rounded-t-lg" style="height: 65%; background-color: var(--color-accent-pink); opacity: 0.8;"></div>
                        <span class="text-xs text-muted">L</span>
                    </div>
                    <div class="flex-1 flex flex-col items-center gap-2">
                        <div class="w-full rounded-t-lg" style="height: 78%; background-color: var(--color-accent-lavender); opacity: 0.8;"></div>
                        <span class="text-xs text-muted">M</span>
                    </div>
                    <div class="flex-1 flex flex-col items-center gap-2">
                        <div class="w-full rounded-t-lg" style="height: 52%; background-color: var(--color-accent-pink); opacity: 0.8;"></div>
                        <span class="text-xs text-muted">X</span>
                    </div>
                    <div class="flex-1 flex flex-col items-center gap-2">
                        <div class="w-full rounded-t-lg" style="height: 89%; background-color: var(--color-accent-lavender); opacity: 0.8;"></div>
                        <span class="text-xs text-muted">J</span>
                    </div>
                    <div class="flex-1 flex flex-col items-center gap-2">
                        <div class="w-full rounded-t-lg" style="height: 71%; background-color: var(--color-accent-pink); opacity: 0.8;"></div>
                        <span class="text-xs text-muted">V</span>
                    </div>
                    <div class="flex-1 flex flex-col items-center gap-2">
                        <div class="w-full rounded-t-lg" style="height: 45%; background-color: var(--color-accent-lavender); opacity: 0.8;"></div>
                        <span class="text-xs text-muted">S</span>
                    </div>
                    <div class="flex-1 flex flex-col items-center gap-2">
                        <div class="w-full rounded-t-lg" style="height: 62%; background-color: var(--color-accent-pink); opacity: 0.8;"></div>
                        <span class="text-xs text-muted">D</span>
                    </div>
                </div>
            </div>
        </div>
    </div>
</asp:Content>