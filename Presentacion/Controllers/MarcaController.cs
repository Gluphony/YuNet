using Entidades;
using LogicaNegocio;
using Microsoft.AspNetCore.Mvc;
using Presentacion.Reportes; // El namespace de tu nueva carpeta
using QuestPDF.Fluent;
using System;
using System.Collections.Generic;

namespace Presentacion.Controllers
{
    public class MarcaController : Controller
    {
        private readonly LogicaMarca _logicaMarca = new LogicaMarca();

        // 1. LISTAR TODO (Al abrir la página)
        [HttpGet]
        public IActionResult Index()
        {
            var lista = _logicaMarca.Listar();
            return View(lista);
        }

        // 2. FILTRAR POR NOMBRE
        [HttpGet]
        public IActionResult Filtrar(string nombre)
        {
            try
            {
                List<Marca> lista;
                if (!string.IsNullOrEmpty(nombre))
                {
                    lista = _logicaMarca.FiltrarPorNombre(nombre);
                }
                else
                {
                    lista = _logicaMarca.Listar();
                }

                // Le decimos que muestre la vista "Index", pero con los datos filtrados
                return View("Index", lista);
            }
            catch (Exception ex)
            {
                TempData["MensajeError"] = "Error al buscar: " + ex.Message;
                return RedirectToAction("Index");
            }
        }

        // 3. CREAR NUEVA MARCA
        [HttpPost]
        public IActionResult Crear(Marca marca)
        {
            try
            {
                bool resultado = _logicaMarca.Crear(marca);
                if (resultado)
                {
                    TempData["MensajeExito"] = "Marca creada correctamente.";
                }
                else
                {
                    TempData["MensajeError"] = "No se pudo completar la operación en la base de datos.";
                }
            }
            catch (Exception ex)
            {
                TempData["MensajeError"] = ex.Message;
            }

            return RedirectToAction("Index");
        }

        // 4. EDITAR MARCA EXISTENTE
        [HttpPost]
        public IActionResult Editar(Marca marca)
        {
            try
            {
                bool resultado = _logicaMarca.Editar(marca);
                if (resultado)
                {
                    TempData["MensajeExito"] = "Marca actualizada correctamente.";
                }
                //else
                //{
                //    TempData["MensajeError"] = "No se pudo actualizar la marca.";
                //}
            }
            catch (Exception ex)
            {
                TempData["MensajeError"] = ex.Message;
            }

            return RedirectToAction("Index");
        }

        // 5. DESACTIVAR MARCA
        [HttpPost]
        public IActionResult Desactivar(int id)
        {
            try
            {
                bool resultado = _logicaMarca.Desactivar(id);
                if (resultado)
                {
                    TempData["MensajeExito"] = "Marca desactivada correctamente.";
                }
                else
                {
                    TempData["MensajeError"] = "No se pudo desactivar la marca.";
                }
            }
            catch (Exception ex)
            {
                // Esto asegura que si el error viene de SQL, solo mostramos el texto amigable
                TempData["MensajeError"] = "Atención: " + ex.Message;
            }

            return RedirectToAction("Index");
        }

        [HttpGet]

        public IActionResult DescargarPdf(string busqueda)
        {
            try
            {
                // 1. Traemos los datos y forzamos a que sea una Lista fija en memoria
                List<Marca> lista = string.IsNullOrEmpty(busqueda)
                                    ? _logicaMarca.Listar().ToList()
                                    : _logicaMarca.FiltrarPorNombre(busqueda).ToList();

                // 2. Si la lista está vacía, evitamos que el PDF falle
                if (lista == null) lista = new List<Marca>();

                var documento = new MarcaReporte(lista);

                // 3. Generamos los bytes
                byte[] pdfBytes = documento.GeneratePdf();

                var stream = new MemoryStream(pdfBytes);

                // 4. Retornamos el archivo
                // Usar "application/pdf" sin el nombre del archivo como tercer parámetro 
                // a veces ayuda a que el navegador no fuerce la descarga inmediata
                return File(stream, "application/pdf");
            }
            catch (Exception ex)
            {
                // Esto evitará que el proyecto se cierre si algo falla dentro
                return Content("Error al generar PDF: " + ex.Message);
            }
        }
    }
}