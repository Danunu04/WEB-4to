<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="PagosCliente.aspx.cs" Inherits="gymAppV2.Pagos.PagosCliente" MasterPageFile="~/DashBoard.Master" EnableEventValidation="false" Title="Pagos" %>

<asp:Content ID="HeadContent" ContentPlaceHolderID="HeadContent" runat="server">
    <link href="<%= ResolveUrl("~/Pagos/Pagos.css?v=5") %>" rel="stylesheet" type="text/css" />
    <link rel="stylesheet" href="https://cdnjs.cloudflare.com/ajax/libs/font-awesome/6.5.0/css/all.min.css" />
</asp:Content>

<asp:Content ID="MainContent" ContentPlaceHolderID="MainContent" runat="server">
    <div class="pagos-container">
        <div class="pagos-header">
            <i class="fa-solid fa-credit-card"></i>
            <div class="pagos-title-text">
                <h1><asp:Literal ID="litTitulo" runat="server" Text="Pagos" /></h1>
                <p><asp:Literal ID="litSubtitulo" runat="server" Text="Gestión de pagos y cuotas" /></p>
            </div>
        </div>

        <!-- ==================== Vista Cliente: estado del abono de sus alumnos + pago online ==================== -->
        <asp:Panel ID="pnlCliente" runat="server" Visible="false">
            <div class="main-content-vertical">
                <p class="pagos-intro"><asp:Literal ID="litClienteMsg" runat="server" Text="Estado del abono de tus alumnos." /></p>

                <div class="abono-grid">
                    <asp:Repeater ID="rptAbonosCliente" runat="server" OnItemCommand="rptAbonosCliente_ItemCommand">
                        <ItemTemplate>
                            <div class="abono-card">
                                <div class="abono-card-header">
                                    <div>
                                        <h3><%# Server.HtmlEncode(Convert.ToString(Eval("NombreCompleto"))) %></h3>
                                        <span class="abono-card-dni">DNI <%# Eval("Dni") %></span>
                                    </div>
                                    <%# BadgeEstado(Container.DataItem) %>
                                </div>
                                <dl class="abono-card-datos">
                                    <div>
                                        <dt><%# T("pagos_col_vence") %></dt>
                                        <dd><%# FormatearFecha(Eval("FechaVencimiento")) %></dd>
                                    </div>
                                    <div>
                                        <dt><%# T("pagos_col_dias") %></dt>
                                        <dd><%# Eval("DiasRestantes") %></dd>
                                    </div>
                                    <asp:PlaceHolder ID="phMonto" runat="server" Visible='<%# MostrarMontoAPagar(Container.DataItem) %>'>
                                        <div>
                                            <dt><%# T("pagos_monto_a_pagar") %></dt>
                                            <dd class="abono-card-monto"><%# FormatearMonto(Eval("MontoARenovar")) %></dd>
                                        </div>
                                    </asp:PlaceHolder>
                                </dl>
                                <asp:Literal ID="litAviso" runat="server" Text='<%# AvisoNoPuedePagar(Container.DataItem) %>' />
                                <asp:LinkButton ID="btnPagar" runat="server" CssClass="btn-action btn-crear abono-card-btn"
                                    CommandName="Pagar" CommandArgument='<%# Eval("Dni") %>'
                                    Visible='<%# PuedePagar(Container.DataItem) %>'>
                                    <i class="fa-solid fa-credit-card"></i> <%# T("pagos_btn_pagar") %>
                                </asp:LinkButton>
                            </div>
                        </ItemTemplate>
                    </asp:Repeater>
                </div>

                <asp:Panel ID="pnlSinAlumnos" runat="server" Visible="false" CssClass="pagos-empty-inline">
                    <i class="fa-solid fa-user-slash"></i>
                    <p><asp:Literal ID="litSinAlumnos" runat="server" Text="No tenés alumnos asociados a tu cuenta." /></p>
                </asp:Panel>

                <div class="table-card">
                    <div class="table-card-header">
                        <h3><i class="fa-solid fa-table-list"></i><asp:Literal ID="litListaClienteTitulo" runat="server" Text="Mis pagos" /></h3>
                    </div>
                    <asp:GridView ID="gvPagosCliente" runat="server" AutoGenerateColumns="false" CssClass="table tabla-tarjetas" GridLines="None">
                        <Columns>
                            <asp:TemplateField HeaderText="Alumno">
                                <ItemTemplate>
                                    <%# Server.HtmlEncode(Convert.ToString(Eval("AlumnoApellido"))) %>, <%# Server.HtmlEncode(Convert.ToString(Eval("AlumnoNombre"))) %>
                                </ItemTemplate>
                            </asp:TemplateField>
                            <asp:BoundField DataField="FechaInicio" HeaderText="Desde" DataFormatString="{0:dd/MM/yyyy}" />
                            <asp:BoundField DataField="FechaVencimiento" HeaderText="Hasta" DataFormatString="{0:dd/MM/yyyy}" />
                            <asp:TemplateField HeaderText="Modalidad">
                                <ItemTemplate>
                                    <span class="pill-modalidad"><%# Eval("ModalidadDescripcion") %></span>
                                </ItemTemplate>
                            </asp:TemplateField>
                            <asp:BoundField DataField="Monto" HeaderText="Monto" DataFormatString="{0:C}" />
                            <asp:BoundField DataField="FechaPago" HeaderText="Fecha de pago" DataFormatString="{0:dd/MM/yyyy}" />
                            <asp:BoundField DataField="MetodoPago" HeaderText="Método" />
                        </Columns>
                        <EmptyDataTemplate>
                            <div class="pagos-empty-inline">
                                <i class="fa-solid fa-receipt"></i>
                                <p><%# T("pagos_sin_pagos") %></p>
                            </div>
                        </EmptyDataTemplate>
                    </asp:GridView>
                </div>

                <!-- Pago online (simulado) -->
                <asp:Panel ID="pnlPagoCliente" runat="server" Visible="false" CssClass="modal-overlay">
                    <div class="modal-content modal-form">
                        <div class="modal-header">
                            <h3>
                                <i class="fa-solid fa-credit-card"></i>
                                <asp:Literal ID="litPagoClienteTitulo" runat="server" Text="Pagar abono" />
                            </h3>
                            <button id="btnCerrarPagoCliente" runat="server" class="btn-close" onserverclick="btnCerrarPagoCliente_Click">
                                <i class="fa-solid fa-xmark"></i>
                            </button>
                        </div>
                        <div class="modal-body detail-body">
                            <div class="form-row">
                                <div class="form-field">
                                    <label><asp:Literal ID="litClienteCampoAlumno" runat="server" Text="Alumno" /></label>
                                    <asp:Label ID="lblClienteAlumno" runat="server" CssClass="campo-fijo" />
                                </div>
                                <div class="form-field">
                                    <label><asp:Literal ID="litClienteCampoModalidad" runat="server" Text="Modalidad" /> *</label>
                                    <asp:DropDownList ID="ddlClienteModalidad" runat="server" AutoPostBack="true" OnSelectedIndexChanged="ddlClienteModalidad_SelectedIndexChanged"></asp:DropDownList>
                                </div>
                            </div>

                            <div class="abono-resumen">
                                <div>
                                    <span class="abono-resumen-label"><asp:Literal ID="litClienteCampoMonto" runat="server" Text="Monto" /></span>
                                    <asp:Label ID="lblClienteMonto" runat="server" CssClass="monto-preview" Text="—" />
                                </div>
                                <asp:Label ID="lblClienteCobertura" runat="server" CssClass="abono-cobertura" />
                            </div>

                            <div class="form-row">
                                <div class="form-field full">
                                    <label><asp:Literal ID="litClienteCampoMetodo" runat="server" Text="Medio de pago" /> *</label>
                                    <asp:RadioButtonList ID="rblClienteMetodo" runat="server" RepeatLayout="Flow" RepeatDirection="Horizontal"
                                        CssClass="medios-pago" AutoPostBack="true" OnSelectedIndexChanged="rblClienteMetodo_SelectedIndexChanged">
                                        <asp:ListItem Value="Tarjeta" Selected="True">Tarjeta</asp:ListItem>
                                        <asp:ListItem Value="Transferencia">Transferencia</asp:ListItem>
                                    </asp:RadioButtonList>
                                </div>
                            </div>

                            <asp:Panel ID="pnlTarjeta" runat="server">
                                <div class="form-row">
                                    <div class="form-field full">
                                        <label><asp:Literal ID="litTarjetaNumero" runat="server" Text="Número de tarjeta" /> *</label>
                                        <asp:TextBox ID="txtTarjetaNumero" runat="server" MaxLength="23" autocomplete="off" inputmode="numeric" placeholder="0000 0000 0000 0000"></asp:TextBox>
                                    </div>
                                </div>
                                <div class="form-row">
                                    <div class="form-field full">
                                        <label><asp:Literal ID="litTarjetaTitular" runat="server" Text="Titular" /> *</label>
                                        <asp:TextBox ID="txtTarjetaTitular" runat="server" MaxLength="60" autocomplete="off"></asp:TextBox>
                                    </div>
                                </div>
                                <div class="form-row">
                                    <div class="form-field">
                                        <label><asp:Literal ID="litTarjetaVence" runat="server" Text="Vencimiento (MM/AA)" /> *</label>
                                        <asp:TextBox ID="txtTarjetaVence" runat="server" MaxLength="5" autocomplete="off" placeholder="MM/AA"></asp:TextBox>
                                    </div>
                                    <div class="form-field">
                                        <label><asp:Literal ID="litTarjetaCvv" runat="server" Text="Código de seguridad" /> *</label>
                                        <asp:TextBox ID="txtTarjetaCvv" runat="server" MaxLength="4" autocomplete="off" inputmode="numeric" TextMode="Password"></asp:TextBox>
                                    </div>
                                </div>
                            </asp:Panel>

                            <asp:Panel ID="pnlTransferencia" runat="server" Visible="false" CssClass="abono-transferencia">
                                <i class="fa-solid fa-building-columns"></i>
                                <p><asp:Literal ID="litTransferenciaDatos" runat="server" Text="Transferí el monto a la cuenta del gimnasio y confirmá el pago." /></p>
                            </asp:Panel>

                            <p class="form-hint abono-aviso-demo">
                                <i class="fa-solid fa-circle-info"></i>
                                <asp:Literal ID="litAvisoDemo" runat="server" Text="Pago de demostración: no se hace ningún cobro real y los datos de la tarjeta no se guardan." />
                            </p>
                        </div>
                        <div class="modal-footer">
                            <button id="btnCancelarPagoCliente" runat="server" class="btn-action btn-cancelar" onserverclick="btnCerrarPagoCliente_Click">
                                <i class="fa-solid fa-xmark"></i> <%= T("btn_cancelar") %>
                            </button>
                            <button id="btnConfirmarPagoCliente" runat="server" class="btn-action btn-guardar" onserverclick="btnConfirmarPagoCliente_Click">
                                <i class="fa-solid fa-lock"></i> <asp:Literal ID="litBtnConfirmarPago" runat="server" Text="Confirmar pago" />
                            </button>
                        </div>
                    </div>
                </asp:Panel>
            </div>
        </asp:Panel>

        <!-- ==================== Vista Admin/Recepcionista: estado de abonos + cobro + historial ==================== -->
        <asp:Panel ID="pnlAdmin" runat="server" Visible="false">
            <div class="main-content-vertical">

                <div class="table-card">
                    <div class="table-card-header">
                        <h3><i class="fa-solid fa-id-card"></i><asp:Literal ID="litEstadosTitulo" runat="server" Text="Estado de abonos" /></h3>
                    </div>
                    <asp:Panel ID="pnlBuscador" runat="server" CssClass="pagos-buscador" DefaultButton="btnBuscar">
                        <i class="fa-solid fa-magnifying-glass"></i>
                        <asp:TextBox ID="txtBuscar" runat="server" MaxLength="60"></asp:TextBox>
                        <asp:LinkButton ID="btnBuscar" runat="server" CssClass="btn-action" OnClick="btnBuscar_Click" Text="Buscar" />
                        <asp:LinkButton ID="btnLimpiarBusqueda" runat="server" CssClass="btn-action btn-cancelar" OnClick="btnLimpiarBusqueda_Click">
                            <i class="fa-solid fa-xmark"></i>
                        </asp:LinkButton>
                    </asp:Panel>
                    <asp:GridView ID="gvEstados" runat="server" AutoGenerateColumns="false" CssClass="table tabla-tarjetas"
                        GridLines="None" AllowPaging="true" PageSize="10" OnPageIndexChanging="gvEstados_PageIndexChanging"
                        OnRowCommand="gvEstados_RowCommand">
                        <Columns>
                            <asp:TemplateField HeaderText="Alumno">
                                <ItemTemplate><%# Server.HtmlEncode(Convert.ToString(Eval("NombreCompleto"))) %></ItemTemplate>
                            </asp:TemplateField>
                            <asp:BoundField DataField="Dni" HeaderText="DNI" />
                            <asp:TemplateField HeaderText="Vence">
                                <ItemTemplate><%# FormatearFecha(Eval("FechaVencimiento")) %></ItemTemplate>
                            </asp:TemplateField>
                            <asp:BoundField DataField="DiasRestantes" HeaderText="Días restantes" />
                            <asp:TemplateField HeaderText="Estado">
                                <ItemTemplate><%# BadgeEstado(Container.DataItem) %></ItemTemplate>
                            </asp:TemplateField>
                            <asp:TemplateField>
                                <ItemTemplate>
                                    <asp:LinkButton ID="btnCobrar" runat="server" CssClass="btn-action btn-cobrar"
                                        CommandName="Cobrar" CommandArgument='<%# Eval("Dni") %>'
                                        Visible='<%# PuedePagar(Container.DataItem) %>'>
                                        <i class="fa-solid fa-cash-register"></i> <%# T("pagos_btn_cobrar") %>
                                    </asp:LinkButton>
                                </ItemTemplate>
                            </asp:TemplateField>
                        </Columns>
                        <PagerStyle CssClass="pagination" />
                        <EmptyDataTemplate>
                            <div class="pagos-empty-inline">
                                <i class="fa-solid fa-magnifying-glass"></i>
                                <p><%# T("pagos_sin_resultados") %></p>
                            </div>
                        </EmptyDataTemplate>
                    </asp:GridView>
                </div>

                <div class="table-card">
                    <div class="table-card-header">
                        <h3><i class="fa-solid fa-table-list"></i><asp:Literal ID="litListaTitulo" runat="server" Text="Historial de pagos" /></h3>
                    </div>
                    <asp:GridView ID="gvPagos" runat="server" AutoGenerateColumns="false" CssClass="table tabla-tarjetas"
                        GridLines="None" AllowPaging="true" PageSize="10" OnPageIndexChanging="gvPagos_PageIndexChanging" DataKeyNames="CodPago">
                        <Columns>
                            <asp:TemplateField HeaderText="Alumno">
                                <ItemTemplate>
                                    <%# Server.HtmlEncode(Convert.ToString(Eval("AlumnoApellido"))) %>, <%# Server.HtmlEncode(Convert.ToString(Eval("AlumnoNombre"))) %>
                                </ItemTemplate>
                            </asp:TemplateField>
                            <asp:BoundField DataField="FechaInicio" HeaderText="Desde" DataFormatString="{0:dd/MM/yyyy}" />
                            <asp:BoundField DataField="FechaVencimiento" HeaderText="Hasta" DataFormatString="{0:dd/MM/yyyy}" />
                            <asp:TemplateField HeaderText="Modalidad">
                                <ItemTemplate>
                                    <span class="pill-modalidad"><%# Eval("ModalidadDescripcion") %></span>
                                </ItemTemplate>
                            </asp:TemplateField>
                            <asp:BoundField DataField="Monto" HeaderText="Monto" DataFormatString="{0:C}" />
                            <asp:BoundField DataField="FechaPago" HeaderText="Fecha de pago" DataFormatString="{0:dd/MM/yyyy}" />
                            <asp:BoundField DataField="MetodoPago" HeaderText="Método" />
                        </Columns>
                        <PagerStyle CssClass="pagination" />
                        <EmptyDataTemplate>
                            <div class="pagos-empty-inline">
                                <i class="fa-solid fa-receipt"></i>
                                <p><%# T("pagos_sin_pagos") %></p>
                            </div>
                        </EmptyDataTemplate>
                    </asp:GridView>
                    <div class="table-footer">
                        <span class="table-footer-text" id="footerText" runat="server"></span>
                    </div>
                    <div class="table-actions">
                        <button id="btnRegistrar" runat="server" class="btn-action btn-crear" onserverclick="btnRegistrar_Click">
                            <i class="fa-solid fa-plus"></i> <asp:Literal ID="litBtnRegistrar" runat="server" Text="Registrar pago" />
                        </button>
                    </div>
                </div>

                <asp:Panel ID="pnlForm" runat="server" Visible="false" CssClass="modal-overlay">
                    <div class="modal-content modal-form">
                    <div class="modal-header">
                        <h3>
                            <i class="fa-solid fa-credit-card"></i>
                            <asp:Literal ID="litFormTitulo" runat="server" Text="Registrar pago" />
                        </h3>
                        <button id="btnCloseForm" runat="server" class="btn-close" onserverclick="btnCloseForm_Click">
                            <i class="fa-solid fa-xmark"></i>
                        </button>
                    </div>
                    <div class="modal-body detail-body">
                        <div class="form-row">
                            <div class="form-field">
                                <label><asp:Literal ID="litCampoAlumno" runat="server" Text="Alumno" /> *</label>
                                <asp:DropDownList ID="ddlAlumno" runat="server" AutoPostBack="true" OnSelectedIndexChanged="ddlAlumno_SelectedIndexChanged"></asp:DropDownList>
                            </div>
                            <div class="form-field">
                                <label><asp:Literal ID="litCampoModalidad" runat="server" Text="Modalidad" /> *</label>
                                <asp:DropDownList ID="ddlModalidad" runat="server" AutoPostBack="true" OnSelectedIndexChanged="ddlModalidad_SelectedIndexChanged"></asp:DropDownList>
                            </div>
                        </div>

                        <div class="form-row">
                            <div class="form-field">
                                <label><asp:Literal ID="litCampoMetodo" runat="server" Text="Método de pago" /> *</label>
                                <asp:DropDownList ID="ddlMetodoPago" runat="server">
                                    <asp:ListItem Text="Efectivo" Value="Efectivo" />
                                    <asp:ListItem Text="Transferencia" Value="Transferencia" />
                                    <asp:ListItem Text="Tarjeta" Value="Tarjeta" />
                                </asp:DropDownList>
                            </div>
                            <div class="form-field">
                                <label><asp:Literal ID="litCampoMonto" runat="server" Text="Monto" /></label>
                                <asp:Label ID="lblMontoPreview" runat="server" CssClass="monto-preview" Text="—" />
                            </div>
                        </div>

                        <asp:Label ID="lblCobertura" runat="server" CssClass="abono-cobertura" />
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
    </div>
</asp:Content>
