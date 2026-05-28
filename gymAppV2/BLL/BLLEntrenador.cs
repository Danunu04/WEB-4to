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

        private void RegistrarEvento(string tipo, string accion)
        {
            try
            {
                var usuario = System.Web.HttpContext.Current?.Session["UsuarioLogueado"] as Usuario;
                string usr = usuario?.USUARIO_Usuario ?? "sistema";
                bllEvento.RegistrarEvento(tipo, usr, accion);
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
                RegistrarEvento("alta_entrenador", $"Entrenador DNI {entrenador.DNI} creado");
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
                RegistrarEvento("modificacion_entrenador", $"Entrenador DNI {entrenador.DNI} modificado");
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
                RegistrarEvento("baja_entrenador", $"Entrenador DNI {dni} eliminado");
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
