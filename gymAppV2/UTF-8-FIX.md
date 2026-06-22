# Solución UTF-8 en ASP.NET Web Forms

## Problema

Los caracteres especiales (tildes, ñ, acentos) no se mostraban correctamente en la aplicación, apareciendo como caracteres ilegibles o símbolos raros.

## Causas

El problema de UTF-8 en ASP.NET Web Forms puede tener múltiples orígenes:

1. **Falta de meta tag de codificación** en el HTML
2. **Configuración incorrecta en Web.config** para globalización
3. **Caché del navegador** que mantiene versiones viejas de archivos
4. **Codificación de archivos** guardados en formato incorrecto

## Solución Implementada

### 1. Meta Tag en HTML

Agregado en `DashBoard.Master`:

```html
<meta http-equiv="Content-Type" content="text/html; charset=utf-8" />
<meta charset="utf-8" />
```

El `http-equiv` fuerza al navegador a interpretar el contenido como UTF-8, mientras que el `charset` es la forma moderna de especificar la codificación.

### 2. Configuración en Web.config

Agregado en `<system.web>`:

```xml
<globalization
    requestEncoding="utf-8"
    responseEncoding="utf-8"
    culture="es-AR"
    uiCulture="es-AR"
    fileEncoding="utf-8" />
```

**Explicación de cada atributo:**

- `requestEncoding="utf-8"`: Codificación para datos recibidos del cliente
- `responseEncoding="utf-8"`: Codificación para datos enviados al cliente
- `culture="es-AR"`: Configuración regional para formato de fechas, números, etc.
- `uiCulture="es-AR"`: Configuración regional para recursos de UI
- `fileEncoding="utf-8"`: Codificación predeterminada para archivos .aspx y .ascx

### 3. Versión de CSS

Para forzar la recarga del CSS y evitar caché:

```html
<link href="<%= ResolveUrl("~/Content/dashboard.css?v=3") %>" rel="stylesheet" type="text/css" />
```

El parámetro `?v=3` hace que el navegador trate el archivo como nuevo, ignorando el caché.

## Pasos para Aplicar la Solución

1. **Editar Web.config**:
   - Abrir `Web.config`
   - Agregar el elemento `<globalization>` dentro de `<system.web>`
   - Guardar el archivo

2. **Editar Master Page**:
   - Abrir `DashBoard.Master`
   - Agregar el meta tag `<meta http-equiv="Content-Type" ...>` después de `<head>`
   - Guardar el archivo

3. **Reiniciar el Servidor**:
   - Detener IIS Express
   - Volver a iniciar el servidor
   - Esto es necesario para que el Web.config se recargue

4. **Recargar el Navegador**:
   - Presionar `Ctrl + Shift + R` (recarga forzada)
   - O borrar caché manualmente

## Verificación

Para verificar que UTF-8 funciona correctamente:

1. Abrir DevTools (`F12`)
2. Ir a la pestaña Network
3. Recargar la página
4. Verificar que los archivos tengan `Content-Type: text/html; charset=utf-8`

## Archivos Modificados

- `Web.config` - Configuración de globalización
- `DashBoard.Master` - Meta tag de codificación
- `DashBoard/WebForm1.aspx` - Versión de CSS actualizada

## Notas Adicionales

- Asegurarse de que todos los archivos `.aspx`, `.ascx`, `.cshtml` estén guardados en UTF-8
- En Visual Studio: `File > Advanced Save Options > Encoding: UTF-8 with BOM`
- El BOM (Byte Order Mark) ayuda a que el editor reconozca la codificación
- Para archivos CSS y JS, también es importante mantener UTF-8