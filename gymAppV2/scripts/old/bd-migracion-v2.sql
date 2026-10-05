-- ============================================================================
-- Migración de Base de Datos - Normalización v2
-- GymApp - Centralización de datos personales en USUARIOS
-- ============================================================================
-- Fecha: 2026-05-28
-- Descripción: Normaliza la base de datos para que todos los datos personales
--              estén en la tabla USUARIOS, diferenciando por Tipo (Empleado,
--              Entrenador, Cliente)
-- ============================================================================

USE [master];
GO

-- ============================================================================
-- FASE 0: BACKUP DE DATOS EXISTENTES
-- ============================================================================
-- IMPORTANTE: Esto crea copias de seguridad temporales.
-- En producción, usar backup completo de SQL Server.

PRINT '=== FASE 0: Creando backups de tablas existentes ===';

IF OBJECT_ID('USUARIOS_BACKUP', 'U') IS NOT NULL DROP TABLE USUARIOS_BACKUP;
IF OBJECT_ID('ALUMNOS_BACKUP', 'U') IS NOT NULL DROP TABLE ALUMNOS_BACKUP;
IF OBJECT_ID('ENTRENADORES_BACKUP', 'U') IS NOT NULL DROP TABLE ENTRENADORES_BACKUP;

SELECT * INTO USUARIOS_BACKUP FROM USUARIOS;
SELECT * INTO ALUMNOS_BACKUP FROM ALUMNOS;
SELECT * INTO ENTRENADORES_BACKUP FROM ENTRENADORES;

PRINT 'Backups creados: USUARIOS_BACKUP, ALUMNOS_BACKUP, ENTRENADORES_BACKUP';
GO

-- ============================================================================
-- FASE 1: AGREGAR NUEVAS COLUMNAS A USUARIOS
-- ============================================================================

PRINT '=== FASE 1: Agregando nuevas columnas a USUARIOS ===';

-- Agregar columnas para datos personales y control de intentos
ALTER TABLE USUARIOS ADD (
    tipo VARCHAR(50) NULL,           -- 'Empleado', 'Entrenador', 'Cliente', 'Familiar'
    dni INT NULL,                     -- DNI único del usuario
    nombre VARCHAR(100) NULL,         -- Nombre completo
    apellido VARCHAR(100) NULL,       -- Apellido completo
    telefono VARCHAR(20) NULL,        -- Teléfono de contacto
    email VARCHAR(255) NULL,          -- Email de contacto
    fechaNacimiento DATE NULL,        -- Fecha de nacimiento
    bloqueado BIT NOT NULL DEFAULT 0, -- Bloqueado por intentos fallidos
    intentos INT NOT NULL DEFAULT 0   -- Intentos fallidos de login
);
GO

-- ============================================================================
-- FASE 2: MIGRAR DATOS DESDE ALUMNOS Y ENTRENADORES
-- ============================================================================

PRINT '=== FASE 2: Migrando datos desde tablas hijas ===';

-- Migrar datos de Clientes (rol = 4) desde ALUMNOS
PRINT 'Migrando Clientes (rol=4) desde ALUMNOS...';
UPDATE u
SET
    u.tipo = 'Cliente',
    u.dni = a.dni,
    u.nombre = a.nombre,
    u.apellido = a.apellido,
    u.telefono = a.telefono,
    u.fechaNacimiento = a.fechaNacimiento
FROM USUARIOS u
INNER JOIN ALUMNOS a ON u.usr = a.usr
WHERE u.rol = 4 AND u.tipo IS NULL;

-- Migrar datos de Entrenadores (rol = 3) desde ENTRENADORES
PRINT 'Migrando Entrenadores (rol=3) desde ENTRENADORES...';
UPDATE u
SET
    u.tipo = 'Entrenador',
    u.dni = e.dni,
    u.nombre = e.nombre,
    u.apellido = e.apellido,
    u.telefono = e.telefono,
    u.fechaNacimiento = e.fechaNacimiento
FROM USUARIOS u
INNER JOIN ENTRENADORES e ON u.usr = e.usr
WHERE u.rol = 3 AND u.tipo IS NULL;

-- Migrar datos de Administradores y Recepcionistas como 'Empleado'
PRINT 'Migrando Admin/Recepcionistas (rol=1,2) como Empleados...';
UPDATE u
SET
    u.tipo = 'Empleado',
    u.dni = 999999990 + u.rol,  -- DNI temporal placeholder
    u.nombre = 'Empleado',
    u.apellido = u.usr,
    u.fechaNacimiento = '1990-01-01',
    u.telefono = '0000-0000'
FROM USUARIOS u
WHERE u.rol IN (1, 2) AND u.tipo IS NULL;
GO

-- ============================================================================
-- FASE 3: MIGRAR INTENTOS DESDE USUARIO_Intentos
-- ============================================================================

PRINT '=== FASE 3: Migrando intentos desde USUARIO_Intentos ===';

UPDATE u
SET
    u.intentos = ISNULL(ui.intentos, 0),
    u.bloqueado = CASE WHEN ISNULL(ui.intentos, 0) >= 3 THEN 1 ELSE 0 END
FROM USUARIOS u
LEFT JOIN USUARIO_Intentos ui ON u.usr = ui.usr;
GO

-- ============================================================================
-- FASE 4: HACER COLUMNAS NOT NULL
-- ============================================================================

PRINT '=== FASE 4: Estableciendo constraints NOT NULL ===';

-- Primero, verificar que no haya NULLs
IF EXISTS (SELECT 1 FROM USUARIOS WHERE tipo IS NULL)
BEGIN
    RAISERROR('ERROR: Hay usuarios sin tipo migrado', 16, 1);
    RETURN;
END

IF EXISTS (SELECT 1 FROM USUARIOS WHERE dni IS NULL)
BEGIN
    RAISERROR('ERROR: Hay usuarios sin DNI migrado', 16, 1);
    RETURN;
END

IF EXISTS (SELECT 1 FROM USUARIOS WHERE nombre IS NULL)
BEGIN
    RAISERROR('ERROR: Hay usuarios sin nombre migrado', 16, 1);
    RETURN;
END

IF EXISTS (SELECT 1 FROM USUARIOS WHERE apellido IS NULL)
BEGIN
    RAISERROR('ERROR: Hay usuarios sin apellido migrado', 16, 1);
    RETURN;
END

-- Ahora hacer las columnas NOT NULL
ALTER TABLE USUARIOS ALTER COLUMN tipo VARCHAR(50) NOT NULL;
ALTER TABLE USUARIOS ALTER COLUMN dni INT NOT NULL;
ALTER TABLE USUARIOS ALTER COLUMN nombre VARCHAR(100) NOT NULL;
ALTER TABLE USUARIOS ALTER COLUMN apellido VARCHAR(100) NOT NULL;
GO

-- ============================================================================
-- FASE 5: AGREGAR CONSTRAINTS Y ÍNDICES
-- ============================================================================

PRINT '=== FASE 5: Agregando constraints y índices ===';

-- Unique constraint en DNI
IF NOT EXISTS (SELECT 1 FROM sys.objects WHERE name = 'UK_USUARIOS_DNI' AND type = 'UQ')
BEGIN
    ALTER TABLE USUARIOS ADD CONSTRAINT UK_USUARIOS_DNI UNIQUE (dni);
    PRINT 'Constraint UK_USUARIOS_DNI agregado';
END

-- Check constraint para Tipo
IF NOT EXISTS (SELECT 1 FROM sys.objects WHERE name = 'CK_USUARIOS_Tipo' AND type = 'C')
BEGIN
    ALTER TABLE USUARIOS ADD CONSTRAINT CK_USUARIOS_Tipo
        CHECK (tipo IN ('Empleado', 'Entrenador', 'Cliente', 'Familiar'));
    PRINT 'Constraint CK_USUARIOS_Tipo agregado';
END

-- Check constraint para Rol
IF NOT EXISTS (SELECT 1 FROM sys.objects WHERE name = 'CK_USUARIOS_Rol' AND type = 'C')
BEGIN
    ALTER TABLE USUARIOS ADD CONSTRAINT CK_USUARIOS_Rol
        CHECK (rol IN (1, 2, 3, 4));
    PRINT 'Constraint CK_USUARIOS_Rol agregado';
END

-- Índice en tipo para búsquedas rápidas
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_USUARIOS_Tipo')
BEGIN
    CREATE NONCLUSTERED INDEX IX_USUARIOS_Tipo ON USUARIOS(tipo);
    PRINT 'Índice IX_USUARIOS_Tipo creado';
END

-- Índice en dni para FK
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_USUARIOS_DNI')
BEGIN
    CREATE NONCLUSTERED INDEX IX_USUARIOS_DNI ON USUARIOS(dni);
    PRINT 'Índice IX_USUARIOS_DNI creado';
END
GO

-- ============================================================================
-- FASE 5.1: CREAR TABLA PreguntasSeguridad
-- ============================================================================

PRINT '=== FASE 5.1: Creando tabla PreguntasSeguridad ===';

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

    PRINT 'Tabla PreguntasSeguridad e índice IX_PreguntasSeguridad_usr creados';
END
ELSE
BEGIN
    PRINT 'Tabla PreguntasSeguridad ya existe';
END
GO

-- ============================================================================
-- FASE 6: MODIFICAR TABLA ALUMNOS (eliminar columnas migradas)
-- ============================================================================

PRINT '=== FASE 6: Modificando tabla ALUMNOS ===';

-- Eliminar columnas que ahora están en USUARIOS
-- Primero verificar qué columnas existen
IF COL_LENGTH('ALUMNOS', 'nombre') IS NOT NULL
BEGIN
    ALTER TABLE ALUMNOS DROP COLUMN nombre;
    PRINT 'Columna ALUMNOS.nombre eliminada';
END

IF COL_LENGTH('ALUMNOS', 'apellido') IS NOT NULL
BEGIN
    ALTER TABLE ALUMNOS DROP COLUMN apellido;
    PRINT 'Columna ALUMNOS.apellido eliminada';
END

IF COL_LENGTH('ALUMNOS', 'telefono') IS NOT NULL
BEGIN
    ALTER TABLE ALUMNOS DROP COLUMN telefono;
    PRINT 'Columna ALUMNOS.telefono eliminada';
END

IF COL_LENGTH('ALUMNOS', 'fechaNacimiento') IS NOT NULL
BEGIN
    ALTER TABLE ALUMNOS DROP COLUMN fechaNacimiento;
    PRINT 'Columna ALUMNOS.fechaNacimiento eliminada';
END

IF COL_LENGTH('ALUMNOS', 'usr') IS NOT NULL
BEGIN
    -- Eliminar FK primero
    IF EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = 'FK_Alumnos_Usuario')
    BEGIN
        ALTER TABLE ALUMNOS DROP CONSTRAINT FK_Alumnos_Usuario;
        PRINT 'FK FK_Alumnos_Usuario eliminada';
    END

    ALTER TABLE ALUMNOS DROP COLUMN usr;
    PRINT 'Columna ALUMNOS.usr eliminada';
END
GO

-- ============================================================================
-- FASE 7: MODIFICAR TABLA ENTRENADORES (eliminar columnas migradas)
-- ============================================================================

PRINT '=== FASE 7: Modificando tabla ENTRENADORES ===';

-- Eliminar columnas que ahora están en USUARIOS
IF COL_LENGTH('ENTRENADORES', 'nombre') IS NOT NULL
BEGIN
    ALTER TABLE ENTRENADORES DROP COLUMN nombre;
    PRINT 'Columna ENTRENADORES.nombre eliminada';
END

IF COL_LENGTH('ENTRENADORES', 'apellido') IS NOT NULL
BEGIN
    ALTER TABLE ENTRENADORES DROP COLUMN apellido;
    PRINT 'Columna ENTRENADORES.apellido eliminada';
END

IF COL_LENGTH('ENTRENADORES', 'telefono') IS NOT NULL
BEGIN
    ALTER TABLE ENTRENADORES DROP COLUMN telefono;
    PRINT 'Columna ENTRENADORES.telefono eliminada';
END

IF COL_LENGTH('ENTRENADORES', 'fechaNacimiento') IS NOT NULL
BEGIN
    ALTER TABLE ENTRENADORES DROP COLUMN fechaNacimiento;
    PRINT 'Columna ENTRENADORES.fechaNacimiento eliminada';
END

IF COL_LENGTH('ENTRENADORES', 'usr') IS NOT NULL
BEGIN
    -- Eliminar FK primero
    IF EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = 'FK_Entrenadores_Usuario')
    BEGIN
        ALTER TABLE ENTRENADORES DROP CONSTRAINT FK_Entrenadores_Usuario;
        PRINT 'FK FK_Entrenadores_Usuario eliminada';
    END

    ALTER TABLE ENTRENADORES DROP COLUMN usr;
    PRINT 'Columna ENTRENADORES.usr eliminada';
END
GO

-- ============================================================================
-- FASE 8: MARCAR TABLA USUARIO_Intentos COMO OBSOLETA
-- ============================================================================
-- NOTA: El código usa USUARIOS.intentos. No se elimina la tabla para permitir
--       un rollback limpio de esta migración, pero se deja vacía y documentada
--       como obsoleta.

PRINT '=== FASE 8: Marcando tabla USUARIO_Intentos como obsoleta ===';

-- Vaciar datos migrados para evitar duplicación/confusión
IF OBJECT_ID('USUARIO_Intentos', 'U') IS NOT NULL
BEGIN
    DELETE FROM USUARIO_Intentos;
    PRINT 'Tabla USUARIO_Intentos vaciada (obsoleta - usar USUARIOS.intentos)';
END
GO

-- ============================================================================
-- FASE 9: ACTUALIZAR VISTAS/PROCEDIMIENTOS QUE REFERENCIAN ESTAS TABLAS
-- ============================================================================

PRINT '=== FASE 9: Verificando dependencias ===';

-- Listar dependencias de USUARIOS
PRINT 'Dependencias de USUARIOS:';
SELECT
    OBJECT_NAME(referencing_id) AS ObjetoDependiente,
    o.type_desc AS Tipo
FROM sys.sql_expression_dependencies d
INNER JOIN sys.objects o ON d.referencing_id = o.object_id
WHERE referenced_entity_name = 'USUARIOS'
AND referenced_schema_name = 'dbo';

-- Listar dependencias de ALUMNOS
PRINT 'Dependencias de ALUMNOS:';
SELECT
    OBJECT_NAME(referencing_id) AS ObjetoDependiente,
    o.type_desc AS Tipo
FROM sys.sql_expression_dependencies d
INNER JOIN sys.objects o ON d.referencing_id = o.object_id
WHERE referenced_entity_name = 'ALUMNOS'
AND referenced_schema_name = 'dbo';

-- Listar dependencias de ENTRENADORES
PRINT 'Dependencias de ENTRENADORES:';
SELECT
    OBJECT_NAME(referencing_id) AS ObjetoDependiente,
    o.type_desc AS Tipo
FROM sys.sql_expression_dependencies d
INNER JOIN sys.objects o ON d.referencing_id = o.object_id
WHERE referenced_entity_name = 'ENTRENADORES'
AND referenced_schema_name = 'dbo';
GO

-- ============================================================================
-- FASE 10: VERIFICACIÓN DE DATOS MIGRADOS
-- ============================================================================

PRINT '=== FASE 10: Verificación de datos migrados ===';

-- Contar usuarios por tipo
PRINT 'Usuarios por tipo:';
SELECT tipo, COUNT(*) as Cantidad FROM USUARIOS GROUP BY tipo;

-- Verificar integridad de datos
PRINT 'Verificación de integridad:';
SELECT
    u.usr,
    u.tipo,
    u.dni,
    u.nombre,
    u.apellido,
    CASE WHEN a.dni IS NOT NULL THEN 'Sí' ELSE 'No' END as EsAlumno,
    CASE WHEN e.dni IS NOT NULL THEN 'Sí' ELSE 'No' END as EsEntrenador
FROM USUARIOS u
LEFT JOIN ALUMNOS a ON u.dni = a.dni
LEFT JOIN ENTRENADORES e ON u.dni = e.dni
ORDER BY u.tipo, u.usr;

-- Verificar que no haya DNI duplicados entre tablas
PRINT 'Verificando DNI duplicados (debe ser 0):';
SELECT dni, COUNT(*) as Cantidad
FROM USUARIOS
GROUP BY dni
HAVING COUNT(*) > 1;

PRINT '=== Migración completada ===';
GO

-- ============================================================================
-- SCRIPT DE ROLLBACK (SOLO EN CASO DE EMERGENCIA)
-- ============================================================================
/*
-- EJECUTAR SOLO SI SE NECESITA RESTAURAR EL ESTADO ANTERIOR

USE [GymApp];
GO

-- Restaurar tablas desde backup
IF OBJECT_ID('USUARIOS', 'U') IS NOT NULL DROP TABLE USUARIOS;
IF OBJECT_ID('ALUMNOS', 'U') IS NOT NULL DROP TABLE ALUMNOS;
IF OBJECT_ID('ENTRENADORES', 'U') IS NOT NULL DROP TABLE ENTRENADORES;

SELECT * INTO USUARIOS FROM USUARIOS_BACKUP;
SELECT * INTO ALUMNOS FROM ALUMNOS_BACKUP;
SELECT * INTO ENTRENADORES FROM ENTRENADORES_BACKUP;

-- Recrear FK eliminadas
ALTER TABLE ALUMNOS ADD CONSTRAINT FK_Alumnos_Usuario
    FOREIGN KEY (usr) REFERENCES USUARIOS(usr);

ALTER TABLE ENTRENADORES ADD CONSTRAINT FK_Entrenadores_Usuario
    FOREIGN KEY (usr) REFERENCES USUARIOS(usr);

-- Recrear tabla USUARIO_Intentos
CREATE TABLE USUARIO_Intentos (
    usr VARCHAR(50) NOT NULL,
    intentos INT NOT NULL,
    dvv VARCHAR(50) NOT NULL,
    dvh VARCHAR(50) NOT NULL,
    CONSTRAINT PK_USUARIO_Intentos PRIMARY KEY (usr),
    CONSTRAINT FK_UsuarioIntentos_Usuario FOREIGN KEY (usr) REFERENCES USUARIOS(usr)
);

PRINT 'ROLLBACK completado - Estado original restaurado';
*/
