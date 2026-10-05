using System;

namespace BE
{
    /// <summary>
    /// Vínculo entre un Alumno y un Usuario que puede gestionarlo/verlo (tabla ALUMNOS_USUARIOS).
    /// Reemplaza a la vieja columna única ALUMNOS.usr: un alumno puede tener varios usuarios
    /// vinculados (ej. madre y padre) y un mismo usuario puede estar vinculado a varios alumnos
    /// (ej. varios hijos, o el propio titular que además se anota como alumno).
    /// </summary>
    public class AlumnoUsuarioVinculo
    {
        public int DniAlumno { get; set; }
        public string Usuario { get; set; }
        public string Parentesco { get; set; }
        public DateTime FechaAsociacion { get; set; }
        public string DVH { get; set; }

        // Datos de visualización del usuario vinculado (se completan vía JOIN con USUARIOS)
        public string NombreUsuario { get; set; }
        public string ApellidoUsuario { get; set; }

        public AlumnoUsuarioVinculo()
        {
        }

        public AlumnoUsuarioVinculo(int dniAlumno, string usuario, string parentesco, DateTime fechaAsociacion, string dvh)
        {
            DniAlumno = dniAlumno;
            Usuario = usuario;
            Parentesco = parentesco;
            FechaAsociacion = fechaAsociacion;
            DVH = dvh;
        }
    }
}
