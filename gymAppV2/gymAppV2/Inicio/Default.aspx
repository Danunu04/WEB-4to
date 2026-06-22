<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="Default.aspx.cs" Inherits="gymAppV2.Default" %>
<!DOCTYPE html>
<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <meta charset="utf-8" />
    <meta name="viewport" content="width=device-width, initial-scale=1.0" />
    <title>Check-in — Sportio</title>
    <link href="~/Inicio/inicio.css" rel="stylesheet" runat="server">
    <link href="https://cdn.jsdelivr.net/npm/bootstrap-icons@1.11.3/font/bootstrap-icons.min.css" rel="stylesheet" />
    <link href="<%= ResolveUrl("~/Content/toast.css") %>" rel="stylesheet" type="text/css" />
    <style>
        .resultado-panel {
            margin-top: 1.25rem;
            padding: 1rem 1.25rem;
            border-radius: 0.625rem;
            text-align: center;
            backdrop-filter: blur(0.5rem);
            -webkit-backdrop-filter: blur(0.5rem);
            border: 0.125rem solid rgba(255,255,255,0.4);
        }
        .resultado-ok {
            background: rgba(34, 197, 94, 0.25);
            border-color: rgba(34, 197, 94, 0.5);
        }
        .resultado-error {
            background: rgba(239, 68, 68, 0.25);
            border-color: rgba(239, 68, 68, 0.5);
        }
        .resultado-titulo {
            font-size: 1.25rem;
            font-weight: 700;
            color: #1a1a1a;
            margin: 0 0 0.25rem 0;
        }
        .resultado-detalle {
            font-size: 0.9375rem;
            color: #333;
            margin: 0;
        }
    </style>
</head>
<body style="background-color:#FAECE7">
    <form id="form1" runat="server">
        <div style="background-color:#FAECE7">
            <div class="BloqueAmarillo">
                <div class="formulario">
                    <h1 class="titulo">Bienvenid@</h1>
                    <h2 class="subtitulo">¿Listo para entrenar?</h2>

                    <asp:TextBox ID="TextBox1" runat="server" CssClass="form-control form-control-lg mt-5" placeholder="Ingresá tu DNI"></asp:TextBox>
                    <asp:RequiredFieldValidator ID="rfvDni" runat="server" ControlToValidate="TextBox1" CssClass="validator" ErrorMessage="El DNI es requerido" Display="Dynamic"></asp:RequiredFieldValidator>

                    <asp:Button ID="Button2" CssClass="btnFormInicio" runat="server" Text="A entrenar!" OnClick="Button2_Click" />

                    <asp:Panel ID="pnlResultado" runat="server" Visible="false">
                        <p class="resultado-titulo"><asp:Label ID="lblResultadoTitulo" runat="server"></asp:Label></p>
                        <p class="resultado-detalle"><asp:Label ID="lblResultadoDetalle" runat="server"></asp:Label></p>
                    </asp:Panel>
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
