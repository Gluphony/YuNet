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
using System.Data; // <-- NUEVO: Necesario para leer la tabla de productos

namespace Presentacion.Controllers // Aseguramos que el namespace coincida con tu proyecto
{
    // Solo los altos mandos y el auditor pueden entrar a este módulo
    [Authorize(Roles = "Administrador,Admin,Auditor")]
    public class CategoriaController : Controller
    {
        private readonly LogicaCategoria _logicaCategoria = new LogicaCategoria();
        private readonly LogicaProducto _logicaProducto = new LogicaProducto(); // <-- NUEVO: El guardia que vigila los productos

        // 1. LISTAR (Todos los autorizados arriba pueden ver esto)
        [HttpGet]
        public IActionResult Index()
        {
            var lista = _logicaCategoria.Listar();
            return View(lista);
        }

        // 2. FILTRAR (Todos los autorizados arriba pueden usar el buscador)
        [HttpGet]
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
                // 🔥 EL ESCUDO DE INTEGRIDAD: Si intentan ponerla inactiva desde la edición...
                if (!categoria.ESTADO)
                {
                    DataTable dtProductos = _logicaProducto.ListarProductos();
                    bool tieneProductos = false;

                    foreach (DataRow row in dtProductos.Rows)
                    {
                        int idCategoriaProducto = row["ID_CATEGORIA"] != DBNull.Value ? Convert.ToInt32(row["ID_CATEGORIA"]) : 0;
                        bool estadoProducto = row["ESTADO"] != DBNull.Value && Convert.ToBoolean(row["ESTADO"]);

                        if (idCategoriaProducto == categoria.ID_CATEGORIA && estadoProducto)
                        {
                            tieneProductos = true;
                            break;
                        }
                    }

                    if (tieneProductos)
                    {
                        TempData["MensajeError"] = "Bloqueado: No puedes cambiar el estado a Inactivo porque existen productos activos en esta categoría.";
                        return RedirectToAction("Index");
                    }
                }

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
        public IActionResult Desactivar(int id) // <-- CAMBIO A 'id' PARA HACER MATCH CON LA VISTA
        {
            try
            {
                // 🔥 EL ESCUDO DE INTEGRIDAD: Revisar si hay productos vivos atados a esta categoría
                DataTable dtProductos = _logicaProducto.ListarProductos();
                bool tieneProductos = false;

                foreach (DataRow row in dtProductos.Rows)
                {
                    int idCategoriaProducto = row["ID_CATEGORIA"] != DBNull.Value ? Convert.ToInt32(row["ID_CATEGORIA"]) : 0;
                    bool estadoProducto = row["ESTADO"] != DBNull.Value && Convert.ToBoolean(row["ESTADO"]);

                    if (idCategoriaProducto == id && estadoProducto)
                    {
                        tieneProductos = true;
                        break;
                    }
                }

                if (tieneProductos)
                {
                    TempData["MensajeError"] = "Acción bloqueada: Esta categoría tiene productos activos en el sistema. Desactívalos o cámbiales la categoría primero.";
                    return RedirectToAction("Index");
                }

                _logicaCategoria.Desactivar(id);
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