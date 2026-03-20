using Datos;
using Entidades;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LogicaNegocio
{
    public class LogicaUsuario
    {
        private readonly DataUsuario _dataUsuario = new DataUsuario();

        // Añade estos métodos dentro de tu clase LogicaSeguridad

        // LISTAR USUARIOS
        public DataTable ListarUsuarios()
        {
            return _dataUsuario.ListarUsuarios();
        }

        // INSERTAR USUARIO
        public bool InsertarUsuario(string nombre, string correo, string clave, int idRol)
        {
            if (string.IsNullOrWhiteSpace(nombre))
                throw new ArgumentException("El nombre del usuario es obligatorio.");

            if (string.IsNullOrWhiteSpace(correo))
                throw new ArgumentException("El correo electrónico es obligatorio.");

            if (string.IsNullOrWhiteSpace(clave))
                throw new ArgumentException("La contraseña es obligatoria para usuarios nuevos.");

            if (idRol <= 0)
                throw new ArgumentException("Debe seleccionar un rol válido para el usuario.");

            // Sanitización
            string nombreLimpio = nombre.Trim();
            string correoLimpio = correo.Trim();

            // Puedes agregar validación de formato de correo aquí si lo deseas
            if (!correoLimpio.Contains("@"))
                throw new ArgumentException("El formato del correo electrónico no es válido.");

            return _dataUsuario.InsertarUsuario(nombreLimpio, correoLimpio, clave, idRol);
        }

        // EDITAR USUARIO
        public bool EditarUsuario(int idUsuario, string nombre, string correo, int idRol, bool estado)
        {
            if (idUsuario <= 0)
                throw new ArgumentException("ID de usuario no válido.");

            if (string.IsNullOrWhiteSpace(nombre))
                throw new ArgumentException("El nombre del usuario es obligatorio.");

            if (string.IsNullOrWhiteSpace(correo))
                throw new ArgumentException("El correo electrónico es obligatorio.");

            if (idRol <= 0)
                throw new ArgumentException("Debe seleccionar un rol válido.");

            // Sanitización
            string nombreLimpio = nombre.Trim();
            string correoLimpio = correo.Trim();

            if (!correoLimpio.Contains("@"))
                throw new ArgumentException("El formato del correo electrónico no es válido.");

            return _dataUsuario.EditarUsuario(idUsuario, nombreLimpio, correoLimpio, idRol, estado);
        }

        // DESACTIVAR USUARIO
        public bool DesactivarUsuario(int idUsuario)
        {
            if (idUsuario <= 0)
                throw new ArgumentException("ID de usuario no válido.");

            return _dataUsuario.DesactivarUsuario(idUsuario);
        }
    }
}