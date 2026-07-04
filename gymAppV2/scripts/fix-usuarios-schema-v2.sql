-- ============================================================================
-- Corrección de esquema USUARIOS para soportar encriptación de datos personales
-- GymApp v2
-- ============================================================================
-- Problema: la aplicación encripta nombre/apellido/telefono/email con AES-256
-- (Base64 + IV) y almacena fechaNacimiento como texto encriptado, por lo que
-- requiere columnas anchas. El script aplicar-cambios-seguridad-bd.sql creó
-- esas columnas demasiado pequeñas (VARCHAR(100)/VARCHAR(20)/VARCHAR(255)) y
-- fechaNacimiento como DATE, lo que provoca errores de conversión al crear
-- usuarios nuevos.
--
-- Este script es idempotente y seguro para ejecutar sobre una BD existente.
-- No elimina datos ni tablas; solo ensancha columnas y ajusta el tipo de
-- fechaNacimiento a VARCHAR(100) para que coincida con bd-schema-v2.sql.
-- ============================================================================

USE [GymApp];
GO

SET NOCOUNT ON;
GO

PRINT '=== INICIO: correccion de columnas de USUARIOS ===';
GO

-- ============================================================================
-- 1. Asegurar columnas personales con longitudes acordes a encriptación AES-256
-- ============================================================================
IF COL_LENGTH('USUARIOS', 'nombre') IS NULL
BEGIN
    ALTER TABLE [dbo].[USUARIOS] ADD [nombre] VARCHAR(500) NOT NULL DEFAULT '';
    PRINT 'Columna USUARIOS.nombre agregada';
END
ELSE
BEGIN
    DECLARE @nombreLen INT;
    SELECT @nombreLen = c.max_length FROM sys.columns c WHERE c.object_id = OBJECT_ID('USUARIOS') AND c.name = 'nombre';
    IF @nombreLen < 500
    BEGIN
        ALTER TABLE [dbo].[USUARIOS] ALTER COLUMN [nombre] VARCHAR(500) NOT NULL;
        PRINT 'USUARIOS.nombre ampliada a VARCHAR(500)';
    END
    ELSE
    BEGIN
        PRINT 'USUARIOS.nombre ya tiene longitud >= 500';
    END
END
GO

IF COL_LENGTH('USUARIOS', 'apellido') IS NULL
BEGIN
    ALTER TABLE [dbo].[USUARIOS] ADD [apellido] VARCHAR(500) NOT NULL DEFAULT '';
    PRINT 'Columna USUARIOS.apellido agregada';
END
ELSE
BEGIN
    DECLARE @apellidoLen INT;
    SELECT @apellidoLen = c.max_length FROM sys.columns c WHERE c.object_id = OBJECT_ID('USUARIOS') AND c.name = 'apellido';
    IF @apellidoLen < 500
    BEGIN
        ALTER TABLE [dbo].[USUARIOS] ALTER COLUMN [apellido] VARCHAR(500) NOT NULL;
        PRINT 'USUARIOS.apellido ampliada a VARCHAR(500)';
    END
    ELSE
    BEGIN
        PRINT 'USUARIOS.apellido ya tiene longitud >= 500';
    END
END
GO

IF COL_LENGTH('USUARIOS', 'telefono') IS NULL
BEGIN
    ALTER TABLE [dbo].[USUARIOS] ADD [telefono] VARCHAR(500) NULL;
    PRINT 'Columna USUARIOS.telefono agregada';
END
ELSE
BEGIN
    DECLARE @telefonoLen INT;
    SELECT @telefonoLen = c.max_length FROM sys.columns c WHERE c.object_id = OBJECT_ID('USUARIOS') AND c.name = 'telefono';
    IF @telefonoLen < 500
    BEGIN
        ALTER TABLE [dbo].[USUARIOS] ALTER COLUMN [telefono] VARCHAR(500) NULL;
        PRINT 'USUARIOS.telefono ampliada a VARCHAR(500)';
    END
    ELSE
    BEGIN
        PRINT 'USUARIOS.telefono ya tiene longitud >= 500';
    END
END
GO

IF COL_LENGTH('USUARIOS', 'email') IS NULL
BEGIN
    ALTER TABLE [dbo].[USUARIOS] ADD [email] VARCHAR(500) NULL;
    PRINT 'Columna USUARIOS.email agregada';
END
ELSE
BEGIN
    DECLARE @emailLen INT;
    SELECT @emailLen = c.max_length FROM sys.columns c WHERE c.object_id = OBJECT_ID('USUARIOS') AND c.name = 'email';
    IF @emailLen < 500
    BEGIN
        ALTER TABLE [dbo].[USUARIOS] ALTER COLUMN [email] VARCHAR(500) NULL;
        PRINT 'USUARIOS.email ampliada a VARCHAR(500)';
    END
    ELSE
    BEGIN
        PRINT 'USUARIOS.email ya tiene longitud >= 500';
    END
END
GO

-- ============================================================================
-- 2. Ajustar fechaNacimiento a VARCHAR(100) para soportar el valor encriptado
--    (Base64 + IV) y fechas planas legacy.
-- ============================================================================
IF COL_LENGTH('USUARIOS', 'fechaNacimiento') IS NULL
BEGIN
    ALTER TABLE [dbo].[USUARIOS] ADD [fechaNacimiento] VARCHAR(100) NULL;
    PRINT 'Columna USUARIOS.fechaNacimiento agregada como VARCHAR(100)';
END
ELSE
BEGIN
    DECLARE @fechaType NVARCHAR(128);
    SELECT @fechaType = t.name
    FROM sys.columns c
    JOIN sys.types t ON c.user_type_id = t.user_type_id
    WHERE c.object_id = OBJECT_ID('USUARIOS') AND c.name = 'fechaNacimiento';

    IF @fechaType <> 'varchar'
    BEGIN
        -- Convertir valores DATE/TEXT a cadena yyyy-MM-dd para que la aplicación
        -- pueda desencriptar o parsear según corresponda.
        UPDATE [dbo].[USUARIOS]
        SET [fechaNacimiento] = CONVERT(VARCHAR(100), [fechaNacimiento], 23)
        WHERE [fechaNacimiento] IS NOT NULL;

        ALTER TABLE [dbo].[USUARIOS] ALTER COLUMN [fechaNacimiento] VARCHAR(100) NULL;
        PRINT 'USUARIOS.fechaNacimiento convertida a VARCHAR(100)';
    END
    ELSE
    BEGIN
        DECLARE @fechaLen INT;
        SELECT @fechaLen = c.max_length FROM sys.columns c WHERE c.object_id = OBJECT_ID('USUARIOS') AND c.name = 'fechaNacimiento';
        IF @fechaLen < 100
        BEGIN
            ALTER TABLE [dbo].[USUARIOS] ALTER COLUMN [fechaNacimiento] VARCHAR(100) NULL;
            PRINT 'USUARIOS.fechaNacimiento ampliada a VARCHAR(100)';
        END
        ELSE
        BEGIN
            PRINT 'USUARIOS.fechaNacimiento ya es VARCHAR(100)';
        END
    END
END
GO

-- ============================================================================
-- 3. Asegurar columnas de control
-- ============================================================================
IF COL_LENGTH('USUARIOS', 'tipo') IS NULL
BEGIN
    ALTER TABLE [dbo].[USUARIOS] ADD [tipo] VARCHAR(50) NOT NULL DEFAULT 'Empleado';
    PRINT 'Columna USUARIOS.tipo agregada';
END
ELSE
BEGIN
    -- Asegurar longitud mínima 50 y quitar NULL si quedara NULL
    ALTER TABLE [dbo].[USUARIOS] ALTER COLUMN [tipo] VARCHAR(50) NOT NULL;
    PRINT 'USUARIOS.tipo asegurada como VARCHAR(50) NOT NULL';
END
GO

IF COL_LENGTH('USUARIOS', 'dni') IS NULL
BEGIN
    ALTER TABLE [dbo].[USUARIOS] ADD [dni] INT NOT NULL DEFAULT 0;
    PRINT 'Columna USUARIOS.dni agregada';
END
ELSE
BEGIN
    ALTER TABLE [dbo].[USUARIOS] ALTER COLUMN [dni] INT NOT NULL;
    PRINT 'USUARIOS.dni asegurada como INT NOT NULL';
END
GO

IF COL_LENGTH('USUARIOS', 'activo') IS NULL
BEGIN
    ALTER TABLE [dbo].[USUARIOS] ADD [activo] BIT NOT NULL DEFAULT 1;
    PRINT 'Columna USUARIOS.activo agregada';
END
GO

IF COL_LENGTH('USUARIOS', 'bloqueado') IS NULL
BEGIN
    ALTER TABLE [dbo].[USUARIOS] ADD [bloqueado] BIT NOT NULL DEFAULT 0;
    PRINT 'Columna USUARIOS.bloqueado agregada';
END
GO

IF COL_LENGTH('USUARIOS', 'intentos') IS NULL
BEGIN
    ALTER TABLE [dbo].[USUARIOS] ADD [intentos] INT NOT NULL DEFAULT 0;
    PRINT 'Columna USUARIOS.intentos agregada';
END
GO

IF COL_LENGTH('USUARIOS', 'rol') IS NULL
BEGIN
    ALTER TABLE [dbo].[USUARIOS] ADD [rol] INT NOT NULL DEFAULT 4;
    PRINT 'Columna USUARIOS.rol agregada';
END
GO

IF COL_LENGTH('USUARIOS', 'primerLogin') IS NULL
BEGIN
    ALTER TABLE [dbo].[USUARIOS] ADD [primerLogin] BIT NOT NULL DEFAULT 1;
    PRINT 'Columna USUARIOS.primerLogin agregada';
END
GO

IF COL_LENGTH('USUARIOS', 'dvv') IS NULL
BEGIN
    ALTER TABLE [dbo].[USUARIOS] ADD [dvv] VARCHAR(64) NOT NULL DEFAULT '';
    PRINT 'Columna USUARIOS.dvv agregada';
END
GO

IF COL_LENGTH('USUARIOS', 'dvh') IS NULL
BEGIN
    ALTER TABLE [dbo].[USUARIOS] ADD [dvh] VARCHAR(64) NOT NULL DEFAULT '';
    PRINT 'Columna USUARIOS.dvh agregada';
END
GO

-- ============================================================================
-- 4. Asegurar constraints e índices
-- ============================================================================
IF NOT EXISTS (SELECT 1 FROM sys.objects WHERE name = 'UK_USUARIOS_DNI' AND type = 'UQ' AND parent_object_id = OBJECT_ID('USUARIOS'))
BEGIN
    ALTER TABLE [dbo].[USUARIOS] ADD CONSTRAINT [UK_USUARIOS_DNI] UNIQUE ([dni]);
    PRINT 'Constraint UK_USUARIOS_DNI agregado';
END
ELSE
BEGIN
    PRINT 'Constraint UK_USUARIOS_DNI ya existe';
END
GO

IF NOT EXISTS (SELECT 1 FROM sys.check_constraints WHERE name = 'CK_USUARIOS_Tipo' AND parent_object_id = OBJECT_ID('USUARIOS'))
BEGIN
    ALTER TABLE [dbo].[USUARIOS] ADD CONSTRAINT [CK_USUARIOS_Tipo]
        CHECK ([tipo] IN ('Empleado', 'Entrenador', 'Cliente', 'Familiar'));
    PRINT 'Constraint CK_USUARIOS_Tipo agregado';
END
ELSE
BEGIN
    DECLARE @defTipo NVARCHAR(MAX);
    SELECT @defTipo = [definition] FROM sys.check_constraints WHERE name = 'CK_USUARIOS_Tipo' AND parent_object_id = OBJECT_ID('USUARIOS');
    IF @defTipo NOT LIKE '%Familiar%'
    BEGIN
        ALTER TABLE [dbo].[USUARIOS] DROP CONSTRAINT [CK_USUARIOS_Tipo];
        ALTER TABLE [dbo].[USUARIOS] ADD CONSTRAINT [CK_USUARIOS_Tipo]
            CHECK ([tipo] IN ('Empleado', 'Entrenador', 'Cliente', 'Familiar'));
        PRINT 'Constraint CK_USUARIOS_Tipo actualizado para incluir Familiar';
    END
    ELSE
    BEGIN
        PRINT 'Constraint CK_USUARIOS_Tipo ya incluye Familiar';
    END
END
GO

IF NOT EXISTS (SELECT 1 FROM sys.check_constraints WHERE name = 'CK_USUARIOS_Rol' AND parent_object_id = OBJECT_ID('USUARIOS'))
BEGIN
    ALTER TABLE [dbo].[USUARIOS] ADD CONSTRAINT [CK_USUARIOS_Rol]
        CHECK ([rol] IN (1, 2, 3, 4));
    PRINT 'Constraint CK_USUARIOS_Rol agregado';
END
ELSE
BEGIN
    PRINT 'Constraint CK_USUARIOS_Rol ya existe';
END
GO

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_USUARIOS_Tipo' AND object_id = OBJECT_ID('USUARIOS'))
BEGIN
    CREATE NONCLUSTERED INDEX [IX_USUARIOS_Tipo] ON [dbo].[USUARIOS] ([tipo] ASC);
    PRINT 'Indice IX_USUARIOS_Tipo creado';
END
ELSE
BEGIN
    PRINT 'Indice IX_USUARIOS_Tipo ya existe';
END
GO

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_USUARIOS_DNI' AND object_id = OBJECT_ID('USUARIOS'))
BEGIN
    CREATE NONCLUSTERED INDEX [IX_USUARIOS_DNI] ON [dbo].[USUARIOS] ([dni] ASC);
    PRINT 'Indice IX_USUARIOS_DNI creado';
END
ELSE
BEGIN
    PRINT 'Indice IX_USUARIOS_DNI ya existe';
END
GO

PRINT '=== FIN: correccion de columnas de USUARIOS completada ===';
GO
