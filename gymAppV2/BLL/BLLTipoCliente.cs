using System;
using BE;
using MPP;

namespace BLL
{
    public class BLLTipoCliente
    {
        private MPPTipoCliente mppTipoCliente;

        public BLLTipoCliente()
        {
            mppTipoCliente = new MPPTipoCliente();
        }

        public TipoCliente DeterminarTipoCliente(string usuario)
        {
            try
            {
                return mppTipoCliente.DeterminarTipoCliente(usuario);
            }
            catch (Exception ex)
            {
                throw new Exception("Error al determinar tipo de cliente: " + ex.Message, ex);
            }
        }

        public bool EsAlumno(string usuario)
        {
            return DeterminarTipoCliente(usuario) == TipoCliente.Alumno;
        }

        public bool EsFamiliar(string usuario)
        {
            return DeterminarTipoCliente(usuario) == TipoCliente.Familiar;
        }
    }
}