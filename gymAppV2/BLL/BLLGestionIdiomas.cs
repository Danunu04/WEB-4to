using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using BE;
using MPP;

namespace BLL
{
    public class BLLGestionIdiomas
    {
        private readonly MPPGestionIdiomas mpp = new MPPGestionIdiomas();

        public List<IdiomaInfo> ObtenerIdiomas()
            => mpp.ObtenerIdiomas();

        public bool ExisteIdioma(string codigo, string nombre)
            => mpp.ExisteCodigoONombre(codigo.Trim().ToUpper(), nombre.Trim());

        // Devuelve las secciones (tablas con sus columnas) sin valores.
        // Los valores se inyectan en el code-behind según el contexto.
        public List<SeccionTraduccion> ObtenerSecciones()
        {
            var columnas  = mpp.ObtenerTodasLasColumnas();
            var secciones = new List<SeccionTraduccion>();
            foreach (string tabla in mpp.ObtenerNombresTablas())
            {
                var sec = new SeccionTraduccion { NombreTabla = tabla };
                if (columnas.TryGetValue(tabla, out var cols))
                    sec.Tags = cols.Select(c => new TagTraduccion { Tag = c }).ToList();
                secciones.Add(sec);
            }
            return secciones;
        }

        // Devuelve todos los valores de un idioma existente como diccionario plano tag→valor.
        public Dictionary<string, string> ObtenerTodosLosValores(int idiomaId)
            => mpp.ObtenerValoresIdioma(idiomaId);

        // Devuelve los datos de todos los idiomas para el selector de referencia JS.
        // Clave string (IdiomaID) porque JavaScriptSerializer requiere claves string en diccionarios.
        public Dictionary<string, Dictionary<string, string>> ObtenerDatosTodosIdiomas()
        {
            var idiomas = mpp.ObtenerIdiomas();
            var result  = new Dictionary<string, Dictionary<string, string>>();
            foreach (var idioma in idiomas)
                result[idioma.IdiomaID.ToString()] = mpp.ObtenerValoresIdioma(idioma.IdiomaID);
            return result;
        }

        // Lee los valores del formulario POST y los organiza por tabla.
        // Las columnas se obtienen de la BD (no del usuario) para evitar inyección.
        public Dictionary<string, Dictionary<string, string>> RecolectarValoresFormulario(
            NameValueCollection form)
        {
            var columnas = mpp.ObtenerTodasLasColumnas();
            var result   = new Dictionary<string, Dictionary<string, string>>();
            foreach (string tabla in mpp.ObtenerNombresTablas())
            {
                if (!columnas.TryGetValue(tabla, out var cols)) continue;
                var vals = new Dictionary<string, string>();
                foreach (string col in cols)
                    vals[col] = form["gft_" + col] ?? string.Empty;
                result[tabla] = vals;
            }
            return result;
        }

        public int CrearIdiomaConTraducciones(
            string codigo,
            string nombre,
            Dictionary<string, Dictionary<string, string>> valoresPorTabla)
            => mpp.CrearIdiomaConTraducciones(codigo.Trim().ToUpper(), nombre.Trim(), valoresPorTabla);

        public void ActualizarTraducciones(
            int idiomaId,
            Dictionary<string, Dictionary<string, string>> valoresPorTabla)
            => mpp.ActualizarTraducciones(idiomaId, valoresPorTabla);
    }
}
