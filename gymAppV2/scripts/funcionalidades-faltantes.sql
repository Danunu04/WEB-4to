USE [GymApp]
GO

-- ============================================================================
-- Funcionalidades faltantes de la documentación (14/11/2026)
--
--   - CUN001 Peso corporal desde Perfil -> tabla PesoHistorial
--   - Notificación de cambio de precio  -> tablas AvisoPrecio y AvisoPrecioVisto
--   - Alerta de check-in vencido        -> tabla AlertaCheckin
--   - Traducciones de: baja lógica de alumnos, cobro del primer mes, peso corporal,
--     precios de cuota, aviso de precio y alertas de check-in.
--
-- También borra, si existen, los triggers del esquema viejo (ScriptCreacion.sql) que
-- escribían en Alumnos/RutinaEjercicio sin recalcular los dígitos verificadores.
--
-- Antes de correrlo: hacer un backup.
-- Después de correrlo: VerificacioDV -> "Recalcular dígitos".
-- IdiomaID: 1 ES, 2 EN, 3 PT, 4 FR, 5 JA.
-- ============================================================================

-- 1. Triggers viejos
IF OBJECT_ID('dbo.trg_PesoHistorial_ActualizarPesoAlumno', 'TR') IS NOT NULL
    DROP TRIGGER [dbo].[trg_PesoHistorial_ActualizarPesoAlumno];
IF OBJECT_ID('dbo.trg_RutinaEjercicio_Porcentaje1RM', 'TR') IS NOT NULL
    DROP TRIGGER [dbo].[trg_RutinaEjercicio_Porcentaje1RM];
GO

-- 2. PesoHistorial (historial de peso corporal de cada alumno)
IF OBJECT_ID('dbo.PesoHistorial', 'U') IS NULL
BEGIN
    CREATE TABLE [dbo].[PesoHistorial](
        [codPeso]  INT           IDENTITY(1,1) NOT NULL,
        [dni]      INT           NOT NULL,
        [peso]     DECIMAL(5,2)  NOT NULL,
        [fecha]    DATE          NOT NULL,
        [dvh]      VARCHAR(64)   NOT NULL,
        CONSTRAINT [PK_PesoHistorial] PRIMARY KEY CLUSTERED ([codPeso] ASC),
        CONSTRAINT [FK_PesoHistorial_Alumno] FOREIGN KEY ([dni]) REFERENCES [dbo].[Alumnos] ([dni]),
        CONSTRAINT [CK_PesoHistorial_Peso] CHECK ([peso] > 0 AND [peso] < 500)
    );
    CREATE INDEX [IX_PesoHistorial_Alumno] ON [dbo].[PesoHistorial] ([dni], [fecha]);
    PRINT 'Tabla PesoHistorial creada.';
END
GO

-- 3. Avisos de cambio de precio
IF OBJECT_ID('dbo.AvisoPrecio', 'U') IS NULL
BEGIN
    CREATE TABLE [dbo].[AvisoPrecio](
        [codAviso]         INT             IDENTITY(1,1) NOT NULL,
        [modalidadId]      INT             NOT NULL,
        [descripcion]      NVARCHAR(100)   NOT NULL,
        [precioAnterior]   DECIMAL(10,2)   NOT NULL,
        [precioNuevo]      DECIMAL(10,2)   NOT NULL,
        [fecha]            DATETIME        NOT NULL,
        [usuarioRegistro]  VARCHAR(50)     NULL,
        [dvh]              VARCHAR(64)     NOT NULL,
        CONSTRAINT [PK_AvisoPrecio] PRIMARY KEY CLUSTERED ([codAviso] ASC)
    );
    CREATE INDEX [IX_AvisoPrecio_Fecha] ON [dbo].[AvisoPrecio] ([fecha]);
    PRINT 'Tabla AvisoPrecio creada.';
END
GO

IF OBJECT_ID('dbo.AvisoPrecioVisto', 'U') IS NULL
BEGIN
    CREATE TABLE [dbo].[AvisoPrecioVisto](
        [codAviso]    INT           NOT NULL,
        [usr]         VARCHAR(50)   NOT NULL,
        [fechaVisto]  DATETIME      NOT NULL,
        [dvh]         VARCHAR(64)   NOT NULL,
        CONSTRAINT [PK_AvisoPrecioVisto] PRIMARY KEY CLUSTERED ([codAviso] ASC, [usr] ASC),
        CONSTRAINT [FK_AvisoPrecioVisto_Aviso] FOREIGN KEY ([codAviso]) REFERENCES [dbo].[AvisoPrecio] ([codAviso])
    );
    PRINT 'Tabla AvisoPrecioVisto creada.';
END
GO

-- 4. Alertas de check-in con abono vencido
IF OBJECT_ID('dbo.AlertaCheckin', 'U') IS NULL
BEGIN
    CREATE TABLE [dbo].[AlertaCheckin](
        [codAlerta]         INT           IDENTITY(1,1) NOT NULL,
        [dni]               INT           NOT NULL,
        [situacion]         VARCHAR(20)   NOT NULL,
        [fechaVencimiento]  DATE          NULL,
        [fecha]             DATETIME      NOT NULL,
        [vista]             BIT           NOT NULL CONSTRAINT [DF_AlertaCheckin_vista] DEFAULT (0),
        [usuarioVista]      VARCHAR(50)   NULL,
        [fechaVista]        DATETIME      NULL,
        [dvh]               VARCHAR(64)   NOT NULL,
        CONSTRAINT [PK_AlertaCheckin] PRIMARY KEY CLUSTERED ([codAlerta] ASC),
        CONSTRAINT [FK_AlertaCheckin_Alumno] FOREIGN KEY ([dni]) REFERENCES [dbo].[Alumnos] ([dni]),
        CONSTRAINT [CK_AlertaCheckin_Situacion] CHECK ([situacion] IN ('Vencido', 'SinAbono'))
    );
    CREATE INDEX [IX_AlertaCheckin_Vista] ON [dbo].[AlertaCheckin] ([vista], [fecha]);
    PRINT 'Tabla AlertaCheckin creada.';
END
GO

-- ============================================================================
-- 5. Traducciones
-- ============================================================================

IF COL_LENGTH('Traducciones.Pantalla_Alumnos', 'alumnos_msg_baja') IS NULL
    ALTER TABLE [Traducciones].[Pantalla_Alumnos] ADD [alumnos_msg_baja] NVARCHAR(500) NOT NULL DEFAULT N'';
IF COL_LENGTH('Traducciones.Pantalla_Alumnos', 'alumnos_msg_baja_cuentas') IS NULL
    ALTER TABLE [Traducciones].[Pantalla_Alumnos] ADD [alumnos_msg_baja_cuentas] NVARCHAR(500) NOT NULL DEFAULT N'';
IF COL_LENGTH('Traducciones.Pantalla_Alumnos', 'alumnos_primer_mes') IS NULL
    ALTER TABLE [Traducciones].[Pantalla_Alumnos] ADD [alumnos_primer_mes] NVARCHAR(500) NOT NULL DEFAULT N'';
IF COL_LENGTH('Traducciones.Pantalla_Alumnos', 'alumnos_btn_cobrar_primer_mes') IS NULL
    ALTER TABLE [Traducciones].[Pantalla_Alumnos] ADD [alumnos_btn_cobrar_primer_mes] NVARCHAR(500) NOT NULL DEFAULT N'';
IF COL_LENGTH('Traducciones.Pantalla_Alumnos', 'perfil_peso_titulo') IS NULL
    ALTER TABLE [Traducciones].[Pantalla_Alumnos] ADD [perfil_peso_titulo] NVARCHAR(500) NOT NULL DEFAULT N'';
IF COL_LENGTH('Traducciones.Pantalla_Alumnos', 'perfil_peso_subtitulo') IS NULL
    ALTER TABLE [Traducciones].[Pantalla_Alumnos] ADD [perfil_peso_subtitulo] NVARCHAR(500) NOT NULL DEFAULT N'';
IF COL_LENGTH('Traducciones.Pantalla_Alumnos', 'perfil_peso_actual') IS NULL
    ALTER TABLE [Traducciones].[Pantalla_Alumnos] ADD [perfil_peso_actual] NVARCHAR(500) NOT NULL DEFAULT N'';
IF COL_LENGTH('Traducciones.Pantalla_Alumnos', 'perfil_peso_sin_dato') IS NULL
    ALTER TABLE [Traducciones].[Pantalla_Alumnos] ADD [perfil_peso_sin_dato] NVARCHAR(500) NOT NULL DEFAULT N'';
IF COL_LENGTH('Traducciones.Pantalla_Alumnos', 'perfil_peso_historial') IS NULL
    ALTER TABLE [Traducciones].[Pantalla_Alumnos] ADD [perfil_peso_historial] NVARCHAR(500) NOT NULL DEFAULT N'';
IF COL_LENGTH('Traducciones.Pantalla_Alumnos', 'perfil_btn_registrar_peso') IS NULL
    ALTER TABLE [Traducciones].[Pantalla_Alumnos] ADD [perfil_btn_registrar_peso] NVARCHAR(500) NOT NULL DEFAULT N'';
IF COL_LENGTH('Traducciones.Pantalla_Alumnos', 'perfil_msg_peso') IS NULL
    ALTER TABLE [Traducciones].[Pantalla_Alumnos] ADD [perfil_msg_peso] NVARCHAR(500) NOT NULL DEFAULT N'';
IF COL_LENGTH('Traducciones.Pantalla_Alumnos', 'perfil_err_peso') IS NULL
    ALTER TABLE [Traducciones].[Pantalla_Alumnos] ADD [perfil_err_peso] NVARCHAR(500) NOT NULL DEFAULT N'';
IF COL_LENGTH('Traducciones.Pantalla_Alumnos', 'perfil_err_no_asociado') IS NULL
    ALTER TABLE [Traducciones].[Pantalla_Alumnos] ADD [perfil_err_no_asociado] NVARCHAR(500) NOT NULL DEFAULT N'';
GO

UPDATE [Traducciones].[Pantalla_Alumnos] SET
    [alumnos_msg_baja] = CASE [IdiomaID] WHEN 1 THEN N'Alumno dado de baja. Sus pagos, rutinas e historial se conservan.' WHEN 2 THEN N'Student deactivated. Their payments, routines and history are kept.' WHEN 3 THEN N'Aluno desativado. Pagamentos, rotinas e histórico são mantidos.' WHEN 4 THEN N'Élève désactivé. Ses paiements, routines et historique sont conservés.' WHEN 5 THEN N'生徒を退会にしました。支払い・ルーティン・履歴は保持されます。' ELSE N'Alumno dado de baja. Sus pagos, rutinas e historial se conservan.' END,
    [alumnos_msg_baja_cuentas] = CASE [IdiomaID] WHEN 1 THEN N'También se desactivó la cuenta: {0}.' WHEN 2 THEN N'The account was also deactivated: {0}.' WHEN 3 THEN N'A conta também foi desativada: {0}.' WHEN 4 THEN N'Le compte a aussi été désactivé : {0}.' WHEN 5 THEN N'アカウントも無効にしました：{0}。' ELSE N'También se desactivó la cuenta: {0}.' END,
    [alumnos_primer_mes] = CASE [IdiomaID] WHEN 1 THEN N'Alumno dado de alta. Cobrale el primer mes para completar la asociación.' WHEN 2 THEN N'Student registered. Charge the first month to complete the membership.' WHEN 3 THEN N'Aluno cadastrado. Cobre o primeiro mês para concluir a associação.' WHEN 4 THEN N'Élève inscrit. Encaissez le premier mois pour finaliser ladhésion.' WHEN 5 THEN N'生徒を登録しました。入会を完了するには初月の会費を受け取ってください。' ELSE N'Alumno dado de alta. Cobrale el primer mes para completar la asociación.' END,
    [alumnos_btn_cobrar_primer_mes] = CASE [IdiomaID] WHEN 1 THEN N'Cobrar primer mes' WHEN 2 THEN N'Charge first month' WHEN 3 THEN N'Cobrar primeiro mês' WHEN 4 THEN N'Encaisser le premier mois' WHEN 5 THEN N'初月を受け取る' ELSE N'Cobrar primer mes' END,
    [perfil_peso_titulo] = CASE [IdiomaID] WHEN 1 THEN N'Peso corporal' WHEN 2 THEN N'Body weight' WHEN 3 THEN N'Peso corporal' WHEN 4 THEN N'Poids corporel' WHEN 5 THEN N'体重' ELSE N'Peso corporal' END,
    [perfil_peso_subtitulo] = CASE [IdiomaID] WHEN 1 THEN N'Registrá el peso de tus alumnos para seguir su evolución.' WHEN 2 THEN N'Record your students weight to track their progress.' WHEN 3 THEN N'Registre o peso dos seus alunos para acompanhar a evolução.' WHEN 4 THEN N'Enregistrez le poids de vos élèves pour suivre leur évolution.' WHEN 5 THEN N'生徒の体重を記録して推移を確認できます。' ELSE N'Registrá el peso de tus alumnos para seguir su evolución.' END,
    [perfil_peso_actual] = CASE [IdiomaID] WHEN 1 THEN N'Peso actual: {0} kg' WHEN 2 THEN N'Current weight: {0} kg' WHEN 3 THEN N'Peso atual: {0} kg' WHEN 4 THEN N'Poids actuel : {0} kg' WHEN 5 THEN N'現在の体重：{0} kg' ELSE N'Peso actual: {0} kg' END,
    [perfil_peso_sin_dato] = CASE [IdiomaID] WHEN 1 THEN N'Todavía no tiene peso registrado.' WHEN 2 THEN N'No weight recorded yet.' WHEN 3 THEN N'Ainda não há peso registrado.' WHEN 4 THEN N'Aucun poids enregistré.' WHEN 5 THEN N'まだ体重の記録がありません。' ELSE N'Todavía no tiene peso registrado.' END,
    [perfil_peso_historial] = CASE [IdiomaID] WHEN 1 THEN N'Últimos registros: {0}' WHEN 2 THEN N'Latest records: {0}' WHEN 3 THEN N'Últimos registros: {0}' WHEN 4 THEN N'Derniers relevés : {0}' WHEN 5 THEN N'最近の記録：{0}' ELSE N'Últimos registros: {0}' END,
    [perfil_btn_registrar_peso] = CASE [IdiomaID] WHEN 1 THEN N'Registrar peso' WHEN 2 THEN N'Record weight' WHEN 3 THEN N'Registrar peso' WHEN 4 THEN N'Enregistrer le poids' WHEN 5 THEN N'体重を記録' ELSE N'Registrar peso' END,
    [perfil_msg_peso] = CASE [IdiomaID] WHEN 1 THEN N'Peso registrado: {0} kg.' WHEN 2 THEN N'Weight recorded: {0} kg.' WHEN 3 THEN N'Peso registrado: {0} kg.' WHEN 4 THEN N'Poids enregistré : {0} kg.' WHEN 5 THEN N'体重を記録しました：{0} kg。' ELSE N'Peso registrado: {0} kg.' END,
    [perfil_err_peso] = CASE [IdiomaID] WHEN 1 THEN N'El peso debe ser un número mayor que 0 y menor que 500 kg.' WHEN 2 THEN N'Weight must be a number greater than 0 and less than 500 kg.' WHEN 3 THEN N'O peso deve ser um número maior que 0 e menor que 500 kg.' WHEN 4 THEN N'Le poids doit être un nombre supérieur à 0 et inférieur à 500 kg.' WHEN 5 THEN N'体重は0より大きく500 kg未満の数値で入力してください。' ELSE N'El peso debe ser un número mayor que 0 y menor que 500 kg.' END,
    [perfil_err_no_asociado] = CASE [IdiomaID] WHEN 1 THEN N'Ese alumno no está asociado a tu cuenta o está dado de baja.' WHEN 2 THEN N'That student is not linked to your account or is inactive.' WHEN 3 THEN N'Esse aluno não está vinculado à sua conta ou está inativo.' WHEN 4 THEN N'Cet élève nest pas associé à votre compte ou est désactivé.' WHEN 5 THEN N'その生徒はアカウントに紐づいていないか、退会しています。' ELSE N'Ese alumno no está asociado a tu cuenta o está dado de baja.' END;
GO

IF COL_LENGTH('Traducciones.Pantalla_Pagos', 'precios_titulo') IS NULL
    ALTER TABLE [Traducciones].[Pantalla_Pagos] ADD [precios_titulo] NVARCHAR(500) NOT NULL DEFAULT N'';
IF COL_LENGTH('Traducciones.Pantalla_Pagos', 'precios_subtitulo') IS NULL
    ALTER TABLE [Traducciones].[Pantalla_Pagos] ADD [precios_subtitulo] NVARCHAR(500) NOT NULL DEFAULT N'';
IF COL_LENGTH('Traducciones.Pantalla_Pagos', 'precios_aviso') IS NULL
    ALTER TABLE [Traducciones].[Pantalla_Pagos] ADD [precios_aviso] NVARCHAR(500) NOT NULL DEFAULT N'';
IF COL_LENGTH('Traducciones.Pantalla_Pagos', 'precios_col_modalidad') IS NULL
    ALTER TABLE [Traducciones].[Pantalla_Pagos] ADD [precios_col_modalidad] NVARCHAR(500) NOT NULL DEFAULT N'';
IF COL_LENGTH('Traducciones.Pantalla_Pagos', 'precios_col_actual') IS NULL
    ALTER TABLE [Traducciones].[Pantalla_Pagos] ADD [precios_col_actual] NVARCHAR(500) NOT NULL DEFAULT N'';
IF COL_LENGTH('Traducciones.Pantalla_Pagos', 'precios_col_modificado') IS NULL
    ALTER TABLE [Traducciones].[Pantalla_Pagos] ADD [precios_col_modificado] NVARCHAR(500) NOT NULL DEFAULT N'';
IF COL_LENGTH('Traducciones.Pantalla_Pagos', 'precios_col_nuevo') IS NULL
    ALTER TABLE [Traducciones].[Pantalla_Pagos] ADD [precios_col_nuevo] NVARCHAR(500) NOT NULL DEFAULT N'';
IF COL_LENGTH('Traducciones.Pantalla_Pagos', 'precios_btn_guardar') IS NULL
    ALTER TABLE [Traducciones].[Pantalla_Pagos] ADD [precios_btn_guardar] NVARCHAR(500) NOT NULL DEFAULT N'';
IF COL_LENGTH('Traducciones.Pantalla_Pagos', 'precios_msg_guardado') IS NULL
    ALTER TABLE [Traducciones].[Pantalla_Pagos] ADD [precios_msg_guardado] NVARCHAR(500) NOT NULL DEFAULT N'';
IF COL_LENGTH('Traducciones.Pantalla_Pagos', 'precios_err_precio') IS NULL
    ALTER TABLE [Traducciones].[Pantalla_Pagos] ADD [precios_err_precio] NVARCHAR(500) NOT NULL DEFAULT N'';
IF COL_LENGTH('Traducciones.Pantalla_Pagos', 'precios_err_sin_cambio') IS NULL
    ALTER TABLE [Traducciones].[Pantalla_Pagos] ADD [precios_err_sin_cambio] NVARCHAR(500) NOT NULL DEFAULT N'';
GO

UPDATE [Traducciones].[Pantalla_Pagos] SET
    [precios_titulo] = CASE [IdiomaID] WHEN 1 THEN N'Precios de cuota' WHEN 2 THEN N'Membership prices' WHEN 3 THEN N'Preços da mensalidade' WHEN 4 THEN N'Tarifs de labonnement' WHEN 5 THEN N'会費' ELSE N'Precios de cuota' END,
    [precios_subtitulo] = CASE [IdiomaID] WHEN 1 THEN N'Precio mensual de cada modalidad de asistencia.' WHEN 2 THEN N'Monthly price for each attendance plan.' WHEN 3 THEN N'Preço mensal de cada modalidade de frequência.' WHEN 4 THEN N'Prix mensuel de chaque formule de fréquentation.' WHEN 5 THEN N'各通い方プランの月額料金。' ELSE N'Precio mensual de cada modalidad de asistencia.' END,
    [precios_aviso] = CASE [IdiomaID] WHEN 1 THEN N'Al cambiar un precio se avisa a todos los usuarios que no son entrenadores con un aviso en la app y un email.' WHEN 2 THEN N'When a price changes, all non-trainer users are notified in the app and by email.' WHEN 3 THEN N'Ao alterar um preço, todos os usuários que não são treinadores recebem um aviso no app e um email.' WHEN 4 THEN N'Lorsquun prix change, tous les utilisateurs non entraîneurs sont prévenus dans lapplication et par email.' WHEN 5 THEN N'料金を変更すると、トレーナー以外の全ユーザーにアプリ内通知とメールでお知らせします。' ELSE N'Al cambiar un precio se avisa a todos los usuarios que no son entrenadores con un aviso en la app y un email.' END,
    [precios_col_modalidad] = CASE [IdiomaID] WHEN 1 THEN N'Modalidad' WHEN 2 THEN N'Plan' WHEN 3 THEN N'Modalidade' WHEN 4 THEN N'Formule' WHEN 5 THEN N'プラン' ELSE N'Modalidad' END,
    [precios_col_actual] = CASE [IdiomaID] WHEN 1 THEN N'Precio actual' WHEN 2 THEN N'Current price' WHEN 3 THEN N'Preço atual' WHEN 4 THEN N'Prix actuel' WHEN 5 THEN N'現在の料金' ELSE N'Precio actual' END,
    [precios_col_modificado] = CASE [IdiomaID] WHEN 1 THEN N'Última modificación' WHEN 2 THEN N'Last change' WHEN 3 THEN N'Última alteração' WHEN 4 THEN N'Dernière modification' WHEN 5 THEN N'最終更新' ELSE N'Última modificación' END,
    [precios_col_nuevo] = CASE [IdiomaID] WHEN 1 THEN N'Nuevo precio' WHEN 2 THEN N'New price' WHEN 3 THEN N'Novo preço' WHEN 4 THEN N'Nouveau prix' WHEN 5 THEN N'新しい料金' ELSE N'Nuevo precio' END,
    [precios_btn_guardar] = CASE [IdiomaID] WHEN 1 THEN N'Guardar' WHEN 2 THEN N'Save' WHEN 3 THEN N'Salvar' WHEN 4 THEN N'Enregistrer' WHEN 5 THEN N'保存' ELSE N'Guardar' END,
    [precios_msg_guardado] = CASE [IdiomaID] WHEN 1 THEN N'Precio actualizado. Se avisó a los usuarios.' WHEN 2 THEN N'Price updated. Users have been notified.' WHEN 3 THEN N'Preço atualizado. Os usuários foram avisados.' WHEN 4 THEN N'Prix mis à jour. Les utilisateurs ont été prévenus.' WHEN 5 THEN N'料金を更新し、ユーザーに通知しました。' ELSE N'Precio actualizado. Se avisó a los usuarios.' END,
    [precios_err_precio] = CASE [IdiomaID] WHEN 1 THEN N'El precio debe ser mayor a 0.' WHEN 2 THEN N'Price must be greater than 0.' WHEN 3 THEN N'O preço deve ser maior que 0.' WHEN 4 THEN N'Le prix doit être supérieur à 0.' WHEN 5 THEN N'料金は0より大きくしてください。' ELSE N'El precio debe ser mayor a 0.' END,
    [precios_err_sin_cambio] = CASE [IdiomaID] WHEN 1 THEN N'El precio nuevo es igual al actual.' WHEN 2 THEN N'The new price is the same as the current one.' WHEN 3 THEN N'O novo preço é igual ao atual.' WHEN 4 THEN N'Le nouveau prix est identique au prix actuel.' WHEN 5 THEN N'新しい料金が現在の料金と同じです。' ELSE N'El precio nuevo es igual al actual.' END;
GO

IF COL_LENGTH('Traducciones.Pantalla_DashBoard', 'menu_precios') IS NULL
    ALTER TABLE [Traducciones].[Pantalla_DashBoard] ADD [menu_precios] NVARCHAR(500) NOT NULL DEFAULT N'';
IF COL_LENGTH('Traducciones.Pantalla_DashBoard', 'aviso_precio_titulo') IS NULL
    ALTER TABLE [Traducciones].[Pantalla_DashBoard] ADD [aviso_precio_titulo] NVARCHAR(500) NOT NULL DEFAULT N'';
IF COL_LENGTH('Traducciones.Pantalla_DashBoard', 'aviso_precio_detalle') IS NULL
    ALTER TABLE [Traducciones].[Pantalla_DashBoard] ADD [aviso_precio_detalle] NVARCHAR(500) NOT NULL DEFAULT N'';
IF COL_LENGTH('Traducciones.Pantalla_DashBoard', 'aviso_precio_entendido') IS NULL
    ALTER TABLE [Traducciones].[Pantalla_DashBoard] ADD [aviso_precio_entendido] NVARCHAR(500) NOT NULL DEFAULT N'';
GO

UPDATE [Traducciones].[Pantalla_DashBoard] SET
    [menu_precios] = CASE [IdiomaID] WHEN 1 THEN N'Precios de cuota' WHEN 2 THEN N'Membership prices' WHEN 3 THEN N'Preços da mensalidade' WHEN 4 THEN N'Tarifs' WHEN 5 THEN N'会費' ELSE N'Precios de cuota' END,
    [aviso_precio_titulo] = CASE [IdiomaID] WHEN 1 THEN N'Cambió el precio de la cuota' WHEN 2 THEN N'The membership price has changed' WHEN 3 THEN N'O preço da mensalidade mudou' WHEN 4 THEN N'Le prix de labonnement a changé' WHEN 5 THEN N'会費が変更されました' ELSE N'Cambió el precio de la cuota' END,
    [aviso_precio_detalle] = CASE [IdiomaID] WHEN 1 THEN N'{0}: de {1} a {2} (desde el {3})' WHEN 2 THEN N'{0}: from {1} to {2} (since {3})' WHEN 3 THEN N'{0}: de {1} para {2} (desde {3})' WHEN 4 THEN N'{0} : de {1} à {2} (depuis le {3})' WHEN 5 THEN N'{0}：{1} → {2}（{3}から）' ELSE N'{0}: de {1} a {2} (desde el {3})' END,
    [aviso_precio_entendido] = CASE [IdiomaID] WHEN 1 THEN N'Entendido' WHEN 2 THEN N'Got it' WHEN 3 THEN N'Entendi' WHEN 4 THEN N'Compris' WHEN 5 THEN N'了解' ELSE N'Entendido' END;
GO

IF COL_LENGTH('Traducciones.Pantalla_DashboardContent', 'dash_alertas_titulo') IS NULL
    ALTER TABLE [Traducciones].[Pantalla_DashboardContent] ADD [dash_alertas_titulo] NVARCHAR(500) NOT NULL DEFAULT N'';
IF COL_LENGTH('Traducciones.Pantalla_DashboardContent', 'dash_alertas_todas') IS NULL
    ALTER TABLE [Traducciones].[Pantalla_DashboardContent] ADD [dash_alertas_todas] NVARCHAR(500) NOT NULL DEFAULT N'';
IF COL_LENGTH('Traducciones.Pantalla_DashboardContent', 'dash_alertas_vista') IS NULL
    ALTER TABLE [Traducciones].[Pantalla_DashboardContent] ADD [dash_alertas_vista] NVARCHAR(500) NOT NULL DEFAULT N'';
IF COL_LENGTH('Traducciones.Pantalla_DashboardContent', 'dash_alertas_col_fecha') IS NULL
    ALTER TABLE [Traducciones].[Pantalla_DashboardContent] ADD [dash_alertas_col_fecha] NVARCHAR(500) NOT NULL DEFAULT N'';
IF COL_LENGTH('Traducciones.Pantalla_DashboardContent', 'dash_alertas_col_alumno') IS NULL
    ALTER TABLE [Traducciones].[Pantalla_DashboardContent] ADD [dash_alertas_col_alumno] NVARCHAR(500) NOT NULL DEFAULT N'';
IF COL_LENGTH('Traducciones.Pantalla_DashboardContent', 'dash_alertas_col_situacion') IS NULL
    ALTER TABLE [Traducciones].[Pantalla_DashboardContent] ADD [dash_alertas_col_situacion] NVARCHAR(500) NOT NULL DEFAULT N'';
IF COL_LENGTH('Traducciones.Pantalla_DashboardContent', 'dash_alertas_vencido') IS NULL
    ALTER TABLE [Traducciones].[Pantalla_DashboardContent] ADD [dash_alertas_vencido] NVARCHAR(500) NOT NULL DEFAULT N'';
IF COL_LENGTH('Traducciones.Pantalla_DashboardContent', 'dash_alertas_sin_abono') IS NULL
    ALTER TABLE [Traducciones].[Pantalla_DashboardContent] ADD [dash_alertas_sin_abono] NVARCHAR(500) NOT NULL DEFAULT N'';
GO

UPDATE [Traducciones].[Pantalla_DashboardContent] SET
    [dash_alertas_titulo] = CASE [IdiomaID] WHEN 1 THEN N'Check-in rechazados por el abono ({0})' WHEN 2 THEN N'Check-ins rejected due to membership ({0})' WHEN 3 THEN N'Check-ins recusados pela mensalidade ({0})' WHEN 4 THEN N'Entrées refusées pour abonnement ({0})' WHEN 5 THEN N'会費のため拒否されたチェックイン（{0}）' ELSE N'Check-in rechazados por el abono ({0})' END,
    [dash_alertas_todas] = CASE [IdiomaID] WHEN 1 THEN N'Marcar todas como vistas' WHEN 2 THEN N'Mark all as seen' WHEN 3 THEN N'Marcar todas como vistas' WHEN 4 THEN N'Tout marquer comme vu' WHEN 5 THEN N'すべて確認済みにする' ELSE N'Marcar todas como vistas' END,
    [dash_alertas_vista] = CASE [IdiomaID] WHEN 1 THEN N'Marcar como vista' WHEN 2 THEN N'Mark as seen' WHEN 3 THEN N'Marcar como vista' WHEN 4 THEN N'Marquer comme vu' WHEN 5 THEN N'確認済みにする' ELSE N'Marcar como vista' END,
    [dash_alertas_col_fecha] = CASE [IdiomaID] WHEN 1 THEN N'Fecha y hora' WHEN 2 THEN N'Date and time' WHEN 3 THEN N'Data e hora' WHEN 4 THEN N'Date et heure' WHEN 5 THEN N'日時' ELSE N'Fecha y hora' END,
    [dash_alertas_col_alumno] = CASE [IdiomaID] WHEN 1 THEN N'Alumno' WHEN 2 THEN N'Student' WHEN 3 THEN N'Aluno' WHEN 4 THEN N'Élève' WHEN 5 THEN N'生徒' ELSE N'Alumno' END,
    [dash_alertas_col_situacion] = CASE [IdiomaID] WHEN 1 THEN N'Situación' WHEN 2 THEN N'Status' WHEN 3 THEN N'Situação' WHEN 4 THEN N'Situation' WHEN 5 THEN N'状況' ELSE N'Situación' END,
    [dash_alertas_vencido] = CASE [IdiomaID] WHEN 1 THEN N'Abono vencido el {0}' WHEN 2 THEN N'Membership expired on {0}' WHEN 3 THEN N'Mensalidade vencida em {0}' WHEN 4 THEN N'Abonnement expiré le {0}' WHEN 5 THEN N'{0} に会費期限切れ' ELSE N'Abono vencido el {0}' END,
    [dash_alertas_sin_abono] = CASE [IdiomaID] WHEN 1 THEN N'Sin abono pago' WHEN 2 THEN N'No paid membership' WHEN 3 THEN N'Sem mensalidade paga' WHEN 4 THEN N'Aucun abonnement payé' WHEN 5 THEN N'会費未払い' ELSE N'Sin abono pago' END;
GO

PRINT 'Funcionalidades faltantes listas. Ahora: VerificacioDV -> Recalcular dígitos.';
GO
