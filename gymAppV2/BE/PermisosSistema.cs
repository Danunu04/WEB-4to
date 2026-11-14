using System.Collections.Generic;

namespace BE
{
    /// <summary>
    /// Catálogo de permisos del sistema: uno por pantalla o función. Son las hojas del
    /// Composite de permisos (tabla Permiso). Cómo se agrupan en familias y qué perfil tiene
    /// cada usuario se configura en la pantalla Permisos.
    /// Para agregar un permiso nuevo: constante acá, en Todos, y una fila en la tabla Permiso.
    /// </summary>
    public static class PermisosSistema
    {
        // --- Módulos generales ---
        public const string Dashboard = "Dashboard";
        public const string Perfil = "Perfil";

        // --- Gestión de usuarios y seguridad ---
        public const string GestionUsuarios = "GestionUsuarios";
        public const string GestionAlumnos = "GestionAlumnos";
        public const string GestionEntrenadores = "GestionEntrenadores";
        public const string Bitacora = "Bitacora";

        // --- Actividades y rutinas ---
        public const string ActividadesCalendario = "ActividadesCalendario";
        public const string GestionActividades = "GestionActividades";
        public const string GestionAulas = "GestionAulas";
        public const string GestionRutinas = "GestionRutinas";

        // --- Pagos y precios ---
        public const string Pagos = "Pagos";
        public const string PreciosCuota = "PreciosCuota";

        // --- Herramientas de administración del sistema ---
        public const string VerificacionDV = "VerificacionDV";
        public const string Backup = "Backup";
        public const string Restore = "Restore";
        public const string RecalcularDV = "RecalcularDV";
        public const string EncriptarDatos = "EncriptarDatos";
        public const string GestionPermisos = "GestionPermisos";
        public const string GestionIdiomas = "GestionIdiomas";

        /// <summary>
        /// Lista de todos los permisos conocidos. Útil para auditoría y tests.
        /// </summary>
        public static IReadOnlyList<string> Todos = new List<string>
        {
            Dashboard, Perfil,
            GestionUsuarios, GestionAlumnos, GestionEntrenadores, Bitacora,
            ActividadesCalendario, GestionActividades, GestionAulas, GestionRutinas,
            Pagos, PreciosCuota,
            VerificacionDV, Backup, Restore, RecalcularDV, EncriptarDatos, GestionPermisos, GestionIdiomas
        }.AsReadOnly();
    }
}
