using Entidades;
using LogicaNegocio;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Authorization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;

namespace Presentacion.Controllers
{
    public class PedidoController : Controller
    {
        // Instancias de tus capas de Lógica de Negocio
        private readonly LogicaPedido _logicaPedido = new LogicaPedido();
        private readonly LogicaUsuario _logicaUsuario = new LogicaUsuario();
        private readonly LogicaDetallePedido _logicaDetalle = new LogicaDetallePedido();

        // ==========================================
        // GET: Pedido/Index (Tabla general para el Admin)
        // ==========================================
        public ActionResult Index()
        {
            try
            {
                var listaPedidos = _logicaPedido.Listar();
                return View(listaPedidos);
            }
            catch (Exception ex)
            {
                ViewBag.Error = "Error al cargar la lista de pedidos: " + ex.Message;
                return View(new List<Pedido>());
            }
        }

[HttpGet]
public ActionResult Historial()
{
    try
    {
        // 1. Obtenemos el ID del usuario directamente de las Claims de su sesión
        var claimId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value 
                      ?? User.FindFirst("IdUsuario")?.Value;

        if (string.IsNullOrEmpty(claimId))
        {
            // Si por alguna razón la sesión se perdió, lo mandamos al Login
            return RedirectToAction("Login", "Cuenta"); 
        }

        int idUsuarioActual = int.Parse(claimId);

                // 2. Filtramos los pedidos para que solo vea los suyos
                var misPedidos = _logicaPedido.ListarPorUsuario(idUsuarioActual);

                return View(misPedidos);
    }
    catch (Exception ex)
    {
        ViewBag.Error = "Error al cargar tu historial de compras: " + ex.Message;
        return View(new List<Pedido>());
    }
}

        public ActionResult Details(int id)
        {
            try
            {
                // 1. Obtenemos la información general del pedido
                var pedido = _logicaPedido.ObtenerPorId(id);

                // Si alguien intenta buscar un ID que no existe, lo regresamos
                if (pedido == null)
                {
                    TempData["MensajeError"] = "El pedido solicitado no existe o fue eliminado.";
                    // Nota: Si esta vista es solo para el cliente, quizás quieras cambiar "Index" por "Historial"
                    return RedirectToAction("Index");
                }

                // 2. ¡LO NUEVO! Buscamos los productos que pertenecen a este ID de pedido
                var listaProductos = _logicaPedido.ObtenerDetallesPorPedido(id);

                // 3. ¡LO NUEVO! Guardamos la lista en el ViewBag para leerla en el HTML
                ViewBag.Detalles = listaProductos;

                // 4. Mandamos el pedido a la vista como modelo principal
                return View(pedido);
            }
            catch (Exception ex)
            {
                TempData["MensajeError"] = "Error al cargar los detalles del pedido: " + ex.Message;
                return RedirectToAction("Index");
            }
        }

        // ==========================================
        // NUEVO: POST PARA ASIGNAR IMEI (AJAX)
        // ==========================================
        [HttpPost]
        public IActionResult AsignarImei(int idDetallePedido, string imei)
        {
            try
            {
                // Instanciamos la lógica
                LogicaDetallePedido logica = new LogicaDetallePedido();

                // Ejecutamos la función
                bool respuesta = logica.AsignarImeiDespacho(idDetallePedido, imei);

                if (respuesta)
                {
                    // Si todo sale al cien, le mandamos el OK al SweetAlert
                    return Json(new { success = true, message = $"IMEI {imei} asignado y descontado del inventario correctamente." });
                }

                return Json(new { success = false, message = "No se pudo asignar el IMEI por un problema interno." });
            }
            catch (Exception ex)
            {
                // Si el SQL bota el error (ej. "El IMEI ya fue vendido"), cae aquí y se lo mandamos a la vista
                return Json(new { success = false, message = ex.Message });
            }
        }
        // ==========================================
        public ActionResult Ticket(int id)
        {
            try
            {
                List<DetallePedido> listaDetalles = _logicaDetalle.ListarTicket(id);

                if (listaDetalles == null || !listaDetalles.Any())
                {
                    ViewBag.Error = "No se encontraron los productos para este pedido o el pedido está vacío.";
                    return View(new List<DetallePedido>());
                }

                // NUEVO: Le pasamos el ID a la vista mediante ViewBag
                ViewBag.NumeroPedido = id;

                return View(listaDetalles);
            }
            catch (Exception ex)
            {
                TempData["MensajeError"] = "Error al cargar el ticket del pedido: " + ex.Message;
                return RedirectToAction("Index");
            }
        }

        // ==========================================
        // GET: Pedido/Create
        // ==========================================
        //public ActionResult Create()
        //{
        //    CargarUsuariosDropdown();
        //    return View(new Pedido());
        //}

        //// ==========================================
        //// POST: Pedido/Create
        //// ==========================================
        //[HttpPost]
        //[ValidateAntiForgeryToken]
        //public ActionResult Create(Pedido nuevoPedido)
        //{
        //    try
        //    {
        //        nuevoPedido.FECHA_PEDIDO = DateTime.Now;

        //        // Limpiamos validaciones
        //        ModelState.Remove("ID_PEDIDO");
        //        ModelState.Remove("Usuario");
        //        ModelState.Remove("Detalles");
        //        ModelState.Remove("DetallePedidos");

        //        if (ModelState.IsValid)
        //        {
        //            int idGenerado = _logicaPedido.Crear(nuevoPedido);
        //            TempData["MensajeExito"] = $"¡Pedido #{idGenerado} creado correctamente!";
        //            return RedirectToAction("Index");
        //        }
        //        else
        //        {
        //            var listaErrores = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList();
        //            ViewBag.Error = "Revise los campos: " + string.Join(" | ", listaErrores);
        //        }
        //    }
        //    catch (ArgumentException argEx)
        //    {
        //        ViewBag.Error = argEx.Message;
        //    }
        //    catch (Exception ex)
        //    {
        //        ViewBag.Error = "Error en el servidor: " + ex.Message;
        //    }

        //    CargarUsuariosDropdown();
        //    return View(nuevoPedido);
        //}

        // ==========================================
        // POST: Pedido/ActualizarEstado
        // ==========================================
        [HttpPost]
        public ActionResult ActualizarEstado(int idPedido, string nuevoEstado)
        {
            try
            {
                // Llamamos a tu lógica de negocio
                bool exito = _logicaPedido.ActualizarEstado(idPedido, nuevoEstado);

                if (exito)
                {
                    // Devolvemos JSON confirmando el éxito
                    return Json(new { success = true });
                }
                else
                {
                    // Devolvemos JSON indicando que falló lógicamente
                    return Json(new { success = false, message = "No se pudo actualizar el estado en la base de datos." });
                }
            }
            catch (Exception ex)
            {
                // Devolvemos JSON con el error exacto
                return Json(new { success = false, message = "Error: " + ex.Message });
            }
        }

        // ==========================================
        // MÉTODOS AUXILIARES
        // ==========================================
        private void CargarUsuariosDropdown()
        {
            try
            {
                var dtUsuarios = _logicaUsuario.ListarUsuarios();
                var listaOpciones = new List<SelectListItem>();

                if (dtUsuarios != null)
                {
                    foreach (System.Data.DataRow row in dtUsuarios.Rows)
                    {
                        listaOpciones.Add(new SelectListItem
                        {
                            Value = row["ID_USUARIO"].ToString(),
                            Text = $"{row["NOMBRE"]} (ID: {row["ID_USUARIO"]})"
                        });
                    }
                }
                ViewBag.ListaUsuarios = listaOpciones;
            }
            catch (Exception)
            {
                ViewBag.ListaUsuarios = new List<SelectListItem>();
            }
        }
    }
}