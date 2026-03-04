using Datos;
using Entidades;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LogicaNegocio
{
    public class LogicaDetalleCarrito
    {
        private readonly DataDetalleCarrito _dataDetalle = new DataDetalleCarrito();

        // 1. LISTAR DETALLES
        public List<DetalleCarrito> Listar(int idCarrito)
        {
            if (idCarrito <= 0)
            {
                throw new ArgumentException("El ID del carrito no es válido para consultar sus detalles.");
            }

            return _dataDetalle.Listar(idCarrito);
        }

        // 2. AGREGAR AL CARRITO
        public bool Agregar(int idCarrito, int idProducto, int cantidad)
        {
            // REGLAS DE NEGOCIO CLAVE PARA E-COMMERCE / PUNTO DE VENTA
            if (idCarrito <= 0)
                throw new ArgumentException("ID de carrito inválido.");

            if (idProducto <= 0)
                throw new ArgumentException("Debe seleccionar un producto válido.");

            // ¡Evita que hackeen tu total mandando números negativos!
            if (cantidad <= 0)
                throw new ArgumentException("La cantidad a agregar debe ser mayor a cero.");

            // (Opcional) Aquí podrías agregar otra capa lógica que verifique 
            // primero en el inventario si hay suficiente stock antes de intentar agregarlo.
            // Ej: if (cantidad > LogicaProducto.ObtenerStock(idProducto)) throw Exception...

            return _dataDetalle.Agregar(idCarrito, idProducto, cantidad);
        }

        // 3. ELIMINAR UN ÍTEM
        public bool Eliminar(int idDetalle)
        {
            if (idDetalle <= 0)
            {
                throw new ArgumentException("ID de detalle inválido para eliminar.");
            }

            return _dataDetalle.Eliminar(idDetalle);
        }
    }
}
