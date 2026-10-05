USE [GymApp]
GO

-- ============================================================
-- Traducciones para: aulas del gimnasio y aula/profesores por turno.
-- IdiomaID: 1 ES, 2 EN, 3 PT, 4 FR, 5 JA.
-- ============================================================

IF COL_LENGTH('Traducciones.Pantalla_Actividades', 'actividades_campo_aula') IS NULL
    ALTER TABLE [Traducciones].[Pantalla_Actividades] ADD [actividades_campo_aula] NVARCHAR(500) NOT NULL DEFAULT N'';
IF COL_LENGTH('Traducciones.Pantalla_Actividades', 'actividades_campo_titular') IS NULL
    ALTER TABLE [Traducciones].[Pantalla_Actividades] ADD [actividades_campo_titular] NVARCHAR(500) NOT NULL DEFAULT N'';
IF COL_LENGTH('Traducciones.Pantalla_Actividades', 'actividades_campo_auxiliar') IS NULL
    ALTER TABLE [Traducciones].[Pantalla_Actividades] ADD [actividades_campo_auxiliar] NVARCHAR(500) NOT NULL DEFAULT N'';
IF COL_LENGTH('Traducciones.Pantalla_Actividades', 'actividades_elegir') IS NULL
    ALTER TABLE [Traducciones].[Pantalla_Actividades] ADD [actividades_elegir] NVARCHAR(500) NOT NULL DEFAULT N'';
IF COL_LENGTH('Traducciones.Pantalla_Actividades', 'actividades_sin_auxiliar') IS NULL
    ALTER TABLE [Traducciones].[Pantalla_Actividades] ADD [actividades_sin_auxiliar] NVARCHAR(500) NOT NULL DEFAULT N'';
IF COL_LENGTH('Traducciones.Pantalla_Actividades', 'actividades_quitar_horario') IS NULL
    ALTER TABLE [Traducciones].[Pantalla_Actividades] ADD [actividades_quitar_horario] NVARCHAR(500) NOT NULL DEFAULT N'';
IF COL_LENGTH('Traducciones.Pantalla_Actividades', 'aulas_titulo') IS NULL
    ALTER TABLE [Traducciones].[Pantalla_Actividades] ADD [aulas_titulo] NVARCHAR(500) NOT NULL DEFAULT N'';
IF COL_LENGTH('Traducciones.Pantalla_Actividades', 'aulas_subtitulo') IS NULL
    ALTER TABLE [Traducciones].[Pantalla_Actividades] ADD [aulas_subtitulo] NVARCHAR(500) NOT NULL DEFAULT N'';
IF COL_LENGTH('Traducciones.Pantalla_Actividades', 'aulas_btn_nueva') IS NULL
    ALTER TABLE [Traducciones].[Pantalla_Actividades] ADD [aulas_btn_nueva] NVARCHAR(500) NOT NULL DEFAULT N'';
IF COL_LENGTH('Traducciones.Pantalla_Actividades', 'aulas_col_nombre') IS NULL
    ALTER TABLE [Traducciones].[Pantalla_Actividades] ADD [aulas_col_nombre] NVARCHAR(500) NOT NULL DEFAULT N'';
IF COL_LENGTH('Traducciones.Pantalla_Actividades', 'aulas_col_cupo') IS NULL
    ALTER TABLE [Traducciones].[Pantalla_Actividades] ADD [aulas_col_cupo] NVARCHAR(500) NOT NULL DEFAULT N'';
IF COL_LENGTH('Traducciones.Pantalla_Actividades', 'aulas_col_turnos') IS NULL
    ALTER TABLE [Traducciones].[Pantalla_Actividades] ADD [aulas_col_turnos] NVARCHAR(500) NOT NULL DEFAULT N'';
IF COL_LENGTH('Traducciones.Pantalla_Actividades', 'aulas_sin_aulas') IS NULL
    ALTER TABLE [Traducciones].[Pantalla_Actividades] ADD [aulas_sin_aulas] NVARCHAR(500) NOT NULL DEFAULT N'';
IF COL_LENGTH('Traducciones.Pantalla_Actividades', 'aulas_form_nueva') IS NULL
    ALTER TABLE [Traducciones].[Pantalla_Actividades] ADD [aulas_form_nueva] NVARCHAR(500) NOT NULL DEFAULT N'';
IF COL_LENGTH('Traducciones.Pantalla_Actividades', 'aulas_form_editar') IS NULL
    ALTER TABLE [Traducciones].[Pantalla_Actividades] ADD [aulas_form_editar] NVARCHAR(500) NOT NULL DEFAULT N'';
IF COL_LENGTH('Traducciones.Pantalla_Actividades', 'aulas_campo_cupo') IS NULL
    ALTER TABLE [Traducciones].[Pantalla_Actividades] ADD [aulas_campo_cupo] NVARCHAR(500) NOT NULL DEFAULT N'';
IF COL_LENGTH('Traducciones.Pantalla_Actividades', 'aulas_campo_cupo_ayuda') IS NULL
    ALTER TABLE [Traducciones].[Pantalla_Actividades] ADD [aulas_campo_cupo_ayuda] NVARCHAR(500) NOT NULL DEFAULT N'';
IF COL_LENGTH('Traducciones.Pantalla_Actividades', 'aulas_msg_cupo_invalido') IS NULL
    ALTER TABLE [Traducciones].[Pantalla_Actividades] ADD [aulas_msg_cupo_invalido] NVARCHAR(500) NOT NULL DEFAULT N'';
IF COL_LENGTH('Traducciones.Pantalla_Actividades', 'aulas_msg_guardada') IS NULL
    ALTER TABLE [Traducciones].[Pantalla_Actividades] ADD [aulas_msg_guardada] NVARCHAR(500) NOT NULL DEFAULT N'';
IF COL_LENGTH('Traducciones.Pantalla_Actividades', 'aulas_msg_estado') IS NULL
    ALTER TABLE [Traducciones].[Pantalla_Actividades] ADD [aulas_msg_estado] NVARCHAR(500) NOT NULL DEFAULT N'';
GO

UPDATE [Traducciones].[Pantalla_Actividades] SET
    [actividades_campo_aula] = CASE [IdiomaID] WHEN 1 THEN N'Aula' WHEN 2 THEN N'Room' WHEN 3 THEN N'Sala' WHEN 4 THEN N'Salle' WHEN 5 THEN N'教室' ELSE N'Aula' END,
    [actividades_campo_titular] = CASE [IdiomaID] WHEN 1 THEN N'Profesor titular' WHEN 2 THEN N'Lead instructor' WHEN 3 THEN N'Professor titular' WHEN 4 THEN N'Moniteur principal' WHEN 5 THEN N'担当インストラクター' ELSE N'Profesor titular' END,
    [actividades_campo_auxiliar] = CASE [IdiomaID] WHEN 1 THEN N'Profesor auxiliar' WHEN 2 THEN N'Assistant instructor' WHEN 3 THEN N'Professor auxiliar' WHEN 4 THEN N'Moniteur assistant' WHEN 5 THEN N'補助インストラクター' ELSE N'Profesor auxiliar' END,
    [actividades_elegir] = CASE [IdiomaID] WHEN 1 THEN N'— Elegí —' WHEN 2 THEN N'— Choose —' WHEN 3 THEN N'— Escolha —' WHEN 4 THEN N'— Choisir —' WHEN 5 THEN N'— 選択 —' ELSE N'— Elegí —' END,
    [actividades_sin_auxiliar] = CASE [IdiomaID] WHEN 1 THEN N'Sin auxiliar' WHEN 2 THEN N'No assistant' WHEN 3 THEN N'Sem auxiliar' WHEN 4 THEN N'Sans assistant' WHEN 5 THEN N'補助なし' ELSE N'Sin auxiliar' END,
    [actividades_quitar_horario] = CASE [IdiomaID] WHEN 1 THEN N'Quitar horario' WHEN 2 THEN N'Remove schedule' WHEN 3 THEN N'Remover horário' WHEN 4 THEN N'Retirer l''horaire' WHEN 5 THEN N'時間帯を削除' ELSE N'Quitar horario' END,
    [aulas_titulo] = CASE [IdiomaID] WHEN 1 THEN N'Aulas' WHEN 2 THEN N'Rooms' WHEN 3 THEN N'Salas' WHEN 4 THEN N'Salles' WHEN 5 THEN N'教室' ELSE N'Aulas' END,
    [aulas_subtitulo] = CASE [IdiomaID] WHEN 1 THEN N'Salones del gimnasio y cuántas personas entran en cada uno.' WHEN 2 THEN N'Gym rooms and how many people fit in each.' WHEN 3 THEN N'Salas da academia e quantas pessoas cabem em cada uma.' WHEN 4 THEN N'Salles de la salle de sport et leur capacité.' WHEN 5 THEN N'ジムの教室と各教室の定員。' ELSE N'Salones del gimnasio y cuántas personas entran en cada uno.' END,
    [aulas_btn_nueva] = CASE [IdiomaID] WHEN 1 THEN N'Nueva aula' WHEN 2 THEN N'New room' WHEN 3 THEN N'Nova sala' WHEN 4 THEN N'Nouvelle salle' WHEN 5 THEN N'新しい教室' ELSE N'Nueva aula' END,
    [aulas_col_nombre] = CASE [IdiomaID] WHEN 1 THEN N'Nombre' WHEN 2 THEN N'Name' WHEN 3 THEN N'Nome' WHEN 4 THEN N'Nom' WHEN 5 THEN N'名前' ELSE N'Nombre' END,
    [aulas_col_cupo] = CASE [IdiomaID] WHEN 1 THEN N'Cupo' WHEN 2 THEN N'Capacity' WHEN 3 THEN N'Capacidade' WHEN 4 THEN N'Capacité' WHEN 5 THEN N'定員' ELSE N'Cupo' END,
    [aulas_col_turnos] = CASE [IdiomaID] WHEN 1 THEN N'Turnos que la usan' WHEN 2 THEN N'Time slots using it' WHEN 3 THEN N'Horários que a usam' WHEN 4 THEN N'Créneaux qui l''utilisent' WHEN 5 THEN N'使用中の時間帯' ELSE N'Turnos que la usan' END,
    [aulas_sin_aulas] = CASE [IdiomaID] WHEN 1 THEN N'Todavía no hay aulas cargadas.' WHEN 2 THEN N'No rooms yet.' WHEN 3 THEN N'Ainda não há salas cadastradas.' WHEN 4 THEN N'Aucune salle pour l''instant.' WHEN 5 THEN N'まだ教室が登録されていません。' ELSE N'Todavía no hay aulas cargadas.' END,
    [aulas_form_nueva] = CASE [IdiomaID] WHEN 1 THEN N'Nueva aula' WHEN 2 THEN N'New room' WHEN 3 THEN N'Nova sala' WHEN 4 THEN N'Nouvelle salle' WHEN 5 THEN N'新しい教室' ELSE N'Nueva aula' END,
    [aulas_form_editar] = CASE [IdiomaID] WHEN 1 THEN N'Editar aula' WHEN 2 THEN N'Edit room' WHEN 3 THEN N'Editar sala' WHEN 4 THEN N'Modifier la salle' WHEN 5 THEN N'教室を編集' ELSE N'Editar aula' END,
    [aulas_campo_cupo] = CASE [IdiomaID] WHEN 1 THEN N'Cupo (personas)' WHEN 2 THEN N'Capacity (people)' WHEN 3 THEN N'Capacidade (pessoas)' WHEN 4 THEN N'Capacité (personnes)' WHEN 5 THEN N'定員（人）' ELSE N'Cupo (personas)' END,
    [aulas_campo_cupo_ayuda] = CASE [IdiomaID] WHEN 1 THEN N'Máximo de alumnos por clase en esta aula.' WHEN 2 THEN N'Max. students per class in this room.' WHEN 3 THEN N'Máximo de alunos por aula nesta sala.' WHEN 4 THEN N'Nombre max. d''élèves par cours dans cette salle.' WHEN 5 THEN N'この教室での1クラスあたりの最大生徒数。' ELSE N'Máximo de alumnos por clase en esta aula.' END,
    [aulas_msg_cupo_invalido] = CASE [IdiomaID] WHEN 1 THEN N'El cupo debe ser un número entero entre 1 y 1000.' WHEN 2 THEN N'Capacity must be a whole number between 1 and 1000.' WHEN 3 THEN N'A capacidade deve ser um número inteiro entre 1 e 1000.' WHEN 4 THEN N'La capacité doit être un entier entre 1 et 1000.' WHEN 5 THEN N'定員は1〜1000の整数で入力してください。' ELSE N'El cupo debe ser un número entero entre 1 y 1000.' END,
    [aulas_msg_guardada] = CASE [IdiomaID] WHEN 1 THEN N'Aula guardada.' WHEN 2 THEN N'Room saved.' WHEN 3 THEN N'Sala salva.' WHEN 4 THEN N'Salle enregistrée.' WHEN 5 THEN N'教室を保存しました。' ELSE N'Aula guardada.' END,
    [aulas_msg_estado] = CASE [IdiomaID] WHEN 1 THEN N'Estado del aula actualizado.' WHEN 2 THEN N'Room status updated.' WHEN 3 THEN N'Status da sala atualizado.' WHEN 4 THEN N'Statut de la salle mis à jour.' WHEN 5 THEN N'教室の状態を更新しました。' ELSE N'Estado del aula actualizado.' END;
GO

IF COL_LENGTH('Traducciones.Pantalla_DashBoard', 'menu_aulas') IS NULL
    ALTER TABLE [Traducciones].[Pantalla_DashBoard] ADD [menu_aulas] NVARCHAR(500) NOT NULL DEFAULT N'';
GO

UPDATE [Traducciones].[Pantalla_DashBoard] SET
    [menu_aulas] = CASE [IdiomaID] WHEN 1 THEN N'Aulas' WHEN 2 THEN N'Rooms' WHEN 3 THEN N'Salas' WHEN 4 THEN N'Salles' WHEN 5 THEN N'教室' ELSE N'Aulas' END;
GO

IF COL_LENGTH('Traducciones.Pantalla_Permisos', 'permisos_nombre_gestionaulas') IS NULL
    ALTER TABLE [Traducciones].[Pantalla_Permisos] ADD [permisos_nombre_gestionaulas] NVARCHAR(500) NOT NULL DEFAULT N'';
GO

UPDATE [Traducciones].[Pantalla_Permisos] SET
    [permisos_nombre_gestionaulas] = CASE [IdiomaID] WHEN 1 THEN N'Gestión de aulas' WHEN 2 THEN N'Room management' WHEN 3 THEN N'Gestão de salas' WHEN 4 THEN N'Gestion des salles' WHEN 5 THEN N'教室の管理' ELSE N'Gestión de aulas' END;
GO
