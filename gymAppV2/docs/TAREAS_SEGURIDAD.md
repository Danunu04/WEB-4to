# Tareas de Seguridad — Sportio GymApp

> Lista de tareas pequeñas y concretas para aplicar todo lo de seguridad del modelo de negocio (`LogicaNegocio.txt`) al sistema actual.
> Cada tarea debe poder ser tomada por un modelo pequeño, ser independiente y tener un criterio de aceptación claro.

---

## Cómo usar esta lista

- Marcá `[x]` cuando una tarea esté terminada y probada.
- Si una tarea requiere decisiones de diseño, crear una subtarea de análisis primero.
- Preferí siempre el camino más simple: cambiar lo justo para cumplir el modelo de negocio, sin reinventar.
- Antes de tocar código de seguridad, leé la sección correspondiente del modelo de negocio.

---

## Leyenda de prioridad

| Emoji | Prioridad | Significado |
|-------|-----------|-------------|
| 🔴 | Crítico | Bloquea la seguridad básica del sistema. Hacer primero. |
| 🟠 | Alto | Riesgo importante o requisito del modelo no implementado. |
| 🟡 | Medio | Mejora la defensa en profundidad. |
| 🟢 | Bajo | Pulido, consistencia o deuda técnica. |

---

## 1. Base de datos y esquema

> El script `ScriptCreacion.sql` no coincide con el código actual. La tabla `USUARIOS` necesita más columnas, y hay tablas de seguridad que faltan.

### 1.1 Alinear tabla USUARIOS con el código
- [x] 🔴 **1.1.1** Agregar columna `activo BIT NOT NULL DEFAULT 1` a `USUARIOS` en `ScriptCreacion.sql`.
  - Archivo: `ScriptCreacion.sql`.
  - Criterio: el script crea la tabla con la columna.

- [x] 🔴 **1.1.2** Agregar columna `bloqueado BIT NOT NULL DEFAULT 0` a `USUARIOS`.
  - Archivo: `ScriptCreacion.sql`.
  - Criterio: el script crea la columna y el valor por defecto es `0`.

- [x] 🔴 **1.1.3** Agregar columna `intentos INT NOT NULL DEFAULT 0` a `USUARIOS`.
  - Archivo: `ScriptCreacion.sql`.
  - Criterio: el script crea la columna y el valor por defecto es `0`.

- [x] 🔴 **1.1.4** Agregar columna `rol INT NOT NULL DEFAULT 4` a `USUARIOS`.
  - Archivo: `ScriptCreacion.sql`.
  - Criterio: el script crea la columna. Los valores posibles son 1 Admin, 2 Recepcionista, 3 Entrenador, 4 Cliente.

- [x] 🔴 **1.1.5** Agregar columna `tipo VARCHAR(50)` a `USUARIOS`.
  - Archivo: `ScriptCreacion.sql`.
  - Criterio: la columna almacena `'Empleado'`, `'Entrenador'`, `'Cliente'` o `'Familiar'`.

- [x] 🔴 **1.1.6** Agregar columnas de datos personales a `USUARIOS`: `nombre`, `apellido`, `telefono`, `email`, `fechaNacimiento`.
  - Archivo: `ScriptCreacion.sql`.
  - Criterio: las columnas existen con tipos adecuados y `NULL` permitido donde corresponda.

- [x] 🔴 **1.1.7** Agregar CHECK constraint a `USUARIOS.rol` para limitar valores a 1, 2, 3, 4.
  - Archivo: `ScriptCreacion.sql`.
  - Criterio: el script incluye `CONSTRAINT CK_USUARIOS_Rol CHECK (rol IN (1,2,3,4))`.

- [x] 🟠 **1.1.8** Eliminar o marcar como obsoleta la tabla `USUARIO_Intentos` si el código usa `USUARIOS.intentos`.
  - Archivo: `ScriptCreacion.sql`.
  - Criterio: no hay tablas huérfanas que confundan a nuevos desarrolladores.

### 1.2 Tablas de seguridad faltantes
- [x] 🔴 **1.2.1** Crear tabla `PreguntasSeguridad` en `ScriptCreacion.sql`.
  - Archivo: `ScriptCreacion.sql`.
  - Criterio: la tabla tiene `codPregunta INT IDENTITY PK`, `usr VARCHAR(50) FK`, `pregunta VARCHAR(500)`, `respuesta VARCHAR(500)`, `dvv`, `dvh`.

- [x] 🟠 **1.2.2** Confirmar que `USUARIO_Contras` (historial de contraseñas) existe con `dvv` y `dvh`.
  - Archivo: `ScriptCreacion.sql`.
  - Criterio: la tabla tiene PK compuesta `(usr, contra)` y FK a `USUARIOS(usr)`.

- [x] 🟡 **1.2.3** Agregar índice recomendado en `PreguntasSeguridad.usr`.
  - Archivo: `ScriptCreacion.sql`.
  - Criterio: existe `IX_PreguntasSeguridad_usr`.

### 1.3 Datos iniciales de seguridad
- [x] 🟠 **1.3.1** Crear script de datos iniciales que inserte al menos un usuario administrador activo.
  - Archivo: nuevo `ScriptDatosIniciales.sql`.
  - Criterio: el script es seguro (contraseña hasheada, no texto plano) y documenta cómo regenerar el hash.

- [x] 🟡 **1.3.2** Agregar comentarios en `ScriptCreacion.sql` explicando el propósito de cada tabla de seguridad.
  - Archivo: `ScriptCreacion.sql`.
  - Criterio: cada sección de seguridad tiene un comentario claro.

---

## 2. Autenticación

> El login funciona, pero tiene debilidades enumeradas en el análisis de errores y no cumple del todo con el modelo.

### 2.1 Mensajes de login seguros
- [x] 🔴 **2.1.1** Cambiar el mensaje de "Usuario no encontrado" en `LogIn.aspx.cs` por un mensaje genérico.
  - Archivo: `gymAppV2/LogIn/LogIn.aspx.cs`.
  - Criterio: tanto usuario inexistente como contraseña incorrecta muestran "Credenciales inválidas." sin diferenciar.

- [x] 🔴 **2.1.2** Mostrar "Intentos restantes" solo de forma controlada o eliminarlo para evitar enumeración.
  - Archivo: `gymAppV2/LogIn/LogIn.aspx.cs`.
  - Criterio: un atacante no puede saber si el usuario existe por el mensaje de error.

### 2.2 Validación de complejidad de contraseña en UI
- [x] 🟠 **2.2.1** Agregar validador de expresión regular en `LogIn.aspx` para longitud mínima 6.
  - Archivo: `gymAppV2/LogIn/LogIn.aspx`.
  - Criterio: el campo contraseña tiene `MinLength` o validador de 6 caracteres.

- [x] 🟠 **2.2.2** Agregar `MaxLength` a los campos de usuario y contraseña en `LogIn.aspx`.
  - Archivo: `gymAppV2/LogIn/LogIn.aspx`.
  - Criterio: usuario máximo 50 caracteres, contraseña máximo 128.

### 2.3 Bloqueo de cuenta
- [x] 🟡 **2.3.1** Reemplazar el magic number `3` en `MPPUsuario.AgregarIntento` por una constante `MAX_INTENTOS`.
  - Archivo: `MPP/MPPUsuario.cs`.
  - Criterio: existe una constante legible y se usa en la consulta SQL.

- [x] 🟡 **2.3.2** Reemplazar el magic number `3` en `BLLUsuario.ObtenerIntentosRestantes` por la misma constante.
  - Archivo: `BLL/BLLUsuario.cs`.
  - Criterio: usa la constante compartida o una pública en `BLLUsuario`.

- [x] 🟡 **2.3.3** Extraer la constante de intentos máximos a un lugar compartido (por ejemplo `BE/ConstantesSeguridad.cs`).
  - Archivo: nuevo `BE/ConstantesSeguridad.cs`.
  - Criterio: `BLL`, `MPP` y `UI` usan la misma constante.

### 2.4 Sesión y cookies
- [x] 🟠 **2.4.1** Verificar que `FormsAuthentication.SetAuthCookie` no sea redudante con el master.
  - Archivo: `gymAppV2/LogIn/LogIn.aspx.cs`.
  - Criterio: la cookie de forms y la sesión trabajan juntas sin duplicar lógica.

- [x] 🟠 **2.4.2** En `SesionUsuario.Usuario`, verificar `null` antes del cast para evitar `NullReferenceException`.
  - Archivo: `SERVICIOS/Singleton/SesionUsuario.cs`.
  - Criterio: el getter devuelve `null` de forma segura si `Session` no existe.

- [x] 🟡 **2.4.3** En `LogIn.aspx.cs`, mover la instanciación de `BLLUsuario` y `BLLEvento` a propiedades lazy en lugar de crear en cada postback.
  - Archivo: `gymAppV2/LogIn/LogIn.aspx.cs`.
  - Criterio: los objetos se crean solo cuando se usan.

### 2.5 Logout
- [x] 🟡 **2.5.1** Asegurar que `FormsAuthentication.SignOut()` se llame siempre en logout.
  - Archivo: `gymAppV2/DashBoard.Master.cs`.
  - Criterio: la cookie de forms se invalida al cerrar sesión.

- [x] 🟡 **2.5.2** Invalidar la sesión con `Session.Abandon()` en logout.
  - Archivo: `gymAppV2/DashBoard.Master.cs`.
  - Criterio: la sesión se destruye.

---

## 3. Recuperación y desbloqueo por preguntas de seguridad

> El modelo pide que, al bloquearse, el usuario responda preguntas de seguridad y luego cambie la contraseña. Hoy no existe este flujo.

### 3.1 Backend de preguntas de seguridad
- [x] 🔴 **3.1.1** Revisar `BLLPreguntaSeguridad.GenerarPreguntaSeguridad` para que lea la fecha de nacimiento real del usuario.
  - Archivo: `BLL/BLLPreguntaSeguridad.cs`.
  - Criterio: la pregunta "¿En qué año naciste?" incluye el año real del usuario o se valida contra él.

- [x] 🔴 **3.1.2** Crear método `CantidadAlumnosAsociados(string usuario)` en `BLLAlumno`.
  - Archivo: `BLL/BLLAlumno.cs`.
  - Criterio: devuelve la cantidad de alumnos vinculados al usuario.

- [x] 🔴 **3.1.3** Crear método en `BLLPreguntaSeguridad` para generar la segunda pregunta cuando el usuario tiene más de un alumno asociado.
  - Archivo: `BLL/BLLPreguntaSeguridad.cs`.
  - Criterio: si hay >1 alumno, genera pregunta "¿Conoce a X?" mezclando nombre real con nombres aleatorios.

- [x] 🔴 **3.1.4** Crear método `ValidarRespuesta(string usuario, string respuesta)` en `BLLPreguntaSeguridad`.
  - Archivo: `BLL/BLLPreguntaSeguridad.cs`.
  - Criterio: compara la respuesta (ignorando mayúsculas/minúsculas y espacios) con la almacenada.

- [x] 🟠 **3.1.5** Crear método `MPPUsuario.ObtenerFechaNacimiento(string usuario)` si no existe.
  - Archivo: `MPP/MPPUsuario.cs`.
  - Criterio: devuelve la fecha de nacimiento del usuario o `null`.

### 3.2 UI de preguntas de seguridad
- [x] 🔴 **3.2.1** Crear página `PreguntasSeguridad.aspx` con campo de usuario, pregunta y respuesta.
  - Archivo: `gymAppV2/LogIn/PreguntasSeguridad.aspx`.
  - Criterio: la página tiene controles ASP.NET para mostrar pregunta y recibir respuesta.

- [x] 🔴 **3.2.2** Implementar `PreguntasSeguridad.aspx.cs`: cargar pregunta según usuario.
  - Archivo: `gymAppV2/LogIn/PreguntasSeguridad.aspx.cs`.
  - Criterio: al ingresar un usuario bloqueado, muestra la pregunta correspondiente.

- [x] 🔴 **3.2.3** Implementar validación de respuesta y redirección a cambio de contraseña.
  - Archivo: `gymAppV2/LogIn/PreguntasSeguridad.aspx.cs`.
  - Criterio: respuesta correcta → redirige a `CambiarContra/Cambiar-contra.aspx?usuario=X`; incorrecta → mensaje de error.

- [x] 🔴 **3.2.4** Agregar en `LogIn.aspx.cs` redirección a `PreguntasSeguridad.aspx` cuando la cuenta está bloqueada.
  - Archivo: `gymAppV2/LogIn/LogIn.aspx.cs`.
  - Criterio: al capturar `ResultadosLogIn.AccountLocked`, se redirige al flujo de preguntas.

- [x] 🟠 **3.2.5** Agregar enlace "¿Olvidaste tu contraseña? / Cuenta bloqueada" en `LogIn.aspx`.
  - Archivo: `gymAppV2/LogIn/LogIn.aspx`.
  - Criterio: el enlace apunta a `PreguntasSeguridad.aspx`.

### 3.3 Almacenamiento de preguntas
- [x] 🟠 **3.3.1** Crear método `MPPPreguntaSeguridad.GuardarPregunta` para almacenar pregunta y respuesta.
  - Archivo: `MPP/MPPPreguntaSeguridad.cs`.
  - Criterio: inserta en `PreguntasSeguridad` con `dvv=''` y `dvh=''`.

- [x] 🟠 **3.3.2** Crear método `MPPPreguntaSeguridad.ObtenerPreguntaPorUsuario`.
  - Archivo: `MPP/MPPPreguntaSeguridad.cs`.
  - Criterio: devuelve la pregunta de seguridad del usuario.

- [x] 🟠 **3.3.3** Crear método `MPPPreguntaSeguridad.ObtenerRespuestaPorUsuario`.
  - Archivo: `MPP/MPPPreguntaSeguridad.cs`.
  - Criterio: devuelve la respuesta esperada.

- [x] 🟡 **3.3.4** Asegurar que al crear un usuario se genere la pregunta de seguridad automáticamente con "¿En qué año naciste?".
  - Archivo: `BLL/BLLUsuario.cs`.
  - Criterio: la pregunta se guarda durante `CrearUsuario`.

---

## 4. Cambio de contraseña

> La página `CambiarContra/Cambiar-contra.aspx` está vacía. El backend ya existe.

### 4.1 UI de cambio de contraseña
- [x] 🔴 **4.1.1** Agregar campo "Contraseña actual" a `Cambiar-contra.aspx`.
  - Archivo: `gymAppV2/CambiarContra/Cambiar-contra.aspx`.
  - Criterio: existe un `TextBox` con `TextMode="Password"`.

- [x] 🔴 **4.1.2** Agregar campo "Nueva contraseña".
  - Archivo: `gymAppV2/CambiarContra/Cambiar-contra.aspx`.
  - Criterio: existe `TextBox` de contraseña.

- [x] 🔴 **4.1.3** Agregar campo "Confirmar nueva contraseña".
  - Archivo: `gymAppV2/CambiarContra/Cambiar-contra.aspx`.
  - Criterio: existe `TextBox` de confirmación.

- [x] 🔴 **4.1.4** Agregar `RequiredFieldValidator` para los tres campos.
  - Archivo: `gymAppV2/CambiarContra/Cambiar-contra.aspx`.
  - Criterio: no se permite enviar vacío.

- [x] 🔴 **4.1.5** Agregar `CompareValidator` para que "Nueva" y "Confirmar" coincidan.
  - Archivo: `gymAppV2/CambiarContra/Cambiar-contra.aspx`.
  - Criterio: muestra error si no coinciden.

- [x] 🟠 **4.1.6** Agregar `RegularExpressionValidator` para complejidad mínima (6 caracteres, mayúscula, especial).
  - Archivo: `gymAppV2/CambiarContra/Cambiar-contra.aspx`.
  - Criterio: valida en cliente antes del postback.

- [x] 🟡 **4.1.7** Agregar botón "Guardar" y "Cancelar".
  - Archivo: `gymAppV2/CambiarContra/Cambiar-contra.aspx`.
  - Criterio: los botones disparan eventos server-side.

- [x] 🟡 **4.1.8** Aplicar estilos consistentes con el resto de la app (usar `rem`, colores del dashboard).
  - Archivo: `gymAppV2/CambiarContra/Cambiar-contra.aspx` y `Content/dashboard.css`.
  - Criterio: la página se ve como el resto del sistema.

### 4.2 Lógica de cambio de contraseña
- [x] 🔴 **4.2.1** Implementar `btnGuardar_Click` en `Cambiar-contra.aspx.cs`.
  - Archivo: `gymAppV2/CambiarContra/Cambiar-contra.aspx.cs`.
  - Criterio: llama a `BLLUsuario.CambiarContrasena(usuario, nuevaContrasena)`.

- [x] 🔴 **4.2.2** Validar en servidor que las contraseñas coincidan antes de llamar al BLL.
  - Archivo: `gymAppV2/CambiarContra/Cambiar-contra.aspx.cs`.
  - Criterio: si no coinciden, muestra error y no llama al BLL.

- [x] 🟠 **4.2.3** Verificar que la contraseña actual sea correcta antes de permitir el cambio.
  - Archivo: `gymAppV2/CambiarContra/Cambiar-contra.aspx.cs`.
  - Criterio: usa `BLLUsuario.ValidarLogin(usuario, contrasenaActual)` o similar.

- [x] 🟠 **4.2.4** Mostrar mensaje de éxito y redirigir al dashboard tras cambiar la contraseña.
  - Archivo: `gymAppV2/CambiarContra/Cambiar-contra.aspx.cs`.
  - Criterio: el usuario ve un toast de éxito y vuelve al dashboard.

- [x] 🟠 **4.2.5** Mostrar mensaje de error amigable si la nueva contraseña ya fue usada.
  - Archivo: `gymAppV2/CambiarContra/Cambiar-contra.aspx.cs`.
  - Criterio: captura la excepción del BLL y muestra "No puedes reutilizar una contraseña anterior".

### 4.3 Integración con flujos
- [x] 🟠 **4.3.1** Permitir que `Cambiar-contra.aspx` reciba `?usuario=` por query string para el flujo de desbloqueo.
  - Archivo: `gymAppV2/CambiarContra/Cambiar-contra.aspx.cs`.
  - Criterio: si hay query string, precarga el campo usuario en modo solo lectura.

- [x] 🟠 **4.3.2** Agregar enlace "Cambiar contraseña" en el menú lateral o en el header del dashboard.
  - Archivo: `gymAppV2/DashBoard.Master`.
  - Criterio: los usuarios logueados pueden acceder al cambio de contraseña.

- [x] 🟡 **4.3.3** Al crear un usuario, forzar el cambio de contraseña en el primer login.
  - Archivo: `BLL/BLLUsuario.cs` y `gymAppV2/LogIn/LogIn.aspx.cs`.
  - Criterio: si es primer login, redirige a cambio de contraseña antes del dashboard.
  - Nota: requiere agregar campo `primerLogin` o similar.

### 4.4 Historial de contraseñas
- [x] 🟠 **4.4.1** Guardar la contraseña inicial en `USUARIO_Contras` al crear usuario.
  - Archivo: `BLL/BLLUsuario.cs`.
  - Criterio: cuando se crea un usuario, el hash inicial se inserta en el historial.

- [x] 🟡 **4.4.2** Limitar el historial a las últimas N contraseñas (por ejemplo 10).
  - Archivo: `MPP/MPPUsuario.cs`.
  - Criterio: no se acumulan contraseñas infinitas.

---

## 5. Autorización y control de acceso por rol

> El modelo define una matriz de permisos por rol. Hoy no se aplica en ninguna página.

### 5.1 Helper central de permisos
- [x] 🔴 **5.1.1** Revisar `BLLRol.TieneAccesoAModulo` y asegurar que cubra todos los módulos del modelo.
  - Archivo: `BLL/BLLRol.cs`.
  - Criterio: la matriz coincide con `LogicaNegocio.txt` sección 3.

- [x] 🔴 **5.1.2** Crear método `UsuarioActualTieneAcceso(string modulo)` en `BLLRol`.
  - Archivo: `BLL/BLLRol.cs`.
  - Criterio: lee el rol del usuario en sesión y devuelve `bool`.

- [x] 🟠 **5.1.3** Crear excepción personalizada `AccesoDenegadoException`.
  - Archivo: `SERVICIOS/Excepciones/AccesoDenegadoException.cs`.
  - Criterio: se lanza cuando un usuario no tiene acceso.

### 5.2 Proteger cada página
- [x] 🔴 **5.2.1** Agregar verificación de permisos en `Page_Load` de `UsuariosModulo.aspx.cs`.
  - Archivo: `gymAppV2/Usuarios/UsuariosModulo.aspx.cs`.
  - Criterio: solo Admin/Recepcionista acceden; otros roles redirigen a acceso denegado.

- [x] 🔴 **5.2.2** Agregar verificación de permisos en `Page_Load` de `Alumnos.aspx.cs`.
  - Archivo: `gymAppV2/Alumnos/Alumnos.aspx.cs`.
  - Criterio: Admin/Recepcionista tienen ABM; Cliente solo lectura de sus alumnos; Entrenador sin acceso.

- [x] 🔴 **5.2.3** Agregar verificación de permisos en `Page_Load` de `Bitacora.aspx.cs`.
  - Archivo: `gymAppV2/Bitacora/Bitacora.aspx.cs`.
  - Criterio: solo Admin/Recepcionista.

- [x] 🔴 **5.2.4** Agregar verificación de permisos en `Page_Load` de la página de entrenadores.
  - Archivo: `gymAppV2/Entrenadores/*.aspx.cs`.
  - Criterio: solo Admin/Recepcionista acceden al ABM.

- [x] 🔴 **5.2.5** Agregar verificación de permisos en `Page_Load` de actividades/calendario.
  - Archivo: `gymAppV2/Actividades/*.aspx.cs`.
  - Criterio: Admin/Recepcionista CRUD; Cliente solo visualización de sus clases; Entrenador solo visualización.

- [x] 🔴 **5.2.6** Agregar verificación de permisos en `Page_Load` de rutinas.
  - Archivo: `gymAppV2/Rutinas/Rutinas.aspx.cs`.
  - Criterio: Entrenador ABM; Admin/Recepcionista ABM total; Cliente solo lectura.

- [x] 🟠 **5.2.7** Crear página `AccesoDenegado.aspx`.
  - Archivo: `gymAppV2/AccesoDenegado.aspx`.
  - Criterio: muestra mensaje claro y botón para volver al dashboard.

- [x] 🟠 **5.2.8** Configurar `Web.config` para redirigir a `AccesoDenegado.aspx` en errores 401/403.
  - Archivo: `gymAppV2/Web.config`.
  - Criterio: existe `<customErrors>` o `<httpErrors>` apropiado.

### 5.3 Menú lateral dinámico
- [x] 🔴 **5.3.1** En `DashBoard.Master.cs`, cargar el rol del usuario logueado.
  - Archivo: `gymAppV2/DashBoard.Master.cs`.
  - Criterio: el master puede leer `Singleton.Instancia.Usuario.USUARIO_Rol`.

- [x] 🔴 **5.3.2** Ocultar "Usuarios" en el menú si el rol no es Admin/Recepcionista.
  - Archivo: `gymAppV2/DashBoard.Master.cs`.
  - Criterio: `liUsuarios.Visible = false` para Cliente y Entrenador.

- [x] 🔴 **5.3.3** Ocultar "Alumnos" en el menú si el rol es Cliente o Entrenador.
  - Archivo: `gymAppV2/DashBoard.Master.cs`.
  - Criterio: solo Admin/Recepcionista ven el ABM.

- [x] 🔴 **5.3.4** Ocultar "Entrenadores" en el menú si el rol no es Admin/Recepcionista.
  - Archivo: `gymAppV2/DashBoard.Master.cs`.
  - Criterio: solo Admin/Recepcionista.

- [x] 🔴 **5.3.5** Ocultar "Bitácora" en el menú si el rol no es Admin/Recepcionista.
  - Archivo: `gymAppV2/DashBoard.Master.cs`.
  - Criterio: solo Admin/Recepcionista.

- [x] 🔴 **5.3.6** Ocultar "Actividades" si el rol es Entrenador.
  - Archivo: `gymAppV2/DashBoard.Master.cs`.
  - Criterio: el modelo dice que Entrenador no accede a este módulo.

- [x] 🟠 **5.3.7** Mostrar "Rutinas" según rol (todos, pero con funcionalidad distinta).
  - Archivo: `gymAppV2/DashBoard.Master.cs`.
  - Criterio: se muestra a todos; la página interna controla el ABM o lectura.

- [x] 🟠 **5.3.8** Agregar opción "Perfil" en el menú para Clientes.
  - Archivo: `gymAppV2/DashBoard.Master.cs`.
  - Criterio: solo Cliente (rol 4) ve el menú Perfil.

- [x] 🟠 **5.3.9** Agregar opción "Pagos" para Admin/Recepcionista y Cliente.
  - Archivo: `gymAppV2/DashBoard.Master.cs`.
  - Criterio: Entrenador no accede a pagos.

- [x] 🟡 **5.3.10** Eliminar o condicionar el JavaScript del modal 404 que bloquea links.
  - Archivo: `gymAppV2/DashBoard.Master`.
  - Criterio: el menú funciona con redirecciones reales; el modal 404 solo se usa para funcionalidades realmente no implementadas.

### 5.4 Verificación de propiedad de datos
- [x] 🟠 **5.4.1** En módulo "Perfil", asegurar que un Cliente solo vea/modifique sus propios datos.
  - Archivo: `gymAppV2/Perfil/Perfil.aspx.cs`.
  - Criterio: compara `Session["UsuarioLogueado"]` con el usuario que se intenta editar.

- [x] 🟠 **5.4.2** En listado de alumnos para Cliente, filtrar por los alumnos asociados al usuario.
  - Archivo: `gymAppV2/Alumnos/Alumnos.aspx.cs`.
  - Criterio: un Cliente-Familiar no ve alumnos de otros usuarios.

- [x] 🟠 **5.4.3** En rutinas para Cliente, mostrar solo las rutinas de sus alumnos asociados.
  - Archivo: `gymAppV2/Rutinas/Rutinas.aspx.cs`.
  - Criterio: filtra por `dniAlumno` relacionados al usuario. La lógica de filtrado está preparada; la carga de datos se conectará cuando exista `BLLRutina`.

- [x] 🟠 **5.4.4** En actividades para Cliente, mostrar solo las clases de sus alumnos inscriptos.
  - Archivo: `gymAppV2/Actividades/actividades.aspx.cs`.
  - Criterio: el Cliente no puede crear actividades; se filtran las actividades mediante `BLLActividad.ListarActividadesPorCliente` usando la tabla `Actividad_Alumno`. La capa de datos (`BE.Actividad`, `MPPActividad`, `BLLActividad`) fue creada y el calendario consume los datos serializados desde el servidor.

---

## 6. Encriptación

> El modelo pide encriptación reversible para datos de alumnos e irreversible (SHA-256) para contraseñas.
> **Ampliación:** se encripta también la tabla `PreguntasSeguridad` (pregunta y respuesta) porque contiene información sensible usada para recuperación de cuentas.

### 6.1 Seguridad de contraseñas
- [x] 🔴 **6.1.1** Evaluar si el modelo permite agregar salt al SHA-256.
  - Archivo: análisis interno.
  - Criterio: documento de decisión que confirme si se agrega salt o se mantiene SHA-256 puro.
  - Decisión: por ahora se mantiene SHA-256 puro para no romper contraseñas existentes. Agregar salt requiere migración forzosa de todos los usuarios.

- [x] 🟠 **6.1.2** Si se agrega salt, modificar `CriptoManager._686DPGetSHA256` para aceptar salt.
  - Archivo: `SERVICIOS/CriptoManager.cs`.
  - Criterio: el hash se calcula sobre `salt + contraseña`.
  - Nota: no aplica. Según decisión 6.1.1 se mantiene SHA-256 puro para no romper contraseñas existentes.

- [x] 🟠 **6.1.3** Si se agrega salt, almacenar el salt por usuario en `USUARIOS`.
  - Archivo: `ScriptCreacion.sql`, `BE/Usuario.cs`, `MPP/MPPUsuario.cs`.
  - Criterio: cada usuario tiene un salt único generado al crear la cuenta.
  - Nota: no aplica por la decisión 6.1.1.

- [x] 🟠 **6.1.4** Si se agrega salt, migrar contraseñas existentes o forzar cambio.
  - Archivo: script de migración.
  - Criterio: los usuarios antiguos deben cambiar contraseña si no tienen salt.
  - Nota: no aplica por la decisión 6.1.1.

### 6.2 Encriptación reversible de datos personales
- [x] 🔴 **6.2.1** Mover la clave AES de `CriptoManager` a `Web.config` (no hardcodear).
  - Archivo: `SERVICIOS/CriptoManager.cs`, `gymAppV2/Web.config`.
  - Criterio: la clave se lee de `ConfigurationManager.AppSettings["AesKey"]`.

- [x] 🔴 **6.2.2** Usar IV aleatorio por cada valor encriptado e incrustarlo en el ciphertext con un byte de versión.
  - Archivo: `SERVICIOS/CriptoManager.cs`, `gymAppV2/Web.config`.
  - Criterio: el formato es `Base64([versión 0x01] + [IV 16 bytes] + [ciphertext])`. `AesIV` se conserva en `Web.config` solo como fallback legacy.

- [x] 🟠 **6.2.3** Cambiar `_686DPGetAESDecrypt` para que devuelva `string` en lugar de `object`.
  - Archivo: `SERVICIOS/CriptoManager.cs`.
  - Criterio: el método devuelve `string` y los callers se actualizan.

- [x] 🟠 **6.4** Renombrar métodos crípticos de `CriptoManager` a nombres legibles.
  - Archivo: `SERVICIOS/CriptoManager.cs`.
  - Criterio: `_686DPGetSHA256` → `GenerarHashSHA256`, `_686DPGetAES256` → `EncriptarAES256`, etc.

- [x] 🟡 **6.2.5** Documentar en `Web.config` que la clave AES debe ser cambiada en producción.
  - Archivo: `gymAppV2/Web.config`.
  - Criterio: comentario claro junto a `AesKey` indicando que el IV es aleatorio por valor.

- [x] 🟡 **6.2.6** Mantener compatibilidad de desencriptación con el formato legacy (IV fijo).
  - Archivo: `SERVICIOS/CriptoManager.cs`, `SERVICIOS/CriptoMigracion.cs`.
  - Criterio: `DesencriptarAES256` prueba formato nuevo y luego legacy; `CriptoMigracion` re-encripta los valores legacy al nuevo formato.

### 6.3 Aplicar encriptación a datos personales
- [x] 🟠 **6.3.1** Encriptar `USUARIOS.nombre` al insertar/modificar.
  - Archivo: `MPP/MPPUsuario.cs`.
  - Criterio: usa `CriptoManager.EncriptarAES256` antes del INSERT/UPDATE.

- [x] 🟠 **6.3.2** Encriptar `USUARIOS.apellido` al insertar/modificar.
  - Archivo: `MPP/MPPUsuario.cs`.
  - Criterio: dato encriptado en BD.

- [x] 🟠 **6.3.3** Encriptar `USUARIOS.telefono` al insertar/modificar.
  - Archivo: `MPP/MPPUsuario.cs`.
  - Criterio: dato encriptado en BD.

- [x] 🟠 **6.3.4** Encriptar `USUARIOS.email` al insertar/modificar.
  - Archivo: `MPP/MPPUsuario.cs`.
  - Criterio: dato encriptado en BD.

- [x] 🟠 **6.3.5** Encriptar `USUARIOS.fechaNacimiento` al insertar/modificar.
  - Archivo: `MPP/MPPUsuario.cs`.
  - Criterio: dato encriptado en BD.

- [x] 🟠 **6.3.6** Desencriptar datos personales al leer en `MPPUsuario.ObtenerUsuario`.
  - Archivo: `MPP/MPPUsuario.cs`.
  - Criterio: el `Usuario` devuelto tiene los valores en texto plano.

- [x] 🟠 **6.3.7** Desencriptar datos personales al listar en `MPPUsuario.ListarUsuarios`.
  - Archivo: `MPP/MPPUsuario.cs`.
  - Criterio: el grid muestra datos legibles.

### 6.4 Encriptación de preguntas de seguridad
- [x] 🟠 **6.4.1** Encriptar `PreguntasSeguridad.pregunta` al insertar/modificar.
  - Archivo: `MPP/MPPPreguntaSeguridad.cs`.
  - Criterio: usa `CriptoManager.EncriptarAES256` antes del INSERT/UPDATE.

- [x] 🟠 **6.4.2** Encriptar `PreguntasSeguridad.respuesta` al insertar/modificar.
  - Archivo: `MPP/MPPPreguntaSeguridad.cs`.
  - Criterio: la respuesta se almacena encriptada reversiblemente.

- [x] 🟠 **6.4.3** Desencriptar pregunta y respuesta al leer.
  - Archivo: `MPP/MPPPreguntaSeguridad.cs`.
  - Criterio: el `PreguntaSeguridad` devuelto tiene valores en texto plano.

- [x] 🟠 **6.4.4** Desencriptar respuesta antes de validar en `MPPPreguntaSeguridad.ValidarRespuesta`.
  - Archivo: `MPP/MPPPreguntaSeguridad.cs`.
  - Criterio: compara el texto plano de la respuesta ingresada con la almacenada.

- [x] 🟡 **6.3.8** Evaluar si encriptar DNI de `USUARIOS` y `Alumnos`.
  - Archivo: análisis interno.
  - Criterio: decisión documentada. El modelo dice "datos de alumnos" reversibles.
  - Decisión: por ahora no se encripta DNI porque es clave foránea entre USUARIOS, ALUMNOS, RUTINAS y PAGOS; encriptarlo rompería JOINs y FKs. Los datos personales sensibles (nombre, apellido, teléfono, email, fecha de nacimiento) sí se encriptan.

### 6.5 Migración masiva de datos existentes a encriptación reversible
- [x] 🟠 **6.5.1** Crear componente `SERVICIOS/CriptoMigracion` que recorra tablas con datos personales.
  - Archivo: `SERVICIOS/CriptoMigracion.cs`.
  - Criterio: detecta automáticamente tablas y columnas, evita doble-encriptación y reporta estadísticas.

- [x] 🟠 **6.5.2** Crear BLL `BLLCriptoMigracion` que exponga `EncriptarTodo`.
  - Archivo: `BLL/BLLCriptoMigracion.cs`.
  - Criterio: la UI puede llamar a un solo método para encriptar todos los datos existentes.

- [x] 🟠 **6.5.3** Crear página `Admin/EncriptarDatos.aspx` para ejecutar la migración.
  - Archivo: `gymAppV2/Admin/EncriptarDatos.aspx`.
  - Criterio: solo administradores pueden acceder; muestra tabla de resultados con filas encriptadas, ya encriptadas y errores.

- [x] 🟡 **6.5.4** Documentar en `TAREAS_SEGURIDAD.md` el alcance de la migración.
  - Archivo: `docs/TAREAS_SEGURIDAD.md`.
  - Criterio: queda claro qué tablas/columnas se migran y que los valores ya encriptados se saltan.
  - Tablas cubiertas: `USUARIOS` (nombre, apellido, teléfono, email, fechaNacimiento), `PreguntasSeguridad` (pregunta, respuesta), `ALUMNOS`/`ENTRENADORES` legacy si aún tienen columnas personales.

---

## 7. Bitácora / Auditoría

> El backend de bitácora está avanzado pero faltan eventos por registrar y detalles de UI.

### 7.1 Registrar eventos de seguridad faltantes
- [x] 🟠 **7.1.1** Registrar check-in en el módulo de Inicio.
  - Archivo: `gymAppV2/Inicio/Default.aspx.cs`.
  - Criterio: cada check-in exitoso llama a `BLLEvento.RegistrarCheckin`.

- [x] 🟠 **7.1.2** Registrar intento de check-in con membresía vencida.
  - Archivo: `gymAppV2/Inicio/Default.aspx.cs`.
  - Criterio: se registra como evento `checkin` con criticidad 2 cuando el alumno está inactivo.
  - Nota: el campo de vencimiento de membresía no existe aún; se usa `Activo == false` como proxy temporal.

- [ ] 🟠 **7.1.3** Registrar pagos al realizar un pago.
  - Archivo: `gymAppV2/Pagos/*.aspx.cs`.
  - Criterio: llama a `BLLEvento.RegistrarPago`.
  - Nota: bloqueado — el módulo de pagos no existe aún.

- [ ] 🟠 **7.1.4** Registrar alta de rutina.
  - Archivo: `gymAppV2/Rutinas/*.aspx.cs`.
  - Criterio: llama a `BLLEvento.RegistrarRutinaAlta`.
  - Nota: bloqueado — el módulo de rutinas no existe aún.

- [ ] 🟠 **7.1.5** Registrar modificación de rutina.
  - Archivo: `gymAppV2/Rutinas/*.aspx.cs`.
  - Criterio: llama a `BLLEvento.RegistrarRutinaModificacion`.
  - Nota: bloqueado — el módulo de rutinas no existe aún.

- [ ] 🟠 **7.1.6** Registrar baja de rutina.
  - Archivo: `gymAppV2/Rutinas/*.aspx.cs`.
  - Criterio: llama a `BLLEvento.RegistrarRutinaBaja`.
  - Nota: bloqueado — el módulo de rutinas no existe aún.

- [ ] 🟠 **7.1.7** Registrar inscripción a actividad.
  - Archivo: `gymAppV2/Actividades/*.aspx.cs`.
  - Criterio: llama a `BLLEvento.RegistrarInscripcion`.
  - Nota: bloqueado — la inscripción a actividades no está implementada aún.

- [x] 🟠 **7.1.8** Registrar cambios de datos personales del perfil.
  - Archivo: `gymAppV2/Perfil/Perfil.aspx.cs`.
  - Criterio: llama a `BLLEvento.RegistrarCambioDatosUsuario`.

- [x] 🟠 **7.1.9** Registrar cambios de datos de alumno.
  - Archivo: `gymAppV2/Alumnos/Alumnos.aspx.cs`.
  - Criterio: llama a `BLLEvento.RegistrarCambioDatosAlumno`.
  - Implementación: se registran alta, modificación, baja, asociación/desasociación de usuario y cambio de datos del alumno.

### 7.2 Robustez de bitácora
- [x] 🟠 **7.2.1** Manejar `Session null` en `BLLEvento.RegistrarEvento` sin perder eventos críticos.
  - Archivo: `BLL/BLLEvento.cs`.
  - Criterio: si la sesión expiró, los eventos pre-auth siguen registrándose; post-auth lanzan excepción controlada.

- [x] 🟡 **7.2.2** Eliminar catch vacío en `BLLUsuario.RegistrarEvento` o loguear al menos a un fallback.
  - Archivo: `BLL/BLLUsuario.cs`.
  - Criterio: si falla el log, se escribe en un archivo de fallback o trace.

- [x] 🟡 **7.2.3** Usar constante para el usuario "sistema" en `BLLEvento`.
  - Archivo: `BLL/BLLEvento.cs`.
  - Criterio: existe `private const string USUARIO_SISTEMA = "sistema"`.

### 7.3 UI de bitácora
- [x] 🟠 **7.3.1** Inicializar propiedad `Expandido` en `MPPEvento.ObtenerEventos`.
  - Archivo: `MPP/MPPEvento.cs`.
  - Criterio: cada `Evento` tiene `Expandido = false` al cargar.

- [x] 🟠 **7.3.2** Implementar lógica de expansión/colapso de detalles en `Bitacora.aspx.cs`.
  - Archivo: `gymAppV2/Bitacora/Bitacora.aspx.cs`.
  - Criterio: al hacer click en "Ver detalles" se muestra/oculta el panel.

- [x] 🟡 **7.3.3** Agregar filtros rápidos para eventos de seguridad (bloqueo, desbloqueo, cambio de contraseña).
  - Archivo: `gymAppV2/Bitacora/Bitacora.aspx`.
  - Criterio: existen botones de filtro para esos tipos.

- [x] 🟡 **7.3.4** Mostrar criticidad con colores distintivos en la UI.
  - Archivo: `gymAppV2/Bitacora/Bitacora.aspx` y `Bitacora.css`.
  - Criterio: cada nivel de criticidad tiene un color/bandera; los nuevos tipos de seguridad también tienen estilos distintivos.

---

## 8. Configuración de seguridad en Web.config

- [x] 🔴 **8.1** Mover connection string de `DalGeneral.cs` a `Web.config`.
  - Archivo: `DAL/DalGeneral.cs`, `gymAppV2/Web.config`.
  - Criterio: `DalGeneral` lee de `ConfigurationManager.ConnectionStrings["GymAppConnection"].ConnectionString`.

- [x] 🔴 **8.2** Asegurar que `Web.config` tenga la sección `<connectionStrings>` con la cadena.
  - Archivo: `gymAppV2/Web.config`.
  - Criterio: la cadena existe y `DalGeneral` la lee correctamente.

- [x] 🟠 **8.3** Configurar cookies seguras en `Web.config`.
  - Archivo: `gymAppV2/Web.config`.
  - Criterio: existe `<httpCookies httpOnlyCookies="true" requireSSL="true" sameSite="Lax" />`.

- [x] 🟠 **8.4** Configurar forms authentication con `requireSSL="true"`, `protection="All"` y `slidingExpiration="true"`.
  - Archivo: `gymAppV2/Web.config`.
  - Criterio: la cookie de forms es segura.

- [x] 🟠 **8.5** Configurar `sessionState` con `cookieless="UseCookies"` y `regenerateExpiredSessionId="true"`.
  - Archivo: `gymAppV2/Web.config`.
  - Criterio: la sesión usa cookies y regenera IDs expirados.

- [x] 🟡 **8.6** Agregar encabezados de seguridad vía `system.webServer/httpProtocol`.
  - Archivo: `gymAppV2/Web.config`.
  - Criterio: existen `X-Frame-Options`, `X-Content-Type-Options`, `X-XSS-Protection`, `Referrer-Policy`, además de `Content-Security-Policy`.

- [x] 🟡 **8.7** Configurar `<customErrors>` para no mostrar detalles de excepciones en producción.
  - Archivo: `gymAppV2/Web.config`.
  - Criterio: modo `RemoteOnly` con redirección genérica a `AccesoDenegado.aspx` y handlers para 401/403/404/500.

- [x] 🟡 **8.8** Cambiar `<compilation debug="true">` a `false` para despliegue de producción.
  - Archivo: `gymAppV2/Web.config`.
  - Criterio: `debug="false"`. Para desarrollo local puede volver a `true` temporalmente.

---

## 9. Validaciones y UX de seguridad

### 9.1 Contraseñas iniciales seguras
- [x] 🔴 **9.1.1** Reemplazar la generación de contraseña inicial predecible en `UsuariosModulo.aspx.cs`.
  - Archivo: `gymAppV2/Usuarios/UsuariosModulo.aspx.cs`.
  - Criterio: la contraseña no es `Apellido + DNI` ni datos personales.

- [x] 🔴 **9.1.2** Reemplazar la generación de contraseña inicial predecible en `Alumnos.aspx.cs`.
  - Archivo: `gymAppV2/Alumnos/Alumnos.aspx.cs`.
  - Criterio: la contraseña es aleatoria o se fuerza cambio.

- [x] 🟠 **9.1.3** Usar `BLLUsuario.GenerarContrasenaAutomatica` o similar centralizado en ambos lugares.
  - Archivos: `gymAppV2/Usuarios/UsuariosModulo.aspx.cs`, `gymAppV2/Alumnos/Alumnos.aspx.cs`.
  - Criterio: no hay duplicación de lógica de generación.

- [x] 🟠 **9.1.4** Generar contraseña aleatoria segura (mínimo 10 caracteres, mayúscula, minúscula, número, especial).
  - Archivo: `BLL/BLLUsuario.cs`.
  - Criterio: cumple requisitos del modelo y es impredecible.

- [x] 🟠 **9.1.5** Forzar cambio de contraseña en primer login cuando se usa contraseña generada.
  - Archivo: `BLL/BLLUsuario.cs`, `gymAppV2/LogIn/LogIn.aspx.cs`.
  - Criterio: el usuario debe cambiar la contraseña antes de acceder al dashboard.

### 9.2 UX del login
- [ ] 🟡 **9.2.1** Agregar botón para mostrar/ocultar contraseña.
  - Archivo: `gymAppV2/LogIn/LogIn.aspx`.
  - Criterio: el usuario puede alternar visibilidad.

- [ ] 🟡 **9.2.2** Alinear estilos del login con las guías de CSS (usar `rem`).
  - Archivo: `gymAppV2/LogIn/LogIn.aspx`, `gymAppV2/Content/login.css`.
  - Criterio: no hay `px` en márgenes, paddings, radios ni bordes.

- [x] 🟡 **9.2.3** Agregar mensaje informativo cuando la cuenta fue bloqueada.
  - Archivo: `gymAppV2/LogIn/LogIn.aspx.cs`.
  - Criterio: indica que debe usar el flujo de preguntas de seguridad.

---

## 10. Módulos funcionales con seguridad

> Muchos módulos del modelo no existen o están vacíos. Estas tareas los completan con la seguridad necesaria.

### 10.1 Módulo Perfil (Cliente)
- [x] 🟠 **10.1.1** Crear página `Perfil.aspx` con master `DashBoard.Master`.
  - Archivo: `gymAppV2/Perfil/Perfil.aspx`.
  - Criterio: la página existe y carga datos del usuario logueado.

- [x] 🟠 **10.1.2** Mostrar datos personales del usuario en modo lectura.
  - Archivo: `gymAppV2/Perfil/Perfil.aspx`.
  - Criterio: campos precargados desde `Singleton.Instancia.Usuario`.

- [x] 🟠 **10.1.3** Permitir editar nombre, apellido, teléfono, email.
  - Archivo: `gymAppV2/Perfil/Perfil.aspx`.
  - Criterio: solo el propio usuario puede editar.

- [x] 🟠 **10.1.4** Guardar cambios del perfil llamando a `BLLUsuario.ModificarUsuario` o método dedicado.
  - Archivo: `gymAppV2/Perfil/Perfil.aspx.cs`.
  - Criterio: los cambios persisten y se registran en bitácora.

### 10.2 Módulo Pagos
- [ ] 🟠 **10.2.1** Crear página de pagos para Cliente mostrando vencimientos de sus alumnos.
  - Archivo: `gymAppV2/Pagos/PagosCliente.aspx`.
  - Criterio: lista fechas de vencimiento y montos.

- [ ] 🟠 **10.2.2** Crear página de pagos para Admin/Recepcionista con buscador por DNI/nombre/apellido.
  - Archivo: `gymAppV2/Pagos/PagosAdmin.aspx`.
  - Criterio: permite buscar alumno y registrar pago.

- [ ] 🟠 **10.2.3** Registrar pago y actualizar fecha de vencimiento del abono.
  - Archivo: `BLL/BLLPagos.cs` (crear si no existe).
  - Criterio: se actualiza estado del alumno y se registra en bitácora.

- [ ] 🟡 **10.2.4** Validar membresía vencida según reglas del modelo (días pagados o 30 días).
  - Archivo: `BLL/BLLPagos.cs`.
  - Criterio: devuelve `true`/`false` correctamente.

### 10.3 Módulo Inicio / Check-in
- [ ] 🟠 **10.3.1** Crear UI de check-in con campo DNI.
  - Archivo: `gymAppV2/Inicio/Default.aspx`.
  - Criterio: input de DNI y botón registrar.

- [ ] 🟠 **10.3.2** Validar membresía activa al hacer check-in.
  - Archivo: `gymAppV2/Inicio/Default.aspx.cs`.
  - Criterio: si está vigente, descuenta un día; si venció, muestra error y alerta a recepcionista.

- [ ] 🟠 **10.3.3** Enviar alerta automática al recepcionista cuando un alumno vencido intenta ingresar.
  - Archivo: `BLL/BLLPagos.cs` o servicio de notificaciones.
  - Criterio: se registra en bitácora con criticidad 2 y se muestra alerta en dashboard.

### 10.4 Precios de cuota
- [ ] 🟠 **10.4.1** Crear UI para ver y modificar precios de cuota (solo Admin/Recepcionista).
  - Archivo: `gymAppV2/Precios/Precios.aspx`.
  - Criterio: muestra las 4 modalidades y permite editar.

- [ ] 🟠 **10.4.2** Al modificar precio, registrar evento `modificacion_precio`.
  - Archivo: `BLL/BLLPrecioModalidad.cs`.
  - Criterio: ya existe; verificar que se llame.

- [ ] 🟠 **10.4.3** Implementar alerta interna (banner) para usuarios no entrenadores.
  - Archivo: `gymAppV2/DashBoard.Master`.
  - Criterio: cuando hay precios modificados recientemente, se muestra banner.

- [ ] 🟠 **10.4.4** Implementar envío de email a usuarios no entrenadores.
  - Archivo: `Servicios/EmailService.cs`.
  - Criterio: se envía email al cambiar precio (puede ser mock en desarrollo).

### 10.5 Familiares vs Alumnos
- [ ] 🟠 **10.5.1** Agregar campo/subtipo para distinguir Cliente-Alumno y Cliente-Familiar.
  - Archivo: `BE/Usuario.cs`, `ScriptCreacion.sql`.
  - Criterio: el sistema sabe si el DNI del usuario coincide con el alumno.

- [ ] 🟠 **10.5.2** Permitir asociar múltiples alumnos a un mismo usuario (Familiar).
  - Archivo: `BLL/BLLAlumno.cs`.
  - Criterio: un familiar puede gestionar varios alumnos.

- [ ] 🟠 **10.5.3** Adaptar UI de pagos para mostrar todos los alumnos de un familiar.
  - Archivo: `gymAppV2/Pagos/PagosCliente.aspx`.
  - Criterio: lista todos los DNIs asociados.

---

## 11. Integridad de datos (DVH/DVV)

- [x] 🟡 **11.1** Decidir si se implementa DVH/DVV o se eliminan las columnas.
  - Archivo: `docs/plan-dvv-dvh.md`.
  - Criterio: se implementa usando las columnas `dvv`/`dvh` de cada fila.
  - Decisión: DVH = hash de fila; DVV = hash acumulado de DVH por tabla. Ambos se guardan en cada fila y en la tabla de control `DigitoVerificador`.

- [x] 🟡 **11.2** Crear helper `DigitoVerificadorManager`.
  - Archivo: `SERVICIOS/DigitoVerificadorManager.cs`.
  - Criterio: calcula hash de fila y hash acumulado por columna con SHA-256; normaliza nulos, fechas y tipos numéricos.

- [x] 🟡 **11.3** Calcular DVH/DVH al insertar/actualizar cada tabla con MPP existente.
  - Archivo: `MPPUsuario`, `MPPAlumno`, `MPPEntrenador`, `MPPPreguntaSeguridad`, `MPPEvento`, `MPPPrecioModalidad`.
  - Criterio: cada INSERT/UPDATE actualiza `dvh` y `dvv` de la fila usando valores en texto plano (antes de encriptar columnas AES).

- [x] 🟡 **11.4** Crear tabla de control `DigitoVerificador` y MPP asociado.
  - Archivo: `bd-schema-v2.sql`, `MPP/MPPDigitoVerificador.cs`, `BLL/BLLDigitoVerificador.cs`.
  - Criterio: la tabla almacena `dvhTabla` (concatenación de hashes de fila) y `dvvTabla` (hash acumulado) por tabla; el MPP permite verificar y recalcular integridad global.

- [x] 🔴 **11.5** Crear página de verificación de integridad `VerificacioDV/VerificacioDV.aspx`.
  - Archivo: `gymAppV2/VerificacioDV/VerificacioDV.aspx`, `.aspx.cs`, `.css`, `.aspx.designer.cs`.
  - Criterio: el administrador ve tabla/campo con error, puede restaurar backup, recalcular todos los valores o salir; los no administradores ven pantalla de sistema pausado.
  - Incluye un grid de estado (`gvEstadoControl`) que muestra, por cada tabla con `dvv`/`dvh`, si tiene registro de control, cantidad de filas y cuántas tienen `dvh`/`dvv` vacíos; las filas con problemas se resaltan en naranja.

- [x] 🟡 **11.6** Integrar verificación de integridad en `BasePage` para pausar el sistema.
  - Archivo: `gymAppV2/BasePage.cs`.
  - Criterio: si existe error de integridad y el usuario no es admin, se redirige a `VerificacioDV.aspx` en cada carga de página protegida (excepto la propia página de verificación).

- [x] 🟡 **11.7** Registrar archivos DVH/DVV en los proyectos de la solución.
  - Archivo: `BE/BE.csproj` (incluye `ResultadoVerificacionDV.cs`, `EstadoControlDV.cs`), `BLL/BLL.csproj`, `MPP/MPP.csproj`, `gymAppV2/gymAppV2.csproj`, `SERVICIOS/SERVICIOS.csproj`.
  - Criterio: todos los archivos nuevos compilan con la solución.

- [x] 🟡 **11.8** Crear scripts SQL de migración para `DigitoVerificador` y recálculo de hashes.
  - Archivo: `scripts/crear-digito-verificador.sql`, `scripts/alter-dvv-dvh-varchar64.sql`, `scripts/recalcular-dvv-dvh.sql`.
  - Criterio: las columnas `dvv`/`dvh` son `VARCHAR(64) NOT NULL`; existe tabla de control; se puede recalcular masivamente.

- [ ] 🟢 **11.9** Cubrir tablas del schema que aún no tienen MPP.
  - Archivo: triggers SQL o MPP mínimos para `Rutinas`, `Ejercicio`, `AlumnoRM`, `PesoHistorial`, tablas de permisos y relaciones.
  - Criterio: ninguna fila nueva queda con `dvv`/`dvh` vacíos.
  - Nota: pendiente para cuando se implementen sus respectivas capas de datos.

---

## 12. Notificaciones

- [ ] 🟠 **12.1** Crear servicio `EmailService` con método `EnviarNotificacionCambioPrecio`.
  - Archivo: `Servicios/EmailService.cs`.
  - Criterio: recibe lista de destinatarios y datos del cambio.

- [ ] 🟠 **12.2** Configurar datos SMTP en `Web.config`.
  - Archivo: `gymAppV2/Web.config`.
  - Criterio: existe sección `system.net/mailSettings`.

- [ ] 🟠 **12.3** Implementar alerta interna persistente para notificaciones.
  - Archivo: `BE/Alerta.cs`, `BLL/BLLAlerta.cs`, `MPP/MPPAlerta.cs`.
  - Criterio: los usuarios logueados ven alertas no leídas.

- [ ] 🟡 **12.4** Marcar alertas como leídas cuando el usuario las cierra.
  - Archivo: `BLL/BLLAlerta.cs`.
  - Criterio: el estado persiste en BD.

---

## 13. Pruebas y hardening

- [ ] 🟠 **13.1** Crear lista de casos de prueba de seguridad.
  - Archivo: `docs/PRUEBAS_SEGURIDAD.md`.
  - Criterio: cubre login, bloqueo, preguntas, cambio de contraseña, permisos por rol.

- [ ] 🟠 **13.2** Probar que un Cliente no pueda acceder a `/Usuarios/UsuariosModulo.aspx`.
  - Criterio: redirige a acceso denegado.

- [ ] 🟠 **13.3** Probar que un Entrenador no pueda acceder a Bitácora.
  - Criterio: redirige a acceso denegado.

- [ ] 🟠 **13.4** Probar bloqueo tras 3 intentos fallidos.
  - Criterio: el botón se deshabilita y aparece flujo de preguntas.

- [ ] 🟠 **13.5** Probar cambio de contraseña con una contraseña usada anteriormente.
  - Criterio: el sistema rechaza la operación.

- [ ] 🟡 **13.6** Revisar que no haya contraseñas en texto plano en comentarios o scripts.
  - Criterio: no se encuentran contraseñas hardcodeadas.

- [ ] 🟡 **13.7** Auditar todos los queries SQL para confirmar que usan parámetros.
  - Criterio: no hay concatenación de strings en queries.

- [ ] 🟢 **13.8** Revisar que `ViewState` no guarde objetos sensibles (contraseñas, tokens).
  - Criterio: no se persiste información sensible en ViewState.

- [ ] 🟢 **13.9** Agregar comentarios de seguridad en cada método crítico de `BLLUsuario`.
  - Archivo: `BLL/BLLUsuario.cs`.
  - Criterio: cada método de autenticación tiene un comentario breve explicando su propósito.

---

## 14. Deuda técnica relacionada a seguridad

- [x] 🟢 **14.1** Crear clase base `BasePage` con métodos `MostrarToast`, verificación de sesión y permisos.
  - Archivo: `gymAppV2/BasePage.cs`.
  - Criterio: todas las páginas protegidas heredan de `BasePage`.

- [ ] 🟢 **14.2** Mover métodos `MostrarError`/`MostrarExito` duplicados a `BasePage`.
  - Archivo: `gymAppV2/BasePage.cs`.
  - Criterio: se eliminan duplicados de `UsuariosModulo.aspx.cs`, `Alumnos.aspx.cs`, etc.

- [ ] 🟢 **14.3** Implementar `IDisposable` en `DalGeneral` para cerrar conexiones correctamente.
  - Archivo: `DAL/DalGeneral.cs`.
  - Criterio: no quedan conexiones abiertas ni campos públicos compartidos.

- [ ] 🟢 **14.4** Reemplazar `ArrayList` por `List<SqlParameter>` en todos los MPP.
  - Archivo: todos los `MPP/*.cs`.
  - Criterio: no hay `ArrayList` en capa MPP.

- [ ] 🟢 **14.5** En `DashBoard.Master.cs`, agregar `!IsPostBack` a la redirección de sesión.
  - Archivo: `gymAppV2/DashBoard.Master.cs`.
  - Criterio: evita loops de redirección.

---

## Checklist final de entrega

Antes de considerar que la seguridad del modelo está aplicada:

- [ ] Todos los items 🔴 de esta lista están completados.
- [ ] El login cumple: usuario único, contraseña SHA-256, 3 intentos, bloqueo, preguntas de seguridad, cambio de contraseña.
- [ ] Cada página verifica el rol antes de cargar.
- [ ] El menú lateral se adapta al rol.
- [ ] Los datos personales sensibles están encriptados reversiblemente.
- [ ] La bitácora registra login, logout, bloqueos, pagos, check-ins, cambios de datos y precios.
- [ ] El `Web.config` tiene cookies seguras y encabezados de seguridad.
- [ ] No hay contraseñas predecibles ni hardcodeadas.
- [ ] Existe documentación de pruebas de seguridad y pasó al menos una revisión manual.

---

> **Nota para el equipo:** no intentes resolver toda la lista de una sola vez. Agarrá las tareas 🔴 primero, luego 🟠, y avanzá por área. Cada tarea debe terminar en un commit pequeño y una prueba manual mínima.
