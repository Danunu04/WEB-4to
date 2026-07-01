-- =============================================================================
-- Script de verificación de seguridad — GymApp
-- Ejecutar antes del plan de pruebas manuales para confirmar que la BD tiene
-- los datos mínimos necesarios.
-- =============================================================================
USE [GymApp];
GO

PRINT '--- 1. Tabla de control DigitoVerificador ---';
SELECT
    nombreTabla AS Tabla,
    dvhTabla AS DVH_Tabla,
    dvvTabla AS DVV_Tabla,
    fechaCalculo AS FechaCalculo
FROM [dbo].[DigitoVerificador]
ORDER BY nombreTabla;

PRINT '--- 2. Cantidad de tablas con registros de control ---';
SELECT COUNT(*) AS TablasRegistradas
FROM [dbo].[DigitoVerificador];

PRINT '--- 3. Usuarios de prueba por rol ---';
SELECT
    u.usr AS Usuario,
    u.rol AS Rol,
    u.tipo AS Tipo,
    u.activo AS Activo,
    u.bloqueado AS Bloqueado,
    u.intentos AS Intentos,
    u.primerLogin AS PrimerLogin
FROM [dbo].[USUARIOS] u
WHERE u.rol IN (1, 2, 3, 4)
ORDER BY u.rol, u.usr;

PRINT '--- 4. Usuarios con datos de integridad vacíos ---';
SELECT
    u.usr AS Usuario,
    u.dvh AS DVH,
    u.dvv AS DVV
FROM [dbo].[USUARIOS] u
WHERE NULLIF(LTRIM(RTRIM(u.dvh)), '') IS NULL
   OR NULLIF(LTRIM(RTRIM(u.dvv)), '') IS NULL;

PRINT '--- 5. Preguntas de seguridad configuradas ---';
SELECT
    p.usr AS Usuario,
    LEN(p.pregunta) AS LargoPregunta,
    LEN(p.respuesta) AS LargoRespuesta
FROM [dbo].[PreguntasSeguridad] p;

PRINT '--- 6. Usuarios sin pregunta de seguridad ---';
SELECT
    u.usr AS Usuario
FROM [dbo].[USUARIOS] u
LEFT JOIN [dbo].[PreguntasSeguridad] p ON u.usr = p.usr
WHERE p.usr IS NULL;

PRINT '--- 7. Integridad básica: tablas principales con filas sin dvh/dvv ---';
SELECT 'USUARIOS' AS Tabla, COUNT(*) AS FilasVacias
FROM [dbo].[USUARIOS]
WHERE NULLIF(LTRIM(RTRIM(dvh)), '') IS NULL OR NULLIF(LTRIM(RTRIM(dvv)), '') IS NULL
UNION ALL
SELECT 'ALUMNOS', COUNT(*)
FROM [dbo].[ALUMNOS]
WHERE NULLIF(LTRIM(RTRIM(dvh)), '') IS NULL OR NULLIF(LTRIM(RTRIM(dvv)), '') IS NULL
UNION ALL
SELECT 'ENTRENADORES', COUNT(*)
FROM [dbo].[ENTRENADORES]
WHERE NULLIF(LTRIM(RTRIM(dvh)), '') IS NULL OR NULLIF(LTRIM(RTRIM(dvv)), '') IS NULL
UNION ALL
SELECT 'PreguntasSeguridad', COUNT(*)
FROM [dbo].[PreguntasSeguridad]
WHERE NULLIF(LTRIM(RTRIM(dvh)), '') IS NULL OR NULLIF(LTRIM(RTRIM(dvv)), '') IS NULL
UNION ALL
SELECT 'Evento', COUNT(*)
FROM [dbo].[Evento]
WHERE NULLIF(LTRIM(RTRIM(dvh)), '') IS NULL OR NULLIF(LTRIM(RTRIM(dvv)), '') IS NULL;

PRINT '--- Verificación completada ---';
GO
