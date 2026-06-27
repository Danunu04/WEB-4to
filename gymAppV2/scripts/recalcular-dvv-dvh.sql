-- ============================================================================
-- Migración: recalcular DVH/DVV de filas existentes
-- ============================================================================
-- Este script usa HASHBYTES('SHA2_256') para recalcular los dígitos verificadores
-- de las tablas que no contienen campos encriptados con AES.
--
-- Tablas EXCLUIDAS de este script (requieren desencriptación previa):
--   - USUARIOS       (nombre, apellido, teléfono, email, fechaNacimiento encriptados)
--   - PreguntasSeguridad (pregunta, respuesta encriptadas)
--
-- Para esas tablas usar la utilidad de recálculo desde la aplicación o ejecutar
-- operaciones de lectura/escritura que disparen el cálculo en C#.
--
-- Algoritmo:
--   DVH  = SHA-256(concatenación de campos con separador '|')
--   DVV  = SHA-256(concatenación de SHA-256 individuales de cada campo)
-- ============================================================================

USE [GymApp];
GO

-- Función auxiliar: normaliza un valor para el hash.
-- NULL -> 'NULL', booleano -> '1'/'0', numérico/fecha -> texto invariante.
CREATE OR ALTER FUNCTION dbo.fn_NormalizarDV(@valor sql_variant)
RETURNS VARCHAR(MAX)
AS
BEGIN
    IF @valor IS NULL
        RETURN 'NULL';

    DECLARE @tipo SYSNAME = SQL_VARIANT_PROPERTY(@valor, 'BaseType');

    IF @tipo IN ('bit')
        RETURN CASE WHEN CAST(@valor AS BIT) = 1 THEN '1' ELSE '0' END;

    IF @tipo IN ('decimal', 'numeric', 'float', 'real', 'money', 'smallmoney')
        RETURN REPLACE(CONVERT(VARCHAR(MAX), CAST(@valor AS DECIMAL(38,18)), 128), ',', '.');

    IF @tipo IN ('datetime', 'datetime2', 'smalldatetime', 'date')
        RETURN FORMAT(CAST(@valor AS DATETIME2), 'yyyy-MM-dd HH:mm:ss');

    RETURN CONVERT(VARCHAR(MAX), @valor);
END;
GO

-- Función: calcula SHA-256 de un texto y devuelve hex minúsculas.
CREATE OR ALTER FUNCTION dbo.fn_HashDV(@texto VARCHAR(MAX))
RETURNS VARCHAR(64)
AS
BEGIN
    RETURN LOWER(CONVERT(VARCHAR(64), HASHBYTES('SHA2_256', @texto), 2));
END;
GO

-- ============================================================================
-- USUARIO_Contras
-- ============================================================================
UPDATE [dbo].[USUARIO_Contras]
SET
    dvh = dbo.fn_HashDV(
        ISNULL(dbo.fn_NormalizarDV(CAST([usr] AS SQL_VARIANT)), 'NULL') + '|' +
        ISNULL(dbo.fn_NormalizarDV(CAST([contra] AS SQL_VARIANT)), 'NULL')
    ),
    dvv = dbo.fn_HashDV(
        dbo.fn_HashDV(ISNULL(dbo.fn_NormalizarDV(CAST([usr] AS SQL_VARIANT)), 'NULL')) +
        dbo.fn_HashDV(ISNULL(dbo.fn_NormalizarDV(CAST([contra] AS SQL_VARIANT)), 'NULL'))
    );
GO

-- ============================================================================
-- ALUMNOS
-- ============================================================================
UPDATE [dbo].[Alumnos]
SET
    dvh = dbo.fn_HashDV(
        ISNULL(dbo.fn_NormalizarDV(CAST([dni] AS SQL_VARIANT)), 'NULL') + '|' +
        ISNULL(dbo.fn_NormalizarDV(CAST([peso] AS SQL_VARIANT)), 'NULL') + '|' +
        ISNULL(dbo.fn_NormalizarDV(CAST([activo] AS SQL_VARIANT)), 'NULL') + '|' +
        ISNULL(dbo.fn_NormalizarDV(CAST([tieneRutinas] AS SQL_VARIANT)), 'NULL') + '|' +
        ISNULL(dbo.fn_NormalizarDV(CAST([usr] AS SQL_VARIANT)), 'NULL')
    ),
    dvv = dbo.fn_HashDV(
        dbo.fn_HashDV(ISNULL(dbo.fn_NormalizarDV(CAST([dni] AS SQL_VARIANT)), 'NULL')) +
        dbo.fn_HashDV(ISNULL(dbo.fn_NormalizarDV(CAST([peso] AS SQL_VARIANT)), 'NULL')) +
        dbo.fn_HashDV(ISNULL(dbo.fn_NormalizarDV(CAST([activo] AS SQL_VARIANT)), 'NULL')) +
        dbo.fn_HashDV(ISNULL(dbo.fn_NormalizarDV(CAST([tieneRutinas] AS SQL_VARIANT)), 'NULL')) +
        dbo.fn_HashDV(ISNULL(dbo.fn_NormalizarDV(CAST([usr] AS SQL_VARIANT)), 'NULL'))
    );
GO

-- ============================================================================
-- ENTRENADORES
-- ============================================================================
UPDATE [dbo].[Entrenadores]
SET
    dvh = dbo.fn_HashDV(
        ISNULL(dbo.fn_NormalizarDV(CAST([dni] AS SQL_VARIANT)), 'NULL') + '|' +
        ISNULL(dbo.fn_NormalizarDV(CAST([alumnosCount] AS SQL_VARIANT)), 'NULL') + '|' +
        ISNULL(dbo.fn_NormalizarDV(CAST([activo] AS SQL_VARIANT)), 'NULL') + '|' +
        ISNULL(dbo.fn_NormalizarDV(CAST([usr] AS SQL_VARIANT)), 'NULL')
    ),
    dvv = dbo.fn_HashDV(
        dbo.fn_HashDV(ISNULL(dbo.fn_NormalizarDV(CAST([dni] AS SQL_VARIANT)), 'NULL')) +
        dbo.fn_HashDV(ISNULL(dbo.fn_NormalizarDV(CAST([alumnosCount] AS SQL_VARIANT)), 'NULL')) +
        dbo.fn_HashDV(ISNULL(dbo.fn_NormalizarDV(CAST([activo] AS SQL_VARIANT)), 'NULL')) +
        dbo.fn_HashDV(ISNULL(dbo.fn_NormalizarDV(CAST([usr] AS SQL_VARIANT)), 'NULL'))
    );
GO

-- ============================================================================
-- Evento
-- ============================================================================
UPDATE [dbo].[Evento]
SET
    dvh = dbo.fn_HashDV(
        ISNULL(dbo.fn_NormalizarDV(CAST([tipo] AS SQL_VARIANT)), 'NULL') + '|' +
        ISNULL(dbo.fn_NormalizarDV(CAST([usr] AS SQL_VARIANT)), 'NULL') + '|' +
        ISNULL(dbo.fn_NormalizarDV(CAST([descripcion] AS SQL_VARIANT)), 'NULL') + '|' +
        ISNULL(dbo.fn_NormalizarDV(CAST([fecha] AS SQL_VARIANT)), 'NULL') + '|' +
        ISNULL(dbo.fn_NormalizarDV(CAST([criticidad] AS SQL_VARIANT)), 'NULL') + '|' +
        ISNULL(dbo.fn_NormalizarDV(CAST([modulo] AS SQL_VARIANT)), 'NULL')
    ),
    dvv = dbo.fn_HashDV(
        dbo.fn_HashDV(ISNULL(dbo.fn_NormalizarDV(CAST([tipo] AS SQL_VARIANT)), 'NULL')) +
        dbo.fn_HashDV(ISNULL(dbo.fn_NormalizarDV(CAST([usr] AS SQL_VARIANT)), 'NULL')) +
        dbo.fn_HashDV(ISNULL(dbo.fn_NormalizarDV(CAST([descripcion] AS SQL_VARIANT)), 'NULL')) +
        dbo.fn_HashDV(ISNULL(dbo.fn_NormalizarDV(CAST([fecha] AS SQL_VARIANT)), 'NULL')) +
        dbo.fn_HashDV(ISNULL(dbo.fn_NormalizarDV(CAST([criticidad] AS SQL_VARIANT)), 'NULL')) +
        dbo.fn_HashDV(ISNULL(dbo.fn_NormalizarDV(CAST([modulo] AS SQL_VARIANT)), 'NULL'))
    );
GO

-- ============================================================================
-- PrecioModalidad
-- ============================================================================
UPDATE [dbo].[PrecioModalidad]
SET
    dvh = dbo.fn_HashDV(
        ISNULL(dbo.fn_NormalizarDV(CAST([Id] AS SQL_VARIANT)), 'NULL') + '|' +
        ISNULL(dbo.fn_NormalizarDV(CAST([DiasPorSemana] AS SQL_VARIANT)), 'NULL') + '|' +
        ISNULL(dbo.fn_NormalizarDV(CAST([EsDiario] AS SQL_VARIANT)), 'NULL') + '|' +
        ISNULL(dbo.fn_NormalizarDV(CAST([Precio] AS SQL_VARIANT)), 'NULL') + '|' +
        ISNULL(dbo.fn_NormalizarDV(CAST([Activo] AS SQL_VARIANT)), 'NULL') + '|' +
        ISNULL(dbo.fn_NormalizarDV(CAST([FechaModificacion] AS SQL_VARIANT)), 'NULL')
    ),
    dvv = dbo.fn_HashDV(
        dbo.fn_HashDV(ISNULL(dbo.fn_NormalizarDV(CAST([Id] AS SQL_VARIANT)), 'NULL')) +
        dbo.fn_HashDV(ISNULL(dbo.fn_NormalizarDV(CAST([DiasPorSemana] AS SQL_VARIANT)), 'NULL')) +
        dbo.fn_HashDV(ISNULL(dbo.fn_NormalizarDV(CAST([EsDiario] AS SQL_VARIANT)), 'NULL')) +
        dbo.fn_HashDV(ISNULL(dbo.fn_NormalizarDV(CAST([Precio] AS SQL_VARIANT)), 'NULL')) +
        dbo.fn_HashDV(ISNULL(dbo.fn_NormalizarDV(CAST([Activo] AS SQL_VARIANT)), 'NULL')) +
        dbo.fn_HashDV(ISNULL(dbo.fn_NormalizarDV(CAST([FechaModificacion] AS SQL_VARIANT)), 'NULL'))
    );
GO

PRINT 'Recálculo DVH/DVV completado para tablas no encriptadas.';
GO

-- ============================================================================
-- Limpieza opcional de funciones auxiliares
-- ============================================================================
-- DROP FUNCTION IF EXISTS dbo.fn_HashDV;
-- DROP FUNCTION IF EXISTS dbo.fn_NormalizarDV;
GO
