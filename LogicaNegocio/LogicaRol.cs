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
    public class LogicaRol
    {
        private readonly DatosRol _datosRol = new DatosRol();

        // 1. LISTAR ROLES
        public DataTable ListarRoles()
        {
            return _datosRol.ListarRoles();
        }

        // 2. INSERTAR ROL
        public bool InsertarRol(Rol rol)
        {
            ValidarReglasNegocioRol(rol);
            return _datosRol.InsertarRol(rol);
        }

        // 3. EDITAR ROL (Con control de concurrencia)
        public bool EditarRol(Rol rol, byte[] versionOriginal)
        {
            if (rol.ID_ROL <= 0)
                throw new ArgumentException("El ID del rol a editar no es válido.");

            // Validamos que el token de concurrencia exista
            if (versionOriginal == null || versionOriginal.Length == 0)
                throw new ArgumentException("Se requiere la versión original del registro para evitar conflictos de concurrencia.");

            ValidarReglasNegocioRol(rol);
            return _datosRol.EditarRol(rol, versionOriginal);
        }

        // 4. EDITAR ROL POR NOMBRE
        public bool EditarRolPorNombre(string nombreActual, string nuevoNombre, string nuevaDescripcion, bool estado)
        {
            if (string.IsNullOrWhiteSpace(nombreActual))
                throw new ArgumentException("Debe especificar el nombre actual del rol a editar.");

            if (string.IsNullOrWhiteSpace(nuevoNombre))
                throw new ArgumentException("El nuevo nombre del rol no puede estar vacío.");

            string nombreLimpioActual = nombreActual.Trim();
            string nombreLimpioNuevo = nuevoNombre.Trim();
            string descLimpia = string.IsNullOrWhiteSpace(nuevaDescripcion) ? null : nuevaDescripcion.Trim();

            return _datosRol.EditarRolPorNombre(nombreLimpioActual, nombreLimpioNuevo, descLimpia, estado);
        }

        // 5. DESACTIVAR ROL
        public bool DesactivarRol(int idRol)
        {
            if (idRol <= 0)
                throw new ArgumentException("El ID del rol a desactivar no es válido.");

            return _datosRol.DesactivarRol(idRol);
        }

        // --- MÉTODO PRIVADO PARA REUTILIZAR VALIDACIONES ---
        private void ValidarReglasNegocioRol(Rol rol)
        {
            if (rol == null)
                throw new ArgumentNullException(nameof(rol), "La información del rol no puede estar vacía.");

            if (string.IsNullOrWhiteSpace(rol.NOMBRE_ROL))
                throw new ArgumentException("El nombre del rol es obligatorio.");

            // Sanitización: Quitamos espacios y, por convención de seguridad, los roles suelen ir en mayúsculas
            rol.NOMBRE_ROL = rol.NOMBRE_ROL.Trim();

            if (!string.IsNullOrWhiteSpace(rol.DESCRIPCION))
                rol.DESCRIPCION = rol.DESCRIPCION.Trim();
        }
    }
}