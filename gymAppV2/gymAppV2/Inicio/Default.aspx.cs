using System;
using System.Web.UI;
using System.Web.UI.WebControls;
using BE;
using BLL;

namespace gymAppV2
{
    public partial class Default : Page
    {
        private BLLAlumno bllAlumno;
        private BLLEvento bllEvento;
        private const string USUARIO_SISTEMA = "sistema";

        protected void Page_Load(object sender, EventArgs e)
        {
            bllAlumno = new BLLAlumno();
            bllEvento = new BLLEvento();
        }

        protected void Button1_Click(object sender, EventArgs e)
        {
            Response.Redirect("~/LogIn/LogIn.aspx");
        }

        protected void Button2_Click(object sender, EventArgs e)
        {
            string dniTexto = TextBox1.Text.Trim();
            if (string.IsNullOrEmpty(dniTexto))
            {
                MostrarError("Por favor ingresa tu DNI");
                return;
            }

            if (!int.TryParse(dniTexto, out int dni))
            {
                MostrarError("El DNI debe ser numérico");
                return;
            }

            try
            {
                Alumno alumno = bllAlumno.ObtenerAlumno(dni);

                if (alumno == null)
                {
                    // Alumno no existe: registrar intento fallido de check-in.
                    bllEvento.RegistrarEvento(BLLEvento.EVENTO_CHECKIN, USUARIO_SISTEMA,
                        $"Intento de check-in rechazado - DNI: {dni} (alumno no existe)", 2, "Alumnos");
                    MostrarAdvertencia("No se encontró un alumno con ese DNI");
                    return;
                }

                if (!alumno.Activo)
                {
                    // Alumno inactivo se trata como membresía vencida hasta tener campo de vencimiento.
                    bllEvento.RegistrarEvento(BLLEvento.EVENTO_CHECKIN, USUARIO_SISTEMA,
                        $"Intento de check-in rechazado - Alumno DNI: {dni} (membresía vencida/inactivo)", 2, "Alumnos");
                    MostrarAdvertencia("Tu membresía se encuentra vencida o inactiva");
                    return;
                }

                // Check-in exitoso.
                bllEvento.RegistrarCheckin(USUARIO_SISTEMA, dni);
                MostrarExito($"¡Bienvenido/a, {alumno.Nombre} {alumno.Apellido}!");
            }
            catch (Exception ex)
            {
                MostrarError("Error al procesar el check-in: " + ex.Message);
            }
        }

        private void MostrarError(string mensaje)
        {
            ScriptManager.RegisterStartupScript(this, GetType(), "error", $"if(window.showToast) showToast('{System.Security.SecurityElement.Escape(mensaje)}', 'error');", true);
        }

        private void MostrarExito(string mensaje)
        {
            ScriptManager.RegisterStartupScript(this, GetType(), "exito", $"if(window.showToast) showToast('{System.Security.SecurityElement.Escape(mensaje)}', 'success');", true);
        }

        private void MostrarAdvertencia(string mensaje)
        {
            ScriptManager.RegisterStartupScript(this, GetType(), "advertencia", $"if(window.showToast) showToast('{System.Security.SecurityElement.Escape(mensaje)}', 'warning');", true);
        }

        private void MostrarInfo(string mensaje)
        {
            ScriptManager.RegisterStartupScript(this, GetType(), "info", $"if(window.showToast) showToast('{System.Security.SecurityElement.Escape(mensaje)}', 'info');", true);
        }
    }
}