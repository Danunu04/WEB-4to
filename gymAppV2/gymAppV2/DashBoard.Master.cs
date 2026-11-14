using System;
using System.Web;
using BE;
using Servicios.Singleton;
using BLL;
using System.Web.UI;
using SERVICIOS.Observer;
using System.Collections.Generic;
using System.Linq;

namespace gymAppV2
{
    public partial class DashBoardMaster : System.Web.UI.MasterPage, IIdiomaObserver
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            GestorIdioma.Suscribir(this);
            AplicarIdioma(GestorIdioma.IdiomaActual);

            if (!IsPostBack)
            {
                if (!Singleton.Instancia.IsLogged())
                {
                    RedirigirSeguro("~/LogIn/LogIn.aspx");
                    return;
                }
            }

            if (SistemaEnPausa() && !UsuarioActualPuedeRepararIntegridad())
            {
                ConfigurarMenuPausa();
                string paginaActual = Request.AppRelativeCurrentExecutionFilePath;
                // FriendlyUrls puede devolver la ruta sin .aspx; comparar sin extensión.
                string paginaNorm = NormalizarPagina(paginaActual);
                if (!string.IsNullOrEmpty(paginaNorm)
                    && !paginaNorm.Equals("VerificacioDV/VerificacioDV", StringComparison.OrdinalIgnoreCase))
                {
                    RedirigirSeguro("~/VerificacioDV/VerificacioDV.aspx");
                    return;
                }
            }
            else
            {
                ConfigurarMenuSegunRol();
                CargarAvisoPrecio();
            }
        }

        /// <summary>
        /// Indica si el sistema está pausado por un error de integridad de datos.
        /// </summary>
        private bool SistemaEnPausa()
        {
            try
            {
                var bllDV = new BLLDigitoVerificador();
                return bllDV.ExisteErrorIntegridad();
            }
            catch
            {
                // Si no se puede verificar, se asume pausa para no continuar con datos dudosos.
                return true;
            }
        }

        /// <summary>
        /// Indica si el usuario logueado puede entrar con el sistema pausado (permiso VerificacionDV).
        /// </summary>
        private bool UsuarioActualPuedeRepararIntegridad()
        {
            try
            {
                var bllRol = new BLLRol();
                return bllRol.UsuarioActualPuedeRepararIntegridad();
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Durante una pausa de integridad para usuarios no administradores,
        /// oculta todo el menú de navegación excepto la opción de cerrar sesión.
        /// </summary>
        private void ConfigurarMenuPausa()
        {
            ulMenuNavegacion.Visible = false;
            ulCambiarContra.Visible = false;
            divDivider.Visible = false;
        }

        /// <summary>
        /// Aplica las traducciones del menú al idioma dado.
        /// Implementación del patrón Observer: se llama cuando cambia el idioma.
        /// </summary>
        public void OnIdiomaChanged(IdiomaApp idioma)
        {
            AplicarIdioma(idioma);
        }

        private void AplicarIdioma(IdiomaApp idioma)
        {
            try
            {
                var dict = new BLLTraduccion().ObtenerDiccionario(idioma);
                litMenuDashboard.Text    = Traducir(dict, "menu_dashboard");
                litMenuUsuarios.Text     = Traducir(dict, "menu_usuarios");
                litMenuAlumnos.Text      = Traducir(dict, "menu_alumnos");
                litMenuEntrenadores.Text = Traducir(dict, "menu_entrenadores");
                litMenuActividades.Text  = Traducir(dict, "menu_actividades");
                litMenuAulas.Text        = Traducir(dict, "menu_aulas");
                litMenuRutinas.Text      = Traducir(dict, "menu_rutinas");
                litMenuCalculoRM.Text    = Traducir(dict, "menu_calculo_rm");
                litMenuPermisos.Text     = Traducir(dict, "menu_permisos");
                litMenuBitacora.Text     = Traducir(dict, "menu_bitacora");
                litMenuRespaldo.Text     = Traducir(dict, "menu_respaldo");
                litMenuPagos.Text        = Traducir(dict, "menu_pagos");
                litMenuPrecios.Text      = TraducirConRespaldo(dict, "menu_precios", "Precios de cuota");
                litMenuPerfil.Text       = Traducir(dict, "menu_perfil");
                litMenuIdioma.Text       = Traducir(dict, "menu_idioma");
                litMenuCambiarContra.Text = Traducir(dict, "menu_cambiar_contra");
                litMenuCerrarSesion.Text = Traducir(dict, "menu_cerrar_sesion");
            }
            catch
            {
                // Si falla la carga de traducciones, los textos por defecto del .master quedan intactos.
            }
        }

        private static string Traducir(Dictionary<string, string> dict, string tag)
        {
            return dict.TryGetValue(tag, out var texto) ? texto : tag;
        }

        // Usado desde el .master con <%= TraducirTag("tag") %> para incrustar textos en JS.
        protected string TraducirTag(string tag)
        {
            try
            {
                var dict = new BLLTraduccion().ObtenerDiccionario(GestorIdioma.IdiomaActual);
                return Traducir(dict, tag);
            }
            catch { return tag; }
        }

        /// <summary>
        /// Muestra u oculta las opciones del menú lateral según el perfil del usuario logueado.
        /// </summary>
        // ==================== AVISO DE CAMBIO DE PRECIO ====================

        /// <summary>
        /// Muestra el banner con los cambios de precio que el usuario todavía no vio.
        /// Si las tablas de avisos no existen todavía, simplemente no se muestra.
        /// </summary>
        private void CargarAvisoPrecio()
        {
            try
            {
                List<AvisoPrecio> avisos = new BLLAvisoPrecio().ListarPendientes(Singleton.Instancia.Usuario);
                pnlAvisoPrecio.Visible = avisos.Count > 0;
                if (avisos.Count == 0)
                    return;

                var dict = new BLLTraduccion().ObtenerDiccionario(GestorIdioma.IdiomaActual);
                string titulo = TraducirConRespaldo(dict, "aviso_precio_titulo", "Cambió el precio de la cuota");
                string plantilla = TraducirConRespaldo(dict, "aviso_precio_detalle", "{0}: de {1} a {2} (desde el {3})");

                IEnumerable<string> lineas = avisos.Select(a => HttpUtility.HtmlEncode(FormatearSeguro(plantilla,
                    "{0}: de {1} a {2} (desde el {3})",
                    a.Descripcion, a.PrecioAnterior.ToString("C"), a.PrecioNuevo.ToString("C"), a.Fecha.ToString("dd/MM/yyyy"))));

                litAvisoPrecio.Text = "<strong>" + HttpUtility.HtmlEncode(titulo) + "</strong><br />" + string.Join("<br />", lineas);
                litAvisoVisto.Text = HttpUtility.HtmlEncode(TraducirConRespaldo(dict, "aviso_precio_entendido", "Entendido"));
            }
            catch
            {
                pnlAvisoPrecio.Visible = false;
            }
        }

        protected void btnAvisoVisto_Click(object sender, EventArgs e)
        {
            try
            {
                var bllAviso = new BLLAvisoPrecio();
                Usuario usuario = Singleton.Instancia.Usuario;
                bllAviso.MarcarVistos(usuario, bllAviso.ListarPendientes(usuario).Select(a => a.CodAviso).ToList());
            }
            catch
            {
                // Si no se pudo guardar, el aviso vuelve a aparecer en la próxima página
            }

            pnlAvisoPrecio.Visible = false;
        }

        private static string TraducirConRespaldo(Dictionary<string, string> dict, string tag, string respaldo)
        {
            return dict != null && dict.TryGetValue(tag, out var texto) && !string.IsNullOrEmpty(texto) ? texto : respaldo;
        }

        private static string FormatearSeguro(string plantilla, string respaldo, params object[] args)
        {
            try { return string.Format(plantilla, args); }
            catch (FormatException) { return string.Format(respaldo, args); }
        }

        private void ConfigurarMenuSegunRol()
        {
            var bllRol = new BLLRol();

            liDashboard.Visible = bllRol.UsuarioActualTieneAcceso("Dashboard");
            liUsuarios.Visible = bllRol.UsuarioActualTieneAcceso("GestionUsuarios");
            liAlumnos.Visible = bllRol.UsuarioActualTieneAcceso("GestionAlumnos");
            liEntrenadores.Visible = bllRol.UsuarioActualTieneAcceso("GestionEntrenadores");
            liActividades.Visible = bllRol.UsuarioActualTieneAcceso("ActividadesCalendario");
            liAulas.Visible = bllRol.UsuarioActualTieneAcceso(PermisosSistema.GestionAulas);
            liRutinas.Visible = bllRol.UsuarioActualTieneAcceso("GestionRutinas");
            liCalculoRM.Visible = bllRol.UsuarioActualTieneAcceso(PermisosSistema.CalculoRM);
            liPermisos.Visible = bllRol.UsuarioActualTieneAcceso(PermisosSistema.GestionPermisos);
            liBitacora.Visible = bllRol.UsuarioActualTieneAcceso("Bitacora");
            liRespaldo.Visible = bllRol.UsuarioActualTieneAcceso(PermisosSistema.Backup)
                             || bllRol.UsuarioActualTieneAcceso(PermisosSistema.Restore);
            liPagos.Visible = bllRol.UsuarioActualTieneAcceso(PermisosSistema.Pagos);
            liPrecios.Visible = bllRol.UsuarioActualTieneAcceso(PermisosSistema.PreciosCuota);
            liPerfil.Visible = bllRol.UsuarioActualTieneAcceso(PermisosSistema.Perfil);

        }

        protected void LnkLogout_Click(object sender, EventArgs e)
        {
            var usuario = Singleton.Instancia.Usuario;
            string usuarioNombre = usuario?.USUARIO_Usuario ?? "desconocido";

            // Persistir el último idioma usado antes de destruir la sesión.
            try
            {
                if (!string.IsNullOrEmpty(usuarioNombre) && usuarioNombre != "desconocido")
                    new BLLUsuario().GuardarIdioma(usuarioNombre, GestorIdioma.IdiomaActual.ToString());
            }
            catch
            {
                // No impedir el logout si falla la persistencia del idioma.
            }

            // Invalidar la cookie de autenticación de forms antes de destruir la sesión.
            // El orden es importante: primero la cookie, luego la sesión.
            System.Web.Security.FormsAuthentication.SignOut();

            // Registrar evento de logout antes de cerrar sesión.
            try
            {
                var bllEvento = new BLLEvento();
                bllEvento.RegistrarLogout(usuarioNombre);
            }
            catch
            {
                // No impedir el logout si falla el log.
            }

            try
            {
                Singleton.Instancia.LogOut();
            }
            catch
            {
                // Si el logout falla, al menos la cookie de forms ya fue invalidada.
            }

            RedirigirSeguro("~/LogIn/LogIn.aspx");
        }

        private static string NormalizarPagina(string paginaActual)
        {
            if (string.IsNullOrEmpty(paginaActual)) return paginaActual;
            string result = paginaActual.Replace("~/", "");
            if (result.EndsWith(".aspx", StringComparison.OrdinalIgnoreCase))
                result = result.Substring(0, result.Length - 5);
            return result;
        }

        /// <summary>
        /// Redirige de forma segura terminando la request correctamente.
        /// </summary>
        private void RedirigirSeguro(string url)
        {
            try
            {
                Response.Redirect(ResolveUrl(url), false);
                Context.ApplicationInstance.CompleteRequest();
            }
            catch (HttpException)
            {
                // Contexto no disponible.
            }
            catch (Exception)
            {
                // Ignorar errores menores de redirección.
            }
        }
    }
}
