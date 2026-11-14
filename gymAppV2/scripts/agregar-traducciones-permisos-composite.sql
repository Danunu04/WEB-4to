USE [GymApp]
GO

-- ============================================================
-- Traducciones de la pantalla Permisos (patrón Composite): pestañas, editor, mensajes,
-- nombres de las familias y perfiles del sistema y del permiso GestionIdiomas.
-- IdiomaID: 1 ES, 2 EN, 3 PT, 4 FR, 5 JA.
-- Generado con un script: si hay que cambiar un texto, cambiarlo en las 5 columnas del CASE.
-- ============================================================

IF COL_LENGTH('Traducciones.Pantalla_Permisos', 'permisos_titulo') IS NULL
    ALTER TABLE [Traducciones].[Pantalla_Permisos] ADD [permisos_titulo] NVARCHAR(500) NOT NULL DEFAULT N'';
IF COL_LENGTH('Traducciones.Pantalla_Permisos', 'permisos_subtitulo') IS NULL
    ALTER TABLE [Traducciones].[Pantalla_Permisos] ADD [permisos_subtitulo] NVARCHAR(500) NOT NULL DEFAULT N'';
IF COL_LENGTH('Traducciones.Pantalla_Permisos', 'permisos_sin_tabla_titulo') IS NULL
    ALTER TABLE [Traducciones].[Pantalla_Permisos] ADD [permisos_sin_tabla_titulo] NVARCHAR(500) NOT NULL DEFAULT N'';
IF COL_LENGTH('Traducciones.Pantalla_Permisos', 'permisos_sin_tabla_msg') IS NULL
    ALTER TABLE [Traducciones].[Pantalla_Permisos] ADD [permisos_sin_tabla_msg] NVARCHAR(500) NOT NULL DEFAULT N'';
IF COL_LENGTH('Traducciones.Pantalla_Permisos', 'permisos_tab_perfiles') IS NULL
    ALTER TABLE [Traducciones].[Pantalla_Permisos] ADD [permisos_tab_perfiles] NVARCHAR(500) NOT NULL DEFAULT N'';
IF COL_LENGTH('Traducciones.Pantalla_Permisos', 'permisos_tab_familias') IS NULL
    ALTER TABLE [Traducciones].[Pantalla_Permisos] ADD [permisos_tab_familias] NVARCHAR(500) NOT NULL DEFAULT N'';
IF COL_LENGTH('Traducciones.Pantalla_Permisos', 'permisos_tab_usuarios') IS NULL
    ALTER TABLE [Traducciones].[Pantalla_Permisos] ADD [permisos_tab_usuarios] NVARCHAR(500) NOT NULL DEFAULT N'';
IF COL_LENGTH('Traducciones.Pantalla_Permisos', 'permisos_ayuda_perfiles') IS NULL
    ALTER TABLE [Traducciones].[Pantalla_Permisos] ADD [permisos_ayuda_perfiles] NVARCHAR(500) NOT NULL DEFAULT N'';
IF COL_LENGTH('Traducciones.Pantalla_Permisos', 'permisos_ayuda_familias') IS NULL
    ALTER TABLE [Traducciones].[Pantalla_Permisos] ADD [permisos_ayuda_familias] NVARCHAR(500) NOT NULL DEFAULT N'';
IF COL_LENGTH('Traducciones.Pantalla_Permisos', 'permisos_ayuda_usuarios') IS NULL
    ALTER TABLE [Traducciones].[Pantalla_Permisos] ADD [permisos_ayuda_usuarios] NVARCHAR(500) NOT NULL DEFAULT N'';
IF COL_LENGTH('Traducciones.Pantalla_Permisos', 'permisos_btn_nuevo_perfil') IS NULL
    ALTER TABLE [Traducciones].[Pantalla_Permisos] ADD [permisos_btn_nuevo_perfil] NVARCHAR(500) NOT NULL DEFAULT N'';
IF COL_LENGTH('Traducciones.Pantalla_Permisos', 'permisos_btn_nueva_familia') IS NULL
    ALTER TABLE [Traducciones].[Pantalla_Permisos] ADD [permisos_btn_nueva_familia] NVARCHAR(500) NOT NULL DEFAULT N'';
IF COL_LENGTH('Traducciones.Pantalla_Permisos', 'permisos_btn_editar') IS NULL
    ALTER TABLE [Traducciones].[Pantalla_Permisos] ADD [permisos_btn_editar] NVARCHAR(500) NOT NULL DEFAULT N'';
IF COL_LENGTH('Traducciones.Pantalla_Permisos', 'permisos_btn_eliminar') IS NULL
    ALTER TABLE [Traducciones].[Pantalla_Permisos] ADD [permisos_btn_eliminar] NVARCHAR(500) NOT NULL DEFAULT N'';
IF COL_LENGTH('Traducciones.Pantalla_Permisos', 'permisos_btn_agregar') IS NULL
    ALTER TABLE [Traducciones].[Pantalla_Permisos] ADD [permisos_btn_agregar] NVARCHAR(500) NOT NULL DEFAULT N'';
IF COL_LENGTH('Traducciones.Pantalla_Permisos', 'permisos_btn_quitar') IS NULL
    ALTER TABLE [Traducciones].[Pantalla_Permisos] ADD [permisos_btn_quitar] NVARCHAR(500) NOT NULL DEFAULT N'';
IF COL_LENGTH('Traducciones.Pantalla_Permisos', 'permisos_btn_asignar') IS NULL
    ALTER TABLE [Traducciones].[Pantalla_Permisos] ADD [permisos_btn_asignar] NVARCHAR(500) NOT NULL DEFAULT N'';
IF COL_LENGTH('Traducciones.Pantalla_Permisos', 'permisos_btn_cancelar') IS NULL
    ALTER TABLE [Traducciones].[Pantalla_Permisos] ADD [permisos_btn_cancelar] NVARCHAR(500) NOT NULL DEFAULT N'';
IF COL_LENGTH('Traducciones.Pantalla_Permisos', 'permisos_badge_sistema') IS NULL
    ALTER TABLE [Traducciones].[Pantalla_Permisos] ADD [permisos_badge_sistema] NVARCHAR(500) NOT NULL DEFAULT N'';
IF COL_LENGTH('Traducciones.Pantalla_Permisos', 'permisos_usuarios_cantidad') IS NULL
    ALTER TABLE [Traducciones].[Pantalla_Permisos] ADD [permisos_usuarios_cantidad] NVARCHAR(500) NOT NULL DEFAULT N'';
IF COL_LENGTH('Traducciones.Pantalla_Permisos', 'permisos_usada_en') IS NULL
    ALTER TABLE [Traducciones].[Pantalla_Permisos] ADD [permisos_usada_en] NVARCHAR(500) NOT NULL DEFAULT N'';
IF COL_LENGTH('Traducciones.Pantalla_Permisos', 'permisos_sin_uso') IS NULL
    ALTER TABLE [Traducciones].[Pantalla_Permisos] ADD [permisos_sin_uso] NVARCHAR(500) NOT NULL DEFAULT N'';
IF COL_LENGTH('Traducciones.Pantalla_Permisos', 'permisos_webmaster_todo') IS NULL
    ALTER TABLE [Traducciones].[Pantalla_Permisos] ADD [permisos_webmaster_todo] NVARCHAR(500) NOT NULL DEFAULT N'';
IF COL_LENGTH('Traducciones.Pantalla_Permisos', 'permisos_sin_perfiles') IS NULL
    ALTER TABLE [Traducciones].[Pantalla_Permisos] ADD [permisos_sin_perfiles] NVARCHAR(500) NOT NULL DEFAULT N'';
IF COL_LENGTH('Traducciones.Pantalla_Permisos', 'permisos_sin_familias') IS NULL
    ALTER TABLE [Traducciones].[Pantalla_Permisos] ADD [permisos_sin_familias] NVARCHAR(500) NOT NULL DEFAULT N'';
IF COL_LENGTH('Traducciones.Pantalla_Permisos', 'permisos_confirmar_eliminar') IS NULL
    ALTER TABLE [Traducciones].[Pantalla_Permisos] ADD [permisos_confirmar_eliminar] NVARCHAR(500) NOT NULL DEFAULT N'';
IF COL_LENGTH('Traducciones.Pantalla_Permisos', 'permisos_editor_nuevo_perfil') IS NULL
    ALTER TABLE [Traducciones].[Pantalla_Permisos] ADD [permisos_editor_nuevo_perfil] NVARCHAR(500) NOT NULL DEFAULT N'';
IF COL_LENGTH('Traducciones.Pantalla_Permisos', 'permisos_editor_editar_perfil') IS NULL
    ALTER TABLE [Traducciones].[Pantalla_Permisos] ADD [permisos_editor_editar_perfil] NVARCHAR(500) NOT NULL DEFAULT N'';
IF COL_LENGTH('Traducciones.Pantalla_Permisos', 'permisos_editor_nueva_familia') IS NULL
    ALTER TABLE [Traducciones].[Pantalla_Permisos] ADD [permisos_editor_nueva_familia] NVARCHAR(500) NOT NULL DEFAULT N'';
IF COL_LENGTH('Traducciones.Pantalla_Permisos', 'permisos_editor_editar_familia') IS NULL
    ALTER TABLE [Traducciones].[Pantalla_Permisos] ADD [permisos_editor_editar_familia] NVARCHAR(500) NOT NULL DEFAULT N'';
IF COL_LENGTH('Traducciones.Pantalla_Permisos', 'permisos_campo_nombre') IS NULL
    ALTER TABLE [Traducciones].[Pantalla_Permisos] ADD [permisos_campo_nombre] NVARCHAR(500) NOT NULL DEFAULT N'';
IF COL_LENGTH('Traducciones.Pantalla_Permisos', 'permisos_nombre_bloqueado') IS NULL
    ALTER TABLE [Traducciones].[Pantalla_Permisos] ADD [permisos_nombre_bloqueado] NVARCHAR(500) NOT NULL DEFAULT N'';
IF COL_LENGTH('Traducciones.Pantalla_Permisos', 'permisos_disponibles') IS NULL
    ALTER TABLE [Traducciones].[Pantalla_Permisos] ADD [permisos_disponibles] NVARCHAR(500) NOT NULL DEFAULT N'';
IF COL_LENGTH('Traducciones.Pantalla_Permisos', 'permisos_seccion_permisos') IS NULL
    ALTER TABLE [Traducciones].[Pantalla_Permisos] ADD [permisos_seccion_permisos] NVARCHAR(500) NOT NULL DEFAULT N'';
IF COL_LENGTH('Traducciones.Pantalla_Permisos', 'permisos_seccion_familias') IS NULL
    ALTER TABLE [Traducciones].[Pantalla_Permisos] ADD [permisos_seccion_familias] NVARCHAR(500) NOT NULL DEFAULT N'';
IF COL_LENGTH('Traducciones.Pantalla_Permisos', 'permisos_contenido') IS NULL
    ALTER TABLE [Traducciones].[Pantalla_Permisos] ADD [permisos_contenido] NVARCHAR(500) NOT NULL DEFAULT N'';
IF COL_LENGTH('Traducciones.Pantalla_Permisos', 'permisos_contenido_vacio') IS NULL
    ALTER TABLE [Traducciones].[Pantalla_Permisos] ADD [permisos_contenido_vacio] NVARCHAR(500) NOT NULL DEFAULT N'';
IF COL_LENGTH('Traducciones.Pantalla_Permisos', 'permisos_ya_incluido') IS NULL
    ALTER TABLE [Traducciones].[Pantalla_Permisos] ADD [permisos_ya_incluido] NVARCHAR(500) NOT NULL DEFAULT N'';
IF COL_LENGTH('Traducciones.Pantalla_Permisos', 'permisos_editado') IS NULL
    ALTER TABLE [Traducciones].[Pantalla_Permisos] ADD [permisos_editado] NVARCHAR(500) NOT NULL DEFAULT N'';
IF COL_LENGTH('Traducciones.Pantalla_Permisos', 'permisos_buscar_usuario') IS NULL
    ALTER TABLE [Traducciones].[Pantalla_Permisos] ADD [permisos_buscar_usuario] NVARCHAR(500) NOT NULL DEFAULT N'';
IF COL_LENGTH('Traducciones.Pantalla_Permisos', 'permisos_col_usuario') IS NULL
    ALTER TABLE [Traducciones].[Pantalla_Permisos] ADD [permisos_col_usuario] NVARCHAR(500) NOT NULL DEFAULT N'';
IF COL_LENGTH('Traducciones.Pantalla_Permisos', 'permisos_col_nombre') IS NULL
    ALTER TABLE [Traducciones].[Pantalla_Permisos] ADD [permisos_col_nombre] NVARCHAR(500) NOT NULL DEFAULT N'';
IF COL_LENGTH('Traducciones.Pantalla_Permisos', 'permisos_col_tipo') IS NULL
    ALTER TABLE [Traducciones].[Pantalla_Permisos] ADD [permisos_col_tipo] NVARCHAR(500) NOT NULL DEFAULT N'';
IF COL_LENGTH('Traducciones.Pantalla_Permisos', 'permisos_col_estado') IS NULL
    ALTER TABLE [Traducciones].[Pantalla_Permisos] ADD [permisos_col_estado] NVARCHAR(500) NOT NULL DEFAULT N'';
IF COL_LENGTH('Traducciones.Pantalla_Permisos', 'permisos_col_perfil') IS NULL
    ALTER TABLE [Traducciones].[Pantalla_Permisos] ADD [permisos_col_perfil] NVARCHAR(500) NOT NULL DEFAULT N'';
IF COL_LENGTH('Traducciones.Pantalla_Permisos', 'permisos_sin_perfil') IS NULL
    ALTER TABLE [Traducciones].[Pantalla_Permisos] ADD [permisos_sin_perfil] NVARCHAR(500) NOT NULL DEFAULT N'';
IF COL_LENGTH('Traducciones.Pantalla_Permisos', 'permisos_sin_usuarios') IS NULL
    ALTER TABLE [Traducciones].[Pantalla_Permisos] ADD [permisos_sin_usuarios] NVARCHAR(500) NOT NULL DEFAULT N'';
IF COL_LENGTH('Traducciones.Pantalla_Permisos', 'permisos_estado_activo') IS NULL
    ALTER TABLE [Traducciones].[Pantalla_Permisos] ADD [permisos_estado_activo] NVARCHAR(500) NOT NULL DEFAULT N'';
IF COL_LENGTH('Traducciones.Pantalla_Permisos', 'permisos_estado_inactivo') IS NULL
    ALTER TABLE [Traducciones].[Pantalla_Permisos] ADD [permisos_estado_inactivo] NVARCHAR(500) NOT NULL DEFAULT N'';
IF COL_LENGTH('Traducciones.Pantalla_Permisos', 'permisos_elegir_perfil') IS NULL
    ALTER TABLE [Traducciones].[Pantalla_Permisos] ADD [permisos_elegir_perfil] NVARCHAR(500) NOT NULL DEFAULT N'';
IF COL_LENGTH('Traducciones.Pantalla_Permisos', 'permisos_tipo_1') IS NULL
    ALTER TABLE [Traducciones].[Pantalla_Permisos] ADD [permisos_tipo_1] NVARCHAR(500) NOT NULL DEFAULT N'';
IF COL_LENGTH('Traducciones.Pantalla_Permisos', 'permisos_tipo_2') IS NULL
    ALTER TABLE [Traducciones].[Pantalla_Permisos] ADD [permisos_tipo_2] NVARCHAR(500) NOT NULL DEFAULT N'';
IF COL_LENGTH('Traducciones.Pantalla_Permisos', 'permisos_tipo_3') IS NULL
    ALTER TABLE [Traducciones].[Pantalla_Permisos] ADD [permisos_tipo_3] NVARCHAR(500) NOT NULL DEFAULT N'';
IF COL_LENGTH('Traducciones.Pantalla_Permisos', 'permisos_tipo_4') IS NULL
    ALTER TABLE [Traducciones].[Pantalla_Permisos] ADD [permisos_tipo_4] NVARCHAR(500) NOT NULL DEFAULT N'';
IF COL_LENGTH('Traducciones.Pantalla_Permisos', 'permisos_tipo_5') IS NULL
    ALTER TABLE [Traducciones].[Pantalla_Permisos] ADD [permisos_tipo_5] NVARCHAR(500) NOT NULL DEFAULT N'';
IF COL_LENGTH('Traducciones.Pantalla_Permisos', 'permisos_tipo_6') IS NULL
    ALTER TABLE [Traducciones].[Pantalla_Permisos] ADD [permisos_tipo_6] NVARCHAR(500) NOT NULL DEFAULT N'';
IF COL_LENGTH('Traducciones.Pantalla_Permisos', 'permisos_msg_perfil_guardado') IS NULL
    ALTER TABLE [Traducciones].[Pantalla_Permisos] ADD [permisos_msg_perfil_guardado] NVARCHAR(500) NOT NULL DEFAULT N'';
IF COL_LENGTH('Traducciones.Pantalla_Permisos', 'permisos_msg_familia_guardada') IS NULL
    ALTER TABLE [Traducciones].[Pantalla_Permisos] ADD [permisos_msg_familia_guardada] NVARCHAR(500) NOT NULL DEFAULT N'';
IF COL_LENGTH('Traducciones.Pantalla_Permisos', 'permisos_msg_perfil_eliminado') IS NULL
    ALTER TABLE [Traducciones].[Pantalla_Permisos] ADD [permisos_msg_perfil_eliminado] NVARCHAR(500) NOT NULL DEFAULT N'';
IF COL_LENGTH('Traducciones.Pantalla_Permisos', 'permisos_msg_familia_eliminada') IS NULL
    ALTER TABLE [Traducciones].[Pantalla_Permisos] ADD [permisos_msg_familia_eliminada] NVARCHAR(500) NOT NULL DEFAULT N'';
IF COL_LENGTH('Traducciones.Pantalla_Permisos', 'permisos_msg_perfil_asignado') IS NULL
    ALTER TABLE [Traducciones].[Pantalla_Permisos] ADD [permisos_msg_perfil_asignado] NVARCHAR(500) NOT NULL DEFAULT N'';
IF COL_LENGTH('Traducciones.Pantalla_Permisos', 'permisos_err_sin_permiso') IS NULL
    ALTER TABLE [Traducciones].[Pantalla_Permisos] ADD [permisos_err_sin_permiso] NVARCHAR(500) NOT NULL DEFAULT N'';
IF COL_LENGTH('Traducciones.Pantalla_Permisos', 'permisos_err_no_existe') IS NULL
    ALTER TABLE [Traducciones].[Pantalla_Permisos] ADD [permisos_err_no_existe] NVARCHAR(500) NOT NULL DEFAULT N'';
IF COL_LENGTH('Traducciones.Pantalla_Permisos', 'permisos_err_nombre_obligatorio') IS NULL
    ALTER TABLE [Traducciones].[Pantalla_Permisos] ADD [permisos_err_nombre_obligatorio] NVARCHAR(500) NOT NULL DEFAULT N'';
IF COL_LENGTH('Traducciones.Pantalla_Permisos', 'permisos_err_nombre_largo') IS NULL
    ALTER TABLE [Traducciones].[Pantalla_Permisos] ADD [permisos_err_nombre_largo] NVARCHAR(500) NOT NULL DEFAULT N'';
IF COL_LENGTH('Traducciones.Pantalla_Permisos', 'permisos_err_nombre_repetido_familia') IS NULL
    ALTER TABLE [Traducciones].[Pantalla_Permisos] ADD [permisos_err_nombre_repetido_familia] NVARCHAR(500) NOT NULL DEFAULT N'';
IF COL_LENGTH('Traducciones.Pantalla_Permisos', 'permisos_err_nombre_repetido_perfil') IS NULL
    ALTER TABLE [Traducciones].[Pantalla_Permisos] ADD [permisos_err_nombre_repetido_perfil] NVARCHAR(500) NOT NULL DEFAULT N'';
IF COL_LENGTH('Traducciones.Pantalla_Permisos', 'permisos_err_familia_vacia') IS NULL
    ALTER TABLE [Traducciones].[Pantalla_Permisos] ADD [permisos_err_familia_vacia] NVARCHAR(500) NOT NULL DEFAULT N'';
IF COL_LENGTH('Traducciones.Pantalla_Permisos', 'permisos_err_perfil_vacio') IS NULL
    ALTER TABLE [Traducciones].[Pantalla_Permisos] ADD [permisos_err_perfil_vacio] NVARCHAR(500) NOT NULL DEFAULT N'';
IF COL_LENGTH('Traducciones.Pantalla_Permisos', 'permisos_err_componente_inexistente') IS NULL
    ALTER TABLE [Traducciones].[Pantalla_Permisos] ADD [permisos_err_componente_inexistente] NVARCHAR(500) NOT NULL DEFAULT N'';
IF COL_LENGTH('Traducciones.Pantalla_Permisos', 'permisos_err_familia_en_uso') IS NULL
    ALTER TABLE [Traducciones].[Pantalla_Permisos] ADD [permisos_err_familia_en_uso] NVARCHAR(500) NOT NULL DEFAULT N'';
IF COL_LENGTH('Traducciones.Pantalla_Permisos', 'permisos_err_perfil_sistema_eliminar') IS NULL
    ALTER TABLE [Traducciones].[Pantalla_Permisos] ADD [permisos_err_perfil_sistema_eliminar] NVARCHAR(500) NOT NULL DEFAULT N'';
IF COL_LENGTH('Traducciones.Pantalla_Permisos', 'permisos_err_perfil_con_usuarios') IS NULL
    ALTER TABLE [Traducciones].[Pantalla_Permisos] ADD [permisos_err_perfil_con_usuarios] NVARCHAR(500) NOT NULL DEFAULT N'';
IF COL_LENGTH('Traducciones.Pantalla_Permisos', 'permisos_err_webmaster_editar') IS NULL
    ALTER TABLE [Traducciones].[Pantalla_Permisos] ADD [permisos_err_webmaster_editar] NVARCHAR(500) NOT NULL DEFAULT N'';
IF COL_LENGTH('Traducciones.Pantalla_Permisos', 'permisos_err_webmaster_asignar') IS NULL
    ALTER TABLE [Traducciones].[Pantalla_Permisos] ADD [permisos_err_webmaster_asignar] NVARCHAR(500) NOT NULL DEFAULT N'';
IF COL_LENGTH('Traducciones.Pantalla_Permisos', 'permisos_err_admin_fijos') IS NULL
    ALTER TABLE [Traducciones].[Pantalla_Permisos] ADD [permisos_err_admin_fijos] NVARCHAR(500) NOT NULL DEFAULT N'';
IF COL_LENGTH('Traducciones.Pantalla_Permisos', 'permisos_err_sin_gestor') IS NULL
    ALTER TABLE [Traducciones].[Pantalla_Permisos] ADD [permisos_err_sin_gestor] NVARCHAR(500) NOT NULL DEFAULT N'';
IF COL_LENGTH('Traducciones.Pantalla_Permisos', 'permisos_err_asignar_gestion') IS NULL
    ALTER TABLE [Traducciones].[Pantalla_Permisos] ADD [permisos_err_asignar_gestion] NVARCHAR(500) NOT NULL DEFAULT N'';
IF COL_LENGTH('Traducciones.Pantalla_Permisos', 'permisos_err_sin_perfil_defecto') IS NULL
    ALTER TABLE [Traducciones].[Pantalla_Permisos] ADD [permisos_err_sin_perfil_defecto] NVARCHAR(500) NOT NULL DEFAULT N'';
IF COL_LENGTH('Traducciones.Pantalla_Permisos', 'permisos_err_conflicto') IS NULL
    ALTER TABLE [Traducciones].[Pantalla_Permisos] ADD [permisos_err_conflicto] NVARCHAR(500) NOT NULL DEFAULT N'';
IF COL_LENGTH('Traducciones.Pantalla_Permisos', 'permisos_err_duplicado') IS NULL
    ALTER TABLE [Traducciones].[Pantalla_Permisos] ADD [permisos_err_duplicado] NVARCHAR(500) NOT NULL DEFAULT N'';
IF COL_LENGTH('Traducciones.Pantalla_Permisos', 'permisos_err_repetido') IS NULL
    ALTER TABLE [Traducciones].[Pantalla_Permisos] ADD [permisos_err_repetido] NVARCHAR(500) NOT NULL DEFAULT N'';
IF COL_LENGTH('Traducciones.Pantalla_Permisos', 'permisos_err_ciclo') IS NULL
    ALTER TABLE [Traducciones].[Pantalla_Permisos] ADD [permisos_err_ciclo] NVARCHAR(500) NOT NULL DEFAULT N'';
IF COL_LENGTH('Traducciones.Pantalla_Permisos', 'permisos_nombre_gestionidiomas') IS NULL
    ALTER TABLE [Traducciones].[Pantalla_Permisos] ADD [permisos_nombre_gestionidiomas] NVARCHAR(500) NOT NULL DEFAULT N'';
IF COL_LENGTH('Traducciones.Pantalla_Permisos', 'permisos_familia_portal_cliente') IS NULL
    ALTER TABLE [Traducciones].[Pantalla_Permisos] ADD [permisos_familia_portal_cliente] NVARCHAR(500) NOT NULL DEFAULT N'';
IF COL_LENGTH('Traducciones.Pantalla_Permisos', 'permisos_familia_operacion') IS NULL
    ALTER TABLE [Traducciones].[Pantalla_Permisos] ADD [permisos_familia_operacion] NVARCHAR(500) NOT NULL DEFAULT N'';
IF COL_LENGTH('Traducciones.Pantalla_Permisos', 'permisos_familia_admin_gimnasio') IS NULL
    ALTER TABLE [Traducciones].[Pantalla_Permisos] ADD [permisos_familia_admin_gimnasio] NVARCHAR(500) NOT NULL DEFAULT N'';
IF COL_LENGTH('Traducciones.Pantalla_Permisos', 'permisos_familia_caja') IS NULL
    ALTER TABLE [Traducciones].[Pantalla_Permisos] ADD [permisos_familia_caja] NVARCHAR(500) NOT NULL DEFAULT N'';
IF COL_LENGTH('Traducciones.Pantalla_Permisos', 'permisos_familia_gestion_usuarios') IS NULL
    ALTER TABLE [Traducciones].[Pantalla_Permisos] ADD [permisos_familia_gestion_usuarios] NVARCHAR(500) NOT NULL DEFAULT N'';
IF COL_LENGTH('Traducciones.Pantalla_Permisos', 'permisos_familia_seguridad') IS NULL
    ALTER TABLE [Traducciones].[Pantalla_Permisos] ADD [permisos_familia_seguridad] NVARCHAR(500) NOT NULL DEFAULT N'';
IF COL_LENGTH('Traducciones.Pantalla_Permisos', 'permisos_familia_mantenimiento_bd') IS NULL
    ALTER TABLE [Traducciones].[Pantalla_Permisos] ADD [permisos_familia_mantenimiento_bd] NVARCHAR(500) NOT NULL DEFAULT N'';
IF COL_LENGTH('Traducciones.Pantalla_Permisos', 'permisos_perfil_webmaster') IS NULL
    ALTER TABLE [Traducciones].[Pantalla_Permisos] ADD [permisos_perfil_webmaster] NVARCHAR(500) NOT NULL DEFAULT N'';
IF COL_LENGTH('Traducciones.Pantalla_Permisos', 'permisos_perfil_admin') IS NULL
    ALTER TABLE [Traducciones].[Pantalla_Permisos] ADD [permisos_perfil_admin] NVARCHAR(500) NOT NULL DEFAULT N'';
IF COL_LENGTH('Traducciones.Pantalla_Permisos', 'permisos_perfil_recepcion') IS NULL
    ALTER TABLE [Traducciones].[Pantalla_Permisos] ADD [permisos_perfil_recepcion] NVARCHAR(500) NOT NULL DEFAULT N'';
IF COL_LENGTH('Traducciones.Pantalla_Permisos', 'permisos_perfil_entrenador') IS NULL
    ALTER TABLE [Traducciones].[Pantalla_Permisos] ADD [permisos_perfil_entrenador] NVARCHAR(500) NOT NULL DEFAULT N'';
IF COL_LENGTH('Traducciones.Pantalla_Permisos', 'permisos_perfil_cliente') IS NULL
    ALTER TABLE [Traducciones].[Pantalla_Permisos] ADD [permisos_perfil_cliente] NVARCHAR(500) NOT NULL DEFAULT N'';
IF COL_LENGTH('Traducciones.Pantalla_Permisos', 'permisos_perfil_familiar') IS NULL
    ALTER TABLE [Traducciones].[Pantalla_Permisos] ADD [permisos_perfil_familiar] NVARCHAR(500) NOT NULL DEFAULT N'';
GO

UPDATE [Traducciones].[Pantalla_Permisos] SET
    [permisos_titulo] = CASE [IdiomaID] WHEN 1 THEN N'Permisos' WHEN 2 THEN N'Permissions' WHEN 3 THEN N'Permissões' WHEN 4 THEN N'Autorisations' WHEN 5 THEN N'権限' ELSE N'Permisos' END,
    [permisos_subtitulo] = CASE [IdiomaID] WHEN 1 THEN N'Perfiles, familias de permisos y el perfil de cada usuario.' WHEN 2 THEN N'Profiles, permission groups and each user''s profile.' WHEN 3 THEN N'Perfis, famílias de permissões e o perfil de cada usuário.' WHEN 4 THEN N'Profils, familles d''autorisations et profil de chaque utilisateur.' WHEN 5 THEN N'プロファイル、権限グループ、各ユーザーのプロファイル。' ELSE N'Perfiles, familias de permisos y el perfil de cada usuario.' END,
    [permisos_sin_tabla_titulo] = CASE [IdiomaID] WHEN 1 THEN N'Faltan las tablas de permisos' WHEN 2 THEN N'Permission tables are missing' WHEN 3 THEN N'Faltam as tabelas de permissões' WHEN 4 THEN N'Les tables d''autorisations sont absentes' WHEN 5 THEN N'権限テーブルがありません' ELSE N'Faltan las tablas de permisos' END,
    [permisos_sin_tabla_msg] = CASE [IdiomaID] WHEN 1 THEN N'Corré scripts/crear-permisos-composite.sql en la base de datos y volvé a entrar.' WHEN 2 THEN N'Run scripts/crear-permisos-composite.sql on the database and reload.' WHEN 3 THEN N'Execute scripts/crear-permisos-composite.sql no banco de dados e recarregue.' WHEN 4 THEN N'Exécutez scripts/crear-permisos-composite.sql sur la base de données puis rechargez.' WHEN 5 THEN N'データベースで scripts/crear-permisos-composite.sql を実行してから再読み込みしてください。' ELSE N'Corré scripts/crear-permisos-composite.sql en la base de datos y volvé a entrar.' END,
    [permisos_tab_perfiles] = CASE [IdiomaID] WHEN 1 THEN N'Perfiles' WHEN 2 THEN N'Profiles' WHEN 3 THEN N'Perfis' WHEN 4 THEN N'Profils' WHEN 5 THEN N'プロファイル' ELSE N'Perfiles' END,
    [permisos_tab_familias] = CASE [IdiomaID] WHEN 1 THEN N'Familias' WHEN 2 THEN N'Permission groups' WHEN 3 THEN N'Famílias' WHEN 4 THEN N'Familles' WHEN 5 THEN N'権限グループ' ELSE N'Familias' END,
    [permisos_tab_usuarios] = CASE [IdiomaID] WHEN 1 THEN N'Usuarios' WHEN 2 THEN N'Users' WHEN 3 THEN N'Usuários' WHEN 4 THEN N'Utilisateurs' WHEN 5 THEN N'ユーザー' ELSE N'Usuarios' END,
    [permisos_ayuda_perfiles] = CASE [IdiomaID] WHEN 1 THEN N'Un perfil es lo que se le asigna a cada usuario: permisos sueltos y familias de permisos. Un mismo permiso no puede llegar dos veces a un perfil.' WHEN 2 THEN N'A profile is what each user is given: individual permissions and permission groups. The same permission can''t reach a profile twice.' WHEN 3 THEN N'Um perfil é o que cada usuário recebe: permissões avulsas e famílias de permissões. A mesma permissão não pode chegar duas vezes a um perfil.' WHEN 4 THEN N'Un profil est ce que reçoit chaque utilisateur : des autorisations isolées et des familles d''autorisations. Une même autorisation ne peut pas arriver deux fois dans un profil.' WHEN 5 THEN N'プロファイルは各ユーザーに割り当てるもので、個別の権限と権限グループで構成されます。同じ権限を1つのプロファイルに2回含めることはできません。' ELSE N'Un perfil es lo que se le asigna a cada usuario: permisos sueltos y familias de permisos. Un mismo permiso no puede llegar dos veces a un perfil.' END,
    [permisos_ayuda_familias] = CASE [IdiomaID] WHEN 1 THEN N'Una familia agrupa permisos y otras familias. Si cambiás una familia, el cambio llega a todos los perfiles que la usan.' WHEN 2 THEN N'A group bundles permissions and other groups. Changing a group affects every profile that uses it.' WHEN 3 THEN N'Uma família agrupa permissões e outras famílias. Ao alterar uma família, a mudança chega a todos os perfis que a usam.' WHEN 4 THEN N'Une famille regroupe des autorisations et d''autres familles. Modifier une famille modifie tous les profils qui l''utilisent.' WHEN 5 THEN N'グループは権限や他のグループをまとめたものです。グループを変更すると、それを使うすべてのプロファイルに反映されます。' ELSE N'Una familia agrupa permisos y otras familias. Si cambiás una familia, el cambio llega a todos los perfiles que la usan.' END,
    [permisos_ayuda_usuarios] = CASE [IdiomaID] WHEN 1 THEN N'Cada usuario tiene un solo perfil. Las cuentas nuevas reciben el perfil de su tipo.' WHEN 2 THEN N'Each user has exactly one profile. New accounts get the profile for their type.' WHEN 3 THEN N'Cada usuário tem um único perfil. Contas novas recebem o perfil do seu tipo.' WHEN 4 THEN N'Chaque utilisateur a un seul profil. Les nouveaux comptes reçoivent le profil de leur type.' WHEN 5 THEN N'各ユーザーのプロファイルは1つだけです。新しいアカウントには種類に応じたプロファイルが割り当てられます。' ELSE N'Cada usuario tiene un solo perfil. Las cuentas nuevas reciben el perfil de su tipo.' END,
    [permisos_btn_nuevo_perfil] = CASE [IdiomaID] WHEN 1 THEN N'Nuevo perfil' WHEN 2 THEN N'New profile' WHEN 3 THEN N'Novo perfil' WHEN 4 THEN N'Nouveau profil' WHEN 5 THEN N'新しいプロファイル' ELSE N'Nuevo perfil' END,
    [permisos_btn_nueva_familia] = CASE [IdiomaID] WHEN 1 THEN N'Nueva familia' WHEN 2 THEN N'New group' WHEN 3 THEN N'Nova família' WHEN 4 THEN N'Nouvelle famille' WHEN 5 THEN N'新しいグループ' ELSE N'Nueva familia' END,
    [permisos_btn_editar] = CASE [IdiomaID] WHEN 1 THEN N'Editar' WHEN 2 THEN N'Edit' WHEN 3 THEN N'Editar' WHEN 4 THEN N'Modifier' WHEN 5 THEN N'編集' ELSE N'Editar' END,
    [permisos_btn_eliminar] = CASE [IdiomaID] WHEN 1 THEN N'Eliminar' WHEN 2 THEN N'Delete' WHEN 3 THEN N'Excluir' WHEN 4 THEN N'Supprimer' WHEN 5 THEN N'削除' ELSE N'Eliminar' END,
    [permisos_btn_agregar] = CASE [IdiomaID] WHEN 1 THEN N'Agregar' WHEN 2 THEN N'Add' WHEN 3 THEN N'Adicionar' WHEN 4 THEN N'Ajouter' WHEN 5 THEN N'追加' ELSE N'Agregar' END,
    [permisos_btn_quitar] = CASE [IdiomaID] WHEN 1 THEN N'Quitar' WHEN 2 THEN N'Remove' WHEN 3 THEN N'Remover' WHEN 4 THEN N'Retirer' WHEN 5 THEN N'外す' ELSE N'Quitar' END,
    [permisos_btn_asignar] = CASE [IdiomaID] WHEN 1 THEN N'Guardar' WHEN 2 THEN N'Save' WHEN 3 THEN N'Salvar' WHEN 4 THEN N'Enregistrer' WHEN 5 THEN N'保存' ELSE N'Guardar' END,
    [permisos_btn_cancelar] = CASE [IdiomaID] WHEN 1 THEN N'Cancelar' WHEN 2 THEN N'Cancel' WHEN 3 THEN N'Cancelar' WHEN 4 THEN N'Annuler' WHEN 5 THEN N'キャンセル' ELSE N'Cancelar' END,
    [permisos_badge_sistema] = CASE [IdiomaID] WHEN 1 THEN N'Del sistema' WHEN 2 THEN N'System' WHEN 3 THEN N'Do sistema' WHEN 4 THEN N'Système' WHEN 5 THEN N'システム' ELSE N'Del sistema' END,
    [permisos_usuarios_cantidad] = CASE [IdiomaID] WHEN 1 THEN N'{0} usuario(s)' WHEN 2 THEN N'{0} user(s)' WHEN 3 THEN N'{0} usuário(s)' WHEN 4 THEN N'{0} utilisateur(s)' WHEN 5 THEN N'ユーザー {0} 人' ELSE N'{0} usuario(s)' END,
    [permisos_usada_en] = CASE [IdiomaID] WHEN 1 THEN N'Usada en: {0}' WHEN 2 THEN N'Used in: {0}' WHEN 3 THEN N'Usada em: {0}' WHEN 4 THEN N'Utilisée dans : {0}' WHEN 5 THEN N'使用先：{0}' ELSE N'Usada en: {0}' END,
    [permisos_sin_uso] = CASE [IdiomaID] WHEN 1 THEN N'Sin usar' WHEN 2 THEN N'Not used' WHEN 3 THEN N'Sem uso' WHEN 4 THEN N'Non utilisée' WHEN 5 THEN N'未使用' ELSE N'Sin usar' END,
    [permisos_webmaster_todo] = CASE [IdiomaID] WHEN 1 THEN N'Tiene todos los permisos del sistema, siempre.' WHEN 2 THEN N'Always has every permission in the system.' WHEN 3 THEN N'Tem sempre todas as permissões do sistema.' WHEN 4 THEN N'Possède toujours toutes les autorisations du système.' WHEN 5 THEN N'システムのすべての権限を常に持ちます。' ELSE N'Tiene todos los permisos del sistema, siempre.' END,
    [permisos_sin_perfiles] = CASE [IdiomaID] WHEN 1 THEN N'No hay perfiles.' WHEN 2 THEN N'No profiles.' WHEN 3 THEN N'Não há perfis.' WHEN 4 THEN N'Aucun profil.' WHEN 5 THEN N'プロファイルがありません。' ELSE N'No hay perfiles.' END,
    [permisos_sin_familias] = CASE [IdiomaID] WHEN 1 THEN N'No hay familias.' WHEN 2 THEN N'No groups.' WHEN 3 THEN N'Não há famílias.' WHEN 4 THEN N'Aucune famille.' WHEN 5 THEN N'グループがありません。' ELSE N'No hay familias.' END,
    [permisos_confirmar_eliminar] = CASE [IdiomaID] WHEN 1 THEN N'¿Seguro que querés eliminarlo?' WHEN 2 THEN N'Are you sure you want to delete it?' WHEN 3 THEN N'Tem certeza de que deseja excluí-lo?' WHEN 4 THEN N'Voulez-vous vraiment le supprimer ?' WHEN 5 THEN N'本当に削除しますか？' ELSE N'¿Seguro que querés eliminarlo?' END,
    [permisos_editor_nuevo_perfil] = CASE [IdiomaID] WHEN 1 THEN N'Nuevo perfil' WHEN 2 THEN N'New profile' WHEN 3 THEN N'Novo perfil' WHEN 4 THEN N'Nouveau profil' WHEN 5 THEN N'新しいプロファイル' ELSE N'Nuevo perfil' END,
    [permisos_editor_editar_perfil] = CASE [IdiomaID] WHEN 1 THEN N'Editar perfil' WHEN 2 THEN N'Edit profile' WHEN 3 THEN N'Editar perfil' WHEN 4 THEN N'Modifier le profil' WHEN 5 THEN N'プロファイルを編集' ELSE N'Editar perfil' END,
    [permisos_editor_nueva_familia] = CASE [IdiomaID] WHEN 1 THEN N'Nueva familia' WHEN 2 THEN N'New group' WHEN 3 THEN N'Nova família' WHEN 4 THEN N'Nouvelle famille' WHEN 5 THEN N'新しいグループ' ELSE N'Nueva familia' END,
    [permisos_editor_editar_familia] = CASE [IdiomaID] WHEN 1 THEN N'Editar familia' WHEN 2 THEN N'Edit group' WHEN 3 THEN N'Editar família' WHEN 4 THEN N'Modifier la famille' WHEN 5 THEN N'グループを編集' ELSE N'Editar familia' END,
    [permisos_campo_nombre] = CASE [IdiomaID] WHEN 1 THEN N'Nombre' WHEN 2 THEN N'Name' WHEN 3 THEN N'Nome' WHEN 4 THEN N'Nom' WHEN 5 THEN N'名前' ELSE N'Nombre' END,
    [permisos_nombre_bloqueado] = CASE [IdiomaID] WHEN 1 THEN N'Los perfiles y familias del sistema no se renombran.' WHEN 2 THEN N'System profiles and groups can''t be renamed.' WHEN 3 THEN N'Perfis e famílias do sistema não podem ser renomeados.' WHEN 4 THEN N'Les profils et familles du système ne peuvent pas être renommés.' WHEN 5 THEN N'システムのプロファイルとグループは名前を変更できません。' ELSE N'Los perfiles y familias del sistema no se renombran.' END,
    [permisos_disponibles] = CASE [IdiomaID] WHEN 1 THEN N'Disponibles' WHEN 2 THEN N'Available' WHEN 3 THEN N'Disponíveis' WHEN 4 THEN N'Disponibles' WHEN 5 THEN N'選択可能' ELSE N'Disponibles' END,
    [permisos_seccion_permisos] = CASE [IdiomaID] WHEN 1 THEN N'Permisos' WHEN 2 THEN N'Permissions' WHEN 3 THEN N'Permissões' WHEN 4 THEN N'Autorisations' WHEN 5 THEN N'権限' ELSE N'Permisos' END,
    [permisos_seccion_familias] = CASE [IdiomaID] WHEN 1 THEN N'Familias' WHEN 2 THEN N'Groups' WHEN 3 THEN N'Famílias' WHEN 4 THEN N'Familles' WHEN 5 THEN N'グループ' ELSE N'Familias' END,
    [permisos_contenido] = CASE [IdiomaID] WHEN 1 THEN N'Contenido' WHEN 2 THEN N'Contents' WHEN 3 THEN N'Conteúdo' WHEN 4 THEN N'Contenu' WHEN 5 THEN N'内容' ELSE N'Contenido' END,
    [permisos_contenido_vacio] = CASE [IdiomaID] WHEN 1 THEN N'Todavía no agregaste nada. Elegí permisos o familias de la izquierda.' WHEN 2 THEN N'Nothing added yet. Pick permissions or groups on the left.' WHEN 3 THEN N'Nada adicionado ainda. Escolha permissões ou famílias à esquerda.' WHEN 4 THEN N'Rien d''ajouté pour l''instant. Choisissez des autorisations ou des familles à gauche.' WHEN 5 THEN N'まだ何も追加されていません。左から権限またはグループを選んでください。' ELSE N'Todavía no agregaste nada. Elegí permisos o familias de la izquierda.' END,
    [permisos_ya_incluido] = CASE [IdiomaID] WHEN 1 THEN N'Incluido' WHEN 2 THEN N'Included' WHEN 3 THEN N'Incluído' WHEN 4 THEN N'Inclus' WHEN 5 THEN N'追加済み' ELSE N'Incluido' END,
    [permisos_editado] = CASE [IdiomaID] WHEN 1 THEN N'lo que estás editando' WHEN 2 THEN N'what you''re editing' WHEN 3 THEN N'o que você está editando' WHEN 4 THEN N'ce que vous modifiez' WHEN 5 THEN N'編集中の項目' ELSE N'lo que estás editando' END,
    [permisos_buscar_usuario] = CASE [IdiomaID] WHEN 1 THEN N'Buscar usuario o nombre' WHEN 2 THEN N'Search user or name' WHEN 3 THEN N'Buscar usuário ou nome' WHEN 4 THEN N'Rechercher un utilisateur ou un nom' WHEN 5 THEN N'ユーザー名または氏名で検索' ELSE N'Buscar usuario o nombre' END,
    [permisos_col_usuario] = CASE [IdiomaID] WHEN 1 THEN N'Usuario' WHEN 2 THEN N'User' WHEN 3 THEN N'Usuário' WHEN 4 THEN N'Utilisateur' WHEN 5 THEN N'ユーザー' ELSE N'Usuario' END,
    [permisos_col_nombre] = CASE [IdiomaID] WHEN 1 THEN N'Nombre' WHEN 2 THEN N'Name' WHEN 3 THEN N'Nome' WHEN 4 THEN N'Nom' WHEN 5 THEN N'氏名' ELSE N'Nombre' END,
    [permisos_col_tipo] = CASE [IdiomaID] WHEN 1 THEN N'Tipo de cuenta' WHEN 2 THEN N'Account type' WHEN 3 THEN N'Tipo de conta' WHEN 4 THEN N'Type de compte' WHEN 5 THEN N'アカウントの種類' ELSE N'Tipo de cuenta' END,
    [permisos_col_estado] = CASE [IdiomaID] WHEN 1 THEN N'Estado' WHEN 2 THEN N'Status' WHEN 3 THEN N'Status' WHEN 4 THEN N'Statut' WHEN 5 THEN N'状態' ELSE N'Estado' END,
    [permisos_col_perfil] = CASE [IdiomaID] WHEN 1 THEN N'Perfil' WHEN 2 THEN N'Profile' WHEN 3 THEN N'Perfil' WHEN 4 THEN N'Profil' WHEN 5 THEN N'プロファイル' ELSE N'Perfil' END,
    [permisos_sin_perfil] = CASE [IdiomaID] WHEN 1 THEN N'— Sin perfil —' WHEN 2 THEN N'— No profile —' WHEN 3 THEN N'— Sem perfil —' WHEN 4 THEN N'— Aucun profil —' WHEN 5 THEN N'— なし —' ELSE N'— Sin perfil —' END,
    [permisos_sin_usuarios] = CASE [IdiomaID] WHEN 1 THEN N'No hay usuarios que coincidan.' WHEN 2 THEN N'No matching users.' WHEN 3 THEN N'Nenhum usuário encontrado.' WHEN 4 THEN N'Aucun utilisateur trouvé.' WHEN 5 THEN N'該当するユーザーがいません。' ELSE N'No hay usuarios que coincidan.' END,
    [permisos_estado_activo] = CASE [IdiomaID] WHEN 1 THEN N'Activo' WHEN 2 THEN N'Active' WHEN 3 THEN N'Ativo' WHEN 4 THEN N'Actif' WHEN 5 THEN N'有効' ELSE N'Activo' END,
    [permisos_estado_inactivo] = CASE [IdiomaID] WHEN 1 THEN N'Inactivo' WHEN 2 THEN N'Inactive' WHEN 3 THEN N'Inativo' WHEN 4 THEN N'Inactif' WHEN 5 THEN N'無効' ELSE N'Inactivo' END,
    [permisos_elegir_perfil] = CASE [IdiomaID] WHEN 1 THEN N'Elegí un perfil.' WHEN 2 THEN N'Choose a profile.' WHEN 3 THEN N'Escolha um perfil.' WHEN 4 THEN N'Choisissez un profil.' WHEN 5 THEN N'プロファイルを選んでください。' ELSE N'Elegí un perfil.' END,
    [permisos_tipo_1] = CASE [IdiomaID] WHEN 1 THEN N'Administrador' WHEN 2 THEN N'Administrator' WHEN 3 THEN N'Administrador' WHEN 4 THEN N'Administrateur' WHEN 5 THEN N'管理者' ELSE N'Administrador' END,
    [permisos_tipo_2] = CASE [IdiomaID] WHEN 1 THEN N'Recepcionista' WHEN 2 THEN N'Receptionist' WHEN 3 THEN N'Recepcionista' WHEN 4 THEN N'Réceptionniste' WHEN 5 THEN N'受付' ELSE N'Recepcionista' END,
    [permisos_tipo_3] = CASE [IdiomaID] WHEN 1 THEN N'Entrenador' WHEN 2 THEN N'Trainer' WHEN 3 THEN N'Treinador' WHEN 4 THEN N'Entraîneur' WHEN 5 THEN N'トレーナー' ELSE N'Entrenador' END,
    [permisos_tipo_4] = CASE [IdiomaID] WHEN 1 THEN N'Cliente' WHEN 2 THEN N'Client' WHEN 3 THEN N'Cliente' WHEN 4 THEN N'Client' WHEN 5 THEN N'会員' ELSE N'Cliente' END,
    [permisos_tipo_5] = CASE [IdiomaID] WHEN 1 THEN N'WebMaster' WHEN 2 THEN N'WebMaster' WHEN 3 THEN N'WebMaster' WHEN 4 THEN N'WebMaster' WHEN 5 THEN N'WebMaster' ELSE N'WebMaster' END,
    [permisos_tipo_6] = CASE [IdiomaID] WHEN 1 THEN N'Familiar' WHEN 2 THEN N'Family member' WHEN 3 THEN N'Familiar' WHEN 4 THEN N'Membre de la famille' WHEN 5 THEN N'家族' ELSE N'Familiar' END,
    [permisos_msg_perfil_guardado] = CASE [IdiomaID] WHEN 1 THEN N'Perfil guardado.' WHEN 2 THEN N'Profile saved.' WHEN 3 THEN N'Perfil salvo.' WHEN 4 THEN N'Profil enregistré.' WHEN 5 THEN N'プロファイルを保存しました。' ELSE N'Perfil guardado.' END,
    [permisos_msg_familia_guardada] = CASE [IdiomaID] WHEN 1 THEN N'Familia guardada.' WHEN 2 THEN N'Group saved.' WHEN 3 THEN N'Família salva.' WHEN 4 THEN N'Famille enregistrée.' WHEN 5 THEN N'グループを保存しました。' ELSE N'Familia guardada.' END,
    [permisos_msg_perfil_eliminado] = CASE [IdiomaID] WHEN 1 THEN N'Perfil eliminado.' WHEN 2 THEN N'Profile deleted.' WHEN 3 THEN N'Perfil excluído.' WHEN 4 THEN N'Profil supprimé.' WHEN 5 THEN N'プロファイルを削除しました。' ELSE N'Perfil eliminado.' END,
    [permisos_msg_familia_eliminada] = CASE [IdiomaID] WHEN 1 THEN N'Familia eliminada.' WHEN 2 THEN N'Group deleted.' WHEN 3 THEN N'Família excluída.' WHEN 4 THEN N'Famille supprimée.' WHEN 5 THEN N'グループを削除しました。' ELSE N'Familia eliminada.' END,
    [permisos_msg_perfil_asignado] = CASE [IdiomaID] WHEN 1 THEN N'Perfil de {0} actualizado.' WHEN 2 THEN N'Profile of {0} updated.' WHEN 3 THEN N'Perfil de {0} atualizado.' WHEN 4 THEN N'Profil de {0} mis à jour.' WHEN 5 THEN N'{0} のプロファイルを更新しました。' ELSE N'Perfil de {0} actualizado.' END,
    [permisos_err_sin_permiso] = CASE [IdiomaID] WHEN 1 THEN N'No tenés permiso para gestionar permisos.' WHEN 2 THEN N'You don''t have permission to manage permissions.' WHEN 3 THEN N'Você não tem permissão para gerenciar permissões.' WHEN 4 THEN N'Vous n''avez pas l''autorisation de gérer les autorisations.' WHEN 5 THEN N'権限を管理する権限がありません。' ELSE N'No tenés permiso para gestionar permisos.' END,
    [permisos_err_no_existe] = CASE [IdiomaID] WHEN 1 THEN N'Ya no existe: puede que otra persona lo haya eliminado. Recargá la pantalla.' WHEN 2 THEN N'It no longer exists; someone may have deleted it. Reload the page.' WHEN 3 THEN N'Não existe mais; alguém pode tê-lo excluído. Recarregue a página.' WHEN 4 THEN N'Il n''existe plus ; quelqu''un l''a peut-être supprimé. Rechargez la page.' WHEN 5 THEN N'すでに存在しません。他の人が削除した可能性があります。ページを再読み込みしてください。' ELSE N'Ya no existe: puede que otra persona lo haya eliminado. Recargá la pantalla.' END,
    [permisos_err_nombre_obligatorio] = CASE [IdiomaID] WHEN 1 THEN N'El nombre es obligatorio.' WHEN 2 THEN N'Name is required.' WHEN 3 THEN N'O nome é obrigatório.' WHEN 4 THEN N'Le nom est obligatoire.' WHEN 5 THEN N'名前は必須です。' ELSE N'El nombre es obligatorio.' END,
    [permisos_err_nombre_largo] = CASE [IdiomaID] WHEN 1 THEN N'El nombre no puede superar los {0} caracteres.' WHEN 2 THEN N'Name can''t exceed {0} characters.' WHEN 3 THEN N'O nome não pode ter mais de {0} caracteres.' WHEN 4 THEN N'Le nom ne peut pas dépasser {0} caractères.' WHEN 5 THEN N'名前は{0}文字以内にしてください。' ELSE N'El nombre no puede superar los {0} caracteres.' END,
    [permisos_err_nombre_repetido_familia] = CASE [IdiomaID] WHEN 1 THEN N'Ya existe una familia llamada {0}.' WHEN 2 THEN N'A group named {0} already exists.' WHEN 3 THEN N'Já existe uma família chamada {0}.' WHEN 4 THEN N'Une famille nommée {0} existe déjà.' WHEN 5 THEN N'{0} という名前のグループはすでにあります。' ELSE N'Ya existe una familia llamada {0}.' END,
    [permisos_err_nombre_repetido_perfil] = CASE [IdiomaID] WHEN 1 THEN N'Ya existe un perfil llamado {0}.' WHEN 2 THEN N'A profile named {0} already exists.' WHEN 3 THEN N'Já existe um perfil chamado {0}.' WHEN 4 THEN N'Un profil nommé {0} existe déjà.' WHEN 5 THEN N'{0} という名前のプロファイルはすでにあります。' ELSE N'Ya existe un perfil llamado {0}.' END,
    [permisos_err_familia_vacia] = CASE [IdiomaID] WHEN 1 THEN N'La familia tiene que tener al menos un permiso o una familia.' WHEN 2 THEN N'A group needs at least one permission or group.' WHEN 3 THEN N'A família precisa ter pelo menos uma permissão ou família.' WHEN 4 THEN N'Une famille doit contenir au moins une autorisation ou une famille.' WHEN 5 THEN N'グループには少なくとも1つの権限またはグループが必要です。' ELSE N'La familia tiene que tener al menos un permiso o una familia.' END,
    [permisos_err_perfil_vacio] = CASE [IdiomaID] WHEN 1 THEN N'El perfil tiene que tener al menos un permiso o una familia.' WHEN 2 THEN N'A profile needs at least one permission or group.' WHEN 3 THEN N'O perfil precisa ter pelo menos uma permissão ou família.' WHEN 4 THEN N'Un profil doit contenir au moins une autorisation ou une famille.' WHEN 5 THEN N'プロファイルには少なくとも1つの権限またはグループが必要です。' ELSE N'El perfil tiene que tener al menos un permiso o una familia.' END,
    [permisos_err_componente_inexistente] = CASE [IdiomaID] WHEN 1 THEN N'Alguno de los permisos o familias elegidos ya no existe. Cerrá el formulario y volvé a abrirlo.' WHEN 2 THEN N'One of the chosen permissions or groups no longer exists. Close the form and open it again.' WHEN 3 THEN N'Alguma das permissões ou famílias escolhidas não existe mais. Feche o formulário e abra-o novamente.' WHEN 4 THEN N'Une des autorisations ou familles choisies n''existe plus. Fermez le formulaire et rouvrez-le.' WHEN 5 THEN N'選んだ権限またはグループの一部がすでに存在しません。フォームを閉じて開き直してください。' ELSE N'Alguno de los permisos o familias elegidos ya no existe. Cerrá el formulario y volvé a abrirlo.' END,
    [permisos_err_familia_en_uso] = CASE [IdiomaID] WHEN 1 THEN N'La familia {0} se usa en: {1}. Quitala de ahí antes de eliminarla.' WHEN 2 THEN N'Group {0} is used in: {1}. Remove it from there before deleting it.' WHEN 3 THEN N'A família {0} é usada em: {1}. Remova-a de lá antes de excluí-la.' WHEN 4 THEN N'La famille {0} est utilisée dans : {1}. Retirez-la avant de la supprimer.' WHEN 5 THEN N'グループ {0} は次で使われています：{1}。削除する前にそこから外してください。' ELSE N'La familia {0} se usa en: {1}. Quitala de ahí antes de eliminarla.' END,
    [permisos_err_perfil_sistema_eliminar] = CASE [IdiomaID] WHEN 1 THEN N'{0} es un perfil del sistema: no se puede eliminar.' WHEN 2 THEN N'{0} is a system profile and can''t be deleted.' WHEN 3 THEN N'{0} é um perfil do sistema e não pode ser excluído.' WHEN 4 THEN N'{0} est un profil du système et ne peut pas être supprimé.' WHEN 5 THEN N'{0} はシステムのプロファイルのため削除できません。' ELSE N'{0} es un perfil del sistema: no se puede eliminar.' END,
    [permisos_err_perfil_con_usuarios] = CASE [IdiomaID] WHEN 1 THEN N'El perfil {0} lo tienen {1} usuario(s). Asignales otro perfil antes de eliminarlo.' WHEN 2 THEN N'{1} user(s) have the {0} profile. Give them another profile before deleting it.' WHEN 3 THEN N'{1} usuário(s) têm o perfil {0}. Atribua outro perfil antes de excluí-lo.' WHEN 4 THEN N'{1} utilisateur(s) ont le profil {0}. Attribuez-leur un autre profil avant de le supprimer.' WHEN 5 THEN N'{1} 人のユーザーがプロファイル {0} を使っています。削除する前に別のプロファイルを割り当ててください。' ELSE N'El perfil {0} lo tienen {1} usuario(s). Asignales otro perfil antes de eliminarlo.' END,
    [permisos_err_webmaster_editar] = CASE [IdiomaID] WHEN 1 THEN N'El perfil WebMaster tiene siempre todos los permisos: no se modifica.' WHEN 2 THEN N'The WebMaster profile always has every permission and can''t be changed.' WHEN 3 THEN N'O perfil WebMaster sempre tem todas as permissões e não pode ser alterado.' WHEN 4 THEN N'Le profil WebMaster a toujours toutes les autorisations et ne peut pas être modifié.' WHEN 5 THEN N'WebMaster プロファイルは常にすべての権限を持つため変更できません。' ELSE N'El perfil WebMaster tiene siempre todos los permisos: no se modifica.' END,
    [permisos_err_webmaster_asignar] = CASE [IdiomaID] WHEN 1 THEN N'Solo un WebMaster puede asignar o quitar el perfil WebMaster.' WHEN 2 THEN N'Only a WebMaster can give or remove the WebMaster profile.' WHEN 3 THEN N'Somente um WebMaster pode atribuir ou remover o perfil WebMaster.' WHEN 4 THEN N'Seul un WebMaster peut attribuer ou retirer le profil WebMaster.' WHEN 5 THEN N'WebMaster プロファイルの付与・解除は WebMaster のみが行えます。' ELSE N'Solo un WebMaster puede asignar o quitar el perfil WebMaster.' END,
    [permisos_err_admin_fijos] = CASE [IdiomaID] WHEN 1 THEN N'El perfil {0} tiene que conservar los permisos de Dashboard y Permisos.' WHEN 2 THEN N'The {0} profile must keep the Dashboard and Permissions permissions.' WHEN 3 THEN N'O perfil {0} deve manter as permissões de Dashboard e Permissões.' WHEN 4 THEN N'Le profil {0} doit conserver les autorisations Tableau de bord et Autorisations.' WHEN 5 THEN N'{0} プロファイルにはダッシュボードと権限の権限が必要です。' ELSE N'El perfil {0} tiene que conservar los permisos de Dashboard y Permisos.' END,
    [permisos_err_sin_gestor] = CASE [IdiomaID] WHEN 1 THEN N'No se puede: tiene que quedar al menos un usuario activo con acceso a Permisos.' WHEN 2 THEN N'Not allowed: at least one active user must keep access to Permissions.' WHEN 3 THEN N'Não é possível: pelo menos um usuário ativo deve manter acesso a Permissões.' WHEN 4 THEN N'Impossible : au moins un utilisateur actif doit garder l''accès aux Autorisations.' WHEN 5 THEN N'できません。権限画面にアクセスできる有効なユーザーが少なくとも1人必要です。' ELSE N'No se puede: tiene que quedar al menos un usuario activo con acceso a Permisos.' END,
    [permisos_err_asignar_gestion] = CASE [IdiomaID] WHEN 1 THEN N'Solo quien gestiona permisos puede dar el perfil {0}.' WHEN 2 THEN N'Only someone who manages permissions can give the {0} profile.' WHEN 3 THEN N'Somente quem gerencia permissões pode atribuir o perfil {0}.' WHEN 4 THEN N'Seule une personne qui gère les autorisations peut attribuer le profil {0}.' WHEN 5 THEN N'{0} プロファイルを付与できるのは権限を管理するユーザーのみです。' ELSE N'Solo quien gestiona permisos puede dar el perfil {0}.' END,
    [permisos_err_sin_perfil_defecto] = CASE [IdiomaID] WHEN 1 THEN N'No existe el perfil por defecto para este tipo de cuenta. Revisá la pantalla Permisos.' WHEN 2 THEN N'There''s no default profile for this account type. Check the Permissions page.' WHEN 3 THEN N'Não existe perfil padrão para este tipo de conta. Verifique a tela Permissões.' WHEN 4 THEN N'Aucun profil par défaut pour ce type de compte. Vérifiez la page Autorisations.' WHEN 5 THEN N'この種類のアカウントの既定プロファイルがありません。権限画面を確認してください。' ELSE N'No existe el perfil por defecto para este tipo de cuenta. Revisá la pantalla Permisos.' END,
    [permisos_err_conflicto] = CASE [IdiomaID] WHEN 1 THEN N'Con este cambio, {0} quedaría repetido en {1}.' WHEN 2 THEN N'With this change, {0} would be duplicated in {1}.' WHEN 3 THEN N'Com esta alteração, {0} ficaria repetido em {1}.' WHEN 4 THEN N'Avec ce changement, {0} serait en double dans {1}.' WHEN 5 THEN N'この変更により、{1} で {0} が重複します。' ELSE N'Con este cambio, {0} quedaría repetido en {1}.' END,
    [permisos_err_duplicado] = CASE [IdiomaID] WHEN 1 THEN N'El permiso {0} ya está incluido en {1}.' WHEN 2 THEN N'The {0} permission is already included in {1}.' WHEN 3 THEN N'A permissão {0} já está incluída em {1}.' WHEN 4 THEN N'L''autorisation {0} est déjà incluse dans {1}.' WHEN 5 THEN N'権限 {0} はすでに {1} に含まれています。' ELSE N'El permiso {0} ya está incluido en {1}.' END,
    [permisos_err_repetido] = CASE [IdiomaID] WHEN 1 THEN N'{0} ya está agregado en {1}.' WHEN 2 THEN N'{0} is already added to {1}.' WHEN 3 THEN N'{0} já foi adicionado a {1}.' WHEN 4 THEN N'{0} est déjà ajouté à {1}.' WHEN 5 THEN N'{0} はすでに {1} に追加されています。' ELSE N'{0} ya está agregado en {1}.' END,
    [permisos_err_ciclo] = CASE [IdiomaID] WHEN 1 THEN N'{0} no puede ir dentro de {1} porque {0} ya contiene a {1}.' WHEN 2 THEN N'{0} can''t go inside {1} because {0} already contains {1}.' WHEN 3 THEN N'{0} não pode ficar dentro de {1} porque {0} já contém {1}.' WHEN 4 THEN N'{0} ne peut pas aller dans {1} car {0} contient déjà {1}.' WHEN 5 THEN N'{0} はすでに {1} を含んでいるため、{1} の中に入れられません。' ELSE N'{0} no puede ir dentro de {1} porque {0} ya contiene a {1}.' END,
    [permisos_nombre_gestionidiomas] = CASE [IdiomaID] WHEN 1 THEN N'Gestión de idiomas' WHEN 2 THEN N'Language management' WHEN 3 THEN N'Gestão de idiomas' WHEN 4 THEN N'Gestion des langues' WHEN 5 THEN N'言語の管理' ELSE N'Gestión de idiomas' END,
    [permisos_familia_portal_cliente] = CASE [IdiomaID] WHEN 1 THEN N'Portal del cliente' WHEN 2 THEN N'Client portal' WHEN 3 THEN N'Portal do cliente' WHEN 4 THEN N'Portail client' WHEN 5 THEN N'会員ポータル' ELSE N'Portal del cliente' END,
    [permisos_familia_operacion] = CASE [IdiomaID] WHEN 1 THEN N'Operación del gimnasio' WHEN 2 THEN N'Gym operations' WHEN 3 THEN N'Operação da academia' WHEN 4 THEN N'Fonctionnement de la salle' WHEN 5 THEN N'ジム運営' ELSE N'Operación del gimnasio' END,
    [permisos_familia_admin_gimnasio] = CASE [IdiomaID] WHEN 1 THEN N'Administración del gimnasio' WHEN 2 THEN N'Gym administration' WHEN 3 THEN N'Administração da academia' WHEN 4 THEN N'Administration de la salle' WHEN 5 THEN N'ジム管理' ELSE N'Administración del gimnasio' END,
    [permisos_familia_caja] = CASE [IdiomaID] WHEN 1 THEN N'Caja' WHEN 2 THEN N'Cashier' WHEN 3 THEN N'Caixa' WHEN 4 THEN N'Caisse' WHEN 5 THEN N'会計' ELSE N'Caja' END,
    [permisos_familia_gestion_usuarios] = CASE [IdiomaID] WHEN 1 THEN N'Gestión de usuarios' WHEN 2 THEN N'User management' WHEN 3 THEN N'Gestão de usuários' WHEN 4 THEN N'Gestion des utilisateurs' WHEN 5 THEN N'ユーザー管理' ELSE N'Gestión de usuarios' END,
    [permisos_familia_seguridad] = CASE [IdiomaID] WHEN 1 THEN N'Seguridad' WHEN 2 THEN N'Security' WHEN 3 THEN N'Segurança' WHEN 4 THEN N'Sécurité' WHEN 5 THEN N'セキュリティ' ELSE N'Seguridad' END,
    [permisos_familia_mantenimiento_bd] = CASE [IdiomaID] WHEN 1 THEN N'Mantenimiento de la base de datos' WHEN 2 THEN N'Database maintenance' WHEN 3 THEN N'Manutenção do banco de dados' WHEN 4 THEN N'Maintenance de la base de données' WHEN 5 THEN N'データベース保守' ELSE N'Mantenimiento de la base de datos' END,
    [permisos_perfil_webmaster] = CASE [IdiomaID] WHEN 1 THEN N'WebMaster' WHEN 2 THEN N'WebMaster' WHEN 3 THEN N'WebMaster' WHEN 4 THEN N'WebMaster' WHEN 5 THEN N'WebMaster' ELSE N'WebMaster' END,
    [permisos_perfil_admin] = CASE [IdiomaID] WHEN 1 THEN N'Administrador' WHEN 2 THEN N'Administrator' WHEN 3 THEN N'Administrador' WHEN 4 THEN N'Administrateur' WHEN 5 THEN N'管理者' ELSE N'Administrador' END,
    [permisos_perfil_recepcion] = CASE [IdiomaID] WHEN 1 THEN N'Recepcionista' WHEN 2 THEN N'Receptionist' WHEN 3 THEN N'Recepcionista' WHEN 4 THEN N'Réceptionniste' WHEN 5 THEN N'受付' ELSE N'Recepcionista' END,
    [permisos_perfil_entrenador] = CASE [IdiomaID] WHEN 1 THEN N'Entrenador' WHEN 2 THEN N'Trainer' WHEN 3 THEN N'Treinador' WHEN 4 THEN N'Entraîneur' WHEN 5 THEN N'トレーナー' ELSE N'Entrenador' END,
    [permisos_perfil_cliente] = CASE [IdiomaID] WHEN 1 THEN N'Cliente' WHEN 2 THEN N'Client' WHEN 3 THEN N'Cliente' WHEN 4 THEN N'Client' WHEN 5 THEN N'会員' ELSE N'Cliente' END,
    [permisos_perfil_familiar] = CASE [IdiomaID] WHEN 1 THEN N'Familiar' WHEN 2 THEN N'Family member' WHEN 3 THEN N'Familiar' WHEN 4 THEN N'Membre de la famille' WHEN 5 THEN N'家族' ELSE N'Familiar' END;
GO
