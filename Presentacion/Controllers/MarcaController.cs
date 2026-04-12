using Entidades;
using LogicaNegocio;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization; // <-- NECESARIO PARA LA SEGURIDAD
using Presentacion.Reportes;
using QuestPDF.Fluent;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Data; // <-- NUEVO: Necesario para leer la tabla de productos

namespace Presentacion.Controllers
{
    // 🔥 EL ESCUDO BASE: Solo el Staff puede entrar al módulo de Marcas
    [Authorize(Roles = "Administrador,Admin,Auditor,Vendedor")]
    public class MarcaController : Controller
    {
        private readonly LogicaMarca _logicaMarca = new LogicaMarca();
        private readonly LogicaProducto _logicaProducto = new LogicaProducto(); // <-- NUEVO: El guardia que vigila los productos

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
        // 🔥 EL ESCUDO ESTRICTO: Auditor NO puede crear marcas
        [Authorize(Roles = "Administrador,Admin,Vendedor")]
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
        // 🔥 EL ESCUDO ESTRICTO: Auditor NO puede editar marcas
        [Authorize(Roles = "Administrador,Admin,Vendedor")]
        public IActionResult Editar(Marca marca)
        {
            try
            {
                // 🔥 EL ESCUDO DE INTEGRIDAD: Si intentan ponerla inactiva desde la edición...
                if (!marca.ESTADO)
                {
                    DataTable dtProductos = _logicaProducto.ListarProductos();
                    bool tieneProductos = false;

                    foreach (DataRow row in dtProductos.Rows)
                    {
                        int idMarcaProducto = row["ID_MARCA"] != DBNull.Value ? Convert.ToInt32(row["ID_MARCA"]) : 0;
                        bool estadoProducto = row["ESTADO"] != DBNull.Value && Convert.ToBoolean(row["ESTADO"]);

                        if (idMarcaProducto == marca.ID_MARCA && estadoProducto)
                        {
                            tieneProductos = true;
                            break;
                        }
                    }

                    if (tieneProductos)
                    {
                        TempData["MensajeError"] = "Bloqueado: No puedes cambiar el estado a Inactivo porque existen productos activos de esta marca.";
                        return RedirectToAction("Index");
                    }
                }

                bool resultado = _logicaMarca.Editar(marca);
                if (resultado)
                {
                    TempData["MensajeExito"] = "Marca actualizada correctamente.";
                }
            }
            catch (Exception ex)
            {
                TempData["MensajeError"] = ex.Message;
            }

            return RedirectToAction("Index");
        }

        // 5. DESACTIVAR MARCA
        [HttpPost]
        // 🔥 EL ESCUDO ESTRICTO: Auditor NO puede desactivar marcas
        [Authorize(Roles = "Administrador,Admin,Vendedor")]
        public IActionResult Desactivar(int id)
        {
            try
            {
                // 🔥 EL ESCUDO DE INTEGRIDAD: Revisar si hay productos vivos atados a esta marca
                DataTable dtProductos = _logicaProducto.ListarProductos();
                bool tieneProductos = false;

                foreach (DataRow row in dtProductos.Rows)
                {
                    int idMarcaProducto = row["ID_MARCA"] != DBNull.Value ? Convert.ToInt32(row["ID_MARCA"]) : 0;
                    bool estadoProducto = row["ESTADO"] != DBNull.Value && Convert.ToBoolean(row["ESTADO"]);

                    if (idMarcaProducto == id && estadoProducto)
                    {
                        tieneProductos = true;
                        break;
                    }
                }

                if (tieneProductos)
                {
                    TempData["MensajeError"] = "Acción bloqueada: Esta marca tiene productos activos en el sistema. Desactívalos o cámbiales la marca primero.";
                    return RedirectToAction("Index");
                }

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
                TempData["MensajeError"] = "Atención: " + ex.Message;
            }

            return RedirectToAction("Index");
        }

        // 6. EXPORTAR PDF (El Auditor SÍ puede hacer esto por el escudo base)
        [HttpGet]
        public IActionResult DescargarPdf(string busqueda)
        {
            try
            {
                List<Marca> lista = string.IsNullOrEmpty(busqueda)
                                    ? _logicaMarca.Listar().ToList()
                                    : _logicaMarca.FiltrarPorNombre(busqueda).ToList();

                if (lista == null) lista = new List<Marca>();

                var documento = new MarcaReporte(lista);

                byte[] pdfBytes = documento.GeneratePdf();

                var stream = new MemoryStream(pdfBytes);

                return File(stream, "application/pdf");
            }
            catch (Exception ex)
            {
                return Content("Error al generar PDF: " + ex.Message);
            }
        }
    }
}