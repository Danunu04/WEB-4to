\# Commit con backup de SQL Server



Cuando el usuario pida hacer un commit, ejecutá los siguientes pasos en orden.



\## Pasos



\### 1. Backup de la base de datos SQL Server



Usá `sqlcmd` para generar el backup `.bak` directamente desde la instancia de SQL Server.



Si el nombre de la instancia o la base de datos no están definidos en el proyecto, preguntale al usuario antes de continuar.



```powershell

$timestamp = Get-Date -Format "yyyyMMdd\_HHmmss"

$dbName = "GymApp"

$instance = "."   # ej: localhost\\SQLEXPRESS

$backupPath = "C:\\Users\\Danunu\\Desktop\\WEB-4to\\backups\\${dbName}\_${timestamp}.bak"



\# Crear carpeta si no existe

New-Item -ItemType Directory -Force -Path "C:\\Backups" | Out-Null



\# Ejecutar backup

sqlcmd -S $instance -Q "BACKUP DATABASE \[$dbName] TO DISK = N'$backupPath' WITH NOFORMAT, NOINIT, NAME = '${dbName} backup', SKIP, NOREWIND, NOUNLOAD, STATS = 10"

```



\*\*Autenticación:\*\*

\- Windows (lo más común): no hace falta agregar nada, `sqlcmd` usa el usuario actual.

\- SQL Server (usuario/contraseña): agregá `-U <usuario> -P <contraseña>` al comando.



\*\*Si `sqlcmd` no está en el PATH\*\*, usá la ruta completa:

```

"C:\\Program Files\\Microsoft SQL Server\\<version>\\Tools\\Binn\\sqlcmd.exe"

```



\### 2. Git add



```powershell

git add .

```



> El `.bak` se guarda en `C:\\Backups\\` y generalmente \*\*no\*\* se incluye en el repo por su tamaño. Si el usuario quiere trackearlo, agregarlo explícitamente al `git add`.



\### 3. Git commit



Usá el \*\*mismo\*\* `$timestamp` del paso 1:



```powershell

git commit -m "Tarea general $timestamp"

```



\### 4. Git push



```powershell

git push

```



\## Notas



\- El timestamp se genera una sola vez y se reutiliza en el nombre del `.bak` y en el mensaje del commit para que queden sincronizados.

\- Si el backup falla, \*\*no continuar\*\* con el commit. Avisarle al usuario el error de `sqlcmd`.

\- Si `git push` falla por falta de remote, avisarle al usuario.

