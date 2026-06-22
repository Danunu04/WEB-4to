# Bitácora - Estructura de Base de Datos

## Tabla: Bitacora

```sql
CREATE TABLE Bitacora (
    id INT IDENTITY(1,1) PRIMARY KEY,
    tipo VARCHAR(50) NOT NULL,        -- login, logout, backup, new_user, update, error
    usuario VARCHAR(100) NOT NULL,
    accion VARCHAR(500) NOT NULL,
    timestamp DATETIME NOT NULL DEFAULT GETDATE(),
    
    -- Integridad (siguiendo el patrón existente del proyecto)
    dvv VARCHAR(32),
    dvh VARCHAR(32)
);
```

## Índices

```sql
-- Índice por tipo para filtros rápidos
CREATE INDEX IX_Bitacora_Tipo ON Bitacora(tipo);

-- Índice por usuario para búsquedas
CREATE INDEX IX_Bitacora_Usuario ON Bitacora(usuario);

-- Índice por timestamp descendente para ordenar por fecha
CREATE INDEX IX_Bitacora_Timestamp ON Bitacora(timestamp DESC);
```

## Tipos de Eventos

| Tipo           | Descripción                  |
|----------------|------------------------------|
| `login`        | Inicio de sesión             |
| `logout`       | Cierre de sesión             |
| `backup`       | Respaldo de base de datos    |
| `new_user`     | Usuario nuevo registrado     |
| `update`       | Actualización del sistema    |
| `error`        | Error del sistema            |

## Consultas SQL

### Obtener Eventos (con filtros)

```sql
SELECT id, tipo, usuario, accion, timestamp
FROM Bitacora
WHERE (@filtro = 'all' OR tipo = @filtro)
AND (@busqueda = '' OR usuario LIKE '%' + @busqueda + '%' OR accion LIKE '%' + @busqueda + '%')
ORDER BY timestamp DESC
```

**Parámetros:**
- `@filtro` (VARCHAR) - Tipo de evento o 'all' para todos
- `@busqueda` (VARCHAR) - Texto a buscar en usuario o acción

### Obtener Estadísticas

```sql
SELECT 
    COUNT(*) as Total,
    SUM(CASE WHEN tipo = 'login' THEN 1 ELSE 0 END) as Logins,
    SUM(CASE WHEN tipo = 'new_user' THEN 1 ELSE 0 END) as UsuariosNuevos,
    SUM(CASE WHEN tipo = 'error' THEN 1 ELSE 0 END) as Errores
FROM Bitacora
```

## Ejemplos de Insertar Eventos

```sql
-- Login exitoso
INSERT INTO Bitacora (tipo, usuario, accion)
VALUES ('login', 'juan.perez', 'Inicio de sesión exitoso desde IP 192.168.1.100');

-- Logout
INSERT INTO Bitacora (tipo, usuario, accion)
VALUES ('logout', 'juan.perez', 'Cierre de sesión normal');

-- Backup completado
INSERT INTO Bitacora (tipo, usuario, accion)
VALUES ('backup', 'admin', 'Backup completado exitosamente');

-- Usuario nuevo
INSERT INTO Bitacora (tipo, usuario, accion)
VALUES ('new_user', 'maria.garcia', 'Usuario registrado: maria.garcia');

-- Actualización
INSERT INTO Bitacora (tipo, usuario, accion)
VALUES ('update', 'admin', 'Sistema actualizado a versión 2.4.1');

-- Error
INSERT INTO Bitacora (tipo, usuario, accion)
VALUES ('error', 'system', 'Error de conexión a base de datos');
```

## Implementación en C# (Bitacora.aspx.cs)

```csharp
using System.Data;
using System.Data.SqlClient;

private string connectionString = "TU_CONNECTION_STRING_AQUI";

private List<EventoBitacora> ObtenerEventos(string filtro, string busqueda)
{
    var eventos = new List<EventoBitacora>();
    
    using (SqlConnection conn = new SqlConnection(connectionString))
    {
        conn.Open();
        string sql = @"
            SELECT id, tipo, usuario, accion, timestamp
            FROM Bitacora
            WHERE (@filtro = 'all' OR tipo = @filtro)
            AND (@busqueda = '' OR usuario LIKE '%' + @busqueda + '%' OR accion LIKE '%' + @busqueda + '%')
            ORDER BY timestamp DESC";
            
        using (SqlCommand cmd = new SqlCommand(sql, conn))
        {
            cmd.Parameters.AddWithValue("@filtro", filtro ?? "all");
            cmd.Parameters.AddWithValue("@busqueda", busqueda ?? "");
            
            using (SqlDataReader reader = cmd.ExecuteReader())
            {
                while (reader.Read())
                {
                    eventos.Add(new EventoBitacora
                    {
                        Id = reader.GetInt32(0),
                        Tipo = reader.GetString(1),
                        Usuario = reader.GetString(2),
                        Accion = reader.GetString(3),
                        Timestamp = reader.GetDateTime(4),
                        Expandido = false
                    });
                }
            }
        }
    }
    
    return eventos;
}

private EstadisticasBitacora ObtenerEstadisticas()
{
    using (SqlConnection conn = new SqlConnection(connectionString))
    {
        conn.Open();
        string sql = @"
            SELECT 
                COUNT(*) as Total,
                SUM(CASE WHEN tipo = 'login' THEN 1 ELSE 0 END) as Logins,
                SUM(CASE WHEN tipo = 'new_user' THEN 1 ELSE 0 END) as UsuariosNuevos,
                SUM(CASE WHEN tipo = 'error' THEN 1 ELSE 0 END) as Errores
            FROM Bitacora";
            
        using (SqlCommand cmd = new SqlCommand(sql, conn))
        using (SqlDataReader reader = cmd.ExecuteReader())
        {
            if (reader.Read())
            {
                return new EstadisticasBitacora
                {
                    Total = reader.GetInt32(0),
                    Logins = reader.IsDBNull(1) ? 0 : reader.GetInt32(1),
                    UsuariosNuevos = reader.IsDBNull(2) ? 0 : reader.GetInt32(2),
                    Errores = reader.IsDBNull(3) ? 0 : reader.GetInt32(3)
                };
            }
        }
    }
    
    return new EstadisticasBitacora();
}
```

## Notas

- El campo `dvv` y `dvh` son para verificación de integridad de datos (patrón existente en el proyecto)
- Los índices mejoran el rendimiento de las consultas, especialmente con muchos registros
- Para paginación en el futuro, agregar `OFFSET x ROWS FETCH NEXT y ROWS ONLY` a la consulta de eventos