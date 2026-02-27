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
    public class DataDetallePedido
    {
        private readonly Conexion _con = new Conexion();

        // 1. INSERTAR DETALLE (Procesa Venta, Stock, Kárdex e IMEI)
        public bool InsertarDetalle(DetallePedido oDetalle, int idUsuarioSesion)
        {
            bool respuesta = false;
            using (SqlConnection connection = new SqlConnection(_con.GetCadena()))
            {
                using (SqlCommand cmd = new SqlCommand("Sp_InsertarDetallePedido", connection))
                {
                    cmd.CommandType = CommandType.StoredProcedure;

                    // Parámetros del SP
                    cmd.Parameters.AddWithValue("@Id_Pedido", oDetalle.IdPedido);
                    cmd.Parameters.AddWithValue("@Id_Producto", oDetalle.IdProducto);
                    // Manejamos el NULL si es un accesorio sin IMEI
                    cmd.Parameters.AddWithValue("@Imei_Vendido", (object)oDetalle.ImeiVendido ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@Cantidad", oDetalle.Cantidad);
                    cmd.Parameters.AddWithValue("@Precio_Unitario", oDetalle.PrecioUnitario);
                    cmd.Parameters.AddWithValue("@Id_Usuario", idUsuarioSesion);

                    try
                    {
                        connection.Open();
                        // Si el SP falla por stock o IMEI, devuelve 0 filas afectadas
                        respuesta = cmd.ExecuteNonQuery() > 0;
                    }
                    catch (Exception) { respuesta = false; }
                }
            }
            return respuesta;
        }

        // 2. LISTAR TICKET (Para imprimir la factura/comprobante)
        public List<DetallePedido> ListarTicket(int idPedido)
        {
            var olista = new List<DetallePedido>();
            using (SqlConnection connection = new SqlConnection(_con.GetCadena()))
            {
                using (SqlCommand cmd = new SqlCommand("Sp_ListarTicketPedido", connection))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@Id_Pedido", idPedido);

                    try
                    {
                        connection.Open();
                        using (SqlDataReader reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                olista.Add(new DetallePedido()
                                {
                                    IdDetallePedido = Convert.ToInt32(reader["Id_Detalle_Pedido"]),
                                    Producto = reader["Producto"].ToString(),
                                    ImeiVendido = reader["Numero_Serie_IMEI"].ToString(),
                                    Cantidad = Convert.ToInt32(reader["Cantidad"]),
                                    PrecioUnitario = Convert.ToDecimal(reader["Precio_Unitario"])
                                    // Subtotal se calcula solo en el Model
                                });
                            }
                        }
                    }
                    catch (Exception) { /* Log error */ }
                }
            }
            return olista;
        }
    }
}

