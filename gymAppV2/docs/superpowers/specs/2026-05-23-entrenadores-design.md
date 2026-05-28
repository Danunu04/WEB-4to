# Entrenadores Module Design

**Date:** 2026-05-23  
**Status:** Approved

## Overview
Create an Entrenadores (Trainers) management page with the same design as the Alumnos page, including inline table editing.

## Entities

### Entrenador BE Class
**Location:** `BE\Entrenador.cs`

```csharp
public class Entrenador
{
    public int DNI { get; set; }
    public string Nombre { get; set; }
    public string Apellido { get; set; }
    public DateTime FechaNacimiento { get; set; }
    public string Usuario { get; set; }
    public string DVV { get; set; }
    public string DVH { get; set; }
}
```

## Page Structure

### Entrenadores.aspx
**Location:** `gymAppV2\Entrenadores\Entrenadores.aspx`

Components:
- Page header with title and badge count
- Stats row (4 cards):
  - Total entrenadores
  - Activos (inferred from USUARIO_Activo)
  - Con alumnos (count via Rutinas)
  - Sin usuario (null or empty Usuario field)
- Filter card:
  - Estado dropdown (Todos, Activos, Inactivos)
  - Usuario asociado dropdown
  - Search input (Nombre, apellido, DNI)
- GridView table:
  - Columns: Entrenador (name+avatar), Fecha Nacimiento, Usuario, Estado, Acciones
  - Inline editing for all fields except DNI
  - Action buttons: Modify, Delete
- Table footer with pagination

### Entrenadores.css
**Location:** `gymAppV2\Entrenadores\Entrenadores.css`

Same design system as Alumnos.css:
- Color variables (peach theme for trainers)
- Responsive card view on mobile
- Hover states and transitions
- Avatar classes with different colors

## Inline Editing (Option A)

When user clicks a cell:
1. Cell content becomes an `<input>` element
2. Edit mode indicator appears on the row
3. Save/Cancel buttons appear in the Actions column
4. On Save → update via BLL → refresh row
5. On Cancel → revert to original value

## Navigation Update

**File:** `gymAppV2\DashBoard.Master`

Change the Entrenadores nav link from `~/Entrenadores` to `~/Entrenadores/Entrenadores.aspx` and add it to the allowed pages list in the JavaScript.

## Layers

### BLL
**Location:** `BLL\BLLEntrenador.cs`

Methods:
- `ListarEntrenadores()` - Get all trainers
- `ObtenerEntrenador(int dni)` - Get single trainer
- `ActualizarEntrenador(Entrenador e)` - Update trainer
- `EliminarEntrenador(int dni)` - Delete trainer
- `ObtenerEstadisticas()` - Get stats counts

### MPP
**Location:** `MPP\MPPEntrenador.cs`

SQL operations for Entrenadores table.

## Responsive Design

- Desktop: Full table view
- Mobile (<768px): Card view with labeled fields
- Same breakpoints as Alumnos.css