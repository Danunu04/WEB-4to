using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using BE;
using DAL;

namespace MPP
{
    /// <summary>
    /// Acceso a datos de los avisos de cambio de precio (AvisoPrecio) y de quién ya los vio
    /// (AvisoPrecioVisto).
    /// </summary>
    public class MPPAvisoPrecio
    {
        private DalGeneral dal;

        public MPPAvisoPrecio()
        {
            dal = new DalGeneral();
        }

        public int Crear(AvisoPrecio aviso)
        {
            try
            {
                string consulta = @"
                    INSERT INTO [GymApp].[dbo].[AvisoPrecio]
                    (modalidadId, descripcion, precioAnterior, precioNuevo, fecha, usuarioRegistro, dvh)
                    VALUES
                    (@ModalidadId, @Descripcion, @PrecioAnterior, @PrecioNuevo, @Fecha, @UsuarioRegistro, '');

                    SELECT CAST(SCOPE_IDENTITY() AS INT);";

                List<SqlParameter> parametros = new List<SqlParameter>
                {
                    new SqlParameter("@ModalidadId", aviso.ModalidadId),
                    new SqlParameter("@Descripcion", aviso.Descripcion),
                    new SqlParameter("@PrecioAnterior", aviso.PrecioAnterior),
                    new SqlParameter("@PrecioNuevo", aviso.PrecioNuevo),
                    new SqlParameter("@Fecha", aviso.Fecha),
                    new SqlParameter("@UsuarioRegistro", (object)aviso.UsuarioRegistro ?? DBNull.Value)
                };

                object resultado = dal._686DPEscalar(consulta, parametros);
                int codAviso = (resultado != null && resultado != DBNull.Value) ? Convert.ToInt32(resultado) : 0;

                MPPDigitoVerificador.RecalcularDvhFilas("AvisoPrecio", "codAviso = @Cod", new SqlParameter("@Cod", codAviso));
                return codAviso;
            }
            catch (Exception ex)
            {
                throw new Exception("Error al guardar el aviso de precio: " + ex.Message, ex);
            }
        }

        /// <summary>
        /// Avisos desde una fecha que el usuario todavía no marcó como vistos, del más nuevo al más viejo.
        /// </summary>
        public List<AvisoPrecio> ListarPendientes(string usuario, DateTime desde)
        {
            try
            {
                string consulta = @"
                    SELECT a.codAviso, a.modalidadId, a.descripcion, a.precioAnterior, a.precioNuevo,
                           a.fecha, a.usuarioRegistro, a.dvh
                    FROM [GymApp].[dbo].[AvisoPrecio] a
                    WHERE a.fecha >= @Desde
                      AND NOT EXISTS (SELECT 1 FROM [GymApp].[dbo].[AvisoPrecioVisto] v
                                      WHERE v.codAviso = a.codAviso AND v.usr = @Usuario)
                    ORDER BY a.fecha DESC";

                List<SqlParameter> parametros = new List<SqlParameter>
                {
                    new SqlParameter("@Desde", desde),
                    new SqlParameter("@Usuario", usuario)
                };

                DataTable dt = dal._686DPConsultar(consulta, parametros);
                List<AvisoPrecio> avisos = new List<AvisoPrecio>();

                foreach (DataRow row in dt.Rows)
                {
                    avisos.Add(new AvisoPrecio
                    {
                        CodAviso = Convert.ToInt32(row["codAviso"]),
                        ModalidadId = Convert.ToInt32(row["modalidadId"]),
                        Descripcion = row["descripcion"].ToString(),
                        PrecioAnterior = Convert.ToDecimal(row["precioAnterior"]),
                        PrecioNuevo = Convert.ToDecimal(row["precioNuevo"]),
                        Fecha = Convert.ToDateTime(row["fecha"]),
                        UsuarioRegistro = row["usuarioRegistro"] != DBNull.Value ? row["usuarioRegistro"].ToString() : null,
                        DVH = row["dvh"] != DBNull.Value ? row["dvh"].ToString() : string.Empty
                    });
                }

                return avisos;
            }
            catch (Exception ex)
            {
                throw new Exception("Error al listar los avisos de precio: " + ex.Message, ex);
            }
        }

        public void MarcarVisto(int codAviso, string usuario)
        {
            try
            {
                string consulta = @"
                    IF NOT EXISTS (SELECT 1 FROM [GymApp].[dbo].[AvisoPrecioVisto] WHERE codAviso = @Cod AND usr = @Usuario)
                        INSERT INTO [GymApp].[dbo].[AvisoPrecioVisto] (codAviso, usr, fechaVisto, dvh)
                        VALUES (@Cod, @Usuario, GETDATE(), '');";

                List<SqlParameter> parametros = new List<SqlParameter>
                {
                    new SqlParameter("@Cod", codAviso),
                    new SqlParameter("@Usuario", usuario)
                };

                dal._686DPEscribir(consulta, parametros);

                MPPDigitoVerificador.RecalcularDvhFilas("AvisoPrecioVisto", "codAviso = @Cod AND usr = @Usuario",
                    new SqlParameter("@Cod", codAviso), new SqlParameter("@Usuario", usuario));
            }
            catch (Exception ex)
            {
                throw new Exception("Error al marcar el aviso como visto: " + ex.Message, ex);
            }
        }
    }
}
