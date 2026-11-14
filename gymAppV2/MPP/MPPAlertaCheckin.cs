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
    /// Acceso a datos de las alertas de check-in con abono vencido (tabla AlertaCheckin).
    /// </summary>
    public class MPPAlertaCheckin
    {
        private DalGeneral dal;
        private CriptoManager criptoManager;

        public MPPAlertaCheckin()
        {
            dal = new DalGeneral();
            criptoManager = new CriptoManager();
        }

        public int Crear(AlertaCheckin alerta)
        {
            try
            {
                string consulta = @"
                    INSERT INTO [GymApp].[dbo].[AlertaCheckin]
                    (dni, situacion, fechaVencimiento, fecha, vista, dvh)
                    VALUES
                    (@Dni, @Situacion, @FechaVencimiento, @Fecha, 0, '');

                    SELECT CAST(SCOPE_IDENTITY() AS INT);";

                List<SqlParameter> parametros = new List<SqlParameter>
                {
                    new SqlParameter("@Dni", alerta.Dni),
                    new SqlParameter("@Situacion", alerta.Situacion),
                    new SqlParameter("@FechaVencimiento", (object)alerta.FechaVencimiento?.Date ?? DBNull.Value),
                    new SqlParameter("@Fecha", alerta.Fecha)
                };

                object resultado = dal._686DPEscalar(consulta, parametros);
                int codAlerta = (resultado != null && resultado != DBNull.Value) ? Convert.ToInt32(resultado) : 0;

                MPPDigitoVerificador.RecalcularDvhFilas("AlertaCheckin", "codAlerta = @Cod", new SqlParameter("@Cod", codAlerta));
                return codAlerta;
            }
            catch (Exception ex)
            {
                throw new Exception("Error al guardar la alerta de check-in: " + ex.Message, ex);
            }
        }

        /// <summary>
        /// Alertas que nadie marcó como vistas, de la más nueva a la más vieja, con el nombre del alumno.
        /// </summary>
        public List<AlertaCheckin> ListarPendientes()
        {
            try
            {
                string consulta = @"
                    SELECT a.codAlerta, a.dni, a.situacion, a.fechaVencimiento, a.fecha, a.vista,
                           a.usuarioVista, a.fechaVista, a.dvh,
                           u.nombre, u.apellido
                    FROM [GymApp].[dbo].[AlertaCheckin] a
                    LEFT JOIN [GymApp].[dbo].[USUARIOS] u ON u.dni = a.dni
                    WHERE a.vista = 0
                    ORDER BY a.fecha DESC";

                DataTable dt = dal._686DPConsultar(consulta, new List<SqlParameter>());
                List<AlertaCheckin> alertas = new List<AlertaCheckin>();
                HashSet<int> vistas = new HashSet<int>();

                foreach (DataRow row in dt.Rows)
                {
                    // Un mismo DNI puede tener más de un usuario vinculado: una sola fila por alerta
                    int codAlerta = Convert.ToInt32(row["codAlerta"]);
                    if (!vistas.Add(codAlerta))
                        continue;

                    alertas.Add(new AlertaCheckin
                    {
                        CodAlerta = codAlerta,
                        Dni = Convert.ToInt32(row["dni"]),
                        Situacion = row["situacion"].ToString(),
                        FechaVencimiento = row["fechaVencimiento"] != DBNull.Value ? Convert.ToDateTime(row["fechaVencimiento"]) : (DateTime?)null,
                        Fecha = Convert.ToDateTime(row["fecha"]),
                        Vista = Convert.ToBoolean(row["vista"]),
                        UsuarioVista = row["usuarioVista"] != DBNull.Value ? row["usuarioVista"].ToString() : null,
                        FechaVista = row["fechaVista"] != DBNull.Value ? Convert.ToDateTime(row["fechaVista"]) : (DateTime?)null,
                        DVH = row["dvh"] != DBNull.Value ? row["dvh"].ToString() : string.Empty,
                        Nombre = row["nombre"] != DBNull.Value ? criptoManager.DesencriptarCampoPersonal(row["nombre"].ToString()) : null,
                        Apellido = row["apellido"] != DBNull.Value ? criptoManager.DesencriptarCampoPersonal(row["apellido"].ToString()) : null
                    });
                }

                return alertas;
            }
            catch (Exception ex)
            {
                throw new Exception("Error al listar las alertas de check-in: " + ex.Message, ex);
            }
        }

        public void MarcarVista(int codAlerta, string usuario)
        {
            try
            {
                string consulta = @"
                    UPDATE [GymApp].[dbo].[AlertaCheckin]
                    SET vista = 1, usuarioVista = @Usuario, fechaVista = GETDATE(), dvh = ''
                    WHERE codAlerta = @Cod AND vista = 0";

                List<SqlParameter> parametros = new List<SqlParameter>
                {
                    new SqlParameter("@Cod", codAlerta),
                    new SqlParameter("@Usuario", usuario)
                };

                dal._686DPEscribir(consulta, parametros);
                MPPDigitoVerificador.RecalcularDvhFilas("AlertaCheckin", "codAlerta = @Cod", new SqlParameter("@Cod", codAlerta));
            }
            catch (Exception ex)
            {
                throw new Exception("Error al marcar la alerta como vista: " + ex.Message, ex);
            }
        }
    }
}
