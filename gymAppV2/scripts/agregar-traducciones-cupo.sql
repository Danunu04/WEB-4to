USE [GymApp]
GO

-- ============================================================
-- Traducciones para: cupo de alumnos por clase (tope opcional de la actividad).
-- IdiomaID: 1 ES, 2 EN, 3 PT, 4 FR, 5 JA.
-- ============================================================

IF COL_LENGTH('Traducciones.Pantalla_Actividades', 'actividades_campo_cupo') IS NULL
    ALTER TABLE [Traducciones].[Pantalla_Actividades] ADD [actividades_campo_cupo] NVARCHAR(500) NOT NULL DEFAULT N'';
IF COL_LENGTH('Traducciones.Pantalla_Actividades', 'actividades_campo_cupo_ayuda') IS NULL
    ALTER TABLE [Traducciones].[Pantalla_Actividades] ADD [actividades_campo_cupo_ayuda] NVARCHAR(500) NOT NULL DEFAULT N'';
IF COL_LENGTH('Traducciones.Pantalla_Actividades', 'actividades_col_cupo') IS NULL
    ALTER TABLE [Traducciones].[Pantalla_Actividades] ADD [actividades_col_cupo] NVARCHAR(500) NOT NULL DEFAULT N'';
IF COL_LENGTH('Traducciones.Pantalla_Actividades', 'actividades_cupo_segun_aula') IS NULL
    ALTER TABLE [Traducciones].[Pantalla_Actividades] ADD [actividades_cupo_segun_aula] NVARCHAR(500) NOT NULL DEFAULT N'';
IF COL_LENGTH('Traducciones.Pantalla_Actividades', 'actividades_msg_cupo_invalido') IS NULL
    ALTER TABLE [Traducciones].[Pantalla_Actividades] ADD [actividades_msg_cupo_invalido] NVARCHAR(500) NOT NULL DEFAULT N'';
IF COL_LENGTH('Traducciones.Pantalla_Actividades', 'actividades_cupo_fmt') IS NULL
    ALTER TABLE [Traducciones].[Pantalla_Actividades] ADD [actividades_cupo_fmt] NVARCHAR(500) NOT NULL DEFAULT N'';
IF COL_LENGTH('Traducciones.Pantalla_Actividades', 'actividades_estado_completa') IS NULL
    ALTER TABLE [Traducciones].[Pantalla_Actividades] ADD [actividades_estado_completa] NVARCHAR(500) NOT NULL DEFAULT N'';
IF COL_LENGTH('Traducciones.Pantalla_Actividades', 'actividades_clase_completa_msg') IS NULL
    ALTER TABLE [Traducciones].[Pantalla_Actividades] ADD [actividades_clase_completa_msg] NVARCHAR(500) NOT NULL DEFAULT N'';
GO

UPDATE [Traducciones].[Pantalla_Actividades] SET
    [actividades_campo_cupo] = CASE [IdiomaID] WHEN 1 THEN N'Cupo máximo por clase (opcional)' WHEN 2 THEN N'Max. students per class (optional)' WHEN 3 THEN N'Vagas máximas por aula (opcional)' WHEN 4 THEN N'Places max. par cours (facultatif)' WHEN 5 THEN N'1クラスの最大人数（任意）' ELSE N'Cupo máximo por clase (opcional)' END,
    [actividades_campo_cupo_ayuda] = CASE [IdiomaID] WHEN 1 THEN N'Vacío = se usa el cupo del aula. Si ponés un número menor, la clase se completa antes.' WHEN 2 THEN N'Empty = the room capacity is used. A lower number fills the class sooner.' WHEN 3 THEN N'Vazio = usa as vagas da sala. Um número menor lota a aula antes.' WHEN 4 THEN N'Vide = capacité de la salle. Un nombre inférieur remplit le cours plus tôt.' WHEN 5 THEN N'空欄の場合は教室の定員を使用します。小さい数にすると早く満員になります。' ELSE N'Vacío = se usa el cupo del aula. Si ponés un número menor, la clase se completa antes.' END,
    [actividades_col_cupo] = CASE [IdiomaID] WHEN 1 THEN N'Cupo' WHEN 2 THEN N'Capacity' WHEN 3 THEN N'Vagas' WHEN 4 THEN N'Places' WHEN 5 THEN N'定員' ELSE N'Cupo' END,
    [actividades_cupo_segun_aula] = CASE [IdiomaID] WHEN 1 THEN N'Según aula' WHEN 2 THEN N'By room' WHEN 3 THEN N'Conforme a sala' WHEN 4 THEN N'Selon la salle' WHEN 5 THEN N'教室に準ずる' ELSE N'Según aula' END,
    [actividades_msg_cupo_invalido] = CASE [IdiomaID] WHEN 1 THEN N'El cupo máximo debe ser un número entero, o dejalo vacío para usar el del aula.' WHEN 2 THEN N'Max. capacity must be a whole number, or leave it empty to use the room''s.' WHEN 3 THEN N'As vagas máximas devem ser um número inteiro, ou deixe vazio para usar as da sala.' WHEN 4 THEN N'Le nombre max. doit être un entier, ou laissez vide pour utiliser celui de la salle.' WHEN 5 THEN N'最大人数は整数で入力するか、教室の定員を使う場合は空欄にしてください。' ELSE N'El cupo máximo debe ser un número entero, o dejalo vacío para usar el del aula.' END,
    [actividades_cupo_fmt] = CASE [IdiomaID] WHEN 1 THEN N'Anotados: {0} / {1}' WHEN 2 THEN N'Signed up: {0} / {1}' WHEN 3 THEN N'Inscritos: {0} / {1}' WHEN 4 THEN N'Inscrits : {0} / {1}' WHEN 5 THEN N'申込数：{0} / {1}' ELSE N'Anotados: {0} / {1}' END,
    [actividades_estado_completa] = CASE [IdiomaID] WHEN 1 THEN N'Completa' WHEN 2 THEN N'Full' WHEN 3 THEN N'Lotada' WHEN 4 THEN N'Complet' WHEN 5 THEN N'満員' ELSE N'Completa' END,
    [actividades_clase_completa_msg] = CASE [IdiomaID] WHEN 1 THEN N'Esta clase tiene el cupo completo: no se pueden anotar más alumnos.' WHEN 2 THEN N'This class is full: no more students can sign up.' WHEN 3 THEN N'Esta aula está lotada: não é possível inscrever mais alunos.' WHEN 4 THEN N'Ce cours est complet : impossible d''inscrire d''autres élèves.' WHEN 5 THEN N'このクラスは満員のため、これ以上申し込めません。' ELSE N'Esta clase tiene el cupo completo: no se pueden anotar más alumnos.' END;
GO
