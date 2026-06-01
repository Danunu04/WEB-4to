-- ============================================================================
-- Script de Creación de Base de Datos - GymApp v2 Normalizada
-- ============================================================================
-- Fecha: 2026-05-28
-- Descripción: Esquema normalizado con datos personales centralizados en USUARIOS
--              y diferenciación por Tipo (Empleado, Entrenador, Cliente)
-- ============================================================================

USE [master];
GO

CREATE DATABASE [GymApp]
  CONTAINMENT = NONE
  ON  PRIMARY
( NAME = N'GymApp', FILENAME = N'C:\Program Files\Microsoft SQL Server\MSSQL17.MSSQLSERVER\MSSQL\DATA\GymApp.mdf' , SIZE = 8192KB , MAXSIZE = UNLIMITED, FILEGROWTH = 65536KB )
  LOG ON
( NAME = N'GymApp_log', FILENAME = N'C:\Program Files\Microsoft SQL Server\MSSQL17.MSSQLSERVER\MSSQL\DATA\GymApp_log.ldf' , SIZE = 8192KB , MAXSIZE = 2048GB , FILEGROWTH = 65536KB )
  WITH CATALOG_COLLATION = DATABASE_DEFAULT, LEDGER = OFF
GO

ALTER DATABASE [GymApp] SET COMPATIBILITY_LEVEL = 170
GO

ALTER DATABASE [GymApp] SET ANSI_NULL_DEFAULT OFF
GO
ALTER DATABASE [GymApp] SET ANSI_NULLS OFF
GO
ALTER DATABASE [GymApp] SET ANSI_PADDING OFF
GO
ALTER DATABASE [GymApp] SET ANSI_WARNINGS OFF
GO
ALTER DATABASE [GymApp] SET ARITHABORT OFF
GO
ALTER DATABASE [GymApp] SET AUTO_CLOSE OFF
GO
ALTER DATABASE [GymApp] SET AUTO_SHRINK OFF
GO
ALTER DATABASE [GymApp] SET AUTO_UPDATE_STATISTICS ON
GO
ALTER DATABASE [GymApp] SET CURSOR_CLOSE_ON_COMMIT OFF
GO
ALTER DATABASE [GymApp] SET CURSOR_DEFAULT  GLOBAL
GO
ALTER DATABASE [GymApp] SET CONCAT_NULL_YIELDS_NULL OFF
GO
ALTER DATABASE [GymApp] SET NUMERIC_ROUNDABORT OFF
GO
ALTER DATABASE [GymApp] SET QUOTED_IDENTIFIER OFF
GO
ALTER DATABASE [GymApp] SET RECURSIVE_TRIGGERS OFF
GO
ALTER DATABASE [GymApp] SET  ENABLE_BROKER
GO
ALTER DATABASE [GymApp] SET AUTO_UPDATE_STATISTICS_ASYNC OFF
GO
ALTER DATABASE [GymApp] SET DATE_CORRELATION_OPTIMIZATION OFF
GO
ALTER DATABASE [GymApp] SET TRUSTWORTHY OFF
GO
ALTER DATABASE [GymApp] SET ALLOW_SNAPSHOT_ISOLATION OFF
GO
ALTER DATABASE [GymApp] SET PARAMETERIZATION SIMPLE
GO
ALTER DATABASE [GymApp] SET READ_COMMITTED_SNAPSHOT OFF
GO
ALTER DATABASE [GymApp] SET HONOR_BROKER_PRIORITY OFF
GO
ALTER DATABASE [GymApp] SET RECOVERY FULL
GO
ALTER DATABASE [GymApp] SET  MULTI_USER
GO
ALTER DATABASE [GymApp] SET PAGE_VERIFY CHECKSUM
GO
ALTER DATABASE [GymApp] SET DB_CHAINING OFF
GO
ALTER DATABASE [GymApp] SET FILESTREAM( NON_TRANSACTED_ACCESS = OFF )
GO
ALTER DATABASE [GymApp] SET TARGET_RECOVERY_TIME = 60 SECONDS
GO
ALTER DATABASE [GymApp] SET DELAYED_DURABILITY = DISABLED
GO
ALTER DATABASE [GymApp] SET ACCELERATED_DATABASE_RECOVERY = OFF
GO
ALTER DATABASE [GymApp] SET QUERY_STORE = ON
GO

USE [GymApp]
GO

-- ============================================================================
-- TABLA PRINCIPAL: USUARIOS (con datos personales centralizados)
-- ============================================================================

CREATE TABLE [dbo].[USUARIOS](
    [usr]               VARCHAR(50)     NOT NULL,
    [contra]            VARCHAR(255)    NOT NULL,
    [activo]            BIT             NOT NULL,
    [bloqueado]         BIT             NOT NULL DEFAULT 0,
    [intentos]          INT             NOT NULL DEFAULT 0,
    [tipo]              VARCHAR(20)     NOT NULL,  -- 'Empleado', 'Entrenador', 'Cliente'
    [dni]               INT             NOT NULL,
    [nombre]            VARCHAR(100)    NOT NULL,
    [apellido]          VARCHAR(100)    NOT NULL,
    [telefono]          VARCHAR(20)     NULL,
    [email]             VARCHAR(255)    NULL,
    [fechaNacimiento]   DATE            NULL,
    [rol]               INT             NOT NULL,  -- 1=Admin, 2=Recepcionista, 3=Entrenador, 4=Cliente
    [dvv]               VARCHAR(50)     NOT NULL,
    [dvh]               VARCHAR(50)     NOT NULL,
    CONSTRAINT [PK_USUARIOS] PRIMARY KEY CLUSTERED ([usr] ASC),
    CONSTRAINT [UK_USUARIOS_DNI] UNIQUE ([dni] ASC),
    CONSTRAINT [CK_USUARIOS_Tipo] CHECK ([tipo] IN ('Empleado', 'Entrenador', 'Cliente'))
) ON [PRIMARY]
GO

-- ============================================================================
-- TABLA: ALUMNOS (solo datos específicos del rol)
-- ============================================================================

CREATE TABLE [dbo].[ALUMNOS](
    [dni]           INT             NOT NULL,
    [peso]          DECIMAL(5,2)    NULL,
    [tieneRutinas]  BIT             NOT NULL DEFAULT 0,
    [activo]        BIT             NOT NULL DEFAULT 1,
    [dvv]           VARCHAR(50)     NOT NULL,
    [dvh]           VARCHAR(50)     NOT NULL,
    CONSTRAINT [PK_ALUMNOS] PRIMARY KEY CLUSTERED ([dni] ASC),
    CONSTRAINT [FK_ALUMNOS_USUARIOS] FOREIGN KEY ([dni])
        REFERENCES [dbo].[USUARIOS] ([dni])
) ON [PRIMARY]
GO

-- Constraint de peso válido
ALTER TABLE [dbo].[ALUMNOS] WITH CHECK ADD CONSTRAINT [CK_ALUMNOS_Peso]
    CHECK (([peso] IS NULL OR ([peso] > 0 AND [peso] < 500)))
GO

-- ============================================================================
-- TABLA: ENTRENADORES (solo datos específicos del rol)
-- ============================================================================

CREATE TABLE [dbo].[ENTRENADORES](
    [dni]           INT             NOT NULL,
    [alumnosCount]  INT             NOT NULL DEFAULT 0,
    [activo]        BIT             NOT NULL DEFAULT 1,
    [dvv]           VARCHAR(50)     NOT NULL,
    [dvh]           VARCHAR(50)     NOT NULL,
    CONSTRAINT [PK_ENTRENADORES] PRIMARY KEY CLUSTERED ([dni] ASC),
    CONSTRAINT [FK_ENTRENADORES_USUARIOS] FOREIGN KEY ([dni])
        REFERENCES [dbo].[USUARIOS] ([dni])
) ON [PRIMARY]
GO

-- ============================================================================
-- TABLA: USUARIO_Contras (historial de contraseñas)
-- ============================================================================

CREATE TABLE [dbo].[USUARIO_Contras](
    [usr]       VARCHAR(50)     NOT NULL,
    [contra]    VARCHAR(255)    NOT NULL,
    [dvv]       VARCHAR(50)     NOT NULL,
    [dvh]       VARCHAR(50)     NOT NULL,
    CONSTRAINT [PK_USUARIO_Contras] PRIMARY KEY CLUSTERED ([usr] ASC, [contra] ASC),
    CONSTRAINT [FK_UsuarioContras_Usuario] FOREIGN KEY ([usr])
        REFERENCES [dbo].[USUARIOS] ([usr])
) ON [PRIMARY]
GO

-- ============================================================================
-- TABLA: Roles/Perfiles (permisos del sistema)
-- ============================================================================

CREATE TABLE [dbo].[Perfiles](
    [idPerfil]      INT             IDENTITY(1,1) NOT NULL,
    [nombrePerfil]  VARCHAR(100)    NOT NULL,
    [activo]        BIT             NOT NULL DEFAULT 1,
    [dvv]           VARCHAR(50)     NOT NULL,
    [dvh]           VARCHAR(50)     NOT NULL,
    CONSTRAINT [PK_Perfiles] PRIMARY KEY CLUSTERED ([idPerfil] ASC),
    CONSTRAINT [UK_Perfiles_Nombre] UNIQUE NONCLUSTERED ([nombrePerfil] ASC)
) ON [PRIMARY]
GO

CREATE TABLE [dbo].[Usuario_Perfil](
    [usr]       VARCHAR(50) NOT NULL,
    [idPerfil]  INT         NOT NULL,
    [dvv]       VARCHAR(50) NOT NULL,
    [dvh]       VARCHAR(50) NOT NULL,
    CONSTRAINT [PK_Usuario_Perfil] PRIMARY KEY CLUSTERED ([usr] ASC, [idPerfil] ASC),
    CONSTRAINT [FK_UsuarioPerfil_Usuario] FOREIGN KEY ([usr]) REFERENCES [dbo].[USUARIOS] ([usr]),
    CONSTRAINT [FK_UsuarioPerfil_Perfil] FOREIGN KEY ([idPerfil]) REFERENCES [dbo].[Perfiles] ([idPerfil])
) ON [PRIMARY]
GO

-- ============================================================================
-- TABLA: Actividades
-- ============================================================================

CREATE TABLE [dbo].[Actividades](
    [codActividad]      INT             IDENTITY(1,1) NOT NULL,
    [descripcion]        VARCHAR(200)    NOT NULL,
    [cantXSemana]       INT             NOT NULL,
    [costoInterno]      DECIMAL(10,2)   NOT NULL,
    [precioAlumno]      DECIMAL(10,2)   NOT NULL,
    [activo]            BIT             NOT NULL DEFAULT 1,
    [dvv]               VARCHAR(50)     NOT NULL,
    [dvh]               VARCHAR(50)     NOT NULL,
    CONSTRAINT [PK_Actividades] PRIMARY KEY CLUSTERED ([codActividad] ASC)
) ON [PRIMARY]
GO

-- ============================================================================
-- TABLA: Ejercicios
-- ============================================================================

CREATE TABLE [dbo].[Ejercicio](
    [codEjercicio]      INT             IDENTITY(1,1) NOT NULL,
    [nombre]            VARCHAR(100)    NOT NULL,
    [grupoMuscular]     VARCHAR(100)    NOT NULL,
    [descripcion]       VARCHAR(500)    NULL,
    [dvv]               VARCHAR(50)     NOT NULL,
    [dvh]               VARCHAR(50)     NOT NULL,
    CONSTRAINT [PK_Ejercicio] PRIMARY KEY CLUSTERED ([codEjercicio] ASC),
    CONSTRAINT [UK_Ejercicio_Nombre] UNIQUE NONCLUSTERED ([nombre] ASC)
) ON [PRIMARY]
GO

-- ============================================================================
-- TABLA: Rutinas
-- ============================================================================

CREATE TABLE [dbo].[Rutinas](
    [codRutina]        INT             IDENTITY(1,1) NOT NULL,
    [descripcion]       VARCHAR(500)    NOT NULL,
    [fecha]            DATE            NOT NULL,
    [dniAlumno]        INT             NOT NULL,
    [dniEntrenador]    INT             NOT NULL,
    [codActividad]     INT             NOT NULL,
    [dvv]              VARCHAR(50)     NOT NULL,
    [dvh]              VARCHAR(50)     NOT NULL,
    CONSTRAINT [PK_Rutinas] PRIMARY KEY CLUSTERED ([codRutina] ASC),
    CONSTRAINT [FK_Rutinas_Alumno] FOREIGN KEY ([dniAlumno]) REFERENCES [dbo].[ALUMNOS] ([dni]),
    CONSTRAINT [FK_Rutinas_Entrenador] FOREIGN KEY ([dniEntrenador]) REFERENCES [dbo].[ENTRENADORES] ([dni]),
    CONSTRAINT [FK_Rutinas_Actividad] FOREIGN KEY ([codActividad]) REFERENCES [dbo].[Actividades] ([codActividad])
) ON [PRIMARY]
GO

-- ============================================================================
-- TABLA: RutinaEjercicio (detalle de ejercicios por rutina)
-- ============================================================================

CREATE TABLE [dbo].[RutinaEjercicio](
    [codRutinaEjercicio]   INT             IDENTITY(1,1) NOT NULL,
    [codRutina]            INT             NOT NULL,
    [codEjercicio]         INT             NOT NULL,
    [series]               INT             NOT NULL,
    [repeticiones]         INT             NOT NULL,
    [pesoLevantado]        DECIMAL(6,2)    NOT NULL,
    [porcentaje1RM]        DECIMAL(5,2)    NULL,
    [descansoSegundos]     INT             NULL,
    [dvv]                  VARCHAR(50)     NOT NULL,
    [dvh]                  VARCHAR(50)     NOT NULL,
    [volumen]              AS ((([series]*[repeticiones])*[pesoLevantado])),
    CONSTRAINT [PK_RutinaEjercicio] PRIMARY KEY CLUSTERED ([codRutinaEjercicio] ASC),
    CONSTRAINT [FK_RutinaEjercicio_Rutina] FOREIGN KEY ([codRutina]) REFERENCES [dbo].[Rutinas] ([codRutina]),
    CONSTRAINT [FK_RutinaEjercicio_Ejercicio] FOREIGN KEY ([codEjercicio]) REFERENCES [dbo].[Ejercicio] ([codEjercicio]),
    CONSTRAINT [CK_RutinaEjercicio_Series] CHECK (([series] > 0)),
    CONSTRAINT [CK_RutinaEjercicio_Repeticiones] CHECK (([repeticiones] > 0)),
    CONSTRAINT [CK_RutinaEjercicio_Peso] CHECK (([pesoLevantado] >= 0)),
    CONSTRAINT [CK_RutinaEjercicio_Porcentaje] CHECK (([porcentaje1RM] IS NULL OR ([porcentaje1RM] > 0 AND [porcentaje1RM] <= 100))),
    CONSTRAINT [CK_RutinaEjercicio_Descanso] CHECK (([descansoSegundos] IS NULL OR [descansoSegundos] > 0))
) ON [PRIMARY]
GO

-- ============================================================================
-- TABLA: AlumnoRM (registro de 1RM por alumno)
-- ============================================================================

CREATE TABLE [dbo].[AlumnoRM](
    [codRM]           INT             IDENTITY(1,1) NOT NULL,
    [dni]             INT             NOT NULL,
    [codEjercicio]    INT             NOT NULL,
    [pesoRM]          DECIMAL(6,2)    NOT NULL,
    [fechaRM]         DATE            NOT NULL,
    [dvv]             VARCHAR(50)     NOT NULL,
    [dvh]             VARCHAR(50)     NOT NULL,
    CONSTRAINT [PK_AlumnoRM] PRIMARY KEY CLUSTERED ([codRM] ASC),
    CONSTRAINT [UK_AlumnoRM_DniEjercicioFecha] UNIQUE NONCLUSTERED ([dni] ASC, [codEjercicio] ASC, [fechaRM] ASC),
    CONSTRAINT [FK_AlumnoRM_Alumno] FOREIGN KEY ([dni]) REFERENCES [dbo].[ALUMNOS] ([dni]),
    CONSTRAINT [FK_AlumnoRM_Ejercicio] FOREIGN KEY ([codEjercicio]) REFERENCES [dbo].[Ejercicio] ([codEjercicio]),
    CONSTRAINT [CK_AlumnoRM_Peso] CHECK (([pesoRM] > 0))
) ON [PRIMARY]
GO

-- ============================================================================
-- TABLA: PesoHistorial (histórico de peso de alumnos)
-- ============================================================================

CREATE TABLE [dbo].[PesoHistorial](
    [codPeso]     INT             IDENTITY(1,1) NOT NULL,
    [dni]         INT             NOT NULL,
    [peso]        DECIMAL(5,2)    NOT NULL,
    [fecha]       DATE            NOT NULL,
    [dvv]         VARCHAR(50)     NOT NULL,
    [dvh]         VARCHAR(50)     NOT NULL,
    CONSTRAINT [PK_PesoHistorial] PRIMARY KEY CLUSTERED ([codPeso] ASC),
    CONSTRAINT [FK_PesoHistorial_Alumno] FOREIGN KEY ([dni]) REFERENCES [dbo].[ALUMNOS] ([dni]),
    CONSTRAINT [CK_PesoHistorial_Peso] CHECK (([peso] > 0 AND [peso] < 500))
) ON [PRIMARY]
GO

-- ============================================================================
-- TABLA: Actividad_Alumno (alumnos inscriptos en actividades)
-- ============================================================================

CREATE TABLE [dbo].[Actividad_Alumno](
    [dni]           INT         NOT NULL,
    [codActividad]  INT         NOT NULL,
    [dvv]           VARCHAR(50) NOT NULL,
    [dvh]           VARCHAR(50) NOT NULL,
    CONSTRAINT [PK_Actividad_Alumno] PRIMARY KEY CLUSTERED ([dni] ASC, [codActividad] ASC),
    CONSTRAINT [FK_ActividadAlumno_Alumno] FOREIGN KEY ([dni]) REFERENCES [dbo].[ALUMNOS] ([dni]),
    CONSTRAINT [FK_ActividadAlumno_Actividad] FOREIGN KEY ([codActividad]) REFERENCES [dbo].[Actividades] ([codActividad])
) ON [PRIMARY]
GO

-- ============================================================================
-- TABLA: Actividad_Entrenador (entrenadores asignados a actividades)
-- ============================================================================

CREATE TABLE [dbo].[Actividad_Entrenador](
    [codActividad]      INT         NOT NULL,
    [dniEntrenador]     INT         NOT NULL,
    [dvv]               VARCHAR(50) NOT NULL,
    [dvh]               VARCHAR(50) NOT NULL,
    CONSTRAINT [PK_Actividad_Entrenador] PRIMARY KEY CLUSTERED ([codActividad] ASC, [dniEntrenador] ASC),
    CONSTRAINT [FK_ActividadEntrenador_Actividad] FOREIGN KEY ([codActividad]) REFERENCES [dbo].[Actividades] ([codActividad]),
    CONSTRAINT [FK_ActividadEntrenador_Entrenador] FOREIGN KEY ([dniEntrenador]) REFERENCES [dbo].[ENTRENADORES] ([dni])
) ON [PRIMARY]
GO

-- ============================================================================
-- TABLA: Evento (log de auditoría)
-- ============================================================================

CREATE TABLE [dbo].[Evento](
    [codEvento]     INT             IDENTITY(1,1) NOT NULL,
    [usr]           VARCHAR(50)     NULL,
    [fecha]         DATETIME        NOT NULL DEFAULT GETDATE(),
    [modulo]        VARCHAR(100)    NULL,
    [descripcion]   VARCHAR(500)    NULL,
    [criticidad]    INT             NULL,
    [tipo]          VARCHAR(256)    NULL,
    [dvv]           VARCHAR(50)     NULL,
    [dvh]           VARCHAR(256)    NULL,
    CONSTRAINT [PK_Evento] PRIMARY KEY CLUSTERED ([codEvento] ASC),
    CONSTRAINT [FK_Evento_Usuario] FOREIGN KEY ([usr]) REFERENCES [dbo].[USUARIOS] ([usr]),
    CONSTRAINT [CK_Evento_Criticidad] CHECK (([criticidad] = 1 OR [criticidad] = 2 OR [criticidad] = 3 OR [criticidad] = 4))
) ON [PRIMARY]
GO

CREATE NONCLUSTERED INDEX [IX_Evento_fecha] ON [dbo].[Evento] ([fecha] ASC)
GO
CREATE NONCLUSTERED INDEX [IX_Evento_usr] ON [dbo].[Evento] ([usr] ASC)
GO

-- ============================================================================
-- TABLA: Familia (grupos de permisos)
-- ============================================================================

CREATE TABLE [dbo].[Familia](
    [familiaId]     INT             IDENTITY(1,1) NOT NULL,
    [nombre]        VARCHAR(100)    NOT NULL,
    [profundidad]   BIT             NOT NULL DEFAULT 0,
    [dvv]           VARCHAR(50)     NOT NULL,
    [dvh]           VARCHAR(50)     NOT NULL,
    CONSTRAINT [PK_Familia] PRIMARY KEY CLUSTERED ([familiaId] ASC)
) ON [PRIMARY]
GO

-- ============================================================================
-- TABLA: Permiso
-- ============================================================================

CREATE TABLE [dbo].[Permiso](
    [permisoId]     INT             IDENTITY(1,1) NOT NULL,
    [nombre]        VARCHAR(100)    NOT NULL,
    [dvv]           VARCHAR(50)     NOT NULL,
    [dvh]           VARCHAR(50)     NOT NULL,
    CONSTRAINT [PK_Permiso] PRIMARY KEY CLUSTERED ([permisoId] ASC),
    CONSTRAINT [UK_Permiso_Nombre] UNIQUE NONCLUSTERED ([nombre] ASC)
) ON [PRIMARY]
GO

-- ============================================================================
-- TABLAS INTERMEDIAS DE PERMISOS
-- ============================================================================

CREATE TABLE [dbo].[PermisoFamilia](
    [permisoId]     INT         NOT NULL,
    [familiaId]     INT         NOT NULL,
    [dvv]           VARCHAR(50) NOT NULL,
    [dvh]           VARCHAR(50) NOT NULL,
    CONSTRAINT [PK_PermisoFamilia] PRIMARY KEY CLUSTERED ([permisoId] ASC, [familiaId] ASC),
    CONSTRAINT [FK_PermisoFamilia_Permiso] FOREIGN KEY ([permisoId]) REFERENCES [dbo].[Permiso] ([permisoId]),
    CONSTRAINT [FK_PermisoFamilia_Familia] FOREIGN KEY ([familiaId]) REFERENCES [dbo].[Familia] ([familiaId])
) ON [PRIMARY]
GO

CREATE TABLE [dbo].[Perfil_Familia](
    [idPerfil]      INT         NOT NULL,
    [idFamilia]     INT         NOT NULL,
    [dvv]           VARCHAR(50) NOT NULL,
    [dvh]           VARCHAR(50) NOT NULL,
    CONSTRAINT [PK_Perfil_Familia] PRIMARY KEY CLUSTERED ([idPerfil] ASC, [idFamilia] ASC),
    CONSTRAINT [FK_PerfilFamilia_Perfil] FOREIGN KEY ([idPerfil]) REFERENCES [dbo].[Perfiles] ([idPerfil]),
    CONSTRAINT [FK_PerfilFamilia_Familia] FOREIGN KEY ([idFamilia]) REFERENCES [dbo].[Familia] ([familiaId])
) ON [PRIMARY]
GO

CREATE TABLE [dbo].[Perfil_Permiso](
    [idPerfil]      INT         NOT NULL,
    [idPermiso]     INT         NOT NULL,
    [dvv]           VARCHAR(50) NOT NULL,
    [dvh]           VARCHAR(50) NOT NULL,
    CONSTRAINT [PK_Perfil_Permiso] PRIMARY KEY CLUSTERED ([idPerfil] ASC, [idPermiso] ASC),
    CONSTRAINT [FK_PerfilPermiso_Perfil] FOREIGN KEY ([idPerfil]) REFERENCES [dbo].[Perfiles] ([idPerfil]),
    CONSTRAINT [FK_PerfilPermiso_Permiso] FOREIGN KEY ([idPermiso]) REFERENCES [dbo].[Permiso] ([permisoId])
) ON [PRIMARY]
GO

-- ============================================================================
-- TABLA: PrecioModalidad (tipos de cuota mensual)
-- ============================================================================

CREATE TABLE [dbo].[PrecioModalidad](
    [Id]                INT             IDENTITY(1,1) NOT NULL,
    [DiasPorSemana]     INT             NOT NULL,
    [EsDiario]          BIT             NOT NULL,
    [Precio]            DECIMAL(10,2)   NOT NULL,
    [Activo]            BIT             NOT NULL DEFAULT 1,
    [FechaModificacion] DATETIME        NOT NULL DEFAULT GETDATE(),
    [dvv]               VARCHAR(50)     NOT NULL,
    [dvh]               VARCHAR(50)     NOT NULL,
    CONSTRAINT [PK_PrecioModalidad] PRIMARY KEY CLUSTERED ([Id] ASC),
    CONSTRAINT [CK_PrecioModalidad_Dias] CHECK (
        ([EsDiario] = 1 AND [DiasPorSemana] = 0)
        OR ([EsDiario] = 0 AND ([DiasPorSemana] = 1 OR [DiasPorSemana] = 2 OR [DiasPorSemana] = 3))
    ),
    CONSTRAINT [CK_PrecioModalidad_Precio] CHECK (([Precio] > 0))
) ON [PRIMARY]
GO

-- ============================================================================
-- DATOS INICIALES: PrecioModalidad
-- ============================================================================

INSERT INTO [dbo].[PrecioModalidad] ([DiasPorSemana], [EsDiario], [Precio], [Activo], [FechaModificacion], [dvv], [dvh])
VALUES
    (1, 0, 25000.00, 1, GETDATE(), '', ''),  -- 1 día por semana
    (2, 0, 29000.00, 1, GETDATE(), '', ''),  -- 2 días por semana
    (3, 0, 33000.00, 1, GETDATE(), '', ''),  -- 3 días por semana
    (0, 1, 44000.00, 1, GETDATE(), '', '')   -- Diario (todos los días)
GO

-- ============================================================================
-- ÍNDICES ADICIONALES
-- ============================================================================

CREATE NONCLUSTERED INDEX [IX_USUARIOS_Tipo] ON [dbo].[USUARIOS] ([tipo] ASC)
GO
CREATE NONCLUSTERED INDEX [IX_USUARIOS_DNI] ON [dbo].[USUARIOS] ([dni] ASC)
GO

CREATE NONCLUSTERED INDEX [IX_Alumnos_dni] ON [dbo].[ALUMNOS] ([dni] ASC)
GO

CREATE NONCLUSTERED INDEX [IX_Entrenadores_dni] ON [dbo].[ENTRENADORES] ([dni] ASC)
GO

CREATE NONCLUSTERED INDEX [IX_Actividad_Alumno_codActividad] ON [dbo].[Actividad_Alumno] ([codActividad] ASC)
GO

CREATE NONCLUSTERED INDEX [IX_Actividad_Entrenador_codActividad] ON [dbo].[Actividad_Entrenador] ([codActividad] ASC)
GO

CREATE NONCLUSTERED INDEX [IX_AlumnoRM_Alumno] ON [dbo].[AlumnoRM] ([dni] ASC)
GO
CREATE NONCLUSTERED INDEX [IX_AlumnoRM_Ejercicio] ON [dbo].[AlumnoRM] ([codEjercicio] ASC)
GO

CREATE NONCLUSTERED INDEX [IX_PesoHistorial_Alumno] ON [dbo].[PesoHistorial] ([dni] ASC)
GO
CREATE NONCLUSTERED INDEX [IX_PesoHistorial_Fecha] ON [dbo].[PesoHistorial] ([fecha] ASC)
GO

CREATE NONCLUSTERED INDEX [IX_Rutinas_dniAlumno] ON [dbo].[Rutinas] ([dniAlumno] ASC)
GO
CREATE NONCLUSTERED INDEX [IX_Rutinas_dniEntrenador] ON [dbo].[Rutinas] ([dniEntrenador] ASC)
GO
CREATE NONCLUSTERED INDEX [IX_Rutinas_fecha] ON [dbo].[Rutinas] ([fecha] ASC)
GO

CREATE NONCLUSTERED INDEX [IX_RutinaEjercicio_Rutina] ON [dbo].[RutinaEjercicio] ([codRutina] ASC)
GO
CREATE NONCLUSTERED INDEX [IX_RutinaEjercicio_Ejercicio] ON [dbo].[RutinaEjercicio] ([codEjercicio] ASC)
GO

-- ============================================================================
-- FIN DEL SCRIPT
-- ============================================================================

PRINT 'Base de datos GymApp v2 creada exitosamente';
GO
