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
        private DigitoVerificadorManager dvManager;
        private CriptoManager criptoManager;

        public MPPPago()
        {
            dal = new DalGeneral();
            dvManager = new DigitoVerificadorManager();
            criptoManager = new CriptoManager();
        }

        private const string SELECT_BASE = @"
            SELECT
                p.codPago,
                p.dni,
                p.modalidadId,
                p.periodo,
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

        /// <summary>
        /// Arma el diccionario de valores usado para calcular el DVH. No incluye codPago
        /// porque es autogenerado por la base de datos y no se conoce en el momento del INSERT.
        /// </summary>
        private Dictionary<string, object> ArmarValoresDV(Pago pago)
        {
            return new Dictionary<string, object>
            {
                { "dni", pago.Dni },
                { "modalidadId", pago.ModalidadId },
                { "periodo", pago.Periodo.Date },
                { "monto", pago.Monto },
                { "fechaPago", pago.FechaPago },
                { "metodoPago", pago.MetodoPago },
                { "usuarioRegistro", pago.UsuarioRegistro }
            };
        }

        private string CalcularDigitosPago(Pago pago)
        {
            return dvManager.CalcularDVH(ArmarValoresDV(pago));
        }

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
        /// Indica si ya existe un pago registrado para el alumno en el período (mes) indicado.
        /// </summary>
        public bool ExistePagoEnPeriodo(int dni, DateTime periodo)
        {
            try
            {
                string consulta = @"
                    SELECT COUNT(1)
                    FROM [GymApp].[dbo].[Pagos]
                    WHERE dni = @Dni AND periodo = @Periodo";

                List<SqlParameter> parametros = new List<SqlParameter>
                {
                    new SqlParameter("@Dni", dni),
                    new SqlParameter("@Periodo", periodo.Date)
                };

                object resultado = dal._686DPEscalar(consulta, parametros);
                return resultado != null && resultado != DBNull.Value && Convert.ToInt32(resultado) > 0;
            }
            catch (Exception ex)
            {
                throw new Exception("Error al verificar pago existente: " + ex.Message, ex);
            }
        }

        public int CrearPago(Pago pago)
        {
            try
            {
                string dvh = CalcularDigitosPago(pago);

                string consulta = @"
                    INSERT INTO [GymApp].[dbo].[Pagos]
                    (dni, modalidadId, periodo, monto, fechaPago, metodoPago, usuarioRegistro, dvh)
                    VALUES
                    (@Dni, @ModalidadId, @Periodo, @Monto, @FechaPago, @MetodoPago, @UsuarioRegistro, @DVH);

                    SELECT CAST(SCOPE_IDENTITY() AS INT);";

                List<SqlParameter> parametros = new List<SqlParameter>
                {
                    new SqlParameter("@Dni", pago.Dni),
                    new SqlParameter("@ModalidadId", pago.ModalidadId),
                    new SqlParameter("@Periodo", pago.Periodo.Date),
                    new SqlParameter("@Monto", pago.Monto),
                    new SqlParameter("@FechaPago", pago.FechaPago),
                    new SqlParameter("@MetodoPago", pago.MetodoPago),
                    new SqlParameter("@UsuarioRegistro", pago.UsuarioRegistro),
                    new SqlParameter("@DVH", dvh)
                };

                object resultado = dal._686DPEscalar(consulta, parametros);
                MPPDigitoVerificador.SincronizarControl("Pagos");
                return (resultado != null && resultado != DBNull.Value) ? Convert.ToInt32(resultado) : 0;
            }
            catch (Exception ex)
            {
                throw new Exception("Error al crear pago: " + ex.Message, ex);
            }
        }
    }
}
