# Plan: control de versión en backup/restore + export alternativa

## Problema confirmado

El usuario intentó restaurar un backup generado en SQL Server 2025 (database version 998) sobre SQL Server 2022 (database version 957). SQL Server rechaza esto porque no permite restaurar un backup en un servidor de versión menor a la de origen.

No existe opción de `BACKUP DATABASE` para generar un `.bak` con versión de servidor anterior.

## Solución propuesta

### 1. Detección y mensaje claro en restore (obligatorio)

En `MPP/MPPDigitoVerificador.cs`, método `RestaurarBackup`:

- Leer `RESTORE HEADERONLY` del `.bak`.
- Obtener `DatabaseVersion` del backup.
- Obtener `DatabaseVersion` del servidor destino con `SERVERPROPERTY('DatabaseVersion')`.
- Si `DatabaseVersion_backup > DatabaseVersion_servidor`, lanzar excepción con mensaje amigable:
  > "El backup fue creado en SQL Server {VersionBackupTexto} (versión interna {x}) y no puede restaurarse en este SQL Server {VersionServidorTexto} (versión interna {y}). Restaure en una instancia igual o posterior a {VersionBackupTexto}."

Esto evita que el usuario vea el error técnico de SMO/SqlClient.

### 2. Información de compatibilidad en backup

En `MPP/MPPDigitoVerificador.cs`, método `RealizarBackup`:

- Detectar la versión del servidor donde se ejecuta el backup.
- Devolver un mensaje informativo que la UI pueda mostrar indicando:
  > "Backup generado en SQL Server {versión}. Este archivo solo puede restaurarse en SQL Server {versión} o posterior."

Cambiar el retorno de `RealizarBackup` para que devuelva un string con el mensaje de compatibilidad sin romper la firma existente (se puede devolver string o agregar un `out`).

Opción elegida: mantener `void RealizarBackup` pero agregar un método `ObtenerInfoCompatibilidadBackup` que la UI consulte después de hacer el backup.

### 3. UI de backup/restore actualizada

En `gymAppV2/Admin/BackupRestore.aspx` y `.aspx.cs`:

- Mostrar la versión del servidor SQL Server actual.
- Mostrar la advertencia de compatibilidad justo después de realizar un backup.
- En restore, si el archivo es incompatible, mostrar el mensaje claro del punto 1.

### 4. Exportación portable como alternativa (opcional)

Agregar un botón secundario en la UI: **"Exportar scripts SQL"**. Esto generaría un `.sql` con el esquema y datos básicos usando `ScriptingOptions` de SMO o un script manual para tablas clave. Esto sí permite migrar de SQL 2025 a SQL 2022.

**Decisión**: implementar los puntos 1, 2 y 3 en esta iteración. El punto 4 se deja como opcional porque requiere SMO o generación manual y puede ser extenso; se pregunta al usuario al final si lo quiere.

## Archivos a modificar

- `MPP/MPPDigitoVerificador.cs`: lógica de validación de versión en restore y método de info de compatibilidad en backup.
- `BLL/BLLDigitoVerificador.cs`: exponer el método de info de compatibilidad si es necesario.
- `gymAppV2/Admin/BackupRestore.aspx`: agregar labels para versión del servidor y advertencia.
- `gymAppV2/Admin/BackupRestore.aspx.cs`: mostrar versión y advertencias.

## Nota sobre versiones

Mapeo de database version a versiones comerciales (aproximado):
- 957 = SQL Server 2022 (16.x)
- 998 = SQL Server 2025 (17.x)

Se usará `SERVERPROPERTY('ProductMajorVersion')` o `SERVERPROPERTY('ProductVersion')` para texto amigable, y `SERVERPROPERTY('DatabaseVersion')` para comparación numérica.
