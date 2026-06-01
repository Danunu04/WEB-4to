<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="UsuariosModulo.aspx.cs" Inherits="gymAppV2.Usuarios.UsuariosModulo" MasterPageFile="~/DashBoard.Master" EnableEventValidation="false" %>

<asp:Content ID="HeadContent" ContentPlaceHolderID="HeadContent" runat="server">
    <title>Gestión de Usuarios - GymApp</title>
    <link href="<%= ResolveUrl("~/Usuarios/Usuarios.css?v=2") %>" rel="stylesheet" type="text/css" />
    <link rel="stylesheet" href="https://cdnjs.cloudflare.com/ajax/libs/font-awesome/6.5.0/css/all.min.css">
</asp:Content>

<asp:Content ID="MainContent" ContentPlaceHolderID="MainContent" runat="server">
    <div class="usuarios-container">
        <!-- Page header -->
        <div class="usuarios-header">
            <div class="usuarios-title">
                <i class="fa-solid fa-users"></i>
                Gestión de Usuarios
                <span class="badge-count" id="badgeCount" runat="server">0 usuarios</span>
            </div>
        </div>

        <!-- Stats -->
        <div class="stats-row">
            <div class="stat-card">
                <div class="stat-icon stat-icon-teal"><i class="fa-solid fa-users"></i></div>
                <div class="stat-info"><p>Total usuarios</p><h4><asp:Label ID="lblTotal" runat="server" Text="0"></asp:Label></h4></div>
            </div>
            <div class="stat-card">
                <div class="stat-icon stat-icon-orange"><i class="fa-solid fa-circle-check"></i></div>
                <div class="stat-info"><p>Activos</p><h4><asp:Label ID="lblActivos" runat="server" Text="0"></asp:Label></h4></div>
            </div>
            <div class="stat-card">
                <div class="stat-icon stat-icon-red"><i class="fa-solid fa-lock"></i></div>
                <div class="stat-info"><p>Bloqueados</p><h4><asp:Label ID="lblBloqueados" runat="server" Text="0"></asp:Label></h4></div>
            </div>
            <div class="stat-card">
                <div class="stat-icon stat-icon-yellow"><i class="fa-solid fa-circle-xmark"></i></div>
                <div class="stat-info"><p>Inactivos</p><h4><asp:Label ID="lblInactivos" runat="server" Text="0"></asp:Label></h4></div>
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
                    <asp:ListItem Value="activo">Activados</asp:ListItem>
                    <asp:ListItem Value="inactivo">Desactivados</asp:ListItem>
                </asp:DropDownList>
            </div>
            <div class="filter-group">
                <label>Bloqueados</label>
                <asp:DropDownList ID="ddlBloqueado" runat="server" AutoPostBack="true" OnSelectedIndexChanged="ddlBloqueado_SelectedIndexChanged">
                    <asp:ListItem Value="">Todos</asp:ListItem>
                    <asp:ListItem Value="bloqueado">Bloqueados</asp:ListItem>
                    <asp:ListItem Value="no_bloqueado">No bloqueados</asp:ListItem>
                </asp:DropDownList>
            </div>
            <div class="filter-group">
                <label>Rol</label>
                <asp:DropDownList ID="ddlRol" runat="server" AutoPostBack="true" OnSelectedIndexChanged="ddlRol_SelectedIndexChanged">
                    <asp:ListItem Value="">Todos los roles</asp:ListItem>
                    <asp:ListItem Value="Administrador">Administrador</asp:ListItem>
                    <asp:ListItem Value="Recepcionista">Recepcionista</asp:ListItem>
                    <asp:ListItem Value="Entrenador">Entrenador</asp:ListItem>
                    <asp:ListItem Value="Cliente">Cliente</asp:ListItem>
                </asp:DropDownList>
            </div>
            <div class="search-wrap">
                <label>Buscar</label>
                <div class="search-inner">
                    <i class="fa-solid fa-magnifying-glass"></i>
                    <asp:TextBox ID="txtBusqueda" runat="server" CssClass="search-input" placeholder="Nombre, apellido o usuario..." AutoPostBack="true" OnTextChanged="txtBusqueda_TextChanged"></asp:TextBox>
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
                    <h3><i class="fa-solid fa-table-list" style="margin-right:7px;color:var(--pink)"></i>Lista de usuarios</h3>
                    <div class="table-actions-row">
                        <button id="btnExportar" runat="server" class="btn-action btn-actualizar" title="Exportar" onserverclick="btnExportar_Click">
                            <i class="fa-solid fa-file-export"></i> Exportar
                        </button>
                        <button id="btnActualizar" runat="server" class="btn-action btn-modificar" title="Actualizar" onserverclick="btnActualizar_Click">
                            <i class="fa-solid fa-arrows-rotate"></i> Actualizar
                        </button>
                    </div>
                </div>
                <div style="margin-bottom: 2px;">
                <asp:GridView ID="gvUsuarios" runat="server" AutoGenerateColumns="false" CssClass="table"
                    GridLines="None" AllowPaging="true" PageSize="10" OnPageIndexChanging="gvUsuarios_PageIndexChanging"
                    OnRowCommand="gvUsuarios_RowCommand" OnRowDataBound="gvUsuarios_RowDataBound"
                    DataKeyNames="USUARIO_Usuario" SelectedRowStyle-CssClass="selected">
                    <Columns>
                        <asp:TemplateField HeaderText="Usuario">
                            <ItemTemplate>
                                <div class="td-name">
                                    <div class="td-avatar <%# GetAvatarClass(Container.DataItemIndex) %>">
                                        <%# GetInitials(Eval("Nombre"), Eval("Apellido"), Eval("USUARIO_Usuario")) %>
                                    </div>
                                    <div>
                                        <div style="font-weight:600;font-size:0.88rem"><%# Eval("USUARIO_Usuario") %></div>
                                    </div>
                                </div>
                            </ItemTemplate>
                        </asp:TemplateField>
                        <asp:BoundField DataField="DNI" HeaderText="DNI" ItemStyle-CssClass="dni-cell" />
                        <asp:BoundField DataField="Telefono" HeaderText="Teléfono" />
                        <asp:TemplateField HeaderText="Rol">
                            <ItemTemplate>
                                <span class="role-pill <%# GetRolClass(Eval("USUARIO_Tipo")) %>">
                                    <%# Eval("USUARIO_Tipo") %>
                                </span>
                            </ItemTemplate>
                        </asp:TemplateField>
                        <asp:TemplateField HeaderText="Estado">
                            <ItemTemplate>
                                <span class="pill <%# GetEstadoClass(Eval("USUARIO_Activo")) %>">
                                    <span class="pill-dot"></span><%# GetEstadoText(Eval("USUARIO_Activo")) %>
                                </span>
                            </ItemTemplate>
                        </asp:TemplateField>
                        <asp:TemplateField HeaderText="Bloqueado">
                            <ItemTemplate>
                                <%# GetBloqueadoText(Eval("USUARIO_Bloqueado")) %>
                            </ItemTemplate>
                        </asp:TemplateField>
                    </Columns>
                    <PagerStyle CssClass="pagination" />
                    <EmptyDataTemplate>
                        <div style="padding: 2rem; text-align: center; color: var(--text-muted);">
                            <i class="fa-solid fa-users-slash" style="font-size: 2rem; margin-bottom: 0.5rem;"></i>
                            <p>No se encontraron usuarios</p>
                        </div>
                    </EmptyDataTemplate>
                </asp:GridView>
                </div>
                <div class="table-footer">
                    <span class="table-footer-text" id="footerText" runat="server">Mostrando 0 de 0 usuarios</span>
                </div>
                <div class="table-actions" style="display:flex;justify-content:flex-end;gap:0.5rem;padding-top:1rem;border-top:1px solid var(--border-color);margin:4px;">
                    <button id="btnCrear" runat="server" class="btn-action btn-crear" onserverclick="btnCrear_Click">
                        <i class="fa-solid fa-plus"></i> Crear
                    </button>
                    <button id="btnModificar" runat="server" class="btn-action btn-modificar" onserverclick="btnModificar_Click">
                        <i class="fa-solid fa-pen"></i> Modificar
                    </button>
                    <button id="btnDesbloquear" runat="server" class="btn-action btn-desbloquear" onserverclick="btnDesbloquear_Click">
                        <i class="fa-solid fa-lock-open"></i> Desbloquear
                    </button>
                    <button id="btnActivar" runat="server" class="btn-action btn-activar" onserverclick="btnActivar_Click">
                        <i class="fa-solid fa-circle-check"></i> Activar
                    </button>
                    <button id="btnDesactivar" runat="server" class="btn-action btn-desactivar" onserverclick="btnDesactivar_Click">
                        <i class="fa-solid fa-circle-xmark"></i> Desactivar
                    </button>
                    <button id="btnCancelar" runat="server" class="btn-action btn-cancelar" onserverclick="btnCancelar_Click">
                        <i class="fa-solid fa-xmark"></i> Cancelar
                    </button>
                </div>
            </div>

            <!-- Form - Now below the table -->
            <asp:Panel ID="pnlForm" runat="server" Visible="false" style="background:var(--card-bg);border-radius:0.625rem;padding:1.25rem;border:1px solid var(--border-color);box-shadow:0 0.125rem 0.375rem rgba(0,0,0,0.1);">
                <div style="display:flex;justify-content:space-between;align-items:center;padding-bottom:0.9375rem;border-bottom:1px solid var(--border-color);margin-bottom:1.25rem;">
                    <h3 style="margin:0;font-size:1.125rem;font-weight:600;color:var(--text-color);display:flex;align-items:center;gap:0.5rem;">
                        <i class="fa-solid fa-id-card" style="color:var(--pink);"></i>
                        <asp:Label ID="lblFormTitle" runat="server" Text="Detalle del usuario"></asp:Label>
                    </h3>
                    <button id="btnCloseForm" runat="server" style="background:transparent;border:none;color:var(--text-muted);cursor:pointer;padding:0.5rem;border-radius:0.375rem;font-size:1rem;" onserverclick="btnCloseForm_Click">
                        <i class="fa-solid fa-xmark"></i>
                    </button>
                </div>

                <div style="display:grid;grid-template-columns:1fr 1fr;gap:1rem;">
                    <div style="display:flex;flex-direction:column;gap:0.375rem;">
                        <label style="font-size:0.8125rem;font-weight:600;color:var(--text-color);">DNI</label>
                        <asp:TextBox ID="txtDNI" runat="server" placeholder="Ej: 30456789" style="padding:0.625rem;border:1px solid var(--border-color);border-radius:0.375rem;font-size:0.875rem;"></asp:TextBox>
                        <asp:RequiredFieldValidator ID="RequiredFieldValidator1" runat="server" ErrorMessage="El dni es obligatorio" ControlToValidate="txtDNI" ForeColor="Red"></asp:RequiredFieldValidator>
                    </div>
                    <div style="display:flex;flex-direction:column;gap:0.375rem;">
                        <label style="font-size:0.8125rem;font-weight:600;color:var(--text-color);">Teléfono</label>
                        <asp:TextBox ID="txtTelefono" runat="server" placeholder="Ej: 11-4567-8901" style="padding:0.625rem;border:1px solid var(--border-color);border-radius:0.375rem;font-size:0.875rem;"></asp:TextBox>
                         <asp:RequiredFieldValidator ID="RequiredFieldValidator2" runat="server" ErrorMessage="El Telefono es obligatorio" ControlToValidate="txtTelefono" ForeColor="Red"></asp:RequiredFieldValidator>
                    </div>
                </div>

                <div style="display:grid;grid-template-columns:1fr 1fr;gap:1rem;margin-top:1rem;">
                    <div style="display:flex;flex-direction:column;gap:0.375rem;">
                        <label style="font-size:0.8125rem;font-weight:600;color:var(--text-color);">Apellido/s</label>
                        <asp:TextBox ID="txtApellido" runat="server" placeholder="Apellido" style="padding:0.625rem;border:1px solid var(--border-color);border-radius:0.375rem;font-size:0.875rem;"></asp:TextBox>
                         <asp:RequiredFieldValidator ID="RequiredFieldValidator3" runat="server" ErrorMessage="El apellido es obligatorio" ControlToValidate="txtApellido" ForeColor="Red"></asp:RequiredFieldValidator>
                    </div>
                    <div style="display:flex;flex-direction:column;gap:0.375rem;">
                        <label style="font-size:0.8125rem;font-weight:600;color:var(--text-color);">Nombre/s</label>
                        <asp:TextBox ID="txtNombre" runat="server" placeholder="Nombre" style="padding:0.625rem;border:1px solid var(--border-color);border-radius:0.375rem;font-size:0.875rem;"></asp:TextBox>
                         <asp:RequiredFieldValidator ID="RequiredFieldValidator4" runat="server" ErrorMessage="El nombre es obligatorio" ControlToValidate="txtNombre" ForeColor="Red"></asp:RequiredFieldValidator>
                    </div>
                </div>

                <div style="display:flex;flex-direction:column;gap:0.375rem;margin-top:1rem;">
                    <label style="font-size:0.8125rem;font-weight:600;color:var(--text-color);">Email</label>
                    <asp:TextBox ID="txtEmail" runat="server" placeholder="usuario@email.com" TextMode="Email" style="padding:0.625rem;border:1px solid var(--border-color);border-radius:0.375rem;font-size:0.875rem;"></asp:TextBox>
                    <asp:RegularExpressionValidator ID="RegularExpressionValidator1" runat="server" ErrorMessage="Email inválido" ControlToValidate="txtEmail" ForeColor="Red" ValidationExpression="\w+([-+.']\w+)*@\w+([-.]\w+)*\.\w+([-.]\w+)*"></asp:RegularExpressionValidator>
                </div>

                <div style="display:grid;grid-template-columns:1fr 1fr;gap:1rem;margin-top:1rem;">
                    <div style="display:flex;flex-direction:column;gap:0.375rem;">
                        <label style="font-size:0.8125rem;font-weight:600;color:var(--text-color);">Nombre de usuario</label>
                        <asp:TextBox ID="txtUsuario" runat="server" placeholder="usuario123" style="padding:0.625rem;border:1px solid var(--border-color);border-radius:0.375rem;font-size:0.875rem;"></asp:TextBox>
                         <asp:RequiredFieldValidator ID="RequiredFieldValidator6" runat="server" ErrorMessage="El ususario es obligatorio" ControlToValidate="txtUsuario" ForeColor="Red"></asp:RequiredFieldValidator>
                    </div>
                    <div style="display:flex;flex-direction:column;gap:0.375rem;">
                        <label style="font-size:0.8125rem;font-weight:600;color:var(--text-color);">Rol</label>
                        <asp:DropDownList ID="ddlRolForm" runat="server" AutoPostBack="true" OnSelectedIndexChanged="ddlRolForm_SelectedIndexChanged" style="padding:0.625rem;border:1px solid var(--border-color);border-radius:0.375rem;font-size:0.875rem;background:var(--card-bg);color:var(--text-color);">
                            <asp:ListItem Value="">Seleccionar rol</asp:ListItem>
                            <asp:ListItem Value="1">Administrador</asp:ListItem>
                            <asp:ListItem Value="2">Recepcionista</asp:ListItem>
                            <asp:ListItem Value="3">Entrenador</asp:ListItem>
                            <asp:ListItem Value="4">Cliente</asp:ListItem>
                        </asp:DropDownList>
                         <asp:RequiredFieldValidator ID="RequiredFieldValidator7" runat="server" ErrorMessage="Selecciona un tipo de usuario" ControlToValidate="ddlRolForm" ForeColor="Red"></asp:RequiredFieldValidator>
                    </div>
                </div>

                <div style="display:grid;grid-template-columns:1fr 1fr;gap:1rem;margin-top:1rem;">
                    <div style="display:flex;flex-direction:column;gap:0.375rem;">
                        <label style="font-size:0.8125rem;font-weight:600;color:var(--text-color);">Fecha de Nacimiento</label>
                        <asp:TextBox ID="txtFechaNacimiento" runat="server" TextMode="Date" style="padding:0.625rem;border:1px solid var(--border-color);border-radius:0.375rem;font-size:0.875rem;"></asp:TextBox>
                        <asp:RequiredFieldValidator ID="RequiredFieldValidator8" runat="server" ErrorMessage="La fecha de nacimiento es obligatoria" ControlToValidate="txtFechaNacimiento" ForeColor="Red" Display="Dynamic"></asp:RequiredFieldValidator>
                    </div>
                    <div style="display:flex;flex-direction:column;gap:0.375rem;">
                        <label style="font-size:0.8125rem;font-weight:600;color:var(--text-color);">Estado</label>
                        <asp:DropDownList ID="ddlEstadoForm" runat="server" style="padding:0.625rem;border:1px solid var(--border-color);border-radius:0.375rem;font-size:0.875rem;background:var(--card-bg);color:var(--text-color);">
                            <asp:ListItem Value="1">Activo</asp:ListItem>
                            <asp:ListItem Value="0">Inactivo</asp:ListItem>
                        </asp:DropDownList>
                    </div>
                </div>

                    <asp:Panel ID="EntField" runat="server" Visible="false" style="margin-top:1.25rem;padding:0.9375rem;background:var(--bg-light);border-radius:0.375rem;border-left:0.25rem solid var(--pink);">
                        <div style="margin-bottom:0.9375rem;">
                            <label style="font-size:0.875rem;font-weight:600;color:var(--pink);display:flex;align-items:center;gap:0.5rem;">
                                <i class="fa-solid fa-dumbbell"></i> Datos específicos del Entrenador
                            </label>
                            <small style="color:var(--text-muted);font-size:0.75rem;">
                                <i class="fa-solid fa-info-circle"></i> Al guardar, se creará el registro en la tabla ENTRENADORES con el mismo DNI
                            </small>
                        </div>
                    </asp:Panel>

                    <asp:Panel ID="clienteFields" runat="server" Visible="false" style="margin-top:1.25rem;padding:0.9375rem;background:var(--bg-light);border-radius:0.375rem;border-left:0.25rem solid var(--pink);">
                        <div style="margin-bottom:0.9375rem;">
                            <label style="font-size:0.875rem;font-weight:600;color:var(--pink);display:flex;align-items:center;gap:0.5rem;">
                                <i class="fa-solid fa-user"></i> Datos específicos del Cliente
                            </label>
                            <small style="color:var(--text-muted);font-size:0.75rem;">
                                <i class="fa-solid fa-info-circle"></i> Al guardar, se creará el registro en la tabla ALUMNOS con el mismo DNI
                            </small>
                        </div>
                    </asp:Panel>

                    <div id="passwordRow" runat="server" style="display:flex;flex-direction:column;gap:0.375rem;margin-top:1rem;">
                        <label style="font-size:0.8125rem;font-weight:600;color:var(--text-color);">Contraseña (opcional - se genera automáticamente)</label>
                        <asp:TextBox ID="txtContrasena" runat="server" placeholder="Se generará automáticamente: Apellido + DNI" style="padding:0.625rem;border:1px solid var(--border-color);border-radius:0.375rem;font-size:0.875rem;"></asp:TextBox>
                        <small style="color:var(--text-muted);font-size:0.75rem;">
                            <i class="fa-solid fa-info-circle"></i> Si se deja vacío, se generará automáticamente: Apellido + DNI
                        </small>
                    </div>

                    <div style="display:flex;gap:0.75rem;margin-top:1.5rem;padding-top:1rem;border-top:1px solid var(--border-color);">
                        <button id="btnGuardar" runat="server" style="flex:1;padding:0.75rem;background-color:lightpink;color:white;border:none;border-radius:0.375rem;font-weight:600;cursor:pointer;display:flex;align-items:center;justify-content:center;gap:0.5rem;font-size:0.875rem;" onserverclick="btnGuardar_Click">
                            <i class="fa-solid fa-floppy-disk"></i> Guardar
                        </button>
                        <button id="btnCancelarForm" runat="server" style="flex:1;padding:0.75rem;background-color:lightpink;color:white;border:none;border-radius:0.375rem;font-weight:600;cursor:pointer;display:flex;align-items:center;justify-content:center;gap:0.5rem;font-size:0.875rem;" onserverclick="btnCancelarForm_Click">
                            <i class="fa-solid fa-xmark"></i> Cancelar
                        </button>
                    </div>
            </asp:Panel>

        </div><!-- /main-content-vertical -->
    </div>
</asp:Content>