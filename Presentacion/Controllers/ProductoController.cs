using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Rendering; // IMPORTANTE: Necesario para los menús desplegables
using LogicaNegocio;
using Entidades;

namespace Presentacion.Controllers
{
    public class ProductoController : Controller
    {
        private readonly LogicaProducto _logicaProducto = new LogicaProducto();

        // --- INSTANCIAS DE LÓGICA PARA MARCA Y CATEGORÍA ---
        // (Asegúrate de tener estas clases creadas en tu capa LogicaNegocio)
        private readonly LogicaMarca _logicaMarca = new LogicaMarca();
        private readonly LogicaCategoria _logicaCategoria = new LogicaCategoria();

        // GET: Producto
        public ActionResult Index()
        {
            try
            {
                // Convertimos el DataTable de la Lógica a una Lista de Entidades
                DataTable dt = _logicaProducto.ListarProductos();
                List<Producto> lista = ConvertirDataTableALista(dt);

                // --- CARGAMOS LAS LISTAS PARA EL MODAL ---
                CargarListasDesplegables();

                return View(lista);
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Error al listar productos: " + ex.Message;

                // Si falla, enviamos listas vacías para que no truene la vista
                ViewBag.Marcas = new List<SelectListItem>();
                ViewBag.Categorias = new List<SelectListItem>();

                return View(new List<Producto>());
            }
        }

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
        // POST: Producto/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
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
                    TempData["Success"] = "Producto registrado con éxito.";
                    return RedirectToAction("Index");
                }
            }
            catch (Exception ex)
            {
                // ¡ESTO ES LO QUE CAMBIA! 
                // Guardamos el error y redireccionamos al Index en lugar de buscar la vista "Create"
                TempData["Error"] = ex.Message;
                return RedirectToAction("Index");
            }

            TempData["Error"] = "No se pudo registrar el producto.";
            return RedirectToAction("Index");
        }

        // POST: Producto/Desactivar/5
        [HttpPost]
        public JsonResult Desactivar(int id)
        {
            try
            {
                bool resultado = _logicaProducto.DesactivarProducto(id);
                return Json(new { success = resultado });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        // --- MÉTODOS AUXILIARES ---

        private void CargarListasDesplegables()
        {
            try
            {
                // 1. Cargar Marcas (Trabajando con List<Marca> como lo tienes en tu Lógica)
                List<SelectListItem> listaMarcas = new List<SelectListItem>();
                var marcas = _logicaMarca.Listar(); // Llama a tu método Listar()

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
                // Asumo que tu LogicaCategoria funciona exactamente igual que LogicaMarca
                List<SelectListItem> listaCategorias = new List<SelectListItem>();
                var categorias = _logicaCategoria.Listar(); // Ajusta el nombre si se llama distinto

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
                // Previene que la vista se caiga si hay error
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
                    MODELO = row["MODELO"].ToString(),
                    DESCRIPCION = row["DESCRIPCION"].ToString(),
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