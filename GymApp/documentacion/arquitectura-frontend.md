# Arquitectura del Frontend - GymApp

## Lo General

### Propósito

Este documento describe la arquitectura del frontend en ASP.NET Web Forms para el proyecto GymApp, proporcionando una visión completa de las capas, componentes y su interacción.

### Visión General

El frontend de GymApp sigue una arquitectura basada en **ASP.NET Web Forms**, que utiliza un modelo de programación basado en eventos y controles de servidor. La arquitectura se organiza en varias capas que trabajan juntas para proporcionar una experiencia de usuario cohesiva.

### Componentes Principales

1. **Master Pages (.master)**: Plantillas maestras que definen el layout consistente de la aplicación
2. **Content Pages (.aspx)**: Páginas de contenido que heredan de Master Pages
3. **User Controls (.ascx)**: Componentes reutilizables encapsulados
4. **Controles de Servidor**: Controles ASP.NET predefinidos y personalizados
5. **Code-Behind (.aspx.cs)**: Lógica del lado del servidor en C#
6. **Recursos Estáticos**: CSS, JavaScript, imágenes y otros assets

### Patrones de Diseño Utilizados

- **Master Page Pattern**: Herencia de layout para consistencia visual
- **User Control Pattern**: Reutilización de componentes
- **Code-Behind Pattern**: Separación entre marcado y lógica
- **ViewState Pattern**: Mantenimiento de estado entre postbacks
- **Event-Driven Programming**: Manejo de eventos del lado del servidor

### Stack Tecnológico

- **Framework**: ASP.NET Web Forms (.NET Framework 4.7.2)
- **Lenguaje**: C#
- **Marcado**: ASPX (HTML con controles de servidor)
- **Estilos**: CSS3
- **Scripts**: JavaScript / jQuery
- **Servidor**: IIS Express
- **IDE**: Visual Studio 2017+

## Comunicación de Capas

### Arquitectura de Capas

El frontend de GymApp se organiza en las siguientes capas:

```mermaid
graph TB
    subgraph "Capa de Presentación"
        A[Master Pages]
        B[Content Pages]
        C[User Controls]
    end

    subgraph "Capa de Lógica de Presentación"
        D[Code-Behind]
        E[Event Handlers]
        F[Page Lifecycle]
    end

    subgraph "Capa de Datos de Presentación"
        G[ViewState]
        H[Session State]
        I[Application State]
    end

    subgraph "Capa de Recursos"
        J[CSS]
        K[JavaScript]
        L[Imágenes]
    end

    A --> B
    A --> C
    B --> D
    C --> D
    D --> E
    E --> F
    F --> G
    F --> H
    F --> I
    B --> J
    B --> K
    B --> L
```

### Flujo de Datos entre Capas

```mermaid
sequenceDiagram
    participant Usuario as Usuario
    participant Browser as Navegador
    participant Page as Página ASPX
    participant CodeBehind as Code-Behind
    participant Server as Servidor IIS
    participant DB as Base de Datos

    Usuario->>Browser: Solicita página
    Browser->>Server: HTTP GET
    Server->>Page: Inicializa página
    Page->>CodeBehind: Page_Load
    CodeBehind->>DB: Consulta datos
    DB-->>CodeBehind: Retorna datos
    CodeBehind->>Page: Renderiza controles
    Page-->>Server: HTML generado
    Server-->>Browser: HTTP Response
    Browser-->>Usuario: Muestra página

    Usuario->>Browser: Interactúa (click, submit)
    Browser->>Server: HTTP POST
    Server->>Page: Procesa postback
    Page->>CodeBehind: Event Handlers
    CodeBehind->>DB: Actualiza datos
    CodeBehind->>Page: Actualiza ViewState
    Page-->>Server: HTML actualizado
    Server-->>Browser: HTTP Response
    Browser-->>Usuario: Muestra actualización
```

### Contratos de API e Interfaces

#### Master Page Interface

```csharp
// Interfaz implícita de Master Page
public interface IMasterPage
{
    string Title { get; set; }
    void ShowMessage(string message, MessageType type);
    void SetActiveMenu(string menuId);
}
```

#### Content Page Interface

```csharp
// Interfaz base para páginas de contenido
public interface IContentPage
{
    void InitializePage();
    void LoadData();
    void SaveData();
    void ValidateInput();
}
```

#### User Control Interface

```csharp
// Interfaz para User Controls
public interface IUserControl
{
    void LoadControlData(object data);
    object GetControlData();
    void ClearControl();
    event EventHandler DataChanged;
}
```

### Inyección de Dependencias y Relaciones de Servicios

```mermaid
graph LR
    subgraph "Servicios de Negocio"
        A[UsuarioService]
        B[AlumnoService]
        C[EntrenadorService]
        D[ActividadService]
    end

    subgraph "Capa de Presentación"
        E[Default.aspx]
        F[Alumnos.aspx]
        G[Entrenadores.aspx]
        H[Actividades.aspx]
    end

    subgraph "User Controls"
        I[MenuControl.ascx]
        J[HeaderControl.ascx]
        K[FooterControl.ascx]
        L[NotificationControl.ascx]
    end

    A --> E
    B --> F
    C --> G
    D --> H
    I --> E
    I --> F
    I --> G
    I --> H
    J --> E
    J --> F
    J --> G
    J --> H
    K --> E
    K --> F
    K --> G
    K --> H
    L --> E
    L --> F
    L --> G
    L --> H
```

## Diagramas UML

### Diagrama de Secuencia: Ciclo de Vida de Página

```mermaid
sequenceDiagram
    participant Request as HTTP Request
    participant Page as Page
    participant Controls as Controles
    participant ViewState as ViewState
    participant Events as Event Handlers
    participant Response as HTTP Response

    Request->>Page: Inicia solicitud
    Page->>Page: Construct
    Page->>Page: Page_Init
    Page->>Controls: Crear controles
    Controls-->>Page: Controles creados
    Page->>ViewState: Cargar ViewState
    ViewState-->>Page: Estado restaurado
    Page->>Page: Page_Load
    Page->>Page: Control Events
    Page->>Events: Disparar eventos
    Events-->>Page: Eventos procesados
    Page->>Page: Page_PreRender
    Page->>ViewState: Guardar ViewState
    ViewState-->>Page: Estado guardado
    Page->>Page: Page_Render
    Page->>Response: HTML generado
    Response-->>Request: Respuesta enviada
```

### Diagrama de Actividad: Proceso de Renderizado

```mermaid
flowchart TD
    A[Solicitud HTTP] --> B{Es Postback?}
    B -->|No| C[Page_Init]
    B -->|Sí| D[Restaurar ViewState]
    D --> C
    C --> E[Crear Controles]
    E --> F[Page_Load]
    F --> G{Es Postback?}
    G -->|No| H[Cargar Datos Iniciales]
    G -->|Sí| I[Procesar Eventos]
    H --> J[Page_PreRender]
    I --> J
    J --> K[Guardar ViewState]
    K --> L[Page_Render]
    L --> M[Generar HTML]
    M --> N[Enviar Respuesta]
```

### Diagrama de Proceso: Flujo de Desarrollo Frontend

```mermaid
graph LR
    A[Requisitos] --> B[Diseño UI/UX]
    B --> C[Crear Master Page]
    C --> D[Definir ContentPlaceholders]
    D --> E[Crear Content Pages]
    E --> F[Implementar Code-Behind]
    F --> G[Crear User Controls]
    G --> H[Agregar Estilos CSS]
    H --> I[Agregar Scripts JS]
    I --> J[Pruebas Unitarias]
    J --> K[Pruebas de Integración]
    K --> L[Despliegue]
```

### Diagrama de Clases: Jerarquía de Páginas

```mermaid
classDiagram
    class Page {
        +Page_Init()
        +Page_Load()
        +Page_PreRender()
        +Page_Render()
        +IsPostBack
        +ViewState
    }

    class MasterPage {
        +MasterPage_Init()
        +MasterPage_Load()
        +ContentPlaceHolders
        +Header
        +Footer
        +Menu
    }

    class ContentPage {
        +Page_Load()
        +Custom Methods
        +Event Handlers
    }

    class UserControl {
        +Page_Load()
        +Custom Properties
        +Custom Methods
        +Events
    }

    class ServerControl {
        +ID
        +Text
        +Visible
        +Enabled
        +Events
    }

    Page <|-- MasterPage
    Page <|-- ContentPage
    Page <|-- UserControl
    UserControl *-- ServerControl
    ContentPage --> MasterPage : uses
    ContentPage --> UserControl : contains
```

### Diagrama de Componentes: Estructura del Frontend

```mermaid
graph TB
    subgraph "Aplicación GymApp"
        subgraph "Master Pages"
            MP1[Site.master]
            MP2[Admin.master]
        end

        subgraph "Páginas Públicas"
            CP1[Default.aspx]
            CP2[Login.aspx]
            CP3[Register.aspx]
        end

        subgraph "Páginas de Alumnos"
            CP4[Dashboard.aspx]
            CP5[MiRutina.aspx]
            CP6[MiProgreso.aspx]
        end

        subgraph "Páginas de Entrenadores"
            CP7[EntrenadorDashboard.aspx]
            CP8[GestionarAlumnos.aspx]
            CP9[CrearRutina.aspx]
        end

        subgraph "Páginas de Administración"
            CP10[AdminDashboard.aspx]
            CP11[GestionarUsuarios.aspx]
            CP12[Reportes.aspx]
        end

        subgraph "User Controls"
            UC1[MenuControl.ascx]
            UC2[HeaderControl.ascx]
            UC3[FooterControl.ascx]
            UC4[NotificationControl.ascx]
            UC5[UserProfileControl.ascx]
        end

        subgraph "Recursos"
            R1[CSS/]
            R2[JS/]
            R3[Images/]
        end
    end

    MP1 --> CP1
    MP1 --> CP2
    MP1 --> CP3
    MP1 --> CP4
    MP1 --> CP5
    MP1 --> CP6
    MP2 --> CP7
    MP2 --> CP8
    MP2 --> CP9
    MP2 --> CP10
    MP2 --> CP11
    MP2 --> CP12

    CP1 --> UC1
    CP1 --> UC2
    CP1 --> UC3
    CP4 --> UC1
    CP4 --> UC2
    CP4 --> UC3
    CP4 --> UC5
    CP7 --> UC1
    CP7 --> UC2
    CP7 --> UC3
    CP10 --> UC1
    CP10 --> UC2
    CP10 --> UC3

    MP1 --> R1
    MP1 --> R2
    MP1 --> R3
    MP2 --> R1
    MP2 --> R2
    MP2 --> R3
```

## Límites de Módulos e Interfaces

### Módulo de Autenticación

**Responsabilidades**:
- Gestión de login de usuarios
- Control de acceso basado en roles
- Manejo de sesiones

**Interfaces**:
- `IAuthenticationService`: Servicios de autenticación
- `IUserSession`: Gestión de sesión de usuario

**Dependencias**:
- Base de datos de usuarios
- Sistema de permisos

### Módulo de Gestión de Alumnos

**Responsabilidades**:
- Visualización de perfil de alumno
- Gestión de rutinas asignadas
- Seguimiento de progreso

**Interfaces**:
- `IAlumnoService`: Servicios de gestión de alumnos
- `IRutinaService`: Servicios de rutinas

**Dependencias**:
- Módulo de autenticación
- Base de datos de alumnos y rutinas

### Módulo de Gestión de Entrenadores

**Responsabilidades**:
- Gestión de alumnos asignados
- Creación y edición de rutinas
- Visualización de progreso de alumnos

**Interfaces**:
- `IEntrenadorService`: Servicios de gestión de entrenadores
- `IRutinaService`: Servicios de rutinas

**Dependencias**:
- Módulo de autenticación
- Base de datos de entrenadores y rutinas

### Módulo de Administración

**Responsabilidades**:
- Gestión de usuarios y permisos
- Configuración del sistema
- Generación de reportes

**Interfaces**:
- `IAdminService`: Servicios de administración
- `IReportService`: Servicios de reportes

**Dependencias**:
- Todos los módulos de negocio
- Base de datos completa

## Consideraciones de Diseño

### Separación de Responsabilidades

- **Presentación**: ASPX/ASCX files (marcado)
- **Lógica**: Code-Behind files (C#)
- **Estilos**: CSS files
- **Comportamiento**: JavaScript files

### Mantenimiento de Estado

- **ViewState**: Estado de controles entre postbacks
- **SessionState**: Datos de sesión del usuario
- **ApplicationState**: Datos globales de la aplicación
- **Cookies**: Datos persistentes en el cliente

### Seguridad

- Validación de entrada en servidor
- Protección contra CSRF
- Encriptación de datos sensibles
- Control de acceso basado en roles

### Performance

- Minimizar ViewState
- Usar caching apropiadamente
- Optimizar consultas a base de datos
- Comprimir recursos estáticos

---

**Última actualización**: 2026-04-19
**Versión**: 1.0