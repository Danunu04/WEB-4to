USE [GymApp]
GO

-- ============================================================
-- Traducciones para: selección de varias clases en el calendario
-- (anotarse o darse de baja de varias clases juntas).
-- IdiomaID: 1 ES, 2 EN, 3 PT, 4 FR, 5 JA.
-- ============================================================

IF COL_LENGTH('Traducciones.Pantalla_Actividades', 'actividades_seleccion_fmt') IS NULL
    ALTER TABLE [Traducciones].[Pantalla_Actividades] ADD [actividades_seleccion_fmt] NVARCHAR(500) NOT NULL DEFAULT N'';
IF COL_LENGTH('Traducciones.Pantalla_Actividades', 'actividades_seleccionar_clase') IS NULL
    ALTER TABLE [Traducciones].[Pantalla_Actividades] ADD [actividades_seleccionar_clase] NVARCHAR(500) NOT NULL DEFAULT N'';
IF COL_LENGTH('Traducciones.Pantalla_Actividades', 'actividades_btn_anotar_seleccion') IS NULL
    ALTER TABLE [Traducciones].[Pantalla_Actividades] ADD [actividades_btn_anotar_seleccion] NVARCHAR(500) NOT NULL DEFAULT N'';
IF COL_LENGTH('Traducciones.Pantalla_Actividades', 'actividades_btn_baja_seleccion') IS NULL
    ALTER TABLE [Traducciones].[Pantalla_Actividades] ADD [actividades_btn_baja_seleccion] NVARCHAR(500) NOT NULL DEFAULT N'';
IF COL_LENGTH('Traducciones.Pantalla_Actividades', 'actividades_btn_limpiar_seleccion') IS NULL
    ALTER TABLE [Traducciones].[Pantalla_Actividades] ADD [actividades_btn_limpiar_seleccion] NVARCHAR(500) NOT NULL DEFAULT N'';
IF COL_LENGTH('Traducciones.Pantalla_Actividades', 'actividades_msg_anotado_varias_fmt') IS NULL
    ALTER TABLE [Traducciones].[Pantalla_Actividades] ADD [actividades_msg_anotado_varias_fmt] NVARCHAR(500) NOT NULL DEFAULT N'';
IF COL_LENGTH('Traducciones.Pantalla_Actividades', 'actividades_msg_desanotado_varias_fmt') IS NULL
    ALTER TABLE [Traducciones].[Pantalla_Actividades] ADD [actividades_msg_desanotado_varias_fmt] NVARCHAR(500) NOT NULL DEFAULT N'';
IF COL_LENGTH('Traducciones.Pantalla_Actividades', 'actividades_cliente_info') IS NULL
    ALTER TABLE [Traducciones].[Pantalla_Actividades] ADD [actividades_cliente_info] NVARCHAR(500) NOT NULL DEFAULT N'';
GO

UPDATE [Traducciones].[Pantalla_Actividades] SET
    [actividades_seleccion_fmt] = CASE [IdiomaID] WHEN 1 THEN N'{0} clases seleccionadas' WHEN 2 THEN N'{0} classes selected' WHEN 3 THEN N'{0} aulas selecionadas' WHEN 4 THEN N'{0} cours sélectionnés' WHEN 5 THEN N'{0} クラスを選択中' ELSE N'{0} clases seleccionadas' END,
    [actividades_seleccionar_clase] = CASE [IdiomaID] WHEN 1 THEN N'Seleccionar esta clase' WHEN 2 THEN N'Select this class' WHEN 3 THEN N'Selecionar esta aula' WHEN 4 THEN N'Sélectionner ce cours' WHEN 5 THEN N'このクラスを選択' ELSE N'Seleccionar esta clase' END,
    [actividades_btn_anotar_seleccion] = CASE [IdiomaID] WHEN 1 THEN N'Anotarme' WHEN 2 THEN N'Sign up' WHEN 3 THEN N'Inscrever-me' WHEN 4 THEN N'M''inscrire' WHEN 5 THEN N'申し込む' ELSE N'Anotarme' END,
    [actividades_btn_baja_seleccion] = CASE [IdiomaID] WHEN 1 THEN N'Darme de baja' WHEN 2 THEN N'Cancel' WHEN 3 THEN N'Cancelar inscrição' WHEN 4 THEN N'Me désinscrire' WHEN 5 THEN N'取り消す' ELSE N'Darme de baja' END,
    [actividades_btn_limpiar_seleccion] = CASE [IdiomaID] WHEN 1 THEN N'Limpiar selección' WHEN 2 THEN N'Clear selection' WHEN 3 THEN N'Limpar seleção' WHEN 4 THEN N'Effacer la sélection' WHEN 5 THEN N'選択を解除' ELSE N'Limpiar selección' END,
    [actividades_msg_anotado_varias_fmt] = CASE [IdiomaID] WHEN 1 THEN N'Te anotaste en {0} clases.' WHEN 2 THEN N'You signed up for {0} classes.' WHEN 3 THEN N'Você se inscreveu em {0} aulas.' WHEN 4 THEN N'Vous vous êtes inscrit à {0} cours.' WHEN 5 THEN N'{0} クラスに申し込みました。' ELSE N'Te anotaste en {0} clases.' END,
    [actividades_msg_desanotado_varias_fmt] = CASE [IdiomaID] WHEN 1 THEN N'Te diste de baja de {0} clases.' WHEN 2 THEN N'You cancelled {0} classes.' WHEN 3 THEN N'Você cancelou {0} aulas.' WHEN 4 THEN N'Vous vous êtes désinscrit de {0} cours.' WHEN 5 THEN N'{0} クラスの申し込みを取り消しました。' ELSE N'Te diste de baja de {0} clases.' END,
    [actividades_cliente_info] = CASE [IdiomaID] WHEN 1 THEN N'Las clases con ✓ son a las que vas. Tocá clases del calendario para seleccionar varias y anotarte o darte de baja de todas juntas, o tocá un día para ver más opciones.' WHEN 2 THEN N'Classes with ✓ are the ones you attend. Tap classes in the calendar to select several and sign up or cancel them together, or tap a day for more options.' WHEN 3 THEN N'As aulas com ✓ são as que você frequenta. Toque nas aulas do calendário para selecionar várias e se inscrever ou cancelar todas juntas, ou toque em um dia para ver mais opções.' WHEN 4 THEN N'Les cours marqués ✓ sont ceux auxquels vous participez. Touchez des cours du calendrier pour en sélectionner plusieurs et vous y inscrire ou vous désinscrire en une fois, ou touchez un jour pour plus d''options.' WHEN 5 THEN N'✓ のクラスが参加予定です。カレンダーのクラスをタップして複数選択し、まとめて申し込み・取り消しができます。日付をタップすると他のオプションも表示されます。' ELSE N'Las clases con ✓ son a las que vas. Tocá clases del calendario para seleccionar varias y anotarte o darte de baja de todas juntas, o tocá un día para ver más opciones.' END;
GO
