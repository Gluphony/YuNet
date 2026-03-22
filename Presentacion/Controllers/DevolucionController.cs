using Microsoft.AspNetCore.Mvc;
using Entidades;
using LogicaNegocio;
using System;
using System.Collections.Generic;

namespace Presentacion.Controllers
{
    public class DevolucionController : Controller
    {
        private readonly LogicaDevolucion _logicaDevolucion = new LogicaDevolucion();

        // --- 1. ACCIÓN PARA MOSTRAR LA LISTA DE DEVOLUCIONES ---
        [HttpGet]
        public IActionResult Index()
        {
            try
            {
                List<Devolucion> devoluciones = _logicaDevolucion.Listar();
                return View(devoluciones);
            }
            catch (Exception ex)
            {
                ViewBag.Error = ex.Message;
                return View(new List<Devolucion>());
            }
        }

        // ========================================================
        // NUEVO: GET: Devolucion/Create (Muestra el formulario)
        // ========================================================
        [HttpGet]
        public IActionResult Create()
        {
            // Mandamos una entidad en blanco a la vista
            return View(new Devolucion());
        }

        // ========================================================
        // NUEVO: POST: Devolucion/Create (Guarda los datos)
        // ========================================================
        [HttpPost]
        [ValidateAntiForgeryToken] // Seguridad contra ataques CSRF
        public IActionResult Create(Devolucion nuevaDevolucion)
        {
            try
            {
                // Limpiamos la validación del ID por si la BD lo genera automáticamente
                ModelState.Remove("ID_DEVOLUCION");

                if (ModelState.IsValid)
                {
                    // Llamamos a tu capa lógica para guardar
                    // Asumo que tienes un método Crear() en tu LogicaDevolucion
                    _logicaDevolucion.Crear(nuevaDevolucion);

                    TempData["MensajeExito"] = "¡La devolución se registró correctamente!";
                    return RedirectToAction("Index");
                }

                // Si hay errores de validación, recargamos la vista con los datos que escribió el usuario
                ViewBag.Error = "Por favor, revisa los campos del formulario.";
                return View(nuevaDevolucion);
            }
            catch (Exception ex)
            {
                ViewBag.Error = "Error al registrar la devolución: " + ex.Message;
                return View(nuevaDevolucion);
            }
        }
    }
}