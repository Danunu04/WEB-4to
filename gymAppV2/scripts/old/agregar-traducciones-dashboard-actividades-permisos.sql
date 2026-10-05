USE [GymApp]
GO

-- ============================================================
-- Traducciones para: Dashboard con datos reales, gestión de Actividades
-- (horarios, instructores, inscripciones) y pantalla de Permisos por rol.
-- IdiomaID: 1 ES, 2 EN, 3 PT, 4 FR, 5 JA.
-- ============================================================

IF COL_LENGTH('Traducciones.Pantalla_DashboardContent', 'dash_kpi_al_dia') IS NULL
    ALTER TABLE [Traducciones].[Pantalla_DashboardContent] ADD [dash_kpi_al_dia] NVARCHAR(500) NOT NULL DEFAULT N'';
IF COL_LENGTH('Traducciones.Pantalla_DashboardContent', 'dash_sin_horarios') IS NULL
    ALTER TABLE [Traducciones].[Pantalla_DashboardContent] ADD [dash_sin_horarios] NVARCHAR(500) NOT NULL DEFAULT N'';
GO

UPDATE [Traducciones].[Pantalla_DashboardContent] SET
    [dash_kpi_al_dia] = CASE [IdiomaID] WHEN 1 THEN N'Cuotas al día' WHEN 2 THEN N'Fees up to date' WHEN 3 THEN N'Mensalidades em dia' WHEN 4 THEN N'Cotisations à jour' WHEN 5 THEN N'会費支払済み' ELSE N'Cuotas al día' END,
    [dash_sin_horarios] = CASE [IdiomaID] WHEN 1 THEN N'No hay actividades con horarios cargados.' WHEN 2 THEN N'There are no activities with schedules.' WHEN 3 THEN N'Não há atividades com horários cadastrados.' WHEN 4 THEN N'Aucune activité avec des horaires.' WHEN 5 THEN N'スケジュールが登録されたアクティビティはありません。' ELSE N'No hay actividades con horarios cargados.' END;
GO

IF COL_LENGTH('Traducciones.Pantalla_Actividades', 'actividades_gestion_titulo') IS NULL
    ALTER TABLE [Traducciones].[Pantalla_Actividades] ADD [actividades_gestion_titulo] NVARCHAR(500) NOT NULL DEFAULT N'';
IF COL_LENGTH('Traducciones.Pantalla_Actividades', 'actividades_col_descripcion') IS NULL
    ALTER TABLE [Traducciones].[Pantalla_Actividades] ADD [actividades_col_descripcion] NVARCHAR(500) NOT NULL DEFAULT N'';
IF COL_LENGTH('Traducciones.Pantalla_Actividades', 'actividades_col_horarios') IS NULL
    ALTER TABLE [Traducciones].[Pantalla_Actividades] ADD [actividades_col_horarios] NVARCHAR(500) NOT NULL DEFAULT N'';
IF COL_LENGTH('Traducciones.Pantalla_Actividades', 'actividades_col_instructores') IS NULL
    ALTER TABLE [Traducciones].[Pantalla_Actividades] ADD [actividades_col_instructores] NVARCHAR(500) NOT NULL DEFAULT N'';
IF COL_LENGTH('Traducciones.Pantalla_Actividades', 'actividades_col_precio') IS NULL
    ALTER TABLE [Traducciones].[Pantalla_Actividades] ADD [actividades_col_precio] NVARCHAR(500) NOT NULL DEFAULT N'';
IF COL_LENGTH('Traducciones.Pantalla_Actividades', 'actividades_estado_activa') IS NULL
    ALTER TABLE [Traducciones].[Pantalla_Actividades] ADD [actividades_estado_activa] NVARCHAR(500) NOT NULL DEFAULT N'';
IF COL_LENGTH('Traducciones.Pantalla_Actividades', 'actividades_estado_inactiva') IS NULL
    ALTER TABLE [Traducciones].[Pantalla_Actividades] ADD [actividades_estado_inactiva] NVARCHAR(500) NOT NULL DEFAULT N'';
IF COL_LENGTH('Traducciones.Pantalla_Actividades', 'actividades_btn_editar') IS NULL
    ALTER TABLE [Traducciones].[Pantalla_Actividades] ADD [actividades_btn_editar] NVARCHAR(500) NOT NULL DEFAULT N'';
IF COL_LENGTH('Traducciones.Pantalla_Actividades', 'actividades_btn_alumnos') IS NULL
    ALTER TABLE [Traducciones].[Pantalla_Actividades] ADD [actividades_btn_alumnos] NVARCHAR(500) NOT NULL DEFAULT N'';
IF COL_LENGTH('Traducciones.Pantalla_Actividades', 'actividades_btn_desactivar') IS NULL
    ALTER TABLE [Traducciones].[Pantalla_Actividades] ADD [actividades_btn_desactivar] NVARCHAR(500) NOT NULL DEFAULT N'';
IF COL_LENGTH('Traducciones.Pantalla_Actividades', 'actividades_btn_activar') IS NULL
    ALTER TABLE [Traducciones].[Pantalla_Actividades] ADD [actividades_btn_activar] NVARCHAR(500) NOT NULL DEFAULT N'';
IF COL_LENGTH('Traducciones.Pantalla_Actividades', 'actividades_btn_agregar_horario') IS NULL
    ALTER TABLE [Traducciones].[Pantalla_Actividades] ADD [actividades_btn_agregar_horario] NVARCHAR(500) NOT NULL DEFAULT N'';
IF COL_LENGTH('Traducciones.Pantalla_Actividades', 'actividades_form_nueva') IS NULL
    ALTER TABLE [Traducciones].[Pantalla_Actividades] ADD [actividades_form_nueva] NVARCHAR(500) NOT NULL DEFAULT N'';
IF COL_LENGTH('Traducciones.Pantalla_Actividades', 'actividades_form_editar') IS NULL
    ALTER TABLE [Traducciones].[Pantalla_Actividades] ADD [actividades_form_editar] NVARCHAR(500) NOT NULL DEFAULT N'';
IF COL_LENGTH('Traducciones.Pantalla_Actividades', 'actividades_campo_costo') IS NULL
    ALTER TABLE [Traducciones].[Pantalla_Actividades] ADD [actividades_campo_costo] NVARCHAR(500) NOT NULL DEFAULT N'';
IF COL_LENGTH('Traducciones.Pantalla_Actividades', 'actividades_campo_precio') IS NULL
    ALTER TABLE [Traducciones].[Pantalla_Actividades] ADD [actividades_campo_precio] NVARCHAR(500) NOT NULL DEFAULT N'';
IF COL_LENGTH('Traducciones.Pantalla_Actividades', 'actividades_campo_activa') IS NULL
    ALTER TABLE [Traducciones].[Pantalla_Actividades] ADD [actividades_campo_activa] NVARCHAR(500) NOT NULL DEFAULT N'';
IF COL_LENGTH('Traducciones.Pantalla_Actividades', 'actividades_campo_dia') IS NULL
    ALTER TABLE [Traducciones].[Pantalla_Actividades] ADD [actividades_campo_dia] NVARCHAR(500) NOT NULL DEFAULT N'';
IF COL_LENGTH('Traducciones.Pantalla_Actividades', 'actividades_horarios_titulo') IS NULL
    ALTER TABLE [Traducciones].[Pantalla_Actividades] ADD [actividades_horarios_titulo] NVARCHAR(500) NOT NULL DEFAULT N'';
IF COL_LENGTH('Traducciones.Pantalla_Actividades', 'actividades_instructores_titulo') IS NULL
    ALTER TABLE [Traducciones].[Pantalla_Actividades] ADD [actividades_instructores_titulo] NVARCHAR(500) NOT NULL DEFAULT N'';
IF COL_LENGTH('Traducciones.Pantalla_Actividades', 'actividades_sin_entrenadores') IS NULL
    ALTER TABLE [Traducciones].[Pantalla_Actividades] ADD [actividades_sin_entrenadores] NVARCHAR(500) NOT NULL DEFAULT N'';
IF COL_LENGTH('Traducciones.Pantalla_Actividades', 'actividades_sin_alumnos') IS NULL
    ALTER TABLE [Traducciones].[Pantalla_Actividades] ADD [actividades_sin_alumnos] NVARCHAR(500) NOT NULL DEFAULT N'';
IF COL_LENGTH('Traducciones.Pantalla_Actividades', 'actividades_sin_actividades') IS NULL
    ALTER TABLE [Traducciones].[Pantalla_Actividades] ADD [actividades_sin_actividades] NVARCHAR(500) NOT NULL DEFAULT N'';
IF COL_LENGTH('Traducciones.Pantalla_Actividades', 'actividades_sin_instructor') IS NULL
    ALTER TABLE [Traducciones].[Pantalla_Actividades] ADD [actividades_sin_instructor] NVARCHAR(500) NOT NULL DEFAULT N'';
IF COL_LENGTH('Traducciones.Pantalla_Actividades', 'actividades_inscripciones_titulo_fmt') IS NULL
    ALTER TABLE [Traducciones].[Pantalla_Actividades] ADD [actividades_inscripciones_titulo_fmt] NVARCHAR(500) NOT NULL DEFAULT N'';
IF COL_LENGTH('Traducciones.Pantalla_Actividades', 'actividades_confirmar_baja_titulo') IS NULL
    ALTER TABLE [Traducciones].[Pantalla_Actividades] ADD [actividades_confirmar_baja_titulo] NVARCHAR(500) NOT NULL DEFAULT N'';
IF COL_LENGTH('Traducciones.Pantalla_Actividades', 'actividades_confirmar_baja_msg_fmt') IS NULL
    ALTER TABLE [Traducciones].[Pantalla_Actividades] ADD [actividades_confirmar_baja_msg_fmt] NVARCHAR(500) NOT NULL DEFAULT N'';
IF COL_LENGTH('Traducciones.Pantalla_Actividades', 'actividades_msg_guardada') IS NULL
    ALTER TABLE [Traducciones].[Pantalla_Actividades] ADD [actividades_msg_guardada] NVARCHAR(500) NOT NULL DEFAULT N'';
IF COL_LENGTH('Traducciones.Pantalla_Actividades', 'actividades_msg_estado') IS NULL
    ALTER TABLE [Traducciones].[Pantalla_Actividades] ADD [actividades_msg_estado] NVARCHAR(500) NOT NULL DEFAULT N'';
IF COL_LENGTH('Traducciones.Pantalla_Actividades', 'actividades_msg_inscripciones_guardadas') IS NULL
    ALTER TABLE [Traducciones].[Pantalla_Actividades] ADD [actividades_msg_inscripciones_guardadas] NVARCHAR(500) NOT NULL DEFAULT N'';
IF COL_LENGTH('Traducciones.Pantalla_Actividades', 'actividades_msg_importe_invalido') IS NULL
    ALTER TABLE [Traducciones].[Pantalla_Actividades] ADD [actividades_msg_importe_invalido] NVARCHAR(500) NOT NULL DEFAULT N'';
IF COL_LENGTH('Traducciones.Pantalla_Actividades', 'actividades_msg_horario_invalido') IS NULL
    ALTER TABLE [Traducciones].[Pantalla_Actividades] ADD [actividades_msg_horario_invalido] NVARCHAR(500) NOT NULL DEFAULT N'';
GO

UPDATE [Traducciones].[Pantalla_Actividades] SET
    [actividades_gestion_titulo] = CASE [IdiomaID] WHEN 1 THEN N'Actividades' WHEN 2 THEN N'Activities' WHEN 3 THEN N'Atividades' WHEN 4 THEN N'Activités' WHEN 5 THEN N'アクティビティ' ELSE N'Actividades' END,
    [actividades_col_descripcion] = CASE [IdiomaID] WHEN 1 THEN N'Actividad' WHEN 2 THEN N'Activity' WHEN 3 THEN N'Atividade' WHEN 4 THEN N'Activité' WHEN 5 THEN N'アクティビティ' ELSE N'Actividad' END,
    [actividades_col_horarios] = CASE [IdiomaID] WHEN 1 THEN N'Horarios' WHEN 2 THEN N'Schedule' WHEN 3 THEN N'Horários' WHEN 4 THEN N'Horaires' WHEN 5 THEN N'スケジュール' ELSE N'Horarios' END,
    [actividades_col_instructores] = CASE [IdiomaID] WHEN 1 THEN N'Instructores' WHEN 2 THEN N'Instructors' WHEN 3 THEN N'Instrutores' WHEN 4 THEN N'Moniteurs' WHEN 5 THEN N'インストラクター' ELSE N'Instructores' END,
    [actividades_col_precio] = CASE [IdiomaID] WHEN 1 THEN N'Precio' WHEN 2 THEN N'Price' WHEN 3 THEN N'Preço' WHEN 4 THEN N'Prix' WHEN 5 THEN N'料金' ELSE N'Precio' END,
    [actividades_estado_activa] = CASE [IdiomaID] WHEN 1 THEN N'Activa' WHEN 2 THEN N'Active' WHEN 3 THEN N'Ativa' WHEN 4 THEN N'Active' WHEN 5 THEN N'有効' ELSE N'Activa' END,
    [actividades_estado_inactiva] = CASE [IdiomaID] WHEN 1 THEN N'Inactiva' WHEN 2 THEN N'Inactive' WHEN 3 THEN N'Inativa' WHEN 4 THEN N'Inactive' WHEN 5 THEN N'無効' ELSE N'Inactiva' END,
    [actividades_btn_editar] = CASE [IdiomaID] WHEN 1 THEN N'Editar' WHEN 2 THEN N'Edit' WHEN 3 THEN N'Editar' WHEN 4 THEN N'Modifier' WHEN 5 THEN N'編集' ELSE N'Editar' END,
    [actividades_btn_alumnos] = CASE [IdiomaID] WHEN 1 THEN N'Alumnos' WHEN 2 THEN N'Students' WHEN 3 THEN N'Alunos' WHEN 4 THEN N'Élèves' WHEN 5 THEN N'生徒' ELSE N'Alumnos' END,
    [actividades_btn_desactivar] = CASE [IdiomaID] WHEN 1 THEN N'Dar de baja' WHEN 2 THEN N'Deactivate' WHEN 3 THEN N'Desativar' WHEN 4 THEN N'Désactiver' WHEN 5 THEN N'無効にする' ELSE N'Dar de baja' END,
    [actividades_btn_activar] = CASE [IdiomaID] WHEN 1 THEN N'Reactivar' WHEN 2 THEN N'Reactivate' WHEN 3 THEN N'Reativar' WHEN 4 THEN N'Réactiver' WHEN 5 THEN N'再有効化' ELSE N'Reactivar' END,
    [actividades_btn_agregar_horario] = CASE [IdiomaID] WHEN 1 THEN N'Agregar horario' WHEN 2 THEN N'Add schedule' WHEN 3 THEN N'Adicionar horário' WHEN 4 THEN N'Ajouter un horaire' WHEN 5 THEN N'時間帯を追加' ELSE N'Agregar horario' END,
    [actividades_form_nueva] = CASE [IdiomaID] WHEN 1 THEN N'Nueva actividad' WHEN 2 THEN N'New activity' WHEN 3 THEN N'Nova atividade' WHEN 4 THEN N'Nouvelle activité' WHEN 5 THEN N'新しいアクティビティ' ELSE N'Nueva actividad' END,
    [actividades_form_editar] = CASE [IdiomaID] WHEN 1 THEN N'Editar actividad' WHEN 2 THEN N'Edit activity' WHEN 3 THEN N'Editar atividade' WHEN 4 THEN N'Modifier l''activité' WHEN 5 THEN N'アクティビティを編集' ELSE N'Editar actividad' END,
    [actividades_campo_costo] = CASE [IdiomaID] WHEN 1 THEN N'Costo interno' WHEN 2 THEN N'Internal cost' WHEN 3 THEN N'Custo interno' WHEN 4 THEN N'Coût interne' WHEN 5 THEN N'内部コスト' ELSE N'Costo interno' END,
    [actividades_campo_precio] = CASE [IdiomaID] WHEN 1 THEN N'Precio por alumno' WHEN 2 THEN N'Price per student' WHEN 3 THEN N'Preço por aluno' WHEN 4 THEN N'Prix par élève' WHEN 5 THEN N'生徒あたりの料金' ELSE N'Precio por alumno' END,
    [actividades_campo_activa] = CASE [IdiomaID] WHEN 1 THEN N'Actividad activa' WHEN 2 THEN N'Active activity' WHEN 3 THEN N'Atividade ativa' WHEN 4 THEN N'Activité active' WHEN 5 THEN N'有効なアクティビティ' ELSE N'Actividad activa' END,
    [actividades_campo_dia] = CASE [IdiomaID] WHEN 1 THEN N'Día' WHEN 2 THEN N'Day' WHEN 3 THEN N'Dia' WHEN 4 THEN N'Jour' WHEN 5 THEN N'曜日' ELSE N'Día' END,
    [actividades_horarios_titulo] = CASE [IdiomaID] WHEN 1 THEN N'Horarios semanales' WHEN 2 THEN N'Weekly schedule' WHEN 3 THEN N'Horários semanais' WHEN 4 THEN N'Horaires hebdomadaires' WHEN 5 THEN N'週間スケジュール' ELSE N'Horarios semanales' END,
    [actividades_instructores_titulo] = CASE [IdiomaID] WHEN 1 THEN N'Instructores' WHEN 2 THEN N'Instructors' WHEN 3 THEN N'Instrutores' WHEN 4 THEN N'Moniteurs' WHEN 5 THEN N'インストラクター' ELSE N'Instructores' END,
    [actividades_sin_entrenadores] = CASE [IdiomaID] WHEN 1 THEN N'No hay entrenadores cargados.' WHEN 2 THEN N'There are no trainers.' WHEN 3 THEN N'Não há treinadores cadastrados.' WHEN 4 THEN N'Aucun entraîneur enregistré.' WHEN 5 THEN N'トレーナーが登録されていません。' ELSE N'No hay entrenadores cargados.' END,
    [actividades_sin_alumnos] = CASE [IdiomaID] WHEN 1 THEN N'No hay alumnos activos.' WHEN 2 THEN N'There are no active students.' WHEN 3 THEN N'Não há alunos ativos.' WHEN 4 THEN N'Aucun élève actif.' WHEN 5 THEN N'有効な生徒がいません。' ELSE N'No hay alumnos activos.' END,
    [actividades_sin_actividades] = CASE [IdiomaID] WHEN 1 THEN N'Todavía no hay actividades cargadas.' WHEN 2 THEN N'There are no activities yet.' WHEN 3 THEN N'Ainda não há atividades cadastradas.' WHEN 4 THEN N'Aucune activité pour l''instant.' WHEN 5 THEN N'まだアクティビティがありません。' ELSE N'Todavía no hay actividades cargadas.' END,
    [actividades_sin_instructor] = CASE [IdiomaID] WHEN 1 THEN N'Sin instructor' WHEN 2 THEN N'No instructor' WHEN 3 THEN N'Sem instrutor' WHEN 4 THEN N'Sans moniteur' WHEN 5 THEN N'インストラクターなし' ELSE N'Sin instructor' END,
    [actividades_inscripciones_titulo_fmt] = CASE [IdiomaID] WHEN 1 THEN N'Alumnos inscriptos en {0}' WHEN 2 THEN N'Students enrolled in {0}' WHEN 3 THEN N'Alunos inscritos em {0}' WHEN 4 THEN N'Élèves inscrits à {0}' WHEN 5 THEN N'{0} の登録生徒' ELSE N'Alumnos inscriptos en {0}' END,
    [actividades_confirmar_baja_titulo] = CASE [IdiomaID] WHEN 1 THEN N'Dar de baja la actividad' WHEN 2 THEN N'Deactivate activity' WHEN 3 THEN N'Desativar atividade' WHEN 4 THEN N'Désactiver l''activité' WHEN 5 THEN N'アクティビティを無効にする' ELSE N'Dar de baja la actividad' END,
    [actividades_confirmar_baja_msg_fmt] = CASE [IdiomaID] WHEN 1 THEN N'¿Dar de baja "{0}"? Deja de aparecer en el calendario, pero se conserva su historial y se puede reactivar.' WHEN 2 THEN N'Deactivate "{0}"? It will no longer appear in the calendar, but its history is kept and it can be reactivated.' WHEN 3 THEN N'Desativar "{0}"? Ela deixará de aparecer no calendário, mas o histórico é mantido e pode ser reativada.' WHEN 4 THEN N'Désactiver « {0} » ? Elle n’apparaîtra plus dans le calendrier, mais son historique est conservé et elle peut être réactivée.' WHEN 5 THEN N'「{0}」を無効にしますか？カレンダーには表示されなくなりますが、履歴は保持され、再有効化できます。' ELSE N'¿Dar de baja "{0}"? Deja de aparecer en el calendario, pero se conserva su historial y se puede reactivar.' END,
    [actividades_msg_guardada] = CASE [IdiomaID] WHEN 1 THEN N'Actividad guardada correctamente.' WHEN 2 THEN N'Activity saved successfully.' WHEN 3 THEN N'Atividade salva com sucesso.' WHEN 4 THEN N'Activité enregistrée avec succès.' WHEN 5 THEN N'アクティビティを保存しました。' ELSE N'Actividad guardada correctamente.' END,
    [actividades_msg_estado] = CASE [IdiomaID] WHEN 1 THEN N'Estado de la actividad actualizado.' WHEN 2 THEN N'Activity status updated.' WHEN 3 THEN N'Status da atividade atualizado.' WHEN 4 THEN N'Statut de l''activité mis à jour.' WHEN 5 THEN N'アクティビティの状態を更新しました。' ELSE N'Estado de la actividad actualizado.' END,
    [actividades_msg_inscripciones_guardadas] = CASE [IdiomaID] WHEN 1 THEN N'Inscripciones guardadas.' WHEN 2 THEN N'Enrollments saved.' WHEN 3 THEN N'Inscrições salvas.' WHEN 4 THEN N'Inscriptions enregistrées.' WHEN 5 THEN N'登録を保存しました。' ELSE N'Inscripciones guardadas.' END,
    [actividades_msg_importe_invalido] = CASE [IdiomaID] WHEN 1 THEN N'El costo y el precio deben ser números válidos.' WHEN 2 THEN N'Cost and price must be valid numbers.' WHEN 3 THEN N'O custo e o preço devem ser números válidos.' WHEN 4 THEN N'Le coût et le prix doivent être des nombres valides.' WHEN 5 THEN N'コストと料金は有効な数値でなければなりません。' ELSE N'El costo y el precio deben ser números válidos.' END,
    [actividades_msg_horario_invalido] = CASE [IdiomaID] WHEN 1 THEN N'Revisá los horarios: cada uno necesita hora y duración.' WHEN 2 THEN N'Check the schedules: each one needs a time and duration.' WHEN 3 THEN N'Verifique os horários: cada um precisa de hora e duração.' WHEN 4 THEN N'Vérifiez les horaires : chacun nécessite une heure et une durée.' WHEN 5 THEN N'時間帯を確認してください：それぞれに時刻と時間が必要です。' ELSE N'Revisá los horarios: cada uno necesita hora y duración.' END;
GO

IF COL_LENGTH('Traducciones.Pantalla_Permisos', 'permisos_ayuda') IS NULL
    ALTER TABLE [Traducciones].[Pantalla_Permisos] ADD [permisos_ayuda] NVARCHAR(500) NOT NULL DEFAULT N'';
IF COL_LENGTH('Traducciones.Pantalla_Permisos', 'permisos_col_permiso') IS NULL
    ALTER TABLE [Traducciones].[Pantalla_Permisos] ADD [permisos_col_permiso] NVARCHAR(500) NOT NULL DEFAULT N'';
IF COL_LENGTH('Traducciones.Pantalla_Permisos', 'permisos_sin_tabla_titulo') IS NULL
    ALTER TABLE [Traducciones].[Pantalla_Permisos] ADD [permisos_sin_tabla_titulo] NVARCHAR(500) NOT NULL DEFAULT N'';
IF COL_LENGTH('Traducciones.Pantalla_Permisos', 'permisos_sin_tabla_msg') IS NULL
    ALTER TABLE [Traducciones].[Pantalla_Permisos] ADD [permisos_sin_tabla_msg] NVARCHAR(500) NOT NULL DEFAULT N'';
IF COL_LENGTH('Traducciones.Pantalla_Permisos', 'permisos_btn_restablecer') IS NULL
    ALTER TABLE [Traducciones].[Pantalla_Permisos] ADD [permisos_btn_restablecer] NVARCHAR(500) NOT NULL DEFAULT N'';
IF COL_LENGTH('Traducciones.Pantalla_Permisos', 'permisos_msg_guardado') IS NULL
    ALTER TABLE [Traducciones].[Pantalla_Permisos] ADD [permisos_msg_guardado] NVARCHAR(500) NOT NULL DEFAULT N'';
IF COL_LENGTH('Traducciones.Pantalla_Permisos', 'permisos_msg_restablecido') IS NULL
    ALTER TABLE [Traducciones].[Pantalla_Permisos] ADD [permisos_msg_restablecido] NVARCHAR(500) NOT NULL DEFAULT N'';
IF COL_LENGTH('Traducciones.Pantalla_Permisos', 'permisos_nombre_dashboard') IS NULL
    ALTER TABLE [Traducciones].[Pantalla_Permisos] ADD [permisos_nombre_dashboard] NVARCHAR(500) NOT NULL DEFAULT N'';
IF COL_LENGTH('Traducciones.Pantalla_Permisos', 'permisos_nombre_perfil') IS NULL
    ALTER TABLE [Traducciones].[Pantalla_Permisos] ADD [permisos_nombre_perfil] NVARCHAR(500) NOT NULL DEFAULT N'';
IF COL_LENGTH('Traducciones.Pantalla_Permisos', 'permisos_nombre_gestionusuarios') IS NULL
    ALTER TABLE [Traducciones].[Pantalla_Permisos] ADD [permisos_nombre_gestionusuarios] NVARCHAR(500) NOT NULL DEFAULT N'';
IF COL_LENGTH('Traducciones.Pantalla_Permisos', 'permisos_nombre_gestionalumnos') IS NULL
    ALTER TABLE [Traducciones].[Pantalla_Permisos] ADD [permisos_nombre_gestionalumnos] NVARCHAR(500) NOT NULL DEFAULT N'';
IF COL_LENGTH('Traducciones.Pantalla_Permisos', 'permisos_nombre_gestionentrenadores') IS NULL
    ALTER TABLE [Traducciones].[Pantalla_Permisos] ADD [permisos_nombre_gestionentrenadores] NVARCHAR(500) NOT NULL DEFAULT N'';
IF COL_LENGTH('Traducciones.Pantalla_Permisos', 'permisos_nombre_bitacora') IS NULL
    ALTER TABLE [Traducciones].[Pantalla_Permisos] ADD [permisos_nombre_bitacora] NVARCHAR(500) NOT NULL DEFAULT N'';
IF COL_LENGTH('Traducciones.Pantalla_Permisos', 'permisos_nombre_actividadescalendario') IS NULL
    ALTER TABLE [Traducciones].[Pantalla_Permisos] ADD [permisos_nombre_actividadescalendario] NVARCHAR(500) NOT NULL DEFAULT N'';
IF COL_LENGTH('Traducciones.Pantalla_Permisos', 'permisos_nombre_gestionactividades') IS NULL
    ALTER TABLE [Traducciones].[Pantalla_Permisos] ADD [permisos_nombre_gestionactividades] NVARCHAR(500) NOT NULL DEFAULT N'';
IF COL_LENGTH('Traducciones.Pantalla_Permisos', 'permisos_nombre_gestionrutinas') IS NULL
    ALTER TABLE [Traducciones].[Pantalla_Permisos] ADD [permisos_nombre_gestionrutinas] NVARCHAR(500) NOT NULL DEFAULT N'';
IF COL_LENGTH('Traducciones.Pantalla_Permisos', 'permisos_nombre_pagos') IS NULL
    ALTER TABLE [Traducciones].[Pantalla_Permisos] ADD [permisos_nombre_pagos] NVARCHAR(500) NOT NULL DEFAULT N'';
IF COL_LENGTH('Traducciones.Pantalla_Permisos', 'permisos_nombre_precioscuota') IS NULL
    ALTER TABLE [Traducciones].[Pantalla_Permisos] ADD [permisos_nombre_precioscuota] NVARCHAR(500) NOT NULL DEFAULT N'';
IF COL_LENGTH('Traducciones.Pantalla_Permisos', 'permisos_nombre_verificaciondv') IS NULL
    ALTER TABLE [Traducciones].[Pantalla_Permisos] ADD [permisos_nombre_verificaciondv] NVARCHAR(500) NOT NULL DEFAULT N'';
IF COL_LENGTH('Traducciones.Pantalla_Permisos', 'permisos_nombre_backup') IS NULL
    ALTER TABLE [Traducciones].[Pantalla_Permisos] ADD [permisos_nombre_backup] NVARCHAR(500) NOT NULL DEFAULT N'';
IF COL_LENGTH('Traducciones.Pantalla_Permisos', 'permisos_nombre_restore') IS NULL
    ALTER TABLE [Traducciones].[Pantalla_Permisos] ADD [permisos_nombre_restore] NVARCHAR(500) NOT NULL DEFAULT N'';
IF COL_LENGTH('Traducciones.Pantalla_Permisos', 'permisos_nombre_recalculardv') IS NULL
    ALTER TABLE [Traducciones].[Pantalla_Permisos] ADD [permisos_nombre_recalculardv] NVARCHAR(500) NOT NULL DEFAULT N'';
IF COL_LENGTH('Traducciones.Pantalla_Permisos', 'permisos_nombre_encriptardatos') IS NULL
    ALTER TABLE [Traducciones].[Pantalla_Permisos] ADD [permisos_nombre_encriptardatos] NVARCHAR(500) NOT NULL DEFAULT N'';
IF COL_LENGTH('Traducciones.Pantalla_Permisos', 'permisos_nombre_gestionpermisos') IS NULL
    ALTER TABLE [Traducciones].[Pantalla_Permisos] ADD [permisos_nombre_gestionpermisos] NVARCHAR(500) NOT NULL DEFAULT N'';
GO

UPDATE [Traducciones].[Pantalla_Permisos] SET
    [permisos_ayuda] = CASE [IdiomaID] WHEN 1 THEN N'Marcá qué puede hacer cada rol. El WebMaster siempre tiene todos los permisos y el Administrador conserva siempre el acceso a esta pantalla. Los cambios se aplican en el próximo cambio de página.' WHEN 2 THEN N'Check what each role can do. The WebMaster always has every permission and the Administrator always keeps access to this screen. Changes apply on the next page load.' WHEN 3 THEN N'Marque o que cada função pode fazer. O WebMaster sempre tem todas as permissões e o Administrador sempre mantém o acesso a esta tela. As alterações valem no próximo carregamento de página.' WHEN 4 THEN N'Cochez ce que chaque rôle peut faire. Le WebMaster a toujours toutes les permissions et l''Administrateur conserve toujours l''accès à cet écran. Les changements s''appliquent au prochain chargement de page.' WHEN 5 THEN N'各ロールができることをチェックしてください。WebMasterは常にすべての権限を持ち、管理者は常にこの画面へのアクセスを保持します。変更は次のページ読み込みで反映されます。' ELSE N'Marcá qué puede hacer cada rol. El WebMaster siempre tiene todos los permisos y el Administrador conserva siempre el acceso a esta pantalla. Los cambios se aplican en el próximo cambio de página.' END,
    [permisos_col_permiso] = CASE [IdiomaID] WHEN 1 THEN N'Permiso' WHEN 2 THEN N'Permission' WHEN 3 THEN N'Permissão' WHEN 4 THEN N'Permission' WHEN 5 THEN N'権限' ELSE N'Permiso' END,
    [permisos_sin_tabla_titulo] = CASE [IdiomaID] WHEN 1 THEN N'Falta la tabla de permisos' WHEN 2 THEN N'Permissions table missing' WHEN 3 THEN N'Falta a tabela de permissões' WHEN 4 THEN N'Table des permissions manquante' WHEN 5 THEN N'権限テーブルがありません' ELSE N'Falta la tabla de permisos' END,
    [permisos_sin_tabla_msg] = CASE [IdiomaID] WHEN 1 THEN N'Ejecutá scripts/crear-tabla-rol-permiso.sql en la base para poder editar los permisos. Mientras tanto se usan los permisos por defecto.' WHEN 2 THEN N'Run scripts/crear-tabla-rol-permiso.sql on the database to edit permissions. Default permissions are used meanwhile.' WHEN 3 THEN N'Execute scripts/crear-tabla-rol-permiso.sql no banco para editar as permissões. Enquanto isso, são usadas as permissões padrão.' WHEN 4 THEN N'Exécutez scripts/crear-tabla-rol-permiso.sql sur la base pour modifier les permissions. En attendant, les permissions par défaut sont utilisées.' WHEN 5 THEN N'権限を編集するには、データベースで scripts/crear-tabla-rol-permiso.sql を実行してください。それまではデフォルトの権限が使用されます。' ELSE N'Ejecutá scripts/crear-tabla-rol-permiso.sql en la base para poder editar los permisos. Mientras tanto se usan los permisos por defecto.' END,
    [permisos_btn_restablecer] = CASE [IdiomaID] WHEN 1 THEN N'Restablecer valores por defecto' WHEN 2 THEN N'Restore defaults' WHEN 3 THEN N'Restaurar padrões' WHEN 4 THEN N'Rétablir les valeurs par défaut' WHEN 5 THEN N'デフォルトに戻す' ELSE N'Restablecer valores por defecto' END,
    [permisos_msg_guardado] = CASE [IdiomaID] WHEN 1 THEN N'Permisos guardados.' WHEN 2 THEN N'Permissions saved.' WHEN 3 THEN N'Permissões salvas.' WHEN 4 THEN N'Permissions enregistrées.' WHEN 5 THEN N'権限を保存しました。' ELSE N'Permisos guardados.' END,
    [permisos_msg_restablecido] = CASE [IdiomaID] WHEN 1 THEN N'Se cargaron los valores por defecto. Apretá Guardar para aplicarlos.' WHEN 2 THEN N'Defaults loaded. Press Save to apply them.' WHEN 3 THEN N'Valores padrão carregados. Clique em Salvar para aplicá-los.' WHEN 4 THEN N'Valeurs par défaut chargées. Cliquez sur Enregistrer pour les appliquer.' WHEN 5 THEN N'デフォルト値を読み込みました。保存を押して適用してください。' ELSE N'Se cargaron los valores por defecto. Apretá Guardar para aplicarlos.' END,
    [permisos_nombre_dashboard] = CASE [IdiomaID] WHEN 1 THEN N'Panel principal' WHEN 2 THEN N'Dashboard' WHEN 3 THEN N'Painel principal' WHEN 4 THEN N'Tableau de bord' WHEN 5 THEN N'ダッシュボード' ELSE N'Panel principal' END,
    [permisos_nombre_perfil] = CASE [IdiomaID] WHEN 1 THEN N'Mi perfil' WHEN 2 THEN N'My profile' WHEN 3 THEN N'Meu perfil' WHEN 4 THEN N'Mon profil' WHEN 5 THEN N'マイプロフィール' ELSE N'Mi perfil' END,
    [permisos_nombre_gestionusuarios] = CASE [IdiomaID] WHEN 1 THEN N'Gestión de usuarios' WHEN 2 THEN N'User management' WHEN 3 THEN N'Gestão de usuários' WHEN 4 THEN N'Gestion des utilisateurs' WHEN 5 THEN N'ユーザー管理' ELSE N'Gestión de usuarios' END,
    [permisos_nombre_gestionalumnos] = CASE [IdiomaID] WHEN 1 THEN N'Gestión de alumnos' WHEN 2 THEN N'Student management' WHEN 3 THEN N'Gestão de alunos' WHEN 4 THEN N'Gestion des élèves' WHEN 5 THEN N'生徒管理' ELSE N'Gestión de alumnos' END,
    [permisos_nombre_gestionentrenadores] = CASE [IdiomaID] WHEN 1 THEN N'Gestión de entrenadores' WHEN 2 THEN N'Trainer management' WHEN 3 THEN N'Gestão de treinadores' WHEN 4 THEN N'Gestion des entraîneurs' WHEN 5 THEN N'トレーナー管理' ELSE N'Gestión de entrenadores' END,
    [permisos_nombre_bitacora] = CASE [IdiomaID] WHEN 1 THEN N'Bitácora' WHEN 2 THEN N'Event log' WHEN 3 THEN N'Registro de eventos' WHEN 4 THEN N'Journal des événements' WHEN 5 THEN N'イベントログ' ELSE N'Bitácora' END,
    [permisos_nombre_actividadescalendario] = CASE [IdiomaID] WHEN 1 THEN N'Ver calendario de actividades' WHEN 2 THEN N'View activity calendar' WHEN 3 THEN N'Ver calendário de atividades' WHEN 4 THEN N'Voir le calendrier des activités' WHEN 5 THEN N'アクティビティカレンダーを見る' ELSE N'Ver calendario de actividades' END,
    [permisos_nombre_gestionactividades] = CASE [IdiomaID] WHEN 1 THEN N'Gestionar actividades e inscripciones' WHEN 2 THEN N'Manage activities and enrollments' WHEN 3 THEN N'Gerenciar atividades e inscrições' WHEN 4 THEN N'Gérer les activités et inscriptions' WHEN 5 THEN N'アクティビティと登録の管理' ELSE N'Gestionar actividades e inscripciones' END,
    [permisos_nombre_gestionrutinas] = CASE [IdiomaID] WHEN 1 THEN N'Gestión de rutinas' WHEN 2 THEN N'Routine management' WHEN 3 THEN N'Gestão de rotinas' WHEN 4 THEN N'Gestion des routines' WHEN 5 THEN N'ルーティン管理' ELSE N'Gestión de rutinas' END,
    [permisos_nombre_pagos] = CASE [IdiomaID] WHEN 1 THEN N'Pagos' WHEN 2 THEN N'Payments' WHEN 3 THEN N'Pagamentos' WHEN 4 THEN N'Paiements' WHEN 5 THEN N'支払い' ELSE N'Pagos' END,
    [permisos_nombre_precioscuota] = CASE [IdiomaID] WHEN 1 THEN N'Precios de cuota' WHEN 2 THEN N'Fee prices' WHEN 3 THEN N'Preços da mensalidade' WHEN 4 THEN N'Tarifs des cotisations' WHEN 5 THEN N'会費の料金' ELSE N'Precios de cuota' END,
    [permisos_nombre_verificaciondv] = CASE [IdiomaID] WHEN 1 THEN N'Verificación de integridad' WHEN 2 THEN N'Integrity check' WHEN 3 THEN N'Verificação de integridade' WHEN 4 THEN N'Vérification d''intégrité' WHEN 5 THEN N'整合性チェック' ELSE N'Verificación de integridad' END,
    [permisos_nombre_backup] = CASE [IdiomaID] WHEN 1 THEN N'Backup' WHEN 2 THEN N'Backup' WHEN 3 THEN N'Backup' WHEN 4 THEN N'Sauvegarde' WHEN 5 THEN N'バックアップ' ELSE N'Backup' END,
    [permisos_nombre_restore] = CASE [IdiomaID] WHEN 1 THEN N'Restore' WHEN 2 THEN N'Restore' WHEN 3 THEN N'Restauração' WHEN 4 THEN N'Restauration' WHEN 5 THEN N'リストア' ELSE N'Restore' END,
    [permisos_nombre_recalculardv] = CASE [IdiomaID] WHEN 1 THEN N'Recalcular dígitos verificadores' WHEN 2 THEN N'Recalculate check digits' WHEN 3 THEN N'Recalcular dígitos verificadores' WHEN 4 THEN N'Recalculer les chiffres de contrôle' WHEN 5 THEN N'チェックディジットの再計算' ELSE N'Recalcular dígitos verificadores' END,
    [permisos_nombre_encriptardatos] = CASE [IdiomaID] WHEN 1 THEN N'Encriptar datos' WHEN 2 THEN N'Encrypt data' WHEN 3 THEN N'Criptografar dados' WHEN 4 THEN N'Chiffrer les données' WHEN 5 THEN N'データの暗号化' ELSE N'Encriptar datos' END,
    [permisos_nombre_gestionpermisos] = CASE [IdiomaID] WHEN 1 THEN N'Gestión de permisos' WHEN 2 THEN N'Permission management' WHEN 3 THEN N'Gestão de permissões' WHEN 4 THEN N'Gestion des permissions' WHEN 5 THEN N'権限管理' ELSE N'Gestión de permisos' END;
GO
