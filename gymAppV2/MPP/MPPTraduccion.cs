using System.Collections.Generic;
using BE;
using DAL;

namespace MPP
{
    public class MPPTraduccion
    {
        private DalTraduccion dal;

        public MPPTraduccion()
        {
            dal = new DalTraduccion();
        }

        /// <summary>
        /// Convierte el enum IdiomaApp a IdiomaID de la tabla Traducciones.Idiomas.
        /// El orden de inserción en la tabla coincide con el enum: ES=1, EN=2, PT=3, FR=4, JA=5.
        /// </summary>
        public Dictionary<string, string> ObtenerDiccionario(IdiomaApp idioma)
        {
            int idiomaId = (int)idioma + 1;
            return dal.ObtenerDiccionario(idiomaId);
        }
    }
}
