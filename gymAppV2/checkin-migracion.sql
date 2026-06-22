-- ============================================================================
-- Migración: Agregar campos de membresía a ALUMNOS para módulo Check-in
-- Ejecutar en SQL Server Management Studio contra la base [GymApp]
-- ============================================================================

USE [GymApp];
GO

-- diasRestantes: clases restantes del mes. NULL = modalidad diaria (ilimitada)
IF COL_LENGTH('dbo.ALUMNOS', 'diasRestantes') IS NULL
BEGIN
    ALTER TABLE [dbo].[ALUMNOS] ADD [diasRestantes] INT NULL;
    PRINT 'Columna diasRestantes agregada a ALUMNOS';
END
ELSE
    PRINT 'diasRestantes ya existe, se omite';
GO

-- fechaVencimiento: fecha límite de la membresía (30 días desde el pago)
IF COL_LENGTH('dbo.ALUMNOS', 'fechaVencimiento') IS NULL
BEGIN
    ALTER TABLE [dbo].[ALUMNOS] ADD [fechaVencimiento] DATE NULL;
    PRINT 'Columna fechaVencimiento agregada a ALUMNOS';
END
ELSE
    PRINT 'fechaVencimiento ya existe, se omite';
GO

-- idModalidad: referencia al tipo de cuota (PrecioModalidad)
IF COL_LENGTH('dbo.ALUMNOS', 'idModalidad') IS NULL
BEGIN
    ALTER TABLE [dbo].[ALUMNOS] ADD [idModalidad] INT NULL;

    -- FK solo si la columna se acaba de crear
    IF NOT EXISTS (
        SELECT 1 FROM sys.foreign_keys
        WHERE name = 'FK_ALUMNOS_PrecioModalidad'
    )
    BEGIN
        ALTER TABLE [dbo].[ALUMNOS]
            ADD CONSTRAINT [FK_ALUMNOS_PrecioModalidad]
            FOREIGN KEY ([idModalidad]) REFERENCES [dbo].[PrecioModalidad] ([Id]);
        PRINT 'FK_ALUMNOS_PrecioModalidad creada';
    END

    PRINT 'Columna idModalidad agregada a ALUMNOS';
END
ELSE
    PRINT 'idModalidad ya existe, se omite';
GO

PRINT '=== Migración check-in completada ===';
GO
