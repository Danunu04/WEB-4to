<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="PreguntasSeguridad.aspx.cs" Inherits="gymAppV2.LogIn.PreguntasSeguridad" %>

<!DOCTYPE html>

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <meta charset="utf-8" />
    <meta name="viewport" content="width=device-width, initial-scale=1.0" />
    <title>Recuperar Acceso</title>
    <link href="https://cdn.jsdelivr.net/npm/bootstrap@5.3.8/dist/css/bootstrap.min.css" rel="stylesheet" integrity="sha384-sRIl4kxILFvY47J16cr9ZwB07vP4J8+LH7qKQnuqkuIAvNWLzeN8tE5YBujZqJLB" crossorigin="anonymous">
    <link href="https://cdn.jsdelivr.net/npm/bootstrap-icons@1.11.3/font/bootstrap-icons.min.css" rel="stylesheet">
    <link href="~/LogIn/StyleSheet1.css" rel="stylesheet" runat="server">
    <link href="<%= ResolveUrl("~/Content/toast.css") %>" rel="stylesheet">
    <style>
        @keyframes slideIn { from { opacity: 0; transform: translateX(100%); } to { opacity: 1; transform: translateX(0); } }
        @keyframes slideOut { from { opacity: 1; transform: translateX(0); } to { opacity: 0; transform: translateX(100%); } }

        .pregunta-texto {
            font-size: 1.125rem;
            color: #FAECE7;
            background: rgba(255, 115, 107, 0.5);
            border: 0.0625rem solid rgba(255, 255, 255, 0.3);
            border-radius: 0.5rem;
            padding: 0.75rem 1rem;
            margin-bottom: 1.25rem;
            text-align: center;
            backdrop-filter: blur(0.5rem);
            -webkit-backdrop-filter: blur(0.5rem);
        }

        .link-volver {
            display: inline-block;
            margin-top: 1rem;
            color: #FAECE7;
            font-size: 0.9375rem;
            text-decoration: none;
            transition: color 0.2s ease;
        }

        .link-volver:hover {
            color: #fff;
            text-decoration: underline;
        }
    </style>
</head>
<body>
    <form id="form1" runat="server">
        <asp:ScriptManager runat="server" />
        <div>
            <asp:Button CssClass="BtnInicio" ID="btnIrInicio" runat="server" Text="Ir a inicio" OnClick="btnIrInicio_Click" />
            <div id="toastContainer" style="position:fixed;top:1.5rem;right:1.5rem;z-index:9999;display:flex;flex-direction:column;gap:0.75rem;max-width:400px;"></div>
            <div class="BloqueRosa">
                <div class="formulario">
                    <h1 class="titulo">Recuperar Acceso</h1>
                    <p style="color: #FAECE7; font-size: 1rem; margin-top: 0.5rem;">Responde tu pregunta de seguridad para desbloquear la cuenta.</p>

                    <%-- Paso 1: ingresar usuario --%>
                    <asp:Panel ID="pnlUsuario" runat="server">
                        <asp:TextBox ID="txtUsuario" runat="server" CssClass="form-control form-control-lg mt-4" placeholder="Usuario" MaxLength="50" />
                        <asp:RequiredFieldValidator
                            ID="rfvUsuario"
                            runat="server"
                            ErrorMessage="* Ingrese su usuario"
                            ControlToValidate="txtUsuario"
                            ValidationGroup="PreguntaGroup"
                            CssClass="text-danger"
                            Display="Dynamic"></asp:RequiredFieldValidator>
                        <asp:Button ID="btnContinuar" runat="server" CssClass="btnFormLogIn" Text="Continuar" OnClick="btnContinuar_Click" CausesValidation="true" ValidationGroup="PreguntaGroup" />
                    </asp:Panel>

                    <%-- Paso 2: responder pregunta --%>
                    <asp:Panel ID="pnlPregunta" runat="server" Visible="false">
                        <asp:Label ID="lblPregunta" runat="server" CssClass="pregunta-texto d-block" Text=""></asp:Label>
                        <asp:TextBox ID="txtRespuesta" runat="server" CssClass="form-control form-control-lg mt-3" placeholder="Respuesta" MaxLength="500" />
                        <asp:RequiredFieldValidator
                            ID="rfvRespuesta"
                            runat="server"
                            ErrorMessage="* Ingrese su respuesta"
                            ControlToValidate="txtRespuesta"
                            ValidationGroup="RespuestaGroup"
                            CssClass="text-danger"
                            Display="Dynamic"></asp:RequiredFieldValidator>
                        <asp:Button ID="btnVerificar" runat="server" CssClass="btnFormLogIn" Text="Verificar" OnClick="btnVerificar_Click" CausesValidation="true" ValidationGroup="RespuestaGroup" />
                        <asp:Button ID="btnVolver" runat="server" CssClass="btnFormLogIn" Text="Volver al login" OnClick="btnVolver_Click" CausesValidation="false" Style="background: rgba(255,255,255,0.3); margin-top: 0.75rem;" />
                    </asp:Panel>

                    <asp:Label ID="lblMensaje" runat="server" CssClass="lblMensaje" Visible="false"></asp:Label>

                    <%-- Enlace si el usuario recuerda su contraseña --%>
                    <asp:Panel ID="pnlVolverLogin" runat="server">
                        <a href="<%= ResolveUrl("~/LogIn/LogIn.aspx") %>" class="link-volver">Volver al inicio de sesión</a>
                    </asp:Panel>
                </div>
            </div>
        </div>
    </form>
    <script src="https://cdn.jsdelivr.net/npm/bootstrap@5.3.8/dist/js/bootstrap.bundle.min.js" integrity="sha384-FKyoEForCGlyvwx9Hj09JcYn3nv7wiPVlz7YYwJrWVcXK/BmnVDxM+D2scQbITxI" crossorigin="anonymous"></script>
</body>
</html>
