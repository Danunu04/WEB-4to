<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="actividades.aspx.cs" Inherits="gymAppV2.Actividades.actividades" MasterPageFile="~/DashBoard.Master" Title="Actividades" %>

<asp:Content ID="Content1" ContentPlaceHolderID="HeadContent" runat="server">
    <style>
        :root {
            --color-bg: #FFF8F0;
            --color-surface: #FFFFFF;
            --color-surface-2: #FDF6EC;
            --color-text: #2D2D2D;
            --color-muted: #6B6B6B;
            --color-border: #E8E0D5;
            --color-accent-pink: #FFB5C5;
            --color-accent-pink-light: #FFE5EB;
            --color-accent-mint: #B5EAD7;
            --color-accent-mint-light: #E5F9F0;
            --color-accent-lavender: #C7B5FF;
            --color-accent-lavender-light: #F0EBFF;
            --color-accent-peach: #FFD5B5;
            --color-accent-peach-light: #FFF0E5;
            --color-accent-sky: #B5D5FF;
            --color-accent-sky-light: #E5F0FF;
            --radius-sm: 0.375rem;
            --radius-md: 0.5rem;
            --radius-lg: 0.75rem;
            --radius-xl: 1rem;
        }
        .calendar-container { max-width: 100%; margin: 0; padding: 1.5rem; }
        .calendar-header { display: flex; align-items: center; justify-content: space-between; flex-wrap: wrap; gap: 0.75rem; margin-bottom: 1.25rem; }
        .calendar-title h2 { font-family: 'Fraunces', serif; font-size: 1.5rem; font-weight: 700; color: var(--color-text); margin: 0; }
        .calendar-title p { color: var(--color-muted); margin: 0.25rem 0 0 0; font-size: 0.875rem; }
        .calendar-nav { display: flex; align-items: center; gap: 0.5rem; flex-wrap: wrap; }
        .calendar-nav button { padding: 0.5rem; border-radius: var(--radius-lg); border: 1px solid var(--color-border); background: var(--color-surface); cursor: pointer; }
        .calendar-nav button:hover { background: var(--color-surface-2); }
        .btn-primary { display: flex; align-items: center; gap: 0.5rem; padding: 0.5rem 1rem; background: var(--color-accent-lavender); color: white; border: none; border-radius: var(--radius-lg); cursor: pointer; font-weight: 500; }
        .btn-primary:hover { transform: translateY(-2px); }

        .calendar-grid { background: var(--color-surface); border: 1px solid var(--color-border); border-radius: var(--radius-xl); overflow: hidden; }
        .calendar-weekdays { display: grid; grid-template-columns: repeat(7, 1fr); background: var(--color-surface-2); border-bottom: 1px solid var(--color-border); }
        .calendar-weekdays div { padding: 0.75rem; text-align: center; font-size: 0.875rem; font-weight: 500; color: var(--color-muted); }
        .calendar-days { display: grid; grid-template-columns: repeat(7, 1fr); }
        .calendar-day { min-height: 6rem; padding: 0.5rem; border-bottom: 1px solid var(--color-border); border-right: 1px solid var(--color-border); cursor: pointer; transition: background-color 120ms; }
        .calendar-day:hover { background: var(--color-surface-2); }
        .calendar-day.selected { background: var(--color-accent-lavender-light); }
        .calendar-day.empty { background: var(--color-bg); opacity: 0.5; pointer-events: none; }
        .calendar-day:nth-child(7n) { border-right: none; }
        .day-number { font-size: 0.875rem; font-weight: 500; color: var(--color-muted); margin-bottom: 0.25rem; }
        .class-item { font-size: 0.625rem; padding: 0.125rem 0.375rem; border-radius: var(--radius-sm); background: var(--color-surface); border: 1px solid var(--color-border); margin-bottom: 0.125rem; overflow: hidden; text-overflow: ellipsis; white-space: nowrap; }
        .class-item.pink { border-left: 2px solid var(--color-accent-pink); }
        .class-item.mint { border-left: 2px solid var(--color-accent-mint); }
        .class-item.lavender { border-left: 2px solid var(--color-accent-lavender); }
        .class-item.peach { border-left: 2px solid var(--color-accent-peach); }
        .class-item.sky { border-left: 2px solid var(--color-accent-sky); }
        .class-name { font-weight: 500; color: var(--color-text); overflow: hidden; text-overflow: ellipsis; }
        .class-time { font-size: 0.625rem; color: var(--color-muted); font-family: 'JetBrains Mono', monospace; }
        .more-classes { font-size: 0.625rem; text-align: center; color: var(--color-muted); }

        .selected-day-panel { margin-top: 1.25rem; background: var(--color-surface); border: 1px solid var(--color-border); border-radius: var(--radius-xl); padding: 1rem; animation: fadeIn 0.3s ease-out; }
        .selected-day-panel h3 { font-family: 'Fraunces', serif; font-size: 1.125rem; font-weight: 700; color: var(--color-text); margin: 0 0 0.75rem 0; }
        .selected-class { display: flex; align-items: center; justify-content: space-between; padding: 0.75rem; border-radius: var(--radius-lg); background: var(--color-surface-2); margin-bottom: 0.5rem; }
        .selected-class:last-child { margin-bottom: 0; }
        .selected-class-info { display: flex; align-items: center; gap: 0.75rem; }
        .class-icon { width: 2.5rem; height: 2.5rem; border-radius: var(--radius-lg); display: flex; align-items: center; justify-content: center; font-family: 'Fraunces', serif; font-weight: 700; color: white; font-size: 0.875rem; }
        .class-icon.pink { background: var(--color-accent-pink); }
        .class-icon.mint { background: var(--color-accent-mint); }
        .class-icon.lavender { background: var(--color-accent-lavender); }
        .class-icon.peach { background: var(--color-accent-peach); }
        .class-icon.sky { background: var(--color-accent-sky); }
        .class-details h4 { font-weight: 500; color: var(--color-text); margin: 0; }
        .class-details p { font-size: 0.875rem; color: var(--color-muted); font-family: 'JetBrains Mono', monospace; margin: 0.125rem 0 0 0; }
        .btn-details { font-size: 0.875rem; font-weight: 500; color: var(--color-accent-lavender); background: none; border: none; cursor: pointer; text-decoration: underline; }

        .modal-overlay { position: fixed; inset: 0; background: rgba(0, 0, 0, 0.5); display: flex; align-items: center; justify-content: center; z-index: 1000; padding: 1rem; }
        .modal-content { background: var(--color-surface); border: 1px solid var(--color-border); border-radius: var(--radius-xl); padding: 1.5rem; width: 100%; max-width: 28rem; animation: fadeIn 0.3s ease-out; }
        .modal-header { display: flex; align-items: center; justify-content: space-between; margin-bottom: 1rem; }
        .modal-title { font-family: 'Fraunces', serif; font-size: 1.25rem; font-weight: 700; color: var(--color-text); margin: 0; }
        .modal-close { padding: 0.5rem; border-radius: var(--radius-lg); background: none; border: none; cursor: pointer; }
        .modal-close:hover { background: var(--color-surface-2); }
        .form-group { margin-bottom: 1rem; }
        .form-group label { display: block; font-size: 0.875rem; font-weight: 500; color: var(--color-text); margin-bottom: 0.25rem; }
        .form-group input, .form-group select { width: 100%; padding: 0.5rem 0.75rem; border-radius: var(--radius-lg); border: 1px solid var(--color-border); font-family: 'DM Sans', sans-serif; }
        .form-group input:focus, .form-group select:focus { outline: none; box-shadow: 0 0 0 2px var(--color-accent-lavender); }
        .form-row { display: grid; grid-template-columns: 1fr 1fr; gap: 0.75rem; }
        .modal-actions { display: flex; gap: 0.5rem; padding-top: 1rem; border-top: 1px solid var(--color-border); }
        .btn-secondary { flex: 1; padding: 0.5rem 1rem; border-radius: var(--radius-lg); border: 1px solid var(--color-border); background: var(--color-surface); color: var(--color-muted); cursor: pointer; font-weight: 500; }
        .btn-secondary:hover { background: var(--color-surface-2); }
        .btn-submit { flex: 1; padding: 0.5rem 1rem; border-radius: var(--radius-lg); border: none; background: var(--color-accent-lavender); color: white; cursor: pointer; font-weight: 500; }
        .btn-submit:hover { transform: translateY(-2px); }

        @keyframes fadeIn { from { opacity: 0; transform: translateY(8px); } to { opacity: 1; transform: translateY(0); } }
        .animate-fade-in { animation: fadeIn 0.3s ease-out; }

        .cliente-info-panel {
            margin-top: 1rem;
            background: var(--color-accent-sky-light, #E5F0FF);
            border: 1px solid var(--color-accent-sky, #B5D5FF);
            border-radius: var(--radius-xl, 0.75rem);
            padding: 0.75rem 1rem;
            display: flex;
            align-items: center;
            gap: 0.5rem;
            color: var(--color-text, #2D2D2D);
            font-size: 0.875rem;
        }

        .cliente-info-panel i {
            color: var(--color-accent-sky, #B5D5FF);
            font-size: 1rem;
        }

        /* Highlight active nav link */
        .sidebar-menu li a[href*="Actividades"] {
            background: var(--color-accent-lavender);
            color: white;
        }
        .sidebar-menu li a[href*="Actividades"] i,
        .sidebar-menu li a[href*="Actividades"] .menu-text {
            color: white;
        }
        /* ── Gestión de actividades ── */
        .gestion-card { margin-top: 1.5rem; }
        .gestion-card .table td { vertical-align: middle; }
        .acciones-fila { display: flex; gap: 0.5rem; flex-wrap: wrap; }
        .btn-link-accion { background: none; border: none; padding: 0; font-size: 0.875rem; font-weight: 500; color: var(--color-accent-lavender); cursor: pointer; text-decoration: underline; }
        .btn-link-accion.peligro { color: #C0392B; }
        .estado-pill { display: inline-block; padding: 0.125rem 0.5rem; border-radius: 62.5rem; font-size: 0.75rem; font-weight: 600; }
        .estado-pill.activa { background: var(--color-accent-mint-light); color: #0F6E56; }
        .estado-pill.inactiva { background: #F1EFE8; color: #5F5E5A; }
        .modal-content.modal-wide { max-width: 42rem; max-height: 90vh; overflow-y: auto; }
        .seccion-form { margin-top: 1rem; padding-top: 1rem; border-top: 0.0625rem solid var(--color-border); }
        .seccion-form h4 { font-size: 0.9375rem; font-weight: 600; color: var(--color-text); margin: 0 0 0.75rem 0; }
        .fila-horario { padding: 0.75rem; margin-bottom: 0.5rem; border: 0.0625rem solid var(--color-border); border-radius: var(--radius-lg); background: var(--color-surface-2); }
        .fila-horario-tiempo { display: grid; grid-template-columns: 1.4fr 1fr 1fr auto; gap: 0.5rem; align-items: end; }
        .fila-horario-asignacion { display: grid; grid-template-columns: repeat(3, minmax(0, 1fr)); gap: 0.5rem; margin-top: 0.5rem; }
        .fila-horario .form-group { margin-bottom: 0; }
        .btn-agregar { background: var(--color-accent-lavender-light); color: #4C1D95; border: 0.0625rem dashed var(--color-accent-lavender); border-radius: var(--radius-lg); padding: 0.5rem 0.75rem; cursor: pointer; font-weight: 500; font-size: 0.875rem; }
        .lista-checks { max-height: 16rem; overflow-y: auto; border: 0.0625rem solid var(--color-border); border-radius: var(--radius-lg); padding: 0.5rem 0.75rem; }
        .lista-checks td { padding: 0.25rem 0; }
        .lista-checks input[type="checkbox"] { margin-right: 0.5rem; accent-color: var(--color-accent-lavender); }
        .texto-vacio { color: var(--color-muted); font-size: 0.875rem; }
        .form-check-inline { display: flex; align-items: center; gap: 0.5rem; }
        .form-check-inline input { width: auto; }
        .mis-actividades-header { display: flex; align-items: flex-end; justify-content: space-between; flex-wrap: wrap; gap: 0.75rem; margin-bottom: 0.75rem; }
        .mis-actividades-header .card-title { margin: 0; }
        .selector-alumno { margin-bottom: 0; min-width: 14rem; }
        .ayuda { margin: 0.25rem 0 0 0; }
        .clase-detalle { margin: 0.25rem 0 0 0; font-size: 0.875rem; color: var(--color-muted); }
        .estado-clase { font-weight: 600; color: var(--color-text); margin: 0 0 0.5rem 0; }
        .aviso-clase { background: var(--color-accent-peach-light); border: 0.0625rem solid var(--color-accent-peach); border-radius: var(--radius-lg); padding: 0.5rem 0.75rem; font-size: 0.875rem; }
        .lista-alcance { display: flex; flex-direction: column; gap: 0.375rem; }
        .lista-alcance input[type="radio"] { margin-right: 0.5rem; accent-color: var(--color-accent-lavender); }
        .lista-alcance label { font-size: 0.875rem; color: var(--color-text); }
        .btn-submit.btn-peligro { background: #C0392B; }
        .fila-asistente { display: flex; align-items: center; justify-content: space-between; gap: 0.5rem; flex-wrap: wrap; padding: 0.375rem 0; border-bottom: 0.0625rem solid var(--color-border); }
        .fila-asistente:last-child { border-bottom: none; }
        .class-item.anotado { background: var(--color-accent-mint-light); }
        .class-item.no-anotado { opacity: 0.55; }
        .estado-pill.puntual { background: var(--color-accent-sky-light); color: #1E4E8C; }
        .acciones-clase { display: flex; align-items: center; gap: 0.75rem; flex-wrap: wrap; }

        /* ── Selector de vista ── */
        .vista-switch { display: inline-flex; border: 0.0625rem solid var(--color-border); border-radius: var(--radius-lg); overflow: hidden; background: var(--color-surface); }
        .vista-switch button { padding: 0.4375rem 0.875rem; border: none; background: transparent; color: var(--color-muted); font-size: 0.875rem; font-weight: 500; cursor: pointer; }
        .vista-switch button + button { border-left: 0.0625rem solid var(--color-border); }
        .vista-switch button.activa { background: var(--color-accent-lavender); color: white; }
        .btn-hoy { padding: 0.4375rem 0.875rem; border-radius: var(--radius-lg); border: 0.0625rem solid var(--color-border); background: var(--color-surface); font-size: 0.875rem; font-weight: 500; cursor: pointer; }
        .btn-hoy:hover { background: var(--color-surface-2); }

        /* ── Vista mensual ── */
        .calendar-day { display: flex; flex-direction: column; gap: 0.125rem; min-width: 0; }
        .calendar-day.pasado { background: var(--color-bg); }
        .calendar-day.hoy .day-number { background: var(--color-accent-lavender); color: white; border-radius: 62.5rem; width: 1.5rem; height: 1.5rem; display: flex; align-items: center; justify-content: center; }
        .calendar-days .class-item { display: flex; gap: 0.25rem; align-items: baseline; min-width: 0; font-size: 0.6875rem; }
        .calendar-days .class-item .class-time { flex: 0 0 auto; }
        .calendar-days .class-item .class-name { min-width: 0; white-space: nowrap; overflow: hidden; text-overflow: ellipsis; }
        .calendar-days .more-classes { background: none; border: none; padding: 0; text-align: left; font-size: 0.6875rem; font-weight: 600; color: var(--color-accent-lavender); cursor: pointer; }
        .dots-mes { display: none; flex-wrap: wrap; gap: 0.1875rem; }
        .dot { width: 0.4375rem; height: 0.4375rem; border-radius: 62.5rem; background: var(--color-border); }
        .dot.pink { background: var(--color-accent-pink); }
        .dot.mint { background: var(--color-accent-mint); }
        .dot.lavender { background: var(--color-accent-lavender); }
        .dot.peach { background: var(--color-accent-peach); }
        .dot.sky { background: var(--color-accent-sky); }
        .dot.no-anotado { opacity: 0.35; }
        .dots-mas { font-size: 0.625rem; line-height: 0.4375rem; color: var(--color-muted); }

        /* ── Vista semanal ── */
        .semana-scroll { overflow-x: auto; }
        .semana { min-width: 46rem; }
        .semana-header, .semana-cuerpo { display: grid; grid-template-columns: 3.5rem repeat(7, minmax(0, 1fr)); }
        .semana-header { background: var(--color-surface-2); border-bottom: 0.0625rem solid var(--color-border); }
        .semana-dia-titulo { padding: 0.5rem 0.25rem; border: none; border-left: 0.0625rem solid var(--color-border); background: transparent; font-size: 0.8125rem; color: var(--color-muted); cursor: pointer; }
        .semana-dia-titulo strong { display: block; font-size: 1rem; color: var(--color-text); }
        .semana-dia-titulo.hoy strong { color: var(--color-accent-lavender); }
        .semana-horas { position: relative; }
        .semana-hora { position: absolute; right: 0.375rem; transform: translateY(-50%); font-size: 0.6875rem; color: var(--color-muted); font-family: 'JetBrains Mono', monospace; }
        .semana-columna { position: relative; border-left: 0.0625rem solid var(--color-border); background-image: repeating-linear-gradient(to bottom, var(--color-border) 0, var(--color-border) 0.0625rem, transparent 0.0625rem, transparent 3.5rem); }
        .semana-columna.hoy { background-color: var(--color-accent-lavender-light); }
        .bloque-clase { position: absolute; box-sizing: border-box; overflow: hidden; padding: 0.25rem 0.375rem; border-radius: var(--radius-sm); border-left: 0.1875rem solid var(--color-border); background: var(--color-surface-2); font-size: 0.6875rem; line-height: 1.25; cursor: pointer; }
        .bloque-clase.pink { background: var(--color-accent-pink-light); border-left-color: var(--color-accent-pink); }
        .bloque-clase.mint { background: var(--color-accent-mint-light); border-left-color: var(--color-accent-mint); }
        .bloque-clase.lavender { background: var(--color-accent-lavender-light); border-left-color: var(--color-accent-lavender); }
        .bloque-clase.peach { background: var(--color-accent-peach-light); border-left-color: var(--color-accent-peach); }
        .bloque-clase.sky { background: var(--color-accent-sky-light); border-left-color: var(--color-accent-sky); }
        .bloque-clase.no-anotado { opacity: 0.55; }
        .bloque-clase.seleccionada { outline: 0.125rem solid var(--color-accent-lavender); outline-offset: -0.125rem; opacity: 1; }
        .bloque-clase.sin-accion { cursor: default; }
        .bloque-nombre { font-weight: 600; color: var(--color-text); white-space: nowrap; overflow: hidden; text-overflow: ellipsis; }
        .bloque-hora { color: var(--color-muted); font-family: 'JetBrains Mono', monospace; white-space: nowrap; }
        .bloque-check { position: absolute; top: 0.1875rem; right: 0.1875rem; margin: 0; width: 0.875rem; height: 0.875rem; accent-color: var(--color-accent-lavender); cursor: pointer; }

        /* ── Vista diaria ── */
        .dia-lista { display: flex; flex-direction: column; gap: 1.25rem; padding: 1rem; }
        .dia-grupo h4 { margin: 0 0 0.5rem 0; font-size: 0.875rem; color: var(--color-muted); font-family: 'JetBrains Mono', monospace; font-weight: 600; }
        .dia-tarjetas { display: grid; grid-template-columns: repeat(auto-fill, minmax(16rem, 1fr)); gap: 0.75rem; }
        .tarjeta-clase { display: flex; flex-direction: column; gap: 0.5rem; padding: 0.75rem; border: 0.0625rem solid var(--color-border); border-left: 0.25rem solid var(--color-border); border-radius: var(--radius-lg); background: var(--color-surface); }
        .tarjeta-clase.pink { border-left-color: var(--color-accent-pink); }
        .tarjeta-clase.mint { border-left-color: var(--color-accent-mint); }
        .tarjeta-clase.lavender { border-left-color: var(--color-accent-lavender); }
        .tarjeta-clase.peach { border-left-color: var(--color-accent-peach); }
        .tarjeta-clase.sky { border-left-color: var(--color-accent-sky); }
        .tarjeta-clase.seleccionada { outline: 0.125rem solid var(--color-accent-lavender); }
        .tarjeta-clase h5 { margin: 0; font-size: 1rem; font-weight: 600; color: var(--color-text); }
        .tarjeta-clase p { margin: 0; font-size: 0.8125rem; color: var(--color-muted); }
        .dia-vacio { padding: 2rem 1rem; text-align: center; color: var(--color-muted); }

        /* ── Clases con el cupo completo: en gris ── */
        .calendar-days .class-item.llena, .bloque-clase.llena { background: #EEECEA; border-left-color: #B4B2A9; opacity: 1; }
        .calendar-days .class-item.llena .class-name, .bloque-clase.llena .bloque-nombre { color: #888780; }
        .tarjeta-clase.llena { background: #F5F4F2; border-left-color: #B4B2A9; }
        .tarjeta-clase.llena h5 { color: #888780; }
        .dot.llena { background: #B4B2A9; opacity: 1; }
        .cupo-texto { font-size: 0.75rem; color: var(--color-muted); font-family: 'JetBrains Mono', monospace; }
        .cupo-texto.llena { color: #A32D2D; font-weight: 600; }
        .bloque-cupo { font-size: 0.625rem; color: var(--color-muted); white-space: nowrap; }
        .clase-cupo { font-family: 'JetBrains Mono', monospace; }

        @media (max-width: 40rem) {
            .calendar-day { min-height: 3.5rem; padding: 0.25rem; }
            .calendar-days .class-item, .calendar-days .more-classes { display: none; }
            .dots-mes { display: flex; }
        }
        .class-item.seleccionable { cursor: pointer; }
        .class-item.seleccionada { outline: 0.125rem solid var(--color-accent-lavender); outline-offset: -0.125rem; background: var(--color-accent-lavender-light); opacity: 1; }
        .check-seleccion { display: flex; align-items: center; cursor: pointer; }
        .check-seleccion input { width: 1.125rem; height: 1.125rem; accent-color: var(--color-accent-lavender); cursor: pointer; }
        .barra-seleccion { position: sticky; bottom: 1rem; z-index: 50; margin-top: 1rem; display: flex; align-items: center; justify-content: space-between; flex-wrap: wrap; gap: 0.75rem; padding: 0.75rem 1rem; background: var(--color-surface); border: 0.0625rem solid var(--color-accent-lavender); border-radius: var(--radius-xl); box-shadow: 0 0.5rem 1.5rem rgba(0, 0, 0, 0.12); animation: fadeIn 0.2s ease-out; }
        .texto-seleccion { font-weight: 600; color: var(--color-text); }
        .barra-acciones { display: flex; gap: 0.5rem; flex-wrap: wrap; }
        .barra-acciones .btn-secondary, .barra-acciones .btn-submit { flex: 0 0 auto; }
        .barra-acciones .btn-submit:disabled { opacity: 0.45; cursor: not-allowed; transform: none; }
        @media (max-width: 40rem) {
            .fila-horario-tiempo { grid-template-columns: 1fr 1fr; }
            .fila-horario-asignacion { grid-template-columns: 1fr; }
        }
    </style>
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="MainContent" runat="server">
    <div class="calendar-container">
        <div class="calendar-header animate-fade-in">
            <div class="calendar-title">
                <h2><asp:Literal ID="litTitulo" runat="server" Text="Calendario de Actividades" /></h2>
                <p id="currentMonthDisplay"></p>
            </div>
            <div class="calendar-nav">
                <div class="vista-switch" role="group">
                    <button type="button" data-vista="mes"><%= T("actividades_vista_mes") %></button>
                    <button type="button" data-vista="semana"><%= T("actividades_vista_semana") %></button>
                    <button type="button" data-vista="dia"><%= T("actividades_vista_dia") %></button>
                </div>
                <button type="button" id="btnHoy" class="btn-hoy"><%= T("actividades_btn_hoy") %></button>
                <button type="button" id="prevMonth" class="btn-transition">
                    <svg width="20" height="20" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2">
                        <polyline points="15 18 9 12 15 6"/>
                    </svg>
                </button>
                <button type="button" id="nextMonth" class="btn-transition">
                    <svg width="20" height="20" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2">
                        <polyline points="9 18 15 12 9 6"/>
                    </svg>
                </button>
                <asp:LinkButton ID="btnNuevaActividad" runat="server" CssClass="btn-primary btn-transition" OnClick="btnNuevaActividad_Click" CausesValidation="false">
                    <svg width="18" height="18" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2">
                        <line x1="12" y1="5" x2="12" y2="19"/>
                        <line x1="5" y1="12" x2="19" y2="12"/>
                    </svg>
                    <span><asp:Literal ID="litBtnNueva" runat="server" Text="Nueva Actividad" /></span>
                </asp:LinkButton>

                <asp:HiddenField ID="hdnEsCliente" runat="server" Value="0" ClientIDMode="Static" />
                <asp:HiddenField ID="hdnActividadesJson" runat="server" Value="{}" ClientIDMode="Static" />
                <asp:HiddenField ID="hdnAgendaJson" runat="server" Value="{}" ClientIDMode="Static" />
                <asp:HiddenField ID="hdnOcupacionJson" runat="server" Value="{}" ClientIDMode="Static" />
                <asp:HiddenField ID="hdnPuedeGestionar" runat="server" Value="0" ClientIDMode="Static" />
                <asp:HiddenField ID="hdnHoy" runat="server" ClientIDMode="Static" />
                <asp:HiddenField ID="hdnFechaCalendario" runat="server" ClientIDMode="Static" />
                <asp:HiddenField ID="hdnVistaCalendario" runat="server" ClientIDMode="Static" />
                <asp:HiddenField ID="hdnClaseHorario" runat="server" ClientIDMode="Static" />
                <asp:HiddenField ID="hdnClaseFecha" runat="server" ClientIDMode="Static" />
                <asp:HiddenField ID="hdnSeleccion" runat="server" ClientIDMode="Static" />
                <asp:Button ID="btnAbrirClase" runat="server" ClientIDMode="Static" OnClick="btnAbrirClase_Click" CausesValidation="false" style="display:none;" />
            </div>
        </div>

        <div id="panelClienteInfo" class="cliente-info-panel" style="display: none;">
            <i class="bi bi-info-circle"></i>
            <span><asp:Literal ID="litClienteInfo" runat="server" Text="Se muestran las actividades asociadas a tus alumnos. Si no ves clases, contactá a recepción." /></span>
        </div>

        <!-- Vista mensual, semanal o diaria: la arma el script de abajo -->
        <div id="calendarioVista" class="calendar-grid animate-fade-in"></div>

        <!-- Varias clases seleccionadas (Cliente/Familiar): anotarse o darse de baja de todas juntas -->
        <div id="barraSeleccion" class="barra-seleccion" style="display: none;">
            <span id="textoSeleccion" class="texto-seleccion"></span>
            <div class="barra-acciones">
                <button type="button" id="btnLimpiarSeleccion" class="btn-secondary"><%= T("actividades_btn_limpiar_seleccion") %></button>
                <asp:Button ID="btnBajaSeleccion" runat="server" ClientIDMode="Static" CssClass="btn-submit btn-peligro" OnClick="btnBajaSeleccion_Click" CausesValidation="false" />
                <asp:Button ID="btnAnotarSeleccion" runat="server" ClientIDMode="Static" CssClass="btn-submit" OnClick="btnAnotarSeleccion_Click" CausesValidation="false" />
            </div>
        </div>

        <!-- Mis actividades: el Cliente/Familiar se anota y se desanota -->
        <asp:Panel ID="pnlMisActividades" runat="server" Visible="false" CssClass="card gestion-card">
            <div class="mis-actividades-header">
                <div>
                    <h3 class="card-title"><asp:Literal ID="litMisActividadesTitulo" runat="server" Text="Inscripción fija" /></h3>
                    <p class="texto-vacio ayuda"><asp:Literal ID="litMisActividadesAyuda" runat="server" /></p>
                </div>
                <asp:PlaceHolder ID="phSelectorAlumno" runat="server" Visible="false">
                    <div class="form-group selector-alumno">
                        <label for="<%= ddlMisAlumnos.ClientID %>"><%= T("actividades_campo_alumno") %></label>
                        <asp:DropDownList ID="ddlMisAlumnos" runat="server" AutoPostBack="true" OnSelectedIndexChanged="ddlMisAlumnos_SelectedIndexChanged" />
                    </div>
                </asp:PlaceHolder>
            </div>
            <div class="table-container">
                <table class="table">
                    <thead>
                        <tr>
                            <th><%= T("actividades_col_descripcion") %></th>
                            <th><%= T("actividades_col_turno") %></th>
                            <th><%= T("actividades_col_instructores") %></th>
                            <th><%= T("actividades_col_precio") %></th>
                            <th><%= T("dash_col_estado") %></th>
                            <th></th>
                        </tr>
                    </thead>
                    <tbody>
                        <asp:Repeater ID="rptMisActividades" runat="server" OnItemCommand="rptMisActividades_ItemCommand">
                            <ItemTemplate>
                                <tr>
                                    <td class="font-medium"><%#: Eval("Descripcion") %></td>
                                    <td class="text-muted"><%#: Eval("Turno") %></td>
                                    <td><%#: Eval("Instructores") %></td>
                                    <td class="tabular-nums"><%#: Eval("Precio") %></td>
                                    <td><span class="estado-pill <%# (bool)Eval("Fija") ? "activa" : "inactiva" %>"><%#: Eval("EstadoTexto") %></span></td>
                                    <td>
                                        <asp:LinkButton runat="server" CssClass="btn-link-accion" CommandName="AnotarFija" CommandArgument='<%# Eval("CodHorario") %>' CausesValidation="false" Visible='<%# !(bool)Eval("Fija") %>'><%# T("actividades_btn_anotarme_todas") %></asp:LinkButton>
                                        <asp:LinkButton runat="server" CssClass="btn-link-accion peligro" CommandName="BajaFija" CommandArgument='<%# Eval("CodHorario") %>' CausesValidation="false" Visible='<%# (bool)Eval("Fija") %>'><%# T("actividades_btn_desanotarme") %></asp:LinkButton>
                                    </td>
                                </tr>
                            </ItemTemplate>
                        </asp:Repeater>
                        <asp:PlaceHolder ID="phSinMisActividades" runat="server" Visible="false">
                            <tr><td colspan="6" class="texto-vacio"><asp:Literal ID="litSinMisActividades" runat="server" /></td></tr>
                        </asp:PlaceHolder>
                    </tbody>
                </table>
            </div>
        </asp:Panel>

        <!-- Gestión: solo con permiso GestionActividades -->
        <asp:Panel ID="pnlGestion" runat="server" CssClass="card gestion-card">
            <h3 class="card-title"><asp:Literal ID="litGestionTitulo" runat="server" Text="Actividades" /></h3>
            <div class="table-container">
                <table class="table">
                    <thead>
                        <tr>
                            <th><%= T("actividades_col_descripcion") %></th>
                            <th><%= T("actividades_col_horarios") %></th>
                            <th><%= T("actividades_col_instructores") %></th>
                            <th><%= T("actividades_col_precio") %></th>
                            <th><%= T("actividades_col_cupo") %></th>
                            <th><%= T("dash_col_estado") %></th>
                            <th></th>
                        </tr>
                    </thead>
                    <tbody>
                        <asp:Repeater ID="rptActividades" runat="server" OnItemCommand="rptActividades_ItemCommand">
                            <ItemTemplate>
                                <tr>
                                    <td class="font-medium"><%#: Eval("Descripcion") %></td>
                                    <td class="text-muted"><%#: Eval("ResumenHorarios") %></td>
                                    <td><%#: Eval("Instructores") %></td>
                                    <td class="tabular-nums"><%#: Eval("Precio") %></td>
                                    <td class="tabular-nums"><%#: Eval("Cupo") %></td>
                                    <td><span class="estado-pill <%# (bool)Eval("Activo") ? "activa" : "inactiva" %>"><%#: Eval("EstadoTexto") %></span></td>
                                    <td>
                                        <div class="acciones-fila">
                                            <asp:LinkButton runat="server" CssClass="btn-link-accion" CommandName="Editar" CommandArgument='<%# Eval("CodActividad") %>' CausesValidation="false"><%# T("actividades_btn_editar") %></asp:LinkButton>
                                            <asp:LinkButton runat="server" CssClass="btn-link-accion" CommandName="Alumnos" CommandArgument='<%# Eval("CodActividad") %>' CausesValidation="false" Visible='<%# (bool)Eval("Activo") %>'><%# T("actividades_btn_alumnos") %></asp:LinkButton>
                                            <asp:LinkButton runat="server" CssClass="btn-link-accion peligro" CommandName="Desactivar" CommandArgument='<%# Eval("CodActividad") %>' CausesValidation="false" Visible='<%# (bool)Eval("Activo") %>'><%# T("actividades_btn_desactivar") %></asp:LinkButton>
                                            <asp:LinkButton runat="server" CssClass="btn-link-accion" CommandName="Activar" CommandArgument='<%# Eval("CodActividad") %>' CausesValidation="false" Visible='<%# !(bool)Eval("Activo") %>'><%# T("actividades_btn_activar") %></asp:LinkButton>
                                        </div>
                                    </td>
                                </tr>
                            </ItemTemplate>
                        </asp:Repeater>
                        <asp:PlaceHolder ID="phSinActividades" runat="server" Visible="false">
                            <tr><td colspan="7" class="texto-vacio"><%= T("actividades_sin_actividades") %></td></tr>
                        </asp:PlaceHolder>
                    </tbody>
                </table>
            </div>
        </asp:Panel>
    </div>

    <!-- Modal alta/modificación de actividad -->
    <asp:Panel ID="pnlForm" runat="server" Visible="false" CssClass="modal-overlay">
        <div class="modal-content modal-wide">
            <div class="modal-header">
                <h3 class="modal-title"><asp:Literal ID="litModalTitulo" runat="server" Text="Nueva Actividad" /></h3>
                <asp:LinkButton ID="btnCerrarForm" runat="server" CssClass="modal-close" OnClick="btnCerrarForm_Click" CausesValidation="false">
                    <svg width="18" height="18" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2">
                        <line x1="18" y1="6" x2="6" y2="18"/>
                        <line x1="6" y1="6" x2="18" y2="18"/>
                    </svg>
                </asp:LinkButton>
            </div>

            <div class="form-group">
                <label><%= T("actividades_campo_nombre") %></label>
                <asp:TextBox ID="txtDescripcion" runat="server" MaxLength="200" />
            </div>
            <div class="form-row">
                <div class="form-group">
                    <label><%= T("actividades_campo_costo") %></label>
                    <asp:TextBox ID="txtCostoInterno" runat="server" TextMode="Number" step="0.01" min="0" />
                </div>
                <div class="form-group">
                    <label><%= T("actividades_campo_precio") %></label>
                    <asp:TextBox ID="txtPrecioAlumno" runat="server" TextMode="Number" step="0.01" min="0" />
                </div>
            </div>
            <div class="form-group">
                <label for="<%= txtCupo.ClientID %>"><%= T("actividades_campo_cupo") %></label>
                <asp:TextBox ID="txtCupo" runat="server" TextMode="Number" step="1" min="1" max="1000" />
                <small class="texto-vacio"><%= T("actividades_campo_cupo_ayuda") %></small>
            </div>
            <div class="form-group form-check-inline">
                <asp:CheckBox ID="chkActiva" runat="server" Checked="true" />
                <label for="<%= chkActiva.ClientID %>" style="margin:0;"><%= T("actividades_campo_activa") %></label>
            </div>

            <div class="seccion-form">
                <h4><%= T("actividades_horarios_titulo") %></h4>
                <asp:Repeater ID="rptHorarios" runat="server" OnItemCommand="rptHorarios_ItemCommand" OnItemDataBound="rptHorarios_ItemDataBound">
                    <ItemTemplate>
                        <div class="fila-horario">
                            <asp:HiddenField ID="hdnCodHorario" runat="server" Value='<%# Eval("CodHorario") %>' />
                            <div class="fila-horario-tiempo">
                                <div class="form-group">
                                    <label><%# T("actividades_campo_dia") %></label>
                                    <asp:DropDownList ID="ddlDia" runat="server" />
                                </div>
                                <div class="form-group">
                                    <label><%# T("actividades_campo_horario") %></label>
                                    <asp:TextBox ID="txtHora" runat="server" TextMode="Time" Text='<%# Eval("Hora") %>' />
                                </div>
                                <div class="form-group">
                                    <label><%# T("actividades_campo_duracion") %></label>
                                    <asp:TextBox ID="txtDuracion" runat="server" TextMode="Number" min="1" max="600" Text='<%# Eval("Duracion") %>' />
                                </div>
                                <asp:LinkButton runat="server" CssClass="btn-link-accion peligro" CommandName="Quitar" CommandArgument='<%# Container.ItemIndex %>' CausesValidation="false" ToolTip='<%# T("actividades_quitar_horario") %>'>✕</asp:LinkButton>
                            </div>
                            <div class="fila-horario-asignacion">
                                <div class="form-group">
                                    <label><%# T("actividades_campo_aula") %></label>
                                    <asp:DropDownList ID="ddlAula" runat="server" />
                                </div>
                                <div class="form-group">
                                    <label><%# T("actividades_campo_titular") %></label>
                                    <asp:DropDownList ID="ddlTitular" runat="server" />
                                </div>
                                <div class="form-group">
                                    <label><%# T("actividades_campo_auxiliar") %></label>
                                    <asp:DropDownList ID="ddlAuxiliar" runat="server" />
                                </div>
                            </div>
                        </div>
                    </ItemTemplate>
                </asp:Repeater>
                <asp:Button ID="btnAgregarHorario" runat="server" CssClass="btn-agregar" OnClick="btnAgregarHorario_Click" CausesValidation="false" />
            </div>

            <div class="modal-actions">
                <asp:Button ID="btnCancelarForm" runat="server" CssClass="btn-secondary" OnClick="btnCerrarForm_Click" CausesValidation="false" />
                <asp:Button ID="btnGuardarActividad" runat="server" CssClass="btn-submit" OnClick="btnGuardarActividad_Click" CausesValidation="false" />
            </div>
        </div>
    </asp:Panel>

    <!-- Modal inscripción de alumnos -->
    <asp:Panel ID="pnlInscripciones" runat="server" Visible="false" CssClass="modal-overlay">
        <div class="modal-content modal-wide">
            <div class="modal-header">
                <h3 class="modal-title"><asp:Literal ID="litInscripcionesTitulo" runat="server" /></h3>
                <asp:LinkButton ID="btnCerrarInscripciones" runat="server" CssClass="modal-close" OnClick="btnCerrarInscripciones_Click" CausesValidation="false">
                    <svg width="18" height="18" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2">
                        <line x1="18" y1="6" x2="6" y2="18"/>
                        <line x1="6" y1="6" x2="18" y2="18"/>
                    </svg>
                </asp:LinkButton>
            </div>
            <p class="texto-vacio ayuda"><asp:Literal ID="litInscripcionesAyuda" runat="server" /></p>
            <div class="form-group">
                <label for="<%= ddlTurnoInscripciones.ClientID %>"><%= T("actividades_col_turno") %></label>
                <asp:DropDownList ID="ddlTurnoInscripciones" runat="server" AutoPostBack="true" OnSelectedIndexChanged="ddlTurnoInscripciones_SelectedIndexChanged" />
            </div>
            <div class="lista-checks">
                <asp:CheckBoxList ID="cblAlumnos" runat="server" RepeatLayout="Table" />
                <asp:Literal ID="litSinAlumnos" runat="server" Visible="false" />
            </div>
            <div class="modal-actions">
                <asp:Button ID="btnCancelarInscripciones" runat="server" CssClass="btn-secondary" OnClick="btnCerrarInscripciones_Click" CausesValidation="false" />
                <asp:Button ID="btnGuardarInscripciones" runat="server" CssClass="btn-submit" OnClick="btnGuardarInscripciones_Click" CausesValidation="false" />
            </div>
        </div>
    </asp:Panel>

    <!-- Modal de una clase concreta (turno + fecha) -->
    <asp:Panel ID="pnlClase" runat="server" Visible="false" CssClass="modal-overlay">
        <div class="modal-content modal-wide">
            <div class="modal-header">
                <div>
                    <h3 class="modal-title"><asp:Literal ID="litClaseTitulo" runat="server" /></h3>
                    <p class="clase-detalle"><asp:Literal ID="litClaseDetalle" runat="server" /></p>
                    <p class="clase-detalle clase-cupo"><asp:Literal ID="litClaseCupo" runat="server" /></p>
                </div>
                <asp:LinkButton ID="btnCerrarClaseX" runat="server" CssClass="modal-close" OnClick="btnCerrarClase_Click" CausesValidation="false">
                    <svg width="18" height="18" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2">
                        <line x1="18" y1="6" x2="6" y2="18"/>
                        <line x1="6" y1="6" x2="18" y2="18"/>
                    </svg>
                </asp:LinkButton>
            </div>

            <asp:PlaceHolder ID="phClaseCompleta" runat="server" Visible="false">
                <p class="aviso-clase"><%= T("actividades_clase_completa_msg") %></p>
            </asp:PlaceHolder>

            <asp:PlaceHolder ID="phClasePasada" runat="server" Visible="false">
                <p class="aviso-clase"><asp:Literal ID="litClasePasada" runat="server" /></p>
            </asp:PlaceHolder>

            <asp:PlaceHolder ID="phClaseCliente" runat="server" Visible="false">
                <p class="estado-clase"><asp:Literal ID="litClaseEstado" runat="server" /></p>
                <asp:PlaceHolder ID="phClaseAnotar" runat="server">
                    <div class="seccion-form">
                        <h4><%= T("actividades_alcance_titulo") %></h4>
                        <asp:RadioButtonList ID="rblAlcanceAlta" runat="server" CssClass="lista-alcance" RepeatLayout="Flow" />
                        <div class="modal-actions">
                            <asp:Button ID="btnClaseAnotar" runat="server" CssClass="btn-submit" OnClick="btnClaseAnotar_Click" CausesValidation="false" />
                        </div>
                    </div>
                </asp:PlaceHolder>
                <asp:PlaceHolder ID="phClaseDesanotar" runat="server">
                    <div class="seccion-form">
                        <h4><%= T("actividades_baja_titulo") %></h4>
                        <asp:RadioButtonList ID="rblAlcanceBaja" runat="server" CssClass="lista-alcance" RepeatLayout="Flow" />
                        <div class="modal-actions">
                            <asp:Button ID="btnClaseDesanotar" runat="server" CssClass="btn-submit btn-peligro" OnClick="btnClaseDesanotar_Click" CausesValidation="false" />
                        </div>
                    </div>
                </asp:PlaceHolder>
            </asp:PlaceHolder>

            <asp:PlaceHolder ID="phClaseGestor" runat="server" Visible="false">
                <div class="seccion-form">
                    <h4><asp:Literal ID="litAsistentesTitulo" runat="server" /></h4>
                    <div class="lista-checks">
                        <asp:Repeater ID="rptAsistentes" runat="server" OnItemCommand="rptAsistentes_ItemCommand">
                            <ItemTemplate>
                                <div class="fila-asistente">
                                    <div>
                                        <span class="font-medium"><%#: Eval("Nombre") %></span>
                                        <span class="estado-pill activa"><%#: Eval("TipoTexto") %></span>
                                    </div>
                                    <div class="acciones-fila">
                                        <asp:LinkButton runat="server" CssClass="btn-link-accion peligro" CommandName="QuitarUna" CommandArgument='<%# Eval("Dni") %>' CausesValidation="false" Visible='<%# (bool)Eval("PuedeQuitar") %>'><%# T("actividades_btn_quitar_una") %></asp:LinkButton>
                                        <asp:LinkButton runat="server" CssClass="btn-link-accion peligro" CommandName="QuitarDesde" CommandArgument='<%# Eval("Dni") %>' CausesValidation="false" Visible='<%# (bool)Eval("PuedeQuitar") %>'><%# T("actividades_btn_quitar_desde") %></asp:LinkButton>
                                    </div>
                                </div>
                            </ItemTemplate>
                        </asp:Repeater>
                        <asp:PlaceHolder ID="phSinAsistentes" runat="server" Visible="false">
                            <span class="texto-vacio"><%= T("actividades_clase_sin_asistentes") %></span>
                        </asp:PlaceHolder>
                    </div>
                </div>
                <asp:PlaceHolder ID="phAgregarAsistente" runat="server">
                    <div class="seccion-form">
                        <h4><%= T("actividades_anotar_alumno_titulo") %></h4>
                        <div class="form-group">
                            <asp:DropDownList ID="ddlAgregarAlumno" runat="server" />
                        </div>
                        <asp:RadioButtonList ID="rblAlcanceAltaGestor" runat="server" CssClass="lista-alcance" RepeatLayout="Flow" />
                        <div class="modal-actions">
                            <asp:Button ID="btnClaseAgregar" runat="server" CssClass="btn-submit" OnClick="btnClaseAgregar_Click" CausesValidation="false" />
                        </div>
                    </div>
                </asp:PlaceHolder>
            </asp:PlaceHolder>

            <div class="modal-actions">
                <asp:Button ID="btnCerrarClase" runat="server" CssClass="btn-secondary" OnClick="btnCerrarClase_Click" CausesValidation="false" />
            </div>
        </div>
    </asp:Panel>

    <!-- Modal confirmación de baja -->
    <asp:Panel ID="pnlConfirmarBaja" runat="server" Visible="false" CssClass="modal-overlay">
        <div class="modal-content">
            <div class="modal-header">
                <h3 class="modal-title"><%= T("actividades_confirmar_baja_titulo") %></h3>
            </div>
            <p><asp:Literal ID="litConfirmarBaja" runat="server" /></p>
            <div class="modal-actions">
                <asp:Button ID="btnCancelarBaja" runat="server" CssClass="btn-secondary" OnClick="btnCancelarBaja_Click" CausesValidation="false" />
                <asp:Button ID="btnConfirmarBaja" runat="server" CssClass="btn-submit" OnClick="btnConfirmarBaja_Click" CausesValidation="false" />
            </div>
        </div>
    </asp:Panel>

    <script>
        // Horarios semanales cargados desde el servidor, agrupados por día de la semana
        // (1 = Lunes ... 7 = Domingo). Cada uno trae su código de turno (h).
        function leerJson(id) {
            try {
                return JSON.parse(document.getElementById(id).value || '{}');
            } catch (e) {
                return {};
            }
        }

        const activitiesByWeekday = leerJson('hdnActividadesJson');
        const esCliente = document.getElementById('hdnEsCliente').value === '1';
        const puedeGestionar = document.getElementById('hdnPuedeGestionar').value === '1';
        const hoyIso = document.getElementById('hdnHoy').value;

        // Inscripciones del alumno (solo Cliente/Familiar): fijas por turno con rango de fechas,
        // y altas/bajas puntuales como "turno|yyyy-MM-dd".
        const agenda = leerJson('hdnAgendaJson');
        const altas = new Set(agenda.altas || []);
        const bajas = new Set(agenda.bajas || []);
        const fijas = agenda.fijas || [];

        // Ocupación por turno (sin datos de alumnos): f = períodos de fijas, a/b = altas/bajas por fecha
        const ocupacionPorTurno = leerJson('hdnOcupacionJson');

        // Clases seleccionadas para anotarse/darse de baja juntas ("turno|yyyy-MM-dd").
        // Se guardan en un campo oculto para mantenerlas al navegar y entre postbacks.
        const hdnSeleccion = document.getElementById('hdnSeleccion');
        const seleccion = new Set();

        // Mostrar mensaje informativo para usuarios Cliente.
        (function () {
            const panelInfo = document.getElementById('panelClienteInfo');
            if (esCliente && panelInfo) {
                panelInfo.style.display = 'flex';
            }
        })();

        const monthNames = <%=T("actividades_meses_json")%>;
        const nombresDias = [
            '<%= JsT("actividades_dia_lun") %>', '<%= JsT("actividades_dia_mar") %>', '<%= JsT("actividades_dia_mie") %>',
            '<%= JsT("actividades_dia_jue") %>', '<%= JsT("actividades_dia_vie") %>', '<%= JsT("actividades_dia_sab") %>',
            '<%= JsT("actividades_dia_dom") %>'
        ];
        const _actDayFmt = '<%= JsT("actividades_dia_titulo_fmt") %>';
        const _actMas = '<%= JsT("actividades_mas") %>';
        const _txtAnotado = '<%= JsT("actividades_estado_anotado") %>';
        const _txtNoAnotado = '<%= JsT("actividades_estado_no_anotado") %>';
        const _txtAnotarme = '<%= JsT("actividades_btn_anotarme") %>';
        const _txtCambiar = '<%= JsT("actividades_btn_desanotarme") %>';
        const _txtVerAlumnos = '<%= JsT("actividades_btn_ver_alumnos") %>';
        const _txtSeleccionFmt = '<%= JsT("actividades_seleccion_fmt") %>';
        const _txtSeleccionar = '<%= JsT("actividades_seleccionar_clase") %>';
        const _txtSinClases = '<%= JsT("actividades_dia_sin_clases") %>';
        const _txtCompleta = '<%= JsT("actividades_estado_completa") %>';

        const MAX_CLASES_MES = 3;   // clases visibles por día en la vista mensual
        const ALTO_HORA = 3.5;      // rem por hora en la vista semanal (igual que las líneas del CSS)

        // ==================== FECHAS ====================

        function parseIso(iso) {
            const [y, m, d] = iso.split('-').map(Number);
            return new Date(y, m - 1, d);
        }

        function aIso(fecha) {
            return `${fecha.getFullYear()}-${String(fecha.getMonth() + 1).padStart(2, '0')}-${String(fecha.getDate()).padStart(2, '0')}`;
        }

        // 1 = Lunes ... 7 = Domingo, igual que en la base
        function diaSemana(fecha) {
            return ((fecha.getDay() + 6) % 7) + 1;
        }

        function sumarDias(fecha, dias) {
            const resultado = new Date(fecha);
            resultado.setDate(resultado.getDate() + dias);
            return resultado;
        }

        function inicioSemana(fecha) {
            return sumarDias(fecha, 1 - diaSemana(fecha));
        }

        function aMinutos(hhmm) {
            const [h, m] = hhmm.split(':').map(Number);
            return h * 60 + m;
        }

        // ==================== ESTADO (vista + fecha) ====================
        // Se guardan en campos ocultos para no perderlos en cada postback.

        const hdnFecha = document.getElementById('hdnFechaCalendario');
        const hdnVista = document.getElementById('hdnVistaCalendario');
        let vista = ['mes', 'semana', 'dia'].includes(hdnVista.value) ? hdnVista.value : 'mes';
        let fechaActual = /^\d{4}-\d{2}-\d{2}$/.test(hdnFecha.value) ? parseIso(hdnFecha.value) : parseIso(hoyIso);

        function guardarPosicion() {
            hdnFecha.value = aIso(fechaActual);
            hdnVista.value = vista;
        }

        // Los nombres vienen de la base: escaparlos antes de insertarlos como HTML
        function esc(texto) {
            const div = document.createElement('div');
            div.textContent = texto == null ? '' : String(texto);
            return div.innerHTML;
        }

        // 'fija', 'puntual' o 'no' (las fechas ISO se comparan como texto)
        function estadoClase(h, iso) {
            const clave = `${h}|${iso}`;
            if (altas.has(clave)) return 'puntual';
            const tieneFija = fijas.some(f => f.h === h && f.d <= iso && (!f.u || f.u >= iso));
            return tieneFija && !bajas.has(clave) ? 'fija' : 'no';
        }

        // Cuántos alumnos van a la clase del turno h en la fecha (solo se conoce desde hoy)
        function anotadosEn(h, iso) {
            const o = ocupacionPorTurno[h];
            if (!o) return 0;
            const fijasVigentes = (o.f || []).filter(p => p[0] <= iso && (!p[1] || p[1] >= iso)).length;
            return fijasVigentes - ((o.b || {})[iso] || 0) + ((o.a || {})[iso] || 0);
        }

        // Clases de una fecha, ordenadas por hora, con su fecha, ocupación y (para el Cliente) su estado
        function clasesDelDia(fecha) {
            const iso = aIso(fecha);
            return (activitiesByWeekday[diaSemana(fecha)] || [])
                .map(a => {
                    const anotados = iso >= hoyIso ? anotadosEn(a.h, iso) : null;
                    const estado = esCliente ? estadoClase(a.h, iso) : null;
                    const completa = anotados != null && a.cupo > 0 && anotados >= a.cupo;
                    // Al Cliente que ya va no se le muestra en gris: tiene su lugar
                    return { ...a, iso: iso, estado: estado, anotados: anotados, completa: completa, gris: completa && !(esCliente && estado !== 'no') };
                })
                .sort((a, b) => a.time.localeCompare(b.time) || a.name.localeCompare(b.name));
        }

        function textoCupo(clase) {
            if (clase.anotados == null) return '';
            return `<span class="cupo-texto${clase.completa ? ' llena' : ''}">${clase.anotados}/${clase.cupo}${clase.completa ? ' · ' + esc(_txtCompleta) : ''}</span>`;
        }

        // Clases CSS comunes: completa, estado del Cliente y selección
        function clasesEstado(clase) {
            let css = clase.gris ? ' llena' : '';
            if (esCliente && !clase.gris) {
                css += clase.estado === 'no' ? ' no-anotado' : ' anotado';
            }
            if (seleccion.has(`${clase.h}|${clase.iso}`)) {
                css += ' seleccionada';
            }
            return css;
        }

        function marcaAnotado(clase) {
            return esCliente && clase.estado !== 'no' ? '✓ ' : '';
        }

        function puedeAbrir(clase) {
            return puedeGestionar || (esCliente && clase.iso >= hoyIso);
        }

        function claseSeleccionable(clase) {
            return esSeleccionable(clase.iso) && !(clase.completa && clase.estado === 'no');
        }

        // ==================== SELECCIÓN ====================

        function esSeleccionable(iso) {
            return esCliente && iso >= hoyIso;
        }

        function guardarSeleccion() {
            hdnSeleccion.value = Array.from(seleccion).join(',');
            actualizarBarra();
        }

        function alternarSeleccion(h, iso) {
            const clave = `${h}|${iso}`;
            if (seleccion.has(clave)) {
                seleccion.delete(clave);
            } else {
                seleccion.add(clave);
            }
            guardarSeleccion();
            render();
        }

        function checkSeleccion(clase, css) {
            if (!claseSeleccionable(clase)) return '';
            const marcado = seleccion.has(`${clase.h}|${clase.iso}`) ? 'checked' : '';
            return `<input type="checkbox" class="${css}" title="${esc(_txtSeleccionar)}" data-sel-h="${clase.h}" data-sel-f="${clase.iso}" ${marcado} />`;
        }

        // Muestra la barra con la cantidad elegida; cada botón cuenta solo las clases a las que aplica
        function actualizarBarra() {
            const barra = document.getElementById('barraSeleccion');
            if (!esCliente || seleccion.size === 0) {
                barra.style.display = 'none';
                return;
            }

            let paraAnotar = 0;
            seleccion.forEach(clave => {
                const [h, iso] = clave.split('|');
                if (estadoClase(parseInt(h, 10), iso) === 'no') paraAnotar++;
            });
            const paraBaja = seleccion.size - paraAnotar;

            document.getElementById('textoSeleccion').textContent = _txtSeleccionFmt.replace('{0}', seleccion.size);

            const btnAnotar = document.getElementById('btnAnotarSeleccion');
            const btnBaja = document.getElementById('btnBajaSeleccion');
            btnAnotar.dataset.base = btnAnotar.dataset.base || btnAnotar.value;
            btnBaja.dataset.base = btnBaja.dataset.base || btnBaja.value;
            btnAnotar.value = `${btnAnotar.dataset.base} (${paraAnotar})`;
            btnBaja.value = `${btnBaja.dataset.base} (${paraBaja})`;
            btnAnotar.disabled = paraAnotar === 0;
            btnBaja.disabled = paraBaja === 0;

            barra.style.display = 'flex';
        }

        // ==================== ACCIONES ====================

        function abrirClase(h, iso) {
            guardarPosicion();
            document.getElementById('hdnClaseHorario').value = h;
            document.getElementById('hdnClaseFecha').value = iso;
            document.getElementById('btnAbrirClase').click();
        }

        function irADia(iso) {
            fechaActual = parseIso(iso);
            vista = 'dia';
            render();
        }

        // Estado y botones de una clase (vista diaria)
        function accionesClase(clase) {
            const pasada = clase.iso < hoyIso;

            if (esCliente) {
                const pill = clase.estado === 'no'
                    ? `<span class="estado-pill inactiva">${esc(_txtNoAnotado)}</span>`
                    : `<span class="estado-pill ${clase.estado === 'fija' ? 'activa' : 'puntual'}">${esc(_txtAnotado)}</span>`;
                const sinLugar = clase.completa && clase.estado === 'no';
                const boton = pasada || sinLugar ? '' :
                    `<button type="button" class="btn-details" data-h="${clase.h}" data-f="${clase.iso}">${esc(clase.estado === 'no' ? _txtAnotarme : _txtCambiar)}</button>`;
                const check = checkSeleccion(clase, '');
                return `<div class="acciones-clase">${check ? `<label class="check-seleccion">${check}</label>` : ''}${pill}${textoCupo(clase)}${boton}</div>`;
            }

            if (puedeGestionar) {
                return `<div class="acciones-clase">${textoCupo(clase)}<button type="button" class="btn-details" data-h="${clase.h}" data-f="${clase.iso}">${esc(_txtVerAlumnos)}</button></div>`;
            }

            return `<div class="acciones-clase">${textoCupo(clase)}</div>`;
        }

        // ==================== VISTA MENSUAL ====================
        // Cada día muestra hasta MAX_CLASES_MES clases en una línea ("18:00 Yoga") y un "+N más".
        // En pantallas chicas se reemplazan por puntos de color. Tocar el día abre la vista diaria.

        function renderMes() {
            const y = fechaActual.getFullYear();
            const m = fechaActual.getMonth();
            const primero = new Date(y, m, 1);
            const vacios = diaSemana(primero) - 1;
            const diasMes = new Date(y, m + 1, 0).getDate();

            let html = `<div class="calendar-weekdays">${nombresDias.map(n => `<div>${esc(n)}</div>`).join('')}</div><div class="calendar-days">`;

            for (let i = 0; i < vacios; i++) {
                html += '<div class="calendar-day empty"></div>';
            }

            for (let dia = 1; dia <= diasMes; dia++) {
                const fecha = new Date(y, m, dia);
                const iso = aIso(fecha);
                const clases = clasesDelDia(fecha);
                const css = 'calendar-day' + (iso === hoyIso ? ' hoy' : '') + (iso < hoyIso ? ' pasado' : '');

                html += `<div class="${css}" data-dia="${iso}"><div class="day-number">${dia}</div>`;

                clases.slice(0, MAX_CLASES_MES).forEach(clase => {
                    const tituloCupo = clase.anotados != null ? ` · ${clase.anotados}/${clase.cupo}${clase.completa ? ' ' + _txtCompleta : ''}` : '';
                    html += `<div class="class-item ${clase.color}${clasesEstado(clase)}${claseSeleccionable(clase) ? ' seleccionable' : ''}"
                                  data-chip-h="${clase.h}" data-chip-f="${iso}" data-chip-sel="${claseSeleccionable(clase) ? 1 : 0}" title="${esc(`${clase.name} · ${clase.time}–${clase.end} · ${clase.aula} · ${clase.instructor}${tituloCupo}`)}">
                                <span class="class-time">${esc(clase.time)}</span>
                                <span class="class-name">${marcaAnotado(clase)}${esc(clase.name)}</span>
                             </div>`;
                });

                if (clases.length > MAX_CLASES_MES) {
                    html += `<button type="button" class="more-classes" data-ver-dia="${iso}">+${clases.length - MAX_CLASES_MES} ${esc(_actMas)}</button>`;
                }

                if (clases.length > 0) {
                    html += `<div class="dots-mes">${clases.slice(0, 8).map(c => `<span class="dot ${c.gris ? 'llena' : c.color}${!c.gris && esCliente && c.estado === 'no' ? ' no-anotado' : ''}"></span>`).join('')}${clases.length > 8 ? '<span class="dots-mas">+</span>' : ''}</div>`;
                }

                html += '</div>';
            }

            const resto = (vacios + diasMes) % 7;
            for (let i = 0; resto > 0 && i < 7 - resto; i++) {
                html += '<div class="calendar-day empty"></div>';
            }

            return html + '</div>';
        }

        // ==================== VISTA SEMANAL ====================
        // Grilla por horas. Las clases que se pisan en horario se reparten en columnas lado a lado.

        // Primera y última hora a mostrar, según los horarios cargados
        function rangoHoras() {
            let desde = 24, hasta = 0;
            Object.values(activitiesByWeekday).forEach(lista => lista.forEach(a => {
                desde = Math.min(desde, Math.floor(aMinutos(a.time) / 60));
                hasta = Math.max(hasta, Math.ceil(aMinutos(a.end) / 60));
            }));
            return desde < hasta ? { desde: desde, hasta: hasta } : { desde: 8, hasta: 21 };
        }

        // Asigna a cada clase una columna (lane) dentro de su grupo de clases superpuestas
        function repartirEnColumnas(clases) {
            const items = clases
                .map(c => ({ c: c, ini: aMinutos(c.time), fin: aMinutos(c.end) }))
                .sort((a, b) => a.ini - b.ini || a.fin - b.fin);
            const resultado = [];
            let grupo = [];
            let finGrupo = -1;

            function cerrarGrupo() {
                const finPorColumna = [];
                grupo.forEach(it => {
                    let col = finPorColumna.findIndex(fin => fin <= it.ini);
                    if (col < 0) {
                        col = finPorColumna.length;
                        finPorColumna.push(0);
                    }
                    finPorColumna[col] = it.fin;
                    it.col = col;
                });
                grupo.forEach(it => { it.columnas = finPorColumna.length; });
                resultado.push(...grupo);
                grupo = [];
            }

            items.forEach(it => {
                if (grupo.length > 0 && it.ini >= finGrupo) {
                    cerrarGrupo();
                    finGrupo = -1;
                }
                grupo.push(it);
                finGrupo = Math.max(finGrupo, it.fin);
            });
            if (grupo.length > 0) cerrarGrupo();

            return resultado;
        }

        function renderSemana() {
            const lunes = inicioSemana(fechaActual);
            const dias = [0, 1, 2, 3, 4, 5, 6].map(i => sumarDias(lunes, i));
            const rango = rangoHoras();
            const altoTotal = (rango.hasta - rango.desde) * ALTO_HORA;

            let html = '<div class="semana-scroll"><div class="semana"><div class="semana-header"><div></div>';
            dias.forEach((fecha, i) => {
                const iso = aIso(fecha);
                html += `<button type="button" class="semana-dia-titulo${iso === hoyIso ? ' hoy' : ''}" data-ver-dia="${iso}">${esc(nombresDias[i])}<strong>${fecha.getDate()}</strong></button>`;
            });
            html += `</div><div class="semana-cuerpo" style="height:${altoTotal}rem"><div class="semana-horas">`;

            for (let hora = rango.desde + 1; hora < rango.hasta; hora++) {
                html += `<span class="semana-hora" style="top:${(hora - rango.desde) * ALTO_HORA}rem">${String(hora).padStart(2, '0')}:00</span>`;
            }
            html += '</div>';

            dias.forEach(fecha => {
                const iso = aIso(fecha);
                html += `<div class="semana-columna${iso === hoyIso ? ' hoy' : ''}">`;

                repartirEnColumnas(clasesDelDia(fecha)).forEach(it => {
                    const clase = it.c;
                    const top = (it.ini - rango.desde * 60) / 60 * ALTO_HORA;
                    const alto = Math.max((it.fin - it.ini) / 60 * ALTO_HORA, 1.5);
                    const ancho = 100 / it.columnas;
                    const estilo = `top:${top}rem;height:${alto}rem;left:calc(${it.col * ancho}% + 0.125rem);width:calc(${ancho}% - 0.25rem)`;
                    const css = `bloque-clase ${clase.color}${clasesEstado(clase)}${puedeAbrir(clase) ? '' : ' sin-accion'}`;

                    html += `<div class="${css}" style="${estilo}" data-bloque-h="${clase.h}" data-bloque-f="${clase.iso}"
                                  title="${esc(`${clase.name} · ${clase.time}–${clase.end} · ${clase.aula} · ${clase.instructor}`)}">
                                ${checkSeleccion(clase, 'bloque-check')}
                                <div class="bloque-nombre">${marcaAnotado(clase)}${esc(clase.name)}</div>
                                <div class="bloque-hora">${esc(clase.time)}–${esc(clase.end)}</div>
                                ${clase.anotados != null ? `<div class="bloque-cupo">${clase.anotados}/${clase.cupo}${clase.completa ? ' · ' + esc(_txtCompleta) : ''}</div>` : ''}
                             </div>`;
                });

                html += '</div>';
            });

            return html + '</div></div></div>';
        }

        // ==================== VISTA DIARIA ====================
        // Clases agrupadas por horario de inicio, en tarjetas con todo el detalle y las acciones.

        function renderDia() {
            const clases = clasesDelDia(fechaActual);
            if (clases.length === 0) {
                return `<div class="dia-vacio">${esc(_txtSinClases)}</div>`;
            }

            const grupos = [];
            clases.forEach(clase => {
                const ultimo = grupos[grupos.length - 1];
                if (ultimo && ultimo.hora === clase.time) {
                    ultimo.clases.push(clase);
                } else {
                    grupos.push({ hora: clase.time, clases: [clase] });
                }
            });

            return `<div class="dia-lista">${grupos.map(g => `
                <div class="dia-grupo">
                    <h4>${esc(g.hora)}</h4>
                    <div class="dia-tarjetas">${g.clases.map(clase => `
                        <div class="tarjeta-clase ${clase.color}${clase.gris ? ' llena' : ''}${seleccion.has(`${clase.h}|${clase.iso}`) ? ' seleccionada' : ''}">
                            <div>
                                <h5>${marcaAnotado(clase)}${esc(clase.name)}</h5>
                                <p>${esc(clase.time)} – ${esc(clase.end)} · ${esc(clase.aula)}</p>
                                <p>${esc(clase.instructor)}</p>
                            </div>
                            ${accionesClase(clase)}
                        </div>`).join('')}
                    </div>
                </div>`).join('')}
            </div>`;
        }

        // ==================== RENDER Y NAVEGACIÓN ====================

        function tituloVista() {
            if (vista === 'mes') {
                return `${monthNames[fechaActual.getMonth()]} ${fechaActual.getFullYear()}`;
            }
            if (vista === 'semana') {
                const lunes = inicioSemana(fechaActual);
                const domingo = sumarDias(lunes, 6);
                return `${lunes.getDate()} ${monthNames[lunes.getMonth()]} – ${domingo.getDate()} ${monthNames[domingo.getMonth()]} ${domingo.getFullYear()}`;
            }
            const dia = _actDayFmt.replace('{0}', fechaActual.getDate()).replace('{1}', monthNames[fechaActual.getMonth()]);
            return `${nombresDias[diaSemana(fechaActual) - 1]} ${dia} ${fechaActual.getFullYear()}`;
        }

        const contenedor = document.getElementById('calendarioVista');

        function render() {
            guardarPosicion();
            document.getElementById('currentMonthDisplay').textContent = tituloVista();
            document.querySelectorAll('.vista-switch button').forEach(b => b.classList.toggle('activa', b.dataset.vista === vista));

            if (vista === 'semana') {
                contenedor.innerHTML = renderSemana();
            } else if (vista === 'dia') {
                contenedor.innerHTML = renderDia();
            } else {
                contenedor.innerHTML = renderMes();
            }
        }

        // Un solo manejador para todas las vistas (el contenido se vuelve a generar en cada render)
        contenedor.addEventListener('click', ev => {
            if (ev.target.closest('input[data-sel-h]')) {
                return; // la casilla se maneja en 'change'
            }

            const verDia = ev.target.closest('[data-ver-dia]');
            if (verDia) {
                irADia(verDia.dataset.verDia);
                return;
            }

            const boton = ev.target.closest('button[data-h]');
            if (boton) {
                abrirClase(boton.dataset.h, boton.dataset.f);
                return;
            }

            // Vista mensual: el Cliente selecciona la clase; el gestor la abre; el resto va al día
            const chip = ev.target.closest('[data-chip-h]');
            if (chip) {
                const h = parseInt(chip.dataset.chipH, 10);
                const iso = chip.dataset.chipF;
                if (chip.dataset.chipSel === '1') {
                    alternarSeleccion(h, iso);
                } else if (puedeGestionar) {
                    abrirClase(h, iso);
                } else {
                    irADia(iso);
                }
                return;
            }

            // Vista semanal: abrir la clase
            const bloque = ev.target.closest('[data-bloque-h]');
            if (bloque) {
                if (!bloque.classList.contains('sin-accion')) {
                    abrirClase(bloque.dataset.bloqueH, bloque.dataset.bloqueF);
                }
                return;
            }

            const dia = ev.target.closest('.calendar-day[data-dia]');
            if (dia) {
                irADia(dia.dataset.dia);
            }
        });

        contenedor.addEventListener('change', ev => {
            const check = ev.target.closest('input[data-sel-h]');
            if (check) {
                alternarSeleccion(parseInt(check.dataset.selH, 10), check.dataset.selF);
            }
        });

        function mover(sentido) {
            if (vista === 'mes') {
                fechaActual = new Date(fechaActual.getFullYear(), fechaActual.getMonth() + sentido, 1);
            } else {
                fechaActual = sumarDias(fechaActual, vista === 'semana' ? 7 * sentido : sentido);
            }
            render();
        }

        document.getElementById('prevMonth').addEventListener('click', () => mover(-1));
        document.getElementById('nextMonth').addEventListener('click', () => mover(1));
        document.getElementById('btnHoy').addEventListener('click', () => {
            fechaActual = parseIso(hoyIso);
            render();
        });
        document.querySelectorAll('.vista-switch button').forEach(boton => {
            boton.addEventListener('click', () => {
                vista = boton.dataset.vista;
                render();
            });
        });

        // Selección guardada (se descartan las clases que ya pasaron)
        (hdnSeleccion.value || '').split(',').forEach(clave => {
            const partes = clave.split('|');
            if (partes.length === 2 && esSeleccionable(partes[1])) {
                seleccion.add(`${parseInt(partes[0], 10)}|${partes[1]}`);
            }
        });

        document.getElementById('btnLimpiarSeleccion').addEventListener('click', () => {
            seleccion.clear();
            guardarSeleccion();
            render();
        });

        guardarSeleccion();
        render();
    </script>
</asp:Content>
