using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Entidades;
using LogicaNegocio;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Presentacion.Reportes;
using QuestPDF.Fluent;

namespace TuProyecto.Controllers
{
    // Solo los altos mandos y el auditor pueden entrar a este módulo
    [Authorize(Roles = "Administrador,Admin,Auditor")]
    public class CategoriaController : Controller
    {
        private readonly LogicaCategoria _logicaCategoria = new LogicaCategoria();

        // 1. LISTAR (Todos los autorizados arriba pueden ver esto)
        public IActionResult Index()
        {
            var lista = _logicaCategoria.Listar();
            return View(lista);
        }

        // 2. FILTRAR (Todos los autorizados arriba pueden usar el buscador)
        public IActionResult Filtrar(string nombre)
        {
            var lista = string.IsNullOrEmpty(nombre)
                        ? _logicaCategoria.Listar()
                        : _logicaCategoria.FiltrarPorNombre(nombre);

            ViewBag.NombreFiltro = nombre;

            return View("Index", lista);
        }

        // 3. CREAR (🔥 ESCUDO: El Auditor NO puede crear)
        [HttpPost]
        [Authorize(Roles = "Administrador,Admin")]
        public IActionResult Crear(Categoria categoria)
        {
            try
            {
                _logicaCategoria.Crear(categoria);
                TempData["MensajeExito"] = "Categoría creada correctamente.";
            }
            catch (Exception ex)
            {
                TempData["MensajeError"] = ex.Message;
            }

            return RedirectToAction("Index");
        }

        // 4. EDITAR (🔥 ESCUDO: El Auditor NO puede editar)
        [HttpPost]
        [Authorize(Roles = "Administrador,Admin")]
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

        // 5. DESACTIVAR (🔥 ESCUDO: El Auditor NO puede desactivar)
        [HttpPost]
        [Authorize(Roles = "Administrador,Admin")]
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

        // 6. EXPORTAR PDF (El Auditor SÍ puede hacer esto)
        [HttpGet]
        public IActionResult DescargarPdf(string busqueda)
        {
            try
            {
                List<Categoria> lista = string.IsNullOrEmpty(busqueda)
                                     ? _logicaCategoria.Listar().ToList()
                                     : _logicaCategoria.FiltrarPorNombre(busqueda).ToList();

                if (lista == null) lista = new List<Categoria>();

                var documento = new CategoriaReporte(lista);
                byte[] pdfBytes = documento.GeneratePdf();
                var stream = new MemoryStream(pdfBytes);

                return File(stream, "application/pdf");
            }
            catch (Exception ex)
            {
                return Content("Error al generar el reporte de categorías: " + ex.Message);
            }
        }
    }
}