USE [GymApp]
GO

-- Agregar columna Idioma a USUARIOS
-- Valores: 'ES', 'EN', 'PT', 'FR', 'JA' (coinciden con el enum IdiomaApp.ToString())
-- NO incluir en cálculo de DVH (campo operativo, no de integridad de datos)
ALTER TABLE [dbo].[USUARIOS]
    ADD [Idioma] NVARCHAR(5) NOT NULL DEFAULT N'ES';
GO
