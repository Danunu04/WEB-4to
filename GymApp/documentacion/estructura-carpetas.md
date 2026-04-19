# Estructura de Carpetas - GymApp

## Lo General

### Propósito

Este documento define la estructura recomendada de carpetas para organizar el frontend del proyecto GymApp en ASP.NET Web Forms, facilitando el mantenimiento, escalabilidad y colaboración en el desarrollo.

### Principios de Organización

1. **Separación de responsabilidades**: Cada carpeta tiene un propósito claro y definido
2. **Escalabilidad**: La estructura soporta el crecimiento del proyecto
3. **Mantenibilidad**: Fácil localización y modificación de archivos
4. **Colaboración**: Facilita el trabajo en equipo
5. **Convenciones**: Nombres descriptivos y consistentes

### Estructura Recomendada

```
GymApp/
├── App_Code/                    # Código compartido y utilidades
│   ├── Helpers/                 # Clases helper
│   ├── Extensions/              # Extension methods
│   └── Models/                  # Modelos de datos
│
├── App_Data/                    # Datos de la aplicación
│   └── Database/                # Archivos de base de datos local
│
├── App_GlobalResources/         # Recursos globales (localización)
│   └── Resources.resx
│
├── App_LocalResources/          # Recursos locales por página
│   ├── Default.aspx.resx
│   └── Login.aspx.resx
│
├── App_Themes/                   # Temas de la aplicación
│   └── Default/
│       ├── Default.skin
│       └── Default.css
│
├── Content/                      # Recursos estáticos
│   ├── css/                     # Hojas de estilo
│   │   ├── site.css
│   │   ├── layout.css
│   │   ├── components.css
│   │   └── responsive.css
│   │
│   ├── js/                      # Scripts JavaScript
│   │   ├── site.js
│   │   ├── validation.js
│   │   ├── ajax.js
│   │   └── lib/                 # Librerías externas
│   │       ├── jquery.min.js
│   │       └── bootstrap.min.js
│   │
│   ├── images/                  # Imágenes
│   │   ├── icons/
│   │   ├── logos/
│   │   └── backgrounds/
│   │
│   └── fonts/                   # Fuentes personalizadas
│       └── custom-fonts/
│
├── Controls/                     # User Controls reutilizables
│   ├── Common/                  # Controles comunes
│   │   ├── HeaderControl.ascx
│   │   ├── FooterControl.ascx
│   │   ├── MenuControl.ascx
│   │   └── NotificationControl.ascx
│   │
│   ├── Authentication/          # Controles de autenticación
│   │   ├── LoginForm.ascx
│   │   └── RegisterForm.ascx
│   │
│   ├── Alumnos/                 # Controles específicos de alumnos
│   │   ├── AlumnoProfile.ascx
│   │   ├── RutinaDisplay.ascx
│   │   └── ProgresoChart.ascx
│   │
│   ├── Entrenadores/            # Controles específicos de entrenadores
│   │   ├── AlumnoList.ascx
│   │   ├── RutinaEditor.ascx
│   │   └── ProgresoViewer.ascx
│   │
│   └── Admin/                   # Controles de administración
│       ├── UserGrid.ascx
│       ├── PermissionEditor.ascx
│       └── ReportViewer.ascx
│
├── MasterPages/                  # Master Pages
│   ├── Site.master              # Master page principal
│   ├── Site.master.cs           # Code-behind
│   ├── Admin.master             # Master page de administración
│   ├── Admin.master.cs
│   ├── Public.master            # Master page pública
│   └── Public.master.cs
│
├── Pages/                        # Páginas de contenido
│   ├── Public/                  # Páginas públicas
│   │   ├── Default.aspx
│   │   ├── Default.aspx.cs
│   │   ├── About.aspx
│   │   ├── About.aspx.cs
│   │   ├── Contact.aspx
│   │   └── Contact.aspx.cs
│   │
│   ├── Authentication/          # Páginas de autenticación
│   │   ├── Login.aspx
│   │   ├── Login.aspx.cs
│   │   ├── Register.aspx
│   │   ├── Register.aspx.cs
│   │   ├── ForgotPassword.aspx
│   │   └── ForgotPassword.aspx.cs
│   │
│   ├── Alumnos/                 # Páginas de alumnos
│   │   ├── Dashboard.aspx
│   │   ├── Dashboard.aspx.cs
│   │   ├── MiRutina.aspx
│   │   ├── MiRutina.aspx.cs
│   │   ├── MiProgreso.aspx
│   │   └── MiProgreso.aspx.cs
│   │
│   ├── Entrenadores/            # Páginas de entrenadores
│   │   ├── Dashboard.aspx
│   │   ├── Dashboard.aspx.cs
│   │   ├── GestionarAlumnos.aspx
│   │   ├── GestionarAlumnos.aspx.cs
│   │   ├── CrearRutina.aspx
│   │   ├── CrearRutina.aspx.cs
│   │   ├── EditarRutina.aspx
│   │   └── EditarRutina.aspx.cs
│   │
│   └── Admin/                   # Páginas de administración
│       ├── Dashboard.aspx
│       ├── Dashboard.aspx.cs
│       ├── GestionarUsuarios.aspx
│       ├── GestionarUsuarios.aspx.cs
│       ├── GestionarPermisos.aspx
│       ├── GestionarPermisos.aspx.cs
│       ├── Reportes.aspx
│       └── Reportes.aspx.cs
│
├── Services/                     # Servicios y lógica de negocio
│   ├── UsuarioService.cs
│   ├── AlumnoService.cs
│   ├── EntrenadorService.cs
│   ├── ActividadService.cs
│   └── RutinaService.cs
│
├── Utils/                        # Utilidades y helpers
│   ├── SecurityHelper.cs
│   ├── ValidationHelper.cs
│   ├── FormatHelper.cs
│   └── LogHelper.cs
│
├── Web.config                    # Configuración de la aplicación
├── Global.asax                   # Eventos globales de la aplicación
└── Default.aspx                  # Página por defecto (legacy)
```

## Comunicación de Capas

### Relación entre Carpetas y Capas

```mermaid
graph TB
    subgraph "Capa de Presentación"
        A[MasterPages/]
        B[Pages/]
        C[Controls/]
    end

    subgraph "Capa de Lógica"
        D[Services/]
        E[Utils/]
        F[App_Code/]
    end

    subgraph "Capa de Recursos"
        G[Content/]
        H[App_Themes/]
    end

    subgraph "Capa de Datos"
        I[App_Data/]
    end

    subgraph "Capa de Localización"
        J[App_GlobalResources/]
        K[App_LocalResources/]
    end

    A --> G
    B --> G
    C --> G
    A --> H
    B --> H
    B --> D
    C --> D
    D --> E
    D --> F
    D --> I
    A --> J
    B --> K
```

### Flujo de Archivos entre Carpetas

```mermaid
sequenceDiagram
    participant MP as MasterPages
    participant P as Pages
    participant C as Controls
    participant S as Services
    participant CT as Content
    participant U as Utils

    MP->>CT: Carga CSS/JS
    P->>MP: Hereda layout
    P->>C: Usa controles
    C->>CT: Carga recursos
    P->>S: Solicita datos
    S->>U: Usa utilidades
    S-->>P: Retorna datos
    P->>CT: Renderiza HTML
```

## Diagramas UML

### Diagrama de Actividad: Organización de Archivos

```mermaid
flowchart TD
    A[Archivo Nuevo] --> B{Tipo de Archivo?}
    B -->|Master Page| C[MasterPages/]
    B -->|Página ASPX| D{Público?}
    B -->|User Control| E{Tipo de Control?}
    B -->|CSS| F[Content/css/]
    B -->|JavaScript| G[Content/js/]
    B -->|Imagen| H[Content/images/]
    B -->|Servicio| I[Services/]
    B -->|Utilidad| J[Utils/]

    D -->|Sí| K[Pages/Public/]
    D -->|No| L{Rol?}

    L -->|Alumno| M[Pages/Alumnos/]
    L -->|Entrenador| N[Pages/Entrenadores/]
    L -->|Admin| O[Pages/Admin/]

    E -->|Común| P[Controls/Common/]
    E -->|Autenticación| Q[Controls/Authentication/]
    E -->|Alumno| R[Controls/Alumnos/]
    E -->|Entrenador| S[Controls/Entrenadores/]
    E -->|Admin| T[Controls/Admin/]
```

### Diagrama de Proceso: Flujo de Desarrollo

```mermaid
graph LR
    A[Requisitos] --> B[Identificar Tipo]
    B --> C{Es Página?}
    C -->|Sí| D[Crear en Pages/]
    C -->|No| E{Es Control?}
    E -->|Sí| F[Crear en Controls/]
    E -->|No| G{Es Master Page?}
    G -->|Sí| H[Crear en MasterPages/]
    G -->|No| I{Es Servicio?}
    I -->|Sí| J[Crear en Services/]
    I -->|No| K[Crear en ubicación apropiada]

    D --> L[Crear Code-Behind]
    F --> L
    H --> L
    J --> L
    L --> M[Implementar lógica]
    M --> N[Agregar estilos]
    N --> O[Agregar scripts]
    O --> P[Probar]
```

### Diagrama de Componentes: Estructura de Carpetas

```mermaid
graph TB
    subgraph "GymApp"
        subgraph "Presentación"
            MP[MasterPages/]
            P[Pages/]
            C[Controls/]
        end

        subgraph "Lógica"
            S[Services/]
            U[Utils/]
            AC[App_Code/]
        end

        subgraph "Recursos"
            CT[Content/]
            AT[App_Themes/]
        end

        subgraph "Datos"
            AD[App_Data/]
        end

        subgraph "Localización"
            AGR[App_GlobalResources/]
            ALR[App_LocalResources/]
        end

        subgraph "Configuración"
            WC[Web.config]
            GA[Global.asax]
        end
    end

    MP --> CT
    P --> CT
    C --> CT
    MP --> AT
    P --> AT
    P --> S
    C --> S
    S --> U
    S --> AC
    S --> AD
    P --> ALR
    MP --> AGR
```

## Convenciones de Nomenclatura

### Archivos

- **Master Pages**: PascalCase con extensión `.master`
  - Ejemplo: `Site.master`, `Admin.master`

- **Páginas ASPX**: PascalCase con extensión `.aspx`
  - Ejemplo: `Default.aspx`, `Dashboard.aspx`

- **Code-Behind**: Mismo nombre que la página con extensión `.aspx.cs`
  - Ejemplo: `Default.aspx.cs`, `Dashboard.aspx.cs`

- **User Controls**: PascalCase con extensión `.ascx`
  - Ejemplo: `HeaderControl.ascx`, `MenuControl.ascx`

- **CSS**: kebab-case con extensión `.css`
  - Ejemplo: `site.css`, `main-layout.css`

- **JavaScript**: kebab-case con extensión `.js`
  - Ejemplo: `site.js`, `validation.js`

- **Imágenes**: kebab-case con extensión apropiada
  - Ejemplo: `logo.png`, `background-image.jpg`

- **Clases C#**: PascalCase
  - Ejemplo: `UsuarioService`, `ValidationHelper`

### Carpetas

- **Carpetas principales**: PascalCase
  - Ejemplo: `MasterPages`, `Controls`, `Services`

- **Subcarpetas**: PascalCase
  - Ejemplo: `Authentication`, `Alumnos`, `Entrenadores`

- **Recursos estáticos**: minúsculas
  - Ejemplo: `css`, `js`, `images`, `fonts`

## Consideraciones Especiales

### Separación por Rol

Las páginas y controles están organizados por rol de usuario:
- **Public**: Páginas accesibles sin autenticación
- **Authentication**: Páginas de login y registro
- **Alumnos**: Páginas específicas para alumnos
- **Entrenadores**: Páginas específicas para entrenadores
- **Admin**: Páginas de administración

### Reutilización de Componentes

Los User Controls se organizan por funcionalidad:
- **Common**: Controles usados en múltiples secciones
- **Authentication**: Controles de autenticación
- **Alumnos**: Controles específicos de alumnos
- **Entrenadores**: Controles específicos de entrenadores
- **Admin**: Controles de administración

### Gestión de Recursos

Los recursos estáticos se organizan por tipo:
- **css**: Hojas de estilo
- **js**: Scripts JavaScript
- **images**: Imágenes
- **fonts**: Fuentes personalizadas

### Localización

Los recursos de localización se organizan por alcance:
- **App_GlobalResources**: Recursos compartidos globalmente
- **App_LocalResources**: Recursos específicos por página

## Mejores Prácticas

### Organización de Archivos

1. **Mantener archivos relacionados juntos**: Code-behind junto con su archivo .aspx
2. **Usar subcarpetas lógicas**: Agrupar archivos por funcionalidad
3. **Limitar profundidad de carpetas**: Máximo 3-4 niveles de profundidad
4. **Nombres descriptivos**: Usar nombres que indiquen claramente el propósito

### Gestión de Dependencias

1. **Minimizar dependencias circulares**: Evitar que módulos dependan entre sí
2. **Usar interfaces**: Definir contratos claros entre componentes
3. **Separar capas**: Mantener presentación, lógica y datos separados

### Mantenimiento

1. **Documentar estructura**: Mantener documentación actualizada
2. **Revisar periódicamente**: Reorganizar si la estructura deja de ser óptima
3. **Eliminar archivos no usados**: Mantener el proyecto limpio
4. **Versionar recursos**: Usar control de versiones para todos los archivos

---

**Última actualización**: 2026-04-19
**Versión**: 1.0