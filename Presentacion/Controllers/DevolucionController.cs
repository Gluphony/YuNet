using Microsoft.AspNetCore.Mvc;
using Entidades;
using LogicaNegocio;
using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Authorization;
using System.Linq;
using System.IO;
using Microsoft.AspNetCore.Http;
using System.Threading.Tasks;

namespace Presentacion.Controllers
{
    // 🔥 EL ESCUDO BASE: Dejamos entrar al Auditor para que pueda ver y sacar PDFs
    [Authorize(Roles = "Administrador,Admin,Vendedor,Auditor")]
    public class DevolucionController : Controller
    {
        private readonly LogicaDevolucion _logicaDevolucion = new LogicaDevolucion();

        // ========================================================
        // 1. GET: MUESTRA LA LISTA DE DEVOLUCIONES 
        // ========================================================
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
        // 2. POST: El cliente envía su reporte 
        // ========================================================
        [HttpPost]
        [AllowAnonymous] // El cliente (incluso sin login si así lo deseas) puede mandar su solicitud
        public async Task<IActionResult> Create(int ID_DETALLE_PEDIDO, string MOTIVO_CLIENTE, IFormFile EVIDENCIA_IMAGEN)
        {
            try
            {
                if (ID_DETALLE_PEDIDO <= 0)
                {
                    return Json(new { exito = false, mensaje = "ID de producto inválido." });
                }

                string rutaImagenBd = null;

                if (EVIDENCIA_IMAGEN != null && EVIDENCIA_IMAGEN.Length > 0)
                {
                    string carpetaDestino = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "imagenes", "evidencias");

                    if (!Directory.Exists(carpetaDestino))
                    {
                        Directory.CreateDirectory(carpetaDestino);
                    }

                    string extension = Path.GetExtension(EVIDENCIA_IMAGEN.FileName);
                    string nombreArchivo = $"Evidencia_{ID_DETALLE_PEDIDO}_{Guid.NewGuid()}{extension}";
                    string rutaFisicaCompleta = Path.Combine(carpetaDestino, nombreArchivo);

                    using (var stream = new FileStream(rutaFisicaCompleta, FileMode.Create))
                    {
                        await EVIDENCIA_IMAGEN.CopyToAsync(stream);
                    }

                    rutaImagenBd = $"/imagenes/evidencias/{nombreArchivo}";
                }

                bool respuesta = _logicaDevolucion.Crear(ID_DETALLE_PEDIDO, MOTIVO_CLIENTE, rutaImagenBd, out string mensajeRespuesta);

                return Json(new { exito = respuesta, mensaje = mensajeRespuesta });
            }
            catch (Exception ex)
            {
                return Json(new { exito = false, mensaje = "Error inesperado: " + ex.Message });
            }
        }

        // ========================================================
        // 3. POST: El Admin/Vendedor procesa el reporte (Aprobar/Rechazar)
        // ========================================================
        [HttpPost]
        // 🔥 EL ESCUDO ESTRICTO: ¡El Auditor NO puede procesar devoluciones!
        [Authorize(Roles = "Administrador,Admin,Vendedor")]
        public IActionResult Procesar(Entidades.DevolucionProcesar modelo)
        {
            try
            {
                var listaDevoluciones = _logicaDevolucion.Listar();
                var devOriginal = listaDevoluciones.FirstOrDefault(d => d.ID_DEVOLUCION == modelo.ID_DEVOLUCION);

                if (devOriginal == null)
                {
                    TempData["MensajeError"] = "No se encontró el registro original del reporte.";
                    return RedirectToAction("Index");
                }

                string imeiAfectado = "";
                string motivo = devOriginal.MOTIVO_CLIENTE ?? "";

                if (motivo.StartsWith("[Unidad: "))
                {
                    int finCorchete = motivo.IndexOf(']');
                    if (finCorchete > 0)
                    {
                        imeiAfectado = motivo.Substring(9, finCorchete - 9).Trim();
                    }
                }
                else
                {
                    imeiAfectado = devOriginal.IMEI?.Split(',')[0].Trim() ?? "";
                }

                decimal montoReembolso = 0;

                bool respuesta = _logicaDevolucion.ProcesarReporte(
                    modelo.ID_DEVOLUCION,
                    modelo.ESTADO_REPORTE,
                    modelo.ESTADO_DISPOSITIVO_RECIBIDO,
                    modelo.ACCION_TOMADA,
                    modelo.REGRESAR_INVENTARIO,
                    imeiAfectado,
                    montoReembolso
                );

                if (respuesta)
                {
                    TempData["MensajeExito"] = $"Reporte #{modelo.ID_DEVOLUCION} procesado exitosamente como {modelo.ESTADO_REPORTE}.";
                }
                else
                {
                    TempData["MensajeError"] = "Ocurrió un problema al intentar procesar la devolución en la base de datos.";
                }

                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                TempData["MensajeError"] = "Error al procesar: " + ex.Message;
                return RedirectToAction("Index");
            }
        }

        // ========================================================
        // 4. GET: Detalles de la Devolución (SOLO LECTURA)
        // ========================================================
        [HttpGet]
        public IActionResult Details(int id)
        {
            try
            {
                var devolucion = _logicaDevolucion.Listar().FirstOrDefault(d => d.ID_DEVOLUCION == id);

                if (devolucion == null)
                {
                    TempData["MensajeError"] = "No se encontró la devolución solicitada.";
                    return RedirectToAction("Index");
                }

                return View(devolucion);
            }
            catch (Exception ex)
            {
                TempData["MensajeError"] = "Error al cargar los detalles: " + ex.Message;
                return RedirectToAction("Index");
            }
        }

        // ========================================================
        // 5. GET: DOCUMENTOS PDF (Garantía, NC, Cambio)
        // (El auditor sí puede ver esto por el candado de la clase)
        // ========================================================

        [HttpGet]
        public IActionResult OrdenServicio(int id)
        {
            var devolucion = _logicaDevolucion.Listar().FirstOrDefault(d => d.ID_DEVOLUCION == id);
            if (devolucion == null) return RedirectToAction("Index");

            string nombreAdmin = User.Claims.FirstOrDefault(c => c.Type == System.Security.Claims.ClaimTypes.Name)?.Value
                                 ?? User.Identity.Name ?? "Christhian Ortiz";
            devolucion.AUTORIZADO_POR_NOMBRE = nombreAdmin;

            try
            {
                var _logicaPedido = new LogicaPedido();
                var pedido = _logicaPedido.Listar().FirstOrDefault(p => p.ID_PEDIDO == devolucion.FACTURA_ORIGINAL);

                if (pedido != null)
                {
                    devolucion.CLIENTE = pedido.CLIENTE;

                    var _logicaUsuario = new LogicaUsuario();
                    var dtUsuarios = _logicaUsuario.ListarUsuarios();

                    foreach (System.Data.DataRow row in dtUsuarios.Rows)
                    {
                        string nombreBd = row.Table.Columns.Contains("NOMBRE") ? row["NOMBRE"].ToString() : "";
                        int idBd = row.Table.Columns.Contains("ID_USUARIO") && row["ID_USUARIO"] != DBNull.Value ? Convert.ToInt32(row["ID_USUARIO"]) : 0;

                        if (idBd == pedido.ID_USUARIO || nombreBd == pedido.CLIENTE)
                        {
                            if (row.Table.Columns.Contains("CORREO"))
                                devolucion.CORREO_CLIENTE = row["CORREO"].ToString();
                            else if (row.Table.Columns.Contains("Correo"))
                                devolucion.CORREO_CLIENTE = row["Correo"].ToString();

                            break;
                        }
                    }

                    if (string.IsNullOrEmpty(devolucion.CORREO_CLIENTE))
                        devolucion.CORREO_CLIENTE = "Sin correo en base de datos";
                }
            }
            catch (Exception ex)
            {
                devolucion.CORREO_CLIENTE = "Error: " + ex.Message;
            }

            return View(devolucion);
        }

        [HttpGet]
        public IActionResult NotaCredito(int id)
        {
            var devolucion = _logicaDevolucion.Listar().FirstOrDefault(d => d.ID_DEVOLUCION == id);
            if (devolucion == null) return RedirectToAction("Index");

            string nombreAdmin = User.Claims.FirstOrDefault(c => c.Type == System.Security.Claims.ClaimTypes.Name)?.Value ?? User.Identity.Name ?? "Christhian Ortiz";
            devolucion.AUTORIZADO_POR_NOMBRE = nombreAdmin;

            try
            {
                var _logicaPedido = new LogicaPedido();
                var pedido = _logicaPedido.Listar().FirstOrDefault(p => p.ID_PEDIDO == devolucion.FACTURA_ORIGINAL);
                if (pedido != null)
                {
                    devolucion.CLIENTE = pedido.CLIENTE;
                    devolucion.MONTO_REEMBOLSO = pedido.TOTAL;

                    var _logicaUsuario = new LogicaUsuario();
                    var dtUsuarios = _logicaUsuario.ListarUsuarios();
                    foreach (System.Data.DataRow row in dtUsuarios.Rows)
                    {
                        string nombreBd = row.Table.Columns.Contains("NOMBRE") ? row["NOMBRE"].ToString() : "";
                        int idBd = row.Table.Columns.Contains("ID_USUARIO") && row["ID_USUARIO"] != DBNull.Value ? Convert.ToInt32(row["ID_USUARIO"]) : 0;

                        if (idBd == pedido.ID_USUARIO || nombreBd == pedido.CLIENTE)
                        {
                            if (row.Table.Columns.Contains("CORREO")) devolucion.CORREO_CLIENTE = row["CORREO"].ToString();
                            else if (row.Table.Columns.Contains("Correo")) devolucion.CORREO_CLIENTE = row["Correo"].ToString();
                            break;
                        }
                    }
                    if (string.IsNullOrEmpty(devolucion.CORREO_CLIENTE)) devolucion.CORREO_CLIENTE = "Sin correo en base de datos";
                }
            }
            catch (Exception ex) { devolucion.CORREO_CLIENTE = "Error: " + ex.Message; }

            return View(devolucion);
        }

        [HttpGet]
        public IActionResult CambioEquipo(int id)
        {
            var devolucion = _logicaDevolucion.Listar().FirstOrDefault(d => d.ID_DEVOLUCION == id);
            if (devolucion == null) return RedirectToAction("Index");

            string nombreAdmin = User.Claims.FirstOrDefault(c => c.Type == System.Security.Claims.ClaimTypes.Name)?.Value ?? User.Identity.Name ?? "Christhian Ortiz";
            devolucion.AUTORIZADO_POR_NOMBRE = nombreAdmin;

            try
            {
                var _logicaPedido = new LogicaPedido();
                var pedido = _logicaPedido.Listar().FirstOrDefault(p => p.ID_PEDIDO == devolucion.FACTURA_ORIGINAL);
                if (pedido != null)
                {
                    devolucion.CLIENTE = pedido.CLIENTE;
                    var _logicaUsuario = new LogicaUsuario();
                    var dtUsuarios = _logicaUsuario.ListarUsuarios();
                    foreach (System.Data.DataRow row in dtUsuarios.Rows)
                    {
                        string nombreBd = row.Table.Columns.Contains("NOMBRE") ? row["NOMBRE"].ToString() : "";
                        int idBd = row.Table.Columns.Contains("ID_USUARIO") && row["ID_USUARIO"] != DBNull.Value ? Convert.ToInt32(row["ID_USUARIO"]) : 0;

                        if (idBd == pedido.ID_USUARIO || nombreBd == pedido.CLIENTE)
                        {
                            if (row.Table.Columns.Contains("CORREO")) devolucion.CORREO_CLIENTE = row["CORREO"].ToString();
                            else if (row.Table.Columns.Contains("Correo")) devolucion.CORREO_CLIENTE = row["Correo"].ToString();
                            break;
                        }
                    }
                    if (string.IsNullOrEmpty(devolucion.CORREO_CLIENTE)) devolucion.CORREO_CLIENTE = "Sin correo en base de datos";
                }
            }
            catch (Exception ex) { devolucion.CORREO_CLIENTE = "Error: " + ex.Message; }

            return View(devolucion);
        }
    }
}