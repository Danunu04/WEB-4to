USE [GymApp]
GO

-- ============================================================
-- TABLA: Pagos (cuota mensual de un alumno, según su modalidad)
-- Sigue la convención post-migración de dígito verificador:
-- solo dvh, sin dvv (ver scripts/migration-simplificar-dv.sql).
-- ============================================================

IF OBJECT_ID('dbo.Pagos', 'U') IS NOT NULL
BEGIN
    PRINT 'La tabla dbo.Pagos ya existe, no se recrea.';
END
ELSE
BEGIN
    CREATE TABLE [dbo].[Pagos](
        [codPago]          INT             IDENTITY(1,1) NOT NULL,
        [dni]              INT             NOT NULL,
        [modalidadId]      INT             NOT NULL,
        [periodo]          DATE            NOT NULL,
        [monto]            DECIMAL(10,2)   NOT NULL,
        [fechaPago]        DATETIME        NOT NULL,
        [metodoPago]       VARCHAR(30)     NOT NULL,
        [usuarioRegistro]  VARCHAR(50)     NOT NULL,
        [dvh]              VARCHAR(64)     NOT NULL,
        CONSTRAINT [PK_Pagos] PRIMARY KEY CLUSTERED ([codPago] ASC),
        CONSTRAINT [FK_Pagos_Alumno] FOREIGN KEY ([dni]) REFERENCES [dbo].[ALUMNOS] ([dni]),
        CONSTRAINT [FK_Pagos_Modalidad] FOREIGN KEY ([modalidadId]) REFERENCES [dbo].[PrecioModalidad] ([Id]),
        CONSTRAINT [UQ_Pagos_AlumnoPeriodo] UNIQUE ([dni], [periodo]),
        CONSTRAINT [CK_Pagos_Monto] CHECK (([monto] > 0)),
        CONSTRAINT [CK_Pagos_MetodoPago] CHECK ([metodoPago] IN ('Efectivo', 'Transferencia', 'Tarjeta'))
    ) ON [PRIMARY];

    CREATE NONCLUSTERED INDEX [IX_Pagos_dni] ON [dbo].[Pagos] ([dni] ASC);

    PRINT 'Tabla dbo.Pagos creada correctamente.';
END
GO
