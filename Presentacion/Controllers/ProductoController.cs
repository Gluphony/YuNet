using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Rendering;
using LogicaNegocio;
using Entidades;

namespace Presentacion.Controllers
{
    public class ProductoController : Controller
    {
        private readonly LogicaProducto _logicaProducto = new LogicaProducto();
        private readonly LogicaMarca _logicaMarca = new LogicaMarca();
        private readonly LogicaCategoria _logicaCategoria = new LogicaCategoria();

        // GET: Producto
        public ActionResult Index()
        {
            try
            {
                DataTable dt = _logicaProducto.ListarProductos();
                List<Producto> lista = ConvertirDataTableALista(dt);

                // ========================================================
                // 🚦 LÓGICA DE SEMÁFORO: SEPARAR VISTA SEGÚN EL ROL
                // ========================================================

                // Si el usuario es Administrador, le mostramos la vista clásica (la tabla)
                if (User.IsInRole("Administrador") || User.IsInRole("Admin"))
                {
                    CargarListasDesplegables(); // Solo el Admin necesita listas para editar/crear
                    return View(lista); // Retorna "Views/Producto/Index.cshtml"
                }
                else
                {
                    // Si es Cajero, Vendedor o cualquier otro, le mostramos el POS
                    // Retorna "Views/Producto/Tienda.cshtml"
                    return View("Tienda", lista);
                }
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Error al listar productos: " + ex.Message;

                if (User.IsInRole("Administrador"))
                {
                    ViewBag.Marcas = new List<SelectListItem>();
                    ViewBag.Categorias = new List<SelectListItem>();
                    return View(new List<Producto>());
                }
                else
                {
                    return View("Tienda", new List<Producto>());
                }
            }
        }

        // ... MANTÉN TODOS TUS OTROS MÉTODOS IGUALES DE AQUÍ HACIA ABAJO ...
        // (VerImagen, Create, Editar, Desactivar, CargarListasDesplegables, ConvertirDataTableALista)

        // GET: Producto/Imagen/5
        public ActionResult VerImagen(int id)
        {
            byte[] imagen = _logicaProducto.ObtenerImagenProducto(id);
            if (imagen != null && imagen.Length > 0)
            {
                return File(imagen, "image/jpeg");
            }
            return File("~/img/no-image.png", "image/png");
        }

        // GET: Producto/Create
        public ActionResult Create()
        {
            CargarListasDesplegables();
            return View();
        }

        // POST: Producto/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(Producto oProducto, IFormFile archivoImagen)
        {
            try
            {
                // 1. Procesamos la imagen si el usuario subió una
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
                    // Opcional: Si tu DB exige imagen, aquí podrías asignarle un arreglo de bytes vacío o una imagen por defecto
                    // oProducto.IMAGEN = new byte[0]; 
                }

                // 2. Intentamos guardar en la base de datos
                if (_logicaProducto.InsertarProducto(oProducto))
                {
                    // Usamos la variable que la vista YuNet entiende
                    TempData["MensajeExito"] = "Producto registrado con éxito.";
                    return RedirectToAction("Index");
                }
                else
                {
                    // Si la lógica devuelve falso
                    ViewBag.Error = "No se pudo registrar el producto en la base de datos. Verifica los datos.";
                    CargarListasDesplegables(); // Recargamos las listas para que no truene el formulario
                    return View(oProducto); // Regresamos a la vista de crear con los datos que ya había escrito
                }
            }
            catch (Exception ex)
            {
                // ¡AQUÍ CAPTURAMOS EL ERROR EXACTO!
                // Si la base de datos se queja de un campo faltante, lo veremos aquí.
                ViewBag.Error = "Error SQL/Sistema: " + ex.Message;
                CargarListasDesplegables();
                return View(oProducto);
            }
        }

        // ==========================================
        // MÉTODOS PARA EDITAR
        // ==========================================

        // Nota: Como usas un Modal en el Index para editar, este método GET no lo usará 
        // tu vista principal, pero lo mantenemos por si lo llamas directo desde URL o lo necesitas luego.
        public ActionResult Editar(int id)
        {
            try
            {
                DataTable dt = _logicaProducto.ListarProductos();
                List<Producto> lista = ConvertirDataTableALista(dt);

                Producto productoAEditar = lista.FirstOrDefault(p => p.ID_PRODUCTO == id);

                if (productoAEditar == null)
                {
                    TempData["MensajeError"] = "El producto no fue encontrado.";
                    return RedirectToAction("Index");
                }

                CargarListasDesplegables();
                return View(productoAEditar);
            }
            catch (Exception ex)
            {
                TempData["MensajeError"] = "Error al buscar el producto: " + ex.Message;
                return RedirectToAction("Index");
            }
        }

        // POST: Producto/Editar
        // ESTE ES EL QUE RECIBE LOS DATOS DEL MODAL
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Editar(Producto oProducto, IFormFile archivoImagen)
        {
            try
            {
                // 1. Procesamos la nueva imagen si subió una
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
                    // Si no sube imagen, lo dejamos en null para que tu lógica de BD (espero) 
                    // sepa que no debe actualizar el campo de la imagen.
                    oProducto.IMAGEN = null;
                }

                // 2. Guardamos en DB
                if (_logicaProducto.EditarProducto(oProducto))
                {
                    TempData["MensajeExito"] = "Producto actualizado correctamente.";
                }
                else
                {
                    TempData["MensajeError"] = "No se pudo actualizar el producto.";
                }
            }
            catch (Exception ex)
            {
                TempData["MensajeError"] = "Error al editar: " + ex.Message;
            }

            // REDIRIGE AL INDEX (ESTO ARREGLA TU ERROR DE PANTALLA ROJA Y ACTUALIZA LA TABLA)
            return RedirectToAction("Index");
        }


        // ==========================================
        // MÉTODO PARA DESACTIVAR
        // ==========================================

        // POST: Producto/Desactivar/5
        [HttpPost]
        public ActionResult Desactivar(int id)
        {
            try
            {
                if (_logicaProducto.DesactivarProducto(id))
                {
                    TempData["MensajeExito"] = "Producto desactivado correctamente.";
                }
                else
                {
                    TempData["MensajeError"] = "No se pudo desactivar el producto.";
                }

                // Redirigimos de vuelta a la tabla
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                TempData["MensajeError"] = "Error al desactivar: " + ex.Message;
                return RedirectToAction("Index");
            }
        }

        // --- MÉTODOS AUXILIARES ---

        private void CargarListasDesplegables()
        {
            try
            {
                // 1. Cargar Marcas
                List<SelectListItem> listaMarcas = new List<SelectListItem>();
                var marcas = _logicaMarca.Listar();

                foreach (var marca in marcas)
                {
                    listaMarcas.Add(new SelectListItem
                    {
                        Value = marca.ID_MARCA.ToString(),
                        Text = marca.NOMBRE
                    });
                }
                ViewBag.Marcas = listaMarcas;

                // 2. Cargar Categorías
                List<SelectListItem> listaCategorias = new List<SelectListItem>();
                var categorias = _logicaCategoria.Listar();

                foreach (var cat in categorias)
                {
                    listaCategorias.Add(new SelectListItem
                    {
                        Value = cat.ID_CATEGORIA.ToString(),
                        Text = cat.NOMBRE_CATEGORIA
                    });
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
                lista.Add(new Producto
                {
                    ID_PRODUCTO = Convert.ToInt32(row["ID_PRODUCTO"]),
                    ID_MARCA = Convert.ToInt32(row["ID_MARCA"]),

                    // AGREGADO: ID_CATEGORIA
                    // Usamos el operador "as" o DBNull para que no truene si viene nulo de la BD
                    ID_CATEGORIA = row["ID_CATEGORIA"] != DBNull.Value ? Convert.ToInt32(row["ID_CATEGORIA"]) : 0,

                    MODELO = row["MODELO"].ToString(),
                    DESCRIPCION = row["DESCRIPCION"].ToString(),

                    // AGREGADO: CARACTERÍSTICAS
                    CARACTERISTICAS = row.Table.Columns.Contains("CARACTERISTICAS") && row["CARACTERISTICAS"] != DBNull.Value
                                        ? row["CARACTERISTICAS"].ToString()
                                        : "",

                    PRECIO = Convert.ToDecimal(row["PRECIO"]),
                    EXISTENCIAS = Convert.ToInt32(row["EXISTENCIAS"]),
                    ESTADO = Convert.ToBoolean(row["ESTADO"]),
                    // FECHA_CREACION = Convert.ToDateTime(row["FECHA_CREACION"]) 
                });
            }
            return lista;
        }
    }
}