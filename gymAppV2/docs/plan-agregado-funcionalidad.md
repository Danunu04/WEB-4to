# Plan — Puesta en marcha de las pantallas nuevas (Pagos, Permisos, Entrenadores, Rutinas)

## Contexto

En la sesión anterior se completaron en código las 4 pantallas que estaban vacías o marcadas como "en desarrollo" en el sidebar: **Pagos**, **Permisos**, **Entrenadores** y **Rutinas**. Todo el código (BE/BLL/MPP, `.aspx`, `.aspx.cs`, `.css`, cambios en `DashBoard.Master` y `PermisosSistema`/`BLLRol`) ya está escrito y comiteado.

Lo que falta es **ponerlo en marcha y validarlo**, porque el entorno donde se escribió el código (macOS, sin Visual Studio ni SQL Server) no permite compilar ni ejecutar la app. Este documento es la lista de pasos pendientes para que cada pantalla funcione de verdad, ordenada de la más simple a la más compleja.

Los `.aspx.designer.cs` de los módulos nuevos se escribieron a mano (no los generó Visual Studio), así que el primer riesgo real es que al abrir el proyecto aparezcan errores de compilación por algún control mal declarado — hay que revisar eso módulo por módulo.

## Paso 0 — Requisitos previos (una sola vez, antes de tocar cualquier módulo)

- [ ] Abrir `gymAppV2.sln` en Visual Studio y hacer un **build completo de la solución** (`Ctrl+Shift+B`). Anotar cualquier error de compilación — lo más probable es que aparezca en algún `.aspx.designer.cs` si algún control del `.aspx` no quedó bien declarado.
- [ ] Correr contra la base de datos el script `gymAppV2/scripts/agregar-traducciones-entrenadores-rutinas-pagos-permisos.sql` (SSMS o `sqlcmd`). Sin esto, las 4 pantallas nuevas van a mostrarse igual pero con los textos en español fijo (fallback), no vas a poder probar el cambio de idioma.
- [ ] Confirmar con qué usuario/rol vas a probar cada pantalla. Como referencia (definido en `BLLRol.TieneAccesoAModulo`):
  - **Pagos**: cualquier rol excepto Entrenador.
  - **Permisos**: solo Administrador (o WebMaster, que tiene acceso a todo).
  - **Entrenadores**: Administrador o Recepcionista.
  - **Rutinas**: Administrador, Recepcionista o Entrenador (vista completa); Cliente (vista de solo lectura, filtrada a sus alumnos).

---

## 1. Pagos (implementado — 2026-09-01, pendiente de probar contra una base real)

Se construyó el módulo real siguiendo el diseño acordado más abajo: `BE.Pago`, `MPP.MPPPago`, `BLL.BLLPago`
(mismo patrón que `MPPRutina`/`BLLRutina`: `DalGeneral` + `DigitoVerificadorManager`, solo `dvh`). La UI
(`Pagos/PagosCliente.aspx`) ahora tiene dos vistas como Rutinas: `pnlAdmin` (historial + alta de pago) y
`pnlCliente` (solo lectura, historial propio vía `Alumnos.usr`). El alta es: elegir alumno, elegir modalidad
(autocompleta el monto desde `BLLPrecioModalidad`), período (se toma el mes de la fecha elegida) y método de
pago (Efectivo/Transferencia/Tarjeta); reusa `BLLEvento.RegistrarPago` para la bitácora. No se agregó
modificación/baja de pagos — un pago registrado queda como registro fijo, solo se valida no duplicar
alumno+período (`UQ_Pagos_AlumnoPeriodo` + chequeo previo en `BLLPago.ExistePagoEnPeriodo`).

De paso se corrigió un bug real: `DAL/DalTraduccion.cs` (`TABLAS[]`) no incluía `Pantalla_Entrenadores`,
`Pantalla_Pagos` ni `Pantalla_Permisos`, así que ninguna de esas pantallas iba a cargar sus traducciones
aunque el script SQL se corriera. Ya está agregado.

Scripts nuevos a correr contra la base, en este orden:
1. `scripts/crear-tabla-pagos.sql` — crea la tabla `Pagos` (no existía; `PrecioModalidad` sí existe desde `bd-schema-v2.sql`).
2. `scripts/agregar-traducciones-entrenadores-rutinas-pagos-permisos.sql` — si todavía no se corrió (Paso 0).
3. `scripts/agregar-traducciones-pagos-real.sql` — tags nuevos del formulario de alta (alumno, modalidad, período, método, monto).

Pendiente de validar en Visual Studio/SQL Server real (no se pudo compilar ni correr desde este entorno macOS):
- [ ] Build completo — es el primer módulo que usa `TextMode="Date"` + `AutoPostBack` en un `DropDownList` (`ddlModalidad`) dentro de este proyecto; confirmar que el postback de "cambio de modalidad" autocompleta `lblMontoPreview` sin perder los demás valores del formulario.
- [ ] Alta de un pago como Admin/Recepcionista: elegir alumno, modalidad, período, método, guardar. Confirmar que aparece en la tabla y que el monto guardado coincide con el precio vigente de la modalidad.
- [ ] Intentar cargar dos pagos para el mismo alumno+mes y confirmar que se rechaza con el mensaje amigable (no una excepción SQL cruda por el `UNIQUE`).
- [ ] Vista Cliente: loguearse con un Cliente con alumnos asociados y pagos cargados; confirmar que solo ve el historial de sus propios alumnos y no tiene botón de "Registrar pago".
- [ ] Loguearse con un Entrenador y confirmar que el link "Pagos" sigue sin aparecer en el sidebar.
- [ ] Cambiar el idioma desde la pantalla "Idioma" y confirmar que título, historial, formulario y hint del período cambian (esto ahora depende del fix de `DalTraduccion.TABLAS` de arriba).

**Diseño acordado para el módulo real (2026-09-01)** — cuando se quiera construir, va así:

- **Concepto**: un pago es la **cuota mensual de un alumno**, según su modalidad (ya existe `BE.PrecioModalidad` con 4 tarifas: 1/2/3 días por semana o Diario — pero apunta a una tabla `PrecioModalidad` que **no existe** en la base; hay que crearla antes o junto con la de Pagos).
- **Quién lo carga**: Admin/Recepcionista registra el pago manualmente (efectivo/transferencia, sin pasarela online). Cliente solo consulta su propio historial — de ahí que la página ya se llame `PagosCliente.aspx`.
- **Tabla nueva sugerida**:
  ```sql
  CREATE TABLE Pagos (
      codPago       INT           IDENTITY(1,1) NOT NULL,
      dni           INT           NOT NULL,               -- FK a Alumnos.dni
      modalidadId   INT           NOT NULL,               -- FK a PrecioModalidad.Id
      periodo       DATE          NOT NULL,                -- primer día del mes que cubre (ej. 2026-09-01)
      monto         DECIMAL(10,2) NOT NULL,                -- copiado de PrecioModalidad al momento del pago
      fechaPago     DATETIME      NOT NULL,
      metodoPago    VARCHAR(30)   NOT NULL,               -- 'Efectivo' | 'Transferencia' | 'Tarjeta'
      usuarioRegistro VARCHAR(50) NOT NULL,               -- quién lo cargó (Recepcionista/Admin)
      dvh           VARCHAR(64)   NOT NULL,
      CONSTRAINT PK_Pagos PRIMARY KEY (codPago),
      CONSTRAINT FK_Pagos_Alumno FOREIGN KEY (dni) REFERENCES Alumnos(dni),
      CONSTRAINT FK_Pagos_Modalidad FOREIGN KEY (modalidadId) REFERENCES PrecioModalidad(Id),
      CONSTRAINT UQ_Pagos_AlumnoPeriodo UNIQUE (dni, periodo)  -- evita cargar el mismo mes dos veces
  );
  ```
- **Capas**: `BE.Pago`, `MPPPago` (usa `DalGeneral` + `DigitoVerificadorManager`, mismo patrón que `MPPRutina`), `BLLPago` con `RegistrarPago` (¡ojo! ya existe `BLLEvento.RegistrarPago(usuario, dniAlumno, monto, medioPago)` sin usar — reusar ese para la bitácora).
- **UI**: vista Admin/Recepcionista con tabla + alta de pago (elige alumno, modalidad autocompleta el monto, fecha, método); vista Cliente de solo lectura con su propio historial (mismo patrón que la vista Cliente de Rutinas).
- **Pendiente de decidir cuando se arranque**: si un mes sin pago registrado debe marcarse como "vencido" automáticamente (necesitaría un job o un cálculo on-the-fly comparando `periodo` contra la fecha actual), o si por ahora alcanza con un simple historial de pagos sin ese seguimiento de morosidad.

---

## 2. Permisos (simple — placeholder + un permiso nuevo)

Mismo tratamiento que Pagos, pero con una diferencia: se agregó un permiso (`GestionPermisos`) que no existía antes.

- [ ] Loguearse como Administrador (o WebMaster) y confirmar que "Permisos" aparece en el sidebar, ya no muestra el modal 404, y carga `Permisos/Permisos.aspx`.
- [ ] Loguearse con cualquier otro rol (Recepcionista, Entrenador, Cliente) y confirmar que el link "Permisos" **no aparece** en el sidebar.
- [ ] Intentar entrar directo por URL (`/Permisos/Permisos.aspx`) con un usuario sin permiso y confirmar que redirige a `AccesoDenegado.aspx` (esto prueba que `VerificarAcceso(PermisosSistema.GestionPermisos)` está funcionando).
- [ ] Con el script de traducciones corrido, probar el cambio de idioma igual que en Pagos.

**Decisión de arquitectura (2026-09-01)**: se descartó migrar a las tablas `Familia`/`Permiso`/`Perfiles` (existen en el esquema pero están huérfanas — nada del código las usa hoy). Se mantiene el enfoque hardcodeado de `BLLRol.TieneAccesoAModulo`. Migrar sería un refactor de la autorización de toda la app, con riesgo de dejar mal mapeado el acceso a algún módulo — no vale la pena para lo que se necesita.

**Diseño acordado para el módulo real** (reemplaza el placeholder cuando se quiera construir): pantalla de **solo consulta**, sin edición. Una matriz Permiso × Rol que refleja lo que ya decide `BLLRol.TieneAccesoAModulo` en código:

| Permiso | Administrador | Recepcionista | Entrenador | Cliente |
|---|---|---|---|---|
| GestionAlumnos | ✔ | ✔ | | ✔ (propio) |
| GestionEntrenadores | ✔ | ✔ | | |
| GestionRutinas | ✔ | ✔ | ✔ | |
| Pagos | ✔ | ✔ | | ✔ |
| ... | | | | |

(WebMaster no se lista aparte: tiene acceso a todo siempre, es el primer `if` de `TieneAccesoAModulo`.)

Implementación: recorrer `PermisosSistema.Todos` y para cada uno llamar `BLLRol.TieneAccesoAModulo(rol, permiso)` con los 4 roles (`PerfilesSistema.RolAdministrador/RolRecepcionista/RolEntrenador/RolCliente`), armar la matriz en el code-behind y bindearla a una tabla. No hace falta tocar `BLLRol` ni la base — es un reporte de lo que ya existe. Si el día de mañana se quiere permitir editar esto desde la UI, ahí sí habría que replantear si vale la pena migrar a las tablas de la base.

---

## 3. Entrenadores (medio — CRUD completo, pero el backend ya existía)

Acá sí hay lógica real para probar: alta, baja, modificación, y la relación con el sistema de Usuarios.

- [ ] **Alta de un entrenador nuevo**: completar DNI, Nombre, Apellido, Teléfono, Fecha de Nacimiento y guardar. Verificar en la base que se creó:
  - una fila en `USUARIOS` con usuario `entrenador_<DNI>` y una contraseña generada,
  - una fila en `Entrenadores` con ese mismo DNI.
- [ ] Verificar que las estadísticas del header (Total, Activos, Con alumnos, Sin usuario) se actualizan después del alta.
- [ ] **Modificación**: cambiar el estado Activo/Inactivo de un entrenador existente y confirmar que se refleja en la tabla y en la pill de estado.
- [ ] **Baja**: eliminar un entrenador que tenga rutinas asignadas y confirmar que no rompe nada (`BLLEntrenador.EliminarEntrenador` borra en cascada `Actividad_Entrenador` y `Rutinas` antes de borrar el entrenador — probarlo después de tener Rutinas funcionando, ver módulo 4).
- [ ] **Caso de error esperado**: intentar crear un entrenador con un DNI que ya existe y confirmar que se muestra el mensaje de error en vez de duplicar el registro o crear un usuario huérfano en `USUARIOS`.
- [ ] Confirmar que la pantalla respeta el diseño del resto del sistema (ya no es la página standalone de antes) y que el link del sidebar (`/Entrenadores/Entrenadores.aspx`) navega bien.
- [ ] Con el script de traducciones corrido, probar el cambio de idioma (título, stats, botones, cartel de confirmación de borrado).

**Puntos a vigilar si algo falla:**
- Si el alta tira error, revisar que `BLLUsuario.CrearUsuario` con `rol: 3` siga esperando los mismos parámetros (`datosEntrenador`, `fechaNacimiento` obligatoria) — no se tocó ese método, pero es la pieza más frágil de la integración.
- Si las columnas de la tabla salen vacías (Teléfono, Fecha de Nacimiento), es porque `MPPEntrenador` las trae con un `LEFT JOIN` contra `USUARIOS.dni` — confirmar que ese join realmente devuelve datos en tu base actual.

---

## 4. Rutinas (el más complejo — capa de datos nueva de punta a punta)

Es el módulo con más riesgo porque `BE/Rutina.cs`, `MPP/MPPRutina.cs` y `BLL/BLLRutina.cs` son código nuevo que nunca corrió contra una base real. Antes de probar la pantalla, conviene validar el modelo de datos.

- [ ] **Verificar el esquema real de la tabla `Rutinas`** en tu base actual: columnas `codRutina, descripcion, fecha, dniAlumno, dniEntrenador, codActividad, dvh` (sin `dvv`). El código se escribió basándose en `scripts/bd-schema-v2.sql` + `scripts/migration-simplificar-dv.sql`, pero si tu base tiene alguna diferencia (por ejemplo si todavía tiene la columna `dvv`), `MPPRutina.cs` va a fallar al insertar/actualizar. Si falta o sobra alguna columna, avisame y ajustamos las consultas.
- [ ] **Verificar que `USUARIOS` tenga los datos necesarios** para los JOIN de `MPPRutina` (nombre/apellido de alumno y entrenador vía `USUARIOS.dni`) — es el mismo patrón que ya usa `MPPEntrenador` y `MPPAlumno`, así que si Entrenadores (módulo 3) funcionó bien, esto debería estar cubierto.
- [ ] Antes de poder crear una rutina hace falta tener **al menos un Alumno, un Entrenador y una Actividad ya cargados** en el sistema — si las listas desplegables aparecen vacías, cargar datos de prueba en esos tres módulos primero.
- [ ] **Alta de una rutina** (como Admin/Recepcionista/Entrenador): elegir Alumno, Entrenador y Actividad de los combos, poner fecha y descripción, guardar. Confirmar que aparece en la tabla con los nombres correctos (no solo los DNI).
- [ ] **Modificación**: editar una rutina existente y confirmar que los combos se precargan con los valores actuales.
- [ ] **Baja**: eliminar una rutina y confirmar que desaparece de la lista.
- [ ] **Vista Cliente**: loguearse con un usuario Cliente que tenga alumnos asociados (con `usr` seteado en la tabla `Alumnos`) y con rutinas cargadas para esos alumnos. Confirmar que:
  - ve únicamente las rutinas de sus propios alumnos (no las de otros),
  - no ve botones de Crear/Modificar/Eliminar (la vista Cliente es de solo lectura).
- [ ] Probar el caso de un Cliente **sin alumnos asociados o sin rutinas**: debería ver el estado vacío ("No hay rutinas asignadas todavía") sin errores.
- [ ] Repetir la prueba de baja de un Entrenador con rutinas asignadas (pendiente del módulo 3) ahora que Rutinas ya tiene datos reales.
- [ ] Con el script de traducciones corrido, probar el cambio de idioma en ambas vistas (Cliente y Admin).

**Puntos a vigilar si algo falla:**
- Si el alta tira una excepción de SQL, lo primero a mirar es el `INSERT` en `MPPRutina.CrearRutina` contra el nombre real de columnas de tu tabla `Rutinas`.
- El cálculo del dígito verificador (`dvh`) de una rutina no incluye `codRutina` (autogenerado) — si más adelante corrés una herramienta de "Recalcular DV" o "Verificar Integridad" sobre esta tabla y da error, es el primer lugar donde mirar.
- Si el Cliente ve rutinas que no le corresponden, revisar que la columna `Alumnos.usr` esté bien cargada para ese alumno (el filtro de `MPPRutina.ListarRutinasPorCliente` depende de eso).

---

## Resumen de orden sugerido

1. Paso 0 (una vez).
2. Pagos.
3. Permisos.
4. Entrenadores.
5. Rutinas.

Si en cualquier punto algo no compila o no anda como se espera, mejor pararse ahí y revisarlo juntos antes de seguir al siguiente módulo — los últimos dos (Entrenadores y Rutinas) comparten datos entre sí (una rutina necesita un entrenador cargado), así que un problema en Entrenadores se va a arrastrar a las pruebas de Rutinas.
