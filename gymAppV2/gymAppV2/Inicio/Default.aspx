<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="Default.aspx.cs" Inherits="gymAppV2.Default" %>
<!DOCTYPE html>
<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <meta charset="utf-8" />
    <meta name="viewport" content="width=device-width, initial-scale=1.0" />
    <title>GymApp</title>
    <link href="~/Inicio/inicio.css" rel="stylesheet" runat="server">
    <link href="https://cdn.jsdelivr.net/npm/bootstrap-icons@1.11.3/font/bootstrap-icons.min.css" rel="stylesheet" />
    <link href="<%= ResolveUrl("~/Content/toast.css") %>" rel="stylesheet" type="text/css" />
</head>
<body style="background-color:#FAECE7">
    <form id="form1" runat="server">
        <div style="background-color:#FAECE7">
            <div class="BloqueAmarillo">
                <div class="formulario">
                    <h1 class="titulo">Bienvenid@</h1>
                    <h2 class="subtitulo">¿Listo para entrenar?</h2>

                    <asp:TextBox ID="TextBox1" runat="server" CssClass="form-control form-control-lg mt-5" placeholder="Ingresa tu dni"></asp:TextBox>
                    <asp:RequiredFieldValidator ID="rfvDni" runat="server" ControlToValidate="TextBox1" CssClass="validator" ErrorMessage="El DNI es requerido" Display="Dynamic"></asp:RequiredFieldValidator>

                     <asp:Button ID="Button2" CssClass="btnFormInicio" runat="server" Text="A entrenar!" OnClick="Button2_Click" />
                </div>
            </div>

        </div>

        <!-- Toast Notifications Container -->
        <div class="toast-container" id="toastContainer"></div>
    </form>
    <script src="https://cdn.jsdelivr.net/npm/bootstrap@5.3.8/dist/js/bootstrap.bundle.min.js" integrity="sha384-FKyoEForCGlyvwx9Hj09JcYn3nv7wiPVlz7YYwJrWVcXK/BmnVDxM+D2scQbITxI" crossorigin="anonymous"></script>
    <script>
        window.showToast = function(message, type) {
            var container = document.getElementById('toastContainer');
            if (!container) return;

            var icons = {
                success: '<i class="bi bi-check-circle-fill"></i>',
                error: '<i class="bi bi-exclamation-circle-fill"></i>',
                warning: '<i class="bi bi-exclamation-triangle-fill"></i>',
                info: '<i class="bi bi-info-circle-fill"></i>'
            };

            var titles = {
                success: '¡Éxito!',
                error: 'Error',
                warning: 'Advertencia',
                info: 'Información'
            };

            var toast = document.createElement('div');
            toast.className = 'toast toast-' + type;
            toast.innerHTML =
                '<div class="toast-icon">' + icons[type] + '</div>' +
                '<div class="toast-content">' +
                    '<div class="toast-title">' + titles[type] + '</div>' +
                    '<div class="toast-message">' + message + '</div>' +
                '</div>' +
                '<button class="toast-close" onclick="this.parentElement.remove()">' +
                    '<i class="bi bi-x"></i>' +
                '</button>';

            container.appendChild(toast);

            setTimeout(function() { toast.classList.add('show'); }, 10);

            setTimeout(function() {
                toast.classList.add('hiding');
                setTimeout(function() { toast.remove(); }, 300);
            }, 4000);
        };
    </script>
</body>
</html>

