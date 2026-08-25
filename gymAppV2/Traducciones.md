# Sistema de Traducciones — GymApp

## Cómo funciona

Cada página hereda de `BasePage`, que expone el método `T("tag")`. Cuando se llama, busca el tag en un diccionario `Dictionary<string, string>` cargado desde SQL para el idioma activo. El diccionario se cachea en `HttpContext.Current.Items` por request (no por sesión).

```
ASPX / CS  →  T("tag")  →  BasePage  →  BLLTraduccion  →  MPPTraduccion  →  DalTraduccion
                                                                              ↓
                                                                   lee TODAS las tablas
                                                                   del esquema Traducciones
                                                                   para el IdiomaID activo
```

El diccionario es **plano y global**: todos los tags de todas las tablas se mezclan en un solo `Dictionary<string, string>`. Por eso un tag de `Pantalla_Alumnos` puede usarse desde `Usuarios.aspx.cs` — no hay scope por página.

El idioma activo vive en `GestorIdioma.IdiomaActual` (singleton de sesión). Cuando el usuario lo cambia, el gestor notifica a todas las páginas suscritas vía el patrón Observer (`IIdiomaObserver.OnIdiomaChanged`).

---

## Idiomas soportados

| IdiomaID | Código | Nombre    | Enum `IdiomaApp` |
|----------|--------|-----------|------------------|
| 1        | ES     | Español   | `IdiomaApp.ES`   |
| 2        | EN     | Inglés    | `IdiomaApp.EN`   |
| 3        | PT     | Portugués | `IdiomaApp.PT`   |
| 4        | FR     | Francés   | `IdiomaApp.FR`   |
| 5        | JA     | Japonés   | `IdiomaApp.JA`   |

> El DAL usa `(int)IdiomaApp + 1` como `IdiomaID`. **No cambiar el orden de la tabla Idiomas.**

---

## Tablas SQL (esquema `Traducciones`)

Cada tabla tiene:
- `TraduccionID` IDENTITY PK
- `IdiomaID` FK → `Traducciones.Idiomas` + UNIQUE (una fila por idioma)
- Una columna `NVARCHAR(500)` por cada tag de esa pantalla

### Tablas actuales y sus tags

| Tabla                      | Pantalla / Sección              | Tags |
|----------------------------|---------------------------------|------|
| `Pantalla_Login`           | Login                           | `login_titulo`, `login_usuario`, `login_contrasena`, `login_btn`, `login_error_credenciales` |
| `Pantalla_Idioma`          | Selector de idioma              | `idioma_titulo`, `idioma_subtitulo`, `idioma_guardado`, `idioma_activo` |
| `Pantalla_DashBoard`       | Sidebar / menú lateral          | `menu_dashboard`, `menu_usuarios`, `menu_alumnos`, `menu_entrenadores`, `menu_actividades`, `menu_rutinas`, `menu_permisos`, `menu_bitacora`, `menu_respaldo`, `menu_pagos`, `menu_perfil`, `menu_idioma`, `menu_cambiar_contra`, `menu_cerrar_sesion` |
| `Pantalla_DashboardContent`| Panel principal (WebForm1)      | `dash_titulo`, `dash_bienvenido`, `dash_kpi_miembros`, `dash_kpi_clases`, `dash_kpi_ingresos`, `dash_kpi_retencion`, `dash_semana_titulo`, `dash_col_actividad`, `dash_col_instructor`, `dash_col_dia`, `dash_col_horario`, `dash_col_duracion`, `dash_col_estado`, `dash_badge_completada`, `dash_badge_pendiente`, `dash_badge_programada` |
| `Pantalla_Usuarios`        | Gestión de Usuarios             | ver detalle abajo |
| `Pantalla_Alumnos`         | Gestión de Alumnos              | ver detalle abajo |
| `Pantalla_Actividades`     | Calendario de Actividades       | `actividades_titulo`, `actividades_btn_nueva`, `actividades_cliente_info`, `actividades_modal_titulo`, `actividades_dia_dom`…`actividades_dia_sab`, `actividades_meses_json` (array JSON), `actividades_dia_titulo_fmt` (formato con `{0}` día `{1}` mes), `actividades_ver_detalles`, más tags de campos y colores del modal |
| `Pantalla_Rutinas`         | Rutinas                         | `rutinas_titulo`, `rutinas_subtitulo`, `rutinas_cliente_msg`, `rutinas_admin_msg` |
| `Comunes_Botones`          | Botones reutilizables           | `btn_guardar`, `btn_cancelar`, `btn_eliminar`, `btn_editar`, `btn_agregar`, `btn_buscar`, `btn_volver`, `btn_confirmar` |
| `Comunes_Mensajes`         | Mensajes de estado y footer     | `toast_exito`, `toast_error`, `toast_advertencia`, `toast_informacion`, `msg_sin_resultados`, `msg_cargando`, `msg_error_generico`, `msg_acceso_denegado`, `msg_mostrando_fmt`, `msg_dni_invalido` |

---

### Tags completos: `Pantalla_Usuarios`

**Estructura y estadísticas:**
`usuarios_titulo`, `usuarios_stat_total`, `usuarios_stat_activos`, `usuarios_stat_bloqueados`, `usuarios_stat_inactivos`, `usuarios_lista_titulo`, `usuarios_col_usuario`, `usuarios_col_estado`, `usuarios_col_bloqueado`, `usuarios_sin_resultados`, `usuarios_estado_activo`, `usuarios_estado_inactivo`, `usuarios_bloqueado_si`

**Botones de acción:**
`usuarios_btn_crear`, `usuarios_btn_modificar`, `usuarios_btn_desbloquear`, `usuarios_btn_blanquear`, `usuarios_btn_activar`, `usuarios_btn_desactivar`

**Formulario:**
`usuarios_form_titulo`, `usuarios_form_nuevo`, `usuarios_form_modificar`

**Sección de filtros:**
`usuarios_label_filtros`, `usuarios_label_estado`, `usuarios_label_bloqueados`, `usuarios_label_rol`, `usuarios_label_buscar`, `usuarios_filtro_todos`, `usuarios_filtro_activados`, `usuarios_filtro_desactivados`, `usuarios_filtro_bloqueados`, `usuarios_filtro_no_bloqueados`, `usuarios_filtro_todos_roles`, `usuarios_buscar_placeholder`, `usuarios_btn_filtrar`, `usuarios_btn_exportar`, `usuarios_btn_actualizar`

**Mensajes de acción:**
`usuarios_msg_sel_requerido`, `usuarios_msg_no_existe`, `usuarios_msg_desbloqueado`, `usuarios_msg_sel_desbloquear`, `usuarios_msg_blanqueado`, `usuarios_msg_sel_blanquear`, `usuarios_msg_sel_activar`, `usuarios_msg_sel_desactivar`, `usuarios_msg_activado`, `usuarios_msg_desactivado`, `usuarios_msg_creado`, `usuarios_msg_modificado`, `usuarios_msg_rol_invalido`

---

### Tags completos: `Pantalla_Alumnos`

**Estructura y estadísticas:**
`alumnos_titulo`, `alumnos_stat_total`, `alumnos_stat_activos`, `alumnos_stat_con_rutinas`, `alumnos_stat_sin_usuario`, `alumnos_lista_titulo`, `alumnos_col_alumno`, `alumnos_col_estado`, `alumnos_sin_resultados`, `alumnos_estado_activo`, `alumnos_estado_inactivo`

**Botones de acción:**
`alumnos_btn_crear`, `alumnos_btn_modificar`, `alumnos_btn_eliminar`, `alumnos_btn_asociar`

**Formulario:**
`alumnos_form_titulo`, `alumnos_form_nuevo`, `alumnos_form_modificar`, `alumnos_sin_asociar`

**Modal confirmación de eliminación:**
`alumnos_confirmar_elim_titulo`, `alumnos_confirmar_elim_msg`, `alumnos_confirmar_elim_aviso`

**Sección de filtros:**
`alumnos_label_filtros`, `alumnos_label_estado`, `alumnos_label_usuario`, `alumnos_label_buscar`, `alumnos_filtro_todos`, `alumnos_filtro_activos`, `alumnos_filtro_inactivos`, `alumnos_filtro_con_usuario`, `alumnos_filtro_sin_usuario`, `alumnos_buscar_placeholder`, `alumnos_btn_filtrar`

**Mensajes de validación:**
`alumnos_msg_dni_obligatorio`, `alumnos_msg_dni_invalido`, `alumnos_msg_nombre_oblig`, `alumnos_msg_apellido_oblig`, `alumnos_msg_fecha_oblig`, `alumnos_msg_fecha_invalida`, `alumnos_msg_fecha_futura`, `alumnos_msg_peso_invalido`

**Mensajes de acción:**
`alumnos_msg_actualizado`, `alumnos_msg_sel_requerido`, `alumnos_msg_sel_eliminar`, `alumnos_msg_sel_asociar`, `alumnos_msg_no_existe`, `alumnos_msg_creado`, `alumnos_msg_modificado`, `alumnos_msg_eliminado`, `alumnos_msg_ya_existe`, `alumnos_msg_ya_asociado`

---

## DAL: registrar una tabla nueva

Abrir `gymAppV2/DAL/DalTraduccion.cs` y agregar el nombre a `TABLAS`:

```csharp
private static readonly string[] TABLAS = {
    "Pantalla_Login",
    // ... existentes ...
    "Pantalla_NuevaPantalla",   // ← agregar aquí
    "Comunes_Botones",
    "Comunes_Mensajes",
};
```

El DAL hace `SELECT * FROM [Traducciones].[nombre] WHERE IdiomaID = @id` por cada tabla. Los nombres de columna se usan directamente como tags.

---

## Scripts SQL ejecutados

| Archivo | Descripción |
|---------|-------------|
| `scripts/migrar-esquema-traducciones.sql` | Crea el esquema y las tablas base (Login, Idioma, DashBoard, Comunes) |
| `scripts/crear-traducciones.sql` | Pobla las tablas de pantallas principales |
| `scripts/agregar-traducciones-filtros.sql` | ALTER TABLE en Pantalla_Usuarios y Pantalla_Alumnos: sección de filtros |
| `scripts/agregar-traducciones-mensajes.sql` | ALTER TABLE en Comunes_Mensajes, Pantalla_Alumnos y Pantalla_Usuarios: mensajes de acción y footer |

> Para agregar tags a una tabla existente usar ALTER TABLE con `DEFAULT N''` y luego un UPDATE, no DROP+CREATE (se perderían los datos).

```sql
-- Patrón para agregar tags a tabla existente
ALTER TABLE [Traducciones].[Pantalla_X]
    ADD [x_nuevo_tag] NVARCHAR(500) NOT NULL DEFAULT N'';
GO
UPDATE [Traducciones].[Pantalla_X] SET
    [x_nuevo_tag] = CASE [IdiomaID]
        WHEN 1 THEN N'Valor ES'
        WHEN 2 THEN N'Valor EN'
        WHEN 3 THEN N'Valor PT'
        WHEN 4 THEN N'Valor FR'
        WHEN 5 THEN N'Valor JA'
    END;
GO
```

---

## Patrón para una pantalla nueva

### 1. SQL
Crear tabla en el script y ejecutar en SSMS:

```sql
IF OBJECT_ID('Traducciones.Pantalla_Nueva', 'U') IS NOT NULL
    DROP TABLE [Traducciones].[Pantalla_Nueva];
GO
CREATE TABLE [Traducciones].[Pantalla_Nueva] (
    [TraduccionID]   INT IDENTITY(1,1) NOT NULL,
    [IdiomaID]       INT NOT NULL,
    [nueva_titulo]   NVARCHAR(500) NOT NULL,
    -- un campo por tag
    CONSTRAINT PK_Pantalla_Nueva        PRIMARY KEY ([TraduccionID]),
    CONSTRAINT FK_Pantalla_Nueva_Idioma FOREIGN KEY ([IdiomaID]) REFERENCES [Traducciones].[Idiomas]([IdiomaID]),
    CONSTRAINT UQ_Pantalla_Nueva_Idioma UNIQUE ([IdiomaID])
);
GO
INSERT INTO [Traducciones].[Pantalla_Nueva] ([IdiomaID], [nueva_titulo])
VALUES (1, N'Título ES'), (2, N'Title EN'), (3, N'Título PT'), (4, N'Titre FR'), (5, N'タイトル');
GO
```

### 2. DAL
Agregar `"Pantalla_Nueva"` al array `TABLAS` en `DalTraduccion.cs`.

### 3. ASPX — controles y expresiones inline

Para texto estático con `asp:Literal`:
```aspx
<h1><asp:Literal ID="litTitulo" runat="server" Text="Título" /></h1>
```

Para texto que aparece múltiples veces en templates o junto a HTML no-server (etiquetas, botones `<button>` sin `runat`):
```aspx
<label><%= T("nueva_label_campo") %></label>
<button id="btnFiltrar" onserverclick="btnFiltrar_Click"><%= T("nueva_btn_filtrar") %></button>
```

Para placeholder de TextBox server-side (no admite atributo ASPX directo):
```csharp
// En AplicarIdioma():
txtBusqueda.Attributes["placeholder"] = T("nueva_buscar_placeholder");
```

Para opciones de DropDownList server-side (el texto ASPX no es traducible, se sobreescribe en código):
```csharp
// En AplicarIdioma():
ddlEstado.Items[0].Text = T("nueva_filtro_todos");
ddlEstado.Items[1].Text = T("nueva_filtro_activos");
// ViewState guarda el VALUE seleccionado, no el TEXT → seguro cambiar el texto post-load.
```

Si el DropDownList se recrea en cada carga (`.Items.Clear()` + `.Add()`), usar T() directamente en el constructor:
```csharp
ddlFiltro.Items.Add(new ListItem(T("nueva_filtro_todos"), ""));
ddlFiltro.Items.Add(new ListItem(T("nueva_filtro_opcion"), "valor"));
```

Para headers de `GridView` (no admiten `asp:Literal` en `HeaderText`):
```csharp
((TemplateField)gvMiGrid.Columns[0]).HeaderText = T("nueva_col_nombre");
```

### 4. Code-behind (.aspx.cs)
La página debe heredar de `BasePage` e implementar `OnIdiomaChanged`:

```csharp
using BE;  // para IdiomaApp

protected void Page_Load(object sender, EventArgs e)
{
    VerificarAcceso(PermisosSistema.XxxPermiso);
    if (!IsPostBack) AplicarIdioma();
}

public override void OnIdiomaChanged(IdiomaApp idioma)
{
    base.OnIdiomaChanged(idioma);  // invalida el cache del diccionario
    AplicarIdioma();
}

private void AplicarIdioma()
{
    litTitulo.Text = T("nueva_titulo");
    // dropdowns, placeholders, headers, etc.
}
```

**Footer de tabla** — usar `msg_mostrando_fmt` de Comunes_Mensajes:
```csharp
footerText.InnerText = string.Format(T("msg_mostrando_fmt"), lista.Count, lista.Count);
```
El ASPX debe dejar el span vacío (no poner texto hardcodeado):
```aspx
<span class="table-footer-text" id="footerText" runat="server"></span>
```

**Mensajes de acción** — usar tags `_msg_xxx` de la tabla de la pantalla:
```csharp
// Selección vacía:
MostrarError(T("nueva_msg_sel_requerido"));
// Éxito:
MostrarExito(T("nueva_msg_creado"));
// Error técnico (excepción inesperada):
MostrarError(T("msg_error_generico"));
```

### 5. Designer (.aspx.designer.cs)
Declarar cada `asp:Literal` nuevo (Visual Studio no lo hace automáticamente si no regenera):

```csharp
protected global::System.Web.UI.WebControls.Literal litTitulo;
```

---

## Localización en JavaScript (DashBoard.Master)

Los scripts no pueden llamar a `T()` en runtime. Dos estrategias:

**1. Expresión inline en .master** — usar `TraducirTag()` (método en `DashBoardMaster.cs`):
```javascript
// En DashBoard.Master: usa <%=TraducirTag("tag")%>
var titles = {
    success: '<%=TraducirTag("toast_exito")%>',
    error:   '<%=TraducirTag("toast_error")%>',
    warning: '<%=TraducirTag("toast_advertencia")%>',
    info:    '<%=TraducirTag("toast_informacion")%>'
};
```

**2. Expresión inline en .aspx** — usar `T()` directamente:
```javascript
// En un bloque <script> dentro de la página ASPX:
const monthNames = <%=T("actividades_meses_json")%>;
const dayFmt = '<%=T("actividades_dia_titulo_fmt").Replace("'","&#39;")%>';
const title = dayFmt.replace('{0}', day).replace('{1}', monthNames[month]);
```

---

## Idioma page — patrón PRG (Post/Redirect/Get)

Los botones de selección de idioma en `Idioma.aspx` usan `asp:LinkButton` con divs `runat="server"` anidados adentro. Al hacer PostBack, ASP.NET Web Forms vuelve a renderizar el control servidor y los hijos HTML quedan vacíos. La solución es no renderizar la respuesta del PostBack:

```csharp
private void CambiarY(IdiomaApp idioma)
{
    GestorIdioma.CambiarIdioma(idioma);
    Session["IdiomaCambiadoMsg"] = T("idioma_guardado");
    RedirigirSeguro(Request.RawUrl);  // GET limpio → los controles se renderizan correctamente
}

protected void Page_Load(object sender, EventArgs e)
{
    if (!IsPostBack)
    {
        ActualizarVista();
        var msg = Session["IdiomaCambiadoMsg"] as string;
        if (msg != null)
        {
            MostrarExito(msg);
            Session.Remove("IdiomaCambiadoMsg");
        }
    }
}
```

---

## Convenciones de nombres de tags

| Prefijo          | Uso                               |
|------------------|-----------------------------------|
| `menu_`          | Sidebar / navegación              |
| `login_`         | Pantalla de login                 |
| `idioma_`        | Pantalla de selector de idioma    |
| `dash_`          | Dashboard (panel principal)       |
| `usuarios_`      | Gestión de usuarios               |
| `alumnos_`       | Gestión de alumnos                |
| `actividades_`   | Calendario de actividades         |
| `rutinas_`       | Rutinas                           |
| `btn_`           | Botones comunes (Comunes_Botones) |
| `toast_`         | Notificaciones toast              |
| `msg_`           | Mensajes de estado y formato      |

Sufijos dentro de una pantalla:
| Sufijo               | Uso |
|----------------------|-----|
| `_titulo`            | Título de la página |
| `_stat_xxx`          | Etiqueta de estadística |
| `_col_xxx`           | Header de columna en grilla |
| `_btn_xxx`           | Texto de botón específico de la pantalla |
| `_label_xxx`         | Etiqueta de sección o campo en filtros/formulario |
| `_filtro_xxx`        | Texto de opción en un DropDownList de filtro |
| `_buscar_placeholder`| Placeholder del campo de búsqueda |
| `_sin_resultados`    | Mensaje de grilla vacía |
| `_form_titulo`       | Título del formulario (modo lectura/detalle) |
| `_form_nuevo`        | Título del formulario al crear |
| `_form_modificar`    | Título del formulario al editar |
| `_estado_activo`     | Texto de estado "activo" en celdas |
| `_estado_inactivo`   | Texto de estado "inactivo" en celdas |
| `_msg_sel_xxx`       | "Seleccione un elemento para…" |
| `_msg_no_existe`     | "El elemento no existe" |
| `_msg_creado`        | "Elemento creado correctamente" |
| `_msg_modificado`    | "Elemento modificado correctamente" |
| `_msg_eliminado`     | "Elemento eliminado correctamente" |
| `_msg_xxx_oblig`     | "El campo X es obligatorio" |
| `_sin_asociar`       | Opción vacía/nula en dropdown de asociación |
