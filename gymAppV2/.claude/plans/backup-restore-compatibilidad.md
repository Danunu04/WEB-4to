# Plan: compatibilidad de Backup/Restore con SQL Server 2022/2025

## Contexto

El sistema actual tiene backup/restore implementado en `MPP/MPPDigitoVerificador.cs` (`RealizarBackup` y `RestaurarBackup`). El usuario requiere que estas operaciones sean compatibles con SQL Server 2022 y la versión más reciente (SQL Server 2025, referida informalmente como "2026").

## Problemas identificados en la implementación actual

1. **Backup**: usa `WITH INIT` únicamente, sin `FORMAT`, `COPY_ONLY` ni `CHECKSUM`. Esto puede generar media sets extraños y no verifica integridad de páginas.
2. **Restore**: ejecuta `ALTER DATABASE ... SET SINGLE_USER`, `RESTORE DATABASE` y `ALTER DATABASE ... SET MULTI_USER` en un solo batch. Si el `RESTORE` falla, la base queda en `SINGLE_USER`.
3. **Restore**: no verifica si la base destino existe antes de ponerla en `SINGLE_USER`.
4. **Restore**: asume exactamente 2 archivos (datos y log) y los mapea a `InstanceDefaultDataPath`/`InstanceDefaultLogPath`. No maneja bases con archivos adicionales ni reutiliza las rutas actuales de la base existente.
5. **Restore**: no valida compatibilidad de versiones. Un backup creado en SQL Server 2025 no puede restaurarse en SQL Server 2022; es mejor detectar esto antes y dar un mensaje claro.

## Enfoque

Modificar únicamente `MPP/MPPDigitoVerificador.cs` para:

- Generar backups más estándar y compatibles.
- Hacer el restore más robusto y seguro, garantizando que la base vuelva a `MULTI_USER` incluso si falla.
- Detectar incompatibilidad de versión del backup vs. servidor antes de intentar el restore.
- Reutilizar las rutas físicas actuales de la base cuando existe, y usar las rutas por defecto de la instancia solo cuando no existe.

## Cambios propuestos

### 1. `RealizarBackup`

Cambiar la sentencia a:

```sql
BACKUP DATABASE [nombre]
TO DISK = @Ruta
WITH FORMAT, COPY_ONLY, CHECKSUM, INIT, STATS = 10;
```

Beneficios:
- `FORMAT`: crea un media set limpio (evita conflictos con backups previos en el mismo archivo).
- `COPY_ONLY`: no rompe la cadena de backups diferenciales/log del servidor.
- `CHECKSUM`: verifica integridad de páginas; compatible desde SQL 2005.
- `INIT` + `FORMAT`: reescribe el archivo si existe.
- `STATS = 10`: progreso visible si se ejecuta desde SQL Server logs/agentes.

### 2. Métodos auxiliares nuevos

- `ObtenerVersionServidor(SqlConnection)`: devuelve `SERVERPROPERTY('ProductMajorVersion')` como `int`.
- `ObtenerInfoBackup(SqlConnection, string ruta)`: ejecuta `RESTORE HEADERONLY FROM DISK = @Ruta` y devuelve `DataTable`.
- `ObtenerArchivosBackup(SqlConnection, string ruta)`: ejecuta `RESTORE FILELISTONLY FROM DISK = @Ruta`.
- `BaseDatosExiste(SqlConnection, string nombreBase)`: consulta `sys.databases`.
- `ObtenerArchivosBaseDatos(SqlConnection, string nombreBase)`: devuelve lista de archivos lógicos/físicos actuales de la base destino (para reutilizar rutas).
- `ObtenerRutasDefault(SqlConnection)`: devuelve `InstanceDefaultDataPath` / `InstanceDefaultLogPath`.
- `MapearArchivosRestore`: arma la cláusula `MOVE` dinámicamente según archivos del backup y archivos/rutas destino.
- `EjecutarNonQueryMaster`: helper para ejecutar comandos con parámetros en `master`.

### 3. `RestaurarBackup` refactorizado

Pasos:

1. Validar que el archivo `.bak` exista.
2. Abrir conexión a `master`.
3. Leer `RESTORE HEADERONLY` y validar que `SoftwareVersionMajor` del backup sea `<=` la versión mayor del servidor.
   - Si no, lanzar excepción clara: "El backup fue creado en SQL Server XX y no puede restaurarse en SQL Server YY."
4. Leer `RESTORE FILELISTONLY` para obtener todos los archivos lógicos del backup.
5. Determinar rutas destino:
   - Si la base destino existe, leer `sys.database_files` dentro de ella y mapear archivos por tipo (`D`/`L`).
   - Si no existe, usar `InstanceDefaultDataPath` / `InstanceDefaultLogPath`.
   - Si no hay rutas por defecto, lanzar excepción con instrucciones claras.
6. Poner la base en `SINGLE_USER WITH ROLLBACK IMMEDIATE` solo si existe.
7. Ejecutar `RESTORE DATABASE [...] FROM DISK = @Ruta WITH MOVE... REPLACE, RECOVERY, STATS = 10`.
8. En `finally`, ejecutar `ALTER DATABASE [...] SET MULTI_USER` (con `try/catch` interno) para garantizar que la base no quede bloqueada.

### 4. Consideraciones de seguridad

- No se cambian permisos; el restore sigue requiriendo `sysadmin` o `dbcreator`.
- Se mantiene `SqlConnection.ClearAllPools()` antes del restore para evitar conexiones abiertas desde el pool de ADO.NET.
- No se modifica `BLLDigitoVerificador.cs` ni las páginas `.aspx`/`.aspx.cs`; los cambios se propagan automáticamente porque ambas llaman a `RestaurarBackup`.

## Archivos a modificar

- `MPP/MPPDigitoVerificador.cs`: único archivo con cambios de código.

## Verificación

- Compilar la solución con `msbuild Gym-APP/Gym-APP.sln`.
- Revisar que no haya referencias rotas ni métodos duplicados.
- (No se puede probar restore real sin instancias de SQL Server 2022/2025, pero la lógica de validación de versión y estructura estará cubierta.)

## Nota sobre "SQL 2026"

No existe "SQL Server 2026". La versión comercial más reciente es SQL Server 2025 (a veces referida informalmente por el año de uso). El plan asume que el objetivo es **SQL Server 2022 y SQL Server 2025**. Si el usuario se refería a otra versión, se ajustará antes de implementar.
