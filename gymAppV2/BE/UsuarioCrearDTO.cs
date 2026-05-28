using System;

namespace BE
{
    /// <summary>
    /// DTO para la creación de un nuevo usuario con todos los datos necesarios según el rol
    /// </summary>
    public class UsuarioCrearDTO
    {
        // Datos básicos del usuario
        public string Usuario { get; set; }
        public string Contrasena { get; set; }
        public int Rol { get; set; }  // 1=Admin, 2=Recepcionista, 3=Entrenador, 4=Cliente

        // Datos específicos para Entrenador (Rol 3)
        public int? EntrenadorDNI { get; set; }
        public string EntrenadorNombre { get; set; }
        public string EntrenadorApellido { get; set; }
        public DateTime? EntrenadorFechaNacimiento { get; set; }
        public string EntrenadorTelefono { get; set; }

        // Datos específicos para Cliente (Rol 4) - asociar alumno existente
        public int? AlumnoDNI { get; set; }

        public UsuarioCrearDTO()
        {
        }

        public void Validar()
        {
            // Validaciones comunes
            if (string.IsNullOrEmpty(Usuario))
            {
                throw new ArgumentException("El nombre de usuario es requerido");
            }

            // La contraseña es opcional (se genera automáticamente si no se proporciona)
            // if (string.IsNullOrEmpty(Contrasena))
            // {
            //     throw new ArgumentException("La contraseña es requerida");
            // }

            if (Rol < 1 || Rol > 4)
            {
                throw new ArgumentException("El rol debe ser 1, 2, 3 o 4");
            }

            // Validaciones específicas por rol
            if (Rol == 3)
            {
                // Validar datos de entrenador
                if (!EntrenadorDNI.HasValue || EntrenadorDNI <= 0)
                {
                    throw new ArgumentException("Para crear un Entrenador, el DNI es requerido");
                }

                if (string.IsNullOrEmpty(EntrenadorNombre))
                {
                    throw new ArgumentException("Para crear un Entrenador, el nombre es requerido");
                }

                if (string.IsNullOrEmpty(EntrenadorApellido))
                {
                    throw new ArgumentException("Para crear un Entrenador, el apellido es requerido");
                }

                if (!EntrenadorFechaNacimiento.HasValue)
                {
                    throw new ArgumentException("Para crear un Entrenador, la fecha de nacimiento es requerida");
                }
            }
            else if (Rol == 4)
            {
                // Validar datos de cliente (alumno a asociar)
                if (!AlumnoDNI.HasValue || AlumnoDNI <= 0)
                {
                    throw new ArgumentException("Para crear un Cliente, el DNI del alumno a asociar es requerido");
                }
            }
        }
    }
}