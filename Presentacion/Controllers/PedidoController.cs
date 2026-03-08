using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc; // O System.Web.Mvc dependiendo de tu versión
using LogicaNegocio;
using Entidades;

namespace Presentacion.Controllers
{
    public class PedidoController : Controller
    {
        // Instanciamos la lógica de negocio
        private readonly LogicaPedido _logicaPedido = new LogicaPedido();

        // GET: Pedido
        // Muestra la lista de todos los pedidos
        public ActionResult Index()
        {
            try
            {
                List<Pedido> lista = _logicaPedido.Listar();
                return View(lista);
            }
            catch (Exception ex)
            {
                ViewBag.Error = "Error al obtener pedidos: " + ex.Message;
                return View(new List<Pedido>());
            }
        }

        // GET: Pedido/Create
        public ActionResult Create()
        {
            return View();
        }

        // POST: Pedido/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(Pedido nuevoPedido)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    _logicaPedido.Crear(nuevoPedido);
                    return RedirectToAction("Index");
                }
            }
            catch (Exception ex)
            {
                // Aquí capturamos los "throw" que lanzaste en tu LogicaPedido
                ViewBag.Error = ex.Message;
            }

            return View(nuevoPedido);
        }

        // POST: Pedido/ActualizarEstado
        [HttpPost]
        public JsonResult ActualizarEstado(int id, string nuevoEstado)
        {
            try
            {
                bool exito = _logicaPedido.ActualizarEstado(id, nuevoEstado);
                return Json(new { success = exito });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        // GET: Pedido/MisPedidos/5
        public ActionResult MisPedidos(int idUsuario)
        {
            try
            {
                var pedidos = _logicaPedido.ListarPorUsuario(idUsuario);
                return View(pedidos);
            }
            catch (Exception ex)
            {
                TempData["Error"] = ex.Message;
                return RedirectToAction("Index");
            }
        }
    }
}