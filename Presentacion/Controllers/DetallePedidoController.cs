using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Entidades;
using LogicaNegocio;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims; // Agregado para leer el ID real del usuario

namespace Presentacion.Controllers
{
    // Escudo Base: Tienes que haber iniciado sesión para entrar a este controlador
    [Authorize]
    public class DetallePedidoController : Controller
    {
        // Instanciamos la capa de negocio
        private readonly LogicaDetallePedido _logicaDetalle = new LogicaDetallePedido();

        // --- 1. ACCIÓN PARA INSERTAR DETALLE (Llamada desde AJAX) ---
        // 🔥 EL ESCUDO: El Auditor NO tiene permiso para insertar nada
        [HttpPost]
        [Authorize(Roles = "Administrador,Admin,Vendedor,Usuario")]
        public IActionResult Insertar([FromBody] DetallePedido oDetalle)
        {
            try
            {
                // Obtenemos el ID del usuario real desde los Claims (Adiós a la simulación)
                int idUsuarioSesion = 1;
                var claimId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (int.TryParse(claimId, out int parsedId))
                {
                    idUsuarioSesion = parsedId;
                }

                // Llamamos a la capa de negocio
                bool resultado = _logicaDetalle.InsertarDetalle(oDetalle, idUsuarioSesion);

                if (resultado)
                {
                    return Json(new { exito = true, mensaje = "Producto agregado al pedido." });
                }

                return Json(new { exito = false, mensaje = "No se pudo registrar el detalle en la base de datos." });
            }
            catch (ArgumentException ex)
            {
                // Atrapamos validaciones de negocio
                return Json(new { exito = false, mensaje = ex.Message });
            }
            catch (Exception ex)
            {
                // Atrapamos errores graves
                return Json(new { exito = false, mensaje = "Error del sistema: " + ex.Message });
            }
        }

        // --- 2. ACCIÓN PARA MOSTRAR EL TICKET ---
        // Todos pueden ver el ticket, INCLUYENDO al Auditor para sus revisiones
        [HttpGet]
        [Authorize(Roles = "Administrador,Admin,Vendedor,Auditor,Usuario")]
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