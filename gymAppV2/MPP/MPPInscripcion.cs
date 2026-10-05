using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using BE;
using DAL;

namespace MPP
{
    /// <summary>
    /// Acceso a datos de la inscripción a clases: inscripción fija por turno (InscripcionFija)
    /// y excepciones puntuales por fecha (InscripcionClase).
    ///
    /// Las escrituras insertan con dvh vacío y después recalculan el dvh desde la fila real
    /// (MPPDigitoVerificador.RecalcularDvhFilas). Después de un DELETE se sincroniza el control.
    /// </summary>
    public class MPPInscripcion
    {
        private DalGeneral dal;

        public MPPInscripcion()
        {
            dal = new DalGeneral();
        }

        private static SqlParameter Fecha(string nombre, DateTime fecha)
        {
            return new SqlParameter(nombre, SqlDbType.Date) { Value = fecha.Date };
        }

        #region Consultas

        /// <summary>
        /// Inscripciones fijas de un alumno (todas, incluidas las ya terminadas).
        /// </summary>
        public List<InscripcionFija> ListarFijasDeAlumno(int dni)
        {
            try
            {
                DataTable dt = dal._686DPConsultar(@"
                    SELECT codInscripcion, dni, codHorario, fechaDesde, fechaHasta, dvh
                    FROM [GymApp].[dbo].[InscripcionFija]
                    WHERE dni = @Dni",
                    new List<SqlParameter> { new SqlParameter("@Dni", dni) });

                return MapearFijas(dt);
            }
            catch (Exception ex)
            {
                throw new Exception("Error al listar las inscripciones fijas del alumno: " + ex.Message, ex);
            }
        }

        /// <summary>
        /// Excepciones puntuales (altas y bajas) de un alumno.
        /// </summary>
        public List<InscripcionClase> ListarClasesDeAlumno(int dni)
        {
            try
            {
                DataTable dt = dal._686DPConsultar(@"
                    SELECT codInscripcionClase, dni, codHorario, fecha, tipo, dvh
                    FROM [GymApp].[dbo].[InscripcionClase]
                    WHERE dni = @Dni",
                    new List<SqlParameter> { new SqlParameter("@Dni", dni) });

                return MapearClases(dt);
            }
            catch (Exception ex)
            {
                throw new Exception("Error al listar las clases del alumno: " + ex.Message, ex);
            }
        }

        /// <summary>
        /// Inscripciones fijas de un turno que cubren la fecha indicada.
        /// </summary>
        public List<InscripcionFija> ListarFijasDeHorario(int codHorario, DateTime fecha)
        {
            try
            {
                DataTable dt = dal._686DPConsultar(@"
                    SELECT codInscripcion, dni, codHorario, fechaDesde, fechaHasta, dvh
                    FROM [GymApp].[dbo].[InscripcionFija]
                    WHERE codHorario = @Horario
                      AND fechaDesde <= @Fecha
                      AND (fechaHasta IS NULL OR fechaHasta >= @Fecha)",
                    new List<SqlParameter>
                    {
                        new SqlParameter("@Horario", codHorario),
                        Fecha("@Fecha", fecha)
                    });

                return MapearFijas(dt);
            }
            catch (Exception ex)
            {
                throw new Exception("Error al listar las inscripciones fijas del turno: " + ex.Message, ex);
            }
        }

        /// <summary>
        /// Excepciones puntuales de un turno en una fecha.
        /// </summary>
        public List<InscripcionClase> ListarClasesDeHorario(int codHorario, DateTime fecha)
        {
            try
            {
                DataTable dt = dal._686DPConsultar(@"
                    SELECT codInscripcionClase, dni, codHorario, fecha, tipo, dvh
                    FROM [GymApp].[dbo].[InscripcionClase]
                    WHERE codHorario = @Horario AND fecha = @Fecha",
                    new List<SqlParameter>
                    {
                        new SqlParameter("@Horario", codHorario),
                        Fecha("@Fecha", fecha)
                    });

                return MapearClases(dt);
            }
            catch (Exception ex)
            {
                throw new Exception("Error al listar las clases del turno: " + ex.Message, ex);
            }
        }

        /// <summary>
        /// Alumnos que van (o pueden ir) al turno desde la fecha indicada: con inscripción fija
        /// vigente en esa fecha o después, o con un alta puntual en esa fecha o después.
        /// </summary>
        public List<int> ListarAlumnosConInscripcionDesde(int codHorario, DateTime desde)
        {
            try
            {
                DataTable dt = dal._686DPConsultar(@"
                    SELECT dni FROM [GymApp].[dbo].[InscripcionFija]
                    WHERE codHorario = @Horario AND (fechaHasta IS NULL OR fechaHasta >= @Desde)
                    UNION
                    SELECT dni FROM [GymApp].[dbo].[InscripcionClase]
                    WHERE codHorario = @Horario AND tipo = 'A' AND fecha >= @Desde",
                    new List<SqlParameter>
                    {
                        new SqlParameter("@Horario", codHorario),
                        Fecha("@Desde", desde)
                    });

                return dt.Rows.Cast<DataRow>().Select(r => Convert.ToInt32(r["dni"])).ToList();
            }
            catch (Exception ex)
            {
                throw new Exception("Error al listar los alumnos del turno: " + ex.Message, ex);
            }
        }

        /// <summary>
        /// Fechas desde las que puede subir la cantidad de anotados de un turno: inicios de
        /// inscripciones fijas y altas puntuales, desde la fecha indicada.
        /// </summary>
        public List<DateTime> ListarFechasDeAltaDesde(int codHorario, DateTime desde)
        {
            try
            {
                DataTable dt = dal._686DPConsultar(@"
                    SELECT fechaDesde AS fecha FROM [GymApp].[dbo].[InscripcionFija]
                    WHERE codHorario = @Horario AND fechaDesde >= @Desde
                    UNION
                    SELECT fecha FROM [GymApp].[dbo].[InscripcionClase]
                    WHERE codHorario = @Horario AND tipo = 'A' AND fecha >= @Desde",
                    new List<SqlParameter>
                    {
                        new SqlParameter("@Horario", codHorario),
                        Fecha("@Desde", desde)
                    });

                return dt.Rows.Cast<DataRow>().Select(r => Convert.ToDateTime(r["fecha"]).Date).ToList();
            }
            catch (Exception ex)
            {
                throw new Exception("Error al listar las fechas de alta del turno: " + ex.Message, ex);
            }
        }

        /// <summary>
        /// Inscripciones fijas de todos los turnos que siguen vigentes en la fecha indicada o después.
        /// </summary>
        public List<InscripcionFija> ListarFijasVigentesDesde(DateTime desde)
        {
            try
            {
                DataTable dt = dal._686DPConsultar(@"
                    SELECT codInscripcion, dni, codHorario, fechaDesde, fechaHasta, dvh
                    FROM [GymApp].[dbo].[InscripcionFija]
                    WHERE fechaHasta IS NULL OR fechaHasta >= @Desde",
                    new List<SqlParameter> { Fecha("@Desde", desde) });

                return MapearFijas(dt);
            }
            catch (Exception ex)
            {
                throw new Exception("Error al listar las inscripciones fijas: " + ex.Message, ex);
            }
        }

        /// <summary>
        /// Altas y bajas puntuales de todos los turnos desde la fecha indicada.
        /// </summary>
        public List<InscripcionClase> ListarClasesDesde(DateTime desde)
        {
            try
            {
                DataTable dt = dal._686DPConsultar(@"
                    SELECT codInscripcionClase, dni, codHorario, fecha, tipo, dvh
                    FROM [GymApp].[dbo].[InscripcionClase]
                    WHERE fecha >= @Desde",
                    new List<SqlParameter> { Fecha("@Desde", desde) });

                return MapearClases(dt);
            }
            catch (Exception ex)
            {
                throw new Exception("Error al listar las clases: " + ex.Message, ex);
            }
        }

        #endregion

        #region Escrituras

        /// <summary>
        /// Crea una inscripción fija al turno desde la fecha indicada, sin fecha de fin.
        /// </summary>
        public void CrearFija(int dni, int codHorario, DateTime fechaDesde)
        {
            try
            {
                dal._686DPEscribir(@"
                    INSERT INTO [GymApp].[dbo].[InscripcionFija] (dni, codHorario, fechaDesde, fechaHasta, dvh)
                    VALUES (@Dni, @Horario, @Desde, NULL, '')",
                    new List<SqlParameter>
                    {
                        new SqlParameter("@Dni", dni),
                        new SqlParameter("@Horario", codHorario),
                        Fecha("@Desde", fechaDesde)
                    });

                MPPDigitoVerificador.RecalcularDvhFilas("InscripcionFija", "dni = @Dni AND codHorario = @Horario",
                    new SqlParameter("@Dni", dni), new SqlParameter("@Horario", codHorario));
            }
            catch (Exception ex)
            {
                throw new Exception("Error al crear la inscripción fija: " + ex.Message, ex);
            }
        }

        /// <summary>
        /// Termina las inscripciones fijas del alumno al turno a partir de la fecha indicada:
        /// las que empiezan en esa fecha o después se borran, y las que la cubren pasan a terminar el día anterior.
        /// </summary>
        public void CerrarFijasDesde(int dni, int codHorario, DateTime fecha)
        {
            try
            {
                dal._686DPEscribir(@"
                    DELETE FROM [GymApp].[dbo].[InscripcionFija]
                    WHERE dni = @Dni AND codHorario = @Horario AND fechaDesde >= @Fecha",
                    new List<SqlParameter>
                    {
                        new SqlParameter("@Dni", dni),
                        new SqlParameter("@Horario", codHorario),
                        Fecha("@Fecha", fecha)
                    });

                dal._686DPEscribir(@"
                    UPDATE [GymApp].[dbo].[InscripcionFija]
                    SET fechaHasta = DATEADD(DAY, -1, @Fecha)
                    WHERE dni = @Dni AND codHorario = @Horario
                      AND fechaDesde < @Fecha
                      AND (fechaHasta IS NULL OR fechaHasta >= @Fecha)",
                    new List<SqlParameter>
                    {
                        new SqlParameter("@Dni", dni),
                        new SqlParameter("@Horario", codHorario),
                        Fecha("@Fecha", fecha)
                    });

                MPPDigitoVerificador.RecalcularDvhFilas("InscripcionFija", "dni = @Dni AND codHorario = @Horario",
                    new SqlParameter("@Dni", dni), new SqlParameter("@Horario", codHorario));
            }
            catch (Exception ex)
            {
                throw new Exception("Error al terminar la inscripción fija: " + ex.Message, ex);
            }
        }

        /// <summary>
        /// Guarda la excepción de una fecha (reemplaza la que hubiera para ese alumno, turno y fecha).
        /// </summary>
        public void GuardarClase(int dni, int codHorario, DateTime fecha, string tipo)
        {
            try
            {
                dal._686DPEscribir(@"
                    DELETE FROM [GymApp].[dbo].[InscripcionClase]
                    WHERE dni = @Dni AND codHorario = @Horario AND fecha = @Fecha",
                    new List<SqlParameter>
                    {
                        new SqlParameter("@Dni", dni),
                        new SqlParameter("@Horario", codHorario),
                        Fecha("@Fecha", fecha)
                    });

                dal._686DPEscribir(@"
                    INSERT INTO [GymApp].[dbo].[InscripcionClase] (dni, codHorario, fecha, tipo, dvh)
                    VALUES (@Dni, @Horario, @Fecha, @Tipo, '')",
                    new List<SqlParameter>
                    {
                        new SqlParameter("@Dni", dni),
                        new SqlParameter("@Horario", codHorario),
                        Fecha("@Fecha", fecha),
                        new SqlParameter("@Tipo", tipo)
                    });

                MPPDigitoVerificador.RecalcularDvhFilas("InscripcionClase", "dni = @Dni AND codHorario = @Horario AND fecha = @Fecha",
                    new SqlParameter("@Dni", dni), new SqlParameter("@Horario", codHorario), Fecha("@Fecha", fecha));
            }
            catch (Exception ex)
            {
                throw new Exception("Error al guardar la clase: " + ex.Message, ex);
            }
        }

        /// <summary>
        /// Borra la excepción de una fecha, si existe.
        /// </summary>
        public void BorrarClase(int dni, int codHorario, DateTime fecha)
        {
            try
            {
                dal._686DPEscribir(@"
                    DELETE FROM [GymApp].[dbo].[InscripcionClase]
                    WHERE dni = @Dni AND codHorario = @Horario AND fecha = @Fecha",
                    new List<SqlParameter>
                    {
                        new SqlParameter("@Dni", dni),
                        new SqlParameter("@Horario", codHorario),
                        Fecha("@Fecha", fecha)
                    });

                MPPDigitoVerificador.SincronizarControl("InscripcionClase");
            }
            catch (Exception ex)
            {
                throw new Exception("Error al borrar la clase: " + ex.Message, ex);
            }
        }

        /// <summary>
        /// Borra todas las excepciones del alumno en el turno desde la fecha indicada (inclusive).
        /// </summary>
        public void BorrarClasesDesde(int dni, int codHorario, DateTime fecha)
        {
            try
            {
                dal._686DPEscribir(@"
                    DELETE FROM [GymApp].[dbo].[InscripcionClase]
                    WHERE dni = @Dni AND codHorario = @Horario AND fecha >= @Fecha",
                    new List<SqlParameter>
                    {
                        new SqlParameter("@Dni", dni),
                        new SqlParameter("@Horario", codHorario),
                        Fecha("@Fecha", fecha)
                    });

                MPPDigitoVerificador.SincronizarControl("InscripcionClase");
            }
            catch (Exception ex)
            {
                throw new Exception("Error al borrar las clases: " + ex.Message, ex);
            }
        }

        #endregion

        #region Mapeo

        private static List<InscripcionFija> MapearFijas(DataTable dt)
        {
            return dt.Rows.Cast<DataRow>().Select(row => new InscripcionFija
            {
                CodInscripcion = Convert.ToInt32(row["codInscripcion"]),
                DNI = Convert.ToInt32(row["dni"]),
                CodHorario = Convert.ToInt32(row["codHorario"]),
                FechaDesde = Convert.ToDateTime(row["fechaDesde"]).Date,
                FechaHasta = row["fechaHasta"] == DBNull.Value ? (DateTime?)null : Convert.ToDateTime(row["fechaHasta"]).Date,
                DVH = row["dvh"] != DBNull.Value ? row["dvh"].ToString() : string.Empty
            }).ToList();
        }

        private static List<InscripcionClase> MapearClases(DataTable dt)
        {
            return dt.Rows.Cast<DataRow>().Select(row => new InscripcionClase
            {
                CodInscripcionClase = Convert.ToInt32(row["codInscripcionClase"]),
                DNI = Convert.ToInt32(row["dni"]),
                CodHorario = Convert.ToInt32(row["codHorario"]),
                Fecha = Convert.ToDateTime(row["fecha"]).Date,
                Tipo = row["tipo"].ToString().Trim(),
                DVH = row["dvh"] != DBNull.Value ? row["dvh"].ToString() : string.Empty
            }).ToList();
        }

        #endregion
    }
}
