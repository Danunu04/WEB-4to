USE [master]
GO

-- ============================================================================
-- Pruebas de TDE (14/11/2026)
--
-- 1. Estado de la encriptación.
-- 2. Ruta del último backup de GymApp (para buscar texto adentro con PowerShell).
--
-- La comprobación del certificado respaldado (.cer/.pvk + contraseña) la hace
-- scripts/respaldar-certificado-tde.sql en la misma corrida del respaldo.
-- ============================================================================

-- 1. Estado: GymApp tiene que estar en 3 (Encriptada) con AES 256
SELECT DB_NAME(database_id) AS base, encryption_state, key_algorithm, key_length
FROM sys.dm_database_encryption_keys
WHERE database_id = DB_ID('GymApp');
GO

-- 2. Último backup de GymApp
SELECT TOP 1 bs.backup_finish_date, bmf.physical_device_name
FROM msdb.dbo.backupset bs
JOIN msdb.dbo.backupmediafamily bmf ON bmf.media_set_id = bs.media_set_id
WHERE bs.database_name = 'GymApp'
ORDER BY bs.backup_finish_date DESC;
GO
