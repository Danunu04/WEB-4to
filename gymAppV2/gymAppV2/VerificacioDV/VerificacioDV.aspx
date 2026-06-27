<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="VerificacioDV.aspx.cs" Inherits="gymAppV2.VerificacioDV.VerificacioDV" MasterPageFile="~/DashBoard.Master" %>

<asp:Content ID="HeadContent" ContentPlaceHolderID="HeadContent" runat="server">
    <title>Verificación de Integridad - GymApp</title>
    <link href="<%= ResolveUrl("~/VerificacioDV/VerificacioDV.css") %>" rel="stylesheet" type="text/css" />
    <link rel="stylesheet" href="https://cdnjs.cloudflare.com/ajax/libs/font-awesome/6.5.0/css/all.min.css">
</asp:Content>

<asp:Content ID="MainContent" ContentPlaceHolderID="MainContent" runat="server">
    <asp:Panel ID="pnlNoAdmin" runat="server" Visible="false" CssClass="dv-bloqueo">
        <div class="dv-bloqueo-icono">
            <i class="fa-solid fa-triangle-exclamation"></i>
        </div>
        <h1 class="dv-bloqueo-titulo">Error de integridad en el sistema</h1>
        <p class="dv-bloqueo-mensaje">
            Detectamos una inconsistencia en la base de datos. El sistema está pausado temporalmente
            mientras el equipo de administración lo resuelve.
        </p>
        <p class="dv-bloqueo-sub">
            Estamos trabajando para restablecer el servicio a la brevedad. Por favor, no intentes
            realizar operaciones hasta nuevo aviso.
        </p>
        <asp:Button ID="btnLogoutBloqueo" runat="server" Text="Cerrar sesión" CssClass="dv-bloqueo-btn"
            OnClick="btnLogoutBloqueo_Click" />
    </asp:Panel>

    <asp:Panel ID="pnlAdmin" runat="server" Visible="false" CssClass="dv-admin">
        <div class="dv-admin-header">
            <h1 class="dv-admin-titulo"><i class="fa-solid fa-shield-halved"></i> Verificación de Integridad</h1>
            <p class="dv-admin-sub">Detecta y corrige errores de DVH/DVV en la base de datos.</p>
        </div>

        <asp:Panel ID="pnlMensaje" runat="server" Visible="false" CssClass="dv-mensaje">
            <asp:Label ID="lblMensaje" runat="server"></asp:Label>
        </asp:Panel>

        <div class="dv-resumen">
            <div class="dv-resumen-card" id="cardTotal" runat="server">
                <div class="dv-resumen-icon"><i class="fa-solid fa-table"></i></div>
                <div class="dv-resumen-info">
                    <span class="dv-resumen-label">Tablas verificadas</span>
                    <span class="dv-resumen-valor"><asp:Label ID="lblTotalTablas" runat="server" Text="0"></asp:Label></span>
                </div>
            </div>
            <div class="dv-resumen-card dv-resumen-error" id="cardErrores" runat="server">
                <div class="dv-resumen-icon"><i class="fa-solid fa-bug"></i></div>
                <div class="dv-resumen-info">
                    <span class="dv-resumen-label">Errores detectados</span>
                    <span class="dv-resumen-valor"><asp:Label ID="lblTotalErrores" runat="server" Text="0"></asp:Label></span>
                </div>
            </div>
            <div class="dv-resumen-card dv-resumen-ok" id="cardOk" runat="server">
                <div class="dv-resumen-icon"><i class="fa-solid fa-check"></i></div>
                <div class="dv-resumen-info">
                    <span class="dv-resumen-label">Tablas OK</span>
                    <span class="dv-resumen-valor"><asp:Label ID="lblTablasOk" runat="server" Text="0"></asp:Label></span>
                </div>
            </div>
        </div>

        <asp:Panel ID="pnlInicializar" runat="server" Visible="false" CssClass="dv-inicializar">
            <p>
                <i class="fa-solid fa-circle-info"></i>
                Hay tablas con columnas dvv/dvh que aún no están registradas en el control de integridad.
                Presione "Inicializar control" para registrarlas y poder verificarlas.
            </p>
            <asp:Button ID="btnInicializar" runat="server" Text="Inicializar control" CssClass="dv-btn dv-btn-info"
                OnClick="btnInicializar_Click" />
        </asp:Panel>

        <div class="dv-acciones">
            <asp:Button ID="btnVerificar" runat="server" Text="Verificar ahora" CssClass="dv-btn dv-btn-primary"
                OnClick="btnVerificar_Click" />
            <asp:Button ID="btnRecalcular" runat="server" Text="Recalcular todos los valores" CssClass="dv-btn dv-btn-warning"
                OnClick="btnRecalcular_Click" />
            <asp:Button ID="btnRestaurar" runat="server" Text="Restaurar backup" CssClass="dv-btn dv-btn-danger"
                OnClick="btnRestaurar_Click" />
            <asp:Button ID="btnSalir" runat="server" Text="Salir" CssClass="dv-btn dv-btn-secondary"
                OnClick="btnSalir_Click" />
        </div>

        <asp:Panel ID="pnlRestaurar" runat="server" Visible="false" CssClass="dv-restaurar">
            <label class="dv-label">Ruta del backup a restaurar</label>
            <asp:TextBox ID="txtRutaBackup" runat="server" CssClass="dv-input" placeholder="C:\Backups\GymApp_20260623.bak"></asp:TextBox>
            <p class="dv-ayuda">Esta acción reemplazará la base de datos actual. Todos los cambios posteriores al backup se perderán.</p>
            <asp:Button ID="btnConfirmarRestaurar" runat="server" Text="Confirmar restauración" CssClass="dv-btn dv-btn-danger"
                OnClick="btnConfirmarRestaurar_Click" />
            <asp:Button ID="btnCancelarRestaurar" runat="server" Text="Cancelar" CssClass="dv-btn dv-btn-secondary"
                OnClick="btnCancelarRestaurar_Click" />
        </asp:Panel>

        <div class="dv-resultados">
            <h3 class="dv-resultados-titulo"><i class="fa-solid fa-table-list"></i> Estado del control por tabla</h3>
            <asp:GridView ID="gvEstadoControl" runat="server" AutoGenerateColumns="false" CssClass="dv-grid"
                EmptyDataText="No se pudo cargar el estado de control."
                OnRowDataBound="gvEstadoControl_RowDataBound">
                <Columns>
                    <asp:BoundField DataField="NombreTabla" HeaderText="Tabla" />
                    <asp:TemplateField HeaderText="¿Control?">
                        <ItemTemplate>
                            <span class='<%# Eval("TieneControl").ToString() == "True" ? "dv-estado-ok" : "dv-estado-error" %>'>
                                <%# Eval("TieneControl").ToString() == "True" ? "Sí" : "No" %>
                            </span>
                        </ItemTemplate>
                    </asp:TemplateField>
                    <asp:BoundField DataField="TotalFilas" HeaderText="Filas" />
                    <asp:BoundField DataField="FilasDVHVacio" HeaderText="DVH vacío" />
                    <asp:BoundField DataField="FilasDVVVacio" HeaderText="DVV vacío" />
                    <asp:BoundField DataField="FechaCalculo" HeaderText="Último cálculo" DataFormatString="{0:dd/MM/yyyy HH:mm}" />
                </Columns>
            </asp:GridView>
        </div>

        <div class="dv-resultados">
            <h3 class="dv-resultados-titulo"><i class="fa-solid fa-list"></i> Detalle de verificación</h3>
            <asp:GridView ID="gvResultados" runat="server" AutoGenerateColumns="false" CssClass="dv-grid"
                EmptyDataText="No hay resultados para mostrar. Presione 'Verificar ahora'.">
                <Columns>
                    <asp:BoundField DataField="NombreTabla" HeaderText="Tabla" />
                    <asp:BoundField DataField="ClaveFila" HeaderText="Clave" />
                    <asp:BoundField DataField="Campo" HeaderText="Campo" />
                    <asp:TemplateField HeaderText="Estado">
                        <ItemTemplate>
                            <span class='<%# Eval("EsValido").ToString() == "True" ? "dv-estado-ok" : "dv-estado-error" %>'>
                                <%# Eval("EsValido").ToString() == "True" ? "OK" : "ERROR" %>
                            </span>
                        </ItemTemplate>
                    </asp:TemplateField>
                    <asp:BoundField DataField="Mensaje" HeaderText="Mensaje" />
                </Columns>
            </asp:GridView>
        </div>
    </asp:Panel>
</asp:Content>
