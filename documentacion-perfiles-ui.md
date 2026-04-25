# Documentación: Adaptación de UI por Perfil de Usuario

## Resumen

Esta documentación describe cómo implementar un sistema donde **una misma pantalla cambia su diseño visual y los datos que muestra al 100% según el perfil del usuario logueado** (Administrador, Recepcionista, Entrenador, Alumno).

---

## 1. Problema actual

El esquema de base de datos define perfiles y permisos, pero **no existe una tabla que vincule usuarios con perfiles**. Sin esa relación, no es posible saber qué perfil tiene cada usuario.

```
USUARIOS ──(?)──> Perfiles     ← NO EXISTE esta tabla
Perfiles ──> Perfil_Permiso    ← SÍ existe
Perfiles ──> Perfil_Familia    ← SÍ existe
```

---

## 2. Cambios en la base de datos

### 2.1 Crear tabla USUARIO_Perfil

```sql
CREATE TABLE USUARIO_Perfil (
    usr         VARCHAR(50)     NOT NULL,
    idPerfil    INT             NOT NULL,
    dvv         VARCHAR(50)     NOT NULL,
    dvh         VARCHAR(50)     NOT NULL,
    CONSTRAINT PK_USUARIO_Perfil PRIMARY KEY (usr, idPerfil),
    CONSTRAINT FK_USUARIO_Perfil_Usuario FOREIGN KEY (usr) REFERENCES USUARIOS(usr),
    CONSTRAINT FK_USUARIO_Perfil_Perfil FOREIGN KEY (idPerfil) REFERENCES Perfiles(idPerfil)
);
GO

CREATE INDEX IX_USUARIO_Perfil_usr ON USUARIO_Perfil(usr);
GO
```

### 2.2 Datos de ejemplo para Perfiles

```sql
INSERT INTO Perfiles (nombrePerfil, dvv, dvh) VALUES ('Administrador', '0', '0');
INSERT INTO Perfiles (nombrePerfil, dvv, dvh) VALUES ('Recepcionista', '0', '0');
INSERT INTO Perfiles (nombrePerfil, dvv, dvh) VALUES ('Entrenador', '0', '0');
INSERT INTO Perfiles (nombrePerfil, dvv, dvh) VALUES ('Alumno', '0', '0');
GO
```

### 2.3 Datos de ejemplo para Permisos

```sql
-- Permisos de navegación (uno por cada página/módulo)
INSERT INTO Permiso (nombre, dvv, dvh) VALUES ('pag_inicio', '0', '0');
INSERT INTO Permiso (nombre, dvv, dvh) VALUES ('pag_alumnos', '0', '0');
INSERT INTO Permiso (nombre, dvv, dvh) VALUES ('pag_entrenadores', '0', '0');
INSERT INTO Permiso (nombre, dvv, dvh) VALUES ('pag_actividades', '0', '0');
INSERT INTO Permiso (nombre, dvv, dvh) VALUES ('pag_rutinas', '0', '0');
INSERT INTO Permiso (nombre, dvv, dvh) VALUES ('pag_pagos', '0', '0');
INSERT INTO Permiso (nombre, dvv, dvh) VALUES ('pag_reportes', '0', '0');
INSERT INTO Permiso (nombre, dvv, dvh) VALUES ('pag_eventos', '0', '0');
INSERT INTO Permiso (nombre, dvv, dvh) VALUES ('pag_usuarios', '0', '0');
INSERT INTO Permiso (nombre, dvv, dvh) VALUES ('pag_mi_perfil', '0', '0');

-- Permisos de acción (CRUD y operaciones)
INSERT INTO Permiso (nombre, dvv, dvh) VALUES ('crear_alumno', '0', '0');
INSERT INTO Permiso (nombre, dvv, dvh) VALUES ('modificar_alumno', '0', '0');
INSERT INTO Permiso (nombre, dvv, dvh) VALUES ('eliminar_alumno', '0', '0');
INSERT INTO Permiso (nombre, dvv, dvh) VALUES ('crear_entrenador', '0', '0');
INSERT INTO Permiso (nombre, dvv, dvh) VALUES ('modificar_entrenador', '0', '0');
INSERT INTO Permiso (nombre, dvv, dvh) VALUES ('ver_rutina_propia', '0', '0');
INSERT INTO Permiso (nombre, dvv, dvh) VALUES ('crear_rutina', '0', '0');
INSERT INTO Permiso (nombre, dvv, dvh) VALUES ('registrar_pago', '0', '0');
INSERT INTO Permiso (nombre, dvv, dvh) VALUES ('ver_reportes', '0', '0');
INSERT INTO Permiso (nombre, dvv, dvh) VALUES ('gestionar_usuarios', '0', '0');
GO
```

### 2.4 Familias de permisos (patrón Composite)

```sql
-- Familias (grupos lógicos de permisos)
INSERT INTO Familia (nombre, profundidad, dvv, dvh) VALUES ('Gestión Alumnos', 0, '0', '0');
INSERT INTO Familia (nombre, profundidad, dvv, dvh) VALUES ('Gestión Entrenadores', 0, '0', '0');
INSERT INTO Familia (nombre, profundidad, dvv, dvh) VALUES ('Gestión Rutinas', 0, '0', '0');
INSERT INTO Familia (nombre, profundidad, dvv, dvh) VALUES ('Gestión Pagos', 0, '0', '0');
INSERT INTO Familia (nombre, profundidad, dvv, dvh) VALUES ('Administración', 0, '0', '0');
GO

-- Asociar permisos a familias
-- Gestión Alumnos
INSERT INTO PermisoFamilia (permisoId, familiaId, dvv, dvh)
SELECT permisoId, (SELECT familiaId FROM Familia WHERE nombre='Gestión Alumnos'), '0', '0'
FROM Permiso WHERE nombre IN ('pag_alumnos', 'crear_alumno', 'modificar_alumno', 'eliminar_alumno');

-- Gestión Entrenadores
INSERT INTO PermisoFamilia (permisoId, familiaId, dvv, dvh)
SELECT permisoId, (SELECT familiaId FROM Familia WHERE nombre='Gestión Entrenadores'), '0', '0'
FROM Permiso WHERE nombre IN ('pag_entrenadores', 'crear_entrenador', 'modificar_entrenador');

-- Gestión Rutinas
INSERT INTO PermisoFamilia (permisoId, familiaId, dvv, dvh)
SELECT permisoId, (SELECT familiaId FROM Familia WHERE nombre='Gestión Rutinas'), '0', '0'
FROM Permiso WHERE nombre IN ('pag_rutinas', 'ver_rutina_propia', 'crear_rutina');

-- Gestión Pagos
INSERT INTO PermisoFamilia (permisoId, familiaId, dvv, dvh)
SELECT permisoId, (SELECT familiaId FROM Familia WHERE nombre='Gestión Pagos'), '0', '0'
FROM Permiso WHERE nombre IN ('pag_pagos', 'registrar_pago');

-- Administración
INSERT INTO PermisoFamilia (permisoId, familiaId, dvv, dvh)
SELECT permisoId, (SELECT familiaId FROM Familia WHERE nombre='Administración'), '0', '0'
FROM Permiso WHERE nombre IN ('pag_reportes', 'pag_usuarios', 'pag_eventos', 'ver_reportes', 'gestionar_usuarios');
GO
```

### 2.5 Asignar permisos a perfiles

```sql
-- Administrador: TODO
INSERT INTO Perfil_Familia (idPerfil, idFamilia, dvv, dvh)
SELECT (SELECT idPerfil FROM Perfiles WHERE nombrePerfil='Administrador'), familiaId, '0', '0'
FROM Familia;

-- Recepcionista: Gestión Alumnos + Pagos
INSERT INTO Perfil_Familia (idPerfil, idFamilia, dvv, dvh) VALUES
((SELECT idPerfil FROM Perfiles WHERE nombrePerfil='Recepcionista'),
 (SELECT familiaId FROM Familia WHERE nombre='Gestión Alumnos'), '0', '0');
INSERT INTO Perfil_Familia (idPerfil, idFamilia, dvv, dvh) VALUES
((SELECT idPerfil FROM Perfiles WHERE nombrePerfil='Recepcionista'),
 (SELECT familiaId FROM Familia WHERE nombre='Gestión Pagos'), '0', '0');

-- Entrenador: Gestión Rutinas (crear/ver) + ver Entrenadores
INSERT INTO Perfil_Permiso (idPerfil, idPermiso, dvv, dvh) VALUES
((SELECT idPerfil FROM Perfiles WHERE nombrePerfil='Entrenador'),
 (SELECT permisoId FROM Permiso WHERE nombre='pag_rutinas'), '0', '0');
INSERT INTO Perfil_Permiso (idPerfil, idPermiso, dvv, dvh) VALUES
((SELECT idPerfil FROM Perfiles WHERE nombrePerfil='Entrenador'),
 (SELECT permisoId FROM Permiso WHERE nombre='crear_rutina'), '0', '0');
INSERT INTO Perfil_Permiso (idPerfil, idPermiso, dvv, dvh) VALUES
((SELECT idPerfil FROM Perfiles WHERE nombrePerfil='Entrenador'),
 (SELECT permisoId FROM Permiso WHERE nombre='pag_entrenadores'), '0', '0');
INSERT INTO Perfil_Permiso (idPerfil, idPermiso, dvv, dvh) VALUES
((SELECT idPerfil FROM Perfiles WHERE nombrePerfil='Entrenador'),
 (SELECT permisoId FROM Permiso WHERE nombre='pag_mi_perfil'), '0', '0');

-- Alumno: ver Rutinas propias + Mi Perfil + Actividades
INSERT INTO Perfil_Permiso (idPerfil, idPermiso, dvv, dvh) VALUES
((SELECT idPerfil FROM Perfiles WHERE nombrePerfil='Alumno'),
 (SELECT permisoId FROM Permiso WHERE nombre='pag_inicio'), '0', '0');
INSERT INTO Perfil_Permiso (idPerfil, idPermiso, dvv, dvh) VALUES
((SELECT idPerfil FROM Perfiles WHERE nombrePerfil='Alumno'),
 (SELECT permisoId FROM Permiso WHERE nombre='pag_actividades'), '0', '0');
INSERT INTO Perfil_Permiso (idPerfil, idPermiso, dvv, dvh) VALUES
((SELECT idPerfil FROM Perfiles WHERE nombrePerfil='Alumno'),
 (SELECT permisoId FROM Permiso WHERE nombre='ver_rutina_propia'), '0', '0');
INSERT INTO Perfil_Permiso (idPerfil, idPermiso, dvv, dvh) VALUES
((SELECT idPerfil FROM Perfiles WHERE nombrePerfil='Alumno'),
 (SELECT permisoId FROM Permiso WHERE nombre='pag_mi_perfil'), '0', '0');
GO
```

---

## 3. Arquitectura por capas

La solución usa las 6 capas ya existentes en el proyecto. Cada capa tiene una responsabilidad:

```
┌──────────────────────────────┐
│   gymAppV2 (Web Forms UI)    │  ← Páginas .aspx, Master Page, CSS
├──────────────────────────────┤
│   BLL (Business Logic)       │  ← Validaciones, orquestación
├──────────────────────────────┤
│   SERVICIOS (Services)       │  ← Casos de uso (LoginService, PermisoService)
├──────────────────────────────┤
│   BE (Business Entities)     │  ← Clases de dominio
├──────────────────────────────┤
│   MPP (Mapper/Persistence)  │  ← Mapeo DataSet → BE
├──────────────────────────────┤
│   DAL (Data Access)          │  ← Conexión SQL, ejecución de queries
└──────────────────────────────┘
```

### 3.1 BE — Entidades de negocio

Crear los siguientes archivos en `BE/`:

```
BE/
├── Usuario.cs              # Propiedades: Usr, Contrasena, Contrasenas (lista)
├── Perfil.cs               # Propiedades: IdPerfil, NombrePerfil
├── Permiso.cs               # Propiedades: PermisoId, Nombre
├── FamiliaPermiso.cs        # Propiedades: FamiliaId, Nombre, Permisos (lista), Profundidad
├── Alumno.cs                # Propiedades: Dni, Nombre, Apellido, Telefono, FechaNacimiento, Usr
├── Entrenador.cs            # Propiedades: Dni, Nombre, Apellido, FechaNacimiento, Usr
├── IComponentePermiso.cs    # Interface Composite: ObtenerPermisos(), TienePermiso(nombre)
└── SesionUsuario.cs         # Singleton: UsuarioActual, Perfil, Permisos, IsAutenticado
```

**IComponentePermiso.cs** — Interface del patrón Composite:
```csharp
public interface IComponentePermiso
{
    string Nombre { get; }
    List<Permiso> ObtenerPermisos();
    bool TienePermiso(string nombrePermiso);
}
```

**Permiso.cs** — Hoja del Composite:
```csharp
public class Permiso : IComponentePermiso
{
    public int PermisoId { get; set; }
    public string Nombre { get; set; }

    public List<Permiso> ObtenerPermisos() => new List<Permiso> { this };

    public bool TienePermiso(string nombrePermiso)
        => Nombre.Equals(nombrePermiso, StringComparison.OrdinalIgnoreCase);
}
```

**FamiliaPermiso.cs** — Compuesto del Composite:
```csharp
public class FamiliaPermiso : IComponentePermiso
{
    public int FamiliaId { get; set; }
    public string Nombre { get; set; }
    public bool Profundidad { get; set; }
    public List<IComponentePermiso> Componentes { get; set; } = new List<IComponentePermiso>();

    public List<Permiso> ObtenerPermisos()
    {
        var permisos = new List<Permiso>();
        foreach (var comp in Componentes)
            permisos.AddRange(comp.ObtenerPermisos());
        return permisos;
    }

    public bool TienePermiso(string nombrePermiso)
        => Componentes.Any(c => c.TienePermiso(nombrePermiso));
}
```

**SesionUsuario.cs** — Singleton para manejar sesión:
```csharp
public sealed class SesionUsuario
{
    private static SesionUsuario _instancia;
    private static readonly object _lock = new object();

    public string Usr { get; private set; }
    public Perfil PerfilActual { get; private set; }
    public List<IComponentePermiso> Permisos { get; private set; }
    public bool IsAutenticado { get; private set; }
    public string TipoEntidad { get; private set; } // "Alumno", "Entrenador", "Admin", "Recepcionista"

    private SesionUsuario() { }

    public static SesionUsuario Instancia
    {
        get
        {
            lock (_lock)
            {
                if (_instancia == null)
                    _instancia = new SesionUsuario();
                return _instancia;
            }
        }
    }

    public void Login(string usr, Perfil perfil, List<IComponentePermiso> permisos, string tipoEntidad)
    {
        Usr = usr;
        PerfilActual = perfil;
        Permisos = permisos;
        TipoEntidad = tipoEntidad;
        IsAutenticado = true;
    }

    public void Logout()
    {
        Usr = null;
        PerfilActual = null;
        Permisos = null;
        TipoEntidad = null;
        IsAutenticado = false;
    }

    public bool TienePermiso(string nombrePermiso)
    {
        if (Permisos == null) return false;
        return Permisos.Any(p => p.TienePermiso(nombrePermiso));
    }
}
```

### 3.2 DAL — Acceso a datos

Crear `DAL/DALUsuario.cs`:
```csharp
public class DALUsuario
{
    public DataSet ObtenerUsuario(string usr) { /* SELECT * FROM USUARIOS WHERE usr = @usr */ }
    public DataSet ObtenerPerfilesDeUsuario(string usr) { /* SELECT p.* FROM Perfiles p INNER JOIN USUARIO_Perfil up ON p.idPerfil = up.idPerfil WHERE up.usr = @usr */ }
    public DataSet ObtenerPermisosDePerfil(int idPerfil) { /* SELECT per.* FROM Permiso per INNER JOIN Perfil_Permiso pp ON per.permisoId = pp.idPermiso WHERE pp.idPerfil = @idPerfil */ }
    public DataSet ObtenerFamiliasDePerfil(int idPerfil) { /* SELECT f.* FROM Familia f INNER JOIN Perfil_Familia pf ON f.familiaId = pf.idFamilia WHERE pf.idPerfil = @idPerfil */ }
    public DataSet ObtenerPermisosDeFamilia(int familiaId) { /* SELECT per.* FROM Permiso per INNER JOIN PermisoFamilia pf ON per.permisoId = pf.permisoId WHERE pf.familiaId = @familiaId */ }
    public DataSet ValidarCredenciales(string usr, string hashContra) { /* SELECT usr FROM USUARIOS WHERE usr = @usr AND contra = @hashContra */ }
    public int ObtenerIntentos(string usr) { /* SELECT intentos FROM USUARIO_Intentos WHERE usr = @usr */ }
    public void IncrementarIntento(string usr) { /* UPDATE USUARIO_Intentos SET intentos = intentos + 1 WHERE usr = @usr */ }
    public void ResetearIntentos(string usr) { /* UPDATE USUARIO_Intentos SET intentos = 0 WHERE usr = @usr */ }
}
```

### 3.3 MPP — Mapeo DataSet → BE

Crear `MPP/MPPUsuario.cs`:
```csharp
public class MPPUsuario
{
    public Usuario MapearUsuario(DataSet ds) { /* Mapea fila a BE.Usuario */ }
    public List<Perfil> MapearPerfiles(DataSet ds) { /* Mapea filas a List<Perfil> */ }
    public List<Permiso> MapearPermisos(DataSet ds) { /* Mapea filas a List<Permiso> */ }
    public List<FamiliaPermiso> MapearFamilias(DataSet ds, List<Permiso> todosPermisos) { /* Mapea filas y asocia permisos */ }
}
```

### 3.4 SERVICIOS — Lógica de negocio

Crear `SERVICIOS/LoginService.cs`:
```csharp
public class LoginService
{
    private readonly DALUsuario _dal = new DALUsuario();
    private readonly MPPUsuario _mpp = new MPPUsuario();

    public LoginResultado Login(string usr, string contra)
    {
        // 1. Verificar intentos (< 3)
        // 2. Hashear contraseña con SHA256
        // 3. Validar credenciales contra USUARIOS
        // 4. Si falla: incrementar intento, lanzar excepción
        // 5. Si OK: resetear intentos, cargar perfil y permisos
        // 6. Llamar a SesionUsuario.Instancia.Login(...)
        // 7. Registrar evento en tabla Evento
    }

    public void Logout()
    {
        // Registrar evento, llamar SesionUsuario.Instancia.Logout()
        // Abandonar sesión HTTP
    }
}
```

Crear `SERVICIOS/PermisoService.cs`:
```csharp
public class PermisoService
{
    public List<IComponentePermiso> ObtenerPermisosPerfil(int idPerfil)
    {
        // 1. Traer permisos directos de Perfil_Permiso
        // 2. Traer familias de Perfil_Familia
        // 3. Por cada familia, traer sus permisos de PermisoFamilia
        // 4. Armar estructura Composite
        // 5. Retornar lista mixta (Permisos + FamiliaPermiso)
    }
}
```

### 3.5 BLL — Fachada

Crear `BLL/BLLSeguridad.cs`:
```csharp
public class BLLSeguridad
{
    private readonly LoginService _loginService = new LoginService();
    private readonly PermisoService _permisoService = new PermisoService();

    public LoginResultado Login(string usr, string contra) => _loginService.Login(usr, contra);
    public void Logout() => _loginService.Logout();
    public bool TienePermiso(string nombrePermiso) => SesionUsuario.Instancia.TienePermiso(nombrePermiso);
    public string ObtenerPerfil() => SesionUsuario.Instancia.PerfilActual?.NombrePerfil ?? "";
    public string ObtenerTipoEntidad() => SesionUsuario.Instancia.TipoEntidad ?? "";
}
```

---

## 4. Flujo de login completo

```
Usuario ingresa usr + contra
        │
        ▼
┌─ LogIn.aspx.cs ─────────────────────────────────────────────┐
│ 1. BLLSeguridad.Login(usr, contra)                          │
│    ├─ SHA256(contra)                                         │
│    ├─ Verificar intentos < 3                                 │
│    ├─ Buscar en USUARIOS                                     │
│    ├─ Si falla: intentos++ → bloquear si == 3               │
│    └─ Si OK: resetear intentos, cargar datos                │
│ 2. Obtener perfil de USUARIO_Perfil                          │
│ 3. Obtener permisos del perfil (Composite)                   │
│ 4. SesionUsuario.Instancia.Login(usr, perfil, permisos, tipo)│
│ 5. Guardar usr en Session["Usr"]                            │
│ 6. Registrar evento en tabla Evento                          │
│ 7. Response.Redirect("~/Default.aspx")                       │
└──────────────────────────────────────────────────────────────┘
```

---

## 5. Adaptación de la UI por perfil

Hay **4 niveles** donde la UI se adapta al perfil:

### 5.1 Nivel 1: Master Page (navegación y layout)

**`Site.Master.cs`** — Page_Load dinámico:

```csharp
protected void Page_Load(object sender, EventArgs e)
{
    if (Session["Usr"] == null)
        Response.Redirect("~/LogIn/LogIn.aspx");

    var sesion = SesionUsuario.Instancia;
    var bll = new BLLSeguridad();

    // 1. Construir menú de navegación según permisos
    ConstruirMenu(sesion);

    // 2. Aplicar tema CSS según perfil
    AplicarTema(sesion.PerfilActual.NombrePerfil);

    // 3. Mostrar nombre del usuario
    lblUsuario.Text = $"{sesion.Usr} ({sesion.PerfilActual.NombrePerfil})";
}

private void ConstruirMenu(SesionUsuario sesion)
{
    // Mapeo de permisos a páginas del menú
    var menuItems = new Dictionary<string, (string Url, string Icono, string Texto)>
    {
        { "pag_inicio",        ("~/Default.aspx",        "fa-home",      "Inicio") },
        { "pag_alumnos",       ("~/Alumnos.aspx",       "fa-users",      "Alumnos") },
        { "pag_entrenadores",  ("~/Entrenadores.aspx",  "fa-dumbbell",   "Entrenadores") },
        { "pag_actividades",  ("~/Actividades.aspx",    "fa-calendar",   "Actividades") },
        { "pag_rutinas",       ("~/Rutinas.aspx",       "fa-list-alt",   "Rutinas") },
        { "pag_pagos",         ("~/Pagos.aspx",         "fa-credit-card","Pagos") },
        { "pag_reportes",      ("~/Reportes.aspx",      "fa-chart-bar",  "Reportes") },
        { "pag_usuarios",      ("~/Usuarios.aspx",       "fa-user-shield","Usuarios") },
    };

    navMenu.Controls.Clear();
    foreach (var kvp in menuItems)
    {
        if (sesion.TienePermiso(kvp.Key))
        {
            var li = new HtmlGenericControl("li");
            li.Attributes["class"] = "nav-item";
            var a = new HtmlGenericControl("a");
            a.Attributes["class"] = "nav-link";
            a.Attributes["href"] = ResolveUrl(kvp.Value.Url);
            a.InnerHtml = $"<i class=\"fas {kvp.Value.Icono}\"></i> {kvp.Value.Texto}";
            li.Controls.Add(a);
            navMenu.Controls.Add(li);
        }
    }
}

private void AplicarTema(string perfil)
{
    // Agrega una clase CSS al body según el perfil
    bodyTag.Attributes["class"] = $"theme-{perfil.ToLower()}";
}
```

**`Site.Master` markup** — Definir el contenedor con `runat="server"`:
```html
<body id="bodyTag" runat="server">
    <!-- ... navbar con <ul id="navMenu" runat="server"> ... -->
</body>
```

### 5.2 Nivel 2: Temas CSS por perfil

Crear un archivo `Content/themes.css` con variables CSS por perfil:

```css
/* ============================================
   TEMAS POR PERFIL - Sportio
   Usa rem como unidad (1rem = 16px)
   ============================================ */

/* --- Administrador: Teal Strength --- */
.theme-administrador {
    --primary: #2D8C7C;
    --primary-light: #E1F5EE;
    --primary-dark: #1F6B5E;
    --bg-body: #FFF8F0;
    --bg-card: #FFFFFF;
    --text-heading: #2C2A26;
    --text-body: #2C2A26;
    --accent: #F5874F;
    --navbar-bg: #2D8C7C;
    --navbar-text: #FFFFFF;
    --btn-primary-bg: #2D8C7C;
    --btn-primary-hover: #1F6B5E;
    --btn-danger-bg: #F4736B;
    --sidebar-width: 15rem;
}

/* --- Recepcionista: Warm Orange --- */
.theme-recepcionista {
    --primary: #F5874F;
    --primary-light: #FAECE7;
    --primary-dark: #D46A35;
    --bg-body: #FFF8F0;
    --bg-card: #FFFFFF;
    --text-heading: #2C2A26;
    --text-body: #2C2A26;
    --accent: #2D8C7C;
    --navbar-bg: #F5874F;
    --navbar-text: #FFFFFF;
    --btn-primary-bg: #F5874F;
    --btn-primary-hover: #D46A35;
    --btn-danger-bg: #F4736B;
    --sidebar-width: 15rem;
}

/* --- Entrenador: Energy Yellow --- */
.theme-entrenador {
    --primary: #F5C842;
    --primary-light: #FEF8E7;
    --primary-dark: #D4A830;
    --bg-body: #FFF8F0;
    --bg-card: #FFFFFF;
    --text-heading: #2C2A26;
    --text-body: #2C2A26;
    --accent: #2D8C7C;
    --navbar-bg: #F5C842;
    --navbar-text: #2C2A26;
    --btn-primary-bg: #F5C842;
    --btn-primary-hover: #D4A830;
    --btn-danger-bg: #F4736B;
    --sidebar-width: 15rem;
}

/* --- Alumno: Coral Burn --- */
.theme-alumno {
    --primary: #F4736B;
    --primary-light: #FAEAE9;
    --primary-dark: #D45A53;
    --bg-body: #FFF8F0;
    --bg-card: #FFFFFF;
    --text-heading: #2C2A26;
    --text-body: #2C2A26;
    --accent: #2D8C7C;
    --navbar-bg: #F4736B;
    --navbar-text: #FFFFFF;
    --btn-primary-bg: #F4736B;
    --btn-primary-hover: #D45A53;
    --btn-danger-bg: #F4736B;
    --btn-danger-bg: #D45A53;
    --sidebar-width: 0rem; /* Sin sidebar para alumnos */
}

/* --- Estilos base que usan las variables --- */
.theme-administrador .navbar,
.theme-recepcionista .navbar,
.theme-entrenador .navbar,
.theme-alumno .navbar {
    background-color: var(--navbar-bg) !important;
}

.theme-administrador .nav-link,
.theme-recepcionista .nav-link,
.theme-entrenador .nav-link,
.theme-alumno .nav-link {
    color: var(--navbar-text) !important;
}

.theme-administrador .btn-primary,
.theme-recepcionista .btn-primary,
.theme-entrenador .btn-primary,
.theme-alumno .btn-primary {
    background-color: var(--btn-primary-bg);
    border-color: var(--btn-primary-bg);
    color: white;
}

.theme-administrador .btn-primary:hover,
.theme-recepcionista .btn-primary:hover,
.theme-entrenador .btn-primary:hover,
.theme-alumno .btn-primary:hover {
    background-color: var(--btn-primary-hover);
    border-color: var(--btn-primary-hover);
}

.theme-administrador .card,
.theme-recepcionista .card,
.theme-entrenador .card,
.theme-alumno .card {
    background-color: var(--bg-card);
    border-left: 0.25rem solid var(--primary);
}

.theme-administrador .page-header,
.theme-recepcionista .page-header,
.theme-entrenador .page-header,
.theme-alumno .page-header {
    border-bottom: 0.125rem solid var(--primary);
    color: var(--primary-dark);
}

/* Dashboard cards */
.theme-administrador .stat-card,
.theme-recepcionista .stat-card,
.theme-entrenador .stat-card,
.theme-alumno .stat-card {
    border-left: 0.375rem solid var(--primary);
    background-color: var(--primary-light);
}
```

### 5.3 Nivel 3: Página compartida con Paneles condicionales

**Ejemplo: `Default.aspx` como Dashboard por perfil**

```html
<%@ Page Title="Inicio" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="Default.aspx.cs" Inherits="gymAppV2.Default" %>

<asp:Content ID="Content1" ContentPlaceHolderID="MainContent" runat="server">

    <h2 class="page-header">Bienvenido, <asp:Literal ID="litNombre" runat="server" /></h2>

    <!-- Panel Administrador: vista completa del gimnasio -->
    <asp:Panel ID="pnlAdmin" runat="server" Visible="false">
        <div class="row">
            <div class="col-md-3"><div class="stat-card card p-3 mb-3"><h5>Alumnos Activos</h5><asp:Label ID="lblTotalAlumnos" runat="server" CssClass="h2" /></div></div>
            <div class="col-md-3"><div class="stat-card card p-3 mb-3"><h5>Entrenadores</h5><asp:Label ID="lblTotalEntrenadores" runat="server" CssClass="h2" /></div></div>
            <div class="col-md-3"><div class="stat-card card p-3 mb-3"><h5>Actividades</h5><asp:Label ID="lblTotalActividades" runat="server" CssClass="h2" /></div></div>
            <div class="col-md-3"><div class="stat-card card p-3 mb-3"><h5>Pagos del Mes</h5><asp:Label ID="lblPagosMes" runat="server" CssClass="h2" /></div></div>
        </div>
        <div class="row">
            <div class="col-md-8"><h4>Últimos Eventos</h4><asp:GridView ID="gvEventos" runat="server" CssClass="table table-striped" /></div>
            <div class="col-md-4"><h4>Pagos Pendientes</h4><asp:GridView ID="gvPagosPendientes" runat="server" CssClass="table table-striped" /></div>
        </div>
    </asp:Panel>

    <!-- Panel Recepcionista: gestión de alumnos y pagos -->
    <asp:Panel ID="pnlRecepcionista" runat="server" Visible="false">
        <div class="row">
            <div class="col-md-4"><div class="stat-card card p-3 mb-3"><h5>Nuevos Alumnos Hoy</h5><asp:Label ID="lblNuevosHoy" runat="server" CssClass="h2" /></div></div>
            <div class="col-md-4"><div class="stat-card card p-3 mb-3"><h5>Abonos Vencidos</h5><asp:Label ID="lblVencidos" runat="server" CssClass="h2" /></div></div>
            <div class="col-md-4"><div class="stat-card card p-3 mb-3"><h5>Pagos Hoy</h5><asp:Label ID="lblPagosHoy" runat="server" CssClass="h2" /></div></div>
        </div>
        <div class="row">
            <div class="col-md-12">
                <h4>Alumnos con Abono Próximo a Vencer</h4>
                <asp:GridView ID="gvAlumnosVencimiento" runat="server" CssClass="table table-striped" />
            </div>
        </div>
    </asp:Panel>

    <!-- Panel Entrenador: sus clases y rutinas -->
    <asp:Panel ID="pnlEntrenador" runat="server" Visible="false">
        <div class="row">
            <div class="col-md-4"><div class="stat-card card p-3 mb-3"><h5>Mis Clases Hoy</h5><asp:Label ID="lblClasesHoy" runat="server" CssClass="h2" /></div></div>
            <div class="col-md-4"><div class="stat-card card p-3 mb-3"><h5>Alumnos Asignados</h5><asp:Label ID="lblAlumnosAsignados" runat="server" CssClass="h2" /></div></div>
            <div class="col-md-4"><div class="stat-card card p-3 mb-3"><h5>Rutinas Creadas</h5><asp:Label ID="lblRutinasCreadas" runat="server" CssClass="h2" /></div></div>
        </div>
        <div class="row">
            <div class="col-md-12">
                <h4>Mis Actividades</h4>
                <asp:GridView ID="gvMisActividades" runat="server" CssClass="table table-striped" />
            </div>
        </div>
    </asp:Panel>

    <!-- Panel Alumno: su información personal -->
    <asp:Panel ID="pnlAlumno" runat="server" Visible="false">
        <div class="row">
            <div class="col-md-6">
                <div class="card p-4 mb-3">
                    <h4>Mi Información</h4>
                    <p><strong>Nombre:</strong> <asp:Label ID="lblNombreAlumno" runat="server" /></p>
                    <p><strong>Abono hasta:</strong> <asp:Label ID="lblAbonoHasta" runat="server" /></p>
                    <p><strong>Estado:</strong> <asp:Label ID="lblEstadoAlumno" runat="server" /></p>
                </div>
            </div>
            <div class="col-md-6">
                <div class="card p-4 mb-3">
                    <h4>Mis Actividades</h4>
                    <asp:GridView ID="gvMisActividadesAlumno" runat="server" CssClass="table table-striped" />
                </div>
            </div>
        </div>
        <div class="row">
            <div class="col-md-12">
                <h4>Mi Rutina</h4>
                <asp:GridView ID="gvMiRutina" runat="server" CssClass="table table-striped" />
            </div>
        </div>
    </asp:Panel>

</asp:Content>
```

**`Default.aspx.cs`** — Code-behind:
```csharp
protected void Page_Load(object sender, EventArgs e)
{
    var sesion = SesionUsuario.Instancia;

    if (!sesion.IsAutenticado)
        Response.Redirect("~/LogIn/LogIn.aspx");

    litNombre.Text = sesion.Usr;

    switch (sesion.PerfilActual.NombrePerfil.ToLower())
    {
        case "administrador":
            pnlAdmin.Visible = true;
            CargarDashboardAdmin();
            break;
        case "recepcionista":
            pnlRecepcionista.Visible = true;
            CargarDashboardRecepcionista();
            break;
        case "entrenador":
            pnlEntrenador.Visible = true;
            CargarDashboardEntrenador(sesion.Usr);
            break;
        case "alumno":
            pnlAlumno.Visible = true;
            CargarDashboardAlumno(sesion.Usr);
            break;
    }
}
```

### 5.4 Nivel 4: Misma página, mismos datos, diferente vista

Para páginas donde **todos los perfiles** ven datos similares pero con distinta información o acciones:

```csharp
// Ejemplo: Página de Actividades
// - Administrador: ve TODO, puede CRUD completo
// - Entrenador: ve solo SUS actividades, puede modificarlas
// - Alumno: ve actividades disponibles, puede inscribirse
// - Recepcionista: ve actividades con info de pagos

protected void Page_Load(object sender, EventArgs e)
{
    var sesion = SesionUsuario.Instancia;

    // Botones de acción según permisos
    btnCrearActividad.Visible = sesion.TienePermiso("crear_actividad");
    btnModificarActividad.Visible = sesion.TienePermiso("modificar_actividad");
    btnEliminarActividad.Visible = sesion.TienePermiso("eliminar_actividad");

    // Columnas del GridView según perfil
    gvActividades.Columns[0].Visible = sesion.TienePermiso("modificar_actividad"); // Editar
    gvActividades.Columns[1].Visible = sesion.TienePermiso("eliminar_actividad"); // Eliminar
    gvActividades.Columns[5].Visible = sesion.TienePermiso("ver_costo");          // Costo
    gvActividades.Columns[6].Visible = sesion.TienePermiso("ver_valor");          // Valor (solo admin)

    // Cargar datos filtrados según perfil
    switch (sesion.TipoEntidad)
    {
        case "Alumno":
            gvActividades.DataSource = actividadService.ObtenerActividadesDisponibles();
            break;
        case "Entrenador":
            gvActividades.DataSource = actividadService.ObtenerActividadesPorEntrenador(sesion.Usr);
            break;
        default: // Admin, Recepcionista
            gvActividades.DataSource = actividadService.ObtenerTodas();
            break;
    }
    gvActividades.DataBind();
}
```

---

## 6. Configuración de Web.config

Agregar en `<system.web>` dentro de `<configuration>`:

```xml
<system.web>
    <authentication mode="Forms">
        <forms loginUrl="~/LogIn/LogIn.aspx" timeout="30" />
    </authentication>
    <authorization>
        <deny users="?" />
    </authorization>
    <sessionState timeout="30" />
</system.web>

<!-- Permitir acceso anónimo solo a la página de login -->
<location path="LogIn">
    <system.web>
        <authorization>
            <allow users="*" />
        </authorization>
    </system.web>
</location>
```

Agregar la connection string:
```xml
<connectionStrings>
    <add name="GymAppConnection"
         connectionString="Server=localhost;Database=GymApp;Integrated Security=True;"
         providerName="System.Data.SqlClient" />
</connectionStrings>
```

---

## 7. Página de Login funcional

### `LogIn/LogIn.aspx` (markup corregido)

Los `<input>` actuales deben convertirse a controles ASP.NET con `runat="server"`:

```html
<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="LogIn.aspx.cs" Inherits="gymAppV2.LogIn" %>

<!DOCTYPE html>
<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <meta charset="utf-8" />
    <meta name="viewport" content="width=device-width, initial-scale=1" />
    <title>Sportio - Iniciar Sesión</title>
    <link href="https://cdn.jsdelivr.net/npm/bootstrap@5.3.0/dist/css/bootstrap.min.css" rel="stylesheet" />
    <link href="StyleSheet1.css" rel="stylesheet" />
</head>
<body>
    <form id="form1" runat="server">
        <div class="BloqueRosa">
            <h1 class="titulo">Sportio</h1>
            <p class="subtitulo">Iniciá sesión</p>

            <div class="formulario">
                <asp:Label ID="lblError" runat="server" CssClass="text-danger" Visible="false" />
                <div class="mb-3">
                    <label for="txtUsuario" class="form-label">Usuario</label>
                    <asp:TextBox ID="txtUsuario" runat="server" CssClass="form-control" placeholder="Tu usuario" />
                </div>
                <div class="mb-3">
                    <label for="txtContrasena" class="form-label">Contraseña</label>
                    <asp:TextBox ID="txtContrasena" runat="server" CssClass="form-control" TextMode="Password" placeholder="Tu contraseña" />
                </div>
                <asp:Button ID="btnLogin" runat="server" Text="Iniciar sesión" CssClass="btnFormLogIn" OnClick="btnLogin_Click" />
            </div>
        </div>
    </form>
</body>
</html>
```

### `LogIn/LogIn.aspx.cs` (code-behind)

```csharp
using System;
using gymAppV2.BLL;
using gymAppV2.BE;

namespace gymAppV2
{
    public partial class LogIn : System.Web.UI.Page
    {
        private readonly BLLSeguridad _bll = new BLLSeguridad();

        protected void Page_Load(object sender, EventArgs e)
        {
            // Si ya está logueado, redirigir al inicio
            if (SesionUsuario.Instancia.IsAutenticado)
                Response.Redirect("~/Default.aspx");
        }

        protected void btnLogin_Click(object sender, EventArgs e)
        {
            try
            {
                var resultado = _bll.Login(txtUsuario.Text.Trim(), txtContrasena.Text);

                if (resultado.Exito)
                {
                    FormsAuthentication.SetAuthCookie(resultado.Usr, false);
                    Session["Usr"] = resultado.Usr;
                    Response.Redirect("~/Default.aspx");
                }
                else
                {
                    lblError.Text = resultado.Mensaje;
                    lblError.Visible = true;
                }
            }
            catch (Exception ex)
            {
                lblError.Text = $"Error: {ex.Message}";
                lblError.Visible = true;
            }
        }
    }
}
```

---

## 8. BasePage para validación de permisos

Crear `gymAppV2/BasePage.cs` — todas las páginas heredan de esta:

```csharp
using System;
using System.Web.UI;
using gymAppV2.BE;

namespace gymAppV2
{
    public class BasePage : Page
    {
        protected SesionUsuario Sesion => SesionUsuario.Instancia;

        /// <summary>
        /// Permiso requerido para acceder a esta página.
        /// Sobreescribir en cada página con el permiso correspondiente.
        /// Ej: protected override string PermisoRequerido => "pag_alumnos";
        /// </summary>
        protected virtual string PermisoRequerido => null;

        protected override void OnPreInit(EventArgs e)
        {
            base.OnPreInit(e);

            // Verificar autenticación
            if (!Sesion.IsAutenticado || Session["Usr"] == null)
            {
                Response.Redirect("~/LogIn/LogIn.aspx");
                return;
            }

            // Verificar permiso
            if (!string.IsNullOrEmpty(PermisoRequerido) && !Sesion.TienePermiso(PermisoRequerido))
            {
                Response.Redirect("~/Default.aspx?sinPermiso=1");
            }
        }
    }
}
```

**Uso en cada página:**
```csharp
// Alumnos.aspx.cs
public partial class Alumnos : BasePage
{
    protected override string PermisoRequerido => "pag_alumnos";

    protected void Page_Load(object sender, EventArgs e)
    {
        // Solo llega aquí si tiene el permiso
        // Mostrar/ocultar botones según permisos adicionales
        btnCrear.Visible = Sesion.TienePermiso("crear_alumno");
        btnEliminar.Visible = Sesion.TienePermiso("eliminar_alumno");
    }
}
```

---

## 9. Diagrama de flujo completo

```
┌──────────────────────────────────────────────────────────────────────┐
│                        LOG IN                                       │
│  Usuario → LogIn.aspx → BLLSeguridad.Login()                       │
│     ├── Credenciales OK → SesionUsuario.Login() → Default.aspx     │
│     ├── Credenciales mal → Intentos++ → ¿3? → Bloquear cuenta     │
│     └── Cuenta bloqueada → Preguntas de seguridad → Cambio contra  │
└──────────────────────────────────────────────────────────────────────┘
        │
        ▼
┌──────────────────────────────────────────────────────────────────────┐
│                    DEFAULT.ASPX (Dashboard)                         │
│  ┌────────────────────────────────────────────────────────────────┐  │
│  │ SesionUsuario.Instancia.PerfilActual.NombrePerfil             │  │
│  │                                                                │  │
│  │  "Administrador"  → pnlAdmin.Visible = true                  │  │
│  │  "Recepcionista"  → pnlRecepcionista.Visible = true          │  │
│  │  "Entrenador"     → pnlEntrenador.Visible = true             │  │
│  │  "Alumno"         → pnlAlumno.Visible = true                 │  │
│  └────────────────────────────────────────────────────────────────┘  │
│  ┌────────────────────────────────────────────────────────────────┐  │
│  │ CSS: <body class="theme-administrador|recepcionista|...">     │  │
│  │ → Colores, navbar, botones cambian automáticamente            │  │
│  └────────────────────────────────────────────────────────────────┘  │
│  ┌────────────────────────────────────────────────────────────────┐  │
│  │ NAVBAR: Solo muestra links de páginas donde                   │  │
│  │ SesionUsuario.TienePermiso("pag_xxx") == true                │  │
│  └────────────────────────────────────────────────────────────────┘  │
└──────────────────────────────────────────────────────────────────────┘
        │
        ▼
┌──────────────────────────────────────────────────────────────────────┐
│              CUALQUIER PÁGINA (hereda de BasePage)                   │
│  ┌────────────────────────────────────────────────────────────────┐  │
│  │ OnPreInit: ¿Autenticado? ¿Tiene PermisoRequerido?            │  │
│  │    No → Redirect a Login o a Default                          │  │
│  └────────────────────────────────────────────────────────────────┘  │
│  ┌────────────────────────────────────────────────────────────────┐  │
│  │ Page_Load:                                                    │  │
│  │   btnCrear.Visible = Sesion.TienePermiso("crear_xxx")         │  │
│  │   btnEliminar.Visible = Sesion.TienePermiso("eliminar_xxx")   │  │
│  │   gvDatos.Columns[5].Visible = Sesion.TienePermiso("ver_xxx")│  │
│  │   gvDatos.DataSource = service.ObtenerXPerfil(tipoEntidad)    │  │
│  └────────────────────────────────────────────────────────────────┘  │
└──────────────────────────────────────────────────────────────────────┘
```

---

## 10. Resumen de archivos a crear/modificar

### Archivos nuevos
| Archivo | Capa | Descripción |
|---------|------|-------------|
| `BE/Usuario.cs` | BE | Entidad Usuario |
| `BE/Perfil.cs` | BE | Entidad Perfil |
| `BE/Permiso.cs` | BE | Entidad Permiso (hoja del Composite) |
| `BE/FamiliaPermiso.cs` | BE | Entidad FamiliaPermiso (compuesto del Composite) |
| `BE/IComponentePermiso.cs` | BE | Interface del patrón Composite |
| `BE/SesionUsuario.cs` | BE | Singleton de sesión |
| `BE/LoginResultado.cs` | BE | Resultado del login (éxito, mensaje, datos) |
| `DAL/DALUsuario.cs` | DAL | Queries SQL para usuarios, perfiles y permisos |
| `MPP/MPPUsuario.cs` | MPP | Mapeo DataSet → BE |
| `SERVICIOS/LoginService.cs` | SERVICIOS | Lógica de autenticación |
| `SERVICIOS/PermisoService.cs` | SERVICIOS | Lógica de permisos (Composite) |
| `SERVICIOS/EncriptacionService.cs` | SERVICIOS | SHA256 y AES-256 |
| `SERVICIOS/EventoService.cs` | SERVICIOS | Registro de eventos en bitácora |
| `BLL/BLLSeguridad.cs` | BLL | Fachada de seguridad |
| `gymAppV2/BasePage.cs` | Web | Página base con verificación de permisos |
| `Content/themes.css` | Web | Variables CSS por perfil |

### Archivos a modificar
| Archivo | Cambio |
|---------|--------|
| `ScriptCreacion.sql` | Agregar tabla USUARIO_Perfil + datos de ejemplo |
| `Web.config` | Authentication forms, connection string |
| `Site.Master` | Agregar `runat="server"` al body, menú dinámico |
| `Site.Master.cs` | Construir menú y aplicar tema según perfil |
| `LogIn/LogIn.aspx` | Reemplazar `<input>` por `asp:TextBox`, agregar lógica |
| `LogIn/LogIn.aspx.cs` | Implementar btnLogin_Click con BLLSeguridad |
| `Default.aspx` | Convertir a Dashboard con paneles por perfil |
| `Default.aspx.cs` | Lógica condicional por perfil |

---

## 11. Orden de implementación recomendado

```
1. Base de datos        → Script USUARIO_Perfil + datos de ejemplo
2. BE                   → Entidades + IComponentePermiso + SesionUsuario
3. DAL                  → DALUsuario (queries)
4. MPP                  → MPPUsuario (mapeo)
5. SERVICIOS            → EncriptacionService → LoginService → PermisoService → EventoService
6. BLL                  → BLLSeguridad
7. Web.config           → Authentication + connection string
8. Login funcional      → LogIn.aspx + LogIn.aspx.cs
9. BasePage + Master    → BasePage.cs + Site.Master modificado
10. CSS por perfil      → themes.css
11. Dashboard           → Default.aspx con paneles por perfil
12. Páginas internas    → Cada .aspx hereda de BasePage, filtra por permisos
```

Cada paso se puede probar independientemente antes de pasar al siguiente.