<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="Alumnos.aspx.cs" Inherits="gymAppV2.Alumnos.Alumnos" MasterPageFile="~/DashBoard.Master" EnableEventValidation="false" %>

<asp:Content ID="HeadContent" ContentPlaceHolderID="HeadContent" runat="server">
    <title>Gestión de Alumnos - GymApp</title>me sa
    <link href="<%= ResolveUrl("~/Alumnos/Alumnos.css?v=1") %>" rel="stylesheet" type="text/css" />
    <link rel="stylesheet" href="https://cdnjs.cloudflare.com/ajax/libs/font-awesome/6.5.0/css/all.min.css" />
</asp:Content>

<asp:Content ID="MainContent" ContentPlaceHolderID="MainContent" runat="server">
    <div class="alumnos-container">
        <!-- Page header -->
        <div class="alumnos-header">
            <div class="alumnos-title">
                <i class="fa-solid fa-user-graduate"></i>
                Gestión de Alumnos
                <span class="badge-count" id="badgeCount" runat="server">0 alumnos</span>
            </div>
        </div>

        <!-- Stats -->
        <div class="stats-row">
            <div class="stat-card">
                <div class="stat-icon stat-icon-pink"><i class="fa-solid fa-users"></i></div>
                <div class="stat-info"><p>Total alumnos</p><h4><asp:Label ID="lblTotal" runat="server" Text="0"></asp:Label></h4></div>
            </div>
            <div class="stat-card">
                <div class="stat-icon stat-icon-mint"><i class="fa-solid fa-calendar-check"></i></div>
                <div class="stat-info"><p>Activos</p><h4><asp:Label ID="lblActivos" runat="server" Text="0"></asp:Label></h4></div>
            </div>
            <div class="stat-card">
                <div class="stat-icon stat-icon-lavender"><i class="fa-solid fa-dumbbell"></i></div>
                <div class="stat-info"><p>Con rutinas</p><h4><asp:Label ID="lblConRutinas" runat="server" Text="0"></asp:Label></h4></div>
            </div>
            <div class="stat-card">
                <div class="stat-icon stat-icon-peach"><i class="fa-solid fa-user-group"></i></div>
                <div class="stat-info"><p>Sin usuario</p><h4><asp:Label ID="lblSinUsuario" runat="server" Text="0"></asp:Label></h4></div>
            </div>
        </div>

        <!-- Filters -->
        <div class="filter-card">
            <div style="font-size:0.78rem;font-weight:700;color:var(--text-muted);text-transform:uppercase;letter-spacing:0.5px;align-self:flex-end;padding-bottom:9px;">
                <i class="fa-solid fa-sliders" style="margin-right:6px"></i>Filtros
            </div>
            <div class="filter-group">
                <label>Estado</label>
                <asp:DropDownList ID="ddlEstado" runat="server" AutoPostBack="true" OnSelectedIndexChanged="ddlEstado_SelectedIndexChanged">
                    <asp:ListItem Value="">Todos</asp:ListItem>
                    <asp:ListItem Value="activo">Activos</asp:ListItem>
                    <asp:ListItem Value="inactivo">Inactivos</asp:ListItem>
                </asp:DropDownList>
            </div>
            <div class="filter-group">
                <label>Usuario asociado</label>
                <asp:DropDownList ID="ddlUsuario" runat="server" AutoPostBack="true" OnSelectedIndexChanged="ddlUsuario_SelectedIndexChanged">
                    <asp:ListItem Value="">Todos</asp:ListItem>
                </asp:DropDownList>
            </div>
            <div class="search-wrap">
                <label>Buscar</label>
                <div class="search-inner">
                    <i class="fa-solid fa-magnifying-glass"></i>
                    <asp:TextBox ID="txtBusqueda" runat="server" CssClass="search-input" placeholder="Nombre, apellido o DNI..." AutoPostBack="true" OnTextChanged="txtBusqueda_TextChanged"></asp:TextBox>
                </div>
            </div>
            <button id="btnFiltrar" runat="server" class="btn-filter" onserverclick="btnFiltrar_Click">
                <i class="fa-solid fa-magnifying-glass"></i> Filtrar
            </button>
        </div>

        <!-- Main content - Table and Form in single column -->
        <div class="main-content-vertical">

            <!-- Table -->
            <div class="table-card">
                <div class="table-card-header">
                    <h3><i class="fa-solid fa-table-list" style="margin-right:7px;color:var(--pink)"></i>Lista de alumnos</h3>
                </div>
                <asp:GridView ID="gvAlumnos" runat="server" AutoGenerateColumns="false" CssClass="table"
                    GridLines="None" AllowPaging="true" PageSize="10" OnPageIndexChanging="gvAlumnos_PageIndexChanging"
                    OnRowCommand="gvAlumnos_RowCommand" OnRowDataBound="gvAlumnos_RowDataBound"
                    DataKeyNames="DNI">
                    <Columns>
                        <asp:TemplateField>
                            <ItemTemplate>
                                <asp:LinkButton ID="btnSelect" runat="server" CommandName="Select"
                                    ToolTip="Seleccionar alumno" Style="display:block;width:100%;height:100%;padding:0.5rem;border:none;background:transparent;cursor:pointer;">
                                </asp:LinkButton>
                            </ItemTemplate>
                        </asp:TemplateField>
                        <asp:TemplateField HeaderText="Alumno">
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
                        <asp:BoundField DataField="Telefono" HeaderText="Teléfono" />
                        <asp:BoundField DataField="FechaNacimiento" HeaderText="Fecha Nacimiento" DataFormatString="{0:dd/MM/yyyy}" />
                        <asp:BoundField DataField="Peso" HeaderText="Peso (kg)" DataFormatString="{0:F1}" NullDisplayText="-" />
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
                            <i class="fa-solid fa-user-slash" style="font-size: 2rem; margin-bottom: 0.5rem;"></i>
                            <p>No se encontraron alumnos</p>
                        </div>
                    </EmptyDataTemplate>
                </asp:GridView>
                <div class="table-footer">
                    <span class="table-footer-text" id="footerText" runat="server">Mostrando 0 de 0 alumnos</span>
                </div>
                <div class="table-actions">
                    <button id="btnCrear" runat="server" class="btn-action btn-crear" onserverclick="btnCrear_Click">
                        <i class="fa-solid fa-plus"></i> Crear
                    </button>
                    <button id="btnModificar" runat="server" class="btn-action btn-modificar" onserverclick="btnModificar_Click">
                        <i class="fa-solid fa-pen"></i> Modificar
                    </button>
                    <button id="btnEliminar" runat="server" class="btn-action btn-eliminar" onserverclick="btnEliminar_Click">
                        <i class="fa-solid fa-trash"></i> Eliminar
                    </button>
                    <button id="btnAsociarUsuario" runat="server" class="btn-action btn-asociar" onserverclick="btnAsociarUsuario_Click">
                        <i class="fa-solid fa-link"></i> Asociar Usuario
                    </button>
                    <button id="btnCancelar" runat="server" class="btn-action btn-cancelar" onserverclick="btnCancelar_Click">
                        <i class="fa-solid fa-xmark"></i> Cancelar
                    </button>
                </div>
            </div>

            <!-- Form -->
            <asp:Panel ID="pnlForm" runat="server" Visible="false" CssClass="detail-card">
                <div class="detail-header">
                    <h3>
                        <i class="fa-solid fa-user-graduate"></i>
                        <asp:Label ID="lblFormTitle" runat="server" Text="Detalle del alumno"></asp:Label>
                    </h3>
                    <button id="btnCloseForm" runat="server" class="btn-icon" style="min-width:auto;padding:8px;" onserverclick="btnCloseForm_Click">
                        <i class="fa-solid fa-xmark"></i>
                    </button>
                </div>
                <div class="detail-body">
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
                            <label>Peso (kg)</label>
                            <asp:TextBox ID="txtPeso" runat="server" TextMode="Number" placeholder="0.0"></asp:TextBox>
                        </div>
                    </div>

                    <div class="form-row">
                        <div class="form-field">
                            <label>Estado</label>
                            <asp:CheckBox ID="chkActivo" runat="server" />
                        </div>
                        <div class="form-field">
                            <label>Asociar Usuario Cliente</label>
                            <asp:DropDownList ID="ddlUsuarioAsociar" runat="server">
                                <asp:ListItem Value="">-- Sin asociar --</asp:ListItem>
                            </asp:DropDownList>
                        </div>
                    </div>

                    <div class="form-row">
                        <div class="form-field full">
                            <button id="btnGuardar" runat="server" class="btn-guardar" onserverclick="btnGuardar_Click">
                                <i class="fa-solid fa-floppy-disk"></i> Guardar
                            </button>
                        </div>
                    </div>
                </div>
            </asp:Panel>

        </div><!-- /main-content-vertical -->
    </div>

    <!-- Panel de Confirmación Eliminación -->
    <asp:Panel ID="pnlConfirmarEliminar" runat="server" Visible="false" CssClass="modal-overlay">
        <div class="modal-content modal-sm">
            <div class="modal-header modal-header-warning">
                <h3><i class="fa-solid fa-triangle-exclamation"></i> Confirmar Eliminación</h3>
                <button id="btnCloseConfirm" runat="server" class="btn-close" onserverclick="btnCloseConfirm_Click">
                    <i class="fa-solid fa-xmark"></i>
                </button>
            </div>
            <div class="modal-body">
                <p><strong>¿Está seguro que desea eliminar este alumno?</strong></p>
                <p class="text-danger">
                    <i class="fa-solid fa-circle-exclamation"></i>
                    Esta acción eliminará también todas sus rutinas asociadas.
                </p>
                <p>Alumno: <asp:Label ID="lblAlumnoAEliminar" runat="server" Font-Bold="true"></asp:Label></p>
                <asp:HiddenField ID="hdnDniAEliminar" runat="server" />
            </div>
            <div class="modal-footer">
                <button id="btnCancelarEliminar" runat="server" class="btn-action btn-cancelar" onserverclick="btnCancelarEliminar_Click">
                    Cancelar
                </button>
                <button id="btnConfirmarEliminar" runat="server" class="btn-action btn-eliminar" onserverclick="btnConfirmarEliminar_Click">
                    Eliminar
                </button>
            </div>
        </div>
    </asp:Panel>
</asp:Content>