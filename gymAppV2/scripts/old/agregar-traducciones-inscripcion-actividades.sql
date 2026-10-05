USE [GymApp]
GO

-- ============================================================
-- Traducciones para: autoinscripción del Cliente/Familiar en Actividades
-- (panel "Anotarme a actividades").
-- IdiomaID: 1 ES, 2 EN, 3 PT, 4 FR, 5 JA.
-- ============================================================

IF COL_LENGTH('Traducciones.Pantalla_Actividades', 'actividades_mis_titulo') IS NULL
    ALTER TABLE [Traducciones].[Pantalla_Actividades] ADD [actividades_mis_titulo] NVARCHAR(500) NOT NULL DEFAULT N'';
IF COL_LENGTH('Traducciones.Pantalla_Actividades', 'actividades_campo_alumno') IS NULL
    ALTER TABLE [Traducciones].[Pantalla_Actividades] ADD [actividades_campo_alumno] NVARCHAR(500) NOT NULL DEFAULT N'';
IF COL_LENGTH('Traducciones.Pantalla_Actividades', 'actividades_btn_anotarme') IS NULL
    ALTER TABLE [Traducciones].[Pantalla_Actividades] ADD [actividades_btn_anotarme] NVARCHAR(500) NOT NULL DEFAULT N'';
IF COL_LENGTH('Traducciones.Pantalla_Actividades', 'actividades_btn_desanotarme') IS NULL
    ALTER TABLE [Traducciones].[Pantalla_Actividades] ADD [actividades_btn_desanotarme] NVARCHAR(500) NOT NULL DEFAULT N'';
IF COL_LENGTH('Traducciones.Pantalla_Actividades', 'actividades_estado_anotado') IS NULL
    ALTER TABLE [Traducciones].[Pantalla_Actividades] ADD [actividades_estado_anotado] NVARCHAR(500) NOT NULL DEFAULT N'';
IF COL_LENGTH('Traducciones.Pantalla_Actividades', 'actividades_estado_no_anotado') IS NULL
    ALTER TABLE [Traducciones].[Pantalla_Actividades] ADD [actividades_estado_no_anotado] NVARCHAR(500) NOT NULL DEFAULT N'';
IF COL_LENGTH('Traducciones.Pantalla_Actividades', 'actividades_sin_alumnos_vinculados') IS NULL
    ALTER TABLE [Traducciones].[Pantalla_Actividades] ADD [actividades_sin_alumnos_vinculados] NVARCHAR(500) NOT NULL DEFAULT N'';
IF COL_LENGTH('Traducciones.Pantalla_Actividades', 'actividades_msg_anotado') IS NULL
    ALTER TABLE [Traducciones].[Pantalla_Actividades] ADD [actividades_msg_anotado] NVARCHAR(500) NOT NULL DEFAULT N'';
IF COL_LENGTH('Traducciones.Pantalla_Actividades', 'actividades_msg_desanotado') IS NULL
    ALTER TABLE [Traducciones].[Pantalla_Actividades] ADD [actividades_msg_desanotado] NVARCHAR(500) NOT NULL DEFAULT N'';
GO

UPDATE [Traducciones].[Pantalla_Actividades] SET
    [actividades_mis_titulo] = CASE [IdiomaID] WHEN 1 THEN N'Anotarme a actividades' WHEN 2 THEN N'Sign up for activities' WHEN 3 THEN N'Inscrever-me em atividades' WHEN 4 THEN N'M''inscrire aux activités' WHEN 5 THEN N'アクティビティに申し込む' ELSE N'Anotarme a actividades' END,
    [actividades_campo_alumno] = CASE [IdiomaID] WHEN 1 THEN N'Alumno' WHEN 2 THEN N'Student' WHEN 3 THEN N'Aluno' WHEN 4 THEN N'Élève' WHEN 5 THEN N'生徒' ELSE N'Alumno' END,
    [actividades_btn_anotarme] = CASE [IdiomaID] WHEN 1 THEN N'Anotarme' WHEN 2 THEN N'Sign up' WHEN 3 THEN N'Inscrever-me' WHEN 4 THEN N'M''inscrire' WHEN 5 THEN N'申し込む' ELSE N'Anotarme' END,
    [actividades_btn_desanotarme] = CASE [IdiomaID] WHEN 1 THEN N'Darme de baja' WHEN 2 THEN N'Cancel sign-up' WHEN 3 THEN N'Cancelar inscrição' WHEN 4 THEN N'Me désinscrire' WHEN 5 THEN N'申し込みを取り消す' ELSE N'Darme de baja' END,
    [actividades_estado_anotado] = CASE [IdiomaID] WHEN 1 THEN N'Anotado' WHEN 2 THEN N'Signed up' WHEN 3 THEN N'Inscrito' WHEN 4 THEN N'Inscrit' WHEN 5 THEN N'申込済み' ELSE N'Anotado' END,
    [actividades_estado_no_anotado] = CASE [IdiomaID] WHEN 1 THEN N'No anotado' WHEN 2 THEN N'Not signed up' WHEN 3 THEN N'Não inscrito' WHEN 4 THEN N'Non inscrit' WHEN 5 THEN N'未申込' ELSE N'No anotado' END,
    [actividades_sin_alumnos_vinculados] = CASE [IdiomaID] WHEN 1 THEN N'Tu cuenta no tiene alumnos vinculados. Contactá a recepción.' WHEN 2 THEN N'Your account has no linked students. Please contact the front desk.' WHEN 3 THEN N'Sua conta não tem alunos vinculados. Entre em contato com a recepção.' WHEN 4 THEN N'Votre compte n''a aucun élève associé. Contactez l''accueil.' WHEN 5 THEN N'アカウントに紐づく生徒がいません。受付にお問い合わせください。' ELSE N'Tu cuenta no tiene alumnos vinculados. Contactá a recepción.' END,
    [actividades_msg_anotado] = CASE [IdiomaID] WHEN 1 THEN N'Te anotaste en la actividad.' WHEN 2 THEN N'You signed up for the activity.' WHEN 3 THEN N'Você se inscreveu na atividade.' WHEN 4 THEN N'Vous êtes inscrit à l''activité.' WHEN 5 THEN N'アクティビティに申し込みました。' ELSE N'Te anotaste en la actividad.' END,
    [actividades_msg_desanotado] = CASE [IdiomaID] WHEN 1 THEN N'Te diste de baja de la actividad.' WHEN 2 THEN N'You cancelled your sign-up.' WHEN 3 THEN N'Você cancelou a inscrição.' WHEN 4 THEN N'Vous vous êtes désinscrit de l''activité.' WHEN 5 THEN N'アクティビティの申し込みを取り消しました。' ELSE N'Te diste de baja de la actividad.' END,
    [actividades_cliente_info] = CASE [IdiomaID] WHEN 1 THEN N'Se muestran las actividades en las que estás anotado. Podés anotarte o darte de baja más abajo.' WHEN 2 THEN N'Showing the activities you are signed up for. You can sign up or cancel below.' WHEN 3 THEN N'Mostrando as atividades em que você está inscrito. Você pode se inscrever ou cancelar abaixo.' WHEN 4 THEN N'Voici les activités auxquelles vous êtes inscrit. Vous pouvez vous inscrire ou vous désinscrire plus bas.' WHEN 5 THEN N'申し込み済みのアクティビティを表示しています。下で申し込み・取り消しができます。' ELSE N'Se muestran las actividades en las que estás anotado. Podés anotarte o darte de baja más abajo.' END;
GO
