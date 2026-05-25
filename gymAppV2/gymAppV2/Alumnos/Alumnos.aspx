<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="Alumnos.aspx.cs" Inherits="gymAppV2.Alumnos.Alumnos" MasterPageFile="~/DashBoard.Master" EnableEventValidation="false" %>

<asp:Content ID="HeadContent" ContentPlaceHolderID="HeadContent" runat="server">
    <title>Gestión de Alumnos - GymApp</title>
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

        <!-- Main grid -->
        <div class="main-grid">

            <!-- Table -->
            <div>
                <div class="table-card">
                    <div class="table-card-header">
                        <h3><i class="fa-solid fa-table-list" style="margin-right:7px;color:var(--pink)"></i>Lista de alumnos</h3>
                        <div class="table-actions-row">
                            <button id="btnExportar" runat="server" class="btn-icon" title="Exportar" onserverclick="btnExportar_Click">
                                <i class="fa-solid fa-file-export"></i>
                            </button>
                            <button id="btnActualizar" runat="server" class="btn-icon" title="Actualizar" onserverclick="btnActualizar_Click">
                                <i class="fa-solid fa-arrows-rotate"></i>
                            </button>
                            <button id="btnNuevo" runat="server" class="btn-icon" title="Nuevo Alumno" onserverclick="btnNuevo_Click">
                                <i class="fa-solid fa-plus"></i>
                            </button>
                        </div>
                    </div>
                    <asp:GridView ID="gvAlumnos" runat="server" AutoGenerateColumns="false" CssClass="table"
                        GridLines="None" AllowPaging="true" PageSize="10" OnPageIndexChanging="gvAlumnos_PageIndexChanging"
                        OnRowCommand="gvAlumnos_RowCommand" OnRowDataBound="gvAlumnos_RowDataBound"
                        DataKeyNames="DNI" SelectedRowStyle-CssClass="selected" Width="100%">
                        <Columns>
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
                            <asp:TemplateField HeaderText="Acciones">
                                <ItemTemplate>
                                    <div class="action-buttons-inline">
                                        <asp:LinkButton ID="btnModificar" runat="server" CssClass="btn-icon-small" CommandName="Modificar" CommandArgument='<%# Container.DataItemIndex %>' ToolTip="Modificar">
                                            <i class="fa-solid fa-pen"></i>
                                        </asp:LinkButton>
                                        <asp:LinkButton ID="btnEliminar" runat="server" CssClass="btn-icon-small btn-icon-danger" CommandName="Eliminar" CommandArgument='<%# Container.DataItemIndex %>' ToolTip="Eliminar">
                                            <i class="fa-solid fa-trash"></i>
                                        </asp:LinkButton>
                                    </div>
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
                </div>
            </div>

        </div><!-- /main-grid -->
    </div>

    <!-- Modal de Crear/Modificar Alumno -->
    <asp:Panel ID="pnlFormulario" runat="server" CssClass="modal-overlay" Visible="false">
        <div class="modal-content">
            <div class="modal-header">
                <h3><i class="fa-solid fa-user-plus"></i> <asp:Label ID="lblFormTitle" runat="server" Text="Nuevo Alumno"></asp:Label></h3>
                <button id="btnCloseForm" runat="server" class="btn-close" onserverclick="btnCloseForm_Click">
                    <i class="fa-solid fa-times"></i>
                </button>
            </div>

            <div class="modal-body">
                <div class="form-row">
                    <div class="form-group">
                        <label for="txtDNI">DNI *</label>
                        <asp:TextBox ID="txtDNI" runat="server" CssClass="form-control" placeholder="Ingrese DNI"></asp:TextBox>
                    </div>
                    <div class="form-group">
                        <label for="txtNombre">Nombre *</label>
                        <asp:TextBox ID="txtNombre" runat="server" CssClass="form-control" placeholder="Ingrese nombre"></asp:TextBox>
                    </div>
                </div>

                <div class="form-row">
                    <div class="form-group">
                        <label for="txtApellido">Apellido *</label>
                        <asp:TextBox ID="txtApellido" runat="server" CssClass="form-control" placeholder="Ingrese apellido"></asp:TextBox>
                    </div>
                    <div class="form-group">
                        <label for="txtTelefono">Teléfono</label>
                        <asp:TextBox ID="txtTelefono" runat="server" CssClass="form-control" placeholder="Ingrese teléfono"></asp:TextBox>
                    </div>
                </div>

                <div class="form-row">
                    <div class="form-group">
                        <label for="txtFechaNacimiento">Fecha de Nacimiento *</label>
                        <asp:TextBox ID="txtFechaNacimiento" runat="server" CssClass="form-control" TextMode="Date"></asp:TextBox>
                    </div>
                    <div class="form-group">
                        <label for="txtPeso">Peso (kg)</label>
                        <asp:TextBox ID="txtPeso" runat="server" CssClass="form-control" TextMode="Number" placeholder="0.0"></asp:TextBox>
                    </div>
                </div>

                <div class="form-row">
                    <div class="form-group">
                        <label for="chkActivo">Activo</label>
                        <asp:CheckBox ID="chkActivo" runat="server" Checked="true" />
                    </div>
                </div>

                <div class="form-row">
                    <div class="form-group">
                        <label for="ddlUsuarioAsociar">Asociar Usuario Cliente (opcional)</label>
                        <asp:DropDownList ID="ddlUsuarioAsociar" runat="server" CssClass="form-control">
                            <asp:ListItem Value="">-- Sin asociar --</asp:ListItem>
                        </asp:DropDownList>
                        <small class="form-text">Solo usuarios Cliente sin alumno asociado</small>
                    </div>
                </div>
            </div>

            <div class="modal-footer">
                <button id="btnCancelarForm" runat="server" class="btn btn-secondary" onserverclick="btnCancelarForm_Click">Cancelar</button>
                <button id="btnGuardar" runat="server" class="btn btn-primary" onserverclick="btnGuardar_Click">Guardar</button>
            </div>
        </div>
    </asp:Panel>

    <!-- Modal de Confirmación Eliminación -->
    <asp:Panel ID="pnlConfirmarEliminar" runat="server" CssClass="modal-overlay" Visible="false">
        <div class="modal-content modal-sm">
            <div class="modal-header modal-header-warning">
                <h3><i class="fa-solid fa-triangle-exclamation"></i> Confirmar Eliminación</h3>
                <button id="btnCloseConfirm" runat="server" class="btn-close" onserverclick="btnCloseConfirm_Click">
                    <i class="fa-solid fa-times"></i>
                </button>
            </div>

            <div class="modal-body">
                <p><strong>¿Está seguro que desea eliminar este alumno?</strong></p>
                <p class="text-danger">
                    <i class="fa-solid fa-circle-exclamation"></i>
                    Esta acción eliminará también todas sus rutinas asociadas.
                </p>
                <p>Alumno: <asp:Label ID="lblAlumnoAEliminar" runat="server"></asp:Label></p>
                <asp:HiddenField ID="hdnDniAEliminar" runat="server" />
            </div>

            <div class="modal-footer">
                <button id="btnCancelarEliminar" runat="server" class="btn btn-secondary" onserverclick="btnCancelarEliminar_Click">Cancelar</button>
                <button id="btnConfirmarEliminar" runat="server" class="btn btn-danger" onserverclick="btnConfirmarEliminar_Click">Eliminar</button>
            </div>
        </div>
    </asp:Panel>
</asp:Content>