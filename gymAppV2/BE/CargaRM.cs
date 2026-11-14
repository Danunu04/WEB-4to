namespace BE
{
    /// <summary>
    /// Fila de la tabla de cargas de entrenamiento: un porcentaje de la 1RM, el peso que
    /// le corresponde y cuántas repeticiones se pueden hacer aproximadamente con ese peso.
    /// </summary>
    public class CargaRM
    {
        public int Porcentaje { get; set; }
        public decimal Peso { get; set; }
        public int RepeticionesAprox { get; set; }
    }
}
