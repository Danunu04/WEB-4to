using System.Collections.Generic;
using BE;
using MPP;

namespace BLL
{
    public class BLLTraduccion
    {
        private MPPTraduccion mpp;

        public BLLTraduccion()
        {
            mpp = new MPPTraduccion();
        }

        /// <summary>
        /// Devuelve un diccionario tag → texto para el idioma dado.
        /// Agrega los tags de todas las tablas del esquema Traducciones.
        /// </summary>
        public Dictionary<string, string> ObtenerDiccionario(IdiomaApp idioma)
        {
            return mpp.ObtenerDiccionario(idioma);
        }
    }
}
