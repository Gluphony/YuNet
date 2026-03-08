using Microsoft.AspNetCore.Mvc; 
using Entidades;
using LogicaNegocio;
using System;
using System.Collections.Generic;

namespace Presentacion.Controllers
{
    public class DetallePedidoController : Controller
    {
        // Instanciamos la capa de negocio
        private readonly LogicaDetallePedido _logicaDetalle = new LogicaDetallePedido();

        // --- 1. ACCIÓN PARA INSERTAR DETALLE (Llamada desde AJAX / Fetch en la vista) ---
        [HttpPost]
        public IActionResult Insertar([FromBody] DetallePedido oDetalle)
        {
            try
            {

                int idUsuarioSesion = 1; // Simulando el ID para el ejemplo

                // Llamamos a la capa de negocio
                bool resultado = _logicaDetalle.InsertarDetalle(oDetalle, idUsuarioSesion);

                if (resultado)
                {
                    // Retornamos JSON porque normalmente los detalles se agregan sin recargar toda la página
                    return Json(new { exito = true, mensaje = "Producto agregado al pedido." });
                }

                return Json(new { exito = false, mensaje = "No se pudo registrar el detalle en la base de datos." });
            }
            catch (ArgumentException ex) // ¡Aquí atrapamos TUS validaciones de negocio!
            {
                // Si faltó el precio, la cantidad, etc., le devolvemos tu mensaje exacto a la vista
                return Json(new { exito = false, mensaje = ex.Message });
            }
            catch (Exception ex)
            {
                // Atrapamos errores graves (caída de BD, etc.)
                return Json(new { exito = false, mensaje = "Error del sistema: " + ex.Message });
            }
        }

        // --- 2. ACCIÓN PARA MOSTRAR EL TICKET (Para imprimir o visualizar) ---
        [HttpGet]
        public IActionResult Ticket(int idPedido)
        {
            try
            {
                // Pedimos los datos a la capa de negocio
                List<DetallePedido> detalles = _logicaDetalle.ListarTicket(idPedido);

                // Le pasamos la lista a la vista "Ticket.cshtml"
                return View(detalles);
            }
            catch (ArgumentException ex)
            {
                // Si el idPedido es <= 0, mandamos el error a la vista usando ViewBag
                ViewBag.Error = ex.Message;
                return View(new List<DetallePedido>());
            }
            catch (Exception)
            {
                ViewBag.Error = "Ocurrió un error al intentar generar el ticket.";
                return View(new List<DetallePedido>());
            }
        }
    }
}