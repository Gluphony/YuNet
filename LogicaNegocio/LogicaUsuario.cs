using Datos;
using Entidades;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LogicaNegocio
{
    public class LogicaUsuario
    {
        private readonly DataUsuario _dataUsuario = new DataUsuario();

        // 1. LISTADO GENERAL
        //public List<Usuario> Listar()
        //{
        //    return _dataUsuario.Listar();
        //}

        //// 2. INSERTAR USUARIO
        //public bool Insertar(Usuario obj)
        //{
        //    if (string.IsNullOrWhiteSpace(obj.NOMBRE))
        //        throw new ArgumentException("El nombre del usuario es obligatorio.");

        //    if (string.IsNullOrWhiteSpace(obj.CORREO))
        //        throw new ArgumentException("El correo electrónico es obligatorio.");

        //    // CORRECCIÓN: Validación para byte[] en lugar de string
        //    if (obj.CONTRASEÑA == null || obj.CONTRASEÑA.Length == 0)
        //        throw new ArgumentException("La contraseña no puede estar vacía.");

        //    if (obj.ID_ROL <= 0)
        //        throw new ArgumentException("Debe asignar un rol válido al nuevo usuario.");

        //    obj.NOMBRE = obj.NOMBRE.Trim();
        //    obj.CORREO = obj.CORREO.Trim();

        //    return _dataUsuario.Insertar(obj);
        //}

        //// 3. EDITAR USUARIO
        //public bool Editar(Usuario obj)
        //{
        //    if (obj.ID_USUARIO <= 0)
        //        throw new ArgumentException("ID de usuario no válido.");

        //    if (string.IsNullOrWhiteSpace(obj.NOMBRE))
        //        throw new ArgumentException("El nombre del usuario no puede quedar vacío.");

        //    if (string.IsNullOrWhiteSpace(obj.CORREO))
        //        throw new ArgumentException("El correo electrónico no puede quedar vacío.");

        //    if (obj.ID_ROL <= 0)
        //        throw new ArgumentException("Debe asignar un rol válido al usuario.");

        //    obj.NOMBRE = obj.NOMBRE.Trim();
        //    obj.CORREO = obj.CORREO.Trim();

        //    return _dataUsuario.Editar(obj);
        //}

        //// 4. VALIDAR LOGIN (Inicio de Sesión)
        //public Usuario Validar(string correo, string clave)
        //{
        //    // Evitamos que vayan a la base de datos si ni siquiera escribieron nada
        //    if (string.IsNullOrWhiteSpace(correo) || string.IsNullOrWhiteSpace(clave))
        //    {
        //        throw new ArgumentException("El correo y la contraseña son obligatorios para iniciar sesión.");
        //    }

        //    return _dataUsuario.Validar(correo.Trim(), clave);
        //}

        //// 5. CAMBIAR CLAVE
        //public bool CambiarClave(int idUsuario, string nuevaClave)
        //{
        //    if (idUsuario <= 0)
        //        throw new ArgumentException("Usuario no válido para el cambio de contraseña.");

        //    // Podemos forzar una regla de negocio para contraseñas seguras
        //    if (string.IsNullOrWhiteSpace(nuevaClave) || nuevaClave.Length < 6)
        //    {
        //        throw new ArgumentException("La nueva contraseña debe tener al menos 6 caracteres.");
        //    }

        //    return _dataUsuario.CambiarClave(idUsuario, nuevaClave);
        //}
    }
}