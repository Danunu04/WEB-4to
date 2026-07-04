-- ============================================================================
-- Migración: crear tabla DigitoVerificador para control global DVH/DVV
-- ============================================================================

USE [GymApp];
GO

IF OBJECT_ID('[dbo].[DigitoVerificador]', 'U') IS NULL
BEGIN
    CREATE TABLE [dbo].[DigitoVerificador](
        [idDigitoVerificador]   INT             IDENTITY(1,1) NOT NULL,
        [nombreTabla]           VARCHAR(100)    NOT NULL,
        [dvhTabla]              VARCHAR(64)     NOT NULL,
        [dvvTabla]              VARCHAR(64)     NOT NULL,
        [fechaCalculo]          DATETIME        NOT NULL DEFAULT GETDATE(),
        CONSTRAINT [PK_DigitoVerificador] PRIMARY KEY CLUSTERED ([idDigitoVerificador] ASC),
        CONSTRAINT [UK_DigitoVerificador_NombreTabla] UNIQUE NONCLUSTERED ([nombreTabla] ASC)
    ) ON [PRIMARY];

    PRINT 'Tabla DigitoVerificador creada.';
END
ELSE
BEGIN
    PRINT 'Tabla DigitoVerificador ya existe.';
END
GO
