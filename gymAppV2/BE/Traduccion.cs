namespace BE
{
    public class Traduccion
    {
        public int TraduccionID { get; set; }
        public string TraduccionTAG { get; set; }
        public string Español { get; set; }
        public string Ingles { get; set; }
        public string Portugues { get; set; }
        public string Frances { get; set; }
        public string Japones { get; set; }

        public string ObtenerTexto(IdiomaApp idioma)
        {
            switch (idioma)
            {
                case IdiomaApp.EN: return Ingles;
                case IdiomaApp.PT: return Portugues;
                case IdiomaApp.FR: return Frances;
                case IdiomaApp.JA: return Japones;
                default:           return Español;
            }
        }
    }
}
