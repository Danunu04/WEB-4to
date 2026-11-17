USE [GymApp]
GO

-- ============================================================================
-- Usuarios incompletos (17/11/2026)
--
-- SOLO LECTURA: no modifica nada.
--
-- Devuelve 3 resultados:
--   1. Todos los usuarios, con "Completo" o "Incompleto" y la lista de lo que le falta a cada uno.
--   2. Alumnos que no tienen ningún usuario con su DNI (sin nombre ni datos personales).
--   3. Resumen: cuántos usuarios completos e incompletos hay.
--
-- Los datos personales están encriptados, así que solo se puede ver si están vacíos o no
-- (el contenido se ve desde la pantalla Usuarios). La app guarda NULL cuando un dato queda vacío.
--
-- Qué se controla:
--   - Todos: nombre, apellido, email, teléfono, fecha de nacimiento y perfil de permisos asignado.
--   - Entrenador (3): que tenga su fila en Entrenadores.
--   - Cliente (4): que tenga su fila en Alumnos y que esté vinculado a su alumno.
--   - Familiar (6): que tenga al menos un alumno vinculado.
--   - Pregunta de seguridad: si ya pasó el primer inicio de sesión y no la tiene.
--     (Si todavía no inició sesión, la configura en ese momento: no cuenta como faltante.)
-- ============================================================================

;WITH Usuarios AS (
    SELECT
        u.usr,
        u.dni,
        u.rol,
        CASE u.rol
            WHEN 1 THEN 'Administrador'
            WHEN 2 THEN 'Recepcionista'
            WHEN 3 THEN 'Entrenador'
            WHEN 4 THEN 'Cliente'
            WHEN 5 THEN 'WebMaster'
            WHEN 6 THEN 'Familiar'
            ELSE 'Rol ' + CAST(u.rol AS VARCHAR(10))
        END AS tipoCuenta,
        u.activo,
        u.primerLogin,
        CONCAT_WS(', ',
            CASE WHEN NULLIF(LTRIM(RTRIM(u.nombre)), '') IS NULL THEN 'nombre' END,
            CASE WHEN NULLIF(LTRIM(RTRIM(u.apellido)), '') IS NULL THEN 'apellido' END,
            CASE WHEN NULLIF(LTRIM(RTRIM(u.email)), '') IS NULL THEN 'email' END,
            CASE WHEN NULLIF(LTRIM(RTRIM(u.telefono)), '') IS NULL THEN 'teléfono' END,
            CASE WHEN NULLIF(LTRIM(RTRIM(u.fechaNacimiento)), '') IS NULL THEN 'fecha de nacimiento' END,
            CASE WHEN up.usr IS NULL THEN 'perfil de permisos' END,
            CASE WHEN u.rol = 3 AND e.dni IS NULL THEN 'ficha de entrenador' END,
            CASE WHEN u.rol = 4 AND a.dni IS NULL THEN 'ficha de alumno' END,
            CASE WHEN u.rol = 4 AND a.dni IS NOT NULL AND propio.usr IS NULL THEN 'vínculo con su alumno' END,
            CASE WHEN u.rol = 6 AND vinculos.cantidad IS NULL THEN 'alumno vinculado' END,
            CASE WHEN u.primerLogin = 0 AND ps.usr IS NULL THEN 'pregunta de seguridad' END
        ) AS faltantes
    FROM [dbo].[USUARIOS] u
    LEFT JOIN [dbo].[UsuarioPerfil] up ON up.usr = u.usr
    LEFT JOIN [dbo].[Entrenadores] e ON e.dni = u.dni
    LEFT JOIN [dbo].[Alumnos] a ON a.dni = u.dni
    LEFT JOIN [dbo].[ALUMNOS_USUARIOS] propio ON propio.usr = u.usr AND propio.dniAlumno = u.dni
    OUTER APPLY (
        SELECT COUNT(*) AS cantidad FROM [dbo].[ALUMNOS_USUARIOS] au WHERE au.usr = u.usr HAVING COUNT(*) > 0
    ) vinculos
    OUTER APPLY (
        SELECT TOP 1 p.usr FROM [dbo].[PreguntasSeguridad] p WHERE p.usr = u.usr
    ) ps
)
SELECT
    usr AS usuario,
    dni,
    tipoCuenta,
    CASE WHEN activo = 1 THEN 'Activo' ELSE 'Inactivo' END AS estado,
    CASE WHEN faltantes = '' THEN 'Completo' ELSE 'Incompleto' END AS resultado,
    CASE WHEN faltantes = '' THEN '—' ELSE faltantes END AS leFalta
FROM Usuarios
ORDER BY
    CASE WHEN faltantes = '' THEN 1 ELSE 0 END,   -- primero los incompletos
    rol,
    usr;

-- 2. Alumnos sin ningún usuario con su DNI (no tienen nombre ni datos personales)
SELECT
    a.dni,
    CASE WHEN a.activo = 1 THEN 'Activo' ELSE 'Inactivo' END AS estado,
    (SELECT STRING_AGG(au.usr, ', ') FROM [dbo].[ALUMNOS_USUARIOS] au WHERE au.dniAlumno = a.dni) AS cuentasVinculadas
FROM [dbo].[Alumnos] a
WHERE NOT EXISTS (SELECT 1 FROM [dbo].[USUARIOS] u WHERE u.dni = a.dni)
ORDER BY a.dni;

-- 3. Resumen
;WITH Faltantes AS (
    SELECT
        u.usr,
        CASE WHEN NULLIF(LTRIM(RTRIM(u.nombre)), '') IS NULL
               OR NULLIF(LTRIM(RTRIM(u.apellido)), '') IS NULL
               OR NULLIF(LTRIM(RTRIM(u.email)), '') IS NULL
               OR NULLIF(LTRIM(RTRIM(u.telefono)), '') IS NULL
               OR NULLIF(LTRIM(RTRIM(u.fechaNacimiento)), '') IS NULL
               OR NOT EXISTS (SELECT 1 FROM [dbo].[UsuarioPerfil] up WHERE up.usr = u.usr)
               OR (u.rol = 3 AND NOT EXISTS (SELECT 1 FROM [dbo].[Entrenadores] e WHERE e.dni = u.dni))
               OR (u.rol = 4 AND NOT EXISTS (SELECT 1 FROM [dbo].[Alumnos] a WHERE a.dni = u.dni))
               OR (u.rol = 4 AND EXISTS (SELECT 1 FROM [dbo].[Alumnos] a WHERE a.dni = u.dni)
                             AND NOT EXISTS (SELECT 1 FROM [dbo].[ALUMNOS_USUARIOS] au WHERE au.usr = u.usr AND au.dniAlumno = u.dni))
               OR (u.rol = 6 AND NOT EXISTS (SELECT 1 FROM [dbo].[ALUMNOS_USUARIOS] au WHERE au.usr = u.usr))
               OR (u.primerLogin = 0 AND NOT EXISTS (SELECT 1 FROM [dbo].[PreguntasSeguridad] p WHERE p.usr = u.usr))
             THEN 1 ELSE 0 END AS incompleto
    FROM [dbo].[USUARIOS] u
)
SELECT
    COUNT(*) AS totalUsuarios,
    SUM(CASE WHEN incompleto = 0 THEN 1 ELSE 0 END) AS completos,
    SUM(incompleto) AS incompletos
FROM Faltantes;
GO
