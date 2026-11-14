namespace BE
{
    /// <summary>
    /// Ejercicio de fuerza sobre el que se calcula la repetición máxima (tabla Ejercicio).
    /// </summary>
    public class Ejercicio
    {
        public int CodEjercicio { get; set; }
        public string Nombre { get; set; }
        public string GrupoMuscular { get; set; }
        public string Descripcion { get; set; }
        public string DVH { get; set; }
    }
}
