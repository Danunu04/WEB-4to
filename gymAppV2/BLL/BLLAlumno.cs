using System;
using System.Collections.Generic;
using BE;
using MPP;

namespace BLL
{
    public class BLLAlumno
    {
        private MPPAlumno mppAlumno;

        public BLLAlumno()
        {
            mppAlumno = new MPPAlumno();
        }

        public void CrearAlumno(Alumno alumno)
        {
            try
            {
                if (mppAlumno.AlumnoExiste(alumno.DNI))
                {
                    throw new Exception("Ya existe un alumno con ese DNI");
                }

                mppAlumno.CrearAlumno(alumno);
            }
            catch (Exception ex)
            {
                throw new Exception("Error al crear alumno: " + ex.Message, ex);
            }
        }

        public Alumno ObtenerAlumno(int dni)
        {
            try
            {
                return mppAlumno.ObtenerAlumno(dni);
            }
            catch (Exception ex)
            {
                throw new Exception("Error al obtener alumno: " + ex.Message, ex);
            }
        }

        public void ActualizarAlumno(Alumno alumno)
        {
            try
            {
                mppAlumno.ActualizarAlumno(alumno);
            }
            catch (Exception ex)
            {
                throw new Exception("Error al actualizar alumno: " + ex.Message, ex);
            }
        }

        public bool AlumnoExiste(int dni)
        {
            try
            {
                return mppAlumno.AlumnoExiste(dni);
            }
            catch (Exception ex)
            {
                throw new Exception("Error al verificar si existe el alumno: " + ex.Message, ex);
            }
        }

        public List<Alumno> ListarAlumnos()
        {
            try
            {
                return mppAlumno.ListarAlumnos();
            }
            catch (Exception ex)
            {
                throw new Exception("Error al listar alumnos: " + ex.Message, ex);
            }
        }

        public void EliminarAlumno(int dni)
        {
            try
            {
                if (!AlumnoExiste(dni))
                {
                    throw new Exception($"No existe un alumno con DNI {dni}");
                }

                mppAlumno.EliminarAlumno(dni);
            }
            catch (Exception ex)
            {
                throw new Exception("Error al eliminar alumno: " + ex.Message, ex);
            }
        }

        public void AsociarUsuario(int dni, string usuario)
        {
            try
            {
                // Validar que el alumno exista
                if (!AlumnoExiste(dni))
                {
                    throw new Exception($"No existe un alumno con DNI {dni}");
                }

                // Validar que el usuario exista
                BLLUsuario bllUsuario = new BLLUsuario();
                BE.Usuario usuarioBD = bllUsuario.ObtenerUsuario(usuario);

                if (usuarioBD == null)
                {
                    throw new Exception($"No existe el usuario '{usuario}'");
                }

                // Validar que el usuario sea Cliente (Rol 4)
                if (usuarioBD.USUARIO_Rol != 4)
                {
                    throw new Exception($"El usuario '{usuario}' no es de tipo Cliente (Rol {usuarioBD.USUARIO_Rol})");
                }

                // Validar que el alumno no tenga ya un usuario
                Alumno alumno = ObtenerAlumno(dni);
                if (!string.IsNullOrEmpty(alumno.Usuario))
                {
                    throw new Exception($"El alumno con DNI {dni} ya tiene un usuario asociado: {alumno.Usuario}");
                }

                // Validar que el usuario no tenga ya un alumno asociado
                List<BE.UsuarioGestion> usuarios = bllUsuario.ListarUsuarios();
                var usuarioConAlumno = usuarios.FirstOrDefault(u => u.USUARIO_Usuario == usuario && u.DNI.HasValue);

                if (usuarioConAlumno != null)
                {
                    throw new Exception($"El usuario '{usuario}' ya tiene un alumno asociado (DNI: {usuarioConAlumno.DNI})");
                }

                mppAlumno.AsociarUsuario(dni, usuario);
            }
            catch (Exception ex)
            {
                throw new Exception("Error al asociar usuario: " + ex.Message, ex);
            }
        }

        public void DesasociarUsuario(int dni)
        {
            try
            {
                if (!AlumnoExiste(dni))
                {
                    throw new Exception($"No existe un alumno con DNI {dni}");
                }

                Alumno alumno = ObtenerAlumno(dni);
                if (string.IsNullOrEmpty(alumno.Usuario))
                {
                    throw new Exception($"El alumno con DNI {dni} no tiene un usuario asociado");
                }

                mppAlumno.AsociarUsuario(dni, null);
            }
            catch (Exception ex)
            {
                throw new Exception("Error al desasociar usuario: " + ex.Message, ex);
            }
        }

        public List<Alumno> ListarAlumnosSinUsuario()
        {
            try
            {
                return mppAlumno.ListarAlumnosSinUsuario();
            }
            catch (Exception ex)
            {
                throw new Exception("Error al listar alumnos sin usuario: " + ex.Message, ex);
            }
        }
    }
}