# Commit con backup de SQL Server

Cuando el usuario pida hacer un commit, ejecutá los siguientes pasos en orden.

## Pasos

### 1. Backup de la base de datos SQL Server

El backup se genera primero en `C:\Backups\` (donde SQL Server tiene permisos) y luego se mueve a la carpeta del proyecto.

```powershell
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

# Ejecutar backup en C:\Backups\ (SQL Server tiene permisos ahí)
sqlcmd -S $instance -Q "BACKUP DATABASE [$dbName] TO DISK = N'$tempPath' WITH NOFORMAT, NOINIT, NAME = '${dbName} backup', SKIP, NOREWIND, NOUNLOAD, STATS = 10"

# Mover el .bak a la carpeta del proyecto
Move-Item -Path $tempPath -Destination $destPath
```

**Autenticación:**
- Windows (lo más común): no hace falta agregar nada, `sqlcmd` usa el usuario actual.
- SQL Server (usuario/contraseña): agregá `-U <usuario> -P <contraseña>` al comando.

**Si `sqlcmd` no está en el PATH**, usá la ruta completa:
```
"C:\Program Files\Microsoft SQL Server\<version>\Tools\Binn\sqlcmd.exe"
```

### 2. Git add

```powershell
git add .
```

> El `.bak` vive en `C:\Users\Danunu\Desktop\WEB-4to\backups\` y **no** se incluye en el repo por su tamaño.

### 3. Git commit

Usá el **mismo** `$timestamp` del paso 1:

```powershell
git commit -m "Tarea general $timestamp"
```

### 4. Git push

```powershell
git push
```

## Notas

- El backup se crea primero en `C:\Backups\` para evitar errores de permisos de SQL Server, y luego se mueve automáticamente a `C:\Users\Danunu\Desktop\WEB-4to\backups\`.
- El timestamp se genera una sola vez y se reutiliza en el nombre del `.bak` y en el mensaje del commit para que queden sincronizados.
- Si el backup falla, **no continuar** con el commit. Avisarle al usuario el error de `sqlcmd`.
- Si el `Move-Item` falla, avisarle al usuario y no continuar.
- Si `git push` falla por falta de remote, avisarle al usuario.