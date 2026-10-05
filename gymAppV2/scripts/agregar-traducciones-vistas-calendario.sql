USE [GymApp]
GO

-- ============================================================
-- Traducciones para: vistas mensual, semanal y diaria del calendario de Actividades.
-- IdiomaID: 1 ES, 2 EN, 3 PT, 4 FR, 5 JA.
-- ============================================================

IF COL_LENGTH('Traducciones.Pantalla_Actividades', 'actividades_vista_mes') IS NULL
    ALTER TABLE [Traducciones].[Pantalla_Actividades] ADD [actividades_vista_mes] NVARCHAR(500) NOT NULL DEFAULT N'';
IF COL_LENGTH('Traducciones.Pantalla_Actividades', 'actividades_vista_semana') IS NULL
    ALTER TABLE [Traducciones].[Pantalla_Actividades] ADD [actividades_vista_semana] NVARCHAR(500) NOT NULL DEFAULT N'';
IF COL_LENGTH('Traducciones.Pantalla_Actividades', 'actividades_vista_dia') IS NULL
    ALTER TABLE [Traducciones].[Pantalla_Actividades] ADD [actividades_vista_dia] NVARCHAR(500) NOT NULL DEFAULT N'';
IF COL_LENGTH('Traducciones.Pantalla_Actividades', 'actividades_btn_hoy') IS NULL
    ALTER TABLE [Traducciones].[Pantalla_Actividades] ADD [actividades_btn_hoy] NVARCHAR(500) NOT NULL DEFAULT N'';
IF COL_LENGTH('Traducciones.Pantalla_Actividades', 'actividades_dia_sin_clases') IS NULL
    ALTER TABLE [Traducciones].[Pantalla_Actividades] ADD [actividades_dia_sin_clases] NVARCHAR(500) NOT NULL DEFAULT N'';
GO

UPDATE [Traducciones].[Pantalla_Actividades] SET
    [actividades_vista_mes] = CASE [IdiomaID] WHEN 1 THEN N'Mes' WHEN 2 THEN N'Month' WHEN 3 THEN N'Mês' WHEN 4 THEN N'Mois' WHEN 5 THEN N'月' ELSE N'Mes' END,
    [actividades_vista_semana] = CASE [IdiomaID] WHEN 1 THEN N'Semana' WHEN 2 THEN N'Week' WHEN 3 THEN N'Semana' WHEN 4 THEN N'Semaine' WHEN 5 THEN N'週' ELSE N'Semana' END,
    [actividades_vista_dia] = CASE [IdiomaID] WHEN 1 THEN N'Día' WHEN 2 THEN N'Day' WHEN 3 THEN N'Dia' WHEN 4 THEN N'Jour' WHEN 5 THEN N'日' ELSE N'Día' END,
    [actividades_btn_hoy] = CASE [IdiomaID] WHEN 1 THEN N'Hoy' WHEN 2 THEN N'Today' WHEN 3 THEN N'Hoje' WHEN 4 THEN N'Aujourd''hui' WHEN 5 THEN N'今日' ELSE N'Hoy' END,
    [actividades_dia_sin_clases] = CASE [IdiomaID] WHEN 1 THEN N'No hay clases este día.' WHEN 2 THEN N'There are no classes on this day.' WHEN 3 THEN N'Não há aulas neste dia.' WHEN 4 THEN N'Aucun cours ce jour-là.' WHEN 5 THEN N'この日はクラスがありません。' ELSE N'No hay clases este día.' END;
GO
