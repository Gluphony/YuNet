using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using LogicaNegocio;
using Entidades;

namespace Presentacion.Controllers
{
    public class ProductoController : Controller
    {
        private readonly LogicaProducto _logicaProducto = new LogicaProducto();

        // GET: Producto
        public ActionResult Index()
        {
            try
            {
                // Convertimos el DataTable de la Lógica a una Lista de Entidades
                DataTable dt = _logicaProducto.ListarProductos();
                List<Producto> lista = ConvertirDataTableALista(dt);

                return View(lista);
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Error al listar productos: " + ex.Message;
                return View(new List<Producto>());
            }
        }

        // GET: Producto/Imagen/5
        // Método para mostrar la imagen en la etiqueta <img src="...">
        public ActionResult VerImagen(int id)
        {
            byte[] imagen = _logicaProducto.ObtenerImagenProducto(id);
            if (imagen != null && imagen.Length > 0)
            {
                return File(imagen, "image/jpeg"); // O el formato que uses
            }
            return File("~/img/no-image.png", "image/png"); // Imagen por defecto
        }

        // GET: Producto/Create
        public ActionResult Create()
        {
            return View();
        }

        // POST: Producto/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(Producto oProducto, IFormFile archivoImagen)
        {
            try
            {
                // Manejo de la subida de imagen
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
                ViewBag.Error = ex.Message;
            }
            return View(oProducto);
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
                    FECHA_CREACION = Convert.ToDateTime(row["FECHA_CREACION"])
                });
            }
            return lista;
        }
    }
}
