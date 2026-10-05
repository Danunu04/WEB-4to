USE [GymApp]
GO

-- ============================================================
-- Permisos por rol, editables desde la pantalla Permisos.
-- Reemplaza el switch fijo de BLLRol.TieneAccesoAModulo (que queda solo como
-- valor por defecto si la tabla no existe o un permiso no tiene filas).
--
-- Roles: 1 Administrador, 2 Recepcionista, 3 Entrenador, 4 Cliente,
--        5 WebMaster (siempre tiene todo, no se guarda), 6 Familiar.
--
-- La carga inicial replica exactamente lo que hoy está en el código.
--
-- Después de correr este script:
--   VerificacioDV -> "Inicializar control" y luego "Recalcular dígitos"
--   (las filas iniciales se insertan con dvh vacío).
-- ============================================================

IF OBJECT_ID('[dbo].[RolPermiso]', 'U') IS NULL
BEGIN
    CREATE TABLE [dbo].[RolPermiso](
        [rol]      INT          NOT NULL,
        [permiso]  VARCHAR(50)  NOT NULL,
        [dvh]      VARCHAR(64)  NOT NULL,
        CONSTRAINT [PK_RolPermiso] PRIMARY KEY CLUSTERED ([rol] ASC, [permiso] ASC),
        CONSTRAINT [CK_RolPermiso_Rol] CHECK ([rol] IN (1, 2, 3, 4, 6))
    ) ON [PRIMARY];

    PRINT 'Tabla RolPermiso creada.';
END
GO

IF NOT EXISTS (SELECT 1 FROM [dbo].[RolPermiso])
BEGIN
    INSERT INTO [dbo].[RolPermiso] ([rol], [permiso], [dvh])
    SELECT p.rol, p.permiso, ''
    FROM (VALUES
        -- Todos
        (1, 'Dashboard'), (2, 'Dashboard'), (3, 'Dashboard'), (4, 'Dashboard'), (6, 'Dashboard'),
        -- Cliente y Familiar
        (4, 'Perfil'), (6, 'Perfil'),
        -- Admin y Recepcionista
        (1, 'GestionUsuarios'), (2, 'GestionUsuarios'),
        (1, 'GestionEntrenadores'), (2, 'GestionEntrenadores'),
        (1, 'Bitacora'), (2, 'Bitacora'),
        (1, 'PreciosCuota'), (2, 'PreciosCuota'),
        (1, 'GestionActividades'), (2, 'GestionActividades'),
        -- Admin, Recepcionista, Cliente y Familiar
        (1, 'GestionAlumnos'), (2, 'GestionAlumnos'), (6, 'GestionAlumnos'),
        -- Todos excepto Entrenador
        (1, 'ActividadesCalendario'), (2, 'ActividadesCalendario'), (4, 'ActividadesCalendario'), (6, 'ActividadesCalendario'),
        (1, 'Pagos'), (2, 'Pagos'), (4, 'Pagos'), (6, 'Pagos'),
        -- Admin, Recepcionista y Entrenador
        (1, 'GestionRutinas'), (2, 'GestionRutinas'), (3, 'GestionRutinas'),
        -- Solo Administrador
        (1, 'VerificacionDV'), (1, 'Backup'), (1, 'Restore'), (1, 'RecalcularDV'),
        (1, 'EncriptarDatos'), (1, 'GestionPermisos')
    ) AS p(rol, permiso);

    PRINT 'Permisos iniciales cargados.';
END
GO
