USE [GymApp]
GO

-- ============================================================
-- Horarios semanales de las actividades.
-- Una actividad puede tener varios turnos por semana (ej. Yoga lunes 08:00
-- y miércoles 18:00). Actividades.cantXSemana pasa a ser la cantidad de
-- turnos cargados (la mantiene la aplicación).
--
-- diaSemana: 1 = Lunes ... 7 = Domingo.
--
-- Después de correr este script:
--   VerificacioDV -> "Inicializar control" para que ActividadHorario quede
--   bajo control de integridad (se detecta sola por tener columna dvh).
-- ============================================================

IF OBJECT_ID('[dbo].[ActividadHorario]', 'U') IS NULL
BEGIN
    CREATE TABLE [dbo].[ActividadHorario](
        [codHorario]    INT          IDENTITY(1,1) NOT NULL,
        [codActividad]  INT          NOT NULL,
        [diaSemana]     TINYINT      NOT NULL,
        [horaInicio]    TIME(0)      NOT NULL,
        [duracionMin]   INT          NOT NULL,
        [dvh]           VARCHAR(64)  NOT NULL,
        CONSTRAINT [PK_ActividadHorario] PRIMARY KEY CLUSTERED ([codHorario] ASC),
        CONSTRAINT [FK_ActividadHorario_Actividad] FOREIGN KEY ([codActividad])
            REFERENCES [dbo].[Actividades] ([codActividad]),
        CONSTRAINT [CK_ActividadHorario_Dia] CHECK ([diaSemana] BETWEEN 1 AND 7),
        CONSTRAINT [CK_ActividadHorario_Duracion] CHECK ([duracionMin] BETWEEN 1 AND 600)
    ) ON [PRIMARY];

    CREATE INDEX [IX_ActividadHorario_Actividad] ON [dbo].[ActividadHorario] ([codActividad]);

    PRINT 'Tabla ActividadHorario creada.';
END
ELSE
    PRINT 'La tabla ActividadHorario ya existe.';
GO
