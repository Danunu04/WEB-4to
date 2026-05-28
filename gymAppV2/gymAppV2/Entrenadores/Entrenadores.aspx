<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="Entrenadores.aspx.cs" Inherits="gymAppV2.Entrenadores.Entrenadores" %>

<!DOCTYPE html>

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <meta charset="utf-8" />
    <meta name="viewport" content="width=device-width, initial-scale=1.0" />
    <title>Entrenadores - GymApp</title>
    <link href="https://cdn.jsdelivr.net/npm/bootstrap@5.3.8/dist/css/bootstrap.min.css" rel="stylesheet" />
    <link href="https://cdn.jsdelivr.net/npm/bootstrap-icons@1.11.3/font/bootstrap-icons.min.css" rel="stylesheet" />
    <link href="<%= ResolveUrl("~/Content/toast.css") %>" rel="stylesheet" type="text/css" />
    <style>
        body { font-family: "DM Sans", system-ui, sans-serif; background: #FFF8F0; padding: 2rem; }
        .container { max-width: 1200px; margin: 0 auto; }
        .page-header { display: flex; justify-content: space-between; align-items: center; margin-bottom: 2rem; }
        .page-title h1 { font-family: 'Fraunces', serif; font-size: 1.75rem; font-weight: 700; color: #2D2D2D; margin: 0; }
        .page-title p { color: #6B6B6B; margin: 0.25rem 0 0 0; }
    </style>
</head>
<body>
    <form id="form1" runat="server">
        <div class="container">
            <div class="page-header">
                <div class="page-title">
                    <h1>Gestión de Entrenadores</h1>
                    <p>Administra el equipo de entrenadores</p>
                </div>
            </div>

            <asp:Label ID="lblMensaje" runat="server" />
        </div>

        <!-- Toast Notifications Container -->
        <div class="toast-container" id="toastContainer"></div>
    </form>

    <script src="https://cdn.jsdelivr.net/npm/bootstrap@5.3.8/dist/js/bootstrap.bundle.min.js"></script>
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
