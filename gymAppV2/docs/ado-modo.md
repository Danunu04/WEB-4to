# ADO Conectado vs Desconectado — Decisión de diseño

> Fecha: 2026-07-01  
> Aplicación: GymApp (ASP.NET Web Forms / .NET Framework 4.7.2)

## Principio general

En una aplicación web la **conexión a la base de datos debe ser lo más breve posible**. Por eso, salvo operaciones de escritura o procesos transaccionales, se prefiere el **modo desconectado** (`SqlDataAdapter` → `DataTable`/`DataSet`). El modo conectado (`SqlDataReader`, `ExecuteNonQuery`) se reserva para:

- `INSERT`, `UPDATE`, `DELETE`.
- Procedimientos almacenados que modifican estado.
- Operaciones masivas que requieren control transaccional explícito.

## Mapeo de métodos de `DalGeneral`

| Método | Modo ADO | Uso típico |
|---|---|---|
| `_686DPConsultar(string consulta, ArrayList parametros)` | **Desconectado** (`SqlDataAdapter.Fill`) | `SELECT` que devuelve varias filas para grids, listas, reportes. |
| `_686DPConsultarSP(string nombreSP, ArrayList parametros)` | **Desconectado** (`SqlDataAdapter.Fill`) | `SELECT` vía stored procedure con múltiples filas. |
| `_686DPEjecutar(string nombreSP, ArrayList parametros)` | **Conectado** (`ExecuteNonQuery`) | Stored procedures que ejecutan `INSERT/UPDATE/DELETE`. |
| `_686DPEscalar(string consulta, ArrayList parametros)` | **Conectado** (`ExecuteScalar`) | Conteos, valores únicos, `COUNT(*)`, `MAX`, etc. |
| `_686DPEscribir(string consulta, ArrayList parametros)` | **Conectado** (`ExecuteNonQuery`) | Sentencias `INSERT/UPDATE/DELETE` directas. |

## Reglas de uso para desarrolladores

1. **Lecturas**
   - Si el resultado son varias filas → usar `_686DPConsultar` o `_686DPConsultarSP` (desconectado).
   - Si se necesita un solo valor escalar → preferir `_686DPConsultar` con `TOP 1` y leer `dt.Rows[0][0]`, o `_686DPEscalar` si la consulta es trivial y atómica.

2. **Escrituras**
   - Siempre conectado (`_686DPEscribir` / `_686DPEjecutar`).
   - Nunca devolver un `DataTable` solo para saber si se afectó una fila; usar `ExecuteNonQuery` y, si es necesario, el valor de retorno del SP.

3. **Transacciones**
   - Para operaciones que afecten varias tablas, abrir explícitamente una `SqlTransaction` en el método conectado correspondiente.
   - Hoy `DalGeneral` no expone transacciones; si se necesitan, se debe extender con cuidado.

4. **Cierre de conexiones**
   - Todos los métodos de `DalGeneral` abren la conexión, ejecutan y la cierran en `finally`.
   - No mantener `conn` abierto entre llamadas; no compartir la misma instancia de `DalGeneral` entre threads.

## Nota sobre el contexto web

ASP.NET maneja requests concurrentes. Mantener conexiones abiertas (modo conectado prolongado) agota el pool de conexiones de SQL Server y reduce la escalabilidad. El modo desconectado libera la conexión inmediatamente después de llenar el `DataTable`, dejando los datos en memoria para que la capa de presentación los consuma sin presionar la BD.
