using Microsoft.AspNetCore.Mvc;
using Entidades;
using LogicaNegocio;
using System;
using Presentacion.Reportes; // El namespace de tu nueva carpeta
using QuestPDF.Fluent;

namespace TuProyecto.Controllers
{
    public class CategoriaController : Controller
    {
        // Instanciamos la clase de Lógica de Negocio que me compartiste
        private readonly LogicaCategoria _logicaCategoria = new LogicaCategoria();

        // 1. LISTAR (GET: /Categoria o /Categoria/Index)
        public IActionResult Index()
        {
            var lista = _logicaCategoria.Listar();
            return View(lista);
        }

        // 2. FILTRAR (GET: /Categoria/Filtrar?nombre=algo)
        public IActionResult Filtrar(string nombre)
        {
            // Si el nombre es nulo, traemos todo; si no, filtramos.
            var lista = string.IsNullOrEmpty(nombre)
                        ? _logicaCategoria.Listar()
                        : _logicaCategoria.FiltrarPorNombre(nombre);

            // Mantenemos el valor en el buscador para que el usuario sepa qué buscó
            ViewBag.NombreFiltro = nombre;

            return View("Index", lista);
        }

        // 3. CREAR (POST: /Categoria/Crear)
        [HttpPost]
        public IActionResult Crear(Categoria categoria)
        {
            try
            {
                _logicaCategoria.Crear(categoria);
                TempData["MensajeExito"] = "Categoría creada correctamente.";
            }
            catch (Exception ex)
            {
                // Atrapa las validaciones que pusiste en tu LN (ej. nombre vacío)
                TempData["MensajeError"] = ex.Message;
            }

            return RedirectToAction("Index");
        }

        // 4. EDITAR (POST: /Categoria/Editar)
        [HttpPost]
        public IActionResult Editar(Categoria categoria)
        {
            try
            {
                _logicaCategoria.Editar(categoria);
                TempData["MensajeExito"] = "Categoría actualizada correctamente.";
            }
            catch (Exception ex)
            {
                TempData["MensajeError"] = ex.Message;
            }

            return RedirectToAction("Index");
        }

        // 5. DESACTIVAR (POST: /Categoria/Desactivar/5)
        [HttpPost]
        public IActionResult Desactivar(int idCategoria)
        {
            try
            {
                _logicaCategoria.Desactivar(idCategoria);
                TempData["MensajeExito"] = "Categoría desactivada.";
            }
            catch (Exception ex)
            {
                TempData["MensajeError"] = ex.Message;
            }

            return RedirectToAction("Index");
        }

        [HttpGet]
        public IActionResult DescargarPdf(string busqueda)
        {
            try
            {
                // 1. Traemos los datos y forzamos a que sea una Lista fija en memoria
                // Cambiamos 'Marca' por 'Categoria' y usamos tu lógica de categorías
                List<Categoria> lista = string.IsNullOrEmpty(busqueda)
                                     ? _logicaCategoria.Listar().ToList()
                                     : _logicaCategoria.FiltrarPorNombre(busqueda).ToList();

                // 2. Si la lista está vacía, evitamos que el PDF falle
                if (lista == null) lista = new List<Categoria>();

                // 3. Instanciamos el reporte específico de Categorías
                // Nota: Debes tener creada la clase CategoriaReporte (similar a MarcaReporte)
                var documento = new CategoriaReporte(lista);

                // 4. Generamos los bytes
                byte[] pdfBytes = documento.GeneratePdf();

                var stream = new MemoryStream(pdfBytes);

                // 5. Retornamos el archivo
                // Al no poner el nombre del archivo como tercer parámetro, 
                // el navegador suele abrirlo en una pestaña nueva (target="_blank")
                return File(stream, "application/pdf");
            }
            catch (Exception ex)
            {
                // Manejo de errores para que no se caiga tu proyecto de la UdeM
                return Content("Error al generar el reporte de categorías: " + ex.Message);
            }
        }
    }
}