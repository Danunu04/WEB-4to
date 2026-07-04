<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="ConfigurarPreguntas.aspx.cs" Inherits="gymAppV2.LogIn.ConfigurarPreguntas" %>

<!DOCTYPE html>

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <meta charset="utf-8" />
    <meta name="viewport" content="width=device-width, initial-scale=1.0" />
    <title>Configurar Preguntas de Seguridad</title>
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

        .cambio-hint {
            color: rgba(250, 236, 231, 0.85);
            font-size: 0.8125rem;
            margin-top: -0.25rem;
            margin-bottom: 0.75rem;
            display: block;
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
                    <h1 class="titulo">Configurar pregunta de seguridad</h1>
                    <p style="color: #FAECE7; font-size: 1rem; margin-top: 0.5rem;">
                        Elegí una pregunta de seguridad y su respuesta. La vas a necesitar si olvidás tu contraseña.
                    </p>

                    <div class="form-group mt-4">
                        <label class="form-label" style="color: #FAECE7; display: block; margin-bottom: 0.375rem;">Usuario</label>
                        <asp:TextBox ID="txtUsuario" runat="server" CssClass="form-control form-control-lg" ReadOnly="true" />
                    </div>

                    <div class="form-group mt-3">
                        <label class="form-label" style="color: #FAECE7; display: block; margin-bottom: 0.375rem;">Pregunta de seguridad</label>
                        <asp:TextBox ID="txtPregunta" runat="server" CssClass="form-control form-control-lg" placeholder="Ej: ¿Cuál es el nombre de tu primera mascota?" MaxLength="500" />
                        <asp:RequiredFieldValidator
                            ID="rfvPregunta"
                            runat="server"
                            ErrorMessage="* Ingrese una pregunta de seguridad"
                            ControlToValidate="txtPregunta"
                            ValidationGroup="ConfigurarPreguntaGroup"
                            CssClass="text-danger"
                            Display="Dynamic"></asp:RequiredFieldValidator>
                    </div>

                    <div class="form-group mt-3">
                        <label class="form-label" style="color: #FAECE7; display: block; margin-bottom: 0.375rem;">Respuesta</label>
                        <asp:TextBox ID="txtRespuesta" runat="server" CssClass="form-control form-control-lg" placeholder="Respuesta a tu pregunta" MaxLength="500" />
                        <asp:RequiredFieldValidator
                            ID="rfvRespuesta"
                            runat="server"
                            ErrorMessage="* Ingrese la respuesta"
                            ControlToValidate="txtRespuesta"
                            ValidationGroup="ConfigurarPreguntaGroup"
                            CssClass="text-danger"
                            Display="Dynamic"></asp:RequiredFieldValidator>
                        <span class="cambio-hint">Guardá bien la respuesta. Se usará para recuperar tu cuenta si perdés el acceso.</span>
                    </div>

                    <asp:Label ID="lblMensaje" runat="server" CssClass="lblMensaje" Visible="false"></asp:Label>

                    <asp:Button ID="btnGuardar" runat="server" CssClass="btnFormLogIn" Text="Guardar y continuar" OnClick="btnGuardar_Click" CausesValidation="true" ValidationGroup="ConfigurarPreguntaGroup" />
                </div>
            </div>
        </div>
    </form>
    <script src="https://cdn.jsdelivr.net/npm/bootstrap@5.3.8/dist/js/bootstrap.bundle.min.js" integrity="sha384-FKyoEForCGlyvwx9Hj09JcYn3nv7wiPVlz7YYwJrWVcXK/BmnVDxM+D2scQbITxI" crossorigin="anonymous"></script>
</body>
</html>
