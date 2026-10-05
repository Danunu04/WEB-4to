USE [GymApp]
GO

-- ============================================================
-- Traducciones nuevas para el módulo real de Pagos (alta + historial).
-- Amplía Traducciones.Pantalla_Pagos, creada como placeholder en
-- scripts/agregar-traducciones-entrenadores-rutinas-pagos-permisos.sql.
-- ============================================================

IF COL_LENGTH('Traducciones.Pantalla_Pagos', 'pagos_cliente_msg') IS NULL
    ALTER TABLE [Traducciones].[Pantalla_Pagos]
        ADD [pagos_cliente_msg]          NVARCHAR(500) NOT NULL DEFAULT N'',
            [pagos_lista_cliente_titulo] NVARCHAR(500) NOT NULL DEFAULT N'',
            [pagos_lista_titulo]         NVARCHAR(500) NOT NULL DEFAULT N'',
            [pagos_btn_registrar]        NVARCHAR(500) NOT NULL DEFAULT N'',
            [pagos_form_titulo]          NVARCHAR(500) NOT NULL DEFAULT N'',
            [pagos_campo_alumno]         NVARCHAR(500) NOT NULL DEFAULT N'',
            [pagos_campo_modalidad]      NVARCHAR(500) NOT NULL DEFAULT N'',
            [pagos_campo_periodo]        NVARCHAR(500) NOT NULL DEFAULT N'',
            [pagos_campo_periodo_hint]   NVARCHAR(500) NOT NULL DEFAULT N'',
            [pagos_campo_metodo]         NVARCHAR(500) NOT NULL DEFAULT N'',
            [pagos_campo_monto]          NVARCHAR(500) NOT NULL DEFAULT N'';
GO

UPDATE [Traducciones].[Pantalla_Pagos] SET
    [pagos_cliente_msg]          = CASE [IdiomaID] WHEN 1 THEN N'Historial de pagos de tus alumnos.'          WHEN 2 THEN N'Payment history for your students.'          WHEN 3 THEN N'Histórico de pagamentos dos seus alunos.'      WHEN 4 THEN N'Historique des paiements de vos élèves.'         WHEN 5 THEN N'あなたの生徒の支払い履歴。'    END,
    [pagos_lista_cliente_titulo] = CASE [IdiomaID] WHEN 1 THEN N'Mis pagos'                                    WHEN 2 THEN N'My payments'                                  WHEN 3 THEN N'Meus pagamentos'                                WHEN 4 THEN N'Mes paiements'                                    WHEN 5 THEN N'マイ支払い'          END,
    [pagos_lista_titulo]         = CASE [IdiomaID] WHEN 1 THEN N'Historial de pagos'                           WHEN 2 THEN N'Payment history'                              WHEN 3 THEN N'Histórico de pagamentos'                        WHEN 4 THEN N'Historique des paiements'                         WHEN 5 THEN N'支払い履歴'          END,
    [pagos_btn_registrar]        = CASE [IdiomaID] WHEN 1 THEN N'Registrar pago'                               WHEN 2 THEN N'Register payment'                             WHEN 3 THEN N'Registrar pagamento'                            WHEN 4 THEN N'Enregistrer un paiement'                          WHEN 5 THEN N'支払いを登録'        END,
    [pagos_form_titulo]          = CASE [IdiomaID] WHEN 1 THEN N'Registrar pago'                               WHEN 2 THEN N'Register payment'                             WHEN 3 THEN N'Registrar pagamento'                            WHEN 4 THEN N'Enregistrer un paiement'                          WHEN 5 THEN N'支払いを登録'        END,
    [pagos_campo_alumno]         = CASE [IdiomaID] WHEN 1 THEN N'Alumno'                                       WHEN 2 THEN N'Student'                                      WHEN 3 THEN N'Aluno'                                          WHEN 4 THEN N'Élève'                                            WHEN 5 THEN N'生徒'                END,
    [pagos_campo_modalidad]      = CASE [IdiomaID] WHEN 1 THEN N'Modalidad'                                    WHEN 2 THEN N'Plan'                                         WHEN 3 THEN N'Modalidade'                                     WHEN 4 THEN N'Formule'                                          WHEN 5 THEN N'プラン'              END,
    [pagos_campo_periodo]        = CASE [IdiomaID] WHEN 1 THEN N'Período (mes)'                                WHEN 2 THEN N'Period (month)'                               WHEN 3 THEN N'Período (mês)'                                  WHEN 4 THEN N'Période (mois)'                                   WHEN 5 THEN N'期間（月）'          END,
    [pagos_campo_periodo_hint]   = CASE [IdiomaID] WHEN 1 THEN N'Se toma el mes de la fecha elegida.'          WHEN 2 THEN N'The month of the chosen date will be used.'   WHEN 3 THEN N'Será considerado o mês da data escolhida.'      WHEN 4 THEN N'Le mois de la date choisie sera utilisé.'         WHEN 5 THEN N'選択した日付の月が使用されます。' END,
    [pagos_campo_metodo]         = CASE [IdiomaID] WHEN 1 THEN N'Método de pago'                               WHEN 2 THEN N'Payment method'                               WHEN 3 THEN N'Método de pagamento'                            WHEN 4 THEN N'Mode de paiement'                                 WHEN 5 THEN N'支払い方法'          END,
    [pagos_campo_monto]          = CASE [IdiomaID] WHEN 1 THEN N'Monto'                                        WHEN 2 THEN N'Amount'                                       WHEN 3 THEN N'Valor'                                          WHEN 4 THEN N'Montant'                                          WHEN 5 THEN N'金額'                END;
GO
