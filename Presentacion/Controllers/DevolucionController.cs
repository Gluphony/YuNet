using Microsoft.AspNetCore.Mvc;
using Entidades;
using LogicaNegocio;
using System;
using System.Collections.Generic;
using System.Security.Claims; // Necesario para obtener el ID de usuario de la sesión

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
        // Recibe el ID del detalle del pedido a devolver
        // ========================================================
        [HttpGet]
        public IActionResult Create(int? idDetalle)
        {
            var modelo = new Devolucion();

            if (idDetalle.HasValue)
            {
                // Asignamos el ID que viene desde el botón "Devolver" de la vista del pedido
                modelo.ID_DETALLE_DEVOLUCION = idDetalle.Value;
            }
            else
            {
                // Si el usuario entra directamente a /Devolucion/Create sin pasar por un pedido
                ViewBag.Error = "Atención: No se ha seleccionado un producto específico para devolver.";
            }

            return View(modelo);
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
                // 1. Asignar el usuario que autoriza automáticamente desde la sesión activa
                var claimId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (!string.IsNullOrEmpty(claimId))
                {
                    nuevaDevolucion.ID_USUARIO_AUTORIZA = int.Parse(claimId);
                }
                else
                {
                    // Fallback de seguridad: Si por alguna razón no hay sesión o claims, 
                    // asignamos el ID 1 (Asegúrate de que el usuario con ID 1 exista en tu base de datos)
                    nuevaDevolucion.ID_USUARIO_AUTORIZA = 1;
                }

                // 2. Limpiamos validaciones de campos que el usuario no llena a mano
                ModelState.Remove("ID_DEVOLUCION");
                ModelState.Remove("ID_USUARIO_AUTORIZA");

                if (ModelState.IsValid)
                {
                    // Llamamos a tu capa lógica para guardar
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