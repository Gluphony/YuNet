using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Entidades;
using LogicaNegocio;
using System;
using System.Collections.Generic;

namespace Presentacion.Controllers
{
    public class DevolucionController : Controller
    {
        private readonly LogicaDevolucion _LogicaDevolucion = new LogicaDevolucion();
        private readonly LogicaUsuario _LogicaUsuario = new LogicaUsuario();
        private readonly LogicaDetallePedido _LogicaDetallePedido = new LogicaDetallePedido();

        [HttpGet]
        public IActionResult Index()
        {
            try
            {
                List<Devolucion> devoluciones = _LogicaDevolucion.Listar();
                return View(devoluciones);
            }
            catch (Exception ex)
            {
                ViewBag.Error = ex.Message;
                return View(new List<Devolucion>());
            }
        }

        [HttpGet]
        public IActionResult Create()
        {
            CargarListasDesplegables();
            return View(new Devolucion());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(Devolucion nuevaDevolucion)
        {
            try
            {
                ModelState.Remove("ID_DEVOLUCION");
                ModelState.Remove("FECHA_DEVOLUCION");
                ModelState.Remove("FACTURA_ORIGINAL");
                ModelState.Remove("PRODUCTO_NOMBRE");
                ModelState.Remove("IMEI");
                ModelState.Remove("AUTORIZADO_POR_NOMBRE");

                if (ModelState.IsValid)
                {
                    bool creado = _LogicaDevolucion.Crear(nuevaDevolucion);
                    if (creado)
                    {
                        TempData["MensajeExito"] = "¡La devolución se registró correctamente!";
                        return RedirectToAction("Index");
                    }
                }

                CargarListasDesplegables();
                ViewBag.Error = "Por favor, revisa los campos marcados en el formulario.";
                return View(nuevaDevolucion);
            }
            catch (Exception ex)
            {
                CargarListasDesplegables();
                ViewBag.Error = "Error al registrar la devolución: " + ex.Message;
                return View(nuevaDevolucion);
            }
        }

        // ====== AQUÍ ESTÁ LA MAGIA PARA EVITAR LOS ERRORES ======
        private void CargarListasDesplegables()
        {
            // --- 1. CARGAR USUARIOS DESDE EL DATATABLE ---
            var dtUsuarios = _LogicaUsuario.ListarUsuarios();
            var listaUsuarios = new List<SelectListItem>();

            // Recorremos las filas del DataTable para armar la lista
            if (dtUsuarios != null && dtUsuarios.Rows.Count > 0)
            {
                foreach (System.Data.DataRow row in dtUsuarios.Rows)
                {
                    listaUsuarios.Add(new SelectListItem
                    {
                        // OJO: Si la columna se llama diferente en BD, cámbialo aquí
                        Value = row["ID_USUARIO"].ToString(),
                        Text = row["NOMBRE"].ToString()
                    });
                }
            }

            ViewBag.ListaUsuarios = new SelectList(listaUsuarios, "Value", "Text");

            // --- 2. CARGAR DETALLES ---
            // Como LogicaDetallePedido no tiene un método general para listar, 
            // mandamos una lista vacía para que no explote la vista.
            ViewBag.ListaDetalles = new SelectList(new List<SelectListItem>());
        }
    }
}