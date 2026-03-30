using Datos;
using Entidades;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LogicaNegocio
{
    public class LogicaProducto
    {
        private readonly DatosProductos _datosProductos = new DatosProductos();

        // 1. LISTAR TODOS
        public DataTable ListarProductos()
        {
            return _datosProductos.ListarProductos();
        }

        // 2. INSERTAR PRODUCTO
        public bool InsertarProducto(Producto producto)
        {
            ValidarReglasNegocioProducto(producto);
            return _datosProductos.InsertarProducto(producto);
        }

        // 3. EDITAR PRODUCTO
        public bool EditarProducto(Producto producto)
        {
            if (producto.ID_PRODUCTO <= 0)
                throw new ArgumentException("El ID del producto a editar no es válido.");

            ValidarReglasNegocioProducto(producto);
            return _datosProductos.EditarProducto(producto);
        }

        // 4. DESACTIVAR PRODUCTO
        public bool DesactivarProducto(int idProducto)
        {
            if (idProducto <= 0)
                throw new ArgumentException("El ID del producto a desactivar no es válido.");

            return _datosProductos.DesactivarProducto(idProducto);
        }
        public List<UnidadExistencia> ListarImeisDisponibles(int idProducto)
        {
            // Podrías poner validaciones aquí si quisieras, 
            // pero como solo es una consulta, lo mandamos directo a la capa de datos.
            return _datosProductos.ListarImeisDisponibles(idProducto);
        }
        // 5. OBTENER IMAGEN
        public byte[] ObtenerImagenProducto(int idProducto)
        {
            if (idProducto <= 0)
                throw new ArgumentException("ID de producto inválido para obtener la imagen.");

            return _datosProductos.ObtenerImagenProducto(idProducto);
        }

        // 6. LISTAR POR CATEGORÍA
        public DataTable ListarProductoPorCategoria(int idCategoria)
        {
            if (idCategoria <= 0)
                throw new ArgumentException("El ID de la categoría no es válido.");

            return _datosProductos.ListarProductoPorCategoria(idCategoria);
        }

        // 7. LISTAR POR FECHAS
        public DataTable ListarProductoPorFecha(DateTime fechaInicio, DateTime fechaFin)
        {
            if (fechaInicio > fechaFin)
                throw new ArgumentException("La fecha de inicio no puede ser posterior a la fecha final.");

            return _datosProductos.ListarProductoPorFecha(fechaInicio, fechaFin);
        }

        // 8. BUSCAR POR NOMBRE
        public DataTable BuscarProductoPorNombre(string textoBusqueda)
        {
            // Sanitizamos la entrada
            string termino = string.IsNullOrWhiteSpace(textoBusqueda) ? "" : textoBusqueda.Trim();
            return _datosProductos.BuscarProductoPorNombre(termino);
        }

        // --- MÉTODO PRIVADO PARA REUTILIZAR VALIDACIONES ---
        private void ValidarReglasNegocioProducto(Producto producto)
        {
            if (producto == null)
                throw new ArgumentNullException(nameof(producto), "Los datos del producto están vacíos.");

            // Validaciones de claves foráneas
            if (producto.ID_MARCA <= 0)
                throw new ArgumentException("El producto debe pertenecer a una Marca válida.");

            // Reglas de negocio críticas para un inventario
            if (producto.PRECIO < 0)
                throw new ArgumentException("El precio del producto no puede ser negativo.");

            if (producto.EXISTENCIAS < 0)
                throw new ArgumentException("Las existencias del producto no pueden ser negativas.");

            // Sanitización opcional de textos
            if (!string.IsNullOrWhiteSpace(producto.MODELO))
                producto.MODELO = producto.MODELO.Trim();

            if (!string.IsNullOrWhiteSpace(producto.DESCRIPCION))
                producto.DESCRIPCION = producto.DESCRIPCION.Trim();
        }
    }
}