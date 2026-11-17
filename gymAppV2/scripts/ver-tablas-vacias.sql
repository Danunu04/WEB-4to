-- Lista todas las tablas de GymApp con su cantidad de filas,
-- primero las vacías. Solo lectura, no modifica nada.
USE GymApp;
GO

SELECT
    s.name                                   AS Esquema,
    t.name                                   AS Tabla,
    SUM(p.rows)                              AS Filas,
    CASE WHEN SUM(p.rows) = 0 THEN 'VACIA' ELSE 'con datos' END AS Estado
FROM sys.tables t
JOIN sys.schemas s     ON s.schema_id = t.schema_id
JOIN sys.partitions p  ON p.object_id = t.object_id
                      AND p.index_id IN (0, 1)   -- heap o índice clustered
WHERE t.is_ms_shipped = 0
GROUP BY s.name, t.name
ORDER BY Filas, s.name, t.name;
