USE [GymApp]
GO

-- ============================================================
-- Agregar tags de la sección de filtros a Pantalla_Usuarios
-- Ejecutar con ALTER TABLE para no perder los datos existentes.
-- ============================================================
ALTER TABLE [Traducciones].[Pantalla_Usuarios]
    ADD [usuarios_label_filtros]       NVARCHAR(500) NOT NULL DEFAULT N'',
        [usuarios_label_estado]        NVARCHAR(500) NOT NULL DEFAULT N'',
        [usuarios_label_bloqueados]    NVARCHAR(500) NOT NULL DEFAULT N'',
        [usuarios_label_rol]           NVARCHAR(500) NOT NULL DEFAULT N'',
        [usuarios_label_buscar]        NVARCHAR(500) NOT NULL DEFAULT N'',
        [usuarios_filtro_todos]        NVARCHAR(500) NOT NULL DEFAULT N'',
        [usuarios_filtro_activados]    NVARCHAR(500) NOT NULL DEFAULT N'',
        [usuarios_filtro_desactivados] NVARCHAR(500) NOT NULL DEFAULT N'',
        [usuarios_filtro_bloqueados]   NVARCHAR(500) NOT NULL DEFAULT N'',
        [usuarios_filtro_no_bloqueados] NVARCHAR(500) NOT NULL DEFAULT N'',
        [usuarios_filtro_todos_roles]  NVARCHAR(500) NOT NULL DEFAULT N'',
        [usuarios_buscar_placeholder]  NVARCHAR(500) NOT NULL DEFAULT N'',
        [usuarios_btn_filtrar]         NVARCHAR(500) NOT NULL DEFAULT N'',
        [usuarios_btn_exportar]        NVARCHAR(500) NOT NULL DEFAULT N'',
        [usuarios_btn_actualizar]      NVARCHAR(500) NOT NULL DEFAULT N'';
GO

UPDATE [Traducciones].[Pantalla_Usuarios] SET
    [usuarios_label_filtros]       = CASE [IdiomaID] WHEN 1 THEN N'Filtros'           WHEN 2 THEN N'Filters'                     WHEN 3 THEN N'Filtros'            WHEN 4 THEN N'Filtres'             WHEN 5 THEN N'フィルター'          END,
    [usuarios_label_estado]        = CASE [IdiomaID] WHEN 1 THEN N'Estado'            WHEN 2 THEN N'Status'                      WHEN 3 THEN N'Estado'             WHEN 4 THEN N'Statut'              WHEN 5 THEN N'状態'                END,
    [usuarios_label_bloqueados]    = CASE [IdiomaID] WHEN 1 THEN N'Bloqueados'        WHEN 2 THEN N'Locked'                      WHEN 3 THEN N'Bloqueados'         WHEN 4 THEN N'Bloqués'             WHEN 5 THEN N'ロック済み'          END,
    [usuarios_label_rol]           = CASE [IdiomaID] WHEN 1 THEN N'Rol'               WHEN 2 THEN N'Role'                        WHEN 3 THEN N'Perfil'             WHEN 4 THEN N'Rôle'                WHEN 5 THEN N'役割'                END,
    [usuarios_label_buscar]        = CASE [IdiomaID] WHEN 1 THEN N'Buscar'            WHEN 2 THEN N'Search'                      WHEN 3 THEN N'Buscar'             WHEN 4 THEN N'Rechercher'          WHEN 5 THEN N'検索'                END,
    [usuarios_filtro_todos]        = CASE [IdiomaID] WHEN 1 THEN N'Todos'             WHEN 2 THEN N'All'                         WHEN 3 THEN N'Todos'              WHEN 4 THEN N'Tous'                WHEN 5 THEN N'すべて'              END,
    [usuarios_filtro_activados]    = CASE [IdiomaID] WHEN 1 THEN N'Activados'         WHEN 2 THEN N'Active'                      WHEN 3 THEN N'Ativos'             WHEN 4 THEN N'Actifs'              WHEN 5 THEN N'アクティブ'          END,
    [usuarios_filtro_desactivados] = CASE [IdiomaID] WHEN 1 THEN N'Desactivados'      WHEN 2 THEN N'Inactive'                    WHEN 3 THEN N'Inativos'           WHEN 4 THEN N'Inactifs'            WHEN 5 THEN N'非アクティブ'        END,
    [usuarios_filtro_bloqueados]   = CASE [IdiomaID] WHEN 1 THEN N'Bloqueados'        WHEN 2 THEN N'Locked'                      WHEN 3 THEN N'Bloqueados'         WHEN 4 THEN N'Bloqués'             WHEN 5 THEN N'ロック済み'          END,
    [usuarios_filtro_no_bloqueados]= CASE [IdiomaID] WHEN 1 THEN N'No bloqueados'     WHEN 2 THEN N'Not locked'                  WHEN 3 THEN N'Não bloqueados'     WHEN 4 THEN N'Non bloqués'         WHEN 5 THEN N'ロック解除'          END,
    [usuarios_filtro_todos_roles]  = CASE [IdiomaID] WHEN 1 THEN N'Todos los roles'   WHEN 2 THEN N'All roles'                   WHEN 3 THEN N'Todos os perfis'    WHEN 4 THEN N'Tous les rôles'      WHEN 5 THEN N'すべての役割'        END,
    [usuarios_buscar_placeholder]  = CASE [IdiomaID] WHEN 1 THEN N'Nombre, apellido o usuario...' WHEN 2 THEN N'Name, surname or username...' WHEN 3 THEN N'Nome, sobrenome ou usuário...' WHEN 4 THEN N'Nom, prénom ou utilisateur...' WHEN 5 THEN N'名前、苗字またはユーザー名...' END,
    [usuarios_btn_filtrar]         = CASE [IdiomaID] WHEN 1 THEN N'Filtrar'           WHEN 2 THEN N'Filter'                      WHEN 3 THEN N'Filtrar'            WHEN 4 THEN N'Filtrer'             WHEN 5 THEN N'フィルター'          END,
    [usuarios_btn_exportar]        = CASE [IdiomaID] WHEN 1 THEN N'Exportar'          WHEN 2 THEN N'Export'                      WHEN 3 THEN N'Exportar'           WHEN 4 THEN N'Exporter'            WHEN 5 THEN N'エクスポート'        END,
    [usuarios_btn_actualizar]      = CASE [IdiomaID] WHEN 1 THEN N'Actualizar'        WHEN 2 THEN N'Refresh'                     WHEN 3 THEN N'Atualizar'          WHEN 4 THEN N'Actualiser'          WHEN 5 THEN N'更新'                END;
GO


-- ============================================================
-- Agregar tags de la sección de filtros a Pantalla_Alumnos
-- ============================================================
ALTER TABLE [Traducciones].[Pantalla_Alumnos]
    ADD [alumnos_label_filtros]     NVARCHAR(500) NOT NULL DEFAULT N'',
        [alumnos_label_estado]      NVARCHAR(500) NOT NULL DEFAULT N'',
        [alumnos_label_usuario]     NVARCHAR(500) NOT NULL DEFAULT N'',
        [alumnos_label_buscar]      NVARCHAR(500) NOT NULL DEFAULT N'',
        [alumnos_filtro_todos]      NVARCHAR(500) NOT NULL DEFAULT N'',
        [alumnos_filtro_activos]    NVARCHAR(500) NOT NULL DEFAULT N'',
        [alumnos_filtro_inactivos]   NVARCHAR(500) NOT NULL DEFAULT N'',
        [alumnos_filtro_con_usuario] NVARCHAR(500) NOT NULL DEFAULT N'',
        [alumnos_filtro_sin_usuario] NVARCHAR(500) NOT NULL DEFAULT N'',
        [alumnos_buscar_placeholder] NVARCHAR(500) NOT NULL DEFAULT N'',
        [alumnos_btn_filtrar]        NVARCHAR(500) NOT NULL DEFAULT N'';
GO

UPDATE [Traducciones].[Pantalla_Alumnos] SET
    [alumnos_label_filtros]      = CASE [IdiomaID] WHEN 1 THEN N'Filtros'             WHEN 2 THEN N'Filters'          WHEN 3 THEN N'Filtros'            WHEN 4 THEN N'Filtres'             WHEN 5 THEN N'フィルター'      END,
    [alumnos_label_estado]       = CASE [IdiomaID] WHEN 1 THEN N'Estado'              WHEN 2 THEN N'Status'           WHEN 3 THEN N'Estado'             WHEN 4 THEN N'Statut'              WHEN 5 THEN N'状態'            END,
    [alumnos_label_usuario]      = CASE [IdiomaID] WHEN 1 THEN N'Usuario asociado'    WHEN 2 THEN N'Associated user'  WHEN 3 THEN N'Usuário associado'  WHEN 4 THEN N'Utilisateur associé' WHEN 5 THEN N'関連ユーザー'    END,
    [alumnos_label_buscar]       = CASE [IdiomaID] WHEN 1 THEN N'Buscar'              WHEN 2 THEN N'Search'           WHEN 3 THEN N'Buscar'             WHEN 4 THEN N'Rechercher'          WHEN 5 THEN N'検索'            END,
    [alumnos_filtro_todos]       = CASE [IdiomaID] WHEN 1 THEN N'Todos'               WHEN 2 THEN N'All'              WHEN 3 THEN N'Todos'              WHEN 4 THEN N'Tous'                WHEN 5 THEN N'すべて'          END,
    [alumnos_filtro_activos]     = CASE [IdiomaID] WHEN 1 THEN N'Activos'             WHEN 2 THEN N'Active'           WHEN 3 THEN N'Ativos'             WHEN 4 THEN N'Actifs'              WHEN 5 THEN N'アクティブ'      END,
    [alumnos_filtro_inactivos]   = CASE [IdiomaID] WHEN 1 THEN N'Inactivos'           WHEN 2 THEN N'Inactive'         WHEN 3 THEN N'Inativos'           WHEN 4 THEN N'Inactifs'            WHEN 5 THEN N'非アクティブ'    END,
    [alumnos_filtro_con_usuario] = CASE [IdiomaID] WHEN 1 THEN N'Con usuario'         WHEN 2 THEN N'With user'        WHEN 3 THEN N'Com usuário'        WHEN 4 THEN N'Avec utilisateur'    WHEN 5 THEN N'ユーザーあり'    END,
    [alumnos_filtro_sin_usuario] = CASE [IdiomaID] WHEN 1 THEN N'Sin usuario'         WHEN 2 THEN N'Without user'     WHEN 3 THEN N'Sem usuário'        WHEN 4 THEN N'Sans utilisateur'    WHEN 5 THEN N'ユーザーなし'    END,
    [alumnos_buscar_placeholder] = CASE [IdiomaID] WHEN 1 THEN N'Nombre, apellido o DNI...' WHEN 2 THEN N'Name, surname or ID...' WHEN 3 THEN N'Nome, sobrenome ou CPF...' WHEN 4 THEN N'Nom, prénom ou numéro...' WHEN 5 THEN N'名前、苗字またはID...' END,
    [alumnos_btn_filtrar]        = CASE [IdiomaID] WHEN 1 THEN N'Filtrar'             WHEN 2 THEN N'Filter'           WHEN 3 THEN N'Filtrar'            WHEN 4 THEN N'Filtrer'             WHEN 5 THEN N'フィルター'      END;
GO
