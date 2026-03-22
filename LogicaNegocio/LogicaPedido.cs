using CapaDatos;
using Entidades;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LogicaNegocio
{
    public class LogicaPedido
    {
        private readonly DatosPedidos _datosPedidos = new DatosPedidos();

        // 1. CREAR PEDIDO
        public int Crear(Pedido pedido)
        {
            if (pedido == null)
                throw new ArgumentNullException(nameof(pedido), "La información del pedido no puede estar vacía.");

            if (pedido.ID_USUARIO <= 0)
                throw new ArgumentException("El pedido debe estar asociado a un usuario válido.");

            // Validamos que el total no sea negativo (permitimos 0 en caso de promociones o canjes al 100%)
            if (pedido.TOTAL < 0)
                throw new ArgumentException("El total del pedido no puede ser menor a cero.");

            // Si el método de pago no viene nulo, validamos que su ID sea válido
            if (pedido.ID_METODO_PAGO.HasValue && pedido.ID_METODO_PAGO.Value <= 0)
                throw new ArgumentException("El método de pago seleccionado no es válido.");

            return _datosPedidos.Crear(pedido);
        }

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

            // Opcional: Validar contra estados permitidos
            // string[] estadosValidos = { "Pendiente", "Procesando", "Completado", "Cancelado" };
            // if (!Array.Exists(estadosValidos, e => e.Equals(nuevoEstado.Trim(), StringComparison.OrdinalIgnoreCase)))
            //     throw new ArgumentException("El estado especificado no está permitido.");

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
            // Regla vital para consultas de fechas
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
            // Validamos que el ID tenga lógica
            if (idPedido <= 0)
                throw new ArgumentException("El ID del pedido no es válido.");

            // Llamamos a la capa de datos
            return _datosPedidos.ObtenerPorId(idPedido);
        }
        // Dentro de tu clase LogicaPedido:
        public List<dynamic> ObtenerDetallesPorPedido(int idPedido)
        {
            // Llama al método que creamos en DatosPedidos
            return _datosPedidos.ObtenerDetallesPorPedido(idPedido);
        }
    }
}
 