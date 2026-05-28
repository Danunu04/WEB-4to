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
                string consulta = @"
                    INSERT INTO [GymApp].[dbo].[Alumnos]
                    (dni, nombre, apellido, telefono, fechaNacimiento, peso, activo, usr, dvv, dvh)
                    VALUES
                    (@DNI, @Nombre, @Apellido, @Telefono, @FechaNacimiento, @Peso, @Activo, @Usuario, '', '')";

                ArrayList parametros = new ArrayList
                {
                    new SqlParameter("@DNI", alumno.DNI),
                    new SqlParameter("@Nombre", alumno.Nombre),
                    new SqlParameter("@Apellido", alumno.Apellido),
                    new SqlParameter("@Telefono", alumno.Telefono ?? (object)DBNull.Value),
                    new SqlParameter("@FechaNacimiento", alumno.FechaNacimiento),
                    new SqlParameter("@Peso", alumno.Peso ?? (object)DBNull.Value),
                    new SqlParameter("@Activo", alumno.Activo),
                    new SqlParameter("@Usuario", alumno.Usuario ?? (object)DBNull.Value)
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
                string consulta = @"
                    SELECT
                        a.dni,
                        a.nombre,
                        a.apellido,
                        a.telefono,
                        a.fechaNacimiento,
                        a.peso,
                        a.usr,
                        ISNULL(u.USUARIO_Activo, 0) AS activo,
                        CASE WHEN EXISTS (
                            SELECT 1 FROM [GymApp].[dbo].[Rutinas] r
                            WHERE r.dniAlumno = a.dni
                        ) THEN 1 ELSE 0 END AS tieneRutinas,
                        a.dvv,
                        a.dvh
                    FROM [GymApp].[dbo].[Alumnos] a
                    LEFT JOIN [GymApp].[dbo].[USUARIOS] u ON a.usr = u.usr
                    WHERE a.dni = @DNI";

                ArrayList parametros = new ArrayList
                {
                    new SqlParameter("@DNI", dni)
                };

                DataTable dt = dal._686DPConsultar(consulta, parametros);

                if (dt.Rows.Count > 0)
                {
                    DataRow row = dt.Rows[0];
                    return new Alumno(
                        Convert.ToInt32(row["dni"]),
                        row["nombre"].ToString(),
                        row["apellido"].ToString(),
                        row["telefono"] != DBNull.Value ? Convert.ToInt64(row["telefono"]) : (long?)null,
                        Convert.ToDateTime(row["fechaNacimiento"]),
                        row["peso"] != DBNull.Value ? Convert.ToDecimal(row["peso"]) : (decimal?)null,
                        row["usr"] != DBNull.Value ? row["usr"].ToString() : string.Empty,
                        Convert.ToBoolean(row["activo"]),
                        Convert.ToBoolean(row["tieneRutinas"])
                    );
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
                string consulta = @"
                    UPDATE [GymApp].[dbo].[Alumnos]
                    SET nombre = @Nombre,
                        apellido = @Apellido,
                        telefono = @Telefono,
                        fechaNacimiento = @FechaNacimiento,
                        peso = @Peso,
                        activo = @Activo,
                        usr = @Usuario
                    WHERE dni = @DNI";

                ArrayList parametros = new ArrayList
                {
                    new SqlParameter("@DNI", alumno.DNI),
                    new SqlParameter("@Nombre", alumno.Nombre),
                    new SqlParameter("@Apellido", alumno.Apellido),
                    new SqlParameter("@Telefono", alumno.Telefono ?? (object)DBNull.Value),
                    new SqlParameter("@FechaNacimiento", alumno.FechaNacimiento),
                    new SqlParameter("@Peso", alumno.Peso ?? (object)DBNull.Value),
                    new SqlParameter("@Activo", alumno.Activo),
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
                string consulta = @"
                    SELECT
                        a.dni,
                        a.nombre,
                        a.apellido,
                        a.telefono,
                        a.fechaNacimiento,
                        a.peso,
                        a.usr,
                        ISNULL(u.activo, 0) AS activo,
                        CASE WHEN EXISTS (
                            SELECT 1 FROM [GymApp].[dbo].[Rutinas] r
                            WHERE r.dniAlumno = a.dni
                        ) THEN 1 ELSE 0 END AS tieneRutinas,
                        a.dvv,
                        a.dvh
                    FROM [GymApp].[dbo].[Alumnos] a
                    LEFT JOIN [GymApp].[dbo].[USUARIOS] u ON a.usr = u.usr
                    ORDER BY a.apellido, a.nombre";

                ArrayList parametros = new ArrayList();

                DataTable dt = dal._686DPConsultar(consulta, parametros);
                List<Alumno> alumnos = new List<Alumno>();

                foreach (DataRow row in dt.Rows)
                {
                    alumnos.Add(new Alumno(
                        Convert.ToInt32(row["dni"]),
                        row["nombre"].ToString(),
                        row["apellido"].ToString(),
                        row["telefono"] != DBNull.Value ? Convert.ToInt64(row["telefono"]) : (long?)null,
                        Convert.ToDateTime(row["fechaNacimiento"]),
                        row["peso"] != DBNull.Value ? Convert.ToDecimal(row["peso"]) : (decimal?)null,
                        row["usr"] != DBNull.Value ? row["usr"].ToString() : string.Empty,
                        Convert.ToBoolean(row["activo"]),
                        Convert.ToBoolean(row["tieneRutinas"])
                    ));
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
                string consulta = @"
                    SELECT
                        a.dni,
                        a.nombre,
                        a.apellido,
                        a.telefono,
                        a.fechaNacimiento,
                        a.peso,
                        a.usr,
                        0 AS activo,
                        0 AS tieneRutinas,
                        a.dvv,
                        a.dvh
                    FROM [GymApp].[dbo].[Alumnos] a
                    WHERE a.usr IS NULL OR a.usr = ''
                    ORDER BY a.apellido, a.nombre";

                ArrayList parametros = new ArrayList();
                DataTable dt = dal._686DPConsultar(consulta, parametros);
                List<Alumno> alumnos = new List<Alumno>();

                foreach (DataRow row in dt.Rows)
                {
                    alumnos.Add(new Alumno(
                        Convert.ToInt32(row["dni"]),
                        row["nombre"].ToString(),
                        row["apellido"].ToString(),
                        row["telefono"] != DBNull.Value ? Convert.ToInt64(row["telefono"]) : (long?)null,
                        Convert.ToDateTime(row["fechaNacimiento"]),
                        row["peso"] != DBNull.Value ? Convert.ToDecimal(row["peso"]) : (decimal?)null,
                        row["usr"] != DBNull.Value ? row["usr"].ToString() : string.Empty,
                        false,
                        false
                    ));
                }

                return alumnos;
            }
            catch (Exception ex)
            {
                throw new Exception("Error al listar alumnos sin usuario: " + ex.Message, ex);
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