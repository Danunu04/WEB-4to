<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="LogIn.aspx.cs" Inherits="gymAppV2.LogIn.LogIn" %>

<!DOCTYPE html>

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <meta charset="utf-8" />
    <meta name="viewport" content="width=device-width, initial-scale=1.0" />
    <title>LogIn</title>
     <link href="https://cdn.jsdelivr.net/npm/bootstrap@5.3.8/dist/css/bootstrap.min.css" rel="stylesheet" integrity="sha384-sRIl4kxILFvY47J16cr9ZwB07vP4J8+LH7qKQnuqkuIAvNWLzeN8tE5YBujZqJLB" crossorigin="anonymous">
     <link href="https://cdn.jsdelivr.net/npm/bootstrap-icons@1.11.3/font/bootstrap-icons.min.css" rel="stylesheet">
     <link href="~/LogIn/StyleSheet1.css" rel="stylesheet" runat="server">
     <link href="<%= ResolveUrl("~/Content/toast.css") %>" rel="stylesheet">
     <style>
         @keyframes slideIn { from { opacity: 0; transform: translateX(100%); } to { opacity: 1; transform: translateX(0); } }
         @keyframes slideOut { from { opacity: 1; transform: translateX(0); } to { opacity: 0; transform: translateX(100%); } }
     </style>
     <link rel="preconnect" href="https://fonts.googleapis.com">
     <link rel="preconnect" href="https://fonts.gstatic.com" crossorigin="anonymous">
     <link href="https://fonts.googleapis.com/css2?family=Cherry+Bomb+One&display=swap" rel="stylesheet">
</head>
<body>
    <form id="form1" runat="server">
        <asp:ScriptManager runat="server" />
        <div>
            <asp:Button CssClass="BtnInicio" ID="Button1" runat="server" Text="Ir a inicio" OnClick="Button1_Click" />
            <!-- Toast Notifications Container -->
            <div id="toastContainer" style="position:fixed;top:1.5rem;right:1.5rem;z-index:9999;display:flex;flex-direction:column;gap:0.75rem;max-width:400px;"></div>
            <div class="BloqueRosa">
                <div class="formulario">
                    <h1 class="titulo">Log In</h1>
                    <asp:TextBox ID="txtUsuario" runat="server" CssClass="form-control form-control-lg mt-5" placeholder="Usuario" MaxLength="50" />
                    <asp:RequiredFieldValidator
                    ID="RequiredFieldValidator1"
                    runat="server"
                    ErrorMessage="* Campo requerido"
                    ControlToValidate="txtUsuario"
                    ValidationGroup="LoginGroup"
                    CssClass="text-danger"
                    Display="Dynamic"></asp:RequiredFieldValidator>
                    <asp:TextBox ID="txtContrasena" runat="server" CssClass="form-control form-control-lg mt-5" placeholder="Password" TextMode="Password" MaxLength="128" />
                    <asp:RequiredFieldValidator
                    ID="RequiredFieldValidator2"
                    runat="server"
                    ErrorMessage="* Campo requerido"
                    ControlToValidate="txtContrasena"
                    ValidationGroup="LoginGroup"
                    CssClass="text-danger"
                    Display="Dynamic"></asp:RequiredFieldValidator>
                    <asp:RegularExpressionValidator
                    ID="revContrasena"
                    runat="server"
                    ErrorMessage="* La contraseña debe tener al menos 6 caracteres"
                    ControlToValidate="txtContrasena"
                    ValidationExpression=".{6,128}"
                    ValidationGroup="LoginGroup"
                    CssClass="text-danger"
                    Display="Dynamic"></asp:RegularExpressionValidator>
                    <asp:Button ID="btnLogIn" runat="server" CssClass="btnFormLogIn" Text="Iniciar Sesión" OnClick="btnLogIn_Click" CausesValidation="true" ValidationGroup="LoginGroup" />
                    <a href="<%= ResolveUrl("~/LogIn/PreguntasSeguridad.aspx") %>" class="link-volver">¿Olvidaste tu contraseña? / Cuenta bloqueada</a>
                </div>
            </div>
        </div>
    </form>
     <script src="https://cdn.jsdelivr.net/npm/bootstrap@5.3.8/dist/js/bootstrap.bundle.min.js" integrity="sha384-FKyoEForCGlyvwx9Hj09JcYn3nv7wiPVlz7YYwJrWVcXK/BmnVDxM+D2scQbITxI" crossorigin="anonymous"></script>
</body>
</html>