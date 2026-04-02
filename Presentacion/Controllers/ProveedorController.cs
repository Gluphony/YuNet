using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Entidades;
using LogicaNegocio;
using System.Data;
using System.Collections.Generic;
using System.Linq;
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
            DataTable dt = _logicaProveedor.ListarProveedores();
            List<Proveedor> listaProveedores = ConvertirDataTableALista(dt);
            return View(listaProveedores);
        }

        // 2. DESACTIVAR PROVEEDOR
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
            return View(new Proveedor());
        }

        // --- 4. GUARDAR NUEVO PROVEEDOR (POST) ---
        [HttpPost]
        public IActionResult Crear(Proveedor proveedor)
        {
            try
            {
                // 1. Limpiamos espacios en blanco accidentales al inicio o final
                proveedor.TELEFONO = proveedor.TELEFONO?.Trim();
                proveedor.DIRECCION = proveedor.DIRECCION?.Trim();

                // 2. 🔥 ESCUDO DE SEGURIDAD: Nulos, Vacíos y Límites de caracteres
                // Validamos el Teléfono (No nulo y máximo 15)
                if (string.IsNullOrWhiteSpace(proveedor.TELEFONO) || proveedor.TELEFONO.Length > 15)
                {
                    ViewBag.Error = "El Teléfono es obligatorio y no puede tener más de 15 caracteres.";
                    return View(proveedor);
                }

                // Validamos la Dirección (No nula y máximo 255)
                if (string.IsNullOrWhiteSpace(proveedor.DIRECCION) || proveedor.DIRECCION.Length > 255)
                {
                    ViewBag.Error = "La Dirección es obligatoria y no puede tener más de 255 caracteres.";
                    return View(proveedor);
                }

                // 3. Si pasó las validaciones, procedemos a la lógica
                bool exito = _logicaProveedor.InsertarProveedor(proveedor);

                if (exito)
                {
                    TempData["MensajeExito"] = "El proveedor se registró correctamente.";
                    return RedirectToAction("Index");
                }
                else
                {
                    ViewBag.Error = "No se pudo guardar el proveedor en la base de datos.";
                    return View(proveedor);
                }
            }
            catch (Exception ex)
            {
                // Aquí atrapamos errores de duplicados o fallos de conexión
                ViewBag.Error = ex.Message;
                return View(proveedor);
            }
        }

        // --- 5. VISTA PARA EDITAR (GET) ---
        [HttpGet]
        public IActionResult Editar(int id)
        {
            if (id <= 0)
            {
                TempData["MensajeError"] = "ID de proveedor no válido.";
                return RedirectToAction("Index");
            }

            DataTable dt = _logicaProveedor.ListarProveedores();
            var listaProveedores = ConvertirDataTableALista(dt);
            var proveedor = listaProveedores.Find(p => p.ID_PROVEEDOR == id);

            if (proveedor == null)
            {
                TempData["MensajeError"] = "No se encontró el proveedor solicitado.";
                return RedirectToAction("Index");
            }

            return View(proveedor);
        }

        // --- 6. GUARDAR CAMBIOS DEL PROVEEDOR (POST) ---
        // --- 6. GUARDAR CAMBIOS DEL PROVEEDOR (POST) ---
        [HttpPost]
        public IActionResult Editar(Proveedor proveedor)
        {
            try
            {
                // 1. Limpieza rápida de espacios locos
                proveedor.TELEFONO = proveedor.TELEFONO?.Trim();
                proveedor.DIRECCION = proveedor.DIRECCION?.Trim();

                // 2. 🔥 EL ESCUDO: Validamos los límites antes de ir a SQL
                if (string.IsNullOrWhiteSpace(proveedor.TELEFONO) || proveedor.TELEFONO.Length > 15)
                {
                    ViewBag.Error = "El Teléfono no puede estar vacío ni pasar de 15 caracteres.";
                    return View(proveedor);
                }

                if (string.IsNullOrWhiteSpace(proveedor.DIRECCION) || proveedor.DIRECCION.Length > 255)
                {
                    ViewBag.Error = "La Dirección no puede estar vacía ni pasar de 255 caracteres.";
                    return View(proveedor);
                }

                // 3. Si todo está "fresh", guardamos
                bool exito = _logicaProveedor.EditarProveedor(proveedor);

                if (exito)
                {
                    TempData["MensajeExito"] = "Proveedor actualizado con éxito.";
                    return RedirectToAction("Index");
                }
                else
                {
                    ViewBag.Error = "Error interno al intentar guardar los cambios.";
                    return View(proveedor);
                }
            }
            catch (Exception ex)
            {
                ViewBag.Error = ex.Message;
                return View(proveedor);
            }
        }

        // --- MÉTODO AUXILIAR ACTUALIZADO ---
        private List<Proveedor> ConvertirDataTableALista(DataTable dt)
        {
            var lista = new List<Proveedor>();
            foreach (DataRow row in dt.Rows)
            {
                lista.Add(new Proveedor
                {
                    ID_PROVEEDOR = Convert.ToInt32(row["ID_PROVEEDOR"]),
                    RAZON_SOCIAL = row["RAZON_SOCIAL"] != DBNull.Value ? row["RAZON_SOCIAL"].ToString() : string.Empty,
                    NOMBRE_CONTACTO = row["NOMBRE_CONTACTO"] != DBNull.Value ? row["NOMBRE_CONTACTO"].ToString() : string.Empty,
                    CORREO = row["CORREO"] != DBNull.Value ? row["CORREO"].ToString() : string.Empty,

                    // 🔥 MAPEAMOS LOS NUEVOS CAMPOS AQUÍ:
                    TELEFONO = row["TELEFONO"] != DBNull.Value ? row["TELEFONO"].ToString() : "N/A",
                    DIRECCION = row["DIRECCION"] != DBNull.Value ? row["DIRECCION"].ToString() : "N/A",

                    ESTADO = row["ESTADO"] != DBNull.Value ? Convert.ToBoolean(row["ESTADO"]) : false
                });
            }
            return lista;
        }
    }
}