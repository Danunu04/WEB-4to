using System;
using System.Collections.Generic;
using System.Linq;
using BE;
using MPP;
using Servicios.Singleton;

namespace BLL
{
    /// <summary>
    /// Aulas del gimnasio: alta, modificación y baja lógica. El cupo del aula es el máximo de
    /// alumnos por clase de los turnos que se dan ahí (la actividad puede poner un tope menor).
    /// </summary>
    public class BLLAula
    {
        private MPPAula mppAula;
        private MPPActividad mppActividad;
        private BLLEvento bllEvento;

        public BLLAula()
        {
            mppAula = new MPPAula();
            mppActividad = new MPPActividad();
            bllEvento = new BLLEvento();
        }

        public List<Aula> ListarAulas(bool soloActivas = false)
        {
            try
            {
                return mppAula.ListarAulas(soloActivas);
            }
            catch (Exception ex)
            {
                throw new Exception("Error al listar las aulas: " + ex.Message, ex);
            }
        }

        public Aula ObtenerAula(int codAula)
        {
            try
            {
                return mppAula.ObtenerAula(codAula);
            }
            catch (Exception ex)
            {
                throw new Exception("Error al obtener el aula: " + ex.Message, ex);
            }
        }

        /// <summary>
        /// Crea (CodAula = 0) o modifica un aula. Devuelve su código.
        /// </summary>
        public int GuardarAula(Aula aula)
        {
            ValidarPermiso();
            ValidarAula(aula);

            try
            {
                aula.Nombre = aula.Nombre.Trim();
                bool esAlta = aula.CodAula == 0;

                if (esAlta)
                    aula.CodAula = mppAula.CrearAula(aula);
                else
                    mppAula.ActualizarAula(aula);

                RegistrarEvento((esAlta ? "Alta" : "Modificación") + $" de aula: {aula.Nombre} (cupo {aula.Cupo})");
                return aula.CodAula;
            }
            catch (Exception ex)
            {
                throw new Exception("Error al guardar el aula: " + ex.Message, ex);
            }
        }

        /// <summary>
        /// Activa o desactiva un aula. No se puede desactivar si la usa algún turno de una actividad activa.
        /// </summary>
        public void CambiarEstado(int codAula, bool activo)
        {
            ValidarPermiso();

            Aula aula = mppAula.ObtenerAula(codAula);
            if (aula == null)
                throw new InscripcionException("El aula no existe.");

            if (!activo)
            {
                List<ActividadHorario> turnos = mppActividad.ListarHorariosActivosDeAula(codAula);
                if (turnos.Count > 0)
                    throw new ConflictoHorarioException(
                        $"El aula {aula.Nombre} la usan {turnos.Count} turno(s) de actividades activas: " +
                        string.Join(", ", turnos.Take(5).Select(t => $"{t.DescripcionActividad} ({BLLInscripcion.DescribirHorario(t)})")) +
                        ". Cambiales el aula antes de desactivarla.");
            }

            try
            {
                aula.Activo = activo;
                mppAula.ActualizarAula(aula);
                RegistrarEvento((activo ? "Activación" : "Baja") + $" de aula: {aula.Nombre}");
            }
            catch (Exception ex)
            {
                throw new Exception("Error al cambiar el estado del aula: " + ex.Message, ex);
            }
        }

        private void ValidarAula(Aula aula)
        {
            if (string.IsNullOrWhiteSpace(aula.Nombre))
                throw new InscripcionException("El nombre del aula es obligatorio.");

            if (aula.Nombre.Trim().Length > 100)
                throw new InscripcionException("El nombre del aula no puede superar los 100 caracteres.");

            if (aula.Cupo < 1 || aula.Cupo > 1000)
                throw new InscripcionException("El cupo del aula debe estar entre 1 y 1000 personas.");

            if (mppAula.ExisteNombre(aula.Nombre.Trim(), aula.CodAula))
                throw new InscripcionException("Ya existe un aula con ese nombre.");

            if (aula.CodAula == 0)
                return;

            Aula anterior = mppAula.ObtenerAula(aula.CodAula);
            if (anterior == null)
                throw new InscripcionException("El aula no existe.");

            if (!aula.Activo && anterior.Activo && mppActividad.ListarHorariosActivosDeAula(aula.CodAula).Count > 0)
                throw new ConflictoHorarioException($"El aula {anterior.Nombre} la usan actividades activas: cambiales el aula antes de desactivarla.");

            // Si baja el cupo, ninguna clase futura puede quedar con más anotados que el cupo nuevo
            if (aula.Cupo < anterior.Cupo)
            {
                var bllInscripcion = new BLLInscripcion();
                foreach (ActividadHorario turno in mppActividad.ListarHorariosActivosDeAula(aula.CodAula))
                {
                    Actividad actividad = mppActividad.ObtenerActividad(turno.CodActividad);
                    int cupoNuevo = actividad?.CupoMaximo.HasValue == true && actividad.CupoMaximo.Value < aula.Cupo
                        ? actividad.CupoMaximo.Value
                        : aula.Cupo;

                    KeyValuePair<DateTime, int> maximo = bllInscripcion.MaximoAnotadosDesde(turno, DateTime.Today);
                    if (maximo.Value > cupoNuevo)
                        throw new CupoCompletoException(
                            $"{turno.DescripcionActividad} ({BLLInscripcion.DescribirHorario(turno)}) tiene {maximo.Value} alumnos anotados " +
                            $"el {maximo.Key:dd/MM/yyyy} en esta aula: el cupo no puede ser menor a {maximo.Value}.");
                }
            }
        }

        private static void ValidarPermiso()
        {
            if (!new BLLRol().UsuarioActualTieneAcceso(PermisosSistema.GestionAulas))
                throw new InscripcionException("No tenés permiso para gestionar aulas.");
        }

        private void RegistrarEvento(string accion)
        {
            try
            {
                string usuario = Singleton.Instancia?.Usuario?.USUARIO_Usuario;
                if (!string.IsNullOrEmpty(usuario))
                    bllEvento.RegistrarEvento(BLLEvento.EVENTO_CONFIGURACION, usuario, accion, 3, "Aulas");
            }
            catch
            {
                // No impedir la operación principal si falla el log
            }
        }
    }
}
