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
    public class Logica_Proveedor
    {
        private readonly DatosProveedor _datosProveedor = new DatosProveedor();

        // 1. LISTAR TODOS
        public DataTable ListarProveedores()
        {
            return _datosProveedor.ListarProveedores();
        }

        // 2. LISTAR ACTIVOS
        public DataTable ListarProveedoresActivos()
        {
            return _datosProveedor.ListarProveedoresActivos();
        }

        // 3. INSERTAR PROVEEDOR
        public bool InsertarProveedor(Proveedor proveedor)
        {
            ValidarReglasNegocioProveedor(proveedor);
            return _datosProveedor.InsertarProveedor(proveedor);
        }

        // 4. EDITAR PROVEEDOR
        public bool EditarProveedor(Proveedor proveedor)
        {
            if (proveedor.ID_PROVEEDOR <= 0)
                throw new ArgumentException("El ID del proveedor a editar no es válido.");

            ValidarReglasNegocioProveedor(proveedor);
            return _datosProveedor.EditarProveedor(proveedor);
        }

        // 5. DESACTIVAR PROVEEDOR
        public bool DesactivarProveedor(int idProveedor)
        {
            if (idProveedor <= 0)
                throw new ArgumentException("El ID del proveedor a desactivar no es válido.");

            return _datosProveedor.DesactivarProveedor(idProveedor);
        }

        // 6. BUSCAR POR NOMBRE
        public DataTable BuscarProveedorPorNombre(string textoBusqueda)
        {
            string termino = string.IsNullOrWhiteSpace(textoBusqueda) ? "" : textoBusqueda.Trim();
            return _datosProveedor.BuscarProveedorPorNombre(termino);
        }

        // --- MÉTODO PRIVADO PARA VALIDACIONES ---
        private void ValidarReglasNegocioProveedor(Proveedor proveedor)
        {
            if (proveedor == null)
                throw new ArgumentNullException(nameof(proveedor), "La información del proveedor no puede estar vacía.");

            // Sanitizamos los espacios en blanco
            if (!string.IsNullOrWhiteSpace(proveedor.RAZON_SOCIAL))
                proveedor.RAZON_SOCIAL = proveedor.RAZON_SOCIAL.Trim();

            if (!string.IsNullOrWhiteSpace(proveedor.NOMBRE_CONTACTO))
                proveedor.NOMBRE_CONTACTO = proveedor.NOMBRE_CONTACTO.Trim();

            // Regla principal: Debe tener Razón Social o un Nombre de Contacto
            if (string.IsNullOrWhiteSpace(proveedor.RAZON_SOCIAL) && string.IsNullOrWhiteSpace(proveedor.NOMBRE_CONTACTO))
            {
                throw new ArgumentException("Debe especificar al menos la Razón Social o el Nombre de Contacto del proveedor.");
            }

            // Opcional: Validar formato básico si se proporciona un correo
            if (!string.IsNullOrWhiteSpace(proveedor.CORREO))
            {
                proveedor.CORREO = proveedor.CORREO.Trim();
                if (!proveedor.CORREO.Contains("@") || !proveedor.CORREO.Contains("."))
                {
                    throw new ArgumentException("El formato del correo electrónico no parece válido.");
                }
            }
        }
    }
}