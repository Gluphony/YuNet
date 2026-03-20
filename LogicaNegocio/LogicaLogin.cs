using Datos;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LogicaNegocio
{
    public class LogicaLogin
    {
        private readonly DataLogin _datosSeguridad = new DataLogin();

        // 1. VALIDAR LOGIN
        public DataTable ValidarLogin(string correo, string clave)
        {
            // Validaciones para no hacer viajes inútiles a la Base de Datos
            if (string.IsNullOrWhiteSpace(correo))
                throw new ArgumentException("El correo electrónico es obligatorio para iniciar sesión.");

            if (string.IsNullOrWhiteSpace(clave))
                throw new ArgumentException("La contraseña es obligatoria.");

            // Limpiamos los espacios en blanco del correo (la clave NO se recorta por si el usuario puso un espacio intencional al final)
            string correoLimpio = correo.Trim();

            return _datosSeguridad.ValidarLogin(correoLimpio, clave);
        }

        // 2. CAMBIAR CONTRASEÑA
        public bool CambiarClave(int idUsuario, string nuevaClave)
        {
            if (idUsuario <= 0)
                throw new ArgumentException("El ID del usuario no es válido.");

            if (string.IsNullOrWhiteSpace(nuevaClave))
                throw new ArgumentException("La nueva contraseña no puede estar vacía.");

            // REGLA DE NEGOCIO: Puedes forzar una longitud mínima de seguridad aquí
            if (nuevaClave.Length < 6)
                throw new ArgumentException("La contraseña debe tener al menos 6 caracteres por seguridad.");

            return _datosSeguridad.CambiarClave(idUsuario, nuevaClave);
        }
    }
}
