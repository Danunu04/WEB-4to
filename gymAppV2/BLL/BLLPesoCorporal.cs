using System;
using System.Collections.Generic;
using System.Linq;
using BE;
using MPP;

namespace BLL
{
    /// <summary>
    /// Registro del peso corporal (CUN001). El cliente o familiar lo carga desde su Perfil para
    /// los alumnos asociados a su cuenta: se actualiza el peso actual del alumno y se agrega al
    /// historial (PesoHistorial) para seguir su evolución.
    /// </summary>
    public class BLLPesoCorporal
    {
        /// <summary>Cuántos registros anteriores se muestran por alumno.</summary>
        public const int REGISTROS_A_MOSTRAR = 3;

        private MPPPesoCorporal mppPesoCorporal;
        private MPPAlumno mppAlumno;
        private BLLAlumno bllAlumno;
        private BLLEvento bllEvento;

        public BLLPesoCorporal()
        {
            mppPesoCorporal = new MPPPesoCorporal();
            mppAlumno = new MPPAlumno();
            bllAlumno = new BLLAlumno();
            bllEvento = new BLLEvento();
        }

        /// <summary>Alumnos activos de la cuenta.</summary>
        public List<Alumno> ListarAlumnosDeUsuario(string usuario)
        {
            return (bllAlumno.ObtenerAlumnosDeUsuario(usuario) ?? new List<Alumno>())
                .Where(a => a.Activo)
                .ToList();
        }

        public List<RegistroPeso> ListarUltimos(int dni)
        {
            try
            {
                return mppPesoCorporal.ListarUltimos(dni, REGISTROS_A_MOSTRAR);
            }
            catch (Exception ex)
            {
                throw new Exception("Error al listar el historial de peso: " + ex.Message, ex);
            }
        }

        /// <summary>
        /// Registra el peso de hoy para un alumno de la cuenta: valida el valor (mayor que 0 y
        /// menor que 500 kg), actualiza el peso del alumno y lo agrega al historial.
        /// </summary>
        public void RegistrarPeso(string usuario, int dni, decimal peso)
        {
            if (peso <= 0 || peso >= 500)
            {
                throw new ReglaNegocioException("perfil_err_peso", "El peso debe ser un número mayor que 0 y menor que 500 kg.");
            }

            bool esSuyo = !string.IsNullOrEmpty(usuario)
                && ListarAlumnosDeUsuario(usuario).Any(a => a.DNI == dni);
            if (!esSuyo)
            {
                throw new ReglaNegocioException("perfil_err_no_asociado", "Ese alumno no está asociado a tu cuenta o está dado de baja.");
            }

            try
            {
                Alumno alumno = mppAlumno.ObtenerAlumno(dni);
                alumno.Peso = Math.Round(peso, 2);
                mppAlumno.ActualizarAlumno(alumno);

                mppPesoCorporal.Insertar(new RegistroPeso
                {
                    Dni = dni,
                    Peso = alumno.Peso.Value,
                    Fecha = DateTime.Today
                });

                bllEvento.RegistrarCambioDatosAlumno(usuario, dni, $"peso corporal ({alumno.Peso.Value:0.##} kg)");
            }
            catch (Exception ex)
            {
                throw new Exception("Error al registrar el peso: " + ex.Message, ex);
            }
        }
    }
}
