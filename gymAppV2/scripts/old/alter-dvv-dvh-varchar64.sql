-- ============================================================================
-- Migración: ampliar columnas dvv/dvh a VARCHAR(64) para SHA-256
-- ============================================================================
-- SHA-256 en formato hexadecimal son 64 caracteres. Este script altera todas
-- las columnas dvv/dvh existentes y asegura que no contengan NULL.
-- ============================================================================

USE [GymApp];
GO

-- Tabla USUARIOS
UPDATE [dbo].[USUARIOS] SET [dvv] = '' WHERE [dvv] IS NULL;
UPDATE [dbo].[USUARIOS] SET [dvh] = '' WHERE [dvh] IS NULL;
ALTER TABLE [dbo].[USUARIOS] ALTER COLUMN [dvv] VARCHAR(64) NOT NULL;
ALTER TABLE [dbo].[USUARIOS] ALTER COLUMN [dvh] VARCHAR(64) NOT NULL;
GO

-- Tabla ALUMNOS
UPDATE [dbo].[ALUMNOS] SET [dvv] = '' WHERE [dvv] IS NULL;
UPDATE [dbo].[ALUMNOS] SET [dvh] = '' WHERE [dvh] IS NULL;
ALTER TABLE [dbo].[ALUMNOS] ALTER COLUMN [dvv] VARCHAR(64) NOT NULL;
ALTER TABLE [dbo].[ALUMNOS] ALTER COLUMN [dvh] VARCHAR(64) NOT NULL;
GO

-- Tabla ENTRENADORES
UPDATE [dbo].[ENTRENADORES] SET [dvv] = '' WHERE [dvv] IS NULL;
UPDATE [dbo].[ENTRENADORES] SET [dvh] = '' WHERE [dvh] IS NULL;
ALTER TABLE [dbo].[ENTRENADORES] ALTER COLUMN [dvv] VARCHAR(64) NOT NULL;
ALTER TABLE [dbo].[ENTRENADORES] ALTER COLUMN [dvh] VARCHAR(64) NOT NULL;
GO

-- Tabla USUARIO_Intentos (obsoleta, se mantiene por compatibilidad)
UPDATE [dbo].[USUARIO_Intentos] SET [dvv] = '' WHERE [dvv] IS NULL;
UPDATE [dbo].[USUARIO_Intentos] SET [dvh] = '' WHERE [dvh] IS NULL;
ALTER TABLE [dbo].[USUARIO_Intentos] ALTER COLUMN [dvv] VARCHAR(64) NOT NULL;
ALTER TABLE [dbo].[USUARIO_Intentos] ALTER COLUMN [dvh] VARCHAR(64) NOT NULL;
GO

-- Tabla USUARIO_Contras
UPDATE [dbo].[USUARIO_Contras] SET [dvv] = '' WHERE [dvv] IS NULL;
UPDATE [dbo].[USUARIO_Contras] SET [dvh] = '' WHERE [dvh] IS NULL;
ALTER TABLE [dbo].[USUARIO_Contras] ALTER COLUMN [dvv] VARCHAR(64) NOT NULL;
ALTER TABLE [dbo].[USUARIO_Contras] ALTER COLUMN [dvh] VARCHAR(64) NOT NULL;
GO

-- Tabla PreguntasSeguridad
UPDATE [dbo].[PreguntasSeguridad] SET [dvv] = '' WHERE [dvv] IS NULL;
UPDATE [dbo].[PreguntasSeguridad] SET [dvh] = '' WHERE [dvh] IS NULL;
ALTER TABLE [dbo].[PreguntasSeguridad] ALTER COLUMN [dvv] VARCHAR(64) NOT NULL;
ALTER TABLE [dbo].[PreguntasSeguridad] ALTER COLUMN [dvh] VARCHAR(64) NOT NULL;
GO

-- Tabla Perfiles
UPDATE [dbo].[Perfiles] SET [dvv] = '' WHERE [dvv] IS NULL;
UPDATE [dbo].[Perfiles] SET [dvh] = '' WHERE [dvh] IS NULL;
ALTER TABLE [dbo].[Perfiles] ALTER COLUMN [dvv] VARCHAR(64) NOT NULL;
ALTER TABLE [dbo].[Perfiles] ALTER COLUMN [dvh] VARCHAR(64) NOT NULL;
GO

-- Tabla Usuario_Perfil
UPDATE [dbo].[Usuario_Perfil] SET [dvv] = '' WHERE [dvv] IS NULL;
UPDATE [dbo].[Usuario_Perfil] SET [dvh] = '' WHERE [dvh] IS NULL;
ALTER TABLE [dbo].[Usuario_Perfil] ALTER COLUMN [dvv] VARCHAR(64) NOT NULL;
ALTER TABLE [dbo].[Usuario_Perfil] ALTER COLUMN [dvh] VARCHAR(64) NOT NULL;
GO

-- Tabla Actividades
UPDATE [dbo].[Actividades] SET [dvv] = '' WHERE [dvv] IS NULL;
UPDATE [dbo].[Actividades] SET [dvh] = '' WHERE [dvh] IS NULL;
ALTER TABLE [dbo].[Actividades] ALTER COLUMN [dvv] VARCHAR(64) NOT NULL;
ALTER TABLE [dbo].[Actividades] ALTER COLUMN [dvh] VARCHAR(64) NOT NULL;
GO

-- Tabla Ejercicio
UPDATE [dbo].[Ejercicio] SET [dvv] = '' WHERE [dvv] IS NULL;
UPDATE [dbo].[Ejercicio] SET [dvh] = '' WHERE [dvh] IS NULL;
ALTER TABLE [dbo].[Ejercicio] ALTER COLUMN [dvv] VARCHAR(64) NOT NULL;
ALTER TABLE [dbo].[Ejercicio] ALTER COLUMN [dvh] VARCHAR(64) NOT NULL;
GO

-- Tabla Rutinas
UPDATE [dbo].[Rutinas] SET [dvv] = '' WHERE [dvv] IS NULL;
UPDATE [dbo].[Rutinas] SET [dvh] = '' WHERE [dvh] IS NULL;
ALTER TABLE [dbo].[Rutinas] ALTER COLUMN [dvv] VARCHAR(64) NOT NULL;
ALTER TABLE [dbo].[Rutinas] ALTER COLUMN [dvh] VARCHAR(64) NOT NULL;
GO

-- Tabla RutinaEjercicio
UPDATE [dbo].[RutinaEjercicio] SET [dvv] = '' WHERE [dvv] IS NULL;
UPDATE [dbo].[RutinaEjercicio] SET [dvh] = '' WHERE [dvh] IS NULL;
ALTER TABLE [dbo].[RutinaEjercicio] ALTER COLUMN [dvv] VARCHAR(64) NOT NULL;
ALTER TABLE [dbo].[RutinaEjercicio] ALTER COLUMN [dvh] VARCHAR(64) NOT NULL;
GO

-- Tabla AlumnoRM
UPDATE [dbo].[AlumnoRM] SET [dvv] = '' WHERE [dvv] IS NULL;
UPDATE [dbo].[AlumnoRM] SET [dvh] = '' WHERE [dvh] IS NULL;
ALTER TABLE [dbo].[AlumnoRM] ALTER COLUMN [dvv] VARCHAR(64) NOT NULL;
ALTER TABLE [dbo].[AlumnoRM] ALTER COLUMN [dvh] VARCHAR(64) NOT NULL;
GO

-- Tabla PesoHistorial
UPDATE [dbo].[PesoHistorial] SET [dvv] = '' WHERE [dvv] IS NULL;
UPDATE [dbo].[PesoHistorial] SET [dvh] = '' WHERE [dvh] IS NULL;
ALTER TABLE [dbo].[PesoHistorial] ALTER COLUMN [dvv] VARCHAR(64) NOT NULL;
ALTER TABLE [dbo].[PesoHistorial] ALTER COLUMN [dvh] VARCHAR(64) NOT NULL;
GO

-- Tabla Actividad_Alumno
UPDATE [dbo].[Actividad_Alumno] SET [dvv] = '' WHERE [dvv] IS NULL;
UPDATE [dbo].[Actividad_Alumno] SET [dvh] = '' WHERE [dvh] IS NULL;
ALTER TABLE [dbo].[Actividad_Alumno] ALTER COLUMN [dvv] VARCHAR(64) NOT NULL;
ALTER TABLE [dbo].[Actividad_Alumno] ALTER COLUMN [dvh] VARCHAR(64) NOT NULL;
GO

-- Tabla Actividad_Entrenador
UPDATE [dbo].[Actividad_Entrenador] SET [dvv] = '' WHERE [dvv] IS NULL;
UPDATE [dbo].[Actividad_Entrenador] SET [dvh] = '' WHERE [dvh] IS NULL;
ALTER TABLE [dbo].[Actividad_Entrenador] ALTER COLUMN [dvv] VARCHAR(64) NOT NULL;
ALTER TABLE [dbo].[Actividad_Entrenador] ALTER COLUMN [dvh] VARCHAR(64) NOT NULL;
GO

-- Tabla Evento
UPDATE [dbo].[Evento] SET [dvv] = '' WHERE [dvv] IS NULL;
UPDATE [dbo].[Evento] SET [dvh] = '' WHERE [dvh] IS NULL;
ALTER TABLE [dbo].[Evento] ALTER COLUMN [dvv] VARCHAR(64) NOT NULL;
ALTER TABLE [dbo].[Evento] ALTER COLUMN [dvh] VARCHAR(64) NOT NULL;
GO

-- Tabla Familia
UPDATE [dbo].[Familia] SET [dvv] = '' WHERE [dvv] IS NULL;
UPDATE [dbo].[Familia] SET [dvh] = '' WHERE [dvh] IS NULL;
ALTER TABLE [dbo].[Familia] ALTER COLUMN [dvv] VARCHAR(64) NOT NULL;
ALTER TABLE [dbo].[Familia] ALTER COLUMN [dvh] VARCHAR(64) NOT NULL;
GO

-- Tabla Permiso
UPDATE [dbo].[Permiso] SET [dvv] = '' WHERE [dvv] IS NULL;
UPDATE [dbo].[Permiso] SET [dvh] = '' WHERE [dvh] IS NULL;
ALTER TABLE [dbo].[Permiso] ALTER COLUMN [dvv] VARCHAR(64) NOT NULL;
ALTER TABLE [dbo].[Permiso] ALTER COLUMN [dvh] VARCHAR(64) NOT NULL;
GO

-- Tabla PermisoFamilia
UPDATE [dbo].[PermisoFamilia] SET [dvv] = '' WHERE [dvv] IS NULL;
UPDATE [dbo].[PermisoFamilia] SET [dvh] = '' WHERE [dvh] IS NULL;
ALTER TABLE [dbo].[PermisoFamilia] ALTER COLUMN [dvv] VARCHAR(64) NOT NULL;
ALTER TABLE [dbo].[PermisoFamilia] ALTER COLUMN [dvh] VARCHAR(64) NOT NULL;
GO

-- Tabla Perfil_Familia
UPDATE [dbo].[Perfil_Familia] SET [dvv] = '' WHERE [dvv] IS NULL;
UPDATE [dbo].[Perfil_Familia] SET [dvh] = '' WHERE [dvh] IS NULL;
ALTER TABLE [dbo].[Perfil_Familia] ALTER COLUMN [dvv] VARCHAR(64) NOT NULL;
ALTER TABLE [dbo].[Perfil_Familia] ALTER COLUMN [dvh] VARCHAR(64) NOT NULL;
GO

-- Tabla Perfil_Permiso
UPDATE [dbo].[Perfil_Permiso] SET [dvv] = '' WHERE [dvv] IS NULL;
UPDATE [dbo].[Perfil_Permiso] SET [dvh] = '' WHERE [dvh] IS NULL;
ALTER TABLE [dbo].[Perfil_Permiso] ALTER COLUMN [dvv] VARCHAR(64) NOT NULL;
ALTER TABLE [dbo].[Perfil_Permiso] ALTER COLUMN [dvh] VARCHAR(64) NOT NULL;
GO

-- Tabla PrecioModalidad
UPDATE [dbo].[PrecioModalidad] SET [dvv] = '' WHERE [dvv] IS NULL;
UPDATE [dbo].[PrecioModalidad] SET [dvh] = '' WHERE [dvh] IS NULL;
ALTER TABLE [dbo].[PrecioModalidad] ALTER COLUMN [dvv] VARCHAR(64) NOT NULL;
ALTER TABLE [dbo].[PrecioModalidad] ALTER COLUMN [dvh] VARCHAR(64) NOT NULL;
GO

PRINT 'Columnas dvv/dvh migradas a VARCHAR(64) NOT NULL';
GO
