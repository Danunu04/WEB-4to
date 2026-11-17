using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace gymAppV2.Controles
{
    /// <summary>
    /// Paginador para listas armadas con Repeater (el Repeater no pagina solo).
    /// Se ve y se comporta como el paginador de los GridView: números de a bloques
    /// de 10 y "..." para pasar al bloque anterior o siguiente.
    ///
    /// Uso en la página:
    ///   rptLista.DataSource = pagLista.Paginar(lista);   // devuelve solo la página actual
    ///   pagLista_CambioPagina -> volver a cargar la lista
    ///   Al cambiar un filtro -> pagLista.Reiniciar() antes de cargar
    /// </summary>
    public partial class Paginador : UserControl
    {
        private const int BotonesPorBloque = 10;

        /// <summary>Filas por página. Igual que el PageSize de los GridView.</summary>
        public int TamanoPagina { get; set; } = 10;

        /// <summary>Índice de la página actual, empezando en 0.</summary>
        public int PaginaActual
        {
            get { return ViewState["PaginaActual"] as int? ?? 0; }
            set { ViewState["PaginaActual"] = Math.Max(0, value); }
        }

        /// <summary>Se dispara cuando el usuario elige otra página.</summary>
        public event EventHandler CambioPagina;

        /// <summary>Vuelve a la primera página (por ejemplo, al cambiar un filtro).</summary>
        public void Reiniciar()
        {
            PaginaActual = 0;
        }

        /// <summary>
        /// Arma los botones para la cantidad de datos y devuelve solo los de la página actual.
        /// Si la página quedó fuera de rango (se borraron filas o cambió un filtro), pasa a la última.
        /// </summary>
        public List<T> Paginar<T>(IList<T> datos)
        {
            int total = datos == null ? 0 : datos.Count;
            int cantidadPaginas = (total + TamanoPagina - 1) / TamanoPagina;

            if (PaginaActual >= cantidadPaginas)
                PaginaActual = cantidadPaginas - 1;

            ArmarBotones(cantidadPaginas);

            if (total == 0)
                return new List<T>();

            return datos.Skip(PaginaActual * TamanoPagina).Take(TamanoPagina).ToList();
        }

        private void ArmarBotones(int cantidadPaginas)
        {
            pnlPaginador.Visible = cantidadPaginas > 1;
            if (!pnlPaginador.Visible)
            {
                rptPaginas.DataSource = null;
                rptPaginas.DataBind();
                return;
            }

            int inicioBloque = PaginaActual / BotonesPorBloque * BotonesPorBloque;
            int finBloque = Math.Min(inicioBloque + BotonesPorBloque, cantidadPaginas);

            var botones = new List<BotonPagina>();
            if (inicioBloque > 0)
                botones.Add(new BotonPagina(inicioBloque - 1, "...", false));

            for (int i = inicioBloque; i < finBloque; i++)
                botones.Add(new BotonPagina(i, (i + 1).ToString(), i == PaginaActual));

            if (finBloque < cantidadPaginas)
                botones.Add(new BotonPagina(finBloque, "...", false));

            rptPaginas.DataSource = botones;
            rptPaginas.DataBind();
        }

        protected void rptPaginas_ItemCommand(object source, RepeaterCommandEventArgs e)
        {
            if (e.CommandName != "Pagina" || !int.TryParse(Convert.ToString(e.CommandArgument), out int pagina))
                return;

            PaginaActual = pagina;
            CambioPagina?.Invoke(this, EventArgs.Empty);
        }

        public class BotonPagina
        {
            public BotonPagina(int indice, string texto, bool actual)
            {
                Indice = indice;
                Texto = texto;
                Actual = actual;
            }

            public int Indice { get; private set; }
            public string Texto { get; private set; }
            public bool Actual { get; private set; }
        }
    }
}
