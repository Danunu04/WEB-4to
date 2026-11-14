USE [GymApp]
GO

-- ============================================================
-- El perfil WebMaster deja de tener "todos los permisos siempre" por código:
-- ahora sus permisos se guardan en la base y se editan como los de cualquier perfil.
--
-- Hasta ahora el WebMaster no tenía filas en PerfilPermiso ni en PerfilFamilia. Este script
-- le carga lo mismo que tiene el Administrador (sus familias y permisos sueltos) y, como
-- permiso suelto, cualquier permiso que no le llegue por ese camino (ej. Perfil).
-- Así el WebMaster arranca con todas las pantallas, igual que antes.
--
-- Solo actúa si el WebMaster todavía no tiene permisos ni familias: se puede correr más de una vez.
--
-- ANTES de correrlo: hacer un backup de la base.
-- Correrlo ANTES de usar el código nuevo: sin estas filas, con el código nuevo el WebMaster
-- queda sin ninguna pantalla. El código viejo las ignora (le daba todo por código).
-- DESPUÉS de correrlo: VerificacioDV -> "Recalcular dígitos".
-- ============================================================

SET XACT_ABORT ON;
BEGIN TRANSACTION;

DECLARE @idWebMaster INT = (SELECT idPerfil FROM [dbo].[Perfil] WHERE codigo = 'WEBMASTER');
DECLARE @idAdmin INT = (SELECT idPerfil FROM [dbo].[Perfil] WHERE codigo = 'ADMIN');

IF @idWebMaster IS NULL
    PRINT 'No existe el perfil WEBMASTER: correr antes crear-permisos-composite.sql.';
ELSE IF EXISTS (SELECT 1 FROM [dbo].[PerfilPermiso] WHERE idPerfil = @idWebMaster)
     OR EXISTS (SELECT 1 FROM [dbo].[PerfilFamilia] WHERE idPerfil = @idWebMaster)
    PRINT 'El WebMaster ya tiene permisos cargados: no se cambia nada.';
ELSE
BEGIN
    -- 1) Las mismas familias y permisos sueltos que el Administrador
    INSERT INTO [dbo].[PerfilFamilia] ([idPerfil], [idFamilia], [dvh])
    SELECT @idWebMaster, pf.idFamilia, ''
    FROM [dbo].[PerfilFamilia] pf
    WHERE pf.idPerfil = @idAdmin;

    INSERT INTO [dbo].[PerfilPermiso] ([idPerfil], [idPermiso], [dvh])
    SELECT @idWebMaster, pp.idPermiso, ''
    FROM [dbo].[PerfilPermiso] pp
    WHERE pp.idPerfil = @idAdmin;

    -- 2) Los permisos que todavía no le llegan (ni sueltos ni por alguna familia o subfamilia)
    ;WITH Familias AS (
        SELECT idFamilia FROM [dbo].[PerfilFamilia] WHERE idPerfil = @idWebMaster
        UNION ALL
        SELECT ff.idFamiliaHija
        FROM [dbo].[FamiliaFamilia] ff
        JOIN Familias f ON f.idFamilia = ff.idFamiliaPadre
    ),
    Alcanzados AS (
        SELECT fp.idPermiso FROM [dbo].[FamiliaPermiso] fp WHERE fp.idFamilia IN (SELECT idFamilia FROM Familias)
        UNION
        SELECT pp.idPermiso FROM [dbo].[PerfilPermiso] pp WHERE pp.idPerfil = @idWebMaster
    )
    INSERT INTO [dbo].[PerfilPermiso] ([idPerfil], [idPermiso], [dvh])
    SELECT @idWebMaster, p.idPermiso, ''
    FROM [dbo].[Permiso] p
    WHERE p.idPermiso NOT IN (SELECT idPermiso FROM Alcanzados);

    PRINT 'Permisos del WebMaster cargados.';
END

COMMIT TRANSACTION;
GO

-- ---------- Resultado: permisos que le faltan al WebMaster (tiene que dar vacío) ----------
;WITH Familias AS (
    SELECT pf.idFamilia
    FROM [dbo].[PerfilFamilia] pf
    JOIN [dbo].[Perfil] pe ON pe.idPerfil = pf.idPerfil AND pe.codigo = 'WEBMASTER'
    UNION ALL
    SELECT ff.idFamiliaHija
    FROM [dbo].[FamiliaFamilia] ff
    JOIN Familias f ON f.idFamilia = ff.idFamiliaPadre
),
Alcanzados AS (
    SELECT fp.idPermiso FROM [dbo].[FamiliaPermiso] fp WHERE fp.idFamilia IN (SELECT idFamilia FROM Familias)
    UNION
    SELECT pp.idPermiso
    FROM [dbo].[PerfilPermiso] pp
    JOIN [dbo].[Perfil] pe ON pe.idPerfil = pp.idPerfil AND pe.codigo = 'WEBMASTER'
)
SELECT p.nombre AS PermisoFaltante
FROM [dbo].[Permiso] p
WHERE p.idPermiso NOT IN (SELECT idPermiso FROM Alcanzados);
GO
