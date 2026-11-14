USE [master]
GO

-- ============================================================================
-- TDE: encriptación de toda la base (14/11/2026)
--
-- Encripta en disco los archivos de GymApp (.mdf, .ldf) y los backups (.bak).
-- Las consultas de la aplicación no cambian: SQL Server desencripta en memoria.
-- Se suma a la encriptación por campo que ya hace la app (AES-256 en datos
-- personales y SHA-256 en contraseñas), no la reemplaza.
--
-- Requisitos:
--   - SQL Server 2019 o posterior en edición Standard, Enterprise o Developer
--     (antes de 2019, solo Enterprise/Developer). Express NO tiene TDE.
--   - Correrlo con un usuario sysadmin.
--   - El certificado se respalda en la carpeta de backups de la instancia
--     (SERVERPROPERTY('InstanceDefaultBackupPath')), donde el servicio de SQL
--     Server ya puede escribir. Si el respaldo falla, el script se detiene antes
--     de activar la encriptación.
--
-- Antes de correrlo:
--   1. Hacer un backup (scripts/backup-base.sql).
--   2. Cambiar las dos contraseñas marcadas con "CAMBIAR".
--
-- Después de correrlo:
--   - Guardar los archivos CertificadoTDE-<fecha>.cer y .pvk (la ruta sale en el
--     paso 3), y la contraseña de la clave privada, fuera del servidor. Sin ellos, si se
--     pierde el servidor, los backups NO se pueden restaurar en ningún lado.
--   - Revisar el estado con la consulta del final (paso 6).
--
-- El script se puede correr más de una vez: lo que ya existe no se vuelve a crear.
-- ============================================================================

-- 0. Verificar que la edición soporte TDE
DECLARE @Version INT = CAST(PARSENAME(CAST(SERVERPROPERTY('ProductVersion') AS NVARCHAR(50)), 4) AS INT);
DECLARE @Motor INT = CAST(SERVERPROPERTY('EngineEdition') AS INT); -- 2 = Standard, 3 = Enterprise/Developer, 4 = Express

IF NOT (@Motor = 3 OR (@Motor = 2 AND @Version >= 15))
BEGIN
    RAISERROR('Esta edición de SQL Server no soporta TDE (ver SERVERPROPERTY(''Edition'')). No se hizo ningún cambio.', 16, 1);
    SET NOEXEC ON;
END
GO

-- 1. Clave maestra en master (DMK)
IF NOT EXISTS (SELECT 1 FROM sys.symmetric_keys WHERE name = '##MS_DatabaseMasterKey##')
    CREATE MASTER KEY ENCRYPTION BY PASSWORD = 'CAMBIAR-ClaveMaestra-GymApp-2026!';
GO

-- 2. Certificado que protege la clave de la base
IF NOT EXISTS (SELECT 1 FROM sys.certificates WHERE name = 'CertificadoTDE')
    CREATE CERTIFICATE CertificadoTDE
        WITH SUBJECT = 'Certificado TDE de GymApp',
             EXPIRY_DATE = '2036-12-31';
GO

-- 3. Respaldo del certificado y de su clave privada (OBLIGATORIO)
--    Si falla, no se activa la encriptación (SET NOEXEC ON).
--    Para respaldarlo de nuevo más adelante: scripts/respaldar-certificado-tde.sql
BEGIN TRY
    DECLARE @Carpeta NVARCHAR(400) = CAST(SERVERPROPERTY('InstanceDefaultBackupPath') AS NVARCHAR(400));
    DECLARE @Sello NVARCHAR(20) = FORMAT(SYSDATETIME(), 'yyyyMMdd-HHmmss');
    DECLARE @Cer NVARCHAR(500), @Pvk NVARCHAR(500), @Sql NVARCHAR(MAX);

    IF RIGHT(@Carpeta, 1) <> '\' SET @Carpeta = @Carpeta + '\';
    SET @Cer = @Carpeta + 'CertificadoTDE-' + @Sello + '.cer';
    SET @Pvk = @Carpeta + 'CertificadoTDE-' + @Sello + '.pvk';

    SET @Sql = N'BACKUP CERTIFICATE CertificadoTDE
        TO FILE = ''' + REPLACE(@Cer, '''', '''''') + N'''
        WITH PRIVATE KEY (
            FILE = ''' + REPLACE(@Pvk, '''', '''''') + N''',
            ENCRYPTION BY PASSWORD = ''CAMBIAR-ClavePrivada-GymApp-2026!''
        );';

    EXEC sp_executesql @Sql;

    SELECT @Cer AS certificado, @Pvk AS clave_privada;
END TRY
BEGIN CATCH
    DECLARE @Error NVARCHAR(2048) = ERROR_MESSAGE();
    RAISERROR('No se pudo respaldar el certificado; NO se activó la encriptación. %s', 16, 1, @Error);
    SET NOEXEC ON;
END CATCH
GO

-- 4. Clave de encriptación de la base (DEK)
USE [GymApp]
GO

IF NOT EXISTS (SELECT 1 FROM sys.dm_database_encryption_keys WHERE database_id = DB_ID('GymApp'))
    CREATE DATABASE ENCRYPTION KEY
        WITH ALGORITHM = AES_256
        ENCRYPTION BY SERVER CERTIFICATE CertificadoTDE;
GO

-- 5. Activar la encriptación (corre en segundo plano)
USE [master]
GO

ALTER DATABASE [GymApp] SET ENCRYPTION ON;
GO

SET NOEXEC OFF;
GO

-- 6. Estado: encryption_state 2 = encriptando, 3 = encriptada
SELECT DB_NAME(database_id) AS base,
       encryption_state,
       CASE encryption_state
           WHEN 0 THEN 'Sin clave'
           WHEN 1 THEN 'Sin encriptar'
           WHEN 2 THEN 'Encriptando...'
           WHEN 3 THEN 'Encriptada'
           WHEN 4 THEN 'Cambiando clave'
           WHEN 5 THEN 'Desencriptando...'
           WHEN 6 THEN 'Cambiando protección'
       END AS estado,
       percent_complete AS porcentaje,
       key_algorithm,
       key_length
FROM sys.dm_database_encryption_keys;
GO

-- ============================================================================
-- Desactivar TDE (solo si hace falta volver atrás):
--
--   ALTER DATABASE [GymApp] SET ENCRYPTION OFF;
--   -- esperar a que encryption_state sea 1 y después:
--   USE [GymApp]; DROP DATABASE ENCRYPTION KEY;
--
-- No borrar el certificado mientras existan backups hechos con TDE activo.
--
-- Restaurar un backup en OTRO servidor: primero crear la clave maestra y
-- después el certificado desde los archivos respaldados:
--
--   USE master;
--   CREATE MASTER KEY ENCRYPTION BY PASSWORD = '...';
--   CREATE CERTIFICATE CertificadoTDE
--       FROM FILE = '<ruta>\CertificadoTDE-<fecha>.cer'
--       WITH PRIVATE KEY (FILE = '<ruta>\CertificadoTDE-<fecha>.pvk',
--                         DECRYPTION BY PASSWORD = 'CAMBIAR-ClavePrivada-GymApp-2026!');
-- ============================================================================
