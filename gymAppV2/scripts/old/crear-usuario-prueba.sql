-- ============================================================================
-- SCRIPT: crear-usuario-prueba.sql
-- Objetivo: Crear un usuario de prueba completo para validar los flujos de
--           seguridad: login, cuenta bloqueada, recuperación por pregunta de
--           seguridad, cambio de contraseña y primer login forzado.
--
-- Contraseña del usuario de prueba: Test123!
-- Hash SHA256 (ASCII): 54de7f606f2523cba8efac173fab42fb7f59d56ceff974c8fdb7342cf2cfe345
--
-- Pregunta de seguridad: "¿En qué año naciste?"
-- Respuesta: 1990
-- Fecha de nacimiento: 1990-05-15
--
-- IMPORTANTE: Ejecutar este script en la base de datos de la aplicación.
-- ============================================================================

SET NOCOUNT ON;
GO

DECLARE @usr        VARCHAR(50)  = 'testuser';
DECLARE @contraHash VARCHAR(255) = '54de7f606f2523cba8efac173fab42fb7f59d56ceff974c8fdb7342cf2cfe345';
DECLARE @dni        INT          = 99999999;
DECLARE @nombre     VARCHAR(100) = 'Usuario';
DECLARE @apellido   VARCHAR(100) = 'De Prueba';
DECLARE @email      VARCHAR(255) = 'testuser@gymapp.local';
DECLARE @telefono   VARCHAR(20)  = '5555555555';
DECLARE @fechaNac   DATE         = '1990-05-15';
DECLARE @tipo       VARCHAR(50)  = 'Cliente';
DECLARE @rol        INT          = 4;
DECLARE @dvv        VARCHAR(50)  = 'TEST-DVV';
DECLARE @dvh        VARCHAR(50)  = 'TEST-DVH';
DECLARE @peso       DECIMAL(5,2) = 75.00;

-- ============================================================================
-- 1. LIMPIEZA: eliminar registros previos del usuario de prueba (idempotente)
-- ============================================================================
DELETE FROM [dbo].[PreguntasSeguridad] WHERE [usr] = @usr;
DELETE FROM [dbo].[USUARIO_Contras]    WHERE [usr] = @usr;
DELETE FROM [dbo].[ALUMNOS]            WHERE [dni] = @dni;
DELETE FROM [dbo].[USUARIOS]           WHERE [usr] = @usr;
PRINT 'Registros anteriores de ' + @usr + ' eliminados (si existían).';
GO

-- ============================================================================
-- 2. CREAR USUARIO
-- ============================================================================
INSERT INTO [dbo].[USUARIOS]
(
    [usr],
    [contra],
    [activo],
    [bloqueado],
    [intentos],
    [tipo],
    [dni],
    [nombre],
    [apellido],
    [telefono],
    [email],
    [fechaNacimiento],
    [rol],
    [primerLogin],
    [dvv],
    [dvh]
)
VALUES
(
    @usr,
    @contraHash,
    1,          -- activo
    0,          -- bloqueado
    0,          -- intentos
    @tipo,
    @dni,
    @nombre,
    @apellido,
    @telefono,
    @email,
    @fechaNac,
    @rol,
    0,          -- primerLogin (cambiar a 1 para probar primer login forzado)
    @dvv,
    @dvh
);
PRINT 'Usuario ' + @usr + ' creado.';
GO

-- ============================================================================
-- 3. CREAR ALUMNO ASOCIADO
-- ============================================================================
INSERT INTO [dbo].[ALUMNOS]
(
    [dni],
    [peso],
    [tieneRutinas],
    [activo],
    [dvv],
    [dvh]
)
VALUES
(
    @dni,
    @peso,
    0,
    1,
    @dvv,
    @dvh
);
PRINT 'Alumno con DNI ' + CAST(@dni AS VARCHAR) + ' creado.';
GO

-- ============================================================================
-- 4. CREAR PREGUNTA DE SEGURIDAD (recuperación)
-- ============================================================================
INSERT INTO [dbo].[PreguntasSeguridad]
(
    [usr],
    [pregunta],
    [respuesta],
    [dvv],
    [dvh]
)
VALUES
(
    @usr,
    '¿En qué año naciste?',
    '1990',
    @dvv,
    @dvh
);
PRINT 'Pregunta de seguridad creada para ' + @usr + '.';
GO

-- ============================================================================
-- 5. GUARDAR CONTRASEÑA EN HISTORIAL
-- ============================================================================
INSERT INTO [dbo].[USUARIO_Contras]
(
    [usr],
    [contra],
    [dvv],
    [dvh]
)
VALUES
(
    @usr,
    @contraHash,
    @dvv,
    @dvh
);
PRINT 'Contraseña inicial guardada en historial.';
GO

-- ============================================================================
-- 6. VERIFICACIÓN
-- ============================================================================
SELECT
    u.[usr],
    u.[activo],
    u.[bloqueado],
    u.[intentos],
    u.[primerLogin],
    u.[fechaNacimiento],
    ps.[pregunta],
    ps.[respuesta],
    (SELECT COUNT(*) FROM [dbo].[USUARIO_Contras] WHERE [usr] = u.[usr]) AS [historialContras]
FROM [dbo].[USUARIOS] u
LEFT JOIN [dbo].[PreguntasSeguridad] ps ON ps.[usr] = u.[usr]
WHERE u.[usr] = @usr;
GO

PRINT '';
PRINT '==============================================================';
PRINT 'Usuario de prueba creado exitosamente.';
PRINT '==============================================================';
PRINT 'Usuario : testuser';
PRINT 'Password: Test123!';
PRINT 'DNI     : 99999999';
PRINT 'Pregunta: ¿En qué año naciste?';
PRINT 'Respuesta: 1990';
PRINT '==============================================================';
GO

/*
-- ============================================================================
-- ESCENARIOS DE TESTEO: descomentar según lo que se quiera probar
-- ============================================================================

-- Escenario A: Cuenta bloqueada (debe redirigir a PreguntasSeguridad.aspx)
UPDATE [dbo].[USUARIOS]
SET [bloqueado] = 1, [intentos] = 3
WHERE [usr] = 'testuser';
GO

-- Escenario B: Primer login forzado (debe pedir cambio de contraseña)
UPDATE [dbo].[USUARIOS]
SET [primerLogin] = 1, [bloqueado] = 0, [intentos] = 0
WHERE [usr] = 'testuser';
GO

-- Escenario C: Resetear a estado normal
UPDATE [dbo].[USUARIOS]
SET [activo] = 1, [bloqueado] = 0, [intentos] = 0, [primerLogin] = 0
WHERE [usr] = 'testuser';
GO

-- Escenario D: Borrar usuario de prueba por completo
DELETE FROM [dbo].[PreguntasSeguridad] WHERE [usr] = 'testuser';
DELETE FROM [dbo].[USUARIO_Contras]    WHERE [usr] = 'testuser';
DELETE FROM [dbo].[ALUMNOS]            WHERE [dni]  = 99999999;
DELETE FROM [dbo].[USUARIOS]           WHERE [usr] = 'testuser';
GO
*/
