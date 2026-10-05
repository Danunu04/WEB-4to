USE [GymApp]
GO

-- ============================================================================
-- Migración: modelo Usuario / Alumno / Familia
--
-- 1) Habilita el rol 6 = Familiar (madre/padre/tutor) en USUARIOS.rol.
--    De paso también habilita el 5 = WebMaster, que el código ya usa
--    (BE.PerfilesSistema.RolWebMaster) pero el CHECK constraint nunca permitió
--    persistir.
-- 2) Crea ALUMNOS_USUARIOS: tabla intermedia que reemplaza a la vieja columna
--    única ALUMNOS.usr. Un alumno puede tener varios usuarios vinculados
--    (ej. madre y padre) y un usuario puede estar vinculado a varios alumnos
--    (ej. varios hijos, o su propia cuenta si además se anota como alumno).
-- 3) Migra los vínculos existentes (ALUMNOS.usr) a la tabla nueva.
-- 4) Elimina la columna ALUMNOS.usr y su FK: la relación pasa a vivir
--    100% en ALUMNOS_USUARIOS, para no tener dos fuentes de verdad.
--
-- IMPORTANTE: hacer un backup antes de correr este script (paso 4 es
-- destructivo). Después de aplicarlo, entrar a VerificacioDV y:
--   - "Recalcular dígitos" (los dvh insertados en el paso 3 quedan en blanco)
--   - "Inicializar control" para que ALUMNOS_USUARIOS quede bajo control de
--     integridad (se detecta automáticamente por tener columna dvh).
-- ============================================================================

-- 1) Habilitar roles 5 (WebMaster) y 6 (Familiar)
IF EXISTS (SELECT 1 FROM sys.check_constraints WHERE name = 'CK_USUARIOS_Rol')
BEGIN
    ALTER TABLE [dbo].[USUARIOS] DROP CONSTRAINT [CK_USUARIOS_Rol];
END
GO

ALTER TABLE [dbo].[USUARIOS] WITH CHECK ADD CONSTRAINT [CK_USUARIOS_Rol]
    CHECK ([rol] IN (1, 2, 3, 4, 5, 6));
GO

-- 2) Crear tabla intermedia ALUMNOS_USUARIOS
IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'ALUMNOS_USUARIOS' AND schema_id = SCHEMA_ID('dbo'))
BEGIN
    CREATE TABLE [dbo].[ALUMNOS_USUARIOS](
        [dniAlumno]         INT             NOT NULL,
        [usr]               VARCHAR(50)     NOT NULL,
        [parentesco]        VARCHAR(50)     NULL,
        [fechaAsociacion]   DATETIME        NOT NULL DEFAULT GETDATE(),
        [dvh]               VARCHAR(64)     NOT NULL,
        CONSTRAINT [PK_ALUMNOS_USUARIOS] PRIMARY KEY CLUSTERED ([dniAlumno] ASC, [usr] ASC),
        CONSTRAINT [FK_ALUMNOS_USUARIOS_Alumno] FOREIGN KEY ([dniAlumno])
            REFERENCES [dbo].[ALUMNOS] ([dni]),
        CONSTRAINT [FK_ALUMNOS_USUARIOS_Usuario] FOREIGN KEY ([usr])
            REFERENCES [dbo].[USUARIOS] ([usr])
    ) ON [PRIMARY];
END
GO

-- 3) Migrar los vínculos existentes (1 alumno : 1 usuario) a la tabla nueva.
--    El dvh se recalcula desde la app (VerificacioDV > Recalcular dígitos);
--    acá queda vacío a propósito para no duplicar el algoritmo de hash en SQL.
IF EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('dbo.ALUMNOS') AND name = 'usr')
BEGIN
    INSERT INTO [dbo].[ALUMNOS_USUARIOS] (dniAlumno, usr, parentesco, fechaAsociacion, dvh)
    SELECT a.[dni], a.[usr], N'Titular', GETDATE(), ''
    FROM [dbo].[ALUMNOS] a
    WHERE a.[usr] IS NOT NULL AND a.[usr] <> ''
      AND NOT EXISTS (
          SELECT 1 FROM [dbo].[ALUMNOS_USUARIOS] au
          WHERE au.[dniAlumno] = a.[dni] AND au.[usr] = a.[usr]
      );
END
GO

-- 4) Eliminar la columna ALUMNOS.usr y su FK (verificar el nombre real de la
--    constraint si este script falla acá: SELECT name FROM sys.foreign_keys
--    WHERE parent_object_id = OBJECT_ID('dbo.ALUMNOS')).
IF EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = 'FK_ALUMNOS_Usr_USUARIOS')
BEGIN
    ALTER TABLE [dbo].[ALUMNOS] DROP CONSTRAINT [FK_ALUMNOS_Usr_USUARIOS];
END
GO

IF EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('dbo.ALUMNOS') AND name = 'usr')
BEGIN
    ALTER TABLE [dbo].[ALUMNOS] DROP COLUMN [usr];
END
GO
