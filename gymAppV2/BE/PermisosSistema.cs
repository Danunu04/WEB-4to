using System.Collections.Generic;

namespace BE
{
    /// <summary>
    /// Catálogo de permisos funcionales del sistema. Qué rol tiene cada permiso se guarda
    /// en la tabla RolPermiso (pantalla Permisos); los valores por defecto están en BLLRol.
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

        /// <summary>
        /// Lista de todos los permisos conocidos. Útil para auditoría y tests.
        /// </summary>
        public static IReadOnlyList<string> Todos = new List<string>
        {
            Dashboard, Perfil,
            GestionUsuarios, GestionAlumnos, GestionEntrenadores, Bitacora,
            ActividadesCalendario, GestionActividades, GestionAulas, GestionRutinas,
            Pagos, PreciosCuota,
            VerificacionDV, Backup, Restore, RecalcularDV, EncriptarDatos, GestionPermisos
        }.AsReadOnly();
    }
}
