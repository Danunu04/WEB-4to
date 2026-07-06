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
        /// Genera una ruta sugerida con marca de tiempo y extensión según el formato elegido.
        /// </summary>
        private string ObtenerRutaSugeridaBackup(string formato = "bak")
        {
            string carpeta = @"C:\GymApp";
            try { Directory.CreateDirectory(carpeta); } catch { }
            string ext = formato == "bacpac" ? ".bacpac" : ".bak";
            string nombre = $"GymApp_{DateTime.Now:yyyyMMdd_HHmmss}{ext}";
            return Path.Combine(carpeta, nombre);
        }

        protected void btnGenerarNombre_Click(object sender, EventArgs e)
        {
            txtRutaBackup.Text = ObtenerRutaSugeridaBackup(rblFormatoBackup.SelectedValue);
        }

        protected void btnRealizarBackup_Click(object sender, EventArgs e)
        {
            try
            {
                VerificarAcceso(PermisosSistema.Backup);

                string ruta = txtRutaBackup.Text.Trim();
                string formato = rblFormatoBackup.SelectedValue;
                ValidarRutaSegunFormato(ruta, formato, esDestino: true);

                string usuario = Singleton.Instancia.Usuario?.USUARIO_Usuario ?? "sistema";

                if (formato == "bacpac")
                {
                    bllDV.ExportarBacpac(ruta);
                    bllEvento.RegistrarExportarBacpac(usuario);
                    MostrarExito($"Exportación .bacpac completada: {ruta}");
                }
                else
                {
                    bllDV.RealizarBackup(ruta);
                    bllEvento.RegistrarBackup(usuario);
                    MostrarExito($"Backup .bak realizado correctamente: {ruta}");
                }
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
                string formato = rblFormatoRestore.SelectedValue;
                ValidarRutaSegunFormato(ruta, formato, esDestino: false);

                string usuario = Singleton.Instancia.Usuario?.USUARIO_Usuario ?? "sistema";

                if (formato == "bacpac")
                {
                    bllDV.ImportarBacpac(ruta);
                    RegistrarEventoSilencioso(() => bllEvento.RegistrarImportarBacpac(usuario));
                    MostrarExito("Importación .bacpac completada. Reinicie la aplicación.");
                }
                else
                {
                    bllDV.RestaurarBackup(ruta);
                    RegistrarEventoSilencioso(() => bllEvento.RegistrarRestore(usuario));
                    MostrarExito("Backup .bak restaurado correctamente. Reinicie la aplicación.");
                }
            }
            catch (Exception ex)
            {
                MostrarError("Error al restaurar: " + ex.Message);
            }
        }

        /// <summary>
        /// Valida que la ruta tenga la extensión correcta según el formato elegido
        /// y que el directorio/archivo exista según corresponda.
        /// </summary>
        private void ValidarRutaSegunFormato(string ruta, string formato, bool esDestino)
        {
            if (string.IsNullOrWhiteSpace(ruta))
                throw new Exception("Debe ingresar la ruta del archivo.");

            string extEsperada = formato == "bacpac" ? ".bacpac" : ".bak";
            string extActual = Path.GetExtension(ruta);

            if (!string.Equals(extActual, extEsperada, StringComparison.OrdinalIgnoreCase))
                throw new Exception($"Para el formato {formato} el archivo debe tener extensión {extEsperada}.");

            if (esDestino)
            {
                string directorio = Path.GetDirectoryName(ruta);
                if (string.IsNullOrWhiteSpace(directorio))
                    throw new Exception("La ruta de destino no es válida.");

                if (!Directory.Exists(directorio))
                    throw new Exception($"El directorio de destino no existe: {directorio}");
            }
            else
            {
                if (!File.Exists(ruta))
                    throw new Exception("No se encontró el archivo especificado.");
            }
        }

        /// <summary>
        /// Registra el evento sin propagar errores: el restore/import ya fue exitoso
        /// y la BD recién restaurada puede tener un estado diferente al esperado.
        /// </summary>
        private static void RegistrarEventoSilencioso(Action accion)
        {
            try { accion(); }
            catch { /* no enmascarar el resultado si el log falla post-restore */ }
        }
    }
}
