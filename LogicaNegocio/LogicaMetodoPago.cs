using Datos;
using Entidades;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LogicaNegocio
{
    public class LogicaMetodoPago  // Aquí consolidamos toda la lógica de negocio relacionada con los pagos, como listar métodos de pago
                                   // activos para clientes, listar todo para admins, insertar, editar y desactivar métodos de pago.
                                   // Esto mantiene el código organizado y fácil de mantener.
    {
        private readonly DatosPagos _datosPagos = new DatosPagos();

        // 1. LISTAR ACTIVOS (Para Clientes / Punto de Venta)
        public List<MetodoPago> ListarMetodosPagosActivos()
        {
            return _datosPagos.ListarMetodosPagosActivos();
        }

        // 2. LISTAR TODO (Para Administradores)
        public List<MetodoPago> ListarMetodosPago_Admin()
        {
            return _datosPagos.ListarMetodosPago_Admin();
        }

        // 3. INSERTAR
        public bool InsertarMetodoPago(MetodoPago metodoPago)
        {
            if (metodoPago == null)
                throw new ArgumentNullException(nameof(metodoPago), "El método de pago no puede ser nulo.");

            if (string.IsNullOrWhiteSpace(metodoPago.NOMBRE_METODO))
                throw new ArgumentException("El nombre del método de pago es obligatorio.");

            // Limpiamos los espacios en blanco
            metodoPago.NOMBRE_METODO = metodoPago.NOMBRE_METODO.Trim();

            return _datosPagos.InsertarMetodoPago(metodoPago);
        }

        // 4. EDITAR
        public bool EditarMetodoPago(MetodoPago metodoPago)
        {
            if (metodoPago == null)
                throw new ArgumentNullException(nameof(metodoPago), "El método de pago no puede ser nulo.");

            if (metodoPago.ID_METODO_PAGO <= 0)
                throw new ArgumentException("El ID del método de pago a editar no es válido.");

            if (string.IsNullOrWhiteSpace(metodoPago.NOMBRE_METODO))
                throw new ArgumentException("El nombre del método de pago es obligatorio.");

            // Limpiamos los espacios en blanco
            metodoPago.NOMBRE_METODO = metodoPago.NOMBRE_METODO.Trim();

            return _datosPagos.EditarMetodoPago(metodoPago);
        }

        // 5. DESACTIVAR
        public bool DesactivarMetodoPago(int idMetodoPago)
        {
            if (idMetodoPago <= 0)
                throw new ArgumentException("El ID del método de pago a desactivar no es válido.");

            return _datosPagos.DesactivarMetodoPago(idMetodoPago);
        }
    }
}
