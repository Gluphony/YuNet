using Datos;
using Entidades;
using Microsoft.Data.SqlClient;
using System;
using System.Collections.Generic;
using System.Data;

namespace CapaDatos
{
    public class DatosPedidos
    {
        private readonly Conexion _con = new Conexion();

        // ==============================================================================
        // 1. PROCESAR COMPRA TRANSACCIONAL (NUEVO MÉTODO)
        // Crea el pedido (usando OUTPUT) y sus detalles en una sola transacción segura.
        // ==============================================================================
        public int ProcesarCompraTransaccional(int idUsuario, int? idMetodoPago, decimal total, List<DetalleCarrito> detallesCarrito)
        {
            int nuevoIdPedido = 0;

            try
            {
                using (SqlConnection conexion = new SqlConnection(_con.GetCadena()))
                {
                    conexion.Open();

                    // Iniciamos la transacción para asegurar que si algo falla, no se guarde nada a medias
                    using (SqlTransaction transaccion = conexion.BeginTransaction())
                    {
                        try
                        {
                            // A. Crear el Encabezado del Pedido
                            // A. Crear el Encabezado del Pedido
                            using (SqlCommand cmd = new SqlCommand("Venta.Sp_CrearPedido", conexion, transaccion))
                            {
                                cmd.CommandType = CommandType.StoredProcedure;
                                cmd.Parameters.AddWithValue("@Id_Usuario", idUsuario);
                                cmd.Parameters.AddWithValue("@Id_Metodo_Pago", (object)idMetodoPago ?? DBNull.Value);
                                cmd.Parameters.AddWithValue("@Total", total);
                                cmd.Parameters.AddWithValue("@Estado", "PENDIENTE"); // Estado inicial

                                // Usamos ExecuteScalar para capturar el ID que devuelve el SELECT de tu Stored Procedure
                                object result = cmd.ExecuteScalar();

                                if (result != null && result != DBNull.Value)
                                {
                                    nuevoIdPedido = Convert.ToInt32(result);
                                }
                                else
                                {
                                    throw new Exception("El procedimiento almacenado no devolvió el ID del pedido.");
                                }
                            }

                            // B. Insertar los Detalles del Pedido
                            foreach (var item in detallesCarrito)
                            {
                                using (SqlCommand cmdDetalle = new SqlCommand("Venta.Sp_InsertarDetallePedido", conexion, transaccion))
                                {
                                    cmdDetalle.CommandType = CommandType.StoredProcedure;
                                    cmdDetalle.Parameters.AddWithValue("@Id_Pedido", nuevoIdPedido);
                                    cmdDetalle.Parameters.AddWithValue("@Id_Producto", item.ID_PRODUCTO);
                                    cmdDetalle.Parameters.AddWithValue("@Imei_Vendido", DBNull.Value); // Se asignará en almacén
                                    cmdDetalle.Parameters.AddWithValue("@Cantidad", item.CANTIDAD);
                                    cmdDetalle.Parameters.AddWithValue("@Precio_Unitario", item.PRECIO_UNITARIO);
                                    cmdDetalle.Parameters.AddWithValue("@Id_Usuario", idUsuario); // Para Kárdex

                                    cmdDetalle.ExecuteNonQuery();
                                }
                            }

                            // C. Confirmar la transacción si todo salió bien
                            transaccion.Commit();
                        }
                        catch (Exception innerEx)
                        {
                            // Si algo falla, deshacemos todo lo que intentamos insertar
                            transaccion.Rollback();
                            throw new Exception("Error durante la transacción de compra: " + innerEx.Message);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Error de conexión al procesar la compra: " + ex.Message);
            }

            return nuevoIdPedido;
        }

        // ==============================================================================
        // MÉTODOS EXISTENTES (INTACTOS)
        // ==============================================================================

        // 2. LISTAR TODOS LOS PEDIDOS
        public List<Pedido> Listar()
        {
            List<Pedido> lista = new List<Pedido>();
            try
            {
                using (SqlConnection conexion = new SqlConnection(_con.GetCadena()))
                {
                    SqlCommand cmd = new SqlCommand("Venta.Sp_ListarPedidos", conexion);
                    cmd.CommandType = CommandType.StoredProcedure;
                    conexion.Open();

                    using (SqlDataReader dr = cmd.ExecuteReader())
                    {
                        while (dr.Read())
                        {
                            Pedido pedido = new Pedido();
                            pedido.ID_PEDIDO = Convert.ToInt32(dr["Id_Pedido"]);
                            pedido.FECHA_PEDIDO = Convert.ToDateTime(dr["Fecha_Pedido"]);
                            pedido.TOTAL = Convert.ToDecimal(dr["Total"]);
                            pedido.ESTADO = dr["Estado"].ToString();
                            pedido.CLIENTE = dr["Cliente"] != DBNull.Value ? dr["Cliente"].ToString() : "Desconocido";
                            pedido.METODO_PAGO_NOMBRE = dr["Metodo_Pago"] != DBNull.Value ? dr["Metodo_Pago"].ToString() : "N/A";
                            lista.Add(pedido);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Error al listar los pedidos: " + ex.Message);
            }
            return lista;
        }

        // 3. ACTUALIZAR ESTADO DEL PEDIDO
        public bool ActualizarEstado(int idPedido, string nuevoEstado)
        {
            bool respuesta = false;
            try
            {
                using (SqlConnection conexion = new SqlConnection(_con.GetCadena()))
                {
                    SqlCommand cmd = new SqlCommand("Venta.Sp_ActualizarEstadoPedido", conexion);
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@Id_Pedido", idPedido);
                    cmd.Parameters.AddWithValue("@NuevoEstado", nuevoEstado);

                    conexion.Open();
                    int filasAfectadas = cmd.ExecuteNonQuery();
                    if (filasAfectadas > 0) respuesta = true;
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Error al actualizar el estado del pedido: " + ex.Message);
            }
            return respuesta;
        }

        // 4. LISTAR PEDIDOS POR USUARIO
        public List<Pedido> ListarPorUsuario(int idUsuario)
        {
            List<Pedido> lista = new List<Pedido>();
            try
            {
                using (SqlConnection conexion = new SqlConnection(_con.GetCadena()))
                {
                    SqlCommand cmd = new SqlCommand("Venta.Sp_ListarPedidosPorUsuario", conexion);
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@Id_Usuario", idUsuario);

                    conexion.Open();
                    using (SqlDataReader dr = cmd.ExecuteReader())
                    {
                        while (dr.Read())
                        {
                            Pedido pedido = new Pedido();
                            pedido.ID_PEDIDO = Convert.ToInt32(dr["Id_Pedido"]);
                            pedido.FECHA_PEDIDO = Convert.ToDateTime(dr["Fecha_Pedido"]);
                            pedido.TOTAL = Convert.ToDecimal(dr["Total"]);
                            pedido.ESTADO = dr["Estado"].ToString();
                            pedido.CLIENTE = dr["Cliente"] != DBNull.Value ? dr["Cliente"].ToString() : "Desconocido";
                            pedido.METODO_PAGO_NOMBRE = dr["Metodo_Pago"] != DBNull.Value ? dr["Metodo_Pago"].ToString() : "N/A";
                            lista.Add(pedido);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Error al listar los pedidos del usuario: " + ex.Message);
            }
            return lista;
        }

        // 5. LISTAR PEDIDOS POR RANGO DE FECHAS
        public List<Pedido> ListarPorFecha(DateTime fechaInicio, DateTime fechaFin)
        {
            List<Pedido> lista = new List<Pedido>();
            try
            {
                using (SqlConnection conexion = new SqlConnection(_con.GetCadena()))
                {
                    SqlCommand cmd = new SqlCommand("Venta.Sp_ListarPedidosPorFecha", conexion);
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@FechaInicio", fechaInicio);
                    cmd.Parameters.AddWithValue("@FechaFin", fechaFin);

                    conexion.Open();
                    using (SqlDataReader dr = cmd.ExecuteReader())
                    {
                        while (dr.Read())
                        {
                            Pedido pedido = new Pedido();
                            pedido.ID_PEDIDO = Convert.ToInt32(dr["Id_Pedido"]);
                            pedido.FECHA_PEDIDO = Convert.ToDateTime(dr["Fecha_Pedido"]);
                            pedido.TOTAL = Convert.ToDecimal(dr["Total"]);
                            pedido.ESTADO = dr["Estado"].ToString();
                            pedido.CLIENTE = dr["Cliente"] != DBNull.Value ? dr["Cliente"].ToString() : "Desconocido";
                            pedido.METODO_PAGO_NOMBRE = dr["Metodo_Pago"] != DBNull.Value ? dr["Metodo_Pago"].ToString() : "N/A";
                            lista.Add(pedido);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Error al filtrar pedidos por fecha: " + ex.Message);
            }
            return lista;
        }

        // 6. LISTAR PEDIDOS POR ESTADO
        public List<Pedido> ListarPorEstado(string estado)
        {
            List<Pedido> lista = new List<Pedido>();
            try
            {
                using (SqlConnection conexion = new SqlConnection(_con.GetCadena()))
                {
                    SqlCommand cmd = new SqlCommand("Venta.Sp_ListarPedidosPorEstado", conexion);
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@Estado", estado);

                    conexion.Open();
                    using (SqlDataReader dr = cmd.ExecuteReader())
                    {
                        while (dr.Read())
                        {
                            Pedido pedido = new Pedido();
                            pedido.ID_PEDIDO = Convert.ToInt32(dr["Id_Pedido"]);
                            pedido.FECHA_PEDIDO = Convert.ToDateTime(dr["Fecha_Pedido"]);
                            pedido.TOTAL = Convert.ToDecimal(dr["Total"]);
                            pedido.ESTADO = dr["Estado"].ToString();
                            pedido.CLIENTE = dr["Cliente"] != DBNull.Value ? dr["Cliente"].ToString() : "Desconocido";
                            pedido.METODO_PAGO_NOMBRE = dr["Metodo_Pago"] != DBNull.Value ? dr["Metodo_Pago"].ToString() : "N/A";
                            lista.Add(pedido);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Error al filtrar pedidos por estado: " + ex.Message);
            }
            return lista;
        }

        // 7. OBTENER PEDIDO POR ID (Para los Detalles)
        public Pedido ObtenerPorId(int idPedido)
        {
            Pedido pedido = null;
            try
            {
                using (SqlConnection conexion = new SqlConnection(_con.GetCadena()))
                {
                    SqlCommand cmd = new SqlCommand("Venta.Sp_ObtenerPedidoPorId", conexion);
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@Id_Pedido", idPedido);

                    conexion.Open();
                    using (SqlDataReader dr = cmd.ExecuteReader())
                    {
                        if (dr.Read())
                        {
                            pedido = new Pedido();
                            pedido.ID_PEDIDO = Convert.ToInt32(dr["Id_Pedido"]);
                            pedido.FECHA_PEDIDO = Convert.ToDateTime(dr["Fecha_Pedido"]);
                            pedido.TOTAL = Convert.ToDecimal(dr["Total"]);
                            pedido.ESTADO = dr["Estado"].ToString();
                            pedido.CLIENTE = dr["Cliente"] != DBNull.Value ? dr["Cliente"].ToString() : "Desconocido";
                            pedido.METODO_PAGO_NOMBRE = dr["Metodo_Pago"] != DBNull.Value ? dr["Metodo_Pago"].ToString() : "No especificado";
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Error al obtener los detalles del pedido: " + ex.Message);
            }
            return pedido;
        }

        // 8. OBTENER EL TICKET/DETALLES DEL PEDIDO
        public List<DetallePedido> ObtenerDetallesPorPedido(int idPedido)
        {
            List<DetallePedido> detalles = new List<DetallePedido>();
            try
            {
                using (SqlConnection conexion = new SqlConnection(_con.GetCadena()))
                {
                    SqlCommand cmd = new SqlCommand("Venta.Sp_ListarTicketPedido", conexion);
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@Id_Pedido", idPedido);

                    conexion.Open();
                    using (SqlDataReader dr = cmd.ExecuteReader())
                    {
                        while (dr.Read())
                        {
                            DetallePedido detalle = new DetallePedido();
                            detalle.ID_DETALLE_PEDIDO = Convert.ToInt32(dr["Id_Detalle_Pedido"]);
                            detalle.ID_PEDIDO = idPedido;
                            detalle.PRODUCTO = dr["Producto"].ToString();
                            detalle.IMEI_VENDIDO = dr["Numero_Serie_IMEI"] != DBNull.Value ? dr["Numero_Serie_IMEI"].ToString() : "N/A";
                            detalle.CANTIDAD = Convert.ToInt32(dr["Cantidad"]);
                            detalle.PRECIO_UNITARIO = Convert.ToDecimal(dr["Precio_Unitario"]);
                            detalles.Add(detalle);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Error al obtener el ticket del pedido: " + ex.Message);
            }
            return detalles;
        }
    }
}