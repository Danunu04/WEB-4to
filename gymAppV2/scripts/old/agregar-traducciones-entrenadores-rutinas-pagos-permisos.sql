USE [GymApp]
GO

-- ============================================================
-- Traducciones para los módulos completados: Entrenadores (CRUD nuevo),
-- Rutinas (CRUD ampliado), Pagos y Permisos (placeholders).
-- Sigue el mismo formato que scripts/migrar-esquema-traducciones.sql:
-- una tabla por pantalla, una columna por tag, una fila por IdiomaID (1-5).
-- ============================================================

-- ============================================================
-- 1. Traducciones.Pantalla_Entrenadores  (gestión de entrenadores)
-- ============================================================
IF OBJECT_ID('Traducciones.Pantalla_Entrenadores', 'U') IS NOT NULL
    DROP TABLE [Traducciones].[Pantalla_Entrenadores];
GO

CREATE TABLE [Traducciones].[Pantalla_Entrenadores] (
    [TraduccionID]                      INT           IDENTITY(1,1) NOT NULL,
    [IdiomaID]                          INT                         NOT NULL,
    [entrenadores_titulo]               NVARCHAR(500)               NOT NULL,
    [entrenadores_stat_total]           NVARCHAR(500)               NOT NULL,
    [entrenadores_stat_activos]         NVARCHAR(500)               NOT NULL,
    [entrenadores_stat_con_alumnos]     NVARCHAR(500)               NOT NULL,
    [entrenadores_stat_sin_usuario]     NVARCHAR(500)               NOT NULL,
    [entrenadores_lista_titulo]         NVARCHAR(500)               NOT NULL,
    [entrenadores_col_entrenador]       NVARCHAR(500)               NOT NULL,
    [entrenadores_col_estado]           NVARCHAR(500)               NOT NULL,
    [entrenadores_btn_crear]            NVARCHAR(500)               NOT NULL,
    [entrenadores_btn_modificar]        NVARCHAR(500)               NOT NULL,
    [entrenadores_btn_eliminar]         NVARCHAR(500)               NOT NULL,
    [entrenadores_form_nuevo]           NVARCHAR(500)               NOT NULL,
    [entrenadores_form_modificar]       NVARCHAR(500)               NOT NULL,
    [entrenadores_estado_activo]        NVARCHAR(500)               NOT NULL,
    [entrenadores_estado_inactivo]      NVARCHAR(500)               NOT NULL,
    [entrenadores_confirmar_elim_titulo] NVARCHAR(500)              NOT NULL,
    [entrenadores_confirmar_elim_msg]    NVARCHAR(500)              NOT NULL,
    [entrenadores_confirmar_elim_aviso]  NVARCHAR(500)              NOT NULL,
    CONSTRAINT PK_Pantalla_Entrenadores        PRIMARY KEY ([TraduccionID]),
    CONSTRAINT FK_Pantalla_Entrenadores_Idioma FOREIGN KEY ([IdiomaID]) REFERENCES [Traducciones].[Idiomas]([IdiomaID]),
    CONSTRAINT UQ_Pantalla_Entrenadores_Idioma UNIQUE      ([IdiomaID])
);
GO

INSERT INTO [Traducciones].[Pantalla_Entrenadores]
    ([IdiomaID], [entrenadores_titulo], [entrenadores_stat_total], [entrenadores_stat_activos],
     [entrenadores_stat_con_alumnos], [entrenadores_stat_sin_usuario], [entrenadores_lista_titulo],
     [entrenadores_col_entrenador], [entrenadores_col_estado],
     [entrenadores_btn_crear], [entrenadores_btn_modificar], [entrenadores_btn_eliminar],
     [entrenadores_form_nuevo], [entrenadores_form_modificar],
     [entrenadores_estado_activo], [entrenadores_estado_inactivo],
     [entrenadores_confirmar_elim_titulo], [entrenadores_confirmar_elim_msg], [entrenadores_confirmar_elim_aviso])
VALUES
(1, N'Gestión de Entrenadores', N'Total entrenadores', N'Activos', N'Con alumnos', N'Sin usuario', N'Lista de entrenadores', N'Entrenador', N'Estado', N'Crear', N'Modificar', N'Eliminar', N'Nuevo entrenador', N'Modificar entrenador', N'Activo', N'Inactivo', N'Confirmar Eliminación', N'¿Está seguro que desea eliminar este entrenador?', N'Esta acción eliminará también sus rutinas y actividades asociadas.'),
(2, N'Trainer Management', N'Total trainers', N'Active', N'With students', N'Without user', N'Trainers list', N'Trainer', N'Status', N'Create', N'Edit', N'Delete', N'New trainer', N'Edit trainer', N'Active', N'Inactive', N'Confirm Deletion', N'Are you sure you want to delete this trainer?', N'This action will also delete their associated routines and activities.'),
(3, N'Gestão de Treinadores', N'Total treinadores', N'Ativos', N'Com alunos', N'Sem usuário', N'Lista de treinadores', N'Treinador', N'Estado', N'Criar', N'Editar', N'Excluir', N'Novo treinador', N'Editar treinador', N'Ativo', N'Inativo', N'Confirmar Exclusão', N'Tem certeza que deseja excluir este treinador?', N'Esta ação também excluirá suas rotinas e atividades associadas.'),
(4, N'Gestion des Entraîneurs', N'Total entraîneurs', N'Actifs', N'Avec élèves', N'Sans utilisateur', N'Liste des entraîneurs', N'Entraîneur', N'Statut', N'Créer', N'Modifier', N'Supprimer', N'Nouvel entraîneur', N'Modifier l''entraîneur', N'Actif', N'Inactif', N'Confirmer la Suppression', N'Êtes-vous sûr de vouloir supprimer cet entraîneur ?', N'Cette action supprimera également ses routines et activités associées.'),
(5, N'トレーナー管理', N'総トレーナー数', N'アクティブ', N'生徒あり', N'ユーザーなし', N'トレーナー一覧', N'トレーナー', N'状態', N'作成', N'編集', N'削除', N'新規トレーナー', N'トレーナー編集', N'アクティブ', N'非アクティブ', N'削除の確認', N'このトレーナーを削除してもよろしいですか？', N'この操作により、関連するルーティンとアクティビティも削除されます。');
GO


-- ============================================================
-- 2. Traducciones.Pantalla_Rutinas  (ampliación: CRUD completo)
-- La tabla y sus 4 tags originales (rutinas_titulo, rutinas_subtitulo,
-- rutinas_cliente_msg, rutinas_admin_msg) ya existen. Solo se agregan
-- los tags nuevos que necesita el CRUD.
-- ============================================================
IF COL_LENGTH('Traducciones.Pantalla_Rutinas', 'rutinas_lista_titulo') IS NULL
    ALTER TABLE [Traducciones].[Pantalla_Rutinas]
        ADD [rutinas_lista_titulo]          NVARCHAR(500) NOT NULL DEFAULT N'',
            [rutinas_lista_cliente_titulo]   NVARCHAR(500) NOT NULL DEFAULT N'',
            [rutinas_btn_crear]              NVARCHAR(500) NOT NULL DEFAULT N'',
            [rutinas_btn_modificar]          NVARCHAR(500) NOT NULL DEFAULT N'',
            [rutinas_btn_eliminar]           NVARCHAR(500) NOT NULL DEFAULT N'',
            [rutinas_form_nuevo]             NVARCHAR(500) NOT NULL DEFAULT N'',
            [rutinas_form_modificar]         NVARCHAR(500) NOT NULL DEFAULT N'',
            [rutinas_confirmar_elim_titulo]  NVARCHAR(500) NOT NULL DEFAULT N'',
            [rutinas_confirmar_elim_msg]     NVARCHAR(500) NOT NULL DEFAULT N'';
GO

UPDATE [Traducciones].[Pantalla_Rutinas] SET
    [rutinas_lista_titulo]         = CASE [IdiomaID] WHEN 1 THEN N'Lista de rutinas'          WHEN 2 THEN N'Routines list'                WHEN 3 THEN N'Lista de rotinas'          WHEN 4 THEN N'Liste des routines'            WHEN 5 THEN N'ルーティン一覧'      END,
    [rutinas_lista_cliente_titulo] = CASE [IdiomaID] WHEN 1 THEN N'Mis rutinas'               WHEN 2 THEN N'My routines'                  WHEN 3 THEN N'Minhas rotinas'            WHEN 4 THEN N'Mes routines'                  WHEN 5 THEN N'マイルーティン'      END,
    [rutinas_btn_crear]            = CASE [IdiomaID] WHEN 1 THEN N'Crear'                      WHEN 2 THEN N'Create'                       WHEN 3 THEN N'Criar'                     WHEN 4 THEN N'Créer'                         WHEN 5 THEN N'作成'                END,
    [rutinas_btn_modificar]        = CASE [IdiomaID] WHEN 1 THEN N'Modificar'                  WHEN 2 THEN N'Edit'                         WHEN 3 THEN N'Editar'                    WHEN 4 THEN N'Modifier'                      WHEN 5 THEN N'編集'                END,
    [rutinas_btn_eliminar]         = CASE [IdiomaID] WHEN 1 THEN N'Eliminar'                   WHEN 2 THEN N'Delete'                       WHEN 3 THEN N'Excluir'                   WHEN 4 THEN N'Supprimer'                     WHEN 5 THEN N'削除'                END,
    [rutinas_form_nuevo]           = CASE [IdiomaID] WHEN 1 THEN N'Nueva rutina'                WHEN 2 THEN N'New routine'                  WHEN 3 THEN N'Nova rotina'               WHEN 4 THEN N'Nouvelle routine'              WHEN 5 THEN N'新規ルーティン'      END,
    [rutinas_form_modificar]       = CASE [IdiomaID] WHEN 1 THEN N'Modificar rutina'            WHEN 2 THEN N'Edit routine'                 WHEN 3 THEN N'Editar rotina'             WHEN 4 THEN N'Modifier la routine'           WHEN 5 THEN N'ルーティン編集'      END,
    [rutinas_confirmar_elim_titulo]= CASE [IdiomaID] WHEN 1 THEN N'Confirmar Eliminación'       WHEN 2 THEN N'Confirm Deletion'             WHEN 3 THEN N'Confirmar Exclusão'        WHEN 4 THEN N'Confirmer la Suppression'      WHEN 5 THEN N'削除の確認'          END,
    [rutinas_confirmar_elim_msg]   = CASE [IdiomaID] WHEN 1 THEN N'¿Está seguro que desea eliminar esta rutina?' WHEN 2 THEN N'Are you sure you want to delete this routine?' WHEN 3 THEN N'Tem certeza que deseja excluir esta rotina?' WHEN 4 THEN N'Êtes-vous sûr de vouloir supprimer cette routine ?' WHEN 5 THEN N'このルーティンを削除してもよろしいですか？' END;
GO


-- ============================================================
-- 3. Traducciones.Pantalla_Pagos  (placeholder)
-- ============================================================
IF OBJECT_ID('Traducciones.Pantalla_Pagos', 'U') IS NOT NULL
    DROP TABLE [Traducciones].[Pantalla_Pagos];
GO

CREATE TABLE [Traducciones].[Pantalla_Pagos] (
    [TraduccionID]                  INT           IDENTITY(1,1) NOT NULL,
    [IdiomaID]                      INT                         NOT NULL,
    [pagos_titulo]                  NVARCHAR(500)               NOT NULL,
    [pagos_subtitulo]               NVARCHAR(500)               NOT NULL,
    [pagos_en_desarrollo_titulo]    NVARCHAR(500)               NOT NULL,
    [pagos_en_desarrollo_msg]       NVARCHAR(500)               NOT NULL,
    CONSTRAINT PK_Pantalla_Pagos        PRIMARY KEY ([TraduccionID]),
    CONSTRAINT FK_Pantalla_Pagos_Idioma FOREIGN KEY ([IdiomaID]) REFERENCES [Traducciones].[Idiomas]([IdiomaID]),
    CONSTRAINT UQ_Pantalla_Pagos_Idioma UNIQUE      ([IdiomaID])
);
GO

INSERT INTO [Traducciones].[Pantalla_Pagos]
    ([IdiomaID], [pagos_titulo], [pagos_subtitulo], [pagos_en_desarrollo_titulo], [pagos_en_desarrollo_msg])
VALUES
(1, N'Pagos',    N'Gestión de pagos y cuotas',            N'Módulo en desarrollo', N'Muy pronto vas a poder registrar y consultar pagos de cuotas desde acá.'),
(2, N'Payments', N'Payment and fee management',           N'Module under development', N'Soon you will be able to register and check fee payments here.'),
(3, N'Pagamentos', N'Gestão de pagamentos e mensalidades', N'Módulo em desenvolvimento', N'Em breve você poderá registrar e consultar pagamentos de mensalidades aqui.'),
(4, N'Paiements', N'Gestion des paiements et cotisations', N'Module en cours de développement', N'Vous pourrez bientôt enregistrer et consulter les paiements de cotisations ici.'),
(5, N'支払い', N'支払いと会費の管理',                     N'開発中のモジュール',     N'近日中に、こちらから会費の支払いを登録・確認できるようになります。');
GO


-- ============================================================
-- 4. Traducciones.Pantalla_Permisos  (placeholder)
-- ============================================================
IF OBJECT_ID('Traducciones.Pantalla_Permisos', 'U') IS NOT NULL
    DROP TABLE [Traducciones].[Pantalla_Permisos];
GO

CREATE TABLE [Traducciones].[Pantalla_Permisos] (
    [TraduccionID]                     INT           IDENTITY(1,1) NOT NULL,
    [IdiomaID]                         INT                         NOT NULL,
    [permisos_titulo]                  NVARCHAR(500)               NOT NULL,
    [permisos_subtitulo]               NVARCHAR(500)               NOT NULL,
    [permisos_en_desarrollo_titulo]    NVARCHAR(500)               NOT NULL,
    [permisos_en_desarrollo_msg]       NVARCHAR(500)               NOT NULL,
    CONSTRAINT PK_Pantalla_Permisos        PRIMARY KEY ([TraduccionID]),
    CONSTRAINT FK_Pantalla_Permisos_Idioma FOREIGN KEY ([IdiomaID]) REFERENCES [Traducciones].[Idiomas]([IdiomaID]),
    CONSTRAINT UQ_Pantalla_Permisos_Idioma UNIQUE      ([IdiomaID])
);
GO

INSERT INTO [Traducciones].[Pantalla_Permisos]
    ([IdiomaID], [permisos_titulo], [permisos_subtitulo], [permisos_en_desarrollo_titulo], [permisos_en_desarrollo_msg])
VALUES
(1, N'Permisos', N'Gestión de roles y permisos del sistema', N'Módulo en desarrollo', N'Muy pronto vas a poder administrar los permisos de cada perfil desde acá.'),
(2, N'Permissions', N'System roles and permissions management', N'Module under development', N'Soon you will be able to manage each profile''s permissions here.'),
(3, N'Permissões', N'Gestão de perfis e permissões do sistema', N'Módulo em desenvolvimento', N'Em breve você poderá gerenciar as permissões de cada perfil aqui.'),
(4, N'Permissions', N'Gestion des rôles et permissions du système', N'Module en cours de développement', N'Vous pourrez bientôt gérer les permissions de chaque profil ici.'),
(5, N'権限', N'システムの役割と権限の管理',                N'開発中のモジュール',     N'近日中に、こちらから各プロファイルの権限を管理できるようになります。');
GO
