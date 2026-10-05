using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using BE;
using DAL;
using SERVICIOS;

namespace MPP
{
    public class MPPAlumno
    {
        private DalGeneral dal;
        private DigitoVerificadorManager dvManager;
        private CriptoManager criptoManager;

        public MPPAlumno()
        {
            dal = new DalGeneral();
            dvManager = new DigitoVerificadorManager();
            criptoManager = new CriptoManager();
        }

        /// <summary>
        /// Pobla los datos personales de visualización (Nombre, Apellido, Teléfono,
        /// FechaNacimiento) leídos vía JOIN con USUARIOS, desencriptándolos. Esos campos
        /// se guardan encriptados desde MPPUsuario, por eso no se pueden leer crudos.
        /// </summary>
        private void PoblarDatosPersonales(Alumno alumno, DataRow row)
        {
            alumno.Nombre = row["nombre"] != DBNull.Value ? criptoManager.DesencriptarCampoPersonal(row["nombre"].ToString()) : null;
            alumno.Apellido = row["apellido"] != DBNull.Value ? criptoManager.DesencriptarCampoPersonal(row["apellido"].ToString()) : null;
            alumno.Telefono = row["telefono"] != DBNull.Value ? criptoManager.DesencriptarCampoPersonal(row["telefono"].ToString()) : null;
            alumno.FechaNacimiento = criptoManager.DesencriptarFechaPersonal(row["fechaNacimiento"]);
        }

        /// <summary>
        /// Calcula el DVH de un alumno a partir de sus valores de persistencia.
        /// </summary>
        private string CalcularDigitosAlumno(Alumno alumno)
        {
            var valores = new Dictionary<string, object>
            {
                { "dni", alumno.DNI },
                { "peso", alumno.Peso },
                { "activo", alumno.Activo },
                { "tieneRutinas", alumno.TieneRutinas }
            };

            return dvManager.CalcularDVH(valores);
        }

        /// <summary>
        /// Calcula el DVH de un vínculo Alumno-Usuario (tabla ALUMNOS_USUARIOS).
        /// </summary>
        private string CalcularDigitosVinculo(AlumnoUsuarioVinculo vinculo)
        {
            var valores = new Dictionary<string, object>
            {
                { "dniAlumno", vinculo.DniAlumno },
                { "usr", vinculo.Usuario },
                { "parentesco", vinculo.Parentesco },
                { "fechaAsociacion", vinculo.FechaAsociacion }
            };

            return dvManager.CalcularDVH(valores);
        }

        /// <summary>
        /// Vuelve a calcular y actualizar el dvh de un alumno existente.
        /// </summary>
        private void RecalcularDigitosAlumno(int dni)
        {
            Alumno alumno = ObtenerAlumno(dni);
            if (alumno == null) return;

            string dvh = CalcularDigitosAlumno(alumno);

            string consulta = @"
                UPDATE [GymApp].[dbo].[Alumnos]
                SET dvh = @DVH
                WHERE dni = @DNI";

            List<SqlParameter> parametros = new List<SqlParameter>
            {
                new SqlParameter("@DNI", dni),
                new SqlParameter("@DVH", dvh)
            };

            dal._686DPEscribir(consulta, parametros);
            MPPDigitoVerificador.SincronizarControl("ALUMNOS");
        }

        public void CrearAlumno(Alumno alumno)
        {
            try
            {
                string dvh = CalcularDigitosAlumno(alumno);

                string consulta = @"
                    INSERT INTO [GymApp].[dbo].[Alumnos]
                    (dni, peso, activo, tieneRutinas, dvh)
                    VALUES
                    (@DNI, @Peso, @Activo, @TieneRutinas, @DVH)";

                List<SqlParameter> parametros = new List<SqlParameter>
                {
                    new SqlParameter("@DNI", alumno.DNI),
                    new SqlParameter("@Peso", alumno.Peso ?? (object)DBNull.Value),
                    new SqlParameter("@Activo", alumno.Activo),
                    new SqlParameter("@TieneRutinas", alumno.TieneRutinas),
                    new SqlParameter("@DVH", dvh)
                };

                dal._686DPEscribir(consulta, parametros);
                MPPDigitoVerificador.SincronizarControl("ALUMNOS");
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
                        a.dvh,
                        u.nombre,
                        u.apellido,
                        u.telefono,
                        u.fechaNacimiento,
                        u.activo AS USUARIO_Activo
                    FROM [GymApp].[dbo].[Alumnos] a
                    LEFT JOIN [GymApp].[dbo].[USUARIOS] u ON a.dni = u.dni
                    WHERE a.dni = @DNI";

                List<SqlParameter> parametros = new List<SqlParameter>
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
                        row["dvh"] != DBNull.Value ? row["dvh"].ToString() : string.Empty
                    );
                    // Poblar datos personales desde USUARIOS (para visualización)
                    PoblarDatosPersonales(alumno, row);
                    alumno.Familiares = ObtenerUsuariosDeAlumno(dni);
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
                string dvh = CalcularDigitosAlumno(alumno);

                string consulta = @"
                    UPDATE [GymApp].[dbo].[Alumnos]
                    SET peso = @Peso,
                        activo = @Activo,
                        tieneRutinas = @TieneRutinas,
                        dvh = @DVH
                    WHERE dni = @DNI";

                List<SqlParameter> parametros = new List<SqlParameter>
                {
                    new SqlParameter("@DNI", alumno.DNI),
                    new SqlParameter("@Peso", alumno.Peso ?? (object)DBNull.Value),
                    new SqlParameter("@Activo", alumno.Activo),
                    new SqlParameter("@TieneRutinas", alumno.TieneRutinas),
                    new SqlParameter("@DVH", dvh)
                };

                dal._686DPEscribir(consulta, parametros);
                MPPDigitoVerificador.SincronizarControl("ALUMNOS");
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

                List<SqlParameter> parametros = new List<SqlParameter>
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
                        a.dvh,
                        u.nombre,
                        u.apellido,
                        u.telefono,
                        u.fechaNacimiento,
                        u.activo AS USUARIO_Activo
                    FROM [GymApp].[dbo].[Alumnos] a
                    LEFT JOIN [GymApp].[dbo].[USUARIOS] u ON a.dni = u.dni
                    ORDER BY u.apellido, u.nombre";

                List<SqlParameter> parametros = new List<SqlParameter>();

                DataTable dt = dal._686DPConsultar(consulta, parametros);
                List<Alumno> alumnos = new List<Alumno>();

                foreach (DataRow row in dt.Rows)
                {
                    Alumno alumno = new Alumno(
                        Convert.ToInt32(row["dni"]),
                        row["peso"] != DBNull.Value ? Convert.ToDecimal(row["peso"]) : (decimal?)null,
                        Convert.ToBoolean(row["tieneRutinas"]),
                        Convert.ToBoolean(row["activo"]),
                        row["dvh"] != DBNull.Value ? row["dvh"].ToString() : string.Empty
                    );
                    // Poblar datos personales desde USUARIOS (para visualización)
                    PoblarDatosPersonales(alumno, row);
                    alumnos.Add(alumno);
                }

                AdjuntarFamiliares(alumnos);

                return alumnos;
            }
            catch (Exception ex)
            {
                throw new Exception("Error al listar alumnos: " + ex.Message, ex);
            }
        }

        /// <summary>
        /// Trae de una sola vez los vínculos ALUMNOS_USUARIOS de todos los alumnos dados
        /// y los agrupa en cada Alumno.Familiares, para evitar 1 query por fila (N+1).
        /// </summary>
        private void AdjuntarFamiliares(List<Alumno> alumnos)
        {
            if (alumnos.Count == 0) return;

            string consulta = @"
                SELECT dniAlumno, usr, parentesco, fechaAsociacion, dvh
                FROM [GymApp].[dbo].[ALUMNOS_USUARIOS]";

            DataTable dt = dal._686DPConsultar(consulta, new List<SqlParameter>());

            Dictionary<int, List<AlumnoUsuarioVinculo>> porAlumno = new Dictionary<int, List<AlumnoUsuarioVinculo>>();
            foreach (DataRow row in dt.Rows)
            {
                int dniAlumno = Convert.ToInt32(row["dniAlumno"]);
                var vinculo = new AlumnoUsuarioVinculo(
                    dniAlumno,
                    row["usr"].ToString(),
                    row["parentesco"] != DBNull.Value ? row["parentesco"].ToString() : null,
                    Convert.ToDateTime(row["fechaAsociacion"]),
                    row["dvh"] != DBNull.Value ? row["dvh"].ToString() : string.Empty
                );

                if (!porAlumno.TryGetValue(dniAlumno, out var lista))
                {
                    lista = new List<AlumnoUsuarioVinculo>();
                    porAlumno[dniAlumno] = lista;
                }
                lista.Add(vinculo);
            }

            foreach (var alumno in alumnos)
            {
                alumno.Familiares = porAlumno.TryGetValue(alumno.DNI, out var lista) ? lista : new List<AlumnoUsuarioVinculo>();
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

                List<SqlParameter> parametrosRutinas = new List<SqlParameter>
                {
                    new SqlParameter("@DNI", dni)
                };

                dal._686DPEscribir(eliminarRutinas, parametrosRutinas);

                // Luego eliminar los vínculos con usuarios (familiares/titular)
                string eliminarVinculos = @"
                    DELETE FROM [GymApp].[dbo].[ALUMNOS_USUARIOS]
                    WHERE dniAlumno = @DNI";

                dal._686DPEscribir(eliminarVinculos, new List<SqlParameter> { new SqlParameter("@DNI", dni) });

                // Luego eliminar el alumno
                string eliminarAlumno = @"
                    DELETE FROM [GymApp].[dbo].[Alumnos]
                    WHERE dni = @DNI";

                List<SqlParameter> parametros = new List<SqlParameter>
                {
                    new SqlParameter("@DNI", dni)
                };

                dal._686DPEscribir(eliminarAlumno, parametros);
                MPPDigitoVerificador.SincronizarControl("Rutinas", "ALUMNOS_USUARIOS", "ALUMNOS");
            }
            catch (Exception ex)
            {
                throw new Exception("Error al eliminar alumno: " + ex.Message, ex);
            }
        }

        /// <summary>
        /// Cantidad de alumnos vinculados a una cuenta de usuario (propio titular + hijos, si aplica).
        /// </summary>
        public int CantidadAlumnosAsociados(string usuario)
        {
            try
            {
                string consulta = @"
                    SELECT COUNT(*)
                    FROM [GymApp].[dbo].[ALUMNOS_USUARIOS]
                    WHERE usr = @Usuario";

                List<SqlParameter> parametros = new List<SqlParameter>
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

        /// <summary>
        /// Vincula un usuario (titular o familiar) a un alumno. Un alumno puede tener varios
        /// usuarios vinculados y un usuario puede estar vinculado a varios alumnos.
        /// </summary>
        public void AsociarFamiliar(int dniAlumno, string usuario, string parentesco)
        {
            try
            {
                var vinculo = new AlumnoUsuarioVinculo(dniAlumno, usuario, parentesco, DateTime.Now, string.Empty);
                string dvh = CalcularDigitosVinculo(vinculo);

                string consulta = @"
                    INSERT INTO [GymApp].[dbo].[ALUMNOS_USUARIOS]
                    (dniAlumno, usr, parentesco, fechaAsociacion, dvh)
                    VALUES
                    (@DNI, @Usuario, @Parentesco, @Fecha, @DVH)";

                List<SqlParameter> parametros = new List<SqlParameter>
                {
                    new SqlParameter("@DNI", dniAlumno),
                    new SqlParameter("@Usuario", usuario),
                    new SqlParameter("@Parentesco", string.IsNullOrEmpty(parentesco) ? (object)DBNull.Value : parentesco),
                    new SqlParameter("@Fecha", vinculo.FechaAsociacion),
                    new SqlParameter("@DVH", dvh)
                };

                dal._686DPEscribir(consulta, parametros);
                MPPDigitoVerificador.SincronizarControl("ALUMNOS_USUARIOS");
            }
            catch (Exception ex)
            {
                throw new Exception("Error al asociar familiar: " + ex.Message, ex);
            }
        }

        public void DesasociarFamiliar(int dniAlumno, string usuario)
        {
            try
            {
                string consulta = @"
                    DELETE FROM [GymApp].[dbo].[ALUMNOS_USUARIOS]
                    WHERE dniAlumno = @DNI AND usr = @Usuario";

                List<SqlParameter> parametros = new List<SqlParameter>
                {
                    new SqlParameter("@DNI", dniAlumno),
                    new SqlParameter("@Usuario", usuario)
                };

                dal._686DPEscribir(consulta, parametros);
                MPPDigitoVerificador.SincronizarControl("ALUMNOS_USUARIOS");
            }
            catch (Exception ex)
            {
                throw new Exception("Error al desasociar familiar: " + ex.Message, ex);
            }
        }

        public List<AlumnoUsuarioVinculo> ObtenerUsuariosDeAlumno(int dniAlumno)
        {
            try
            {
                string consulta = @"
                    SELECT au.dniAlumno, au.usr, au.parentesco, au.fechaAsociacion, au.dvh,
                           u.nombre, u.apellido
                    FROM [GymApp].[dbo].[ALUMNOS_USUARIOS] au
                    LEFT JOIN [GymApp].[dbo].[USUARIOS] u ON au.usr = u.usr
                    WHERE au.dniAlumno = @DNI
                    ORDER BY au.fechaAsociacion";

                List<SqlParameter> parametros = new List<SqlParameter>
                {
                    new SqlParameter("@DNI", dniAlumno)
                };

                DataTable dt = dal._686DPConsultar(consulta, parametros);
                List<AlumnoUsuarioVinculo> vinculos = new List<AlumnoUsuarioVinculo>();

                foreach (DataRow row in dt.Rows)
                {
                    var vinculo = new AlumnoUsuarioVinculo(
                        Convert.ToInt32(row["dniAlumno"]),
                        row["usr"].ToString(),
                        row["parentesco"] != DBNull.Value ? row["parentesco"].ToString() : null,
                        Convert.ToDateTime(row["fechaAsociacion"]),
                        row["dvh"] != DBNull.Value ? row["dvh"].ToString() : string.Empty
                    );
                    vinculo.NombreUsuario = row["nombre"] != DBNull.Value ? criptoManager.DesencriptarCampoPersonal(row["nombre"].ToString()) : null;
                    vinculo.ApellidoUsuario = row["apellido"] != DBNull.Value ? criptoManager.DesencriptarCampoPersonal(row["apellido"].ToString()) : null;
                    vinculos.Add(vinculo);
                }

                return vinculos;
            }
            catch (Exception ex)
            {
                throw new Exception("Error al obtener los usuarios vinculados al alumno: " + ex.Message, ex);
            }
        }

        /// <summary>
        /// Alumnos vinculados a una cuenta de usuario (propio titular y/o alumnos a su cargo como familiar).
        /// </summary>
        public List<Alumno> ObtenerAlumnosDeUsuario(string usuario)
        {
            try
            {
                string consulta = @"
                    SELECT
                        a.dni,
                        a.peso,
                        a.activo,
                        a.tieneRutinas,
                        a.dvh,
                        u.nombre,
                        u.apellido,
                        u.telefono,
                        u.fechaNacimiento,
                        u.activo AS USUARIO_Activo
                    FROM [GymApp].[dbo].[ALUMNOS_USUARIOS] au
                    INNER JOIN [GymApp].[dbo].[Alumnos] a ON au.dniAlumno = a.dni
                    LEFT JOIN [GymApp].[dbo].[USUARIOS] u ON a.dni = u.dni
                    WHERE au.usr = @Usuario
                    ORDER BY u.apellido, u.nombre";

                List<SqlParameter> parametros = new List<SqlParameter>
                {
                    new SqlParameter("@Usuario", usuario)
                };

                DataTable dt = dal._686DPConsultar(consulta, parametros);
                List<Alumno> alumnos = new List<Alumno>();

                foreach (DataRow row in dt.Rows)
                {
                    Alumno alumno = new Alumno(
                        Convert.ToInt32(row["dni"]),
                        row["peso"] != DBNull.Value ? Convert.ToDecimal(row["peso"]) : (decimal?)null,
                        Convert.ToBoolean(row["tieneRutinas"]),
                        Convert.ToBoolean(row["activo"]),
                        row["dvh"] != DBNull.Value ? row["dvh"].ToString() : string.Empty
                    );
                    PoblarDatosPersonales(alumno, row);
                    alumnos.Add(alumno);
                }

                AdjuntarFamiliares(alumnos);

                return alumnos;
            }
            catch (Exception ex)
            {
                throw new Exception("Error al obtener los alumnos del usuario: " + ex.Message, ex);
            }
        }

        /// <summary>
        /// Cuando se renombra una cuenta de usuario (USUARIOS.usr), actualiza todos sus
        /// vínculos en ALUMNOS_USUARIOS para que sigan apuntando a la cuenta correcta.
        /// </summary>
        public void RenombrarUsuarioEnVinculos(string usuarioOriginal, string usuarioNuevo)
        {
            try
            {
                if (string.IsNullOrEmpty(usuarioOriginal) || usuarioOriginal == usuarioNuevo)
                    return;

                string consulta = @"
                    UPDATE [GymApp].[dbo].[ALUMNOS_USUARIOS]
                    SET usr = @Nuevo
                    WHERE usr = @Original";

                List<SqlParameter> parametros = new List<SqlParameter>
                {
                    new SqlParameter("@Nuevo", usuarioNuevo),
                    new SqlParameter("@Original", usuarioOriginal)
                };

                dal._686DPEscribir(consulta, parametros);

                // Recalcular dvh de los vínculos afectados
                foreach (var vinculo in ObtenerVinculosDeUsuario(usuarioNuevo))
                {
                    string dvh = CalcularDigitosVinculo(vinculo);
                    string actualizarDvh = @"
                        UPDATE [GymApp].[dbo].[ALUMNOS_USUARIOS]
                        SET dvh = @DVH
                        WHERE dniAlumno = @DNI AND usr = @Usuario";

                    dal._686DPEscribir(actualizarDvh, new List<SqlParameter>
                    {
                        new SqlParameter("@DVH", dvh),
                        new SqlParameter("@DNI", vinculo.DniAlumno),
                        new SqlParameter("@Usuario", usuarioNuevo)
                    });
                }

                MPPDigitoVerificador.SincronizarControl("ALUMNOS_USUARIOS");
            }
            catch (Exception ex)
            {
                throw new Exception("Error al renombrar el usuario en los vínculos de alumnos: " + ex.Message, ex);
            }
        }

        private List<AlumnoUsuarioVinculo> ObtenerVinculosDeUsuario(string usuario)
        {
            string consulta = @"
                SELECT dniAlumno, usr, parentesco, fechaAsociacion, dvh
                FROM [GymApp].[dbo].[ALUMNOS_USUARIOS]
                WHERE usr = @Usuario";

            DataTable dt = dal._686DPConsultar(consulta, new List<SqlParameter> { new SqlParameter("@Usuario", usuario) });
            List<AlumnoUsuarioVinculo> vinculos = new List<AlumnoUsuarioVinculo>();

            foreach (DataRow row in dt.Rows)
            {
                vinculos.Add(new AlumnoUsuarioVinculo(
                    Convert.ToInt32(row["dniAlumno"]),
                    row["usr"].ToString(),
                    row["parentesco"] != DBNull.Value ? row["parentesco"].ToString() : null,
                    Convert.ToDateTime(row["fechaAsociacion"]),
                    row["dvh"] != DBNull.Value ? row["dvh"].ToString() : string.Empty
                ));
            }

            return vinculos;
        }
    }
}
