$timestamp = Get-Date -Format "yyyyMMdd_HHmmss"
$dbName = "GymApp"
$instance = "."
$backupPath = "C:\Backups\${dbName}_${timestamp}.bak"

# Crear carpeta si no existe
New-Item -ItemType Directory -Force -Path "C:\Backups" | Out-Null

# Ejecutar backup
sqlcmd -S $instance -Q "BACKUP DATABASE [$dbName] TO DISK = N'$backupPath' WITH NOFORMAT, NOINIT, NAME = '${dbName} backup', SKIP, NOREWIND, NOUNLOAD, STATS = 10"

Write-Output "Backup completado: $backupPath"