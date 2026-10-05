using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using DAL;

namespace MPP
{
    /// <summary>
    /// Consultas de solo lectura para los indicadores del panel principal.
    /// Cada método devuelve null si la tabla que necesita todavía no existe
    /// (ej. Pagos o ActividadHorario sin su script aplicado), para que el
    /// panel muestre el resto de los indicadores igual.
    /// </summary>
    public class MPPDashboard
    {
        private DalGeneral dal;

        public MPPDashboard()
        {
            dal = new DalGeneral();
        }

        public int? ContarAlumnosActivos()
        {
            return EscalarEntero(@"
                SELECT COUNT(*)
                FROM [GymApp].[dbo].[ALUMNOS]
                WHERE activo = 1", null);
        }

        /// <summary>
        /// Suma de los pagos del período indicado (primer día del mes).
        /// </summary>
        public decimal? SumarIngresosPeriodo(DateTime periodo)
        {
            if (!TablaExiste("Pagos"))
                return null;

            object resultado = dal._686DPEscalar(@"
                SELECT ISNULL(SUM(monto), 0)
                FROM [GymApp].[dbo].[Pagos]
                WHERE periodo = @Periodo",
                new List<SqlParameter> { new SqlParameter("@Periodo", periodo.Date) });

            return resultado != null && resultado != DBNull.Value ? Convert.ToDecimal(resultado) : 0m;
        }

        /// <summary>
        /// Alumnos activos que tienen pagada la cuota del período indicado.
        /// </summary>
        public int? ContarAlumnosAlDia(DateTime periodo)
        {
            if (!TablaExiste("Pagos"))
                return null;

            return EscalarEntero(@"
                SELECT COUNT(DISTINCT p.dni)
                FROM [GymApp].[dbo].[Pagos] p
                INNER JOIN [GymApp].[dbo].[ALUMNOS] al ON al.dni = p.dni
                WHERE p.periodo = @Periodo AND al.activo = 1",
                new SqlParameter("@Periodo", periodo.Date));
        }

        public bool TablaExiste(string nombreTabla)
        {
            object resultado = dal._686DPEscalar(
                "SELECT CASE WHEN OBJECT_ID(@Tabla, 'U') IS NULL THEN 0 ELSE 1 END",
                new List<SqlParameter> { new SqlParameter("@Tabla", "[GymApp].[dbo].[" + nombreTabla + "]") });

            return resultado != null && resultado != DBNull.Value && Convert.ToInt32(resultado) == 1;
        }

        private int? EscalarEntero(string consulta, SqlParameter parametro)
        {
            var parametros = new List<SqlParameter>();
            if (parametro != null)
                parametros.Add(parametro);

            object resultado = dal._686DPEscalar(consulta, parametros);
            return resultado != null && resultado != DBNull.Value ? Convert.ToInt32(resultado) : 0;
        }
    }
}
