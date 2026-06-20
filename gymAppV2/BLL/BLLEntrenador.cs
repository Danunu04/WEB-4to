using System;
using System.Collections.Generic;
using BE;
using MPP;

namespace BLL
{
    public class BLLEntrenador
    {
        private MPPEntrenador mppEntrenador;
        private BLLEvento bllEvento;

        public BLLEntrenador()
        {
            mppEntrenador = new MPPEntrenador();
            bllEvento = new BLLEvento();
        }

        private void RegistrarEvento(string tipo, string accion, int criticidad = 3)
        {
            try
            {
                var usuario = System.Web.HttpContext.Current?.Session["UsuarioLogueado"] as Usuario;
                if (usuario == null)
                {
                    // No registrar evento si no hay usuario válido
                    return;
                }
                bllEvento.RegistrarEvento(tipo, usuario.USUARIO_Usuario, accion, criticidad);
            }
            catch
            {
                // No impedir la operación principal si falla el log
            }
        }

        public void CrearEntrenador(Entrenador entrenador)
        {
            try
            {
                if (mppEntrenador.EntrenadorExiste(entrenador.DNI))
                {
                    throw new Exception("Ya existe un entrenador con ese DNI");
                }

                mppEntrenador.CrearEntrenador(entrenador);

                var usuario = System.Web.HttpContext.Current?.Session["UsuarioLogueado"] as Usuario;
                if (usuario != null)
                {
                    bllEvento.RegistrarAltaEntrenador(usuario.USUARIO_Usuario, entrenador.DNI);
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Error al crear entrenador: " + ex.Message, ex);
            }
        }

        public Entrenador ObtenerEntrenador(int dni)
        {
            try
            {
                return mppEntrenador.ObtenerEntrenador(dni);
            }
            catch (Exception ex)
            {
                throw new Exception("Error al obtener entrenador: " + ex.Message, ex);
            }
        }

        public void ActualizarEntrenador(Entrenador entrenador)
        {
            try
            {
                mppEntrenador.ActualizarEntrenador(entrenador);

                var usuario = System.Web.HttpContext.Current?.Session["UsuarioLogueado"] as Usuario;
                if (usuario != null)
                {
                    bllEvento.RegistrarModificacionEntrenador(usuario.USUARIO_Usuario, entrenador.DNI);
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Error al actualizar entrenador: " + ex.Message, ex);
            }
        }

        public void EliminarEntrenador(int dni)
        {
            try
            {
                mppEntrenador.EliminarEntrenador(dni);

                var usuario = System.Web.HttpContext.Current?.Session["UsuarioLogueado"] as Usuario;
                if (usuario != null)
                {
                    bllEvento.RegistrarBajaEntrenador(usuario.USUARIO_Usuario, dni);
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Error al eliminar entrenador: " + ex.Message, ex);
            }
        }

        public List<Entrenador> ListarEntrenadores()
        {
            try
            {
                return mppEntrenador.ListarEntrenadores();
            }
            catch (Exception ex)
            {
                throw new Exception("Error al listar entrenadores: " + ex.Message, ex);
            }
        }

        public Dictionary<string, int> ObtenerEstadisticas()
        {
            try
            {
                return mppEntrenador.ObtenerEstadisticas();
            }
            catch (Exception ex)
            {
                throw new Exception("Error al obtener estadísticas: " + ex.Message, ex);
            }
        }
    }
}
