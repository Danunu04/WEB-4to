using System.Collections.Generic;

namespace BE
{
    public class TagTraduccion
    {
        public string Tag         { get; set; }
        public string ValorTarget { get; set; }
    }

    public class SeccionTraduccion
    {
        public string              NombreTabla { get; set; }
        public List<TagTraduccion> Tags        { get; set; } = new List<TagTraduccion>();
    }
}
