# Errores encontrados (versión para IA)

Revisión estática del código de `gymAppV2` (ASP.NET Web Forms, .NET Framework 4.7.2, capas BE/DAL/MPP/BLL/SERVICIOS/gymAppV2), hecha el 17/11/2026.
**No se modificó ningún archivo de código.** Cada número coincide con el mismo número de `Herroes_Humano.md`.

Rutas relativas a `gymAppV2/` (la carpeta de la solución). Los números de línea son de esta revisión y pueden moverse.

**Convenciones del proyecto que hay que respetar al corregir:**
- CSS en `rem`.
- Textos con `Texto(clave, respaldo)` o `T(clave)`, y traducciones nuevas por script en `scripts/` (tablas `[Traducciones].[Pantalla_*]`, IdiomaID 1 ES, 2 EN, 3 PT, 4 FR, 5 JA).
- Toda fila nueva o modificada de una tabla con `dvh` necesita `MPPDigitoVerificador.RecalcularDvhFilas(tabla, condicion, params)`.
- Reglas de negocio con `ReglaNegocioException(clave, mensajeEs, args)`, que `BasePage.MensajeDePermisos` traduce.
- Después de cambios en la base: VerificacioDV → "Recalcular dígitos".

---

## 1. 🔴 `RedirigirSeguro` no corta la ejecución de la página

**Dónde:**
- `gymAppV2/BasePage.cs:381` (`RedirigirSeguro`)
- `BasePage.cs:369` (`VerificarAcceso`)
- `BasePage.cs:205` (`OnInit`, cuando no hay sesión)

**Qué pasa:**
- `RedirigirSeguro` hace `Response.Redirect(url, false)` + `Context.ApplicationInstance.CompleteRequest()`. Con `endResponse: false`, el ciclo de vida de la página continúa:
  - `Page_Load` sigue después de `VerificarAcceso(...)` (por ejemplo `Usuarios/UsuariosModulo.aspx.cs:40-51` carga la grilla igual).
  - Se disparan los eventos de postback (`btnGuardar_Click`, etc.).
  - La página se renderiza dentro del cuerpo de la respuesta 302.
- Un usuario sin permiso (o sin sesión) puede leer el HTML con un proxy y hacer postbacks que ejecutan acciones.
- Pocas BLL controlan el permiso por su cuenta: `BLLPermiso.ValidarQuePuedeGestionar` sí; `BLLUsuario.CrearUsuario`, `BLLPago.RegistrarPago` y `BLLRutina.CrearRutina`, no.

**Cómo arreglarlo** (en `BasePage`, una sola vez para todo el sitio):
```csharp
private bool _redirigiendo;
protected bool Redirigiendo { get { return _redirigiendo; } }

protected void RedirigirSeguro(string url)
{
    _redirigiendo = true;
    try { Response.Redirect(ResolveUrl(url), false); Context.ApplicationInstance.CompleteRequest(); }
    catch (HttpException) { }
}

// No dispara eventos de controles si se está redirigiendo
protected override void RaisePostBackEvent(IPostBackEventHandler sourceControl, string eventArgument)
{
    if (_redirigiendo) return;
    base.RaisePostBackEvent(sourceControl, eventArgument);
}

// No escribe el HTML de la página en la respuesta 302
protected override void Render(HtmlTextWriter writer)
{
    if (_redirigiendo) return;
    base.Render(writer);
}
```
- En cada `Page_Load` que llama a `VerificarAcceso`, agregar `if (Redirigiendo) return;` a continuación. Otra opción: que `VerificarAcceso` devuelva `bool` y usar `if (!VerificarAcceso(...)) return;`.
- Los eventos de controles con `AutoPostBack` (`SelectedIndexChanged`, `TextChanged`) no pasan por `RaisePostBackEvent`: también se cortan con el `return` en `Page_Load`, o con un chequeo `if (Redirigiendo) return;` al inicio de los handlers críticos.
- Defensa en profundidad: chequear el permiso en las BLL de escritura con `new BLLRol().UsuarioActualTieneAcceso(PermisosSistema.X)`, como hace `BLLPermiso`.

---

## 2. 🔴 Recuperación de contraseña: inicia sesión completa antes de cambiarla

**Dónde:**
- `gymAppV2/LogIn/PreguntasSeguridad.aspx.cs:134-179`: con la respuesta correcta llama a `BllUsuario.LogearUsuario(usuarioBD)` + `FormsAuthentication.SetAuthCookie(...)` y recién después redirige a `Cambiar-contra.aspx?modo=recuperacion&token=...`.
- `CambiarContra/Cambiar-contra.aspx.cs` (`ValidarFlujoRecuperacion`): sí controla `USUARIO_Activo`, pero la sesión ya está iniciada.

**Qué pasa:**
- Con la sesión ya iniciada, el usuario puede ir directo a `/DashBoard/WebForm1.aspx` sin cambiar la contraseña. `BasePage.OnInit` solo mira `Singleton.Instancia.IsLogged()`.
- La cuenta sigue con `bloqueado = 1` en la base.
- Un usuario **inactivo** también entra: `ValidarRespuesta` no mira `USUARIO_Activo`.

**Cómo arreglarlo:**
- En `PreguntasSeguridad.aspx.cs`, **no** llamar a `LogearUsuario` ni a `SetAuthCookie`. Guardar solo el token: `Session["Recuperacion_" + usuario] = token;`.
- Antes de validar la respuesta, cortar si la cuenta no existe o está inactiva (`usuarioBD == null || !usuarioBD.USUARIO_Activo`), con el mensaje genérico.
- `Cambiar-contra.aspx.cs` ya acepta `modo=recuperacion` sin sesión (rama `ValidarFlujoRecuperacion`). Al cambiar la contraseña ya hace `DesbloquearUsuario`, `ReestablecerIntentos`, `Session.Remove` y redirige al login. No hace falta tocar nada más ahí.
- Verificar que `Cambiar-contra.aspx` no herede de `BasePage`. Hoy no hereda (hereda de `Page`), así que funciona sin sesión.

---

## 3. 🔴 Pregunta de seguridad: bloqueo de cuentas ajenas e intentos ilimitados

**Dónde:** `gymAppV2/LogIn/PreguntasSeguridad.aspx.cs`
- `:82` `btnContinuar_Click` acepta cualquier usuario.
- `:185-205`: cada respuesta incorrecta llama a `BllUsuario.BloquearUsuario(usuario)` y deja el panel de la pregunta abierto.

**Qué pasa:**
- Cualquier visitante anónimo puede bloquear cualquier cuenta, incluido el admin (denegación de servicio).
- No hay límite de intentos de respuesta (fuerza bruta). Si se aplica el punto 2, la fuerza bruta igual lleva al cambio de contraseña.
- Hay enumeración de usuarios: el mensaje "no tiene configurada una pregunta" es distinto del genérico.

**Cómo arreglarlo:**
- Solo ofrecer la pregunta si la cuenta **ya está bloqueada** (`mppUsuario.UsuarioEstaBloqueado`). Así una cuenta activa no se puede bloquear desde acá. El login ya redirige a esta pantalla cuando la cuenta se bloquea.
- Contar los intentos de respuesta. Por ejemplo, una columna nueva `USUARIOS.intentosPregunta` o una tabla nueva con DV. Una opción más simple: `Session["IntentosPregunta_" + usuario]`, que de todos modos se reinicia con una sesión nueva.
- Después de 3 intentos, dejar la cuenta bloqueada "hasta que la desbloquee un administrador" y no volver a mostrar la pregunta. Registrar en la bitácora.
- Usar un mensaje genérico único para "no existe", "no tiene pregunta" y "respuesta incorrecta".

---

## 4. 🔴 Restore: si falla, la base queda en `SINGLE_USER`

**Dónde:** `MPP/MPPDigitoVerificador.cs:696-783` (`RestaurarBackup`). El lote es:
```sql
ALTER DATABASE [X] SET SINGLE_USER WITH ROLLBACK IMMEDIATE;
RESTORE DATABASE [X] FROM DISK = @Ruta WITH MOVE ..., REPLACE, STATS = 5;
ALTER DATABASE [X] SET MULTI_USER;
```

**Qué pasa:**
- Si `RESTORE` falla (archivo corrupto, falta el certificado TDE, falta espacio), el `ALTER ... MULTI_USER` no se ejecuta y la base queda en `SINGLE_USER`.
- Tampoco se valida la extensión `.bak`, como sí hace `RealizarBackup`.

**Cómo arreglarlo:**
```sql
ALTER DATABASE [X] SET SINGLE_USER WITH ROLLBACK IMMEDIATE;
BEGIN TRY
    RESTORE DATABASE [X] FROM DISK = @Ruta WITH MOVE @LogicalData TO @Mdf, MOVE @LogicalLog TO @Ldf, REPLACE, STATS = 5;
    ALTER DATABASE [X] SET MULTI_USER;
END TRY
BEGIN CATCH
    ALTER DATABASE [X] SET MULTI_USER;
    THROW;
END CATCH
```
- Otra opción: en C#, un `finally` con un segundo `SqlCommand` que ejecute `ALTER DATABASE [X] SET MULTI_USER` sobre `connMaster`.
- Agregar la validación de `Path.GetExtension(rutaBackup) == ".bak"`.
- Antes de reemplazar, opcionalmente correr `RESTORE VERIFYONLY FROM DISK = @Ruta`. Eso también cubre el pendiente CUS008 ("verificar el .bak").

---

## 5. 🔴 `ImportarBacpac` borra la base antes de comprobar que existe SqlPackage

**Dónde:** `MPP/MPPDigitoVerificador.cs:814-853`. Hace `DROP DATABASE` y recién después `BuscarSqlPackage()` (que lanza una excepción si no lo encuentra) y `EjecutarSqlPackage`.

**Qué pasa:** si SqlPackage no está instalado, o la importación falla (archivo inválido, versión), la base queda **borrada**.

**Cómo arreglarlo:**
1. Llamar a `string sqlPackage = BuscarSqlPackage();` **antes** del `DROP`.
2. No borrar la base actual: importar a una base temporal (`/TargetDatabaseName:{nombreBase}_import_{yyyyMMddHHmmss}`). Si sale bien:
   - `ALTER DATABASE [X] SET SINGLE_USER WITH ROLLBACK IMMEDIATE`, renombrar la vieja a `X_old` con `ALTER DATABASE ... MODIFY NAME`.
   - Renombrar la temporal a `X`.
   - Borrar `X_old` en un segundo paso.
3. Como mínimo, hacer un `RealizarBackup` automático a `C:\GymApp\antes_import_{fecha}.bak` antes del `DROP`.
4. `/TargetServerName:.` está fijo: tomar el servidor de la cadena de conexión con `SqlConnectionStringBuilder.DataSource`.

---

## 6. 🔴 Peso en Alumnos: cultura es-AR e input `type=number`

**Dónde:** `gymAppV2/Alumnos/Alumnos.aspx.cs`
- `:372`: `txtPeso.Text = alumno.Peso?.ToString("F2")`. Con cultura `es-AR` (`Web.config:31`) produce `"70,00"`.
- `:730`, `:784` y `:1076`: `decimal.TryParse(txtPeso.Text, out decimal p)`, con la cultura actual.
- `Alumnos.aspx:205`: `txtPeso` tiene `TextMode="Number"`.

**Qué pasa:**
- El input `type=number` del navegador rechaza el valor `"70,00"` y lo muestra vacío. Al guardar, `txtPeso.Text` llega vacío → `peso = null` → `AlumnosWS.ModificarAlumno` (`WebServices/AlumnosWS.asmx.cs:71`) asigna `alumno.Peso = null` → **se borra el peso**.
- El navegador envía `"70.5"`. En `es-AR` el punto es separador de miles, así que lo lee como `705` y lo rechaza por el rango de 20 a 300 kg.
- Perfil, Rutinas, CalculoRM y Precios ya usan `CultureInfo.InvariantCulture`; Alumnos es el único inconsistente.

**Cómo arreglarlo:**
```csharp
// :372
txtPeso.Text = alumno.Peso?.ToString("0.##", CultureInfo.InvariantCulture) ?? "";
// :730, :784, :1076
decimal.TryParse(txtPeso.Text, NumberStyles.Number, CultureInfo.InvariantCulture, out decimal p)
```
- Agregar `using System.Globalization;`.
- Opcional: si el campo queda vacío al modificar, conservar el peso anterior en lugar de mandar `null`.

---

## 7. 🟠 El "Estado" de Modificar alumno saltea la baja lógica

**Dónde:**
- `Alumnos/Alumnos.aspx.cs`, la modificación (alrededor de `:1080`) manda `Activo = chkActivo.Checked` a `AlumnosWS.ModificarAlumno` (`WebServices/AlumnosWS.asmx.cs:71-87`), que hace `alumno.Activo = solicitud.Activo; bllAlumno.ActualizarAlumno(alumno)`.
- La baja correcta es `BLLAlumno.DarDeBajaAlumno` (`BLL/BLLAlumno.cs:371`), que además desactiva las cuentas Cliente o Familiar sin otro alumno activo.

**Qué pasa:**
- Destildar "Estado" desactiva al alumno sin desactivar sus cuentas.
- Reactivar con el check no reactiva las cuentas que había desactivado la baja.

**Cómo arreglarlo:**
- Opción A (simple): sacar `chkActivo` también del formulario de modificación (`filaEstado.Visible = false` siempre). Dejar la baja solo con el botón de baja, y agregar un botón "Reactivar" que llame a un nuevo `BLLAlumno.ReactivarAlumno(dni)`. Ese método pone `Activo = true` en el alumno y reactiva las cuentas vinculadas desactivadas, con `RecalcularDigitos` de USUARIOS y la bitácora.
- Opción B: en `AlumnosWS.ModificarAlumno`:
  - Si `alumno.Activo && !solicitud.Activo`, llamar a `DarDeBajaAlumno(dni)` en lugar de asignar el campo.
  - Si `!alumno.Activo && solicitud.Activo`, llamar a `ReactivarAlumno(dni)`.
  - Para el resto, solo el peso.

---

## 8. 🟠 Datos de relleno para cuentas de empleado

**Dónde:**
- `BLL/BLLUsuario.cs:534-545` (sobrecarga con parámetros): `dni = dniAlumno ?? (999999990 + rol)`, `nombre ?? "Empleado"`, `apellido ?? usuario`, `fechaNacimiento ?? 1990-01-01`, `telefono ?? "0000-0000"`.
- `BLL/BLLUsuario.cs:645-656` (sobrecarga con `UsuarioCrearDTO`): para un Empleado **siempre** pisa los datos con valores de relleno (`dni = 999999990 + dto.Rol`, `nombre = "Empleado"`, fecha 1990-01-01). Ignora lo que vino en el DTO. Un segundo empleado del mismo rol choca con `UK_USUARIOS_DNI`.

**Qué pasa:**
- Hay datos falsos guardados como reales.
- `scripts/ver-usuarios-incompletos.sql` los ve como completos (están encriptados y no son NULL).

**Cómo arreglarlo:**
- Quitar los valores de relleno. Si falta un dato obligatorio, lanzar `Exception` con el mensaje (por ejemplo "El DNI es obligatorio") y dejar NULL lo opcional.
- En la sobrecarga del DTO, usar `dto.AlumnoDNI`, `dto.AlumnoNombre`, etc., también para el Empleado.
- Para detectar los existentes: `SELECT usr, dni FROM USUARIOS WHERE dni BETWEEN 999999991 AND 999999996`. Agregar esa condición al script de incompletos como 'DNI de relleno'. El teléfono "0000-0000" y la fecha 1990-01-01 están encriptados y solo se ven desde la pantalla.

---

## 9. 🟠 Cambiar el DNI en Usuarios borra fichas sin transacción

**Dónde:** `BLL/BLLUsuario.cs:828-960` (`ModificarUsuario`)
- Rol 3 con DNI cambiado: `bllEntrenador.EliminarEntrenador(dniViejo)`. Según el aviso de la pantalla Entrenadores, eso "eliminará también sus rutinas y actividades asociadas".
- Rol 4: `bllAlumno.EliminarAlumno(dniViejo)`, borrado físico; falla si el alumno tiene pagos.
- Después: `mppUsuario.ActualizarUsuario`, crear el entrenador o alumno nuevo, renombrar vínculos y `bllPermiso.AplicarCambioDeCuenta`. Son operaciones separadas, sin `TransactionScope`.

**Qué pasa:**
- Se pierden rutinas y actividades por corregir un DNI mal tipeado.
- Si falla un paso intermedio, los datos quedan a medias.

**Cómo arreglarlo:**
- Lo más seguro para la entrega: **no permitir cambiar el DNI** de Entrenador ni Cliente desde Usuarios. Lanzar una regla del estilo "El DNI de un entrenador o alumno no se puede cambiar: dalo de baja y crealo de nuevo", y en la pantalla poner `txtDNI.Enabled = false` al modificar esos roles.
- Si se quiere permitir: hacer `UPDATE` en cascada del DNI en las tablas hijas (Entrenadores, ActividadHorario, Rutinas, Alumnos, ALUMNOS_USUARIOS, Pagos, etc.), recalcular los DVH de cada una y envolver todo en `System.Transactions.TransactionScope`. Es costoso y tiene riesgo.

---

## 10. 🟠 La verificación de integridad completa corre en cada request

**Dónde:**
- `gymAppV2/BasePage.cs:232` (`VerificarIntegridadSiAplica`) llama a `BllDV.ExisteErrorIntegridad()` en **cada** página.
- Eso ejecuta `BLL/BLLDigitoVerificador.cs:32` `VerificarIntegridad()`, que para `USUARIOS`, `PreguntasSeguridad` y `Evento` recorre **todas** las filas (con desencriptación AES en USUARIOS) además del hash de cada tabla.
- `Inicio/Default.aspx.cs` y `Cambiar-contra.aspx.cs` también la llaman.

**Qué pasa:** la tabla `Evento` crece con cada acción, así que la latencia de cada página es O(filas de la bitácora).

**Cómo arreglarlo:**
- En las verificaciones por request, comparar solo el hash de tabla (`VerificarHashTabla`, DVV). Ir fila por fila solo si el DVV no coincide, o solo en la pantalla VerificacioDV.
- Cachear el resultado `bool` en `HttpRuntime.Cache` por unos 30–60 segundos e invalidarlo en `SincronizarControl` y `RecalcularDigitos`. Ejemplo: `HttpRuntime.Cache.Insert("IntegridadOk", true, null, DateTime.UtcNow.AddSeconds(30), Cache.NoSlidingExpiration)`.

---

## 11. 🟠 Secretos versionados y hash de contraseñas sin sal

**Dónde:**
- `gymAppV2/Web.config:20` `AesKey`, `:23` `AesIV` (el IV de configuración no se usa para cifrar: `SERVICIOS/CriptoManager.cs:70` genera un IV aleatorio por valor, lo cual está bien).
- `SERVICIOS/CriptoManager.cs:36` `GenerarHashSHA256` sin sal ni iteraciones.

**Cómo arreglarlo:**
- Mover `AesKey` a un archivo fuera del repositorio (`<appSettings file="secretos.config">`, con `secretos.config` en `.gitignore`), o cifrar la sección con `aspnet_regiis -pe "appSettings"`. Rotar la clave implica volver a encriptar los datos con `CriptoMigracion`.
- Contraseñas: usar PBKDF2 (`Rfc2898DeriveBytes` con sal aleatoria de 16 bytes, 100.000 iteraciones y SHA256), guardado como `v2$iter$salBase64$hashBase64` en `USUARIOS.contra`.
  - En el login: si el valor empieza con `v2$`, validar con PBKDF2; si no, validar con SHA-256 y, si es correcto, rehashear a v2 y guardar (migración transparente).
  - `ContrasenaFueUtilizada` tiene que comparar con cada formato del historial.
  - Recalcular el DV de USUARIOS y de USUARIO_Contras.
- Es grande: dejarlo para después de la entrega si no hay tiempo.

---

## 12. 🟠 Manejo de errores: todo termina en "Acceso denegado"

**Dónde:**
- `gymAppV2/Global.asax.cs:64-108` (`Application_Error`) hace `Response.Redirect("~/AccesoDenegado.aspx")` (`:102`).
- `Web.config:47` `customErrors mode="RemoteOnly" defaultRedirect="~/AccesoDenegado.aspx"`.
- `Web.config:29` `compilation debug="true"`.
- `requestValidationMode="2.0"` (`Web.config:30`) con `validateRequest` activo: cualquier `<` en un campo lanza `HttpRequestValidationException`, que termina en "Acceso denegado".

**Cómo arreglarlo:**
- Crear `Error.aspx` (hereda de `Page`, no de `BasePage`, para evitar bucles) con un mensaje amable ("Ocurrió un problema. Volvé a intentarlo") y un botón al Dashboard o al login.
- En `Application_Error`:
  - Si `ex` (o su `InnerException`) es `HttpRequestValidationException`, redirigir a `Error.aspx?motivo=texto`, con el mensaje "Hay caracteres no permitidos (< o >) en algún campo".
  - Si no, a `Error.aspx`.
- `Web.config`: `customErrors mode="On" defaultRedirect="~/Error.aspx"` y `compilation debug="false"` para la entrega.
- `AccesoDenegado.aspx` queda solo para los permisos.

---

## 13. 🟡 `Eval` sin codificar en el markup

**Dónde:** `Eval("Nombre")`, `Eval("Apellido")`, `Eval("ActividadDescripcion")`, `Eval("EntrenadorNombre")`, `Eval("EntrenadorApellido")`, `Eval("Parentesco")`, `Eval("NombreUsuario")` y otros en `<%# ... %>`. Buscar con `grep -rn '<%# Eval("' --include=*.aspx`.

**Qué pasa:** es un riesgo de XSS persistente, hoy mitigado por la validación de pedidos (punto 12).

**Cómo arreglarlo:**
- Cambiar `<%# Eval("X") %>` por `<%#: Eval("X") %>` (codifica en HTML) en los campos que vienen de datos ingresados por usuarios.
- No tocar los que devuelven HTML armado a propósito: `Eval("Arbol")` en Permisos, `GetIconForType`, `BadgeEstado` y `AvisoNoPuedePagar`.

---

## 14. 🟡 Doble envío: pagos y transferencias duplicados

**Dónde:**
- `Pagos/PagosCliente.aspx.cs`: `btnConfirmarPagoCliente_Click` y `btnGuardar_Click`.
- `BLL/BLLTransferencia.cs`: `Confirmar` registra el pago antes de `Resolver`, que es condicional (`WHERE estado = 'Pendiente'`), pero el pago ya se creó.
- `BLLPago.Registrar` permite un abono adelantado, así que el segundo clic crea uno.

**Cómo arreglarlo:**
- En el cliente: deshabilitar el botón al hacer clic, con `onclick="this.disabled=true; ..."` y `UseSubmitBehavior=false`, o un flag en JS.
- En `BLLTransferencia.Confirmar`:
  1. Primero, "tomar" la transferencia con `UPDATE ... SET estado='Aprobada' ... WHERE codTransferencia=@Cod AND estado='Pendiente'`, usando `mppTransferencia.Resolver` con `codPago = null`. Si devuelve `false`, lanzar `YaResuelta()`.
  2. Después, `RegistrarPago`. Si falla, volver el estado a 'Pendiente'.
  3. Por último, actualizar `codPago`, con un método nuevo `MPPTransferencia.AsignarPago(cod, codPago)` + `RecalcularDvhFilas`.
- Opcional: en `BLLPago.Registrar`, rechazar un segundo pago del mismo DNI con `fechaPago` en los últimos 10 segundos.

---

## 15. 🟡 VerificacioDV: acciones sin control en el servidor

**Dónde:**
- `gymAppV2/VerificacioDV/VerificacioDV.aspx.cs:19` (`Page_Load` sin `VerificarAcceso`).
- Los handlers `btnRecalcular_Click` (`:185`), `btnConfirmarRestaurar_Click` (`:255`) y `btnInicializar_Click` (`:279`) no verifican `bllDV.UsuarioActualPuedeRepararIntegridad()`.
- `BLL/BLLDigitoVerificador.cs` (`RecalcularDigitos`, `RestaurarBackup`, `InicializarControl`) tampoco lo verifica.

**Qué pasa:** hoy depende de que `pnlAdmin.Visible = false` y de la validación de eventos. Con el punto 1 sin resolver, es un riesgo.

**Cómo arreglarlo:** al inicio de cada handler sensible:
```csharp
if (!bllDV.UsuarioActualPuedeRepararIntegridad()) { RedirigirSeguro("~/AccesoDenegado.aspx"); return; }
```
También en la BLL, lanzar `PermisoException` si no corresponde.

---

## 16. 🟡 Vistas por tipo de cuenta en vez de por permiso

**Dónde:**
- `Pagos/PagosCliente.aspx.cs` (`EsCliente` por `USUARIO_Rol` 4/6: todo lo demás ve la vista de administración).
- `DashBoard/WebForm1.aspx.cs` (`VeIndicadores` por rol).
- `Alumnos.aspx.cs` (`EsSoloLectura` por rol).
- `BLLTransferencia.ActualizarAlias` (rol 1/2/5).

**Qué pasa:** con el Composite, un perfil personalizado con el permiso Pagos asignado a un Entrenador vería y cobraría como admin.

**Cómo arreglarlo:** separar el permiso "operar" en permisos distintos, por ejemplo `PagosGestion` (cobrar y confirmar) y `PagosPropios` (pagar lo propio), y decidir la vista con `BllRol.UsuarioActualTieneAcceso(...)`. Necesita script de permisos y traducciones, y asignar a las familias CAJA y PORTAL_CLIENTE. Dejarlo documentado si no hay tiempo.

---

## 17. 🟡 No se regenera el ID de sesión en el login

**Dónde:** `SERVICIOS/Singleton/SesionUsuario.cs:30` (`LogIn`) guarda el usuario en la misma sesión anónima.

**Qué pasa:** permite una fijación de sesión.

**Cómo arreglarlo:** en `LogIn/LogIn.aspx.cs`, después de validar las credenciales y antes de `LogearUsuario`, generar un ID nuevo. Por ejemplo, borrar la cookie `ASP.NET_SessionId` (`Response.Cookies.Add(new HttpCookie("ASP.NET_SessionId", "") { Expires = DateTime.Now.AddDays(-1) })`) y redirigir a un paso intermedio que inicie sesión. O usar `SessionIDManager.CreateSessionID` + `SaveSessionID`. Riesgo bajo: dejarlo documentado.

---

## 18. 🟡 Código muerto

**Qué hay:**
- `BE/AlertaCheckin.cs`, `MPP/MPPAlertaCheckin.cs`, `BLL/BLLAlertaCheckin.cs` y la tabla `AlertaCheckin`: el check-in se sacó el 17/11.
- `BLLEvento.RegistrarCheckin`.
- Los estilos `.alertas-checkin` del Dashboard.
- `Inicio/inicio.css`.
- `gymAppV2/Default.aspx(.cs)` en la raíz (no está en el `.csproj`).
- `WebForm1.aspx`, `About.aspx`, `Contact.aspx`.
- `VerificacioDV/VerificacionDV.aspx` (copia vacía de `VerificacioDV.aspx`).
- `LogIn/LogIn.aspx`: botón `Button1` "Ir a inicio" oculto con `Visible=false`.
- `PagosCliente`: la clave `pagos_transferencia_datos` ya no se usa.

**Cómo arreglarlo:** sacar del `.csproj` (`<Compile>` y `<Content>`) y borrar los archivos. Para la tabla `AlertaCheckin`, un script con `DROP TABLE` + `DELETE FROM DigitoVerificador WHERE nombreTabla = 'AlertaCheckin'`. Hacerlo **después** de la entrega: no aporta nada a la demo y tiene riesgo.

---

## 19. 🟡 Registro cierra la sesión de quien ya estaba logueado

**Dónde:** `BLL/BLLRegistro.cs` (`Registrar`): hace `sesion.LogIn(new Usuario{...})` y, en el `finally`, `sesion.LogOut()`, que ejecuta `Session.Abandon()`.

**Qué pasa:** si el navegador ya tenía una sesión iniciada (el admin probando), la pierde.

**Cómo arreglarlo:** guardar `Usuario anterior = sesion.Usuario;` antes. En el `finally`: si `anterior != null`, `sesion.LogIn(anterior)` (sin `Abandon`); si no, `LogOut()`. Otra opción: en `Registro.aspx.cs`, si hay sesión iniciada, redirigir al Dashboard con el aviso "Cerrá sesión para registrar una cuenta nueva".

---

## 20. 🟡 Renombrar usuario no actualiza las tablas relacionadas

**Dónde:** `BLL/BLLUsuario.cs:841` y `:958`. `ModificarUsuario` permite `nuevoUsuario != usuarioOriginal`: actualiza `USUARIOS.usr` y `ALUMNOS_USUARIOS` (`RenombrarUsuarioEnVinculos`, **después** del `UPDATE` de USUARIOS).

**Qué pasa:**
- No actualiza `PreguntasSeguridad.usr`, `USUARIO_Contras` / `USUARIO_Intentos`, `Entrenadores.usr` ni `Evento.usr`.
- Si existen las FK sin `ON UPDATE CASCADE` (`scripts/old/bd-schema-v2.sql:166, 185, 204, 223, 246`), el `UPDATE` falla.
- Hoy la pantalla pone `txtUsuario.ReadOnly = true` al modificar (`Usuarios/UsuariosModulo.aspx.cs`, `CargarUsuarioEnFormulario`), así que no se puede disparar desde la interfaz.

**Cómo arreglarlo:** en la BLL, rechazar el cambio con `if (usuarioOriginal != nuevoUsuario) throw new Exception("El nombre de usuario no se puede cambiar.");`, alineado con la interfaz. Si en algún momento se quiere permitir, hacerlo en una transacción que actualice todas las tablas hijas y recalcule sus DVH.
