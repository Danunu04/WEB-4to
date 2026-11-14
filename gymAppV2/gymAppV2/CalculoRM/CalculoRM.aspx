<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="CalculoRM.aspx.cs" Inherits="gymAppV2.CalculoRM.CalculoRM" MasterPageFile="~/DashBoard.Master" Title="Cálculo de RM" %>

<asp:Content ID="HeadContent" ContentPlaceHolderID="HeadContent" runat="server">
    <link href="<%= ResolveUrl("~/CalculoRM/CalculoRM.css?v=1") %>" rel="stylesheet" type="text/css" />
    <link rel="stylesheet" href="https://cdnjs.cloudflare.com/ajax/libs/font-awesome/6.5.0/css/all.min.css" />
</asp:Content>

<asp:Content ID="MainContent" ContentPlaceHolderID="MainContent" runat="server">
    <div class="rm-container">
        <div class="rm-header">
            <div class="rm-title">
                <i class="fa-solid fa-dumbbell"></i>
                <asp:Literal ID="litTitulo" runat="server" Text="Cálculo de RM" />
            </div>
            <p><asp:Literal ID="litSubtitulo" runat="server" Text="Estimá la repetición máxima (1RM) con el peso y las repeticiones de una serie." /></p>
        </div>

        <!-- Cliente o Familiar sin alumnos asociados -->
        <asp:Panel ID="pnlSinAlumnos" runat="server" Visible="false" CssClass="rm-empty">
            <i class="fa-solid fa-user-slash"></i>
            <p><asp:Literal ID="litSinAlumnos" runat="server" Text="No tenés alumnos asociados a tu cuenta." /></p>
        </asp:Panel>

        <asp:Panel ID="pnlContenido" runat="server">
            <div class="rm-grid">

                <!-- Calculadora -->
                <div class="rm-card">
                    <div class="rm-card-header">
                        <h3><i class="fa-solid fa-calculator"></i><asp:Literal ID="litCalcularTitulo" runat="server" Text="Calcular 1RM" /></h3>
                    </div>
                    <div class="rm-card-body">
                        <div class="form-row">
                            <div class="form-field">
                                <label for="<%= ddlAlumno.ClientID %>"><asp:Literal ID="litCampoAlumno" runat="server" Text="Alumno" /></label>
                                <asp:DropDownList ID="ddlAlumno" runat="server" AutoPostBack="true" OnSelectedIndexChanged="ddlAlumno_SelectedIndexChanged"></asp:DropDownList>
                            </div>
                            <div class="form-field">
                                <label for="<%= ddlEjercicio.ClientID %>"><asp:Literal ID="litCampoEjercicio" runat="server" Text="Ejercicio" /></label>
                                <asp:DropDownList ID="ddlEjercicio" runat="server"></asp:DropDownList>
                            </div>
                        </div>

                        <div class="form-row">
                            <div class="form-field">
                                <label for="<%= txtPeso.ClientID %>"><asp:Literal ID="litCampoPeso" runat="server" Text="Peso levantado (kg)" /></label>
                                <asp:TextBox ID="txtPeso" runat="server" TextMode="Number" step="0.5" min="0.5" max="500" placeholder="80"></asp:TextBox>
                            </div>
                            <div class="form-field">
                                <label for="<%= txtRepeticiones.ClientID %>"><asp:Literal ID="litCampoReps" runat="server" Text="Repeticiones" /></label>
                                <asp:TextBox ID="txtRepeticiones" runat="server" TextMode="Number" step="1" min="1" max="12" placeholder="8"></asp:TextBox>
                            </div>
                        </div>

                        <p class="rm-ayuda"><i class="fa-solid fa-circle-info"></i> <asp:Literal ID="litAyudaFormula" runat="server" /></p>

                        <div class="rm-acciones">
                            <button id="btnCalcular" runat="server" class="btn-action btn-calcular" onserverclick="btnCalcular_Click">
                                <i class="fa-solid fa-calculator"></i> <asp:Literal ID="litBtnCalcular" runat="server" Text="Calcular" />
                            </button>
                            <button id="btnGuardar" runat="server" class="btn-action btn-guardar" onserverclick="btnGuardar_Click">
                                <i class="fa-solid fa-floppy-disk"></i> <asp:Literal ID="litBtnGuardar" runat="server" Text="Guardar en el historial" />
                            </button>
                        </div>
                    </div>
                </div>

                <!-- Resultado -->
                <asp:Panel ID="pnlResultado" runat="server" Visible="false" CssClass="rm-card rm-resultado">
                    <div class="rm-card-body">
                        <span class="rm-resultado-label"><asp:Literal ID="litResultadoLabel" runat="server" Text="1RM estimada" /></span>
                        <span class="rm-resultado-valor"><asp:Label ID="lblRM" runat="server" /></span>
                        <span class="rm-resultado-serie"><asp:Label ID="lblSerie" runat="server" /></span>
                        <asp:Label ID="lblAnterior" runat="server" CssClass="rm-resultado-anterior" />
                    </div>

                    <div class="rm-card-header rm-card-header-sub">
                        <h3><i class="fa-solid fa-layer-group"></i><asp:Literal ID="litTablaTitulo" runat="server" Text="Cargas de entrenamiento" /></h3>
                    </div>
                    <asp:Repeater ID="rptCargas" runat="server">
                        <HeaderTemplate>
                            <table class="rm-tabla rm-tabla-cargas tabla-tarjetas">
                                <thead>
                                    <tr>
                                        <th><%# Texto("rm_col_porcentaje", "% de la 1RM") %></th>
                                        <th><%# Texto("rm_col_peso", "Peso (kg)") %></th>
                                        <th><%# Texto("rm_col_reps", "Repeticiones aprox.") %></th>
                                    </tr>
                                </thead>
                                <tbody>
                        </HeaderTemplate>
                        <ItemTemplate>
                                    <tr class="<%# (int)Eval("Porcentaje") == 100 ? "rm-fila-maxima" : "" %>">
                                        <td><%# Eval("Porcentaje") %>%</td>
                                        <td><%# FormatoKg(Eval("Peso")) %></td>
                                        <td><%# Eval("RepeticionesAprox") %></td>
                                    </tr>
                        </ItemTemplate>
                        <FooterTemplate>
                                </tbody>
                            </table>
                        </FooterTemplate>
                    </asp:Repeater>
                </asp:Panel>
            </div>

            <!-- Historial del alumno -->
            <div class="rm-card">
                <div class="rm-card-header">
                    <h3><i class="fa-solid fa-chart-line"></i><asp:Literal ID="litHistorialTitulo" runat="server" Text="Historial de 1RM" /></h3>
                    <asp:Label ID="lblHistorialAlumno" runat="server" CssClass="rm-historial-alumno" />
                </div>
                <asp:GridView ID="gvHistorial" runat="server" AutoGenerateColumns="false" CssClass="rm-tabla tabla-tarjetas"
                    GridLines="None" AllowPaging="true" PageSize="10" OnPageIndexChanging="gvHistorial_PageIndexChanging"
                    EmptyDataRowStyle-CssClass="rm-tabla-vacia">
                    <Columns>
                        <asp:BoundField DataField="FechaRM" HeaderText="Fecha" DataFormatString="{0:dd/MM/yyyy}" />
                        <asp:TemplateField HeaderText="Ejercicio">
                            <ItemTemplate>
                                <span class="pill-ejercicio"><%#: Eval("EjercicioNombre") %></span>
                            </ItemTemplate>
                        </asp:TemplateField>
                        <asp:TemplateField HeaderText="Serie">
                            <ItemTemplate><%# FormatoSerie(Container.DataItem) %></ItemTemplate>
                        </asp:TemplateField>
                        <asp:TemplateField HeaderText="1RM">
                            <ItemTemplate><strong><%# FormatoKg(Eval("PesoRM")) %></strong></ItemTemplate>
                        </asp:TemplateField>
                        <asp:BoundField DataField="UsuarioRegistro" HeaderText="Registró" NullDisplayText="—" />
                    </Columns>
                    <PagerStyle CssClass="pagination" />
                </asp:GridView>
            </div>
        </asp:Panel>
    </div>
</asp:Content>
