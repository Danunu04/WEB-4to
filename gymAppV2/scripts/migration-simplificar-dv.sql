-- =============================================================================
-- MIGRACIÓN: Simplificación del sistema de dígito verificador
-- Script idempotente: cada ALTER solo se ejecuta si la columna existe.
-- =============================================================================
-- Qué hace este script:
--   1. Elimina la columna dvv de las 23 tablas con control de integridad.
--      El dvv por fila es redundante: el dvvTabla en DigitoVerificador lo cubre.
--   2. Elimina la columna dvhTabla de DigitoVerificador (redundante con dvvTabla).
--      dvvTabla pasa a ser = SHA256(dvh_fila1 + dvh_fila2 + ...), el resumen vertical.
--   3. Agrega la columna cantidadFilas a DigitoVerificador para detectar eliminaciones.
--
-- IMPORTANTE: después de ejecutar este script, ingresar al sistema como
-- Administrador, ir a Verificación de Integridad y presionar "Recalcular todos
-- los valores". Esto recalcula dvvTabla y cantidadFilas con el nuevo esquema.
-- =============================================================================

-- 1. Eliminar dvv de todas las tablas con control de integridad
IF COL_LENGTH('[GymApp].[dbo].[Actividad_Alumno]', 'dvv')     IS NOT NULL ALTER TABLE [GymApp].[dbo].[Actividad_Alumno]     DROP COLUMN dvv;
IF COL_LENGTH('[GymApp].[dbo].[Actividad_Entrenador]', 'dvv') IS NOT NULL ALTER TABLE [GymApp].[dbo].[Actividad_Entrenador] DROP COLUMN dvv;
IF COL_LENGTH('[GymApp].[dbo].[Actividades]', 'dvv')           IS NOT NULL ALTER TABLE [GymApp].[dbo].[Actividades]           DROP COLUMN dvv;
IF COL_LENGTH('[GymApp].[dbo].[AlumnoRM]', 'dvv')              IS NOT NULL ALTER TABLE [GymApp].[dbo].[AlumnoRM]              DROP COLUMN dvv;
IF COL_LENGTH('[GymApp].[dbo].[ALUMNOS]', 'dvv')               IS NOT NULL ALTER TABLE [GymApp].[dbo].[ALUMNOS]               DROP COLUMN dvv;
IF COL_LENGTH('[GymApp].[dbo].[Ejercicio]', 'dvv')             IS NOT NULL ALTER TABLE [GymApp].[dbo].[Ejercicio]             DROP COLUMN dvv;
IF COL_LENGTH('[GymApp].[dbo].[ENTRENADORES]', 'dvv')          IS NOT NULL ALTER TABLE [GymApp].[dbo].[ENTRENADORES]          DROP COLUMN dvv;
IF COL_LENGTH('[GymApp].[dbo].[Evento]', 'dvv')                IS NOT NULL ALTER TABLE [GymApp].[dbo].[Evento]                DROP COLUMN dvv;
IF COL_LENGTH('[GymApp].[dbo].[Familia]', 'dvv')               IS NOT NULL ALTER TABLE [GymApp].[dbo].[Familia]               DROP COLUMN dvv;
IF COL_LENGTH('[GymApp].[dbo].[Perfil_Familia]', 'dvv')        IS NOT NULL ALTER TABLE [GymApp].[dbo].[Perfil_Familia]        DROP COLUMN dvv;
IF COL_LENGTH('[GymApp].[dbo].[Perfil_Permiso]', 'dvv')        IS NOT NULL ALTER TABLE [GymApp].[dbo].[Perfil_Permiso]        DROP COLUMN dvv;
IF COL_LENGTH('[GymApp].[dbo].[Perfiles]', 'dvv')              IS NOT NULL ALTER TABLE [GymApp].[dbo].[Perfiles]              DROP COLUMN dvv;
IF COL_LENGTH('[GymApp].[dbo].[Permiso]', 'dvv')               IS NOT NULL ALTER TABLE [GymApp].[dbo].[Permiso]               DROP COLUMN dvv;
IF COL_LENGTH('[GymApp].[dbo].[PermisoFamilia]', 'dvv')        IS NOT NULL ALTER TABLE [GymApp].[dbo].[PermisoFamilia]        DROP COLUMN dvv;
IF COL_LENGTH('[GymApp].[dbo].[PesoHistorial]', 'dvv')         IS NOT NULL ALTER TABLE [GymApp].[dbo].[PesoHistorial]         DROP COLUMN dvv;
IF COL_LENGTH('[GymApp].[dbo].[PrecioModalidad]', 'dvv')       IS NOT NULL ALTER TABLE [GymApp].[dbo].[PrecioModalidad]       DROP COLUMN dvv;
IF COL_LENGTH('[GymApp].[dbo].[PreguntasSeguridad]', 'dvv')    IS NOT NULL ALTER TABLE [GymApp].[dbo].[PreguntasSeguridad]    DROP COLUMN dvv;
IF COL_LENGTH('[GymApp].[dbo].[RutinaEjercicio]', 'dvv')       IS NOT NULL ALTER TABLE [GymApp].[dbo].[RutinaEjercicio]       DROP COLUMN dvv;
IF COL_LENGTH('[GymApp].[dbo].[Rutinas]', 'dvv')               IS NOT NULL ALTER TABLE [GymApp].[dbo].[Rutinas]               DROP COLUMN dvv;
IF COL_LENGTH('[GymApp].[dbo].[USUARIO_Contras]', 'dvv')       IS NOT NULL ALTER TABLE [GymApp].[dbo].[USUARIO_Contras]       DROP COLUMN dvv;
IF COL_LENGTH('[GymApp].[dbo].[USUARIO_Intentos]', 'dvv')      IS NOT NULL ALTER TABLE [GymApp].[dbo].[USUARIO_Intentos]      DROP COLUMN dvv;
IF COL_LENGTH('[GymApp].[dbo].[Usuario_Perfil]', 'dvv')        IS NOT NULL ALTER TABLE [GymApp].[dbo].[Usuario_Perfil]        DROP COLUMN dvv;
IF COL_LENGTH('[GymApp].[dbo].[USUARIOS]', 'dvv')              IS NOT NULL ALTER TABLE [GymApp].[dbo].[USUARIOS]              DROP COLUMN dvv;

-- 2. Eliminar dvhTabla de DigitoVerificador
IF COL_LENGTH('[GymApp].[dbo].[DigitoVerificador]', 'dvhTabla') IS NOT NULL
    ALTER TABLE [GymApp].[dbo].[DigitoVerificador] DROP COLUMN dvhTabla;

-- 3. Agregar cantidadFilas a DigitoVerificador (solo si no existe)
IF COL_LENGTH('[GymApp].[dbo].[DigitoVerificador]', 'cantidadFilas') IS NULL
    ALTER TABLE [GymApp].[dbo].[DigitoVerificador] ADD cantidadFilas INT NULL;

-- =============================================================================
-- FIN DEL SCRIPT
-- Próximo paso: ejecutar "Recalcular todos los valores" desde la pantalla
-- de Verificación de Integridad (Administrador).
-- =============================================================================
