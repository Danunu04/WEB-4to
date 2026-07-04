using System;
using System.Collections.Generic;
using System.Linq;
using BE;
using BLL;

namespace CrearPreguntasSeguridad
{
    class Program
    {
        static int Main(string[] args)
        {
            var usuarios = args.Length > 0
                ? args.ToList()
                : new List<string> { "Gil", "alu09" };

            var bllPregunta = new BLLPreguntaSeguridad();
            var bllUsuario = new BLLUsuario();
            int exitosos = 0;
            int fallidos = 0;

            foreach (var usuario in usuarios)
            {
                try
                {
                    Console.WriteLine($"Procesando usuario: {usuario}");

                    // Asegurar que el usuario tenga fecha de nacimiento; de lo contrario
                    // no se puede generar la pregunta por defecto. Se actualiza mediante
                    // la BLL para mantener encriptación y DVH/DVV consistentes.
                    Usuario userBD = bllUsuario.ObtenerUsuario(usuario);
                    if (userBD == null)
                    {
                        Console.WriteLine($"  ERROR: usuario {usuario} no existe");
                        fallidos++;
                        continue;
                    }

                    if (!userBD.FechaNacimiento.HasValue)
                    {
                        Console.WriteLine($"  ADVERTENCIA: fecha de nacimiento faltante, se asigna 1990-01-01");
                        userBD.FechaNacimiento = new DateTime(1990, 1, 1);
                        bllUsuario.ModificarUsuario(
                            userBD.USUARIO_Usuario,
                            userBD.USUARIO_Usuario,
                            userBD.Nombre,
                            userBD.Apellido,
                            userBD.Telefono,
                            userBD.Email,
                            userBD.FechaNacimiento,
                            userBD.USUARIO_Rol,
                            userBD.USUARIO_Activo,
                            userBD.USUARIO_DNI);
                    }

                    bllPregunta.CrearPreguntaSeguridadPorDefecto(usuario);
                    Console.WriteLine($"  OK: pregunta de seguridad creada para {usuario}");
                    exitosos++;
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"  ERROR: {ex.Message}");
                    fallidos++;
                }
            }

            Console.WriteLine($"\nResultado: {exitosos} exitosos, {fallidos} fallidos.");
            return fallidos > 0 ? 1 : 0;
        }
    }
}
