-- ============================================================================
-- Script consolidado: Aplicar todos los cambios de seguridad a BD existente
-- GymApp
-- ============================================================================
-- Fecha: 2026-06-18
-- Descripcion: Script idempotente que aplica TODOS los cambios de esquema y
--              datos iniciales necesarios para el flujo de seguridad completo:
--              - Columnas extendidas en USUARIOS (activo, bloqueado, intentos,
--                rol, tipo, datos personales, dvv, dvh, primerLogin).
--              - Constraints y defaults de USUARIOS.
--              - Tablas USUARIO_Contras y PreguntasSeguridad.
--              - Vaciado de USUARIO_Intentos (obsoleta).
--              - Usuario admin inicial y su contrasena en historial.
--
-- Requisitos previos:
--   - La base de datos [GymApp] debe existir.
--   - Ejecutar con privilegios suficientes para ALTER TABLE / CREATE TABLE.
--
-- Comportamiento idempotente:
--   - Cada seccion verifica existencia antes de crear/alterar.
--   - Si todo ya esta aplicado, los mensajes indican "ya existe" y termina OK.
-- ============================================================================

USE [master];
GO

IF DB_ID('GymApp') IS NULL
BEGIN
    RAISERROR('ERROR: La base de datos [GymApp] no existe.', 16, 1);
    RETURN;
END
GO

USE [GymApp];
GO

SET NOCOUNT ON;
GO

PRINT '=== INICIO: aplicacion de todos los cambios de seguridad ===';
GO

-- ============================================================================
-- 1. Tabla USUARIO_Intentos (obsoleta - crear si no existe para compatibilidad)
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
-- 2. Asegurar columnas base en USUARIOS
-- ============================================================================
IF COL_LENGTH('USUARIOS', 'activo') IS NULL
BEGIN
    ALTER TABLE [dbo].[USUARIOS] ADD [activo] BIT NULL;
    PRINT 'Columna USUARIOS.activo agregada';
END
ELSE
BEGIN
    PRINT 'Columna USUARIOS.activo ya existe';
END
GO

IF COL_LENGTH('USUARIOS', 'bloqueado') IS NULL
BEGIN
    ALTER TABLE [dbo].[USUARIOS] ADD [bloqueado] BIT NULL;
    PRINT 'Columna USUARIOS.bloqueado agregada';
END
ELSE
BEGIN
    PRINT 'Columna USUARIOS.bloqueado ya existe';
END
GO

IF COL_LENGTH('USUARIOS', 'intentos') IS NULL
BEGIN
    ALTER TABLE [dbo].[USUARIOS] ADD [intentos] INT NULL;
    PRINT 'Columna USUARIOS.intentos agregada';
END
ELSE
BEGIN
    PRINT 'Columna USUARIOS.intentos ya existe';
END
GO

IF COL_LENGTH('USUARIOS', 'tipo') IS NULL
BEGIN
    ALTER TABLE [dbo].[USUARIOS] ADD [tipo] VARCHAR(50) NULL;
    PRINT 'Columna USUARIOS.tipo agregada';
END
ELSE
BEGIN
    PRINT 'Columna USUARIOS.tipo ya existe';
END
GO

IF COL_LENGTH('USUARIOS', 'dni') IS NULL
BEGIN
    ALTER TABLE [dbo].[USUARIOS] ADD [dni] INT NULL;
    PRINT 'Columna USUARIOS.dni agregada';
END
ELSE
BEGIN
    PRINT 'Columna USUARIOS.dni ya existe';
END
GO

IF COL_LENGTH('USUARIOS', 'nombre') IS NULL
BEGIN
    ALTER TABLE [dbo].[USUARIOS] ADD [nombre] VARCHAR(100) NULL;
    PRINT 'Columna USUARIOS.nombre agregada';
END
ELSE
BEGIN
    PRINT 'Columna USUARIOS.nombre ya existe';
END
GO

IF COL_LENGTH('USUARIOS', 'apellido') IS NULL
BEGIN
    ALTER TABLE [dbo].[USUARIOS] ADD [apellido] VARCHAR(100) NULL;
    PRINT 'Columna USUARIOS.apellido agregada';
END
ELSE
BEGIN
    PRINT 'Columna USUARIOS.apellido ya existe';
END
GO

IF COL_LENGTH('USUARIOS', 'telefono') IS NULL
BEGIN
    ALTER TABLE [dbo].[USUARIOS] ADD [telefono] VARCHAR(20) NULL;
    PRINT 'Columna USUARIOS.telefono agregada';
END
ELSE
BEGIN
    PRINT 'Columna USUARIOS.telefono ya existe';
END
GO

IF COL_LENGTH('USUARIOS', 'email') IS NULL
BEGIN
    ALTER TABLE [dbo].[USUARIOS] ADD [email] VARCHAR(255) NULL;
    PRINT 'Columna USUARIOS.email agregada';
END
ELSE
BEGIN
    PRINT 'Columna USUARIOS.email ya existe';
END
GO

IF COL_LENGTH('USUARIOS', 'fechaNacimiento') IS NULL
BEGIN
    ALTER TABLE [dbo].[USUARIOS] ADD [fechaNacimiento] DATE NULL;
    PRINT 'Columna USUARIOS.fechaNacimiento agregada';
END
ELSE
BEGIN
    PRINT 'Columna USUARIOS.fechaNacimiento ya existe';
END
GO

IF COL_LENGTH('USUARIOS', 'rol') IS NULL
BEGIN
    ALTER TABLE [dbo].[USUARIOS] ADD [rol] INT NULL;
    PRINT 'Columna USUARIOS.rol agregada';
END
ELSE
BEGIN
    PRINT 'Columna USUARIOS.rol ya existe';
END
GO

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

IF COL_LENGTH('USUARIOS', 'dvv') IS NULL
BEGIN
    ALTER TABLE [dbo].[USUARIOS] ADD [dvv] VARCHAR(50) NULL;
    PRINT 'Columna USUARIOS.dvv agregada';
END
ELSE
BEGIN
    PRINT 'Columna USUARIOS.dvv ya existe';
END
GO

IF COL_LENGTH('USUARIOS', 'dvh') IS NULL
BEGIN
    ALTER TABLE [dbo].[USUARIOS] ADD [dvh] VARCHAR(50) NULL;
    PRINT 'Columna USUARIOS.dvh agregada';
END
ELSE
BEGIN
    PRINT 'Columna USUARIOS.dvh ya existe';
END
GO

-- ============================================================================
-- 3. Asegurar defaults en USUARIOS
-- ============================================================================
IF NOT EXISTS (
    SELECT 1 FROM sys.default_constraints d
    JOIN sys.columns c ON d.parent_column_id = c.column_id AND d.parent_object_id = c.object_id
    JOIN sys.tables t ON c.object_id = t.object_id
    WHERE t.name = 'USUARIOS' AND c.name = 'activo'
)
BEGIN
    ALTER TABLE [dbo].[USUARIOS] ADD CONSTRAINT [DF_USUARIOS_activo] DEFAULT (1) FOR [activo];
    PRINT 'Default DF_USUARIOS_activo agregado';
END
GO

IF NOT EXISTS (
    SELECT 1 FROM sys.default_constraints d
    JOIN sys.columns c ON d.parent_column_id = c.column_id AND d.parent_object_id = c.object_id
    JOIN sys.tables t ON c.object_id = t.object_id
    WHERE t.name = 'USUARIOS' AND c.name = 'bloqueado'
)
BEGIN
    ALTER TABLE [dbo].[USUARIOS] ADD CONSTRAINT [DF_USUARIOS_bloqueado] DEFAULT (0) FOR [bloqueado];
    PRINT 'Default DF_USUARIOS_bloqueado agregado';
END
GO

IF NOT EXISTS (
    SELECT 1 FROM sys.default_constraints d
    JOIN sys.columns c ON d.parent_column_id = c.column_id AND d.parent_object_id = c.object_id
    JOIN sys.tables t ON c.object_id = t.object_id
    WHERE t.name = 'USUARIOS' AND c.name = 'intentos'
)
BEGIN
    ALTER TABLE [dbo].[USUARIOS] ADD CONSTRAINT [DF_USUARIOS_intentos] DEFAULT (0) FOR [intentos];
    PRINT 'Default DF_USUARIOS_intentos agregado';
END
GO

IF NOT EXISTS (
    SELECT 1 FROM sys.default_constraints d
    JOIN sys.columns c ON d.parent_column_id = c.column_id AND d.parent_object_id = c.object_id
    JOIN sys.tables t ON c.object_id = t.object_id
    WHERE t.name = 'USUARIOS' AND c.name = 'rol'
)
BEGIN
    ALTER TABLE [dbo].[USUARIOS] ADD CONSTRAINT [DF_USUARIOS_rol] DEFAULT (4) FOR [rol];
    PRINT 'Default DF_USUARIOS_rol agregado';
END
GO

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
GO

-- ============================================================================
-- 4. Rellenar valores minimos para poder hacer NOT NULL
-- ============================================================================
UPDATE [dbo].[USUARIOS] SET [activo] = 1 WHERE [activo] IS NULL;
UPDATE [dbo].[USUARIOS] SET [bloqueado] = 0 WHERE [bloqueado] IS NULL;
UPDATE [dbo].[USUARIOS] SET [intentos] = 0 WHERE [intentos] IS NULL;
UPDATE [dbo].[USUARIOS] SET [rol] = 4 WHERE [rol] IS NULL;
UPDATE [dbo].[USUARIOS] SET [primerLogin] = 1 WHERE [primerLogin] IS NULL;
UPDATE [dbo].[USUARIOS] SET [tipo] = 'Cliente' WHERE [tipo] IS NULL OR [tipo] = '';
UPDATE [dbo].[USUARIOS] SET [nombre] = [usr] WHERE [nombre] IS NULL OR [nombre] = '';
UPDATE [dbo].[USUARIOS] SET [apellido] = [usr] WHERE [apellido] IS NULL OR [apellido] = '';
UPDATE [dbo].[USUARIOS] SET [dni] = 999999990 + ABS(CHECKSUM(NEWID())) % 10000 WHERE [dni] IS NULL;
UPDATE [dbo].[USUARIOS] SET [dvv] = '' WHERE [dvv] IS NULL;
UPDATE [dbo].[USUARIOS] SET [dvh] = '' WHERE [dvh] IS NULL;
PRINT 'Valores por defecto aplicados en USUARIOS';
GO

-- ============================================================================
-- 5. Convertir columnas a NOT NULL donde corresponda
-- ============================================================================
ALTER TABLE [dbo].[USUARIOS] ALTER COLUMN [activo] BIT NOT NULL;
ALTER TABLE [dbo].[USUARIOS] ALTER COLUMN [bloqueado] BIT NOT NULL;
ALTER TABLE [dbo].[USUARIOS] ALTER COLUMN [intentos] INT NOT NULL;
ALTER TABLE [dbo].[USUARIOS] ALTER COLUMN [tipo] VARCHAR(50) NOT NULL;
ALTER TABLE [dbo].[USUARIOS] ALTER COLUMN [dni] INT NOT NULL;
ALTER TABLE [dbo].[USUARIOS] ALTER COLUMN [nombre] VARCHAR(100) NOT NULL;
ALTER TABLE [dbo].[USUARIOS] ALTER COLUMN [apellido] VARCHAR(100) NOT NULL;
ALTER TABLE [dbo].[USUARIOS] ALTER COLUMN [rol] INT NOT NULL;
ALTER TABLE [dbo].[USUARIOS] ALTER COLUMN [primerLogin] BIT NOT NULL;
ALTER TABLE [dbo].[USUARIOS] ALTER COLUMN [dvv] VARCHAR(50) NOT NULL;
ALTER TABLE [dbo].[USUARIOS] ALTER COLUMN [dvh] VARCHAR(50) NOT NULL;
PRINT 'Columnas requeridas convertidas a NOT NULL';
GO

-- ============================================================================
-- 6. Constraints e indices de USUARIOS
-- ============================================================================
IF NOT EXISTS (SELECT 1 FROM sys.objects WHERE name = 'UK_USUARIOS_DNI' AND type = 'UQ')
BEGIN
    ALTER TABLE [dbo].[USUARIOS] ADD CONSTRAINT [UK_USUARIOS_DNI] UNIQUE ([dni]);
    PRINT 'Constraint UK_USUARIOS_DNI agregado';
END
ELSE
BEGIN
    PRINT 'Constraint UK_USUARIOS_DNI ya existe';
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

IF NOT EXISTS (SELECT 1 FROM sys.check_constraints WHERE name = 'CK_USUARIOS_Rol')
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

-- ============================================================================
-- 7. Tabla USUARIO_Contras
-- ============================================================================
IF OBJECT_ID('USUARIO_Contras', 'U') IS NULL
BEGIN
    CREATE TABLE [dbo].[USUARIO_Contras](
        [usr]       VARCHAR(50)     NOT NULL,
        [contra]    VARCHAR(255)    NOT NULL,
        [dvv]       VARCHAR(50)     NOT NULL,
        [dvh]       VARCHAR(50)     NOT NULL,
        CONSTRAINT [PK_USUARIO_Contras] PRIMARY KEY CLUSTERED ([usr] ASC, [contra] ASC),
        CONSTRAINT [FK_UsuarioContras_Usuario] FOREIGN KEY ([usr])
            REFERENCES [dbo].[USUARIOS] ([usr])
    ) ON [PRIMARY];

    PRINT 'Tabla USUARIO_Contras creada';
END
ELSE
BEGIN
    PRINT 'Tabla USUARIO_Contras ya existe';
END
GO

-- ============================================================================
-- 8. Tabla PreguntasSeguridad
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
    ) ON [PRIMARY];

    CREATE NONCLUSTERED INDEX [IX_PreguntasSeguridad_usr]
        ON [dbo].[PreguntasSeguridad] ([usr] ASC);

    PRINT 'Tabla PreguntasSeguridad e indice IX_PreguntasSeguridad_usr creados';
END
ELSE
BEGIN
    PRINT 'Tabla PreguntasSeguridad ya existe';
END
GO

-- ============================================================================
-- 9. Vaciar tabla USUARIO_Intentos (obsoleta)
-- ============================================================================
IF OBJECT_ID('USUARIO_Intentos', 'U') IS NOT NULL
BEGIN
    DELETE FROM [dbo].[USUARIO_Intentos];
    PRINT 'Tabla USUARIO_Intentos vaciada (obsoleta)';
END
GO

-- ============================================================================
-- 10. Datos iniciales: usuario admin
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
         0,        -- primerLogin: no forzar cambio
         '',       -- dvv
         ''        -- dvh
        );

    PRINT 'Usuario admin creado';
END
ELSE
BEGIN
    -- Asegurar que admin existente no sea forzado a cambiar contrasena
    UPDATE [dbo].[USUARIOS] SET [primerLogin] = 0 WHERE [usr] = 'admin';
    PRINT 'Usuario admin ya existe - primerLogin configurado a 0';
END
GO

IF NOT EXISTS (SELECT 1 FROM [dbo].[USUARIO_Contras] WHERE [usr] = 'admin')
BEGIN
    INSERT INTO [dbo].[USUARIO_Contras]
        ([usr], [contra], [dvv], [dvh])
    VALUES
        ('admin',
         '3eb3fe66b31e3b4d10fa70b5cad49c7112294af6ae4e476a1c405155d45aa121',
         '',
         '');

    PRINT 'Contrasena inicial de admin registrada en USUARIO_Contras';
END
ELSE
BEGIN
    PRINT 'Historial de contrasenas para admin ya existe';
END
GO

-- ============================================================================
-- 11. Normalizar usuarios existentes sin pregunta de seguridad
-- ============================================================================
INSERT INTO [dbo].[PreguntasSeguridad] ([usr], [pregunta], [respuesta], [dvv], [dvh])
SELECT
    u.[usr],
    'En que ano naciste?',
    CAST(YEAR(u.[fechaNacimiento]) AS VARCHAR(4)),
    '',
    ''
FROM [dbo].[USUARIOS] u
LEFT JOIN [dbo].[PreguntasSeguridad] p ON u.[usr] = p.[usr]
WHERE p.[codPregunta] IS NULL
  AND u.[fechaNacimiento] IS NOT NULL;

PRINT 'Preguntas de seguridad por defecto generadas para usuarios sin una';
GO

PRINT '=== FIN: todos los cambios de seguridad aplicados ===';
GO

-- ============================================================================
-- Resumen final
-- ============================================================================
SELECT
    'USUARIOS columnas' AS Verificacion,
    COUNT(*) AS Valor
FROM sys.columns c
JOIN sys.tables t ON c.object_id = t.object_id
WHERE t.name = 'USUARIOS'
UNION ALL
SELECT
    'Constraints en USUARIOS' AS Verificacion,
    COUNT(*)
FROM sys.objects
WHERE parent_object_id = OBJECT_ID('USUARIOS') AND type IN ('C', 'UQ', 'F')
UNION ALL
SELECT
    'Tablas de seguridad presentes' AS Verificacion,
    COUNT(*)
FROM sys.tables
WHERE name IN ('USUARIO_Contras', 'PreguntasSeguridad', 'USUARIO_Intentos')
UNION ALL
SELECT
    'Indices USUARIOS/PreguntasSeguridad' AS Verificacion,
    COUNT(*)
FROM sys.indexes
WHERE name IN ('IX_USUARIOS_Tipo', 'IX_USUARIOS_DNI', 'IX_PreguntasSeguridad_usr')
UNION ALL
SELECT
    'Usuarios con primerLogin = 1' AS Verificacion,
    COUNT(*)
FROM [dbo].[USUARIOS]
WHERE [primerLogin] = 1;
GO
