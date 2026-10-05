USE [GymApp]
GO

-- ============================================================
-- Arranca las clases de cero y agrega aulas.
--
-- 1) Vacía las clases: inscripciones (fijas y puntuales), inscripciones viejas
--    (Actividad_Alumno), rutinas y sus ejercicios, profesores asignados, horarios y actividades.
--    NO borra usuarios, alumnos, profesores, pagos ni la bitácora.
--    Todo el borrado va en una transacción: si algo falla no se borra nada.
--
-- 2) Crea la tabla Aulas (nombre + cupo) con dos aulas de ejemplo.
--
-- 3) Cada turno (ActividadHorario) pasa a tener aula, profesor titular (obligatorios)
--    y profesor auxiliar (opcional). Actividad_Entrenador queda sin uso.
--
-- 4) Actividades.cupoMaximo: tope opcional de alumnos por clase. El cupo de cada clase es
--    el del aula, o el tope de la actividad si es menor. (Reemplaza a la columna "cupo" si existía.)
--
-- 5) Permiso nuevo GestionAulas para el Administrador.
--
-- Se puede correr aunque ya se haya corrido vaciar-clases-y-agregar-cupo.sql.
--
-- Después de correr este script:
--   VerificacioDV -> "Inicializar control" (para la tabla nueva Aulas) y luego "Recalcular dígitos".
-- ============================================================

-- ---------- 1) Vaciar clases ----------
SET XACT_ABORT ON;
BEGIN TRANSACTION;

SELECT 'Actividades' AS Tabla, COUNT(*) AS Filas FROM [dbo].[Actividades]
UNION ALL SELECT 'ActividadHorario', COUNT(*) FROM [dbo].[ActividadHorario]
UNION ALL SELECT 'Actividad_Entrenador', COUNT(*) FROM [dbo].[Actividad_Entrenador]
UNION ALL SELECT 'Rutinas', COUNT(*) FROM [dbo].[Rutinas];

IF OBJECT_ID('[dbo].[InscripcionClase]', 'U') IS NOT NULL DELETE FROM [dbo].[InscripcionClase];
IF OBJECT_ID('[dbo].[InscripcionFija]', 'U') IS NOT NULL DELETE FROM [dbo].[InscripcionFija];
IF OBJECT_ID('[dbo].[Actividad_Alumno]', 'U') IS NOT NULL DELETE FROM [dbo].[Actividad_Alumno];
IF OBJECT_ID('[dbo].[RutinaEjercicio]', 'U') IS NOT NULL DELETE FROM [dbo].[RutinaEjercicio];
IF OBJECT_ID('[dbo].[Rutinas]', 'U') IS NOT NULL DELETE FROM [dbo].[Rutinas];
IF OBJECT_ID('[dbo].[Actividad_Entrenador]', 'U') IS NOT NULL DELETE FROM [dbo].[Actividad_Entrenador];
IF OBJECT_ID('[dbo].[ActividadHorario]', 'U') IS NOT NULL DELETE FROM [dbo].[ActividadHorario];
DELETE FROM [dbo].[Actividades];

COMMIT TRANSACTION;
GO

-- Los códigos vuelven a empezar desde 1
IF OBJECT_ID('[dbo].[InscripcionClase]', 'U') IS NOT NULL DBCC CHECKIDENT ('[dbo].[InscripcionClase]', RESEED, 0);
IF OBJECT_ID('[dbo].[InscripcionFija]', 'U') IS NOT NULL DBCC CHECKIDENT ('[dbo].[InscripcionFija]', RESEED, 0);
IF OBJECT_ID('[dbo].[RutinaEjercicio]', 'U') IS NOT NULL DBCC CHECKIDENT ('[dbo].[RutinaEjercicio]', RESEED, 0);
IF OBJECT_ID('[dbo].[Rutinas]', 'U') IS NOT NULL DBCC CHECKIDENT ('[dbo].[Rutinas]', RESEED, 0);
IF OBJECT_ID('[dbo].[ActividadHorario]', 'U') IS NOT NULL DBCC CHECKIDENT ('[dbo].[ActividadHorario]', RESEED, 0);
DBCC CHECKIDENT ('[dbo].[Actividades]', RESEED, 0);
GO

-- ---------- 2) Aulas ----------
IF OBJECT_ID('[dbo].[Aulas]', 'U') IS NULL
BEGIN
    CREATE TABLE [dbo].[Aulas](
        [codAula]  INT           IDENTITY(1,1) NOT NULL,
        [nombre]   VARCHAR(100)  NOT NULL,
        [cupo]     INT           NOT NULL,
        [activo]   BIT           NOT NULL CONSTRAINT [DF_Aulas_activo] DEFAULT (1),
        [dvh]      VARCHAR(64)   NOT NULL,
        CONSTRAINT [PK_Aulas] PRIMARY KEY CLUSTERED ([codAula] ASC),
        CONSTRAINT [UQ_Aulas_nombre] UNIQUE ([nombre]),
        CONSTRAINT [CK_Aulas_cupo] CHECK ([cupo] BETWEEN 1 AND 1000)
    ) ON [PRIMARY];

    INSERT INTO [dbo].[Aulas] ([nombre], [cupo], [activo], [dvh]) VALUES
        ('Salón Principal', 100, 1, ''),
        ('Salón de Pilates', 20, 1, '');

    PRINT 'Tabla Aulas creada con 2 aulas de ejemplo.';
END
ELSE
    PRINT 'La tabla Aulas ya existe.';
GO

-- ---------- 3) Aula y profesores por turno (la tabla quedó vacía en el paso 1) ----------
IF COL_LENGTH('dbo.ActividadHorario', 'codAula') IS NULL
BEGIN
    ALTER TABLE [dbo].[ActividadHorario] ADD
        [codAula]     INT NOT NULL,
        [dniTitular]  INT NOT NULL,
        [dniAuxiliar] INT NULL;

    PRINT 'Columnas codAula, dniTitular y dniAuxiliar agregadas a ActividadHorario.';
END
GO

IF OBJECT_ID('[dbo].[FK_ActividadHorario_Aula]', 'F') IS NULL
    ALTER TABLE [dbo].[ActividadHorario] ADD
        CONSTRAINT [FK_ActividadHorario_Aula] FOREIGN KEY ([codAula]) REFERENCES [dbo].[Aulas] ([codAula]),
        CONSTRAINT [FK_ActividadHorario_Titular] FOREIGN KEY ([dniTitular]) REFERENCES [dbo].[ENTRENADORES] ([dni]),
        CONSTRAINT [FK_ActividadHorario_Auxiliar] FOREIGN KEY ([dniAuxiliar]) REFERENCES [dbo].[ENTRENADORES] ([dni]),
        CONSTRAINT [CK_ActividadHorario_Profesores] CHECK ([dniAuxiliar] IS NULL OR [dniAuxiliar] <> [dniTitular]);
GO

-- ---------- 4) Tope de cupo opcional por actividad ----------
IF COL_LENGTH('dbo.Actividades', 'cupo') IS NOT NULL
BEGIN
    IF OBJECT_ID('[dbo].[CK_Actividades_cupo]', 'C') IS NOT NULL ALTER TABLE [dbo].[Actividades] DROP CONSTRAINT [CK_Actividades_cupo];
    IF OBJECT_ID('[dbo].[DF_Actividades_cupo]', 'D') IS NOT NULL ALTER TABLE [dbo].[Actividades] DROP CONSTRAINT [DF_Actividades_cupo];
    ALTER TABLE [dbo].[Actividades] DROP COLUMN [cupo];
    PRINT 'Columna Actividades.cupo eliminada (reemplazada por cupoMaximo).';
END
GO

IF COL_LENGTH('dbo.Actividades', 'cupoMaximo') IS NULL
BEGIN
    ALTER TABLE [dbo].[Actividades] ADD [cupoMaximo] INT NULL
        CONSTRAINT [CK_Actividades_cupoMaximo] CHECK ([cupoMaximo] IS NULL OR [cupoMaximo] BETWEEN 1 AND 1000);
    PRINT 'Columna Actividades.cupoMaximo agregada.';
END
GO

-- ---------- 5) Permiso GestionAulas para el Administrador ----------
IF OBJECT_ID('[dbo].[RolPermiso]', 'U') IS NOT NULL
   AND NOT EXISTS (SELECT 1 FROM [dbo].[RolPermiso] WHERE [rol] = 1 AND [permiso] = 'GestionAulas')
BEGIN
    INSERT INTO [dbo].[RolPermiso] ([rol], [permiso], [dvh]) VALUES (1, 'GestionAulas', '');
    PRINT 'Permiso GestionAulas asignado al Administrador.';
END
GO
