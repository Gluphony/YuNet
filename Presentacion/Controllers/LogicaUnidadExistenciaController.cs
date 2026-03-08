using Microsoft.AspNetCore.Mvc;
using Entidades;
using LogicaNegocio;
using System;
using System.Data;

namespace Presentacion.Controllers
{
    public class UnidadExistenciaController : Controller
    {
        // Instanciamos la capa de negocio
        private readonly LogicaUnidadExistencia _logicaUnidad = new LogicaUnidadExistencia();

        // --- 1. ACCIÓN PARA MOSTRAR LA VISTA Y BUSCAR POR PRODUCTO ---
        // Le pasamos idProducto = 0 por defecto para que al abrir la pantalla no falle.
        [HttpGet]
        public IActionResult Index(int idProducto = 0)
        {
            ViewBag.IdProducto = idProducto;
            DataTable dtUnidades = new DataTable();

            try
            {
                // Si el usuario buscó un producto en específico, traemos sus unidades
                if (idProducto > 0)
                {
                    dtUnidades = _logicaUnidad.ListarUnidadesDisponibles(idProducto);
                }
            }
            catch (Exception ex)
            {
                ViewBag.Error = ex.Message;
            }

            // Nota: Aquí le pasamos un DataTable a la vista en lugar de una Lista
            return View(dtUnidades);
        }

        // --- 2. ACCIÓN PARA INSERTAR (Llamada desde AJAX/Fetch) ---
        [HttpPost]
        public IActionResult Insertar([FromBody] UnidadExistencia unidad)
        {
            try
            {
                // Llamamos a tu lógica que ya tiene todas las validaciones (Trim, nulos, etc.)
                bool resultado = _logicaUnidad.InsertarUnidadExistencia(unidad);

                if (resultado)
                {
                    return Json(new { exito = true, mensaje = "Unidad/IMEI registrada correctamente." });
                }

                return Json(new { exito = false, mensaje = "No se pudo registrar la unidad en la base de datos." });
            }
            catch (ArgumentException ex) // ¡Aquí atrapamos TUS validaciones del IF!
            {
                return Json(new { exito = false, mensaje = ex.Message });
            }
            catch (Exception ex)
            {
                return Json(new { exito = false, mensaje = "Error del sistema: " + ex.Message });
            }
        }
    }
}