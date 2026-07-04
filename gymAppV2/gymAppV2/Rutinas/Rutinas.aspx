<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="Rutinas.aspx.cs" Inherits="gymAppV2.Rutinas.Rutinas" MasterPageFile="~/DashBoard.Master" Title="Rutinas" %>

<asp:Content ID="HeadContent" ContentPlaceHolderID="HeadContent" runat="server">
    <style>
        .rutinas-container {
            max-width: 100%;
            margin: 0;
            padding: 1.5rem;
        }

        .rutinas-header {
            display: flex;
            align-items: center;
            justify-content: space-between;
            flex-wrap: wrap;
            gap: 0.75rem;
            margin-bottom: 1.25rem;
        }

        .rutinas-title h1 {
            font-family: 'Fraunces', serif;
            font-size: 1.5rem;
            font-weight: 700;
            color: var(--color-text, #2D2D2D);
            margin: 0;
        }

        .rutinas-title p {
            color: var(--color-muted, #6B6B6B);
            margin: 0.25rem 0 0 0;
            font-size: 0.875rem;
        }

        .rutinas-empty {
            background: var(--color-surface, #FFFFFF);
            border: 1px solid var(--color-border, #E8E0D5);
            border-radius: 0.75rem;
            padding: 2rem;
            text-align: center;
            color: var(--color-muted, #6B6B6B);
        }

        .rutinas-empty i {
            font-size: 2rem;
            margin-bottom: 0.75rem;
            display: block;
        }
    </style>
</asp:Content>

<asp:Content ID="MainContent" ContentPlaceHolderID="MainContent" runat="server">
    <div class="rutinas-container">
        <div class="rutinas-header">
            <div class="rutinas-title">
                <h1>Rutinas</h1>
                <p>Gestión de rutinas de entrenamiento</p>
            </div>
        </div>

        <asp:Panel ID="pnlCliente" runat="server" Visible="false" CssClass="rutinas-empty">
            <i class="bi bi-list-check"></i>
            <p>Aquí se mostrarán las rutinas asociadas a tus alumnos.</p>
        </asp:Panel>

        <asp:Panel ID="pnlAdmin" runat="server" Visible="false" CssClass="rutinas-empty">
            <i class="bi bi-tools"></i>
            <p>Módulo de rutinas en desarrollo.</p>
        </asp:Panel>
    </div>
</asp:Content>
