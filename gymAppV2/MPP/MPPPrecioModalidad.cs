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
    public class MPPPrecioModalidad
    {
        private DalGeneral dal;
        private DigitoVerificadorManager dvManager;

        public MPPPrecioModalidad()
        {
            dal = new DalGeneral();
            dvManager = new DigitoVerificadorManager();
        }

        /// <summary>
        /// Calcula DVH y DVV de una modalidad de precio a partir de sus valores.
        /// </summary>
        private void CalcularDigitosPrecioModalidad(PrecioModalidad modalidad, out string dvh, out string dvv)
        {
            var valores = new Dictionary<string, object>
            {
                { "Id", modalidad.Id },
                { "DiasPorSemana", modalidad.DiasPorSemana },
                { "EsDiario", modalidad.EsDiario },
                { "Precio", modalidad.Precio },
                { "Activo", modalidad.Activo },
                { "FechaModificacion", modalidad.FechaModificacion }
            };

            dvManager.CalcularAmbos(valores, out dvh, out dvv);
        }

        /// <summary>
        /// Lista todas las modalidades de precio
        /// </summary>
        public List<PrecioModalidad> ListarModalidades()
        {
            try
            {
                string consulta = @"
                    SELECT
                        Id,
                        DiasPorSemana,
                        EsDiario,
                        Precio,
                        Activo,
                        FechaModificacion,
                        dvv,
                        dvh
                    FROM [GymApp].[dbo].[PrecioModalidad]
                    ORDER BY Id";

                ArrayList parametros = new ArrayList();
                DataTable dt = dal._686DPConsultar(consulta, parametros);

                List<PrecioModalidad> modalidades = new List<PrecioModalidad>();

                foreach (DataRow row in dt.Rows)
                {
                    modalidades.Add(new PrecioModalidad
                    {
                        Id = row["Id"] != DBNull.Value ? Convert.ToInt32(row["Id"]) : 0,
                        DiasPorSemana = row["DiasPorSemana"] != DBNull.Value ? Convert.ToInt32(row["DiasPorSemana"]) : 0,
                        EsDiario = row["EsDiario"] != DBNull.Value && Convert.ToBoolean(row["EsDiario"]),
                        Precio = row["Precio"] != DBNull.Value ? Convert.ToDecimal(row["Precio"]) : 0,
                        Activo = row["Activo"] != DBNull.Value && Convert.ToBoolean(row["Activo"]),
                        FechaModificacion = row["FechaModificacion"] != DBNull.Value ? Convert.ToDateTime(row["FechaModificacion"]) : DateTime.MinValue,
                        DVV = row["dvv"] != DBNull.Value ? row["dvv"].ToString() : string.Empty,
                        DVH = row["dvh"] != DBNull.Value ? row["dvh"].ToString() : string.Empty
                    });
                }

                return modalidades;
            }
            catch (Exception ex)
            {
                throw new Exception("Error al listar modalidades: " + ex.Message, ex);
            }
        }

        /// <summary>
        /// Obtiene una modalidad específica por ID
        /// </summary>
        public PrecioModalidad ObtenerModalidad(int id)
        {
            try
            {
                string consulta = @"
                    SELECT
                        Id,
                        DiasPorSemana,
                        EsDiario,
                        Precio,
                        Activo,
                        FechaModificacion,
                        dvv,
                        dvh
                    FROM [GymApp].[dbo].[PrecioModalidad]
                    WHERE Id = @Id";

                ArrayList parametros = new ArrayList
                {
                    new SqlParameter("@Id", id)
                };

                DataTable dt = dal._686DPConsultar(consulta, parametros);

                if (dt.Rows.Count > 0)
                {
                    DataRow row = dt.Rows[0];
                    return new PrecioModalidad
                    {
                        Id = row["Id"] != DBNull.Value ? Convert.ToInt32(row["Id"]) : 0,
                        DiasPorSemana = row["DiasPorSemana"] != DBNull.Value ? Convert.ToInt32(row["DiasPorSemana"]) : 0,
                        EsDiario = row["EsDiario"] != DBNull.Value && Convert.ToBoolean(row["EsDiario"]),
                        Precio = row["Precio"] != DBNull.Value ? Convert.ToDecimal(row["Precio"]) : 0,
                        Activo = row["Activo"] != DBNull.Value && Convert.ToBoolean(row["Activo"]),
                        FechaModificacion = row["FechaModificacion"] != DBNull.Value ? Convert.ToDateTime(row["FechaModificacion"]) : DateTime.MinValue,
                        DVV = row["dvv"] != DBNull.Value ? row["dvv"].ToString() : string.Empty,
                        DVH = row["dvh"] != DBNull.Value ? row["dvh"].ToString() : string.Empty
                    };
                }

                return null;
            }
            catch (Exception ex)
            {
                throw new Exception("Error al obtener modalidad: " + ex.Message, ex);
            }
        }

        /// <summary>
        /// Actualiza el precio de una modalidad
        /// </summary>
        public void ActualizarPrecio(int id, decimal nuevoPrecio)
        {
            try
            {
                PrecioModalidad modalidad = ObtenerModalidad(id);
                if (modalidad == null)
                    throw new Exception("No se encontró la modalidad de precio.");

                modalidad.Precio = nuevoPrecio;
                modalidad.FechaModificacion = DateTime.Now;

                CalcularDigitosPrecioModalidad(modalidad, out string dvh, out string dvv);

                string consulta = @"
                    UPDATE [GymApp].[dbo].[PrecioModalidad]
                    SET Precio = @Precio,
                        FechaModificacion = @FechaModificacion,
                        dvv = @DVV,
                        dvh = @DVH
                    WHERE Id = @Id";

                ArrayList parametros = new ArrayList
                {
                    new SqlParameter("@Precio", nuevoPrecio),
                    new SqlParameter("@FechaModificacion", modalidad.FechaModificacion),
                    new SqlParameter("@DVV", dvv),
                    new SqlParameter("@DVH", dvh),
                    new SqlParameter("@Id", id)
                };

                dal._686DPEscribir(consulta, parametros);
            }
            catch (Exception ex)
            {
                throw new Exception("Error al actualizar precio: " + ex.Message, ex);
            }
        }

        /// <summary>
        /// Obtiene el precio según los días por semana
        /// </summary>
        public decimal ObtenerPrecioPorDias(int diasPorSemana)
        {
            try
            {
                string consulta = @"
                    SELECT Precio
                    FROM [GymApp].[dbo].[PrecioModalidad]
                    WHERE (DiasPorSemana = @Dias OR (DiasPorSemana = 0 AND EsDiario = 1))
                      AND Activo = 1";

                ArrayList parametros = new ArrayList
                {
                    new SqlParameter("@Dias", diasPorSemana)
                };

                object resultado = dal._686DPEscalar(consulta, parametros);

                if (resultado != null && resultado != DBNull.Value)
                {
                    return Convert.ToDecimal(resultado);
                }

                return 0;
            }
            catch (Exception ex)
            {
                throw new Exception("Error al obtener precio: " + ex.Message, ex);
            }
        }
    }
}
