using Microsoft.AspNetCore.Mvc;
using LogicaNegocio;
using Entidades;
using System;
using System.Collections.Generic;
using System.Linq;

namespace TuProyecto.Controllers // Acordate de cambiar esto por el namespace de tu app
{
    public class CompraController : Controller
    {
        // 1. Instanciamos tus clases con TUS nombres exactos
        private readonly LogicaCompra _logica = new LogicaCompra();
        private readonly Logica_Proveedor _logicaProveedor = new Logica_Proveedor();
        private readonly LogicaProducto _logicaProducto = new LogicaProducto();

        public IActionResult Index()
        {
            try
            {
                // A. Llenamos los combos (Esto está bien)
                ViewBag.Proveedores = _logicaProveedor.ListarProveedores();
                ViewBag.Productos = _logicaProducto.ListarProductos();

                // B. 🔥 AQUÍ ESTÁ EL CAMBIO: Llamamos a la base de datos de verdad
                // En lugar de: List<Compra> listaCompras = new List<Compra>();
                List<Compra> listaCompras = _logica.ListarCompras();

                return View(listaCompras);
            }
            catch (Exception ex)
            {
                ViewBag.Error = "Error al cargar compras: " + ex.Message;
                return View(new List<Compra>());
            }
        }

        // 3. RECIBE LOS DATOS DEL MODAL (AJAX)
        [HttpPost]
        public IActionResult GuardarCompra(int idProveedor, string factura, int idProducto, int cantidad, decimal costo, string listaImeis)
        {
            try
            {
                // Obtenemos el ID del usuario logueado
                int idUsuarioActual = 1; // Valor por defecto

                if (User.Identity.IsAuthenticated)
                {
                    var claimId = User.Claims.FirstOrDefault(c => c.Type == "IdUsuario");
                    if (claimId != null)
                    {
                        idUsuarioActual = Convert.ToInt32(claimId.Value);
                    }
                }

                // Armamos el objeto detalle
                DetalleCompra detalle = new DetalleCompra
                {
                    ID_PRODUCTO = idProducto,
                    CANTIDAD = cantidad,
                    COSTO_UNITARIO = costo,
                    LISTA_IMEIS = listaImeis
                };

                // Mandamos a la lógica de compra
                bool exito = _logica.RegistrarEntrada(idProveedor, idUsuarioActual, factura, detalle);

                if (exito)
                {
                    return Json(new { success = true, message = "¡Inventario cargado y stock actualizado!" });
                }
                else
                {
                    return Json(new { success = false, message = "No se pudo completar el registro." });
                }
            }
            catch (Exception ex)
            {
                // Atrapamos errores (Ej: IMEIs no coinciden con cantidad)
                return Json(new { success = false, message = ex.Message });
            }
        }
    }
}