USE [master]
GO

-- ============================================================
-- Backup completo de GymApp a un archivo .bak con fecha y hora en el nombre
-- (ej. GymApp_2026-10-05_1830.bak), en la carpeta de backups por defecto de SQL Server.
--
-- Es COPY_ONLY: no altera la cadena de backups que haga la pantalla Backup/Restore.
-- Al final verifica que el archivo se pueda restaurar.
--
-- Para guardarlo en otra carpeta, poner la ruta en @Carpeta (ej. N'C:\Backups').
-- La carpeta tiene que existir y el servicio de SQL Server tiene que poder escribir en ella.
--
-- Para restaurarlo: pantalla Backup/Restore de la app, o en SSMS
--   RESTORE DATABASE [GymApp] FROM DISK = N'<ruta que muestra este script>' WITH REPLACE;
-- ============================================================

DECLARE @Carpeta NVARCHAR(260) = NULL;   -- NULL = carpeta de backups por defecto

IF @Carpeta IS NULL
    SET @Carpeta = CAST(SERVERPROPERTY('InstanceDefaultBackupPath') AS NVARCHAR(260));

-- Versiones viejas de SQL Server no tienen esa propiedad: se lee del registro
IF @Carpeta IS NULL
    EXEC master.dbo.xp_instance_regread
        N'HKEY_LOCAL_MACHINE',
        N'Software\Microsoft\MSSQLServer\MSSQLServer',
        N'BackupDirectory',
        @Carpeta OUTPUT;

IF @Carpeta IS NULL
BEGIN
    RAISERROR('No se encontró la carpeta de backups por defecto. Poné una ruta en @Carpeta.', 16, 1);
    RETURN;
END

DECLARE @Ruta NVARCHAR(400) =
    @Carpeta
    + CASE WHEN RIGHT(@Carpeta, 1) = '\' THEN '' ELSE '\' END
    + N'GymApp_' + FORMAT(GETDATE(), 'yyyy-MM-dd_HHmm') + N'.bak';

PRINT 'Haciendo backup en: ' + @Ruta;

BACKUP DATABASE [GymApp]
    TO DISK = @Ruta
    WITH COPY_ONLY, INIT, CHECKSUM, STATS = 10,
         NAME = N'GymApp - backup manual';

RESTORE VERIFYONLY FROM DISK = @Ruta WITH CHECKSUM;

PRINT 'Backup terminado y verificado: ' + @Ruta;
GO
