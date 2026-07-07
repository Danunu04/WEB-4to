# Plan: botón "Exportar scripts SQL 2022"

## Contexto

El usuario confirmó que quiere un botón en la página de Backup/Restore que permita exportar la base de datos de forma compatible con SQL Server 2022, dado que `BACKUP DATABASE` no puede generar `.bak` retrocompatibles.

## Enfoque

Agregar un botón **"Exportar scripts SQL 2022"** que genere un archivo `.sql` con:
1. Esquema de la base de datos (creación de tablas, PK, FK, índices, constraints).
2. Datos de las tablas como sentencias `INSERT` o `MERGE`.

El script resultante se puede ejecutar manualmente en SQL Server 2022 para recrear la base.

## Opciones consideradas

### Opción A: Usar SMO (Microsoft.SqlServer.Smo)
- Requiere agregar librerías de SMO (`Microsoft.SqlServer.Smo`, `Microsoft.SqlServer.ConnectionInfo`, etc.).
- Muy poderoso, genera scripts de esquema + datos de forma nativa.
- Desventaja: agrega dependencias externas pesadas y puede no estar disponible en el entorno de build.

### Opción B: Script manual con INFORMATION_SCHEMA + consulta de datos
- No requiere librerías externas.
- Genera scripts básicos pero suficientes para recrear tablas e insertar datos.
- Más controlado y portable dentro del proyecto actual.

**Decisión**: Opción B. Es más liviana, no agrega dependencias, y es suficiente para un export de compatibilidad. Se pueden agregar advertencias en la UI de que es un export básico y que objetos complejos (stored procedures, triggers, vistas) pueden requerir ajuste manual.

## Implementación

### 1. Nuevo método en `MPP/MPPDigitoVerificador.cs`

`ExportarScriptsSql(string rutaDestino)`:
- Abre conexión a la base de datos (no a master).
- Genera el script en un `StringBuilder`.
- Escribe el archivo `.sql` en la ruta destino.

Estructura del script:
```sql
-- GymApp export para SQL Server 2022
-- Fecha: ...
-- Servidor origen: ...

USE [master];
GO
IF DB_ID('GymApp') IS NOT NULL DROP DATABASE [GymApp];
GO
CREATE DATABASE [GymApp];
GO
USE [GymApp];
GO

-- Tablas
CREATE TABLE ...;
GO
ALTER TABLE ... ADD CONSTRAINT ... PRIMARY KEY ...;
GO
ALTER TABLE ... ADD CONSTRAINT ... FOREIGN KEY ...;
GO

-- Datos
INSERT INTO ... VALUES ...;
GO
```

### 2. Métodos auxiliares

- `ObtenerTablasParaScript()`: lista tablas de `INFORMATION_SCHEMA.TABLES` excluyendo `DigitoVerificador` si es de control (opcional).
- `GenerarCreateTable(string tabla)`: arma `CREATE TABLE` a partir de `INFORMATION_SCHEMA.COLUMNS` con tipos, nullable, identidad, default.
- `GenerarPrimaryKeys(string tabla)`: arma `ALTER TABLE ... ADD PRIMARY KEY` desde `INFORMATION_SCHEMA.KEY_COLUMN_USAGE` + `sys.key_constraints`.
- `GenerarForeignKeys(string tabla)`: arma `ALTER TABLE ... ADD FOREIGN KEY` desde `INFORMATION_SCHEMA.REFERENTIAL_CONSTRAINTS` + `KEY_COLUMN_USAGE`.
- `GenerarDatosTabla(string tabla)`: consulta la tabla y genera `INSERT INTO ... (cols) VALUES (...)`.
  - Manejar `NULL`, cadenas con escape de comillas simples, fechas con formato `yyyy-MM-dd HH:mm:ss`, binarios (convertir a `0x...` o saltear tablas binarias).
  - Para evitar problemas de identidad, usar `SET IDENTITY_INSERT [tabla] ON/OFF` cuando la tabla tenga identity.
- `TablaTieneIdentity(string tabla)`: consulta `sys.columns` buscando `is_identity = 1`.

### 3. Exposición en BLL

`BLLDigitoVerificador.ExportarScriptsSql(string rutaDestino)` que delega a MPP.

### 4. UI

En `Admin/BackupRestore.aspx`:
- Nuevo panel o sección con botón `btnExportarScriptsSql`.
- Campo de texto `txtRutaExportSql` para la ruta destino (sugerida como `C:\GymApp\GymApp_20250706_120000.sql`).
- Label informativo explicando que el `.sql` reemplaza al `.bak` para migraciones de versión.

En `Admin/BackupRestore.aspx.cs`:
- Evento `btnExportarScriptsSql_Click`.
- Validar extensión `.sql`.
- Llamar a `bllDV.ExportarScriptsSql(ruta)`.
- Mostrar mensaje de éxito.

En `Admin/BackupRestore.aspx.designer.cs`:
- Declarar los nuevos controles.

En `Admin/BackupRestore.css`:
- Estilos para la nueva sección y botón (usando `rem`).

## Archivos a modificar

- `MPP/MPPDigitoVerificador.cs`: lógica de exportación.
- `BLL/BLLDigitoVerificador.cs`: exposición del método.
- `gymAppV2/Admin/BackupRestore.aspx`: nueva sección de UI.
- `gymAppV2/Admin/BackupRestore.aspx.cs`: manejo del evento.
- `gymAppV2/Admin/BackupRestore.aspx.designer.cs`: declaraciones de controles.
- `gymAppV2/Admin/BackupRestore.css`: estilos.

## Limitaciones y advertencias

- El script generado incluirá tablas, columnas, PK, FK e inserts de datos. No incluye stored procedures, triggers, vistas, funciones, roles, usuarios ni permisos.
- Tablas con columnas `VARBINARY`/`IMAGE` se intentarán exportar como `0x...`; si son muy grandes, el script puede ser pesado.
- Se recomienda ejecutar el script en una base vacía en SQL Server 2022.
- Se mostrará advertencia en UI: *“Esta exportación es una alternativa manual al .bak para migrar a SQL Server 2022. No incluye objetos avanzados ni configuraciones de seguridad del servidor.”*

## Verificación

- Compilar con MSBuild.
- Revisar que la UI renderice correctamente.
- (No se puede probar export real sin conexión a SQL Server 2025, pero la lógica de generación de script estará cubierta.)
