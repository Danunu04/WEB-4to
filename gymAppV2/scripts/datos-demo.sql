USE [GymApp]
GO

-- ============================================================================
-- Datos ficticios para la demo (16/11/2026)
--
-- Llena las tablas que estaban vacías para que ninguna pantalla quede sin datos:
--   Actividades + ActividadHorario (5 actividades nuevas con sus turnos),
--   Pagos, InscripcionFija, InscripcionClase, Rutinas, RutinaEjercicio,
--   AlumnoRM, PesoHistorial, AvisoPrecio, AvisoPrecioVisto y AlertaCheckin.
--
-- Usa los alumnos, entrenadores, aulas, modalidades y ejercicios que ya existen
-- (no crea usuarios: sus datos van encriptados y eso lo tiene que hacer la app).
-- Las fechas son relativas a hoy, así la demo siempre muestra abonos al día,
-- uno vencido y clases próximas.
--
-- Cada tabla se llena solo si está vacía: se puede volver a correr sin duplicar.
-- No se tocan Actividad_Alumno ni Actividad_Entrenador (tablas viejas, sin uso)
-- ni USUARIO_Intentos (obsoleta).
--
-- Antes de correrlo: hacer un backup.
-- Después de correrlo: VerificacioDV -> "Recalcular dígitos" (las filas nuevas
-- quedan con dvh vacío y ALUMNOS cambia por el peso).
-- ============================================================================

-- ---------- Control previo: columnas que agregan otros scripts ----------
-- Va en un lote aparte: si falta una columna, el lote de abajo ni siquiera compila.
DECLARE @Faltan NVARCHAR(2000) = N'';

IF COL_LENGTH('dbo.Pagos', 'fechaInicio') IS NULL OR COL_LENGTH('dbo.Pagos', 'fechaVencimiento') IS NULL
    SET @Faltan += NCHAR(10) + N'  - scripts/abono-mensual.sql (Pagos.fechaInicio y fechaVencimiento)';
IF COL_LENGTH('dbo.RutinaEjercicio', 'orden') IS NULL OR COL_LENGTH('dbo.RutinaEjercicio', 'observaciones') IS NULL
    SET @Faltan += NCHAR(10) + N'  - scripts/rutinas-detalladas.sql (RutinaEjercicio.orden y observaciones)';
IF COL_LENGTH('dbo.AlumnoRM', 'pesoLevantado') IS NULL OR COL_LENGTH('dbo.AlumnoRM', 'usuarioRegistro') IS NULL
    SET @Faltan += NCHAR(10) + N'  - scripts/calculo-rm.sql (AlumnoRM.pesoLevantado y usuarioRegistro)';
IF COL_LENGTH('dbo.ActividadHorario', 'codAula') IS NULL OR COL_LENGTH('dbo.Actividades', 'cupoMaximo') IS NULL
    SET @Faltan += NCHAR(10) + N'  - scripts/vaciar-clases-y-crear-aulas.sql (ActividadHorario.codAula y Actividades.cupoMaximo)';
IF OBJECT_ID('dbo.PesoHistorial', 'U') IS NULL OR OBJECT_ID('dbo.AlertaCheckin', 'U') IS NULL OR OBJECT_ID('dbo.AvisoPrecio', 'U') IS NULL
    SET @Faltan += NCHAR(10) + N'  - scripts/funcionalidades-faltantes.sql (PesoHistorial, AvisoPrecio, AlertaCheckin)';

IF @Faltan <> N''
BEGIN
    RAISERROR(N'Faltan scripts por correr antes de este (no se hizo ningún cambio):%s', 16, 1, @Faltan);
    SET NOEXEC ON;
END
GO

SET NOCOUNT ON;
SET XACT_ABORT ON;

DECLARE @Hoy DATE = CAST(GETDATE() AS DATE);

-- ---------- Datos existentes que se reutilizan ----------
DECLARE @A1 INT, @A2 INT, @A3 INT, @A4 INT;
SELECT @A1 = MAX(CASE WHEN n = 1 THEN dni END),
       @A2 = MAX(CASE WHEN n = 2 THEN dni END),
       @A3 = MAX(CASE WHEN n = 3 THEN dni END),
       @A4 = MAX(CASE WHEN n = 4 THEN dni END)
FROM (SELECT dni, ROW_NUMBER() OVER (ORDER BY dni) AS n FROM [dbo].[ALUMNOS] WHERE activo = 1) x;

DECLARE @E1 INT, @E2 INT;
SELECT @E1 = MAX(CASE WHEN n = 1 THEN dni END),
       @E2 = MAX(CASE WHEN n = 2 THEN dni END)
FROM (SELECT dni, ROW_NUMBER() OVER (ORDER BY dni) AS n FROM [dbo].[ENTRENADORES] WHERE activo = 1) x;

IF @A1 IS NULL OR @E1 IS NULL
BEGIN
    RAISERROR('Hace falta al menos un alumno y un entrenador activos. No se hizo ningún cambio.', 16, 1);
    RETURN;
END

DECLARE @Admin VARCHAR(50) = (SELECT TOP 1 usr FROM [dbo].[USUARIOS] WHERE rol = 1 AND activo = 1 ORDER BY usr);
DECLARE @Recep VARCHAR(50) = ISNULL((SELECT TOP 1 usr FROM [dbo].[USUARIOS] WHERE rol = 2 AND activo = 1 ORDER BY usr), @Admin);
DECLARE @UsrE1 VARCHAR(50) = ISNULL((SELECT usr FROM [dbo].[ENTRENADORES] WHERE dni = @E1), (SELECT usr FROM [dbo].[USUARIOS] WHERE dni = @E1));
DECLARE @UsrE2 VARCHAR(50) = ISNULL((SELECT usr FROM [dbo].[ENTRENADORES] WHERE dni = @E2), (SELECT usr FROM [dbo].[USUARIOS] WHERE dni = @E2));
DECLARE @UsrCliente VARCHAR(50) = (SELECT TOP 1 usr FROM [dbo].[ALUMNOS_USUARIOS] WHERE dniAlumno = @A1 ORDER BY usr);
SET @UsrE2 = ISNULL(@UsrE2, @UsrE1);

DECLARE @ModPrimera INT = (SELECT TOP 1 Id FROM [dbo].[PrecioModalidad] WHERE Activo = 1 ORDER BY Id);
DECLARE @Mod1 INT = ISNULL((SELECT TOP 1 Id FROM [dbo].[PrecioModalidad] WHERE Activo = 1 AND EsDiario = 0 AND DiasPorSemana = 1), @ModPrimera);
DECLARE @Mod2 INT = ISNULL((SELECT TOP 1 Id FROM [dbo].[PrecioModalidad] WHERE Activo = 1 AND EsDiario = 0 AND DiasPorSemana = 2), @ModPrimera);
DECLARE @Mod3 INT = ISNULL((SELECT TOP 1 Id FROM [dbo].[PrecioModalidad] WHERE Activo = 1 AND EsDiario = 0 AND DiasPorSemana = 3), @ModPrimera);

DECLARE @AulaPrincipal INT = ISNULL(
    (SELECT TOP 1 codAula FROM [dbo].[Aulas] WHERE activo = 1 AND nombre LIKE '%Principal%' ORDER BY codAula),
    (SELECT TOP 1 codAula FROM [dbo].[Aulas] WHERE activo = 1 ORDER BY codAula));
DECLARE @AulaPilates INT = ISNULL(
    (SELECT TOP 1 codAula FROM [dbo].[Aulas] WHERE activo = 1 AND nombre LIKE '%Pilates%' ORDER BY codAula),
    @AulaPrincipal);
DECLARE @AulaOtra INT = ISNULL(
    (SELECT TOP 1 codAula FROM [dbo].[Aulas] WHERE activo = 1 AND codAula NOT IN (@AulaPrincipal, @AulaPilates) ORDER BY codAula),
    @AulaPrincipal);

IF @Admin IS NULL OR @ModPrimera IS NULL OR @AulaPrincipal IS NULL
BEGIN
    RAISERROR('Falta un administrador activo, una modalidad activa o un aula activa. No se hizo ningún cambio.', 16, 1);
    RETURN;
END

PRINT CONCAT('Alumnos: ', @A1, ', ', @A2, ', ', @A3, ', ', @A4, ' | Entrenadores: ', @E1, ', ', @E2);

BEGIN TRANSACTION;

-- ---------- 1) Actividades ----------
INSERT INTO [dbo].[Actividades] (descripcion, cantXSemana, costoInterno, precioAlumno, activo, cupoMaximo, dvh)
SELECT v.descripcion, v.cant, v.costo, v.precio, 1, v.cupo, ''
FROM (VALUES
    ('Funcional', 3, 180000.00, 12000.00, 15),
    ('Pilates',   2, 220000.00, 15000.00, 10),
    ('Spinning',  2, 150000.00, 10000.00, 12),
    ('Yoga',      1, 120000.00,  9000.00, 15),
    ('Crossfit',  2, 200000.00, 14000.00, 12)
) v (descripcion, cant, costo, precio, cupo)
WHERE NOT EXISTS (SELECT 1 FROM [dbo].[Actividades] a WHERE a.descripcion = v.descripcion);
PRINT CONCAT('Actividades nuevas: ', @@ROWCOUNT);

-- ---------- 2) Turnos (diaSemana: 1 = lunes ... 7 = domingo) ----------
-- No se agrega un turno si se superpone con otro de la misma aula o del mismo profesor.
INSERT INTO [dbo].[ActividadHorario] (codActividad, diaSemana, horaInicio, duracionMin, codAula, dniTitular, dniAuxiliar, dvh)
SELECT a.codActividad, v.dia, v.hora, v.duracion, v.aula, v.titular,
       CASE WHEN v.auxiliar = v.titular THEN NULL ELSE v.auxiliar END, ''
FROM (VALUES
    ('Funcional', 1, CAST('08:00' AS TIME(0)), 60, @AulaPrincipal, @E1, @E2),
    ('Funcional', 3, CAST('08:00' AS TIME(0)), 60, @AulaPrincipal, @E1, @E2),
    ('Funcional', 5, CAST('08:00' AS TIME(0)), 60, @AulaPrincipal, @E1, @E2),
    ('Pilates',   2, CAST('18:00' AS TIME(0)), 50, @AulaPilates,   ISNULL(@E2, @E1), NULL),
    ('Pilates',   4, CAST('18:00' AS TIME(0)), 50, @AulaPilates,   ISNULL(@E2, @E1), NULL),
    ('Spinning',  1, CAST('19:00' AS TIME(0)), 45, @AulaOtra,      @E1, NULL),
    ('Spinning',  3, CAST('19:00' AS TIME(0)), 45, @AulaOtra,      @E1, NULL),
    ('Yoga',      6, CAST('10:00' AS TIME(0)), 60, @AulaPilates,   ISNULL(@E2, @E1), NULL),
    ('Crossfit',  2, CAST('07:00' AS TIME(0)), 60, @AulaPrincipal, @E1, @E2),
    ('Crossfit',  4, CAST('07:00' AS TIME(0)), 60, @AulaPrincipal, @E1, @E2)
) v (actividad, dia, hora, duracion, aula, titular, auxiliar)
JOIN [dbo].[Actividades] a ON a.descripcion = v.actividad
WHERE NOT EXISTS (
    SELECT 1 FROM [dbo].[ActividadHorario] h
    WHERE h.diaSemana = v.dia
      AND (h.codAula = v.aula OR h.dniTitular = v.titular)
      AND h.horaInicio < DATEADD(MINUTE, v.duracion, v.hora)
      AND v.hora < DATEADD(MINUTE, h.duracionMin, h.horaInicio));
PRINT CONCAT('Turnos nuevos: ', @@ROWCOUNT);

-- ---------- 3) Pagos (abonos de 30 días encadenados) ----------
-- A1: al día (3 meses) | A2: al día (2 meses) | A3: vencido hace 7 días | A4: nunca pagó
IF NOT EXISTS (SELECT 1 FROM [dbo].[Pagos])
BEGIN
    INSERT INTO [dbo].[Pagos] (dni, modalidadId, periodo, fechaInicio, fechaVencimiento, monto, fechaPago, metodoPago, usuarioRegistro, dvh)
    SELECT v.dni, v.modalidad, DATEFROMPARTS(YEAR(d.inicio), MONTH(d.inicio), 1), d.inicio, DATEADD(DAY, 29, d.inicio),
           pm.Precio, DATEADD(MINUTE, v.minutos, CAST(d.inicio AS DATETIME)), v.metodo, v.usuario, ''
    FROM (VALUES
        (@A1, @Mod3, -75, 'Efectivo',      @Recep,           9 * 60 + 15),
        (@A1, @Mod3, -45, 'Transferencia', @Recep,          10 * 60 + 40),
        (@A1, @Mod3, -15, 'Tarjeta',       ISNULL(@UsrCliente, @Recep), 21 * 60 + 5),
        (@A2, @Mod2, -40, 'Efectivo',      @Recep,          18 * 60 + 30),
        (@A2, @Mod2, -10, 'Efectivo',      @Recep,          18 * 60 + 10),
        (@A3, @Mod1, -66, 'Transferencia', @Admin,          11 * 60 + 20),
        (@A3, @Mod1, -36, 'Efectivo',      @Recep,           8 * 60 + 50)
    ) v (dni, modalidad, dias, metodo, usuario, minutos)
    CROSS APPLY (SELECT DATEADD(DAY, v.dias, @Hoy) AS inicio) d
    JOIN [dbo].[PrecioModalidad] pm ON pm.Id = v.modalidad
    WHERE v.dni IS NOT NULL;
    PRINT CONCAT('Pagos: ', @@ROWCOUNT);
END

-- ---------- 4) Inscripciones fijas ----------
IF NOT EXISTS (SELECT 1 FROM [dbo].[InscripcionFija])
BEGIN
    INSERT INTO [dbo].[InscripcionFija] (dni, codHorario, fechaDesde, fechaHasta, dvh)
    SELECT v.dni, h.codHorario, DATEADD(DAY, v.desde, @Hoy), NULL, ''
    FROM (VALUES
        (@A1, 'Funcional', 1, -75),
        (@A1, 'Funcional', 3, -75),
        (@A1, 'Funcional', 5, -75),
        (@A2, 'Pilates',   2, -40),
        (@A2, 'Pilates',   4, -40),
        (@A3, 'Yoga',      6, -66)
    ) v (dni, actividad, dia, desde)
    JOIN [dbo].[Actividades] a ON a.descripcion = v.actividad
    JOIN [dbo].[ActividadHorario] h ON h.codActividad = a.codActividad AND h.diaSemana = v.dia
    WHERE v.dni IS NOT NULL;
    PRINT CONCAT('Inscripciones fijas: ', @@ROWCOUNT);
END

-- ---------- 5) Cambios por clase (A = clase extra, B = falta avisada) ----------
-- semana 0 = la próxima clase de ese turno (de mañana a 7 días), -1 = la anterior.
IF NOT EXISTS (SELECT 1 FROM [dbo].[InscripcionClase])
BEGIN
    INSERT INTO [dbo].[InscripcionClase] (dni, codHorario, fecha, tipo, dvh)
    SELECT v.dni, h.codHorario,
           DATEADD(DAY, (h.diaSemana - 1 - DATEDIFF(DAY, 0, @Hoy) % 7 + 6) % 7 + 1 + v.semana * 7, @Hoy),
           v.tipo, ''
    FROM (VALUES
        (@A1, 'Funcional', 3,  0, 'B'),
        (@A1, 'Yoga',      6,  0, 'A'),
        (@A2, 'Spinning',  3,  0, 'A'),
        (@A2, 'Pilates',   4, -1, 'B')
    ) v (dni, actividad, dia, semana, tipo)
    JOIN [dbo].[Actividades] a ON a.descripcion = v.actividad
    JOIN [dbo].[ActividadHorario] h ON h.codActividad = a.codActividad AND h.diaSemana = v.dia
    WHERE v.dni IS NOT NULL;
    PRINT CONCAT('Cambios por clase: ', @@ROWCOUNT);
END

-- ---------- 6) Rutinas y sus ejercicios ----------
IF NOT EXISTS (SELECT 1 FROM [dbo].[Rutinas])
BEGIN
    INSERT INTO [dbo].[Rutinas] (descripcion, fecha, dniAlumno, dniEntrenador, codActividad, dvh)
    SELECT v.descripcion, DATEADD(DAY, v.dias, @Hoy), v.alumno, v.entrenador, a.codActividad, ''
    FROM (VALUES
        ('Fuerza general - tren inferior', -30, @A1, @E1, 'Funcional'),
        ('Fuerza general - tren superior', -30, @A1, @E1, 'Funcional'),
        ('Acondicionamiento y core',       -20, @A2, ISNULL(@E2, @E1), 'Pilates'),
        ('Iniciación a la fuerza',         -60, @A3, @E1, 'Crossfit')
    ) v (descripcion, dias, alumno, entrenador, actividad)
    JOIN [dbo].[Actividades] a ON a.descripcion = v.actividad
    WHERE v.alumno IS NOT NULL;
    PRINT CONCAT('Rutinas: ', @@ROWCOUNT);
END

IF NOT EXISTS (SELECT 1 FROM [dbo].[RutinaEjercicio])
BEGIN
    INSERT INTO [dbo].[RutinaEjercicio] (codRutina, codEjercicio, orden, series, repeticiones, pesoLevantado, descansoSegundos, observaciones, dvh)
    SELECT r.codRutina, e.codEjercicio, v.orden, v.series, v.reps, v.peso, v.descanso, v.obs, ''
    FROM (VALUES
        (@A1, 'Fuerza general - tren inferior', 1, 'Sentadilla',               4,  8,  70.00, 120, N'Bajar hasta paralelo'),
        (@A1, 'Fuerza general - tren inferior', 2, 'Peso muerto',              3,  6,  90.00, 150, N'Espalda neutra'),
        (@A1, 'Fuerza general - tren inferior', 3, 'Prensa de piernas',        3, 12, 120.00,  90, NULL),
        (@A1, 'Fuerza general - tren inferior', 4, 'Hip thrust',               3, 10,  80.00,  90, NULL),
        (@A1, 'Fuerza general - tren superior', 1, 'Press de banca',           4,  8,  60.00, 120, NULL),
        (@A1, 'Fuerza general - tren superior', 2, 'Remo con barra',           4, 10,  50.00,  90, NULL),
        (@A1, 'Fuerza general - tren superior', 3, 'Press militar',            3, 10,  35.00,  90, NULL),
        (@A1, 'Fuerza general - tren superior', 4, 'Curl de bíceps con barra', 3, 12,  25.00,  60, N'Sin balanceo'),
        (@A2, 'Acondicionamiento y core',       1, 'Hip thrust',               3, 15,  40.00,  60, N'Pausa de 2 s arriba'),
        (@A2, 'Acondicionamiento y core',       2, 'Sentadilla',               3, 15,   NULL,  60, N'Con peso corporal'),
        (@A2, 'Acondicionamiento y core',       3, 'Prensa de piernas',        3, 12,  60.00,  60, NULL),
        (@A3, 'Iniciación a la fuerza',         1, 'Sentadilla',               3, 10,  40.00,  90, N'Técnica antes que peso'),
        (@A3, 'Iniciación a la fuerza',         2, 'Press de banca',           3, 10,  40.00,  90, NULL),
        (@A3, 'Iniciación a la fuerza',         3, 'Remo con barra',           3, 10,  30.00,  90, NULL),
        (@A3, 'Iniciación a la fuerza',         4, 'Peso muerto',              3,  8,  50.00, 120, NULL)
    ) v (alumno, rutina, orden, ejercicio, series, reps, peso, descanso, obs)
    JOIN [dbo].[Rutinas] r ON r.dniAlumno = v.alumno AND r.descripcion = v.rutina
    JOIN [dbo].[Ejercicio] e ON e.nombre = v.ejercicio;
    PRINT CONCAT('Ejercicios de rutinas: ', @@ROWCOUNT);
END

-- ---------- 7) Cálculo de RM (Epley: peso x (1 + reps / 30)) ----------
IF NOT EXISTS (SELECT 1 FROM [dbo].[AlumnoRM])
BEGIN
    INSERT INTO [dbo].[AlumnoRM] (dni, codEjercicio, pesoRM, fechaRM, pesoLevantado, repeticiones, usuarioRegistro, dvh)
    SELECT v.dni, e.codEjercicio,
           CAST(ROUND(v.peso * (1 + v.reps / 30.0), 2) AS DECIMAL(6,2)),
           DATEADD(DAY, v.dias, @Hoy), v.peso, v.reps, v.usuario, ''
    FROM (VALUES
        (@A1, 'Sentadilla',     -60,  80.00, 5, @UsrE1),
        (@A1, 'Sentadilla',     -10,  85.00, 5, @UsrE1),
        (@A1, 'Press de banca', -60,  65.00, 6, @UsrE1),
        (@A1, 'Press de banca', -10,  70.00, 5, @UsrE1),
        (@A1, 'Peso muerto',    -10, 100.00, 5, @UsrE1),
        (@A2, 'Hip thrust',     -30,  70.00, 8, @UsrE2),
        (@A2, 'Hip thrust',      -5,  80.00, 6, @UsrE2),
        (@A2, 'Sentadilla',      -5,  50.00, 8, @UsrE2),
        (@A3, 'Press de banca', -40,  50.00, 10, @UsrE1)
    ) v (dni, ejercicio, dias, peso, reps, usuario)
    JOIN [dbo].[Ejercicio] e ON e.nombre = v.ejercicio
    WHERE v.dni IS NOT NULL;
    PRINT CONCAT('Registros de RM: ', @@ROWCOUNT);
END

-- ---------- 8) Peso corporal ----------
IF NOT EXISTS (SELECT 1 FROM [dbo].[PesoHistorial])
BEGIN
    INSERT INTO [dbo].[PesoHistorial] (dni, peso, fecha, dvh)
    SELECT v.dni, v.peso, DATEADD(DAY, v.dias, @Hoy), ''
    FROM (VALUES
        (@A1, -90, 82.50), (@A1, -60, 81.80), (@A1, -30, 81.20), (@A1, -3, 80.90),
        (@A2, -60, 64.00), (@A2, -30, 63.40), (@A2, -3, 62.80),
        (@A3, -90, 95.00), (@A3, -45, 94.10), (@A3, -10, 93.20),
        (@A4, -30, 58.00), (@A4, -2, 58.60)
    ) v (dni, dias, peso)
    WHERE v.dni IS NOT NULL;
    PRINT CONCAT('Registros de peso: ', @@ROWCOUNT);

    -- El peso actual del alumno es el último registrado
    UPDATE al
    SET al.peso = ult.peso
    FROM [dbo].[ALUMNOS] al
    CROSS APPLY (SELECT TOP 1 ph.peso FROM [dbo].[PesoHistorial] ph WHERE ph.dni = al.dni ORDER BY ph.fecha DESC, ph.codPeso DESC) ult;
END

-- ---------- 9) Avisos de cambio de precio ----------
-- Uno de hace 45 días (ya no se muestra) y uno de hace 3 días (aparece el banner).
IF NOT EXISTS (SELECT 1 FROM [dbo].[AvisoPrecio])
BEGIN
    INSERT INTO [dbo].[AvisoPrecio] (modalidadId, descripcion, precioAnterior, precioNuevo, fecha, usuarioRegistro, dvh)
    SELECT pm.Id,
           CASE WHEN pm.EsDiario = 1 THEN 'Diario (todos los días)'
                ELSE CONCAT(pm.DiasPorSemana, ' día', CASE WHEN pm.DiasPorSemana > 1 THEN 's' ELSE '' END, ' por semana') END,
           pm.Precio - v.aumento, pm.Precio,
           DATEADD(MINUTE, v.minutos, CAST(DATEADD(DAY, v.dias, @Hoy) AS DATETIME)), @Admin, ''
    FROM (VALUES
        (@Mod2, -45, 4000.00, 10 * 60),
        (@Mod3,  -3, 3000.00, 10 * 60 + 30)
    ) v (modalidad, dias, aumento, minutos)
    JOIN [dbo].[PrecioModalidad] pm ON pm.Id = v.modalidad
    WHERE pm.Precio > v.aumento;
    PRINT CONCAT('Avisos de precio: ', @@ROWCOUNT);

    -- El viejo lo vio el administrador; el nuevo, solo el cliente del primer alumno
    -- (así el banner sigue apareciendo para el resto en la demo).
    INSERT INTO [dbo].[AvisoPrecioVisto] (codAviso, usr, fechaVisto, dvh)
    SELECT ap.codAviso, v.usr, DATEADD(HOUR, 2, ap.fecha), ''
    FROM (VALUES (@Mod2, @Admin), (@Mod3, @UsrCliente)) v (modalidad, usr)
    JOIN [dbo].[AvisoPrecio] ap ON ap.modalidadId = v.modalidad
    WHERE v.usr IS NOT NULL;
    PRINT CONCAT('Avisos vistos: ', @@ROWCOUNT);
END

-- ---------- 10) Alertas de check-in ----------
-- A3 tiene el abono vencido hace 7 días; A4 nunca pagó.
IF NOT EXISTS (SELECT 1 FROM [dbo].[AlertaCheckin])
BEGIN
    INSERT INTO [dbo].[AlertaCheckin] (dni, situacion, fechaVencimiento, fecha, vista, usuarioVista, fechaVista, dvh)
    SELECT v.dni, v.situacion, v.vencimiento,
           DATEADD(MINUTE, v.minutos, CAST(DATEADD(DAY, v.dias, @Hoy) AS DATETIME)),
           v.vista,
           CASE WHEN v.vista = 1 THEN @Recep END,
           CASE WHEN v.vista = 1 THEN DATEADD(MINUTE, v.minutos + 10, CAST(DATEADD(DAY, v.dias, @Hoy) AS DATETIME)) END,
           ''
    FROM (VALUES
        (@A3, 'Vencido',  DATEADD(DAY, -7, @Hoy), -5,  9 * 60 + 10, 1),
        (@A3, 'Vencido',  DATEADD(DAY, -7, @Hoy), -1, 18 * 60 +  5, 0),
        (@A4, 'SinAbono', NULL,                   -1,  8 * 60 + 20, 0)
    ) v (dni, situacion, vencimiento, dias, minutos, vista)
    WHERE v.dni IS NOT NULL;
    PRINT CONCAT('Alertas de check-in: ', @@ROWCOUNT);
END

COMMIT TRANSACTION;

PRINT 'Datos de demo cargados. Ahora: VerificacioDV -> Recalcular dígitos.';
GO

-- Por si el control previo lo había activado
SET NOEXEC OFF;
GO
