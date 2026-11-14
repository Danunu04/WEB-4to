USE [GymApp]
GO

-- ============================================================================
-- Rutinas detalladas (CUN008) y ABM de alumnos por el web service (14/11/2026)
--
--   - RutinaEjercicio: ejercicios de cada rutina (series, repeticiones, peso opcional,
--     descanso y observaciones). Si la tabla ya existía del esquema viejo, se adapta:
--     se quita la columna calculada "volumen" y el peso pasa a ser opcional.
--   - Permiso GestionRutinas para la familia Portal del cliente (ver sus rutinas).
--   - Traducciones de la pantalla de ejercicios y de los mensajes del web service.
--
-- Requiere haber corrido antes scripts/calculo-rm.sql (tabla Ejercicio).
-- Antes de correrlo: hacer un backup.
-- Después de correrlo: VerificacioDV -> "Recalcular dígitos".
-- IdiomaID: 1 ES, 2 EN, 3 PT, 4 FR, 5 JA.
-- ============================================================================

IF OBJECT_ID('dbo.Ejercicio', 'U') IS NULL
BEGIN
    RAISERROR('Falta la tabla Ejercicio: correr antes scripts/calculo-rm.sql. No se hizo ningún cambio.', 16, 1);
    SET NOEXEC ON;
END
GO

-- 1. Trigger viejo del esquema original (escribía porcentaje1RM sin recalcular dígitos)
IF OBJECT_ID('dbo.trg_RutinaEjercicio_Porcentaje1RM', 'TR') IS NOT NULL
    DROP TRIGGER [dbo].[trg_RutinaEjercicio_Porcentaje1RM];
GO

-- 2. RutinaEjercicio
IF OBJECT_ID('dbo.RutinaEjercicio', 'U') IS NULL
BEGIN
    CREATE TABLE [dbo].[RutinaEjercicio](
        [codRutinaEjercicio]  INT             IDENTITY(1,1) NOT NULL,
        [codRutina]           INT             NOT NULL,
        [codEjercicio]        INT             NOT NULL,
        [orden]               INT             NOT NULL CONSTRAINT [DF_RutinaEjercicio_orden] DEFAULT (0),
        [series]              INT             NOT NULL,
        [repeticiones]        INT             NOT NULL,
        [pesoLevantado]       DECIMAL(6,2)    NULL,
        [descansoSegundos]    INT             NULL,
        [observaciones]       NVARCHAR(300)   NULL,
        [dvh]                 VARCHAR(64)     NOT NULL,
        CONSTRAINT [PK_RutinaEjercicio] PRIMARY KEY CLUSTERED ([codRutinaEjercicio] ASC),
        CONSTRAINT [FK_RutinaEjercicio_Rutina] FOREIGN KEY ([codRutina]) REFERENCES [dbo].[Rutinas] ([codRutina]),
        CONSTRAINT [FK_RutinaEjercicio_Ejercicio] FOREIGN KEY ([codEjercicio]) REFERENCES [dbo].[Ejercicio] ([codEjercicio])
    );
    CREATE INDEX [IX_RutinaEjercicio_Rutina] ON [dbo].[RutinaEjercicio] ([codRutina], [orden]);
    PRINT 'Tabla RutinaEjercicio creada.';
END
ELSE
BEGIN
    -- La columna calculada y la restricción del peso impiden cambiar la columna
    IF COL_LENGTH('dbo.RutinaEjercicio', 'volumen') IS NOT NULL
        ALTER TABLE [dbo].[RutinaEjercicio] DROP COLUMN [volumen];
    IF OBJECT_ID('dbo.CK_RutinaEjercicio_Peso', 'C') IS NOT NULL
        ALTER TABLE [dbo].[RutinaEjercicio] DROP CONSTRAINT [CK_RutinaEjercicio_Peso];
    IF OBJECT_ID('dbo.CK_RutinaEjercicio_Descanso', 'C') IS NOT NULL
        ALTER TABLE [dbo].[RutinaEjercicio] DROP CONSTRAINT [CK_RutinaEjercicio_Descanso];

    ALTER TABLE [dbo].[RutinaEjercicio] ALTER COLUMN [pesoLevantado] DECIMAL(6,2) NULL;

    IF COL_LENGTH('dbo.RutinaEjercicio', 'orden') IS NULL
        ALTER TABLE [dbo].[RutinaEjercicio] ADD [orden] INT NOT NULL CONSTRAINT [DF_RutinaEjercicio_orden] DEFAULT (0);
    IF COL_LENGTH('dbo.RutinaEjercicio', 'observaciones') IS NULL
        ALTER TABLE [dbo].[RutinaEjercicio] ADD [observaciones] NVARCHAR(300) NULL;

    PRINT 'Tabla RutinaEjercicio adaptada.';
END
GO

IF OBJECT_ID('dbo.CK_RutinaEjercicio_Series', 'C') IS NULL
    ALTER TABLE [dbo].[RutinaEjercicio] ADD CONSTRAINT [CK_RutinaEjercicio_Series] CHECK ([series] BETWEEN 1 AND 20);
IF OBJECT_ID('dbo.CK_RutinaEjercicio_Repeticiones', 'C') IS NULL
    ALTER TABLE [dbo].[RutinaEjercicio] ADD CONSTRAINT [CK_RutinaEjercicio_Repeticiones] CHECK ([repeticiones] BETWEEN 1 AND 100);
IF OBJECT_ID('dbo.CK_RutinaEjercicio_Peso', 'C') IS NULL
    ALTER TABLE [dbo].[RutinaEjercicio] ADD CONSTRAINT [CK_RutinaEjercicio_Peso] CHECK ([pesoLevantado] IS NULL OR ([pesoLevantado] > 0 AND [pesoLevantado] <= 500));
IF OBJECT_ID('dbo.CK_RutinaEjercicio_Descanso', 'C') IS NULL
    ALTER TABLE [dbo].[RutinaEjercicio] ADD CONSTRAINT [CK_RutinaEjercicio_Descanso] CHECK ([descansoSegundos] IS NULL OR [descansoSegundos] BETWEEN 0 AND 900);
GO

-- 3. El cliente y el familiar ven sus rutinas en modo lectura (CUN008): la familia
--    "Portal del cliente" suma el permiso GestionRutinas (la pantalla ya les muestra
--    solo sus rutinas y sin botones de edición).
INSERT INTO [dbo].[FamiliaPermiso] ([idFamilia], [idPermiso], [dvh])
SELECT f.idFamilia, p.idPermiso, ''
FROM [dbo].[Familia] f
JOIN [dbo].[Permiso] p ON p.nombre = 'GestionRutinas'
WHERE f.codigo = 'PORTAL_CLIENTE'
  AND NOT EXISTS (SELECT 1 FROM [dbo].[FamiliaPermiso] fp
                  WHERE fp.idFamilia = f.idFamilia AND fp.idPermiso = p.idPermiso);
GO

-- ============================================================================
-- 4. Traducciones
-- ============================================================================

IF COL_LENGTH('Traducciones.Pantalla_Rutinas', 'rutinas_ej_btn') IS NULL
    ALTER TABLE [Traducciones].[Pantalla_Rutinas] ADD [rutinas_ej_btn] NVARCHAR(500) NOT NULL DEFAULT N'';
IF COL_LENGTH('Traducciones.Pantalla_Rutinas', 'rutinas_ej_ver_detalle') IS NULL
    ALTER TABLE [Traducciones].[Pantalla_Rutinas] ADD [rutinas_ej_ver_detalle] NVARCHAR(500) NOT NULL DEFAULT N'';
IF COL_LENGTH('Traducciones.Pantalla_Rutinas', 'rutinas_ej_titulo') IS NULL
    ALTER TABLE [Traducciones].[Pantalla_Rutinas] ADD [rutinas_ej_titulo] NVARCHAR(500) NOT NULL DEFAULT N'';
IF COL_LENGTH('Traducciones.Pantalla_Rutinas', 'rutinas_ej_vacio') IS NULL
    ALTER TABLE [Traducciones].[Pantalla_Rutinas] ADD [rutinas_ej_vacio] NVARCHAR(500) NOT NULL DEFAULT N'';
IF COL_LENGTH('Traducciones.Pantalla_Rutinas', 'rutinas_ej_agregar_titulo') IS NULL
    ALTER TABLE [Traducciones].[Pantalla_Rutinas] ADD [rutinas_ej_agregar_titulo] NVARCHAR(500) NOT NULL DEFAULT N'';
IF COL_LENGTH('Traducciones.Pantalla_Rutinas', 'rutinas_ej_col_ejercicio') IS NULL
    ALTER TABLE [Traducciones].[Pantalla_Rutinas] ADD [rutinas_ej_col_ejercicio] NVARCHAR(500) NOT NULL DEFAULT N'';
IF COL_LENGTH('Traducciones.Pantalla_Rutinas', 'rutinas_ej_col_series') IS NULL
    ALTER TABLE [Traducciones].[Pantalla_Rutinas] ADD [rutinas_ej_col_series] NVARCHAR(500) NOT NULL DEFAULT N'';
IF COL_LENGTH('Traducciones.Pantalla_Rutinas', 'rutinas_ej_col_peso') IS NULL
    ALTER TABLE [Traducciones].[Pantalla_Rutinas] ADD [rutinas_ej_col_peso] NVARCHAR(500) NOT NULL DEFAULT N'';
IF COL_LENGTH('Traducciones.Pantalla_Rutinas', 'rutinas_ej_col_descanso') IS NULL
    ALTER TABLE [Traducciones].[Pantalla_Rutinas] ADD [rutinas_ej_col_descanso] NVARCHAR(500) NOT NULL DEFAULT N'';
IF COL_LENGTH('Traducciones.Pantalla_Rutinas', 'rutinas_ej_col_observaciones') IS NULL
    ALTER TABLE [Traducciones].[Pantalla_Rutinas] ADD [rutinas_ej_col_observaciones] NVARCHAR(500) NOT NULL DEFAULT N'';
IF COL_LENGTH('Traducciones.Pantalla_Rutinas', 'rutinas_ej_campo_series') IS NULL
    ALTER TABLE [Traducciones].[Pantalla_Rutinas] ADD [rutinas_ej_campo_series] NVARCHAR(500) NOT NULL DEFAULT N'';
IF COL_LENGTH('Traducciones.Pantalla_Rutinas', 'rutinas_ej_campo_reps') IS NULL
    ALTER TABLE [Traducciones].[Pantalla_Rutinas] ADD [rutinas_ej_campo_reps] NVARCHAR(500) NOT NULL DEFAULT N'';
IF COL_LENGTH('Traducciones.Pantalla_Rutinas', 'rutinas_ej_campo_peso') IS NULL
    ALTER TABLE [Traducciones].[Pantalla_Rutinas] ADD [rutinas_ej_campo_peso] NVARCHAR(500) NOT NULL DEFAULT N'';
IF COL_LENGTH('Traducciones.Pantalla_Rutinas', 'rutinas_ej_campo_descanso') IS NULL
    ALTER TABLE [Traducciones].[Pantalla_Rutinas] ADD [rutinas_ej_campo_descanso] NVARCHAR(500) NOT NULL DEFAULT N'';
IF COL_LENGTH('Traducciones.Pantalla_Rutinas', 'rutinas_ej_btn_agregar') IS NULL
    ALTER TABLE [Traducciones].[Pantalla_Rutinas] ADD [rutinas_ej_btn_agregar] NVARCHAR(500) NOT NULL DEFAULT N'';
IF COL_LENGTH('Traducciones.Pantalla_Rutinas', 'rutinas_ej_btn_cerrar') IS NULL
    ALTER TABLE [Traducciones].[Pantalla_Rutinas] ADD [rutinas_ej_btn_cerrar] NVARCHAR(500) NOT NULL DEFAULT N'';
IF COL_LENGTH('Traducciones.Pantalla_Rutinas', 'rutinas_ej_quitar') IS NULL
    ALTER TABLE [Traducciones].[Pantalla_Rutinas] ADD [rutinas_ej_quitar] NVARCHAR(500) NOT NULL DEFAULT N'';
IF COL_LENGTH('Traducciones.Pantalla_Rutinas', 'rutinas_ej_sel') IS NULL
    ALTER TABLE [Traducciones].[Pantalla_Rutinas] ADD [rutinas_ej_sel] NVARCHAR(500) NOT NULL DEFAULT N'';
IF COL_LENGTH('Traducciones.Pantalla_Rutinas', 'rutinas_ej_msg_agregado') IS NULL
    ALTER TABLE [Traducciones].[Pantalla_Rutinas] ADD [rutinas_ej_msg_agregado] NVARCHAR(500) NOT NULL DEFAULT N'';
IF COL_LENGTH('Traducciones.Pantalla_Rutinas', 'rutinas_ej_msg_quitado') IS NULL
    ALTER TABLE [Traducciones].[Pantalla_Rutinas] ADD [rutinas_ej_msg_quitado] NVARCHAR(500) NOT NULL DEFAULT N'';
IF COL_LENGTH('Traducciones.Pantalla_Rutinas', 'rutinas_ej_porcentaje_rm') IS NULL
    ALTER TABLE [Traducciones].[Pantalla_Rutinas] ADD [rutinas_ej_porcentaje_rm] NVARCHAR(500) NOT NULL DEFAULT N'';
IF COL_LENGTH('Traducciones.Pantalla_Rutinas', 'rutinas_ej_err_ejercicio') IS NULL
    ALTER TABLE [Traducciones].[Pantalla_Rutinas] ADD [rutinas_ej_err_ejercicio] NVARCHAR(500) NOT NULL DEFAULT N'';
IF COL_LENGTH('Traducciones.Pantalla_Rutinas', 'rutinas_ej_err_series') IS NULL
    ALTER TABLE [Traducciones].[Pantalla_Rutinas] ADD [rutinas_ej_err_series] NVARCHAR(500) NOT NULL DEFAULT N'';
IF COL_LENGTH('Traducciones.Pantalla_Rutinas', 'rutinas_ej_err_reps') IS NULL
    ALTER TABLE [Traducciones].[Pantalla_Rutinas] ADD [rutinas_ej_err_reps] NVARCHAR(500) NOT NULL DEFAULT N'';
IF COL_LENGTH('Traducciones.Pantalla_Rutinas', 'rutinas_ej_err_peso') IS NULL
    ALTER TABLE [Traducciones].[Pantalla_Rutinas] ADD [rutinas_ej_err_peso] NVARCHAR(500) NOT NULL DEFAULT N'';
IF COL_LENGTH('Traducciones.Pantalla_Rutinas', 'rutinas_ej_err_descanso') IS NULL
    ALTER TABLE [Traducciones].[Pantalla_Rutinas] ADD [rutinas_ej_err_descanso] NVARCHAR(500) NOT NULL DEFAULT N'';
IF COL_LENGTH('Traducciones.Pantalla_Rutinas', 'rutinas_ej_err_observaciones') IS NULL
    ALTER TABLE [Traducciones].[Pantalla_Rutinas] ADD [rutinas_ej_err_observaciones] NVARCHAR(500) NOT NULL DEFAULT N'';
IF COL_LENGTH('Traducciones.Pantalla_Rutinas', 'rutinas_ej_err_rutina') IS NULL
    ALTER TABLE [Traducciones].[Pantalla_Rutinas] ADD [rutinas_ej_err_rutina] NVARCHAR(500) NOT NULL DEFAULT N'';
IF COL_LENGTH('Traducciones.Pantalla_Rutinas', 'rutinas_ej_err_no_existe') IS NULL
    ALTER TABLE [Traducciones].[Pantalla_Rutinas] ADD [rutinas_ej_err_no_existe] NVARCHAR(500) NOT NULL DEFAULT N'';
IF COL_LENGTH('Traducciones.Pantalla_Rutinas', 'rutinas_ej_err_sin_permiso') IS NULL
    ALTER TABLE [Traducciones].[Pantalla_Rutinas] ADD [rutinas_ej_err_sin_permiso] NVARCHAR(500) NOT NULL DEFAULT N'';
GO

UPDATE [Traducciones].[Pantalla_Rutinas] SET
    [rutinas_ej_btn] = CASE [IdiomaID] WHEN 1 THEN N'Ejercicios' WHEN 2 THEN N'Exercises' WHEN 3 THEN N'Exercícios' WHEN 4 THEN N'Exercices' WHEN 5 THEN N'種目' ELSE N'Ejercicios' END,
    [rutinas_ej_ver_detalle] = CASE [IdiomaID] WHEN 1 THEN N'Ver ejercicios' WHEN 2 THEN N'View exercises' WHEN 3 THEN N'Ver exercícios' WHEN 4 THEN N'Voir les exercices' WHEN 5 THEN N'種目を見る' ELSE N'Ver ejercicios' END,
    [rutinas_ej_titulo] = CASE [IdiomaID] WHEN 1 THEN N'Ejercicios de la rutina' WHEN 2 THEN N'Routine exercises' WHEN 3 THEN N'Exercícios da rotina' WHEN 4 THEN N'Exercices de la routine' WHEN 5 THEN N'ルーティンの種目' ELSE N'Ejercicios de la rutina' END,
    [rutinas_ej_vacio] = CASE [IdiomaID] WHEN 1 THEN N'Esta rutina todavía no tiene ejercicios.' WHEN 2 THEN N'This routine has no exercises yet.' WHEN 3 THEN N'Esta rotina ainda não tem exercícios.' WHEN 4 THEN N'Cette routine na pas encore dexercices.' WHEN 5 THEN N'このルーティンにはまだ種目がありません。' ELSE N'Esta rutina todavía no tiene ejercicios.' END,
    [rutinas_ej_agregar_titulo] = CASE [IdiomaID] WHEN 1 THEN N'Agregar ejercicio' WHEN 2 THEN N'Add exercise' WHEN 3 THEN N'Adicionar exercício' WHEN 4 THEN N'Ajouter un exercice' WHEN 5 THEN N'種目を追加' ELSE N'Agregar ejercicio' END,
    [rutinas_ej_col_ejercicio] = CASE [IdiomaID] WHEN 1 THEN N'Ejercicio' WHEN 2 THEN N'Exercise' WHEN 3 THEN N'Exercício' WHEN 4 THEN N'Exercice' WHEN 5 THEN N'種目' ELSE N'Ejercicio' END,
    [rutinas_ej_col_series] = CASE [IdiomaID] WHEN 1 THEN N'Series × reps' WHEN 2 THEN N'Sets × reps' WHEN 3 THEN N'Séries × reps' WHEN 4 THEN N'Séries × reps' WHEN 5 THEN N'セット × 回数' ELSE N'Series × reps' END,
    [rutinas_ej_col_peso] = CASE [IdiomaID] WHEN 1 THEN N'Peso' WHEN 2 THEN N'Weight' WHEN 3 THEN N'Peso' WHEN 4 THEN N'Poids' WHEN 5 THEN N'重量' ELSE N'Peso' END,
    [rutinas_ej_col_descanso] = CASE [IdiomaID] WHEN 1 THEN N'Descanso' WHEN 2 THEN N'Rest' WHEN 3 THEN N'Descanso' WHEN 4 THEN N'Repos' WHEN 5 THEN N'休憩' ELSE N'Descanso' END,
    [rutinas_ej_col_observaciones] = CASE [IdiomaID] WHEN 1 THEN N'Observaciones' WHEN 2 THEN N'Notes' WHEN 3 THEN N'Observações' WHEN 4 THEN N'Remarques' WHEN 5 THEN N'メモ' ELSE N'Observaciones' END,
    [rutinas_ej_campo_series] = CASE [IdiomaID] WHEN 1 THEN N'Series' WHEN 2 THEN N'Sets' WHEN 3 THEN N'Séries' WHEN 4 THEN N'Séries' WHEN 5 THEN N'セット数' ELSE N'Series' END,
    [rutinas_ej_campo_reps] = CASE [IdiomaID] WHEN 1 THEN N'Repeticiones' WHEN 2 THEN N'Reps' WHEN 3 THEN N'Repetições' WHEN 4 THEN N'Répétitions' WHEN 5 THEN N'回数' ELSE N'Repeticiones' END,
    [rutinas_ej_campo_peso] = CASE [IdiomaID] WHEN 1 THEN N'Peso (kg)' WHEN 2 THEN N'Weight (kg)' WHEN 3 THEN N'Peso (kg)' WHEN 4 THEN N'Poids (kg)' WHEN 5 THEN N'重量（kg）' ELSE N'Peso (kg)' END,
    [rutinas_ej_campo_descanso] = CASE [IdiomaID] WHEN 1 THEN N'Descanso (seg)' WHEN 2 THEN N'Rest (sec)' WHEN 3 THEN N'Descanso (seg)' WHEN 4 THEN N'Repos (s)' WHEN 5 THEN N'休憩（秒）' ELSE N'Descanso (seg)' END,
    [rutinas_ej_btn_agregar] = CASE [IdiomaID] WHEN 1 THEN N'Agregar' WHEN 2 THEN N'Add' WHEN 3 THEN N'Adicionar' WHEN 4 THEN N'Ajouter' WHEN 5 THEN N'追加' ELSE N'Agregar' END,
    [rutinas_ej_btn_cerrar] = CASE [IdiomaID] WHEN 1 THEN N'Cerrar' WHEN 2 THEN N'Close' WHEN 3 THEN N'Fechar' WHEN 4 THEN N'Fermer' WHEN 5 THEN N'閉じる' ELSE N'Cerrar' END,
    [rutinas_ej_quitar] = CASE [IdiomaID] WHEN 1 THEN N'Quitar' WHEN 2 THEN N'Remove' WHEN 3 THEN N'Remover' WHEN 4 THEN N'Retirer' WHEN 5 THEN N'削除' ELSE N'Quitar' END,
    [rutinas_ej_sel] = CASE [IdiomaID] WHEN 1 THEN N'Seleccioná una rutina de la lista.' WHEN 2 THEN N'Select a routine from the list.' WHEN 3 THEN N'Selecione uma rotina da lista.' WHEN 4 THEN N'Sélectionnez une routine dans la liste.' WHEN 5 THEN N'一覧からルーティンを選択してください。' ELSE N'Seleccioná una rutina de la lista.' END,
    [rutinas_ej_msg_agregado] = CASE [IdiomaID] WHEN 1 THEN N'Ejercicio agregado.' WHEN 2 THEN N'Exercise added.' WHEN 3 THEN N'Exercício adicionado.' WHEN 4 THEN N'Exercice ajouté.' WHEN 5 THEN N'種目を追加しました。' ELSE N'Ejercicio agregado.' END,
    [rutinas_ej_msg_quitado] = CASE [IdiomaID] WHEN 1 THEN N'Ejercicio quitado.' WHEN 2 THEN N'Exercise removed.' WHEN 3 THEN N'Exercício removido.' WHEN 4 THEN N'Exercice retiré.' WHEN 5 THEN N'種目を削除しました。' ELSE N'Ejercicio quitado.' END,
    [rutinas_ej_porcentaje_rm] = CASE [IdiomaID] WHEN 1 THEN N'{0}% de la 1RM' WHEN 2 THEN N'{0}% of 1RM' WHEN 3 THEN N'{0}% da 1RM' WHEN 4 THEN N'{0} % du 1RM' WHEN 5 THEN N'1RMの{0}%' ELSE N'{0}% de la 1RM' END,
    [rutinas_ej_err_ejercicio] = CASE [IdiomaID] WHEN 1 THEN N'Elegí un ejercicio.' WHEN 2 THEN N'Choose an exercise.' WHEN 3 THEN N'Escolha um exercício.' WHEN 4 THEN N'Choisissez un exercice.' WHEN 5 THEN N'種目を選択してください。' ELSE N'Elegí un ejercicio.' END,
    [rutinas_ej_err_series] = CASE [IdiomaID] WHEN 1 THEN N'Las series deben ser un número entero entre 1 y {0}.' WHEN 2 THEN N'Sets must be a whole number between 1 and {0}.' WHEN 3 THEN N'As séries devem ser um número inteiro entre 1 e {0}.' WHEN 4 THEN N'Les séries doivent être un entier entre 1 et {0}.' WHEN 5 THEN N'セット数は1〜{0}の整数で入力してください。' ELSE N'Las series deben ser un número entero entre 1 y {0}.' END,
    [rutinas_ej_err_reps] = CASE [IdiomaID] WHEN 1 THEN N'Las repeticiones deben ser un número entero entre 1 y {0}.' WHEN 2 THEN N'Reps must be a whole number between 1 and {0}.' WHEN 3 THEN N'As repetições devem ser um número inteiro entre 1 e {0}.' WHEN 4 THEN N'Les répétitions doivent être un entier entre 1 et {0}.' WHEN 5 THEN N'回数は1〜{0}の整数で入力してください。' ELSE N'Las repeticiones deben ser un número entero entre 1 y {0}.' END,
    [rutinas_ej_err_peso] = CASE [IdiomaID] WHEN 1 THEN N'El peso debe ser mayor que 0 y hasta {0} kg.' WHEN 2 THEN N'Weight must be greater than 0 and up to {0} kg.' WHEN 3 THEN N'O peso deve ser maior que 0 e até {0} kg.' WHEN 4 THEN N'Le poids doit être supérieur à 0 et au plus {0} kg.' WHEN 5 THEN N'重量は0より大きく{0} kg以下で入力してください。' ELSE N'El peso debe ser mayor que 0 y hasta {0} kg.' END,
    [rutinas_ej_err_descanso] = CASE [IdiomaID] WHEN 1 THEN N'El descanso debe estar entre 0 y {0} segundos.' WHEN 2 THEN N'Rest must be between 0 and {0} seconds.' WHEN 3 THEN N'O descanso deve estar entre 0 e {0} segundos.' WHEN 4 THEN N'Le repos doit être compris entre 0 et {0} secondes.' WHEN 5 THEN N'休憩は0〜{0}秒で入力してください。' ELSE N'El descanso debe estar entre 0 y {0} segundos.' END,
    [rutinas_ej_err_observaciones] = CASE [IdiomaID] WHEN 1 THEN N'Las observaciones no pueden superar los {0} caracteres.' WHEN 2 THEN N'Notes cannot exceed {0} characters.' WHEN 3 THEN N'As observações não podem passar de {0} caracteres.' WHEN 4 THEN N'Les remarques ne peuvent pas dépasser {0} caractères.' WHEN 5 THEN N'メモは{0}文字以内で入力してください。' ELSE N'Las observaciones no pueden superar los {0} caracteres.' END,
    [rutinas_ej_err_rutina] = CASE [IdiomaID] WHEN 1 THEN N'La rutina ya no existe.' WHEN 2 THEN N'The routine no longer exists.' WHEN 3 THEN N'A rotina não existe mais.' WHEN 4 THEN N'La routine nexiste plus.' WHEN 5 THEN N'このルーティンはもう存在しません。' ELSE N'La rutina ya no existe.' END,
    [rutinas_ej_err_no_existe] = CASE [IdiomaID] WHEN 1 THEN N'El ejercicio ya no está en la rutina.' WHEN 2 THEN N'The exercise is no longer in the routine.' WHEN 3 THEN N'O exercício não está mais na rotina.' WHEN 4 THEN N'Lexercice ne fait plus partie de la routine.' WHEN 5 THEN N'この種目はもうルーティンにありません。' ELSE N'El ejercicio ya no está en la rutina.' END,
    [rutinas_ej_err_sin_permiso] = CASE [IdiomaID] WHEN 1 THEN N'No podés modificar las rutinas.' WHEN 2 THEN N'You cannot edit routines.' WHEN 3 THEN N'Você não pode alterar as rotinas.' WHEN 4 THEN N'Vous ne pouvez pas modifier les routines.' WHEN 5 THEN N'ルーティンは編集できません。' ELSE N'No podés modificar las rutinas.' END;
GO

IF COL_LENGTH('Traducciones.Pantalla_Alumnos', 'alumnos_err_no_existe') IS NULL
    ALTER TABLE [Traducciones].[Pantalla_Alumnos] ADD [alumnos_err_no_existe] NVARCHAR(500) NOT NULL DEFAULT N'';
IF COL_LENGTH('Traducciones.Pantalla_Alumnos', 'alumnos_err_ya_existe') IS NULL
    ALTER TABLE [Traducciones].[Pantalla_Alumnos] ADD [alumnos_err_ya_existe] NVARCHAR(500) NOT NULL DEFAULT N'';
IF COL_LENGTH('Traducciones.Pantalla_Alumnos', 'alumnos_err_usuario_no_existe') IS NULL
    ALTER TABLE [Traducciones].[Pantalla_Alumnos] ADD [alumnos_err_usuario_no_existe] NVARCHAR(500) NOT NULL DEFAULT N'';
IF COL_LENGTH('Traducciones.Pantalla_Alumnos', 'alumnos_err_ya_vinculado') IS NULL
    ALTER TABLE [Traducciones].[Pantalla_Alumnos] ADD [alumnos_err_ya_vinculado] NVARCHAR(500) NOT NULL DEFAULT N'';
IF COL_LENGTH('Traducciones.Pantalla_Alumnos', 'alumnos_err_datos') IS NULL
    ALTER TABLE [Traducciones].[Pantalla_Alumnos] ADD [alumnos_err_datos] NVARCHAR(500) NOT NULL DEFAULT N'';
GO

UPDATE [Traducciones].[Pantalla_Alumnos] SET
    [alumnos_err_no_existe] = CASE [IdiomaID] WHEN 1 THEN N'No existe un alumno con DNI {0}.' WHEN 2 THEN N'There is no student with ID {0}.' WHEN 3 THEN N'Não existe aluno com documento {0}.' WHEN 4 THEN N'Aucun élève avec la pièce didentité {0}.' WHEN 5 THEN N'DNI {0} の生徒は存在しません。' ELSE N'No existe un alumno con DNI {0}.' END,
    [alumnos_err_ya_existe] = CASE [IdiomaID] WHEN 1 THEN N'Ya existe un alumno con DNI {0}.' WHEN 2 THEN N'A student with ID {0} already exists.' WHEN 3 THEN N'Já existe um aluno com documento {0}.' WHEN 4 THEN N'Un élève avec la pièce didentité {0} existe déjà.' WHEN 5 THEN N'DNI {0} の生徒はすでに登録されています。' ELSE N'Ya existe un alumno con DNI {0}.' END,
    [alumnos_err_usuario_no_existe] = CASE [IdiomaID] WHEN 1 THEN N'No existe el usuario {0}.' WHEN 2 THEN N'User {0} does not exist.' WHEN 3 THEN N'O usuário {0} não existe.' WHEN 4 THEN N'Lutilisateur « {0} » nexiste pas.' WHEN 5 THEN N'ユーザー「{0}」は存在しません。' ELSE N'No existe el usuario {0}.' END,
    [alumnos_err_ya_vinculado] = CASE [IdiomaID] WHEN 1 THEN N'El usuario {0} ya está vinculado a este alumno.' WHEN 2 THEN N'User {0} is already linked to this student.' WHEN 3 THEN N'O usuário {0} já está vinculado a este aluno.' WHEN 4 THEN N'Lutilisateur « {0} » est déjà associé à cet élève.' WHEN 5 THEN N'ユーザー「{0}」はすでにこの生徒に紐づいています。' ELSE N'El usuario {0} ya está vinculado a este alumno.' END,
    [alumnos_err_datos] = CASE [IdiomaID] WHEN 1 THEN N'Faltan los datos del alumno.' WHEN 2 THEN N'Student data is missing.' WHEN 3 THEN N'Faltam os dados do aluno.' WHEN 4 THEN N'Les données de lélève sont manquantes.' WHEN 5 THEN N'生徒の情報が不足しています。' ELSE N'Faltan los datos del alumno.' END;
GO

SET NOEXEC OFF;
GO

PRINT 'Rutinas detalladas listas. Ahora: VerificacioDV -> Recalcular dígitos.';
GO
