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
                using (SqlCommand cmd = new SqlCommand("Venta.Sp_InsertarDetallePedido", connection))
                {
                    cmd.CommandType = CommandType.StoredProcedure;

                    // Parámetros del SP
                    cmd.Parameters.AddWithValue("@Id_Pedido", oDetalle.ID_PEDIDO);
                    cmd.Parameters.AddWithValue("@Id_Producto", oDetalle.ID_PRODUCTO);
                    // Manejamos el NULL si es un accesorio sin IMEI
                    cmd.Parameters.AddWithValue("@Imei_Vendido", (object)oDetalle.IMEI_VENDIDO ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@Cantidad", oDetalle.CANTIDAD);
                    cmd.Parameters.AddWithValue("@Precio_Unitario", oDetalle.PRECIO_UNITARIO);
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
        // ==============================================================================
        // NUEVO: ASIGNAR IMEI EN DESPACHO
        // ==============================================================================
        public bool AsignarImeiDespacho(int idDetallePedido, string imei)
        {
            bool respuesta = false;
            using (SqlConnection conexion = new SqlConnection(_con.GetCadena()))
            {
                try
                {
                    using (SqlCommand cmd = new SqlCommand("Venta.Sp_AsignarImei_Despacho", conexion))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Parameters.AddWithValue("@Id_Detalle_Pedido", idDetallePedido);
                        cmd.Parameters.AddWithValue("@Imei", imei);

                        conexion.Open();
                        // Ejecutamos la asignación
                        respuesta = cmd.ExecuteNonQuery() > 0;
                    }
                }
                catch (SqlException sqlEx)
                {
                    // Atrapa los THROW del SQL (ej. "El IMEI ya fue vendido")
                    throw new Exception(sqlEx.Message);
                }
                catch (Exception ex)
                {
                    throw new Exception("Error al conectar con la base de datos: " + ex.Message);
                }
            }
            return respuesta;
        }
        // 2. LISTAR TICKET (Para imprimir la factura/comprobante)
        // 2. LISTAR TICKET (Para imprimir la factura/comprobante)
        public List<DetallePedido> ListarTicket(int idPedido)
        {
            var olista = new List<DetallePedido>();
            using (SqlConnection connection = new SqlConnection(_con.GetCadena()))
            {
                using (SqlCommand cmd = new SqlCommand("Venta.Sp_ListarTicketPedido", connection))
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
                                // 1. Armamos el objeto base
                                var detalle = new DetallePedido()
                                {
                                    ID_DETALLE_PEDIDO = Convert.ToInt32(reader["Id_Detalle_Pedido"]),
                                    PRODUCTO = reader["Producto"].ToString(),
                                    IMEI_VENDIDO = reader["Numero_Serie_IMEI"].ToString(),
                                    CANTIDAD = Convert.ToInt32(reader["Cantidad"]),
                                    PRECIO_UNITARIO = Convert.ToDecimal(reader["Precio_Unitario"])
                                };

                                // 2. Lógica para saber si ya hay un reporte (Devolución)
                                if (reader["ID_DEVOLUCION"] != DBNull.Value)
                                {
                                    detalle.Tiene_Devolucion = true;
                                    detalle.Id_Devolucion_Vinculada = Convert.ToInt32(reader["ID_DEVOLUCION"]);
                                    detalle.ESTADO_REPORTE = reader["ESTADO_REPORTE"] != DBNull.Value ? reader["ESTADO_REPORTE"].ToString() : "Pendiente";
                                }
                                else
                                {
                                    // No hay reporte todavía
                                    detalle.Tiene_Devolucion = false;
                                    detalle.Id_Devolucion_Vinculada = null;
                                    detalle.ESTADO_REPORTE = null;
                                }

                                // 3. Lo agregamos a la lista final
                                olista.Add(detalle);
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