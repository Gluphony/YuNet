using Entidades;
using LogicaNegocio;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Presentacion.Controllers
{
    public class PedidoController : Controller
    {
        private readonly LogicaPedido _logicaPedido = new LogicaPedido();
        private readonly LogicaUsuario _logicaUsuario = new LogicaUsuario(); // Asumo que aún lo necesitas para el Dropdown

        // GET: Pedido/Index
        public ActionResult Index()
        {
            try
            {
                // Consumimos el método Listar() que creaste en tu capa de Lógica
                var listaPedidos = _logicaPedido.Listar();
                return View(listaPedidos);
            }
            catch (Exception ex)
            {
                ViewBag.Error = "Error al cargar la lista de pedidos: " + ex.Message;
                // Devolvemos una lista vacía para que la vista no explote por recibir un modelo nulo
                return View(new List<Pedido>());
            }
        }

        // ==========================================
        // NUEVO MÉTODO: GET: Pedido/Details/5
        // ==========================================
        public ActionResult Details(int id)
        {
            try
            {
                // Consumimos el método de tu lógica para buscar un pedido específico.
                // IMPORTANTE: Asegúrate de que este método exista en tu clase LogicaPedido.
                var pedido = _logicaPedido.ObtenerPorId(id);

                if (pedido == null)
                {
                    TempData["MensajeError"] = "El pedido solicitado no existe o fue eliminado.";
                    return RedirectToAction("Index");
                }

                return View(pedido);
            }
            catch (Exception ex)
            {
                TempData["MensajeError"] = "Error al cargar los detalles del pedido: " + ex.Message;
                return RedirectToAction("Index");
            }
        }

        // GET: Pedido/Create
        public ActionResult Create()
        {
            CargarUsuariosDropdown();
            return View(new Pedido());
        }

        // POST: Pedido/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(Pedido nuevoPedido)
        {
            try
            {
                nuevoPedido.FECHA_PEDIDO = DateTime.Now;

                // Limpiamos validaciones de propiedades que se llenan en la BD o son relaciones
                ModelState.Remove("ID_PEDIDO");
                ModelState.Remove("Usuario");
                ModelState.Remove("Detalles");
                ModelState.Remove("DetallePedidos");

                if (ModelState.IsValid)
                {
                    // Consumimos tu método Crear, el cual devuelve el ID del nuevo pedido
                    int idGenerado = _logicaPedido.Crear(nuevoPedido);

                    // TempData nos sirve para mandar mensajes a la vista Index después de un Redirect
                    TempData["MensajeExito"] = $"¡Pedido #{idGenerado} creado correctamente!";
                    return RedirectToAction("Index");
                }
                else
                {
                    var listaErrores = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList();
                    ViewBag.Error = "Revise los campos: " + string.Join(" | ", listaErrores);
                }
            }
            catch (ArgumentException argEx)
            {
                // ¡AQUÍ ATRAPAMOS TUS EXCEPCIONES PERSONALIZADAS!
                // Por ejemplo: "El total del pedido no puede ser menor a cero."
                ViewBag.Error = argEx.Message;
            }
            catch (Exception ex)
            {
                ViewBag.Error = "Error en el servidor: " + ex.Message;
            }

            // Si llegamos aquí es porque falló, recargamos el dropdown y devolvemos la vista
            CargarUsuariosDropdown();
            return View(nuevoPedido);
        }

        // POST: Pedido/ActualizarEstado
        // Te dejo este método listo por si quieres agregar un botón para cambiar estados desde el Index
        [HttpPost]
        public ActionResult CambiarEstado(int idPedido, string nuevoEstado)
        {
            try
            {
                bool exito = _logicaPedido.ActualizarEstado(idPedido, nuevoEstado);
                if (exito)
                {
                    TempData["MensajeExito"] = $"El estado del pedido #{idPedido} se actualizó a {nuevoEstado}.";
                }
            }
            catch (Exception ex)
            {
                TempData["MensajeError"] = "Error al cambiar estado: " + ex.Message;
            }

            return RedirectToAction("Index");
        }

        // === MÉTODO AUXILIAR PARA EL MENÚ DESPLEGABLE ===
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
                // Mandamos lista vacía para prevenir errores de referencia nula en la vista
                ViewBag.ListaUsuarios = new List<SelectListItem>();
            }
        }
    }
}