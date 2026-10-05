USE [GymApp]
GO

-- ============================================================
-- Traducciones para: inscripción a clases por turno y por fecha
-- (ventana de clase del calendario, inscripción fija por turno).
-- IdiomaID: 1 ES, 2 EN, 3 PT, 4 FR, 5 JA.
-- ============================================================

IF COL_LENGTH('Traducciones.Pantalla_Actividades', 'actividades_mis_titulo') IS NULL
    ALTER TABLE [Traducciones].[Pantalla_Actividades] ADD [actividades_mis_titulo] NVARCHAR(500) NOT NULL DEFAULT N'';
IF COL_LENGTH('Traducciones.Pantalla_Actividades', 'actividades_mis_ayuda') IS NULL
    ALTER TABLE [Traducciones].[Pantalla_Actividades] ADD [actividades_mis_ayuda] NVARCHAR(500) NOT NULL DEFAULT N'';
IF COL_LENGTH('Traducciones.Pantalla_Actividades', 'actividades_cliente_info') IS NULL
    ALTER TABLE [Traducciones].[Pantalla_Actividades] ADD [actividades_cliente_info] NVARCHAR(500) NOT NULL DEFAULT N'';
IF COL_LENGTH('Traducciones.Pantalla_Actividades', 'actividades_inscripciones_ayuda') IS NULL
    ALTER TABLE [Traducciones].[Pantalla_Actividades] ADD [actividades_inscripciones_ayuda] NVARCHAR(500) NOT NULL DEFAULT N'';
IF COL_LENGTH('Traducciones.Pantalla_Actividades', 'actividades_col_turno') IS NULL
    ALTER TABLE [Traducciones].[Pantalla_Actividades] ADD [actividades_col_turno] NVARCHAR(500) NOT NULL DEFAULT N'';
IF COL_LENGTH('Traducciones.Pantalla_Actividades', 'actividades_estado_fija') IS NULL
    ALTER TABLE [Traducciones].[Pantalla_Actividades] ADD [actividades_estado_fija] NVARCHAR(500) NOT NULL DEFAULT N'';
IF COL_LENGTH('Traducciones.Pantalla_Actividades', 'actividades_btn_anotarme_todas') IS NULL
    ALTER TABLE [Traducciones].[Pantalla_Actividades] ADD [actividades_btn_anotarme_todas] NVARCHAR(500) NOT NULL DEFAULT N'';
IF COL_LENGTH('Traducciones.Pantalla_Actividades', 'actividades_btn_ver_alumnos') IS NULL
    ALTER TABLE [Traducciones].[Pantalla_Actividades] ADD [actividades_btn_ver_alumnos] NVARCHAR(500) NOT NULL DEFAULT N'';
IF COL_LENGTH('Traducciones.Pantalla_Actividades', 'actividades_btn_anotar_alumno') IS NULL
    ALTER TABLE [Traducciones].[Pantalla_Actividades] ADD [actividades_btn_anotar_alumno] NVARCHAR(500) NOT NULL DEFAULT N'';
IF COL_LENGTH('Traducciones.Pantalla_Actividades', 'actividades_btn_cerrar') IS NULL
    ALTER TABLE [Traducciones].[Pantalla_Actividades] ADD [actividades_btn_cerrar] NVARCHAR(500) NOT NULL DEFAULT N'';
IF COL_LENGTH('Traducciones.Pantalla_Actividades', 'actividades_btn_quitar_una') IS NULL
    ALTER TABLE [Traducciones].[Pantalla_Actividades] ADD [actividades_btn_quitar_una] NVARCHAR(500) NOT NULL DEFAULT N'';
IF COL_LENGTH('Traducciones.Pantalla_Actividades', 'actividades_btn_quitar_desde') IS NULL
    ALTER TABLE [Traducciones].[Pantalla_Actividades] ADD [actividades_btn_quitar_desde] NVARCHAR(500) NOT NULL DEFAULT N'';
IF COL_LENGTH('Traducciones.Pantalla_Actividades', 'actividades_clase_pasada') IS NULL
    ALTER TABLE [Traducciones].[Pantalla_Actividades] ADD [actividades_clase_pasada] NVARCHAR(500) NOT NULL DEFAULT N'';
IF COL_LENGTH('Traducciones.Pantalla_Actividades', 'actividades_clase_estado_fija') IS NULL
    ALTER TABLE [Traducciones].[Pantalla_Actividades] ADD [actividades_clase_estado_fija] NVARCHAR(500) NOT NULL DEFAULT N'';
IF COL_LENGTH('Traducciones.Pantalla_Actividades', 'actividades_clase_estado_puntual') IS NULL
    ALTER TABLE [Traducciones].[Pantalla_Actividades] ADD [actividades_clase_estado_puntual] NVARCHAR(500) NOT NULL DEFAULT N'';
IF COL_LENGTH('Traducciones.Pantalla_Actividades', 'actividades_clase_estado_no') IS NULL
    ALTER TABLE [Traducciones].[Pantalla_Actividades] ADD [actividades_clase_estado_no] NVARCHAR(500) NOT NULL DEFAULT N'';
IF COL_LENGTH('Traducciones.Pantalla_Actividades', 'actividades_alcance_titulo') IS NULL
    ALTER TABLE [Traducciones].[Pantalla_Actividades] ADD [actividades_alcance_titulo] NVARCHAR(500) NOT NULL DEFAULT N'';
IF COL_LENGTH('Traducciones.Pantalla_Actividades', 'actividades_alcance_una') IS NULL
    ALTER TABLE [Traducciones].[Pantalla_Actividades] ADD [actividades_alcance_una] NVARCHAR(500) NOT NULL DEFAULT N'';
IF COL_LENGTH('Traducciones.Pantalla_Actividades', 'actividades_alcance_mes') IS NULL
    ALTER TABLE [Traducciones].[Pantalla_Actividades] ADD [actividades_alcance_mes] NVARCHAR(500) NOT NULL DEFAULT N'';
IF COL_LENGTH('Traducciones.Pantalla_Actividades', 'actividades_alcance_4semanas') IS NULL
    ALTER TABLE [Traducciones].[Pantalla_Actividades] ADD [actividades_alcance_4semanas] NVARCHAR(500) NOT NULL DEFAULT N'';
IF COL_LENGTH('Traducciones.Pantalla_Actividades', 'actividades_alcance_todas') IS NULL
    ALTER TABLE [Traducciones].[Pantalla_Actividades] ADD [actividades_alcance_todas] NVARCHAR(500) NOT NULL DEFAULT N'';
IF COL_LENGTH('Traducciones.Pantalla_Actividades', 'actividades_baja_titulo') IS NULL
    ALTER TABLE [Traducciones].[Pantalla_Actividades] ADD [actividades_baja_titulo] NVARCHAR(500) NOT NULL DEFAULT N'';
IF COL_LENGTH('Traducciones.Pantalla_Actividades', 'actividades_baja_una') IS NULL
    ALTER TABLE [Traducciones].[Pantalla_Actividades] ADD [actividades_baja_una] NVARCHAR(500) NOT NULL DEFAULT N'';
IF COL_LENGTH('Traducciones.Pantalla_Actividades', 'actividades_baja_desde') IS NULL
    ALTER TABLE [Traducciones].[Pantalla_Actividades] ADD [actividades_baja_desde] NVARCHAR(500) NOT NULL DEFAULT N'';
IF COL_LENGTH('Traducciones.Pantalla_Actividades', 'actividades_tipo_fija') IS NULL
    ALTER TABLE [Traducciones].[Pantalla_Actividades] ADD [actividades_tipo_fija] NVARCHAR(500) NOT NULL DEFAULT N'';
IF COL_LENGTH('Traducciones.Pantalla_Actividades', 'actividades_tipo_puntual') IS NULL
    ALTER TABLE [Traducciones].[Pantalla_Actividades] ADD [actividades_tipo_puntual] NVARCHAR(500) NOT NULL DEFAULT N'';
IF COL_LENGTH('Traducciones.Pantalla_Actividades', 'actividades_clase_asistentes_fmt') IS NULL
    ALTER TABLE [Traducciones].[Pantalla_Actividades] ADD [actividades_clase_asistentes_fmt] NVARCHAR(500) NOT NULL DEFAULT N'';
IF COL_LENGTH('Traducciones.Pantalla_Actividades', 'actividades_clase_sin_asistentes') IS NULL
    ALTER TABLE [Traducciones].[Pantalla_Actividades] ADD [actividades_clase_sin_asistentes] NVARCHAR(500) NOT NULL DEFAULT N'';
IF COL_LENGTH('Traducciones.Pantalla_Actividades', 'actividades_anotar_alumno_titulo') IS NULL
    ALTER TABLE [Traducciones].[Pantalla_Actividades] ADD [actividades_anotar_alumno_titulo] NVARCHAR(500) NOT NULL DEFAULT N'';
IF COL_LENGTH('Traducciones.Pantalla_Actividades', 'actividades_sin_turnos') IS NULL
    ALTER TABLE [Traducciones].[Pantalla_Actividades] ADD [actividades_sin_turnos] NVARCHAR(500) NOT NULL DEFAULT N'';
IF COL_LENGTH('Traducciones.Pantalla_Actividades', 'actividades_msg_alumno_anotado') IS NULL
    ALTER TABLE [Traducciones].[Pantalla_Actividades] ADD [actividades_msg_alumno_anotado] NVARCHAR(500) NOT NULL DEFAULT N'';
IF COL_LENGTH('Traducciones.Pantalla_Actividades', 'actividades_msg_alumno_quitado') IS NULL
    ALTER TABLE [Traducciones].[Pantalla_Actividades] ADD [actividades_msg_alumno_quitado] NVARCHAR(500) NOT NULL DEFAULT N'';
GO

UPDATE [Traducciones].[Pantalla_Actividades] SET
    [actividades_mis_titulo] = CASE [IdiomaID] WHEN 1 THEN N'Inscripción fija (todas las semanas)' WHEN 2 THEN N'Regular sign-up (every week)' WHEN 3 THEN N'Inscrição fixa (todas as semanas)' WHEN 4 THEN N'Inscription régulière (chaque semaine)' WHEN 5 THEN N'定期申込（毎週）' ELSE N'Inscripción fija (todas las semanas)' END,
    [actividades_mis_ayuda] = CASE [IdiomaID] WHEN 1 THEN N'Para anotarte a una clase puntual, tocá un día del calendario y elegí la clase.' WHEN 2 THEN N'To sign up for a single class, tap a day in the calendar and choose the class.' WHEN 3 THEN N'Para se inscrever em uma aula específica, toque em um dia do calendário e escolha a aula.' WHEN 4 THEN N'Pour vous inscrire à un seul cours, touchez un jour du calendrier et choisissez le cours.' WHEN 5 THEN N'単発のクラスに申し込むには、カレンダーの日付をタップしてクラスを選んでください。' ELSE N'Para anotarte a una clase puntual, tocá un día del calendario y elegí la clase.' END,
    [actividades_cliente_info] = CASE [IdiomaID] WHEN 1 THEN N'Las clases marcadas con ✓ son a las que vas. Tocá un día y elegí una clase para anotarte o darte de baja.' WHEN 2 THEN N'Classes marked with ✓ are the ones you attend. Tap a day and choose a class to sign up or cancel.' WHEN 3 THEN N'As aulas marcadas com ✓ são as que você frequenta. Toque em um dia e escolha uma aula para se inscrever ou cancelar.' WHEN 4 THEN N'Les cours marqués ✓ sont ceux auxquels vous participez. Touchez un jour et choisissez un cours pour vous inscrire ou vous désinscrire.' WHEN 5 THEN N'✓ が付いたクラスが参加予定のクラスです。日付をタップしてクラスを選び、申し込みまたは取り消しができます。' ELSE N'Las clases marcadas con ✓ son a las que vas. Tocá un día y elegí una clase para anotarte o darte de baja.' END,
    [actividades_inscripciones_ayuda] = CASE [IdiomaID] WHEN 1 THEN N'Inscripción fija: el alumno va todas las semanas a este turno desde la próxima clase. Para una clase puntual, abrí la clase desde el calendario.' WHEN 2 THEN N'Regular sign-up: the student attends this slot every week starting from the next class. For a single class, open it from the calendar.' WHEN 3 THEN N'Inscrição fixa: o aluno frequenta este horário todas as semanas a partir da próxima aula. Para uma aula específica, abra-a pelo calendário.' WHEN 4 THEN N'Inscription régulière : l''élève suit ce créneau chaque semaine à partir du prochain cours. Pour un seul cours, ouvrez-le depuis le calendrier.' WHEN 5 THEN N'定期申込：次回のクラスから毎週この時間帯に参加します。単発のクラスはカレンダーから開いてください。' ELSE N'Inscripción fija: el alumno va todas las semanas a este turno desde la próxima clase. Para una clase puntual, abrí la clase desde el calendario.' END,
    [actividades_col_turno] = CASE [IdiomaID] WHEN 1 THEN N'Turno' WHEN 2 THEN N'Time slot' WHEN 3 THEN N'Horário' WHEN 4 THEN N'Créneau' WHEN 5 THEN N'時間帯' ELSE N'Turno' END,
    [actividades_estado_fija] = CASE [IdiomaID] WHEN 1 THEN N'Todas las semanas' WHEN 2 THEN N'Every week' WHEN 3 THEN N'Todas as semanas' WHEN 4 THEN N'Chaque semaine' WHEN 5 THEN N'毎週' ELSE N'Todas las semanas' END,
    [actividades_btn_anotarme_todas] = CASE [IdiomaID] WHEN 1 THEN N'Anotarme a todas' WHEN 2 THEN N'Sign up for all' WHEN 3 THEN N'Inscrever-me em todas' WHEN 4 THEN N'M''inscrire à tous' WHEN 5 THEN N'すべてに申し込む' ELSE N'Anotarme a todas' END,
    [actividades_btn_ver_alumnos] = CASE [IdiomaID] WHEN 1 THEN N'Ver alumnos' WHEN 2 THEN N'View students' WHEN 3 THEN N'Ver alunos' WHEN 4 THEN N'Voir les élèves' WHEN 5 THEN N'生徒を見る' ELSE N'Ver alumnos' END,
    [actividades_btn_anotar_alumno] = CASE [IdiomaID] WHEN 1 THEN N'Anotar alumno' WHEN 2 THEN N'Sign up student' WHEN 3 THEN N'Inscrever aluno' WHEN 4 THEN N'Inscrire l''élève' WHEN 5 THEN N'生徒を申し込む' ELSE N'Anotar alumno' END,
    [actividades_btn_cerrar] = CASE [IdiomaID] WHEN 1 THEN N'Cerrar' WHEN 2 THEN N'Close' WHEN 3 THEN N'Fechar' WHEN 4 THEN N'Fermer' WHEN 5 THEN N'閉じる' ELSE N'Cerrar' END,
    [actividades_btn_quitar_una] = CASE [IdiomaID] WHEN 1 THEN N'Quitar de esta clase' WHEN 2 THEN N'Remove from this class' WHEN 3 THEN N'Remover desta aula' WHEN 4 THEN N'Retirer de ce cours' WHEN 5 THEN N'このクラスから外す' ELSE N'Quitar de esta clase' END,
    [actividades_btn_quitar_desde] = CASE [IdiomaID] WHEN 1 THEN N'Quitar de esta y las siguientes' WHEN 2 THEN N'Remove from this and following' WHEN 3 THEN N'Remover desta e das seguintes' WHEN 4 THEN N'Retirer de ce cours et des suivants' WHEN 5 THEN N'このクラス以降から外す' ELSE N'Quitar de esta y las siguientes' END,
    [actividades_clase_pasada] = CASE [IdiomaID] WHEN 1 THEN N'Esta clase ya pasó: no se pueden hacer cambios.' WHEN 2 THEN N'This class has already taken place: no changes allowed.' WHEN 3 THEN N'Esta aula já aconteceu: não é possível fazer alterações.' WHEN 4 THEN N'Ce cours a déjà eu lieu : aucune modification possible.' WHEN 5 THEN N'このクラスは終了しているため、変更できません。' ELSE N'Esta clase ya pasó: no se pueden hacer cambios.' END,
    [actividades_clase_estado_fija] = CASE [IdiomaID] WHEN 1 THEN N'Vas a esta clase (inscripción fija, todas las semanas).' WHEN 2 THEN N'You attend this class (regular sign-up, every week).' WHEN 3 THEN N'Você vai a esta aula (inscrição fixa, todas as semanas).' WHEN 4 THEN N'Vous participez à ce cours (inscription régulière, chaque semaine).' WHEN 5 THEN N'このクラスに参加します（定期申込・毎週）。' ELSE N'Vas a esta clase (inscripción fija, todas las semanas).' END,
    [actividades_clase_estado_puntual] = CASE [IdiomaID] WHEN 1 THEN N'Vas a esta clase.' WHEN 2 THEN N'You attend this class.' WHEN 3 THEN N'Você vai a esta aula.' WHEN 4 THEN N'Vous participez à ce cours.' WHEN 5 THEN N'このクラスに参加します。' ELSE N'Vas a esta clase.' END,
    [actividades_clase_estado_no] = CASE [IdiomaID] WHEN 1 THEN N'No estás anotado a esta clase.' WHEN 2 THEN N'You are not signed up for this class.' WHEN 3 THEN N'Você não está inscrito nesta aula.' WHEN 4 THEN N'Vous n''êtes pas inscrit à ce cours.' WHEN 5 THEN N'このクラスには申し込んでいません。' ELSE N'No estás anotado a esta clase.' END,
    [actividades_alcance_titulo] = CASE [IdiomaID] WHEN 1 THEN N'¿A qué clases querés anotarte?' WHEN 2 THEN N'Which classes do you want to sign up for?' WHEN 3 THEN N'Em quais aulas você quer se inscrever?' WHEN 4 THEN N'À quels cours voulez-vous vous inscrire ?' WHEN 5 THEN N'どのクラスに申し込みますか？' ELSE N'¿A qué clases querés anotarte?' END,
    [actividades_alcance_una] = CASE [IdiomaID] WHEN 1 THEN N'Solo esta clase' WHEN 2 THEN N'Only this class' WHEN 3 THEN N'Somente esta aula' WHEN 4 THEN N'Seulement ce cours' WHEN 5 THEN N'このクラスのみ' ELSE N'Solo esta clase' END,
    [actividades_alcance_mes] = CASE [IdiomaID] WHEN 1 THEN N'Esta y las que quedan del mes' WHEN 2 THEN N'This one and the rest of the month' WHEN 3 THEN N'Esta e as que restam do mês' WHEN 4 THEN N'Celui-ci et le reste du mois' WHEN 5 THEN N'このクラスと今月の残り' ELSE N'Esta y las que quedan del mes' END,
    [actividades_alcance_4semanas] = CASE [IdiomaID] WHEN 1 THEN N'Las próximas 4 semanas' WHEN 2 THEN N'The next 4 weeks' WHEN 3 THEN N'As próximas 4 semanas' WHEN 4 THEN N'Les 4 prochaines semaines' WHEN 5 THEN N'今後4週間' ELSE N'Las próximas 4 semanas' END,
    [actividades_alcance_todas] = CASE [IdiomaID] WHEN 1 THEN N'Todas las semanas (inscripción fija)' WHEN 2 THEN N'Every week (regular sign-up)' WHEN 3 THEN N'Todas as semanas (inscrição fixa)' WHEN 4 THEN N'Chaque semaine (inscription régulière)' WHEN 5 THEN N'毎週（定期申込）' ELSE N'Todas las semanas (inscripción fija)' END,
    [actividades_baja_titulo] = CASE [IdiomaID] WHEN 1 THEN N'¿De qué clases te querés dar de baja?' WHEN 2 THEN N'Which classes do you want to cancel?' WHEN 3 THEN N'De quais aulas você quer cancelar a inscrição?' WHEN 4 THEN N'De quels cours voulez-vous vous désinscrire ?' WHEN 5 THEN N'どのクラスを取り消しますか？' ELSE N'¿De qué clases te querés dar de baja?' END,
    [actividades_baja_una] = CASE [IdiomaID] WHEN 1 THEN N'Solo esta clase' WHEN 2 THEN N'Only this class' WHEN 3 THEN N'Somente esta aula' WHEN 4 THEN N'Seulement ce cours' WHEN 5 THEN N'このクラスのみ' ELSE N'Solo esta clase' END,
    [actividades_baja_desde] = CASE [IdiomaID] WHEN 1 THEN N'Esta y todas las siguientes' WHEN 2 THEN N'This one and all following' WHEN 3 THEN N'Esta e todas as seguintes' WHEN 4 THEN N'Celui-ci et tous les suivants' WHEN 5 THEN N'このクラス以降すべて' ELSE N'Esta y todas las siguientes' END,
    [actividades_tipo_fija] = CASE [IdiomaID] WHEN 1 THEN N'Fija' WHEN 2 THEN N'Regular' WHEN 3 THEN N'Fixa' WHEN 4 THEN N'Régulière' WHEN 5 THEN N'定期' ELSE N'Fija' END,
    [actividades_tipo_puntual] = CASE [IdiomaID] WHEN 1 THEN N'Solo esta clase' WHEN 2 THEN N'This class only' WHEN 3 THEN N'Somente esta aula' WHEN 4 THEN N'Ce cours uniquement' WHEN 5 THEN N'このクラスのみ' ELSE N'Solo esta clase' END,
    [actividades_clase_asistentes_fmt] = CASE [IdiomaID] WHEN 1 THEN N'Alumnos anotados ({0})' WHEN 2 THEN N'Signed-up students ({0})' WHEN 3 THEN N'Alunos inscritos ({0})' WHEN 4 THEN N'Élèves inscrits ({0})' WHEN 5 THEN N'申込済みの生徒（{0}）' ELSE N'Alumnos anotados ({0})' END,
    [actividades_clase_sin_asistentes] = CASE [IdiomaID] WHEN 1 THEN N'Todavía no hay alumnos anotados.' WHEN 2 THEN N'No students signed up yet.' WHEN 3 THEN N'Ainda não há alunos inscritos.' WHEN 4 THEN N'Aucun élève inscrit pour l''instant.' WHEN 5 THEN N'まだ申し込んだ生徒はいません。' ELSE N'Todavía no hay alumnos anotados.' END,
    [actividades_anotar_alumno_titulo] = CASE [IdiomaID] WHEN 1 THEN N'Anotar a un alumno' WHEN 2 THEN N'Sign up a student' WHEN 3 THEN N'Inscrever um aluno' WHEN 4 THEN N'Inscrire un élève' WHEN 5 THEN N'生徒を申し込む' ELSE N'Anotar a un alumno' END,
    [actividades_sin_turnos] = CASE [IdiomaID] WHEN 1 THEN N'La actividad no tiene horarios cargados.' WHEN 2 THEN N'The activity has no schedules.' WHEN 3 THEN N'A atividade não tem horários cadastrados.' WHEN 4 THEN N'L''activité n''a pas d''horaires.' WHEN 5 THEN N'このアクティビティには時間帯が登録されていません。' ELSE N'La actividad no tiene horarios cargados.' END,
    [actividades_msg_alumno_anotado] = CASE [IdiomaID] WHEN 1 THEN N'Alumno anotado.' WHEN 2 THEN N'Student signed up.' WHEN 3 THEN N'Aluno inscrito.' WHEN 4 THEN N'Élève inscrit.' WHEN 5 THEN N'生徒を申し込みました。' ELSE N'Alumno anotado.' END,
    [actividades_msg_alumno_quitado] = CASE [IdiomaID] WHEN 1 THEN N'Alumno quitado de la clase.' WHEN 2 THEN N'Student removed from the class.' WHEN 3 THEN N'Aluno removido da aula.' WHEN 4 THEN N'Élève retiré du cours.' WHEN 5 THEN N'生徒をクラスから外しました。' ELSE N'Alumno quitado de la clase.' END;
GO
