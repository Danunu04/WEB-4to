using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using BE;
using DAL;
using SERVICIOS;

namespace MPP
{
    public class MPPPago
    {
        private DalGeneral dal;
        private CriptoManager criptoManager;

        public MPPPago()
        {
            dal = new DalGeneral();
            criptoManager = new CriptoManager();
        }

        private const string SELECT_BASE = @"
            SELECT
                p.codPago,
                p.dni,
                p.modalidadId,
                p.periodo,
                p.fechaInicio,
                p.fechaVencimiento,
                p.monto,
                p.fechaPago,
                p.metodoPago,
                p.usuarioRegistro,
                p.dvh,
                u.nombre AS alumnoNombre,
                u.apellido AS alumnoApellido,
                pm.DiasPorSemana AS modalidadDias,
                pm.EsDiario AS modalidadEsDiario
            FROM [GymApp].[dbo].[Pagos] p
            INNER JOIN [GymApp].[dbo].[Alumnos] al ON p.dni = al.dni
            LEFT JOIN [GymApp].[dbo].[USUARIOS] u ON al.dni = u.dni
            INNER JOIN [GymApp].[dbo].[PrecioModalidad] pm ON p.modalidadId = pm.Id";

        private Pago MapearFila(DataRow row)
        {
            bool esDiario = row["modalidadEsDiario"] != DBNull.Value && Convert.ToBoolean(row["modalidadEsDiario"]);
            int diasPorSemana = row["modalidadDias"] != DBNull.Value ? Convert.ToInt32(row["modalidadDias"]) : 0;

            return new Pago
            {
                CodPago = Convert.ToInt32(row["codPago"]),
                Dni = Convert.ToInt32(row["dni"]),
                ModalidadId = Convert.ToInt32(row["modalidadId"]),
                Periodo = Convert.ToDateTime(row["periodo"]),
                FechaInicio = Convert.ToDateTime(row["fechaInicio"]),
                FechaVencimiento = Convert.ToDateTime(row["fechaVencimiento"]),
                Monto = Convert.ToDecimal(row["monto"]),
                FechaPago = Convert.ToDateTime(row["fechaPago"]),
                MetodoPago = row["metodoPago"] != DBNull.Value ? row["metodoPago"].ToString() : string.Empty,
                UsuarioRegistro = row["usuarioRegistro"] != DBNull.Value ? row["usuarioRegistro"].ToString() : string.Empty,
                DVH = row["dvh"] != DBNull.Value ? row["dvh"].ToString() : string.Empty,
                AlumnoNombre = row["alumnoNombre"] != DBNull.Value ? criptoManager.DesencriptarCampoPersonal(row["alumnoNombre"].ToString()) : null,
                AlumnoApellido = row["alumnoApellido"] != DBNull.Value ? criptoManager.DesencriptarCampoPersonal(row["alumnoApellido"].ToString()) : null,
                ModalidadDescripcion = esDiario ? "Diario" : $"{diasPorSemana} día{(diasPorSemana > 1 ? "s" : "")}/semana"
            };
        }

        public List<Pago> ListarPagos()
        {
            try
            {
                string consulta = SELECT_BASE + " ORDER BY p.fechaPago DESC";

                DataTable dt = dal._686DPConsultar(consulta, new List<SqlParameter>());
                List<Pago> pagos = new List<Pago>();

                foreach (DataRow row in dt.Rows)
                {
                    pagos.Add(MapearFila(row));
                }

                return pagos;
            }
            catch (Exception ex)
            {
                throw new Exception("Error al listar pagos: " + ex.Message, ex);
            }
        }

        /// <summary>
        /// Lista el historial de pagos de los alumnos asociados al usuario Cliente indicado.
        /// </summary>
        public List<Pago> ListarPagosPorCliente(string usuario)
        {
            try
            {
                string consulta = SELECT_BASE + " WHERE EXISTS (SELECT 1 FROM [GymApp].[dbo].[ALUMNOS_USUARIOS] au WHERE au.dniAlumno = al.dni AND au.usr = @Usuario) ORDER BY p.fechaPago DESC";

                List<SqlParameter> parametros = new List<SqlParameter>
                {
                    new SqlParameter("@Usuario", usuario)
                };

                DataTable dt = dal._686DPConsultar(consulta, parametros);
                List<Pago> pagos = new List<Pago>();

                foreach (DataRow row in dt.Rows)
                {
                    pagos.Add(MapearFila(row));
                }

                return pagos;
            }
            catch (Exception ex)
            {
                throw new Exception("Error al listar pagos del cliente: " + ex.Message, ex);
            }
        }

        /// <summary>
        /// Último abono de cada alumno (el de fechaInicio más reciente). Como los abonos se
        /// encadenan, su vencimiento es hasta cuándo está cubierto el alumno.
        /// Con dni, solo el de ese alumno. Solo llena dni, modalidadId, fechaInicio y fechaVencimiento.
        /// </summary>
        public List<Pago> ListarUltimosAbonos(int? dni = null)
        {
            try
            {
                string consulta = @"
                    SELECT p.dni, p.modalidadId, p.fechaInicio, p.fechaVencimiento
                    FROM [GymApp].[dbo].[Pagos] p
                    WHERE p.fechaInicio = (SELECT MAX(p2.fechaInicio)
                                           FROM [GymApp].[dbo].[Pagos] p2
                                           WHERE p2.dni = p.dni)";

                List<SqlParameter> parametros = new List<SqlParameter>();
                if (dni.HasValue)
                {
                    consulta += " AND p.dni = @Dni";
                    parametros.Add(new SqlParameter("@Dni", dni.Value));
                }

                DataTable dt = dal._686DPConsultar(consulta, parametros);
                List<Pago> abonos = new List<Pago>();

                foreach (DataRow row in dt.Rows)
                {
                    abonos.Add(new Pago
                    {
                        Dni = Convert.ToInt32(row["dni"]),
                        ModalidadId = Convert.ToInt32(row["modalidadId"]),
                        FechaInicio = Convert.ToDateTime(row["fechaInicio"]),
                        FechaVencimiento = Convert.ToDateTime(row["fechaVencimiento"])
                    });
                }

                return abonos;
            }
            catch (Exception ex)
            {
                throw new Exception("Error al listar los últimos abonos: " + ex.Message, ex);
            }
        }

        /// <summary>
        /// Inserta el pago con dvh vacío y después calcula el dvh leyendo la fila real
        /// (incluye codPago, que es IDENTITY y no se conoce antes del INSERT).
        /// </summary>
        public int CrearPago(Pago pago)
        {
            try
            {
                string consulta = @"
                    INSERT INTO [GymApp].[dbo].[Pagos]
                    (dni, modalidadId, periodo, fechaInicio, fechaVencimiento, monto, fechaPago, metodoPago, usuarioRegistro, dvh)
                    VALUES
                    (@Dni, @ModalidadId, @Periodo, @FechaInicio, @FechaVencimiento, @Monto, @FechaPago, @MetodoPago, @UsuarioRegistro, '');

                    SELECT CAST(SCOPE_IDENTITY() AS INT);";

                List<SqlParameter> parametros = new List<SqlParameter>
                {
                    new SqlParameter("@Dni", pago.Dni),
                    new SqlParameter("@ModalidadId", pago.ModalidadId),
                    new SqlParameter("@Periodo", pago.Periodo.Date),
                    new SqlParameter("@FechaInicio", pago.FechaInicio.Date),
                    new SqlParameter("@FechaVencimiento", pago.FechaVencimiento.Date),
                    new SqlParameter("@Monto", pago.Monto),
                    new SqlParameter("@FechaPago", pago.FechaPago),
                    new SqlParameter("@MetodoPago", pago.MetodoPago),
                    new SqlParameter("@UsuarioRegistro", pago.UsuarioRegistro)
                };

                object resultado = dal._686DPEscalar(consulta, parametros);
                int codPago = (resultado != null && resultado != DBNull.Value) ? Convert.ToInt32(resultado) : 0;

                MPPDigitoVerificador.RecalcularDvhFilas("Pagos", "codPago = @Cod", new SqlParameter("@Cod", codPago));
                return codPago;
            }
            catch (Exception ex)
            {
                throw new Exception("Error al crear pago: " + ex.Message, ex);
            }
        }
    }
}
