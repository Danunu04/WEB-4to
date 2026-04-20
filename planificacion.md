# Planificación del Proyecto Sportio

**Fecha de creación:** 2026-04-20
**Equipo:** Dana Perelmuter, Ivan Urso
**Tecnología:** ASP.NET Web Forms (.NET Framework 4.7.2), SQL Server

---

## Resumen del Proyecto

Sportio es una aplicación web integral para la gestión de un gimnasio que permite:
- Gestión de asociación y abono mensual
- Sistema de seguridad con login, cambio de contraseña y encriptación
- Gestión de usuarios (alumnos, administrativos)
- Sistema de permisos con patrones Singleton y Composite

---

## Fases del Proyecto

### Fase 1: Configuración de Base de Datos e Infraestructura
**Duración estimada:** 3 días

#### Tareas de Ivan
1. **Crear esquema de base de datos**
   - Descripción: Crear tablas para usuarios, alumnos, pagos, historial de contraseñas
   - Pasos:
     1. Crear tabla USUARIOS con campos: ID, Usuario, Contrasena, Intentos, Estado
     2. Crear tabla ALUMNOS con campos: ID, UsuarioID, Nombre, Apellido, DNI, Telefono, FechaNacimiento, Estado, FechaInicio, FechaVencimiento
     3. Crear tabla PAGOS con campos: ID, AlumnoID, Monto, MedioPago, MesAbonado, FechaPago
     4. Crear tabla HISTORIAL_CONTRASENAS con campos: ID, UsuarioID, Contrasena, FechaCambio
     5. Crear tabla PREGUNTAS_SEGURIDAD con campos: ID, UsuarioID, Pregunta, Respuesta
     6. Agregar columnas dvv y dvh para integridad de datos
   - Criterios de aceptación: Script SQL ejecutable sin errores, todas las foreign keys definidas
   - Complejidad: Media

2. **Implementar encriptación reversible para datos de alumnos**
   - Descripción: Crear función de encriptación/desencriptación para datos personales
   - Pasos:
     1. Crear clase EncriptacionService con método Encriptar(string texto)
     2. Crear metodo Desencriptar(string textoEncriptado)
     3. Usar algoritmo AES-256 con clave almacenada en Web.config
     4. Probar encriptación de Nombre, Apellido, DNI, Telefono
   - Criterios de aceptación: Funciones que encriptan y desencriptan correctamente, pruebas unitarias pasando
   - Complejidad: Media

3. **Implementar encriptación irreversible para contraseñas**
   - Descripción: Crear función de hash SHA256 para contraseñas
   - Pasos:
     1. Crear clase HashService con metodo Hash(string contrasena)
     2. Implementar SHA256 con salt aleatorio
     3. Crear metodo Verificar(string contrasena, string hashAlmacenado)
     4. Probar con contraseñas de prueba
   - Criterios de aceptación: Hash generado correctamente, verificación funciona, no reversible
   - Complejidad: Baja

#### Tareas de Dana
1. **Configurar estructura del proyecto ASP.NET Web Forms**
   - Descripción: Organizar carpetas y archivos del proyecto
   - Requisitos:
     - Crear carpetas: Pages, Services, Models, Utils, Styles, Scripts
     - Configurar Web.config con connection strings y appSettings
     - Configurar Master Page principal
     - Establecer estructura de navegación
   - Entregables: Estructura de carpetas creada, Web.config configurado, Master Page base
   - Complejidad: Media

2. **Diseñar interfaz de login**
   - Descripción: Crear página de inicio de sesión con diseño responsive
   - Requisitos:
     - Usar unidades rem para todos los estilos (nunca px)
     - Campos: Usuario, Contraseña
     - Mensaje de error para intentos fallidos
     - Indicador de intentos restantes
     - Diseño limpio y profesional
   - Entregables: Login.aspx, Login.aspx.cs, login.css
   - Complejidad: Media

3. **Crear servicio de conexión a base de datos**
   - Descripción: Implementar capa de acceso a datos
   - Requisitos:
     - Crear clase DatabaseService con metodos CRUD
     - Implementar manejo de excepciones
     - Crear metodos para cada tabla principal
     - Incluir logging de operaciones
   - Entregables: DatabaseService.cs con todos los metodos implementados
   - Complejidad: Alta

#### Dependencias
- Tarea 1 de Ivan debe completarse antes de Tarea 3 de Dana
- Tarea 1 de Dana debe completarse antes de Tarea 2 de Dana

---

### Fase 2: Sistema de Seguridad y Autenticación
**Duración estimada:** 5 días

#### Tareas de Ivan
1. **Implementar lógica de login con intentos**
   - Descripción: Crear funcionalidad de autenticación con control de intentos
   - Pasos:
     1. Crear metodo ValidarCredenciales(string usuario, string contrasena)
     2. Incrementar contador de intentos en cada fallo
     3. Bloquear cuenta despues de 3 intentos
     4. Registrar intentos en base de datos
     5. Crear metodo DesbloquearCuenta(int usuarioID)
   - Criterios de aceptación: Login funciona, contador de intentos funciona, bloqueo despues de 3 fallos
   - Complejidad: Media

2. **Implementar cambio de contraseña**
   - Descripción: Crear funcionalidad para cambiar contraseña con validación de historial
   - Pasos:
     1. Crear metodo CambiarContrasena(int usuarioID, string nuevaContrasena)
     2. Verificar que la contraseña no este en historial
     3. Validar requisitos: minimo 6 caracteres, 1 mayuscula, 1 caracter especial
     4. Guardar en historial de contraseñas
     5. Actualizar tabla USUARIOS
   - Criterios de aceptación: Cambio de contraseña funciona, validacion de requisitos, rechazo de contraseñas repetidas
   - Complejidad: Media

3. **Implementar registro de eventos (logging)**
   - Descripción: Crear sistema de registro de eventos con niveles de severidad
   - Pasos:
     1. Crear tabla EVENTO con campos: ID, UsuarioID, Descripcion, Severidad, Fecha
     2. Crear clase EventoService con metodo RegistrarEvento(int usuarioID, string descripcion, int severidad)
     3. Definir niveles: 1=Info, 2=Advertencia, 3=Error, 4=Critico
     4. Registrar eventos de login, cambio de contraseña, pagos
   - Criterios de aceptación: Eventos se registran correctamente, todos los niveles funcionan
   - Complejidad: Baja

#### Tareas de Dana
1. **Implementar sistema de preguntas de seguridad**
   - Descripción: Crear funcionalidad de desbloqueo con preguntas de seguridad
   - Requisitos:
     - Diseñar flujo de preguntas despues de bloqueo
     - Pregunta 1: Año de nacimiento
     - Pregunta 2: Validar conocimiento de alumno asociado (si aplica)
     - Redirigir a cambio de contraseña despues de validar
     - Manejar respuestas incorrectas
   - Entregables: PreguntasSeguridad.aspx, PreguntasSeguridad.aspx.cs, servicio de validación
   - Complejidad: Alta

2. **Diseñar página de cambio de contraseña**
   - Descripción: Crear interfaz para cambiar contraseña
   - Requisitos:
     - Campos: Contraseña actual, Nueva contraseña, Confirmar contraseña
     - Validaciones en tiempo real
     - Mensajes de error claros
     - Indicadores de requisitos de contraseña
     - Diseño responsive con rem
   - Entregables: CambiarContrasena.aspx, CambiarContrasena.aspx.cs, estilos CSS
   - Complejidad: Media

3. **Implementar validación de requisitos de contraseña**
   - Descripción: Crear servicio de validación de contraseñas
   - Requisitos:
     - Validar longitud minima de 6 caracteres
     - Validar al menos 1 letra mayuscula
     - Validar al menos 1 caracter especial
     - Validar que no este en historial
     - Proporcionar mensajes de error especificos
   - Entregables: ValidacionContrasenaService.cs con todas las validaciones
   - Complejidad: Media

#### Dependencias
- Tarea 1 de Ivan depende de Fase 1 completada
- Tarea 1 de Dana depende de Tarea 1 de Ivan
- Tarea 2 de Dana depende de Tarea 2 de Ivan
- Tarea 3 de Dana depende de Tarea 2 de Ivan

---

### Fase 3: Gestión de Usuarios
**Duración estimada:** 4 días

#### Tareas de Ivan
1. **Implementar CRUD de usuarios**
   - Descripción: Crear operaciones basicas para gestionar usuarios
   - Pasos:
     1. Crear metodo CrearUsuario(Usuario usuario)
     2. Crear metodo ObtenerUsuario(int usuarioID)
     3. Crear metodo ActualizarUsuario(Usuario usuario)
     4. Crear metodo EliminarUsuario(int usuarioID)
     5. Crear metodo ListarUsuarios()
     6. Validar que el nombre de usuario sea unico
   - Criterios de aceptación: Todas las operaciones CRUD funcionan, validacion de unicidad
   - Complejidad: Media

2. **Implementar CRUD de alumnos**
   - Descripción: Crear operaciones para gestionar alumnos
   - Pasos:
     1. Crear metodo CrearAlumno(Alumno alumno)
     2. Crear metodo ObtenerAlumno(int alumnoID)
     3. Crear metodo ActualizarAlumno(Alumno alumno)
     4. Crear metodo ListarAlumnosPorUsuario(int usuarioID)
     5. Encriptar datos personales al guardar
     6. Desencriptar datos al recuperar
   - Criterios de aceptación: CRUD funciona, encriptación/desencriptación correcta
   - Complejidad: Media

3. **Implementar validación de unicidad de usuario**
   - Descripción: Crear validación para evitar nombres de usuario duplicados
   - Pasos:
     1. Crear metodo UsuarioExiste(string nombreUsuario)
     2. Integrar validación en CrearUsuario
     3. Proporcionar mensaje de error claro
     4. Probar con usuarios duplicados
   - Criterios de aceptación: No permite usuarios duplicados, mensaje de error claro
   - Complejidad: Baja

#### Tareas de Dana
1. **Diseñar página de registro de usuario**
   - Descripción: Crear interfaz para crear nueva cuenta
   - Requisitos:
     - Campos: Nombre de usuario, Contraseña, Confirmar contraseña
     - Validaciones en tiempo real
     - Indicadores de requisitos de contraseña
     - Mensaje de error para usuario duplicado
     - Diseño responsive con rem
   - Entregables: Registro.aspx, Registro.aspx.cs, estilos CSS
   - Complejidad: Media

2. **Diseñar página de registro de alumno**
   - Descripción: Crear formulario para registrar datos del alumno
   - Requisitos:
     - Campos: Nombre, Apellido, DNI, Teléfono, Fecha de nacimiento
     - Validaciones de formato
     - Mensaje de confirmación con monto del primer mes
     - Redirección a pago despues de confirmar
     - Diseño limpio y profesional
   - Entregables: RegistroAlumno.aspx, RegistroAlumno.aspx.cs, estilos CSS
   - Complejidad: Media

3. **Crear página de perfil de usuario**
   - Descripción: Diseñar interfaz para ver y editar datos del usuario
   - Requisitos:
     - Mostrar información del usuario
     - Permitir editar datos basicos
     - Mostrar alumnos asociados
     - Opción para cambiar contraseña
     - Diseño responsive con rem
   - Entregables: Perfil.aspx, Perfil.aspx.cs, estilos CSS
   - Complejidad: Media

#### Dependencias
- Tarea 1 de Ivan depende de Fase 2 completada
- Tarea 2 de Ivan depende de Tarea 1 de Ivan
- Tarea 1 de Dana depende de Tarea 1 de Ivan
- Tarea 2 de Dana depende de Tarea 2 de Ivan
- Tarea 3 de Dana depende de Tarea 1 y 2 de Ivan

---

### Fase 4: Sistema de Suscripción y Pagos
**Duración estimada:** 5 días

#### Tareas de Ivan
1. **Implementar lógica de abono mensual**
   - Descripción: Crear funcionalidad para gestionar pagos mensuales
   - Pasos:
     1. Crear metodo CalcularMontoAbono(int alumnoID)
     2. Crear metodo RegistrarPago(Pago pago)
     3. Crear metodo ActualizarFechaVencimiento(int alumnoID, DateTime nuevaFecha)
     4. Crear metodo ObtenerEstadoAbono(int alumnoID)
     5. Calcular fecha de vencimiento (1 mes despues del pago)
   - Criterios de aceptación: Calculo de monto correcto, registro de pago funciona, actualización de vencimiento
   - Complejidad: Media

2. **Implementar validación de estado de abono**
   - Descripción: Crear lógica para verificar si el abono esta vigente
   - Pasos:
     1. Crear metodo AbonoVigente(int alumnoID)
     2. Crear metodo DiasRestantes(int alumnoID)
     3. Crear metodo MontoAdeudado(int alumnoID)
     4. Calcular recargos por mora si aplica
   - Criterios de aceptación: Validación de vigencia funciona, calculo de dias restantes correcto
   - Complejidad: Media

3. **Implementar historial de pagos**
   - Descripción: Crear funcionalidad para ver historial de pagos de un alumno
   - Pasos:
     1. Crear metodo ObtenerHistorialPagos(int alumnoID)
     2. Crear metodo ObtenerUltimoPago(int alumnoID)
     3. Ordenar pagos por fecha descendente
     4. Incluir detalles de cada pago
   - Criterios de aceptación: Historial se muestra correctamente, ordenamiento correcto
   - Complejidad: Baja

#### Tareas de Dana
1. **Diseñar página de abono mensual**
   - Descripción: Crear interfaz para visualizar y pagar abono mensual
   - Requisitos:
     - Mostrar fecha de vencimiento actual
     - Mostrar estado del abono (vigente/vencido)
     - Mostrar monto a pagar si esta vencido
     - Opción para realizar pago
     - Diseño responsive con rem
   - Entregables: AbonoMensual.aspx, AbonoMensual.aspx.cs, estilos CSS
   - Complejidad: Media

2. **Diseñar página de procesamiento de pago**
   - Descripción: Crear interfaz para realizar pago
   - Requisitos:
     - Campos: Monto, Medio de pago (efectivo, tarjeta, transferencia)
     - Confirmación de pago
     - Generación de recibo
     - Mensaje de éxito con nueva fecha de vencimiento
     - Diseño profesional
   - Entregables: Pago.aspx, Pago.aspx.cs, estilos CSS
   - Complejidad: Media

3. **Crear página de historial de pagos**
   - Descripción: Diseñar interfaz para ver historial de pagos
   - Requisitos:
     - Tabla con historial de pagos
     - Columnas: Fecha, Monto, Medio de pago, Mes abonado
     - Paginación si hay muchos registros
     - Opción para exportar a PDF
     - Diseño responsive con rem
   - Entregables: HistorialPagos.aspx, HistorialPagos.aspx.cs, estilos CSS
   - Complejidad: Media

4. **Implementar vista administrativa de abonos**
   - Descripción: Crear interfaz para administrativos para gestionar abonos
   - Requisitos:
     - Buscador de alumnos por nombre, apellido, DNI
     - Mostrar estado de abono del alumno seleccionado
     - Opción para registrar pago manual
     - Lista de alumnos con abonos vencidos
     - Diseño optimizado para administrativos
   - Entregables: AbonosAdmin.aspx, AbonosAdmin.aspx.cs, estilos CSS
   - Complejidad: Alta

#### Dependencias
- Tarea 1 de Ivan depende de Fase 3 completada
- Tarea 2 de Ivan depende de Tarea 1 de Ivan
- Tarea 3 de Ivan depende de Tarea 1 de Ivan
- Tarea 1 de Dana depende de Tarea 1 y 2 de Ivan
- Tarea 2 de Dana depende de Tarea 1 de Ivan
- Tarea 3 de Dana depende de Tarea 3 de Ivan
- Tarea 4 de Dana depende de Tarea 1 y 2 de Ivan

---

### Fase 5: Sistema de Permisos
**Duración estimada:** 6 días

#### Tareas de Ivan
1. **Implementar patrón Singleton para gestión de sesión**
   - Descripción: Crear clase Singleton para gestionar la sesión activa
   - Pasos:
     1. Crear clase SesionManager con constructor privado
     2. Crear propiedad estatica Instance
     3. Crear metodo IniciarSesion(Usuario usuario)
     4. Crear metodo CerrarSesion()
     5. Crear metodo ObtenerUsuarioActual()
     6. Crear metodo SesionActiva()
   - Criterios de aceptación: Solo existe una instancia, gestión de sesión funciona
   - Complejidad: Media

2. **Crear tablas de permisos y perfiles**
   - Descripción: Crear esquema de base de datos para sistema de permisos
   - Pasos:
     1. Crear tabla FAMILIA con campos: ID, Nombre, Profundidad
     2. Crear tabla PERMISO con campos: ID, Nombre, Descripcion, FamiliaID
     3. Crear tabla PERFIL con campos: ID, Nombre, Descripcion
     4. Crear tabla PERFIL_PERMISO con campos: PerfilID, PermisoID
     5. Crear tabla USUARIO_PERFIL con campos: UsuarioID, PerfilID
     6. Agregar foreign keys apropiadas
   - Criterios de aceptación: Script SQL ejecutable, todas las relaciones definidas
   - Complejidad: Media

3. **Implementar validación de permisos**
   - Descripción: Crear lógica para verificar permisos de usuario
   - Pasos:
     1. Crear metodo TienePermiso(int usuarioID, string nombrePermiso)
     2. Crear metodo ObtenerPermisos(int usuarioID)
     3. Crear metodo ObtenerPermisosPorFamilia(int usuarioID, int familiaID)
     4. Implementar cache de permisos para optimización
   - Criterios de aceptación: Validación de permisos funciona, cache funciona
   - Complejidad: Media

#### Tareas de Dana
1. **Implementar patrón Composite para sistema de permisos**
   - Descripción: Crear estructura jerárquica de permisos usando Composite
   - Requisitos:
     - Crear interfaz IComponentePermiso
     - Crear clase Permiso (hoja)
     - Crear clase FamiliaPermiso (composite)
     - Implementar metodos: Agregar, Eliminar, ObtenerHijos, VerificarPermiso
     - Crear jerarquía de permisos por módulos
   - Entregables: Clases del patrón Composite implementadas
   - Complejidad: Alta

2. **Definir perfiles de usuario**
   - Descripción: Crear estructura de perfiles con sus permisos
   - Requisitos:
     - Perfil Administrador: Todos los permisos
     - Perfil Recepcionista: Gestión de alumnos, pagos, abonos
     - Perfil Alumno: Ver perfil, pagar abono, ver historial
     - Perfil Entrenador: Ver alumnos asignados, crear rutinas
     - Documentar cada perfil y sus permisos
   - Entregables: Documentación de perfiles, script de inserción de perfiles
   - Complejidad: Media

3. **Crear servicio de gestión de permisos**
   - Descripción: Implementar servicio centralizado para gestión de permisos
   - Requisitos:
     - Crear clase PermisoService
     - Metodos para asignar permisos a perfiles
     - Metodos para asignar perfiles a usuarios
     - Metodos para verificar permisos en tiempo de ejecución
     - Integración con patrón Composite
   - Entregables: PermisoService.cs completamente implementado
   - Complejidad: Alta

4. **Diseñar interfaz de gestión de permisos (admin)**
   - Descripción: Crear página para administradores para gestionar permisos
   - Requisitos:
     - Vista de jerarquía de permisos
     - Opción para crear/editar perfiles
     - Opción para asignar permisos a perfiles
     - Opción para asignar perfiles a usuarios
     - Diseño intuitivo con rem
   - Entregables: GestionPermisos.aspx, GestionPermisos.aspx.cs, estilos CSS
   - Complejidad: Alta

#### Dependencias
- Tarea 1 de Ivan depende de Fase 2 completada
- Tarea 2 de Ivan depende de Tarea 1 de Ivan
- Tarea 3 de Ivan depende de Tasea 2 de Ivan
- Tarea 1 de Dana depende de Tarea 2 de Ivan
- Tarea 2 de Dana depende de Tarea 2 de Ivan
- Tarea 3 de Dana depende de Tarea 1 y 2 de Dana
- Tarea 4 de Dana depende de Tarea 3 de Dana

---

### Fase 6: Interfaz de Usuario y Experiencia
**Duración estimada:** 4 días

#### Tareas de Ivan
1. **Implementar menú de navegación dinámico**
   - Descripción: Crear menú que se adapta según permisos del usuario
   - Pasos:
     1. Crear metodo ObtenerMenu(int usuarioID)
     2. Filtrar opciones según permisos
     3. Generar estructura HTML del menú
     4. Integrar con Master Page
   - Criterios de aceptación: Menú muestra solo opciones permitidas, funciona correctamente
   - Complejidad: Media

2. **Implementar notificaciones del sistema**
   - Descripción: Crear sistema de notificaciones para usuarios
   - Pasos:
     1. Crear tabla NOTIFICACIONES con campos: ID, UsuarioID, Mensaje, Leida, Fecha
     2. Crear metodo CrearNotificacion(int usuarioID, string mensaje)
     3. Crear metodo ObtenerNotificaciones(int usuarioID)
     4. Crear metodo MarcarComoLeida(int notificacionID)
     5. Mostrar contador de notificaciones no leídas
   - Criterios de aceptación: Notificaciones se crean y muestran correctamente
   - Complejidad: Baja

#### Tareas de Dana
1. **Diseñar Master Page principal**
   - Descripción: Crear página maestra con diseño consistente
   - Requisitos:
     - Header con logo y nombre del sistema
     - Menú de navegación dinámico
     - Footer con información de contacto
     - Área de contenido principal
     - Diseño responsive con rem
     - Paleta de colores consistente
   - Entregables: Site.Master, Site.Master.css
   - Complejidad: Alta

2. **Crear página de inicio (dashboard)**
   - Descripción: Diseñar página principal con información relevante
   - Requisitos:
     - Bienvenida al usuario
     - Resumen de estado de abono
     - Accesos rápidos según perfil
     - Notificaciones recientes
     - Diseño atractivo y funcional
   - Entregables: Inicio.aspx, Inicio.aspx.cs, estilos CSS
   - Complejidad: Media

3. **Implementar diseño responsive global**
   - Descripción: Asegurar que todas las páginas sean responsive
   - Requisitos:
     - Usar unidades rem en todos los estilos
     - Media queries para diferentes tamaños de pantalla
     - Diseño mobile-first
     - Pruebas en diferentes dispositivos
     - Consistencia visual en todas las páginas
   - Entregables: Estilos CSS responsive para todo el sitio
   - Complejidad: Alta

4. **Crear página de error personalizada**
   - Descripción: Diseñar páginas de error amigables
   - Requisitos:
     - Página 404 (no encontrado)
     - Página 403 (acceso denegado)
     - Página 500 (error del servidor)
     - Diseño consistente con el sitio
     - Mensajes claros y útiles
   - Entregables: Error404.aspx, Error403.aspx, Error500.aspx, estilos CSS
   - Complejidad: Baja

#### Dependencias
- Tarea 1 de Ivan depende de Fase 5 completada
- Tarea 2 de Ivan puede hacerse en paralelo con Tarea 1 de Ivan
- Tarea 1 de Dana depende de Tarea 1 de Ivan
- Tarea 2 de Dana depende de Tarea 1 de Dana
- Tarea 3 de Dana depende de todas las páginas anteriores
- Tarea 4 de Dana puede hacerse en paralelo con otras tareas

---

### Fase 7: Pruebas y Despliegue
**Duración estimada:** 3 días

#### Tareas de Ivan
1. **Realizar pruebas unitarias**
   - Descripción: Crear pruebas unitarias para servicios principales
   - Pasos:
     1. Crear pruebas para EncriptacionService
     2. Crear pruebas para HashService
     3. Crear pruebas para SesionManager
     4. Crear pruebas para ValidacionContrasenaService
     5. Ejecutar todas las pruebas
     6. Corregir errores encontrados
   - Criterios de aceptación: Todas las pruebas pasan
   - Complejidad: Media

2. **Realizar pruebas de integración**
   - Descripción: Probar el flujo completo de la aplicación
   - Pasos:
     1. Probar flujo de registro completo
     2. Probar flujo de login y cambio de contraseña
     3. Probar flujo de pago de abono
     4. Probar validación de permisos
     5. Probar todos los perfiles de usuario
   - Criterios de aceptación: Todos los flujos funcionan correctamente
   - Complejidad: Alta

3. **Optimizar consultas a base de datos**
   - Descripción: Revisar y optimizar consultas SQL
   - Pasos:
     1. Identificar consultas lentas
     2. Agregar índices donde sea necesario
     3. Optimizar joins
     4. Revisar planes de ejecución
     5. Probar mejoras
   - Criterios de aceptación: Consultas optimizadas, tiempos de respuesta mejorados
   - Complejidad: Media

#### Tareas de Dana
1. **Realizar pruebas de UI/UX**
   - Descripción: Probar la interfaz de usuario en diferentes escenarios
   - Requisitos:
     - Probar en diferentes navegadores (Chrome, Firefox, Edge)
     - Probar en diferentes tamaños de pantalla
     - Probar con diferentes perfiles de usuario
     - Verificar accesibilidad
     - Documentar problemas encontrados
   - Entregables: Reporte de pruebas de UI/UX, correcciones aplicadas
   - Complejidad: Media

2. **Crear documentación de usuario**
   - Descripción: Escribir manual de usuario para el sistema
   - Requisitos:
     - Guía de registro
     - Guía de login y cambio de contraseña
     - Guía de pago de abono
     - Guía para administrativos
     - Capturas de pantalla
     - FAQ
   - Entregables: Manual de usuario en formato PDF
   - Complejidad: Media

3. **Preparar despliegue en producción**
   - Descripción: Configurar aplicación para despliegue
   - Requisitos:
     - Configurar Web.config para producción
     - Eliminar modo debug
     - Configurar connection strings de producción
     - Optimizar recursos (CSS, JS)
     - Crear script de despliegue
   - Entregables: Aplicación lista para despliegue, script de despliegue
   - Complejidad: Media

#### Dependencias
- Tarea 1 de Ivan depende de Fase 6 completada
- Tarea 2 de Ivan depende de Tarea 1 de Ivan
- Tarea 3 de Ivan depende de Tarea 2 de Ivan
- Tarea 1 de Dana depende de Fase 6 completada
- Tarea 2 de Dana depende de Tarea 1 de Dana
- Tarea 3 de Dana depende de Tarea 2 de Dana

---

## Resumen de Tiempos

| Fase | Duración | Tareas Ivan | Tareas Dana |
|------|----------|-------------|-------------|
| Fase 1: Base de Datos e Infraestructura | 3 días | 3 | 3 |
| Fase 2: Seguridad y Autenticación | 5 días | 3 | 3 |
| Fase 3: Gestión de Usuarios | 4 días | 3 | 3 |
| Fase 4: Suscripción y Pagos | 5 días | 3 | 4 |
| Fase 5: Sistema de Permisos | 6 días | 3 | 4 |
| Fase 6: UI y Experiencia | 4 días | 2 | 4 |
| Fase 7: Pruebas y Despliegue | 3 días | 3 | 3 |
| **Total** | **30 días** | **20** | **24** |

---

## Cronograma Recomendado

**Semana 1-2:** Fase 1 y Fase 2
**Semana 3:** Fase 3
**Semana 4:** Fase 4
**Semana 5-6:** Fase 5
**Semana 7:** Fase 6
**Semana 8:** Fase 7

---

## Notas Importantes

### Para Ivan
- Mantener las tareas enfocadas y específicas
- Seguir los pasos numerados en orden
- Verificar criterios de aceptación antes de pasar a la siguiente tarea
- NO asignar tareas relacionadas con patrones de diseño (Singleton y Composite están asignados a Dana)
- Documentar el código con comentarios claros

### Para Dana
- Aprovechar su capacidad para manejar tareas complejas y estructuradas
- Prestar atención a los detalles de UI/UX
- Asegurar que todos los estilos usen unidades rem
- Documentar patrones de diseño implementados
- Coordinar con Ivan para integrar servicios con interfaz

### Para el Equipo
- Reuniones diarias de 15 minutos para sincronización
- Revisión de código antes de integrar
- Pruebas continuas durante el desarrollo
- Documentación actualizada en cada fase
- Comunicación clara sobre dependencias

---

## Riesgos y Mitigación

| Riesgo | Probabilidad | Impacto | Mitigación |
|--------|--------------|---------|------------|
| Retraso en implementación de permisos | Media | Alto | Priorizar Fase 5, asignar más tiempo si es necesario |
| Problemas de compatibilidad responsive | Baja | Medio | Pruebas tempranas en diferentes dispositivos |
| Errores en encriptación de datos | Baja | Alto | Pruebas exhaustivas de encriptación/desencriptación |
| Cambios en requisitos | Media | Medio | Mantener comunicación constante con stakeholders |
| Problemas de rendimiento en base de datos | Baja | Medio | Optimización continua, índices apropiados |

---

## Criterios de Éxito del Proyecto

1. **Funcionalidad:** Todos los requisitos funcionales implementados y probados
2. **Seguridad:** Sistema de autenticación y permisos funcionando correctamente
3. **Usabilidad:** Interfaz intuitiva y responsive
4. **Performance:** Tiempos de respuesta aceptables (< 2 segundos)
5. **Calidad:** Código limpio, documentado y mantenible
6. **Testing:** Todas las pruebas unitarias y de integración pasando

---

## Entregables Finales

1. Aplicación web Sportio completamente funcional
2. Base de datos con todos los esquemas y datos iniciales
3. Documentación técnica del sistema
4. Manual de usuario
5. Script de despliegue
6. Código fuente con control de versiones

---

**Última actualización:** 2026-04-20
**Versión:** 1.0