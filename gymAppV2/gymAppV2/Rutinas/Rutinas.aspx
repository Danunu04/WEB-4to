<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="Rutinas.aspx.cs" Inherits="gymAppV2.Rutinas.Rutinas" MasterPageFile="~/DashBoard.Master" EnableEventValidation="false" Title="Rutinas" %>

<asp:Content ID="HeadContent" ContentPlaceHolderID="HeadContent" runat="server">
    <link href="<%= ResolveUrl("~/Rutinas/Rutinas.css?v=4") %>" rel="stylesheet" type="text/css" />
    <link rel="stylesheet" href="https://cdnjs.cloudflare.com/ajax/libs/font-awesome/6.5.0/css/all.min.css" />
</asp:Content>

<asp:Content ID="MainContent" ContentPlaceHolderID="MainContent" runat="server">
    <div class="rutinas-container">
        <div class="rutinas-header">
            <div class="rutinas-title-text">
                <div class="rutinas-title">
                    <i class="bi bi-list-check"></i>
                    <asp:Literal ID="litTitulo" runat="server" Text="Rutinas" />
                </div>
                <p><asp:Literal ID="litSubtitulo" runat="server" Text="Gestión de rutinas de entrenamiento" /></p>
            </div>
        </div>

        <!-- Vista Cliente: solo lectura, rutinas de sus alumnos asociados -->
        <asp:Panel ID="pnlCliente" runat="server" Visible="false">
            <p style="margin-bottom:1rem;color:var(--text-muted);font-size:0.875rem;"><asp:Literal ID="litClienteMsg" runat="server" Text="Rutinas asignadas a tus alumnos." /></p>

            <div class="table-card">
                <div class="table-card-header">
                    <h3><i class="fa-solid fa-table-list" style="margin-right:7px;color:var(--mint-dark)"></i><asp:Literal ID="litListaClienteTitulo" runat="server" Text="Mis rutinas" /></h3>
                </div>
                <asp:GridView ID="gvRutinasCliente" runat="server" AutoGenerateColumns="false" CssClass="table tabla-tarjetas" GridLines="None"
                    OnRowCommand="gvRutinasCliente_RowCommand">
                    <Columns>
                        <asp:BoundField DataField="Fecha" HeaderText="Fecha" DataFormatString="{0:dd/MM/yyyy}" />
                        <asp:TemplateField HeaderText="Actividad">
                            <ItemTemplate>
                                <span class="pill-actividad"><%# Eval("ActividadDescripcion") %></span>
                            </ItemTemplate>
                        </asp:TemplateField>
                        <asp:TemplateField HeaderText="Entrenador">
                            <ItemTemplate>
                                <%# Eval("EntrenadorApellido") %>, <%# Eval("EntrenadorNombre") %>
                            </ItemTemplate>
                        </asp:TemplateField>
                        <asp:BoundField DataField="Descripcion" HeaderText="Descripción" />
                        <asp:TemplateField>
                            <ItemTemplate>
                                <asp:LinkButton ID="btnVerDetalle" runat="server" CssClass="btn-detalle"
                                    CommandName="Detalle" CommandArgument='<%# Eval("CodRutina") %>'>
                                    <i class="fa-solid fa-dumbbell"></i> <%# Texto("rutinas_ej_ver_detalle", "Ver ejercicios") %>
                                </asp:LinkButton>
                            </ItemTemplate>
                        </asp:TemplateField>
                    </Columns>
                    <EmptyDataTemplate>
                        <div class="rutinas-empty">
                            <i class="bi bi-list-check"></i>
                            <p>No hay rutinas asignadas todavía.</p>
                        </div>
                    </EmptyDataTemplate>
                </asp:GridView>
            </div>
        </asp:Panel>

        <!-- Vista Admin/Recepcionista/Entrenador: CRUD completo -->
        <asp:Panel ID="pnlAdmin" runat="server" Visible="false">
            <div class="main-content-vertical">

                <div class="table-card">
                    <div class="table-card-header">
                        <h3><i class="fa-solid fa-table-list" style="margin-right:7px;color:var(--mint-dark)"></i><asp:Literal ID="litListaTitulo" runat="server" Text="Lista de rutinas" /></h3>
                    </div>
                    <asp:GridView ID="gvRutinas" runat="server" AutoGenerateColumns="false" CssClass="table tabla-tarjetas"
                        GridLines="None" AllowPaging="true" PageSize="10" OnPageIndexChanging="gvRutinas_PageIndexChanging"
                        OnRowCommand="gvRutinas_RowCommand"
                        DataKeyNames="CodRutina">
                        <Columns>
                            <asp:TemplateField>
                                <ItemTemplate>
                                    <asp:LinkButton ID="btnSelect" runat="server" CommandName="Select"
                                        ToolTip="Seleccionar rutina" Style="display:block;width:100%;height:100%;padding:0.5rem;border:none;background:transparent;cursor:pointer;">
                                    </asp:LinkButton>
                                </ItemTemplate>
                            </asp:TemplateField>
                            <asp:BoundField DataField="Fecha" HeaderText="Fecha" DataFormatString="{0:dd/MM/yyyy}" />
                            <asp:TemplateField HeaderText="Alumno">
                                <ItemTemplate>
                                    <%# Eval("AlumnoApellido") %>, <%# Eval("AlumnoNombre") %>
                                </ItemTemplate>
                            </asp:TemplateField>
                            <asp:TemplateField HeaderText="Entrenador">
                                <ItemTemplate>
                                    <%# Eval("EntrenadorApellido") %>, <%# Eval("EntrenadorNombre") %>
                                </ItemTemplate>
                            </asp:TemplateField>
                            <asp:TemplateField HeaderText="Actividad">
                                <ItemTemplate>
                                    <span class="pill-actividad"><%# Eval("ActividadDescripcion") %></span>
                                </ItemTemplate>
                            </asp:TemplateField>
                            <asp:BoundField DataField="Descripcion" HeaderText="Descripción" />
                        </Columns>
                        <PagerStyle CssClass="pagination" />
                        <EmptyDataTemplate>
                            <div class="rutinas-empty">
                                <i class="bi bi-list-check"></i>
                                <p>No hay rutinas cargadas todavía.</p>
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
                        <button id="btnEjercicios" runat="server" class="btn-action btn-ejercicios" onserverclick="btnEjercicios_Click">
                            <i class="fa-solid fa-dumbbell"></i> <asp:Literal ID="litBtnEjercicios" runat="server" Text="Ejercicios" />
                        </button>
                        <button id="btnEliminar" runat="server" class="btn-action btn-eliminar" onserverclick="btnEliminar_Click">
                            <i class="fa-solid fa-trash"></i> <asp:Literal ID="litBtnEliminar" runat="server" Text="Eliminar" />
                        </button>
                        <button id="btnCancelar" runat="server" class="btn-action btn-cancelar" onserverclick="btnCancelar_Click">
                            <i class="fa-solid fa-xmark"></i> <asp:Literal ID="litBtnCancelar" runat="server" Text="Cancelar" />
                        </button>
                    </div>
                </div>

                <asp:Panel ID="pnlForm" runat="server" Visible="false" CssClass="modal-overlay">
                    <div class="modal-content modal-form">
                    <div class="modal-header">
                        <h3>
                            <i class="fa-solid fa-list-check"></i>
                            <asp:Label ID="lblFormTitle" runat="server" Text="Detalle de la rutina" />
                        </h3>
                        <button id="btnCloseForm" runat="server" class="btn-close" onserverclick="btnCloseForm_Click">
                            <i class="fa-solid fa-xmark"></i>
                        </button>
                    </div>
                    <div class="modal-body detail-body">
                        <div class="form-row">
                            <div class="form-field">
                                <label>Alumno *</label>
                                <asp:DropDownList ID="ddlAlumno" runat="server"></asp:DropDownList>
                            </div>
                            <div class="form-field">
                                <label>Entrenador *</label>
                                <asp:DropDownList ID="ddlEntrenador" runat="server"></asp:DropDownList>
                            </div>
                        </div>

                        <div class="form-row">
                            <div class="form-field">
                                <label>Actividad *</label>
                                <asp:DropDownList ID="ddlActividad" runat="server"></asp:DropDownList>
                            </div>
                            <div class="form-field">
                                <label>Fecha *</label>
                                <asp:TextBox ID="txtFecha" runat="server" TextMode="Date"></asp:TextBox>
                            </div>
                        </div>

                        <div class="form-row">
                            <div class="form-field full">
                                <label>Descripción *</label>
                                <asp:TextBox ID="txtDescripcion" runat="server" TextMode="MultiLine" placeholder="Detalle de la rutina..."></asp:TextBox>
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

            </div>
        </asp:Panel>

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
                <p><strong><asp:Literal ID="litConfirmarMsg" runat="server" Text="¿Está seguro que desea eliminar esta rutina?" /></strong></p>
                <p>Rutina: <asp:Label ID="lblRutinaAEliminar" runat="server" Font-Bold="true"></asp:Label></p>
                <asp:HiddenField ID="hdnCodRutinaAEliminar" runat="server" />
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
    <!-- Ejercicios de la rutina (CUN008): edición para el personal, solo lectura para el cliente -->
    <asp:Panel ID="pnlEjercicios" runat="server" Visible="false" CssClass="modal-overlay">
        <div class="modal-content modal-ejercicios">
            <div class="modal-header">
                <h3><i class="fa-solid fa-dumbbell"></i> <asp:Literal ID="litEjerciciosTitulo" runat="server" Text="Ejercicios de la rutina" /></h3>
                <button id="btnCerrarEjercicios" runat="server" class="btn-close" onserverclick="btnCerrarEjercicios_Click">
                    <i class="fa-solid fa-xmark"></i>
                </button>
            </div>
            <div class="modal-body">
                <p class="ej-rutina"><asp:Literal ID="litEjerciciosRutina" runat="server" /></p>

                <asp:Repeater ID="rptEjercicios" runat="server" OnItemCommand="rptEjercicios_ItemCommand">
                    <HeaderTemplate>
                        <table class="ej-tabla tabla-tarjetas">
                            <thead>
                                <tr>
                                    <th>#</th>
                                    <th><%# Texto("rutinas_ej_col_ejercicio", "Ejercicio") %></th>
                                    <th><%# Texto("rutinas_ej_col_series", "Series × reps") %></th>
                                    <th><%# Texto("rutinas_ej_col_peso", "Peso") %></th>
                                    <th><%# Texto("rutinas_ej_col_descanso", "Descanso") %></th>
                                    <th><%# Texto("rutinas_ej_col_observaciones", "Observaciones") %></th>
                                    <asp:PlaceHolder runat="server" Visible='<%# !EsCliente %>'><th></th></asp:PlaceHolder>
                                </tr>
                            </thead>
                            <tbody>
                    </HeaderTemplate>
                    <ItemTemplate>
                                <tr>
                                    <td><%# Container.ItemIndex + 1 %></td>
                                    <td><strong><%#: Eval("EjercicioNombre") %></strong></td>
                                    <td class="ej-num"><%# Eval("Series") %> × <%# Eval("Repeticiones") %></td>
                                    <td class="ej-num"><%#: FormatoPeso(Container.DataItem) %></td>
                                    <td class="ej-num"><%#: FormatoDescanso(Eval("DescansoSegundos")) %></td>
                                    <td><%#: Eval("Observaciones") ?? "—" %></td>
                                    <asp:PlaceHolder runat="server" Visible='<%# !EsCliente %>'>
                                        <td>
                                            <asp:LinkButton ID="btnQuitarEjercicio" runat="server" CssClass="btn-quitar"
                                                CommandName="Quitar" CommandArgument='<%# Eval("CodRutinaEjercicio") %>'
                                                ToolTip='<%# Texto("rutinas_ej_quitar", "Quitar") %>'>
                                                <i class="fa-solid fa-trash"></i>
                                            </asp:LinkButton>
                                        </td>
                                    </asp:PlaceHolder>
                                </tr>
                    </ItemTemplate>
                    <FooterTemplate>
                            </tbody>
                        </table>
                    </FooterTemplate>
                </asp:Repeater>
                <asp:Panel ID="pnlSinEjercicios" runat="server" Visible="false" CssClass="rutinas-empty">
                    <p><asp:Literal ID="litSinEjercicios" runat="server" Text="Esta rutina todavía no tiene ejercicios." /></p>
                </asp:Panel>

                <asp:Panel ID="pnlAgregarEjercicio" runat="server" CssClass="ej-agregar" DefaultButton="btnAgregarEjercicio">
                    <h4><asp:Literal ID="litAgregarTitulo" runat="server" Text="Agregar ejercicio" /></h4>
                    <div class="ej-form">
                        <div class="form-field ej-campo-ejercicio">
                            <label><asp:Literal ID="litCampoEjercicio" runat="server" Text="Ejercicio" /> *</label>
                            <asp:DropDownList ID="ddlEjercicio" runat="server"></asp:DropDownList>
                        </div>
                        <div class="form-field">
                            <label><asp:Literal ID="litCampoSeries" runat="server" Text="Series" /> *</label>
                            <asp:TextBox ID="txtSeries" runat="server" TextMode="Number" min="1" max="20" placeholder="4"></asp:TextBox>
                        </div>
                        <div class="form-field">
                            <label><asp:Literal ID="litCampoReps" runat="server" Text="Repeticiones" /> *</label>
                            <asp:TextBox ID="txtRepeticiones" runat="server" TextMode="Number" min="1" max="100" placeholder="10"></asp:TextBox>
                        </div>
                        <div class="form-field">
                            <label><asp:Literal ID="litCampoPeso" runat="server" Text="Peso (kg)" /></label>
                            <asp:TextBox ID="txtPesoEjercicio" runat="server" TextMode="Number" step="0.5" min="0.5" max="500"></asp:TextBox>
                        </div>
                        <div class="form-field">
                            <label><asp:Literal ID="litCampoDescanso" runat="server" Text="Descanso (seg)" /></label>
                            <asp:TextBox ID="txtDescanso" runat="server" TextMode="Number" min="0" max="900" placeholder="90"></asp:TextBox>
                        </div>
                        <div class="form-field ej-campo-observaciones">
                            <label><asp:Literal ID="litCampoObservaciones" runat="server" Text="Observaciones" /></label>
                            <asp:TextBox ID="txtObservaciones" runat="server" MaxLength="300"></asp:TextBox>
                        </div>
                    </div>
                    <button id="btnAgregarEjercicio" runat="server" class="btn-action btn-crear" onserverclick="btnAgregarEjercicio_Click">
                        <i class="fa-solid fa-plus"></i> <asp:Literal ID="litBtnAgregarEjercicio" runat="server" Text="Agregar" />
                    </button>
                </asp:Panel>
            </div>
            <div class="modal-footer">
                <button id="btnCerrarEjercicios2" runat="server" class="btn-action btn-cancelar" onserverclick="btnCerrarEjercicios_Click">
                    <asp:Literal ID="litBtnCerrarEjercicios" runat="server" Text="Cerrar" />
                </button>
            </div>
        </div>
    </asp:Panel>
    </div><!-- /container -->
</asp:Content>
