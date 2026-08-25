USE [GymApp]
GO

-- ============================================================
-- MIGRACIÓN: esquema Traducciones (multi-tabla por pantalla)
--
-- Estructura nueva:
--   · Traducciones.Idiomas          → catálogo de idiomas
--   · Traducciones.Pantalla_Login   → 1 fila por idioma, 1 col por tag
--   · Traducciones.Pantalla_Idioma
--   · Traducciones.Pantalla_DashBoard
--   · Traducciones.Comunes_Botones
--   · Traducciones.Comunes_Mensajes
--
-- NOTA: la tabla dbo.Traducciones se deja intacta hasta confirmar
--       que la nueva capa C# funciona correctamente. Luego ejecutar:
--         DROP TABLE dbo.Traducciones;
-- ============================================================


-- ============================================================
-- 1. Esquema
-- ============================================================
IF NOT EXISTS (SELECT 1 FROM sys.schemas WHERE name = 'Traducciones')
    EXEC('CREATE SCHEMA [Traducciones]');
GO


-- ============================================================
-- 2. Traducciones.Idiomas
--    Codigo: coincide con el enum IdiomaApp (ES, EN, PT, FR, JA).
--    IdiomaID es IDENTITY: ES=1, EN=2, PT=3, FR=4, JA=5.
--    El DAL usa (int)IdiomaApp + 1 para el lookup — no cambiar el orden.
-- ============================================================
IF OBJECT_ID('Traducciones.Idiomas', 'U') IS NOT NULL
    DROP TABLE [Traducciones].[Idiomas];
GO

CREATE TABLE [Traducciones].[Idiomas] (
    [IdiomaID]     INT           IDENTITY(1,1) NOT NULL,
    [Codigo]       NVARCHAR(5)                 NOT NULL,
    [NombreIdioma] NVARCHAR(50)                NOT NULL,
    CONSTRAINT PK_Idiomas         PRIMARY KEY ([IdiomaID]),
    CONSTRAINT UQ_Idiomas_Codigo  UNIQUE      ([Codigo]),
    CONSTRAINT UQ_Idiomas_Nombre  UNIQUE      ([NombreIdioma])
);
GO

INSERT INTO [Traducciones].[Idiomas] ([Codigo], [NombreIdioma]) VALUES
('ES', N'Español'),
('EN', N'Inglés'),
('PT', N'Portugués'),
('FR', N'Francés'),
('JA', N'Japonés');
GO


-- ============================================================
-- 3. Traducciones.Pantalla_Login
-- ============================================================
IF OBJECT_ID('Traducciones.Pantalla_Login', 'U') IS NOT NULL
    DROP TABLE [Traducciones].[Pantalla_Login];
GO

CREATE TABLE [Traducciones].[Pantalla_Login] (
    [TraduccionID]              INT           IDENTITY(1,1) NOT NULL,
    [IdiomaID]                  INT                         NOT NULL,
    [login_titulo]              NVARCHAR(500)               NOT NULL,
    [login_usuario]             NVARCHAR(500)               NOT NULL,
    [login_contrasena]          NVARCHAR(500)               NOT NULL,
    [login_btn]                 NVARCHAR(500)               NOT NULL,
    [login_error_credenciales]  NVARCHAR(500)               NOT NULL,
    CONSTRAINT PK_Pantalla_Login        PRIMARY KEY ([TraduccionID]),
    CONSTRAINT FK_Pantalla_Login_Idioma FOREIGN KEY ([IdiomaID]) REFERENCES [Traducciones].[Idiomas]([IdiomaID]),
    CONSTRAINT UQ_Pantalla_Login_Idioma UNIQUE      ([IdiomaID])
);
GO

INSERT INTO [Traducciones].[Pantalla_Login]
    ([IdiomaID], [login_titulo], [login_usuario], [login_contrasena], [login_btn], [login_error_credenciales])
VALUES
(1, N'Iniciar sesión',  N'Usuario',            N'Contraseña',     N'Ingresar',    N'Usuario o contraseña incorrectos.'),
(2, N'Log in',          N'Username',           N'Password',       N'Sign in',     N'Incorrect username or password.'),
(3, N'Entrar',          N'Usuário',            N'Senha',          N'Entrar',      N'Usuário ou senha incorretos.'),
(4, N'Se connecter',    N'Nom d''utilisateur', N'Mot de passe',   N'Connexion',   N'Nom d''utilisateur ou mot de passe incorrect.'),
(5, N'ログイン',         N'ユーザー名',          N'パスワード',      N'ログイン',    N'ユーザー名またはパスワードが間違っています。');
GO


-- ============================================================
-- 4. Traducciones.Pantalla_Idioma
-- ============================================================
IF OBJECT_ID('Traducciones.Pantalla_Idioma', 'U') IS NOT NULL
    DROP TABLE [Traducciones].[Pantalla_Idioma];
GO

CREATE TABLE [Traducciones].[Pantalla_Idioma] (
    [TraduccionID]      INT           IDENTITY(1,1) NOT NULL,
    [IdiomaID]          INT                         NOT NULL,
    [idioma_titulo]     NVARCHAR(500)               NOT NULL,
    [idioma_subtitulo]  NVARCHAR(500)               NOT NULL,
    [idioma_guardado]   NVARCHAR(500)               NOT NULL,
    [idioma_activo]     NVARCHAR(500)               NOT NULL,
    CONSTRAINT PK_Pantalla_Idioma        PRIMARY KEY ([TraduccionID]),
    CONSTRAINT FK_Pantalla_Idioma_Idioma FOREIGN KEY ([IdiomaID]) REFERENCES [Traducciones].[Idiomas]([IdiomaID]),
    CONSTRAINT UQ_Pantalla_Idioma_Idioma UNIQUE      ([IdiomaID])
);
GO

INSERT INTO [Traducciones].[Pantalla_Idioma]
    ([IdiomaID], [idioma_titulo], [idioma_subtitulo], [idioma_guardado], [idioma_activo])
VALUES
(1, N'Seleccionar idioma',    N'Elegí el idioma en que querés usar la aplicación.',               N'Idioma actualizado correctamente.',       N'Activo'),
(2, N'Select language',       N'Choose the language you want to use the app in.',                  N'Language updated successfully.',          N'Active'),
(3, N'Selecionar idioma',     N'Escolha o idioma em que deseja usar o aplicativo.',               N'Idioma atualizado com sucesso.',          N'Ativo'),
(4, N'Sélectionner la langue',N'Choisissez la langue dans laquelle vous souhaitez utiliser l''application.', N'Langue mise à jour avec succès.', N'Actif'),
(5, N'言語を選択',             N'アプリで使用する言語を選んでください。',                              N'言語が正常に更新されました。',              N'有効');
GO


-- ============================================================
-- 5. Traducciones.Pantalla_DashBoard  (sidebar / menú lateral)
-- ============================================================
IF OBJECT_ID('Traducciones.Pantalla_DashBoard', 'U') IS NOT NULL
    DROP TABLE [Traducciones].[Pantalla_DashBoard];
GO

CREATE TABLE [Traducciones].[Pantalla_DashBoard] (
    [TraduccionID]          INT           IDENTITY(1,1) NOT NULL,
    [IdiomaID]              INT                         NOT NULL,
    [menu_dashboard]        NVARCHAR(500)               NOT NULL,
    [menu_usuarios]         NVARCHAR(500)               NOT NULL,
    [menu_alumnos]          NVARCHAR(500)               NOT NULL,
    [menu_entrenadores]     NVARCHAR(500)               NOT NULL,
    [menu_actividades]      NVARCHAR(500)               NOT NULL,
    [menu_rutinas]          NVARCHAR(500)               NOT NULL,
    [menu_permisos]         NVARCHAR(500)               NOT NULL,
    [menu_bitacora]         NVARCHAR(500)               NOT NULL,
    [menu_respaldo]         NVARCHAR(500)               NOT NULL,
    [menu_pagos]            NVARCHAR(500)               NOT NULL,
    [menu_perfil]           NVARCHAR(500)               NOT NULL,
    [menu_idioma]           NVARCHAR(500)               NOT NULL,
    [menu_cambiar_contra]   NVARCHAR(500)               NOT NULL,
    [menu_cerrar_sesion]    NVARCHAR(500)               NOT NULL,
    CONSTRAINT PK_Pantalla_DashBoard        PRIMARY KEY ([TraduccionID]),
    CONSTRAINT FK_Pantalla_DashBoard_Idioma FOREIGN KEY ([IdiomaID]) REFERENCES [Traducciones].[Idiomas]([IdiomaID]),
    CONSTRAINT UQ_Pantalla_DashBoard_Idioma UNIQUE      ([IdiomaID])
);
GO

INSERT INTO [Traducciones].[Pantalla_DashBoard]
    ([IdiomaID], [menu_dashboard], [menu_usuarios], [menu_alumnos], [menu_entrenadores],
     [menu_actividades], [menu_rutinas], [menu_permisos], [menu_bitacora],
     [menu_respaldo], [menu_pagos], [menu_perfil], [menu_idioma],
     [menu_cambiar_contra], [menu_cerrar_sesion])
VALUES
(1, N'Dashboard',       N'Usuarios',       N'Alumnos',     N'Entrenadores',   N'Actividades',   N'Rutinas',   N'Permisos',     N'Bitácora',  N'Gestión de respaldo',   N'Pagos',       N'Perfil',    N'Idioma',    N'Cambiar contraseña',    N'Cerrar sesión'),
(2, N'Dashboard',       N'Users',          N'Students',    N'Trainers',       N'Activities',    N'Routines',  N'Permissions',  N'Audit Log', N'Backup Management',      N'Payments',    N'Profile',   N'Language',  N'Change password',       N'Log out'),
(3, N'Dashboard',       N'Usuários',       N'Alunos',      N'Treinadores',    N'Atividades',    N'Rotinas',   N'Permissões',   N'Bitácora',  N'Gestão de backup',       N'Pagamentos',  N'Perfil',    N'Idioma',    N'Alterar senha',         N'Sair'),
(4, N'Tableau de bord', N'Utilisateurs',   N'Élèves',      N'Entraîneurs',    N'Activités',     N'Routines',  N'Permissions',  N'Journal',   N'Gestion de sauvegarde',  N'Paiements',   N'Profil',    N'Langue',    N'Changer le mot de passe',N'Déconnexion'),
(5, N'ダッシュボード',   N'ユーザー',       N'生徒',         N'トレーナー',      N'アクティビティ', N'ルーティン', N'権限',          N'ログ',      N'バックアップ管理',         N'支払い',      N'プロフィール',N'言語',      N'パスワード変更',          N'ログアウト');
GO


-- ============================================================
-- 6. Traducciones.Comunes_Botones
-- ============================================================
IF OBJECT_ID('Traducciones.Comunes_Botones', 'U') IS NOT NULL
    DROP TABLE [Traducciones].[Comunes_Botones];
GO

CREATE TABLE [Traducciones].[Comunes_Botones] (
    [TraduccionID]   INT           IDENTITY(1,1) NOT NULL,
    [IdiomaID]       INT                         NOT NULL,
    [btn_guardar]    NVARCHAR(500)               NOT NULL,
    [btn_cancelar]   NVARCHAR(500)               NOT NULL,
    [btn_eliminar]   NVARCHAR(500)               NOT NULL,
    [btn_editar]     NVARCHAR(500)               NOT NULL,
    [btn_agregar]    NVARCHAR(500)               NOT NULL,
    [btn_buscar]     NVARCHAR(500)               NOT NULL,
    [btn_volver]     NVARCHAR(500)               NOT NULL,
    [btn_confirmar]  NVARCHAR(500)               NOT NULL,
    CONSTRAINT PK_Comunes_Botones        PRIMARY KEY ([TraduccionID]),
    CONSTRAINT FK_Comunes_Botones_Idioma FOREIGN KEY ([IdiomaID]) REFERENCES [Traducciones].[Idiomas]([IdiomaID]),
    CONSTRAINT UQ_Comunes_Botones_Idioma UNIQUE      ([IdiomaID])
);
GO

INSERT INTO [Traducciones].[Comunes_Botones]
    ([IdiomaID], [btn_guardar], [btn_cancelar], [btn_eliminar], [btn_editar],
     [btn_agregar], [btn_buscar], [btn_volver], [btn_confirmar])
VALUES
(1, N'Guardar',      N'Cancelar', N'Eliminar',  N'Editar',    N'Agregar',    N'Buscar',     N'Volver',   N'Confirmar'),
(2, N'Save',         N'Cancel',   N'Delete',    N'Edit',      N'Add',        N'Search',     N'Back',     N'Confirm'),
(3, N'Salvar',       N'Cancelar', N'Excluir',   N'Editar',    N'Adicionar',  N'Buscar',     N'Voltar',   N'Confirmar'),
(4, N'Enregistrer',  N'Annuler',  N'Supprimer', N'Modifier',  N'Ajouter',    N'Rechercher', N'Retour',   N'Confirmer'),
(5, N'保存',          N'キャンセル',N'削除',       N'編集',      N'追加',        N'検索',       N'戻る',     N'確認');
GO


-- ============================================================
-- 7. Traducciones.Comunes_Mensajes
-- ============================================================
IF OBJECT_ID('Traducciones.Comunes_Mensajes', 'U') IS NOT NULL
    DROP TABLE [Traducciones].[Comunes_Mensajes];
GO

CREATE TABLE [Traducciones].[Comunes_Mensajes] (
    [TraduccionID]          INT           IDENTITY(1,1) NOT NULL,
    [IdiomaID]              INT                         NOT NULL,
    [toast_exito]           NVARCHAR(500)               NOT NULL,
    [toast_error]           NVARCHAR(500)               NOT NULL,
    [toast_advertencia]     NVARCHAR(500)               NOT NULL,
    [toast_informacion]     NVARCHAR(500)               NOT NULL,
    [msg_sin_resultados]    NVARCHAR(500)               NOT NULL,
    [msg_cargando]          NVARCHAR(500)               NOT NULL,
    [msg_error_generico]    NVARCHAR(500)               NOT NULL,
    [msg_acceso_denegado]   NVARCHAR(500)               NOT NULL,
    CONSTRAINT PK_Comunes_Mensajes        PRIMARY KEY ([TraduccionID]),
    CONSTRAINT FK_Comunes_Mensajes_Idioma FOREIGN KEY ([IdiomaID]) REFERENCES [Traducciones].[Idiomas]([IdiomaID]),
    CONSTRAINT UQ_Comunes_Mensajes_Idioma UNIQUE      ([IdiomaID])
);
GO

INSERT INTO [Traducciones].[Comunes_Mensajes]
    ([IdiomaID], [toast_exito], [toast_error], [toast_advertencia], [toast_informacion],
     [msg_sin_resultados], [msg_cargando], [msg_error_generico], [msg_acceso_denegado])
VALUES
(1, N'¡Éxito!',   N'Error',   N'Advertencia',    N'Información', N'No se encontraron resultados.',                           N'Cargando...',      N'Ocurrió un error inesperado. Por favor, intente nuevamente.',              N'No tenés permiso para acceder a esta sección.'),
(2, N'Success!',  N'Error',   N'Warning',         N'Information', N'No results found.',                                       N'Loading...',       N'An unexpected error occurred. Please try again.',                         N'You do not have permission to access this section.'),
(3, N'Sucesso!',  N'Erro',    N'Aviso',           N'Informação',  N'Nenhum resultado encontrado.',                            N'Carregando...',    N'Ocorreu um erro inesperado. Por favor, tente novamente.',                  N'Você não tem permissão para acessar esta seção.'),
(4, N'Succès !',  N'Erreur',  N'Avertissement',   N'Information', N'Aucun résultat trouvé.',                                  N'Chargement...',    N'Une erreur inattendue s''est produite. Veuillez réessayer.',               N'Vous n''avez pas la permission d''accéder à cette section.'),
(5, N'成功！',     N'エラー',  N'警告',             N'情報',        N'結果が見つかりませんでした。',                               N'読み込み中...',    N'予期しないエラーが発生しました。もう一度お試しください。',                     N'このセクションへのアクセス権がありません。');
GO


-- ============================================================
-- (Opcional — ejecutar solo tras validar la nueva capa C#)
-- DROP TABLE dbo.Traducciones;
-- ============================================================


-- ============================================================
-- 8. Traducciones.Pantalla_DashboardContent  (panel principal)
-- ============================================================
IF OBJECT_ID('Traducciones.Pantalla_DashboardContent', 'U') IS NOT NULL
    DROP TABLE [Traducciones].[Pantalla_DashboardContent];
GO

CREATE TABLE [Traducciones].[Pantalla_DashboardContent] (
    [TraduccionID]          INT           IDENTITY(1,1) NOT NULL,
    [IdiomaID]              INT                         NOT NULL,
    [dash_titulo]           NVARCHAR(500)               NOT NULL,
    [dash_bienvenido]       NVARCHAR(500)               NOT NULL,
    [dash_kpi_miembros]     NVARCHAR(500)               NOT NULL,
    [dash_kpi_clases]       NVARCHAR(500)               NOT NULL,
    [dash_kpi_ingresos]     NVARCHAR(500)               NOT NULL,
    [dash_kpi_retencion]    NVARCHAR(500)               NOT NULL,
    [dash_semana_titulo]    NVARCHAR(500)               NOT NULL,
    [dash_col_actividad]    NVARCHAR(500)               NOT NULL,
    [dash_col_instructor]   NVARCHAR(500)               NOT NULL,
    [dash_col_dia]          NVARCHAR(500)               NOT NULL,
    [dash_col_horario]      NVARCHAR(500)               NOT NULL,
    [dash_col_duracion]     NVARCHAR(500)               NOT NULL,
    [dash_col_estado]       NVARCHAR(500)               NOT NULL,
    [dash_badge_completada] NVARCHAR(500)               NOT NULL,
    [dash_badge_pendiente]  NVARCHAR(500)               NOT NULL,
    [dash_badge_programada] NVARCHAR(500)               NOT NULL,
    CONSTRAINT PK_Pantalla_DashboardContent        PRIMARY KEY ([TraduccionID]),
    CONSTRAINT FK_Pantalla_DashboardContent_Idioma FOREIGN KEY ([IdiomaID]) REFERENCES [Traducciones].[Idiomas]([IdiomaID]),
    CONSTRAINT UQ_Pantalla_DashboardContent_Idioma UNIQUE      ([IdiomaID])
);
GO

INSERT INTO [Traducciones].[Pantalla_DashboardContent]
    ([IdiomaID], [dash_titulo], [dash_bienvenido], [dash_kpi_miembros], [dash_kpi_clases],
     [dash_kpi_ingresos], [dash_kpi_retencion], [dash_semana_titulo],
     [dash_col_actividad], [dash_col_instructor], [dash_col_dia], [dash_col_horario],
     [dash_col_duracion], [dash_col_estado],
     [dash_badge_completada], [dash_badge_pendiente], [dash_badge_programada])
VALUES
(1, N'Panel de Administración',   N'Bienvenido al panel de gestión',          N'Miembros Activos',   N'Clases este Mes',      N'Ingresos (Mes)',  N'Tasa Retención',   N'Actividades de la Semana', N'Actividad',  N'Instructor',  N'Día',  N'Horario',   N'Duración', N'Estado', N'Completada', N'Pendiente', N'Programada'),
(2, N'Administration Panel',      N'Welcome to the management panel',         N'Active Members',     N'Classes this Month',   N'Income (Month)',  N'Retention Rate',   N'Weekly Activities',        N'Activity',   N'Instructor',  N'Day',  N'Schedule',  N'Duration', N'Status', N'Completed',  N'Pending',   N'Scheduled'),
(3, N'Painel de Administração',   N'Bem-vindo ao painel de gestão',           N'Membros Ativos',     N'Aulas este Mês',       N'Receita (Mês)',   N'Taxa de Retenção', N'Atividades da Semana',     N'Atividade',  N'Instrutor',   N'Dia',  N'Horário',   N'Duração',  N'Estado', N'Concluída',  N'Pendente',  N'Agendada'),
(4, N'Panneau d''administration', N'Bienvenue sur le panneau de gestion',     N'Membres Actifs',     N'Cours ce Mois',        N'Revenus (Mois)', N'Taux de Rétention', N'Activités de la Semaine',  N'Activité',   N'Instructeur', N'Jour', N'Horaire',   N'Durée',    N'Statut', N'Terminée',   N'En attente',N'Programmée'),
(5, N'管理パネル',                 N'管理パネルへようこそ',                      N'アクティブメンバー',  N'今月のクラス',          N'収入（月）',      N'維持率',           N'週間アクティビティ',        N'アクティビティ',N'インストラクター',N'日', N'スケジュール',N'時間',     N'状態',   N'完了',       N'保留中',    N'予定');
GO


-- ============================================================
-- 9. Traducciones.Pantalla_Usuarios  (gestión de usuarios)
-- ============================================================
IF OBJECT_ID('Traducciones.Pantalla_Usuarios', 'U') IS NOT NULL
    DROP TABLE [Traducciones].[Pantalla_Usuarios];
GO

CREATE TABLE [Traducciones].[Pantalla_Usuarios] (
    [TraduccionID]              INT           IDENTITY(1,1) NOT NULL,
    [IdiomaID]                  INT                         NOT NULL,
    [usuarios_titulo]           NVARCHAR(500)               NOT NULL,
    [usuarios_stat_total]       NVARCHAR(500)               NOT NULL,
    [usuarios_stat_activos]     NVARCHAR(500)               NOT NULL,
    [usuarios_stat_bloqueados]  NVARCHAR(500)               NOT NULL,
    [usuarios_stat_inactivos]   NVARCHAR(500)               NOT NULL,
    [usuarios_lista_titulo]     NVARCHAR(500)               NOT NULL,
    [usuarios_col_usuario]      NVARCHAR(500)               NOT NULL,
    [usuarios_col_estado]       NVARCHAR(500)               NOT NULL,
    [usuarios_col_bloqueado]    NVARCHAR(500)               NOT NULL,
    [usuarios_sin_resultados]   NVARCHAR(500)               NOT NULL,
    [usuarios_btn_crear]        NVARCHAR(500)               NOT NULL,
    [usuarios_btn_modificar]    NVARCHAR(500)               NOT NULL,
    [usuarios_btn_desbloquear]  NVARCHAR(500)               NOT NULL,
    [usuarios_btn_blanquear]    NVARCHAR(500)               NOT NULL,
    [usuarios_btn_activar]      NVARCHAR(500)               NOT NULL,
    [usuarios_btn_desactivar]   NVARCHAR(500)               NOT NULL,
    [usuarios_form_titulo]      NVARCHAR(500)               NOT NULL,
    [usuarios_estado_activo]    NVARCHAR(500)               NOT NULL,
    [usuarios_estado_inactivo]  NVARCHAR(500)               NOT NULL,
    [usuarios_bloqueado_si]     NVARCHAR(500)               NOT NULL,
    CONSTRAINT PK_Pantalla_Usuarios        PRIMARY KEY ([TraduccionID]),
    CONSTRAINT FK_Pantalla_Usuarios_Idioma FOREIGN KEY ([IdiomaID]) REFERENCES [Traducciones].[Idiomas]([IdiomaID]),
    CONSTRAINT UQ_Pantalla_Usuarios_Idioma UNIQUE      ([IdiomaID])
);
GO

INSERT INTO [Traducciones].[Pantalla_Usuarios]
    ([IdiomaID], [usuarios_titulo], [usuarios_stat_total], [usuarios_stat_activos],
     [usuarios_stat_bloqueados], [usuarios_stat_inactivos], [usuarios_lista_titulo],
     [usuarios_col_usuario], [usuarios_col_estado], [usuarios_col_bloqueado],
     [usuarios_sin_resultados], [usuarios_btn_crear], [usuarios_btn_modificar],
     [usuarios_btn_desbloquear], [usuarios_btn_blanquear], [usuarios_btn_activar],
     [usuarios_btn_desactivar], [usuarios_form_titulo],
     [usuarios_estado_activo], [usuarios_estado_inactivo], [usuarios_bloqueado_si])
VALUES
(1, N'Gestión de Usuarios',       N'Total usuarios',      N'Activos',  N'Bloqueados', N'Inactivos', N'Lista de usuarios',      N'Usuario',      N'Estado',  N'Bloqueado', N'No se encontraron usuarios',        N'Crear',   N'Modificar', N'Desbloquear', N'Blanquear contraseña',       N'Activar',   N'Desactivar',   N'Detalle del usuario', N'Activo',  N'Inactivo',  N'Bloqueado'),
(2, N'User Management',           N'Total users',         N'Active',   N'Locked',     N'Inactive',  N'Users list',             N'User',         N'Status',  N'Locked',    N'No users found',                    N'Create',  N'Edit',      N'Unlock',      N'Reset password',             N'Activate',  N'Deactivate',   N'User detail',         N'Active',  N'Inactive',  N'Locked'),
(3, N'Gestão de Usuários',        N'Total usuários',      N'Ativos',   N'Bloqueados', N'Inativos',  N'Lista de usuários',      N'Usuário',      N'Estado',  N'Bloqueado', N'Nenhum usuário encontrado',         N'Criar',   N'Editar',    N'Desbloquear', N'Redefinir senha',            N'Ativar',    N'Desativar',    N'Detalhe do usuário',  N'Ativo',   N'Inativo',   N'Bloqueado'),
(4, N'Gestion des Utilisateurs',  N'Total utilisateurs',  N'Actifs',   N'Bloqués',    N'Inactifs',  N'Liste des utilisateurs', N'Utilisateur',  N'Statut',  N'Bloqué',    N'Aucun utilisateur trouvé',          N'Créer',   N'Modifier',  N'Débloquer',   N'Réinitialiser le mot de passe', N'Activer', N'Désactiver',   N'Détail de l''utilisateur', N'Actif', N'Inactif',  N'Bloqué'),
(5, N'ユーザー管理',               N'総ユーザー数',        N'アクティブ', N'ロック済み', N'非アクティブ', N'ユーザー一覧',          N'ユーザー',     N'状態',    N'ロック',    N'ユーザーが見つかりませんでした',     N'作成',    N'編集',      N'ロック解除',  N'パスワードリセット',         N'有効化',    N'無効化',       N'ユーザー詳細',        N'アクティブ',N'非アクティブ',N'ロック済み');
GO


-- ============================================================
-- 10. Traducciones.Pantalla_Alumnos  (gestión de alumnos)
-- ============================================================
IF OBJECT_ID('Traducciones.Pantalla_Alumnos', 'U') IS NOT NULL
    DROP TABLE [Traducciones].[Pantalla_Alumnos];
GO

CREATE TABLE [Traducciones].[Pantalla_Alumnos] (
    [TraduccionID]                  INT           IDENTITY(1,1) NOT NULL,
    [IdiomaID]                      INT                         NOT NULL,
    [alumnos_titulo]                NVARCHAR(500)               NOT NULL,
    [alumnos_stat_total]            NVARCHAR(500)               NOT NULL,
    [alumnos_stat_activos]          NVARCHAR(500)               NOT NULL,
    [alumnos_stat_con_rutinas]      NVARCHAR(500)               NOT NULL,
    [alumnos_stat_sin_usuario]      NVARCHAR(500)               NOT NULL,
    [alumnos_lista_titulo]          NVARCHAR(500)               NOT NULL,
    [alumnos_col_alumno]            NVARCHAR(500)               NOT NULL,
    [alumnos_col_estado]            NVARCHAR(500)               NOT NULL,
    [alumnos_sin_resultados]        NVARCHAR(500)               NOT NULL,
    [alumnos_btn_crear]             NVARCHAR(500)               NOT NULL,
    [alumnos_btn_modificar]         NVARCHAR(500)               NOT NULL,
    [alumnos_btn_eliminar]          NVARCHAR(500)               NOT NULL,
    [alumnos_btn_asociar]           NVARCHAR(500)               NOT NULL,
    [alumnos_form_titulo]           NVARCHAR(500)               NOT NULL,
    [alumnos_estado_activo]         NVARCHAR(500)               NOT NULL,
    [alumnos_estado_inactivo]       NVARCHAR(500)               NOT NULL,
    [alumnos_confirmar_elim_titulo] NVARCHAR(500)               NOT NULL,
    [alumnos_confirmar_elim_msg]    NVARCHAR(500)               NOT NULL,
    [alumnos_confirmar_elim_aviso]  NVARCHAR(500)               NOT NULL,
    CONSTRAINT PK_Pantalla_Alumnos        PRIMARY KEY ([TraduccionID]),
    CONSTRAINT FK_Pantalla_Alumnos_Idioma FOREIGN KEY ([IdiomaID]) REFERENCES [Traducciones].[Idiomas]([IdiomaID]),
    CONSTRAINT UQ_Pantalla_Alumnos_Idioma UNIQUE      ([IdiomaID])
);
GO

INSERT INTO [Traducciones].[Pantalla_Alumnos]
    ([IdiomaID], [alumnos_titulo], [alumnos_stat_total], [alumnos_stat_activos],
     [alumnos_stat_con_rutinas], [alumnos_stat_sin_usuario], [alumnos_lista_titulo],
     [alumnos_col_alumno], [alumnos_col_estado], [alumnos_sin_resultados],
     [alumnos_btn_crear], [alumnos_btn_modificar], [alumnos_btn_eliminar], [alumnos_btn_asociar],
     [alumnos_form_titulo], [alumnos_estado_activo], [alumnos_estado_inactivo],
     [alumnos_confirmar_elim_titulo], [alumnos_confirmar_elim_msg], [alumnos_confirmar_elim_aviso])
VALUES
(1, N'Gestión de Alumnos',     N'Total alumnos',    N'Activos',  N'Con rutinas',    N'Sin usuario',      N'Lista de alumnos',   N'Alumno',   N'Estado', N'No se encontraron alumnos',        N'Crear',  N'Modificar', N'Eliminar', N'Asociar Usuario',    N'Detalle del alumno', N'Activo',  N'Inactivo',  N'Confirmar Eliminación',  N'¿Está seguro que desea eliminar este alumno?',              N'Esta acción eliminará también todas sus rutinas asociadas.'),
(2, N'Student Management',     N'Total students',   N'Active',   N'With routines',  N'Without user',     N'Students list',      N'Student',  N'Status', N'No students found',                N'Create', N'Edit',      N'Delete',   N'Associate User',     N'Student detail',     N'Active',  N'Inactive',  N'Confirm Deletion',       N'Are you sure you want to delete this student?',             N'This action will also delete all associated routines.'),
(3, N'Gestão de Alunos',       N'Total alunos',     N'Ativos',   N'Com rotinas',    N'Sem usuário',      N'Lista de alunos',    N'Aluno',    N'Estado', N'Nenhum aluno encontrado',          N'Criar',  N'Editar',    N'Excluir',  N'Associar Usuário',   N'Detalhe do aluno',   N'Ativo',   N'Inativo',   N'Confirmar Exclusão',     N'Tem certeza que deseja excluir este aluno?',                N'Esta ação também excluirá todas as rotinas associadas.'),
(4, N'Gestion des Élèves',     N'Total élèves',     N'Actifs',   N'Avec routines',  N'Sans utilisateur', N'Liste des élèves',   N'Élève',    N'Statut', N'Aucun élève trouvé',               N'Créer',  N'Modifier',  N'Supprimer',N'Associer Utilisateur',N'Détail de l''élève', N'Actif',   N'Inactif',   N'Confirmer la Suppression',N'Êtes-vous sûr de vouloir supprimer cet élève ?',           N'Cette action supprimera également toutes les routines associées.'),
(5, N'生徒管理',               N'総生徒数',         N'アクティブ', N'ルーティンあり', N'ユーザーなし',    N'生徒一覧',           N'生徒',     N'状態',   N'生徒が見つかりませんでした',       N'作成',   N'編集',      N'削除',     N'ユーザーを関連付ける',N'生徒詳細',           N'アクティブ',N'非アクティブ',N'削除の確認',            N'この生徒を削除してもよろしいですか？',                      N'この操作により、関連するすべてのルーティンも削除されます。');
GO


-- ============================================================
-- 11. Traducciones.Pantalla_Actividades  (calendario de actividades)
-- ============================================================
IF OBJECT_ID('Traducciones.Pantalla_Actividades', 'U') IS NOT NULL
    DROP TABLE [Traducciones].[Pantalla_Actividades];
GO

CREATE TABLE [Traducciones].[Pantalla_Actividades] (
    [TraduccionID]                  INT            IDENTITY(1,1) NOT NULL,
    [IdiomaID]                      INT                          NOT NULL,
    [actividades_titulo]            NVARCHAR(500)                NOT NULL,
    [actividades_btn_nueva]         NVARCHAR(500)                NOT NULL,
    [actividades_cliente_info]      NVARCHAR(500)                NOT NULL,
    [actividades_modal_titulo]      NVARCHAR(500)                NOT NULL,
    [actividades_campo_nombre]      NVARCHAR(500)                NOT NULL,
    [actividades_campo_instructor]  NVARCHAR(500)                NOT NULL,
    [actividades_campo_horario]     NVARCHAR(500)                NOT NULL,
    [actividades_campo_duracion]    NVARCHAR(500)                NOT NULL,
    [actividades_campo_color]       NVARCHAR(500)                NOT NULL,
    [actividades_color_rosa]        NVARCHAR(500)                NOT NULL,
    [actividades_color_menta]       NVARCHAR(500)                NOT NULL,
    [actividades_color_lavanda]     NVARCHAR(500)                NOT NULL,
    [actividades_color_durazno]     NVARCHAR(500)                NOT NULL,
    [actividades_color_celeste]     NVARCHAR(500)                NOT NULL,
    [actividades_btn_cancelar]      NVARCHAR(500)                NOT NULL,
    [actividades_btn_crear_act]     NVARCHAR(500)                NOT NULL,
    [actividades_ver_detalles]      NVARCHAR(500)                NOT NULL,
    [actividades_mas]               NVARCHAR(500)                NOT NULL,
    [actividades_dia_dom]           NVARCHAR(50)                 NOT NULL,
    [actividades_dia_lun]           NVARCHAR(50)                 NOT NULL,
    [actividades_dia_mar]           NVARCHAR(50)                 NOT NULL,
    [actividades_dia_mie]           NVARCHAR(50)                 NOT NULL,
    [actividades_dia_jue]           NVARCHAR(50)                 NOT NULL,
    [actividades_dia_vie]           NVARCHAR(50)                 NOT NULL,
    [actividades_dia_sab]           NVARCHAR(50)                 NOT NULL,
    [actividades_meses_json]        NVARCHAR(500)                NOT NULL,
    [actividades_dia_titulo_fmt]    NVARCHAR(500)                NOT NULL,
    CONSTRAINT PK_Pantalla_Actividades        PRIMARY KEY ([TraduccionID]),
    CONSTRAINT FK_Pantalla_Actividades_Idioma FOREIGN KEY ([IdiomaID]) REFERENCES [Traducciones].[Idiomas]([IdiomaID]),
    CONSTRAINT UQ_Pantalla_Actividades_Idioma UNIQUE      ([IdiomaID])
);
GO

INSERT INTO [Traducciones].[Pantalla_Actividades]
    ([IdiomaID], [actividades_titulo], [actividades_btn_nueva], [actividades_cliente_info],
     [actividades_modal_titulo], [actividades_campo_nombre], [actividades_campo_instructor],
     [actividades_campo_horario], [actividades_campo_duracion], [actividades_campo_color],
     [actividades_color_rosa], [actividades_color_menta], [actividades_color_lavanda],
     [actividades_color_durazno], [actividades_color_celeste],
     [actividades_btn_cancelar], [actividades_btn_crear_act],
     [actividades_ver_detalles], [actividades_mas],
     [actividades_dia_dom], [actividades_dia_lun], [actividades_dia_mar], [actividades_dia_mie],
     [actividades_dia_jue], [actividades_dia_vie], [actividades_dia_sab],
     [actividades_meses_json], [actividades_dia_titulo_fmt])
VALUES
(1, N'Calendario de Actividades', N'Nueva Actividad',  N'Se muestran las actividades asociadas a tus alumnos. Si no ves clases, contactá a recepción.',
    N'Nueva Actividad', N'Nombre de la actividad', N'Instructor', N'Horario', N'Duración (min)', N'Color',
    N'Rosa', N'Menta', N'Lavanda', N'Durazno', N'Celeste',
    N'Cancelar', N'Crear Actividad', N'Ver detalles', N'más',
    N'Dom', N'Lun', N'Mar', N'Mié', N'Jue', N'Vie', N'Sáb',
    N'["Enero","Febrero","Marzo","Abril","Mayo","Junio","Julio","Agosto","Septiembre","Octubre","Noviembre","Diciembre"]',
    N'Actividades del {0} de {1}'),
(2, N'Activities Calendar',        N'New Activity',     N'Activities associated with your students are shown. If you don''t see classes, contact reception.',
    N'New Activity',    N'Activity name',           N'Instructor', N'Schedule', N'Duration (min)', N'Color',
    N'Pink', N'Mint', N'Lavender', N'Peach', N'Sky',
    N'Cancel', N'Create Activity', N'View details', N'more',
    N'Sun', N'Mon', N'Tue', N'Wed', N'Thu', N'Fri', N'Sat',
    N'["January","February","March","April","May","June","July","August","September","October","November","December"]',
    N'Activities for {0} {1}'),
(3, N'Calendário de Atividades',   N'Nova Atividade',   N'São exibidas as atividades associadas aos seus alunos. Se não vir aulas, contate a recepção.',
    N'Nova Atividade',  N'Nome da atividade',        N'Instrutor',  N'Horário',  N'Duração (min)',  N'Cor',
    N'Rosa', N'Menta', N'Lavanda', N'Pêssego', N'Azul Céu',
    N'Cancelar', N'Criar Atividade', N'Ver detalhes', N'mais',
    N'Dom', N'Seg', N'Ter', N'Qua', N'Qui', N'Sex', N'Sáb',
    N'["Janeiro","Fevereiro","Março","Abril","Maio","Junho","Julho","Agosto","Setembro","Outubro","Novembro","Dezembro"]',
    N'Atividades do dia {0} de {1}'),
(4, N'Calendrier des Activités',   N'Nouvelle Activité',N'Les activités associées à vos élèves sont affichées. Si vous ne voyez pas de cours, contactez la réception.',
    N'Nouvelle Activité',N'Nom de l''activité',     N'Instructeur',N'Horaire',  N'Durée (min)',    N'Couleur',
    N'Rose', N'Menthe', N'Lavande', N'Pêche', N'Bleu ciel',
    N'Annuler', N'Créer une Activité', N'Voir les détails', N'de plus',
    N'Dim', N'Lun', N'Mar', N'Mer', N'Jeu', N'Ven', N'Sam',
    N'["Janvier","Février","Mars","Avril","Mai","Juin","Juillet","Août","Septembre","Octobre","Novembre","Décembre"]',
    N'Activités du {0} {1}'),
(5, N'アクティビティカレンダー',    N'新しいアクティビティ', N'生徒に関連するアクティビティが表示されます。クラスが表示されない場合は、受付にお問い合わせください。',
    N'新しいアクティビティ', N'アクティビティ名',   N'インストラクター', N'スケジュール', N'時間（分）', N'色',
    N'ピンク', N'ミント', N'ラベンダー', N'ピーチ', N'スカイ',
    N'キャンセル', N'アクティビティを作成', N'詳細を見る', N'つ以上',
    N'日', N'月', N'火', N'水', N'木', N'金', N'土',
    N'["1月","2月","3月","4月","5月","6月","7月","8月","9月","10月","11月","12月"]',
    N'{1}{0}日のアクティビティ');
GO


-- ============================================================
-- 12. Traducciones.Pantalla_Rutinas  (gestión de rutinas)
-- ============================================================
IF OBJECT_ID('Traducciones.Pantalla_Rutinas', 'U') IS NOT NULL
    DROP TABLE [Traducciones].[Pantalla_Rutinas];
GO

CREATE TABLE [Traducciones].[Pantalla_Rutinas] (
    [TraduccionID]          INT           IDENTITY(1,1) NOT NULL,
    [IdiomaID]              INT                         NOT NULL,
    [rutinas_titulo]        NVARCHAR(500)               NOT NULL,
    [rutinas_subtitulo]     NVARCHAR(500)               NOT NULL,
    [rutinas_cliente_msg]   NVARCHAR(500)               NOT NULL,
    [rutinas_admin_msg]     NVARCHAR(500)               NOT NULL,
    CONSTRAINT PK_Pantalla_Rutinas        PRIMARY KEY ([TraduccionID]),
    CONSTRAINT FK_Pantalla_Rutinas_Idioma FOREIGN KEY ([IdiomaID]) REFERENCES [Traducciones].[Idiomas]([IdiomaID]),
    CONSTRAINT UQ_Pantalla_Rutinas_Idioma UNIQUE      ([IdiomaID])
);
GO

INSERT INTO [Traducciones].[Pantalla_Rutinas]
    ([IdiomaID], [rutinas_titulo], [rutinas_subtitulo], [rutinas_cliente_msg], [rutinas_admin_msg])
VALUES
(1, N'Rutinas',   N'Gestión de rutinas de entrenamiento',         N'Aquí se mostrarán las rutinas asociadas a tus alumnos.',   N'Módulo de rutinas en desarrollo.'),
(2, N'Routines',  N'Training routine management',                  N'Your students'' routines will be shown here.',             N'Routines module under development.'),
(3, N'Rotinas',   N'Gestão de rotinas de treino',                  N'As rotinas dos seus alunos serão exibidas aqui.',         N'Módulo de rotinas em desenvolvimento.'),
(4, N'Routines',  N'Gestion des routines d''entraînement',         N'Les routines associées à vos élèves seront affichées ici.',N'Module de routines en cours de développement.'),
(5, N'ルーティン', N'トレーニングルーティン管理',                   N'生徒のルーティンがここに表示されます。',                    N'ルーティンモジュールは開発中です。');
GO
