using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authentication; // Necesario para SignOutAsync
using System.Security.Claims; // Necesario para leer el usuario actual
using System.Threading.Tasks; // Necesario para métodos asíncronos
using LogicaNegocio;
using System;
using System.Data;

namespace Presentacion.Controllers
{
    // Solo el administrador gestiona usuarios
    [Authorize(Roles = "Administrador")]
    public class UsuariosController : Controller
    {
        private readonly LogicaUsuario _logicaUsuario = new LogicaUsuario();

        // --- MÉTODOS AUXILIARES ---

        // Obtiene el ID del usuario que tiene la sesión iniciada
        private int ObtenerIdUsuarioActual()
        {
            // Asume que al hacer Login guardaste el ID en ClaimTypes.NameIdentifier
            var claimId = User.FindFirst(ClaimTypes.NameIdentifier);
            if (claimId != null && int.TryParse(claimId.Value, out int id))
            {
                return id;
            }
            return 0;
        }

        // Verifica si el usuario que intentamos desactivar es el ÚLTIMO administrador activo
        private bool EsElUltimoAdministradorActivo(int idUsuarioAfectado)
        {
            DataTable tablaUsuarios = _logicaUsuario.ListarUsuarios();
            int cantidadAdminsActivos = 0;
            bool esElUsuarioAfectadoAdmin = false;

            foreach (DataRow fila in tablaUsuarios.Rows)
            {
                bool estado = fila["Estado"] != DBNull.Value && Convert.ToBoolean(fila["Estado"]);
                int idRol = fila["Id_Rol"] != DBNull.Value ? Convert.ToInt32(fila["Id_Rol"]) : 0;
                int idUser = Convert.ToInt32(fila["Id_Usuario"]);

                // Contamos administradores (Rol 1) que estén activos
                if (estado && idRol == 1)
                {
                    cantidadAdminsActivos++;
                    if (idUser == idUsuarioAfectado)
                    {
                        esElUsuarioAfectadoAdmin = true;
                    }
                }
            }

            // Si el usuario es admin y el conteo total es 1 o menos, significa que es el último
            return esElUsuarioAfectadoAdmin && cantidadAdminsActivos <= 1;
        }
        // 1. Mostrar la vista
        [HttpGet]
        public IActionResult RestablecerClave()
        {
            return View();
        }

        // 2. Procesar el formulario
        [HttpPost]
        public IActionResult RestablecerClave(string Correo, string NuevaClave)
        {
            try
            {
                LogicaUsuario _logica = new LogicaUsuario();
                bool exito = _logica.RestablecerClave(Correo, NuevaClave);

                if (exito)
                {
                    TempData["Exito"] = "¡Contraseña actualizada! Ya puedes iniciar sesión.";
                    return RedirectToAction("Index"); // Lo mandamos de vuelta al Login
                }
                else
                {
                    TempData["Error"] = "El correo ingresado no pertenece a ningún usuario activo.";
                }
            }
            catch (Exception ex)
            {
                TempData["Error"] = ex.Message;
            }

            // Si falla, lo devolvemos a la misma pantalla de restablecer
            return View();
        }

        // --- ACCIONES DEL CONTROLADOR ---

        //  VISTA PRINCIPAL
        [HttpGet]
        public IActionResult Index()
        {
            // Enviamos el ID del usuario actual a la vista (ViewBag) para que oculte los botones de autodesactivación
            ViewBag.IdUsuarioActual = ObtenerIdUsuarioActual();

            DataTable tablaUsuarios = new DataTable();
            try
            {
                tablaUsuarios = _logicaUsuario.ListarUsuarios();
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Error al cargar los usuarios: " + ex.Message;
            }

            return View(tablaUsuarios);
        }

        // 2. CREAR USUARIO
        [HttpPost]
        [ValidateAntiForgeryToken] // Seguridad CSRF
        public IActionResult Crear(string Nombre, string Correo, string Clave, int Id_Rol)
        {
            try
            {
                bool respuesta = _logicaUsuario.InsertarUsuario(Nombre, Correo, Clave, Id_Rol);
                if (respuesta)
                    TempData["Exito"] = "Usuario registrado correctamente.";
            }
            catch (Exception ex)
            {
                TempData["Error"] = ex.Message;
            }
            return RedirectToAction("Index");
        }

        // 3. EDITAR USUARIO
        [HttpPost]
        public IActionResult Editar(int Id_Usuario, string Nombre, string Correo, int Id_Rol, string Estado)
        {
            try
            {
                // ==========================================
                // 1. OBTENER EL CARNET (ID DEL USUARIO LOGUEADO)
                // ==========================================
                // Dependiendo de tu Login, descomenta la opción que uses:

                // OPCIÓN A (Si usas Identity o Autenticación por Cookies/Claims):
                // int idLogueado = int.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier).Value);

                // OPCIÓN B (Si usas Variables de Sesión tradicionales):
                // int idLogueado = HttpContext.Session.GetInt32("IdUsuario") ?? 0;

                // -> Pon aquí tu variable real:
                int idLogueado = 1; // (Borra este 1 y usa tu Opción A o B)

                // ==========================================
                // 2. CONVERSIÓN Y LLAMADA A LA LÓGICA
                // ==========================================
                bool boolEstado = (Estado == "1" || Estado == "true" || Estado == "True");

                // ¡Aquí le pasamos el idLogueado al final!
                bool respuesta = _logicaUsuario.EditarUsuario(Id_Usuario, Nombre, Correo, Id_Rol, boolEstado, idLogueado);

                if (respuesta)
                    TempData["Exito"] = "Usuario actualizado correctamente.";
            }
            catch (Exception ex)
            {
                // Esto atrapará el RAISERROR de SQL ("No puedes desactivarte a ti mismo") 
                // y lo mostrará en tu vista gracias al TempData.
                TempData["Error"] = ex.Message;
            }

            return RedirectToAction("Index");
        }

        // 4. DESACTIVAR USUARIO
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Desactivar(int Id_Usuario)
        {
            try
            {
                int idActual = ObtenerIdUsuarioActual();

                // REGLA 1: Evitar desactivar al último administrador
                if (EsElUltimoAdministradorActivo(Id_Usuario))
                {
                    TempData["Error"] = "Operación denegada: No puedes desactivar a este usuario porque es el único Administrador activo del sistema.";
                    return RedirectToAction("Index");
                }

                // <-- AQUÍ ESTÁ LA MAGIA: Le pasamos idActual a la lógica
                bool respuesta = _logicaUsuario.DesactivarUsuario(Id_Usuario, idActual);

                if (respuesta)
                {
                    // REGLA 2: Si el usuario se desactivó a sí mismo a la fuerza, lo sacamos de la app
                    if (Id_Usuario == idActual)
                    {
                        await HttpContext.SignOutAsync();
                        return RedirectToAction("Index", "Login");
                    }

                    TempData["Exito"] = "Usuario desactivado correctamente.";
                }
            }
            catch (Exception ex)
            {
                TempData["Error"] = ex.Message;
            }
            return RedirectToAction("Index");
        }
    }
}