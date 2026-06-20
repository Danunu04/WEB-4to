<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="Cambiar-contra.aspx.cs" Inherits="gymAppV2.CambiarContra.Cambiar_contra" %>

<!DOCTYPE html>

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <meta charset="utf-8" />
    <meta name="viewport" content="width=device-width, initial-scale=1.0" />
    <title>Cambiar Contraseña</title>
    <link href="https://cdn.jsdelivr.net/npm/bootstrap@5.3.8/dist/css/bootstrap.min.css" rel="stylesheet" />
    <link href="https://cdn.jsdelivr.net/npm/bootstrap-icons@1.11.3/font/bootstrap-icons.min.css" rel="stylesheet" />
    <link rel="preconnect" href="https://fonts.googleapis.com" />
    <link rel="preconnect" href="https://fonts.gstatic.com" crossorigin />
    <link href="https://fonts.googleapis.com/css2?family=Cherry+Bomb+One&display=swap" rel="stylesheet" />
    <link href="<%= ResolveUrl("~/LogIn/StyleSheet1.css") %>" rel="stylesheet" />
    <link href="<%= ResolveUrl("~/Content/toast.css") %>" rel="stylesheet" />
    <style>
        @keyframes slideIn { from { opacity: 0; transform: translateX(100%); } to { opacity: 1; transform: translateX(0); } }
        @keyframes slideOut { from { opacity: 1; transform: translateX(0); } to { opacity: 0; transform: translateX(100%); } }

        .cambio-card {
            text-align: center;
            margin-top: 2.5rem;
            width: 100%;
            padding: 0 1.25rem;
        }

        .cambio-card .form-label {
            display: block;
            color: #FAECE7;
            font-size: 0.9375rem;
            margin-bottom: 0.375rem;
            font-weight: 500;
        }

        .cambio-card input[type="text"],
        .cambio-card input[type="password"],
        .cambio-card input.form-control {
            width: 100%;
            text-align: center;
            background: rgba(255, 255, 255, 0.6);
            backdrop-filter: blur(0.5rem);
            -webkit-backdrop-filter: blur(0.5rem);
            border: 0.125rem solid rgba(255, 255, 255, 0.5);
            border-radius: 0.5rem;
            padding: 0.625rem 0.9375rem;
            font-size: 1rem;
            color: #333;
            box-sizing: border-box;
            margin-bottom: 0.75rem;
        }

        .cambio-card input::placeholder {
            color: rgba(255, 115, 107, 0.6);
            opacity: 1;
        }

        .cambio-card input:focus {
            outline: none;
            border-color: rgba(255, 115, 107, 0.8);
            box-shadow: 0 0 0.5rem rgba(255, 115, 107, 0.3);
        }

        .cambio-card input.bg-surface-2 {
            background: rgba(255, 255, 255, 0.35) !important;
        }

        .cambio-hint {
            color: rgba(250, 236, 231, 0.85);
            font-size: 0.8125rem;
            margin-top: -0.25rem;
            margin-bottom: 0.75rem;
            display: block;
        }

        .cambio-botones {
            display: flex;
            flex-direction: column;
            gap: 0.75rem;
            margin-top: 1.5rem;
        }

        .cambio-botones .btnFormLogIn {
            margin-top: 0;
        }

        .btnSecundario {
            width: 100%;
            background: rgba(255, 255, 255, 0.3);
            backdrop-filter: blur(0.5rem);
            -webkit-backdrop-filter: blur(0.5rem);
            border: 0.125rem solid rgba(255, 255, 255, 0.5);
            color: #FAECE7;
            padding: 0.75rem 1.5rem;
            border-radius: 0.5rem;
            font-size: 1rem;
            cursor: pointer;
            transition: background-color 0.3s ease, transform 0.2s ease, backdrop-filter 0.3s ease;
        }

        .btnSecundario:hover {
            background-color: rgba(255, 255, 255, 0.5);
            transform: scale(1.02);
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

        .BloqueRosa.alto-auto {
            height: auto;
            min-height: 35rem;
            padding-bottom: 2rem;
        }

        .lblMensaje-success {
            background-color: rgba(40, 120, 80, 0.75) !important;
        }
    </style>
</head>
<body>
    <form id="form1" runat="server">
        <asp:ScriptManager runat="server" />
        <asp:Button CssClass="BtnInicio" ID="btnIrInicio" runat="server" Text="Ir a inicio" OnClick="btnIrInicio_Click" />
        <div id="toastContainer" style="position:fixed;top:1.5rem;right:1.5rem;z-index:9999;display:flex;flex-direction:column;gap:0.75rem;max-width:400px;"></div>

        <div class="BloqueRosa alto-auto">
            <div class="cambio-card">
                <h1 class="titulo">Cambiar Contraseña</h1>
                <p style="color: #FAECE7; font-size: 1rem; margin-top: 0.5rem; margin-bottom: 1.25rem;">
                    Complete los campos para actualizar su contraseña.
                </p>

                <div class="form-group">
                    <label class="form-label" for="<%= txtUsuario.ClientID %>">Usuario</label>
                    <asp:TextBox ID="txtUsuario" runat="server" CssClass="form-control" MaxLength="50" />
                </div>

                <div class="form-group" id="grupoContrasenaActual" runat="server">
                    <label class="form-label" for="<%= txtContrasenaActual.ClientID %>">Contraseña actual</label>
                    <asp:TextBox ID="txtContrasenaActual" runat="server" TextMode="Password" CssClass="form-control" MaxLength="128" />
                    <asp:RequiredFieldValidator
                        ID="rfvContrasenaActual"
                        runat="server"
                        ControlToValidate="txtContrasenaActual"
                        ErrorMessage="Ingrese su contraseña actual."
                        ValidationGroup="CambioContrasenaGroup"
                        CssClass="text-danger"
                        Display="Dynamic" />
                </div>

                <div class="form-group">
                    <label class="form-label" for="<%= txtNuevaContrasena.ClientID %>">Nueva contraseña</label>
                    <asp:TextBox ID="txtNuevaContrasena" runat="server" TextMode="Password" CssClass="form-control" MaxLength="128" />
                    <asp:RequiredFieldValidator
                        ID="rfvNuevaContrasena"
                        runat="server"
                        ControlToValidate="txtNuevaContrasena"
                        ErrorMessage="Ingrese la nueva contraseña."
                        ValidationGroup="CambioContrasenaGroup"
                        CssClass="text-danger"
                        Display="Dynamic" />
                    <asp:RegularExpressionValidator
                        ID="revNuevaContrasena"
                        runat="server"
                        ControlToValidate="txtNuevaContrasena"
                        ValidationExpression="^(?=.*[A-Z])(?=.*[^a-zA-Z0-9]).{6,128}$"
                        ErrorMessage="Mínimo 6 caracteres, una mayúscula y un carácter especial."
                        ValidationGroup="CambioContrasenaGroup"
                        CssClass="text-danger"
                        Display="Dynamic" />
                    <span class="cambio-hint">Mínimo 6 caracteres, una mayúscula y un carácter especial.</span>
                </div>

                <div class="form-group">
                    <label class="form-label" for="<%= txtConfirmarContrasena.ClientID %>">Confirmar nueva contraseña</label>
                    <asp:TextBox ID="txtConfirmarContrasena" runat="server" TextMode="Password" CssClass="form-control" MaxLength="128" />
                    <asp:RequiredFieldValidator
                        ID="rfvConfirmarContrasena"
                        runat="server"
                        ControlToValidate="txtConfirmarContrasena"
                        ErrorMessage="Confirme la nueva contraseña."
                        ValidationGroup="CambioContrasenaGroup"
                        CssClass="text-danger"
                        Display="Dynamic" />
                    <asp:CompareValidator
                        ID="cvConfirmarContrasena"
                        runat="server"
                        ControlToValidate="txtConfirmarContrasena"
                        ControlToCompare="txtNuevaContrasena"
                        Operator="Equal"
                        Type="String"
                        ErrorMessage="Las contraseñas no coinciden."
                        ValidationGroup="CambioContrasenaGroup"
                        CssClass="text-danger"
                        Display="Dynamic" />
                </div>

                <asp:Label ID="lblMensaje" runat="server" CssClass="lblMensaje" Visible="false"></asp:Label>

                <div class="cambio-botones">
                    <asp:Button ID="btnGuardar" runat="server" CssClass="btnFormLogIn" Text="Guardar" OnClick="btnGuardar_Click" CausesValidation="true" ValidationGroup="CambioContrasenaGroup" />
                    <asp:Button ID="btnCancelar" runat="server" CssClass="btnSecundario" Text="Cancelar" OnClick="btnCancelar_Click" CausesValidation="false" />
                </div>

                <a href="<%= ResolveUrl("~/LogIn/LogIn.aspx") %>" class="link-volver">Volver al inicio de sesión</a>
            </div>
        </div>
    </form>
    <script src="https://cdn.jsdelivr.net/npm/bootstrap@5.3.8/dist/js/bootstrap.bundle.min.js"></script>
</body>
</html>
