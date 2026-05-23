using System;
using BE;
using MPP;
using Servicios;
using Servicios.Singleton;
using SERVICIOS;

namespace BLL
{
    public class BLLUsuario
    {
        private MPPUsuario mppUsuario;
        private CriptoManager criptoManager;
        private BLLRol bllRol;
        private BLLPreguntaSeguridad bllPreguntaSeguridad;

        public BLLUsuario()
        {
            mppUsuario = new MPPUsuario();
            criptoManager = new CriptoManager();
            bllRol = new BLLRol();
            bllPreguntaSeguridad = new BLLPreguntaSeguridad();
        }

        public bool ValidarLogin(string usuario, string contrasena)
        {
            bool ok = false;
            try
            {
                if (string.IsNullOrEmpty(usuario) || string.IsNullOrEmpty(contrasena))
                {
                    //throw new ExcepcionesLogIn(ResultadosLogIn.InvalidUsername);
                }

                Usuario usuarioBD = mppUsuario.ObtenerUsuario(usuario);

                if (usuarioBD == null)
                {
                    //throw new ExcepcionesLogIn(ResultadosLogIn.InvalidUsername);
                }

                if (!usuarioBD.USUARIO_Activo)
                {
                    //throw new Exception("El usuario está desactivado");
                }

                if (mppUsuario.UsuarioEstaBloqueado(usuario))
                {
                    //throw new Exception("El usuario está bloqueado por demasiados intentos fallidos");
                }

                string contrasenaHash = criptoManager._686DPGetSHA256(contrasena);
                string contrasenaBD = mppUsuario.ObtenerContrasena(usuario);

                if (contrasenaHash == contrasenaBD)
                {
                    ok = true;
                    return ok;
                }
                else
                {
                    return ok;
                }
            }
            catch (ExcepcionesLogIn)
            {
                throw;
            }
            catch (Exception ex)
            {
                throw new Exception("Error al validar el login: " + ex.Message, ex);
            }
        }

        public void RegistrarIntentoFallido(string usuario)
        {
            try
            {
                mppUsuario.AgregarIntento(usuario);
            }
            catch (Exception ex)
            {
                throw new Exception("Error al registrar el intento fallido: " + ex.Message, ex);
            }
        }

        public int ObtenerIntentosRestantes(string usuario)
        {
            try
            {
                int intentos = mppUsuario.ObtenerIntentos(usuario);
                return 3 - intentos;
            }
            catch (Exception ex)
            {
                throw new Exception("Error al obtener los intentos restantes: " + ex.Message, ex);
            }
        }

        public void ReestablecerIntentos(string usuario)
        {
            try
            {
                mppUsuario.ReestablecerIntentos(usuario);
            }
            catch (Exception ex)
            {
                throw new Exception("Error al reestablecer los intentos: " + ex.Message, ex);
            }
        }

        public Usuario ObtenerUsuario(string usuario)
        {
            try
            {
                return mppUsuario.ObtenerUsuario(usuario);
            }
            catch (Exception ex)
            {
                throw new Exception("Error al obtener el usuario: " + ex.Message, ex);
            }
        }

        public void LogearUsuario(Usuario usuario)
        {
            try
            {

                Singleton.Instancia.LogIn(usuario);
            }
            catch (Exception ex)
            {
                throw new Exception("Error al iniciar sesión: " + ex.Message, ex);
            }
        }

        public void DeslogearUsuario()
        {
            try
            {
                Singleton.Instancia.LogOut();
            }
            catch (Exception ex)
            {
                throw new Exception("Error al cerrar sesión: " + ex.Message, ex);
            }
        }

        public bool UsuarioEstaLogueado()
        {
            try
            {
                return Singleton.Instancia.IsLogged();
            }
            catch
            {
                return false;
            }
        }

        public void ValidarRequisitosContrasena(string contrasena)
        {
            if (string.IsNullOrEmpty(contrasena))
            {
                throw new Exception("La contraseña no puede estar vacía");
            }

            if (contrasena.Length < 6)
            {
                throw new Exception("La contraseña debe tener al menos 6 caracteres");
            }

            bool tieneMayuscula = false;
            bool tieneCaracterEspecial = false;

            foreach (char c in contrasena)
            {
                if (char.IsUpper(c))
                {
                    tieneMayuscula = true;
                }

                if (!char.IsLetterOrDigit(c))
                {
                    tieneCaracterEspecial = true;
                }
            }

            if (!tieneMayuscula)
            {
                throw new Exception("La contraseña debe tener al menos una letra mayúscula");
            }

            if (!tieneCaracterEspecial)
            {
                throw new Exception("La contraseña debe tener al menos un carácter especial");
            }
        }

        public void CambiarContrasena(string usuario, string nuevaContrasena)
        {
            try
            {
                // Validate password requirements
                ValidarRequisitosContrasena(nuevaContrasena);

                // Hash the new password
                string nuevaContrasenaHash = criptoManager._686DPGetSHA256(nuevaContrasena);

                // Check if password was used before
                if (mppUsuario.ContrasenaFueUtilizada(usuario, nuevaContrasenaHash))
                {
                    throw new Exception("No puedes reutilizar una contraseña anterior");
                }

                // Save to history
                mppUsuario.GuardarContrasenaEnHistorial(usuario, nuevaContrasenaHash);

                // Update password
                mppUsuario.ActualizarContrasena(usuario, nuevaContrasenaHash);

                // Reset failed attempts
                mppUsuario.ReestablecerIntentos(usuario);
            }
            catch (Exception ex)
            {
                throw new Exception("Error al cambiar contraseña: " + ex.Message, ex);
            }
        }

        public Rol ObtenerRol(string usuario)
        {
            try
            {
                return bllRol.ObtenerRol(usuario);
            }
            catch (Exception ex)
            {
                throw new Exception("Error al obtener el rol: " + ex.Message, ex);
            }
        }

        public bool RequierePreguntaSeguridad(string usuario)
        {
            try
            {
                int intentos = mppUsuario.ObtenerIntentos(usuario);
                return intentos >= 3;
            }
            catch (Exception ex)
            {
                throw new Exception("Error al verificar si requiere pregunta de seguridad: " + ex.Message, ex);
            }
        }
    }
}