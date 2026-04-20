$timestamp = Get-Date -Format "yyyyMMdd_HHmmss"
$dbName = "GymApp"
$instance = "./MSSQLSERVER"
$tempFolder = "C:\Backups"
$tempPath = "${tempFolder}\${dbName}_${timestamp}.bak"
$destFolder = "C:\Users\Danunu\Desktop\WEB-4to\backups"
$destPath = "${destFolder}\${dbName}_${timestamp}.bak"

# Crear carpetas si no existen
New-Item -ItemType Directory -Force -Path $tempFolder | Out-Null
New-Item -ItemType Directory -Force -Path $destFolder | Out-Null

# Ejecutar backup
sqlcmd -S $instance -Q "BACKUP DATABASE [$dbName] TO DISK = N'$tempPath' WITH NOFORMAT, NOINIT, NAME = '${dbName} backup', SKIP, NOREWIND, NOUNLOAD, STATS = 10"

# Mover el .bak a la carpeta del proyecto
Move-Item -Path $tempPath -Destination $destPath

Write-Output $timestamp