USE [GymApp]
GO

-- ============================================================================
-- Abono mensual (03/11/2026)
--
-- Cada pago pasa a ser un abono de 30 días con fecha de inicio y de vencimiento.
-- Las renovaciones se encadenan (el nuevo abono empieza el día siguiente al
-- vencimiento del anterior), así que puede haber dos abonos que empiezan en el
-- mismo mes: la restricción única pasa de (dni, periodo) a (dni, fechaInicio).
--
-- Antes de correrlo: hacer un backup.
-- Después de correrlo: VerificacioDV -> "Recalcular dígitos" (las filas de Pagos
-- tienen columnas nuevas, así que su dvh cambia).
-- ============================================================================

-- 1. Columnas nuevas
IF COL_LENGTH('dbo.Pagos', 'fechaInicio') IS NULL
    ALTER TABLE [dbo].[Pagos] ADD [fechaInicio] DATE NULL;
IF COL_LENGTH('dbo.Pagos', 'fechaVencimiento') IS NULL
    ALTER TABLE [dbo].[Pagos] ADD [fechaVencimiento] DATE NULL;
GO

-- 2. Pagos ya cargados: el abono se toma desde el primer día del mes pagado
UPDATE [dbo].[Pagos]
SET fechaInicio = periodo,
    fechaVencimiento = DATEADD(DAY, 29, periodo)
WHERE fechaInicio IS NULL OR fechaVencimiento IS NULL;
GO

ALTER TABLE [dbo].[Pagos] ALTER COLUMN [fechaInicio] DATE NOT NULL;
ALTER TABLE [dbo].[Pagos] ALTER COLUMN [fechaVencimiento] DATE NOT NULL;
GO

-- 3. Restricciones
IF OBJECT_ID('dbo.UQ_Pagos_AlumnoPeriodo', 'UQ') IS NOT NULL
    ALTER TABLE [dbo].[Pagos] DROP CONSTRAINT [UQ_Pagos_AlumnoPeriodo];

IF OBJECT_ID('dbo.UQ_Pagos_AlumnoInicio', 'UQ') IS NULL
    ALTER TABLE [dbo].[Pagos] ADD CONSTRAINT [UQ_Pagos_AlumnoInicio] UNIQUE ([dni], [fechaInicio]);

IF OBJECT_ID('dbo.CK_Pagos_Fechas', 'C') IS NULL
    ALTER TABLE [dbo].[Pagos] ADD CONSTRAINT [CK_Pagos_Fechas] CHECK ([fechaVencimiento] >= [fechaInicio]);
GO

PRINT 'Pagos: columnas de abono listas.';
GO

-- ============================================================================
-- 4. Traducciones
-- ============================================================================

IF COL_LENGTH('Traducciones.Pantalla_Pagos', 'pagos_cliente_estado_msg') IS NULL
    ALTER TABLE [Traducciones].[Pantalla_Pagos] ADD [pagos_cliente_estado_msg] NVARCHAR(500) NOT NULL DEFAULT N'';
IF COL_LENGTH('Traducciones.Pantalla_Pagos', 'pagos_cliente_sin_alumnos') IS NULL
    ALTER TABLE [Traducciones].[Pantalla_Pagos] ADD [pagos_cliente_sin_alumnos] NVARCHAR(500) NOT NULL DEFAULT N'';
IF COL_LENGTH('Traducciones.Pantalla_Pagos', 'pagos_btn_pagar') IS NULL
    ALTER TABLE [Traducciones].[Pantalla_Pagos] ADD [pagos_btn_pagar] NVARCHAR(500) NOT NULL DEFAULT N'';
IF COL_LENGTH('Traducciones.Pantalla_Pagos', 'pagos_tarjeta_numero') IS NULL
    ALTER TABLE [Traducciones].[Pantalla_Pagos] ADD [pagos_tarjeta_numero] NVARCHAR(500) NOT NULL DEFAULT N'';
IF COL_LENGTH('Traducciones.Pantalla_Pagos', 'pagos_tarjeta_titular') IS NULL
    ALTER TABLE [Traducciones].[Pantalla_Pagos] ADD [pagos_tarjeta_titular] NVARCHAR(500) NOT NULL DEFAULT N'';
IF COL_LENGTH('Traducciones.Pantalla_Pagos', 'pagos_tarjeta_vence') IS NULL
    ALTER TABLE [Traducciones].[Pantalla_Pagos] ADD [pagos_tarjeta_vence] NVARCHAR(500) NOT NULL DEFAULT N'';
IF COL_LENGTH('Traducciones.Pantalla_Pagos', 'pagos_tarjeta_cvv') IS NULL
    ALTER TABLE [Traducciones].[Pantalla_Pagos] ADD [pagos_tarjeta_cvv] NVARCHAR(500) NOT NULL DEFAULT N'';
IF COL_LENGTH('Traducciones.Pantalla_Pagos', 'pagos_transferencia_datos') IS NULL
    ALTER TABLE [Traducciones].[Pantalla_Pagos] ADD [pagos_transferencia_datos] NVARCHAR(500) NOT NULL DEFAULT N'';
IF COL_LENGTH('Traducciones.Pantalla_Pagos', 'pagos_aviso_demo') IS NULL
    ALTER TABLE [Traducciones].[Pantalla_Pagos] ADD [pagos_aviso_demo] NVARCHAR(500) NOT NULL DEFAULT N'';
IF COL_LENGTH('Traducciones.Pantalla_Pagos', 'pagos_btn_confirmar') IS NULL
    ALTER TABLE [Traducciones].[Pantalla_Pagos] ADD [pagos_btn_confirmar] NVARCHAR(500) NOT NULL DEFAULT N'';
IF COL_LENGTH('Traducciones.Pantalla_Pagos', 'pagos_medio_tarjeta') IS NULL
    ALTER TABLE [Traducciones].[Pantalla_Pagos] ADD [pagos_medio_tarjeta] NVARCHAR(500) NOT NULL DEFAULT N'';
IF COL_LENGTH('Traducciones.Pantalla_Pagos', 'pagos_medio_transferencia') IS NULL
    ALTER TABLE [Traducciones].[Pantalla_Pagos] ADD [pagos_medio_transferencia] NVARCHAR(500) NOT NULL DEFAULT N'';
IF COL_LENGTH('Traducciones.Pantalla_Pagos', 'pagos_estados_titulo') IS NULL
    ALTER TABLE [Traducciones].[Pantalla_Pagos] ADD [pagos_estados_titulo] NVARCHAR(500) NOT NULL DEFAULT N'';
IF COL_LENGTH('Traducciones.Pantalla_Pagos', 'pagos_buscar_placeholder') IS NULL
    ALTER TABLE [Traducciones].[Pantalla_Pagos] ADD [pagos_buscar_placeholder] NVARCHAR(500) NOT NULL DEFAULT N'';
IF COL_LENGTH('Traducciones.Pantalla_Pagos', 'pagos_btn_buscar') IS NULL
    ALTER TABLE [Traducciones].[Pantalla_Pagos] ADD [pagos_btn_buscar] NVARCHAR(500) NOT NULL DEFAULT N'';
IF COL_LENGTH('Traducciones.Pantalla_Pagos', 'pagos_btn_cobrar') IS NULL
    ALTER TABLE [Traducciones].[Pantalla_Pagos] ADD [pagos_btn_cobrar] NVARCHAR(500) NOT NULL DEFAULT N'';
IF COL_LENGTH('Traducciones.Pantalla_Pagos', 'pagos_col_alumno') IS NULL
    ALTER TABLE [Traducciones].[Pantalla_Pagos] ADD [pagos_col_alumno] NVARCHAR(500) NOT NULL DEFAULT N'';
IF COL_LENGTH('Traducciones.Pantalla_Pagos', 'pagos_col_dni') IS NULL
    ALTER TABLE [Traducciones].[Pantalla_Pagos] ADD [pagos_col_dni] NVARCHAR(500) NOT NULL DEFAULT N'';
IF COL_LENGTH('Traducciones.Pantalla_Pagos', 'pagos_col_vence') IS NULL
    ALTER TABLE [Traducciones].[Pantalla_Pagos] ADD [pagos_col_vence] NVARCHAR(500) NOT NULL DEFAULT N'';
IF COL_LENGTH('Traducciones.Pantalla_Pagos', 'pagos_col_dias') IS NULL
    ALTER TABLE [Traducciones].[Pantalla_Pagos] ADD [pagos_col_dias] NVARCHAR(500) NOT NULL DEFAULT N'';
IF COL_LENGTH('Traducciones.Pantalla_Pagos', 'pagos_col_estado') IS NULL
    ALTER TABLE [Traducciones].[Pantalla_Pagos] ADD [pagos_col_estado] NVARCHAR(500) NOT NULL DEFAULT N'';
IF COL_LENGTH('Traducciones.Pantalla_Pagos', 'pagos_col_desde') IS NULL
    ALTER TABLE [Traducciones].[Pantalla_Pagos] ADD [pagos_col_desde] NVARCHAR(500) NOT NULL DEFAULT N'';
IF COL_LENGTH('Traducciones.Pantalla_Pagos', 'pagos_col_hasta') IS NULL
    ALTER TABLE [Traducciones].[Pantalla_Pagos] ADD [pagos_col_hasta] NVARCHAR(500) NOT NULL DEFAULT N'';
IF COL_LENGTH('Traducciones.Pantalla_Pagos', 'pagos_col_modalidad') IS NULL
    ALTER TABLE [Traducciones].[Pantalla_Pagos] ADD [pagos_col_modalidad] NVARCHAR(500) NOT NULL DEFAULT N'';
IF COL_LENGTH('Traducciones.Pantalla_Pagos', 'pagos_col_monto') IS NULL
    ALTER TABLE [Traducciones].[Pantalla_Pagos] ADD [pagos_col_monto] NVARCHAR(500) NOT NULL DEFAULT N'';
IF COL_LENGTH('Traducciones.Pantalla_Pagos', 'pagos_col_fecha_pago') IS NULL
    ALTER TABLE [Traducciones].[Pantalla_Pagos] ADD [pagos_col_fecha_pago] NVARCHAR(500) NOT NULL DEFAULT N'';
IF COL_LENGTH('Traducciones.Pantalla_Pagos', 'pagos_col_metodo') IS NULL
    ALTER TABLE [Traducciones].[Pantalla_Pagos] ADD [pagos_col_metodo] NVARCHAR(500) NOT NULL DEFAULT N'';
IF COL_LENGTH('Traducciones.Pantalla_Pagos', 'pagos_monto_a_pagar') IS NULL
    ALTER TABLE [Traducciones].[Pantalla_Pagos] ADD [pagos_monto_a_pagar] NVARCHAR(500) NOT NULL DEFAULT N'';
IF COL_LENGTH('Traducciones.Pantalla_Pagos', 'pagos_sin_pagos') IS NULL
    ALTER TABLE [Traducciones].[Pantalla_Pagos] ADD [pagos_sin_pagos] NVARCHAR(500) NOT NULL DEFAULT N'';
IF COL_LENGTH('Traducciones.Pantalla_Pagos', 'pagos_sin_resultados') IS NULL
    ALTER TABLE [Traducciones].[Pantalla_Pagos] ADD [pagos_sin_resultados] NVARCHAR(500) NOT NULL DEFAULT N'';
IF COL_LENGTH('Traducciones.Pantalla_Pagos', 'pagos_ok') IS NULL
    ALTER TABLE [Traducciones].[Pantalla_Pagos] ADD [pagos_ok] NVARCHAR(500) NOT NULL DEFAULT N'';
IF COL_LENGTH('Traducciones.Pantalla_Pagos', 'pagos_cobertura') IS NULL
    ALTER TABLE [Traducciones].[Pantalla_Pagos] ADD [pagos_cobertura] NVARCHAR(500) NOT NULL DEFAULT N'';
IF COL_LENGTH('Traducciones.Pantalla_Pagos', 'pagos_estado_vigente') IS NULL
    ALTER TABLE [Traducciones].[Pantalla_Pagos] ADD [pagos_estado_vigente] NVARCHAR(500) NOT NULL DEFAULT N'';
IF COL_LENGTH('Traducciones.Pantalla_Pagos', 'pagos_estado_por_vencer') IS NULL
    ALTER TABLE [Traducciones].[Pantalla_Pagos] ADD [pagos_estado_por_vencer] NVARCHAR(500) NOT NULL DEFAULT N'';
IF COL_LENGTH('Traducciones.Pantalla_Pagos', 'pagos_estado_vencido') IS NULL
    ALTER TABLE [Traducciones].[Pantalla_Pagos] ADD [pagos_estado_vencido] NVARCHAR(500) NOT NULL DEFAULT N'';
IF COL_LENGTH('Traducciones.Pantalla_Pagos', 'pagos_estado_sin_abono') IS NULL
    ALTER TABLE [Traducciones].[Pantalla_Pagos] ADD [pagos_estado_sin_abono] NVARCHAR(500) NOT NULL DEFAULT N'';
IF COL_LENGTH('Traducciones.Pantalla_Pagos', 'pagos_estado_inactivo') IS NULL
    ALTER TABLE [Traducciones].[Pantalla_Pagos] ADD [pagos_estado_inactivo] NVARCHAR(500) NOT NULL DEFAULT N'';
IF COL_LENGTH('Traducciones.Pantalla_Pagos', 'pagos_aviso_inactivo') IS NULL
    ALTER TABLE [Traducciones].[Pantalla_Pagos] ADD [pagos_aviso_inactivo] NVARCHAR(500) NOT NULL DEFAULT N'';
IF COL_LENGTH('Traducciones.Pantalla_Pagos', 'pagos_aviso_adelantado') IS NULL
    ALTER TABLE [Traducciones].[Pantalla_Pagos] ADD [pagos_aviso_adelantado] NVARCHAR(500) NOT NULL DEFAULT N'';
IF COL_LENGTH('Traducciones.Pantalla_Pagos', 'pagos_err_alumno') IS NULL
    ALTER TABLE [Traducciones].[Pantalla_Pagos] ADD [pagos_err_alumno] NVARCHAR(500) NOT NULL DEFAULT N'';
IF COL_LENGTH('Traducciones.Pantalla_Pagos', 'pagos_err_alumno_inactivo') IS NULL
    ALTER TABLE [Traducciones].[Pantalla_Pagos] ADD [pagos_err_alumno_inactivo] NVARCHAR(500) NOT NULL DEFAULT N'';
IF COL_LENGTH('Traducciones.Pantalla_Pagos', 'pagos_err_modalidad') IS NULL
    ALTER TABLE [Traducciones].[Pantalla_Pagos] ADD [pagos_err_modalidad] NVARCHAR(500) NOT NULL DEFAULT N'';
IF COL_LENGTH('Traducciones.Pantalla_Pagos', 'pagos_err_metodo') IS NULL
    ALTER TABLE [Traducciones].[Pantalla_Pagos] ADD [pagos_err_metodo] NVARCHAR(500) NOT NULL DEFAULT N'';
IF COL_LENGTH('Traducciones.Pantalla_Pagos', 'pagos_err_adelantado') IS NULL
    ALTER TABLE [Traducciones].[Pantalla_Pagos] ADD [pagos_err_adelantado] NVARCHAR(500) NOT NULL DEFAULT N'';
IF COL_LENGTH('Traducciones.Pantalla_Pagos', 'pagos_err_no_asociado') IS NULL
    ALTER TABLE [Traducciones].[Pantalla_Pagos] ADD [pagos_err_no_asociado] NVARCHAR(500) NOT NULL DEFAULT N'';
IF COL_LENGTH('Traducciones.Pantalla_Pagos', 'pagos_err_tarjeta_numero') IS NULL
    ALTER TABLE [Traducciones].[Pantalla_Pagos] ADD [pagos_err_tarjeta_numero] NVARCHAR(500) NOT NULL DEFAULT N'';
IF COL_LENGTH('Traducciones.Pantalla_Pagos', 'pagos_err_tarjeta_titular') IS NULL
    ALTER TABLE [Traducciones].[Pantalla_Pagos] ADD [pagos_err_tarjeta_titular] NVARCHAR(500) NOT NULL DEFAULT N'';
IF COL_LENGTH('Traducciones.Pantalla_Pagos', 'pagos_err_tarjeta_vence') IS NULL
    ALTER TABLE [Traducciones].[Pantalla_Pagos] ADD [pagos_err_tarjeta_vence] NVARCHAR(500) NOT NULL DEFAULT N'';
IF COL_LENGTH('Traducciones.Pantalla_Pagos', 'pagos_err_tarjeta_cvv') IS NULL
    ALTER TABLE [Traducciones].[Pantalla_Pagos] ADD [pagos_err_tarjeta_cvv] NVARCHAR(500) NOT NULL DEFAULT N'';
GO

UPDATE [Traducciones].[Pantalla_Pagos] SET
    [pagos_cliente_estado_msg] = CASE [IdiomaID] WHEN 1 THEN N'Estado del abono de tus alumnos.' WHEN 2 THEN N'Membership status for your students.' WHEN 3 THEN N'Situação da mensalidade dos seus alunos.' WHEN 4 THEN N'État de l''abonnement de vos élèves.' WHEN 5 THEN N'あなたの生徒の会費の状況。' ELSE N'Estado del abono de tus alumnos.' END,
    [pagos_cliente_sin_alumnos] = CASE [IdiomaID] WHEN 1 THEN N'No tenés alumnos asociados a tu cuenta.' WHEN 2 THEN N'You have no students linked to your account.' WHEN 3 THEN N'Você não tem alunos vinculados à sua conta.' WHEN 4 THEN N'Aucun élève n''est associé à votre compte.' WHEN 5 THEN N'アカウントに紐づく生徒はいません。' ELSE N'No tenés alumnos asociados a tu cuenta.' END,
    [pagos_btn_pagar] = CASE [IdiomaID] WHEN 1 THEN N'Pagar abono' WHEN 2 THEN N'Pay membership' WHEN 3 THEN N'Pagar mensalidade' WHEN 4 THEN N'Payer l''abonnement' WHEN 5 THEN N'会費を支払う' ELSE N'Pagar abono' END,
    [pagos_tarjeta_numero] = CASE [IdiomaID] WHEN 1 THEN N'Número de tarjeta' WHEN 2 THEN N'Card number' WHEN 3 THEN N'Número do cartão' WHEN 4 THEN N'Numéro de carte' WHEN 5 THEN N'カード番号' ELSE N'Número de tarjeta' END,
    [pagos_tarjeta_titular] = CASE [IdiomaID] WHEN 1 THEN N'Titular' WHEN 2 THEN N'Cardholder' WHEN 3 THEN N'Titular' WHEN 4 THEN N'Titulaire' WHEN 5 THEN N'名義人' ELSE N'Titular' END,
    [pagos_tarjeta_vence] = CASE [IdiomaID] WHEN 1 THEN N'Vencimiento (MM/AA)' WHEN 2 THEN N'Expiry (MM/YY)' WHEN 3 THEN N'Validade (MM/AA)' WHEN 4 THEN N'Expiration (MM/AA)' WHEN 5 THEN N'有効期限 (MM/YY)' ELSE N'Vencimiento (MM/AA)' END,
    [pagos_tarjeta_cvv] = CASE [IdiomaID] WHEN 1 THEN N'Código de seguridad' WHEN 2 THEN N'Security code' WHEN 3 THEN N'Código de segurança' WHEN 4 THEN N'Code de sécurité' WHEN 5 THEN N'セキュリティコード' ELSE N'Código de seguridad' END,
    [pagos_transferencia_datos] = CASE [IdiomaID] WHEN 1 THEN N'Transferí el monto al alias SPORTIO.GYM.PAGOS y confirmá el pago.' WHEN 2 THEN N'Transfer the amount to the alias SPORTIO.GYM.PAGOS and confirm the payment.' WHEN 3 THEN N'Transfira o valor para o alias SPORTIO.GYM.PAGOS e confirme o pagamento.' WHEN 4 THEN N'Virez le montant à l''alias SPORTIO.GYM.PAGOS et confirmez le paiement.' WHEN 5 THEN N'エイリアス SPORTIO.GYM.PAGOS に振り込み、支払いを確定してください。' ELSE N'Transferí el monto al alias SPORTIO.GYM.PAGOS y confirmá el pago.' END,
    [pagos_aviso_demo] = CASE [IdiomaID] WHEN 1 THEN N'Pago de demostración: no se hace ningún cobro real y los datos de la tarjeta no se guardan.' WHEN 2 THEN N'Demo payment: no real charge is made and card details are not stored.' WHEN 3 THEN N'Pagamento de demonstração: nenhuma cobrança real é feita e os dados do cartão não são salvos.' WHEN 4 THEN N'Paiement de démonstration : aucun débit réel n''est effectué et les données de la carte ne sont pas enregistrées.' WHEN 5 THEN N'デモ決済です。実際の請求は行われず、カード情報は保存されません。' ELSE N'Pago de demostración: no se hace ningún cobro real y los datos de la tarjeta no se guardan.' END,
    [pagos_btn_confirmar] = CASE [IdiomaID] WHEN 1 THEN N'Confirmar pago' WHEN 2 THEN N'Confirm payment' WHEN 3 THEN N'Confirmar pagamento' WHEN 4 THEN N'Confirmer le paiement' WHEN 5 THEN N'支払いを確定' ELSE N'Confirmar pago' END,
    [pagos_medio_tarjeta] = CASE [IdiomaID] WHEN 1 THEN N'Tarjeta' WHEN 2 THEN N'Card' WHEN 3 THEN N'Cartão' WHEN 4 THEN N'Carte' WHEN 5 THEN N'カード' ELSE N'Tarjeta' END,
    [pagos_medio_transferencia] = CASE [IdiomaID] WHEN 1 THEN N'Transferencia' WHEN 2 THEN N'Bank transfer' WHEN 3 THEN N'Transferência' WHEN 4 THEN N'Virement' WHEN 5 THEN N'振込' ELSE N'Transferencia' END,
    [pagos_estados_titulo] = CASE [IdiomaID] WHEN 1 THEN N'Estado de abonos' WHEN 2 THEN N'Membership status' WHEN 3 THEN N'Situação das mensalidades' WHEN 4 THEN N'État des abonnements' WHEN 5 THEN N'会費の状況' ELSE N'Estado de abonos' END,
    [pagos_buscar_placeholder] = CASE [IdiomaID] WHEN 1 THEN N'Buscar por nombre, apellido o DNI' WHEN 2 THEN N'Search by first name, last name or ID' WHEN 3 THEN N'Buscar por nome, sobrenome ou documento' WHEN 4 THEN N'Rechercher par prénom, nom ou pièce d''identité' WHEN 5 THEN N'名前、姓、またはIDで検索' ELSE N'Buscar por nombre, apellido o DNI' END,
    [pagos_btn_buscar] = CASE [IdiomaID] WHEN 1 THEN N'Buscar' WHEN 2 THEN N'Search' WHEN 3 THEN N'Buscar' WHEN 4 THEN N'Rechercher' WHEN 5 THEN N'検索' ELSE N'Buscar' END,
    [pagos_btn_cobrar] = CASE [IdiomaID] WHEN 1 THEN N'Cobrar' WHEN 2 THEN N'Collect' WHEN 3 THEN N'Cobrar' WHEN 4 THEN N'Encaisser' WHEN 5 THEN N'受領' ELSE N'Cobrar' END,
    [pagos_col_alumno] = CASE [IdiomaID] WHEN 1 THEN N'Alumno' WHEN 2 THEN N'Student' WHEN 3 THEN N'Aluno' WHEN 4 THEN N'Élève' WHEN 5 THEN N'生徒' ELSE N'Alumno' END,
    [pagos_col_dni] = CASE [IdiomaID] WHEN 1 THEN N'DNI' WHEN 2 THEN N'ID' WHEN 3 THEN N'Documento' WHEN 4 THEN N'Pièce d''identité' WHEN 5 THEN N'ID' ELSE N'DNI' END,
    [pagos_col_vence] = CASE [IdiomaID] WHEN 1 THEN N'Vence' WHEN 2 THEN N'Expires' WHEN 3 THEN N'Vence' WHEN 4 THEN N'Expire' WHEN 5 THEN N'有効期限' ELSE N'Vence' END,
    [pagos_col_dias] = CASE [IdiomaID] WHEN 1 THEN N'Días restantes' WHEN 2 THEN N'Days left' WHEN 3 THEN N'Dias restantes' WHEN 4 THEN N'Jours restants' WHEN 5 THEN N'残り日数' ELSE N'Días restantes' END,
    [pagos_col_estado] = CASE [IdiomaID] WHEN 1 THEN N'Estado' WHEN 2 THEN N'Status' WHEN 3 THEN N'Situação' WHEN 4 THEN N'État' WHEN 5 THEN N'状況' ELSE N'Estado' END,
    [pagos_col_desde] = CASE [IdiomaID] WHEN 1 THEN N'Desde' WHEN 2 THEN N'From' WHEN 3 THEN N'De' WHEN 4 THEN N'Du' WHEN 5 THEN N'開始' ELSE N'Desde' END,
    [pagos_col_hasta] = CASE [IdiomaID] WHEN 1 THEN N'Hasta' WHEN 2 THEN N'Until' WHEN 3 THEN N'Até' WHEN 4 THEN N'Au' WHEN 5 THEN N'終了' ELSE N'Hasta' END,
    [pagos_col_modalidad] = CASE [IdiomaID] WHEN 1 THEN N'Modalidad' WHEN 2 THEN N'Plan' WHEN 3 THEN N'Modalidade' WHEN 4 THEN N'Formule' WHEN 5 THEN N'プラン' ELSE N'Modalidad' END,
    [pagos_col_monto] = CASE [IdiomaID] WHEN 1 THEN N'Monto' WHEN 2 THEN N'Amount' WHEN 3 THEN N'Valor' WHEN 4 THEN N'Montant' WHEN 5 THEN N'金額' ELSE N'Monto' END,
    [pagos_col_fecha_pago] = CASE [IdiomaID] WHEN 1 THEN N'Fecha de pago' WHEN 2 THEN N'Payment date' WHEN 3 THEN N'Data do pagamento' WHEN 4 THEN N'Date de paiement' WHEN 5 THEN N'支払日' ELSE N'Fecha de pago' END,
    [pagos_col_metodo] = CASE [IdiomaID] WHEN 1 THEN N'Método' WHEN 2 THEN N'Method' WHEN 3 THEN N'Método' WHEN 4 THEN N'Mode' WHEN 5 THEN N'方法' ELSE N'Método' END,
    [pagos_monto_a_pagar] = CASE [IdiomaID] WHEN 1 THEN N'Monto a pagar' WHEN 2 THEN N'Amount due' WHEN 3 THEN N'Valor a pagar' WHEN 4 THEN N'Montant à payer' WHEN 5 THEN N'支払額' ELSE N'Monto a pagar' END,
    [pagos_sin_pagos] = CASE [IdiomaID] WHEN 1 THEN N'No hay pagos registrados todavía.' WHEN 2 THEN N'No payments recorded yet.' WHEN 3 THEN N'Ainda não há pagamentos registrados.' WHEN 4 THEN N'Aucun paiement enregistré pour le moment.' WHEN 5 THEN N'まだ支払いの記録はありません。' ELSE N'No hay pagos registrados todavía.' END,
    [pagos_sin_resultados] = CASE [IdiomaID] WHEN 1 THEN N'No se encontraron alumnos.' WHEN 2 THEN N'No students found.' WHEN 3 THEN N'Nenhum aluno encontrado.' WHEN 4 THEN N'Aucun élève trouvé.' WHEN 5 THEN N'生徒が見つかりません。' ELSE N'No se encontraron alumnos.' END,
    [pagos_ok] = CASE [IdiomaID] WHEN 1 THEN N'Pago registrado. El abono cubre del {0} al {1}.' WHEN 2 THEN N'Payment recorded. The membership covers {0} to {1}.' WHEN 3 THEN N'Pagamento registrado. A mensalidade cobre de {0} a {1}.' WHEN 4 THEN N'Paiement enregistré. L''abonnement couvre du {0} au {1}.' WHEN 5 THEN N'支払いを登録しました。期間は {0} から {1} までです。' ELSE N'Pago registrado. El abono cubre del {0} al {1}.' END,
    [pagos_cobertura] = CASE [IdiomaID] WHEN 1 THEN N'El abono cubre del {0} al {1}.' WHEN 2 THEN N'The membership covers {0} to {1}.' WHEN 3 THEN N'A mensalidade cobre de {0} a {1}.' WHEN 4 THEN N'L''abonnement couvre du {0} au {1}.' WHEN 5 THEN N'期間は {0} から {1} までです。' ELSE N'El abono cubre del {0} al {1}.' END,
    [pagos_estado_vigente] = CASE [IdiomaID] WHEN 1 THEN N'Al día' WHEN 2 THEN N'Up to date' WHEN 3 THEN N'Em dia' WHEN 4 THEN N'À jour' WHEN 5 THEN N'有効' ELSE N'Al día' END,
    [pagos_estado_por_vencer] = CASE [IdiomaID] WHEN 1 THEN N'Por vencer' WHEN 2 THEN N'Expiring soon' WHEN 3 THEN N'A vencer' WHEN 4 THEN N'Expire bientôt' WHEN 5 THEN N'まもなく期限切れ' ELSE N'Por vencer' END,
    [pagos_estado_vencido] = CASE [IdiomaID] WHEN 1 THEN N'Vencido' WHEN 2 THEN N'Expired' WHEN 3 THEN N'Vencida' WHEN 4 THEN N'Expiré' WHEN 5 THEN N'期限切れ' ELSE N'Vencido' END,
    [pagos_estado_sin_abono] = CASE [IdiomaID] WHEN 1 THEN N'Sin abono' WHEN 2 THEN N'No membership' WHEN 3 THEN N'Sem mensalidade' WHEN 4 THEN N'Sans abonnement' WHEN 5 THEN N'会費なし' ELSE N'Sin abono' END,
    [pagos_estado_inactivo] = CASE [IdiomaID] WHEN 1 THEN N'Dado de baja' WHEN 2 THEN N'Inactive' WHEN 3 THEN N'Inativo' WHEN 4 THEN N'Inactif' WHEN 5 THEN N'退会' ELSE N'Dado de baja' END,
    [pagos_aviso_inactivo] = CASE [IdiomaID] WHEN 1 THEN N'El alumno está dado de baja. Consultá en recepción.' WHEN 2 THEN N'The student is inactive. Please ask at the front desk.' WHEN 3 THEN N'O aluno está inativo. Consulte a recepção.' WHEN 4 THEN N'L''élève est inactif. Renseignez-vous à l''accueil.' WHEN 5 THEN N'この生徒は退会しています。受付にお問い合わせください。' ELSE N'El alumno está dado de baja. Consultá en recepción.' END,
    [pagos_aviso_adelantado] = CASE [IdiomaID] WHEN 1 THEN N'El próximo abono ya está pago.' WHEN 2 THEN N'The next membership is already paid.' WHEN 3 THEN N'A próxima mensalidade já está paga.' WHEN 4 THEN N'Le prochain abonnement est déjà payé.' WHEN 5 THEN N'次の会費は支払い済みです。' ELSE N'El próximo abono ya está pago.' END,
    [pagos_err_alumno] = CASE [IdiomaID] WHEN 1 THEN N'Debe seleccionar un alumno válido.' WHEN 2 THEN N'You must select a valid student.' WHEN 3 THEN N'Selecione um aluno válido.' WHEN 4 THEN N'Vous devez sélectionner un élève valide.' WHEN 5 THEN N'有効な生徒を選択してください。' ELSE N'Debe seleccionar un alumno válido.' END,
    [pagos_err_alumno_inactivo] = CASE [IdiomaID] WHEN 1 THEN N'El alumno está dado de baja: no se le puede cobrar el abono.' WHEN 2 THEN N'The student is inactive: the membership cannot be charged.' WHEN 3 THEN N'O aluno está inativo: não é possível cobrar a mensalidade.' WHEN 4 THEN N'L''élève est inactif : l''abonnement ne peut pas être encaissé.' WHEN 5 THEN N'この生徒は退会しているため、会費を受領できません。' ELSE N'El alumno está dado de baja: no se le puede cobrar el abono.' END,
    [pagos_err_modalidad] = CASE [IdiomaID] WHEN 1 THEN N'Debe seleccionar una modalidad válida.' WHEN 2 THEN N'You must select a valid plan.' WHEN 3 THEN N'Selecione uma modalidade válida.' WHEN 4 THEN N'Vous devez sélectionner une formule valide.' WHEN 5 THEN N'有効なプランを選択してください。' ELSE N'Debe seleccionar una modalidad válida.' END,
    [pagos_err_metodo] = CASE [IdiomaID] WHEN 1 THEN N'Debe seleccionar un medio de pago válido.' WHEN 2 THEN N'You must select a valid payment method.' WHEN 3 THEN N'Selecione um meio de pagamento válido.' WHEN 4 THEN N'Vous devez sélectionner un mode de paiement valide.' WHEN 5 THEN N'有効な支払い方法を選択してください。' ELSE N'Debe seleccionar un medio de pago válido.' END,
    [pagos_err_adelantado] = CASE [IdiomaID] WHEN 1 THEN N'El alumno ya tiene un abono pagado por adelantado (desde el {0}).' WHEN 2 THEN N'The student already has a membership paid in advance (from {0}).' WHEN 3 THEN N'O aluno já tem uma mensalidade paga antecipadamente (a partir de {0}).' WHEN 4 THEN N'L''élève a déjà un abonnement payé d''avance (à partir du {0}).' WHEN 5 THEN N'この生徒はすでに前払いの会費があります（{0} から）。' ELSE N'El alumno ya tiene un abono pagado por adelantado (desde el {0}).' END,
    [pagos_err_no_asociado] = CASE [IdiomaID] WHEN 1 THEN N'Ese alumno no está asociado a tu cuenta.' WHEN 2 THEN N'That student is not linked to your account.' WHEN 3 THEN N'Esse aluno não está vinculado à sua conta.' WHEN 4 THEN N'Cet élève n''est pas associé à votre compte.' WHEN 5 THEN N'その生徒はあなたのアカウントに紐づいていません。' ELSE N'Ese alumno no está asociado a tu cuenta.' END,
    [pagos_err_tarjeta_numero] = CASE [IdiomaID] WHEN 1 THEN N'El número de tarjeta no es válido.' WHEN 2 THEN N'The card number is not valid.' WHEN 3 THEN N'O número do cartão não é válido.' WHEN 4 THEN N'Le numéro de carte n''est pas valide.' WHEN 5 THEN N'カード番号が無効です。' ELSE N'El número de tarjeta no es válido.' END,
    [pagos_err_tarjeta_titular] = CASE [IdiomaID] WHEN 1 THEN N'Ingresá el nombre del titular.' WHEN 2 THEN N'Enter the cardholder name.' WHEN 3 THEN N'Informe o nome do titular.' WHEN 4 THEN N'Saisissez le nom du titulaire.' WHEN 5 THEN N'名義人を入力してください。' ELSE N'Ingresá el nombre del titular.' END,
    [pagos_err_tarjeta_vence] = CASE [IdiomaID] WHEN 1 THEN N'El vencimiento de la tarjeta no es válido o ya pasó.' WHEN 2 THEN N'The card expiry is not valid or has passed.' WHEN 3 THEN N'A validade do cartão não é válida ou já passou.' WHEN 4 THEN N'La date d''expiration n''est pas valide ou est dépassée.' WHEN 5 THEN N'カードの有効期限が無効か、すでに過ぎています。' ELSE N'El vencimiento de la tarjeta no es válido o ya pasó.' END,
    [pagos_err_tarjeta_cvv] = CASE [IdiomaID] WHEN 1 THEN N'El código de seguridad no es válido.' WHEN 2 THEN N'The security code is not valid.' WHEN 3 THEN N'O código de segurança não é válido.' WHEN 4 THEN N'Le code de sécurité n''est pas valide.' WHEN 5 THEN N'セキュリティコードが無効です。' ELSE N'El código de seguridad no es válido.' END;
GO

IF COL_LENGTH('Traducciones.Pantalla_DashboardContent', 'dash_abono_titulo') IS NULL
    ALTER TABLE [Traducciones].[Pantalla_DashboardContent] ADD [dash_abono_titulo] NVARCHAR(500) NOT NULL DEFAULT N'';
IF COL_LENGTH('Traducciones.Pantalla_DashboardContent', 'dash_abono_hoy') IS NULL
    ALTER TABLE [Traducciones].[Pantalla_DashboardContent] ADD [dash_abono_hoy] NVARCHAR(500) NOT NULL DEFAULT N'';
IF COL_LENGTH('Traducciones.Pantalla_DashboardContent', 'dash_abono_vence') IS NULL
    ALTER TABLE [Traducciones].[Pantalla_DashboardContent] ADD [dash_abono_vence] NVARCHAR(500) NOT NULL DEFAULT N'';
IF COL_LENGTH('Traducciones.Pantalla_DashboardContent', 'dash_abono_vencio') IS NULL
    ALTER TABLE [Traducciones].[Pantalla_DashboardContent] ADD [dash_abono_vencio] NVARCHAR(500) NOT NULL DEFAULT N'';
IF COL_LENGTH('Traducciones.Pantalla_DashboardContent', 'dash_abono_sin_pagos') IS NULL
    ALTER TABLE [Traducciones].[Pantalla_DashboardContent] ADD [dash_abono_sin_pagos] NVARCHAR(500) NOT NULL DEFAULT N'';
IF COL_LENGTH('Traducciones.Pantalla_DashboardContent', 'dash_abono_dias') IS NULL
    ALTER TABLE [Traducciones].[Pantalla_DashboardContent] ADD [dash_abono_dias] NVARCHAR(500) NOT NULL DEFAULT N'';
GO

UPDATE [Traducciones].[Pantalla_DashboardContent] SET
    [dash_abono_titulo] = CASE [IdiomaID] WHEN 1 THEN N'Mi abono' WHEN 2 THEN N'My membership' WHEN 3 THEN N'Minha mensalidade' WHEN 4 THEN N'Mon abonnement' WHEN 5 THEN N'会費' ELSE N'Mi abono' END,
    [dash_abono_hoy] = CASE [IdiomaID] WHEN 1 THEN N'Hoy: {0}' WHEN 2 THEN N'Today: {0}' WHEN 3 THEN N'Hoje: {0}' WHEN 4 THEN N'Aujourd''hui : {0}' WHEN 5 THEN N'今日: {0}' ELSE N'Hoy: {0}' END,
    [dash_abono_vence] = CASE [IdiomaID] WHEN 1 THEN N'Vence el {0}' WHEN 2 THEN N'Expires on {0}' WHEN 3 THEN N'Vence em {0}' WHEN 4 THEN N'Expire le {0}' WHEN 5 THEN N'有効期限 {0}' ELSE N'Vence el {0}' END,
    [dash_abono_vencio] = CASE [IdiomaID] WHEN 1 THEN N'Venció el {0}' WHEN 2 THEN N'Expired on {0}' WHEN 3 THEN N'Venceu em {0}' WHEN 4 THEN N'Expiré le {0}' WHEN 5 THEN N'{0} に期限切れ' ELSE N'Venció el {0}' END,
    [dash_abono_sin_pagos] = CASE [IdiomaID] WHEN 1 THEN N'Todavía no hay pagos registrados' WHEN 2 THEN N'No payments recorded yet' WHEN 3 THEN N'Ainda não há pagamentos registrados' WHEN 4 THEN N'Aucun paiement enregistré' WHEN 5 THEN N'まだ支払いの記録はありません' ELSE N'Todavía no hay pagos registrados' END,
    [dash_abono_dias] = CASE [IdiomaID] WHEN 1 THEN N'{0} días restantes' WHEN 2 THEN N'{0} days left' WHEN 3 THEN N'{0} dias restantes' WHEN 4 THEN N'{0} jours restants' WHEN 5 THEN N'残り {0} 日' ELSE N'{0} días restantes' END;
GO

PRINT 'Traducciones del abono mensual cargadas. Ahora: VerificacioDV -> Recalcular dígitos.';
GO
