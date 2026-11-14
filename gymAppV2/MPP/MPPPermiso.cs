using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Text;
using DAL;
using SERVICIOS.Composite;

namespace MPP
{
    /// <summary>
    /// Acceso a datos del Composite de permisos: tablas Permiso, Familia, FamiliaPermiso,
    /// FamiliaFamilia, Perfil, PerfilPermiso, PerfilFamilia y UsuarioPerfil.
    ///
    /// Cada guardado es un solo lote SQL dentro de una transacción: o se guarda la familia
    /// (o el perfil) completa con todos sus hijos, o no se guarda nada. Después se recalcula
    /// el dvh de las filas escritas y el control de cada tabla.
    /// </summary>
    public class MPPPermiso
    {
        private DalGeneral dal;

        public MPPPermiso()
        {
            dal = new DalGeneral();
        }

        /// <summary>
        /// True si ya se corrió scripts/crear-permisos-composite.sql. Se fija en una columna
        /// propia de la tabla nueva porque el esquema viejo tenía otra tabla Permiso.
        /// </summary>
        public bool TablasExisten()
        {
            try
            {
                object resultado = dal._686DPEscalar(@"
                    SELECT CASE WHEN OBJECT_ID('[GymApp].[dbo].[UsuarioPerfil]', 'U') IS NOT NULL
                                 AND COL_LENGTH('[GymApp].[dbo].[Permiso]', 'idPermiso') IS NOT NULL
                            THEN 1 ELSE 0 END",
                    new List<SqlParameter>());

                return resultado != null && resultado != DBNull.Value && Convert.ToInt32(resultado) == 1;
            }
            catch (Exception ex)
            {
                throw new Exception("Error al verificar las tablas de permisos: " + ex.Message, ex);
            }
        }

        /// <summary>
        /// Lee todas las tablas de permisos en una sola consulta. Cada fila trae en "t" la tabla
        /// de origen; "a"/"b" son ids y "n"/"c" textos (nombre, código o usuario).
        /// </summary>
        public EstructuraPermisos CargarEstructura()
        {
            try
            {
                DataTable dt = dal._686DPConsultar(@"
                    SELECT 'PE' AS t, idPermiso AS a, NULL AS b, CAST(nombre AS VARCHAR(100)) AS n, CAST(NULL AS VARCHAR(30)) AS c FROM [GymApp].[dbo].[Permiso]
                    UNION ALL SELECT 'FA', idFamilia, NULL, nombre, codigo FROM [GymApp].[dbo].[Familia]
                    UNION ALL SELECT 'FP', idFamilia, idPermiso, NULL, NULL FROM [GymApp].[dbo].[FamiliaPermiso]
                    UNION ALL SELECT 'FF', idFamiliaPadre, idFamiliaHija, NULL, NULL FROM [GymApp].[dbo].[FamiliaFamilia]
                    UNION ALL SELECT 'PF', idPerfil, NULL, nombre, codigo FROM [GymApp].[dbo].[Perfil]
                    UNION ALL SELECT 'RP', idPerfil, idPermiso, NULL, NULL FROM [GymApp].[dbo].[PerfilPermiso]
                    UNION ALL SELECT 'RF', idPerfil, idFamilia, NULL, NULL FROM [GymApp].[dbo].[PerfilFamilia]
                    UNION ALL SELECT 'UP', idPerfil, NULL, usr, NULL FROM [GymApp].[dbo].[UsuarioPerfil]",
                    new List<SqlParameter>());

                var estructura = new EstructuraPermisos();

                foreach (DataRow row in dt.Rows)
                {
                    int a = Convert.ToInt32(row["a"]);
                    int b = row["b"] != DBNull.Value ? Convert.ToInt32(row["b"]) : 0;
                    string n = row["n"] != DBNull.Value ? row["n"].ToString() : null;
                    string c = row["c"] != DBNull.Value ? row["c"].ToString() : null;

                    switch (row["t"].ToString())
                    {
                        case "PE": estructura.Permisos[a] = n; break;
                        case "FA": estructura.Familias[a] = new EstructuraPermisos.DatosNodo { Nombre = n, Codigo = c }; break;
                        case "FP": EstructuraPermisos.AgregarRelacion(estructura.PermisosDeFamilia, a, b); break;
                        case "FF": EstructuraPermisos.AgregarRelacion(estructura.FamiliasDeFamilia, a, b); break;
                        case "PF": estructura.Perfiles[a] = new EstructuraPermisos.DatosNodo { Nombre = n, Codigo = c }; break;
                        case "RP": EstructuraPermisos.AgregarRelacion(estructura.PermisosDePerfil, a, b); break;
                        case "RF": EstructuraPermisos.AgregarRelacion(estructura.FamiliasDePerfil, a, b); break;
                        case "UP": estructura.PerfilDeUsuario[n] = a; break;
                    }
                }

                return estructura;
            }
            catch (Exception ex)
            {
                throw new Exception("Error al cargar los permisos: " + ex.Message, ex);
            }
        }

        /// <summary>
        /// Usuarios con la cuenta activa (para controlar que siempre quede alguien con acceso a Permisos).
        /// </summary>
        public HashSet<string> ListarUsuariosActivos()
        {
            try
            {
                DataTable dt = dal._686DPConsultar(
                    "SELECT usr FROM [GymApp].[dbo].[USUARIOS] WHERE activo = 1",
                    new List<SqlParameter>());

                var usuarios = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
                foreach (DataRow row in dt.Rows)
                    usuarios.Add(row["usr"].ToString());
                return usuarios;
            }
            catch (Exception ex)
            {
                throw new Exception("Error al listar los usuarios activos: " + ex.Message, ex);
            }
        }

        #region Familias

        /// <summary>
        /// Crea (idFamilia = 0) o reemplaza una familia con sus permisos y subfamilias.
        /// Devuelve el id de la familia.
        /// </summary>
        public int GuardarFamilia(int idFamilia, string nombre, string codigo, bool profundidad, IList<int> permisos, IList<int> familias)
        {
            try
            {
                var parametros = new List<SqlParameter>
                {
                    new SqlParameter("@IdFamilia", idFamilia),
                    new SqlParameter("@Nombre", nombre),
                    new SqlParameter("@Codigo", (object)codigo ?? DBNull.Value),
                    new SqlParameter("@Profundidad", profundidad)
                };

                string lote = @"
                    DECLARE @Id INT = @IdFamilia;
                    IF @Id = 0
                    BEGIN
                        INSERT INTO [GymApp].[dbo].[Familia] (nombre, codigo, profundidad, dvh)
                        VALUES (@Nombre, @Codigo, @Profundidad, '');
                        SET @Id = CAST(SCOPE_IDENTITY() AS INT);
                    END
                    ELSE
                        UPDATE [GymApp].[dbo].[Familia]
                        SET nombre = @Nombre, codigo = @Codigo, profundidad = @Profundidad
                        WHERE idFamilia = @Id;

                    DELETE FROM [GymApp].[dbo].[FamiliaPermiso] WHERE idFamilia = @Id;
                    DELETE FROM [GymApp].[dbo].[FamiliaFamilia] WHERE idFamiliaPadre = @Id;"
                    + InsertarHijos("FamiliaPermiso", "idFamilia", "idPermiso", permisos, "@Pe", parametros)
                    + InsertarHijos("FamiliaFamilia", "idFamiliaPadre", "idFamiliaHija", familias, "@Fa", parametros);

                int id = EjecutarEnTransaccion(lote, parametros);

                MPPDigitoVerificador.RecalcularDvhFilas("Familia", "idFamilia = @Id", new SqlParameter("@Id", id));
                MPPDigitoVerificador.RecalcularDvhFilas("FamiliaPermiso", "idFamilia = @Id", new SqlParameter("@Id", id));
                MPPDigitoVerificador.RecalcularDvhFilas("FamiliaFamilia", "idFamiliaPadre = @Id", new SqlParameter("@Id", id));
                return id;
            }
            catch (Exception ex)
            {
                throw new Exception("Error al guardar la familia: " + ex.Message, ex);
            }
        }

        /// <summary>
        /// Borra la familia y sus relaciones. La BLL ya controló que nadie la use.
        /// </summary>
        public void EliminarFamilia(int idFamilia)
        {
            try
            {
                EjecutarEnTransaccion(@"
                    DECLARE @Id INT = @IdFamilia;
                    DELETE FROM [GymApp].[dbo].[FamiliaPermiso] WHERE idFamilia = @Id;
                    DELETE FROM [GymApp].[dbo].[FamiliaFamilia] WHERE idFamiliaPadre = @Id OR idFamiliaHija = @Id;
                    DELETE FROM [GymApp].[dbo].[PerfilFamilia] WHERE idFamilia = @Id;
                    DELETE FROM [GymApp].[dbo].[Familia] WHERE idFamilia = @Id;",
                    new List<SqlParameter> { new SqlParameter("@IdFamilia", idFamilia) });

                MPPDigitoVerificador.SincronizarControl("FamiliaPermiso", "FamiliaFamilia", "PerfilFamilia", "Familia");
            }
            catch (Exception ex)
            {
                throw new Exception("Error al eliminar la familia: " + ex.Message, ex);
            }
        }

        #endregion

        #region Perfiles

        /// <summary>
        /// Crea (idPerfil = 0) o reemplaza un perfil con sus permisos sueltos y familias.
        /// Devuelve el id del perfil.
        /// </summary>
        public int GuardarPerfil(int idPerfil, string nombre, string codigo, IList<int> permisos, IList<int> familias)
        {
            try
            {
                var parametros = new List<SqlParameter>
                {
                    new SqlParameter("@IdPerfil", idPerfil),
                    new SqlParameter("@Nombre", nombre),
                    new SqlParameter("@Codigo", (object)codigo ?? DBNull.Value)
                };

                string lote = @"
                    DECLARE @Id INT = @IdPerfil;
                    IF @Id = 0
                    BEGIN
                        INSERT INTO [GymApp].[dbo].[Perfil] (nombre, codigo, dvh)
                        VALUES (@Nombre, @Codigo, '');
                        SET @Id = CAST(SCOPE_IDENTITY() AS INT);
                    END
                    ELSE
                        UPDATE [GymApp].[dbo].[Perfil]
                        SET nombre = @Nombre, codigo = @Codigo
                        WHERE idPerfil = @Id;

                    DELETE FROM [GymApp].[dbo].[PerfilPermiso] WHERE idPerfil = @Id;
                    DELETE FROM [GymApp].[dbo].[PerfilFamilia] WHERE idPerfil = @Id;"
                    + InsertarHijos("PerfilPermiso", "idPerfil", "idPermiso", permisos, "@Pe", parametros)
                    + InsertarHijos("PerfilFamilia", "idPerfil", "idFamilia", familias, "@Fa", parametros);

                int id = EjecutarEnTransaccion(lote, parametros);

                MPPDigitoVerificador.RecalcularDvhFilas("Perfil", "idPerfil = @Id", new SqlParameter("@Id", id));
                MPPDigitoVerificador.RecalcularDvhFilas("PerfilPermiso", "idPerfil = @Id", new SqlParameter("@Id", id));
                MPPDigitoVerificador.RecalcularDvhFilas("PerfilFamilia", "idPerfil = @Id", new SqlParameter("@Id", id));
                return id;
            }
            catch (Exception ex)
            {
                throw new Exception("Error al guardar el perfil: " + ex.Message, ex);
            }
        }

        /// <summary>
        /// Borra el perfil y sus relaciones. La BLL ya controló que no tenga usuarios.
        /// </summary>
        public void EliminarPerfil(int idPerfil)
        {
            try
            {
                EjecutarEnTransaccion(@"
                    DECLARE @Id INT = @IdPerfil;
                    DELETE FROM [GymApp].[dbo].[PerfilPermiso] WHERE idPerfil = @Id;
                    DELETE FROM [GymApp].[dbo].[PerfilFamilia] WHERE idPerfil = @Id;
                    DELETE FROM [GymApp].[dbo].[Perfil] WHERE idPerfil = @Id;",
                    new List<SqlParameter> { new SqlParameter("@IdPerfil", idPerfil) });

                MPPDigitoVerificador.SincronizarControl("PerfilPermiso", "PerfilFamilia", "Perfil");
            }
            catch (Exception ex)
            {
                throw new Exception("Error al eliminar el perfil: " + ex.Message, ex);
            }
        }

        #endregion

        #region Usuarios

        /// <summary>
        /// Asigna el perfil al usuario (reemplaza el que tenía: cada usuario tiene uno solo).
        /// </summary>
        public void AsignarPerfil(string usuario, int idPerfil)
        {
            try
            {
                EjecutarEnTransaccion(@"
                    DECLARE @Id INT = 0;
                    IF EXISTS (SELECT 1 FROM [GymApp].[dbo].[UsuarioPerfil] WHERE usr = @Usr)
                        UPDATE [GymApp].[dbo].[UsuarioPerfil] SET idPerfil = @IdPerfil WHERE usr = @Usr;
                    ELSE
                        INSERT INTO [GymApp].[dbo].[UsuarioPerfil] (usr, idPerfil, dvh) VALUES (@Usr, @IdPerfil, '');",
                    new List<SqlParameter>
                    {
                        new SqlParameter("@Usr", usuario),
                        new SqlParameter("@IdPerfil", idPerfil)
                    });

                RecalcularDvhUsuario(usuario);
            }
            catch (Exception ex)
            {
                throw new Exception("Error al asignar el perfil: " + ex.Message, ex);
            }
        }

        /// <summary>
        /// Recalcula el dvh de la asignación del usuario. Se usa después de renombrar la cuenta
        /// (la FK con ON UPDATE CASCADE cambia el usr pero no el dvh).
        /// </summary>
        public void RecalcularDvhUsuario(string usuario)
        {
            MPPDigitoVerificador.RecalcularDvhFilas("UsuarioPerfil", "usr = @Usr", new SqlParameter("@Usr", usuario));
        }

        #endregion

        /// <summary>
        /// Arma los INSERT de una tabla de relación: (@Id, @Pe0, ''), (@Id, @Pe1, '')...
        /// @Id es la variable del lote con el id del padre.
        /// </summary>
        private static string InsertarHijos(string tabla, string columnaPadre, string columnaHijo, IList<int> hijos,
            string prefijo, List<SqlParameter> parametros)
        {
            if (hijos == null || hijos.Count == 0)
                return string.Empty;

            var sql = new StringBuilder();
            sql.Append($"\nINSERT INTO [GymApp].[dbo].[{tabla}] ({columnaPadre}, {columnaHijo}, dvh) VALUES ");

            for (int i = 0; i < hijos.Count; i++)
            {
                string nombre = prefijo + i;
                parametros.Add(new SqlParameter(nombre, hijos[i]));
                sql.Append(i == 0 ? string.Empty : ", ").Append($"(@Id, {nombre}, '')");
            }

            return sql.Append(';').ToString();
        }

        /// <summary>
        /// Ejecuta el lote en una transacción (si algo falla se deshace todo y se relanza el
        /// error original) y devuelve el valor final de la variable @Id del lote.
        /// </summary>
        private int EjecutarEnTransaccion(string lote, List<SqlParameter> parametros)
        {
            object resultado = dal._686DPEscalar(@"
                SET NOCOUNT ON;
                BEGIN TRY
                    BEGIN TRANSACTION;
                    " + lote + @"
                    COMMIT TRANSACTION;
                    SELECT @Id;
                END TRY
                BEGIN CATCH
                    IF @@TRANCOUNT > 0 ROLLBACK TRANSACTION;
                    THROW;
                END CATCH",
                parametros);

            return resultado != null && resultado != DBNull.Value ? Convert.ToInt32(resultado) : 0;
        }
    }
}
