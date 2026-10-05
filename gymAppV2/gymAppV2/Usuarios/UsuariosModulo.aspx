<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="UsuariosModulo.aspx.cs" Inherits="gymAppV2.Usuarios.UsuariosModulo" MasterPageFile="~/DashBoard.Master" EnableEventValidation="false" %>

<asp:Content ID="HeadContent" ContentPlaceHolderID="HeadContent" runat="server">
    <title>Gestión de Usuarios - GymApp</title>
    <link href="<%= ResolveUrl("~/Usuarios/Usuarios.css?v=3") %>" rel="stylesheet" type="text/css" />
    <link rel="stylesheet" href="https://cdnjs.cloudflare.com/ajax/libs/font-awesome/6.5.0/css/all.min.css">
    <script type="text/javascript">
        function soloNumeros(e) {
            var key = e.keyCode || e.which;
            var tecla = String.fromCharCode(key);
            // Permitir: números (48-57), backspace (8), tab (9), delete (46), flechas (37-40)
            if ((key >= 48 && key <= 57) || key == 8 || key == 9 || key == 46 || (key >= 37 && key <= 40)) {
                return true;
            }
            return false;
        }
    </script>
</asp:Content>

<asp:Content ID="MainContent" ContentPlaceHolderID="MainContent" runat="server">
    <div class="usuarios-container">
        <!-- Page header -->
        <div class="usuarios-header">
            <div class="usuarios-title">
                <i class="fa-solid fa-users"></i>
                <asp:Literal ID="litTitulo" runat="server" Text="Gestión de Usuarios" />
                <span class="badge-count" id="badgeCount" runat="server">0</span>
            </div>
        </div>

        <!-- Stats -->
        <div class="stats-row">
            <div class="stat-card">
                <div class="stat-icon stat-icon-teal"><i class="fa-solid fa-users"></i></div>
                <div class="stat-info"><p><asp:Literal ID="litStatTotal" runat="server" Text="Total usuarios" /></p><h4><asp:Label ID="lblTotal" runat="server" Text="0"></asp:Label></h4></div>
            </div>
            <div class="stat-card">
                <div class="stat-icon stat-icon-orange"><i class="fa-solid fa-circle-check"></i></div>
                <div class="stat-info"><p><asp:Literal ID="litStatActivos" runat="server" Text="Activos" /></p><h4><asp:Label ID="lblActivos" runat="server" Text="0"></asp:Label></h4></div>
            </div>
            <div class="stat-card">
                <div class="stat-icon stat-icon-red"><i class="fa-solid fa-lock"></i></div>
                <div class="stat-info"><p><asp:Literal ID="litStatBloqueados" runat="server" Text="Bloqueados" /></p><h4><asp:Label ID="lblBloqueados" runat="server" Text="0"></asp:Label></h4></div>
            </div>
            <div class="stat-card">
                <div class="stat-icon stat-icon-yellow"><i class="fa-solid fa-circle-xmark"></i></div>
                <div class="stat-info"><p><asp:Literal ID="litStatInactivos" runat="server" Text="Inactivos" /></p><h4><asp:Label ID="lblInactivos" runat="server" Text="0"></asp:Label></h4></div>
            </div>
        </div>

        <!-- Filters -->
        <div class="filter-card">
            <div style="font-size:0.78rem;font-weight:700;color:var(--text-muted);text-transform:uppercase;letter-spacing:0.5px;align-self:flex-end;padding-bottom:9px;">
                <i class="fa-solid fa-sliders" style="margin-right:6px"></i><%= T("usuarios_label_filtros") %>
            </div>
            <div class="filter-group">
                <label><%= T("usuarios_label_estado") %></label>
                <asp:DropDownList ID="ddlEstado" runat="server" AutoPostBack="true" OnSelectedIndexChanged="ddlEstado_SelectedIndexChanged">
                    <asp:ListItem Value="">Todos</asp:ListItem>
                    <asp:ListItem Value="activo">Activados</asp:ListItem>
                    <asp:ListItem Value="inactivo">Desactivados</asp:ListItem>
                </asp:DropDownList>
            </div>
            <div class="filter-group">
                <label><%= T("usuarios_label_bloqueados") %></label>
                <asp:DropDownList ID="ddlBloqueado" runat="server" AutoPostBack="true" OnSelectedIndexChanged="ddlBloqueado_SelectedIndexChanged">
                    <asp:ListItem Value="">Todos</asp:ListItem>
                    <asp:ListItem Value="bloqueado">Bloqueados</asp:ListItem>
                    <asp:ListItem Value="no_bloqueado">No bloqueados</asp:ListItem>
                </asp:DropDownList>
            </div>
            <div class="filter-group">
                <label><%= T("usuarios_label_rol") %></label>
                <asp:DropDownList ID="ddlRol" runat="server" AutoPostBack="true" OnSelectedIndexChanged="ddlRol_SelectedIndexChanged">
                    <asp:ListItem Value="">Todos los roles</asp:ListItem>
                    <asp:ListItem Value="WebMaster">WebMaster</asp:ListItem>
                    <asp:ListItem Value="Administrador">Administrador</asp:ListItem>
                    <asp:ListItem Value="Recepcionista">Recepcionista</asp:ListItem>
                    <asp:ListItem Value="Entrenador">Entrenador</asp:ListItem>
                    <asp:ListItem Value="Cliente">Cliente</asp:ListItem>
                    <asp:ListItem Value="Familiar">Familiar</asp:ListItem>
                </asp:DropDownList>
            </div>
            <div class="search-wrap">
                <label><%= T("usuarios_label_buscar") %></label>
                <div class="search-inner">
                    <i class="fa-solid fa-magnifying-glass"></i>
                    <asp:TextBox ID="txtBusqueda" runat="server" CssClass="search-input" placeholder="Nombre, apellido o usuario..." AutoPostBack="true" OnTextChanged="txtBusqueda_TextChanged"></asp:TextBox>
                </div>
            </div>
            <button id="btnFiltrar" runat="server" class="btn-filter" onserverclick="btnFiltrar_Click">
                <i class="fa-solid fa-magnifying-glass"></i> <%= T("usuarios_btn_filtrar") %>
            </button>
        </div>

        <!-- Main content - Table and Form in single column -->
        <div class="main-content-vertical">

            <!-- Table -->
            <div class="table-card">
                <div class="table-card-header">
                    <h3><i class="fa-solid fa-table-list" style="margin-right:7px;color:var(--pink)"></i><asp:Literal ID="litListaTitulo" runat="server" Text="Lista de usuarios" /></h3>
                    <div class="table-actions-row">
                        <button id="btnExportar" runat="server" class="btn-action btn-actualizar" title="Exportar" onserverclick="btnExportar_Click">
                            <i class="fa-solid fa-file-export"></i> <%= T("usuarios_btn_exportar") %>
                        </button>
                        <button id="btnActualizar" runat="server" class="btn-action btn-modificar" title="Actualizar" onserverclick="btnActualizar_Click">
                            <i class="fa-solid fa-arrows-rotate"></i> <%= T("usuarios_btn_actualizar") %>
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
                            <p><%= T("usuarios_sin_resultados") %></p>
                        </div>
                    </EmptyDataTemplate>
                </asp:GridView>
                </div>
                <div class="table-footer">
                    <span class="table-footer-text" id="footerText" runat="server"></span>
                </div>
                <div class="table-actions" style="display:flex;justify-content:flex-end;gap:0.5rem;padding-top:1rem;border-top:1px solid var(--border-color);margin:4px;">
                    <button id="btnCrear" runat="server" class="btn-action btn-crear" onserverclick="btnCrear_Click">
                        <i class="fa-solid fa-plus"></i> <asp:Literal ID="litBtnCrear" runat="server" Text="Crear" />
                    </button>
                    <button id="btnModificar" runat="server" class="btn-action btn-modificar" onserverclick="btnModificar_Click">
                        <i class="fa-solid fa-pen"></i> <asp:Literal ID="litBtnModificar" runat="server" Text="Modificar" />
                    </button>
                    <button id="btnDesbloquear" runat="server" class="btn-action btn-desbloquear" onserverclick="btnDesbloquear_Click">
                        <i class="fa-solid fa-lock-open"></i> <asp:Literal ID="litBtnDesbloquear" runat="server" Text="Desbloquear" />
                    </button>
                    <button id="btnBlanquearContrasena" runat="server" class="btn-action btn-desbloquear" onserverclick="btnBlanquearContrasena_Click" title="Fuerza al usuario a cambiar su contraseña en el próximo inicio de sesión">
                        <i class="fa-solid fa-key"></i> <asp:Literal ID="litBtnBlanquear" runat="server" Text="Blanquear contraseña" />
                    </button>
                    <button id="btnActivar" runat="server" class="btn-action btn-activar" onserverclick="btnActivar_Click">
                        <i class="fa-solid fa-circle-check"></i> <asp:Literal ID="litBtnActivar" runat="server" Text="Activar" />
                    </button>
                    <button id="btnDesactivar" runat="server" class="btn-action btn-desactivar" onserverclick="btnDesactivar_Click">
                        <i class="fa-solid fa-circle-xmark"></i> <asp:Literal ID="litBtnDesactivar" runat="server" Text="Desactivar" />
                    </button>
                    <button id="btnCancelar" runat="server" class="btn-action btn-cancelar" onserverclick="btnCancelar_Click">
                        <i class="fa-solid fa-xmark"></i> <asp:Literal ID="litBtnCancelar" runat="server" Text="Cancelar" />
                    </button>
                </div>
            </div>

        </div><!-- /main-content-vertical -->

        <!-- Formulario de alta/modificación (ventana flotante) -->
        <asp:Panel ID="pnlForm" runat="server" Visible="false" CssClass="modal-overlay">
            <div class="modal-content modal-form">
                <div class="modal-header">
                    <h3>
                        <i class="fa-solid fa-id-card"></i>
                        <asp:Label ID="lblFormTitle" runat="server" Text="Detalle del usuario" />
                    </h3>
                    <button id="btnCloseForm" runat="server" class="btn-close" onserverclick="btnCloseForm_Click">
                        <i class="fa-solid fa-xmark"></i>
                    </button>
                </div>

                <div class="modal-body">
                    <div class="form-row">
                        <div class="form-field">
                            <label>DNI</label>
                            <asp:TextBox ID="txtDNI" runat="server" placeholder="Ej: 30456789" onkeypress="return soloNumeros(event)" maxlength="10"></asp:TextBox>
                            <asp:RequiredFieldValidator ID="RequiredFieldValidator1" runat="server" ErrorMessage="El dni es obligatorio" ControlToValidate="txtDNI" CssClass="field-error" Display="Dynamic"></asp:RequiredFieldValidator>
                            <asp:RegularExpressionValidator ID="RegularExpressionValidatorDNI" runat="server" ErrorMessage="El DNI solo debe contener números" ControlToValidate="txtDNI" CssClass="field-error" Display="Dynamic" ValidationExpression="^\d+$"></asp:RegularExpressionValidator>
                        </div>
                        <div class="form-field">
                            <label>Teléfono</label>
                            <asp:TextBox ID="txtTelefono" runat="server" placeholder="Ej: 11-4567-8901" onkeypress="return soloNumeros(event)" maxlength="15"></asp:TextBox>
                            <asp:RequiredFieldValidator ID="RequiredFieldValidator2" runat="server" ErrorMessage="El Telefono es obligatorio" ControlToValidate="txtTelefono" CssClass="field-error" Display="Dynamic"></asp:RequiredFieldValidator>
                            <asp:RegularExpressionValidator ID="RegularExpressionValidatorTel" runat="server" ErrorMessage="El Teléfono solo debe contener números" ControlToValidate="txtTelefono" CssClass="field-error" Display="Dynamic" ValidationExpression="^\d+$"></asp:RegularExpressionValidator>
                        </div>
                    </div>

                    <div class="form-row">
                        <div class="form-field">
                            <label>Apellido/s</label>
                            <asp:TextBox ID="txtApellido" runat="server" placeholder="Apellido"></asp:TextBox>
                            <asp:RequiredFieldValidator ID="RequiredFieldValidator3" runat="server" ErrorMessage="El apellido es obligatorio" ControlToValidate="txtApellido" CssClass="field-error" Display="Dynamic"></asp:RequiredFieldValidator>
                        </div>
                        <div class="form-field">
                            <label>Nombre/s</label>
                            <asp:TextBox ID="txtNombre" runat="server" placeholder="Nombre"></asp:TextBox>
                            <asp:RequiredFieldValidator ID="RequiredFieldValidator4" runat="server" ErrorMessage="El nombre es obligatorio" ControlToValidate="txtNombre" CssClass="field-error" Display="Dynamic"></asp:RequiredFieldValidator>
                        </div>
                    </div>

                    <div class="form-row">
                        <div class="form-field full">
                            <label>Email</label>
                            <asp:TextBox ID="txtEmail" runat="server" placeholder="usuario@email.com" TextMode="Email"></asp:TextBox>
                            <asp:RegularExpressionValidator ID="RegularExpressionValidator1" runat="server" ErrorMessage="Email inválido" ControlToValidate="txtEmail" CssClass="field-error" Display="Dynamic" ValidationExpression="\w+([-+.']\w+)*@\w+([-.]\w+)*\.\w+([-.]\w+)*"></asp:RegularExpressionValidator>
                        </div>
                    </div>

                    <div class="form-row">
                        <div class="form-field">
                            <label>Nombre de usuario</label>
                            <asp:TextBox ID="txtUsuario" runat="server" placeholder="usuario123"></asp:TextBox>
                            <asp:RequiredFieldValidator ID="RequiredFieldValidator6" runat="server" ErrorMessage="El ususario es obligatorio" ControlToValidate="txtUsuario" CssClass="field-error" Display="Dynamic"></asp:RequiredFieldValidator>
                        </div>
                        <div class="form-field">
                            <label>Rol</label>
                            <asp:DropDownList ID="ddlRolForm" runat="server" AutoPostBack="true" OnSelectedIndexChanged="ddlRolForm_SelectedIndexChanged">
                                <asp:ListItem Value="">Seleccionar rol</asp:ListItem>
                                <asp:ListItem Value="5">WebMaster</asp:ListItem>
                                <asp:ListItem Value="1">Administrador</asp:ListItem>
                                <asp:ListItem Value="2">Recepcionista</asp:ListItem>
                                <asp:ListItem Value="3">Entrenador</asp:ListItem>
                                <asp:ListItem Value="4">Cliente</asp:ListItem>
                                <asp:ListItem Value="6">Familiar</asp:ListItem>
                            </asp:DropDownList>
                            <asp:RequiredFieldValidator ID="RequiredFieldValidator7" runat="server" ErrorMessage="Selecciona un tipo de usuario" ControlToValidate="ddlRolForm" CssClass="field-error" Display="Dynamic"></asp:RequiredFieldValidator>
                        </div>
                    </div>

                    <div class="form-row">
                        <div class="form-field">
                            <label>Fecha de Nacimiento</label>
                            <asp:TextBox ID="txtFechaNacimiento" runat="server" TextMode="Date"></asp:TextBox>
                            <asp:RequiredFieldValidator ID="RequiredFieldValidator8" runat="server" ErrorMessage="La fecha de nacimiento es obligatoria" ControlToValidate="txtFechaNacimiento" CssClass="field-error" Display="Dynamic"></asp:RequiredFieldValidator>
                        </div>
                        <div class="form-field">
                            <label>Estado</label>
                            <asp:DropDownList ID="ddlEstadoForm" runat="server">
                                <asp:ListItem Value="1">Activo</asp:ListItem>
                                <asp:ListItem Value="0">Inactivo</asp:ListItem>
                            </asp:DropDownList>
                        </div>
                    </div>

                    <asp:Panel ID="EntField" runat="server" Visible="false" CssClass="rol-info">
                        <label><i class="fa-solid fa-dumbbell"></i> Datos específicos del Entrenador</label>
                        <small><i class="fa-solid fa-info-circle"></i> Al guardar, se creará el registro en la tabla ENTRENADORES con el mismo DNI</small>
                    </asp:Panel>

                    <asp:Panel ID="clienteFields" runat="server" Visible="false" CssClass="rol-info">
                        <label><i class="fa-solid fa-user"></i> Datos específicos del Cliente</label>
                        <small><i class="fa-solid fa-info-circle"></i> Al guardar, se creará el registro en la tabla ALUMNOS con el mismo DNI</small>
                    </asp:Panel>

                    <div id="passwordRow" runat="server" class="form-row">
                        <div class="form-field full">
                            <label>Contraseña (opcional - se genera automáticamente)</label>
                            <asp:TextBox ID="txtContrasena" runat="server" placeholder="Se generará automáticamente una contraseña segura"></asp:TextBox>
                            <small class="field-hint"><i class="fa-solid fa-info-circle"></i> Si se deja vacío, se generará una contraseña segura de 12 caracteres.</small>
                        </div>
                    </div>

                    <asp:Label ID="lblMensajeForm" runat="server" Visible="false"></asp:Label>
                </div>

                <div class="modal-footer">
                    <button id="btnCancelarForm" runat="server" class="btn-action btn-cancelar" onserverclick="btnCancelarForm_Click" type="button">
                        <i class="fa-solid fa-xmark"></i> <asp:Literal ID="litBtnCancelarForm" runat="server" Text="Cancelar" />
                    </button>
                    <button id="btnGuardar" runat="server" class="btn-action btn-guardar" onserverclick="btnGuardar_Click" type="button">
                        <i class="fa-solid fa-floppy-disk"></i> <asp:Literal ID="litBtnGuardar" runat="server" Text="Guardar" />
                    </button>
                </div>
            </div>
        </asp:Panel>
    </div><!-- /usuarios-container -->
</asp:Content>