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
    // 🔥 EL ESCUDO BASE: Solo Staff Operativo y Auditoría entran aquí
    [Authorize(Roles = "Administrador,Admin,Vendedor,Auditor")]
    public class ProveedorController : Controller
    {
        private readonly Logica_Proveedor _logicaProveedor = new Logica_Proveedor();

        // 1. VISTA PRINCIPAL (LISTADO)
        // Todos los autorizados pueden ver la lista
        public IActionResult Index()
        {
            DataTable dt = _logicaProveedor.ListarProveedores();
            List<Proveedor> listaProveedores = ConvertirDataTableALista(dt);
            return View(listaProveedores);
        }

        // 2. DESACTIVAR PROVEEDOR
        // 🔥 ESCUDO: Auditor NO puede desactivar
        [HttpPost]
        [Authorize(Roles = "Administrador,Admin,Vendedor")]
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
        // 🔥 ESCUDO: Auditor NO puede crear
        [HttpGet]
        [Authorize(Roles = "Administrador,Admin,Vendedor")]
        public IActionResult Crear()
        {
            return View(new Proveedor());
        }

        // --- 4. GUARDAR NUEVO PROVEEDOR (POST) ---
        [HttpPost]
        [Authorize(Roles = "Administrador,Admin,Vendedor")]
        public IActionResult Crear(Proveedor proveedor)
        {
            try
            {
                // 1. Limpieza rápida de espacios locos al inicio y al final
                proveedor.RAZON_SOCIAL = proveedor.RAZON_SOCIAL?.Trim();
                proveedor.NOMBRE_CONTACTO = proveedor.NOMBRE_CONTACTO?.Trim();
                proveedor.TELEFONO = proveedor.TELEFONO?.Trim();
                proveedor.CORREO = proveedor.CORREO?.Trim();
                proveedor.DIRECCION = proveedor.DIRECCION?.Trim();

                // 2. 🔥 EL SÚPER ESCUDO: Barrera Anti-Spam y Validaciones Estrictas

                // Función interna para detectar 5 o más caracteres idénticos seguidos (ej: "aaaaa", "33333")
                bool EsSpam(string texto)
                {
                    if (string.IsNullOrWhiteSpace(texto)) return false;
                    return System.Text.RegularExpressions.Regex.IsMatch(texto, @"(.)\1{4,}");
                }

                // A) Validar campos estrictamente obligatorios
                if (string.IsNullOrWhiteSpace(proveedor.RAZON_SOCIAL) ||
                    string.IsNullOrWhiteSpace(proveedor.TELEFONO) ||
                    string.IsNullOrWhiteSpace(proveedor.DIRECCION))
                {
                    ViewBag.Error = "Los campos Razón Social, Teléfono y Dirección son obligatorios.";
                    return View(proveedor);
                }

                // B) Bloqueo de Spam (Teclas pegadas)
                if (EsSpam(proveedor.RAZON_SOCIAL) || EsSpam(proveedor.NOMBRE_CONTACTO) || EsSpam(proveedor.DIRECCION))
                {
                    ViewBag.Error = "El sistema ha detectado texto sin sentido o repetitivo. Por favor, ingrese datos reales.";
                    return View(proveedor);
                }

                // C) Validar longitudes y formato del Teléfono
                if (proveedor.TELEFONO.Length < 8 || proveedor.TELEFONO.Length > 15)
                {
                    ViewBag.Error = "El Teléfono debe tener entre 8 y 15 caracteres válidos.";
                    return View(proveedor);
                }

                // D) Validar longitud de la Dirección y Razón Social
                if (proveedor.RAZON_SOCIAL.Length > 100)
                {
                    ViewBag.Error = "La Razón Social no puede pasar de 100 caracteres.";
                    return View(proveedor);
                }

                if (proveedor.DIRECCION.Length > 255)
                {
                    ViewBag.Error = "La Dirección no puede pasar de 255 caracteres.";
                    return View(proveedor);
                }

                // E) Validar Correo Electrónico con Lista Blanca (Whitelist)
                if (!string.IsNullOrWhiteSpace(proveedor.CORREO))
                {
                    // Patrón: usuario de 3 a 30 caracteres, arroba, y dominios específicos
                    bool correoValido = System.Text.RegularExpressions.Regex.IsMatch(proveedor.CORREO, @"^[a-zA-Z0-9._%+-]{3,30}@(gmail\.com|hotmail\.com|yahoo\.com|outlook\.com|yunet\.com)$");
                    if (!correoValido)
                    {
                        ViewBag.Error = "El correo ingresado es inválido o usa un dominio no permitido (solo Gmail, Hotmail, Yahoo, Outlook).";
                        return View(proveedor);
                    }
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
        // 🔥 ESCUDO: Auditor NO puede editar
        [HttpGet]
        [Authorize(Roles = "Administrador,Admin,Vendedor")]
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
        [HttpPost]
        [Authorize(Roles = "Administrador,Admin,Vendedor")]
        public IActionResult Editar(Proveedor proveedor)
        {
            try
            {
                // 1. Limpieza rápida de espacios locos al inicio y al final
                proveedor.RAZON_SOCIAL = proveedor.RAZON_SOCIAL?.Trim();
                proveedor.NOMBRE_CONTACTO = proveedor.NOMBRE_CONTACTO?.Trim();
                proveedor.TELEFONO = proveedor.TELEFONO?.Trim();
                proveedor.CORREO = proveedor.CORREO?.Trim();
                proveedor.DIRECCION = proveedor.DIRECCION?.Trim();

                // 2. 🔥 EL SÚPER ESCUDO: Barrera Anti-Spam y Validaciones Estrictas

                // Función interna para detectar 5 o más caracteres idénticos seguidos (ej: "aaaaa", "33333")
                bool EsSpam(string texto)
                {
                    if (string.IsNullOrWhiteSpace(texto)) return false;
                    return System.Text.RegularExpressions.Regex.IsMatch(texto, @"(.)\1{4,}");
                }

                // A) Validar campos estrictamente obligatorios
                if (string.IsNullOrWhiteSpace(proveedor.RAZON_SOCIAL) ||
                    string.IsNullOrWhiteSpace(proveedor.TELEFONO) ||
                    string.IsNullOrWhiteSpace(proveedor.DIRECCION))
                {
                    ViewBag.Error = "Los campos Razón Social, Teléfono y Dirección son obligatorios.";
                    return View(proveedor);
                }

                // B) Bloqueo de Spam (Teclas pegadas)
                if (EsSpam(proveedor.RAZON_SOCIAL) || EsSpam(proveedor.NOMBRE_CONTACTO) || EsSpam(proveedor.DIRECCION))
                {
                    ViewBag.Error = "El sistema ha detectado texto sin sentido o repetitivo. Por favor, ingrese datos reales.";
                    return View(proveedor);
                }

                // C) Validar longitudes y formato del Teléfono
                if (proveedor.TELEFONO.Length < 8 || proveedor.TELEFONO.Length > 15)
                {
                    ViewBag.Error = "El Teléfono debe tener entre 8 y 15 caracteres válidos.";
                    return View(proveedor);
                }

                // D) Validar longitud de la Dirección y Razón Social
                if (proveedor.RAZON_SOCIAL.Length > 100)
                {
                    ViewBag.Error = "La Razón Social no puede pasar de 100 caracteres.";
                    return View(proveedor);
                }

                if (proveedor.DIRECCION.Length > 255)
                {
                    ViewBag.Error = "La Dirección no puede pasar de 255 caracteres.";
                    return View(proveedor);
                }

                // E) Validar Correo Electrónico con Lista Blanca (Whitelist)
                if (!string.IsNullOrWhiteSpace(proveedor.CORREO))
                {
                    // Patrón: usuario de 3 a 30 caracteres, arroba, y dominios específicos
                    bool correoValido = System.Text.RegularExpressions.Regex.IsMatch(proveedor.CORREO, @"^[a-zA-Z0-9._%+-]{3,30}@(gmail\.com|hotmail\.com|yahoo\.com|outlook\.com|yunet\.com)$");
                    if (!correoValido)
                    {
                        ViewBag.Error = "El correo ingresado es inválido o usa un dominio no permitido (solo Gmail, Hotmail, Yahoo, Outlook).";
                        return View(proveedor);
                    }
                }

                // 3. Si todo está "fresh" y pasó la aduana, guardamos en SQL
                bool exito = _logicaProveedor.EditarProveedor(proveedor);

                if (exito)
                {
                    TempData["MensajeExito"] = "Proveedor actualizado con éxito.";
                    return RedirectToAction("Index");
                }
                else
                {
                    ViewBag.Error = "Error interno al intentar guardar los cambios en la base de datos.";
                    return View(proveedor);
                }
            }
            catch (Exception ex)
            {
                ViewBag.Error = "Error inesperado: " + ex.Message;
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