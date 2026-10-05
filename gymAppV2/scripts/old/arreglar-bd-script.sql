-- ============================================================================
-- Script de corrección de esquema para BD creada con
-- C:\Users\Danunu\Documents\script.sql
-- ============================================================================
-- Objetivo: alinear el esquema existente con bd-schema-v2.sql
--           (USUARIOS.tipo/rol/dvv/dvh y tabla USUARIO_Intentos obsoleta).
--
-- Ejecutar contra la base de datos [GymApp].
-- ============================================================================

USE [GymApp];
GO

SET NOCOUNT ON;
GO

PRINT '=== INICIO: correccion de esquema ===';
GO

-- ============================================================================
-- 1. USUARIOS: columna activo con DEFAULT 1
-- ============================================================================
IF NOT EXISTS (
    SELECT 1
    FROM sys.columns c
    JOIN sys.tables t ON c.object_id = t.object_id
    JOIN sys.default_constraints d ON c.default_object_id = d.object_id
    WHERE t.name = 'USUARIOS' AND c.name = 'activo'
)
BEGIN
    ALTER TABLE [dbo].[USUARIOS] ADD CONSTRAINT [DF_USUARIOS_activo] DEFAULT (1) FOR [activo];
    PRINT 'Default DF_USUARIOS_activo (1) agregado';
END
ELSE
BEGIN
    PRINT 'Default en USUARIOS.activo ya existe';
END
GO

-- ============================================================================
-- 2. USUARIOS: columna rol NOT NULL DEFAULT 4 + CHECK (1,2,3,4)
-- ============================================================================
DECLARE @rolNullable BIT;
SELECT @rolNullable = c.is_nullable
FROM sys.columns c
JOIN sys.tables t ON c.object_id = t.object_id
WHERE t.name = 'USUARIOS' AND c.name = 'rol';

IF @rolNullable = 1
BEGIN
    -- Asignar rol por defecto a filas sin valor
    UPDATE [dbo].[USUARIOS] SET [rol] = 4 WHERE [rol] IS NULL;
    PRINT 'Filas con rol NULL actualizadas a 4';

    ALTER TABLE [dbo].[USUARIOS] ALTER COLUMN [rol] INT NOT NULL;
    PRINT 'USUARIOS.rol ahora es NOT NULL';

    IF NOT EXISTS (
        SELECT 1 FROM sys.default_constraints
        WHERE parent_object_id = OBJECT_ID('USUARIOS') AND parent_column_id = COLUMNPROPERTY(OBJECT_ID('USUARIOS'), 'rol', 'ColumnId')
    )
    BEGIN
        ALTER TABLE [dbo].[USUARIOS] ADD CONSTRAINT [DF_USUARIOS_rol] DEFAULT (4) FOR [rol];
        PRINT 'Default DF_USUARIOS_rol (4) agregado';
    END
END
ELSE
BEGIN
    PRINT 'USUARIOS.rol ya es NOT NULL';
END
GO

IF NOT EXISTS (SELECT 1 FROM sys.check_constraints WHERE name = 'CK_USUARIOS_Rol')
BEGIN
    ALTER TABLE [dbo].[USUARIOS] ADD CONSTRAINT [CK_USUARIOS_Rol] CHECK ([rol] IN (1, 2, 3, 4));
    PRINT 'Constraint CK_USUARIOS_Rol agregado';
END
ELSE
BEGIN
    PRINT 'Constraint CK_USUARIOS_Rol ya existe';
END
GO

-- ============================================================================
-- 3. USUARIOS: columna tipo VARCHAR(50) + CHECK con 'Familiar'
-- ============================================================================
DECLARE @tipoLength INT;
SELECT @tipoLength = c.max_length
FROM sys.columns c
JOIN sys.tables t ON c.object_id = t.object_id
WHERE t.name = 'USUARIOS' AND c.name = 'tipo';

IF @tipoLength < 50
BEGIN
    -- Eliminar el CHECK viejo para poder modificar el tipo sin problemas
    IF EXISTS (SELECT 1 FROM sys.check_constraints WHERE name = 'CK_USUARIOS_Tipo')
    BEGIN
        ALTER TABLE [dbo].[USUARIOS] DROP CONSTRAINT [CK_USUARIOS_Tipo];
        PRINT 'Constraint CK_USUARIOS_Tipo anterior eliminado';
    END

    ALTER TABLE [dbo].[USUARIOS] ALTER COLUMN [tipo] VARCHAR(50) NOT NULL;
    PRINT 'USUARIOS.tipo ahora es VARCHAR(50)';
END
ELSE
BEGIN
    PRINT 'USUARIOS.tipo ya tiene longitud >= 50';
END
GO

IF NOT EXISTS (SELECT 1 FROM sys.check_constraints WHERE name = 'CK_USUARIOS_Tipo')
BEGIN
    ALTER TABLE [dbo].[USUARIOS] ADD CONSTRAINT [CK_USUARIOS_Tipo]
        CHECK ([tipo] IN ('Empleado', 'Entrenador', 'Cliente', 'Familiar'));
    PRINT 'Constraint CK_USUARIOS_Tipo agregado';
END
ELSE
BEGIN
    -- Si ya existe, verificar que incluya Familiar; si no, recrearlo
    DECLARE @defTipo NVARCHAR(MAX);
    SELECT @defTipo = [definition] FROM sys.check_constraints WHERE name = 'CK_USUARIOS_Tipo';

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

-- ============================================================================
-- 4. USUARIOS: columnas dvv y dvh NOT NULL
-- ============================================================================
DECLARE @dvvNullable BIT, @dvhNullable BIT;
SELECT @dvvNullable = c.is_nullable
FROM sys.columns c
JOIN sys.tables t ON c.object_id = t.object_id
WHERE t.name = 'USUARIOS' AND c.name = 'dvv';

SELECT @dvhNullable = c.is_nullable
FROM sys.columns c
JOIN sys.tables t ON c.object_id = t.object_id
WHERE t.name = 'USUARIOS' AND c.name = 'dvh';

IF @dvvNullable = 1
BEGIN
    UPDATE [dbo].[USUARIOS] SET [dvv] = '' WHERE [dvv] IS NULL;
    ALTER TABLE [dbo].[USUARIOS] ALTER COLUMN [dvv] VARCHAR(50) NOT NULL;
    PRINT 'USUARIOS.dvv ahora es NOT NULL';
END
ELSE
BEGIN
    PRINT 'USUARIOS.dvv ya es NOT NULL';
END

IF @dvhNullable = 1
BEGIN
    UPDATE [dbo].[USUARIOS] SET [dvh] = '' WHERE [dvh] IS NULL;
    ALTER TABLE [dbo].[USUARIOS] ALTER COLUMN [dvh] VARCHAR(50) NOT NULL;
    PRINT 'USUARIOS.dvh ahora es NOT NULL';
END
ELSE
BEGIN
    PRINT 'USUARIOS.dvh ya es NOT NULL';
END
GO

-- ============================================================================
-- 5. USUARIO_Intentos: crear como tabla obsoleta si no existe
-- ============================================================================
IF OBJECT_ID('USUARIO_Intentos', 'U') IS NULL
BEGIN
    CREATE TABLE [dbo].[USUARIO_Intentos](
        [usr]           VARCHAR(50)     NOT NULL,
        [intentos]      INT             NOT NULL DEFAULT 0,
        [dvv]           VARCHAR(50)     NOT NULL,
        [dvh]           VARCHAR(50)     NOT NULL,
        CONSTRAINT [PK_USUARIO_Intentos] PRIMARY KEY CLUSTERED ([usr] ASC),
        CONSTRAINT [FK_UsuarioIntentos_Usuario] FOREIGN KEY ([usr])
            REFERENCES [dbo].[USUARIOS] ([usr])
    ) ON [PRIMARY];

    PRINT 'Tabla USUARIO_Intentos creada (obsoleta - usar USUARIOS.intentos)';
END
ELSE
BEGIN
    PRINT 'Tabla USUARIO_Intentos ya existe';
END
GO

-- ============================================================================
-- 6. Indices en USUARIOS (por si faltan)
-- ============================================================================
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

-- ============================================================================
-- 7. Tabla PreguntasSeguridad
-- ============================================================================
IF OBJECT_ID('PreguntasSeguridad', 'U') IS NULL
BEGIN
    CREATE TABLE [dbo].[PreguntasSeguridad](
        [codPregunta]   INT             IDENTITY(1,1) NOT NULL,
        [usr]           VARCHAR(50)     NOT NULL,
        [pregunta]      VARCHAR(500)    NOT NULL,
        [respuesta]     VARCHAR(500)    NOT NULL,
        [dvv]           VARCHAR(50)     NOT NULL,
        [dvh]           VARCHAR(50)     NOT NULL,
        CONSTRAINT [PK_PreguntasSeguridad] PRIMARY KEY CLUSTERED ([codPregunta] ASC),
        CONSTRAINT [FK_PreguntasSeguridad_Usuario] FOREIGN KEY ([usr])
            REFERENCES [dbo].[USUARIOS] ([usr])
    );

    CREATE NONCLUSTERED INDEX [IX_PreguntasSeguridad_usr]
        ON [dbo].[PreguntasSeguridad] ([usr] ASC);

    PRINT 'Tabla PreguntasSeguridad e indice IX_PreguntasSeguridad_usr creados';
END
ELSE
BEGIN
    PRINT 'Tabla PreguntasSeguridad ya existe';
END
GO

PRINT '=== FIN: correccion de esquema completada ===';
GO
