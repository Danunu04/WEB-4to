namespace BE
{
    /// <summary>
    /// Salón/aula del gimnasio (tabla Aulas). Su cupo es la cantidad máxima de alumnos
    /// por clase en ese espacio. Cada gimnasio carga las suyas.
    /// </summary>
    public class Aula
    {
        public int CodAula { get; set; }
        public string Nombre { get; set; }
        public int Cupo { get; set; }
        public bool Activo { get; set; } = true;

        // Campo de verificación de integridad
        public string DVH { get; set; }
    }
}
