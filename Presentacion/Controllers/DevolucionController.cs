using Microsoft.AspNetCore.Mvc;
using Entidades;
using LogicaNegocio;
using System;
using System.Collections.Generic;

namespace Presentacion.Controllers
{
    public class DevolucionController : Controller
    {
        // Instanciamos la capa de negocio
        private readonly LogicaDevolucion _logicaDevolucion = new LogicaDevolucion();

        // --- 1. ACCIÓN PARA MOSTRAR LA LISTA DE DEVOLUCIONES ---
        [HttpGet]
        public IActionResult Index()
        {
            try
            {
                // Pedimos los datos a la capa de negocio
                List<Devolucion> devoluciones = _logicaDevolucion.Listar();

                // Le pasamos la lista a la vista "Index.cshtml"
                return View(devoluciones);
            }
            catch (Exception ex)
            {
                // Si la capa de negocio "tira" un error (el throw new Exception que pusiste),
                // lo atrapamos aquí y lo mandamos a la vista usando ViewBag.
                ViewBag.Error = ex.Message;

                // Retornamos una lista vacía para que la vista no colapse al hacer el @foreach
                return View(new List<Devolucion>());
            }
        }
    }
}