# Plan de implementación — Revisión de seguridad integral

> Rama: `Seguridad`  
> Fecha: 2026-07-01  
> Objetivo: verificar que los puntos de seguridad solicitados estén implementados y, en su defecto, implementarlos sin romper la aplicación.

---

## 1. Estado actual resumido

| Punto solicitado | ¿Existe hoy? | Estado |
|---|---|---|
| Login con singleton | Sí (`SERVICIOS/Singleton/Singleton.cs` + `SesionUsuario.cs`) | Funciona, pero conviene endurecerlo |
| Usuario anónimo solo ve Inicio y Login | Parcial (`Web.config` permite también `CambiarContra`) | Ajustar |
| Frenar TODO ante error de integridad en BD | Parcial (`BasePage.VerificarIntegridadSiAplica`) | Debe ser más automático y estricto |
| ADO Conectado vs Desconectado documentado | No | Definir y documentar |
| Robustez “que no reviente” | Parcial | Mejorar redirecciones y catch silenciosos |
| Perfiles hardcodeados | Parcial (roles numéricos 1-4 en `BLLRol`) | Mapear a perfiles con nombres |

---

## 2. Decisiones de diseño propuestas

### 2.1 Singleton de sesión
- **Mantener** el patrón `Singleton.Instancia` que devuelve `SesionUsuario` desde `HttpContext.Current.Session`.
- **Mejorar**: validar nulos en todos los getters, nunca propagar `HttpException` al usuario final y evitar dobles instancias.

### 2.2 Acceso anónimo
- **Solo** `Inicio/Default.aspx` y `LogIn/LogIn.aspx` permitirán usuarios anónimos.
- `CambiarContra/Cambiar-contra.aspx` seguirá existiendo para flujos de primer login y recuperación, pero se protegerá por **token de flujo** (modo `primerLogin` o `recuperacion` con usuario identificado) en lugar de estar abierta a cualquiera.
- `AccesoDenegado.aspx` seguirá pública.

### 2.3 Integridad de datos (DVH/DVV)
- Ante **cualquier** error de integridad detectado:
  1. Todos los usuarios no-administradores serán redirigidos a `VerificacioDV/VerificacioDV.aspx`.
  2. Los administradores podrán seguir operando para poder recalcular / restaurar backup.
  3. La verificación se ejecutará en **cada carga de página protegida** sin excepciones silenciosas.
  4. Se registrará el evento en bitácora con criticidad alta.
- Se agregará un **Health Check** en `Application_Start` que falle rápido si la tabla de control `DigitoVerificador` no existe o está vacía.

### 2.4 ADO Conectado vs Desconectado
| Tipo de operación | Modo recomendado | Razón |
|---|---|---|
| `SELECT` de muchas filas / grids / reportes | **Desconectado** (`SqlDataAdapter` → `DataTable`) | Escalabilidad web; no mantener conexión abierta |
| `SELECT` de una sola fila / conteos | **Desconectado** (DataTable de una fila) o escalar con `ExecuteScalar` si es atómico | Minimizar conexiones abiertas |
| `INSERT/UPDATE/DELETE` | **Conectado** (`ExecuteNonQuery`) | Necesita transacción y retorno de filas afectadas |
| Procesos masivos (backup, restore, migración) | **Conectado** con `CommandTimeout` extendido | Control transaccional |

- Se documentará en `docs/ado-modo.md` y se marcarán los métodos de `DalGeneral` con comentarios explícitos.

### 2.5 Perfiles hardcodeados
- Se mapearán los roles numéricos actuales a los perfiles solicitados:

| Rol numérico | Perfil(es) hardcodeados | Alcance funcional |
|---|---|---|
| 1 | **Web master**, **Administrador**, **ABM**, **bitacora**, **alumnos**, **profesores**, **back up**, **restore**, **dv** | Todo el sistema |
| 2 | **Recepcionista/ABM** (usuarios, alumnos, profesores, bitacora parcial) | Gestión operativa sin backups ni DV |
| 3 | **Profesor/Entrenador** | Rutinas y actividades de sus alumnos |
| 4 | **Cliente/Docente** (alumno/familiar) | Perfil, actividades inscriptas, pagos propios |

- Se implementará en `BLLRol` una lista de permisos finos (`Permiso`) y familias (`Familia`) **hardcodeados en C#**, evitando depender de datos de BD para roles críticos. La tabla `Perfiles` seguirá existiendo para futuras extensiones, pero la autorización principal usará el catálogo en código.

---

## 3. Plan de trabajo por área

### Área A — Login / Singleton / Sesión
1. **Revisar `SERVICIOS/Singleton/Singleton.cs`**
   - Asegurar que `Instancia` no cree un nuevo objeto si ya existe una sesión válida.
   - Manejar nulos sin lanzar excepciones al usuario.
2. **Revisar `SERVICIOS/Singleton/SesionUsuario.cs`**
   - `LogIn` debe invalidar cualquier sesión previa antes de crear la nueva.
   - `LogOut` debe llamar `Session.Abandon()` y `FormsAuthentication.SignOut()`.
3. **Revisar `BLL/BLLUsuario.cs`**
   - `LogearUsuario` y `DeslogearUsuario` deben usar el singleton sin excepciones no controladas.
   - `UsuarioEstaLogueado` debe ser robusto ante `HttpException`.
4. **Revisar `gymAppV2/LogIn/LogIn.aspx.cs`**
   - Confirmar que no permite login doble accidental.
   - Usar `Response.Redirect(url, false)` + `CompleteRequest()` para evitar `ThreadAbortException`.

### Área B — Acceso anónimo restringido
1. **Modificar `Web.config`**
   - Quitar `<location path="CambiarContra">` del acceso anónimo.
   - Mantener anónimos: `LogIn`, `Inicio`, `AccesoDenegado`, `Content`.
2. **Proteger `CambiarContra/Cambiar-contra.aspx.cs`**
   - Si no hay sesión, exigir query string `?usuario=X&modo=primerLogin|recuperacion` y validar que el flujo sea legítimo (p. ej. cuenta bloqueada o primer login real).
   - Si hay sesión, permitir cambio normal.
3. **Eliminar o redirigir páginas huérfanas**
   - `Default.aspx` (raíz), `About.aspx`, `Contact.aspx`: convertir a `BasePage` o redirigir a `Inicio/Default.aspx` si no tienen función.
   - `WebForm1.aspx` (raíz): mismo tratamiento.

### Área C — Frenar TODO ante error de integridad
1. **Fortalecer `gymAppV2/BasePage.cs`**
   - `VerificarIntegridadSiAplica` debe ejecutarse siempre, sin `catch` vacío.
   - Si `BllDV.ExisteErrorIntegridad()` devuelve `true`, redirigir a `VerificacioDV.aspx` excepto para administradores.
   - Registrar evento de bitácora `error_integridad` con criticidad 4 (Crítico).
2. **Crear/fortalecer `Global.asax`**
   - `Application_Start`: verificar que exista `DigitoVerificador` y que tenga al menos las tablas principales registradas.
   - `Application_Error`: capturar excepciones no controladas, registrar en bitácora y redirigir a página amigable (sin exponer stack trace).
3. **Mejorar `BLL/BLLDigitoVerificador.cs`**
   - `ExisteErrorIntegridad` debe ser determinista; si no puede verificar, asumir error (ya lo hace, pero documentar).
   - Agregar método `PausarSistemaSiHayError()` para uso desde `BasePage`.
4. **Actualizar `VerificacioDV/VerificacioDV.aspx`**
   - Mostrar mensaje claro de “Sistema pausado por falla de integridad” para no administradores.
   - Para administradores: mostrar tabla con errores, botones “Recalcular”, “Restaurar backup” y “Salir”.

### Área D — ADO Conectado vs Desconectado
1. **Crear documento `docs/ado-modo.md`**
   - Tabla de decisión, ejemplos de métodos y justificación web.
2. **Anotar `DalGeneral.cs`**
   - `_686DPConsultar` → desconectado.
   - `_686DPConsultarSP` → desconectado.
   - `_686DPEjecutar` → conectado.
   - `_686DPEscalar` → conectado.
   - `_686DPEscribir` → conectado.
3. **Evaluar refactor futuro**
   - Consolidar `_686DPEjecutar` y `_686DPEscribir` en un solo método conectado para escritura, y `_686DPConsultar` para lectura desconectada.

### Área E — Robustez general (“que no reviente”)
1. **Estandarizar redirecciones**
   - Helper `RedirigirSeguro(url)` en `BasePage` que use `Response.Redirect(url, false)` + `Context.ApplicationInstance.CompleteRequest()`.
   - Reemplazar `Response.Redirect(url)` directo en `BasePage`, `DashBoard.Master`, `LogIn`, etc.
2. **Eliminar catch vacíos o genéricos que ocultan errores**
   - `BasePage.VerificarIntegridadSiAplica` debe loguear el error real.
   - `BLLUsuario.RegistrarEvento` ya loguea fallback; replicar en otros catch vacíos.
3. **Manejo global de errores**
   - `Global.asax Application_Error`: loguear en bitácora y mostrar mensaje genérico al usuario.
4. **Validar nulls en capa UI**
   - Revisar todos los `Page_Load` que acceden a `Singleton.Instancia.Usuario` sin verificar login primero.

### Área F — Perfiles / Permisos hardcodeados
1. **Crear catálogo de permisos en `BE` o `SERVICIOS`**
   - Clase estática `PermisosSistema` con constantes: `Dashboard`, `GestionUsuarios`, `GestionAlumnos`, `GestionEntrenadores`, `GestionRutinas`, `ActividadesCalendario`, `Bitacora`, `Pagos`, `Perfil`, `VerificacionDV`, `Backup`, `Restore`, `RecalcularDV`.
2. **Crear catálogo de perfiles hardcodeados**
   - Clase estática `PerfilesSistema` con los nombres solicitados y el rol numérico asignado.
3. **Refactorizar `BLL/BLLRol.cs`**
   - `TieneAccesoAModulo` debe usar el catálogo de permisos.
   - Agregar `EsAdmin`, `EsWebMaster`, `PuedeBackup`, `PuedeRestore`, `PuedeRecalcularDV`.
4. **Actualizar `DashBoard.Master.cs`**
   - Mostrar/ocultar ítems según permisos finos (no solo rol numérico).
   - Habilitar `liBackup`, `liRestore`, `liRecalcularDV` para administradores.
5. **Actualizar todas las páginas protegidas**
   - Reemplazar `VerificarAcceso("...")` por permisos finos donde aplique.
   - Asegurar que `VerificacionDV`, `Backup`, `Restore` requieran `Web master` / `Administrador`.

### Área G — Verificación y pruebas
1. **Crear/actualizar `docs/PRUEBAS_SEGURIDAD.md`**
   - Casos de prueba para cada perfil y cada punto crítico.
2. **Crear script de verificación `scripts/verificar-seguridad.sql`**
   - Validar que existan usuarios de prueba para cada rol, perfiles hardcodeados y tabla `DigitoVerificador` poblada.
3. **Pruebas manuales mínimas**
   - Acceder anónimamente solo a Inicio/Login.
   - Cliente no entra a Usuarios/Bitácora.
   - Web master puede ver DV/Backup/Restore.
   - Forzar error de integridad (modificar un `dvh` a mano) y confirmar que el sistema pausa para no-admins.
4. **Build**
   - `msbuild Gym-APP.sln` o Visual Studio sin errores.

---

## 4. Archivos a modificar/crear

### Modificados
- `gymAppV2/Web.config`
- `gymAppV2/Global.asax.cs`
- `gymAppV2/BasePage.cs`
- `gymAppV2/DashBoard.Master.cs`
- `gymAppV2/LogIn/LogIn.aspx.cs`
- `gymAppV2/CambiarContra/Cambiar-contra.aspx.cs`
- `gymAppV2/Default.aspx.cs` (raíz)
- `gymAppV2/About.aspx.cs`
- `gymAppV2/Contact.aspx.cs`
- `gymAppV2/WebForm1.aspx.cs` (raíz)
- `BLL/BLLRol.cs`
- `BLL/BLLUsuario.cs`
- `BLL/BLLDigitoVerificador.cs`
- `SERVICIOS/Singleton/Singleton.cs`
- `SERVICIOS/Singleton/SesionUsuario.cs`
- `DAL/DalGeneral.cs`
- `docs/TAREAS_SEGURIDAD.md`

### Nuevos
- `BE/PermisosSistema.cs`
- `BE/PerfilesSistema.cs`
- `docs/ado-modo.md`
- `docs/PRUEBAS_SEGURIDAD.md`
- `scripts/verificar-seguridad.sql`
- (Opcional) `gymAppV2/Seguridad/HealthCheck.aspx` para diagnóstico rápido

---

## 5. Riesgos y mitigaciones

| Riesgo | Mitigación |
|---|---|
| Cambiar `Web.config` deja fuera a usuarios en recuperación | Dejar `CambiarContra` accesible por flujo validado, no anónimo |
| Verificación DV en cada request afecta performance | Ejecutar solo en `BasePage.OnInit`; cachear resultado en `Application["IntegridadOK"]` con timestamp corto |
| Refactor de `BLLRol` rompe menús existentes | Cambiar primero la matriz interna, luego actualizar `DashBoard.Master` en el mismo commit |
| Página huérfanas rompen bookmarks | Redirigir `Default.aspx`, `About.aspx`, `Contact.aspx` a `Inicio/Default.aspx` |
| ThreadAbortException por redirecciones | Usar helper `RedirigirSeguro` en todas partes |

---

## 6. Criterios de aceptación

- [x] Un usuario no autenticado accede solo a `Inicio/Default.aspx`, `LogIn/LogIn.aspx` y `AccesoDenegado.aspx`.
- [x] `CambiarContra` requiere sesión activa **o** flujo de primer login/recuperación validado por token.
- [x] `Singleton.Instancia` devuelve siempre la misma sesión del usuario actual y nunca expone errores técnicos.
- [x] Si se corrompe un `dvh`/`dvv` en BD, cualquier página protegida redirige a `VerificacioDV.aspx` para usuarios no-admin.
- [x] Los administradores pueden recalcular DVH/DVV y restaurar backup desde `VerificacioDV.aspx`.
- [x] `BLLRol` usa perfiles hardcodeados (`Web master`, `Administrador`, `ABM`, `bitacora`, `alumnos`, `profesores`, `Backup`, `Restore`, `DV`, `Cliente/Docente`).
- [x] El menú lateral muestra solo las opciones permitidas para el perfil logueado.
- [x] Todos los métodos de `DalGeneral` están etiquetados como conectado/desconectado.
- [x] La aplicación no expone stack traces ni mensajes técnicos de SQL al usuario final.
- [x] `msbuild` / Visual Studio compilan la solución sin errores.

---

## 7. Orden de ejecución recomendado

1. **Fase 1 — Fundamentos**: Singleton, Web.config, BasePage, Global.asax, redirecciones seguras.
2. **Fase 2 — Integridad**: fortalecer `BasePage`, `BLLDigitoVerificador`, página `VerificacioDV`.
3. **Fase 3 — Permisos**: catálogos `PermisosSistema` / `PerfilesSistema`, refactor `BLLRol`, menú lateral.
4. **Fase 4 — ADO y robustez**: documentar modo ADO, estandarizar redirecciones, manejo global de errores.
5. **Fase 5 — Limpieza**: redirigir páginas huérfanas, ajustar `CambiarContra`.
6. **Fase 6 — Verificación**: pruebas manuales, build final, actualizar `docs/TAREAS_SEGURIDAD.md`.

---

> **Nota:** Este plan es amplio porque el usuario pidió “todo el resto hace un plan para verificar que esté todo esto y en el caso de no estar como implementarlo”. Se puede ejecutar por fases independientes; cada fase puede ser un commit/PR separado.
