using System.IO;
using System.Net.Mail;
using System.Text;

namespace SERVICIOS
{
    /// <summary>
    /// Envío de emails con la configuración de system.net/mailSettings del Web.config.
    /// Con deliveryMethod="SpecifiedPickupDirectory" cada mensaje queda como un archivo .eml en la
    /// carpeta configurada en vez de enviarse (sirve para demostrarlo sin servidor ni contraseñas).
    /// Para enviar de verdad alcanza con cambiar esa sección por un servidor SMTP (host, puerto,
    /// usuario y contraseña); el código no cambia.
    /// </summary>
    public class EmailManager
    {
        /// <summary>
        /// Envía un email en HTML. El remitente sale del atributo "from" de mailSettings.
        /// Lanza excepción si la dirección no es válida o el envío falla.
        /// </summary>
        public void Enviar(string destinatario, string asunto, string cuerpoHtml)
        {
            using (var cliente = new SmtpClient())
            using (var mensaje = new MailMessage())
            {
                if (cliente.DeliveryMethod == SmtpDeliveryMethod.SpecifiedPickupDirectory
                    && !string.IsNullOrEmpty(cliente.PickupDirectoryLocation))
                {
                    Directory.CreateDirectory(cliente.PickupDirectoryLocation);
                }

                mensaje.To.Add(destinatario);
                mensaje.Subject = asunto;
                mensaje.SubjectEncoding = Encoding.UTF8;
                mensaje.Body = cuerpoHtml;
                mensaje.BodyEncoding = Encoding.UTF8;
                mensaje.IsBodyHtml = true;

                cliente.Send(mensaje);
            }
        }
    }
}
