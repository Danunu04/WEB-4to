using System;
using System.Web.UI;
using BLL;
using BE;
using Servicios.Singleton;

namespace gymAppV2
{
    public partial class Default : BasePage
    {
        private BLLAlumno bllAlumno;

        protected void Page_Load(object sender, EventArgs e)
        {
            VerificarAcceso("CheckIn");
            bllAlumno = new BLLAlumno();

            if (!IsPostBack)
            {
                pnlResultado.Visible = false;
            }
        }

        protected void Button2_Click(object sender, EventArgs e)
        {
            pnlResultado.Visible = false;
            string dniTexto = TextBox1.Text.Trim();

            if (string.IsNullOrEmpty(dniTexto))
            {
                lblResultadoTitulo.Text = "Campo requerido";
                lblResultadoDetalle.Text = "Por favor ingresá el DNI";
                pnlResultado.CssClass = "resultado-panel resultado-error";
                pnlResultado.Visible = true;
                return;
            }

            if (!int.TryParse(dniTexto, out int dni))
            {
                lblResultadoTitulo.Text = "DNI inválido";
                lblResultadoDetalle.Text = "El DNI debe contener solo números";
                pnlResultado.CssClass = "resultado-panel resultado-error";
                pnlResultado.Visible = true;
                return;
            }

            try
            {
                Alumno alumno = bllAlumno.RealizarCheckin(dni);

                string nombre = string.IsNullOrWhiteSpace(alumno.Nombre)
                    ? $"DNI {dni}"
                    : $"{alumno.Nombre} {alumno.Apellido}";

                lblResultadoTitulo.Text = $"¡Bienvenid@, {nombre}!";

                if (!alumno.DiasRestantes.HasValue)
                {
                    // Modalidad diaria: mostrar fecha de vencimiento
                    lblResultadoDetalle.Text = $"Membresía activa · Válida hasta el {alumno.FechaVencimiento.Value:dd/MM/yyyy}";
                }
                else
                {
                    int restantes = alumno.DiasRestantes.Value - 1; // ya se decrementó en BD
                    lblResultadoDetalle.Text = $"Membresía activa · Te {(restantes == 1 ? "queda" : "quedan")} {restantes} {(restantes == 1 ? "clase" : "clases")}";
                }

                pnlResultado.CssClass = "resultado-panel resultado-ok";
                pnlResultado.Visible = true;
                TextBox1.Text = "";
            }
            catch (Exception ex)
            {
                lblResultadoTitulo.Text = "Acceso denegado";
                lblResultadoDetalle.Text = ex.Message;
                pnlResultado.CssClass = "resultado-panel resultado-error";
                pnlResultado.Visible = true;
            }
        }
    }
}
