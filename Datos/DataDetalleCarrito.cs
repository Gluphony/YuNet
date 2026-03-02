using Entidades;
using Microsoft.Data.SqlClient;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Datos
{
    public class DataDetalleCarrito
    {
        private readonly Conexion _con = new Conexion();

        // 1. LISTAR DETALLE (Aquí aplicamos lo que discutimos)
        public List<DetalleCarrito> Listar(int idCarrito)
        {
            var olista = new List<DetalleCarrito>();
            using (SqlConnection connection = new SqlConnection(_con.GetCadena()))
            {
                using (SqlCommand cmd = new SqlCommand("Sp_ListarDetalleCarrito", connection))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@Id_Carrito", idCarrito);

                    try
                    {
                        connection.Open();
                        using (SqlDataReader reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                olista.Add(new DetalleCarrito()
                                {
                                    ID_DETALLE = Convert.ToInt32(reader["Id_Detalle"]),
                                    ID_PRODUCTO = Convert.ToInt32(reader["Id_Producto"]),
                                    // "Producto" es el alias que pusiste en el SELECT de tu SP
                                    NOMBRE_PRODUCTO = reader["Producto"].ToString(),
                                    CANTIDAD = Convert.ToInt32(reader["Cantidad"]),
                                    PRECIO_UNITARIO = Convert.ToDecimal(reader["Precio_Unitario"])

                                });
                            }
                        }
                    }
                    catch (Exception) { /* Manejo de logs */ }
                }
            }
            return olista;
        }

        // 2. AGREGAR AL CARRITO
        public bool Agregar(int idCarrito, int idProducto, int cantidad)
        {
            bool respuesta = false;
            using (SqlConnection connection = new SqlConnection(_con.GetCadena()))
            {
                using (SqlCommand cmd = new SqlCommand("Sp_AgregarAlCarrito", connection))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@Id_Carrito", idCarrito);
                    cmd.Parameters.AddWithValue("@Id_Producto", idProducto);
                    cmd.Parameters.AddWithValue("@Cantidad", cantidad);

                    try
                    {
                        connection.Open();
                        respuesta = cmd.ExecuteNonQuery() > 0;
                    }
                    catch (Exception) { respuesta = false; }
                }
            }
            return respuesta;
        }

        // 3. ELIMINAR UN ÍTEM
        public bool Eliminar(int idDetalle)
        {
            bool respuesta = false;
            using (SqlConnection connection = new SqlConnection(_con.GetCadena()))
            {
                using (SqlCommand cmd = new SqlCommand("Sp_EliminarDetalleCarrito", connection))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@Id_Detalle", idDetalle);

                    try
                    {
                        connection.Open();
                        respuesta = cmd.ExecuteNonQuery() > 0;
                    }
                    catch (Exception) { respuesta = false; }
                }
            }
            return respuesta;
        }
    }
}