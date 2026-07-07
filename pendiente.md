# Pendiente — ERR_TOO_MANY_REDIRECTS / VerificacioDV

## Estado
**FIX APLICADO** — Causa raíz identificada y corregida.

---

## Causa raíz (confirmada)

`RouteConfig.cs` usa `Microsoft.AspNet.FriendlyUrls` con `AutoRedirectMode = RedirectMode.Permanent`.

Esto significa que toda URL que termina en `.aspx` recibe un **HTTP 301 permanente** hacia la versión
sin extensión. Ejemplo: `/VerificacioDV/VerificacioDV.aspx` → **301** → `/VerificacioDV/VerificacioDV`.

Consecuencia: `Request.AppRelativeCurrentExecutionFilePath` devuelve `~/VerificacioDV/VerificacioDV`
(sin `.aspx`). Los arrays de páginas exentas contenían `"VerificacioDV/VerificacioDV.aspx"` →
**el match fallaba** → `BasePage` redirigía a `~/VerificacioDV/VerificacioDV.aspx` → FriendlyUrls
hacía 301 a la versión sin extensión → `BasePage` no la reconocía → **loop infinito**.

---

## Archivos modificados

### `gymAppV2/gymAppV2/BasePage.cs`
- Arrays cambiados a rutas SIN `.aspx`:
  - `PAGINAS_EXENTAS_VERIFICACION`: `"VerificacioDV/VerificacioDV"`, `"Admin/BackupRestore"`
  - `PAGINAS_SOLO_CON_ERROR_INTEGRIDAD`: `"VerificacioDV/VerificacioDV"`
  - `PAGINAS_MANTENIMIENTO_SISTEMA`: `"Admin/EncriptarDatos"`
- Agregado helper `NormalizarPagina(string)`: quita `~/` y `.aspx` antes de comparar
- Los tres métodos de verificación usan `NormalizarPagina(paginaActual)` en vez de `.Replace("~/", "")`

### `gymAppV2/gymAppV2/DashBoard.Master.cs`
- Mismo helper `NormalizarPagina` agregado
- Check de pausa de integridad usa `NormalizarPagina` y compara contra `"VerificacioDV/VerificacioDV"` (sin extensión)

### `gymAppV2/gymAppV2/VerificacioDV/VerificacioDV.aspx` *(de la sesión anterior)*
- Cambiado a `MasterPageFile="~/ErrorState.Master"` (ya no depende de DashBoard.Master)

### `gymAppV2/gymAppV2/ErrorState.Master` *(creado en sesión anterior)*
- Master page mínima, sin lógica de redirección, para ser usada por VerificacioDV

---

## Pruebas de verificación

Para simular error de integridad: modificar un `dvh`/`dvv` directamente en la BD:
```sql
UPDATE USUARIOS SET dvh = 'INVALIDO' WHERE USUARIO_ID = 1
```

| # | Escenario | Resultado esperado |
|---|-----------|-------------------|
| 1 | Login no-admin con error de integridad | Carga VerificacioDV con panel de bloqueo — **sin loop** |
| 2 | Login admin con error de integridad | Carga VerificacioDV con panel de reparación — **sin loop** |
| 3 | Botón "Salir" con error activo (ambos roles) | Muestra error "debe resolver antes de salir" |
| 4 | Admin recalcula → no hay error → "Salir" | Hace logout, redirige a Login |
| 5 | Acceder a VerificacioDV sin estar logueado | Redirige a Login (desde BasePage) |
| 6 | Acceder a VerificacioDV sin error de integridad | Redirige al Dashboard |
| 7 | Login normal (sin error de integridad) | Dashboard normal |
