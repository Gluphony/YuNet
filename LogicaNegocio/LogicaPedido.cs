using CapaDatos;
using Entidades;
using System;
using System.Collections.Generic;
using System.Linq;

namespace LogicaNegocio
{
    public class LogicaPedido
    {
        private readonly DatosPedidos _datosPedidos = new DatosPedidos();

        // ==========================================
        // 1. PROCESAR COMPRA (REEMPLAZA AL ANTIGUO CREAR)
        // ==========================================
        public int ProcesarCompra(int idUsuario, int? idMetodoPago, decimal total, List<DetalleCarrito> detallesCarrito)
        {
            // Validaciones de negocio cruciales:
            if (idUsuario <= 0)
                throw new ArgumentException("El pedido debe estar asociado a un usuario válido.");

            if (total < 0)
                throw new ArgumentException("El total del pedido no puede ser menor a cero.");

            if (idMetodoPago.HasValue && idMetodoPago.Value <= 0)
                throw new ArgumentException("El método de pago seleccionado no es válido.");

            if (detallesCarrito == null || !detallesCarrito.Any())
                throw new ArgumentException("El carrito no tiene productos para procesar la compra.");

            // Si todo está correcto, pasamos la pelota a la Capa de Datos
            return _datosPedidos.ProcesarCompraTransaccional(idUsuario, idMetodoPago, total, detallesCarrito);
        }

        // ==========================================
        // MÉTODOS EXISTENTES (INTACTOS)
        // ==========================================

        // 2. LISTAR TODOS LOS PEDIDOS
        public List<Pedido> Listar()
        {
            return _datosPedidos.Listar();
        }

        // 3. ACTUALIZAR ESTADO DEL PEDIDO
        public bool ActualizarEstado(int idPedido, string nuevoEstado)
        {
            if (idPedido <= 0)
                throw new ArgumentException("El ID del pedido no es válido.");

            if (string.IsNullOrWhiteSpace(nuevoEstado))
                throw new ArgumentException("Debe especificar el nuevo estado del pedido.");

            return _datosPedidos.ActualizarEstado(idPedido, nuevoEstado.Trim());
        }

        // 4. LISTAR PEDIDOS POR USUARIO
        public List<Pedido> ListarPorUsuario(int idUsuario)
        {
            if (idUsuario <= 0)
                throw new ArgumentException("El ID del usuario no es válido para consultar pedidos.");

            return _datosPedidos.ListarPorUsuario(idUsuario);
        }

        // 5. LISTAR PEDIDOS POR RANGO DE FECHAS
        public List<Pedido> ListarPorFecha(DateTime fechaInicio, DateTime fechaFin)
        {
            if (fechaInicio > fechaFin)
            {
                throw new ArgumentException("La fecha de inicio no puede ser posterior a la fecha de fin.");
            }

            return _datosPedidos.ListarPorFecha(fechaInicio, fechaFin);
        }

        // 6. LISTAR PEDIDOS POR ESTADO
        public List<Pedido> ListarPorEstado(string estado)
        {
            if (string.IsNullOrWhiteSpace(estado))
                throw new ArgumentException("Debe especificar un estado para filtrar los pedidos.");

            return _datosPedidos.ListarPorEstado(estado.Trim());
        }

        // 7. OBTENER PEDIDO POR ID (Detalles)
        public Pedido ObtenerPorId(int idPedido)
        {
            if (idPedido <= 0)
                throw new ArgumentException("El ID del pedido no es válido.");

            return _datosPedidos.ObtenerPorId(idPedido);
        }

        // 8. OBTENER DETALLES POR PEDIDO
        public List<DetallePedido> ObtenerDetallesPorPedido(int idPedido)
        {
            if (idPedido <= 0)
                throw new ArgumentException("El ID del pedido no es válido para buscar detalles.");

            return _datosPedidos.ObtenerDetallesPorPedido(idPedido);
        }
    }
}