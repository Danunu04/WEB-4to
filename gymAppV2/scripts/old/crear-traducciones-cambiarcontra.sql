USE [GymApp]
GO

IF OBJECT_ID('Traducciones.Pantalla_CambiarContra', 'U') IS NOT NULL
    DROP TABLE [Traducciones].[Pantalla_CambiarContra];
GO

CREATE TABLE [Traducciones].[Pantalla_CambiarContra] (
    [TraduccionID]               INT           IDENTITY(1,1) NOT NULL,
    [IdiomaID]                   INT                         NOT NULL,
    -- Encabezado
    [cambio_titulo]              NVARCHAR(500)               NOT NULL,
    [cambio_subtitulo]           NVARCHAR(500)               NOT NULL,
    -- Labels de campos
    [cambio_label_usuario]       NVARCHAR(500)               NOT NULL,
    [cambio_label_contra_actual] NVARCHAR(500)               NOT NULL,
    [cambio_label_nueva_contra]  NVARCHAR(500)               NOT NULL,
    [cambio_label_confirmar]     NVARCHAR(500)               NOT NULL,
    [cambio_hint_contra]         NVARCHAR(500)               NOT NULL,
    -- Botones y links
    [cambio_btn_guardar]         NVARCHAR(500)               NOT NULL,
    [cambio_btn_cancelar]        NVARCHAR(500)               NOT NULL,
    [cambio_link_volver]         NVARCHAR(500)               NOT NULL,
    -- Mensajes de validadores (ErrorMessage de los asp:Validator)
    [cambio_val_contra_actual]   NVARCHAR(500)               NOT NULL,
    [cambio_val_nueva_contra]    NVARCHAR(500)               NOT NULL,
    [cambio_val_contra_regex]    NVARCHAR(500)               NOT NULL,
    [cambio_val_confirmar]       NVARCHAR(500)               NOT NULL,
    [cambio_val_no_coinciden]    NVARCHAR(500)               NOT NULL,
    -- Mensajes de resultado (code-behind)
    [cambio_msg_no_coinciden]    NVARCHAR(500)               NOT NULL,
    [cambio_msg_contra_incorrecta] NVARCHAR(500)             NOT NULL,
    [cambio_msg_reutilizar]      NVARCHAR(500)               NOT NULL,
    [cambio_msg_ok]              NVARCHAR(500)               NOT NULL,
    [cambio_msg_ok_recuperacion] NVARCHAR(500)               NOT NULL,
    [cambio_msg_ok_primer_login] NVARCHAR(500)               NOT NULL,
    CONSTRAINT PK_Pantalla_CambiarContra        PRIMARY KEY ([TraduccionID]),
    CONSTRAINT FK_Pantalla_CambiarContra_Idioma FOREIGN KEY ([IdiomaID]) REFERENCES [Traducciones].[Idiomas]([IdiomaID]),
    CONSTRAINT UQ_Pantalla_CambiarContra_Idioma UNIQUE      ([IdiomaID])
);
GO

INSERT INTO [Traducciones].[Pantalla_CambiarContra]
    ([IdiomaID], [cambio_titulo], [cambio_subtitulo],
     [cambio_label_usuario], [cambio_label_contra_actual], [cambio_label_nueva_contra], [cambio_label_confirmar], [cambio_hint_contra],
     [cambio_btn_guardar], [cambio_btn_cancelar], [cambio_link_volver],
     [cambio_val_contra_actual], [cambio_val_nueva_contra], [cambio_val_contra_regex], [cambio_val_confirmar], [cambio_val_no_coinciden],
     [cambio_msg_no_coinciden], [cambio_msg_contra_incorrecta], [cambio_msg_reutilizar],
     [cambio_msg_ok], [cambio_msg_ok_recuperacion], [cambio_msg_ok_primer_login])
VALUES
-- ES
(1,
 N'Cambiar Contraseña',
 N'Complete los campos para actualizar su contraseña.',
 N'Usuario', N'Contraseña actual', N'Nueva contraseña', N'Confirmar nueva contraseña',
 N'Mínimo 6 caracteres, una mayúscula y un carácter especial.',
 N'Guardar', N'Cancelar', N'Volver al inicio de sesión',
 N'Ingrese su contraseña actual.', N'Ingrese la nueva contraseña.',
 N'Mínimo 6 caracteres, una mayúscula y un carácter especial.',
 N'Confirme la nueva contraseña.', N'Las contraseñas no coinciden.',
 N'Las contraseñas nuevas no coinciden.', N'La contraseña actual es incorrecta.',
 N'No puedes reutilizar una contraseña anterior.',
 N'Contraseña actualizada correctamente.',
 N'Contraseña actualizada. Inicie sesión con su nueva contraseña.',
 N'Contraseña actualizada. Ahora configure sus preguntas de seguridad.'),
-- EN
(2,
 N'Change Password',
 N'Fill in the fields to update your password.',
 N'Username', N'Current password', N'New password', N'Confirm new password',
 N'At least 6 characters, one uppercase letter and one special character.',
 N'Save', N'Cancel', N'Back to login',
 N'Enter your current password.', N'Enter the new password.',
 N'At least 6 characters, one uppercase letter and one special character.',
 N'Confirm the new password.', N'Passwords do not match.',
 N'New passwords do not match.', N'Current password is incorrect.',
 N'You cannot reuse a previous password.',
 N'Password updated successfully.',
 N'Password updated. Please log in with your new password.',
 N'Password updated. Now configure your security questions.'),
-- PT
(3,
 N'Alterar Senha',
 N'Preencha os campos para atualizar sua senha.',
 N'Usuário', N'Senha atual', N'Nova senha', N'Confirmar nova senha',
 N'Mínimo 6 caracteres, uma letra maiúscula e um caractere especial.',
 N'Salvar', N'Cancelar', N'Voltar ao login',
 N'Digite sua senha atual.', N'Digite a nova senha.',
 N'Mínimo 6 caracteres, uma letra maiúscula e um caractere especial.',
 N'Confirme a nova senha.', N'As senhas não coincidem.',
 N'As novas senhas não coincidem.', N'A senha atual está incorreta.',
 N'Você não pode reutilizar uma senha anterior.',
 N'Senha atualizada com sucesso.',
 N'Senha atualizada. Faça login com sua nova senha.',
 N'Senha atualizada. Agora configure suas perguntas de segurança.'),
-- FR
(4,
 N'Changer le mot de passe',
 N'Remplissez les champs pour mettre à jour votre mot de passe.',
 N'Utilisateur', N'Mot de passe actuel', N'Nouveau mot de passe', N'Confirmer le nouveau mot de passe',
 N'Au moins 6 caractères, une majuscule et un caractère spécial.',
 N'Enregistrer', N'Annuler', N'Retour à la connexion',
 N'Saisissez votre mot de passe actuel.', N'Saisissez le nouveau mot de passe.',
 N'Au moins 6 caractères, une majuscule et un caractère spécial.',
 N'Confirmez le nouveau mot de passe.', N'Les mots de passe ne correspondent pas.',
 N'Les nouveaux mots de passe ne correspondent pas.', N'Le mot de passe actuel est incorrect.',
 N'Vous ne pouvez pas réutiliser un mot de passe précédent.',
 N'Mot de passe mis à jour avec succès.',
 N'Mot de passe mis à jour. Connectez-vous avec votre nouveau mot de passe.',
 N'Mot de passe mis à jour. Configurez maintenant vos questions de sécurité.'),
-- JA
(5,
 N'パスワード変更',
 N'パスワードを更新するために各フィールドを入力してください。',
 N'ユーザー名', N'現在のパスワード', N'新しいパスワード', N'新しいパスワードの確認',
 N'6文字以上、大文字1文字、特殊文字1文字が必要です。',
 N'保存', N'キャンセル', N'ログインに戻る',
 N'現在のパスワードを入力してください。', N'新しいパスワードを入力してください。',
 N'6文字以上、大文字1文字、特殊文字1文字が必要です。',
 N'新しいパスワードを確認してください。', N'パスワードが一致しません。',
 N'新しいパスワードが一致しません。', N'現在のパスワードが正しくありません。',
 N'以前のパスワードは再使用できません。',
 N'パスワードが正常に更新されました。',
 N'パスワードが更新されました。新しいパスワードでログインしてください。',
 N'パスワードが更新されました。セキュリティの質問を設定してください。');
GO
