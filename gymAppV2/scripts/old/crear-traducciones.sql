USE [GymApp]
GO

-- ============================================================
-- Tabla: dbo.Traducciones
-- Descripcion: Almacena los textos del sistema por idioma.
--              Cada fila es un tag unico que apunta a un
--              texto traducido en cada columna de idioma.
-- ============================================================

IF OBJECT_ID('dbo.Traducciones', 'U') IS NOT NULL
    DROP TABLE dbo.Traducciones;
GO

CREATE TABLE dbo.Traducciones (
    TraduccionID    INT            IDENTITY(1,1)  NOT NULL,
    TraduccionTAG   NVARCHAR(100)                 NOT NULL,
    Español         NVARCHAR(500)                 NOT NULL,
    Ingles          NVARCHAR(500)                 NOT NULL,
    Portugues       NVARCHAR(500)                 NOT NULL,
    Frances         NVARCHAR(500)                 NOT NULL,
    Japones         NVARCHAR(500)                 NOT NULL,

    CONSTRAINT PK_Traducciones       PRIMARY KEY (TraduccionID),
    CONSTRAINT UQ_Traducciones_TAG   UNIQUE      (TraduccionTAG)
);
GO

-- ============================================================
-- Datos iniciales
-- ============================================================

INSERT INTO dbo.Traducciones (TraduccionTAG, Español, Ingles, Portugues, Frances, Japones) VALUES

-- Sidebar - menu principal
('menu_dashboard',          'Dashboard',                'Dashboard',                    'Dashboard',                    'Tableau de bord',              'ダッシュボード'),
('menu_usuarios',           'Usuarios',                 'Users',                        'Usuários',                     'Utilisateurs',                 'ユーザー'),
('menu_alumnos',            'Alumnos',                  'Students',                     'Alunos',                       'Élèves',                       '生徒'),
('menu_entrenadores',       'Entrenadores',             'Trainers',                     'Treinadores',                  'Entraîneurs',                  'トレーナー'),
('menu_actividades',        'Actividades',              'Activities',                   'Atividades',                   'Activités',                    'アクティビティ'),
('menu_rutinas',            'Rutinas',                  'Routines',                     'Rotinas',                      'Routines',                     'ルーティン'),
('menu_permisos',           'Permisos',                 'Permissions',                  'Permissões',                   'Permissions',                  '権限'),
('menu_bitacora',           'Bitácora',                 'Audit Log',                    'Bitácora',                     'Journal',                      'ログ'),
('menu_respaldo',           'Gestión de respaldo',      'Backup Management',            'Gestão de backup',             'Gestion de sauvegarde',        'バックアップ管理'),
('menu_pagos',              'Pagos',                    'Payments',                     'Pagamentos',                   'Paiements',                    '支払い'),
('menu_perfil',             'Perfil',                   'Profile',                      'Perfil',                       'Profil',                       'プロフィール'),
('menu_idioma',             'Idioma',                   'Language',                     'Idioma',                       'Langue',                       '言語'),
('menu_cambiar_contra',     'Cambiar contraseña',       'Change password',              'Alterar senha',                'Changer le mot de passe',      'パスワード変更'),
('menu_cerrar_sesion',      'Cerrar sesión',            'Log out',                      'Sair',                         'Déconnexion',                  'ログアウト'),

-- Pagina Idioma
('idioma_titulo',           'Seleccionar idioma',       'Select language',              'Selecionar idioma',            'Sélectionner la langue',       '言語を選択'),
('idioma_subtitulo',        'Elegí el idioma en que querés usar la aplicación.', 'Choose the language you want to use the app in.', 'Escolha o idioma em que deseja usar o aplicativo.', 'Choisissez la langue dans laquelle vous souhaitez utiliser l''application.', 'アプリで使用する言語を選んでください。'),
('idioma_guardado',         'Idioma actualizado correctamente.', 'Language updated successfully.', 'Idioma atualizado com sucesso.', 'Langue mise à jour avec succès.', '言語が正常に更新されました。'),
('idioma_activo',           'Activo',                   'Active',                       'Ativo',                        'Actif',                        '有効'),

-- Botones comunes
('btn_guardar',             'Guardar',                  'Save',                         'Salvar',                       'Enregistrer',                  '保存'),
('btn_cancelar',            'Cancelar',                 'Cancel',                       'Cancelar',                     'Annuler',                      'キャンセル'),
('btn_eliminar',            'Eliminar',                 'Delete',                       'Excluir',                      'Supprimer',                    '削除'),
('btn_editar',              'Editar',                   'Edit',                         'Editar',                       'Modifier',                     '編集'),
('btn_agregar',             'Agregar',                  'Add',                          'Adicionar',                    'Ajouter',                      '追加'),
('btn_buscar',              'Buscar',                   'Search',                       'Buscar',                       'Rechercher',                   '検索'),
('btn_volver',              'Volver',                   'Back',                         'Voltar',                       'Retour',                       '戻る'),
('btn_confirmar',           'Confirmar',                'Confirm',                      'Confirmar',                    'Confirmer',                    '確認'),

-- Toasts / mensajes del sistema
('toast_exito',             '¡Éxito!',                  'Success!',                     'Sucesso!',                     'Succès !',                     '成功！'),
('toast_error',             'Error',                    'Error',                        'Erro',                         'Erreur',                       'エラー'),
('toast_advertencia',       'Advertencia',              'Warning',                      'Aviso',                        'Avertissement',                '警告'),
('toast_informacion',       'Información',              'Information',                  'Informação',                   'Information',                  '情報'),

-- Mensajes generales
('msg_sin_resultados',      'No se encontraron resultados.', 'No results found.',       'Nenhum resultado encontrado.', 'Aucun résultat trouvé.',       '結果が見つかりませんでした。'),
('msg_cargando',            'Cargando...',              'Loading...',                   'Carregando...',                'Chargement...',                '読み込み中...'),
('msg_error_generico',      'Ocurrió un error inesperado. Por favor, intente nuevamente.', 'An unexpected error occurred. Please try again.', 'Ocorreu um erro inesperado. Por favor, tente novamente.', 'Une erreur inattendue s''est produite. Veuillez réessayer.', '予期しないエラーが発生しました。もう一度お試しください。'),
('msg_acceso_denegado',     'No tenés permiso para acceder a esta sección.', 'You do not have permission to access this section.', 'Você não tem permissão para acessar esta seção.', 'Vous n''avez pas la permission d''accéder à cette section.', 'このセクションへのアクセス権がありません。'),

-- Login
('login_titulo',            'Iniciar sesión',           'Log in',                       'Entrar',                       'Se connecter',                 'ログイン'),
('login_usuario',           'Usuario',                  'Username',                     'Usuário',                      'Nom d''utilisateur',           'ユーザー名'),
('login_contrasena',        'Contraseña',               'Password',                     'Senha',                        'Mot de passe',                 'パスワード'),
('login_btn',               'Ingresar',                 'Sign in',                      'Entrar',                       'Connexion',                    'ログイン'),
('login_error_credenciales','Usuario o contraseña incorrectos.', 'Incorrect username or password.', 'Usuário ou senha incorretos.', 'Nom d''utilisateur ou mot de passe incorrect.', 'ユーザー名またはパスワードが間違っています。');

GO
