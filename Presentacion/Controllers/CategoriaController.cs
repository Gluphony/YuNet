using Microsoft.AspNetCore.Mvc;
using Entidades;
using LogicaNegocio;
using System;

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
            var lista = _logicaCategoria.FiltrarPorNombre(nombre);

            // Retornamos a la vista Index pero con la lista filtrada
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
    }
}