USE [GymApp]
GO

-- ============================================================================
-- Cálculo de RM (14/11/2026)
--
-- Pantalla nueva "Cálculo de RM": estima la repetición máxima (1RM) con la
-- fórmula de Epley a partir de un peso y las repeticiones hechas, y guarda el
-- resultado en el historial del alumno (tabla AlumnoRM).
--
-- Las tablas Ejercicio y AlumnoRM ya existían en el esquema viejo sin uso:
-- se crean solo si faltan, y a AlumnoRM se le suman las columnas con los datos
-- de la serie (peso, repeticiones) y quién la registró.
--
-- Permiso nuevo CalculoRM:
--   - Familia "Operación del gimnasio" (Recepcionista y, por herencia, Administrador)
--   - Familia "Portal del cliente" (Cliente y Familiar)
--   - Permiso suelto del perfil Entrenador
--   - El WebMaster lo tiene siempre.
--
-- Antes de correrlo: hacer un backup.
-- Después de correrlo: VerificacioDV -> "Recalcular dígitos" (filas nuevas en
-- Ejercicio, Permiso, FamiliaPermiso, PerfilPermiso y traducciones).
-- IdiomaID: 1 ES, 2 EN, 3 PT, 4 FR, 5 JA.
-- ============================================================================

-- 1. Ejercicio
IF OBJECT_ID('dbo.Ejercicio', 'U') IS NULL
BEGIN
    CREATE TABLE [dbo].[Ejercicio](
        [codEjercicio]   INT            IDENTITY(1,1) NOT NULL,
        [nombre]         VARCHAR(100)   NOT NULL,
        [grupoMuscular]  VARCHAR(100)   NOT NULL,
        [descripcion]    VARCHAR(500)   NULL,
        [dvh]            VARCHAR(64)    NOT NULL,
        CONSTRAINT [PK_Ejercicio] PRIMARY KEY CLUSTERED ([codEjercicio] ASC),
        CONSTRAINT [UK_Ejercicio_Nombre] UNIQUE NONCLUSTERED ([nombre] ASC)
    );
    PRINT 'Tabla Ejercicio creada.';
END
GO

-- Ejercicios básicos para la calculadora (solo los que falten)
INSERT INTO [dbo].[Ejercicio] ([nombre], [grupoMuscular], [descripcion], [dvh])
SELECT v.nombre, v.grupo, NULL, ''
FROM (VALUES
    ('Press de banca',           'Pecho'),
    ('Sentadilla',               'Piernas'),
    ('Peso muerto',              'Espalda y piernas'),
    ('Press militar',            'Hombros'),
    ('Remo con barra',           'Espalda'),
    ('Dominadas lastradas',      'Espalda'),
    ('Hip thrust',               'Glúteos'),
    ('Prensa de piernas',        'Piernas'),
    ('Curl de bíceps con barra', 'Brazos')
) v (nombre, grupo)
WHERE NOT EXISTS (SELECT 1 FROM [dbo].[Ejercicio] e WHERE e.nombre = v.nombre);
GO

-- 2. AlumnoRM (historial de 1RM por alumno y ejercicio)
IF OBJECT_ID('dbo.AlumnoRM', 'U') IS NULL
BEGIN
    CREATE TABLE [dbo].[AlumnoRM](
        [codRM]            INT            IDENTITY(1,1) NOT NULL,
        [dni]              INT            NOT NULL,
        [codEjercicio]     INT            NOT NULL,
        [pesoRM]           DECIMAL(6,2)   NOT NULL,
        [fechaRM]          DATE           NOT NULL,
        [dvh]              VARCHAR(64)    NOT NULL,
        CONSTRAINT [PK_AlumnoRM] PRIMARY KEY CLUSTERED ([codRM] ASC),
        CONSTRAINT [FK_AlumnoRM_Alumno] FOREIGN KEY ([dni]) REFERENCES [dbo].[Alumnos] ([dni]),
        CONSTRAINT [FK_AlumnoRM_Ejercicio] FOREIGN KEY ([codEjercicio]) REFERENCES [dbo].[Ejercicio] ([codEjercicio]),
        CONSTRAINT [CK_AlumnoRM_Peso] CHECK ([pesoRM] > 0)
    );
    CREATE INDEX [IX_AlumnoRM_AlumnoEjercicio] ON [dbo].[AlumnoRM] ([dni], [codEjercicio]);
    PRINT 'Tabla AlumnoRM creada.';
END
GO

-- Datos de la serie con la que se estimó (NULL en filas viejas, si las hubiera)
IF COL_LENGTH('dbo.AlumnoRM', 'pesoLevantado') IS NULL
    ALTER TABLE [dbo].[AlumnoRM] ADD [pesoLevantado] DECIMAL(6,2) NULL;
IF COL_LENGTH('dbo.AlumnoRM', 'repeticiones') IS NULL
    ALTER TABLE [dbo].[AlumnoRM] ADD [repeticiones] INT NULL;
IF COL_LENGTH('dbo.AlumnoRM', 'usuarioRegistro') IS NULL
    ALTER TABLE [dbo].[AlumnoRM] ADD [usuarioRegistro] VARCHAR(50) NULL;
GO

IF OBJECT_ID('dbo.CK_AlumnoRM_Repeticiones', 'C') IS NULL
    ALTER TABLE [dbo].[AlumnoRM] ADD CONSTRAINT [CK_AlumnoRM_Repeticiones]
        CHECK ([repeticiones] IS NULL OR [repeticiones] BETWEEN 1 AND 12);
GO

PRINT 'Ejercicio y AlumnoRM listas.';
GO

-- 3. Permiso CalculoRM
SET XACT_ABORT ON;
BEGIN TRANSACTION;

IF NOT EXISTS (SELECT 1 FROM [dbo].[Permiso] WHERE [nombre] = 'CalculoRM')
    INSERT INTO [dbo].[Permiso] ([nombre], [dvh]) VALUES ('CalculoRM', '');

INSERT INTO [dbo].[FamiliaPermiso] ([idFamilia], [idPermiso], [dvh])
SELECT f.idFamilia, p.idPermiso, ''
FROM [dbo].[Familia] f
JOIN [dbo].[Permiso] p ON p.nombre = 'CalculoRM'
WHERE f.codigo IN ('OPERACION', 'PORTAL_CLIENTE')
  AND NOT EXISTS (SELECT 1 FROM [dbo].[FamiliaPermiso] fp
                  WHERE fp.idFamilia = f.idFamilia AND fp.idPermiso = p.idPermiso);

INSERT INTO [dbo].[PerfilPermiso] ([idPerfil], [idPermiso], [dvh])
SELECT pf.idPerfil, p.idPermiso, ''
FROM [dbo].[Perfil] pf
JOIN [dbo].[Permiso] p ON p.nombre = 'CalculoRM'
WHERE pf.codigo = 'ENTRENADOR'
  AND NOT EXISTS (SELECT 1 FROM [dbo].[PerfilPermiso] pp
                  WHERE pp.idPerfil = pf.idPerfil AND pp.idPermiso = p.idPermiso);

COMMIT TRANSACTION;
GO

PRINT 'Permiso CalculoRM asignado.';
GO

-- ============================================================================
-- 4. Traducciones
-- ============================================================================

IF COL_LENGTH('Traducciones.Pantalla_Rutinas', 'rm_titulo') IS NULL
    ALTER TABLE [Traducciones].[Pantalla_Rutinas] ADD [rm_titulo] NVARCHAR(500) NOT NULL DEFAULT N'';
IF COL_LENGTH('Traducciones.Pantalla_Rutinas', 'rm_subtitulo') IS NULL
    ALTER TABLE [Traducciones].[Pantalla_Rutinas] ADD [rm_subtitulo] NVARCHAR(500) NOT NULL DEFAULT N'';
IF COL_LENGTH('Traducciones.Pantalla_Rutinas', 'rm_calcular_titulo') IS NULL
    ALTER TABLE [Traducciones].[Pantalla_Rutinas] ADD [rm_calcular_titulo] NVARCHAR(500) NOT NULL DEFAULT N'';
IF COL_LENGTH('Traducciones.Pantalla_Rutinas', 'rm_campo_alumno') IS NULL
    ALTER TABLE [Traducciones].[Pantalla_Rutinas] ADD [rm_campo_alumno] NVARCHAR(500) NOT NULL DEFAULT N'';
IF COL_LENGTH('Traducciones.Pantalla_Rutinas', 'rm_campo_ejercicio') IS NULL
    ALTER TABLE [Traducciones].[Pantalla_Rutinas] ADD [rm_campo_ejercicio] NVARCHAR(500) NOT NULL DEFAULT N'';
IF COL_LENGTH('Traducciones.Pantalla_Rutinas', 'rm_campo_peso') IS NULL
    ALTER TABLE [Traducciones].[Pantalla_Rutinas] ADD [rm_campo_peso] NVARCHAR(500) NOT NULL DEFAULT N'';
IF COL_LENGTH('Traducciones.Pantalla_Rutinas', 'rm_campo_reps') IS NULL
    ALTER TABLE [Traducciones].[Pantalla_Rutinas] ADD [rm_campo_reps] NVARCHAR(500) NOT NULL DEFAULT N'';
IF COL_LENGTH('Traducciones.Pantalla_Rutinas', 'rm_ayuda_formula') IS NULL
    ALTER TABLE [Traducciones].[Pantalla_Rutinas] ADD [rm_ayuda_formula] NVARCHAR(500) NOT NULL DEFAULT N'';
IF COL_LENGTH('Traducciones.Pantalla_Rutinas', 'rm_btn_calcular') IS NULL
    ALTER TABLE [Traducciones].[Pantalla_Rutinas] ADD [rm_btn_calcular] NVARCHAR(500) NOT NULL DEFAULT N'';
IF COL_LENGTH('Traducciones.Pantalla_Rutinas', 'rm_btn_guardar') IS NULL
    ALTER TABLE [Traducciones].[Pantalla_Rutinas] ADD [rm_btn_guardar] NVARCHAR(500) NOT NULL DEFAULT N'';
IF COL_LENGTH('Traducciones.Pantalla_Rutinas', 'rm_resultado_label') IS NULL
    ALTER TABLE [Traducciones].[Pantalla_Rutinas] ADD [rm_resultado_label] NVARCHAR(500) NOT NULL DEFAULT N'';
IF COL_LENGTH('Traducciones.Pantalla_Rutinas', 'rm_anterior') IS NULL
    ALTER TABLE [Traducciones].[Pantalla_Rutinas] ADD [rm_anterior] NVARCHAR(500) NOT NULL DEFAULT N'';
IF COL_LENGTH('Traducciones.Pantalla_Rutinas', 'rm_primera') IS NULL
    ALTER TABLE [Traducciones].[Pantalla_Rutinas] ADD [rm_primera] NVARCHAR(500) NOT NULL DEFAULT N'';
IF COL_LENGTH('Traducciones.Pantalla_Rutinas', 'rm_tabla_titulo') IS NULL
    ALTER TABLE [Traducciones].[Pantalla_Rutinas] ADD [rm_tabla_titulo] NVARCHAR(500) NOT NULL DEFAULT N'';
IF COL_LENGTH('Traducciones.Pantalla_Rutinas', 'rm_col_porcentaje') IS NULL
    ALTER TABLE [Traducciones].[Pantalla_Rutinas] ADD [rm_col_porcentaje] NVARCHAR(500) NOT NULL DEFAULT N'';
IF COL_LENGTH('Traducciones.Pantalla_Rutinas', 'rm_col_peso') IS NULL
    ALTER TABLE [Traducciones].[Pantalla_Rutinas] ADD [rm_col_peso] NVARCHAR(500) NOT NULL DEFAULT N'';
IF COL_LENGTH('Traducciones.Pantalla_Rutinas', 'rm_col_reps') IS NULL
    ALTER TABLE [Traducciones].[Pantalla_Rutinas] ADD [rm_col_reps] NVARCHAR(500) NOT NULL DEFAULT N'';
IF COL_LENGTH('Traducciones.Pantalla_Rutinas', 'rm_historial_titulo') IS NULL
    ALTER TABLE [Traducciones].[Pantalla_Rutinas] ADD [rm_historial_titulo] NVARCHAR(500) NOT NULL DEFAULT N'';
IF COL_LENGTH('Traducciones.Pantalla_Rutinas', 'rm_col_fecha') IS NULL
    ALTER TABLE [Traducciones].[Pantalla_Rutinas] ADD [rm_col_fecha] NVARCHAR(500) NOT NULL DEFAULT N'';
IF COL_LENGTH('Traducciones.Pantalla_Rutinas', 'rm_col_serie') IS NULL
    ALTER TABLE [Traducciones].[Pantalla_Rutinas] ADD [rm_col_serie] NVARCHAR(500) NOT NULL DEFAULT N'';
IF COL_LENGTH('Traducciones.Pantalla_Rutinas', 'rm_col_rm') IS NULL
    ALTER TABLE [Traducciones].[Pantalla_Rutinas] ADD [rm_col_rm] NVARCHAR(500) NOT NULL DEFAULT N'';
IF COL_LENGTH('Traducciones.Pantalla_Rutinas', 'rm_col_registro') IS NULL
    ALTER TABLE [Traducciones].[Pantalla_Rutinas] ADD [rm_col_registro] NVARCHAR(500) NOT NULL DEFAULT N'';
IF COL_LENGTH('Traducciones.Pantalla_Rutinas', 'rm_sin_historial') IS NULL
    ALTER TABLE [Traducciones].[Pantalla_Rutinas] ADD [rm_sin_historial] NVARCHAR(500) NOT NULL DEFAULT N'';
IF COL_LENGTH('Traducciones.Pantalla_Rutinas', 'rm_sin_alumnos') IS NULL
    ALTER TABLE [Traducciones].[Pantalla_Rutinas] ADD [rm_sin_alumnos] NVARCHAR(500) NOT NULL DEFAULT N'';
IF COL_LENGTH('Traducciones.Pantalla_Rutinas', 'rm_elegir') IS NULL
    ALTER TABLE [Traducciones].[Pantalla_Rutinas] ADD [rm_elegir] NVARCHAR(500) NOT NULL DEFAULT N'';
IF COL_LENGTH('Traducciones.Pantalla_Rutinas', 'rm_msg_guardado') IS NULL
    ALTER TABLE [Traducciones].[Pantalla_Rutinas] ADD [rm_msg_guardado] NVARCHAR(500) NOT NULL DEFAULT N'';
IF COL_LENGTH('Traducciones.Pantalla_Rutinas', 'rm_err_alumno') IS NULL
    ALTER TABLE [Traducciones].[Pantalla_Rutinas] ADD [rm_err_alumno] NVARCHAR(500) NOT NULL DEFAULT N'';
IF COL_LENGTH('Traducciones.Pantalla_Rutinas', 'rm_err_alumno_inactivo') IS NULL
    ALTER TABLE [Traducciones].[Pantalla_Rutinas] ADD [rm_err_alumno_inactivo] NVARCHAR(500) NOT NULL DEFAULT N'';
IF COL_LENGTH('Traducciones.Pantalla_Rutinas', 'rm_err_no_asociado') IS NULL
    ALTER TABLE [Traducciones].[Pantalla_Rutinas] ADD [rm_err_no_asociado] NVARCHAR(500) NOT NULL DEFAULT N'';
IF COL_LENGTH('Traducciones.Pantalla_Rutinas', 'rm_err_ejercicio') IS NULL
    ALTER TABLE [Traducciones].[Pantalla_Rutinas] ADD [rm_err_ejercicio] NVARCHAR(500) NOT NULL DEFAULT N'';
IF COL_LENGTH('Traducciones.Pantalla_Rutinas', 'rm_err_peso') IS NULL
    ALTER TABLE [Traducciones].[Pantalla_Rutinas] ADD [rm_err_peso] NVARCHAR(500) NOT NULL DEFAULT N'';
IF COL_LENGTH('Traducciones.Pantalla_Rutinas', 'rm_err_reps') IS NULL
    ALTER TABLE [Traducciones].[Pantalla_Rutinas] ADD [rm_err_reps] NVARCHAR(500) NOT NULL DEFAULT N'';
GO

UPDATE [Traducciones].[Pantalla_Rutinas] SET
    [rm_titulo] = CASE [IdiomaID] WHEN 1 THEN N'Cálculo de RM' WHEN 2 THEN N'1RM calculator' WHEN 3 THEN N'Cálculo de RM' WHEN 4 THEN N'Calcul du 1RM' WHEN 5 THEN N'1RM計算' ELSE N'Cálculo de RM' END,
    [rm_subtitulo] = CASE [IdiomaID] WHEN 1 THEN N'Estimá la repetición máxima (1RM) con el peso y las repeticiones de una serie.' WHEN 2 THEN N'Estimate the one-rep max (1RM) from the weight and reps of a set.' WHEN 3 THEN N'Estime a repetição máxima (1RM) com o peso e as repetições de uma série.' WHEN 4 THEN N'Estimez la répétition maximale (1RM) à partir du poids et des répétitions d''une série.' WHEN 5 THEN N'セットの重量と回数から最大挙上重量（1RM）を推定します。' ELSE N'Estimá la repetición máxima (1RM) con el peso y las repeticiones de una serie.' END,
    [rm_calcular_titulo] = CASE [IdiomaID] WHEN 1 THEN N'Calcular 1RM' WHEN 2 THEN N'Calculate 1RM' WHEN 3 THEN N'Calcular 1RM' WHEN 4 THEN N'Calculer le 1RM' WHEN 5 THEN N'1RMを計算' ELSE N'Calcular 1RM' END,
    [rm_campo_alumno] = CASE [IdiomaID] WHEN 1 THEN N'Alumno' WHEN 2 THEN N'Student' WHEN 3 THEN N'Aluno' WHEN 4 THEN N'Élève' WHEN 5 THEN N'生徒' ELSE N'Alumno' END,
    [rm_campo_ejercicio] = CASE [IdiomaID] WHEN 1 THEN N'Ejercicio' WHEN 2 THEN N'Exercise' WHEN 3 THEN N'Exercício' WHEN 4 THEN N'Exercice' WHEN 5 THEN N'種目' ELSE N'Ejercicio' END,
    [rm_campo_peso] = CASE [IdiomaID] WHEN 1 THEN N'Peso levantado (kg)' WHEN 2 THEN N'Weight lifted (kg)' WHEN 3 THEN N'Peso levantado (kg)' WHEN 4 THEN N'Poids soulevé (kg)' WHEN 5 THEN N'挙上重量（kg）' ELSE N'Peso levantado (kg)' END,
    [rm_campo_reps] = CASE [IdiomaID] WHEN 1 THEN N'Repeticiones' WHEN 2 THEN N'Reps' WHEN 3 THEN N'Repetições' WHEN 4 THEN N'Répétitions' WHEN 5 THEN N'回数' ELSE N'Repeticiones' END,
    [rm_ayuda_formula] = CASE [IdiomaID] WHEN 1 THEN N'Fórmula de Epley: 1RM = peso × (1 + repeticiones / 30). Es confiable hasta {0} repeticiones.' WHEN 2 THEN N'Epley formula: 1RM = weight × (1 + reps / 30). Reliable up to {0} reps.' WHEN 3 THEN N'Fórmula de Epley: 1RM = peso × (1 + repetições / 30). É confiável até {0} repetições.' WHEN 4 THEN N'Formule d''Epley : 1RM = poids × (1 + répétitions / 30). Fiable jusqu''à {0} répétitions.' WHEN 5 THEN N'エプリー式：1RM = 重量 ×（1 + 回数 / 30）。{0}回までが目安です。' ELSE N'Fórmula de Epley: 1RM = peso × (1 + repeticiones / 30). Es confiable hasta {0} repeticiones.' END,
    [rm_btn_calcular] = CASE [IdiomaID] WHEN 1 THEN N'Calcular' WHEN 2 THEN N'Calculate' WHEN 3 THEN N'Calcular' WHEN 4 THEN N'Calculer' WHEN 5 THEN N'計算' ELSE N'Calcular' END,
    [rm_btn_guardar] = CASE [IdiomaID] WHEN 1 THEN N'Guardar en el historial' WHEN 2 THEN N'Save to history' WHEN 3 THEN N'Salvar no histórico' WHEN 4 THEN N'Enregistrer dans l''historique' WHEN 5 THEN N'履歴に保存' ELSE N'Guardar en el historial' END,
    [rm_resultado_label] = CASE [IdiomaID] WHEN 1 THEN N'1RM estimada' WHEN 2 THEN N'Estimated 1RM' WHEN 3 THEN N'1RM estimada' WHEN 4 THEN N'1RM estimé' WHEN 5 THEN N'推定1RM' ELSE N'1RM estimada' END,
    [rm_anterior] = CASE [IdiomaID] WHEN 1 THEN N'Último registro: {0} kg el {1} ({2} kg)' WHEN 2 THEN N'Last record: {0} kg on {1} ({2} kg)' WHEN 3 THEN N'Último registro: {0} kg em {1} ({2} kg)' WHEN 4 THEN N'Dernier enregistrement : {0} kg le {1} ({2} kg)' WHEN 5 THEN N'前回の記録：{1} に {0} kg（{2} kg）' ELSE N'Último registro: {0} kg el {1} ({2} kg)' END,
    [rm_primera] = CASE [IdiomaID] WHEN 1 THEN N'Todavía no hay registros de este ejercicio.' WHEN 2 THEN N'No records for this exercise yet.' WHEN 3 THEN N'Ainda não há registros deste exercício.' WHEN 4 THEN N'Aucun enregistrement pour cet exercice.' WHEN 5 THEN N'この種目の記録はまだありません。' ELSE N'Todavía no hay registros de este ejercicio.' END,
    [rm_tabla_titulo] = CASE [IdiomaID] WHEN 1 THEN N'Cargas de entrenamiento' WHEN 2 THEN N'Training loads' WHEN 3 THEN N'Cargas de treino' WHEN 4 THEN N'Charges d''entraînement' WHEN 5 THEN N'トレーニング重量' ELSE N'Cargas de entrenamiento' END,
    [rm_col_porcentaje] = CASE [IdiomaID] WHEN 1 THEN N'% de la 1RM' WHEN 2 THEN N'% of 1RM' WHEN 3 THEN N'% da 1RM' WHEN 4 THEN N'% du 1RM' WHEN 5 THEN N'1RM比' ELSE N'% de la 1RM' END,
    [rm_col_peso] = CASE [IdiomaID] WHEN 1 THEN N'Peso (kg)' WHEN 2 THEN N'Weight (kg)' WHEN 3 THEN N'Peso (kg)' WHEN 4 THEN N'Poids (kg)' WHEN 5 THEN N'重量（kg）' ELSE N'Peso (kg)' END,
    [rm_col_reps] = CASE [IdiomaID] WHEN 1 THEN N'Repeticiones aprox.' WHEN 2 THEN N'Approx. reps' WHEN 3 THEN N'Repetições aprox.' WHEN 4 THEN N'Répétitions env.' WHEN 5 THEN N'目安回数' ELSE N'Repeticiones aprox.' END,
    [rm_historial_titulo] = CASE [IdiomaID] WHEN 1 THEN N'Historial de 1RM' WHEN 2 THEN N'1RM history' WHEN 3 THEN N'Histórico de 1RM' WHEN 4 THEN N'Historique du 1RM' WHEN 5 THEN N'1RMの履歴' ELSE N'Historial de 1RM' END,
    [rm_col_fecha] = CASE [IdiomaID] WHEN 1 THEN N'Fecha' WHEN 2 THEN N'Date' WHEN 3 THEN N'Data' WHEN 4 THEN N'Date' WHEN 5 THEN N'日付' ELSE N'Fecha' END,
    [rm_col_serie] = CASE [IdiomaID] WHEN 1 THEN N'Serie' WHEN 2 THEN N'Set' WHEN 3 THEN N'Série' WHEN 4 THEN N'Série' WHEN 5 THEN N'セット' ELSE N'Serie' END,
    [rm_col_rm] = CASE [IdiomaID] WHEN 1 THEN N'1RM' WHEN 2 THEN N'1RM' WHEN 3 THEN N'1RM' WHEN 4 THEN N'1RM' WHEN 5 THEN N'1RM' ELSE N'1RM' END,
    [rm_col_registro] = CASE [IdiomaID] WHEN 1 THEN N'Registró' WHEN 2 THEN N'Recorded by' WHEN 3 THEN N'Registrado por' WHEN 4 THEN N'Enregistré par' WHEN 5 THEN N'記録者' ELSE N'Registró' END,
    [rm_sin_historial] = CASE [IdiomaID] WHEN 1 THEN N'Todavía no hay registros de 1RM para este alumno.' WHEN 2 THEN N'No 1RM records for this student yet.' WHEN 3 THEN N'Ainda não há registros de 1RM para este aluno.' WHEN 4 THEN N'Aucun 1RM enregistré pour cet élève.' WHEN 5 THEN N'この生徒の1RMの記録はまだありません。' ELSE N'Todavía no hay registros de 1RM para este alumno.' END,
    [rm_sin_alumnos] = CASE [IdiomaID] WHEN 1 THEN N'No tenés alumnos asociados a tu cuenta.' WHEN 2 THEN N'You have no students linked to your account.' WHEN 3 THEN N'Você não tem alunos vinculados à sua conta.' WHEN 4 THEN N'Aucun élève n''est associé à votre compte.' WHEN 5 THEN N'アカウントに紐づく生徒はいません。' ELSE N'No tenés alumnos asociados a tu cuenta.' END,
    [rm_elegir] = CASE [IdiomaID] WHEN 1 THEN N'— Elegí —' WHEN 2 THEN N'— Choose —' WHEN 3 THEN N'— Escolha —' WHEN 4 THEN N'— Choisir —' WHEN 5 THEN N'— 選択 —' ELSE N'— Elegí —' END,
    [rm_msg_guardado] = CASE [IdiomaID] WHEN 1 THEN N'1RM guardada: {0} kg en {1}.' WHEN 2 THEN N'1RM saved: {0} kg on {1}.' WHEN 3 THEN N'1RM salva: {0} kg em {1}.' WHEN 4 THEN N'1RM enregistré : {0} kg en {1}.' WHEN 5 THEN N'1RMを保存しました：{1} {0} kg。' ELSE N'1RM guardada: {0} kg en {1}.' END,
    [rm_err_alumno] = CASE [IdiomaID] WHEN 1 THEN N'Debe seleccionar un alumno válido.' WHEN 2 THEN N'Select a valid student.' WHEN 3 THEN N'Selecione um aluno válido.' WHEN 4 THEN N'Sélectionnez un élève valide.' WHEN 5 THEN N'有効な生徒を選択してください。' ELSE N'Debe seleccionar un alumno válido.' END,
    [rm_err_alumno_inactivo] = CASE [IdiomaID] WHEN 1 THEN N'El alumno está dado de baja.' WHEN 2 THEN N'The student is inactive.' WHEN 3 THEN N'O aluno está inativo.' WHEN 4 THEN N'L''élève est désactivé.' WHEN 5 THEN N'この生徒は退会しています。' ELSE N'El alumno está dado de baja.' END,
    [rm_err_no_asociado] = CASE [IdiomaID] WHEN 1 THEN N'Ese alumno no está asociado a tu cuenta.' WHEN 2 THEN N'That student is not linked to your account.' WHEN 3 THEN N'Esse aluno não está vinculado à sua conta.' WHEN 4 THEN N'Cet élève n''est pas associé à votre compte.' WHEN 5 THEN N'その生徒はあなたのアカウントに紐づいていません。' ELSE N'Ese alumno no está asociado a tu cuenta.' END,
    [rm_err_ejercicio] = CASE [IdiomaID] WHEN 1 THEN N'Debe seleccionar un ejercicio válido.' WHEN 2 THEN N'Select a valid exercise.' WHEN 3 THEN N'Selecione um exercício válido.' WHEN 4 THEN N'Sélectionnez un exercice valide.' WHEN 5 THEN N'有効な種目を選択してください。' ELSE N'Debe seleccionar un ejercicio válido.' END,
    [rm_err_peso] = CASE [IdiomaID] WHEN 1 THEN N'El peso debe ser mayor que 0 y hasta {0} kg.' WHEN 2 THEN N'Weight must be greater than 0 and up to {0} kg.' WHEN 3 THEN N'O peso deve ser maior que 0 e até {0} kg.' WHEN 4 THEN N'Le poids doit être supérieur à 0 et au plus {0} kg.' WHEN 5 THEN N'重量は0より大きく{0} kg以下で入力してください。' ELSE N'El peso debe ser mayor que 0 y hasta {0} kg.' END,
    [rm_err_reps] = CASE [IdiomaID] WHEN 1 THEN N'Las repeticiones deben ser un número entero entre 1 y {0}.' WHEN 2 THEN N'Reps must be a whole number between 1 and {0}.' WHEN 3 THEN N'As repetições devem ser um número inteiro entre 1 e {0}.' WHEN 4 THEN N'Les répétitions doivent être un entier entre 1 et {0}.' WHEN 5 THEN N'回数は1〜{0}の整数で入力してください。' ELSE N'Las repeticiones deben ser un número entero entre 1 y {0}.' END;
GO

IF COL_LENGTH('Traducciones.Pantalla_DashBoard', 'menu_calculo_rm') IS NULL
    ALTER TABLE [Traducciones].[Pantalla_DashBoard] ADD [menu_calculo_rm] NVARCHAR(500) NOT NULL DEFAULT N'';
GO

UPDATE [Traducciones].[Pantalla_DashBoard] SET
    [menu_calculo_rm] = CASE [IdiomaID] WHEN 1 THEN N'Cálculo de RM' WHEN 2 THEN N'1RM calculator' WHEN 3 THEN N'Cálculo de RM' WHEN 4 THEN N'Calcul du 1RM' WHEN 5 THEN N'1RM計算' ELSE N'Cálculo de RM' END;
GO

IF COL_LENGTH('Traducciones.Pantalla_Permisos', 'permisos_nombre_calculorm') IS NULL
    ALTER TABLE [Traducciones].[Pantalla_Permisos] ADD [permisos_nombre_calculorm] NVARCHAR(500) NOT NULL DEFAULT N'';
GO

UPDATE [Traducciones].[Pantalla_Permisos] SET
    [permisos_nombre_calculorm] = CASE [IdiomaID] WHEN 1 THEN N'Cálculo de RM' WHEN 2 THEN N'1RM calculator' WHEN 3 THEN N'Cálculo de RM' WHEN 4 THEN N'Calcul du 1RM' WHEN 5 THEN N'1RM計算' ELSE N'Cálculo de RM' END;
GO

PRINT 'Cálculo de RM listo. Ahora: VerificacioDV -> Recalcular dígitos.';
GO
