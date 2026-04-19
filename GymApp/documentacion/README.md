# Documentación del Frontend - GymApp

## Resumen Rápido

Esta documentación proporciona una guía completa para el desarrollo frontend en ASP.NET Web Forms para el proyecto GymApp (.NET Framework 4.7.2).

## Estructura de Documentación

### Documentos Principales

| Documento | Descripción |
|-----------|-------------|
| [index.md](index.md) | Índice principal de la documentación |
| [arquitectura-frontend.md](arquitectura-frontend.md) | Arquitectura general del frontend |
| [estructura-carpetas.md](estructura-carpetas.md) | Organización de archivos y directorios |
| [master-pages.md](master-pages.md) | Uso de Master Pages para layouts |
| [content-pages.md](content-pages.md) | Creación de páginas de contenido |
| [user-controls.md](user-controls.md) | Componentes reutilizables |
| [controles-servidor.md](controles-servidor.md) | Controles ASP.NET del servidor |
| [estilos-scripts.md](estilos-scripts.md) | Integración con CSS y JavaScript |
| [flujo-trabajo.md](flujo-trabajo.md) | Proceso de desarrollo frontend |

## Inicio Rápido

### 1. Estructura de Carpetas Recomendada

```
GymApp/
├── Content/
│   ├── css/              # Hojas de estilo
│   ├── js/               # Scripts JavaScript
│   └── images/           # Imágenes
├── Controls/             # User Controls
│   ├── Common/          # Controles comunes
│   ├── Authentication/  # Controles de autenticación
│   ├── Alumnos/         # Controles de alumnos
│   ├── Entrenadores/    # Controles de entrenadores
│   └── Admin/           # Controles de administración
├── MasterPages/          # Master Pages
│   ├── Site.master      # Master page principal
│   ├── Admin.master     # Master page de administración
│   └── Public.master    # Master page pública
└── Pages/                # Páginas de contenido
    ├── Public/          # Páginas públicas
    ├── Authentication/  # Páginas de autenticación
    ├── Alumnos/         # Páginas de alumnos
    ├── Entrenadores/    # Páginas de entrenadores
    └── Admin/           # Páginas de administración
```

### 2. Crear una Master Page

```aspx
<%@ Master Language="C#" AutoEventWireup="true"
    CodeBehind="Site.master.cs" Inherits="GymApp.MasterPages.Site" %>

<!DOCTYPE html>
<html>
<head runat="server">
    <title>GymApp</title>
    <asp:ContentPlaceHolder ID="head" runat="server">
    </asp:ContentPlaceHolder>
    <link href="~/Content/css/site.css" rel="stylesheet" />
</head>
<body>
    <form id="form1" runat="server">
        <asp:ContentPlaceHolder ID="MainContent" runat="server">
        </asp:ContentPlaceHolder>
    </form>
</body>
</html>
```

### 3. Crear una Content Page

```aspx
<%@ Page Title="Inicio" Language="C#"
    MasterPageFile="~/MasterPages/Site.master"
    AutoEventWireup="true"
    CodeBehind="Default.aspx.cs"
    Inherits="GymApp.Pages.Public.Default" %>

<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="MainContent" runat="server">
    <h1>Bienvenido a GymApp</h1>
</asp:Content>
```

### 4. Crear un User Control

```aspx
<%@ Control Language="C#"
    AutoEventWireup="true"
    CodeBehind="MenuControl.ascx.cs"
    Inherits="GymApp.Controls.Common.MenuControl" %>

<div class="menu">
    <asp:Repeater ID="rptMenu" runat="server">
        <ItemTemplate>
            <a href='<%# Eval("Url") %>'><%# Eval("Text") %></a>
        </ItemTemplate>
    </asp:Repeater>
</div>
```

## Conceptos Clave

### Master Pages

- **Propósito**: Definir layout consistente en múltiples páginas
- **Componentes**: Layout base, ContentPlaceHolders, controles comunes
- **Uso**: Las páginas heredan el layout de la Master Page

### Content Pages

- **Propósito**: Proporcionar contenido específico para cada página
- **Componentes**: Contenido ASPX, controles de servidor, Code-Behind
- **Uso**: Heredan de Master Page y llenan ContentPlaceHolders

### User Controls

- **Propósito**: Componentes reutilizables encapsulados
- **Componentes**: Marcado HTML, propiedades públicas, eventos
- **Uso**: Se pueden usar en múltiples páginas y Master Pages

### Controles de Servidor

- **Propósito**: Controles que se ejecutan en el servidor
- **Tipos**: Estándar, validación, datos, navegación
- **Uso**: Captura de datos, visualización, interacción

## Diagramas UML

Todos los documentos incluyen diagramas UML en formato Mermaid que ilustran:

- **Arquitectura de capas**: Cómo se organizan las diferentes capas
- **Secuencia**: Interacción entre componentes a lo largo del tiempo
- **Actividad**: Flujo de procesos y decisiones
- **Proceso**: Flujo de trabajo end-to-end
- **Clases**: Jerarquía y relaciones entre clases
- **Componentes**: Estructura y dependencias de componentes

## Convenciones del Proyecto

### Nomenclatura

- **Archivos ASPX**: PascalCase (ej. Default.aspx)
- **Archivos CSS**: kebab-case (ej. site.css)
- **Archivos JS**: kebab-case (ej. site.js)
- **Clases C#**: PascalCase (ej. UsuarioService)
- **Métodos C#**: PascalCase (ej. LoadData)
- **Variables C#**: camelCase (ej. usuarioService)

### Estructura de Código

- **Separación de responsabilidades**: Mantener marcado y lógica separados
- **Organización**: Agrupar código relacionado
- **Comentarios**: Documentar código complejo
- **Validación**: Validar todas las entradas

## Recursos Adicionales

### Documentación Oficial

- [ASP.NET Web Forms Documentation](https://docs.microsoft.com/en-us/aspnet/web-forms/)
- [Master Pages Overview](https://docs.microsoft.com/en-us/aspnet/web-forms/controls/creating-a-site-wide-layout-using-master-pages-cs)
- [User Controls Overview](https://docs.microsoft.com/en-us/aspnet/web-forms/controls/web-user-controls)

### Herramientas

- **Visual Studio**: IDE principal para desarrollo
- **Chrome DevTools**: Herramientas de desarrollo del navegador
- **Fiddler**: Depuración de tráfico HTTP
- **IIS Express**: Servidor web de desarrollo

### Librerías

- **jQuery**: Biblioteca JavaScript
- **Bootstrap**: Framework CSS
- **FontAwesome**: Iconos

## Estado Actual del Proyecto

- **Framework**: ASP.NET Web Forms (.NET Framework 4.7.2)
- **IDE**: Visual Studio 2017+
- **Servidor**: IIS Express (https://localhost:44378/)
- **Páginas existentes**: Default.aspx, About.aspx, Contact.aspx
- **Master Pages**: Por implementar
- **User Controls**: Por implementar

## Próximos Pasos

1. ✅ Crear estructura de carpetas recomendada
2. ⏳ Implementar Master Pages para layout consistente
3. ⏳ Migrar páginas existentes a usar Master Pages
4. ⏳ Crear User Controls para componentes comunes
5. ⏳ Establecer sistema de estilos y scripts
6. ⏳ Implementar páginas de autenticación
7. ⏳ Implementar páginas de alumnos
8. ⏳ Implementar páginas de entrenadores
9. ⏳ Implementar páginas de administración

## Soporte

Para preguntas o problemas relacionados con el desarrollo frontend en GymApp:

1. Consultar la documentación específica del tema
2. Revisar los diagramas UML para entender la arquitectura
3. Revisar los ejemplos de código en cada documento
4. Consultar la documentación oficial de ASP.NET Web Forms

## Contribución

Para contribuir a la documentación:

1. Mantener la documentación actualizada con los cambios
2. Agregar ejemplos de código para nuevos componentes
3. Actualizar diagramas UML cuando cambie la arquitectura
4. Seguir las convenciones de nomenclatura y formato

---

**Última actualización**: 2026-04-19
**Versión**: 1.0
**Autor**: Equipo de Desarrollo GymApp