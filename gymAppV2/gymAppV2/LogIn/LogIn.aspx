<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="LogIn.aspx.cs" Inherits="gymAppV2.LogIn.LogIn" %>

<!DOCTYPE html>

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title>LogIn</title>
     <link href="https://cdn.jsdelivr.net/npm/bootstrap@5.3.8/dist/css/bootstrap.min.css" rel="stylesheet" integrity="sha384-sRIl4kxILFvY47J16cr9ZwB07vP4J8+LH7qKQnuqkuIAvNWLzeN8tE5YBujZqJLB" crossorigin="anonymous">
     <link href="~/LogIn/StyleSheet1.css" rel="stylesheet" runat="server">
     <link rel="preconnect" href="https://fonts.googleapis.com">
     <link rel="preconnect" href="https://fonts.gstatic.com" crossorigin="anonymous">
     <link href="https://fonts.googleapis.com/css2?family=Cherry+Bomb+One&display=swap" rel="stylesheet">
</head>
<body>
    <form id="form1" runat="server">
        <div>
            <div class="BloqueRosa">
                <div class="formulario">
                    <h1 class="titulo">Log In</h1>
                    <input id="Text1" type="text"  class="form-control form-control-lg mt-5" placeholder="Usuario" />
                    <input id="Text1" type="password"  class="form-control form-control-lg mt-5" placeholder="Contraseña" />
                    <asp:Button ID="Button1" CssClass="btnFormLogIn" runat="server" Text="Iniciar sesion" />
                </div>
            </div>
        </div>
    </form>
     <script src="https://cdn.jsdelivr.net/npm/bootstrap@5.3.8/dist/js/bootstrap.bundle.min.js" integrity="sha384-FKyoEForCGlyvwx9Hj09JcYn3nv7wiPVlz7YYwJrWVcXK/BmnVDxM+D2scQbITxI"
</body>
</html>
