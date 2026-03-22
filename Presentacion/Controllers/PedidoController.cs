using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Entidades;
using LogicaNegocio;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;

namespace Presentacion.Controllers
{
    public class PedidoController : Controller
    {
        private readonly LogicaUsuario _logicaUsuario = new LogicaUsuario();
        private readonly LogicaProducto _logicaProducto = new LogicaProducto();
        private readonly LogicaPedido _logicaPedido = new LogicaPedido();
        private readonly LogicaDetallePedido _logicaDetallePedido = new LogicaDetallePedido();

        // 1. LISTADO DE PEDIDOS
        public IActionResult Index()
        {
            try
            {
                // Se asume que LogicaPedido tiene el método Listar()
                var lista = _logicaPedido.Listar();
                return View(lista);
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Error al obtener pedidos: " + ex.Message;
                return View(new List<Pedido>());
            }
        }

        // 2. FORMULARIO DE NUEVO PEDIDO (GET)
        [HttpGet]
        public IActionResult Create()
        {
            // Forzamos la carga de datos
            CargarCombos();

            // Si después de CargarCombos los ViewBag siguen siendo null, 
            // los inicializamos aquí mismo para que la vista NO explote.
            ViewBag.ListaUsuarios ??= new SelectList(new List<SelectListItem>());
            ViewBag.ListaProductos ??= new SelectList(new List<SelectListItem>());

            return View(new Pedido());
        }

        // 3. GUARDAR PEDIDO Y DETALLES (POST)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(Pedido nuevoPedido, List<DetallePedido> Detalles)
        {
            try
            {
                // Validación manual de la lista de detalles
                if (Detalles == null || Detalles.Count == 0)
                {
                    ViewBag.Error = "Debe agregar al menos un producto al pedido.";
                    CargarCombos();
                    return View(nuevoPedido);
                }

                // Limpiamos errores de validación de campos automáticos
                ModelState.Remove("ID_PEDIDO");
                ModelState.Remove("Detalles");

                if (ModelState.IsValid)
                {
                    nuevoPedido.FECHA_PEDIDO = DateTime.Now;
                    nuevoPedido.ESTADO = "Pendiente";

                    // Usamos el método Crear que retorna el ID generado (ajustado según tu error)
                    int idGenerado = _logicaPedido.Crear(nuevoPedido);

                    if (idGenerado > 0)
                    {
                        foreach (var item in Detalles)
                        {
                            item.ID_PEDIDO = idGenerado;
                            // Se asume usuario ID 1 (puedes cambiarlo por el usuario logueado)
                            _logicaDetallePedido.InsertarDetalle(item, 1);
                        }

                        return RedirectToAction("Index");
                    }
                }
            }
            catch (Exception ex)
            {
                ViewBag.Error = "Ocurrió un error: " + ex.Message;
            }

            // Si llegamos aquí es porque algo falló, recargamos combos
            CargarCombos();
            return View(nuevoPedido);
        }

        // 4. ACTUALIZAR ESTADO (AJAX)
        [HttpPost]
        public IActionResult ActualizarEstado(int id, string nuevoEstado)
        {
            try
            {
                bool ok = _logicaPedido.ActualizarEstado(id, nuevoEstado);
                return Json(new { success = ok });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        // --- MÉTODOS AUXILIARES ---
        private void CargarCombos()
        {
            try
            {
                // 1. Usuarios
                DataTable dtUsuarios = _logicaUsuario.ListarUsuarios();
                if (dtUsuarios != null && dtUsuarios.Rows.Count > 0)
                {
                    ViewBag.ListaUsuarios = new SelectList(dtUsuarios.AsEnumerable(), "ID_USUARIO", "NOMBRE");
                }

                // 2. Productos
                DataTable dtProductos = _logicaProducto.ListarProductos();
                if (dtProductos != null && dtProductos.Rows.Count > 0)
                {
                    var prodItems = dtProductos.AsEnumerable().Select(r => new
                    {
                        ID_PRODUCTO = r["ID_PRODUCTO"],
                        DESCRIPCION = $"{r["MODELO"]} (${r["PRECIO"]})"
                    });
                    ViewBag.ListaProductos = new SelectList(prodItems, "ID_PRODUCTO", "DESCRIPCION");
                }
            }
            catch (Exception ex)
            {
                // Si hay un error de conexión, esto evita el crash
                ViewBag.Error = "Error al conectar con la base de datos: " + ex.Message;
            }
        }
    }
}