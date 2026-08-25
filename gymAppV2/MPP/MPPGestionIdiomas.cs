using System.Collections.Generic;
using System.Data;
using BE;
using DAL;

namespace MPP
{
    public class MPPGestionIdiomas
    {
        private readonly DalGestionIdiomas dal     = new DalGestionIdiomas();
        private readonly DalTraduccion     dalTrad = new DalTraduccion();

        public List<IdiomaInfo> ObtenerIdiomas()
        {
            var dt     = dal.ObtenerIdiomas();
            var result = new List<IdiomaInfo>();
            foreach (DataRow row in dt.Rows)
                result.Add(new IdiomaInfo {
                    IdiomaID     = (int)row["IdiomaID"],
                    Codigo       = row["Codigo"].ToString(),
                    NombreIdioma = row["NombreIdioma"].ToString()
                });
            return result;
        }

        public bool ExisteCodigoONombre(string codigo, string nombre)
            => dal.ExisteCodigoONombre(codigo, nombre);

        public Dictionary<string, List<string>> ObtenerTodasLasColumnas()
            => dal.ObtenerTodasLasColumnas();

        // Expone el listado de tablas para que BLL no dependa de DAL directamente.
        public string[] ObtenerNombresTablas()
            => DalTraduccion.TABLAS;

        // Obtiene el diccionario plano tag→valor para un idioma dado.
        public Dictionary<string, string> ObtenerValoresIdioma(int idiomaId)
            => dalTrad.ObtenerDiccionario(idiomaId);

        public int CrearIdiomaConTraducciones(
            string codigo,
            string nombre,
            Dictionary<string, Dictionary<string, string>> valoresPorTabla)
            => dal.CrearIdiomaConTraducciones(codigo, nombre, valoresPorTabla);

        public void ActualizarTraducciones(
            int idiomaId,
            Dictionary<string, Dictionary<string, string>> valoresPorTabla)
            => dal.ActualizarTraducciones(idiomaId, valoresPorTabla);
    }
}
