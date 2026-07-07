# Documentación Técnica Exhaustiva — GymApp v2

> Proyecto: `C:\Users\Danunu\Desktop\WEB-4to\gymAppV2`  
> Tecnología: ASP.NET Web Forms (.NET Framework 4.7.2)  
> Base de datos: SQL Server (esquema `GymApp`)  
> Fecha de generación: 2026-07-06

---

## Tabla de contenidos

1. [Resumen ejecutivo](#1-resumen-ejecutivo)
2. [Arquitectura y capas](#2-arquitectura-y-capas)
3. [Base de datos](#3-base-de-datos)
4. [Capa BE (Business Entities)](#4-capa-be-business-entities)
5. [Capa SERVICIOS](#5-capa-servicios)
6. [Capa DAL](#6-capa-dal)
7. [Capa MPP (Mapper / Repository)](#7-capa-mpp-mapper--repository)
8. [Capa BLL (Business Logic Layer)](#8-capa-bll-business-logic-layer)
9. [Capa UI Web Forms](#9-capa-ui-web-forms)
10. [Flujos principales](#10-flujos-principales)
11. [Seguridad](#11-seguridad)
12. [Dígitos verificadores DVH/DVV](#12-dígitos-verificadores-dvhdvv)
13. [Dependencias entre proyectos](#13-dependencias-entre-proyectos)
14. [Observaciones y deuda técnica](#14-observaciones-y-deuda-técnica)

---

## 1. Resumen ejecutivo

GymApp v2 es una aplicación ASP.NET Web Forms para la gestión de un gimnasio. El sistema está organizado en cinco proyectos de ensamblado (.NET Framework 4.7.2):

| Proyecto | Carpeta | Responsabilidad |
|---|---|---|
| `gymAppV2` | `gymAppV2/` | Capa de presentación Web Forms, masters, code-behind, configuración. |
| `BE` | `BE/` | Entidades de negocio/DTOs. |
| `BLL` | `BLL/` | Reglas de negocio, validaciones, autorización, auditoría. |
| `MPP` | `MPP/` | Persistencia y mapeo SQL ↔ entidades. |
| `DAL` | `DAL/` | Conexión genérica a SQL Server. |
| `SERVICIOS` | `SERVICIOS/` | Utilidades transversales: criptografía, dígito verificador, sesión, excepciones. |
| `CrearPreguntasSeguridad` | `Herramientas/CrearPreguntasSeguridad/` | Herramienta CLI de utilidad (no parte del sitio web). |

El sistema implementa robustas funcionalidades de **seguridad**: login con bloqueo por intentos fallidos, hash de contraseñas SHA-256, encriptación reversible AES-256 de datos personales, historial de contraseñas, preguntas de seguridad encriptadas, bitácora de auditoría, dígitos verificadores DVH/DVV con pausa del sistema, backup/restore nativo y BACPAC, y autorización por roles.

---

## 2. Arquitectura y capas

### 2.1 Flujo de dependencias

```
UI (.aspx / .Master / .ascx)
   ↓ hereda / usa
BasePage (seguridad, sesión, integridad)
   ↓ usa
BLL (reglas + auditoría)
   ↓ usa
MPP (queries + mapeo)
   ↓ usa
DAL (ADO.NET SQL Server)

SERVICIOS ← usado por BLL, MPP y UI
   ├── CriptoManager (hash / AES)
   ├── DigitoVerificadorManager (DVH/DVV)
   ├── Singleton/SesionUsuario (sesión HTTP)
   └── Excepciones (login, acceso denegado)
```

### 2.2 Patrón arquitectónico

- **Three-layer**: UI → BLL → MPP → DAL.
- **Singleton por sesión HTTP**: `Servicios.Singleton.Singleton.Instancia` devuelve una `SesionUsuario` almacenada en `HttpContext.Current.Session`.
- **Catálogos estáticos**: roles y permisos están hardcodeados en `BE.PerfilesSistema` y `BE.PermisosSistema`.
- **No se utiliza inyección de dependencias**: cada BLL/MPP instancia manualmente sus dependencias en el constructor.

---

## 3. Base de datos

### 3.1 Esquema general

El script principal de creación es `bd-schema-v2.sql`. Toda tabla con persistencia relevante incluye columnas `dvv` y `dvh` para verificación de integridad.

### 3.2 Tablas principales

| Tabla | Propósito |
|---|---|
| `USUARIOS` | Identidad central: login, contraseña, intentos, bloqueo, rol, tipo, datos personales encriptados. |
| `ALUMNOS` | Datos específicos del rol Alumno (peso, activo, tieneRutinas). FK por DNI a USUARIOS. |
| `ENTRENADORES` | Datos específicos del rol Entrenador (alumnosCount, activo). FK por DNI. |
| `Actividades` | Catálogo de actividades/clases. |
| `Rutinas` / `RutinaEjercicio` | Rutinas de entrenamiento (esqueleto de BD, poca lógica implementada). |
| `Ejercicio` / `AlumnoRM` / `PesoHistorial` | Esqueleto de ejercicios, récord 1RM y peso corporal. |
| `PreguntasSeguridad` | Pregunta/respuesta encriptada por usuario. |
| `USUARIO_Contras` | Historial de hashes de contraseñas. |
| `USUARIO_Intentos` | Tabla obsoleta (conservada para rollback). |
| `Perfiles` / `Usuario_Perfil` | Esqueleto de perfiles (no usado activamente). |
| `Familia` / `Permiso` / `PermisoFamilia` / `Perfil_Familia` / `Perfil_Permiso` | Modelo de permisos compuesto (no usado activamente). |
| `PrecioModalidad` | Modalidades de cuota mensual. |
| `Evento` | Bitácora/auditoría. |
| `DigitoVerificador` | Control global de DVH/DVV por tabla. |

### 3.3 Datos personales encriptados

En `USUARIOS` se encriptan con AES-256:

- `nombre`
- `apellido`
- `telefono`
- `email`
- `fechaNacimiento` (formato `yyyy-MM-dd` encriptado)

La contraseña se almacena como **hash SHA-256** en el campo `contra`.

---

## 4. Capa BE (Business Entities)

### 4.1 Propósito

Capa de entidades de transporte. No contiene lógica de acceso a datos, solo propiedades, constructores y algunos métodos de presentación/validación.

### 4.2 Clases principales

#### `Usuario`

Representa un registro de la tabla `USUARIOS`.

```csharp
public class Usuario
{
    public string USUARIO_Usuario { get; set; }   // PK
    public string USUARIO_Contras { get; set; }
    public bool USUARIO_Activo { get; set; }
    public bool USUARIO_Bloqueado { get; set; }
    public int USUARIO_Intentos { get; set; }
    public int USUARIO_Rol { get; set; }            // 1..5
    public bool USUARIO_PrimerLogin { get; set; }
    public string USUARIO_Tipo { get; set; }
    public int USUARIO_DNI { get; set; }
    public string Nombre { get; set; }
    public string Apellido { get; set; }
    public string Telefono { get; set; }
    public string Email { get; set; }
    public DateTime? FechaNacimiento { get; set; }
    public string USUARIO_DVV { get; set; }
    public string USUARIO_DVH { get; set; }
}
```

**Relaciones:**
- Es la identidad central.
- `Alumno` y `Entrenador` se vinculan por `DNI`.
- `PreguntaSeguridad`, `Evento` y `USUARIO_Contras` referencian por `usr`.

---

#### `Alumno`

```csharp
public class Alumno
{
    public int DNI { get; set; }
    public decimal? Peso { get; set; }
    public bool TieneRutinas { get; set; }
    public bool Activo { get; set; }
    public string DVV { get; set; }
    public string DVH { get; set; }
    public string Usuario { get; set; }
    // Propiedades de visualización cargadas desde USUARIOS
    public string Nombre { get; set; }
    public string Apellido { get; set; }
    public string Telefono { get; set; }
    public DateTime? FechaNacimiento { get; set; }
}
```

---

#### `Entrenador`

```csharp
public class Entrenador
{
    public int DNI { get; set; }
    public int AlumnosCount { get; set; }
    public bool Activo { get; set; }
    public string DVV { get; set; }
    public string DVH { get; set; }
    public string Usuario { get; set; }
    // Propiedades de visualización desde USUARIOS
    public string Nombre { get; set; }
    public string Apellido { get; set; }
    public string Telefono { get; set; }
    public DateTime? FechaNacimiento { get; set; }
}
```

---

#### `Evento`

```csharp
public class Evento
{
    public int EVENTO_Id { get; set; }
    public string EVENTO_Tipo { get; set; }
    public string EVENTO_Usuario { get; set; }
    public string EVENTO_Accion { get; set; }
    public DateTime EVENTO_Timestamp { get; set; }
    public string EVENTO_DVV { get; set; }
    public string EVENTO_DVH { get; set; }
    public bool Expandido { get; set; }           // UI
    public int EVENTO_Criticidad { get; set; }    // 1..4
    public string EVENTO_Modulo { get; set; }
}
```

---

#### `PrecioModalidad`

```csharp
public class PrecioModalidad
{
    public int Id { get; set; }
    public int DiasPorSemana { get; set; }      // 0,1,2,3
    public bool EsDiario { get; set; }
    public decimal Precio { get; set; }
    public bool Activo { get; set; }
    public DateTime FechaModificacion { get; set; }
    public string DVV { get; set; }
    public string DVH { get; set; }

    public string ObtenerDescripcion()
    {
        if (EsDiario) return "Diario (todos los días)";
        return $"{DiasPorSemana} día{(DiasPorSemana > 1 ? "s" : "")} por semana";
    }
}
```

---

#### `PreguntaSeguridad`

```csharp
public class PreguntaSeguridad
{
    public int Id { get; set; }
    public string Pregunta { get; set; }
    public string Respuesta { get; set; }
    public string Usuario { get; set; }
    public string DVV { get; set; }
    public string DVH { get; set; }
    public TipoPreguntaSeguridad Tipo { get; set; }
}

public enum TipoPreguntaSeguridad
{
    FechaNacimiento = 1,
    AlumnoAsociado = 2
}
```

---

#### `UsuarioCrearDTO`

DTO anémico para crear un nuevo usuario. Incluye datos básicos y campos condicionales para entrenador/cliente.

```csharp
public class UsuarioCrearDTO
{
    public string Usuario { get; set; }
    public string Contrasena { get; set; }
    public int Rol { get; set; }

    // Entrenador
    public int? EntrenadorDNI { get; set; }
    public string EntrenadorNombre { get; set; }
    public string EntrenadorApellido { get; set; }
    public DateTime? EntrenadorFechaNacimiento { get; set; }
    public string EntrenadorTelefono { get; set; }

    // Cliente / Alumno
    public int? AlumnoDNI { get; set; }
    public string AlumnoNombre { get; set; }
    public string AlumnoApellido { get; set; }
    public DateTime? AlumnoFechaNacimiento { get; set; }
    public string AlumnoTelefono { get; set; }
    public string AlumnoEmail { get; set; }

    public void Validar() { /* lanza ArgumentException */ }
}
```

---

#### `UsuarioGestion`

Proyección extendida de `Usuario` para grids de gestión. Marcada `[Serializable]`.

```csharp
[Serializable]
public class UsuarioGestion
{
    public string USUARIO_Usuario { get; set; }
    public string USUARIO_Contras { get; set; }
    public string USUARIO_Tipo { get; set; }
    public int USUARIO_Rol { get; set; }
    public bool USUARIO_Activo { get; set; }
    public bool USUARIO_Bloqueado { get; set; }
    public int USUARIO_Intentos { get; set; }
    public string USUARIO_DVV { get; set; }
    public string USUARIO_DVH { get; set; }
    public int? DNI { get; set; }
    public string Nombre { get; set; }
    public string Apellido { get; set; }
    public string Telefono { get; set; }
    public string Email { get; set; }
    public DateTime? FechaNacimiento { get; set; }
}
```

---

#### Catálogos estáticos

```csharp
public static class PerfilesSistema
{
    public const int RolAdministrador = 1;
    public const int RolRecepcionista = 2;
    public const int RolEntrenador = 3;
    public const int RolCliente = 4;
    public const int RolWebMaster = 5;
    // ... nombres y métodos helper
}

public static class PermisosSistema
{
    public const string Dashboard = "Dashboard";
    public const string Perfil = "Perfil";
    public const string GestionUsuarios = "GestionUsuarios";
    // ... etc.
    public static IReadOnlyList<string> Todos { get; }
}

public static class ConstantesSeguridad
{
    public const int MAX_INTENTOS_LOGIN = 3;
    public const int CONTRASENA_MIN_LENGTH = 6;
    public const int CONTRASENA_MAX_LENGTH = 128;
    public const int USUARIO_MAX_LENGTH = 50;
    public const int MAX_HISTORIAL_CONTRASENAS = 10;
}
```

---

## 5. Capa SERVICIOS

### 5.1 Responsabilidad

Biblioteca de utilidades transversales: criptografía, cálculo de dígitos verificadores, gestión de sesión y excepciones de dominio.

### 5.2 `CriptoManager`

```csharp
public class CriptoManager
{
    public string GenerarHashSHA256(string texto);
    public string EncriptarAES256(string textoPlano);
    public string DesencriptarAES256(string textoEncriptado);
    public bool EsFormatoNuevo(string textoEncriptado);
    public bool EsFormatoLegacy(string textoEncriptado);
}
```

- **Hash**: SHA-256 sobre UTF-8 → hex minúscula de 64 caracteres.
- **Clave AES**: deriva `AesKey` de `Web.config` con SHA-256 → 32 bytes.
- **IV legacy**: `AesIV` de `Web.config` (16 bytes).
- **Formato nuevo**: `[1 byte = 0x01][16 bytes IV aleatorio][ciphertext AES-256-CBC]` en Base64.
- **Migración transparente**: `DesencriptarAES256` intenta formato nuevo primero; si falla, intenta legacy.

**Uso:**
- Hash de contraseñas en `BLLUsuario`/`MPPUsuario`.
- Encriptación de datos personales en `MPPUsuario`.
- Encriptación de preguntas de seguridad en `MPPPreguntaSeguridad`.
- Hash de DVH/DVV en `DigitoVerificadorManager`.

---

### 5.3 `DigitoVerificadorManager`

```csharp
public class DigitoVerificadorManager
{
    public string CalcularDVH(Dictionary<string, object> valores);
    public string CalcularDVV(Dictionary<string, object> valores);
    public bool VerificarDVH(string dvhAlmacenado, Dictionary<string, object> valores);
    public bool VerificarDVV(string dvvAlmacenado, Dictionary<string, object> valores);
    public void CalcularAmbos(Dictionary<string, object> valores, out string dvh, out string dvv);
    public string NormalizarValor(object valor);
}
```

**Lógica:**

- **DVH (horizontal)**: concatena todos los valores de la fila (excepto `dvv`/`dvh`) separados por `|`, normalizados, y aplica SHA-256.
- **DVV (vertical)**: para cada campo (excepto `dvv`/`dvh`) calcula SHA-256 individual, concatena esos hashes y vuelve a hashear.
- **Normalización:**
  - `null` / `DBNull` → `"NULL"`
  - `bool` → `"1"` / `"0"`
  - `DateTime` → `"yyyy-MM-dd HH:mm:ss"`
  - Numéricos → `InvariantCulture`
  - `string` → tal cual (vacío si es `string.Empty`)

**Ejemplo de uso en MPP:**

```csharp
var valores = new Dictionary<string, object>
{
    { "dni", alumno.DNI },
    { "peso", alumno.Peso },
    { "activo", alumno.Activo },
    { "tieneRutinas", alumno.TieneRutinas },
    { "usr", alumno.Usuario }
};
dvManager.CalcularAmbos(valores, out string dvh, out string dvv);
```

---

### 5.4 `CriptoMigracion`

Herramienta one-shot para encriptar datos personales ya existentes.

```csharp
public class CriptoMigracion
{
    public List<ResultadoMigracion> EncriptarTodo();
    public ResultadoMigracion EncriptarCampo(string tabla, string campo, bool encriptar, bool esFecha = false);
}
```

- Detecta si un valor ya está en formato nuevo o legacy.
- Re-encripta legacy al formato nuevo.
- Deja intactos los valores ya encriptados.
- Devuelve conteo de filas encriptadas, ya encriptadas, legacy re-encriptadas y errores.

---

### 5.5 Sesión (`Singleton` / `SesionUsuario`)

```csharp
public class Singleton
{
    public static SesionUsuario Instancia { get; }
}

public class SesionUsuario
{
    public Usuario Usuario { get; }
    public void LogIn(Usuario usuario);
    public void LogOut();
    public bool IsLogged();
}
```

- `Singleton.Instancia` almacena la `SesionUsuario` en `HttpContext.Current.Session["SesionUsuario_Instancia"]`.
- Si no hay contexto HTTP, devuelve `null`.
- `LogOut` limpia la sesión y llama `Session.Abandon()`.

---

### 5.6 Excepciones

```csharp
public class ExcepcionesLogIn : Exception
{
    public ResultadosLogIn Result;
    public ExcepcionesLogIn(ResultadosLogIn result) { Result = result; }
}

public enum ResultadosLogIn
{
    InvalidUsername,
    InvalidPassword,
    AccountLocked,
    ValidUser
}

public class AccesoDenegadoException : Exception
{
    public string Modulo { get; set; }
    public AccesoDenegadoException(string modulo) : base($"No tiene permisos...") { }
}
```

---

## 6. Capa DAL

### 6.1 `DalGeneral`

```csharp
public class DalGeneral : IDisposable
{
    public DalGeneral();

    // ADO desconectado
    public DataTable _686DPConsultar(string consulta, List<SqlParameter> parametros);
    public DataTable _686DPConsultarSP(string nombreSP, List<SqlParameter> parametros);

    // ADO conectado
    public void _686DPEjecutar(string nombreSP, List<SqlParameter> parametros);
    public object _686DPEscalar(string consulta, List<SqlParameter> parametros);
    public void _686DPEscribir(string consulta, List<SqlParameter> parametros);
}
```

**Características:**

- Lee el connection string `GymAppConnection` de `Web.config`.
- Abre la conexión justo antes de ejecutar y la cierra en `finally`.
- Convierte `null` de parámetros a `DBNull.Value`.
- Envuelve `SqlException` en mensajes amigables según `Number`:
  - `547` → FK conflict
  - `2601`/`2627` → duplicado
  - `4060`/`18456` → conexión/login fallido
  - `-2` → timeout
- Implementa `IDisposable`.

**Reglas de uso (documentadas en `docs/ado-modo.md`):**

| Método | Modo | Uso |
|---|---|---|
| `_686DPConsultar` | Desconectado | `SELECT` múltiples filas |
| `_686DPConsultarSP` | Desconectado | `SELECT` vía SP |
| `_686DPEjecutar` | Conectado | SP de `INSERT/UPDATE/DELETE` |
| `_686DPEscalar` | Conectado | Escalar (`COUNT`, `MAX`, `SCOPE_IDENTITY`) |
| `_686DPEscribir` | Conectado | Sentencias directas de escritura |

---

## 7. Capa MPP (Mapper / Repository)

La capa MPP contiene un mapper por entidad. Cada mapper usa `DalGeneral` y, si corresponde, `CriptoManager` y `DigitoVerificadorManager`.

### 7.1 `MPPUsuario`

Responsabilidad: gestión completa de usuarios, login, seguridad e integridad.

```csharp
public class MPPUsuario
{
    // Login / seguridad
    public int ObtenerIntentos(string usuario);
    public void AgregarIntento(string usuario);
    public void BloquearUsuario(string usuario);
    public void ReestablecerIntentos(string usuario);
    public bool UsuarioEstaBloqueado(string usuario);
    public bool UsuarioEstaActivo(string usuario);
    public string ObtenerContrasena(string usuario);
    public bool ContrasenaFueUtilizada(string usuario, string contrasenaHash);
    public void GuardarContrasenaEnHistorial(string usuario, string contrasenaHash);
    public void ActualizarContrasena(string usuario, string nuevaContrasenaHash);
    public void BlanquearContrasena(string usuario);
    public void FinalizarPrimerLogin(string usuario);

    // CRUD
    public Usuario ObtenerUsuario(string usuario);
    public void CrearUsuario(Usuario usuario);
    public void ActualizarUsuario(Usuario usuario, string usuarioOriginal = null);
    public bool UsuarioExiste(string usuario);
    public List<UsuarioGestion> ListarUsuarios();
    public List<UsuarioGestion> ListarUsuariosClientesSinAlumno();
    public void ActualizarEstado(string usuario, bool activo);
    public DateTime? ObtenerFechaNacimiento(string usuario);

    // Integridad
    public void RecalcularDigitosTodosUsuarios();
    public List<ResultadoVerificacionDV> VerificarIntegridadUsuarios();
    public List<ResultadoVerificacionDV> VerificarIntegridadHistorialContrasenas();
    public void RecalcularDigitosHistorialContrasenas();
}
```

**Encriptación:**

```csharp
private string EncriptarCampoPersonal(string valor)
{
    return string.IsNullOrEmpty(valor) ? null : criptoManager.EncriptarAES256(valor);
}

private string DesencriptarCampoPersonal(string valor)
{
    if (string.IsNullOrEmpty(valor)) return null;
    try { return criptoManager.DesencriptarAES256(valor); }
    catch { return valor; } // fallback texto plano
}
```

**Cálculo de DVH/DVV de usuario:**

```csharp
var valores = new Dictionary<string, object>
{
    { "usr", usuario.USUARIO_Usuario },
    { "contra", usuario.USUARIO_Contras },
    { "activo", usuario.USUARIO_Activo },
    { "bloqueado", usuario.USUARIO_Bloqueado },
    { "intentos", usuario.USUARIO_Intentos },
    { "tipo", usuario.USUARIO_Tipo },
    { "dni", usuario.USUARIO_DNI },
    { "nombre", usuario.Nombre },
    { "apellido", usuario.Apellido },
    { "telefono", usuario.Telefono },
    { "email", usuario.Email },
    { "fechaNacimiento", fechaFormateada },
    { "rol", usuario.USUARIO_Rol },
    { "primerLogin", usuario.USUARIO_PrimerLogin }
};
dvManager.CalcularAmbos(valores, out dvh, out dvv);
```

---

### 7.2 `MPPAlumno`

```csharp
public class MPPAlumno
{
    public void CrearAlumno(Alumno alumno);
    public Alumno ObtenerAlumno(int dni);
    public void ActualizarAlumno(Alumno alumno);
    public bool AlumnoExiste(int dni);
    public List<Alumno> ListarAlumnos();
    public void EliminarAlumno(int dni);
    public List<Alumno> ListarAlumnosSinUsuario();
    public int CantidadAlumnosAsociados(string usuario);
    public void AsociarUsuario(int dni, string usuario);
}
```

- JOIN con `USUARIOS` para obtener datos personales desencriptados.
- `EliminarAlumno` elimina primero las `Rutinas` asociadas (cascada manual).
- Calcula DVH/DVV sobre `dni`, `peso`, `activo`, `tieneRutinas`, `usr`.

---

### 7.3 `MPPEntrenador`

```csharp
public class MPPEntrenador
{
    public List<Entrenador> ListarEntrenadores();
    public void CrearEntrenador(Entrenador entrenador);
    public bool EntrenadorExiste(int dni);
    public Entrenador ObtenerEntrenador(int dni);
    public void ActualizarEntrenador(Entrenador entrenador);
    public void EliminarEntrenador(int dni);
    public Dictionary<string, int> ObtenerEstadisticas();
}
```

- `EliminarEntrenador` usa `SqlTransaction` para eliminar en orden: `Actividad_Entrenador`, `Rutinas`, `Entrenadores`.
- Calcula DVH/DVV sobre `dni`, `alumnosCount`, `activo`, `usr`.

---

### 7.4 `MPPEvento`

```csharp
public class MPPEvento
{
    public int RegistrarEvento(Evento evento, int criticidad = 1);
    public List<Evento> ObtenerEventos(string filtro, string busqueda,
        int? filtroCriticidad = null, string filtroModulo = null);
    public Dictionary<string, int> ObtenerEstadisticas();
    public List<string> ObtenerModulos();
    public List<ResultadoVerificacionDV> VerificarIntegridadEventos();
    public void RecalcularDigitosTodosEventos();
}
```

- Inserta eventos con `SCOPE_IDENTITY()`.
- Trunca `fecha` a segundos para coincidencia con SQL.
- Excluye `codEvento` autogenerado del cálculo DVH/DVV.
- DVH/DVV se calculan sobre texto plano; los campos de evento no están encriptados.

---

### 7.5 `MPPPreguntaSeguridad`

```csharp
public class MPPPreguntaSeguridad
{
    public PreguntaSeguridad ObtenerPreguntaPorUsuario(string usuario);
    public void GuardarPregunta(PreguntaSeguridad pregunta);
    public string ObtenerRespuestaPorUsuario(string usuario);
    public bool ValidarRespuesta(string usuario, string respuesta);
    public void RecalcularDigitosTodasPreguntas();
    public List<ResultadoVerificacionDV> VerificarIntegridadPreguntas();
}
```

- Encripta `pregunta` y `respuesta` con AES-256 antes de persistir.
- Al leer intenta desencriptar; si falla devuelve texto plano (migración gradual).
- `GuardarPregunta` realiza upsert por `usr`.
- DVH/DVV se calculan sobre **texto plano** de `usr`, `pregunta`, `respuesta`.

---

### 7.6 `MPPActividad`

```csharp
public class MPPActividad
{
    public List<Actividad> ListarActividades();
    public List<Actividad> ListarActividadesPorCliente(string usuario);
}
```

- Solo lectura.
- `ListarActividadesPorCliente` hace INNER JOIN con `Actividad_Alumno` y `Alumnos` filtrando por `usr`.

---

### 7.7 `MPPPrecioModalidad`

```csharp
public class MPPPrecioModalidad
{
    public List<PrecioModalidad> ListarModalidades();
    public PrecioModalidad ObtenerModalidad(int id);
    public void ActualizarPrecio(int id, decimal nuevoPrecio);
    public decimal ObtenerPrecioPorDias(int diasPorSemana);
}
```

- Actualiza precio, `FechaModificacion` y DVH/DVV.
- `ObtenerPrecioPorDias` soporta modalidad diaria (`DiasPorSemana = 0` y `EsDiario = 1`).

---

### 7.8 `MPPRol`

```csharp
public class MPPRol
{
    public int ObtenerRol(string usuario);
    public void ActualizarRol(string usuario, int rol);
}
```

- Opera directamente sobre `USUARIOS.rol`.
- No recalcula DVH/DVV del usuario; eso queda para la capa BLL/invocante.

---

### 7.9 `MPPDigitoVerificador`

Motor central de integridad, backup/restore y BACPAC.

```csharp
public class MPPDigitoVerificador
{
    // Control global
    public DataRow ObtenerControlPorTabla(string nombreTabla);
    public void GuardarControl(string nombreTabla, string dvhTabla, string dvvTabla);

    // Verificación
    public List<ResultadoVerificacionDV> VerificarIntegridadGlobal();
    public List<ResultadoVerificacionDV> VerificarIntegridadTabla(string nombreTabla);
    public List<string> ObtenerTablasConControl();
    public List<string> ObtenerTablasSinControl();
    public List<EstadoControlDV> ObtenerEstadoControl();

    // Recálculo
    public void RecalcularDigitosGlobal();
    public void RecalcularDigitosTabla(string nombreTabla, bool actualizarFilas = true);

    // Backup / Restore
    public void RealizarBackup(string rutaDestino);
    public void RestaurarBackup(string rutaBackup);
    public void ExportarBacpac(string rutaDestino);
    public void ImportarBacpac(string rutaBacpac);
}
```

**Lógica de verificación:**

1. Lee `dvhTabla` y `dvvTabla` de `DigitoVerificador`.
2. Lee todas las filas de la tabla.
3. Concatena todos los `dvh` de filas y aplica SHA-256 → `dvhTablaCalculado`.
4. Concatena todos los `dvv` de filas y aplica SHA-256 → `dvvTablaCalculado`.
5. Si no coinciden, recorre fila por fila identificando cuáles están corruptas.

**Lógica de recálculo:**

1. Detecta claves primarias de la tabla vía `INFORMATION_SCHEMA`.
2. Para cada fila arma diccionario de valores, calcula DVH/DVV y actualiza.
3. Para tablas encriptadas (USUARIOS, PreguntasSeguridad, Evento) delega en los MPP especializados para desencriptar antes de calcular.
4. Recalcula el hash agregado de tabla y guarda/actualiza `DigitoVerificador`.

**Backup/Restore:**

- Nativo: `BACKUP DATABASE ... TO DISK` y `RESTORE DATABASE ... WITH MOVE, REPLACE`.
- BACPAC: invoca `SqlPackage.exe` mediante `ProcessStartInfo`, buscándolo en rutas estándar de SQL Server/SSDT/PATH.

---

## 8. Capa BLL (Business Logic Layer)

### 8.1 Propósito

Capa de reglas de negocio, validaciones, autorización y auditoría. Cada clase BLL es una fachada sobre uno o más MPP.

### 8.2 `BLLUsuario`

La clase más compleja del sistema.

```csharp
public class BLLUsuario
{
    public bool ValidarLogin(string usuario, string contrasena);
    public void RegistrarIntentoFallido(string usuario);
    public int ObtenerIntentosRestantes(string usuario);
    public void ReestablecerIntentos(string usuario);

    public Usuario ObtenerUsuario(string usuario);
    public void LogearUsuario(Usuario usuario);
    public void DeslogearUsuario();
    public bool UsuarioEstaLogueado();

    public void ValidarRequisitosContrasena(string contrasena);
    public void CambiarContrasena(string usuario, string nuevaContrasena);
    public void FinalizarPrimerLogin(string usuario);
    public bool RequierePreguntaSeguridad(string usuario);

    public List<UsuarioGestion> ListarUsuarios();
    public void CrearUsuario(string usuario, string contrasena, int rol,
        string nombre, string apellido, string telefono, string email,
        DateTime? fechaNacimiento, Entrenador datosEntrenador, int? dniAlumno,
        string confirmarContrasena, bool activo);
    public void CrearUsuario(UsuarioCrearDTO dto);

    public string GenerarContrasenaSegura();
    public List<UsuarioGestion> ListarUsuariosClientesDisponibles();

    public void ActivarUsuario(string usuario);
    public void DesactivarUsuario(string usuario);
    public void BloquearUsuario(string usuario);
    public void DesbloquearUsuario(string usuario);
    public void BlanquearContrasena(string usuario);
    public void ModificarUsuario(string usuarioOriginal, string nuevoUsuario, ...);
}
```

**Flujo de `ValidarLogin`:**

1. Rechaza campos vacíos.
2. Obtiene usuario; si no existe o está inactivo → `ExcepcionesLogIn(InvalidUsername)`.
3. Si está bloqueado → `ExcepcionesLogIn(AccountLocked)`.
4. Hashea la contraseña ingresada.
5. Compara con `ObtenerContrasena`.
6. Si coincide:
   - Reestablece intentos.
   - Guarda en sesión (`LogearUsuario`).
   - Registra login en bitácora.
   - Devuelve `true`.
7. Si no coincide:
   - Incrementa intentos (`AgregarIntento`).
   - Si supera `MAX_INTENTOS_LOGIN`, bloquea y registra evento → `AccountLocked`.
   - Si no, → `ExcepcionesLogIn(InvalidPassword)`.

**Flujo de `CambiarContrasena`:**

1. Valida requisitos de complejidad.
2. Verifica que no esté en historial de contraseñas.
3. Hashea la nueva contraseña.
4. Actualiza en MPP.
5. Guarda la contraseña anterior en `USUARIO_Contras`.
6. Reestablece intentos y desbloquea usuario.
7. Registra evento.

**Requisitos de contraseña:**

- Entre `CONTRASENA_MIN_LENGTH` y `CONTRASENA_MAX_LENGTH`.
- Al menos una mayúscula.
- Al menos un carácter especial.

**Generación de contraseña segura:**

- 12 caracteres aleatorios con mayúsculas, minúsculas, dígitos y especiales.

---

### 8.3 `BLLRol`

```csharp
public class BLLRol
{
    public int ObtenerRol(string usuario);
    public void ActualizarRol(string usuario, int rol);
    public bool TieneAccesoAModulo(int rol, string modulo);
    public bool UsuarioActualTieneAcceso(string modulo);
    public bool UsuarioActualEsAdmin();
    public bool UsuarioActualEsRecepcionista();
    public bool UsuarioActualEsEntrenador();
    public bool UsuarioActualEsCliente();
    public IReadOnlyList<string> ObtenerPerfilesUsuarioActual();
    public string ObtenerNombrePerfilUsuarioActual();
}
```

- `TieneAccesoAModulo` es una matriz de permisos hardcodeada.
- WebMaster (5) y Administrador (1) tienen acceso total.
- Recepcionista (2), Entrenador (3) y Cliente (4) tienen acceso restringido según módulo.

**Ejemplo de matriz (simplificada):**

```csharp
public bool TieneAccesoAModulo(int rol, string modulo)
{
    if (rol == PerfilesSistema.RolWebMaster || rol == PerfilesSistema.RolAdministrador)
        return true;

    switch (modulo)
    {
        case PermisosSistema.Dashboard:
            return rol != PerfilesSistema.RolCliente;
        case PermisosSistema.GestionAlumnos:
            return rol == PerfilesSistema.RolRecepcionista;
        // ... etc
        default: return false;
    }
}
```

---

### 8.4 `BLLAlumno`

```csharp
public class BLLAlumno
{
    public void ValidarDNI(string dniStr);
    public void ValidarNombreApellido(string valor, string campo);
    public void ValidarTelefono(string telefono);
    public void ValidarFechaNacimiento(DateTime fechaNacimiento);
    public void ValidarPeso(decimal? peso);

    public void CrearAlumno(Alumno alumno);
    public Alumno ObtenerAlumno(int dni);
    public void ActualizarAlumno(Alumno alumno);
    public bool AlumnoExiste(int dni);
    public List<Alumno> ListarAlumnos();
    public void EliminarAlumno(int dni);
    public void AsociarUsuario(int dni, string usuario);
    public void DesasociarUsuario(int dni);
    public int CantidadAlumnosAsociados(string usuario);
    public List<Alumno> ListarAlumnosSinUsuario();
}
```

- Valida DNI de 7-8 dígitos.
- Valida nombres con solo letras y espacios.
- Valida peso entre 0 y 500.
- Al crear/actualizar/eliminar registra eventos en bitácora.
- `AsociarUsuario` valida que el usuario sea Cliente (rol 4), que el alumno no tenga usuario y que el usuario no tenga otro alumno asociado.

---

### 8.5 `BLLEntrenador`

```csharp
public class BLLEntrenador
{
    public void CrearEntrenador(Entrenador entrenador);
    public Entrenador ObtenerEntrenador(int dni);
    public void ActualizarEntrenador(Entrenador entrenador);
    public void EliminarEntrenador(int dni);
    public List<Entrenador> ListarEntrenadores();
    public Dictionary<string, int> ObtenerEstadisticas();
}
```

- ABM de entrenadores con auditoría.
- `EliminarEntrenador` delega en `MPPEntrenador` que elimina relaciones transaccionalmente.

---

### 8.6 `BLLEvento`

```csharp
public class BLLEvento
{
    // Constantes de tipos de evento
    public const string EVENTO_LOGIN = "login";
    public const string EVENTO_LOGOUT = "logout";
    // ... (más de 30 constantes)

    public int RegistrarEvento(string tipo, string usuario, string accion,
        int criticidad = 4, string modulo = "");

    public List<Evento> ObtenerEventos(string filtro, string busqueda,
        int? filtroCriticidad = null, string filtroModulo = null);
    public List<string> ObtenerModulos();
    public Dictionary<string, int> ObtenerEstadisticas();

    // Métodos específicos
    public int RegistrarLogin(string usuario);
    public int RegistrarLogout(string usuario);
    public int RegistrarAltaUsuario(string usuario, int rol);
    // ... etc
}
```

- Valida que la criticidad esté entre 1 y 4.
- Valida que eventos post-autenticación tengan sesión activa.
- Permite usuario `"sistema"` solo para eventos específicos como checkin/pago.
- Crea `BE.Evento` y delega en `MPPEvento.RegistrarEvento`.

---

### 8.7 `BLLPreguntaSeguridad`

```csharp
public class BLLPreguntaSeguridad
{
    public PreguntaSeguridad ObtenerPreguntaPorUsuario(string usuario);
    public void GuardarPregunta(PreguntaSeguridad pregunta);
    public string ObtenerRespuestaPorUsuario(string usuario);
    public bool ValidarRespuesta(string usuario, string respuesta);
    public PreguntaSeguridad GenerarPreguntaSeguridad(string usuario);
    public string GenerarPreguntaSeguridad(string usuario, int anioNacimiento);
    public PreguntaSeguridad GenerarPreguntaSeguridadAlumno(string usuario);
    public void CrearPreguntaSeguridadPorDefecto(string usuario);
}
```

- Pregunta por fecha de nacimiento: `¿En qué año naciste?`
- Pregunta por alumno asociado: requiere que el usuario tenga más de un alumno y mezcla el nombre real con nombres aleatorios.
- `CrearPreguntaSeguridadPorDefecto` genera y guarda automáticamente la pregunta de fecha de nacimiento.

---

### 8.8 `BLLPrecioModalidad`

```csharp
public class BLLPrecioModalidad
{
    public void ValidarModalidad(int diasPorSemana, bool esDiario);
    public void ValidarPrecio(decimal precio);
    public List<PrecioModalidad> ListarModalidades();
    public PrecioModalidad ObtenerModalidad(int id);
    public void ModificarPrecio(int id, decimal nuevoPrecio, string usuarioModificador);
    public decimal ObtenerPrecio(int diasPorSemana);
}
```

- Valida modalidades permitidas: diario o 1/2/3 días por semana.
- Valida precio > 0.
- `ModificarPrecio` registra evento de modificación de precio y notifica a usuarios no entrenadores (registrando evento de error si falla la notificación).

---

### 8.9 `BLLActividad`

```csharp
public class BLLActividad
{
    public List<Actividad> ListarActividades();
    public List<Actividad> ListarActividadesPorCliente(string usuario);
}
```

- Delega directamente en `MPPActividad`.
- Envuelve errores en `Exception` descriptiva.

---

### 8.10 `BLLCriptoMigracion`

```csharp
public class BLLCriptoMigracion
{
    public List<CriptoMigracion.ResultadoMigracion> EncriptarTodo();
    public CriptoMigracion.ResultadoMigracion EncriptarCampo(string tabla, string campo, bool esFecha = false);
}
```

- Fachada del servicio de migración para la página `Admin/EncriptarDatos.aspx`.

---

### 8.11 `BLLDigitoVerificador`

```csharp
public class BLLDigitoVerificador
{
    public List<ResultadoVerificacionDV> VerificarIntegridad();
    public bool ExisteErrorIntegridad();
    public bool SistemaDebePausarse();
    public List<string> ObtenerTablasSinControl();
    public void InicializarControl();
    public void RecalcularDigitos();
    public void RegistrarTabla(string nombreTabla);
    public List<EstadoControlDV> ObtenerEstadoControl();

    public void RealizarBackup(string rutaDestino);
    public void RestaurarBackup(string rutaBackup);
    public void ExportarBacpac(string rutaDestino);
    public void ImportarBacpac(string rutaBacpac);

    public bool UsuarioActualEsAdmin();
}
```

- Orquesta la verificación global delegando en `MPPDigitoVerificador`.
- Para tablas encriptadas delega en `MPPUsuario`, `MPPPreguntaSeguridad` y `MPPEvento`.
- `ExisteErrorIntegridad` devuelve `true` en caso de error (fail-safe).
- Incluye backup/restore/bacpac.

---

## 9. Capa UI Web Forms

### 9.1 Infraestructura común

#### `BasePage.cs`

Todas las páginas protegidas heredan de `BasePage`.

```csharp
public class BasePage : System.Web.UI.Page
{
    protected BLLRol BllRol { get; private set; }
    protected BLLDigitoVerificador BllDV { get; private set; }
    protected BLLEvento BllEvento { get; private set; }

    protected override void OnInit(EventArgs e)
    {
        base.OnInit(e);
        BllRol = new BLLRol();
        BllDV = new BLLDigitoVerificador();
        BllEvento = new BLLEvento();

        if (!Singleton.Instancia.IsLogged())
        {
            RedirigirSeguro("~/LogIn/LogIn.aspx");
            return;
        }

        VerificarIntegridadSiAplica();
        VerificarPaginasSoloErrorIntegridad();
        VerificarPaginasMantenimientoSistema();
    }

    protected void VerificarAcceso(string modulo)
    {
        if (!BllRol.UsuarioActualTieneAcceso(modulo))
            RedirigirSeguro("~/AccesoDenegado.aspx");
    }

    protected void RedirigirSeguro(string url)
    {
        Response.Redirect(ResolveUrl(url), false);
        Context.ApplicationInstance.CompleteRequest();
    }

    protected void MostrarToast(string mensaje, string tipo = "info");
    protected void MostrarError(string mensaje);
    protected void MostrarExito(string mensaje);
    protected void MostrarAdvertencia(string mensaje);
}
```

**Responsabilidades:**

- Verifica sesión activa.
- Verifica errores de integridad; si hay error y no es admin, pausa el sistema redirigiendo a `VerificacioDV.aspx`.
- Protege bucles de redirección con `VerificacioDV.aspx`.
- Oculta páginas de mantenimiento (`Admin/EncriptarDatos.aspx`) de la navegación normal.
- Provee `VerificarAcceso` por módulo.
- Provee helpers de redirección y toast.

---

#### `Global.asax.cs`

```csharp
public class Global : HttpApplication
{
    void Application_Start(object sender, EventArgs e)
    {
        RouteConfig.RegisterRoutes(RouteTable.Routes);
        BundleConfig.RegisterBundles(BundleTable.Bundles);
        EjecutarHealthCheckInicial();
    }

    void Application_Error(object sender, EventArgs e)
    {
        Exception ex = Server.GetLastError();
        // registra en bitácora
        Server.ClearError();
        Response.Redirect("~/AccesoDenegado.aspx", false);
        Context.ApplicationInstance.CompleteRequest();
    }
}
```

- Registra rutas FriendlyUrls y bundles.
- Health check de `DigitoVerificador` al arrancar.
- Captura errores globales, registra y redirige amigablemente.

---

#### `DashBoard.Master.cs`

Master page usada por las páginas internas.

```csharp
public partial class DashBoardMaster : System.Web.UI.MasterPage
{
    protected void Page_Load(object sender, EventArgs e)
    {
        if (!IsPostBack)
        {
            if (!Singleton.Instancia.IsLogged())
            {
                RedirigirSeguro("~/LogIn/LogIn.aspx");
                return;
            }
        }

        if (SistemaEnPausa() && !UsuarioActualEsAdmin())
        {
            ConfigurarMenuPausa();
            // redirige a VerificacioDV.aspx si no está ya allí
        }
        else
        {
            ConfigurarMenuSegunRol();
        }
    }

    private void ConfigurarMenuSegunRol()
    {
        liDashboard.Visible = bllRol.UsuarioActualTieneAcceso("Dashboard");
        liUsuarios.Visible = bllRol.UsuarioActualTieneAcceso("GestionUsuarios");
        liAlumnos.Visible = bllRol.UsuarioActualTieneAcceso("GestionAlumnos");
        liEntrenadores.Visible = bllRol.UsuarioActualTieneAcceso("GestionEntrenadores");
        liActividades.Visible = bllRol.UsuarioActualTieneAcceso("ActividadesCalendario");
        liRutinas.Visible = bllRol.UsuarioActualTieneAcceso("GestionRutinas");
        liBitacora.Visible = bllRol.UsuarioActualTieneAcceso("Bitacora");
        liRespaldo.Visible = bllRol.UsuarioActualTieneAcceso(PermisosSistema.Backup)
                         || bllRol.UsuarioActualTieneAcceso(PermisosSistema.Restore);
        liPagos.Visible = bllRol.UsuarioActualTieneAcceso(PermisosSistema.Pagos);
        liPerfil.Visible = bllRol.UsuarioActualTieneAcceso(PermisosSistema.Perfil);
    }

    protected void LnkLogout_Click(object sender, EventArgs e)
    {
        FormsAuthentication.SignOut();
        bllEvento.RegistrarLogout(usuarioNombre);
        Singleton.Instancia.LogOut();
        RedirigirSeguro("~/LogIn/LogIn.aspx");
    }
}
```

---

### 9.2 Módulo Login / Autenticación

#### `LogIn.aspx.cs`

```csharp
protected void btnLogIn_Click(object sender, EventArgs e)
{
    if (!Page.IsValid) return;

    string usuario = txtUsuario.Text.Trim();
    string contra = txtContrasena.Text;

    try
    {
        bool ok = bllUsuario.ValidarLogin(usuario, contra);
        if (!ok) return;

        var userBD = bllUsuario.ObtenerUsuario(usuario);

        // Si hay error de integridad, solo admin/webmaster pueden continuar
        if (bllDV.ExisteErrorIntegridad())
        {
            if (userBD.USUARIO_Rol != RolAdministrador && userBD.USUARIO_Rol != RolWebMaster)
            {
                MostrarToast("Error de integridad...", "error");
                return;
            }
            RedirigirSeguro("~/VerificacioDV/VerificacioDV.aspx");
            return;
        }

        if (userBD.USUARIO_PrimerLogin)
        {
            RedirigirSeguro($"~/LogIn/ConfigurarPreguntas.aspx?usuario={usuario}&modo=primerLogin");
            return;
        }

        FormsAuthentication.SetAuthCookie(usuario, false);
        RedirigirSeguro("~/DashBoard/WebForm1.aspx");
    }
    catch (ExcepcionesLogIn ex)
    {
        if (ex.Result == ResultadosLogIn.AccountLocked)
            RedirigirSeguro($"~/LogIn/PreguntasSeguridad.aspx?usuario={usuario}");
        else
            MostrarError("Usuario o contraseña incorrectos.");
    }
}
```

**Flujo:**

1. Valida credenciales con `BLLUsuario.ValidarLogin`.
2. Si hay error de integridad, solo admin/webmaster ingresan (a `VerificacioDV.aspx`).
3. Si es primer login → `ConfigurarPreguntas.aspx`.
4. Si está bloqueado → `PreguntasSeguridad.aspx`.
5. Login normal → dashboard.

---

#### `ConfigurarPreguntas.aspx.cs`

- Permite al usuario configurar su pregunta de seguridad.
- En modo `primerLogin` requiere que el usuario de URL coincida con sesión.
- Tras guardar, llama `FinalizarPrimerLogin` y redirige al dashboard.

---

#### `PreguntasSeguridad.aspx.cs`

- Accesible sin sesión.
- Pide usuario → muestra pregunta → valida respuesta.
- Si la respuesta es correcta, genera token de recuperación en sesión y redirige a `Cambiar-contra.aspx` con modo `recuperacion`.
- Si la respuesta es incorrecta, bloquea la cuenta (si no lo estaba ya) y registra evento.

---

#### `CambiarContra/Cambiar-contra.aspx.cs`

Soporta tres modos:

| Modo | Contraseña actual requerida | Acción posterior |
|---|---|---|
| `primerLogin` | No | Redirige a `ConfigurarPreguntas.aspx` |
| `recuperacion` | No (valida token en sesión) | Invalida sesión y redirige a login |
| normal | Sí | Redirige al dashboard |

- Valida complejidad con `BLLUsuario.ValidarRequisitosContrasena`.
- Llama `BLLUsuario.CambiarContrasena`.
- Si es recuperación, desbloquea usuario.

---

### 9.3 Módulo Usuarios

#### `Usuarios/UsuariosModulo.aspx.cs`

- Hereda `BasePage`.
- `Page_Load`: `VerificarAcceso(PermisosSistema.GestionUsuarios)`.
- Carga grid con filtros (estado, bloqueado, rol, búsqueda).
- Acciones: Crear, Modificar, Activar, Desactivar, Desbloquear, Blanquear contraseña.
- Formulario modal para alta/modificación con campos de DNI, teléfono, apellido, nombre, email, usuario, rol, fecha nacimiento, estado.
- Paneles `EntField` y `clienteFields` se muestran según rol seleccionado.
- Si contraseña está vacía, llama `GenerarContrasenaSegura`.

**BLLs usados:** `BLLUsuario`.

---

### 9.4 Módulo Alumnos

#### `Alumnos/Alumnos.aspx.cs`

- `VerificarAcceso(PermisosSistema.GestionAlumnos)`.
- Grid de alumnos con filtros.
- Modo solo lectura para rol Cliente (rol 4): oculta botones de edición.
- CRUD de alumnos y asociación/desasociación de usuario cliente.
- Panel de confirmación de eliminación.

**BLLs usados:** `BLLAlumno`, `BLLUsuario`.

---

### 9.5 Módulo Entrenadores

#### `Entrenadores/Entrenadores.aspx.cs`

- Esqueleto; solo verifica acceso.
- No hay handlers ni lógica implementada en el code-behind.

---

### 9.6 Módulo Actividades

#### `Actividades/actividades.aspx.cs`

- `VerificarAcceso(PermisosSistema.ActividadesCalendario)`.
- Si el usuario es Cliente, carga `ListarActividadesPorCliente`.
- Si no, carga `ListarActividades`.
- Serializa actividades a JSON en `hdnActividadesJson`.
- Calendario se renderiza en el front-end (JS). No hay inscripción ni cupos implementados.

---

### 9.7 Módulo Rutinas

#### `Rutinas/Rutinas.aspx.cs`

- Esqueleto: alterna paneles `pnlCliente` / `pnlAdmin` según rol.
- Sin lógica de CRUD implementada.

---

### 9.8 Módulo Bitácora

#### `Bitacora/Bitacora.aspx.cs`

- `VerificarAcceso(PermisosSistema.Bitacora)`.
- Carga estadísticas, filtros por criticidad/módulo/búsqueda.
- Repeater de eventos con expansión de detalle.
- Botones de filtro rápido (login, logout, bloqueo, etc.).

**BLLs usados:** `BLLEvento`.

---

### 9.9 Módulo Perfil

#### `Perfil/Perfil.aspx.cs`

- `VerificarAcceso(PermisosSistema.Perfil)`.
- Carga datos del usuario en sesión.
- Permite modificar nombre, apellido, teléfono, email.
- Llama `BLLUsuario.ModificarUsuario`.

---

### 9.10 Módulo Verificación DV

#### `VerificacioDV/VerificacioDV.aspx.cs`

- Dos vistas según rol: admin (panel de control) o usuario no admin (pantalla de pausa).
- Botones: Verificar integridad, Recalcular dígitos, Inicializar control, Restaurar backup.
- Muestra grids de estado de control y resultados de verificación.
- Hereda `BasePage` pero se excluye de la redirección de pausa para evitar bucles.

**BLLs usados:** `BLLDigitoVerificador`, `BLLEvento`.

---

### 9.11 Módulo Administración

#### `Admin/EncriptarDatos.aspx.cs`

- `VerificarAcceso(PermisosSistema.EncriptarDatos)`.
- `BasePage` redirige esta página al dashboard en navegación normal (es página de mantenimiento oculta).
- Botón "Encriptar todo" llama `BLLCriptoMigracion.EncriptarTodo`.
- Muestra grid de resultados.

---

#### `Admin/BackupRestore.aspx.cs`

- `VerificarAcceso(PermisosSistema.Backup)` para backup.
- `VerificarAcceso(PermisosSistema.Restore)` para restore.
- Soporta formatos `.bak` y `.bacpac`.
- Backup nativo: `BLLDigitoVerificador.RealizarBackup` / `ExportarBacpac`.
- Restore: `RestaurarBackup` / `ImportarBacpac`.
- Registra eventos correspondientes.

---

### 9.12 Dashboard / Inicio / Páginas públicas

| Página | Propósito |
|---|---|
| `DashBoard/WebForm1.aspx` | Página principal interna; solo verifica acceso. |
| `Inicio/Default.aspx` | Kiosco público de check-in por DNI; no requiere login. |
| `About.aspx` / `Contact.aspx` / `Default.aspx` / `WebForm1.aspx` | Redirigen a `Inicio/Default.aspx`. |
| `AccesoDenegado.aspx` | Página amigable de error/permiso. |

---

## 10. Flujos principales

### 10.1 Login

```
Usuario ingresa usr/contra en LogIn.aspx
    ↓
BLLUsuario.ValidarLogin(usr, contra)
    ↓
MPPUsuario.ObtenerUsuario(usr)
    ↓
DalGeneral._686DPConsultar("SELECT ... FROM USUARIOS WHERE usr", ...)
    ↓
DataRow → BE.Usuario (desencriptando campos personales)
    ↓
[ si inactivo/no existe ] → InvalidUsername
[ si bloqueado ] → AccountLocked
    ↓
CriptoManager.GenerarHashSHA256(contra)
    ↓
MPPUsuario.ObtenerContrasena(usr)
    ↓
[ si no coincide ] → BLLUsuario.RegistrarIntentoFallido
                     MPPUsuario.AgregarIntento
                     [ si supera MAX_INTENTOS_LOGIN ] → BloquearUsuario + AccountLocked
                     [ else ] → InvalidPassword
    ↓
[ si coincide ] → MPPUsuario.ReestablecerIntentos
                  Singleton.Instancia.LogIn(usuario)
                  BLLEvento.RegistrarLogin
                  → true
    ↓
LogIn.aspx decide redirección según primerLogin / integridad / bloqueo
```

### 10.2 Registro en bitácora

```
BLL cualquiera llama BLLEvento.RegistrarEvento(tipo, usr, accion, criticidad, modulo)
    ↓
BLLEvento valida criticidad 1-4, sesión activa, usuario no vacío
    ↓
Crea BE.Evento con timestamp truncado a segundos
    ↓
MPPEvento.RegistrarEvento(evento, criticidad)
    ↓
MPPEvento.CalcularDigitosEvento
    ↓
DigitoVerificadorManager.CalcularAmbos(diccionarioValores, out dvh, out dvv)
    ↓
DalGeneral._686DPConsultar("INSERT INTO Evento ...; SELECT SCOPE_IDENTITY();", ...)
    ↓
Devuelve codEvento generado
```

### 10.3 Alta de usuario

```
UsuariosModulo.aspx → btnGuardar_Click
    ↓
Recopila campos del formulario
    ↓
Si rol = Entrenador → prepara BE.Entrenador
Si rol = Cliente  → prepara BE.Alumno (o asocia existente)
    ↓
BLLUsuario.CrearUsuario(...) o CrearUsuario(UsuarioCrearDTO dto)
    ↓
Valida requisitos, genera contraseña si vacía, hashea contraseña
    ↓
MPPUsuario.CrearUsuario(usuario)
    ↓
[ si Entrenador ] BLLEntrenador.CrearEntrenador(entrenador)
[ si Cliente y nuevo ] BLLAlumno.CrearAlumno(alumno)
[ si Cliente existente ] BLLAlumno.AsociarUsuario(dni, usuario)
    ↓
Registra evento EVENTO_ALTA_USUARIO / EVENTO_ASOCIAR_USUARIO
    ↓
Muestra mensaje de éxito
```

### 10.4 Verificación de integridad DV

```
VerificacioDV.aspx → btnVerificar_Click
    ↓
BLLDigitoVerificador.VerificarIntegridad()
    ↓
MPPDigitoVerificador.VerificarIntegridadGlobal()
    ↓
Obtiene tablas registradas en DigitoVerificador
    ↓
Por cada tabla:
  - Lee dvhTabla/dvvTabla almacenados
  - Lee todas las filas
  - Concatena dvh/dvv de filas y hashea
  - Compara con valores almacenados
  - Si no coinciden → VerificarFilasTabla(fila por fila)
    ↓
Para tablas encriptadas delega en MPP especializado
(VerificarIntegridadUsuarios, VerificarIntegridadPreguntas, VerificarIntegridadEventos)
    ↓
Devuelve List<ResultadoVerificacionDV>
    ↓
UI muestra estado por tabla/fila/campo
```

### 10.5 Backup / Restore

```
Admin/BackupRestore.aspx → btnRealizarBackup_Click
    ↓
VerificarAcceso(PermisosSistema.Backup)
    ↓
[ .bak ] BLLDigitoVerificador.RealizarBackup(ruta)
         MPPDigitoVerificador → BACKUP DATABASE [GymApp] TO DISK = @Ruta WITH INIT
[ .bacpac ] BLLDigitoVerificador.ExportarBacpac(ruta)
            MPPDigitoVerificador → ejecuta SqlPackage.exe /Action:Export ...
    ↓
BLLEvento.RegistrarBackup / RegistrarExportarBacpac
    ↓
Mensaje de éxito / error
```

---

## 11. Seguridad

### 11.1 Autenticación

- Forms authentication con cookie segura, HttpOnly, SameSite=Lax.
- Cookie de sesión InProc, 30 minutos.
- Usuarios anónimos denegados globalmente (`<deny users="?"/>`).
- Excepciones: carpetas `LogIn`, `Inicio`, `AccesoDenegado`, `Content`.

### 11.2 Autorización

- `BasePage.VerificarAcceso(modulo)` usa `BLLRol.UsuarioActualTieneAcceso`.
- `DashBoard.Master` oculta opciones de menú según permisos.

### 11.3 Encriptación

- Datos personales: AES-256-CBC con IV aleatorio (formato nuevo) o IV fijo legacy.
- Contraseñas: SHA-256 (no reversible).
- Clave AES en `Web.config` (`AesKey`).

### 11.4 Integridad

- DVH/DVV en cada fila persistente.
- Tabla `DigitoVerificador` con hash agregado por tabla.
- Verificación automática en `BasePage`; pausa el sistema para no administradores si falla.

### 11.5 Auditoría

- `Evento` registra login, logout, CRUD, cambios de rol/contraseña, backup/restore, errores, etc.
- Criticidad 1=Info, 2=Advertencia, 3=Error, 4=Crítico.

### 11.6 Configuración de seguridad en Web.config

```xml
<httpCookies requireSSL="true" httpOnlyCookies="true" sameSite="Lax"/>
<authentication mode="Forms">
  <forms loginUrl="~/LogIn/LogIn.aspx" timeout="30" requireSSL="true"
         protection="All" slidingExpiration="true" cookieSameSite="Lax"/>
</authentication>
<authorization><deny users="?"/></authorization>
<sessionState mode="InProc" cookieless="UseCookies"
              regenerateExpiredSessionId="true" timeout="30"/>
<customErrors mode="RemoteOnly" defaultRedirect="~/AccesoDenegado.aspx">
  <error statusCode="401" redirect="~/AccesoDenegado.aspx"/>
  <error statusCode="403" redirect="~/AccesoDenegado.aspx"/>
  <error statusCode="404" redirect="~/AccesoDenegado.aspx"/>
  <error statusCode="500" redirect="~/AccesoDenegado.aspx"/>
</customErrors>
```

También se envían headers de seguridad HTTP:

```xml
<customHeaders>
  <add name="X-Content-Type-Options" value="nosniff"/>
  <add name="X-Frame-Options" value="DENY"/>
  <add name="X-XSS-Protection" value="1; mode=block"/>
  <add name="Referrer-Policy" value="strict-origin-when-cross-origin"/>
  <add name="Content-Security-Policy" value="..."/>
</customHeaders>
```

---

## 12. Dígitos verificadores DVH/DVV

### 12.1 Concepto

- **DVH (Dígito Verificador Horizontal)**: hash de todos los campos de una fila (excepto `dvv` y `dvh`). Detecta modificaciones de una fila.
- **DVV (Dígito Verificador Vertical)**: hash compuesto por el hash individual de cada campo de la fila. Detecta qué campo específico cambió.
- **Control de tabla**: en `DigitoVerificador` se almacenan los hashes agregados de todos los DVH y DVV de una tabla.

### 12.2 Cálculo

```csharp
// DVH: concatena valores normalizados
string dvHInput = string.Join("|", valores.Values.Select(NormalizarValor));
string dvh = criptoManager.GenerarHashSHA256(dvHInput);

// DVV: concatena hashes individuales de campos
string dvVInput = string.Join("", valores.Select(v => criptoManager.GenerarHashSHA256(NormalizarValor(v.Value))));
string dvv = criptoManager.GenerarHashSHA256(dvVInput);
```

### 12.3 Verificación

1. `MPPDigitoVerificador.VerificarIntegridadGlobal()`.
2. Por cada tabla con control:
   - Lee `dvhTabla`/`dvvTabla`.
   - Lee todas las filas.
   - Recalcula hash agregado.
   - Si coinciden → tabla OK.
   - Si no → verifica fila por fila y produce `ResultadoVerificacionDV`.

### 12.4 Pausa del sistema

```csharp
private void VerificarIntegridadSiAplica()
{
    if (BllDV.ExisteErrorIntegridad() && !BllDV.UsuarioActualEsAdmin())
    {
        RegistrarErrorIntegridad();
        RedirigirSeguro("~/VerificacioDV/VerificacioDV.aspx");
    }
}
```

- Cualquier página protegida (`BasePage`) verifica integridad en `OnInit`.
- Si hay error, usuarios no administradores son redirigidos a la página de verificación y el menú se oculta.
- Administradores pueden continuar y reparar/recalcular.

---

## 13. Dependencias entre proyectos

### 13.1 Referencias de `gymAppV2.csproj`

```xml
<ProjectReference Include="..\BE\BE.csproj">
  <Name>BE</Name>
</ProjectReference>
<ProjectReference Include="..\BLL\BLL.csproj">
  <Name>BLL</Name>
</ProjectReference>
<ProjectReference Include="..\SERVICIOS\SERVICIOS.csproj">
  <Name>SERVICIOS</Name>
</ProjectReference>
```

`BLL` referencia a `BE`, `MPP`, `SERVICIOS`.  
`MPP` referencia a `BE`, `DAL`, `SERVICIOS`.  
`DAL` no referencia a otros proyectos de dominio.  
`SERVICIOS` referencia a `BE` y `DAL` (en `CriptoMigracion`).

### 13.2 Mapa de consumo de capas

| Capa | Consume |
|---|---|
| UI | `BE`, `BLL`, `SERVICIOS` |
| BLL | `BE`, `MPP`, `SERVICIOS` |
| MPP | `BE`, `DAL`, `SERVICIOS` |
| DAL | `System.Data`, `System.Configuration` |
| SERVICIOS | `BE`, `DAL` (parcialmente), `System.Web` |

---

## 14. Observaciones y deuda técnica

### 14.1 Lo que está bien implementado

- Seguridad sólida: encriptación, hashing, bloqueo, historial de contraseñas, bitácora, DVH/DVV.
- Arquitectura en capas clara.
- Manejo de errores global y redirección amigable.
- Backup/restore nativo y BACPAC.
- Preguntas de seguridad encriptadas.

### 14.2 Lo que falta o está incompleto

1. **Módulo de Pagos / Abono mensual**: existe `PrecioModalidad` pero no hay flujo de pago, membresía ni vencimiento.
2. **Inscripción a actividades con cupo**: el calendario es puramente visual; no hay inscripción/desinscripción ni control de cupo.
3. **ABM real de clases programadas**: no existe CRUD de clases con horario y entrenador.
4. **Rutinas, ejercicios, peso y 1RM**: esqueleto de BD sin BLL/MPP/UI implementados.
5. **Gestión de Familias**: no hay entidad `Familia` ni flujo "Cliente familiar".
6. **Notificaciones/alertas**: no hay sistema de banners/email.
7. **Blanqueo de contraseña exacto del documento funcional**: no genera contraseña temporal `Apellido + DNI + "!"`.
8. **Recuperación exacta del documento**: no usa pregunta fija del año de nacimiento ni segunda pregunta por múltiples alumnos.
9. **Check-in con membresía vencida**: solo verifica `Activo`; no descuenta días ni alerta al recepcionista.
10. **Permisos granulares**: las tablas `Familia`/`Permiso` están en BD pero no se usan; la autorización es hardcodeada por rol numérico.

### 14.3 Discrepancias modelo de datos

- El documento funcional indica que datos personales residen en `Alumnos`.
- El código centraliza datos personales en `USUARIOS` y `Alumnos`/`Entrenadores` solo guardan campos específicos del rol.

### 14.4 Dependencia circular potencial

- `BLLUsuario` crea `BLLEvento`.
- `BLLEvento.RegistrarEvento` crea `BLLUsuario` para validar existencia de usuario en eventos pre-autenticación.
- No es recursiva infinita porque cada instancia se usa en contextos separados, pero aumenta el acoplamiento.

### 14.5 Recomendaciones

1. Implementar el módulo de Pagos primero, ya que desbloquea check-in, alertas y membresía.
2. Completar CRUD de clases e inscripciones.
3. Completar Rutinas, Ejercicios, PesoHistorial y AlumnoRM.
4. Refactorizar `BLLEvento` para no depender de `BLLUsuario` (usar MPP directamente o pasar validación a otro lugar).
5. Evaluar inyección de dependencias (aunque Web Forms clásico no la facilita).
6. Documentar y alinear el modelo de datos con el documento funcional o actualizar el documento.

---

## Anexos

### A. Archivos clave por capa

| Capa | Archivos |
|---|---|
| UI | `gymAppV2/*.aspx.cs`, `gymAppV2/*/*.aspx.cs`, `BasePage.cs`, `Global.asax.cs`, `DashBoard.Master.cs`, `Site.Master.cs` |
| BE | `BE/*.cs` |
| BLL | `BLL/*.cs` |
| MPP | `MPP/*.cs` |
| DAL | `DAL/DalGeneral.cs` |
| SERVICIOS | `SERVICIOS/CriptoManager.cs`, `SERVICIOS/DigitoVerificadorManager.cs`, `SERVICIOS/CriptoMigracion.cs`, `SERVICIOS/Singleton/*.cs`, `SERVICIOS/Excepciones/*.cs` |
| Configuración | `gymAppV2/Web.config`, `gymAppV2/gymAppV2.csproj`, `bd-schema-v2.sql` |

### B. Scripts de base de datos relevantes

- `bd-schema-v2.sql`: esquema completo.
- `scripts/crear-digito-verificador.sql`: crea `DigitoVerificador`.
- `scripts/recalcular-dvv-dvh.sql`: recálculo manual.
- `ScriptDatosIniciales.sql`: datos iniciales.

### C. Diagramas existentes

- `docs/diagramas-clase-por-capas.md`: diagramas de clases por capa.
- `docs/diagramas-secuencia.md`: flujos de login, bitácora, backup/restore, DVH/DVV.
- `docs/ado-modo.md`: decisión ADO conectado vs desconectado.

---

*Fin de la documentación técnica.*
