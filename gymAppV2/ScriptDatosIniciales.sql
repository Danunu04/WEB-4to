-- ============================================================================
-- Script de Datos Iniciales - GymApp
-- ============================================================================
-- Fecha: 2026-06-17
-- Descripción: Inserta datos mínimos necesarios para que la aplicación sea
--              usable después de crear la base de datos.
--
-- Requisitos previos:
--   - Ejecutar primero bd-schema-v2.sql (o la BD ya debe existir).
--   - Este script debe correr en el contexto de la base de datos [GymApp].
--
-- Seguridad:
--   - La contraseña del administrador inicial se almacena hasheada con SHA256,
--     igual que en la capa de negocio (CriptoManager._686DPGetSHA256).
--   - El hash de "Admin123!" es:
--       3eb3fe66b31e3b4d10fa70b5cad49c7112294af6ae4e476a1c405155d45aa121
--   - Para regenerar el hash en PowerShell:
--       Add-Type -AssemblyName System.Security
--       $bytes = [System.Text.Encoding]::UTF8.GetBytes('NuevaClave')
--       $hash = [System.Security.Cryptography.SHA256]::Create().ComputeHash($bytes)
--       [BitConverter]::ToString($hash).Replace('-','').ToLower()
--   - Cambiar la contraseña inmediatamente después del primer login.
-- ============================================================================

USE [GymApp];
GO

SET NOCOUNT ON;
GO

PRINT '=== INICIO: carga de datos iniciales ===';
GO

-- ============================================================================
-- 1. Usuario administrador por defecto
-- ============================================================================
IF NOT EXISTS (SELECT 1 FROM [dbo].[USUARIOS] WHERE [usr] = 'admin')
BEGIN
    INSERT INTO [dbo].[USUARIOS]
        ([usr], [contra], [activo], [bloqueado], [intentos],
         [tipo], [dni], [nombre], [apellido], [telefono],
         [email], [fechaNacimiento], [rol], [primerLogin], [dvv], [dvh])
    VALUES
        ('admin',
         '3eb3fe66b31e3b4d10fa70b5cad49c7112294af6ae4e476a1c405155d45aa121', -- Admin123! (SHA256)
         1,        -- activo
         0,        -- bloqueado
         0,        -- intentos
         'Empleado',
         11111111, -- DNI de ejemplo
         'Administrador',
         'Sistema',
         '0000-0000',
         'admin@gymapp.local',
         '1990-01-01',
         1,        -- rol: Administrador
         0,        -- primerLogin: ya fue creado manualmente, no forzar cambio
         '',       -- dvv (valor inicial, recalcular con utilidad de DVV)
         ''        -- dvh (valor inicial, recalcular con utilidad de DVH)
        );

    PRINT 'Usuario admin creado';
END
ELSE
BEGIN
    PRINT 'Usuario admin ya existe';
END
GO

-- ============================================================================
-- 2. Registrar la contraseña inicial en el historial (obligatorio para el flujo)
-- ============================================================================
IF NOT EXISTS (SELECT 1 FROM [dbo].[USUARIO_Contras] WHERE [usr] = 'admin')
BEGIN
    INSERT INTO [dbo].[USUARIO_Contras]
        ([usr], [contra], [dvv], [dvh])
    VALUES
        ('admin',
         '3eb3fe66b31e3b4d10fa70b5cad49c7112294af6ae4e476a1c405155d45aa121',
         '',
         '');

    PRINT 'Contraseña inicial registrada en USUARIO_Contras';
END
ELSE
BEGIN
    PRINT 'Historial de contraseñas para admin ya existe';
END
GO

PRINT '=== FIN: carga de datos iniciales completada ===';
GO
