<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="Entrenadores.aspx.cs" Inherits="gymAppV2.Entrenadores.Entrenadores" MasterPageFile="~/DashBoard.Master" EnableEventValidation="false" Title="Entrenadores" %>

<asp:Content ID="HeadContent" ContentPlaceHolderID="HeadContent" runat="server">
    <link href="<%= ResolveUrl("~/Entrenadores/Entrenadores.css?v=2") %>" rel="stylesheet" type="text/css" />
    <link rel="stylesheet" href="https://cdnjs.cloudflare.com/ajax/libs/font-awesome/6.5.0/css/all.min.css" />
</asp:Content>

<asp:Content ID="MainContent" ContentPlaceHolderID="MainContent" runat="server">
    <div class="entrenadores-container">
        <!-- Page header -->
        <div class="entrenadores-header">
            <div class="entrenadores-title">
                <i class="fa-solid fa-person-chalkboard"></i>
                <asp:Literal ID="litTitulo" runat="server" Text="Gestión de Entrenadores" />
                <span class="badge-count" id="badgeCount" runat="server">0</span>
            </div>
        </div>

        <!-- Stats -->
        <div class="stats-row">
            <div class="stat-card">
                <div class="stat-icon stat-icon-lavender"><i class="fa-solid fa-people-group"></i></div>
                <div class="stat-info"><p><asp:Literal ID="litStatTotal" runat="server" Text="Total entrenadores" /></p><h4><asp:Label ID="lblTotal" runat="server" Text="0"></asp:Label></h4></div>
            </div>
            <div class="stat-card">
                <div class="stat-icon stat-icon-mint"><i class="fa-solid fa-calendar-check"></i></div>
                <div class="stat-info"><p><asp:Literal ID="litStatActivos" runat="server" Text="Activos" /></p><h4><asp:Label ID="lblActivos" runat="server" Text="0"></asp:Label></h4></div>
            </div>
            <div class="stat-card">
                <div class="stat-icon stat-icon-pink"><i class="fa-solid fa-dumbbell"></i></div>
                <div class="stat-info"><p><asp:Literal ID="litStatConAlumnos" runat="server" Text="Con alumnos" /></p><h4><asp:Label ID="lblConAlumnos" runat="server" Text="0"></asp:Label></h4></div>
            </div>
            <div class="stat-card">
                <div class="stat-icon stat-icon-peach"><i class="fa-solid fa-user-group"></i></div>
                <div class="stat-info"><p><asp:Literal ID="litStatSinUsuario" runat="server" Text="Sin usuario" /></p><h4><asp:Label ID="lblSinUsuario" runat="server" Text="0"></asp:Label></h4></div>
            </div>
        </div>

        <!-- Main content -->
        <div class="main-content-vertical">

            <!-- Table -->
            <div class="table-card">
                <div class="table-card-header">
                    <h3><i class="fa-solid fa-table-list" style="margin-right:7px;color:var(--lavender)"></i><asp:Literal ID="litListaTitulo" runat="server" Text="Lista de entrenadores" /></h3>
                </div>
                <asp:GridView ID="gvEntrenadores" runat="server" AutoGenerateColumns="false" CssClass="table"
                    GridLines="None" AllowPaging="true" PageSize="10" OnPageIndexChanging="gvEntrenadores_PageIndexChanging"
                    OnRowCommand="gvEntrenadores_RowCommand" OnRowDataBound="gvEntrenadores_RowDataBound"
                    DataKeyNames="DNI">
                    <Columns>
                        <asp:TemplateField>
                            <ItemTemplate>
                                <asp:LinkButton ID="btnSelect" runat="server" CommandName="Select"
                                    ToolTip="Seleccionar entrenador" Style="display:block;width:100%;height:100%;padding:0.5rem;border:none;background:transparent;cursor:pointer;">
                                </asp:LinkButton>
                            </ItemTemplate>
                        </asp:TemplateField>
                        <asp:TemplateField HeaderText="Entrenador">
                            <ItemTemplate>
                                <div class="td-name">
                                    <div class="td-avatar <%# GetAvatarClass(Container.DataItemIndex) %>">
                                        <%# GetInitials(Eval("Nombre"), Eval("Apellido")) %>
                                    </div>
                                    <div>
                                        <div style="font-weight:600;font-size:0.88rem"><%# Eval("Apellido") %>, <%# Eval("Nombre") %></div>
                                        <div style="font-size:0.76rem;color:var(--text-muted)">DNI: <%# Eval("DNI") %></div>
                                    </div>
                                </div>
                            </ItemTemplate>
                        </asp:TemplateField>
                        <asp:BoundField DataField="Telefono" HeaderText="Teléfono" NullDisplayText="-" />
                        <asp:BoundField DataField="FechaNacimiento" HeaderText="Fecha Nacimiento" DataFormatString="{0:dd/MM/yyyy}" />
                        <asp:BoundField DataField="AlumnosCount" HeaderText="Alumnos" />
                        <asp:TemplateField HeaderText="Usuario">
                            <ItemTemplate>
                                <span class="user-pill <%# GetUsuarioClass(Eval("Usuario")) %>">
                                    <%# Eval("Usuario") ?? "Sin usuario" %>
                                </span>
                            </ItemTemplate>
                        </asp:TemplateField>
                        <asp:TemplateField HeaderText="Estado">
                            <ItemTemplate>
                                <span class="pill <%# GetEstadoClass(Eval("Activo")) %>">
                                    <span class="pill-dot"></span><%# GetEstadoText(Eval("Activo")) %>
                                </span>
                            </ItemTemplate>
                        </asp:TemplateField>
                    </Columns>
                    <PagerStyle CssClass="pagination" />
                    <EmptyDataTemplate>
                        <div style="padding: 2rem; text-align: center; color: var(--text-muted);">
                            <i class="fa-solid fa-person-circle-xmark" style="font-size: 2rem; margin-bottom: 0.5rem;"></i>
                            <p><asp:Literal ID="litSinResultados" runat="server" Text="No hay entrenadores para mostrar." /></p>
                        </div>
                    </EmptyDataTemplate>
                </asp:GridView>
                <div class="table-footer">
                    <span class="table-footer-text" id="footerText" runat="server"></span>
                </div>
                <div class="table-actions">
                    <button id="btnCrear" runat="server" class="btn-action btn-crear" onserverclick="btnCrear_Click">
                        <i class="fa-solid fa-plus"></i> <asp:Literal ID="litBtnCrear" runat="server" Text="Crear" />
                    </button>
                    <button id="btnModificar" runat="server" class="btn-action btn-modificar" onserverclick="btnModificar_Click">
                        <i class="fa-solid fa-pen"></i> <asp:Literal ID="litBtnModificar" runat="server" Text="Modificar" />
                    </button>
                    <button id="btnEliminar" runat="server" class="btn-action btn-eliminar" onserverclick="btnEliminar_Click">
                        <i class="fa-solid fa-trash"></i> <asp:Literal ID="litBtnEliminar" runat="server" Text="Eliminar" />
                    </button>
                    <button id="btnCancelar" runat="server" class="btn-action btn-cancelar" onserverclick="btnCancelar_Click">
                        <i class="fa-solid fa-xmark"></i> <asp:Literal ID="litBtnCancelar" runat="server" Text="Cancelar" />
                    </button>
                </div>
            </div>

            <!-- Form -->
            <asp:Panel ID="pnlForm" runat="server" Visible="false" CssClass="modal-overlay">
                <div class="modal-content modal-form">
                <div class="modal-header">
                    <h3>
                        <i class="fa-solid fa-person-chalkboard"></i>
                        <asp:Label ID="lblFormTitle" runat="server" Text="Detalle del entrenador" />
                    </h3>
                    <button id="btnCloseForm" runat="server" class="btn-close" onserverclick="btnCloseForm_Click">
                        <i class="fa-solid fa-xmark"></i>
                    </button>
                </div>
                <div class="modal-body detail-body">
                    <div class="form-row">
                        <div class="form-field">
                            <label>DNI *</label>
                            <asp:TextBox ID="txtDNI" runat="server" placeholder="Ej: 30456789"></asp:TextBox>
                        </div>
                        <div class="form-field">
                            <label>Teléfono</label>
                            <asp:TextBox ID="txtTelefono" runat="server" placeholder="Ej: 11-4567-8901"></asp:TextBox>
                        </div>
                    </div>

                    <div class="form-row">
                        <div class="form-field">
                            <label>Apellido/s *</label>
                            <asp:TextBox ID="txtApellido" runat="server" placeholder="Apellido"></asp:TextBox>
                        </div>
                        <div class="form-field">
                            <label>Nombre/s *</label>
                            <asp:TextBox ID="txtNombre" runat="server" placeholder="Nombre"></asp:TextBox>
                        </div>
                    </div>

                    <div class="form-row">
                        <div class="form-field">
                            <label>Fecha de Nacimiento *</label>
                            <asp:TextBox ID="txtFechaNacimiento" runat="server" TextMode="Date"></asp:TextBox>
                        </div>
                        <div class="form-field">
                            <label>Estado</label>
                            <asp:CheckBox ID="chkActivo" runat="server" />
                        </div>
                    </div>

                </div>
                <div class="modal-footer">
                    <button id="btnCancelarForm" runat="server" class="btn-action btn-cancelar" onserverclick="btnCloseForm_Click">
                        <i class="fa-solid fa-xmark"></i> <%= T("btn_cancelar") %>
                    </button>
                    <button id="btnGuardar" runat="server" class="btn-action btn-guardar" onserverclick="btnGuardar_Click">
                        <i class="fa-solid fa-floppy-disk"></i> <asp:Literal ID="litBtnGuardar" runat="server" Text="Guardar" />
                    </button>
                </div>
                </div>
            </asp:Panel>

        </div><!-- /main-content-vertical -->

    <!-- Panel de Confirmación Eliminación -->
    <asp:Panel ID="pnlConfirmarEliminar" runat="server" Visible="false" CssClass="modal-overlay">
        <div class="modal-content modal-sm">
            <div class="modal-header modal-header-warning">
                <h3><i class="fa-solid fa-triangle-exclamation"></i> <asp:Literal ID="litConfirmarTitulo" runat="server" Text="Confirmar Eliminación" /></h3>
                <button id="btnCloseConfirm" runat="server" class="btn-close" onserverclick="btnCloseConfirm_Click">
                    <i class="fa-solid fa-xmark"></i>
                </button>
            </div>
            <div class="modal-body">
                <p><strong><asp:Literal ID="litConfirmarMsg" runat="server" Text="¿Está seguro que desea eliminar este entrenador?" /></strong></p>
                <p class="text-danger">
                    <i class="fa-solid fa-circle-exclamation"></i>
                    <asp:Literal ID="litConfirmarAviso" runat="server" Text="Esta acción eliminará también sus rutinas y actividades asociadas." />
                </p>
                <p>Entrenador: <asp:Label ID="lblEntrenadorAEliminar" runat="server" Font-Bold="true"></asp:Label></p>
                <asp:HiddenField ID="hdnDniAEliminar" runat="server" />
            </div>
            <div class="modal-footer">
                <button id="btnCancelarEliminar" runat="server" class="btn-action btn-cancelar" onserverclick="btnCancelarEliminar_Click">
                    <asp:Literal ID="litBtnCancelarEliminar" runat="server" Text="Cancelar" />
                </button>
                <button id="btnConfirmarEliminar" runat="server" class="btn-action btn-eliminar" onserverclick="btnConfirmarEliminar_Click">
                    <asp:Literal ID="litBtnConfirmarEliminar" runat="server" Text="Eliminar" />
                </button>
            </div>
        </div>
    </asp:Panel>
    </div><!-- /container -->
</asp:Content>
