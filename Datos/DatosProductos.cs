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

                    // Validamos los nulos y vacíos de forma segura
                    cmd.Parameters.AddWithValue("@Id_Marca", producto.ID_MARCA);
                    cmd.Parameters.AddWithValue("@Id_Categoria", producto.ID_CATEGORIA > 0 ? (object)producto.ID_CATEGORIA : DBNull.Value);
                    cmd.Parameters.AddWithValue("@Modelo", string.IsNullOrWhiteSpace(producto.MODELO) ? DBNull.Value : (object)producto.MODELO);
                    cmd.Parameters.AddWithValue("@Descripcion", string.IsNullOrWhiteSpace(producto.DESCRIPCION) ? DBNull.Value : (object)producto.DESCRIPCION);
                    cmd.Parameters.AddWithValue("@Caracteristicas", string.IsNullOrWhiteSpace(producto.CARACTERISTICAS) ? DBNull.Value : (object)producto.CARACTERISTICAS);
                    cmd.Parameters.AddWithValue("@Precio", producto.PRECIO);
                    cmd.Parameters.AddWithValue("@Existencias", producto.EXISTENCIAS);

                    // --- SOLUCIÓN PARA LA IMAGEN ---
                    SqlParameter paramImagen = new SqlParameter("@Imagen", SqlDbType.VarBinary, -1);
                    if (producto.IMAGEN != null && producto.IMAGEN.Length > 0)
                    {
                        paramImagen.Value = producto.IMAGEN;
                    }
                    else
                    {
                        paramImagen.Value = DBNull.Value;
                    }
                    cmd.Parameters.Add(paramImagen);

                    conexion.Open();
                    cmd.ExecuteNonQuery();
                    respuesta = true;
                }
                catch (Exception ex)
                {
                    throw new Exception("Error al insertar: " + ex.Message);
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
                    cmd.Parameters.AddWithValue("@Id_Categoria", producto.ID_CATEGORIA > 0 ? (object)producto.ID_CATEGORIA : DBNull.Value);
                    cmd.Parameters.AddWithValue("@Modelo", string.IsNullOrWhiteSpace(producto.MODELO) ? DBNull.Value : (object)producto.MODELO);
                    cmd.Parameters.AddWithValue("@Descripcion", string.IsNullOrWhiteSpace(producto.DESCRIPCION) ? DBNull.Value : (object)producto.DESCRIPCION);
                    cmd.Parameters.AddWithValue("@Caracteristicas", string.IsNullOrWhiteSpace(producto.CARACTERISTICAS) ? DBNull.Value : (object)producto.CARACTERISTICAS);
                    cmd.Parameters.AddWithValue("@Precio", producto.PRECIO);

                    // ¡AQUÍ ESTÁ LA LÍNEA QUE FALTABA!
                    cmd.Parameters.AddWithValue("@Existencias", producto.EXISTENCIAS);

                    // --- SOLUCIÓN PARA LA IMAGEN EN EDICIÓN ---
                    SqlParameter paramImagen = new SqlParameter("@Imagen", SqlDbType.VarBinary, -1);
                    if (producto.IMAGEN != null && producto.IMAGEN.Length > 0)
                    {
                        paramImagen.Value = producto.IMAGEN;
                    }
                    else
                    {
                        paramImagen.Value = DBNull.Value; // Manda nulo real a la BD
                    }
                    cmd.Parameters.Add(paramImagen);

                    cmd.Parameters.AddWithValue("@Estado", producto.ESTADO);

                    conexion.Open();
                    cmd.ExecuteNonQuery();
                    respuesta = true;
                }
                catch (Exception ex)
                {
                    throw new Exception("Error al editar: " + ex.Message);
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
                    object resultado = cmd.ExecuteScalar();

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

        public List<UnidadExistencia> ListarImeisDisponibles(int idProducto)
        {
            List<UnidadExistencia> lista = new List<UnidadExistencia>();
            try
            {
                using (SqlConnection conexion = new SqlConnection(_con.GetCadena()))
                {
                    SqlCommand cmd = new SqlCommand("Inventario.Sp_ListarImeisDisponibles", conexion);
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@Id_Producto", idProducto);

                    conexion.Open();
                    using (SqlDataReader dr = cmd.ExecuteReader())
                    {
                        while (dr.Read())
                        {
                            lista.Add(new UnidadExistencia
                            {
                                ID_UNIDAD = Convert.ToInt32(dr["ID_UNIDAD"]),
                                ID_PRODUCTO = Convert.ToInt32(dr["ID_PRODUCTO"]),
                                IMEI = dr["IMEI"].ToString(),
                                ESTADO_FISICO = dr["ESTADO_FISICO"].ToString(),
                                FECHA_INGRESO = Convert.ToDateTime(dr["FECHA_INGRESO"]),
                                VENDIDO = Convert.ToBoolean(dr["VENDIDO"])
                            });
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Error al obtener IMEIs: " + ex.Message);
            }
            return lista;
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