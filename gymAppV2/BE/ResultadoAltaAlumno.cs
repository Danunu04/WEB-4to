namespace BE
{
    /// <summary>
    /// Resultado del alta de un alumno: la cuenta de Cliente que se le creó (si no tenía) y la
    /// contraseña generada (si no se informó una), para comunicárselas.
    /// </summary>
    public class ResultadoAltaAlumno
    {
        public int Dni { get; set; }
        public string UsuarioCreado { get; set; }
        public string ContrasenaGenerada { get; set; }
    }
}
