using System;
using System.Collections.Generic;
using System.Web;
using BE;
using MPP;

namespace BLL
{
    /// <summary>
    /// Lógica de negocio para la gestión de rutinas de entrenamiento.
    /// </summary>
    public class BLLRutina
    {
        private MPPRutina mppRutina;
        private BLLEvento bllEvento;

        public BLLRutina()
        {
            mppRutina = new MPPRutina();
            bllEvento = new BLLEvento();
        }

        private void ValidarRutina(Rutina rutina)
        {
            if (string.IsNullOrWhiteSpace(rutina.Descripcion))
            {
                throw new Exception("La descripción de la rutina es obligatoria");
            }

            if (rutina.Fecha == DateTime.MinValue)
            {
                throw new Exception("La fecha de la rutina es obligatoria");
            }

            if (rutina.DniAlumno <= 0)
            {
                throw new Exception("Debe seleccionar un alumno");
            }

            if (rutina.DniEntrenador <= 0)
            {
                throw new Exception("Debe seleccionar un entrenador");
            }

            if (rutina.CodActividad <= 0)
            {
                throw new Exception("Debe seleccionar una actividad");
            }
        }

        public List<Rutina> ListarRutinas()
        {
            try
            {
                return mppRutina.ListarRutinas();
            }
            catch (Exception ex)
            {
                throw new Exception("Error al listar rutinas: " + ex.Message, ex);
            }
        }

        /// <summary>
        /// Lista las rutinas de los alumnos asociados al usuario Cliente logueado.
        /// </summary>
        public List<Rutina> ListarRutinasPorCliente(string usuario)
        {
            try
            {
                if (string.IsNullOrEmpty(usuario))
                {
                    return new List<Rutina>();
                }

                return mppRutina.ListarRutinasPorCliente(usuario);
            }
            catch (Exception ex)
            {
                throw new Exception("Error al listar rutinas del cliente: " + ex.Message, ex);
            }
        }

        public Rutina ObtenerRutina(int codRutina)
        {
            try
            {
                return mppRutina.ObtenerRutina(codRutina);
            }
            catch (Exception ex)
            {
                throw new Exception("Error al obtener rutina: " + ex.Message, ex);
            }
        }

        public void CrearRutina(Rutina rutina)
        {
            try
            {
                ValidarRutina(rutina);

                int codRutina = mppRutina.CrearRutina(rutina);
                rutina.CodRutina = codRutina;

                var usuario = HttpContext.Current?.Session["UsuarioLogueado"] as Usuario;
                if (usuario != null)
                {
                    bllEvento.RegistrarRutinaAlta(usuario.USUARIO_Usuario, rutina.DniAlumno);
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Error al crear rutina: " + ex.Message, ex);
            }
        }

        public void ActualizarRutina(Rutina rutina)
        {
            try
            {
                ValidarRutina(rutina);

                mppRutina.ActualizarRutina(rutina);

                var usuario = HttpContext.Current?.Session["UsuarioLogueado"] as Usuario;
                if (usuario != null)
                {
                    bllEvento.RegistrarRutinaModificacion(usuario.USUARIO_Usuario, rutina.DniAlumno);
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Error al actualizar rutina: " + ex.Message, ex);
            }
        }

        public void EliminarRutina(int codRutina)
        {
            try
            {
                var rutina = mppRutina.ObtenerRutina(codRutina);

                mppRutina.EliminarRutina(codRutina);

                var usuario = HttpContext.Current?.Session["UsuarioLogueado"] as Usuario;
                if (usuario != null && rutina != null)
                {
                    bllEvento.RegistrarRutinaBaja(usuario.USUARIO_Usuario, rutina.DniAlumno);
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Error al eliminar rutina: " + ex.Message, ex);
            }
        }
    }
}
