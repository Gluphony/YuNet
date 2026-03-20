using Datos;
using Entidades;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LogicaNegocio
{
    public class LogicaDetallePedido
    {
        private readonly DataDetallePedido _dataDetalle = new DataDetallePedido();

        // 1. INSERTAR DETALLE
        public bool InsertarDetalle(DetallePedido oDetalle, int idUsuarioSesion)
        {
            // --- VALIDACIONES DE NEGOCIO ---

            if (oDetalle == null)
            {
                throw new ArgumentNullException(nameof(oDetalle), "El detalle del pedido no puede ser nulo.");
            }

            if (oDetalle.ID_PEDIDO <= 0)
            {
                throw new ArgumentException("El ID del pedido general no es válido.");
            }

            if (oDetalle.ID_PRODUCTO <= 0)
            {
                throw new ArgumentException("Debe seleccionar un producto válido.");
            }

            if (oDetalle.CANTIDAD <= 0)
            {
                throw new ArgumentException("La cantidad a vender debe ser mayor a cero.");
            }

            if (oDetalle.PRECIO_UNITARIO < 0)
            {
                throw new ArgumentException("El precio unitario no puede ser negativo.");
            }

            if (idUsuarioSesion <= 0)
            {
                throw new ArgumentException("ID de usuario de sesión no válido. Es requerido para registrar el Kárdex.");
            }

            // Opcional: Si es un celular (requiere IMEI), podrías validarlo si tienes un flag en la entidad Producto.
            // Ej: if (producto.RequiereImei && string.IsNullOrWhiteSpace(oDetalle.IMEI_VENDIDO)) throw ...

            // Si pasa todas las validaciones, lo mandamos a la capa de Datos
            return _dataDetalle.InsertarDetalle(oDetalle, idUsuarioSesion);
        }

        // 2. LISTAR TICKET (Para imprimir)
        public List<DetallePedido> ListarTicket(int idPedido)
        {
            if (idPedido <= 0)
            {
                throw new ArgumentException("El número de pedido no es válido para generar el ticket.");
            }

            return _dataDetalle.ListarTicket(idPedido);
        }
    }
}
