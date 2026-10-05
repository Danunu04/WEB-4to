using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using BE;
using DAL;

namespace MPP
{
    public class MPPRol
    {
        private DalGeneral dal;

        public MPPRol()
        {
            dal = new DalGeneral();
        }

        public int ObtenerRol(string usuario)
        {
            try
            {
                string consulta = @"
                    SELECT rol
                    FROM [GymApp].[dbo].[USUARIOS]
                    WHERE usr = @Usuario";

                List<SqlParameter> parametros = new List<SqlParameter>
                {
                    new SqlParameter("@Usuario", usuario)
                };

                object resultado = dal._686DPEscalar(consulta, parametros);

                if (resultado != null && resultado != DBNull.Value)
                {
                    int rolValue = Convert.ToInt32(resultado);
                    return rolValue;
                }

                return 1; // Default role
            }
            catch (Exception ex)
            {
                throw new Exception("Error al obtener el rol: " + ex.Message, ex);
            }
        }

        public void ActualizarRol(string usuario, int rol)
        {
            try
            {
                string consulta = @"
                    UPDATE [GymApp].[dbo].[USUARIOS]
                    SET rol = @Rol
                    WHERE usr = @Usuario";

                List<SqlParameter> parametros = new List<SqlParameter>
                {
                    new SqlParameter("@Usuario", usuario),
                    new SqlParameter("@Rol", (int)rol)
                };

                dal._686DPEscribir(consulta, parametros);

                // El rol forma parte del dvh del usuario: recalcular la fila y el control de USUARIOS
                new MPPUsuario().RecalcularDigitosUsuario(usuario);
            }
            catch (Exception ex)
            {
                throw new Exception("Error al actualizar el rol: " + ex.Message, ex);
            }
        }

        #region Permisos por rol (tabla RolPermiso)

        public bool TablaRolPermisoExiste()
        {
            try
            {
                object resultado = dal._686DPEscalar(
                    "SELECT CASE WHEN OBJECT_ID('[GymApp].[dbo].[RolPermiso]', 'U') IS NULL THEN 0 ELSE 1 END",
                    new List<SqlParameter>());

                return resultado != null && resultado != DBNull.Value && Convert.ToInt32(resultado) == 1;
            }
            catch (Exception ex)
            {
                throw new Exception("Error al verificar la tabla de permisos: " + ex.Message, ex);
            }
        }

        /// <summary>
        /// Devuelve los permisos asignados a cada rol según la tabla RolPermiso.
        /// </summary>
        public Dictionary<int, HashSet<string>> ListarPermisosPorRol()
        {
            try
            {
                DataTable dt = dal._686DPConsultar(
                    "SELECT rol, permiso FROM [GymApp].[dbo].[RolPermiso]",
                    new List<SqlParameter>());

                return dt.Rows.Cast<DataRow>()
                    .GroupBy(r => Convert.ToInt32(r["rol"]))
                    .ToDictionary(
                        g => g.Key,
                        g => new HashSet<string>(g.Select(r => r["permiso"].ToString()), StringComparer.OrdinalIgnoreCase));
            }
            catch (Exception ex)
            {
                throw new Exception("Error al listar los permisos por rol: " + ex.Message, ex);
            }
        }

        /// <summary>
        /// Reemplaza los permisos de un rol.
        /// </summary>
        public void GuardarPermisosRol(int rol, IEnumerable<string> permisos)
        {
            try
            {
                dal._686DPEscribir(
                    "DELETE FROM [GymApp].[dbo].[RolPermiso] WHERE rol = @Rol",
                    new List<SqlParameter> { new SqlParameter("@Rol", rol) });

                foreach (string permiso in permisos.Distinct(StringComparer.OrdinalIgnoreCase))
                {
                    dal._686DPEscribir(@"
                        INSERT INTO [GymApp].[dbo].[RolPermiso] (rol, permiso, dvh)
                        VALUES (@Rol, @Permiso, '')",
                        new List<SqlParameter>
                        {
                            new SqlParameter("@Rol", rol),
                            new SqlParameter("@Permiso", permiso)
                        });
                }

                MPPDigitoVerificador.RecalcularDvhFilas("RolPermiso", "rol = @Rol", new SqlParameter("@Rol", rol));
            }
            catch (Exception ex)
            {
                throw new Exception("Error al guardar los permisos del rol: " + ex.Message, ex);
            }
        }

        #endregion
    }
}
