<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="AccesoDenegado.aspx.cs" Inherits="gymAppV2.AccesoDenegado" %>

<!DOCTYPE html>
<html lang="es">
<head runat="server">
    <meta charset="utf-8" />
    <meta name="viewport" content="width=device-width, initial-scale=1.0" />
    <title>Acceso denegado - GymApp</title>
    <link href="<%= ResolveUrl("~/Content/dashboard.css?v=3") %>" rel="stylesheet" type="text/css" />
    <style>
        .access-denied {
            min-height: 100vh;
            display: flex;
            flex-direction: column;
            align-items: center;
            justify-content: center;
            text-align: center;
            padding: 1.25rem;
            background: var(--bg-light, #f8fafc);
            color: var(--text-dark, #1e293b);
        }
        .access-denied h1 {
            font-size: 2rem;
            margin-bottom: 1rem;
            color: var(--danger, #dc2626);
        }
        .access-denied p {
            max-width: 25rem;
            margin-bottom: 1.5rem;
            font-size: 1rem;
            color: var(--text-muted, #64748b);
        }
        .access-denied .btn {
            display: inline-flex;
            align-items: center;
            gap: 0.5rem;
            padding: 0.625rem 1.25rem;
            border-radius: 0.5rem;
            background: var(--primary, #2563eb);
            color: #fff;
            text-decoration: none;
            font-weight: 600;
            transition: background 0.2s ease;
        }
        .access-denied .btn:hover {
            background: var(--primary-dark, #1d4ed8);
        }
    </style>
</head>
<body>
    <div class="access-denied">
        <h1>🔒 Acceso denegado</h1>
        <p>No tenés permisos para acceder a esta sección. Si crees que es un error, contactá al administrador.</p>
        <a href="<%= ResolveUrl("~/DashBoard/WebForm1.aspx") %>" class="btn">
            <i class="bi bi-arrow-left"></i> Volver al dashboard
        </a>
    </div>
</body>
</html>
