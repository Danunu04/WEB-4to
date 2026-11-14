using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using BE;
using DAL;

namespace MPP
{
    /// <summary>
    /// Acceso a datos del historial de peso corporal de los alumnos (tabla PesoHistorial).
    /// </summary>
    public class MPPPesoCorporal
    {
        private DalGeneral dal;

        public MPPPesoCorporal()
        {
            dal = new DalGeneral();
        }

        /// <summary>
        /// Últimos pesos registrados de un alumno, del más reciente al más viejo.
        /// </summary>
        public List<RegistroPeso> ListarUltimos(int dni, int cantidad)
        {
            try
            {
                string consulta = @"
                    SELECT TOP (@Cantidad) codPeso, dni, peso, fecha, dvh
                    FROM [GymApp].[dbo].[PesoHistorial]
                    WHERE dni = @Dni
                    ORDER BY fecha DESC, codPeso DESC";

                List<SqlParameter> parametros = new List<SqlParameter>
                {
                    new SqlParameter("@Dni", dni),
                    new SqlParameter("@Cantidad", cantidad)
                };

                DataTable dt = dal._686DPConsultar(consulta, parametros);
                List<RegistroPeso> registros = new List<RegistroPeso>();

                foreach (DataRow row in dt.Rows)
                {
                    registros.Add(new RegistroPeso
                    {
                        CodPeso = Convert.ToInt32(row["codPeso"]),
                        Dni = Convert.ToInt32(row["dni"]),
                        Peso = Convert.ToDecimal(row["peso"]),
                        Fecha = Convert.ToDateTime(row["fecha"]),
                        DVH = row["dvh"] != DBNull.Value ? row["dvh"].ToString() : string.Empty
                    });
                }

                return registros;
            }
            catch (Exception ex)
            {
                throw new Exception("Error al listar el historial de peso: " + ex.Message, ex);
            }
        }

        /// <summary>
        /// Agrega un peso al historial. Se inserta con dvh vacío y se calcula después con la
        /// fila real (codPeso es IDENTITY), igual que Pagos.
        /// </summary>
        public int Insertar(RegistroPeso registro)
        {
            try
            {
                string consulta = @"
                    INSERT INTO [GymApp].[dbo].[PesoHistorial] (dni, peso, fecha, dvh)
                    VALUES (@Dni, @Peso, @Fecha, '');

                    SELECT CAST(SCOPE_IDENTITY() AS INT);";

                List<SqlParameter> parametros = new List<SqlParameter>
                {
                    new SqlParameter("@Dni", registro.Dni),
                    new SqlParameter("@Peso", registro.Peso),
                    new SqlParameter("@Fecha", registro.Fecha.Date)
                };

                object resultado = dal._686DPEscalar(consulta, parametros);
                int codPeso = (resultado != null && resultado != DBNull.Value) ? Convert.ToInt32(resultado) : 0;

                MPPDigitoVerificador.RecalcularDvhFilas("PesoHistorial", "codPeso = @Cod", new SqlParameter("@Cod", codPeso));
                return codPeso;
            }
            catch (Exception ex)
            {
                throw new Exception("Error al guardar el peso: " + ex.Message, ex);
            }
        }
    }
}
