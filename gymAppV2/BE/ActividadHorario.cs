using System;

namespace BE
{
    /// <summary>
    /// Turno semanal de una actividad (tabla ActividadHorario).
    /// Una actividad puede tener varios turnos por semana.
    /// </summary>
    public class ActividadHorario
    {
        public int CodHorario { get; set; }
        public int CodActividad { get; set; }

        /// <summary>1 = Lunes ... 7 = Domingo.</summary>
        public int DiaSemana { get; set; }

        public TimeSpan HoraInicio { get; set; }
        public int DuracionMin { get; set; }

        /// <summary>Aula donde se da el turno (obligatoria).</summary>
        public int CodAula { get; set; }

        /// <summary>Profesor titular (obligatorio) y auxiliar (opcional, distinto del titular).</summary>
        public int DniTitular { get; set; }
        public int? DniAuxiliar { get; set; }

        // Campo de verificación de integridad
        public string DVH { get; set; }

        // Datos de visualización (se completan vía JOIN con Actividades, Aulas y USUARIOS)
        public string DescripcionActividad { get; set; }
        public string NombreAula { get; set; }
        public int CupoAula { get; set; }
        public string NombreTitular { get; set; }
        public string NombreAuxiliar { get; set; }

        /// <summary>"Titular" o "Titular · Aux: Auxiliar".</summary>
        public string Instructores { get; set; }

        /// <summary>Cupo de cada clase del turno: el del aula, o el tope de la actividad si es menor.</summary>
        public int Cupo { get; set; }

        /// <summary>DNI de los profesores del turno (titular y, si hay, auxiliar).</summary>
        public int[] Profesores => DniAuxiliar.HasValue ? new[] { DniTitular, DniAuxiliar.Value } : new[] { DniTitular };

        public TimeSpan HoraFin => HoraInicio.Add(TimeSpan.FromMinutes(DuracionMin));

        /// <summary>
        /// Convierte DayOfWeek de .NET (Domingo = 0) al formato de la tabla (Lunes = 1 ... Domingo = 7).
        /// </summary>
        public static int DiaSemanaDesde(DayOfWeek dia)
        {
            return dia == DayOfWeek.Sunday ? 7 : (int)dia;
        }
    }
}
