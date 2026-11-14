<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="Permisos.aspx.cs" Inherits="gymAppV2.Permisos.Permisos" MasterPageFile="~/DashBoard.Master" Title="Permisos" %>

<asp:Content ID="HeadContent" ContentPlaceHolderID="HeadContent" runat="server">
    <link href="<%= ResolveUrl("~/Permisos/Permisos.css?v=4") %>" rel="stylesheet" type="text/css" />
    <link rel="stylesheet" href="https://cdnjs.cloudflare.com/ajax/libs/font-awesome/6.5.0/css/all.min.css" />
</asp:Content>

<asp:Content ID="MainContent" ContentPlaceHolderID="MainContent" runat="server">
    <div class="permisos-container">
        <div class="permisos-header">
            <i class="fa-solid fa-shield-halved"></i>
            <div class="permisos-title-text">
                <h1><asp:Literal ID="litTitulo" runat="server" Text="Permisos" /></h1>
                <p><asp:Literal ID="litSubtitulo" runat="server" /></p>
            </div>
        </div>

        <!-- Se muestra si todavía no se corrió scripts/crear-permisos-composite.sql -->
        <asp:Panel ID="pnlSinTablas" runat="server" Visible="false" CssClass="perm-vacio-grande">
            <i class="fa-solid fa-database"></i>
            <h3><asp:Literal ID="litSinTablasTitulo" runat="server" /></h3>
            <p><asp:Literal ID="litSinTablasMsg" runat="server" /></p>
        </asp:Panel>

        <asp:Panel ID="pnlContenido" runat="server">
            <div class="perm-tabs" role="tablist">
                <asp:LinkButton ID="btnTabPerfiles" runat="server" CssClass="perm-tab" OnClick="btnTabPerfiles_Click" CausesValidation="false" role="tab"></asp:LinkButton>
                <asp:LinkButton ID="btnTabFamilias" runat="server" CssClass="perm-tab" OnClick="btnTabFamilias_Click" CausesValidation="false" role="tab"></asp:LinkButton>
                <asp:LinkButton ID="btnTabUsuarios" runat="server" CssClass="perm-tab" OnClick="btnTabUsuarios_Click" CausesValidation="false" role="tab"></asp:LinkButton>
            </div>

            <!-- ===== Perfiles ===== -->
            <asp:Panel ID="pnlPerfiles" runat="server" CssClass="perm-seccion">
                <div class="perm-toolbar">
                    <p class="perm-ayuda"><asp:Literal ID="litAyudaPerfiles" runat="server" /></p>
                    <asp:LinkButton ID="btnNuevoPerfil" runat="server" CssClass="perm-btn" OnClick="btnNuevoPerfil_Click" CausesValidation="false">
                        <i class="fa-solid fa-plus"></i> <asp:Literal ID="litBtnNuevoPerfil" runat="server" />
                    </asp:LinkButton>
                </div>

                <div class="perm-grilla">
                    <asp:Repeater ID="rptPerfiles" runat="server" OnItemCommand="rptPerfiles_ItemCommand">
                        <ItemTemplate>
                            <div class="perm-tarjeta">
                                <div class="perm-tarjeta-cabecera">
                                    <div>
                                        <h3><%#: Eval("Nombre") %></h3>
                                        <div class="perm-etiquetas">
                                            <asp:PlaceHolder runat="server" Visible='<%# (bool)Eval("EsSistema") %>'>
                                                <span class="perm-etiqueta sistema"><%#: T("permisos_badge_sistema") %></span>
                                            </asp:PlaceHolder>
                                            <%# Eval("EtiquetaUsuarios") %>
                                        </div>
                                    </div>
                                    <div class="perm-acciones">
                                        <asp:LinkButton runat="server" CssClass="perm-link" CommandName="Editar" CommandArgument='<%# Eval("Id") %>'
                                            CausesValidation="false" Visible='<%# (bool)Eval("Editable") %>'><%#: T("permisos_btn_editar") %></asp:LinkButton>
                                        <asp:LinkButton runat="server" CssClass="perm-link peligro" CommandName="Eliminar" CommandArgument='<%# Eval("Id") %>'
                                            CausesValidation="false" Visible='<%# (bool)Eval("Eliminable") %>'
                                            OnClientClick='<%# ConfirmarEliminar() %>'><%#: T("permisos_btn_eliminar") %></asp:LinkButton>
                                    </div>
                                </div>
                                <div class="perm-tarjeta-cuerpo"><%# Eval("Arbol") %></div>
                            </div>
                        </ItemTemplate>
                    </asp:Repeater>
                </div>
                <asp:PlaceHolder ID="phSinPerfiles" runat="server" Visible="false">
                    <p class="perm-vacio"><%: T("permisos_sin_perfiles") %></p>
                </asp:PlaceHolder>
            </asp:Panel>

            <!-- ===== Familias ===== -->
            <asp:Panel ID="pnlFamilias" runat="server" CssClass="perm-seccion" Visible="false">
                <div class="perm-toolbar">
                    <p class="perm-ayuda"><asp:Literal ID="litAyudaFamilias" runat="server" /></p>
                    <asp:LinkButton ID="btnNuevaFamilia" runat="server" CssClass="perm-btn" OnClick="btnNuevaFamilia_Click" CausesValidation="false">
                        <i class="fa-solid fa-plus"></i> <asp:Literal ID="litBtnNuevaFamilia" runat="server" />
                    </asp:LinkButton>
                </div>

                <div class="perm-grilla">
                    <asp:Repeater ID="rptFamilias" runat="server" OnItemCommand="rptFamilias_ItemCommand">
                        <ItemTemplate>
                            <div class="perm-tarjeta">
                                <div class="perm-tarjeta-cabecera">
                                    <div>
                                        <h3><%#: Eval("Nombre") %></h3>
                                        <div class="perm-etiquetas">
                                            <asp:PlaceHolder runat="server" Visible='<%# (bool)Eval("EsSistema") %>'>
                                                <span class="perm-etiqueta sistema"><%#: T("permisos_badge_sistema") %></span>
                                            </asp:PlaceHolder>
                                            <span class="perm-etiqueta"><%#: Eval("TextoUsos") %></span>
                                        </div>
                                    </div>
                                    <div class="perm-acciones">
                                        <asp:LinkButton runat="server" CssClass="perm-link" CommandName="Editar" CommandArgument='<%# Eval("Id") %>'
                                            CausesValidation="false"><%#: T("permisos_btn_editar") %></asp:LinkButton>
                                        <asp:LinkButton runat="server" CssClass="perm-link peligro" CommandName="Eliminar" CommandArgument='<%# Eval("Id") %>'
                                            CausesValidation="false" Visible='<%# (bool)Eval("Eliminable") %>'
                                            OnClientClick='<%# ConfirmarEliminar() %>'><%#: T("permisos_btn_eliminar") %></asp:LinkButton>
                                    </div>
                                </div>
                                <div class="perm-tarjeta-cuerpo"><%# Eval("Arbol") %></div>
                            </div>
                        </ItemTemplate>
                    </asp:Repeater>
                </div>
                <asp:PlaceHolder ID="phSinFamilias" runat="server" Visible="false">
                    <p class="perm-vacio"><%: T("permisos_sin_familias") %></p>
                </asp:PlaceHolder>
            </asp:Panel>

            <!-- ===== Perfil de cada usuario ===== -->
            <asp:Panel ID="pnlUsuarios" runat="server" CssClass="perm-seccion" Visible="false" DefaultButton="btnBuscarUsuario">
                <div class="perm-toolbar">
                    <p class="perm-ayuda"><asp:Literal ID="litAyudaUsuarios" runat="server" /></p>
                    <div class="perm-buscador">
                        <asp:TextBox ID="txtBuscarUsuario" runat="server" MaxLength="100" />
                        <asp:LinkButton ID="btnBuscarUsuario" runat="server" CssClass="perm-btn secundario" OnClick="btnBuscarUsuario_Click" CausesValidation="false">
                            <i class="fa-solid fa-magnifying-glass"></i>
                        </asp:LinkButton>
                    </div>
                </div>

                <div class="perm-tabla-wrap">
                    <table class="perm-tabla">
                        <thead>
                            <tr>
                                <th><%: T("permisos_col_usuario") %></th>
                                <th><%: T("permisos_col_nombre") %></th>
                                <th><%: T("permisos_col_tipo") %></th>
                                <th><%: T("permisos_col_estado") %></th>
                                <th><%: T("permisos_col_perfil") %></th>
                            </tr>
                        </thead>
                        <tbody>
                            <asp:Repeater ID="rptUsuarios" runat="server" OnItemDataBound="rptUsuarios_ItemDataBound" OnItemCommand="rptUsuarios_ItemCommand">
                                <ItemTemplate>
                                    <tr>
                                        <td class="perm-mono"><%#: Eval("Usuario") %></td>
                                        <td><%#: Eval("NombreCompleto") %></td>
                                        <td><%#: Eval("Tipo") %></td>
                                        <td><span class="perm-estado <%# (bool)Eval("Activo") ? "activo" : "inactivo" %>"><%#: Eval("Estado") %></span></td>
                                        <td>
                                            <div class="perm-asignar">
                                                <asp:HiddenField ID="hdnUsuario" runat="server" Value='<%# Eval("Usuario") %>' />
                                                <asp:DropDownList ID="ddlPerfil" runat="server" />
                                                <asp:LinkButton ID="btnAsignar" runat="server" CssClass="perm-link" CommandName="Asignar"
                                                    CausesValidation="false"><%#: T("permisos_btn_asignar") %></asp:LinkButton>
                                            </div>
                                        </td>
                                    </tr>
                                </ItemTemplate>
                            </asp:Repeater>
                            <asp:PlaceHolder ID="phSinUsuarios" runat="server" Visible="false">
                                <tr><td colspan="5" class="perm-vacio"><%: T("permisos_sin_usuarios") %></td></tr>
                            </asp:PlaceHolder>
                        </tbody>
                    </table>
                </div>
            </asp:Panel>
        </asp:Panel>

        <!-- ===== Editor de perfil / familia ===== -->
        <asp:Panel ID="pnlEditor" runat="server" Visible="false" CssClass="perm-modal-fondo">
            <div class="perm-modal" role="dialog" aria-modal="true" aria-labelledby="permEditorTitulo">
                <h2 id="permEditorTitulo" class="perm-modal-titulo"><asp:Literal ID="litEditorTitulo" runat="server" /></h2>

                <div class="perm-campo">
                    <label for="<%= txtNombreEditor.ClientID %>"><%: T("permisos_campo_nombre") %></label>
                    <asp:TextBox ID="txtNombreEditor" runat="server" MaxLength="100" />
                    <asp:PlaceHolder ID="phNombreBloqueado" runat="server" Visible="false">
                        <small><%: T("permisos_nombre_bloqueado") %></small>
                    </asp:PlaceHolder>
                </div>

                <div class="perm-editor-columnas">
                    <!-- Lo que se puede agregar -->
                    <section class="perm-editor-panel">
                        <h3><%: T("permisos_disponibles") %></h3>

                        <h4><%: T("permisos_seccion_permisos") %></h4>
                        <ul class="perm-lista">
                            <asp:Repeater ID="rptDisponiblesPermisos" runat="server" OnItemCommand="rptDisponibles_ItemCommand">
                                <ItemTemplate>
                                    <li class='<%# (bool)Eval("Incluido") ? "incluido" : "" %>'>
                                        <span><i class="fa-solid fa-key"></i> <%#: Eval("Nombre") %></span>
                                        <asp:LinkButton runat="server" CssClass="perm-link" CommandName="AgregarPermiso" CommandArgument='<%# Eval("Id") %>'
                                            CausesValidation="false" Visible='<%# !(bool)Eval("Incluido") %>'><%#: T("permisos_btn_agregar") %></asp:LinkButton>
                                        <asp:PlaceHolder runat="server" Visible='<%# (bool)Eval("Incluido") %>'>
                                            <span class="perm-incluido"><%#: T("permisos_ya_incluido") %></span>
                                        </asp:PlaceHolder>
                                    </li>
                                </ItemTemplate>
                            </asp:Repeater>
                        </ul>

                        <h4><%: T("permisos_seccion_familias") %></h4>
                        <ul class="perm-lista">
                            <asp:Repeater ID="rptDisponiblesFamilias" runat="server" OnItemCommand="rptDisponibles_ItemCommand">
                                <ItemTemplate>
                                    <li class='<%# (bool)Eval("Incluido") ? "incluido" : "" %>'>
                                        <details>
                                            <summary><i class="fa-solid fa-folder"></i> <%#: Eval("Nombre") %></summary>
                                            <%# Eval("Arbol") %>
                                        </details>
                                        <asp:LinkButton runat="server" CssClass="perm-link" CommandName="AgregarFamilia" CommandArgument='<%# Eval("Id") %>'
                                            CausesValidation="false" Visible='<%# !(bool)Eval("Incluido") %>'><%#: T("permisos_btn_agregar") %></asp:LinkButton>
                                        <asp:PlaceHolder runat="server" Visible='<%# (bool)Eval("Incluido") %>'>
                                            <span class="perm-incluido"><%#: T("permisos_ya_incluido") %></span>
                                        </asp:PlaceHolder>
                                    </li>
                                </ItemTemplate>
                            </asp:Repeater>
                        </ul>
                    </section>

                    <!-- Lo que tiene el perfil o la familia que se edita -->
                    <section class="perm-editor-panel contenido">
                        <h3><%: T("permisos_contenido") %></h3>
                        <ul class="perm-lista">
                            <asp:Repeater ID="rptContenido" runat="server" OnItemCommand="rptContenido_ItemCommand">
                                <ItemTemplate>
                                    <li>
                                        <div class="perm-contenido-item">
                                            <span><i class='<%# (bool)Eval("EsFamilia") ? "fa-solid fa-folder" : "fa-solid fa-key" %>'></i> <%#: Eval("Nombre") %></span>
                                            <asp:LinkButton runat="server" CssClass="perm-link peligro"
                                                CommandName='<%# (bool)Eval("EsFamilia") ? "QuitarFamilia" : "QuitarPermiso" %>'
                                                CommandArgument='<%# Eval("Id") %>' CausesValidation="false"><%#: T("permisos_btn_quitar") %></asp:LinkButton>
                                        </div>
                                        <%# Eval("Arbol") %>
                                    </li>
                                </ItemTemplate>
                            </asp:Repeater>
                        </ul>
                        <asp:PlaceHolder ID="phContenidoVacio" runat="server" Visible="false">
                            <p class="perm-vacio"><%: T("permisos_contenido_vacio") %></p>
                        </asp:PlaceHolder>
                    </section>
                </div>

                <div class="perm-modal-acciones">
                    <asp:Button ID="btnCancelarEditor" runat="server" CssClass="perm-btn secundario" OnClick="btnCancelarEditor_Click" CausesValidation="false" />
                    <asp:Button ID="btnGuardarEditor" runat="server" CssClass="perm-btn" OnClick="btnGuardarEditor_Click" CausesValidation="false" />
                </div>
            </div>
        </asp:Panel>
    </div>
</asp:Content>
