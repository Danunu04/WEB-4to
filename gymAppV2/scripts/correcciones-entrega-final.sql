USE [GymApp]
GO

-- ============================================================================
-- Correcciones de la entrega final (17/11/2026)
--
--   - Pago por transferencia con confirmación de administración -> tabla TransferenciaPendiente
--   - Alias para transferencias editable desde Pagos -> tabla ConfiguracionGimnasio
--   - Peso corporal en un rango razonable (20 a 300 kg): textos de error
--   - Traducciones de: transferencias, cobro del primer mes en el alta, credenciales de
--     usuarios y entrenadores nuevos, y mensajes de Cambiar contraseña.
--   - Al final, lista los alumnos con un peso fuera del rango nuevo (hay que corregirlos
--     desde Alumnos -> Modificar; si no, esas fichas no se pueden guardar).
--
-- Se puede correr más de una vez.
-- Antes de correrlo: hacer un backup.
-- Después de correrlo: VerificacioDV -> "Inicializar control" (si aparece
-- TransferenciaPendiente o ConfiguracionGimnasio sin control) y "Recalcular dígitos".
-- IdiomaID: 1 ES, 2 EN, 3 PT, 4 FR, 5 JA.
-- ============================================================================

-- 1. TransferenciaPendiente (transferencias informadas por los clientes)
IF OBJECT_ID('dbo.TransferenciaPendiente', 'U') IS NULL
BEGIN
    CREATE TABLE [dbo].[TransferenciaPendiente](
        [codTransferencia]  INT            IDENTITY(1,1) NOT NULL,
        [dni]               INT            NOT NULL,
        [modalidadId]       INT            NOT NULL,
        [monto]             DECIMAL(10,2)  NOT NULL,
        [fechaSolicitud]    DATETIME       NOT NULL,
        [usuarioSolicita]   VARCHAR(50)    NOT NULL,
        [estado]            VARCHAR(20)    NOT NULL CONSTRAINT [DF_TransferenciaPendiente_estado] DEFAULT ('Pendiente'),
        [usuarioResuelve]   VARCHAR(50)    NULL,
        [fechaResuelve]     DATETIME       NULL,
        [codPago]           INT            NULL,
        [dvh]               VARCHAR(64)    NOT NULL,
        CONSTRAINT [PK_TransferenciaPendiente] PRIMARY KEY CLUSTERED ([codTransferencia] ASC),
        CONSTRAINT [FK_TransferenciaPendiente_Alumno] FOREIGN KEY ([dni]) REFERENCES [dbo].[Alumnos] ([dni]),
        CONSTRAINT [FK_TransferenciaPendiente_Modalidad] FOREIGN KEY ([modalidadId]) REFERENCES [dbo].[PrecioModalidad] ([Id]),
        CONSTRAINT [CK_TransferenciaPendiente_Estado] CHECK ([estado] IN ('Pendiente', 'Aprobada', 'Rechazada'))
    );
    CREATE INDEX [IX_TransferenciaPendiente_Estado] ON [dbo].[TransferenciaPendiente] ([estado], [dni]);
    PRINT 'Tabla TransferenciaPendiente creada.';
END
GO

-- 1b. ConfiguracionGimnasio (valores que edita administración; por ahora, el alias para transferencias)
IF OBJECT_ID('dbo.ConfiguracionGimnasio', 'U') IS NULL
BEGIN
    CREATE TABLE [dbo].[ConfiguracionGimnasio](
        [clave]              VARCHAR(50)    NOT NULL,
        [valor]              NVARCHAR(200)  NOT NULL,
        [usuarioModifico]    VARCHAR(50)    NULL,
        [fechaModificacion]  DATETIME       NOT NULL,
        [dvh]                VARCHAR(64)    NOT NULL,
        CONSTRAINT [PK_ConfiguracionGimnasio] PRIMARY KEY CLUSTERED ([clave] ASC)
    );
    PRINT 'Tabla ConfiguracionGimnasio creada.';
END
GO

IF NOT EXISTS (SELECT 1 FROM [dbo].[ConfiguracionGimnasio] WHERE clave = 'AliasTransferencia')
    INSERT INTO [dbo].[ConfiguracionGimnasio] (clave, valor, usuarioModifico, fechaModificacion, dvh)
    VALUES ('AliasTransferencia', N'placeholder_alias', NULL, GETDATE(), '');
GO

-- 2. Traducciones: Pantalla_Pagos
IF COL_LENGTH('Traducciones.Pantalla_Pagos', 'pagos_alias_titulo') IS NULL
    ALTER TABLE [Traducciones].[Pantalla_Pagos] ADD [pagos_alias_titulo] NVARCHAR(500) NOT NULL DEFAULT N'';
IF COL_LENGTH('Traducciones.Pantalla_Pagos', 'pagos_alias_guardar') IS NULL
    ALTER TABLE [Traducciones].[Pantalla_Pagos] ADD [pagos_alias_guardar] NVARCHAR(500) NOT NULL DEFAULT N'';
IF COL_LENGTH('Traducciones.Pantalla_Pagos', 'pagos_alias_ayuda') IS NULL
    ALTER TABLE [Traducciones].[Pantalla_Pagos] ADD [pagos_alias_ayuda] NVARCHAR(500) NOT NULL DEFAULT N'';
IF COL_LENGTH('Traducciones.Pantalla_Pagos', 'pagos_alias_ok') IS NULL
    ALTER TABLE [Traducciones].[Pantalla_Pagos] ADD [pagos_alias_ok] NVARCHAR(500) NOT NULL DEFAULT N'';
IF COL_LENGTH('Traducciones.Pantalla_Pagos', 'pagos_err_alias') IS NULL
    ALTER TABLE [Traducciones].[Pantalla_Pagos] ADD [pagos_err_alias] NVARCHAR(500) NOT NULL DEFAULT N'';
IF COL_LENGTH('Traducciones.Pantalla_Pagos', 'pagos_err_alias_igual') IS NULL
    ALTER TABLE [Traducciones].[Pantalla_Pagos] ADD [pagos_err_alias_igual] NVARCHAR(500) NOT NULL DEFAULT N'';
IF COL_LENGTH('Traducciones.Pantalla_Pagos', 'pagos_err_alias_permiso') IS NULL
    ALTER TABLE [Traducciones].[Pantalla_Pagos] ADD [pagos_err_alias_permiso] NVARCHAR(500) NOT NULL DEFAULT N'';
IF COL_LENGTH('Traducciones.Pantalla_Pagos', 'pagos_transf_vacio') IS NULL
    ALTER TABLE [Traducciones].[Pantalla_Pagos] ADD [pagos_transf_vacio] NVARCHAR(500) NOT NULL DEFAULT N'';
IF COL_LENGTH('Traducciones.Pantalla_Pagos', 'pagos_transferencia_alias') IS NULL
    ALTER TABLE [Traducciones].[Pantalla_Pagos] ADD [pagos_transferencia_alias] NVARCHAR(500) NOT NULL DEFAULT N'';
IF COL_LENGTH('Traducciones.Pantalla_Pagos', 'pagos_transferencia_info') IS NULL
    ALTER TABLE [Traducciones].[Pantalla_Pagos] ADD [pagos_transferencia_info] NVARCHAR(500) NOT NULL DEFAULT N'';
IF COL_LENGTH('Traducciones.Pantalla_Pagos', 'pagos_btn_informar_transf') IS NULL
    ALTER TABLE [Traducciones].[Pantalla_Pagos] ADD [pagos_btn_informar_transf] NVARCHAR(500) NOT NULL DEFAULT N'';
IF COL_LENGTH('Traducciones.Pantalla_Pagos', 'pagos_transf_informada') IS NULL
    ALTER TABLE [Traducciones].[Pantalla_Pagos] ADD [pagos_transf_informada] NVARCHAR(500) NOT NULL DEFAULT N'';
IF COL_LENGTH('Traducciones.Pantalla_Pagos', 'pagos_aviso_transf_pendiente') IS NULL
    ALTER TABLE [Traducciones].[Pantalla_Pagos] ADD [pagos_aviso_transf_pendiente] NVARCHAR(500) NOT NULL DEFAULT N'';
IF COL_LENGTH('Traducciones.Pantalla_Pagos', 'pagos_estado_transf_pendiente') IS NULL
    ALTER TABLE [Traducciones].[Pantalla_Pagos] ADD [pagos_estado_transf_pendiente] NVARCHAR(500) NOT NULL DEFAULT N'';
IF COL_LENGTH('Traducciones.Pantalla_Pagos', 'pagos_transf_titulo') IS NULL
    ALTER TABLE [Traducciones].[Pantalla_Pagos] ADD [pagos_transf_titulo] NVARCHAR(500) NOT NULL DEFAULT N'';
IF COL_LENGTH('Traducciones.Pantalla_Pagos', 'pagos_transf_ayuda') IS NULL
    ALTER TABLE [Traducciones].[Pantalla_Pagos] ADD [pagos_transf_ayuda] NVARCHAR(500) NOT NULL DEFAULT N'';
IF COL_LENGTH('Traducciones.Pantalla_Pagos', 'pagos_transf_confirmar') IS NULL
    ALTER TABLE [Traducciones].[Pantalla_Pagos] ADD [pagos_transf_confirmar] NVARCHAR(500) NOT NULL DEFAULT N'';
IF COL_LENGTH('Traducciones.Pantalla_Pagos', 'pagos_transf_rechazar') IS NULL
    ALTER TABLE [Traducciones].[Pantalla_Pagos] ADD [pagos_transf_rechazar] NVARCHAR(500) NOT NULL DEFAULT N'';
IF COL_LENGTH('Traducciones.Pantalla_Pagos', 'pagos_transf_rechazar_titulo') IS NULL
    ALTER TABLE [Traducciones].[Pantalla_Pagos] ADD [pagos_transf_rechazar_titulo] NVARCHAR(500) NOT NULL DEFAULT N'';
IF COL_LENGTH('Traducciones.Pantalla_Pagos', 'pagos_transf_rechazar_si') IS NULL
    ALTER TABLE [Traducciones].[Pantalla_Pagos] ADD [pagos_transf_rechazar_si] NVARCHAR(500) NOT NULL DEFAULT N'';
IF COL_LENGTH('Traducciones.Pantalla_Pagos', 'pagos_transf_rechazar_msg') IS NULL
    ALTER TABLE [Traducciones].[Pantalla_Pagos] ADD [pagos_transf_rechazar_msg] NVARCHAR(500) NOT NULL DEFAULT N'';
IF COL_LENGTH('Traducciones.Pantalla_Pagos', 'pagos_transf_confirmada') IS NULL
    ALTER TABLE [Traducciones].[Pantalla_Pagos] ADD [pagos_transf_confirmada] NVARCHAR(500) NOT NULL DEFAULT N'';
IF COL_LENGTH('Traducciones.Pantalla_Pagos', 'pagos_transf_rechazada') IS NULL
    ALTER TABLE [Traducciones].[Pantalla_Pagos] ADD [pagos_transf_rechazada] NVARCHAR(500) NOT NULL DEFAULT N'';
IF COL_LENGTH('Traducciones.Pantalla_Pagos', 'pagos_transf_col_fecha') IS NULL
    ALTER TABLE [Traducciones].[Pantalla_Pagos] ADD [pagos_transf_col_fecha] NVARCHAR(500) NOT NULL DEFAULT N'';
IF COL_LENGTH('Traducciones.Pantalla_Pagos', 'pagos_transf_col_informo') IS NULL
    ALTER TABLE [Traducciones].[Pantalla_Pagos] ADD [pagos_transf_col_informo] NVARCHAR(500) NOT NULL DEFAULT N'';
IF COL_LENGTH('Traducciones.Pantalla_Pagos', 'pagos_err_transf_pendiente') IS NULL
    ALTER TABLE [Traducciones].[Pantalla_Pagos] ADD [pagos_err_transf_pendiente] NVARCHAR(500) NOT NULL DEFAULT N'';
IF COL_LENGTH('Traducciones.Pantalla_Pagos', 'pagos_err_transf_resuelta') IS NULL
    ALTER TABLE [Traducciones].[Pantalla_Pagos] ADD [pagos_err_transf_resuelta] NVARCHAR(500) NOT NULL DEFAULT N'';
GO

UPDATE [Traducciones].[Pantalla_Pagos] SET
    [pagos_alias_titulo] = CASE [IdiomaID] WHEN 1 THEN N'Alias que ven los clientes para transferir' WHEN 2 THEN N'Alias clients see to transfer' WHEN 3 THEN N'Alias que os clientes veem para transferir' WHEN 4 THEN N'Alias affiché aux clients pour le virement' WHEN 5 THEN N'お客様に表示する振込用エイリアス' ELSE N'Alias que ven los clientes para transferir' END,
    [pagos_alias_guardar] = CASE [IdiomaID] WHEN 1 THEN N'Guardar alias' WHEN 2 THEN N'Save alias' WHEN 3 THEN N'Salvar alias' WHEN 4 THEN N'Enregistrer lalias' WHEN 5 THEN N'エイリアスを保存' ELSE N'Guardar alias' END,
    [pagos_alias_ayuda] = CASE [IdiomaID] WHEN 1 THEN N'De 6 a 20 caracteres: letras sin tilde, números, puntos o guiones.' WHEN 2 THEN N'6 to 20 characters: letters without accents, numbers, dots or hyphens.' WHEN 3 THEN N'De 6 a 20 caracteres: letras sem acento, números, pontos ou hífens.' WHEN 4 THEN N'De 6 à 20 caractères : lettres sans accent, chiffres, points ou tirets.' WHEN 5 THEN N'6〜20文字：アクセントなしの英字、数字、ピリオド、ハイフン。' ELSE N'De 6 a 20 caracteres: letras sin tilde, números, puntos o guiones.' END,
    [pagos_alias_ok] = CASE [IdiomaID] WHEN 1 THEN N'Alias actualizado. Los clientes ya ven el nuevo.' WHEN 2 THEN N'Alias updated. Clients now see the new one.' WHEN 3 THEN N'Alias atualizado. Os clientes já veem o novo.' WHEN 4 THEN N'Alias mis à jour. Les clients voient déjà le nouveau.' WHEN 5 THEN N'エイリアスを更新しました。お客様には新しいエイリアスが表示されます。' ELSE N'Alias actualizado. Los clientes ya ven el nuevo.' END,
    [pagos_err_alias] = CASE [IdiomaID] WHEN 1 THEN N'El alias tiene que tener entre {0} y {1} caracteres: letras sin tilde, números, puntos o guiones.' WHEN 2 THEN N'The alias must have between {0} and {1} characters: letters without accents, numbers, dots or hyphens.' WHEN 3 THEN N'O alias deve ter entre {0} e {1} caracteres: letras sem acento, números, pontos ou hífens.' WHEN 4 THEN N'Lalias doit contenir entre {0} et {1} caractères : lettres sans accent, chiffres, points ou tirets.' WHEN 5 THEN N'エイリアスは{0}〜{1}文字で、アクセントなしの英字、数字、ピリオド、ハイフンのみ使えます。' ELSE N'El alias tiene que tener entre {0} y {1} caracteres: letras sin tilde, números, puntos o guiones.' END,
    [pagos_err_alias_igual] = CASE [IdiomaID] WHEN 1 THEN N'Ese ya es el alias actual.' WHEN 2 THEN N'That is already the current alias.' WHEN 3 THEN N'Esse já é o alias atual.' WHEN 4 THEN N'Cest déjà lalias actuel.' WHEN 5 THEN N'すでに現在のエイリアスです。' ELSE N'Ese ya es el alias actual.' END,
    [pagos_err_alias_permiso] = CASE [IdiomaID] WHEN 1 THEN N'Solo administración o recepción puede cambiar el alias.' WHEN 2 THEN N'Only administration or reception can change the alias.' WHEN 3 THEN N'Somente a administração ou a recepção pode alterar o alias.' WHEN 4 THEN N'Seuls ladministration ou laccueil peuvent modifier lalias.' WHEN 5 THEN N'エイリアスを変更できるのは管理者または受付のみです。' ELSE N'Solo administración o recepción puede cambiar el alias.' END,
    [pagos_transf_vacio] = CASE [IdiomaID] WHEN 1 THEN N'No hay transferencias pendientes de confirmar.' WHEN 2 THEN N'There are no transfers waiting for confirmation.' WHEN 3 THEN N'Não há transferências pendentes de confirmação.' WHEN 4 THEN N'Aucun virement en attente de confirmation.' WHEN 5 THEN N'確認待ちの振込はありません。' ELSE N'No hay transferencias pendientes de confirmar.' END,
    [pagos_transferencia_alias] = CASE [IdiomaID] WHEN 1 THEN N'Alias del gimnasio' WHEN 2 THEN N'Gym alias' WHEN 3 THEN N'Alias da academia' WHEN 4 THEN N'Alias de la salle' WHEN 5 THEN N'ジムのエイリアス' ELSE N'Alias del gimnasio' END,
    [pagos_transferencia_info] = CASE [IdiomaID] WHEN 1 THEN N'Transferí el monto a ese alias y después tocá "Informar transferencia". Recepción verifica que llegó y recién ahí se activa el abono.' WHEN 2 THEN N'Transfer the amount to that alias and then tap "Report transfer". Reception checks that it arrived and only then the membership is activated.' WHEN 3 THEN N'Transfira o valor para esse alias e depois toque em "Informar transferência". A recepção confere se chegou e só então o plano é ativado.' WHEN 4 THEN N'Virez le montant à cet alias puis touchez « Signaler le virement ». L''accueil vérifie sa réception et ce n''est qu''alors que l''abonnement est activé.' WHEN 5 THEN N'このエイリアスに金額を振り込み、「振込を報告」を押してください。受付が入金を確認してから会員権が有効になります。' ELSE N'Transferí el monto a ese alias y después tocá "Informar transferencia". Recepción verifica que llegó y recién ahí se activa el abono.' END,
    [pagos_btn_informar_transf] = CASE [IdiomaID] WHEN 1 THEN N'Informar transferencia' WHEN 2 THEN N'Report transfer' WHEN 3 THEN N'Informar transferência' WHEN 4 THEN N'Signaler le virement' WHEN 5 THEN N'振込を報告' ELSE N'Informar transferencia' END,
    [pagos_transf_informada] = CASE [IdiomaID] WHEN 1 THEN N'Recibimos el aviso de tu transferencia. El abono se activa cuando recepción confirme que llegó.' WHEN 2 THEN N'We received your transfer notice. The membership will be activated once reception confirms it arrived.' WHEN 3 THEN N'Recebemos o aviso da sua transferência. O plano será ativado quando a recepção confirmar que chegou.' WHEN 4 THEN N'Nous avons reçu l''avis de votre virement. L''abonnement sera activé quand l''accueil confirmera sa réception.' WHEN 5 THEN N'振込の報告を受け付けました。受付が入金を確認すると会員権が有効になります。' ELSE N'Recibimos el aviso de tu transferencia. El abono se activa cuando recepción confirme que llegó.' END,
    [pagos_aviso_transf_pendiente] = CASE [IdiomaID] WHEN 1 THEN N'Transferencia informada: el abono se activa cuando recepción la confirme.' WHEN 2 THEN N'Transfer reported: the membership is activated when reception confirms it.' WHEN 3 THEN N'Transferência informada: o plano é ativado quando a recepção confirmar.' WHEN 4 THEN N'Virement signalé : l''abonnement sera activé après confirmation de l''accueil.' WHEN 5 THEN N'振込報告済み：受付の確認後に会員権が有効になります。' ELSE N'Transferencia informada: el abono se activa cuando recepción la confirme.' END,
    [pagos_estado_transf_pendiente] = CASE [IdiomaID] WHEN 1 THEN N'Transferencia a confirmar' WHEN 2 THEN N'Transfer to confirm' WHEN 3 THEN N'Transferência a confirmar' WHEN 4 THEN N'Virement à confirmer' WHEN 5 THEN N'確認待ちの振込' ELSE N'Transferencia a confirmar' END,
    [pagos_transf_titulo] = CASE [IdiomaID] WHEN 1 THEN N'Transferencias a confirmar' WHEN 2 THEN N'Transfers to confirm' WHEN 3 THEN N'Transferências a confirmar' WHEN 4 THEN N'Virements à confirmer' WHEN 5 THEN N'確認待ちの振込' ELSE N'Transferencias a confirmar' END,
    [pagos_transf_ayuda] = CASE [IdiomaID] WHEN 1 THEN N'Comprobá en la cuenta del gimnasio que la transferencia llegó antes de confirmarla. Al confirmarla se activa el abono.' WHEN 2 THEN N'Check in the gym account that the transfer arrived before confirming it. Confirming it activates the membership.' WHEN 3 THEN N'Confira na conta da academia se a transferência chegou antes de confirmá-la. Ao confirmar, o plano é ativado.' WHEN 4 THEN N'Vérifiez sur le compte de la salle que le virement est arrivé avant de le confirmer. La confirmation active l''abonnement.' WHEN 5 THEN N'確認する前に、ジムの口座に入金があることを確かめてください。確認すると会員権が有効になります。' ELSE N'Comprobá en la cuenta del gimnasio que la transferencia llegó antes de confirmarla. Al confirmarla se activa el abono.' END,
    [pagos_transf_confirmar] = CASE [IdiomaID] WHEN 1 THEN N'Confirmar' WHEN 2 THEN N'Confirm' WHEN 3 THEN N'Confirmar' WHEN 4 THEN N'Confirmer' WHEN 5 THEN N'確認' ELSE N'Confirmar' END,
    [pagos_transf_rechazar] = CASE [IdiomaID] WHEN 1 THEN N'Rechazar' WHEN 2 THEN N'Reject' WHEN 3 THEN N'Rejeitar' WHEN 4 THEN N'Refuser' WHEN 5 THEN N'却下' ELSE N'Rechazar' END,
    [pagos_transf_rechazar_titulo] = CASE [IdiomaID] WHEN 1 THEN N'Rechazar transferencia' WHEN 2 THEN N'Reject transfer' WHEN 3 THEN N'Rejeitar transferência' WHEN 4 THEN N'Refuser le virement' WHEN 5 THEN N'振込を却下' ELSE N'Rechazar transferencia' END,
    [pagos_transf_rechazar_si] = CASE [IdiomaID] WHEN 1 THEN N'Sí, rechazar' WHEN 2 THEN N'Yes, reject' WHEN 3 THEN N'Sim, rejeitar' WHEN 4 THEN N'Oui, refuser' WHEN 5 THEN N'はい、却下します' ELSE N'Sí, rechazar' END,
    [pagos_transf_rechazar_msg] = CASE [IdiomaID] WHEN 1 THEN N'¿Rechazar la transferencia de {0} por {1}? El abono no se activa y el cliente tiene que volver a pagar.' WHEN 2 THEN N'Reject the transfer from {0} for {1}? The membership is not activated and the client has to pay again.' WHEN 3 THEN N'Rejeitar a transferência de {0} de {1}? O plano não é ativado e o cliente precisa pagar de novo.' WHEN 4 THEN N'Refuser le virement de {0} pour {1} ? L''abonnement n''est pas activé et le client doit payer à nouveau.' WHEN 5 THEN N'{0} の {1} の振込を却下しますか？会員権は有効にならず、お客様は再度支払う必要があります。' ELSE N'¿Rechazar la transferencia de {0} por {1}? El abono no se activa y el cliente tiene que volver a pagar.' END,
    [pagos_transf_confirmada] = CASE [IdiomaID] WHEN 1 THEN N'Transferencia confirmada. El abono cubre del {0} al {1}.' WHEN 2 THEN N'Transfer confirmed. The membership covers {0} to {1}.' WHEN 3 THEN N'Transferência confirmada. O plano cobre de {0} a {1}.' WHEN 4 THEN N'Virement confirmé. L''abonnement couvre du {0} au {1}.' WHEN 5 THEN N'振込を確認しました。会員権の期間は {0} から {1} までです。' ELSE N'Transferencia confirmada. El abono cubre del {0} al {1}.' END,
    [pagos_transf_rechazada] = CASE [IdiomaID] WHEN 1 THEN N'Transferencia rechazada.' WHEN 2 THEN N'Transfer rejected.' WHEN 3 THEN N'Transferência rejeitada.' WHEN 4 THEN N'Virement refusé.' WHEN 5 THEN N'振込を却下しました。' ELSE N'Transferencia rechazada.' END,
    [pagos_transf_col_fecha] = CASE [IdiomaID] WHEN 1 THEN N'Informada' WHEN 2 THEN N'Reported' WHEN 3 THEN N'Informada' WHEN 4 THEN N'Signalé' WHEN 5 THEN N'報告日時' ELSE N'Informada' END,
    [pagos_transf_col_informo] = CASE [IdiomaID] WHEN 1 THEN N'Informó' WHEN 2 THEN N'Reported by' WHEN 3 THEN N'Informou' WHEN 4 THEN N'Signalé par' WHEN 5 THEN N'報告者' ELSE N'Informó' END,
    [pagos_err_transf_pendiente] = CASE [IdiomaID] WHEN 1 THEN N'Ya hay una transferencia informada para este alumno. Recepción la está verificando.' WHEN 2 THEN N'A transfer has already been reported for this student. Reception is checking it.' WHEN 3 THEN N'Já existe uma transferência informada para este aluno. A recepção está conferindo.' WHEN 4 THEN N'Un virement a déjà été signalé pour cet élève. L''accueil est en train de le vérifier.' WHEN 5 THEN N'この生徒の振込はすでに報告されています。受付で確認中です。' ELSE N'Ya hay una transferencia informada para este alumno. Recepción la está verificando.' END,
    [pagos_err_transf_resuelta] = CASE [IdiomaID] WHEN 1 THEN N'Esa transferencia ya fue confirmada o rechazada. Actualizá la página.' WHEN 2 THEN N'That transfer was already confirmed or rejected. Refresh the page.' WHEN 3 THEN N'Essa transferência já foi confirmada ou rejeitada. Atualize a página.' WHEN 4 THEN N'Ce virement a déjà été confirmé ou refusé. Actualisez la page.' WHEN 5 THEN N'その振込はすでに確認または却下されています。ページを更新してください。' ELSE N'Esa transferencia ya fue confirmada o rechazada. Actualizá la página.' END;
GO

-- 3. Traducciones: Pantalla_Alumnos
IF COL_LENGTH('Traducciones.Pantalla_Alumnos', 'alumnos_cobro_titulo') IS NULL
    ALTER TABLE [Traducciones].[Pantalla_Alumnos] ADD [alumnos_cobro_titulo] NVARCHAR(500) NOT NULL DEFAULT N'';
IF COL_LENGTH('Traducciones.Pantalla_Alumnos', 'alumnos_cobro_modalidad') IS NULL
    ALTER TABLE [Traducciones].[Pantalla_Alumnos] ADD [alumnos_cobro_modalidad] NVARCHAR(500) NOT NULL DEFAULT N'';
IF COL_LENGTH('Traducciones.Pantalla_Alumnos', 'alumnos_cobro_ayuda') IS NULL
    ALTER TABLE [Traducciones].[Pantalla_Alumnos] ADD [alumnos_cobro_ayuda] NVARCHAR(500) NOT NULL DEFAULT N'';
IF COL_LENGTH('Traducciones.Pantalla_Alumnos', 'alumnos_cobro_despues') IS NULL
    ALTER TABLE [Traducciones].[Pantalla_Alumnos] ADD [alumnos_cobro_despues] NVARCHAR(500) NOT NULL DEFAULT N'';
IF COL_LENGTH('Traducciones.Pantalla_Alumnos', 'alumnos_cobro_ahora') IS NULL
    ALTER TABLE [Traducciones].[Pantalla_Alumnos] ADD [alumnos_cobro_ahora] NVARCHAR(500) NOT NULL DEFAULT N'';
GO

UPDATE [Traducciones].[Pantalla_Alumnos] SET
    [alumnos_cobro_titulo] = CASE [IdiomaID] WHEN 1 THEN N'Primer mes del abono' WHEN 2 THEN N'First month of the membership' WHEN 3 THEN N'Primeiro mês do plano' WHEN 4 THEN N'Premier mois de l''abonnement' WHEN 5 THEN N'会員権の初月' ELSE N'Primer mes del abono' END,
    [alumnos_cobro_modalidad] = CASE [IdiomaID] WHEN 1 THEN N'Clases por semana' WHEN 2 THEN N'Classes per week' WHEN 3 THEN N'Aulas por semana' WHEN 4 THEN N'Cours par semaine' WHEN 5 THEN N'週あたりのクラス数' ELSE N'Clases por semana' END,
    [alumnos_cobro_ayuda] = CASE [IdiomaID] WHEN 1 THEN N'Si prefiere pagar después, tocá Más tarde: se puede cobrar en cualquier momento desde Pagos.' WHEN 2 THEN N'If they prefer to pay later, tap Later: it can be charged at any time from Payments.' WHEN 3 THEN N'Se preferir pagar depois, toque em Mais tarde: dá para cobrar a qualquer momento em Pagamentos.' WHEN 4 THEN N'S''il préfère payer plus tard, touchez Plus tard : vous pourrez encaisser à tout moment depuis Paiements.' WHEN 5 THEN N'後で支払う場合は「後で」を押してください。支払い画面からいつでも受け取れます。' ELSE N'Si prefiere pagar después, tocá Más tarde: se puede cobrar en cualquier momento desde Pagos.' END,
    [alumnos_cobro_despues] = CASE [IdiomaID] WHEN 1 THEN N'Más tarde' WHEN 2 THEN N'Later' WHEN 3 THEN N'Mais tarde' WHEN 4 THEN N'Plus tard' WHEN 5 THEN N'後で' ELSE N'Más tarde' END,
    [alumnos_cobro_ahora] = CASE [IdiomaID] WHEN 1 THEN N'Cobrar ahora' WHEN 2 THEN N'Charge now' WHEN 3 THEN N'Cobrar agora' WHEN 4 THEN N'Encaisser maintenant' WHEN 5 THEN N'今すぐ受け取る' ELSE N'Cobrar ahora' END;
GO

-- 4. Traducciones: Pantalla_Usuarios
IF COL_LENGTH('Traducciones.Pantalla_Usuarios', 'usuarios_cred_titulo') IS NULL
    ALTER TABLE [Traducciones].[Pantalla_Usuarios] ADD [usuarios_cred_titulo] NVARCHAR(500) NOT NULL DEFAULT N'';
IF COL_LENGTH('Traducciones.Pantalla_Usuarios', 'entrenadores_cred_titulo') IS NULL
    ALTER TABLE [Traducciones].[Pantalla_Usuarios] ADD [entrenadores_cred_titulo] NVARCHAR(500) NOT NULL DEFAULT N'';
IF COL_LENGTH('Traducciones.Pantalla_Usuarios', 'usuarios_cred_aviso') IS NULL
    ALTER TABLE [Traducciones].[Pantalla_Usuarios] ADD [usuarios_cred_aviso] NVARCHAR(500) NOT NULL DEFAULT N'';
IF COL_LENGTH('Traducciones.Pantalla_Usuarios', 'usuarios_cred_aviso_propia') IS NULL
    ALTER TABLE [Traducciones].[Pantalla_Usuarios] ADD [usuarios_cred_aviso_propia] NVARCHAR(500) NOT NULL DEFAULT N'';
IF COL_LENGTH('Traducciones.Pantalla_Usuarios', 'usuarios_cred_usuario') IS NULL
    ALTER TABLE [Traducciones].[Pantalla_Usuarios] ADD [usuarios_cred_usuario] NVARCHAR(500) NOT NULL DEFAULT N'';
IF COL_LENGTH('Traducciones.Pantalla_Usuarios', 'usuarios_cred_contrasena') IS NULL
    ALTER TABLE [Traducciones].[Pantalla_Usuarios] ADD [usuarios_cred_contrasena] NVARCHAR(500) NOT NULL DEFAULT N'';
GO

UPDATE [Traducciones].[Pantalla_Usuarios] SET
    [usuarios_cred_titulo] = CASE [IdiomaID] WHEN 1 THEN N'Usuario creado' WHEN 2 THEN N'User created' WHEN 3 THEN N'Usuário criado' WHEN 4 THEN N'Utilisateur créé' WHEN 5 THEN N'ユーザーを作成しました' ELSE N'Usuario creado' END,
    [entrenadores_cred_titulo] = CASE [IdiomaID] WHEN 1 THEN N'Entrenador creado' WHEN 2 THEN N'Trainer created' WHEN 3 THEN N'Treinador criado' WHEN 4 THEN N'Entraîneur créé' WHEN 5 THEN N'トレーナーを作成しました' ELSE N'Entrenador creado' END,
    [usuarios_cred_aviso] = CASE [IdiomaID] WHEN 1 THEN N'Pasale estos datos a la persona. La contraseña no se vuelve a mostrar y la va a tener que cambiar en el primer inicio de sesión.' WHEN 2 THEN N'Give these details to the person. The password will not be shown again and they will have to change it at first login.' WHEN 3 THEN N'Passe estes dados para a pessoa. A senha não será mostrada de novo e ela terá que trocá-la no primeiro acesso.' WHEN 4 THEN N'Transmettez ces données à la personne. Le mot de passe ne sera plus affiché et elle devra le changer à la première connexion.' WHEN 5 THEN N'この情報を本人に伝えてください。パスワードは再表示されず、初回ログイン時に変更が必要です。' ELSE N'Pasale estos datos a la persona. La contraseña no se vuelve a mostrar y la va a tener que cambiar en el primer inicio de sesión.' END,
    [usuarios_cred_aviso_propia] = CASE [IdiomaID] WHEN 1 THEN N'La persona entra con la contraseña que escribiste y la va a tener que cambiar en el primer inicio de sesión.' WHEN 2 THEN N'The person logs in with the password you entered and will have to change it at first login.' WHEN 3 THEN N'A pessoa entra com a senha que você digitou e terá que trocá-la no primeiro acesso.' WHEN 4 THEN N'La personne se connecte avec le mot de passe saisi et devra le changer à la première connexion.' WHEN 5 THEN N'入力したパスワードでログインし、初回ログイン時に変更が必要です。' ELSE N'La persona entra con la contraseña que escribiste y la va a tener que cambiar en el primer inicio de sesión.' END,
    [usuarios_cred_usuario] = CASE [IdiomaID] WHEN 1 THEN N'Usuario' WHEN 2 THEN N'User' WHEN 3 THEN N'Usuário' WHEN 4 THEN N'Utilisateur' WHEN 5 THEN N'ユーザー' ELSE N'Usuario' END,
    [usuarios_cred_contrasena] = CASE [IdiomaID] WHEN 1 THEN N'Contraseña' WHEN 2 THEN N'Password' WHEN 3 THEN N'Senha' WHEN 4 THEN N'Mot de passe' WHEN 5 THEN N'パスワード' ELSE N'Contraseña' END;
GO

-- 5. Traducciones: Pantalla_CambiarContra
IF COL_LENGTH('Traducciones.Pantalla_CambiarContra', 'cambio_msg_reingresar') IS NULL
    ALTER TABLE [Traducciones].[Pantalla_CambiarContra] ADD [cambio_msg_reingresar] NVARCHAR(500) NOT NULL DEFAULT N'';
IF COL_LENGTH('Traducciones.Pantalla_CambiarContra', 'cambio_msg_bloqueado') IS NULL
    ALTER TABLE [Traducciones].[Pantalla_CambiarContra] ADD [cambio_msg_bloqueado] NVARCHAR(500) NOT NULL DEFAULT N'';
IF COL_LENGTH('Traducciones.Pantalla_CambiarContra', 'cambio_msg_error_generico') IS NULL
    ALTER TABLE [Traducciones].[Pantalla_CambiarContra] ADD [cambio_msg_error_generico] NVARCHAR(500) NOT NULL DEFAULT N'';
GO

UPDATE [Traducciones].[Pantalla_CambiarContra] SET
    [cambio_msg_reingresar] = CASE [IdiomaID] WHEN 1 THEN N'Volvé a iniciar sesión con la contraseña nueva.' WHEN 2 THEN N'Log in again with your new password.' WHEN 3 THEN N'Entre novamente com a nova senha.' WHEN 4 THEN N'Reconnectez-vous avec le nouveau mot de passe.' WHEN 5 THEN N'新しいパスワードでもう一度ログインしてください。' ELSE N'Volvé a iniciar sesión con la contraseña nueva.' END,
    [cambio_msg_bloqueado] = CASE [IdiomaID] WHEN 1 THEN N'La cuenta se bloqueó por demasiados intentos. Vas a poder recuperarla desde el inicio de sesión.' WHEN 2 THEN N'The account was locked after too many attempts. You can recover it from the login page.' WHEN 3 THEN N'A conta foi bloqueada por excesso de tentativas. Você pode recuperá-la na tela de login.' WHEN 4 THEN N'Le compte a été bloqué après trop de tentatives. Vous pourrez le récupérer depuis la connexion.' WHEN 5 THEN N'試行回数が多すぎるためアカウントがロックされました。ログイン画面から復旧できます。' ELSE N'La cuenta se bloqueó por demasiados intentos. Vas a poder recuperarla desde el inicio de sesión.' END,
    [cambio_msg_error_generico] = CASE [IdiomaID] WHEN 1 THEN N'No se pudo cambiar la contraseña. Intentá de nuevo en unos minutos.' WHEN 2 THEN N'The password could not be changed. Try again in a few minutes.' WHEN 3 THEN N'Não foi possível trocar a senha. Tente de novo em alguns minutos.' WHEN 4 THEN N'Impossible de changer le mot de passe. Réessayez dans quelques minutes.' WHEN 5 THEN N'パスワードを変更できませんでした。数分後にもう一度お試しください。' ELSE N'No se pudo cambiar la contraseña. Intentá de nuevo en unos minutos.' END;
GO

-- 6. Peso corporal: rango razonable de 20 a 300 kg
UPDATE [Traducciones].[Pantalla_Alumnos] SET
    [alumnos_msg_peso_invalido] = CASE [IdiomaID] WHEN 1 THEN N'El peso debe estar entre 20 y 300 kg' WHEN 2 THEN N'Weight must be between 20 and 300 kg' WHEN 3 THEN N'O peso deve estar entre 20 e 300 kg' WHEN 4 THEN N'Le poids doit être entre 20 et 300 kg' WHEN 5 THEN N'体重は20〜300kgの間でなければなりません' ELSE N'El peso debe estar entre 20 y 300 kg' END,
    [perfil_err_peso] = CASE [IdiomaID] WHEN 1 THEN N'El peso debe estar entre 20 y 300 kg.' WHEN 2 THEN N'Weight must be between 20 and 300 kg.' WHEN 3 THEN N'O peso deve estar entre 20 e 300 kg.' WHEN 4 THEN N'Le poids doit être entre 20 et 300 kg.' WHEN 5 THEN N'体重は20〜300kgの間で入力してください。' ELSE N'El peso debe estar entre 20 y 300 kg.' END;
GO

-- 7. Alumnos con un peso fuera del rango nuevo (corregir desde Alumnos -> Modificar)
SELECT dni, peso
FROM [dbo].[Alumnos]
WHERE peso IS NOT NULL AND (peso < 20 OR peso > 300);
GO
