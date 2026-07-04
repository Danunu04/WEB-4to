# Entrenadores Module Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Create an Entrenadores (Trainers) management page with inline table editing, matching the Alumnos page design.

**Architecture:** Three-layer architecture (BE → BLL → MPP → SQL), with ASP.NET Web Forms UI. Entrenadores table in SQL Server with DNI as primary key and USUARIOS foreign key relationship.

**Tech Stack:** ASP.NET Web Forms (.NET Framework 4.7.2), C#, SQL Server, Visual Studio 2017+

---

## File Structure

```
BE/
  Entrenador.cs                    # [NEW] Entrenador business entity

BLL/
  BLLEntrenador.cs                 # [NEW] Entrenador business logic layer

MPP/
  MPPEntrenador.cs                 # [NEW] Entrenador data access layer (SQL)

gymAppV2/
  Entrenadores/
    Entrenadores.aspx              # [MODIFY] Main page markup (currently stub)
    Entrenadores.aspx.cs           # [MODIFY] Page code-behind (currently stub)
    Entrenadores.aspx.designer.cs  # [MODIFY] Designer file
    Entrenadores.css               # [NEW] Page styles
  DashBoard.Master                 # [MODIFY] Update nav link
```

---

### Task 1: Create Entrenador BE Entity

**Files:**
- Create: `BE\Entrenador.cs`

- [ ] **Step 1: Write the Entrenador class**

```csharp
using System;

namespace BE
{
    public class Entrenador
    {
        public int DNI { get; set; }
        public string Nombre { get; set; }
        public string Apellido { get; set; }
        public DateTime FechaNacimiento { get; set; }
        public string Usuario { get; set; }
        public bool Activo { get; set; }  // Derived from USUARIO_Activo
        public int AlumnosCount { get; set; }  // Count of students assigned via Rutinas
        public string DVV { get; set; }
        public string DVH { get; set; }

        public Entrenador()
        {
        }

        public Entrenador(int dni, string nombre, string apellido, DateTime fechaNacimiento, string usuario, bool activo, int alumnosCount, string dvv, string dvh)
        {
            DNI = dni;
            Nombre = nombre;
            Apellido = apellido;
            FechaNacimiento = fechaNacimiento;
            Usuario = usuario;
            Activo = activo;
            AlumnosCount = alumnosCount;
            DVV = dvv;
            DVH = dvh;
        }
    }
}
```

- [ ] **Step 2: Add file to BE.csproj**

```xml
<ItemGroup>
  <Compile Include="Entrenador.cs" />
</ItemGroup>
```

- [ ] **Step 3: Commit**

```bash
git add BE/Entrenador.cs BE/BE.csproj
git commit -m "feat: add Entrenador BE entity class"
```

---

### Task 2: Create MPPEntrenador Data Access Layer

**Files:**
- Create: `MPP\MPPEntrenador.cs`

- [ ] **Step 1: Write the MPPEntrenador class**

```csharp
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Configuration;
using BE;

namespace MPP
{
    public class MPPEntrenador
    {
        private static string GetConnectionString()
        {
            return ConfigurationManager.ConnectionStrings["GymAppConnection"].ConnectionString;
        }

        public static List<Entrenador> ListarEntrenadores()
        {
            List<Entrenador> entrenadores = new List<Entrenador>();

            using (SqlConnection connection = new SqlConnection(GetConnectionString()))
            {
                string query = @"
                    SELECT e.dni, e.nombre, e.apellido, e.fechaNacimiento, e.usr, u.USUARIO_Activo, e.dvv, e.dvh,
                           (SELECT COUNT(DISTINCT r.dniAlumno) FROM Rutinas r WHERE r.dniEntrenador = e.dni) AS AlumnosCount
                    FROM Entrenadores e
                    LEFT JOIN USUARIOS u ON e.usr = u.USUARIO_Usuario
                    ORDER BY e.apellido, e.nombre";

                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    connection.Open();
                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            entrenadores.Add(new Entrenador(
                                reader.GetInt32(reader.GetOrdinal("dni")),
                                reader.GetString(reader.GetOrdinal("nombre")),
                                reader.GetString(reader.GetOrdinal("apellido")),
                                reader.GetDateTime(reader.GetOrdinal("fechaNacimiento")),
                                reader.IsDBNull(reader.GetOrdinal("usr")) ? null : reader.GetString(reader.GetOrdinal("usr")),
                                reader.IsDBNull(reader.GetOrdinal("USUARIO_Activo")) ? false : reader.GetBoolean(reader.GetOrdinal("USUARIO_Activo")),
                                reader.IsDBNull(reader.GetOrdinal("AlumnosCount")) ? 0 : reader.GetInt32(reader.GetOrdinal("AlumnosCount")),
                                reader.GetString(reader.GetOrdinal("dvv")),
                                reader.GetString(reader.GetOrdinal("dvh"))
                            ));
                        }
                    }
                }
            }

            return entrenadores;
        }

        public static Entrenador ObtenerEntrenador(int dni)
        {
            using (SqlConnection connection = new SqlConnection(GetConnectionString()))
            {
                string query = @"
                    SELECT e.dni, e.nombre, e.apellido, e.fechaNacimiento, e.usr, u.USUARIO_Activo, e.dvv, e.dvh,
                           (SELECT COUNT(DISTINCT r.dniAlumno) FROM Rutinas r WHERE r.dniEntrenador = e.dni) AS AlumnosCount
                    FROM Entrenadores e
                    LEFT JOIN USUARIOS u ON e.usr = u.USUARIO_Usuario
                    WHERE e.dni = @dni";

                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@dni", dni);
                    connection.Open();

                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            return new Entrenador(
                                reader.GetInt32(reader.GetOrdinal("dni")),
                                reader.GetString(reader.GetOrdinal("nombre")),
                                reader.GetString(reader.GetOrdinal("apellido")),
                                reader.GetDateTime(reader.GetOrdinal("fechaNacimiento")),
                                reader.IsDBNull(reader.GetOrdinal("usr")) ? null : reader.GetString(reader.GetOrdinal("usr")),
                                reader.IsDBNull(reader.GetOrdinal("USUARIO_Activo")) ? false : reader.GetBoolean(reader.GetOrdinal("USUARIO_Activo")),
                                reader.IsDBNull(reader.GetOrdinal("AlumnosCount")) ? 0 : reader.GetInt32(reader.GetOrdinal("AlumnosCount")),
                                reader.GetString(reader.GetOrdinal("dvv")),
                                reader.GetString(reader.GetOrdinal("dvh"))
                            );
                        }
                    }
                }
            }

            return null;
        }

        public static bool ActualizarEntrenador(Entrenador entrenador)
        {
            using (SqlConnection connection = new SqlConnection(GetConnectionString()))
            {
                string query = @"
                    UPDATE Entrenadores
                    SET nombre = @nombre, apellido = @apellido, fechaNacimiento = @fechaNacimiento,
                        usr = @usr
                    WHERE dni = @dni";

                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@dni", entrenador.DNI);
                    command.Parameters.AddWithValue("@nombre", entrenador.Nombre);
                    command.Parameters.AddWithValue("@apellido", entrenador.Apellido);
                    command.Parameters.AddWithValue("@fechaNacimiento", entrenador.FechaNacimiento);
                    command.Parameters.AddWithValue("@usr", (object)entrenador.Usuario ?? DBNull.Value);

                    connection.Open();
                    int rowsAffected = command.ExecuteNonQuery();
                    return rowsAffected > 0;
                }
            }
        }

        public static bool EliminarEntrenador(int dni)
        {
            using (SqlConnection connection = new SqlConnection(GetConnectionString()))
            {
                using (SqlTransaction transaction = connection.BeginTransaction())
                {
                    try
                    {
                        // Delete from Actividad_Entrenador first (FK dependency)
                        string deleteActividades = "DELETE FROM Actividad_Entrenador WHERE dniEntrenador = @dni";
                        using (SqlCommand cmdActividades = new SqlCommand(deleteActividades, connection, transaction))
                        {
                            cmdActividades.Parameters.AddWithValue("@dni", dni);
                            cmdActividades.ExecuteNonQuery();
                        }

                        // Delete from Rutinas (FK dependency)
                        string deleteRutinas = "DELETE FROM Rutinas WHERE dniEntrenador = @dni";
                        using (SqlCommand cmdRutinas = new SqlCommand(deleteRutinas, connection, transaction))
                        {
                            cmdRutinas.Parameters.AddWithValue("@dni", dni);
                            cmdRutinas.ExecuteNonQuery();
                        }

                        // Delete the Entrenador
                        string deleteEntrenador = "DELETE FROM Entrenadores WHERE dni = @dni";
                        using (SqlCommand cmdEntrenador = new SqlCommand(deleteEntrenador, connection, transaction))
                        {
                            cmdEntrenador.Parameters.AddWithValue("@dni", dni);
                            int rowsAffected = cmdEntrenador.ExecuteNonQuery();

                            transaction.Commit();
                            return rowsAffected > 0;
                        }
                    }
                    catch
                    {
                        transaction.Rollback();
                        throw;
                    }
                }
            }
        }

        public static Dictionary<string, int> ObtenerEstadisticas()
        {
            var stats = new Dictionary<string, int>();

            using (SqlConnection connection = new SqlConnection(GetConnectionString()))
            {
                string query = @"
                    SELECT
                        COUNT(*) AS Total,
                        SUM(CASE WHEN u.USUARIO_Activo = 1 THEN 1 ELSE 0 END) AS Activos,
                        SUM(CASE WHEN (SELECT COUNT(*) FROM Rutinas r WHERE r.dniEntrenador = e.dni) > 0 THEN 1 ELSE 0 END) AS ConAlumnos,
                        SUM(CASE WHEN e.usr IS NULL OR e.usr = '' THEN 1 ELSE 0 END) AS SinUsuario
                    FROM Entrenadores e
                    LEFT JOIN USUARIOS u ON e.usr = u.USUARIO_Usuario";

                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    connection.Open();

                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            stats["Total"] = reader.IsDBNull(reader.GetOrdinal("Total")) ? 0 : reader.GetInt32(reader.GetOrdinal("Total"));
                            stats["Activos"] = reader.IsDBNull(reader.GetOrdinal("Activos")) ? 0 : reader.GetInt32(reader.GetOrdinal("Activos"));
                            stats["ConAlumnos"] = reader.IsDBNull(reader.GetOrdinal("ConAlumnos")) ? 0 : reader.GetInt32(reader.GetOrdinal("ConAlumnos"));
                            stats["SinUsuario"] = reader.IsDBNull(reader.GetOrdinal("SinUsuario")) ? 0 : reader.GetInt32(reader.GetOrdinal("SinUsuario"));
                        }
                    }
                }
            }

            return stats;
        }
    }
}
```

- [ ] **Step 2: Add file to MPP.csproj**

```xml
<ItemGroup>
  <Compile Include="MPPEntrenador.cs" />
</ItemGroup>
```

- [ ] **Step 3: Commit**

```bash
git add MPP/MPPEntrenador.cs MPP/MPP.csproj
git commit -m "feat: add MPPEntrenador data access layer"
```

---

### Task 3: Create BLLEntrenador Business Logic Layer

**Files:**
- Create: `BLL\BLLEntrenador.cs`

- [ ] **Step 1: Write the BLLEntrenador class**

```csharp
using System;
using System.Collections.Generic;
using BE;
using MPP;

namespace BLL
{
    public class BLLEntrenador
    {
        public static List<Entrenador> ListarEntrenadores()
        {
            return MPPEntrenador.ListarEntrenadores();
        }

        public static Entrenador ObtenerEntrenador(int dni)
        {
            return MPPEntrenador.ObtenerEntrenador(dni);
        }

        public static bool ActualizarEntrenador(Entrenador entrenador)
        {
            // Validate
            if (entrenador == null)
                throw new ArgumentNullException(nameof(entrenador));
            if (entrenador.DNI <= 0)
                throw new ArgumentException("DNI must be positive");
            if (string.IsNullOrWhiteSpace(entrenador.Nombre))
                throw new ArgumentException("Nombre is required");
            if (string.IsNullOrWhiteSpace(entrenador.Apellido))
                throw new ArgumentException("Apellido is required");
            if (entrenador.FechaNacimiento == default)
                throw new ArgumentException("FechaNacimiento is required");

            return MPPEntrenador.ActualizarEntrenador(entrenador);
        }

        public static bool EliminarEntrenador(int dni)
        {
            if (dni <= 0)
                throw new ArgumentException("DNI must be positive");

            return MPPEntrenador.EliminarEntrenador(dni);
        }

        public static Dictionary<string, int> ObtenerEstadisticas()
        {
            return MPPEntrenador.ObtenerEstadisticas();
        }
    }
}
```

- [ ] **Step 2: Add file to BLL.csproj**

```xml
<ItemGroup>
  <Compile Include="BLLEntrenador.cs" />
</ItemGroup>
```

- [ ] **Step 3: Commit**

```bash
git add BLL/BLLEntrenador.cs BLL/BLL.csproj
git commit -m "feat: add BLLEntrenador business logic layer"
```

---

### Task 4: Create Entrenadores.css Styles

**Files:**
- Create: `gymAppV2\Entrenadores\Entrenadores.css`

- [ ] **Step 1: Write the CSS file**

Copy and adapt Alumnos.css with these changes:
- Replace color theme: Use peach/orange colors for trainers instead of pink
- Change `.stat-icon-pink` to `.stat-icon-peach`
- Change `.av-pink` to use peach colors
- Keep all responsive design and layout exactly the same

```css
/* =============================================
   ENTRENADORES MODULE CSS
   ============================================= */

.entrenadores-container *, .entrenadores-container *::before, .entrenadores-container *::after { box-sizing: border-box; margin: 0; padding: 0; }

.entrenadores-container {
  --peach: #FFD5B5;
  --peach-light: #FFF0E5;
  --peach-dark: #E8A890;
  --peach-mid: #FFE0C5;
  --mint: #B5EAD7;
  --mint-light: #E5F9F0;
  --lavender: #C7B5FF;
  --lavender-light: #F0EBFF;
  --sky: #B5D5FF;
  --sky-light: #E5F0FF;
  --orange: #FF6B35;
  --orange-light: #FFF0EA;
  --yellow: #FFD166;
  --red: #E24B4A;
  --red-light: #FCEBEB;
  --bg: #FFF8F0;
  --surface: #ffffff;
  --surface-2: #FDF6EC;
  --border: #E8E0D5;
  --text: #2D2D2D;
  --text-muted: #6B6B6B;
  --text-light: #9DB8AB;
  --radius: 14px;
  --radius-sm: 8px;
  --radius-md: 0.5rem;
  --radius-lg: 0.75rem;
  --radius-xl: 1rem;
  --shadow: 0 2px 12px rgba(255, 181, 197, 0.08);
  --shadow-md: 0 4px 24px rgba(255, 181, 197, 0.12);
  padding: 2rem;
  width: 100%;
  margin: 0 auto;
}

/* ── PAGE HEADER ────────────────────── */
.entrenadores-header {
  display: flex;
  align-items: center;
  justify-content: space-between;
  margin-bottom: 1.5rem;
}

.entrenadores-title {
  font-family: 'Fraunces', system-ui, serif;
  font-size: 1.6rem;
  font-weight: 700;
  color: var(--text);
  display: flex;
  align-items: center;
  gap: 12px;
}

.entrenadores-title i {
  width: 44px; height: 44px;
  background: var(--peach-light);
  border-radius: var(--radius-sm);
  display: flex; align-items: center; justify-content: center;
  color: var(--peach-dark);
  font-size: 1.1rem;
}

.badge-count {
  font-size: 0.8rem;
  font-weight: 600;
  background: var(--peach);
  color: #fff;
  padding: 3px 10px;
  border-radius: 20px;
  font-family: 'DM Sans', system-ui, sans-serif;
}

.btn-primary {
  background: var(--lavender);
  color: #fff;
  border: none;
  border-radius: var(--radius-md);
  padding: 0.5rem 1rem;
  font-family: 'DM Sans', system-ui, sans-serif;
  font-weight: 500;
  font-size: 0.875rem;
  cursor: pointer;
  transition: transform 120ms ease-out, background-color 120ms, box-shadow 160ms;
  display: flex;
  align-items: center;
  gap: 0.5rem;
}

.btn-primary:hover {
  transform: translateY(-0.125rem);
  background: #B8A0E8;
}

.btn-primary:active {
  transform: scale(0.96);
}

/* ── STATS ROW ────────────────────── */
.stats-row {
  display: grid;
  grid-template-columns: repeat(4, 1fr);
  gap: 1rem;
  margin-bottom: 1.25rem;
}

.stat-card {
  background: var(--surface);
  border-radius: var(--radius-xl);
  border: 1px solid var(--border);
  padding: 1rem 1.25rem;
  box-shadow: var(--shadow);
  display: flex;
  align-items: center;
  gap: 12px;
}

.stat-icon {
  width: 42px; height: 42px;
  border-radius: var(--radius-md);
  display: flex; align-items: center; justify-content: center;
  font-size: 1rem;
  flex-shrink: 0;
}

.stat-icon-peach { background: var(--peach-light); color: var(--peach-dark); }
.stat-icon-mint { background: var(--mint-light); color: #0F6E56; }
.stat-icon-lavender { background: var(--lavender-light); color: #4C1D95; }
.stat-icon-sky { background: var(--sky-light); color: #0369A1; }

.stat-info p { font-size: 0.75rem; color: var(--text-muted); font-weight: 500; margin-bottom: 2px; }
.stat-info h4 { font-family: 'Fraunces', system-ui, serif; font-size: 1.3rem; font-weight: 700; color: var(--text); }

/* ── FILTER CARD ────────────────────── */
.filter-card {
  background: var(--surface);
  border-radius: var(--radius-xl);
  border: 1px solid var(--border);
  padding: 1.1rem 1.5rem;
  margin-bottom: 1.25rem;
  display: flex;
  align-items: flex-end;
  gap: 1rem;
  flex-wrap: wrap;
  box-shadow: var(--shadow);
}

.filter-group {
  display: flex;
  flex-direction: column;
  gap: 5px;
  min-width: 160px;
}

.filter-group label {
  font-size: 0.74rem;
  font-weight: 600;
  color: var(--text-muted);
  text-transform: uppercase;
  letter-spacing: 0.5px;
}

.filter-group select, .search-input {
  border: 1.5px solid var(--border);
  border-radius: var(--radius-sm);
  padding: 8px 12px;
  font-family: 'DM Sans', system-ui, sans-serif;
  font-size: 0.88rem;
  color: var(--text);
  background: var(--bg);
  outline: none;
  cursor: pointer;
  transition: border-color .2s;
}

.filter-group select:focus, .search-input:focus {
  border-color: var(--peach);
}

.search-wrap {
  display: flex; flex-direction: column; gap: 5px; flex: 1; min-width: 200px;
}

.search-wrap label {
  font-size: 0.74rem; font-weight: 600; color: var(--text-muted);
  text-transform: uppercase; letter-spacing: 0.5px;
}

.search-inner {
  position: relative;
}

.search-inner i {
  position: absolute; left: 10px; top: 50%; transform: translateY(-50%);
  color: var(--text-light); font-size: 0.9rem;
}

.search-input { padding-left: 32px; width: 100%; }

.btn-filter {
  background: var(--peach);
  color: #fff;
  border: none;
  border-radius: var(--radius-sm);
  padding: 9px 20px;
  font-family: 'DM Sans', system-ui, sans-serif;
  font-weight: 600;
  font-size: 0.88rem;
  cursor: pointer;
  transition: background .2s, transform .1s;
  display: flex; align-items: center; gap: 7px;
  align-self: flex-end;
}

.btn-filter:hover { background: var(--peach-dark); }
.btn-filter:active { transform: scale(0.97); }

/* ── MAIN GRID ────────────────────── */
.main-grid {
    display: grid;
    grid-template-columns: 1fr;
    gap: 1rem;
    align-items: start;
    width: 100%;
}

.main-grid > div:first-child {
  width: 100%;
  min-width: 0;
  flex: 1;
}

.main-grid > div:first-child > .table-card {
  width: 100%;
}

.main-grid > div:first-child > div {
  width: 100%;
}

/* ── TABLE ────────────────────── */
.table-card {
  background: var(--surface);
  border-radius: var(--radius-xl);
  border: 1px solid var(--border);
  box-shadow: var(--shadow);
  overflow: hidden;
  width: 100%;
  display: flex;
  flex-direction: column;
  flex: 1;
}

.table-card > div {
  width: 100%;
}

.table-card > table,
.table-card > div > table {
  width: 100% !important;
  max-width: none !important;
}

.table-card-header {
  padding: 1rem 1.5rem;
  border-bottom: 1px solid var(--border);
  display: flex;
  align-items: center;
  justify-content: space-between;
}

.table-card-header h3 {
  font-family: 'Fraunces', system-ui, serif;
  font-size: 0.95rem;
  font-weight: 600;
  color: var(--text);
}

#gvEntrenadores {
  width: 100% !important;
  table-layout: fixed !important;
  max-width: none !important;
}

[id="gvEntrenadores"] {
  width: 100% !important;
}

[id="gvEntrenadores"] table {
  width: 100% !important;
  max-width: none !important;
}

.table-actions-row {
  display: flex;
  gap: 6px;
}

.btn-icon {
  border: 1.5px solid var(--border);
  background: transparent;
  border-radius: var(--radius-sm);
  width: 34px; height: 34px;
  display: flex; align-items: center; justify-content: center;
  color: var(--text-muted);
  cursor: pointer;
  transition: all .2s;
  font-size: 0.85rem;
}

.btn-icon:hover { border-color: var(--peach); color: var(--peach); background: var(--peach-light); }

.btn-icon-small {
  width: 28px; height: 28px;
  font-size: 0.75rem;
}

.btn-icon-small a {
  display: flex; align-items: center; justify-content: center;
  width: 100%; height: 100%;
  text-decoration: none;
  color: inherit;
}

.btn-icon-danger:hover { border-color: var(--red); color: var(--red); background: var(--red-light); }

.table-card table,
.gvEntrenadores table,
table {
  width: 100% !important;
  border-collapse: collapse;
  table-layout: fixed;
}

thead th {
  text-align: left;
  padding: 10px 16px;
  font-size: 0.74rem;
  font-weight: 700;
  color: var(--text-muted);
  text-transform: uppercase;
  letter-spacing: 0.5px;
  background: var(--surface-2);
  border-bottom: 1px solid var(--border);
  white-space: nowrap;
  width: auto;
}

thead th:nth-child(1) { width: 25%; }
thead th:nth-child(2) { width: 15%; }
thead th:nth-child(3) { width: 15%; }
thead th:nth-child(4) { width: 15%; }
thead th:nth-child(5) { width: 15%; }
thead th:nth-child(6) { width: 15%; }

tbody tr {
  cursor: pointer;
  transition: background .15s;
}

tbody tr:hover { background: #f0faf6; }
tbody tr.selected { background: var(--peach-light); }
tbody tr.editing { background: var(--peach-light); }

tbody td {
  padding: 11px 16px;
  font-size: 0.88rem;
  border-bottom: 1px solid #f0f4f2;
  color: var(--text);
}

tbody td:nth-child(1) { width: 25%; }
tbody td:nth-child(2) { width: 15%; }
tbody td:nth-child(3) { width: 15%; }
tbody td:nth-child(4) { width: 15%; }
tbody td:nth-child(5) { width: 15%; }
tbody td:nth-child(6) { width: 15%; }

tbody tr:last-child td { border-bottom: none; }

.td-name {
  display: flex;
  align-items: center;
  gap: 10px;
}

.td-avatar {
  width: 32px; height: 32px;
  border-radius: 50%;
  display: flex; align-items: center; justify-content: center;
  font-size: 0.72rem;
  font-weight: 700;
  flex-shrink: 0;
}

.av-peach { background: var(--peach-light); color: var(--peach-dark); }
.av-mint { background: var(--mint-light); color: #0F6E56; }
.av-lavender { background: var(--lavender-light); color: #4C1D95; }
.av-sky { background: var(--sky-light); color: #0369A1; }
.av-orange { background: var(--orange-light); color: #CC5529; }

.pill {
  display: inline-flex;
  align-items: center;
  gap: 5px;
  padding: 3px 10px;
  border-radius: 20px;
  font-size: 0.76rem;
  font-weight: 600;
}

.pill-active { background: var(--mint-light); color: #0F6E56; }
.pill-inactive { background: #F1EFE8; color: #5F5E5A; }

.pill-dot {
  width: 6px; height: 6px; border-radius: 50%; background: currentColor;
}

.user-pill {
  padding: 2px 9px;
  border-radius: 20px;
  font-size: 0.73rem;
  font-weight: 600;
}

.user-with { background: var(--peach-light); color: var(--peach-dark); }
.user-without { background: var(--surface-2); color: var(--text-muted); }

/* ── INLINE EDITING ────────────────────── */
.edit-cell input {
  width: 100%;
  padding: 4px 8px;
  border: 1.5px solid var(--peach);
  border-radius: var(--radius-sm);
  font-size: 0.88rem;
  font-family: 'DM Sans', system-ui, sans-serif;
  color: var(--text);
  background: var(--surface);
  outline: none;
}

.edit-cell input:focus {
  border-color: var(--orange);
  box-shadow: 0 0 0 2px var(--peach-light);
}

.btn-save, .btn-cancel {
  border: none;
  border-radius: var(--radius-sm);
  padding: 4px 10px;
  font-size: 0.75rem;
  font-weight: 600;
  cursor: pointer;
  transition: all .15s;
  display: inline-flex;
  align-items: center;
  gap: 4px;
}

.btn-save {
  background: var(--mint);
  color: #0F6E56;
}

.btn-save:hover { background: #9DDBC5; }

.btn-cancel {
  background: var(--red-light);
  color: var(--red);
}

.btn-cancel:hover { background: #F5D5D5; }

/* ── TABLE FOOTER ────────────────────── */
.table-footer {
  padding: 10px 16px;
  display: flex;
  align-items: center;
  justify-content: space-between;
  border-top: 1px solid var(--border);
  background: var(--surface-2);
}

.table-footer-text {
  font-size: 0.8rem;
  color: var(--text-muted);
}

.pagination {
  display: flex; gap: 4px;
}

.page-btn {
  width: 28px; height: 28px;
  border: 1.5px solid var(--border);
  background: var(--surface);
  border-radius: 6px;
  display: flex; align-items: center; justify-content: center;
  font-size: 0.8rem;
  font-weight: 500;
  color: var(--text-muted);
  cursor: pointer;
  transition: all .15s;
}

.page-btn:hover, .page-btn.active {
  border-color: var(--peach);
  color: var(--peach);
  background: var(--peach-light);
}

/* ── RESPONSIVE ────────────────────── */
@media screen and (max-width: 1024px) {
  .main-grid {
    grid-template-columns: 1fr;
  }

  .stats-row {
    grid-template-columns: repeat(2, 1fr);
  }
}

@media screen and (max-width: 768px) {
  .entrenadores-container {
    padding: 1rem;
  }

  .stats-row {
    grid-template-columns: 1fr;
  }

  .filter-card {
    flex-direction: column;
    align-items: stretch;
  }

  .filter-group, .search-wrap {
    min-width: 100%;
  }

  .btn-filter {
    width: 100%;
    justify-content: center;
  }

  /* ── MOBILE CARD VIEW ────────────────────── */

  .table-card thead,
  .table-card tfoot {
    display: none;
  }

  .table-card tbody,
  .table-card tr,
  .table-card td {
    display: block;
    width: 100%;
  }

  .table-card tr {
    background: var(--surface);
    border-radius: var(--radius-md);
    border: 1px solid var(--border);
    margin-bottom: 1rem;
    padding: 1rem;
    box-shadow: var(--shadow);
  }

  .table-card tr:last-child {
    margin-bottom: 0;
  }

  .table-card tr.selected {
    background: var(--peach-light);
    border-color: var(--peach-mid);
  }

  .table-card td {
    text-align: left;
    padding: 0.5rem 0;
    border: none;
    display: flex;
    align-items: center;
    justify-content: space-between;
  }

  .table-card td::before {
    content: attr(data-label);
    font-weight: 600;
    font-size: 0.75rem;
    color: var(--text-muted);
    text-transform: uppercase;
    letter-spacing: 0.5px;
    min-width: 120px;
  }

  .table-card td:first-child {
    flex-direction: column;
    align-items: flex-start;
    padding-bottom: 0.75rem;
    margin-bottom: 0.75rem;
    border-bottom: 1px solid var(--border);
  }

  .table-card td:first-child::before {
    display: none;
  }

  .table-card td:first-child .td-name {
    gap: 12px;
  }

  .table-card td:first-child .td-avatar {
    width: 48px;
    height: 48px;
    font-size: 1rem;
  }

  .table-card td:nth-child(4),
  .table-card td:nth-child(5) {
    justify-content: flex-end;
  }

  .table-card td:nth-child(4)::before,
  .table-card td:nth-child(5)::before {
    display: none;
  }

  .table-card td:nth-child(6) {
    justify-content: flex-end;
    padding-top: 0.75rem;
    margin-top: 0.5rem;
    border-top: 1px solid var(--border);
  }

  .table-card td:nth-child(6)::before {
    display: none;
  }

  .table-footer {
    display: none;
  }

  .pagination {
    display: none;
  }
}
```

- [ ] **Step 2: Commit**

```bash
git add gymAppV2/Entrenadores/Entrenadores.css
git commit -m "feat: add Entrenadores page styles"
```

---

### Task 5: Update Entrenadores.aspx Mark-up

**Files:**
- Modify: `gymAppV2\Entrenadores\Entrenadores.aspx`

- [ ] **Step 1: Replace entire file content with new mark-up**

```asp
<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="Entrenadores.aspx.cs" Inherits="gymAppV2.Entrenadores.Entrenadores" MasterPageFile="~/DashBoard.Master" EnableEventValidation="false" %>

<asp:Content ID="HeadContent" ContentPlaceHolderID="HeadContent" runat="server">
    <title>Gestión de Entrenadores - GymApp</title>
    <link href="<%= ResolveUrl("~/Entrenadores/Entrenadores.css?v=1") %>" rel="stylesheet" type="text/css" />
    <link rel="stylesheet" href="https://cdnjs.cloudflare.com/ajax/libs/font-awesome/6.5.0/css/all.min.css" />
</asp:Content>

<asp:Content ID="MainContent" ContentPlaceHolderID="MainContent" runat="server">
    <div class="entrenadores-container">
        <!-- Page header -->
        <div class="entrenadores-header">
            <div class="entrenadores-title">
                <i class="fa-solid fa-dumbbell"></i>
                Gestión de Entrenadores
                <span class="badge-count" id="badgeCount" runat="server">0 entrenadores</span>
            </div>
        </div>

        <!-- Stats -->
        <div class="stats-row">
            <div class="stat-card">
                <div class="stat-icon stat-icon-peach"><i class="fa-solid fa-users"></i></div>
                <div class="stat-info"><p>Total entrenadores</p><h4><asp:Label ID="lblTotal" runat="server" Text="0"></asp:Label></h4></div>
            </div>
            <div class="stat-card">
                <div class="stat-icon stat-icon-mint"><i class="fa-solid fa-calendar-check"></i></div>
                <div class="stat-info"><p>Activos</p><h4><asp:Label ID="lblActivos" runat="server" Text="0"></asp:Label></h4></div>
            </div>
            <div class="stat-card">
                <div class="stat-icon stat-icon-lavender"><i class="fa-solid fa-user-group"></i></div>
                <div class="stat-info"><p>Con alumnos</p><h4><asp:Label ID="lblConAlumnos" runat="server" Text="0"></asp:Label></h4></div>
            </div>
            <div class="stat-card">
                <div class="stat-icon stat-icon-sky"><i class="fa-solid fa-user-slash"></i></div>
                <div class="stat-info"><p>Sin usuario</p><h4><asp:Label ID="lblSinUsuario" runat="server" Text="0"></asp:Label></h4></div>
            </div>
        </div>

        <!-- Filters -->
        <div class="filter-card">
            <div style="font-size:0.78rem;font-weight:700;color:var(--text-muted);text-transform:uppercase;letter-spacing:0.5px;align-self:flex-end;padding-bottom:9px;">
                <i class="fa-solid fa-sliders" style="margin-right:6px"></i>Filtros
            </div>
            <div class="filter-group">
                <label>Estado</label>
                <asp:DropDownList ID="ddlEstado" runat="server" AutoPostBack="true" OnSelectedIndexChanged="ddlEstado_SelectedIndexChanged">
                    <asp:ListItem Value="">Todos</asp:ListItem>
                    <asp:ListItem Value="activo">Activos</asp:ListItem>
                    <asp:ListItem Value="inactivo">Inactivos</asp:ListItem>
                </asp:DropDownList>
            </div>
            <div class="filter-group">
                <label>Usuario asociado</label>
                <asp:DropDownList ID="ddlUsuario" runat="server" AutoPostBack="true" OnSelectedIndexChanged="ddlUsuario_SelectedIndexChanged">
                    <asp:ListItem Value="">Todos</asp:ListItem>
                </asp:DropDownList>
            </div>
            <div class="search-wrap">
                <label>Buscar</label>
                <div class="search-inner">
                    <i class="fa-solid fa-magnifying-glass"></i>
                    <asp:TextBox ID="txtBusqueda" runat="server" CssClass="search-input" placeholder="Nombre, apellido o DNI..." AutoPostBack="true" OnTextChanged="txtBusqueda_TextChanged"></asp:TextBox>
                </div>
            </div>
            <button id="btnFiltrar" runat="server" class="btn-filter" onserverclick="btnFiltrar_Click">
                <i class="fa-solid fa-magnifying-glass"></i> Filtrar
            </button>
        </div>

        <!-- Main grid -->
        <div class="main-grid">

            <!-- Table -->
            <div>
                <div class="table-card">
                    <div class="table-card-header">
                        <h3><i class="fa-solid fa-table-list" style="margin-right:7px;color:var(--peach)"></i>Lista de entrenadores</h3>
                        <div class="table-actions-row">
                            <button id="btnExportar" runat="server" class="btn-icon" title="Exportar" onserverclick="btnExportar_Click">
                                <i class="fa-solid fa-file-export"></i>
                            </button>
                            <button id="btnActualizar" runat="server" class="btn-icon" title="Actualizar" onserverclick="btnActualizar_Click">
                                <i class="fa-solid fa-arrows-rotate"></i>
                            </button>
                        </div>
                    </div>
                    <asp:GridView ID="gvEntrenadores" runat="server" AutoGenerateColumns="false" CssClass="table"
                        GridLines="None" AllowPaging="true" PageSize="10" OnPageIndexChanging="gvEntrenadores_PageIndexChanging"
                        OnRowCommand="gvEntrenadores_RowCommand" OnRowDataBound="gvEntrenadores_RowDataBound"
                        DataKeyNames="DNI" SelectedRowStyle-CssClass="selected" Width="100%">
                        <Columns>
                            <asp:TemplateField HeaderText="Entrenador">
                                <ItemTemplate>
                                    <div class="td-name">
                                        <div class="td-avatar <%# GetAvatarClass(Container.DataItemIndex) %>">
                                            <%# GetInitials(Eval("Nombre"), Eval("Apellido")) %>
                                        </div>
                                        <div>
                                            <div style="font-weight:600;font-size:0.88rem"><%# Eval("Apellido") %>, <%# Eval("Nombre") %></div>
                                            <div style="font-size:0.76rem;color:var(--text-muted)">DNI: <%# Eval("DNI") %></div>
                                        </div>
                                    </div>
                                </ItemTemplate>
                            </asp:TemplateField>
                            <asp:TemplateField HeaderText="Fecha Nacimiento">
                                <ItemTemplate>
                                    <asp:TextBox ID="txtFechaNacimiento" runat="server" Text='<%# Eval("FechaNacimiento", "{0:yyyy-MM-dd}") %>'
                                        CssClass="edit-input" style="width:100%;border:none;background:transparent;cursor:pointer;"
                                        onclick="this.type='date'" onblur="if(!this.value)this.type='text'" ReadOnly="true"></asp:TextBox>
                                </ItemTemplate>
                            </asp:TemplateField>
                            <asp:TemplateField HeaderText="Usuario">
                                <ItemTemplate>
                                    <span class="user-pill <%# GetUsuarioClass(Eval("Usuario")) %>">
                                        <%# Eval("Usuario") ?? "Sin usuario" %>
                                    </span>
                                </ItemTemplate>
                            </asp:TemplateField>
                            <asp:TemplateField HeaderText="Estado">
                                <ItemTemplate>
                                    <span class="pill <%# GetEstadoClass(Eval("Activo")) %>">
                                        <span class="pill-dot"></span><%# GetEstadoText(Eval("Activo")) %>
                                    </span>
                                </ItemTemplate>
                            </asp:TemplateField>
                            <asp:BoundField DataField="AlumnosCount" HeaderText="Alumnos" ReadOnly="true" />
                            <asp:TemplateField HeaderText="Acciones">
                                <ItemTemplate>
                                    <div class="action-buttons-inline">
                                        <asp:LinkButton ID="btnEditar" runat="server" CssClass="btn-icon-small" CommandName="Editar" CommandArgument='<%# Container.DataItemIndex %>' ToolTip="Editar">
                                            <i class="fa-solid fa-pen"></i>
                                        </asp:LinkButton>
                                        <asp:LinkButton ID="btnEliminar" runat="server" CssClass="btn-icon-small btn-icon-danger" CommandName="Eliminar" CommandArgument='<%# Container.DataItemIndex %>' ToolTip="Eliminar">
                                            <i class="fa-solid fa-trash"></i>
                                        </asp:LinkButton>
                                    </div>
                                </ItemTemplate>
                            </asp:TemplateField>
                        </Columns>
                        <PagerStyle CssClass="pagination" />
                        <EmptyDataTemplate>
                            <div style="padding: 2rem; text-align: center; color: var(--text-muted);">
                                <i class="fa-solid fa-dumbbell" style="font-size: 2rem; margin-bottom: 0.5rem;"></i>
                                <p>No se encontraron entrenadores</p>
                            </div>
                        </EmptyDataTemplate>
                    </asp:GridView>
                    <div class="table-footer">
                        <span class="table-footer-text" id="footerText" runat="server">Mostrando 0 de 0 entrenadores</span>
                    </div>
                </div>
            </div>

        </div><!-- /main-grid -->
    </div>
</asp:Content>
```

- [ ] **Step 2: Commit**

```bash
git add gymAppV2/Entrenadores/Entrenadores.aspx
git commit -m "feat: add Entrenadores page mark-up"
```

---

### Task 6: Update Entrenadores.aspx.cs Code-Behind

**Files:**
- Modify: `gymAppV2\Entrenadores\Entrenadores.aspx.cs`

- [ ] **Step 1: Replace entire file content with new code**

```csharp
using System;
using System.Web.UI;
using System.Web.UI.WebControls;
using BLL;
using BE;

namespace gymAppV2.Entrenadores
{
    public partial class Entrenadores : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                CargarEntrenadores();
                ActualizarEstadisticas();
                CargarUsuariosDropdown();
            }
        }

        // ==================== MÉTODOS PRINCIPALES ====================

        private void CargarEntrenadores()
        {
            var entrenadores = BLLEntrenador.ListarEntrenadores();
            gvEntrenadores.DataSource = entrenadores;
            gvEntrenadores.DataBind();

            ActualizarFooter(entrenadores.Count);
        }

        private void ActualizarEstadisticas()
        {
            var stats = BLLEntrenador.ObtenerEstadisticas();
            lblTotal.Text = stats["Total"].ToString();
            lblActivos.Text = stats["Activos"].ToString();
            lblConAlumnos.Text = stats["ConAlumnos"].ToString();
            lblSinUsuario.Text = stats["SinUsuario"].ToString();
        }

        private void ActualizarFooter(int total)
        {
            footerText.InnerText = $"Mostrando {total} de {total} entrenadores";
        }

        private void CargarUsuariosDropdown()
        {
            ddlUsuario.Items.Insert(0, new ListItem("", ""));
        }

        // ==================== EVENTOS DE FILTROS ====================

        protected void ddlEstado_SelectedIndexChanged(object sender, EventArgs e)
        {
            CargarEntrenadores();
        }

        protected void ddlUsuario_SelectedIndexChanged(object sender, EventArgs e)
        {
            CargarEntrenadores();
        }

        protected void txtBusqueda_TextChanged(object sender, EventArgs e)
        {
            CargarEntrenadores();
        }

        protected void btnFiltrar_Click(object sender, EventArgs e)
        {
            CargarEntrenadores();
        }

        // ==================== EVENTOS DE GRIDVIEW ====================

        protected void gvEntrenadores_PageIndexChanging(object sender, GridViewPageEventArgs e)
        {
            gvEntrenadores.PageIndex = e.NewPageIndex;
            CargarEntrenadores();
        }

        protected void gvEntrenadores_RowCommand(object sender, GridViewCommandEventArgs e)
        {
            int index = Convert.ToInt32(e.CommandArgument);
            int? dni = gvEntrenadores.DataKeys[index]?.Value as int?;

            if (e.CommandName == "Select")
            {
                // Seleccionar entrenador (opcional)
            }
            else if (e.CommandName == "Editar")
            {
                // Enable inline editing for this row
                HabilitarEdicionFila(index);
            }
            else if (e.CommandName == "Guardar")
            {
                // Save changes from inline editing
                GuardarCambiosFila(index);
            }
            else if (e.CommandName == "Cancelar")
            {
                // Cancel inline editing
                CancelarEdicionFila(index);
            }
            else if (e.CommandName == "Eliminar")
            {
                // Eliminar entrenador
                if (dni.HasValue)
                {
                    BLLEntrenador.EliminarEntrenador(dni.Value);
                    CargarEntrenadores();
                    ActualizarEstadisticas();
                }
            }
        }

        protected void gvEntrenadores_RowDataBound(object sender, GridViewRowEventArgs e)
        {
            if (e.Row.RowType == DataControlRowType.DataRow)
            {
                // Agregar atributos data-label para vista móvil
                e.Row.Cells[1].Attributes["data-label"] = "Fecha Nacimiento";
                e.Row.Cells[2].Attributes["data-label"] = "Usuario";
                e.Row.Cells[3].Attributes["data-label"] = "Estado";
                e.Row.Cells[4].Attributes["data-label"] = "Alumnos";
                e.Row.Cells[5].Attributes["data-label"] = "Acciones";
            }
        }

        // ==================== MÉTODOS DE EDICIÓN INLINE ====================

        private void HabilitarEdicionFila(int index)
        {
            GridViewRow row = gvEntrenadores.Rows[index];

            // Enable date input
            TextBox txtFecha = (TextBox)row.FindControl("txtFechaNacimiento");
            if (txtFecha != null)
            {
                txtFecha.ReadOnly = false;
                txtFecha.CssClass = "edit-input";
            }

            // Add edit styling
            row.CssClass = "editing";

            // Update action buttons
            LinkButton btnEditar = (LinkButton)row.FindControl("btnEditar");
            if (btnEditar != null)
            {
                btnEditar.CommandName = "Guardar";
                btnEditar.ToolTip = "Guardar";
                btnEditar.CssClass = "btn-icon-small";
                btnEditar.Attributes["onclick"] = "return confirm('¿Guardar cambios?');";
            }
        }

        private void GuardarCambiosFila(int index)
        {
            GridViewRow row = gvEntrenadores.Rows[index];
            int? dni = gvEntrenadores.DataKeys[index]?.Value as int?;

            if (dni.HasValue)
            {
                // Get the current entrenador
                var entrenador = BLLEntrenador.ObtenerEntrenador(dni.Value);
                if (entrenador != null)
                {
                    // Get edited values
                    TextBox txtFecha = (TextBox)row.FindControl("txtFechaNacimiento");
                    if (txtFecha != null && DateTime.TryParse(txtFecha.Text, out DateTime fechaNacimiento))
                    {
                        entrenador.FechaNacimiento = fechaNacimiento;
                    }

                    // Update
                    BLLEntrenador.ActualizarEntrenador(entrenador);

                    // Refresh
                    CargarEntrenadores();
                }
            }
        }

        private void CancelarEdicionFila(int index)
        {
            // Just reload to cancel
            CargarEntrenadores();
        }

        // ==================== EVENTOS DE ACCIONES ====================

        protected void btnExportar_Click(object sender, EventArgs e)
        {
            // TODO: Llamar a la capa BLL para exportar entrenadores
        }

        protected void btnActualizar_Click(object sender, EventArgs e)
        {
            CargarEntrenadores();
            ActualizarEstadisticas();
        }

        // ==================== MÉTODOS PARA EL GRIDVIEW ====================

        protected string GetInitials(object nombre, object apellido)
        {
            string n = nombre?.ToString() ?? "";
            string a = apellido?.ToString() ?? "";
            if (string.IsNullOrEmpty(n) || string.IsNullOrEmpty(a))
                return "--";
            return (n[0].ToString() + a[0].ToString()).ToUpper();
        }

        protected string GetAvatarClass(int index)
        {
            string[] classes = { "av-peach", "av-mint", "av-lavender", "av-sky", "av-orange" };
            return classes[index % classes.Length];
        }

        protected string GetUsuarioClass(object usuario)
        {
            string u = usuario?.ToString() ?? "";
            if (string.IsNullOrEmpty(u))
                return "user-without";
            return "user-with";
        }

        protected string GetEstadoClass(object activo)
        {
            bool a = Convert.ToBoolean(activo);
            return a ? "pill-active" : "pill-inactive";
        }

        protected string GetEstadoText(object activo)
        {
            bool a = Convert.ToBoolean(activo);
            return a ? "Activo" : "Inactivo";
        }
    }
}
```

- [ ] **Step 2: Commit**

```bash
git add gymAppV2/Entrenadores/Entrenadores.aspx.cs
git commit -m "feat: add Entrenadores page code-behind logic"
```

---

### Task 7: Update Navigation Link in DashBoard.Master

**Files:**
- Modify: `gymAppV2\DashBoard.Master`

- [ ] **Step 1: Update the Entrenadores nav link path**

Change line 52 from:
```html
<a href="~/Entrenadores" runat="server">
```

To:
```html
<a href="~/Entrenadores/Entrenadores.aspx" runat="server">
```

- [ ] **Step 2: Update the JavaScript allowed pages list**

Change line 183 from:
```javascript
if (href.includes('DashBoard') || href.includes('Bitacora') || href.includes('Usuarios') || href.includes('Alumnos')) {
```

To:
```javascript
if (href.includes('DashBoard') || href.includes('Bitacora') || href.includes('Usuarios') || href.includes('Alumnos') || href.includes('Entrenadores')) {
```

- [ ] **Step 3: Commit**

```bash
git add gymAppV2/DashBoard.Master
git commit -m "fix: update Entrenadores nav link and add to allowed pages"
```

---

## Summary

This plan creates the complete Entrenadores module with:

1. **BE Layer**: Entrenador entity class
2. **MPP Layer**: SQL data access for Entrenadores table
3. **BLL Layer**: Business logic for CRUD operations
4. **UI Layer**: ASP.NET Web Forms page matching Alumnos design
5. **Styles**: CSS file with peach color theme for trainers
6. **Navigation**: Updated nav link to point to correct page

All tasks follow the same patterns established in the existing Alumnos module, ensuring consistency across the codebase.