# Plan de ejecución: Dígitos Verificadores DVH / DVV

## Estado
- Modo: planificación
- Fecha: 2026-06-23
- Rama actual: `Seguridad`

## Decisiones de diseño (confirmadas con el usuario)

| Decisión | Opción elegida | Implicación |
|----------|----------------|-------------|
| Almacenamiento | Columnas `dvv` y `dvh` en **cada fila** de cada tabla, tal como está en el schema v2. | No se crea tabla de control global; ambos hashes viajan con la fila. |
| Alcance | **Todas las tablas del schema v2**. | Hay que cubrir tablas que aún no tienen MPP (Rutinas, Ejercicio, AlumnoRM, PesoHistorial, tablas de permisos, relaciones, etc.). |
| Disparo del cálculo | **C# en cada MPP**. | Se centraliza la lógica en `DigitoVerificadorManager` y cada MPP lo invoca antes de INSERT/UPDATE. |
| Campos encriptados (USUARIOS) | Calcular sobre **texto plano**. | Al verificar integridad habrá que desencriptar primero. |
| Algoritmo de hash | **SHA-256** (reutilizando `CriptoManager.GenerarHashSHA256`). | Produce 64 caracteres hexadecimales. Las columnas actuales `VARCHAR(50)` deben ampliarse a `VARCHAR(64)` o mayor. |

## Definición del cálculo

### DVH (Dígito Verificador Horizontal) — por fila

```
DVH = SHA-256(concatenación de todos los campos de la fila, excepto dvv y dvh)
```

- Orden: orden físico de columnas en la tabla (documentado en `DigitoVerificadorManager`).
- Formato de concatenación: `campo1|campo2|campo3|...` usando `|` como separador.
- Valores nulos se representan como `"NULL"`.
- Booleanos como `"0"` o `"1"`.
- Números en formato invariante (`CultureInfo.InvariantCulture`).

### DVV (Dígito Verificador Vertical) — por columna, llevado a cada fila

```
DVV = SHA-256(SHA-256(campo1) + SHA-256(campo2) + ... + SHA-256(campoN))
```

- Se calcula sobre los **mismos campos** que el DVH (excluyendo `dvv` y `dvh`).
- Para cada campo se calcula su SHA-256 individual.
- Se concatenan esos hashes intermedios y se vuelve a hashear.
- Así se obtiene una huella "por columna" que se guarda en la fila.

## Componente central

**Archivo nuevo:** `SERVICIOS/DigitoVerificadorManager.cs`

Responsabilidades:
- `CalcularDVH(string nombreTabla, Dictionary<string, object> valores)`
- `CalcularDVV(string nombreTabla, Dictionary<string, object> valores)`
- `VerificarDVH(string dvhAlmacenado, string nombreTabla, Dictionary<string, object> valores)`
- `VerificarDVV(string dvvAlmacenado, string nombreTabla, Dictionary<string, object> valores)`
- `RecalcularTablaCompleta(string nombreTabla)` — útil para migración inicial.
- Métodos auxiliares privados para normalizar valores nulos, booleanos, fechas y números decimales.

El componente **no toca base de datos**; solo recibe diccionarios de valores y devuelve strings. Los MPP deciden cuándo llamarlo.

## Cambios en base de datos

1. **Ampliar columnas `dvv` y `dvh`** a `VARCHAR(64)` en todas las tablas (el hash SHA-256 son 64 caracteres hex).
2. **Normalizar `Evento.dvv`** a `VARCHAR(64) NOT NULL` y `Evento.dvh` a `VARCHAR(64)` (hoy es `VARCHAR(50) NULL` y `VARCHAR(256) NULL`).
3. Script de migración para recalcular `dvv`/`dvh` de **todas las filas existentes** que hoy tienen `''`.

## Cambios por capa

### MPP existentes (C#)

Actualizar cada método `INSERT`/`UPDATE` para:
1. Armar un `Dictionary<string, object>` con los valores que se van a persistir.
2. Para `USUARIOS`: usar **valores planos** de `nombre`, `apellido`, `telefono`, `email`, `fechaNacimiento` para el cálculo, aunque a BD se envíen encriptados.
3. Calcular DVH y DVV con `DigitoVerificadorManager`.
4. Incluir `@DVH` y `@DVV` en la consulta SQL.

MPPs afectados:
- `MPPUsuario.cs` (más complejo por encriptación reversible)
- `MPPAlumno.cs`
- `MPPEntrenador.cs`
- `MPPPreguntaSeguridad.cs`
- `MPPEvento.cs`
- `MPPPrecioModalidad.cs`
- `MPPActividad.cs` (cuando se agreguen métodos de escritura)
- `MPPRol.cs` (si maneja escritura de perfiles/permisos)

### Tablas del schema sin MPP (todas las tablas del schema v2)

Como el usuario eligió C# en cada MPP, pero algunas tablas aún no tienen capa de datos, se propone:

- **Opción recomendada:** crear MPP mínimos con solo operaciones de escritura para las tablas que ya tienen datos o que se espera modificar pronto (`PrecioModalidad`, tablas de permisos, etc.).
- **Respaldo:** para tablas puramente transaccionales sin MPP (p. ej. `Actividad_Alumno`, `RutinaEjercicio`), se agregarán **triggers SQL AFTER INSERT/UPDATE/DELETE** que calculen DVH/DVV, evitando dejar filas sin verificar.

> Nota: los triggers no sustituyen a la capa C#; son un resguardo para tablas que aún no tienen MPP.

## Migración de datos existentes

- Script `scripts/migrar-dvv-dvh.sql` (o método expuesto por `BLLDigitoVerificador`) que recalcule y actualice `dvv`/`dvh` de todas las filas de todas las tablas.
- Página admin opcional `Admin/VerificarIntegridad.aspx` para ejecutar recálculo y mostrar estado por tabla (filas OK, filas con error, columnas alteradas).

## Verificación en lectura

- No se agrega verificación automática en cada `SELECT` inicialmente (por performance).
- Se expone `DigitoVerificadorManager.VerificarDVH/VerificarDVV` para que los BLL/UI puedan llamar explícitamente cuando lo necesiten (por ejemplo, desde una página de administración de integridad).

## Archivos a crear/modificar

### Nuevos
1. `SERVICIOS/DigitoVerificadorManager.cs`
2. `BLL/BLLDigitoVerificador.cs` (expone recálculo masivo y verificación)
3. `scripts/migrar-dvv-dvh.sql` o `scripts/alter-columnas-dvv-dvh.sql`
4. (Opcional) `gymAppV2/Admin/VerificarIntegridad.aspx` + codebehind

### Modificados
5. `bd-schema-v2.sql` — tamaño de columnas `dvv`/`dvh`.
6. Todos los `MPP/*.cs` con métodos de escritura.
7. `docs/plan-dvv-dvh.md` — este documento se actualiza con detalles finales.
8. `docs/TAREAS_SEGURIDAD.md` — marcar ítems 11.x como en progreso/done.

## Riesgos y mitigaciones

| Riesgo | Mitigación |
|--------|------------|
| Columnas `VARCHAR(50)` no entran 64 chars de SHA-256. | Script de alteración a `VARCHAR(64)` incluido en el plan. |
| Campos encriptados de USUARIOS dificultan el cálculo sobre texto plano. | El MPP encripta para BD pero usa planos para el hash. |
| Cambiar de foco desde el `plan-apartado-5.md` que está en curso. | **Pendiente de decisión del usuario**: ver sección "Relación con apartado 5". |
| Tablas sin MPP quedarían sin cálculo. | Triggers SQL de respaldo para las tablas que aún no tengan MPP. |
| Performance al recalcular DVV de toda una tabla. | Se hace offline en migración inicial; en operaciones normales solo se recalcula la fila afectada. |

## Relación con el apartado 5 en curso

Actualmente existe un plan en progreso (`docs/plan-apartado-5.md`) para cerrar tareas de autorización y control de acceso por rol, con archivos nuevos sin commitear (`gymAppV2/Perfil/`, `gymAppV2/Rutinas/`, `BE/Actividad.cs`, `BLL/BLLActividad.cs`, `MPP/MPPActividad.cs`).

**Pregunta pendiente para el usuario:**
> ¿Pausamos el apartado 5 y nos enfocamos en DVH/DVV, o terminamos primero el apartado 5 y luego hacemos DVH/DVV?

Recomendación del plan: terminar el apartado 5 primero (es un cambio más cerrado y ya está avanzado), y luego aplicar DVH/DVV sobre una base estable.

## Criterios de aceptación

- [ ] `SERVICIOS/DigitoVerificadorManager.cs` compila y tiene tests manuales de cálculo/verificación.
- [ ] Todos los MPP con escritura actualizan `dvv` y `dvh` en cada INSERT/UPDATE.
- [ ] Script SQL altera columnas `dvv`/`dvh` a `VARCHAR(64)` en todas las tablas.
- [ ] Script SQL recalcula `dvv`/`dvh` de todas las filas existentes.
- [ ] La solución compila sin errores (`msbuild` o Visual Studio).
- [ ] Se actualiza `docs/TAREAS_SEGURIDAD.md` marcando 11.1 a 11.4 como completados.

## Próximos pasos (tras aprobación del plan)

1. Definir lista exacta de tablas y columnas por tabla (metadata) para `DigitoVerificadorManager`.
2. Implementar `SERVICIOS/DigitoVerificadorManager.cs`.
3. Alterar columnas en SQL.
4. Actualizar MPPs operativos de escritura.
5. Crear triggers respaldo para tablas sin MPP.
6. Ejecutar migración inicial de datos.
7. Build + pruebas manuales.
