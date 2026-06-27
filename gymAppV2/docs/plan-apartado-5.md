# Plan — Apartado 5: Autorización y control de acceso por rol

## Contexto
Las tareas pendientes del apartado 5 en `docs/TAREAS_SEGURIDAD.md` son:

- **5.2.6** Agregar verificación de permisos en `Page_Load` de rutinas.
- **5.4.1** En módulo "Perfil", asegurar que un Cliente solo vea/modifique sus propios datos.
- **5.4.3** En rutinas para Cliente, mostrar solo las rutinas de sus alumnos asociados.
- **5.4.4** En actividades para Cliente, mostrar solo las clases de sus alumnos inscriptos.

## Problema detectado
Las páginas `Rutinas.aspx` y `Perfil.aspx` **no existen** en el proyecto; solo están referenciadas en `DashBoard.Master` como `not-implemented` y apuntan a directorios vacíos. Por eso no es posible "agregar verificación de permisos en Page_Load de rutinas" sin crear antes la página.

## Decisión del usuario
Se optó por **crear las páginas mínimas** necesarias para poder cerrar las tareas de seguridad del apartado 5.

## Enfoque
Replicar los patrones existentes:
- Heredar de `BasePage`.
- Llamar `VerificarAcceso("<permiso>")` en `Page_Load`.
- Usar `Singleton.Instancia.Usuario` para obtener el usuario actual y su rol.
- En módulos de Cliente, filtrar datos por los alumnos cuyo `Usuario` coincide con el usuario logueado (igual que en `Alumnos.aspx.cs`).

## Alcance
Se hará lo que sea posible con la capa de datos actual. Las capas de datos de **Rutinas** y **Actividad_Alumno** no existen aún, así que:
- **Rutinas**: se creará la página con VerificarAcceso y se preparará el filtro por alumno asociado. La lista de rutinas quedará como placeholder hasta que se implemente `BLLRutina`.
- **Actividades**: se restringirá la acción de "Nueva Actividad" para clientes y se expondrá el rol al front-end. Se crea la capa de datos de actividades e inscripciones (`BE.Actividad`, `MPPActividad`, `BLLActividad`) y el calendario consume los datos filtrados desde el servidor.
- **Perfil**: se creará la página completa (lectura y edición) usando `BLLUsuario.ModificarUsuario`, asegurando que el usuario solo modifique sus propios datos.

## Archivos a modificar/crear

### Nuevos archivos
1. `gymAppV2/Rutinas/Rutinas.aspx` — página con master `DashBoard.Master`.
2. `gymAppV2/Rutinas/Rutinas.aspx.cs` — hereda de `BasePage`, `VerificarAcceso("GestionRutinas")`.
3. `gymAppV2/Rutinas/Rutinas.aspx.designer.cs`.
4. `gymAppV2/Perfil/Perfil.aspx` — formulario de perfil (lectura/ edición de datos personales).
5. `gymAppV2/Perfil/Perfil.aspx.cs` — hereda de `BasePage`, `VerificarAcceso("Perfil")`, carga/guarda datos del usuario en sesión.
6. `gymAppV2/Perfil/Perfil.aspx.designer.cs`.

### Archivos existentes a editar
7. `gymAppV2/gymAppV2.csproj` — registrar los nuevos archivos (Content + Compile).
8. `gymAppV2/DashBoard.Master` — quitar `class="not-implemented"` de los ítems Rutinas y Perfil; corregir href si es necesario.
9. `gymAppV2/Actividades/actividades.aspx.cs` — ocultar el botón "Nueva Actividad" para clientes (`EsCliente`) y exponer esa propiedad al front-end.

## Criterios de aceptación
- `Rutinas.aspx` redirige a `AccesoDenegado.aspx` si el usuario no tiene permiso `GestionRutinas`.
- `Perfil.aspx` redirige a `AccesoDenegado.aspx` si el usuario no tiene permiso `Perfil` (rol Cliente).
- En `Perfil.aspx`, el usuario logueado solo puede modificar su propio nombre, apellido, teléfono y email.
- En `Rutinas.aspx`, si el usuario es Cliente, el código queda preparado para filtrar rutinas por alumnos asociados a su usuario.
- En `Actividades`, el botón "Nueva Actividad" no se muestra para usuarios Cliente.
- El build de la solución compila sin errores.

## Riesgos/limitaciones
- No se implementa la capa de datos de Rutinas ni de inscripciones a actividades (está fuera del apartado 5).
- Las listas filtradas para Cliente en Rutinas/Actividades quedarán vacías o como demo hasta que existan datos reales y BLL/MPP correspondientes.
