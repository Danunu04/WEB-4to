using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using BE;
using DAL;

namespace MPP
{
    public class MPPAlumno
    {
        private DalGeneral dal;

        public MPPAlumno()
        {
            dal = new DalGeneral();
        }

        public void CrearAlumno(Alumno alumno)
        {
            try
            {
                // En esquema normalizado, ALUMNOS solo tiene dni, peso, activo, tieneRutinas, dvv, dvh
                // Los datos personales están en USUARIOS
                string consulta = @"
                    INSERT INTO [GymApp].[dbo].[Alumnos]
                    (dni, peso, activo, tieneRutinas, dvv, dvh)
                    VALUES
                    (@DNI, @Peso, @Activo, @TieneRutinas, '', '')";

                ArrayList parametros = new ArrayList
                {
                    new SqlParameter("@DNI", alumno.DNI),
                    new SqlParameter("@Peso", alumno.Peso ?? (object)DBNull.Value),
                    new SqlParameter("@Activo", alumno.Activo),
                    new SqlParameter("@TieneRutinas", alumno.TieneRutinas),
                };

                dal._686DPEscribir(consulta, parametros);
            }
            catch (Exception ex)
            {
                throw new Exception("Error al crear alumno: " + ex.Message, ex);
            }
        }

        public Alumno ObtenerAlumno(int dni)
        {
            try
            {
                // En esquema normalizado, los datos personales están en USUARIOS
                string consulta = @"
                    SELECT
                        a.dni,
                        a.peso,
                        a.activo,
                        a.tieneRutinas,
                        a.usr,
                        a.dvv,
                        a.dvh,
                        u.nombre,
                        u.apellido,
                        u.telefono,
                        u.fechaNacimiento,
                        u.activo AS USUARIO_Activo
                    FROM [GymApp].[dbo].[Alumnos] a
                    LEFT JOIN [GymApp].[dbo].[USUARIOS] u ON a.dni = u.dni
                    WHERE a.dni = @DNI";

                ArrayList parametros = new ArrayList
                {
                    new SqlParameter("@DNI", dni)
                };

                DataTable dt = dal._686DPConsultar(consulta, parametros);

                if (dt.Rows.Count > 0)
                {
                    DataRow row = dt.Rows[0];
                    Alumno alumno = new Alumno(
                        Convert.ToInt32(row["dni"]),
                        row["peso"] != DBNull.Value ? Convert.ToDecimal(row["peso"]) : (decimal?)null,
                        Convert.ToBoolean(row["tieneRutinas"]),
                        Convert.ToBoolean(row["activo"]),
                        row["dvv"] != DBNull.Value ? row["dvv"].ToString() : string.Empty,
                        row["dvh"] != DBNull.Value ? row["dvh"].ToString() : string.Empty,
                        row["usr"] != DBNull.Value ? row["usr"].ToString() : string.Empty
                    );
                    // Poblar datos personales desde USUARIOS (para visualización)
                    alumno.Nombre = row["nombre"] != DBNull.Value ? row["nombre"].ToString() : null;
                    alumno.Apellido = row["apellido"] != DBNull.Value ? row["apellido"].ToString() : null;
                    alumno.Telefono = row["telefono"] != DBNull.Value ? row["telefono"].ToString() : null;
                    alumno.FechaNacimiento = row["fechaNacimiento"] != DBNull.Value ? (DateTime?)Convert.ToDateTime(row["fechaNacimiento"]) : null;
                    return alumno;
                }

                return null;
            }
            catch (Exception ex)
            {
                throw new Exception("Error al obtener alumno: " + ex.Message, ex);
            }
        }

        public void ActualizarAlumno(Alumno alumno)
        {
            try
            {
                // En esquema normalizado, ALUMNOS solo tiene campos específicos del rol
                string consulta = @"
                    UPDATE [GymApp].[dbo].[Alumnos]
                    SET peso = @Peso,
                        activo = @Activo,
                        tieneRutinas = @TieneRutinas,
                        usr = @Usuario
                    WHERE dni = @DNI";

                ArrayList parametros = new ArrayList
                {
                    new SqlParameter("@DNI", alumno.DNI),
                    new SqlParameter("@Peso", alumno.Peso ?? (object)DBNull.Value),
                    new SqlParameter("@Activo", alumno.Activo),
                    new SqlParameter("@TieneRutinas", alumno.TieneRutinas),
                    new SqlParameter("@Usuario", alumno.Usuario ?? (object)DBNull.Value)
                };

                dal._686DPEscribir(consulta, parametros);
            }
            catch (Exception ex)
            {
                throw new Exception("Error al actualizar alumno: " + ex.Message, ex);
            }
        }

        public bool AlumnoExiste(int dni)
        {
            try
            {
                string consulta = @"
                    SELECT COUNT(*)
                    FROM [GymApp].[dbo].[Alumnos]
                    WHERE dni = @DNI";

                ArrayList parametros = new ArrayList
                {
                    new SqlParameter("@DNI", dni)
                };

                object resultado = dal._686DPEscalar(consulta, parametros);

                if (resultado != null && resultado != DBNull.Value)
                {
                    return Convert.ToInt32(resultado) > 0;
                }

                return false;
            }
            catch (Exception ex)
            {
                throw new Exception("Error al verificar si existe el alumno: " + ex.Message, ex);
            }
        }

        public List<Alumno> ListarAlumnos()
        {
            try
            {
                // En esquema normalizado, los datos personales están en USUARIOS
                string consulta = @"
                    SELECT
                        a.dni,
                        a.peso,
                        a.activo,
                        a.tieneRutinas,
                        a.usr,
                        a.dvv,
                        a.dvh,
                        u.nombre,
                        u.apellido,
                        u.telefono,
                        u.fechaNacimiento,
                        u.activo AS USUARIO_Activo
                    FROM [GymApp].[dbo].[Alumnos] a
                    LEFT JOIN [GymApp].[dbo].[USUARIOS] u ON a.dni = u.dni
                    ORDER BY u.apellido, u.nombre";

                ArrayList parametros = new ArrayList();

                DataTable dt = dal._686DPConsultar(consulta, parametros);
                List<Alumno> alumnos = new List<Alumno>();

                foreach (DataRow row in dt.Rows)
                {
                    Alumno alumno = new Alumno(
                        Convert.ToInt32(row["dni"]),
                        row["peso"] != DBNull.Value ? Convert.ToDecimal(row["peso"]) : (decimal?)null,
                        Convert.ToBoolean(row["tieneRutinas"]),
                        Convert.ToBoolean(row["activo"]),
                        row["dvv"] != DBNull.Value ? row["dvv"].ToString() : string.Empty,
                        row["dvh"] != DBNull.Value ? row["dvh"].ToString() : string.Empty,
                        row["usr"] != DBNull.Value ? row["usr"].ToString() : string.Empty
                    );
                    // Poblar datos personales desde USUARIOS (para visualización)
                    alumno.Nombre = row["nombre"] != DBNull.Value ? row["nombre"].ToString() : null;
                    alumno.Apellido = row["apellido"] != DBNull.Value ? row["apellido"].ToString() : null;
                    alumno.Telefono = row["telefono"] != DBNull.Value ? row["telefono"].ToString() : null;
                    alumno.FechaNacimiento = row["fechaNacimiento"] != DBNull.Value ? (DateTime?)Convert.ToDateTime(row["fechaNacimiento"]) : null;
                    alumnos.Add(alumno);
                }

                return alumnos;
            }
            catch (Exception ex)
            {
                throw new Exception("Error al listar alumnos: " + ex.Message, ex);
            }
        }

        public void EliminarAlumno(int dni)
        {
            try
            {
                // Primero eliminar rutinas asociadas (cascada manual)
                string eliminarRutinas = @"
                    DELETE FROM [GymApp].[dbo].[Rutinas]
                    WHERE dniAlumno = @DNI";

                ArrayList parametrosRutinas = new ArrayList
                {
                    new SqlParameter("@DNI", dni)
                };

                dal._686DPEscribir(eliminarRutinas, parametrosRutinas);

                // Luego eliminar el alumno
                string eliminarAlumno = @"
                    DELETE FROM [GymApp].[dbo].[Alumnos]
                    WHERE dni = @DNI";

                ArrayList parametros = new ArrayList
                {
                    new SqlParameter("@DNI", dni)
                };

                dal._686DPEscribir(eliminarAlumno, parametros);
            }
            catch (Exception ex)
            {
                throw new Exception("Error al eliminar alumno: " + ex.Message, ex);
            }
        }

        public List<Alumno> ListarAlumnosSinUsuario()
        {
            try
            {
                // En esquema normalizado, buscar ALUMNOS sin usuario asociado
                string consulta = @"
                    SELECT
                        a.dni,
                        a.peso,
                        a.activo,
                        a.tieneRutinas,
                        a.usr,
                        a.dvv,
                        a.dvh,
                        u.nombre,
                        u.apellido,
                        u.telefono,
                        u.fechaNacimiento
                    FROM [GymApp].[dbo].[Alumnos] a
                    LEFT JOIN [GymApp].[dbo].[USUARIOS] u ON a.dni = u.dni
                    WHERE (a.usr IS NULL OR a.usr = '')
                    ORDER BY u.apellido, u.nombre";

                ArrayList parametros = new ArrayList();
                DataTable dt = dal._686DPConsultar(consulta, parametros);
                List<Alumno> alumnos = new List<Alumno>();

                foreach (DataRow row in dt.Rows)
                {
                    Alumno alumno = new Alumno(
                        Convert.ToInt32(row["dni"]),
                        row["peso"] != DBNull.Value ? Convert.ToDecimal(row["peso"]) : (decimal?)null,
                        Convert.ToBoolean(row["tieneRutinas"]),
                        Convert.ToBoolean(row["activo"]),
                        row["dvv"] != DBNull.Value ? row["dvv"].ToString() : string.Empty,
                        row["dvh"] != DBNull.Value ? row["dvh"].ToString() : string.Empty,
                        row["usr"] != DBNull.Value ? row["usr"].ToString() : string.Empty
                    );
                    // Poblar datos personales desde USUARIOS (para visualización)
                    alumno.Nombre = row["nombre"] != DBNull.Value ? row["nombre"].ToString() : null;
                    alumno.Apellido = row["apellido"] != DBNull.Value ? row["apellido"].ToString() : null;
                    alumno.Telefono = row["telefono"] != DBNull.Value ? row["telefono"].ToString() : null;
                    alumno.FechaNacimiento = row["fechaNacimiento"] != DBNull.Value ? (DateTime?)Convert.ToDateTime(row["fechaNacimiento"]) : null;
                    alumnos.Add(alumno);
                }

                return alumnos;
            }
            catch (Exception ex)
            {
                throw new Exception("Error al listar alumnos sin usuario: " + ex.Message, ex);
            }
        }

        public int CantidadAlumnosAsociados(string usuario)
        {
            try
            {
                string consulta = @"
                    SELECT COUNT(*)
                    FROM [GymApp].[dbo].[Alumnos]
                    WHERE usr = @Usuario";

                ArrayList parametros = new ArrayList
                {
                    new SqlParameter("@Usuario", usuario)
                };

                object resultado = dal._686DPEscalar(consulta, parametros);

                if (resultado != null && resultado != DBNull.Value)
                {
                    return Convert.ToInt32(resultado);
                }

                return 0;
            }
            catch (Exception ex)
            {
                throw new Exception("Error al obtener la cantidad de alumnos asociados: " + ex.Message, ex);
            }
        }

        public void AsociarUsuario(int dni, string usuario)
        {
            try
            {
                string consulta = @"
                    UPDATE [GymApp].[dbo].[Alumnos]
                    SET usr = @Usuario
                    WHERE dni = @DNI";

                ArrayList parametros = new ArrayList
                {
                    new SqlParameter("@DNI", dni),
                    new SqlParameter("@Usuario", string.IsNullOrEmpty(usuario) ? (object)DBNull.Value : usuario)
                };

                dal._686DPEscribir(consulta, parametros);
            }
            catch (Exception ex)
            {
                throw new Exception("Error al asociar usuario: " + ex.Message, ex);
            }
        }
    }
}