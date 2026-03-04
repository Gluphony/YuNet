using Datos;
using Entidades;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LogicaNegocio
{
    public class LogicaCarrito
    {
        private readonly DataCarrito _dataCarrito = new DataCarrito();

        // 1. OBTENER O CREAR CARRITO
        public Carrito ObtenerOCrearCarrito(int idUsuario)
        {
            // Regla de Negocio: El ID del usuario debe ser válido
            if (idUsuario <= 0)
            {
                throw new ArgumentException("El ID de usuario no es válido para consultar el carrito.");
            }

            return _dataCarrito.ObtenerOCrearCarrito(idUsuario);
        }

        // 2. ACTUALIZAR TOTAL (Manual)
        public bool ActualizarTotal(int idCarrito, decimal nuevoTotal)
        {
            if (idCarrito <= 0) throw new ArgumentException("ID de carrito inválido.");
            if (nuevoTotal < 0) throw new ArgumentException("El total del carrito no puede ser negativo.");

            return _dataCarrito.ActualizarTotal(idCarrito, nuevoTotal);
        }

        // 3. CAMBIAR ESTADO
        public bool CambiarEstado(int idCarrito, string nuevoEstado)
        {
            if (idCarrito <= 0) throw new ArgumentException("ID de carrito inválido.");

            if (string.IsNullOrWhiteSpace(nuevoEstado))
                throw new ArgumentException("El estado no puede estar vacío.");

            // Regla de Negocio: Validar que solo se usen estados permitidos por tu sistema
            string estadoUpper = nuevoEstado.ToUpper().Trim();
            if (estadoUpper != "PENDIENTE" && estadoUpper != "COMPLETADO" && estadoUpper != "CANCELADO")
            {
                throw new ArgumentException("El estado ingresado no es válido.");
            }

            return _dataCarrito.CambiarEstado(idCarrito, estadoUpper);
        }

        // 4. VACIAR CARRITO
        public bool VaciarCarrito(int idCarrito)
        {
            if (idCarrito <= 0) throw new ArgumentException("ID de carrito inválido.");

            return _dataCarrito.VaciarCarrito(idCarrito);
        }
    }
}
