using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using BE;
using DAL;
using SERVICIOS;

namespace MPP
{
    /// <summary>
    /// Acceso a datos de las transferencias informadas por los clientes (tabla TransferenciaPendiente).
    /// </summary>
    public class MPPTransferencia
    {
        private DalGeneral dal;
        private CriptoManager criptoManager;

        public MPPTransferencia()
        {
            dal = new DalGeneral();
            criptoManager = new CriptoManager();
        }

        public int Crear(TransferenciaPendiente transferencia)
        {
            try
            {
                string consulta = @"
                    INSERT INTO [GymApp].[dbo].[TransferenciaPendiente]
                    (dni, modalidadId, monto, fechaSolicitud, usuarioSolicita, estado, dvh)
                    VALUES
                    (@Dni, @ModalidadId, @Monto, @Fecha, @Usuario, @Estado, '');

                    SELECT CAST(SCOPE_IDENTITY() AS INT);";

                List<SqlParameter> parametros = new List<SqlParameter>
                {
                    new SqlParameter("@Dni", transferencia.Dni),
                    new SqlParameter("@ModalidadId", transferencia.ModalidadId),
                    new SqlParameter("@Monto", transferencia.Monto),
                    new SqlParameter("@Fecha", transferencia.FechaSolicitud),
                    new SqlParameter("@Usuario", transferencia.UsuarioSolicita),
                    new SqlParameter("@Estado", TransferenciaPendiente.ESTADO_PENDIENTE)
                };

                object resultado = dal._686DPEscalar(consulta, parametros);
                int cod = (resultado != null && resultado != DBNull.Value) ? Convert.ToInt32(resultado) : 0;

                MPPDigitoVerificador.RecalcularDvhFilas("TransferenciaPendiente", "codTransferencia = @Cod", new SqlParameter("@Cod", cod));
                return cod;
            }
            catch (Exception ex)
            {
                throw new Exception("Error al guardar la transferencia: " + ex.Message, ex);
            }
        }

        /// <summary>Transferencias sin confirmar ni rechazar, de la más vieja a la más nueva.</summary>
        public List<TransferenciaPendiente> ListarPendientes()
        {
            return Listar("t.estado = @Estado", new SqlParameter("@Estado", TransferenciaPendiente.ESTADO_PENDIENTE));
        }

        public TransferenciaPendiente Obtener(int codTransferencia)
        {
            List<TransferenciaPendiente> lista = Listar("t.codTransferencia = @Cod", new SqlParameter("@Cod", codTransferencia));
            return lista.Count > 0 ? lista[0] : null;
        }

        /// <summary>DNI de los alumnos que tienen una transferencia sin confirmar.</summary>
        public HashSet<int> ListarDnisPendientes()
        {
            try
            {
                string consulta = @"
                    SELECT DISTINCT dni
                    FROM [GymApp].[dbo].[TransferenciaPendiente]
                    WHERE estado = @Estado";

                DataTable dt = dal._686DPConsultar(consulta, new List<SqlParameter>
                {
                    new SqlParameter("@Estado", TransferenciaPendiente.ESTADO_PENDIENTE)
                });

                HashSet<int> dnis = new HashSet<int>();
                foreach (DataRow row in dt.Rows)
                {
                    dnis.Add(Convert.ToInt32(row["dni"]));
                }
                return dnis;
            }
            catch (Exception ex)
            {
                throw new Exception("Error al listar las transferencias pendientes: " + ex.Message, ex);
            }
        }

        /// <summary>
        /// Marca la transferencia como aprobada o rechazada. Solo cambia si todavía estaba pendiente;
        /// devuelve false si otra persona ya la había resuelto.
        /// </summary>
        public bool Resolver(int codTransferencia, string estado, string usuario, int? codPago)
        {
            try
            {
                string consulta = @"
                    UPDATE [GymApp].[dbo].[TransferenciaPendiente]
                    SET estado = @Estado, usuarioResuelve = @Usuario, fechaResuelve = GETDATE(),
                        codPago = @CodPago, dvh = ''
                    WHERE codTransferencia = @Cod AND estado = @Pendiente;

                    SELECT @@ROWCOUNT;";

                List<SqlParameter> parametros = new List<SqlParameter>
                {
                    new SqlParameter("@Cod", codTransferencia),
                    new SqlParameter("@Estado", estado),
                    new SqlParameter("@Usuario", (object)usuario ?? DBNull.Value),
                    new SqlParameter("@CodPago", (object)codPago ?? DBNull.Value),
                    new SqlParameter("@Pendiente", TransferenciaPendiente.ESTADO_PENDIENTE)
                };

                object resultado = dal._686DPEscalar(consulta, parametros);
                bool cambio = resultado != null && resultado != DBNull.Value && Convert.ToInt32(resultado) > 0;

                MPPDigitoVerificador.RecalcularDvhFilas("TransferenciaPendiente", "codTransferencia = @Cod", new SqlParameter("@Cod", codTransferencia));
                return cambio;
            }
            catch (Exception ex)
            {
                throw new Exception("Error al resolver la transferencia: " + ex.Message, ex);
            }
        }

        private List<TransferenciaPendiente> Listar(string condicion, params SqlParameter[] parametros)
        {
            try
            {
                string consulta = $@"
                    SELECT t.codTransferencia, t.dni, t.modalidadId, t.monto, t.fechaSolicitud, t.usuarioSolicita,
                           t.estado, t.usuarioResuelve, t.fechaResuelve, t.codPago, t.dvh,
                           u.nombre, u.apellido, pm.DiasPorSemana, pm.EsDiario
                    FROM [GymApp].[dbo].[TransferenciaPendiente] t
                    LEFT JOIN [GymApp].[dbo].[USUARIOS] u ON u.dni = t.dni
                    LEFT JOIN [GymApp].[dbo].[PrecioModalidad] pm ON pm.Id = t.modalidadId
                    WHERE {condicion}
                    ORDER BY t.fechaSolicitud";

                DataTable dt = dal._686DPConsultar(consulta, new List<SqlParameter>(parametros));
                List<TransferenciaPendiente> lista = new List<TransferenciaPendiente>();
                HashSet<int> vistas = new HashSet<int>();

                foreach (DataRow row in dt.Rows)
                {
                    // Un mismo DNI puede tener más de un usuario vinculado: una sola fila por transferencia
                    int cod = Convert.ToInt32(row["codTransferencia"]);
                    if (!vistas.Add(cod))
                        continue;

                    bool esDiario = row["EsDiario"] != DBNull.Value && Convert.ToBoolean(row["EsDiario"]);
                    int dias = row["DiasPorSemana"] != DBNull.Value ? Convert.ToInt32(row["DiasPorSemana"]) : 0;

                    lista.Add(new TransferenciaPendiente
                    {
                        CodTransferencia = cod,
                        Dni = Convert.ToInt32(row["dni"]),
                        ModalidadId = Convert.ToInt32(row["modalidadId"]),
                        Monto = Convert.ToDecimal(row["monto"]),
                        FechaSolicitud = Convert.ToDateTime(row["fechaSolicitud"]),
                        UsuarioSolicita = row["usuarioSolicita"] != DBNull.Value ? row["usuarioSolicita"].ToString() : null,
                        Estado = row["estado"].ToString(),
                        UsuarioResuelve = row["usuarioResuelve"] != DBNull.Value ? row["usuarioResuelve"].ToString() : null,
                        FechaResuelve = row["fechaResuelve"] != DBNull.Value ? Convert.ToDateTime(row["fechaResuelve"]) : (DateTime?)null,
                        CodPago = row["codPago"] != DBNull.Value ? Convert.ToInt32(row["codPago"]) : (int?)null,
                        DVH = row["dvh"] != DBNull.Value ? row["dvh"].ToString() : string.Empty,
                        AlumnoNombre = row["nombre"] != DBNull.Value ? criptoManager.DesencriptarCampoPersonal(row["nombre"].ToString()) : null,
                        AlumnoApellido = row["apellido"] != DBNull.Value ? criptoManager.DesencriptarCampoPersonal(row["apellido"].ToString()) : null,
                        ModalidadDescripcion = esDiario ? "Diario" : $"{dias} día{(dias > 1 ? "s" : "")}/semana"
                    });
                }

                return lista;
            }
            catch (Exception ex)
            {
                throw new Exception("Error al listar las transferencias: " + ex.Message, ex);
            }
        }
    }
}
