USE [GymApp]
GO

-- ============================================================
-- Inscripción a clases por turno y por fecha.
--
-- InscripcionFija: el alumno va a TODAS las clases de un turno (ActividadHorario)
--   desde fechaDesde y, si tiene fechaHasta, hasta esa fecha inclusive.
--   Un alumno puede tener varias filas para el mismo turno (períodos que no se superponen).
--
-- InscripcionClase: excepciones puntuales sobre una fecha concreta de un turno.
--   tipo 'A' (Alta): va a esa clase aunque no tenga inscripción fija.
--   tipo 'B' (Baja): tiene inscripción fija pero esa fecha no va.
--
-- Un alumno va a la clase del turno H en la fecha F si:
--   (tiene una InscripcionFija de H que cubre F y no hay Baja de H en F) o hay Alta de H en F.
--
-- Reemplaza a Actividad_Alumno (inscripción a toda la actividad sin fecha). Las filas
-- existentes se migran como inscripción fija a todos los turnos de la actividad, desde
-- el primer día del mes actual. Actividad_Alumno queda sin uso (no se borra).
--
-- Después de correr este script:
--   VerificacioDV -> "Inicializar control" y luego "Recalcular dígitos".
-- ============================================================

IF OBJECT_ID('[dbo].[InscripcionFija]', 'U') IS NULL
BEGIN
    CREATE TABLE [dbo].[InscripcionFija](
        [codInscripcion] INT          IDENTITY(1,1) NOT NULL,
        [dni]            INT          NOT NULL,
        [codHorario]     INT          NOT NULL,
        [fechaDesde]     DATE         NOT NULL,
        [fechaHasta]     DATE         NULL,
        [dvh]            VARCHAR(64)  NOT NULL,
        CONSTRAINT [PK_InscripcionFija] PRIMARY KEY CLUSTERED ([codInscripcion] ASC),
        CONSTRAINT [FK_InscripcionFija_Alumno] FOREIGN KEY ([dni])
            REFERENCES [dbo].[ALUMNOS] ([dni]),
        CONSTRAINT [FK_InscripcionFija_Horario] FOREIGN KEY ([codHorario])
            REFERENCES [dbo].[ActividadHorario] ([codHorario]),
        CONSTRAINT [CK_InscripcionFija_Fechas] CHECK ([fechaHasta] IS NULL OR [fechaHasta] >= [fechaDesde])
    ) ON [PRIMARY];

    CREATE INDEX [IX_InscripcionFija_Horario] ON [dbo].[InscripcionFija] ([codHorario]);
    CREATE INDEX [IX_InscripcionFija_Alumno] ON [dbo].[InscripcionFija] ([dni]);

    PRINT 'Tabla InscripcionFija creada.';
END
ELSE
    PRINT 'La tabla InscripcionFija ya existe.';
GO

IF OBJECT_ID('[dbo].[InscripcionClase]', 'U') IS NULL
BEGIN
    CREATE TABLE [dbo].[InscripcionClase](
        [codInscripcionClase] INT          IDENTITY(1,1) NOT NULL,
        [dni]                 INT          NOT NULL,
        [codHorario]          INT          NOT NULL,
        [fecha]               DATE         NOT NULL,
        [tipo]                CHAR(1)      NOT NULL,
        [dvh]                 VARCHAR(64)  NOT NULL,
        CONSTRAINT [PK_InscripcionClase] PRIMARY KEY CLUSTERED ([codInscripcionClase] ASC),
        CONSTRAINT [UQ_InscripcionClase] UNIQUE ([dni], [codHorario], [fecha]),
        CONSTRAINT [FK_InscripcionClase_Alumno] FOREIGN KEY ([dni])
            REFERENCES [dbo].[ALUMNOS] ([dni]),
        CONSTRAINT [FK_InscripcionClase_Horario] FOREIGN KEY ([codHorario])
            REFERENCES [dbo].[ActividadHorario] ([codHorario]),
        CONSTRAINT [CK_InscripcionClase_Tipo] CHECK ([tipo] IN ('A', 'B'))
    ) ON [PRIMARY];

    CREATE INDEX [IX_InscripcionClase_HorarioFecha] ON [dbo].[InscripcionClase] ([codHorario], [fecha]);

    PRINT 'Tabla InscripcionClase creada.';
END
ELSE
    PRINT 'La tabla InscripcionClase ya existe.';
GO

-- Migración: cada inscripción a una actividad pasa a ser inscripción fija a todos sus turnos.
IF OBJECT_ID('[dbo].[Actividad_Alumno]', 'U') IS NOT NULL
   AND NOT EXISTS (SELECT 1 FROM [dbo].[InscripcionFija])
BEGIN
    DECLARE @inicioMes DATE = DATEFROMPARTS(YEAR(GETDATE()), MONTH(GETDATE()), 1);

    INSERT INTO [dbo].[InscripcionFija] ([dni], [codHorario], [fechaDesde], [fechaHasta], [dvh])
    SELECT aa.[dni], h.[codHorario], @inicioMes, NULL, ''
    FROM [dbo].[Actividad_Alumno] aa
    INNER JOIN [dbo].[ActividadHorario] h ON h.[codActividad] = aa.[codActividad];

    PRINT CONCAT('Inscripciones migradas: ', @@ROWCOUNT);
END
GO
