USE [GymApp]
GO

-- ============================================================
-- Agrega la columna [usr] a ALUMNOS y ENTRENADORES.
--
-- Diagnóstico: MPPAlumno.cs y MPPEntrenador.cs leen/escriben "a.usr" / "e.usr"
-- desde hace tiempo (feature "Asociar Usuario": vincula un alumno o entrenador
-- ya cargado por DNI con una cuenta de login de rol Cliente/Entrenador, que
-- puede pertenecer a otra persona, ej. el padre/tutor de un alumno menor).
-- Esa columna nunca se agregó a bd-schema-v2.sql ni a ningún script de
-- migración, así que cualquier base creada desde ese script no la tiene.
-- Resultado: toda consulta a Alumnos/Entrenadores (listar, crear, asociar)
-- falla con "Invalid column name 'usr'", que la UI muestra como el genérico
-- "Ocurrió un error inesperado".
--
-- [dni] sigue siendo la identidad propia del alumno/entrenador (FK a
-- USUARIOS.dni, datos personales). [usr] es opcional y aparte: la cuenta de
-- login (USUARIOS.usr) asociada para portal/consulta, puede quedar NULL
-- ("sin usuario").
-- ============================================================

IF COL_LENGTH('[GymApp].[dbo].[ALUMNOS]', 'usr') IS NULL
BEGIN
    ALTER TABLE [dbo].[ALUMNOS] ADD [usr] VARCHAR(50) NULL;

    ALTER TABLE [dbo].[ALUMNOS] WITH CHECK ADD CONSTRAINT [FK_ALUMNOS_Usr_USUARIOS]
        FOREIGN KEY ([usr]) REFERENCES [dbo].[USUARIOS] ([usr]);

    PRINT 'Columna ALUMNOS.usr agregada.';
END
ELSE
BEGIN
    PRINT 'ALUMNOS.usr ya existe, no se modifica.';
END
GO

IF COL_LENGTH('[GymApp].[dbo].[ENTRENADORES]', 'usr') IS NULL
BEGIN
    ALTER TABLE [dbo].[ENTRENADORES] ADD [usr] VARCHAR(50) NULL;

    ALTER TABLE [dbo].[ENTRENADORES] WITH CHECK ADD CONSTRAINT [FK_ENTRENADORES_Usr_USUARIOS]
        FOREIGN KEY ([usr]) REFERENCES [dbo].[USUARIOS] ([usr]);

    PRINT 'Columna ENTRENADORES.usr agregada.';
END
ELSE
BEGIN
    PRINT 'ENTRENADORES.usr ya existe, no se modifica.';
END
GO
