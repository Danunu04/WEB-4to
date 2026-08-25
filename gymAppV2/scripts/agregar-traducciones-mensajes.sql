USE [GymApp]
GO

-- ============================================================
-- Agregar tags comunes a Comunes_Mensajes
-- ============================================================
ALTER TABLE [Traducciones].[Comunes_Mensajes]
    ADD [msg_mostrando_fmt] NVARCHAR(500) NOT NULL DEFAULT N'',
        [msg_dni_invalido]  NVARCHAR(500) NOT NULL DEFAULT N'';
GO
UPDATE [Traducciones].[Comunes_Mensajes] SET
    [msg_mostrando_fmt] = CASE [IdiomaID]
        WHEN 1 THEN N'Mostrando {0} de {1}'
        WHEN 2 THEN N'Showing {0} of {1}'
        WHEN 3 THEN N'Mostrando {0} de {1}'
        WHEN 4 THEN N'Affichage {0} sur {1}'
        WHEN 5 THEN N'{0}/{1}件を表示'
    END,
    [msg_dni_invalido] = CASE [IdiomaID]
        WHEN 1 THEN N'El DNI debe ser un número válido'
        WHEN 2 THEN N'ID must be a valid number'
        WHEN 3 THEN N'O CPF deve ser um número válido'
        WHEN 4 THEN N'L''identifiant doit être un nombre valide'
        WHEN 5 THEN N'IDは有効な数値でなければなりません'
    END;
GO


-- ============================================================
-- Mensajes de acción de Pantalla_Alumnos
-- ============================================================
ALTER TABLE [Traducciones].[Pantalla_Alumnos]
    ADD [alumnos_msg_actualizado]      NVARCHAR(500) NOT NULL DEFAULT N'',
        [alumnos_msg_sel_requerido]    NVARCHAR(500) NOT NULL DEFAULT N'',
        [alumnos_msg_sel_eliminar]     NVARCHAR(500) NOT NULL DEFAULT N'',
        [alumnos_msg_sel_asociar]      NVARCHAR(500) NOT NULL DEFAULT N'',
        [alumnos_msg_no_existe]        NVARCHAR(500) NOT NULL DEFAULT N'',
        [alumnos_msg_creado]           NVARCHAR(500) NOT NULL DEFAULT N'',
        [alumnos_msg_modificado]       NVARCHAR(500) NOT NULL DEFAULT N'',
        [alumnos_msg_eliminado]        NVARCHAR(500) NOT NULL DEFAULT N'',
        [alumnos_msg_ya_existe]        NVARCHAR(500) NOT NULL DEFAULT N'',
        [alumnos_msg_ya_asociado]      NVARCHAR(500) NOT NULL DEFAULT N'',
        [alumnos_msg_dni_obligatorio]  NVARCHAR(500) NOT NULL DEFAULT N'',
        [alumnos_msg_dni_invalido]     NVARCHAR(500) NOT NULL DEFAULT N'',
        [alumnos_msg_nombre_oblig]     NVARCHAR(500) NOT NULL DEFAULT N'',
        [alumnos_msg_apellido_oblig]   NVARCHAR(500) NOT NULL DEFAULT N'',
        [alumnos_msg_fecha_oblig]      NVARCHAR(500) NOT NULL DEFAULT N'',
        [alumnos_msg_fecha_invalida]   NVARCHAR(500) NOT NULL DEFAULT N'',
        [alumnos_msg_fecha_futura]     NVARCHAR(500) NOT NULL DEFAULT N'',
        [alumnos_msg_peso_invalido]    NVARCHAR(500) NOT NULL DEFAULT N'';
GO
UPDATE [Traducciones].[Pantalla_Alumnos] SET
    [alumnos_msg_actualizado]   = CASE [IdiomaID] WHEN 1 THEN N'Lista actualizada'                        WHEN 2 THEN N'List updated'                           WHEN 3 THEN N'Lista atualizada'                       WHEN 4 THEN N'Liste mise à jour'                       WHEN 5 THEN N'リスト更新済み'              END,
    [alumnos_msg_sel_requerido] = CASE [IdiomaID] WHEN 1 THEN N'Seleccione un alumno de la lista'         WHEN 2 THEN N'Select a student from the list'          WHEN 3 THEN N'Selecione um aluno da lista'            WHEN 4 THEN N'Sélectionnez un élève dans la liste'    WHEN 5 THEN N'リストから生徒を選択してください'   END,
    [alumnos_msg_sel_eliminar]  = CASE [IdiomaID] WHEN 1 THEN N'Seleccione un alumno para eliminar'       WHEN 2 THEN N'Select a student to delete'              WHEN 3 THEN N'Selecione um aluno para excluir'        WHEN 4 THEN N'Sélectionnez un élève à supprimer'      WHEN 5 THEN N'削除する生徒を選択してください'    END,
    [alumnos_msg_sel_asociar]   = CASE [IdiomaID] WHEN 1 THEN N'Seleccione un alumno para asociar usuario' WHEN 2 THEN N'Select a student to associate a user'  WHEN 3 THEN N'Selecione um aluno para associar usuário' WHEN 4 THEN N'Sélectionnez un élève pour associer un utilisateur' WHEN 5 THEN N'ユーザーを関連付ける生徒を選択してください' END,
    [alumnos_msg_no_existe]     = CASE [IdiomaID] WHEN 1 THEN N'El alumno no existe'                       WHEN 2 THEN N'Student not found'                      WHEN 3 THEN N'Aluno não encontrado'                   WHEN 4 THEN N'Élève introuvable'                       WHEN 5 THEN N'生徒が見つかりません'            END,
    [alumnos_msg_creado]        = CASE [IdiomaID] WHEN 1 THEN N'Alumno creado correctamente'               WHEN 2 THEN N'Student created successfully'            WHEN 3 THEN N'Aluno criado com sucesso'               WHEN 4 THEN N'Élève créé avec succès'                  WHEN 5 THEN N'生徒が正常に作成されました'       END,
    [alumnos_msg_modificado]    = CASE [IdiomaID] WHEN 1 THEN N'Alumno modificado correctamente'           WHEN 2 THEN N'Student updated successfully'            WHEN 3 THEN N'Aluno atualizado com sucesso'           WHEN 4 THEN N'Élève mis à jour avec succès'            WHEN 5 THEN N'生徒が正常に更新されました'       END,
    [alumnos_msg_eliminado]     = CASE [IdiomaID] WHEN 1 THEN N'Alumno eliminado correctamente'            WHEN 2 THEN N'Student deleted successfully'            WHEN 3 THEN N'Aluno excluído com sucesso'             WHEN 4 THEN N'Élève supprimé avec succès'              WHEN 5 THEN N'生徒が正常に削除されました'       END,
    [alumnos_msg_ya_existe]     = CASE [IdiomaID] WHEN 1 THEN N'Ya existe un alumno con ese DNI'           WHEN 2 THEN N'A student with that ID already exists'  WHEN 3 THEN N'Já existe um aluno com esse CPF'        WHEN 4 THEN N'Un élève avec cet ID existe déjà'        WHEN 5 THEN N'そのIDの生徒はすでに存在します'  END,
    [alumnos_msg_ya_asociado]   = CASE [IdiomaID] WHEN 1 THEN N'El alumno ya tiene un usuario asociado'   WHEN 2 THEN N'Student already has an associated user' WHEN 3 THEN N'O aluno já tem um usuário associado'    WHEN 4 THEN N'L''élève a déjà un utilisateur associé' WHEN 5 THEN N'生徒にはすでにユーザーが関連付けられています' END,
    [alumnos_msg_dni_obligatorio]= CASE [IdiomaID] WHEN 1 THEN N'El DNI es obligatorio'                   WHEN 2 THEN N'ID is required'                         WHEN 3 THEN N'O CPF é obrigatório'                   WHEN 4 THEN N'L''identifiant est obligatoire'          WHEN 5 THEN N'IDは必須です'                    END,
    [alumnos_msg_dni_invalido]  = CASE [IdiomaID] WHEN 1 THEN N'El DNI debe ser un número válido'          WHEN 2 THEN N'ID must be a valid number'               WHEN 3 THEN N'O CPF deve ser um número válido'        WHEN 4 THEN N'L''identifiant doit être un nombre valide' WHEN 5 THEN N'IDは有効な数値でなければなりません' END,
    [alumnos_msg_nombre_oblig]  = CASE [IdiomaID] WHEN 1 THEN N'El nombre es obligatorio'                  WHEN 2 THEN N'Name is required'                       WHEN 3 THEN N'O nome é obrigatório'                   WHEN 4 THEN N'Le prénom est obligatoire'               WHEN 5 THEN N'名前は必須です'                  END,
    [alumnos_msg_apellido_oblig]= CASE [IdiomaID] WHEN 1 THEN N'El apellido es obligatorio'                WHEN 2 THEN N'Last name is required'                  WHEN 3 THEN N'O sobrenome é obrigatório'              WHEN 4 THEN N'Le nom de famille est obligatoire'       WHEN 5 THEN N'苗字は必須です'                  END,
    [alumnos_msg_fecha_oblig]   = CASE [IdiomaID] WHEN 1 THEN N'La fecha de nacimiento es obligatoria'     WHEN 2 THEN N'Date of birth is required'               WHEN 3 THEN N'A data de nascimento é obrigatória'    WHEN 4 THEN N'La date de naissance est obligatoire'   WHEN 5 THEN N'生年月日は必須です'              END,
    [alumnos_msg_fecha_invalida]= CASE [IdiomaID] WHEN 1 THEN N'La fecha de nacimiento no es válida'       WHEN 2 THEN N'Invalid date of birth'                  WHEN 3 THEN N'A data de nascimento é inválida'        WHEN 4 THEN N'La date de naissance est invalide'       WHEN 5 THEN N'生年月日が無効です'              END,
    [alumnos_msg_fecha_futura]  = CASE [IdiomaID] WHEN 1 THEN N'La fecha de nacimiento no puede ser futura' WHEN 2 THEN N'Date of birth cannot be in the future' WHEN 3 THEN N'A data de nascimento não pode ser futura' WHEN 4 THEN N'La date de naissance ne peut pas être future' WHEN 5 THEN N'生年月日は未来の日付にできません' END,
    [alumnos_msg_peso_invalido] = CASE [IdiomaID] WHEN 1 THEN N'El peso debe estar entre 0 y 500 kg'       WHEN 2 THEN N'Weight must be between 0 and 500 kg'    WHEN 3 THEN N'O peso deve estar entre 0 e 500 kg'    WHEN 4 THEN N'Le poids doit être entre 0 et 500 kg'   WHEN 5 THEN N'体重は0〜500kgの間でなければなりません' END;
GO
ALTER TABLE [Traducciones].[Pantalla_Alumnos]
    ADD [alumnos_form_nuevo]   NVARCHAR(500) NOT NULL DEFAULT N'',
        [alumnos_form_modificar] NVARCHAR(500) NOT NULL DEFAULT N'',
        [alumnos_sin_asociar]  NVARCHAR(500) NOT NULL DEFAULT N'';
GO
UPDATE [Traducciones].[Pantalla_Alumnos] SET
    [alumnos_form_nuevo]     = CASE [IdiomaID] WHEN 1 THEN N'Nuevo alumno'       WHEN 2 THEN N'New student'    WHEN 3 THEN N'Novo aluno'      WHEN 4 THEN N'Nouvel élève'            WHEN 5 THEN N'新規生徒'         END,
    [alumnos_form_modificar] = CASE [IdiomaID] WHEN 1 THEN N'Modificar alumno'   WHEN 2 THEN N'Edit student'   WHEN 3 THEN N'Editar aluno'    WHEN 4 THEN N'Modifier l''élève'       WHEN 5 THEN N'生徒を編集'       END,
    [alumnos_sin_asociar]    = CASE [IdiomaID] WHEN 1 THEN N'-- Sin asociar --'  WHEN 2 THEN N'-- None --'     WHEN 3 THEN N'-- Sem associar --' WHEN 4 THEN N'-- Sans associer --'  WHEN 5 THEN N'-- 未関連付け --' END;
GO


-- ============================================================
-- Mensajes de acción de Pantalla_Usuarios
-- ============================================================
ALTER TABLE [Traducciones].[Pantalla_Usuarios]
    ADD [usuarios_msg_sel_requerido]   NVARCHAR(500) NOT NULL DEFAULT N'',
        [usuarios_msg_no_existe]       NVARCHAR(500) NOT NULL DEFAULT N'',
        [usuarios_msg_desbloqueado]    NVARCHAR(500) NOT NULL DEFAULT N'',
        [usuarios_msg_sel_desbloquear] NVARCHAR(500) NOT NULL DEFAULT N'',
        [usuarios_msg_blanqueado]      NVARCHAR(500) NOT NULL DEFAULT N'',
        [usuarios_msg_sel_blanquear]   NVARCHAR(500) NOT NULL DEFAULT N'',
        [usuarios_msg_sel_activar]     NVARCHAR(500) NOT NULL DEFAULT N'',
        [usuarios_msg_sel_desactivar]  NVARCHAR(500) NOT NULL DEFAULT N'',
        [usuarios_msg_activado]        NVARCHAR(500) NOT NULL DEFAULT N'',
        [usuarios_msg_desactivado]     NVARCHAR(500) NOT NULL DEFAULT N'',
        [usuarios_msg_creado]          NVARCHAR(500) NOT NULL DEFAULT N'',
        [usuarios_msg_modificado]      NVARCHAR(500) NOT NULL DEFAULT N'',
        [usuarios_msg_rol_invalido]    NVARCHAR(500) NOT NULL DEFAULT N'',
        [usuarios_form_nuevo]          NVARCHAR(500) NOT NULL DEFAULT N'',
        [usuarios_form_modificar]      NVARCHAR(500) NOT NULL DEFAULT N'';
GO
UPDATE [Traducciones].[Pantalla_Usuarios] SET
    [usuarios_msg_sel_requerido]   = CASE [IdiomaID] WHEN 1 THEN N'Seleccione un usuario de la lista'          WHEN 2 THEN N'Select a user from the list'              WHEN 3 THEN N'Selecione um usuário da lista'              WHEN 4 THEN N'Sélectionnez un utilisateur dans la liste'    WHEN 5 THEN N'リストからユーザーを選択してください'  END,
    [usuarios_msg_no_existe]       = CASE [IdiomaID] WHEN 1 THEN N'El usuario no existe'                       WHEN 2 THEN N'User not found'                           WHEN 3 THEN N'Usuário não encontrado'                     WHEN 4 THEN N'Utilisateur introuvable'                      WHEN 5 THEN N'ユーザーが見つかりません'          END,
    [usuarios_msg_desbloqueado]    = CASE [IdiomaID] WHEN 1 THEN N'Usuario desbloqueado correctamente'         WHEN 2 THEN N'User unlocked successfully'                WHEN 3 THEN N'Usuário desbloqueado com sucesso'            WHEN 4 THEN N'Utilisateur débloqué avec succès'             WHEN 5 THEN N'ユーザーのロックが解除されました' END,
    [usuarios_msg_sel_desbloquear] = CASE [IdiomaID] WHEN 1 THEN N'Seleccione un usuario para desbloquear'     WHEN 2 THEN N'Select a user to unlock'                  WHEN 3 THEN N'Selecione um usuário para desbloquear'       WHEN 4 THEN N'Sélectionnez un utilisateur à débloquer'      WHEN 5 THEN N'ロック解除するユーザーを選択してください' END,
    [usuarios_msg_blanqueado]      = CASE [IdiomaID] WHEN 1 THEN N'Contraseña blanqueada. El usuario deberá cambiarla en su próximo inicio de sesión.' WHEN 2 THEN N'Password reset. User must change it on next login.' WHEN 3 THEN N'Senha redefinida. O usuário deverá alterá-la no próximo login.' WHEN 4 THEN N'Mot de passe réinitialisé. L''utilisateur devra le changer à la prochaine connexion.' WHEN 5 THEN N'パスワードがリセットされました。次回ログイン時に変更が必要です。' END,
    [usuarios_msg_sel_blanquear]   = CASE [IdiomaID] WHEN 1 THEN N'Seleccione un usuario para blanquear la contraseña' WHEN 2 THEN N'Select a user to reset the password' WHEN 3 THEN N'Selecione um usuário para redefinir a senha' WHEN 4 THEN N'Sélectionnez un utilisateur pour réinitialiser le mot de passe' WHEN 5 THEN N'パスワードをリセットするユーザーを選択してください' END,
    [usuarios_msg_sel_activar]     = CASE [IdiomaID] WHEN 1 THEN N'Seleccione un usuario para activar'         WHEN 2 THEN N'Select a user to activate'                WHEN 3 THEN N'Selecione um usuário para ativar'            WHEN 4 THEN N'Sélectionnez un utilisateur à activer'        WHEN 5 THEN N'有効化するユーザーを選択してください' END,
    [usuarios_msg_sel_desactivar]  = CASE [IdiomaID] WHEN 1 THEN N'Seleccione un usuario para desactivar'      WHEN 2 THEN N'Select a user to deactivate'              WHEN 3 THEN N'Selecione um usuário para desativar'         WHEN 4 THEN N'Sélectionnez un utilisateur à désactiver'     WHEN 5 THEN N'無効化するユーザーを選択してください' END,
    [usuarios_msg_activado]        = CASE [IdiomaID] WHEN 1 THEN N'Usuario activado correctamente'             WHEN 2 THEN N'User activated successfully'               WHEN 3 THEN N'Usuário ativado com sucesso'                 WHEN 4 THEN N'Utilisateur activé avec succès'               WHEN 5 THEN N'ユーザーが有効化されました'         END,
    [usuarios_msg_desactivado]     = CASE [IdiomaID] WHEN 1 THEN N'Usuario desactivado correctamente'          WHEN 2 THEN N'User deactivated successfully'             WHEN 3 THEN N'Usuário desativado com sucesso'              WHEN 4 THEN N'Utilisateur désactivé avec succès'            WHEN 5 THEN N'ユーザーが無効化されました'         END,
    [usuarios_msg_creado]          = CASE [IdiomaID] WHEN 1 THEN N'Usuario creado correctamente'               WHEN 2 THEN N'User created successfully'                WHEN 3 THEN N'Usuário criado com sucesso'                  WHEN 4 THEN N'Utilisateur créé avec succès'                 WHEN 5 THEN N'ユーザーが正常に作成されました'    END,
    [usuarios_msg_modificado]      = CASE [IdiomaID] WHEN 1 THEN N'Usuario modificado correctamente'           WHEN 2 THEN N'User updated successfully'                WHEN 3 THEN N'Usuário atualizado com sucesso'              WHEN 4 THEN N'Utilisateur mis à jour avec succès'           WHEN 5 THEN N'ユーザーが正常に更新されました'    END,
    [usuarios_msg_rol_invalido]    = CASE [IdiomaID] WHEN 1 THEN N'Seleccione un rol válido'                   WHEN 2 THEN N'Select a valid role'                      WHEN 3 THEN N'Selecione um perfil válido'                  WHEN 4 THEN N'Sélectionnez un rôle valide'                  WHEN 5 THEN N'有効な役割を選択してください'      END,
    [usuarios_form_nuevo]          = CASE [IdiomaID] WHEN 1 THEN N'Nuevo usuario'                              WHEN 2 THEN N'New user'                                WHEN 3 THEN N'Novo usuário'                               WHEN 4 THEN N'Nouvel utilisateur'                          WHEN 5 THEN N'新規ユーザー'                      END,
    [usuarios_form_modificar]      = CASE [IdiomaID] WHEN 1 THEN N'Modificar usuario'                          WHEN 2 THEN N'Edit user'                               WHEN 3 THEN N'Editar usuário'                             WHEN 4 THEN N'Modifier l''utilisateur'                      WHEN 5 THEN N'ユーザーを編集'                    END;
GO
