# Plan — Pantalla de Verificación de Integridad (DVH/DVV)

## Contexto
Tras implementar el cálculo de DVH/DVV por fila, el usuario quiere una pantalla visual donde el administrador pueda:

1. Detectar errores de integridad de la base de datos.
2. Ver en qué tabla y qué campo/columna se produce el error.
3. Elegir entre tres acciones:
   - Restaurar un backup previo de la base de datos.
   - Recalcular todos los valores (DVH/DVV) para corregir los dígitos.
   - Salir sin hacer nada.
4. Si el usuario no es administrador, el sistema se "pausa": solo ve un mensaje de error de integridad.

## Decisiones de diseño confirmadas

| Decisión | Opción elegida |
|----------|----------------|
| Almacenamiento de hash por tabla | Tabla de control `DigitoVerificador` con DVH y DVV general por tabla. |
| Detección de campo erróneo | Si el hash general de la tabla falla, se verifica fila por fila y luego campo por campo para identificar la columna afectada. |
| Restaurar versión anterior | Ejecutar `RESTORE DATABASE` desde un backup previoto previamente definido. |
| Pausa para no-admin | Página de bloqueo accesible para todos; la verificación se dispara desde `BasePage` para redirigir a `VerificacioDV.aspx`. |
| Tablas a verificar | Todas las tablas del schema v2. |

## Nueva tabla de control

**`[dbo].[DigitoVerificador]`**

```sql
CREATE TABLE [dbo].[DigitoVerificador](
    [idDigitoVerificador]   INT             IDENTITY(1,1) NOT NULL,
    [nombreTabla]           VARCHAR(100)    NOT NULL,
    [dvhTabla]              VARCHAR(64)     NOT NULL,
    [dvvTabla]              VARCHAR(64)     NOT NULL,
    [fechaCalculo]          DATETIME        NOT NULL DEFAULT GETDATE(),
    CONSTRAINT [PK_DigitoVerificador] PRIMARY KEY CLUSTERED ([idDigitoVerificador] ASC),
    CONSTRAINT [UK_DigitoVerificador_NombreTabla] UNIQUE NONCLUSTERED ([nombreTabla] ASC)
) ON [PRIMARY]
```

- `dvhTabla`: hash de la concatenación de todos los `dvh` de las filas de la tabla.
- `dvvTabla`: hash de la concatenación de todos los `dvv` de las filas de la tabla.
- Se recalcula cada vez que se ejecuta la utilidad de recálculo masivo.

## Componentes nuevos/modificados

### Backend
1. **`MPP/MPPDigitoVerificador.cs`**
   - Leer/escribir en `DigitoVerificador`.
   - Verificar integridad de todas las tablas del schema v2.
   - Detectar filas corruptas y campos específicos.

2. **`BLL/BLLDigitoVerificador.cs`**
   - Exponer métodos de negocio:
     - `VerificarIntegridad()` → lista de `ResultadoVerificacionDV` (tabla, clave, campo, estado).
     - `RecalcularDigitos()` → recalcula dvv/dvh de todas las filas y actualiza `DigitoVerificador`.
     - `RestaurarBackup(string nombreBackup)` → ejecuta `RESTORE DATABASE` con confirmación.
   - Validar que solo administradores invoquen acciones destructivas.

3. **BE/ResultadoVerificacionDV.cs** (nuevo DTO)
   - `NombreTabla`, `ClaveFila`, `Campo`, `Estado`, `Mensaje`.

### Frontend
4. **`gymAppV2/VerificacioDV/VerificacioDV.aspx`**
   - Pantalla con master `DashBoard.Master`.
   - Para admin: grid de errores detectados, botones "Recalcular", "Restaurar backup", "Salir".
   - Para no-admin: mensaje de pausa del sistema y botón logout.

5. **`gymAppV2/VerificacioDV/VerificacioDV.aspx.cs`**
   - Heredar de `BasePage`.
   - Verificar permiso admin.
   - Cargar resultados de integridad.
   - Manejar clicks de botones.

6. **`gymAppV2/BasePage.cs`**
   - Agregar verificación de integridad en `OnInit`.
   - Si falla integridad y el usuario no es admin, redirigir a `~/VerificacioDV/VerificacioDV.aspx`.
   - Permitir que `VerificacioDV.aspx` misma se cargue sin entrar en bucle.

7. **`gymAppV2/DashBoard.Master` / `DashBoard.Master.cs`**
   - Agregar opción "Verificación DV" en el menú lateral solo para Admin.

### Base de datos
8. **`bd-schema-v2.sql`**
   - Agregar tabla `DigitoVerificador`.

9. **`scripts/crear-digito-verificador.sql`**
   - Script de alteración para BD existente.

10. **`scripts/actualizar-digito-verificador.sql`**
    - Insertar/actualizar registros iniciales de `DigitoVerificador` con hashes calculados.

## Cómo detectar el campo erróneo

1. Calcular `dvhTabla` y `dvvTabla` esperados.
2. Comparar con `DigitoVerificador`.
3. Si difieren:
   - Para cada fila de la tabla, recalcular DVH/DVV.
   - Si la fila está corrupta, recalcular hash individual de cada campo.
   - El campo cuyo hash individual difiere del que se usó originalmente se reporta como sospechoso.

> Nota: esta detección depende de que conozcamos los valores originales. Al tener solo `dvv` (hash acumulado de campos), no sabemos individualmente cada hash de campo. Para detectar el campo exacto, se recalcula el hash de cada campo en la fila actual y se compara con el hash de campo de una fila "de referencia". En la implementación se comparará campo contra campo entre la fila corrupta y una fila recién calculada: el campo que produce un hash distinto es el reportado.

## Restaurar backup

- El admin debe indicar el nombre o ruta del backup.
- La operación ejecutará:
  ```sql
  RESTORE DATABASE [GymApp] FROM DISK = @RutaBackup WITH REPLACE;
  ```
- Se mostrarán múltiples confirmaciones porque es destructiva.
- Si la app no tiene permisos de RESTORE, se mostrará error y se sugerirá hacerlo manualmente.

## Pausa del sistema para no-admin

- `BasePage` verifica integridad en cada request.
- Si hay falla y el rol actual no es Admin (rol = 1), se redirige a `VerificacioDV.aspx`.
- `VerificacioDV.aspx` detecta que no es admin y muestra solo el mensaje de pausa.
- Se evita bucle infinito permitiendo que `VerificacioDV.aspx` se cargue aunque haya falla de integridad.

## Criterios de aceptación

- [ ] Tabla `DigitoVerificador` existe en `bd-schema-v2.sql` y en scripts de migración.
- [ ] `BLLDigitoVerificador.VerificarIntegridad()` detecta tablas corruptas.
- [ ] La pantalla muestra tabla, clave de fila y campo problemático.
- [ ] Botón "Recalcular" actualiza dvv/dvh de todas las filas y la tabla de control.
- [ ] Botón "Restaurar backup" ejecuta `RESTORE DATABASE` con confirmaciones.
- [ ] Botón "Salir" redirige al dashboard.
- [ ] Usuario no-admin solo ve mensaje de pausa en `VerificacioDV.aspx`.
- [ ] `BasePage` redirige a la página de verificación cuando hay falla y no es admin.
- [ ] Build compila sin errores.
- [ ] TAREAS_SEGURIDAD.md ítem 11.5 actualizado.

## Riesgos

| Riesgo | Mitigación |
|--------|------------|
| `RESTORE DATABASE` requiere permisos elevados. | Mostrar error claro y documentar paso manual. |
| Verificación en cada request afecta performance. | Cachear resultado en `Session` por 1 minuto. |
| Detección de campo exacto puede ser imprecisa. | Documentar que el campo reportado es el más probable. |
| Bucle infinito si `VerificacioDV.aspx` hereda de `BasePage`. | Excluir la página de la redirección automática. |

## Archivos afectados

### Nuevos
- `MPP/MPPDigitoVerificador.cs`
- `BLL/BLLDigitoVerificador.cs`
- `BE/ResultadoVerificacionDV.cs`
- `gymAppV2/VerificacioDV/VerificacioDV.aspx`
- `gymAppV2/VerificacioDV/VerificacioDV.aspx.cs`
- `gymAppV2/VerificacioDV/VerificacioDV.aspx.designer.cs`
- `scripts/crear-digito-verificador.sql`
- `scripts/actualizar-digito-verificador.sql`

### Modificados
- `gymAppV2/BasePage.cs`
- `gymAppV2/DashBoard.Master`
- `gymAppV2/DashBoard.Master.cs`
- `gymAppV2/gymAppV2.csproj`
- `bd-schema-v2.sql`
- `BLL/BLL.csproj`
- `MPP/MPP.csproj`
- `BE/BE.csproj`
- `docs/TAREAS_SEGURIDAD.md`
