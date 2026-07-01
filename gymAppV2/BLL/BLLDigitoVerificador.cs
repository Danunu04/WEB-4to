using System;
using System.Collections.Generic;
using BE;
using MPP;
using Servicios.Singleton;

namespace BLL
{
    /// <summary>
    /// Lógica de negocio para verificación y mantenimiento de dígitos verificadores.
    /// </summary>
    public class BLLDigitoVerificador
    {
        private MPPDigitoVerificador mppDigitoVerificador;
        private MPPUsuario mppUsuario;
        private MPPPreguntaSeguridad mppPregunta;

        public BLLDigitoVerificador()
        {
            mppDigitoVerificador = new MPPDigitoVerificador();
            mppUsuario = new MPPUsuario();
            mppPregunta = new MPPPreguntaSeguridad();
        }

        /// <summary>
        /// Verifica la integridad de todas las tablas registradas en DigitoVerificador.
        /// Devuelve una lista con los resultados detallados (tabla, fila, campo).
        /// Las tablas encriptadas se verifican con su MPP especializado.
        /// </summary>
        public List<ResultadoVerificacionDV> VerificarIntegridad()
        {
            try
            {
                var resultados = new List<ResultadoVerificacionDV>();
                List<string> tablas = mppDigitoVerificador.ObtenerTablasConControl();

                foreach (string tabla in tablas)
                {
                    if (tabla.Equals("USUARIOS", StringComparison.OrdinalIgnoreCase))
                    {
                        resultados.AddRange(mppUsuario.VerificarIntegridadUsuarios());
                    }
                    else if (tabla.Equals("PreguntasSeguridad", StringComparison.OrdinalIgnoreCase))
                    {
                        resultados.AddRange(mppPregunta.VerificarIntegridadPreguntas());
                    }
                    else
                    {
                        resultados.AddRange(mppDigitoVerificador.VerificarIntegridadTabla(tabla));
                    }
                }

                return resultados;
            }
            catch (Exception ex)
            {
                throw new Exception("Error al verificar integridad: " + ex.Message, ex);
            }
        }

        /// <summary>
        /// Indica si existe al menos un error de integridad en las tablas registradas.
        /// Si no se puede verificar (por ejemplo, tabla inexistente), se asume que hay error
        /// para forzar revisión y no continuar con datos dudosos.
        /// </summary>
        public bool ExisteErrorIntegridad()
        {
            try
            {
                var resultados = VerificarIntegridad();
                return resultados.Exists(r => !r.EsValido);
            }
            catch
            {
                // Si no se puede verificar, se asume error crítico.
                return true;
            }
        }

        /// <summary>
        /// Indica si el sistema debe pausarse por error de integridad.
        /// Equivale a ExisteErrorIntegridad pero con nombre semántico para la capa UI.
        /// </summary>
        public bool SistemaDebePausarse()
        {
            return ExisteErrorIntegridad();
        }

        /// <summary>
        /// Devuelve las tablas con columnas dvv/dvh que aún no tienen registro de control.
        /// </summary>
        public List<string> ObtenerTablasSinControl()
        {
            try
            {
                return mppDigitoVerificador.ObtenerTablasSinControl();
            }
            catch (Exception ex)
            {
                throw new Exception("Error al obtener tablas sin control: " + ex.Message, ex);
            }
        }

        /// <summary>
        /// Registra en DigitoVerificador todas las tablas del schema que tienen
        /// dvv/dvh y aún no están controladas. Usar solo para la configuración inicial.
        /// Las tablas encriptadas se procesan con su MPP especializado.
        /// </summary>
        public void InicializarControl()
        {
            try
            {
                List<string> tablas = mppDigitoVerificador.ObtenerTablasSinControl();

                foreach (string tabla in tablas)
                {
                    RecalcularTabla(tabla);
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Error al inicializar control de dígitos verificadores: " + ex.Message, ex);
            }
        }

        /// <summary>
        /// Recalcula DVH/DVV de todas las filas de todas las tablas que tienen
        /// columnas dvv/dvh, incluyendo las que aún no están registradas en
        /// DigitoVerificador. Esto "corrige" los dígitos para que coincidan con
        /// el estado actual, pero no revierte los cambios de datos.
        /// Las tablas encriptadas se procesan con su MPP especializado.
        /// </summary>
        public void RecalcularDigitos()
        {
            try
            {
                HashSet<string> tablas = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

                foreach (string tabla in mppDigitoVerificador.ObtenerTablasConControl())
                    tablas.Add(tabla);

                foreach (string tabla in mppDigitoVerificador.ObtenerTablasSinControl())
                    tablas.Add(tabla);

                foreach (string tabla in tablas)
                {
                    RecalcularTabla(tabla);
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Error al recalcular dígitos verificadores: " + ex.Message, ex);
            }
        }

        /// <summary>
        /// Registra o recalcula una tabla en DigitoVerificador con sus hashes actuales.
        /// Las tablas encriptadas se procesan con su MPP especializado.
        /// </summary>
        public void RegistrarTabla(string nombreTabla)
        {
            try
            {
                RecalcularTabla(nombreTabla);
            }
            catch (Exception ex)
            {
                throw new Exception($"Error al registrar tabla {nombreTabla}: " + ex.Message, ex);
            }
        }

        /// <summary>
        /// Devuelve un resumen del estado de control de todas las tablas con dvv/dvh.
        /// </summary>
        public List<EstadoControlDV> ObtenerEstadoControl()
        {
            try
            {
                return mppDigitoVerificador.ObtenerEstadoControl();
            }
            catch (Exception ex)
            {
                throw new Exception("Error al obtener estado de control: " + ex.Message, ex);
            }
        }

        /// <summary>
        /// Recalcula DVH/DVV de una tabla. Si la tabla contiene datos encriptados,
        /// utiliza el MPP especializado que desencripta antes de hashear.
        /// </summary>
        private void RecalcularTabla(string nombreTabla)
        {
            string tabla = nombreTabla ?? string.Empty;

            if (tabla.Equals("USUARIOS", StringComparison.OrdinalIgnoreCase))
            {
                mppUsuario.RecalcularDigitosTodosUsuarios();
                mppDigitoVerificador.RecalcularDigitosTabla(tabla, actualizarFilas: false);
                return;
            }

            if (tabla.Equals("PreguntasSeguridad", StringComparison.OrdinalIgnoreCase))
            {
                mppPregunta.RecalcularDigitosTodasPreguntas();
                mppDigitoVerificador.RecalcularDigitosTabla(tabla, actualizarFilas: false);
                return;
            }

            mppDigitoVerificador.RecalcularDigitosTabla(tabla);
        }

        /// <summary>
        /// Restaura la base de datos desde un backup previo.
        /// ADVERTENCIA: operación destructiva. Requiere permisos de sysadmin o dbcreator.
        /// </summary>
        public void RestaurarBackup(string rutaBackup)
        {
            try
            {
                mppDigitoVerificador.RestaurarBackup(rutaBackup);
            }
            catch (Exception ex)
            {
                throw new Exception("Error al restaurar el backup. Verifique permisos y ruta: " + ex.Message, ex);
            }
        }

        /// <summary>
        /// Verifica si el usuario logueado es administrador (rol 1).
        /// </summary>
        public bool UsuarioActualEsAdmin()
        {
            var usuario = Singleton.Instancia.Usuario;
            return usuario != null && usuario.USUARIO_Rol == 1;
        }
    }
}
