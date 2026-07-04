using System;
using System.Collections.Generic;
using SERVICIOS;

namespace BLL
{
    /// <summary>
    /// Orquesta la migración de encriptación reversible de datos personales.
    /// </summary>
    public class BLLCriptoMigracion
    {
        private readonly CriptoMigracion _migracion;

        public BLLCriptoMigracion()
        {
            _migracion = new CriptoMigracion();
        }

        public List<CriptoMigracion.ResultadoMigracion> EncriptarTodo()
        {
            try
            {
                return _migracion.EncriptarTodo();
            }
            catch (Exception ex)
            {
                throw new Exception("Error al ejecutar la migración de encriptación: " + ex.Message, ex);
            }
        }

        public CriptoMigracion.ResultadoMigracion EncriptarCampo(string tabla, string campo, bool esFecha = false)
        {
            try
            {
                return _migracion.EncriptarCampo(tabla, campo, true, esFecha);
            }
            catch (Exception ex)
            {
                throw new Exception($"Error al encriptar {tabla}.{campo}: " + ex.Message, ex);
            }
        }
    }
}
