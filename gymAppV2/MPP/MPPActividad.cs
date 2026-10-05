using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using BE;
using DAL;
using SERVICIOS;

namespace MPP
{
    /// <summary>
    /// Acceso a datos para la entidad Actividad y sus turnos (ActividadHorario). Cada turno tiene
    /// aula, profesor titular y, opcionalmente, un auxiliar. Las inscripciones de alumnos están en MPPInscripcion.
    ///
    /// Las escrituras insertan con dvh vacío y después recalculan el dvh desde la fila real
    /// (MPPDigitoVerificador.RecalcularDvhFilas), así coincide con la verificación de integridad.
    /// </summary>
    public class MPPActividad
    {
        private DalGeneral dal;
        private CriptoManager criptoManager;

        private const string COLUMNAS_ACTIVIDAD = @"
                        codActividad,
                        descripcion,
                        cantXSemana,
                        costoInterno,
                        precioAlumno,
                        activo,
                        cupoMaximo,
                        dvh";

        // Turno con los datos de su actividad, aula y profesores (los nombres están encriptados en USUARIOS)
        private const string SELECT_HORARIOS = @"
                    SELECT h.codHorario, h.codActividad, h.diaSemana, h.horaInicio, h.duracionMin,
                           h.codAula, h.dniTitular, h.dniAuxiliar, h.dvh,
                           a.descripcion, a.cupoMaximo,
                           au.nombre AS nombreAula, au.cupo AS cupoAula,
                           ut.nombre AS titularNombre, ut.apellido AS titularApellido,
                           ux.nombre AS auxiliarNombre, ux.apellido AS auxiliarApellido
                    FROM [GymApp].[dbo].[ActividadHorario] h
                    INNER JOIN [GymApp].[dbo].[Actividades] a ON a.codActividad = h.codActividad
                    LEFT JOIN [GymApp].[dbo].[Aulas] au ON au.codAula = h.codAula
                    LEFT JOIN [GymApp].[dbo].[USUARIOS] ut ON ut.dni = h.dniTitular
                    LEFT JOIN [GymApp].[dbo].[USUARIOS] ux ON ux.dni = h.dniAuxiliar";

        public MPPActividad()
        {
            dal = new DalGeneral();
            criptoManager = new CriptoManager();
        }

        #region Actividades

        /// <summary>
        /// Lista todas las actividades activas del gimnasio.
        /// </summary>
        public List<Actividad> ListarActividades()
        {
            try
            {
                string consulta = $@"
                    SELECT {COLUMNAS_ACTIVIDAD}
                    FROM [GymApp].[dbo].[Actividades]
                    WHERE activo = 1
                    ORDER BY descripcion";

                DataTable dt = dal._686DPConsultar(consulta, new List<SqlParameter>());
                return MapearActividades(dt);
            }
            catch (Exception ex)
            {
                throw new Exception("Error al listar actividades: " + ex.Message, ex);
            }
        }

        /// <summary>
        /// Lista todas las actividades (activas e inactivas) con los nombres de los profesores de sus turnos.
        /// Usado por la pantalla de gestión.
        /// </summary>
        public List<Actividad> ListarTodasLasActividades()
        {
            try
            {
                string consulta = $@"
                    SELECT {COLUMNAS_ACTIVIDAD}
                    FROM [GymApp].[dbo].[Actividades]
                    ORDER BY activo DESC, descripcion";

                List<Actividad> actividades = MapearActividades(dal._686DPConsultar(consulta, new List<SqlParameter>()));

                Dictionary<int, List<ActividadHorario>> turnosPorActividad = ConsultarHorarios(string.Empty, new List<SqlParameter>())
                    .GroupBy(h => h.CodActividad)
                    .ToDictionary(g => g.Key, g => g.ToList());

                foreach (Actividad actividad in actividades)
                {
                    actividad.Instructores = turnosPorActividad.TryGetValue(actividad.CodActividad, out List<ActividadHorario> turnos)
                        ? string.Join(", ", turnos.SelectMany(t => new[] { t.NombreTitular, t.NombreAuxiliar })
                            .Where(n => !string.IsNullOrEmpty(n)).Distinct())
                        : string.Empty;
                }

                return actividades;
            }
            catch (Exception ex)
            {
                throw new Exception("Error al listar actividades: " + ex.Message, ex);
            }
        }

        /// <summary>
        /// Obtiene una actividad con sus turnos (aula y profesores incluidos), o null si no existe.
        /// </summary>
        public Actividad ObtenerActividad(int codActividad)
        {
            try
            {
                string consulta = $@"
                    SELECT {COLUMNAS_ACTIVIDAD}
                    FROM [GymApp].[dbo].[Actividades]
                    WHERE codActividad = @Cod";

                List<Actividad> lista = MapearActividades(dal._686DPConsultar(consulta,
                    new List<SqlParameter> { new SqlParameter("@Cod", codActividad) }));

                if (lista.Count == 0)
                    return null;

                Actividad actividad = lista[0];
                actividad.Horarios = ListarHorariosDeActividad(codActividad);
                return actividad;
            }
            catch (Exception ex)
            {
                throw new Exception("Error al obtener la actividad: " + ex.Message, ex);
            }
        }

        public bool ExisteDescripcion(string descripcion, int codActividadExcluir)
        {
            try
            {
                object resultado = dal._686DPEscalar(@"
                    SELECT COUNT(*)
                    FROM [GymApp].[dbo].[Actividades]
                    WHERE descripcion = @Descripcion AND codActividad <> @Cod",
                    new List<SqlParameter>
                    {
                        new SqlParameter("@Descripcion", descripcion),
                        new SqlParameter("@Cod", codActividadExcluir)
                    });

                return resultado != null && resultado != DBNull.Value && Convert.ToInt32(resultado) > 0;
            }
            catch (Exception ex)
            {
                throw new Exception("Error al verificar la descripción de la actividad: " + ex.Message, ex);
            }
        }

        /// <summary>
        /// Inserta la actividad y devuelve el código generado.
        /// </summary>
        public int CrearActividad(Actividad actividad)
        {
            try
            {
                string consulta = @"
                    INSERT INTO [GymApp].[dbo].[Actividades]
                    (descripcion, cantXSemana, costoInterno, precioAlumno, activo, cupoMaximo, dvh)
                    VALUES
                    (@Descripcion, @CantXSemana, @CostoInterno, @PrecioAlumno, @Activo, @CupoMaximo, '');

                    SELECT CAST(SCOPE_IDENTITY() AS INT);";

                object resultado = dal._686DPEscalar(consulta, new List<SqlParameter>
                {
                    new SqlParameter("@Descripcion", actividad.Descripcion),
                    new SqlParameter("@CantXSemana", actividad.CantXSemana),
                    new SqlParameter("@CostoInterno", actividad.CostoInterno),
                    new SqlParameter("@PrecioAlumno", actividad.PrecioAlumno),
                    new SqlParameter("@Activo", actividad.Activo),
                    ParametroCupoMaximo(actividad.CupoMaximo)
                });

                int codActividad = Convert.ToInt32(resultado);
                MPPDigitoVerificador.RecalcularDvhFilas("Actividades", "codActividad = @Cod", new SqlParameter("@Cod", codActividad));
                return codActividad;
            }
            catch (Exception ex)
            {
                throw new Exception("Error al crear la actividad: " + ex.Message, ex);
            }
        }

        public void ActualizarActividad(Actividad actividad)
        {
            try
            {
                string consulta = @"
                    UPDATE [GymApp].[dbo].[Actividades]
                    SET descripcion = @Descripcion,
                        cantXSemana = @CantXSemana,
                        costoInterno = @CostoInterno,
                        precioAlumno = @PrecioAlumno,
                        activo = @Activo,
                        cupoMaximo = @CupoMaximo
                    WHERE codActividad = @Cod";

                dal._686DPEscribir(consulta, new List<SqlParameter>
                {
                    new SqlParameter("@Cod", actividad.CodActividad),
                    new SqlParameter("@Descripcion", actividad.Descripcion),
                    new SqlParameter("@CantXSemana", actividad.CantXSemana),
                    new SqlParameter("@CostoInterno", actividad.CostoInterno),
                    new SqlParameter("@PrecioAlumno", actividad.PrecioAlumno),
                    new SqlParameter("@Activo", actividad.Activo),
                    ParametroCupoMaximo(actividad.CupoMaximo)
                });

                MPPDigitoVerificador.RecalcularDvhFilas("Actividades", "codActividad = @Cod", new SqlParameter("@Cod", actividad.CodActividad));
            }
            catch (Exception ex)
            {
                throw new Exception("Error al actualizar la actividad: " + ex.Message, ex);
            }
        }

        private static SqlParameter ParametroCupoMaximo(int? cupoMaximo)
        {
            return new SqlParameter("@CupoMaximo", SqlDbType.Int) { Value = cupoMaximo.HasValue ? (object)cupoMaximo.Value : DBNull.Value };
        }

        /// <summary>
        /// Baja lógica: una actividad con rutinas o pagos históricos no se puede borrar físicamente.
        /// </summary>
        public void CambiarEstado(int codActividad, bool activo)
        {
            try
            {
                dal._686DPEscribir(@"
                    UPDATE [GymApp].[dbo].[Actividades]
                    SET activo = @Activo
                    WHERE codActividad = @Cod",
                    new List<SqlParameter>
                    {
                        new SqlParameter("@Cod", codActividad),
                        new SqlParameter("@Activo", activo)
                    });

                MPPDigitoVerificador.RecalcularDvhFilas("Actividades", "codActividad = @Cod", new SqlParameter("@Cod", codActividad));
            }
            catch (Exception ex)
            {
                throw new Exception("Error al cambiar el estado de la actividad: " + ex.Message, ex);
            }
        }

        #endregion

        #region Horarios

        /// <summary>
        /// Guarda los turnos de una actividad (día, hora, duración, aula y profesores) y actualiza su cantXSemana.
        /// Los turnos que ya existían (CodHorario > 0) se actualizan en el lugar para conservar
        /// su código, porque las inscripciones (InscripcionFija / InscripcionClase) lo referencian.
        /// Los turnos que se quitaron se borran junto con sus inscripciones; si un turno cambia
        /// de día, se borran sus inscripciones puntuales futuras (esas fechas ya no corresponden).
        /// </summary>
        public void GuardarHorarios(int codActividad, List<ActividadHorario> horarios)
        {
            try
            {
                Dictionary<int, ActividadHorario> existentes = ListarHorariosDeActividad(codActividad)
                    .ToDictionary(h => h.CodHorario);

                HashSet<int> conservados = new HashSet<int>(horarios
                    .Where(h => h.CodHorario > 0 && existentes.ContainsKey(h.CodHorario))
                    .Select(h => h.CodHorario));

                foreach (int codHorario in existentes.Keys.Where(c => !conservados.Contains(c)))
                {
                    dal._686DPEscribir(@"
                        IF OBJECT_ID('[GymApp].[dbo].[InscripcionClase]', 'U') IS NOT NULL
                            DELETE FROM [GymApp].[dbo].[InscripcionClase] WHERE codHorario = @Horario;
                        IF OBJECT_ID('[GymApp].[dbo].[InscripcionFija]', 'U') IS NOT NULL
                            DELETE FROM [GymApp].[dbo].[InscripcionFija] WHERE codHorario = @Horario;
                        DELETE FROM [GymApp].[dbo].[ActividadHorario] WHERE codHorario = @Horario;",
                        new List<SqlParameter> { new SqlParameter("@Horario", codHorario) });
                }

                foreach (ActividadHorario horario in horarios)
                {
                    if (conservados.Contains(horario.CodHorario))
                    {
                        if (existentes[horario.CodHorario].DiaSemana != horario.DiaSemana)
                        {
                            dal._686DPEscribir(@"
                                IF OBJECT_ID('[GymApp].[dbo].[InscripcionClase]', 'U') IS NOT NULL
                                    DELETE FROM [GymApp].[dbo].[InscripcionClase]
                                    WHERE codHorario = @Horario AND fecha >= CAST(GETDATE() AS DATE);",
                                new List<SqlParameter> { new SqlParameter("@Horario", horario.CodHorario) });
                        }

                        dal._686DPEscribir(@"
                            UPDATE [GymApp].[dbo].[ActividadHorario]
                            SET diaSemana = @Dia, horaInicio = @Hora, duracionMin = @Duracion,
                                codAula = @Aula, dniTitular = @Titular, dniAuxiliar = @Auxiliar
                            WHERE codHorario = @Horario",
                            ParametrosHorario(horario, new SqlParameter("@Horario", horario.CodHorario)));
                        continue;
                    }

                    dal._686DPEscribir(@"
                        INSERT INTO [GymApp].[dbo].[ActividadHorario]
                        (codActividad, diaSemana, horaInicio, duracionMin, codAula, dniTitular, dniAuxiliar, dvh)
                        VALUES
                        (@Cod, @Dia, @Hora, @Duracion, @Aula, @Titular, @Auxiliar, '')",
                        ParametrosHorario(horario, new SqlParameter("@Cod", codActividad)));
                }

                MPPDigitoVerificador.SincronizarControl("InscripcionClase", "InscripcionFija");
                MPPDigitoVerificador.RecalcularDvhFilas("ActividadHorario", "codActividad = @Cod", new SqlParameter("@Cod", codActividad));

                // cantXSemana refleja la cantidad de turnos cargados
                dal._686DPEscribir(@"
                    UPDATE [GymApp].[dbo].[Actividades]
                    SET cantXSemana = @Cant
                    WHERE codActividad = @Cod",
                    new List<SqlParameter>
                    {
                        new SqlParameter("@Cod", codActividad),
                        new SqlParameter("@Cant", horarios.Count)
                    });

                MPPDigitoVerificador.RecalcularDvhFilas("Actividades", "codActividad = @Cod", new SqlParameter("@Cod", codActividad));
            }
            catch (Exception ex)
            {
                throw new Exception("Error al guardar los horarios de la actividad: " + ex.Message, ex);
            }
        }

        private static List<SqlParameter> ParametrosHorario(ActividadHorario horario, SqlParameter clave)
        {
            return new List<SqlParameter>
            {
                clave,
                new SqlParameter("@Dia", (byte)horario.DiaSemana),
                new SqlParameter("@Hora", SqlDbType.Time) { Value = horario.HoraInicio },
                new SqlParameter("@Duracion", horario.DuracionMin),
                new SqlParameter("@Aula", horario.CodAula),
                new SqlParameter("@Titular", horario.DniTitular),
                new SqlParameter("@Auxiliar", SqlDbType.Int) { Value = horario.DniAuxiliar.HasValue ? (object)horario.DniAuxiliar.Value : DBNull.Value }
            };
        }

        public List<ActividadHorario> ListarHorariosDeActividad(int codActividad)
        {
            try
            {
                return ConsultarHorarios("WHERE h.codActividad = @Cod",
                    new List<SqlParameter> { new SqlParameter("@Cod", codActividad) });
            }
            catch (Exception ex)
            {
                throw new Exception("Error al listar los horarios de la actividad: " + ex.Message, ex);
            }
        }

        /// <summary>
        /// Un turno con los datos de su actividad, aula y profesores, o null si no existe.
        /// </summary>
        public ActividadHorario ObtenerHorario(int codHorario)
        {
            try
            {
                return ConsultarHorarios("WHERE h.codHorario = @Horario",
                    new List<SqlParameter> { new SqlParameter("@Horario", codHorario) }).FirstOrDefault();
            }
            catch (Exception ex)
            {
                throw new Exception("Error al obtener el horario: " + ex.Message, ex);
            }
        }

        /// <summary>
        /// Turnos de todas las actividades activas, ordenados por día y hora.
        /// </summary>
        public List<ActividadHorario> ListarHorariosActivos()
        {
            try
            {
                return ConsultarHorarios("WHERE a.activo = 1", new List<SqlParameter>());
            }
            catch (Exception ex)
            {
                throw new Exception("Error al listar los horarios: " + ex.Message, ex);
            }
        }

        /// <summary>
        /// Turnos de actividades activas que se dan en el aula.
        /// </summary>
        public List<ActividadHorario> ListarHorariosActivosDeAula(int codAula)
        {
            try
            {
                return ConsultarHorarios("WHERE a.activo = 1 AND h.codAula = @Aula",
                    new List<SqlParameter> { new SqlParameter("@Aula", codAula) });
            }
            catch (Exception ex)
            {
                throw new Exception("Error al listar los horarios del aula: " + ex.Message, ex);
            }
        }

        /// <summary>
        /// Turnos (de cualquier actividad) en los que el profesor es titular.
        /// </summary>
        public List<ActividadHorario> ListarHorariosComoTitular(int dniEntrenador)
        {
            try
            {
                return ConsultarHorarios("WHERE h.dniTitular = @Dni",
                    new List<SqlParameter> { new SqlParameter("@Dni", dniEntrenador) });
            }
            catch (Exception ex)
            {
                throw new Exception("Error al listar los turnos del profesor: " + ex.Message, ex);
            }
        }

        private List<ActividadHorario> ConsultarHorarios(string where, List<SqlParameter> parametros)
        {
            DataTable dt = dal._686DPConsultar(
                SELECT_HORARIOS + "\n" + where + "\nORDER BY h.diaSemana, h.horaInicio, a.descripcion",
                parametros);
            return MapearHorarios(dt);
        }

        #endregion

        #region Mapeo

        /// <summary>
        /// Mapea un DataTable con columnas de Actividades a una lista de entidades.
        /// </summary>
        private List<Actividad> MapearActividades(DataTable dt)
        {
            List<Actividad> actividades = new List<Actividad>();

            foreach (DataRow row in dt.Rows)
            {
                Actividad actividad = new Actividad(
                    Convert.ToInt32(row["codActividad"]),
                    row["descripcion"] != DBNull.Value ? row["descripcion"].ToString() : string.Empty,
                    Convert.ToInt32(row["cantXSemana"]),
                    Convert.ToDecimal(row["costoInterno"]),
                    Convert.ToDecimal(row["precioAlumno"]),
                    Convert.ToBoolean(row["activo"]),
                    row["dvh"] != DBNull.Value ? row["dvh"].ToString() : string.Empty
                );

                actividad.CupoMaximo = row["cupoMaximo"] != DBNull.Value ? Convert.ToInt32(row["cupoMaximo"]) : (int?)null;
                actividades.Add(actividad);
            }

            return actividades;
        }

        private List<ActividadHorario> MapearHorarios(DataTable dt)
        {
            List<ActividadHorario> horarios = new List<ActividadHorario>();

            foreach (DataRow row in dt.Rows)
            {
                int cupoAula = row["cupoAula"] != DBNull.Value ? Convert.ToInt32(row["cupoAula"]) : 0;
                int? cupoMaximo = row["cupoMaximo"] != DBNull.Value ? Convert.ToInt32(row["cupoMaximo"]) : (int?)null;
                string titular = NombreCompleto(row["titularNombre"], row["titularApellido"]);
                string auxiliar = row["dniAuxiliar"] != DBNull.Value ? NombreCompleto(row["auxiliarNombre"], row["auxiliarApellido"]) : string.Empty;

                horarios.Add(new ActividadHorario
                {
                    CodHorario = Convert.ToInt32(row["codHorario"]),
                    CodActividad = Convert.ToInt32(row["codActividad"]),
                    DiaSemana = Convert.ToInt32(row["diaSemana"]),
                    HoraInicio = (TimeSpan)row["horaInicio"],
                    DuracionMin = Convert.ToInt32(row["duracionMin"]),
                    CodAula = Convert.ToInt32(row["codAula"]),
                    DniTitular = Convert.ToInt32(row["dniTitular"]),
                    DniAuxiliar = row["dniAuxiliar"] != DBNull.Value ? Convert.ToInt32(row["dniAuxiliar"]) : (int?)null,
                    DVH = row["dvh"] != DBNull.Value ? row["dvh"].ToString() : string.Empty,
                    DescripcionActividad = row["descripcion"] != DBNull.Value ? row["descripcion"].ToString() : string.Empty,
                    NombreAula = row["nombreAula"] != DBNull.Value ? row["nombreAula"].ToString() : string.Empty,
                    CupoAula = cupoAula,
                    NombreTitular = titular,
                    NombreAuxiliar = auxiliar,
                    Instructores = string.IsNullOrEmpty(auxiliar) ? titular : $"{titular} · Aux: {auxiliar}",
                    Cupo = cupoMaximo.HasValue && cupoMaximo.Value < cupoAula ? cupoMaximo.Value : cupoAula
                });
            }

            return horarios;
        }

        /// <summary>
        /// Nombre y apellido del profesor; están encriptados en USUARIOS, por eso se desencriptan acá.
        /// </summary>
        private string NombreCompleto(object nombre, object apellido)
        {
            return $"{Desencriptar(nombre)} {Desencriptar(apellido)}".Trim();
        }

        private string Desencriptar(object valor)
        {
            return valor != DBNull.Value ? criptoManager.DesencriptarCampoPersonal(valor.ToString()) : string.Empty;
        }

        #endregion
    }
}
