# Documentación de Arquitectura - Sportio

## Índice de Documentación

Esta documentación describe la arquitectura de capas del sistema Sportio, una aplicación web ASP.NET Web Forms para la gestión integral de un gimnasio.

### Documentos Principales

- [Arquitectura General](arquitectura-general.md) - Visión general de la arquitectura del sistema
- [Capa de Presentación](capas-presentacion.md) - Detalles de la capa de interfaz de usuario
- [Capa de Lógica de Negocio](capas-negocio.md) - Detalles de la capa de lógica de negocio
- [Capa de Acceso a Datos](capas-acceso-datos.md) - Detalles de la capa de persistencia
- [Comunicación entre Capas](comunicacion-capas.md) - Flujo de datos e interacción entre capas
- [Patrones de Diseño](patrones-diseno.md) - Patrones de diseño implementados
- [Diagramas de Secuencia](diagramas-secuencia.md) - Diagramas de secuencia para flujos principales

### Información del Proyecto

- **Nombre del Proyecto:** Sportio
- **Tecnología:** ASP.NET Web Forms (.NET Framework 4.7.2)
- **Base de Datos:** SQL Server
- **Equipo:** Dana Perelmuter, Ivan Urso
- **Fecha de Creación:** 2026-04-20

### Módulos del Sistema

1. **Sistema de Seguridad**
   - Login y autenticación
   - Cambio de contraseña
   - Encriptación de datos
   - Preguntas de seguridad

2. **Gestión de Usuarios**
   - Registro de usuarios
   - Gestión de alumnos
   - Perfiles de usuario

3. **Sistema de Permisos**
   - Patrones Singleton y Composite
   - Gestión de roles y permisos
   - Control de acceso

4. **Gestión de Suscripción y Pagos**
   - Abono mensual
   - Historial de pagos
   - Estado de suscripción

### Convenciones de Nomenclatura

- **Entidades:** Nombres en singular (ej: Usuario, Alumno, Pago)
- **Servicios:** Sufijo "Service" (ej: UsuarioService, PagoService)
- **Mapeadores:** Sufijo "Mapper" (ej: UsuarioMapper, AlumnoMapper)
- **Páginas Web:** Sufijo ".aspx" (ej: Login.aspx, AbonoMensual.aspx)
- **Estilos CSS:** Sufijo ".css" (ej: login.css, estilos.css)

### Estándares de Código

- **Unidades CSS:** Siempre usar `rem` para responsive design
- **Encriptación:**
  - Reversible para datos personales (AES-256)
  - Irreversible para contraseñas (SHA256)
- **Validaciones:**
  - Contraseña: mínimo 6 caracteres, 1 mayúscula, 1 carácter especial
  - Usuario: nombre único en el sistema
- **Intentos de Login:** Máximo 3 intentos antes de bloqueo

---

**Última actualización:** 2026-04-20
**Versión:** 1.0