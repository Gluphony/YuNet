using Microsoft.AspNetCore.Mvc;
using LogicaNegocio;
using Entidades;
using System;
using System.Collections.Generic;

namespace Presentacion.Controllers
{
    public class MarcaController : Controller
    {
        private readonly LogicaMarca _logicaMarca = new LogicaMarca();

        [HttpGet]
        public IActionResult Index(string nombre)
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
            return View(lista);
        }

        [HttpPost]
        public IActionResult Guardar(Marca oMarca)
        {
            try
            {
                bool resultado;
                if (oMarca.ID_MARCA == 0) // Es Nueva
                {
                    resultado = _logicaMarca.Crear(oMarca);
                    TempData["Exito"] = "Marca creada correctamente.";
                }
                else // Es Edición
                {
                    resultado = _logicaMarca.Editar(oMarca);
                    TempData["Exito"] = "Marca actualizada correctamente.";
                }

                if (!resultado) throw new Exception("No se pudo completar la operación en la base de datos.");
            }
            catch (Exception ex)
            {
                TempData["Error"] = ex.Message;
            }

            return RedirectToAction("Index");
        }

        [HttpPost]
        public IActionResult Desactivar(int id)
        {
            try
            {
                bool resultado = _logicaMarca.Desactivar(id);
                return Json(new { success = resultado });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }
    }
}