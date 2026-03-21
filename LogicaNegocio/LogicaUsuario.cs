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

        // LISTAR USUARIOS
        public DataTable ListarUsuarios()
        {
            return _dataUsuario.ListarUsuarios();
        }

        // INSERTAR USUARIO
        public bool InsertarUsuario(int idRolDelUsuarioCreador, string nombre, string correo, string clave, int idRol)
        {
            // 1. VALIDACIÓN DE PRIVILEGIOS
            if (idRol == 1 && idRolDelUsuarioCreador != 1)
                throw new UnauthorizedAccessException("No tienes permisos para crear usuarios Administradores.");

            // 2. VALIDACIONES GENERALES
            if (string.IsNullOrWhiteSpace(nombre)) throw new ArgumentException("El nombre es obligatorio.");
            if (string.IsNullOrWhiteSpace(correo)) throw new ArgumentException("El correo es obligatorio.");
            if (idRol <= 0) throw new ArgumentException("Debe seleccionar un rol válido.");

            string correoLimpio = correo.Trim();
            if (!correoLimpio.Contains("@")) throw new ArgumentException("El formato del correo electrónico no es válido.");

            // 3. POLÍTICAS DE CONTRASEÑA SEGÚN EL ROL
            if (string.IsNullOrWhiteSpace(clave))
                throw new ArgumentException("La contraseña es obligatoria para usuarios nuevos.");

            if (idRol == 1) // Es Administrador
            {
                if (clave.Length < 10 || !clave.Any(char.IsUpper) || !clave.Any(char.IsDigit))
                    throw new ArgumentException("La contraseña de Administrador debe tener 10 caracteres, incluir mayúsculas y números.");
            }
            else // Es Usuario Normal / Otros
            {
                if (clave.Length < 6)
                    throw new ArgumentException("La contraseña debe tener al menos 6 caracteres.");
            }

            // 4. SANITIZACIÓN Y ENCRIPTACIÓN
            string nombreLimpio = nombre.Trim();
            byte[] claveHasheada = Encriptar(clave); // <--- AQUÍ SE USA

            // 5. INSERCIÓN
            return _dataUsuario.InsertarUsuario(nombreLimpio, correoLimpio, claveHasheada, idRol);
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
        private byte[] Encriptar(string textoPlano)
        {
            // Usamos la ruta completa para no tener que agregar más 'usings' arriba
            using (System.Security.Cryptography.SHA256 sha256Hash = System.Security.Cryptography.SHA256.Create())
            {
                return sha256Hash.ComputeHash(Encoding.UTF8.GetBytes(textoPlano));
            }
        }
    }
}