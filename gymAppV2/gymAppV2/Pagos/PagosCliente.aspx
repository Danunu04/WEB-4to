<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="PagosCliente.aspx.cs" Inherits="gymAppV2.Pagos.PagosCliente" MasterPageFile="~/DashBoard.Master" EnableEventValidation="false" Title="Pagos" %>

<asp:Content ID="HeadContent" ContentPlaceHolderID="HeadContent" runat="server">
    <link href="<%= ResolveUrl("~/Pagos/Pagos.css?v=3") %>" rel="stylesheet" type="text/css" />
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

        <!-- Vista Cliente: solo lectura, historial propio -->
        <asp:Panel ID="pnlCliente" runat="server" Visible="false">
            <p style="margin-bottom:1rem;color:var(--text-muted);font-size:0.875rem;"><asp:Literal ID="litClienteMsg" runat="server" Text="Historial de pagos de tus alumnos." /></p>

            <div class="table-card">
                <div class="table-card-header">
                    <h3><i class="fa-solid fa-table-list" style="margin-right:0.4375rem;color:var(--peach-dark)"></i><asp:Literal ID="litListaClienteTitulo" runat="server" Text="Mis pagos" /></h3>
                </div>
                <asp:GridView ID="gvPagosCliente" runat="server" AutoGenerateColumns="false" CssClass="table" GridLines="None">
                    <Columns>
                        <asp:BoundField DataField="Periodo" HeaderText="Período" DataFormatString="{0:MM/yyyy}" />
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
                            <p>No hay pagos registrados todavía.</p>
                        </div>
                    </EmptyDataTemplate>
                </asp:GridView>
            </div>
        </asp:Panel>

        <!-- Vista Admin/Recepcionista: alta de pago + historial completo -->
        <asp:Panel ID="pnlAdmin" runat="server" Visible="false">
            <div class="main-content-vertical">

                <div class="table-card">
                    <div class="table-card-header">
                        <h3><i class="fa-solid fa-table-list" style="margin-right:0.4375rem;color:var(--peach-dark)"></i><asp:Literal ID="litListaTitulo" runat="server" Text="Historial de pagos" /></h3>
                    </div>
                    <asp:GridView ID="gvPagos" runat="server" AutoGenerateColumns="false" CssClass="table"
                        GridLines="None" AllowPaging="true" PageSize="10" OnPageIndexChanging="gvPagos_PageIndexChanging"
                        OnRowDataBound="gvPagos_RowDataBound" DataKeyNames="CodPago">
                        <Columns>
                            <asp:TemplateField HeaderText="Alumno">
                                <ItemTemplate>
                                    <%# Eval("AlumnoApellido") %>, <%# Eval("AlumnoNombre") %>
                                </ItemTemplate>
                            </asp:TemplateField>
                            <asp:BoundField DataField="Periodo" HeaderText="Período" DataFormatString="{0:MM/yyyy}" />
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
                                <p>No hay pagos cargados todavía.</p>
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
                                <asp:DropDownList ID="ddlAlumno" runat="server"></asp:DropDownList>
                            </div>
                            <div class="form-field">
                                <label><asp:Literal ID="litCampoModalidad" runat="server" Text="Modalidad" /> *</label>
                                <asp:DropDownList ID="ddlModalidad" runat="server" AutoPostBack="true" OnSelectedIndexChanged="ddlModalidad_SelectedIndexChanged"></asp:DropDownList>
                            </div>
                        </div>

                        <div class="form-row">
                            <div class="form-field">
                                <label><asp:Literal ID="litCampoPeriodo" runat="server" Text="Período (mes)" /> *</label>
                                <asp:TextBox ID="txtPeriodo" runat="server" TextMode="Date"></asp:TextBox>
                                <span class="form-hint"><asp:Literal ID="litCampoPeriodoHint" runat="server" Text="Se toma el mes de la fecha elegida." /></span>
                            </div>
                            <div class="form-field">
                                <label><asp:Literal ID="litCampoMetodo" runat="server" Text="Método de pago" /> *</label>
                                <asp:DropDownList ID="ddlMetodoPago" runat="server">
                                    <asp:ListItem Text="Efectivo" Value="Efectivo" />
                                    <asp:ListItem Text="Transferencia" Value="Transferencia" />
                                    <asp:ListItem Text="Tarjeta" Value="Tarjeta" />
                                </asp:DropDownList>
                            </div>
                        </div>

                        <div class="form-row">
                            <div class="form-field full">
                                <label><asp:Literal ID="litCampoMonto" runat="server" Text="Monto" /></label>
                                <asp:Label ID="lblMontoPreview" runat="server" CssClass="monto-preview" Text="—" />
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
    </div>
</asp:Content>
