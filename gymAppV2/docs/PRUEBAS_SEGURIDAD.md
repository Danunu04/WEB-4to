# Plan de pruebas de seguridad — GymApp

> Rama: `Seguridad`  
> Fecha: 2026-07-01  
> Objetivo: verificar manualmente que cada requisito de seguridad solicitado esté implementado y funcione sin romper la aplicación.

---

## 1. Alcance

Estas pruebas cubren los puntos solicitados explícitamente:

1. Login con patrón **Singleton**.
2. Usuario anónimo solo puede ver **Inicio** y **LogIn**.
3. Ante un error de integridad en BD el sistema **se frena** hasta que un administrador lo resuelva.
4. Modo de acceso a BD documentado: **ADO Conectado vs Desconectado**.
5. La aplicación **no revienta** ante errores comunes.
6. Perfiles hardcodeados: **Web master / Administrador / ABM / bitácora / alumnos / profesores / Backup / Restore / DV / Cliente/Docente**.

---

## 2. Entorno de prueba

| Ítem | Valor |
|---|---|
| Solución | `C:\Users\Danunu\Desktop\WEB-4to\gymAppV2\gymAppV2.sln` |
| Build | `msbuild gymAppV2.sln -p:Configuration=Debug -p:Platform="Any CPU"` |
| Base de datos | SQL Server, base `GymApp` |
| Navegador | Chrome / Edge (modo incógnito para anónimos) |
| Usuarios de prueba | admin / recepcionista / entrenador / cliente (ver script `scripts/verificar-seguridad.sql`) |

---

## 3. Casos de prueba

### 3.1 Login con Singleton

| Paso | Acción | Resultado esperado |
|---|---|---|
| 1 | Abrir dos pestañas del navegador y loguearse con el mismo usuario en ambas. | La segunda pestaña debe detectar que ya hay sesión o, al menos, el singleton debe devolver la misma instancia de sesión. |
| 2 | En la primera pestaña hacer logout. | Refrescar la segunda pestaña: debe redirigir a `LogIn.aspx` (la sesión fue invalidada globalmente). |
| 3 | Borrar cookies de sesión/forms y acceder a `~/DashBoard/WebForm1.aspx`. | Redirige a `~/LogIn/LogIn.aspx` sin mensaje técnico. |
| 4 | Verificar `SERVICIOS/Singleton/Singleton.cs`. | `Instancia` nunca lanza excepción al usuario; devuelve `null` si no hay sesión HTTP. |

### 3.2 Acceso anónimo restringido

| Paso | Acción | Resultado esperado |
|---|---|---|
| 1 | En navegador incógnito, abrir `~/Inicio/Default.aspx`. | Carga la página de check-in público. |
| 2 | Abrir `~/LogIn/LogIn.aspx`. | Carga el login. |
| 3 | Intentar `~/Usuarios/UsuariosModulo.aspx`, `~/Bitacora/Bitacora.aspx`, `~/Alumnos/Alumnos.aspx`, `~/Entrenadores/Entrenadores.aspx`, `~/Admin/EncriptarDatos.aspx`, `~/VerificacioDV/VerificacioDV.aspx`. | Redirige a `~/LogIn/LogIn.aspx` (o `AccesoDenegado.aspx`) sin mostrar contenido. |
| 4 | Intentar `~/CambiarContra/Cambiar-contra.aspx` sin sesión y sin parámetros. | Redirige a `~/LogIn/LogIn.aspx`. |
| 5 | Revisar `Web.config`. | Solo `LogIn`, `Inicio`, `AccesoDenegado` y `Content` tienen `<allow users="*"/>`. |

### 3.3 Integridad de datos (DVH/DVV)

| Paso | Acción | Resultado esperado |
|---|---|---|
| 1 | Ejecutar `scripts/verificar-seguridad.sql` para confirmar que `DigitoVerificador` está poblado. | El script devuelve al menos las tablas principales. |
| 2 | Corromper a mano un `dvh` en `USUARIOS` (por ejemplo, cambiar el último carácter). | `BllDV.ExisteErrorIntegridad()` debe devolver `true`. |
| 3 | Con usuario **Cliente** o **Entrenador**, cargar cualquier página protegida. | Redirige a `~/VerificacioDV/VerificacioDV.aspx` con mensaje de sistema pausado. |
| 4 | Con usuario **Administrador**, cargar cualquier página protegida. | Puede seguir navegando (solo los admin pueden reparar). |
| 5 | En `VerificacioDV.aspx` como admin, presionar **Recalcular DVH/DVV**. | Los hashes se regeneran, el error desaparece y las páginas protegidas vuelven a funcionar. |
| 6 | Restaurar un backup válido desde la misma página (si existe funcionalidad). | Integridad OK. |

### 3.4 ADO Conectado vs Desconectado

| Paso | Acción | Resultado esperado |
|---|---|---|
| 1 | Abrir `DAL/DalGeneral.cs`. | Cada método público tiene un comentario `/// <summary>` que indica si es **Conectado** o **Desconectado**. |
| 2 | Verificar `docs/ado-modo.md`. | Existe documento con la matriz de decisión y justificación web. |
| 3 | Confirmar que lecturas de grids (`_686DPConsultar`, `_686DPConsultarSP`) usan `SqlDataAdapter`/`DataTable`. | Conexión se abre solo durante el `Fill`. |
| 4 | Confirmar que escrituras (`_686DPEjecutar`, `_686DPEscribir`) usan `ExecuteNonQuery`. | Conexión abierta durante toda la transacción. |

### 3.5 Robustez — "que no reviente"

| Paso | Acción | Resultado esperado |
|---|---|---|
| 1 | Forzar un error de base de datos (detener SQL Server momentáneamente) e intentar login. | Mensaje amigado; no aparece stack trace ni excepción cruda. |
| 2 | Forzar un `404` accediendo a una URL inexistente. | Redirige a `~/AccesoDenegado.aspx`. |
| 3 | Forzar un `401/403` intentando una carpeta protegida. | Redirige a `~/AccesoDenegado.aspx`. |
| 4 | Revisar `Global.asax.cs` → `Application_Error`. | Captura excepciones no controladas, registra en bitácora y redirige a página amigable. |
| 5 | Revisar redirecciones en `BasePage.cs`, `LogIn.aspx.cs`, `DashBoard.Master.cs`. | Usan `Response.Redirect(url, false)` + `CompleteRequest()` para evitar `ThreadAbortException`. |

### 3.6 Perfiles hardcodeados

| Paso | Acción | Resultado esperado |
|---|---|---|
| 1 | Revisar `BE/PermisosSistema.cs` y `BE/PerfilesSistema.cs`. | Existen constantes para perfiles y permisos. |
| 2 | Revisar `BLL/BLLRol.cs`. | `TieneAccesoAModulo` mapea roles numéricos a los permisos finos. |
| 3 | Loguearse como **Administrador** (rol 1). | Menú muestra: Usuarios, Alumnos, Entrenadores, Bitácora, Actividades, Rutinas, Perfil, Backup, Restore, DV, Encriptar datos. |
| 4 | Loguearse como **Recepcionista** (rol 2). | Menú muestra operativa (Usuarios, Alumnos, Entrenadores, Bitácora, Actividades, Rutinas, Perfil) y NO muestra Backup/Restore/DV. |
| 5 | Loguearse como **Entrenador/Profesor** (rol 3). | Menú muestra solo Rutinas, Actividades (lectura), Perfil. NO Usuarios, Alumnos ABM, Bitácora, Backup, DV. |
| 6 | Loguearse como **Cliente/Docente** (rol 4). | Menú muestra solo Perfil y Actividades de sus alumnos inscriptos. NO puede crear ni editar. |
| 7 | Intentar como Cliente ingresar directamente a `~/Usuarios/UsuariosModulo.aspx`. | Redirige a `~/AccesoDenegado.aspx`. |
| 8 | Intentar como Entrenador ingresar a `~/Bitacora/Bitacora.aspx`. | Redirige a `~/AccesoDenegado.aspx`. |

### 3.7 Cambio de contraseña

| Paso | Acción | Resultado esperado |
|---|---|---|
| 1 | Loguearse con un usuario en primer login. | Redirige a `~/CambiarContra/Cambiar-contra.aspx?usuario=X&modo=primerLogin`. |
| 2 | Cambiar la contraseña por una que cumpla complejidad. | Éxito y redirige a configurar preguntas o dashboard. |
| 3 | Intentar cambiar la contraseña por una usada anteriormente. | Mensaje: "No puedes reutilizar una contraseña anterior". |
| 4 | En modo normal (usuario logueado), acceder a CambiarContraseña desde el menú. | Pide contraseña actual; rechaza si es incorrecta. |
| 5 | Acceder a `~/CambiarContra/Cambiar-contra.aspx?usuario=admin&modo=recuperacion` sin haber pasado por preguntas de seguridad. | Redirige a `~/LogIn/LogIn.aspx` (no se permite cambio arbitrario). |

---

## 4. Script de verificación en BD

Ver `scripts/verificar-seguridad.sql`. Debe ejecutarse antes de las pruebas para confirmar que:

- Existe la tabla de control `DigitoVerificador` y tiene registros.
- Existen usuarios de prueba para cada rol (1-4).
- La tabla `PreguntasSeguridad` tiene al menos una pregunta por usuario.
- Las columnas `dvv`/`dvh` no son `NULL` ni vacías en las tablas principales.

---

## 5. Criterios de aceptación final

- [ ] Build de la solución sin errores.
- [ ] Anónimo solo accede a Inicio y LogIn.
- [ ] Error de integridad pausa el sistema para no-admins.
- [ ] Admin puede recalcular/restaurar desde `VerificacioDV.aspx`.
- [ ] Todos los métodos de `DalGeneral.cs` tienen etiqueta conectado/desconectado.
- [ ] `BLLRol` usa perfiles/permisos hardcodeados.
- [ ] Menú lateral se adapta al rol.
- [ ] No se exponen stack traces ni mensajes técnicos al usuario.
- [ ] Cambio de contraseña protegido (no permite cambios arbitrarios de cuentas ajenas).

---

> **Nota:** Estas pruebas son manuales. Para automatización futura se recomienda agregar tests de integración con Selenium/IIS Express y validar los puntos 3.2, 3.3 y 3.6.
