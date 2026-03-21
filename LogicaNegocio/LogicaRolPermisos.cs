using Datos;
using Entidades;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LogicaNegocio
{
    public class LogicaRolPermisos
    {
        private readonly DatosRolesPermisos _datosRolesPermisos = new DatosRolesPermisos();

        // 1. ASIGNAR PERMISO A UN ROL
        public bool AsignarPermiso(int idRol, int idPermiso)
        {
            if (idRol <= 0)
                throw new ArgumentException("El ID del Rol no es válido para asignar un permiso.");

            if (idPermiso <= 0)
                throw new ArgumentException("El ID del Permiso no es válido.");

            return _datosRolesPermisos.AsignarPermiso(idRol, idPermiso);
        }

        // 2. REVOCAR PERMISO DE UN ROL
        public bool RevocarPermiso(int idRol, int idPermiso)
        {
            if (idRol <= 0)
                throw new ArgumentException("El ID del Rol no es válido para revocar un permiso.");

            if (idPermiso <= 0)
                throw new ArgumentException("El ID del Permiso a revocar no es válido.");

            return _datosRolesPermisos.RevocarPermiso(idRol, idPermiso);
        }

        // 3. LISTAR PERMISOS QUE TIENE UN ROL ESPECÍFICO
        public List<Permisos> ListarPermisosPorRol(int idRol)
        {
            if (idRol <= 0)
                throw new ArgumentException("El ID del Rol especificado no es válido.");

            return _datosRolesPermisos.ListarPermisosPorRol(idRol);
        }

        // --- MÉTODO AUXILIAR RECOMENDADO ---
        // 4. VERIFICAR SI UN ROL TIENE UN PERMISO ESPECÍFICO (Por Clave)
        // Esto es muy útil para la interfaz de usuario.
        public bool RolTienePermiso(int idRol, string clavePermiso)
        {
            if (idRol <= 0 || string.IsNullOrWhiteSpace(clavePermiso))
                return false;

            clavePermiso = clavePermiso.Trim().ToUpper();

            // Obtenemos todos los permisos del rol
            List<Permisos> permisosDelRol = ListarPermisosPorRol(idRol);

            // Buscamos si existe alguno con esa clave
            foreach (var permiso in permisosDelRol)
            {
                if (permiso.CLAVE_PERMISO?.ToUpper() == clavePermiso)
                {
                    return true;
                }
            }

            return false;
        }
        public List<Permisos> ListarTodos()
        {
            return _datosRolesPermisos.ListarTodosLosPermisos();

        }
        // 6. CREAR NUEVO PERMISO
        public bool InsertarPermiso(Permisos permiso)
        {
            if (permiso == null)
                throw new ArgumentNullException("La información del permiso no puede estar vacía.");

            if (string.IsNullOrWhiteSpace(permiso.CLAVE_PERMISO))
                throw new ArgumentException("La clave del permiso es obligatoria.");

            if (string.IsNullOrWhiteSpace(permiso.DESCRIPCION))
                throw new ArgumentException("La descripción del permiso es obligatoria.");

            // OJO: Para que esto no marque error, también debes crear este mismo 
            // método "InsertarPermiso" en tu archivo DatosRolesPermisos.cs
            return _datosRolesPermisos.InsertarPermiso(permiso);
        }
    }
}
