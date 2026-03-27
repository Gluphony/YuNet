using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Entidades;
using LogicaNegocio;
using System.Data;
using System.Collections.Generic;
using System;

namespace Presentacion.Controllers
{
    [Authorize(Roles = "Administrador, Admin, Auditor")]
    public class ProveedorController : Controller
    {
        private readonly Logica_Proveedor _logicaProveedor = new Logica_Proveedor();

        // 1. VISTA PRINCIPAL (LISTADO)
        public IActionResult Index()
        {
            // Obtenemos el DataTable de la lógica
            DataTable dt = _logicaProveedor.ListarProveedores();

            // Lo convertimos a Lista para enviarlo a la vista de forma limpia
            List<Proveedor> listaProveedores = ConvertirDataTableALista(dt);

            return View(listaProveedores);
        }

        // 2. DESACTIVAR PROVEEDOR (Llamado desde un botón en la tabla)
        [HttpPost]
        public IActionResult Desactivar(int id)
        {
            try
            {
                _logicaProveedor.DesactivarProveedor(id);
                TempData["MensajeExito"] = "Proveedor desactivado correctamente.";
            }
            catch (Exception ex)
            {
                TempData["MensajeError"] = "Error al desactivar: " + ex.Message;
            }
            return RedirectToAction("Index");
        }
        // --- 3. VISTA PARA CREAR (GET) ---
        [HttpGet]
        public IActionResult Crear()
        {
            // Enviamos un objeto vacío para que la vista lo use como modelo base
            return View(new Proveedor());
        }

        // --- 4. GUARDAR NUEVO PROVEEDOR (POST) ---
        [HttpPost]
        public IActionResult Crear(Proveedor proveedor)
        {
            try
            {
                // Como nuestra capa lógica ya tiene el método ValidarReglasNegocioProveedor,
                // si hay algún error (ej. sin razón social), lanzará una Excepción que atraparemos abajo.

                bool exito = _logicaProveedor.InsertarProveedor(proveedor);

                if (exito)
                {
                    TempData["MensajeExito"] = "El proveedor se registró correctamente.";
                    return RedirectToAction("Index"); // Volvemos a la lista si todo salió bien
                }
                else
                {
                    ViewBag.Error = "No se pudo guardar el proveedor en la base de datos.";
                    return View(proveedor); // Devolvemos la vista con los datos que el usuario ya había escrito
                }
            }
            catch (Exception ex)
            {
                // Aquí capturamos los errores de tu lógica (ej. "Debe especificar al menos la Razón Social")
                ViewBag.Error = ex.Message;
                return View(proveedor);
            }
        }// --- 5. VISTA PARA EDITAR (GET) ---
        [HttpGet]
        public IActionResult Editar(int id)
        {
            if (id <= 0)
            {
                TempData["MensajeError"] = "ID de proveedor no válido.";
                return RedirectToAction("Index");
            }

            // 1. Traemos todos los proveedores (o podrías crear un SP que traiga solo uno por ID en el futuro)
            DataTable dt = _logicaProveedor.ListarProveedores();
            var listaProveedores = ConvertirDataTableALista(dt);

            // 2. Buscamos el proveedor exacto usando LINQ
            var proveedor = listaProveedores.Find(p => p.ID_PROVEEDOR == id);

            if (proveedor == null)
            {
                TempData["MensajeError"] = "No se encontró el proveedor solicitado.";
                return RedirectToAction("Index");
            }

            // 3. Enviamos el proveedor encontrado a la vista
            return View(proveedor);
        }

        // --- 6. GUARDAR CAMBIOS DEL PROVEEDOR (POST) ---
        [HttpPost]
        public IActionResult Editar(Proveedor proveedor)
        {
            try
            {
                bool exito = _logicaProveedor.EditarProveedor(proveedor);

                if (exito)
                {
                    TempData["MensajeExito"] = "El proveedor se actualizó correctamente.";
                    return RedirectToAction("Index");
                }
                else
                {
                    ViewBag.Error = "No se pudieron guardar los cambios en la base de datos.";
                    return View(proveedor);
                }
            }
            catch (Exception ex)
            {
                // Captura validaciones de negocio (ej. dejó la razón social en blanco)
                ViewBag.Error = ex.Message;
                return View(proveedor);
            }
        }

        // --- MÉTODO AUXILIAR PARA MAPEAR DATATABLE A LISTA ---
        private List<Proveedor> ConvertirDataTableALista(DataTable dt)
        {
            var lista = new List<Proveedor>();
            foreach (DataRow row in dt.Rows)
            {
                lista.Add(new Proveedor
                {
                    // Ajusta estos nombres ("ID_PROVEEDOR", etc.) a como vengan de tu SP exacto
                    ID_PROVEEDOR = Convert.ToInt32(row["ID_PROVEEDOR"]),
                    RAZON_SOCIAL = row["RAZON_SOCIAL"] != DBNull.Value ? row["RAZON_SOCIAL"].ToString() : string.Empty,
                    NOMBRE_CONTACTO = row["NOMBRE_CONTACTO"] != DBNull.Value ? row["NOMBRE_CONTACTO"].ToString() : string.Empty,
                    CORREO = row["CORREO"] != DBNull.Value ? row["CORREO"].ToString() : string.Empty,

                    // Si tienes más campos como teléfono o estado, agrégalos aquí. Ejemplo:
                    // TELEFONO = row["TELEFONO"] != DBNull.Value ? row["TELEFONO"].ToString() : string.Empty,
                    ESTADO = row["ESTADO"] != DBNull.Value ? Convert.ToBoolean(row["ESTADO"]) : false
                });
            }
            return lista;
        }
    }
}
