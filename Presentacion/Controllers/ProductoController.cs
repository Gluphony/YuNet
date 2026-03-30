using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Authorization;
using LogicaNegocio;
using Entidades;
using Presentacion.Reportes; // El namespace de tu nueva carpeta
using QuestPDF.Fluent;

namespace Presentacion.Controllers
{
    [Authorize] // 🔒 Por defecto, todo aquí requiere login...
    public class ProductoController : Controller
    {
        private readonly LogicaProducto _logicaProducto = new LogicaProducto();
        private readonly LogicaMarca _logicaMarca = new LogicaMarca();
        private readonly LogicaCategoria _logicaCategoria = new LogicaCategoria();

        // ==========================================
        // 🛍️ VITRINA: PÚBLICA PARA TODOS
        // ==========================================
        [AllowAnonymous] // 🔓 ...EXCEPTO la vitrina, que es pública
        public ActionResult Index()
        {
            try
            {
                DataTable dt = _logicaProducto.ListarProductos();
                List<Producto> lista = ConvertirDataTableALista(dt);

                // Verificamos de forma segura si el usuario está autenticado y tiene los roles
                bool esAdminOGestor = User?.Identity?.IsAuthenticated == true &&
                                     (User.IsInRole("Administrador") || User.IsInRole("Admin") ||
                                      User.IsInRole("Vendedor") || User.IsInRole("Auditor"));

                if (esAdminOGestor)
                {
                    CargarListasDesplegables();
                    return View(lista); // Muestra la tabla CRUD
                }
                else
                {
                    return View("Tienda", lista); // Muestra la vitrina al cliente/invitado
                }
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Error al listar productos: " + ex.Message;
                // Devolvemos una lista vacía para que no se rompa la vista
                return User?.Identity?.IsAuthenticated == true && User.IsInRole("Administrador")
                       ? View(new List<Producto>())
                       : View("Tienda", new List<Producto>());
            }
        }

        [HttpGet]
        public IActionResult ObtenerImeisDisponibles(int idProducto)
        {
            try
            {
                // Asegurate de llamar a tu capa lógica correspondiente
                List<UnidadExistencia> lista = _logicaProducto.ListarImeisDisponibles(idProducto);
                return Json(new { success = true, data = lista });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        [AllowAnonymous]
        public ActionResult FiltrarPorNombre(string nombre)
        {
            try
            {
                // Si el usuario borra todo y da clic en buscar, podrías redirigir al Index normal
                if (string.IsNullOrEmpty(nombre))
                {
                    return RedirectToAction("Index");
                }

                DataTable dt = _logicaProducto.BuscarProductoPorNombre(nombre);
                List<Producto> lista = ConvertirDataTableALista(dt);

                ViewBag.BusquedaActual = nombre; // Esto mantiene el texto en el buscador
                CargarListasDesplegables();

                if (User.Identity.IsAuthenticated && (User.IsInRole("Admin") || User.IsInRole("Vendedor") || User.IsInRole("Auditor")))
                {
                    return View("Index", lista);
                }
                else
                {
                    return View("Tienda", lista);
                }
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Error al filtrar: " + ex.Message;
                return RedirectToAction("Index");
            }
        }

        [AllowAnonymous] // 🔓 Cualquiera debe poder ver las imágenes de los productos
        public ActionResult VerImagen(int id)
        {
            byte[] imagen = _logicaProducto.ObtenerImagenProducto(id);
            if (imagen != null && imagen.Length > 0)
            {
                return File(imagen, "image/jpeg");
            }
            return File("~/img/no-image.png", "image/png"); // Imagen por defecto si no tiene
        }

        // ==========================================
        // 💳 CHECKOUT / PAGAR: REQUIERE ESTAR LOGUEADO
        // ==========================================
        [HttpGet]
        public ActionResult Pagar()
        {
            // Lógica para ir a la pasarela de pago o confirmar carrito
            return View();
        }

        // ==========================================
        // 🛠️ MANTENIMIENTO: SÓLO ADMINISTRADORES
        // ==========================================
        [Authorize(Roles = "Administrador,Admin")] // 🔒 Blindado solo para Admins
        public ActionResult Create()
        {
            CargarListasDesplegables();
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Administrador,Admin")]
        public ActionResult Create(Producto oProducto, IFormFile archivoImagen)
        {
            try
            {
                if (archivoImagen != null && archivoImagen.Length > 0)
                {
                    using (var ms = new System.IO.MemoryStream())
                    {
                        archivoImagen.CopyTo(ms);
                        oProducto.IMAGEN = ms.ToArray();
                    }
                }

                if (_logicaProducto.InsertarProducto(oProducto))
                {
                    TempData["MensajeExito"] = "Producto registrado con éxito.";
                    return RedirectToAction("Index");
                }
                else
                {
                    ViewBag.Error = "No se pudo registrar el producto en la base de datos.";
                    CargarListasDesplegables(oProducto.ID_CATEGORIA, oProducto.ID_MARCA);
                    return View(oProducto);
                }
            }
            catch (Exception ex)
            {
                ViewBag.Error = "Error SQL/Sistema: " + ex.Message;
                CargarListasDesplegables(oProducto.ID_CATEGORIA, oProducto.ID_MARCA);
                return View(oProducto);
            }
        }

        [Authorize(Roles = "Administrador,Admin")]
        public ActionResult Editar(int id)
        {
            try
            {
                // OPTIMIZACIÓN: Solo buscamos el producto específico en lugar de cargar toda la base de datos
                DataTable dt = _logicaProducto.ListarProductos();
                List<Producto> lista = ConvertirDataTableALista(dt);
                Producto productoAEditar = lista.FirstOrDefault(p => p.ID_PRODUCTO == id);

                if (productoAEditar == null)
                {
                    TempData["MensajeError"] = "El producto no fue encontrado.";
                    return RedirectToAction("Index");
                }

                CargarListasDesplegables(productoAEditar.ID_CATEGORIA, productoAEditar.ID_MARCA);
                return View(productoAEditar);
            }
            catch (Exception ex)
            {
                TempData["MensajeError"] = "Error al buscar el producto: " + ex.Message;
                return RedirectToAction("Index");
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Administrador,Admin")]
        public ActionResult Editar(Producto oProducto, IFormFile archivoImagen)
        {
            try
            {
                if (archivoImagen != null && archivoImagen.Length > 0)
                {
                    using (var ms = new System.IO.MemoryStream())
                    {
                        archivoImagen.CopyTo(ms);
                        oProducto.IMAGEN = ms.ToArray();
                    }
                }
                else
                {
                    // Si no sube imagen, aseguramos que sea null para que la lógica de negocio no la sobreescriba en blanco
                    oProducto.IMAGEN = null;
                }

                if (_logicaProducto.EditarProducto(oProducto))
                    TempData["MensajeExito"] = "Producto actualizado correctamente.";
                else
                    TempData["MensajeError"] = "No se pudo actualizar el producto.";
            }
            catch (Exception ex)
            {
                TempData["MensajeError"] = "Error al editar: " + ex.Message;
            }

            return RedirectToAction("Index");
        }

        [HttpPost]
        [Authorize(Roles = "Administrador,Admin")]
        public ActionResult Desactivar(int id)
        {
            try
            {
                if (_logicaProducto.DesactivarProducto(id))
                    TempData["MensajeExito"] = "Producto desactivado correctamente.";
                else
                    TempData["MensajeError"] = "No se pudo desactivar el producto.";

                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                TempData["MensajeError"] = "Error al desactivar: " + ex.Message;
                return RedirectToAction("Index");
            }
        }

        // ==========================================
        // 🔧 MÉTODOS AUXILIARES
        // ==========================================
        private void CargarListasDesplegables(int? idCategoriaActual = null, int? idMarcaActual = null)
        {
            try
            {
                List<SelectListItem> listaMarcas = new List<SelectListItem>();
                var marcas = _logicaMarca.Listar();

                foreach (var marca in marcas)
                {
                    if (marca.ESTADO == true || (idMarcaActual.HasValue && marca.ID_MARCA == idMarcaActual.Value))
                    {
                        listaMarcas.Add(new SelectListItem
                        {
                            Value = marca.ID_MARCA.ToString(),
                            Text = marca.NOMBRE
                        });
                    }
                }
                ViewBag.Marcas = listaMarcas;

                List<SelectListItem> listaCategorias = new List<SelectListItem>();
                var categorias = _logicaCategoria.Listar();

                foreach (var cat in categorias)
                {
                    if (cat.ESTADO == true || (idCategoriaActual.HasValue && cat.ID_CATEGORIA == idCategoriaActual.Value))
                    {
                        listaCategorias.Add(new SelectListItem
                        {
                            Value = cat.ID_CATEGORIA.ToString(),
                            Text = cat.NOMBRE_CATEGORIA
                        });
                    }
                }
                ViewBag.Categorias = listaCategorias;
            }
            catch (Exception)
            {
                ViewBag.Marcas = new List<SelectListItem>();
                ViewBag.Categorias = new List<SelectListItem>();
            }
        }

        private List<Producto> ConvertirDataTableALista(DataTable dt)
        {
            List<Producto> lista = new List<Producto>();
            foreach (DataRow row in dt.Rows)
            {
                // Protegemos contra posibles valores nulos (DBNull) en la BD para evitar crasheos
                lista.Add(new Producto
                {
                    ID_PRODUCTO = row["ID_PRODUCTO"] != DBNull.Value ? Convert.ToInt32(row["ID_PRODUCTO"]) : 0,
                    ID_MARCA = row["ID_MARCA"] != DBNull.Value ? Convert.ToInt32(row["ID_MARCA"]) : 0,
                    ID_CATEGORIA = row["ID_CATEGORIA"] != DBNull.Value ? Convert.ToInt32(row["ID_CATEGORIA"]) : 0,
                    MODELO = row["MODELO"]?.ToString() ?? "",
                    DESCRIPCION = row["DESCRIPCION"]?.ToString() ?? "",
                    CARACTERISTICAS = row.Table.Columns.Contains("CARACTERISTICAS") && row["CARACTERISTICAS"] != DBNull.Value
                                        ? row["CARACTERISTICAS"].ToString()
                                        : "",
                    PRECIO = row["PRECIO"] != DBNull.Value ? Convert.ToDecimal(row["PRECIO"]) : 0m,
                    EXISTENCIAS = row["EXISTENCIAS"] != DBNull.Value ? Convert.ToInt32(row["EXISTENCIAS"]) : 0,
                    ESTADO = row["ESTADO"] != DBNull.Value && Convert.ToBoolean(row["ESTADO"])
                });
            }
            return lista;
        }

        [HttpGet]
        public IActionResult DescargarPdf()
        {
            try
            {
                // 1. Obtenemos los datos (lista completa ya que no hay filtro aún)
                DataTable dt = _logicaProducto.ListarProductos();
                List<Producto> lista = ConvertirDataTableALista(dt);

                // 2. Si la lista es nula, inicializamos para evitar errores
                if (lista == null) lista = new List<Producto>();

                // 3. Generamos el reporte (crearemos esta clase a continuación)
                var documento = new ProductoReporte(lista);

                // 4. Convertimos a bytes y retornamos el stream
                byte[] pdfBytes = documento.GeneratePdf();
                var stream = new MemoryStream(pdfBytes);

                return File(stream, "application/pdf");
            }
            catch (Exception ex)
            {
                return Content("Error al generar reporte de inventario: " + ex.Message);
            }
        }
        }
}