using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization; // <-- NECESARIO PARA LA SEGURIDAD
using Entidades;
using LogicaNegocio;
using System;
using System.Collections.Generic;

namespace Presentacion.Controllers
{
    // 🔥 EL ESCUDO BASE: Dejamos entrar a todo el Staff para que puedan ver los métodos
    [Authorize(Roles = "Administrador,Admin,Auditor,Vendedor")]
    public class MetodoPagoController : Controller
    {
        private readonly LogicaMetodoPago _logicaMetodoPago = new LogicaMetodoPago();

        // --- 1. MOSTRAR LA VISTA CON LA LISTA COMPLETA ---
        [HttpGet]
        public IActionResult Index()
        {
            try
            {
                // Usamos el método Admin para ver tanto los activos como los inactivos
                List<MetodoPago> lista = _logicaMetodoPago.ListarMetodosPago_Admin();
                return View(lista);
            }
            catch (Exception ex)
            {
                ViewBag.Error = "Error al cargar los métodos de pago: " + ex.Message;
                return View(new List<MetodoPago>());
            }
        }

        // --- 2. GUARDAR (INSERTAR O EDITAR) ---
        [HttpPost]
        // 🔥 EL ESCUDO ESTRICTO: El Auditor NO puede crear ni editar métodos de pago
        [Authorize(Roles = "Administrador,Admin,Vendedor")]
        public IActionResult Guardar([FromBody] MetodoPago metodo)
        {
            try
            {
                bool resultado = false;
                string mensaje = "";

                // Si el ID es 0, es uno nuevo. Si tiene ID, lo estamos editando.
                if (metodo.ID_METODO_PAGO == 0)
                {
                    resultado = _logicaMetodoPago.InsertarMetodoPago(metodo);
                    mensaje = "Método de pago registrado correctamente.";
                }
                else
                {
                    resultado = _logicaMetodoPago.EditarMetodoPago(metodo);
                    mensaje = "Método de pago actualizado correctamente.";
                }

                if (resultado)
                    return Json(new { exito = true, mensaje = mensaje });
                else
                    return Json(new { exito = false, mensaje = "No se pudo realizar la operación en la base de datos." });
            }
            catch (ArgumentException ex) // Atrapamos tus validaciones de negocio
            {
                return Json(new { exito = false, mensaje = ex.Message });
            }
            catch (Exception ex)
            {
                return Json(new { exito = false, mensaje = "Error del sistema: " + ex.Message });
            }
        }

        // --- 3. DESACTIVAR ---
        [HttpPost]
        // 🔥 EL ESCUDO ESTRICTO: El Auditor NO puede desactivar métodos de pago
        [Authorize(Roles = "Administrador,Admin,Vendedor")]
        public IActionResult Desactivar([FromBody] int idMetodoPago)
        {
            try
            {
                bool resultado = _logicaMetodoPago.DesactivarMetodoPago(idMetodoPago);

                if (resultado)
                    return Json(new { exito = true, mensaje = "Método de pago desactivado." });
                else
                    return Json(new { exito = false, mensaje = "No se pudo desactivar el método de pago." });
            }
            catch (ArgumentException ex)
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