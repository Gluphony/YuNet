using Datos;
using Entidades;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LogicaNegocio
{
    public class LogicaPermisos
    {
        private readonly DatosPermisos _datosPermisos = new DatosPermisos();

        // 1. LISTAR PERMISOS
        public List<Permisos> ListarPermisos()
        {
            return _datosPermisos.ListarPermisos();
        }

        // 2. CREAR PERMISO
        public bool CrearPermiso(Permisos oPermiso)
        {
            if (oPermiso == null)
                throw new ArgumentNullException(nameof(oPermiso), "La información del permiso no puede estar vacía.");

            if (string.IsNullOrWhiteSpace(oPermiso.CLAVE_PERMISO))
                throw new ArgumentException("La clave del permiso es obligatoria.");

            if (string.IsNullOrWhiteSpace(oPermiso.DESCRIPCION))
                throw new ArgumentException("La descripción del permiso es obligatoria.");

            // Sanitización: Evitamos espacios accidentales, especialmente en la clave
            oPermiso.CLAVE_PERMISO = oPermiso.CLAVE_PERMISO.Trim().ToUpper(); // Es buena práctica que las claves sean mayúsculas
            oPermiso.DESCRIPCION = oPermiso.DESCRIPCION.Trim();

            // Opcional: Validar que no contenga espacios intermedios en la clave
            // if (oPermiso.CLAVE_PERMISO.Contains(" "))
            //     throw new ArgumentException("La clave del permiso no debe contener espacios en blanco.");

            return _datosPermisos.CrearPermiso(oPermiso);
        }

        // 3. EDITAR PERMISO
        public bool EditarPermiso(Permisos oPermiso)
        {
            if (oPermiso == null)
                throw new ArgumentNullException(nameof(oPermiso), "La información del permiso no puede estar vacía.");

            if (oPermiso.ID_PERMISO <= 0)
                throw new ArgumentException("El ID del permiso a editar no es válido.");

            if (string.IsNullOrWhiteSpace(oPermiso.CLAVE_PERMISO))
                throw new ArgumentException("La clave del permiso es obligatoria.");

            if (string.IsNullOrWhiteSpace(oPermiso.DESCRIPCION))
                throw new ArgumentException("La descripción del permiso es obligatoria.");

            // Sanitización
            oPermiso.CLAVE_PERMISO = oPermiso.CLAVE_PERMISO.Trim().ToUpper();
            oPermiso.DESCRIPCION = oPermiso.DESCRIPCION.Trim();

            return _datosPermisos.EditarPermiso(oPermiso);
        }

        // 4. DESACTIVAR PERMISO
        public bool DesactivarPermiso(int idPermiso)
        {
            if (idPermiso <= 0)
                throw new ArgumentException("El ID del permiso a desactivar no es válido.");

            return _datosPermisos.DesactivarPermiso(idPermiso);
        }
    }
}

