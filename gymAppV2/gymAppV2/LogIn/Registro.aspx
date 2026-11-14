<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="Registro.aspx.cs" Inherits="gymAppV2.LogIn.Registro" %>

<!DOCTYPE html>

<html xmlns="http://www.w3.org/1999/xhtml" lang="es">
<head runat="server">
    <meta charset="utf-8" />
    <meta name="viewport" content="width=device-width, initial-scale=1.0" />
    <title>Crear cuenta</title>
    <link href="https://cdn.jsdelivr.net/npm/bootstrap@5.3.8/dist/css/bootstrap.min.css" rel="stylesheet" integrity="sha384-sRIl4kxILFvY47J16cr9ZwB07vP4J8+LH7qKQnuqkuIAvNWLzeN8tE5YBujZqJLB" crossorigin="anonymous">
    <link href="https://cdn.jsdelivr.net/npm/bootstrap-icons@1.11.3/font/bootstrap-icons.min.css" rel="stylesheet">
    <link href="~/LogIn/StyleSheet1.css?v=2" rel="stylesheet" runat="server">
    <link href="~/LogIn/Registro.css?v=1" rel="stylesheet" runat="server">
    <link rel="preconnect" href="https://fonts.googleapis.com">
    <link rel="preconnect" href="https://fonts.gstatic.com" crossorigin="anonymous">
    <link href="https://fonts.googleapis.com/css2?family=Cherry+Bomb+One&display=swap" rel="stylesheet">
</head>
<body>
    <form id="form1" runat="server">
        <asp:Button CssClass="BtnInicio" ID="btnIrLogin" runat="server" Text="Volver al login" OnClick="btnIrLogin_Click" CausesValidation="false" />

        <div class="BloqueRosa registro-bloque">
            <div class="formulario registro-formulario">
                <h1 class="titulo">Crear cuenta</h1>
                <asp:Label ID="lblPaso" runat="server" CssClass="registro-paso" />

                <asp:Label ID="lblMensaje" runat="server" Visible="false" CssClass="registro-mensaje" role="alert" />

                <!-- Paso 1: cuenta -->
                <asp:Panel ID="pnlPaso1" runat="server" CssClass="registro-panel" DefaultButton="btnSiguiente1">
                    <div class="registro-campo">
                        <label for="<%= txtUsuario.ClientID %>">Nombre de usuario</label>
                        <asp:TextBox ID="txtUsuario" runat="server" CssClass="form-control" MaxLength="50" autocomplete="username" />
                    </div>
                    <div class="registro-campo">
                        <label for="<%= txtContrasena.ClientID %>">Contraseña</label>
                        <asp:TextBox ID="txtContrasena" runat="server" CssClass="form-control" TextMode="Password" MaxLength="128" autocomplete="new-password" />
                    </div>
                    <div class="registro-campo">
                        <label for="<%= txtConfirmar.ClientID %>">Repetí la contraseña</label>
                        <asp:TextBox ID="txtConfirmar" runat="server" CssClass="form-control" TextMode="Password" MaxLength="128" autocomplete="new-password" />
                    </div>
                    <p class="registro-ayuda">Al menos 6 caracteres, una letra mayúscula y un carácter especial.</p>
                    <div class="registro-acciones">
                        <asp:Button ID="btnSiguiente1" runat="server" CssClass="btnFormLogIn" Text="Siguiente" OnClick="btnSiguiente1_Click" />
                    </div>
                </asp:Panel>

                <!-- Paso 2: datos del alumno -->
                <asp:Panel ID="pnlPaso2" runat="server" Visible="false" CssClass="registro-panel" DefaultButton="btnSiguiente2">
                    <div class="registro-fila">
                        <div class="registro-campo">
                            <label for="<%= txtNombre.ClientID %>">Nombre</label>
                            <asp:TextBox ID="txtNombre" runat="server" CssClass="form-control" MaxLength="50" autocomplete="given-name" />
                        </div>
                        <div class="registro-campo">
                            <label for="<%= txtApellido.ClientID %>">Apellido</label>
                            <asp:TextBox ID="txtApellido" runat="server" CssClass="form-control" MaxLength="50" autocomplete="family-name" />
                        </div>
                    </div>
                    <div class="registro-fila">
                        <div class="registro-campo">
                            <label for="<%= txtDni.ClientID %>">DNI</label>
                            <asp:TextBox ID="txtDni" runat="server" CssClass="form-control" TextMode="Number" inputmode="numeric" />
                        </div>
                        <div class="registro-campo">
                            <label for="<%= txtFechaNacimiento.ClientID %>">Fecha de nacimiento</label>
                            <asp:TextBox ID="txtFechaNacimiento" runat="server" CssClass="form-control" TextMode="Date" />
                        </div>
                    </div>
                    <div class="registro-fila">
                        <div class="registro-campo">
                            <label for="<%= txtTelefono.ClientID %>">Teléfono</label>
                            <asp:TextBox ID="txtTelefono" runat="server" CssClass="form-control" MaxLength="20" TextMode="Phone" autocomplete="tel" />
                        </div>
                        <div class="registro-campo">
                            <label for="<%= txtEmail.ClientID %>">Email</label>
                            <asp:TextBox ID="txtEmail" runat="server" CssClass="form-control" MaxLength="100" TextMode="Email" autocomplete="email" />
                        </div>
                    </div>
                    <div class="registro-acciones">
                        <asp:Button ID="btnVolver2" runat="server" CssClass="registro-btn-secundario" Text="Volver" OnClick="btnVolver2_Click" />
                        <asp:Button ID="btnSiguiente2" runat="server" CssClass="btnFormLogIn" Text="Siguiente" OnClick="btnSiguiente2_Click" />
                    </div>
                </asp:Panel>

                <!-- Paso 3: primer mes -->
                <asp:Panel ID="pnlPaso3" runat="server" Visible="false" CssClass="registro-panel" DefaultButton="btnPagar">
                    <div class="registro-campo">
                        <label for="<%= ddlModalidad.ClientID %>">Modalidad</label>
                        <asp:DropDownList ID="ddlModalidad" runat="server" CssClass="form-select" AutoPostBack="true" OnSelectedIndexChanged="ddlModalidad_SelectedIndexChanged" />
                    </div>

                    <div class="registro-resumen">
                        <span>Primer mes</span>
                        <strong><asp:Literal ID="litMonto" runat="server" Text="—" /></strong>
                        <small><asp:Literal ID="litCobertura" runat="server" /></small>
                    </div>

                    <div class="registro-campo">
                        <label>Medio de pago</label>
                        <asp:RadioButtonList ID="rblMetodo" runat="server" RepeatLayout="Flow" RepeatDirection="Horizontal"
                            CssClass="registro-medios" AutoPostBack="true" OnSelectedIndexChanged="rblMetodo_SelectedIndexChanged">
                            <asp:ListItem Value="Tarjeta" Selected="True">Tarjeta</asp:ListItem>
                            <asp:ListItem Value="Transferencia">Transferencia</asp:ListItem>
                        </asp:RadioButtonList>
                    </div>

                    <asp:Panel ID="pnlTarjeta" runat="server">
                        <div class="registro-campo">
                            <label for="<%= txtTarjetaNumero.ClientID %>">Número de tarjeta</label>
                            <asp:TextBox ID="txtTarjetaNumero" runat="server" CssClass="form-control" MaxLength="23" autocomplete="off" inputmode="numeric" placeholder="0000 0000 0000 0000" />
                        </div>
                        <div class="registro-campo">
                            <label for="<%= txtTarjetaTitular.ClientID %>">Titular</label>
                            <asp:TextBox ID="txtTarjetaTitular" runat="server" CssClass="form-control" MaxLength="60" autocomplete="off" />
                        </div>
                        <div class="registro-fila">
                            <div class="registro-campo">
                                <label for="<%= txtTarjetaVence.ClientID %>">Vencimiento (MM/AA)</label>
                                <asp:TextBox ID="txtTarjetaVence" runat="server" CssClass="form-control" MaxLength="5" autocomplete="off" placeholder="MM/AA" />
                            </div>
                            <div class="registro-campo">
                                <label for="<%= txtTarjetaCvv.ClientID %>">Código de seguridad</label>
                                <asp:TextBox ID="txtTarjetaCvv" runat="server" CssClass="form-control" MaxLength="4" autocomplete="off" inputmode="numeric" TextMode="Password" />
                            </div>
                        </div>
                    </asp:Panel>

                    <asp:Panel ID="pnlTransferencia" runat="server" Visible="false">
                        <p class="registro-ayuda">Transferí el monto al alias SPORTIO.GYM.PAGOS y confirmá el pago.</p>
                    </asp:Panel>

                    <p class="registro-ayuda registro-demo">Pago de demostración: no se hace ningún cobro real y los datos de la tarjeta no se guardan.</p>

                    <div class="registro-acciones">
                        <asp:Button ID="btnVolver3" runat="server" CssClass="registro-btn-secundario" Text="Volver" OnClick="btnVolver3_Click" />
                        <asp:Button ID="btnPagar" runat="server" CssClass="btnFormLogIn" Text="Pagar y crear cuenta" OnClick="btnPagar_Click" />
                    </div>
                </asp:Panel>

                <!-- Listo -->
                <asp:Panel ID="pnlExito" runat="server" Visible="false" CssClass="registro-panel registro-exito">
                    <i class="bi bi-check-circle"></i>
                    <h2>¡Listo, ya sos parte de Sportio!</h2>
                    <p><asp:Literal ID="litExito" runat="server" /></p>
                    <p class="registro-ayuda">Al iniciar sesión por primera vez vas a configurar tu pregunta de seguridad.</p>
                    <div class="registro-acciones">
                        <asp:Button ID="btnIniciarSesion" runat="server" CssClass="btnFormLogIn" Text="Iniciar sesión" OnClick="btnIrLogin_Click" />
                    </div>
                </asp:Panel>
            </div>
        </div>
    </form>
</body>
</html>
