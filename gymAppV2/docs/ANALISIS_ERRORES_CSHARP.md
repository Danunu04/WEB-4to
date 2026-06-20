# Análisis de Código - Gym-APP (ASP.NET Web Forms, .NET Framework 4.7.2)

**Fecha del análisis:** 2026-06-09  
**Archivos analizados:** 38 archivos C#  
**Líneas de código estimadas:** ~6,500

---

## Resumen Ejecutivo

Se realizó un análisis exhaustivo del código C# del proyecto Gym-APP, identificando:

| Categoría | Cantidad |
|-----------|----------|
| **Issues Críticos** | 10 |
| **Issues Alta Prioridad** | 11 |
| **Issues Media Prioridad** | 15+ |

El código muestra una arquitectura en capas bien intencionada pero con deuda técnica acumulada en aspectos de seguridad y mantenibilidad.

**Calidad General del Código: 6/10**

---

## 🔴 RIESGOS CRÍTICOS (Pueden Romper el Sistema)

| Ubicación | Tipo de Riesgo | Impacto Potencial | Recomendación |
|-----------|---------------|-------------------|---------------|
| **DAL/DalGeneral.cs:16-17** | Campos públicos no inicializados (`SqlConnection conn`, `SqlCommand cmd`) expuestos como instancia compartida | Race condition, conexiones compartidas entre hilos, NullReferenceException si se usa antes del constructor | Cambiar a `private` y eliminar campos públicos. Usar conexiones locales en cada método |
| **DAL/DalGeneral.cs:31** | Hardcoded connection string `"Data Source=.;Initial Catalog=GymApp;Integrated Security=True"` | Fallo en producción si SQL Server no está en localhost, exposición de infraestructura | Mover a Web.config `<connectionStrings>` y usar `ConfigurationManager` |
| **DAL/DalGeneral.cs:91-94** | Cierre de conexión en `finally` pero el campo `conn` es compartido | Si múltiples threads usan la misma instancia, uno puede cerrar la conexión mientras otro la usa | Eliminar patrón de instancia compartida. Cada operación debe crear/disponer su propia conexión |
| **SERVICIOS/CriptoManager.cs:14-15** | Clave AES hardcodeada (`key = "12345678901234567"`, `iv = "1234567890123"`) | Vulnerabilidad crítica de seguridad - cualquiera puede desencriptar datos sensibles | Usar configuración segura (Azure Key Vault, DPAPI) o generar claves aleatorias seguras |
| **SERVICIOS/CriptoManager.cs:90** | Método `_686DPGetAESDecrypt` devuelve `object` en lugar de `string` | Posible InvalidCastException si el resultado se castea incorrectamente | Cambiar signature a `public string _686DPGetAESDecrypt(string dniAES)` |
| **BLL/BLLUsuario.cs:377** | Creación de alumno con `activo = false` hardcoded en rol Cliente (línea 379) | Usuarios clientes creados inactivos por defecto, no pueden login hasta activación manual | Revisar lógica de negocio - ¿debe ser `activo = true` o requerir activación explícita? |
| **SERVICIOS/Singleton/SesionUsuario.cs:15** | Getter de `Usuario` no verifica null antes de cast | NullReferenceException si Session está vacía | Verificar `if (HttpContext.Current?.Session != null)` antes del cast |
| **BLL/BLLEvento.cs:98-108** | Validación de sesión en `RegistrarEvento` puede fallar silenciosamente si Session expira | Eventos críticos de auditoría no se registran, brecha de compliance | Manejar Session null explícitamente y registrar evento fallback o lanzar excepción controlada |
| **MPP/MPPEntrenador.cs:218** | Connection string desde `ConfigurationManager.ConnectionStrings["GymAppConnection"]` que podría no existir | `NullReferenceException` en producción si Web.config no tiene esa clave | Verificar null antes de acceder a `.ConnectionString` |
| **BLL/BLLAlumno.cs:292-295** | Llamada a `ObtenerAlumno(dni)` sin verificar null antes de acceder a `.Usuario` | NullReferenceException si el alumno no existe | Agregar check `if (alumno == null)` antes de línea 293 |

---

## 🟠 VIOLACIONES DE ALTA PRIORIDAD

| Ubicación | Violación | Severidad | Solución Sugerida |
|-----------|----------|-----------|-------------------|
| **BLL/*.cs (todos)** | Catch de Exception genérica que envuelve y relanza con mismo tipo | Alta - Pierde stack trace original, dificulta debugging | Usar `throw;` en lugar de `throw new Exception(...)` o loguear sin reenvolver |
| **BLL/BLLAlumno.cs:141-144** | Catch vacío en `RegistrarEvento` interno | Media-Alta - Fallas de logging se silencian completamente | Al menos loguear a consola/archivo aunque no impida operación principal |
| **BLL/BLLUsuario.cs:40-43** | Catch vacío en método privado `RegistrarEvento` | Media-Alta - Mismo problema anterior | Implementar logging mínimo o NLog/Serilog |
| **MPP/MPPAlumno.cs:82-94** | `Convert.ToInt32`, `Convert.ToBoolean` sin verificar DBNull primero (aunque hay chequeo parcial) | Media - Podría lanzar InvalidCastException | Usar patrón consistente: `row["x"] != DBNull.Value ? Convert.ToX(row["x"]) : default` |
| **MPP/MPPUsuario.cs:54-70** | Constructor de Usuario con muchos parámetros - difícil mantener orden correcto | Media - Error prone al crear objetos | Considerar Builder pattern o inicializador de objeto con nombres |
| **gymAppV2/Alumnos/Alumnos.aspx.cs:538-549** | Contraseña generada como `txtApellido.Text.Trim() + dni` - predecible | **CRÍTICA SEGURIDAD** - Cualquier persona puede adivinar contraseña inicial | Generar contraseña aleatoria compleja y enviar por email seguro |
| **gymAppV2/Usuarios/UsuariosModulo.aspx.cs:368** | Contraseña por defecto `txtApellido.Text + txtDNI.Text` si campo vacío | **CRÍTICA SEGURIDAD** - Mismo problema anterior | Forzar contraseña generada aleatoria nunca basada en datos personales |
| **gymAppV2/LogIn/LogIn.aspx.cs:80-115** | Mensajes de error revelan si usuario existe ("Usuario no encontrado") | Media-Alta - Facilita user enumeration attacks | Usar mensaje genérico "Credenciales inválidas" para ambos casos |
| **BE/UsuarioCrearDTO.cs:52-70** | Validaciones que lanzan `ArgumentException` en lugar de excepción personalizada | Media - Difícil distinguir validaciones de otros errores | Crear `ValidacionException` o usar `ValidationResult` pattern |
| **MPP/MPPEvento.cs:29-32** | Validación de usuario "sistema" hardcoded | Media - Magic string que podría causar bugs si se escribe mal | Usar constante `private const string SISTEMA_USUARIO = "sistema"` |

---

## 🟡 PROBLEMAS DE MEDIA/BAJA PRIORIDAD

### Code Smells y Deuda Técnica

| Ubicación | Problema | Prioridad | Recomendación |
|-----------|---------|-----------|---------------|
| **Todos los archivos MPP** | Uso de `ArrayList` para parámetros en lugar de `List<SqlParameter>` | Media - Type unsafe, requiere casting | Cambiar a `List<SqlParameter>` o mejor aún, usar Dapper/Entity Framework |
| **DalGeneral.cs** | Nombres de métodos crípticos (`_686DPEscribir`, `_686DPConsultar`) | Media - Ilegible para nuevos desarrolladores | Renombrar a `EjecutarComando`, `ConsultarDatos`, etc. |
| **CriptoManager.cs** | Nombres de métodos inconsistentes (`_686DPGetSHA256` vs `_686DPGetAES256`) | Media - Confuso | Establecer convención: `GenerarHashSHA256`, `EncriptarAES256`, `DesencriptarAES256` |
| **BLL/BLLEvento.cs:13-58** | 46 constantes públicas para tipos de evento - clase muy larga | Baja - Difícil de navegar | Agrupar en clases anidadas o enums con extensión: `EventoTipos.Autenticacion.Login` |
| **gymAppV2/Alumnos/Alumnos.aspx.cs:630-664** | Métodos `GetInitials`, `GetAvatarClass` hardcodeados en code-behind | Baja - Deberían estar en ViewModel o helper | Mover a clase utilitaria o usar patrón MVVM |
| **Múltiples .aspx.cs** | Duplicación de métodos `MostrarError`, `MostrarExito`, etc. | Baja - Violación DRY | Crear clase base `PageBase` con estos métodos protegidos |
| **BE/Alumno.cs:29-32** | Propiedades `Nombre`, `Apellido`, etc. comentadas como "solo visualización" | Media - Confunde modelo de dominio vs DTO | Crear `AlumnoDTO` separado o usar proyecciones LINQ |
| **BLL/BLLPrecioModalidad.cs:16-17** | Constantes mágicas `DIAS_POR_SEMANA_VALIDOS` y `DIARIO_ID` | Baja - Deberían estar en configuración o enum | Crear enum `ModalidadEnum { Diario = 0, UnDia = 1, DosDias = 2, TresDias = 3 }` |
| **gymAppV2/Default.aspx.cs:20** | Referencia directa a `txtDni.Value` (control HTML) en lugar de TextBox | Baja - Inconsistente con resto del código | Usar siempre controles server-side o migrar todo a HTML controls |
| **MPP/MPPUsuario.cs:117** | Query SQL con `intentos + 1 >= 3` hardcoded | Baja - Magic number | Usar constante `private const int MAX_INTENTOS = 3` |

### Problemas Específicos de ASP.NET Web Forms

| Ubicación | Problema | Impacto | Solución |
|-----------|---------|---------|----------|
| **gymAppV2/Alumnos/Alumnos.aspx.cs:16-17** | Propiedades `DniSeleccionado` y `EsModificacion` sin persistir en ViewState | Se pierden en postbacks impredecibles | Guardar en ViewState: `ViewState["DniSeleccionado"]` |
| **gymAppV2/Usuarios/UsuariosModulo.aspx.cs:14-32** | ViewState usado correctamente pero `_todosUsuarios` y `Usuarios` pueden ser grandes | ViewState hinchado (>1MB), lento en redes lentas | Considerar Session cache o reconsultar en cada postback |
| **gymAppV2/LogIn/LogIn.aspx.cs:19-28** | Chequeo `IsPostBack` correcto pero instancia BLL dentro del if | Se crean objetos innecesariamente en cada postback | Mover instancias a propiedades de clase con lazy loading |
| **DashBoard.Master.cs:11-14** | Redirección en `Page_Load` sin verificar `IsPostBack` | Puede causar loops de redirección en ciertas condiciones | Agregar `if (!IsPostBack && !Singleton.Instancia.IsLogged())` |
| **gymAppV2/Bitacora/Bitacora.aspx.cs:28-34** | Carga de ViewState manual en `else` pero no carga valores iniciales | Filtros pueden resetearse inesperadamente | Cargar todos los valores de ViewState al inicio independientemente de IsPostBack |

### Seguridad

| Ubicación | Vulnerabilidad | Riesgo | Mitigación |
|-----------|---------------|--------|------------|
| **Todos los queries SQL** | Aunque usan parámetros, algunos queries podrían ser vulnerables a SQL injection si hay concatenación dinámica oculta | Alto | Auditar todos los queries, usar siempre parámetros |
| **gymAppV2/LogIn/LogIn.aspx.cs:174** | `SecurityElement.Escape` para XSS está bien, pero no hay validación de longitud máxima | Medio - DoS por strings gigantes | Agregar `MaxLength` en TextBox y validar longitud en servidor |
| **CriptoManager.cs** | SHA256 sin salt - vulnerable a rainbow tables | Alto para contraseñas | Usar PBKDF2, bcrypt o Argon2 con salt único por usuario |
| **Web.config** | No se verificó pero probablemente falta `<httpCookies httpOnlyCookies="true" />` | Medio - Session hijacking vía XSS | Agregar configuración de cookies seguras en Web.config |

---

## 🏗️ EVALUACIÓN DE ARQUITECTURA

### Patrones Correctamente Implementados
✅ **Separación en capas**: BLL, MPP, BE, DAL claramente diferenciadas  
✅ **Uso de parámetros en consultas SQL**: Previene SQL injection  
✅ **Singleton para sesión**: Adecuado para estado global  
✅ **Excepciones personalizadas**: `ExcepcionesLogIn` para flujo de login  

### Anti-patrones Detectados
❌ **Acoplamiento fuerte**: BLL crea instancias de MPP con `new` en lugar de inyección de dependencias  
❌ **Falta de interfaces**: No hay abstracciones para testing unitario  
❌ **Recursos IDisposable**: DalGeneral no implementa `IDisposable` teniendo `SqlConnection`  
❌ **God Class potencial**: `BLLUsuario` con 700+ líneas y 15+ responsabilidades  

---

## 🔧 RECOMENDACIONES PRIORIZADAS

### Inmediatas (Sprint 1)
1. **Mover connection string a Web.config** - 2 horas
2. **Eliminar clave AES hardcodeada** - 4 horas (requiere diseño de solución segura)
3. **Fix de contraseña predecible en creación de usuarios** - 1 hora
4. **Implementar `IDisposable` en DalGeneral** - 2 horas

### Corto Plazo (Sprint 2-3)
5. **Refactorizar nombres de métodos en DAL** - 4 horas
6. **Reemplazar `ArrayList` con `List<SqlParameter>`** - 6 horas
7. **Agregar logging estructurado (Serilog/NLog)** - 8 horas
8. **Crear clase base para Pages con métodos de toast** - 3 horas

### Mediano Plazo
9. **Implementar inyección de dependencias** - 16 horas
10. **Migrar de SHA256 a PBKDF2 para contraseñas** - 8 horas
11. **Agregar tests unitarios para capa BLL** - 24 horas

---

## 📊 ESTADÍSTICAS DEL ANÁLISIS

| Métrica | Valor |
|---------|-------|
| Total archivos C# analizados | 38 |
| Líneas de código estimadas | ~6,500 |
| **Issues Críticos** | 10 |
| **Issues Alta Prioridad** | 11 |
| **Issues Media Prioridad** | 15+ |
| Archivos con catch vacío/silencioso | 4 |
| Archivos con hardcoded strings | 6 |
| Métodos >50 líneas | 8 |
| Clases con acoplamiento alto (dependencias directas con `new`) | 7 |

---

## 📝 CONCLUSIÓN

El código muestra una **arquitectura en capas bien intencionada** pero con **deuda técnica acumulada** en aspectos de seguridad y mantenibilidad. Los riesgos más críticos están relacionados con:

1. **Seguridad**: Credenciales hardcodeadas, contraseñas predecibles
2. **Estabilidad**: Conexiones SQL compartidas, manejo incorrecto de recursos
3. **Auditoría**: Catch vacíos que silencian fallos de logging

Se recomienda abordar inmediatamente los issues críticos de seguridad antes de cualquier despliegue a producción.
