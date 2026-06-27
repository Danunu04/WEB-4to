<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="Perfil.aspx.cs" Inherits="gymAppV2.Perfil.Perfil" MasterPageFile="~/DashBoard.Master" Title="Perfil" %>

<asp:Content ID="HeadContent" ContentPlaceHolderID="HeadContent" runat="server">
    <style>
        .perfil-container {
            max-width: 100%;
            margin: 0;
            padding: 1.5rem;
        }

        .perfil-header {
            display: flex;
            align-items: center;
            justify-content: space-between;
            flex-wrap: wrap;
            gap: 0.75rem;
            margin-bottom: 1.25rem;
        }

        .perfil-title h1 {
            font-family: 'Fraunces', serif;
            font-size: 1.5rem;
            font-weight: 700;
            color: var(--color-text, #2D2D2D);
            margin: 0;
        }

        .perfil-title p {
            color: var(--color-muted, #6B6B6B);
            margin: 0.25rem 0 0 0;
            font-size: 0.875rem;
        }

        .perfil-card {
            background: var(--color-surface, #FFFFFF);
            border: 1px solid var(--color-border, #E8E0D5);
            border-radius: 0.75rem;
            padding: 1.5rem;
            max-width: 40rem;
        }

        .perfil-form .form-group {
            margin-bottom: 1rem;
        }

        .perfil-form label {
            display: block;
            font-size: 0.875rem;
            font-weight: 500;
            color: var(--color-text, #2D2D2D);
            margin-bottom: 0.25rem;
        }

        .perfil-form input[readonly] {
            background: var(--color-surface-2, #FDF6EC);
            cursor: not-allowed;
        }

        .perfil-form input,
        .perfil-form select {
            width: 100%;
            padding: 0.5rem 0.75rem;
            border-radius: 0.5rem;
            border: 1px solid var(--color-border, #E8E0D5);
            font-family: 'DM Sans', sans-serif;
            font-size: 0.875rem;
        }

        .perfil-form input:focus,
        .perfil-form select:focus {
            outline: none;
            box-shadow: 0 0 0 2px var(--color-accent-lavender, #C7B5FF);
        }

        .perfil-actions {
            display: flex;
            gap: 0.5rem;
            margin-top: 1.25rem;
            padding-top: 1rem;
            border-top: 1px solid var(--color-border, #E8E0D5);
        }

        .perfil-actions .btn-primary {
            display: inline-flex;
            align-items: center;
            gap: 0.5rem;
            padding: 0.5rem 1rem;
            background: var(--color-accent-lavender, #C7B5FF);
            color: white;
            border: none;
            border-radius: 0.5rem;
            cursor: pointer;
            font-weight: 500;
            font-size: 0.875rem;
        }

        .perfil-actions .btn-secondary {
            display: inline-flex;
            align-items: center;
            gap: 0.5rem;
            padding: 0.5rem 1rem;
            border-radius: 0.5rem;
            border: 1px solid var(--color-border, #E8E0D5);
            background: var(--color-surface, #FFFFFF);
            color: var(--color-muted, #6B6B6B);
            cursor: pointer;
            font-weight: 500;
            font-size: 0.875rem;
        }

        .perfil-message {
            margin-bottom: 1rem;
            padding: 0.75rem 1rem;
            border-radius: 0.5rem;
            font-size: 0.875rem;
        }

        .perfil-message.success {
            background: var(--color-accent-mint-light, #E5F9F0);
            color: #1a5c45;
        }

        .perfil-message.error {
            background: var(--color-accent-pink-light, #FFE5EB);
            color: #7a1e33;
        }
    </style>
</asp:Content>

<asp:Content ID="MainContent" ContentPlaceHolderID="MainContent" runat="server">
    <div class="perfil-container">
        <div class="perfil-header">
            <div class="perfil-title">
                <h1>Mi Perfil</h1>
                <p>Visualizá y editá tus datos personales</p>
            </div>
        </div>

        <asp:Panel ID="pnlMensaje" runat="server" Visible="false" CssClass="perfil-message">
        </asp:Panel>

        <div class="perfil-card">
            <div class="perfil-form">
                <div class="form-group">
                    <label>Usuario</label>
                    <asp:TextBox ID="txtUsuario" runat="server" ReadOnly="true" />
                </div>

                <div class="form-group">
                    <label>DNI</label>
                    <asp:TextBox ID="txtDNI" runat="server" ReadOnly="true" />
                </div>

                <div class="form-group">
                    <label>Nombre</label>
                    <asp:TextBox ID="txtNombre" runat="server" />
                </div>

                <div class="form-group">
                    <label>Apellido</label>
                    <asp:TextBox ID="txtApellido" runat="server" />
                </div>

                <div class="form-group">
                    <label>Teléfono</label>
                    <asp:TextBox ID="txtTelefono" runat="server" />
                </div>

                <div class="form-group">
                    <label>Email</label>
                    <asp:TextBox ID="txtEmail" runat="server" TextMode="Email" />
                </div>

                <div class="perfil-actions">
                    <asp:Button ID="btnGuardar" runat="server" Text="Guardar cambios" CssClass="btn-primary" OnClick="btnGuardar_Click" />
                    <asp:Button ID="btnCancelar" runat="server" Text="Cancelar" CssClass="btn-secondary" OnClick="btnCancelar_Click" />
                </div>
            </div>
        </div>
    </div>
</asp:Content>
