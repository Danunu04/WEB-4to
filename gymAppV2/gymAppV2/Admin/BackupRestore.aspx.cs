using System;
using System.IO;
using System.Web.UI;
using BE;
using BLL;
using Servicios.Singleton;

namespace gymAppV2.Admin
{
    /// <summary>
    /// Página de administración para realizar backups y restores de la base de datos.
    /// Requiere permisos específicos de Backup y/o Restore según la sección.
    /// </summary>
    public partial class BackupRestore : BasePage
    {
        private BLLDigitoVerificador bllDV;
        private BLLEvento bllEvento;

        protected void Page_Load(object sender, EventArgs e)
        {
            bllDV = new BLLDigitoVerificador();
            bllEvento = new BLLEvento();

            if (!IsPostBack)
            {
                txtRutaBackup.Text = ObtenerRutaSugeridaBackup();
            }
        }

        /// <summary>
        /// Genera una ruta sugerida para el backup con marca de tiempo.
        /// </summary>
        private string ObtenerRutaSugeridaBackup()
        {
            string carpeta = @"C:\Backups";
            string nombre = $"GymApp_{DateTime.Now:yyyyMMdd_HHmmss}.bak";
            return Path.Combine(carpeta, nombre);
        }

        protected void btnGenerarNombre_Click(object sender, EventArgs e)
        {
            txtRutaBackup.Text = ObtenerRutaSugeridaBackup();
        }

        protected void btnRealizarBackup_Click(object sender, EventArgs e)
        {
            try
            {
                VerificarAcceso(PermisosSistema.Backup);

                string ruta = txtRutaBackup.Text.Trim();
                ValidarRutaBackup(ruta);

                bllDV.RealizarBackup(ruta);

                string usuario = Singleton.Instancia.Usuario?.USUARIO_Usuario ?? "sistema";
                bllEvento.RegistrarBackup(usuario);

                MostrarExito($"Backup realizado correctamente: {ruta}");
            }
            catch (Exception ex)
            {
                MostrarError("Error al realizar el backup: " + ex.Message);
            }
        }

        protected void btnRealizarRestore_Click(object sender, EventArgs e)
        {
            try
            {
                VerificarAcceso(PermisosSistema.Restore);

                string ruta = txtRutaRestore.Text.Trim();
                ValidarRutaRestore(ruta);

                bllDV.RestaurarBackup(ruta);

                string usuario = Singleton.Instancia.Usuario?.USUARIO_Usuario ?? "sistema";
                bllEvento.RegistrarRestore(usuario);

                MostrarExito("Backup restaurado correctamente. Reinicie la aplicación.");
            }
            catch (Exception ex)
            {
                MostrarError("Error al restaurar el backup: " + ex.Message);
            }
        }

        /// <summary>
        /// Valida que la ruta destino del backup sea un archivo .bak con directorio válido.
        /// </summary>
        private void ValidarRutaBackup(string ruta)
        {
            if (string.IsNullOrWhiteSpace(ruta))
                throw new Exception("Debe ingresar la ruta de destino.");

            if (!string.Equals(Path.GetExtension(ruta), ".bak", StringComparison.OrdinalIgnoreCase))
                throw new Exception("El archivo debe tener extensión .bak.");

            string directorio = Path.GetDirectoryName(ruta);
            if (string.IsNullOrWhiteSpace(directorio))
                throw new Exception("La ruta de destino no es válida.");
        }

        /// <summary>
        /// Valida que la ruta del backup a restaurar exista y tenga extensión .bak.
        /// </summary>
        private void ValidarRutaRestore(string ruta)
        {
            if (string.IsNullOrWhiteSpace(ruta))
                throw new Exception("Debe ingresar la ruta del backup.");

            if (!string.Equals(Path.GetExtension(ruta), ".bak", StringComparison.OrdinalIgnoreCase))
                throw new Exception("El archivo debe tener extensión .bak.");

            if (!File.Exists(ruta))
                throw new Exception("No se encontró el archivo de backup especificado.");
        }
    }
}
