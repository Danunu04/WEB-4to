# GymAPP - Diferencias entre documento funcional y código implementado

> Archivo generado el 6 de julio de 2026.
> Proyecto: `C:\Users\Danunu\Desktop\WEB-4to\gymAppV2`

---

## Resumen ejecutivo

Se comparó el documento funcional `GymAPP.docx` contra el código del sistema. El proyecto tiene implementados robustos mecanismos de seguridad (encriptación, DVH/DVV, bitácora, login con bloqueo, historial de contraseñas) y un modelo de datos normalizado que en muchos puntos excede lo pedido por el documento.

Sin embargo, **faltan implementar los módulos operativos centrales del negocio**: pagos, inscripción a actividades con cupo, ABM real de clases, ABM de rutinas, registro de peso/1RM, gestión de familias y notificaciones/alertas.

Este documento lista:

1. Lo que **SÍ está implementado y coincide** con el documento.
2. Lo que está **parcialmente implementado o difiere**.
3. Lo que **falta implementar**.
4. Lo que el código tiene **de más respecto al documento**.

---

## 1. Requisitos implementados y coincidentes

| Requisito del documento | Implementación en el código |
|---|---|
| Roles: Administrador, Recepcionista, Entrenador, Cliente | `BE\PerfilesSistema.cs`; usado en `BLL\BLLUsuario` y `MPP\MPPUsuario`. |
| Login con intentos fallidos y bloqueo | `BLLUsuario.ValidarLogin`, `RegistrarIntentoFallido`, constante `MAX_INTENTOS_LOGIN`. |
| Contraseñas con hash + historial de reutilización | `CriptoManager.GenerarHashSHA256`, tabla `USUARIO_Contras`, `ContrasenaFueUtilizada`. |
| Encriptación reversible de datos personales | `MPPUsuario` encripta/desencripta `nombre`, `apellido`, `telefono`, `email`, `fechaNacimiento`. |
| Preguntas de seguridad encriptadas | `MPP\MPPPreguntaSeguridad.cs`. |
| Dígitos verificadores DVV/DVH | `SERVICIOS\DIGITOVERIFICADORMANAGER`, `MPP\MPPDigitoVerificador.cs`. |
| Bitácora de eventos | `BE\Evento`, `MPP\MPPEvento.cs`, filtros, estadísticas, criticidad, módulo. |
| ABM de usuarios | `gymAppV2\Usuarios\UsuariosModulo.aspx`. |
| ABM de alumnos | `gymAppV2\Alumnos\Alumnos.aspx` + `BLL\BLLAlumno` + `MPP\MPPAlumno`. |
| ABM de entrenadores | `gymAppV2\Entrenadores\Entrenadores.aspx` + `BLL\BLLEntrenador` + `MPP\MPPEntrenador`. |
| Calendario de actividades/clases | `gymAppV2\Actividades\actividades.aspx` (front-end de calendario). |
| Check-in de alumno por DNI | `gymAppV2\Inicio\Default.aspx` (pantalla pública de ingreso de DNI). |
| Backup / Restore | `MPP\MPPDigitoVerificador.RealizarBackup` / `RestaurarBackup` + `gymAppV2\Admin\BackupRestore.aspx`. |
| Singleton de sesión | `SERVICIOS\Singleton\SesionUsuario.cs` / `Singleton.cs`. |
| Cambio de contraseña con validación | `BLLUsuario.CambiarContrasena` + `CambiarContra\Cambiar-contra.aspx`. |

---

## 2. Requisitos parcialmente implementados o con diferencias

### 2.1. Modelo de datos de Alumnos / Entrenadores

- **Documento:** indica que los datos personales (Nombre, Apellido, DNI, Teléfono, Fecha de nacimiento) están en la tabla `Alumnos` y se encriptan allí.
- **Código:** los datos personales están centralizados en `USUARIOS`; `Alumnos` y `Entrenadores` solo guardan campos específicos del rol (`dni`, `peso`, `activo`, `tieneRutinas`, `usr`, `alumnosCount`). El DNI actúa como FK.
- **Impacto:** funcionalmente se puede registrar/modificar/ver, pero la arquitectura de datos no coincide literalmente con el documento.

### 2.2. Blanqueo de contraseña

- **Documento:** generar contraseña temporal: `Apellido + DNI + "!"` según rol, mostrarla al administrador.
- **Código:** `BLLUsuario.BlanquearContrasena` solo pone `primerLogin = 1`; no genera ni muestra una contraseña temporal. Existe `GenerarContrasenaSegura()` pero no se usa en el blanqueo.

### 2.3. Recuperación de contraseña con preguntas de seguridad

- **Documento:** pregunta fija *“¿En qué año naciste?”* y, si el usuario tiene más de un alumno, una segunda pregunta sobre si conoce a una persona.
- **Código:** hay una pregunta de seguridad genérica encriptada (`PreguntasSeguridad`), y existe la lógica de segunda pregunta por múltiples alumnos en `BLLPreguntaSeguridad.GenerarPreguntaSeguridadAlumno`, pero no se usa en el flujo de recuperación actual.

### 2.4. Subtipo “Familiar”

- **Documento:** el rol Cliente se subdivide en **Alumno** (un alumno, mismo DNI) y **Familiar** (varios alumnos, al menos uno con DNI distinto).
- **Código:** hay soporte para contar `CantidadAlumnosAsociados(string usuario)`, pero no hay una entidad `Familia` ni flujo explícito “Crear Familia” como describe el caso `CUS007`. El `UsuarioCrearDTO` permite un solo DNI de alumno.

### 2.5. Rutinas

- **Documento:** ABM de rutinas, registro de peso corporal, registro de 1RM, visualización por cliente.
- **Código:** existe la página `Rutinas\Rutinas.aspx` pero el code-behind indica: *“Módulo de rutinas en desarrollo”* y solo prepara paneles vacíos para cliente/admin. **Falta la capa de datos de Rutinas** (no hay BLL/MPP que operen `Rutinas`, `RutinaEjercicio`, `AlumnoRM`, `PesoHistorial`).

### 2.6. Calendario de clases / Actividades

- **Documento:** actividades con nombre, horario, cupo, inscripción/desinscripción, control de cupo.
- **Código:** el calendario muestra actividades determinísticamente distribuidas por día según `CodActividad`; no hay horarios reales, cupos, ni botones de inscribir/desinscribir. `BLLActividad` lista actividades por cliente, pero no existe ABM real de clases ni inscripciones.

### 2.7. Pagos / Abono mensual

- **Documento:** sección de pagos, visualización de vencimiento, monto según modalidad, registro de pago, comprobante.
- **Código:** existe `BE\PrecioModalidad` y `MPP\MPPPrecioModalidad` (con precios por días/semana), pero **no hay página de Pagos** ni flujo de pago. No se encontró módulo que registre pagos.

### 2.8. Check-in con membresía vencida

- **Documento:** al dar presente con membresía vencida, descontar día, alertar al recepcionista.
- **Código:** el check-in actual solo verifica `alumno.Activo` (no una fecha de vencimiento ni días restantes) y no genera la alerta al recepcionista. Usa `USUARIO_SISTEMA` para registrar el evento.

### 2.9. Notificaciones

- **Documento:** alerta interna/email ante cambio de precio de cuota; alerta automática al recepcionista ante check-in vencido.
- **Código:** no se encontró sistema de notificaciones por email ni banners/popups de alerta.

### 2.10. Permisos por módulo

- **Documento:** matriz de permisos detallada (Dashboard completo/reducido, G. Rutinas solo lectura/ABM, etc.).
- **Código:** `BE\PermisosSistema` define permisos, pero la autorización usa `BasePage.VerificarAcceso` con roles numéricos. No se implementa la matriz granular tipo Composite/Familias mencionada en el documento como “implementación futura”.

---

## 3. Requisitos no implementados (faltantes)

1. **Módulo de Pagos / Abono mensual**
   - Pantalla de pagos.
   - Selección de alumno.
   - Selección de medio de pago.
   - Generación de comprobante.
   - Registro de membresía activa con fecha de inicio/vencimiento.

2. **Inscripción/desinscripción a actividades con cupos**
   - No hay flujo de inscripción.
   - No hay control de cupos.
   - No hay desinscripción.

3. **ABM real de clases programadas**
   - El calendario actual es puramente visual.
   - No hay CRUD de clases con entrenador, horario y cupo.

4. **ABM de rutinas y ejercicios**
   - Incluye registro de peso corporal y 1RM.

5. **Gestión de Familias**
   - Crear usuario tipo Familiar con múltiples alumnos asociados.

6. **Alertas / Notificaciones**
   - Email y banners por cambio de precio.
   - Alerta automática al recepcionista ante check-in vencido.

7. **Recuperación de contraseña exacta del documento**
   - Pregunta fija del año de nacimiento.
   - Segunda pregunta por múltiples alumnos.

8. **Blanqueo de contraseña según especificación**
   - Generar y mostrar contraseña temporal `Apellido + DNI + "!"`.

9. **Dar presente con lógica de membresía**
   - Debería descontar días restantes.
   - Validar fecha de vencimiento.
   - Alertar al recepcionista.

---

## 4. Funcionalidades presentes en el código pero NO solicitadas en el documento

### 4.1. Roles y permisos extras

| Funcionalidad en código | Detalle |
|---|---|
| **Rol WebMaster (5)** | El documento define 4 roles. El código agrega `RolWebMaster = 5` con permisos idénticos a Administrador. |
| **Tablas de permisos compuestos** | `Familia`, `Permiso`, `PermisoFamilia`, `Perfil_Familia`, `Perfil_Permiso` están en BD, aunque el código no las use todavía. |

### 4.2. Seguridad y datos extras

| Funcionalidad en código | Detalle |
|---|---|
| **Historial de contraseñas (`USUARIO_Contras`)** | Guarda hasta 10 hashes históricos con DVH/DVV para evitar reutilización. |
| **Encriptación AES-256 de preguntas de seguridad** | Pregunta y respuesta encriptadas en `PreguntasSeguridad`. |
| **Segunda pregunta de seguridad por alumno** | Existe en `BLLPreguntaSeguridad.GenerarPreguntaSeguridadAlumno` pero no se usa en el flujo. |
| **Campo `primerLogin` + cambio forzoso** | Obliga a cambiar contraseña y configurar pregunta en el primer ingreso. |
| **Modelo de datos normalizado** | Datos personales centralizados en `USUARIOS`; `ALUMNOS`/`ENTRENADORES` solo campos específicos. |
| **Tipo `Empleado`** | Clasificación interna para Administrador y Recepcionista. |
| **Pausa del sistema por error de integridad** | `VerificacioDV.aspx` bloquea a usuarios no-administradores si falla un DVH/DVV. |
| **Página de mantenimiento de encriptación masiva** | `Admin/EncriptarDatos.aspx` protegida como mantenimiento. |

### 4.3. Pantallas y flujos extras

| Funcionalidad en código | Detalle |
|---|---|
| **Página pública de check-in (`Inicio/Default.aspx`)** | Kiosco sin login para ingresar DNI. |
| **Página “Mi Perfil” editable** | Permite al usuario logueado modificar sus datos personales. |
| **Asociar/desasociar usuario a alumno** | Flujo para ligar o quitar un usuario cliente de un alumno existente. |
| **Botones “Exportar”** | Presentes en usuarios y alumnos, aunque sin lógica implementada. |

### 4.4. UX / front-end extras

| Funcionalidad en código | Detalle |
|---|---|
| **`DashBoard.Master` con menú lateral y toast global** | Estructura de master page con `showToast`. |
| **Sistema de toasts** | Notificaciones visuales success/error/warning/info. |
| **Font Awesome 6, Bootstrap 5, variables CSS, responsive** | Decisiones de diseño no detalladas en el documento. |
| **Calendario puramente front-end** | `actividades.aspx` genera el calendario en JS y distribuye actividades por día según `CodActividad % 28`. |

---

## 5. Lista priorizada de trabajo pendiente

| Orden | Módulo | Prioridad | Esfuerzo estimado | Dependencias |
|---|---|---|---|---|
| 1 | **Pagos / Abono mensual** | Alta | Alto | — |
| 2 | **Check-in con lógica de membresía** | Alta | Medio | #1 (o campo mínimo de días en `ALUMNOS`) |
| 3 | **ABM real de clases e inscripciones con cupo** | Alta | Alto | — |
| 4 | **Rutinas, ejercicios, peso y 1RM** | Media-Alta | Medio-Alto | #3 si se asocia a actividad |
| 5 | **Gestión de Familias** | Media | Medio | — |
| 6 | **Blanqueo y recuperación exactos del documento** | Media | Bajo-Medio | #5 para la segunda pregunta |
| 7 | **Notificaciones / alertas** | Media | Medio | #1 y #3 |
| 8 | **Permisos por módulo granulares** | Baja | Alto | — |

### Recomendación de implementación

1. **Pagos / Abono mensual** desbloquea el resto: sin membresía activa no tiene sentido un check-in real ni alertas de vencimiento.
2. **Check-in con membresía** es la operación diaria más visible.
3. **Clases e inscripciones** y **Rutinas** son los módulos operativos centrales del gimnasio.
4. Por último, refinar **Familias**, **blanqueo/recuperación exactos** y **notificaciones**.

---

## 6. Conclusión

El proyecto tiene bien cubiertos los aspectos de **seguridad, usuarios, alumnos, entrenadores, bitácora, dígitos verificadores, encriptación, backup/restore y autenticación**.

Faltan o están incompletos los **módulos operativos centrales del negocio**: pagos, inscripción a actividades, ABM de clases, ABM de rutinas, registro de peso/1RM, familias y notificaciones.

Además, hay discrepancias entre el modelo de datos descrito en el documento (datos personales en `Alumnos`) y el código (datos personales en `USUARIOS`), y varias funcionalidades técnicas en el código que no están contempladas en el documento funcional.
