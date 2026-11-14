using System;
using System.Collections.Generic;
using BE;
using MPP;

namespace BLL
{
    /// <summary>
    /// Alerta interna por cambio de precio de cuota (G05 - Notificaciones). Cada cambio crea un
    /// aviso que los usuarios no entrenadores ven como banner hasta marcarlo como visto. Los
    /// avisos de más de DIAS_VIGENCIA días ya no se muestran.
    /// </summary>
    public class BLLAvisoPrecio
    {
        public const int DIAS_VIGENCIA = 30;

        private MPPAvisoPrecio mppAvisoPrecio;

        public BLLAvisoPrecio()
        {
            mppAvisoPrecio = new MPPAvisoPrecio();
        }

        public int Crear(AvisoPrecio aviso)
        {
            return mppAvisoPrecio.Crear(aviso);
        }

        /// <summary>
        /// Avisos que el usuario todavía no vio. Los entrenadores no reciben avisos de precio.
        /// </summary>
        public List<AvisoPrecio> ListarPendientes(Usuario usuario)
        {
            if (usuario == null || usuario.USUARIO_Rol == PerfilesSistema.RolEntrenador)
            {
                return new List<AvisoPrecio>();
            }

            return mppAvisoPrecio.ListarPendientes(usuario.USUARIO_Usuario, DateTime.Today.AddDays(-DIAS_VIGENCIA));
        }

        public void MarcarVistos(Usuario usuario, IEnumerable<int> codigosAviso)
        {
            if (usuario == null)
            {
                return;
            }

            foreach (int codAviso in codigosAviso)
            {
                mppAvisoPrecio.MarcarVisto(codAviso, usuario.USUARIO_Usuario);
            }
        }
    }
}
