using System.Collections.Generic;
using System.Linq;
using System.Web;
using BE;

namespace SERVICIOS.Observer
{
    /// <summary>
    /// Sujeto del patrón Observer para el sistema de idiomas.
    ///
    /// El idioma activo se persiste en Session para sobrevivir entre requests.
    /// Los observers se almacenan en HttpContext.Current.Items (ámbito de request)
    /// para evitar referencias a objetos de página entre requests, lo que causaría
    /// memory leaks y referencias inválidas.
    ///
    /// Flujo:
    ///   1. Cada página/master se suscribe en OnInit/Page_Load via Suscribir().
    ///   2. Al llamar CambiarIdioma(), el idioma se guarda en Session y se notifica
    ///      a todos los observers del request actual.
    ///   3. En requests posteriores, cada página lee IdiomaActual y aplica las
    ///      traducciones en su propio OnLoad.
    /// </summary>
    public static class GestorIdioma
    {
        private const string SESSION_KEY = "GestorIdioma_Idioma";
        private const string ITEMS_KEY   = "GestorIdioma_Observers";

        public static IdiomaApp IdiomaActual
        {
            get
            {
                var session = HttpContext.Current?.Session;
                if (session?[SESSION_KEY] is IdiomaApp idioma)
                    return idioma;
                return IdiomaApp.ES;
            }
        }

        private static List<IIdiomaObserver> Observers
        {
            get
            {
                var items = HttpContext.Current?.Items;
                if (items == null)
                    return new List<IIdiomaObserver>();
                if (items[ITEMS_KEY] == null)
                    items[ITEMS_KEY] = new List<IIdiomaObserver>();
                return (List<IIdiomaObserver>)items[ITEMS_KEY];
            }
        }

        public static void Suscribir(IIdiomaObserver observer)
        {
            var observers = Observers;
            if (!observers.Contains(observer))
                observers.Add(observer);
        }

        public static void Desuscribir(IIdiomaObserver observer)
        {
            Observers.Remove(observer);
        }

        /// <summary>
        /// Cambia el idioma activo, lo persiste en Session y notifica a todos
        /// los observers registrados en el request actual.
        /// </summary>
        public static void CambiarIdioma(IdiomaApp idioma)
        {
            var session = HttpContext.Current?.Session;
            if (session != null)
                session[SESSION_KEY] = idioma;

            foreach (var observer in Observers.ToList())
            {
                try { observer.OnIdiomaChanged(idioma); }
                catch { }
            }
        }
    }
}
