# Especificación: Mejoras al Gestor de Usuarios

**Fecha:** 2026-05-24  
**Autor:** Claude  
**Estado:** Aprobado

## Resumen

Mejoras al módulo de Gestión de Usuarios del GymApp para:
1. Mejorar el contraste visual del botón Guardar
2. Aumentar los márgenes entre componentes para mejor legibilidad
3. Implementar generación automática de contraseña (apellido + DNI)
4. Implementar persistencia completa en base de datos (crear y modificar)

## 1. Cambios Visuales

### 1.1 Contraste del Botón Guardar

**Problema:** El botón Guardar usa `var(--success)` que no está definido, resultando en bajo contraste.

**Solución:** Implementar un color verde con alto contraste y gradiente.

**CSS a agregar:**
```css
.btn-guardar {
  background: linear-gradient(135deg, #22c55e 0%, #16a34a 100%);
  color: #ffffff;
  box-shadow: 0 4px 6px rgba(34, 197, 94, 0.3);
  transition: all 0.2s ease;
}

.btn-guardar:hover {
  background: linear-gradient(135deg, #16a34a 0%, #15803d 100%);
  transform: translateY(-1px);
  box-shadow: 0 6px 8px rgba(34, 197, 94, 0.4);
}

.btn-guardar:active {
  transform: translateY(0);
  box-shadow: 0 2px 4px rgba(34, 197, 94, 0.3);
}
```

**Archivos a modificar:**
- `gymAppV2/Usuarios/Usuarios.css`

### 1.2 Márgenes Aumentados

**Problema:** Los márgenes actuales de `1.25rem` son insuficientes para una buena separación visual.

**Solución:** Aumentar márgenes a `2rem` entre componentes principales.

**CSS a modificar:**
```css
.usuarios-container .stats-row {
  margin-bottom: 2rem; /* antes: 1.25rem */
}

.usuarios-container .filter-card {
  margin-bottom: 2rem; /* antes: 1.25rem */
}

.usuarios-container .table-card {
  margin-bottom: 2rem; /* nuevo */
}

.usuarios-container .form-row {
  gap: 1.25rem; /* nuevo */
  margin-bottom: 1rem; /* nuevo */
}
```

**Archivos a modificar:**
- `gymAppV2/Usuarios/Usuarios.css`

### 1.3 Campo de Contraseña

**Problema:** El usuario no debe ver ni ingresar la contraseña; se genera automáticamente.

**Solución:** Ocultar completamente el campo `passwordRow` en el ASPX.

**Cambios en ASPX:**
```html
<!-- Eliminar o comentar completamente este div -->
<!-- 
<div id="passwordRow" runat="server" style="...">
    <label style="...">Contraseña (opcional - se genera automáticamente)</label>
    <asp:TextBox ID="txtContrasena" runat="server" ...></asp:TextBox>
    ...
</div>
-->
```

**Archivos a modificar:**
- `gymAppV2/Usuarios/UsuariosModulo.aspx`

## 2. Lógica del Backend

### 2.1 Generación de Contraseña Automática

**Formato:** `{Apellido}{DNI}` (sin espacios ni caracteres especiales)

**Método a implementar:**
```csharp
private string GenerarContrasenaAutomatica(string apellido, string dni)
{
    // Limpiar espacios y formatear
    string apellidoLimpio = apellido?.Trim() ?? "";
    string dniLimpio = dni?.Trim() ?? "";
    
    // Ejemplo: "Pérez" + "12345678" = "Pérez12345678"
    return apellidoLimpio + dniLimpio;
}
```

**Archivos a modificar:**
- `gymAppV2/Usuarios/UsuariosModulo.aspx.cs`

### 2.2 Flujo de btnGuardar_Click

```
INICIO
  ↓
Validar campos obligatorios:
  - Usuario
  - DNI
  - Nombre
  - Apellido
  - Rol seleccionado
  ↓
¿Es modificación?
  (SelectedUsuario tiene valor y lblFormTitle == "Modificar usuario")
  ↓
  ┌──────────────────────────────────┐
  │                                   │
  │  NO (CREAR NUEVO)                │
  │   ↓                              │
  │   Generar contraseña:            │
  │     apellido + dni               │
  │   ↓                              │
  │   Insertar USUARIOS              │
  │     - usr, contra, activo=1, rol│
  │   ↓                              │
  │   Insertar USUARIO_Contras       │
  │     - usr, contra, dvv, dvh      │
  │   ↓                              │
  │   Insertar según rol:            │
  │     - Rol 3 (Entrenador):        │
  │       Insertar Entrenadores      │
  │     - Rol 4 (Cliente):           │
  │       Insertar Alumnos           │
  │   ↓                              │
  │   Mostrar: "Usuario creado..."  │
  │                                   │
  └──────────────────────────────────┘
  ↓
  ┌──────────────────────────────────┐
  │                                   │
  │  SÍ (MODIFICAR EXISTENTE)        │
  │   ↓                              │
  │   NO generar contraseña          │
  │   ↓                              │
  │   Actualizar según rol:          │
  │     - Rol 3 (Entrenador):        │
  │       UPDATE Entrenadores         │
  │         SET nombre, apellido,    │
  │             telefono, fechaNac    │
  │         WHERE usr = ?            │
  │     - Rol 4 (Cliente):           │
  │       UPDATE Alumnos              │
  │         SET nombre, apellido,    │
  │             telefono, peso       │
  │         WHERE usr = ?            │
  │   ↓                              │
  │   Mostrar: "Usuario modificado.."│
  │                                   │
  └──────────────────────────────────┘
  ↓
Cerrar formulario
  ↓
Recargar lista de usuarios
  ↓
Actualizar estadísticas
  ↓
FIN
```

**Archivos a modificar:**
- `gymAppV2/Usuarios/UsuariosModulo.aspx.cs`

## 3. Operaciones de Base de Datos

### 3.1 Tabla USUARIOS - Insertar (Crear)

**SQL:**
```sql
INSERT INTO USUARIOS (usr, contra, activo, rol, dvv, dvh)
VALUES (@usuario, @contra, 1, @rol, 'admin', 'admin')
```

**Parámetros:**
- `@usuario`: Nombre de usuario del formulario
- `@contra`: Contraseña generada (apellido + DNI)
- `@rol`: ID del rol seleccionado (1=Admin, 2=Recepcion, 3=Entrenador, 4=Cliente)

### 3.2 Tabla USUARIO_Contras - Insertar (Crear)

**SQL:**
```sql
INSERT INTO USUARIO_Contras (usr, contra, dvv, dvh)
VALUES (@usuario, @contra, 'admin', 'admin')
```

**Notas:**
- Solo se inserta al crear un usuario nuevo
- No se inserta al modificar

### 3.3 Tabla Entrenadores - Insertar (Crear - Rol 3)

**SQL:**
```sql
INSERT INTO Entrenadores 
(dni, nombre, apellido, telefono, fechaNacimiento, activo, usr, dvv, dvh)
VALUES 
(@dni, @nombre, @apellido, @telefono, @fechaNacimiento, 1, @usuario, 'admin', 'admin')
```

**Parámetros:**
- `@dni`: DNI del entrenador (formulario)
- `@nombre`: Nombre (formulario)
- `@apellido`: Apellido (formulario)
- `@telefono`: Teléfono (formulario)
- `@fechaNacimiento`: Fecha de nacimiento (formulario)
- `@usuario`: Nombre de usuario (vinculado a USUARIOS)

### 3.4 Tabla Alumnos - Insertar (Crear - Rol 4)

**SQL:**
```sql
INSERT INTO Alumnos 
(dni, nombre, apellido, telefono, fechaNacimiento, peso, activo, usr, dvv, dvh)
VALUES 
(@dni, @nombre, @apellido, @telefono, @fechaNacimiento, NULL, 1, @usuario, 'admin', 'admin')
```

**Notas:**
- `peso` se inserta como NULL por defecto
- `fechaNacimiento` se puede obtener del formulario o del alumno asociado

### 3.5 Tabla Entrenadores - Actualizar (Modificar - Rol 3)

**SQL:**
```sql
UPDATE Entrenadores 
SET nombre = @nombre,
    apellido = @apellido,
    telefono = @telefono,
    fechaNacimiento = @fechaNacimiento
WHERE usr = @usuario
```

**Notas:**
- NO se actualiza `dni` (clave primaria)
- NO se actualiza `usr` (clave foránea)
- Solo se actualizan campos editables

### 3.6 Tabla Alumnos - Actualizar (Modificar - Rol 4)

**SQL:**
```sql
UPDATE Alumnos 
SET nombre = @nombre,
    apellido = @apellido,
    telefono = @telefono,
    peso = @peso
WHERE usr = @usuario
```

**Notas:**
- NO se actualiza `dni` (clave primaria)
- NO se actualiza `usr` (clave foránea)

## 4. Validaciones

### 4.1 Campos Obligatorios

Al crear o modificar un usuario, validar:
- `txtUsuario`: No vacío
- `txtDNI`: No vacío y formato numérico válido
- `txtNombre`: No vacío
- `txtApellido`: No vacío
- `ddlRolForm`: Rol seleccionado (no índice 0)

### 4.2 Validaciones Específicas por Rol

**Rol 3 (Entrenador):**
- `txtDNIEntrenador`: No vacío y formato numérico válido
- `txtFechaNacimientoEntrenador`: Fecha válida no futura

**Rol 4 (Cliente):**
- `txtDNIAlumno`: No vacío, formato numérico válido, y alumno existe en tabla Alumnos

### 4.3 Validaciones de Duplicidad

**Al crear:** Verificar que `txtUsuario` no exista en USUARIOS

**Al modificar:** Verificar que el usuario seleccionado existe y está activo

## 5. Manejo de Errores

### 5.1 Mensajes de Error

| Situación | Mensaje | Acción |
|-----------|---------|--------|
| Usuario duplicado | "El nombre de usuario ya existe" | Limpiar campo usuario |
| Campos faltantes | "Por favor complete todos los campos obligatorios" | Resaltar campos faltantes |
| Error base de datos | "Error al guardar el usuario. Intente nuevamente." | Log del error técnico |
| DNI inválido | "El DNI debe ser numérico" | Resaltar campo DNI |
| Rol no seleccionado | "Debe seleccionar un rol para el usuario" | Resaltar dropdown rol |

### 5.2 Mensajes de Éxito

| Operación | Mensaje |
|-----------|---------|
| Crear usuario | "✅ Usuario creado correctamente" |
| Modificar usuario | "✅ Usuario modificado correctamente" |

## 6. Archivos a Modificar

### Frontend
1. `gymAppV2/Usuarios/UsuariosModulo.aspx` - Ocultar campo de contraseña
2. `gymAppV2/Usuarios/Usuarios.css` - Aumentar márgenes y agregar estilos del botón guardar

### Backend
3. `gymAppV2/Usuarios/UsuariosModulo.aspx.cs` - Implementar lógica de guardado

### Capa de Datos (si aún no existe)
4. `MPP/MPPUsuario.cs` - Implementar métodos de CRUD
5. `BLL/BLLUsuario.cs` - Implementar lógica de negocio

## 7. Implementación en Capas

### Capa MPP (Datos)
```csharp
public class MPPUsuario
{
    public bool CrearUsuario(UsuarioCrearDTO dto)
    {
        // 1. Insertar USUARIOS
        // 2. Insertar USUARIO_Contras
        // 3. Insertar tabla específica (Alumnos/Entrenadores)
        // 4. Retornar éxito/fallo
    }

    public bool ModificarUsuario(UsuarioCrearDTO dto, string usuarioExistente)
    {
        // 1. Actualizar tabla específica (Alumnos/Entrenadores)
        // 2. Retornar éxito/fallo
    }
}
```

### Capa BLL (Negocio)
```csharp
public class BLLUsuario
{
    public bool GuardarUsuario(UsuarioCrearDTO dto, bool esModificacion)
    {
        if (esModificacion)
        {
            return mpp.ModificarUsuario(dto, dto.Usuario);
        }
        else
        {
            // Generar contraseña automática
            dto.Contrasena = GenerarContrasena(dto.Apellido, dto.DNI);
            return mpp.CrearUsuario(dto);
        }
    }

    private string GenerarContrasena(string apellido, string dni)
    {
        return (apellido?.Trim() ?? "") + (dni?.Trim() ?? "");
    }
}
```

### Capa BE (Entidades)
```csharp
public class UsuarioCrearDTO
{
    public string Usuario { get; set; }
    public string Contrasena { get; set; } // Se genera automáticamente
    public string DNI { get; set; }
    public string Nombre { get; set; }
    public string Apellido { get; set; }
    public string Telefono { get; set; }
    public string Email { get; set; }
    public int Rol { get; set; }
    
    // Campos específicos
    public string? DNIEntrenador { get; set; }
    public DateTime? FechaNacimientoEntrenador { get; set; }
    public string? DNIAlumno { get; set; }
}
```

## 8. Orden de Implementación

1. **CSS**: Aumentar márgenes y agregar estilos del botón guardar
2. **ASPX**: Ocultar campo de contraseña
3. **Backend (ASPX.CS)**: Implementar `GenerarContrasenaAutomatica` y lógica de `btnGuardar_Click`
4. **Capa MPP**: Implementar métodos de `CrearUsuario` y `ModificarUsuario`
5. **Capa BLL**: Implementar `GuardarUsuario` con lógica de negocio
6. **Capa BE**: Crear `UsuarioCrearDTO` si no existe

## 9. Pruebas

### 9.1 Pruebas Visuales
- [ ] Verificar que el botón Guardar tenga alto contraste
- [ ] Verificar que los márgenes sean notables entre componentes
- [ ] Verificar que el campo de contraseña no sea visible

### 9.2 Pruebas Funcionales - Crear Usuario
- [ ] Crear usuario Administrador
- [ ] Crear usuario Entrenador con campos específicos
- [ ] Crear usuario Cliente asociando alumno existente
- [ ] Verificar que la contraseña se genere como Apellido + DNI
- [ ] Verificar que se guarde en USUARIO_Contras

### 9.3 Pruebas Funcionales - Modificar Usuario
- [ ] Modificar usuario Entrenador existente
- [ ] Modificar usuario Cliente existente
- [ ] Verificar que NO se genere nueva contraseña
- [ ] Verificar que NO se inserte en USUARIO_Contras

### 9.4 Pruebas de Validación
- [ ] Intentar crear usuario duplicado (debe fallar)
- [ ] Intentar guardar sin campos obligatorios (debe fallar)
- [ ] Intentar modificar usuario inexistente (debe fallar)