-- ============================================================================
-- Script: Aplicar columna primerLogin a USUARIOS existente
-- GymApp
-- ============================================================================
-- Fecha: 2026-06-18
-- Descripción: Agrega de forma idempotente la columna [primerLogin] a la tabla
--              [dbo].[USUARIOS] para habilitar el flujo de cambio de contraseña
--              forzado en el primer login.
--
-- Requisitos previos:
--   - La base de datos [GymApp] debe existir.
--   - Ejecutar con privilegios suficientes para ALTER TABLE.
--
-- Efecto:
--   - Si la columna no existe, la crea como BIT NULL, rellena NULLs con 1,
--     la convierte a NOT NULL y agrega default constraint 1.
--   - Si ya existe, solo asegura el default constraint y normaliza NULLs.
--   - El usuario 'admin' (si existe) queda con primerLogin = 0 para no forzarle
--     cambio de contraseña.
-- ============================================================================

USE [GymApp];
GO

SET NOCOUNT ON;
GO

PRINT '=== INICIO: aplicar columna primerLogin ===';
GO

-- 1. Agregar columna si no existe
IF COL_LENGTH('USUARIOS', 'primerLogin') IS NULL
BEGIN
    ALTER TABLE [dbo].[USUARIOS] ADD [primerLogin] BIT NULL;
    PRINT 'Columna USUARIOS.primerLogin agregada';
END
ELSE
BEGIN
    PRINT 'Columna USUARIOS.primerLogin ya existe';
END
GO

-- 2. Normalizar valores NULL a 1 (deben cambiar contraseña)
UPDATE [dbo].[USUARIOS] SET [primerLogin] = 1 WHERE [primerLogin] IS NULL;
PRINT 'Valores NULL en USUARIOS.primerLogin normalizados a 1';
GO

-- 3. Convertir a NOT NULL
ALTER TABLE [dbo].[USUARIOS] ALTER COLUMN [primerLogin] BIT NOT NULL;
PRINT 'Columna USUARIOS.primerLogin convertida a NOT NULL';
GO

-- 4. Agregar default constraint 1 si no existe
IF NOT EXISTS (
    SELECT 1 FROM sys.default_constraints d
    JOIN sys.columns c ON d.parent_column_id = c.column_id AND d.parent_object_id = c.object_id
    JOIN sys.tables t ON c.object_id = t.object_id
    WHERE t.name = 'USUARIOS' AND c.name = 'primerLogin'
)
BEGIN
    ALTER TABLE [dbo].[USUARIOS] ADD CONSTRAINT [DF_USUARIOS_primerLogin] DEFAULT (1) FOR [primerLogin];
    PRINT 'Default DF_USUARIOS_primerLogin agregado';
END
ELSE
BEGIN
    PRINT 'Default DF_USUARIOS_primerLogin ya existe';
END
GO

-- 5. El usuario admin inicial no debe ser forzado a cambiar contraseña
IF EXISTS (SELECT 1 FROM [dbo].[USUARIOS] WHERE [usr] = 'admin')
BEGIN
    UPDATE [dbo].[USUARIOS] SET [primerLogin] = 0 WHERE [usr] = 'admin';
    PRINT 'Usuario admin configurado con primerLogin = 0';
END
GO

PRINT '=== FIN: columna primerLogin aplicada ===';
GO

-- Verificación final
SELECT
    [usr],
    [primerLogin]
FROM [dbo].[USUARIOS]
WHERE [usr] = 'admin';
GO
