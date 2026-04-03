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
        // NUEVO: ASIGNAR IMEI EN DESPACHO (Arreglado el Error Fantasma y Múltiples)
        // ==============================================================================
        // ==============================================================================
        // OPCIÓN A: ASIGNAR IMEI EN DESPACHO (Un solo registro a la vez)
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

                        // Enviamos los 2 parámetros que espera SQL
                        cmd.Parameters.AddWithValue("@Id_Detalle_Pedido", idDetallePedido);
                        cmd.Parameters.AddWithValue("@Imei", imei);

                        conexion.Open();

                        using (SqlDataReader dr = cmd.ExecuteReader())
                        {
                            if (dr.Read())
                            {
                                int exito = Convert.ToInt32(dr["Exito"]);
                                if (exito == 1)
                                {
                                    respuesta = true;
                                }
                                else
                                {
                                    // Si SQL arroja los THROW (Ej: "Ya se asignaron todos"), lo capturamos
                                    throw new Exception(dr["Mensaje"].ToString());
                                }
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    throw new Exception(ex.Message);
                }
            }
            return respuesta;
        }
        // Método dentro de DatosDetallePedido.cs
        public List<DetallePedido> ListarTicket(int idPedido)
        {
            List<DetallePedido> lista = new List<DetallePedido>();
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

                            // Leemos ID con seguridad
                            if (HasColumn(dr, "Id_Detalle_Pedido")) detalle.ID_DETALLE_PEDIDO = Convert.ToInt32(dr["Id_Detalle_Pedido"]);
                            else if (HasColumn(dr, "ID_DETALLE_PEDIDO")) detalle.ID_DETALLE_PEDIDO = Convert.ToInt32(dr["ID_DETALLE_PEDIDO"]);

                            detalle.ID_PEDIDO = idPedido;

                            // Leemos Producto con seguridad
                            if (HasColumn(dr, "Producto")) detalle.PRODUCTO = dr["Producto"].ToString();
                            else if (HasColumn(dr, "PRODUCTO")) detalle.PRODUCTO = dr["PRODUCTO"].ToString();

                            // Leemos IMEI con seguridad
                            if (HasColumn(dr, "Numero_Serie_IMEI"))
                                detalle.IMEI_VENDIDO = dr["Numero_Serie_IMEI"] != DBNull.Value ? dr["Numero_Serie_IMEI"].ToString() : "N/A";
                            else if (HasColumn(dr, "IMEI_VENDIDO"))
                                detalle.IMEI_VENDIDO = dr["IMEI_VENDIDO"] != DBNull.Value ? dr["IMEI_VENDIDO"].ToString() : "N/A";
                            else
                                detalle.IMEI_VENDIDO = "N/A";

                            // Leemos Cantidad y Precio
                            if (HasColumn(dr, "Cantidad")) detalle.CANTIDAD = Convert.ToInt32(dr["Cantidad"]);
                            else if (HasColumn(dr, "CANTIDAD")) detalle.CANTIDAD = Convert.ToInt32(dr["CANTIDAD"]);

                            if (HasColumn(dr, "Precio_Unitario")) detalle.PRECIO_UNITARIO = Convert.ToDecimal(dr["Precio_Unitario"]);
                            else if (HasColumn(dr, "PRECIO_UNITARIO")) detalle.PRECIO_UNITARIO = Convert.ToDecimal(dr["PRECIO_UNITARIO"]);

                            // ❌ NO LEEMOS EL SUBTOTAL PORQUE C# LO CALCULA SOLO

                            lista.Add(detalle);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Error al obtener el ticket: " + ex.Message);
            }
            return lista;
        }

        // Función auxiliar (pégala al final de la clase si no existe)
        private bool HasColumn(SqlDataReader reader, string columnName)
        {
            for (int i = 0; i < reader.FieldCount; i++)
            {
                if (reader.GetName(i).Equals(columnName, StringComparison.InvariantCultureIgnoreCase))
                    return true;
            }
            return false;
        }
    }
}