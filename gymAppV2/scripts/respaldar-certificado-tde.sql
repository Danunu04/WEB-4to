USE [master]
GO

-- ============================================================================
-- Respaldo del certificado TDE (14/11/2026)
--
-- Respalda CertificadoTDE y su clave privada en la carpeta de backups de la
-- instancia, donde la cuenta del servicio de SQL Server ya tiene permiso de
-- escritura. Los archivos llevan fecha y hora en el nombre para no chocar con
-- respaldos anteriores.
--
-- En la misma corrida comprueba que los archivos se puedan abrir con la
-- contraseña: recrea el certificado en una base temporal (que se borra) y
-- compara la huella con la del certificado en uso.
--
-- Antes de correrlo: poner la contraseña en @Clave (una sola vez). No guardar
-- ni commitear el script con la contraseña real.
-- Después: copiar los dos archivos y la contraseña FUERA del servidor
-- (pendrive, nube, etc.). Sin ellos, los backups de GymApp no se pueden
-- restaurar en otro servidor.
--
-- Se puede correr las veces que haga falta.
-- ============================================================================

DECLARE @Clave NVARCHAR(128) = N'CAMBIAR-ClavePrivada-GymApp-2026!';

DECLARE @Carpeta NVARCHAR(400) = CAST(SERVERPROPERTY('InstanceDefaultBackupPath') AS NVARCHAR(400));
DECLARE @Sello NVARCHAR(20) = FORMAT(SYSDATETIME(), 'yyyyMMdd-HHmmss');
DECLARE @Cer NVARCHAR(500);
DECLARE @Pvk NVARCHAR(500);
DECLARE @ClaveSql NVARCHAR(300) = N'N''' + REPLACE(@Clave, N'''', N'''''') + N'''';
DECLARE @Sql NVARCHAR(MAX);

IF @Carpeta IS NULL
BEGIN
    RAISERROR('No se pudo obtener la carpeta de backups de la instancia.', 16, 1);
    RETURN;
END

IF RIGHT(@Carpeta, 1) <> '\' SET @Carpeta = @Carpeta + '\';

SET @Cer = @Carpeta + 'CertificadoTDE-' + @Sello + '.cer';
SET @Pvk = @Carpeta + 'CertificadoTDE-' + @Sello + '.pvk';

-- 1. Respaldo
SET @Sql = N'BACKUP CERTIFICATE CertificadoTDE
    TO FILE = ''' + REPLACE(@Cer, '''', '''''') + N'''
    WITH PRIVATE KEY (
        FILE = ''' + REPLACE(@Pvk, '''', '''''') + N''',
        ENCRYPTION BY PASSWORD = ' + @ClaveSql + N'
    );';

EXEC sp_executesql @Sql;

-- 2. Comprobación en una base temporal
IF DB_ID('PruebaCertTDE') IS NOT NULL DROP DATABASE [PruebaCertTDE];
CREATE DATABASE [PruebaCertTDE];

SET @Sql = N'USE [PruebaCertTDE];
    CREATE MASTER KEY ENCRYPTION BY PASSWORD = ''Prueba-Temporal-2026!'';
    CREATE CERTIFICATE CertificadoTDE_Prueba
        FROM FILE = ''' + REPLACE(@Cer, '''', '''''') + N'''
        WITH PRIVATE KEY (
            FILE = ''' + REPLACE(@Pvk, '''', '''''') + N''',
            DECRYPTION BY PASSWORD = ' + @ClaveSql + N'
        );';

BEGIN TRY
    EXEC sp_executesql @Sql;

    SELECT @Cer AS certificado,
           @Pvk AS clave_privada,
           CASE WHEN p.thumbprint = m.thumbprint
                THEN 'OK: los archivos abren con la contraseña'
                ELSE 'ERROR: la huella no coincide'
           END AS comprobacion
    FROM master.sys.certificates m
    CROSS JOIN PruebaCertTDE.sys.certificates p
    WHERE m.name = 'CertificadoTDE' AND p.name = 'CertificadoTDE_Prueba';
END TRY
BEGIN CATCH
    DECLARE @Error NVARCHAR(2048) = ERROR_MESSAGE();
    RAISERROR('El respaldo se hizo pero NO se pudo abrir con la contraseña: %s', 16, 1, @Error);
END CATCH

USE [master];
DROP DATABASE [PruebaCertTDE];
GO
