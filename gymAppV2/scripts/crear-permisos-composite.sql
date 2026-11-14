USE [GymApp]
GO

-- ============================================================
-- Permisos con el patrón Composite.
--
-- 1) Borra las tablas viejas de permisos que no usa ningún código:
--    RolPermiso (permisos por rol, versión provisoria del 04/10) y las del esquema viejo
--    (Familia/Permiso con columnas familiaId/permisoId, PermisoFamilia, Perfiles,
--    Perfil_Familia, Perfil_Permiso, Usuario_Perfil), con sus filas de control en DigitoVerificador.
--
-- 2) Crea las tablas nuevas, todas con dvh:
--      Permiso          un permiso por pantalla (hoja del Composite)
--      Familia          grupo de permisos y de otras familias (compuesto)
--      FamiliaPermiso   permisos de cada familia
--      FamiliaFamilia   subfamilias de cada familia
--      Perfil           lo que se le asigna a un usuario
--      PerfilPermiso    permisos sueltos del perfil
--      PerfilFamilia    familias del perfil
--      UsuarioPerfil    perfil de cada usuario (uno solo: usr es la clave)
--
-- 3) Carga los permisos, las familias y los perfiles iniciales y le asigna a cada usuario
--    el perfil de su tipo de cuenta (USUARIOS.rol):
--      1 Administrador, 2 Recepcionista, 3 Entrenador, 4 Cliente, 5 WebMaster, 6 Familiar.
--    Cada perfil queda con las mismas pantallas que tenía su rol por defecto.
--    Lo que se haya cambiado a mano en la grilla vieja de Permisos (RolPermiso) no se copia.
--
-- Se puede correr más de una vez: lo que ya existe no se vuelve a crear.
--
-- ANTES de correrlo: hacer un backup de la base.
-- DESPUÉS de correrlo:
--   1) agregar-traducciones-permisos-composite.sql
--   2) VerificacioDV -> "Inicializar control" (tablas nuevas) y luego "Recalcular dígitos".
-- ============================================================

-- ---------- 1) Tablas viejas ----------
DECLARE @viejas TABLE (nombre SYSNAME PRIMARY KEY);

INSERT INTO @viejas (nombre)
SELECT name FROM sys.tables
WHERE schema_id = SCHEMA_ID('dbo')
  AND name IN ('RolPermiso', 'PermisoFamilia', 'Perfiles', 'Perfil_Familia', 'Perfil_Permiso', 'Usuario_Perfil');

-- Familia y Permiso se llaman igual que las nuevas: solo se borran si son las del esquema viejo
IF OBJECT_ID('[dbo].[Familia]', 'U') IS NOT NULL AND COL_LENGTH('dbo.Familia', 'idFamilia') IS NULL
    INSERT INTO @viejas (nombre) VALUES ('Familia');
IF OBJECT_ID('[dbo].[Permiso]', 'U') IS NOT NULL AND COL_LENGTH('dbo.Permiso', 'idPermiso') IS NULL
    INSERT INTO @viejas (nombre) VALUES ('Permiso');

SELECT nombre AS [Tablas viejas que se borran] FROM @viejas;

DECLARE @sql NVARCHAR(MAX) = N'';

-- Primero las claves foráneas que entran o salen de esas tablas
SELECT @sql += N'ALTER TABLE ' + QUOTENAME(SCHEMA_NAME(t.schema_id)) + N'.' + QUOTENAME(t.name)
             + N' DROP CONSTRAINT ' + QUOTENAME(fk.name) + N';' + CHAR(10)
FROM sys.foreign_keys fk
JOIN sys.tables t ON t.object_id = fk.parent_object_id
WHERE t.name IN (SELECT nombre FROM @viejas)
   OR OBJECT_NAME(fk.referenced_object_id) IN (SELECT nombre FROM @viejas);
EXEC sp_executesql @sql;

SET @sql = N'';
SELECT @sql += N'DROP TABLE [dbo].' + QUOTENAME(nombre) + N';' + CHAR(10) FROM @viejas;
EXEC sp_executesql @sql;

IF OBJECT_ID('[dbo].[DigitoVerificador]', 'U') IS NOT NULL
    DELETE FROM [dbo].[DigitoVerificador] WHERE nombreTabla IN (SELECT nombre FROM @viejas);
GO

-- ---------- 2) Tablas nuevas ----------
IF OBJECT_ID('[dbo].[Permiso]', 'U') IS NULL
BEGIN
    CREATE TABLE [dbo].[Permiso](
        [idPermiso]  INT          IDENTITY(1,1) NOT NULL,
        [nombre]     VARCHAR(50)  NOT NULL,
        [dvh]        VARCHAR(64)  NOT NULL,
        CONSTRAINT [PK_Permiso] PRIMARY KEY CLUSTERED ([idPermiso] ASC),
        CONSTRAINT [UQ_Permiso_nombre] UNIQUE ([nombre])
    ) ON [PRIMARY];
    PRINT 'Tabla Permiso creada.';
END
GO

IF OBJECT_ID('[dbo].[Familia]', 'U') IS NULL
BEGIN
    CREATE TABLE [dbo].[Familia](
        [idFamilia]    INT           IDENTITY(1,1) NOT NULL,
        [nombre]       VARCHAR(100)  NOT NULL,
        [codigo]       VARCHAR(30)   NULL,
        [profundidad]  BIT           NOT NULL CONSTRAINT [DF_Familia_profundidad] DEFAULT (0),
        [dvh]          VARCHAR(64)   NOT NULL,
        CONSTRAINT [PK_Familia] PRIMARY KEY CLUSTERED ([idFamilia] ASC),
        CONSTRAINT [UQ_Familia_nombre] UNIQUE ([nombre])
    ) ON [PRIMARY];

    -- El código identifica a las familias que crea este script (se usa para traducir el nombre)
    CREATE UNIQUE NONCLUSTERED INDEX [UX_Familia_codigo] ON [dbo].[Familia] ([codigo]) WHERE [codigo] IS NOT NULL;
    PRINT 'Tabla Familia creada.';
END
GO

IF OBJECT_ID('[dbo].[FamiliaPermiso]', 'U') IS NULL
BEGIN
    CREATE TABLE [dbo].[FamiliaPermiso](
        [idFamilia]  INT          NOT NULL,
        [idPermiso]  INT          NOT NULL,
        [dvh]        VARCHAR(64)  NOT NULL,
        CONSTRAINT [PK_FamiliaPermiso] PRIMARY KEY CLUSTERED ([idFamilia] ASC, [idPermiso] ASC),
        CONSTRAINT [FK_FamiliaPermiso_Familia] FOREIGN KEY ([idFamilia]) REFERENCES [dbo].[Familia] ([idFamilia]),
        CONSTRAINT [FK_FamiliaPermiso_Permiso] FOREIGN KEY ([idPermiso]) REFERENCES [dbo].[Permiso] ([idPermiso])
    ) ON [PRIMARY];
    PRINT 'Tabla FamiliaPermiso creada.';
END
GO

IF OBJECT_ID('[dbo].[FamiliaFamilia]', 'U') IS NULL
BEGIN
    CREATE TABLE [dbo].[FamiliaFamilia](
        [idFamiliaPadre]  INT          NOT NULL,
        [idFamiliaHija]   INT          NOT NULL,
        [dvh]             VARCHAR(64)  NOT NULL,
        CONSTRAINT [PK_FamiliaFamilia] PRIMARY KEY CLUSTERED ([idFamiliaPadre] ASC, [idFamiliaHija] ASC),
        CONSTRAINT [FK_FamiliaFamilia_Padre] FOREIGN KEY ([idFamiliaPadre]) REFERENCES [dbo].[Familia] ([idFamilia]),
        CONSTRAINT [FK_FamiliaFamilia_Hija] FOREIGN KEY ([idFamiliaHija]) REFERENCES [dbo].[Familia] ([idFamilia]),
        CONSTRAINT [CK_FamiliaFamilia_distintas] CHECK ([idFamiliaPadre] <> [idFamiliaHija])
    ) ON [PRIMARY];
    PRINT 'Tabla FamiliaFamilia creada.';
END
GO

IF OBJECT_ID('[dbo].[Perfil]', 'U') IS NULL
BEGIN
    CREATE TABLE [dbo].[Perfil](
        [idPerfil]  INT           IDENTITY(1,1) NOT NULL,
        [nombre]    VARCHAR(100)  NOT NULL,
        [codigo]    VARCHAR(30)   NULL,
        [dvh]       VARCHAR(64)   NOT NULL,
        CONSTRAINT [PK_Perfil] PRIMARY KEY CLUSTERED ([idPerfil] ASC),
        CONSTRAINT [UQ_Perfil_nombre] UNIQUE ([nombre])
    ) ON [PRIMARY];

    -- Código de los perfiles del sistema (WEBMASTER, ADMIN, ...): no se renombran ni se eliminan
    CREATE UNIQUE NONCLUSTERED INDEX [UX_Perfil_codigo] ON [dbo].[Perfil] ([codigo]) WHERE [codigo] IS NOT NULL;
    PRINT 'Tabla Perfil creada.';
END
GO

IF OBJECT_ID('[dbo].[PerfilPermiso]', 'U') IS NULL
BEGIN
    CREATE TABLE [dbo].[PerfilPermiso](
        [idPerfil]   INT          NOT NULL,
        [idPermiso]  INT          NOT NULL,
        [dvh]        VARCHAR(64)  NOT NULL,
        CONSTRAINT [PK_PerfilPermiso] PRIMARY KEY CLUSTERED ([idPerfil] ASC, [idPermiso] ASC),
        CONSTRAINT [FK_PerfilPermiso_Perfil] FOREIGN KEY ([idPerfil]) REFERENCES [dbo].[Perfil] ([idPerfil]),
        CONSTRAINT [FK_PerfilPermiso_Permiso] FOREIGN KEY ([idPermiso]) REFERENCES [dbo].[Permiso] ([idPermiso])
    ) ON [PRIMARY];
    PRINT 'Tabla PerfilPermiso creada.';
END
GO

IF OBJECT_ID('[dbo].[PerfilFamilia]', 'U') IS NULL
BEGIN
    CREATE TABLE [dbo].[PerfilFamilia](
        [idPerfil]   INT          NOT NULL,
        [idFamilia]  INT          NOT NULL,
        [dvh]        VARCHAR(64)  NOT NULL,
        CONSTRAINT [PK_PerfilFamilia] PRIMARY KEY CLUSTERED ([idPerfil] ASC, [idFamilia] ASC),
        CONSTRAINT [FK_PerfilFamilia_Perfil] FOREIGN KEY ([idPerfil]) REFERENCES [dbo].[Perfil] ([idPerfil]),
        CONSTRAINT [FK_PerfilFamilia_Familia] FOREIGN KEY ([idFamilia]) REFERENCES [dbo].[Familia] ([idFamilia])
    ) ON [PRIMARY];
    PRINT 'Tabla PerfilFamilia creada.';
END
GO

IF OBJECT_ID('[dbo].[UsuarioPerfil]', 'U') IS NULL
BEGIN
    CREATE TABLE [dbo].[UsuarioPerfil](
        [usr]       VARCHAR(50)  NOT NULL,
        [idPerfil]  INT          NOT NULL,
        [dvh]       VARCHAR(64)  NOT NULL,
        CONSTRAINT [PK_UsuarioPerfil] PRIMARY KEY CLUSTERED ([usr] ASC),
        -- Si se renombra la cuenta, la asignación la sigue (la app recalcula el dvh)
        CONSTRAINT [FK_UsuarioPerfil_Usuario] FOREIGN KEY ([usr]) REFERENCES [dbo].[USUARIOS] ([usr]) ON UPDATE CASCADE,
        CONSTRAINT [FK_UsuarioPerfil_Perfil] FOREIGN KEY ([idPerfil]) REFERENCES [dbo].[Perfil] ([idPerfil])
    ) ON [PRIMARY];
    PRINT 'Tabla UsuarioPerfil creada.';
END
GO

-- ---------- 3) Datos iniciales ----------
SET XACT_ABORT ON;
BEGIN TRANSACTION;

-- Permisos: uno por pantalla (BE.PermisosSistema). Se agregan los que falten.
INSERT INTO [dbo].[Permiso] ([nombre], [dvh])
SELECT v.nombre, ''
FROM (VALUES
    ('Dashboard'), ('Perfil'),
    ('GestionUsuarios'), ('GestionAlumnos'), ('GestionEntrenadores'), ('Bitacora'),
    ('ActividadesCalendario'), ('GestionActividades'), ('GestionAulas'), ('GestionRutinas'),
    ('Pagos'), ('PreciosCuota'),
    ('VerificacionDV'), ('Backup'), ('Restore'), ('RecalcularDV'), ('EncriptarDatos'),
    ('GestionPermisos'), ('GestionIdiomas')
) v (nombre)
WHERE NOT EXISTS (SELECT 1 FROM [dbo].[Permiso] p WHERE p.nombre = v.nombre);

-- Familias y perfiles: solo la primera vez
IF NOT EXISTS (SELECT 1 FROM [dbo].[Familia]) AND NOT EXISTS (SELECT 1 FROM [dbo].[Perfil])
BEGIN
    INSERT INTO [dbo].[Familia] ([nombre], [codigo], [profundidad], [dvh]) VALUES
        ('Portal del cliente',                'PORTAL_CLIENTE',   0, ''),
        ('Operación del gimnasio',            'OPERACION',        0, ''),
        ('Administración del gimnasio',       'ADMIN_GIMNASIO',   1, ''),
        ('Caja',                              'CAJA',             0, ''),
        ('Gestión de usuarios',               'GESTION_USUARIOS', 0, ''),
        ('Seguridad',                         'SEGURIDAD',        1, ''),
        ('Mantenimiento de la base de datos', 'MANTENIMIENTO_BD', 0, '');

    INSERT INTO [dbo].[FamiliaPermiso] ([idFamilia], [idPermiso], [dvh])
    SELECT f.idFamilia, p.idPermiso, ''
    FROM (VALUES
        ('PORTAL_CLIENTE',   'ActividadesCalendario'),
        ('PORTAL_CLIENTE',   'Pagos'),
        ('PORTAL_CLIENTE',   'Perfil'),
        ('OPERACION',        'GestionAlumnos'),
        ('OPERACION',        'GestionEntrenadores'),
        ('OPERACION',        'GestionActividades'),
        ('OPERACION',        'GestionRutinas'),
        ('ADMIN_GIMNASIO',   'GestionAulas'),
        ('CAJA',             'Pagos'),
        ('CAJA',             'PreciosCuota'),
        ('GESTION_USUARIOS', 'GestionUsuarios'),
        ('GESTION_USUARIOS', 'Bitacora'),
        ('SEGURIDAD',        'GestionPermisos'),
        ('MANTENIMIENTO_BD', 'Backup'),
        ('MANTENIMIENTO_BD', 'Restore'),
        ('MANTENIMIENTO_BD', 'VerificacionDV'),
        ('MANTENIMIENTO_BD', 'RecalcularDV'),
        ('MANTENIMIENTO_BD', 'EncriptarDatos')
    ) v (codigo, permiso)
    JOIN [dbo].[Familia] f ON f.codigo = v.codigo
    JOIN [dbo].[Permiso] p ON p.nombre = v.permiso;

    -- Administración del gimnasio ⊃ Operación del gimnasio; Seguridad ⊃ Gestión de usuarios
    INSERT INTO [dbo].[FamiliaFamilia] ([idFamiliaPadre], [idFamiliaHija], [dvh])
    SELECT padre.idFamilia, hija.idFamilia, ''
    FROM (VALUES
        ('ADMIN_GIMNASIO', 'OPERACION'),
        ('SEGURIDAD',      'GESTION_USUARIOS')
    ) v (padre, hija)
    JOIN [dbo].[Familia] padre ON padre.codigo = v.padre
    JOIN [dbo].[Familia] hija ON hija.codigo = v.hija;

    INSERT INTO [dbo].[Perfil] ([nombre], [codigo], [dvh]) VALUES
        ('WebMaster',     'WEBMASTER',  ''),
        ('Administrador', 'ADMIN',      ''),
        ('Recepcionista', 'RECEPCION',  ''),
        ('Entrenador',    'ENTRENADOR', ''),
        ('Cliente',       'CLIENTE',    ''),
        ('Familiar',      'FAMILIAR',   '');

    -- El WebMaster no tiene componentes: tiene todos los permisos siempre
    INSERT INTO [dbo].[PerfilPermiso] ([idPerfil], [idPermiso], [dvh])
    SELECT pf.idPerfil, p.idPermiso, ''
    FROM (VALUES
        ('ADMIN',      'Dashboard'),
        ('ADMIN',      'ActividadesCalendario'),
        ('ADMIN',      'GestionIdiomas'),
        ('RECEPCION',  'Dashboard'),
        ('RECEPCION',  'ActividadesCalendario'),
        ('ENTRENADOR', 'Dashboard'),
        ('ENTRENADOR', 'GestionRutinas'),
        ('CLIENTE',    'Dashboard'),
        ('FAMILIAR',   'Dashboard'),
        ('FAMILIAR',   'GestionAlumnos')
    ) v (perfil, permiso)
    JOIN [dbo].[Perfil] pf ON pf.codigo = v.perfil
    JOIN [dbo].[Permiso] p ON p.nombre = v.permiso;

    INSERT INTO [dbo].[PerfilFamilia] ([idPerfil], [idFamilia], [dvh])
    SELECT pf.idPerfil, f.idFamilia, ''
    FROM (VALUES
        ('ADMIN',     'ADMIN_GIMNASIO'),
        ('ADMIN',     'CAJA'),
        ('ADMIN',     'SEGURIDAD'),
        ('ADMIN',     'MANTENIMIENTO_BD'),
        ('RECEPCION', 'OPERACION'),
        ('RECEPCION', 'CAJA'),
        ('RECEPCION', 'GESTION_USUARIOS'),
        ('CLIENTE',   'PORTAL_CLIENTE'),
        ('FAMILIAR',  'PORTAL_CLIENTE')
    ) v (perfil, familia)
    JOIN [dbo].[Perfil] pf ON pf.codigo = v.perfil
    JOIN [dbo].[Familia] f ON f.codigo = v.familia;

    PRINT 'Familias y perfiles iniciales cargados.';
END
ELSE
    PRINT 'Ya había familias o perfiles: no se cargan los iniciales.';

-- Cada usuario sin perfil recibe el de su tipo de cuenta
INSERT INTO [dbo].[UsuarioPerfil] ([usr], [idPerfil], [dvh])
SELECT u.usr, pf.idPerfil, ''
FROM [dbo].[USUARIOS] u
JOIN (VALUES
    (1, 'ADMIN'), (2, 'RECEPCION'), (3, 'ENTRENADOR'),
    (4, 'CLIENTE'), (5, 'WEBMASTER'), (6, 'FAMILIAR')
) m (rol, codigo) ON m.rol = u.rol
JOIN [dbo].[Perfil] pf ON pf.codigo = m.codigo
WHERE NOT EXISTS (SELECT 1 FROM [dbo].[UsuarioPerfil] up WHERE up.usr = u.usr);

COMMIT TRANSACTION;
GO

-- ---------- Resultado ----------
SELECT pf.nombre AS Perfil, COUNT(up.usr) AS Usuarios
FROM [dbo].[Perfil] pf
LEFT JOIN [dbo].[UsuarioPerfil] up ON up.idPerfil = pf.idPerfil
GROUP BY pf.idPerfil, pf.nombre
ORDER BY pf.idPerfil;

-- Usuarios que quedaron sin perfil (rol desconocido): no van a poder entrar a ninguna pantalla
SELECT u.usr, u.rol AS [rol sin perfil]
FROM [dbo].[USUARIOS] u
WHERE NOT EXISTS (SELECT 1 FROM [dbo].[UsuarioPerfil] up WHERE up.usr = u.usr);
GO
