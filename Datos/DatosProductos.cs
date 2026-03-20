using System;
using System.Data;
using Microsoft.Data.SqlClient;
using Entidades;

namespace Datos
{
    public class DatosProductos
    {
        private readonly Conexion _con = new Conexion();


        public DataTable ListarProductos()
        {
            DataTable tabla = new DataTable(); 
            using (SqlConnection conexion = new SqlConnection(_con.GetCadena()))
            {
                try
                {
                    SqlCommand cmd = new SqlCommand("Inventario.Sp_ListarProducto", conexion);
                    cmd.CommandType = CommandType.StoredProcedure;

                    SqlDataAdapter da = new SqlDataAdapter(cmd);
                    da.Fill(tabla);
                }
                catch (Exception ex)
                {
                    throw new Exception("Error al listar los productos: " + ex.Message);
                }
            }
            return tabla;
        }

        public bool InsertarProducto(Producto producto)
        {
            bool respuesta = false;
            using (SqlConnection conexion = new SqlConnection(_con.GetCadena()))
            {
                try
                {
                    SqlCommand cmd = new SqlCommand("Inventario.Sp_InsertarProducto", conexion);
                    cmd.CommandType = CommandType.StoredProcedure;

                    cmd.Parameters.AddWithValue("@Id_Marca", producto.ID_MARCA);
                    cmd.Parameters.AddWithValue("@Id_Categoria", producto.ID_CATEGORIA.HasValue ? (object)producto.ID_CATEGORIA.Value : DBNull.Value);
                    cmd.Parameters.AddWithValue("@Modelo", producto.MODELO ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@Descripcion", producto.DESCRIPCION ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@Caracteristicas", producto.CARACTERISTICAS ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@Precio", producto.PRECIO);
                    cmd.Parameters.AddWithValue("@Existencias", producto.EXISTENCIAS);
                    cmd.Parameters.AddWithValue("@Imagen", producto.IMAGEN != null ? (object)producto.IMAGEN : DBNull.Value);

                    conexion.Open();
                    cmd.ExecuteNonQuery();
                    respuesta = true;
                }
                catch (Exception ex)
                {
                    throw new Exception("Error al insertar el producto: " + ex.Message);
                }
            }
            return respuesta;
        }

        public bool EditarProducto(Producto producto)
        {
            bool respuesta = false;
            using (SqlConnection conexion = new SqlConnection(_con.GetCadena()))
            {
                try
                {
                    SqlCommand cmd = new SqlCommand("Inventario.Sp_EditarProducto", conexion);
                    cmd.CommandType = CommandType.StoredProcedure;

                    cmd.Parameters.AddWithValue("@Id_Producto", producto.ID_PRODUCTO);
                    cmd.Parameters.AddWithValue("@Id_Marca", producto.ID_MARCA);
                    cmd.Parameters.AddWithValue("@Id_Categoria", producto.ID_CATEGORIA.HasValue ? (object)producto.ID_CATEGORIA.Value : DBNull.Value);
                    cmd.Parameters.AddWithValue("@Modelo", producto.MODELO ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@Descripcion", producto.DESCRIPCION ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@Caracteristicas", producto.CARACTERISTICAS ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@Precio", producto.PRECIO);
                    cmd.Parameters.AddWithValue("@Imagen", producto.IMAGEN != null ? (object)producto.IMAGEN : DBNull.Value);
                    cmd.Parameters.AddWithValue("@Estado", producto.ESTADO);

                    conexion.Open();
                    cmd.ExecuteNonQuery();
                    respuesta = true;
                }
                catch (Exception ex)
                {
                    throw new Exception("Error al editar el producto: " + ex.Message);
                }
            }
            return respuesta;
        }

        public bool DesactivarProducto(int idProducto)
        {
            bool respuesta = false;
            using (SqlConnection conexion = new SqlConnection(_con.GetCadena()))
            {
                try
                {
                    SqlCommand cmd = new SqlCommand("Inventario.Sp_DesactivarProducto", conexion);
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@Id_Producto", idProducto);

                    conexion.Open();
                    cmd.ExecuteNonQuery();
                    respuesta = true;
                }
                catch (Exception ex)
                {
                    throw new Exception("Error al desactivar el producto: " + ex.Message);
                }
            }
            return respuesta;
        }

        public byte[] ObtenerImagenProducto(int idProducto)
        {
            byte[] imagen = null;
            using (SqlConnection conexion = new SqlConnection(_con.GetCadena()))
            {
                try
                {
                    SqlCommand cmd = new SqlCommand("Inventario.Sp_ObtenerImagenProducto", conexion);
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@Id_Producto", idProducto);

                    conexion.Open();
                    object resultado = cmd.ExecuteScalar(); // ExecuteScalar es ideal para obtener un solo valor

                    if (resultado != null && resultado != DBNull.Value)
                    {
                        imagen = (byte[])resultado;
                    }
                }
                catch (Exception ex)
                {
                    throw new Exception("Error al obtener la imagen: " + ex.Message);
                }
            }
            return imagen;
        }

        public DataTable ListarProductoPorCategoria(int idCategoria)
        {
            DataTable tabla = new DataTable();
            using (SqlConnection conexion = new SqlConnection(_con.GetCadena()))
            {
                try
                {
                    SqlCommand cmd = new SqlCommand("Inventario.Sp_ListarProductoPorCategoria", conexion);
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@Id_Categoria", idCategoria);

                    SqlDataAdapter da = new SqlDataAdapter(cmd);
                    da.Fill(tabla);
                }
                catch (Exception ex)
                {
                    throw new Exception("Error al filtrar por categoría: " + ex.Message);
                }
            }
            return tabla;
        }

        public DataTable ListarProductoPorFecha(DateTime fechaInicio, DateTime fechaFin)
        {
            DataTable tabla = new DataTable();
            using (SqlConnection conexion = new SqlConnection(_con.GetCadena()))
            {
                try
                {
                    SqlCommand cmd = new SqlCommand("Inventario.Sp_ListarProductoPorFecha", conexion);
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@FechaInicio", fechaInicio.Date);
                    cmd.Parameters.AddWithValue("@FechaFin", fechaFin.Date);

                    SqlDataAdapter da = new SqlDataAdapter(cmd);
                    da.Fill(tabla);
                }
                catch (Exception ex)
                {
                    throw new Exception("Error al filtrar por fechas: " + ex.Message);
                }
            }
            return tabla;
        }

        public DataTable BuscarProductoPorNombre(string textoBusqueda)
        {
            DataTable tabla = new DataTable();
            using (SqlConnection conexion = new SqlConnection(_con.GetCadena()))
            {
                try
                {
                    SqlCommand cmd = new SqlCommand("Inventario.Sp_BuscarProductoPorNombre", conexion);
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@TextoBusqueda", textoBusqueda);

                    SqlDataAdapter da = new SqlDataAdapter(cmd);
                    da.Fill(tabla);
                }
                catch (Exception ex)
                {
                    throw new Exception("Error al buscar el producto: " + ex.Message);
                }
            }
            return tabla;
        }
    }
}