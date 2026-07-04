using System;
using System.Collections.Generic;
using BE;
using MPP;

namespace BLL
{
    /// <summary>
    /// Lógica de negocio para la gestión de actividades y inscripciones de alumnos.
    /// </summary>
    public class BLLActividad
    {
        private MPPActividad mppActividad;

        public BLLActividad()
        {
            mppActividad = new MPPActividad();
        }

        /// <summary>
        /// Lista todas las actividades activas del gimnasio.
        /// </summary>
        public List<Actividad> ListarActividades()
        {
            try
            {
                return mppActividad.ListarActividades();
            }
            catch (Exception ex)
            {
                throw new Exception("Error al listar actividades: " + ex.Message, ex);
            }
        }

        /// <summary>
        /// Lista las actividades en las que están inscriptos los alumnos asociados a un usuario cliente.
        /// Cumple con el requisito de que un Cliente solo vea las clases de sus alumnos inscriptos.
        /// </summary>
        /// <param name="usuario">Nombre de usuario (usr) del cliente logueado.</param>
        public List<Actividad> ListarActividadesPorCliente(string usuario)
        {
            try
            {
                if (string.IsNullOrEmpty(usuario))
                {
                    return new List<Actividad>();
                }

                return mppActividad.ListarActividadesPorCliente(usuario);
            }
            catch (Exception ex)
            {
                throw new Exception("Error al listar actividades del cliente: " + ex.Message, ex);
            }
        }
    }
}
