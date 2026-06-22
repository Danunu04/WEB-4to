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

        private Alumno MapearAlumno(DataRow row)
        {
            Alumno alumno = new Alumno(
                Convert.ToInt32(row["dni"]),
                row["peso"] != DBNull.Value ? Convert.ToDecimal(row["peso"]) : (decimal?)null,
                false,
                Convert.ToBoolean(row["activo"]),
                row["dvv"] != DBNull.Value ? row["dvv"].ToString() : string.Empty,
                row["dvh"] != DBNull.Value ? row["dvh"].ToString() : string.Empty,
                row["usr"] != DBNull.Value ? row["usr"].ToString() : string.Empty
            );
            alumno.Nombre = row["nombre"] != DBNull.Value ? row["nombre"].ToString() : null;
            alumno.Apellido = row["apellido"] != DBNull.Value ? row["apellido"].ToString() : null;
            alumno.Telefono = row["telefono"] != DBNull.Value ? row["telefono"].ToString() : null;
            alumno.FechaNacimiento = row["fechaNacimiento"] != DBNull.Value ? (DateTime?)Convert.ToDateTime(row["fechaNacimiento"]) : null;
            alumno.DiasRestantes = row["diasRestantes"] != DBNull.Value ? (int?)Convert.ToInt32(row["diasRestantes"]) : null;
            alumno.FechaVencimiento = row["fechaVencimiento"] != DBNull.Value ? (DateTime?)Convert.ToDateTime(row["fechaVencimiento"]) : null;
            alumno.IdModalidad = row["idModalidad"] != DBNull.Value ? (int?)Convert.ToInt32(row["idModalidad"]) : null;
            return alumno;
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
                    new SqlParameter("@Nombre", alumno.Nombre ?? (object)DBNull.Value),
                    new SqlParameter("@Apellido", alumno.Apellido ?? (object)DBNull.Value),
                    new SqlParameter("@Telefono", alumno.Telefono ?? (object)DBNull.Value),
                    new SqlParameter("@FechaNacimiento", alumno.FechaNacimiento ?? (object)DBNull.Value),
                    new SqlParameter("@Peso", alumno.Peso ?? (object)DBNull.Value),
                    new SqlParameter("@Activo", alumno.Activo),
                    new SqlParameter("@Usuario", string.IsNullOrEmpty(alumno.Usuario) ? (object)DBNull.Value : alumno.Usuario),
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
                    SELECT dni, nombre, apellido, telefono, fechaNacimiento,
                           peso, activo, usr, dvv, dvh,
                           diasRestantes, fechaVencimiento, idModalidad
                    FROM [GymApp].[dbo].[Alumnos]
                    WHERE dni = @DNI";

                ArrayList parametros = new ArrayList
                {
                    new SqlParameter("@DNI", dni)
                };

                DataTable dt = dal._686DPConsultar(consulta, parametros);

                return dt.Rows.Count > 0 ? MapearAlumno(dt.Rows[0]) : null;
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
                    new SqlParameter("@Nombre", alumno.Nombre ?? (object)DBNull.Value),
                    new SqlParameter("@Apellido", alumno.Apellido ?? (object)DBNull.Value),
                    new SqlParameter("@Telefono", alumno.Telefono ?? (object)DBNull.Value),
                    new SqlParameter("@FechaNacimiento", alumno.FechaNacimiento ?? (object)DBNull.Value),
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
                return resultado != null && resultado != DBNull.Value && Convert.ToInt32(resultado) > 0;
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
                    SELECT dni, nombre, apellido, telefono, fechaNacimiento,
                           peso, activo, usr, dvv, dvh,
                           diasRestantes, fechaVencimiento, idModalidad
                    FROM [GymApp].[dbo].[Alumnos]
                    ORDER BY apellido, nombre";

                DataTable dt = dal._686DPConsultar(consulta, new ArrayList());
                List<Alumno> alumnos = new List<Alumno>();

                foreach (DataRow row in dt.Rows)
                    alumnos.Add(MapearAlumno(row));

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
                string eliminarRutinas = @"DELETE FROM [GymApp].[dbo].[Rutinas] WHERE dniAlumno = @DNI";
                ArrayList p = new ArrayList { new SqlParameter("@DNI", dni) };
                dal._686DPEscribir(eliminarRutinas, p);

                string eliminarAlumno = @"DELETE FROM [GymApp].[dbo].[Alumnos] WHERE dni = @DNI";
                ArrayList p2 = new ArrayList { new SqlParameter("@DNI", dni) };
                dal._686DPEscribir(eliminarAlumno, p2);
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
                    SELECT dni, nombre, apellido, telefono, fechaNacimiento,
                           peso, activo, usr, dvv, dvh,
                           diasRestantes, fechaVencimiento, idModalidad
                    FROM [GymApp].[dbo].[Alumnos]
                    WHERE (usr IS NULL OR usr = '')
                    ORDER BY apellido, nombre";

                DataTable dt = dal._686DPConsultar(consulta, new ArrayList());
                List<Alumno> alumnos = new List<Alumno>();

                foreach (DataRow row in dt.Rows)
                    alumnos.Add(MapearAlumno(row));

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
                    SELECT COUNT(*) FROM [GymApp].[dbo].[Alumnos] WHERE usr = @Usuario";

                ArrayList parametros = new ArrayList { new SqlParameter("@Usuario", usuario) };
                object resultado = dal._686DPEscalar(consulta, parametros);

                return resultado != null && resultado != DBNull.Value ? Convert.ToInt32(resultado) : 0;
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
                string consulta = @"UPDATE [GymApp].[dbo].[Alumnos] SET usr = @Usuario WHERE dni = @DNI";

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

        public void RealizarCheckin(int dni)
        {
            try
            {
                string consulta = @"
                    UPDATE [GymApp].[dbo].[Alumnos]
                    SET diasRestantes = CASE WHEN diasRestantes IS NOT NULL THEN diasRestantes - 1 ELSE NULL END
                    WHERE dni = @DNI";

                ArrayList parametros = new ArrayList { new SqlParameter("@DNI", dni) };
                dal._686DPEscribir(consulta, parametros);
            }
            catch (Exception ex)
            {
                throw new Exception("Error al realizar check-in: " + ex.Message, ex);
            }
        }
    }
}
