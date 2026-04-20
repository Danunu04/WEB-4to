<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="Inicio.aspx.cs" Inherits="GymApp.Inicio" %>
<!DOCTYPE html>
<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <meta charset="utf-8">
    <meta name="viewport" content="width=device-width, initial-scale=1">
    <title>GymApp</title>
    <link href="https://cdn.jsdelivr.net/npm/bootstrap@5.3.8/dist/css/bootstrap.min.css" rel="stylesheet" integrity="sha384-sRIl4kxILFvY47J16cr9ZwB07vP4J8+LH7qKQnuqkuIAvNWLzeN8tE5YBujZqJLB" crossorigin="anonymous">
    <link href="Inicio/inicio.css" rel="stylesheet">
    <link rel="preconnect" href="https://fonts.googleapis.com">
    <link rel="preconnect" href="https://fonts.gstatic.com" crossorigin="anonymous">
    <link href="https://fonts.googleapis.com/css2?family=Cherry+Bomb+One&display=swap" rel="stylesheet">
</head>
<body style="background-color:#FAECE7">
    <form id="form1" runat="server">
        <div style="background-color:#FAECE7">

            <asp:Button CssClass="btnFormLogIn" ID="Button1" runat="server" Text="Log In" OnClick="Button1_Click" />

            <div class="ContAmarillo">
                <div class="formulario">
                    <h1 class="titulo">Bienvenid@</h1>
                    <h2 class="titulo">¿Listo para entrenar?</h2>

                    <input type="text" id="txtDni" runat="server"
                           class="form-control form-control-lg mt-5"
                           placeholder="Ingresa tu dni" />

                    <input class="btnForm" type="submit" value="A entrenar!"
                           runat="server" onserverclick="Ingresar" />
                </div>
            </div>

        </div>
    </form>
    <script src="https://cdn.jsdelivr.net/npm/bootstrap@5.3.8/dist/js/bootstrap.bundle.min.js" integrity="sha384-FKyoEForCGlyvwx9Hj09JcYn3nv7wiPVlz7YYwJrWVcXK/BmnVDxM+D2scQbITxI" crossorigin="anonymous"></script>
</body>
</html>

