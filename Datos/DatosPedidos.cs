using System;
using System.Collections.Generic;
using System.Data;
using Microsoft.Data.SqlClient;
using Entidades;
using Datos;

namespace CapaDatos
{
    public class DatosPedidos
    {

        private readonly Conexion _con = new Conexion();
        // 1. CREAR PEDIDO (Retorna el ID del nuevo pedido)
        public int Crear(Pedido pedido)
        {
            int idGenerado = 0;

            try
            {
                using (SqlConnection conexion = new SqlConnection(_con.GetCadena()))
                {
                    SqlCommand cmd = new SqlCommand("Venta.Sp_CrearPedido", conexion);
                    cmd.CommandType = CommandType.StoredProcedure;

                    cmd.Parameters.AddWithValue("@Id_Usuario", pedido.ID_USUARIO);
                    // Manejamos el int? en caso de que venga nulo
                    cmd.Parameters.AddWithValue("@Id_Metodo_Pago", pedido.ID_METODO_PAGO ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@Total", pedido.TOTAL);

                    conexion.Open();

                    // Usamos ExecuteScalar porque el SP hace un SELECT del SCOPE_IDENTITY()
                    object resultado = cmd.ExecuteScalar();

                    if (resultado != null)
                    {
                        idGenerado = Convert.ToInt32(resultado);
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Error al crear el pedido: " + ex.Message);
            }

            return idGenerado;
        }

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

                            /* NOTA SOBRE COLUMNAS EXTRA: 
                             * Tu SP devuelve 'Cliente' y 'Metodo_Pago'. Si agregas las propiedades 
                             * 'public string? CLIENTE { get; set; }' y 'public string? METODO_PAGO_NOMBRE { get; set; }' 
                             * a tu entidad, las mapeas así:
                             * * pedido.CLIENTE = dr["Cliente"].ToString();
                             * pedido.METODO_PAGO_NOMBRE = dr["Metodo_Pago"] != DBNull.Value ? dr["Metodo_Pago"].ToString() : null;
                             */

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
                            // Aquí también podrías mapear METODO_PAGO_NOMBRE si lo agregas a la entidad

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

                            // Si agregas las propiedades extra:
                            // pedido.CLIENTE = dr["Cliente"].ToString();
                            // pedido.METODO_PAGO_NOMBRE = dr["Metodo_Pago"].ToString();

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

                            // Si agregas las propiedades extra:
                            // pedido.CLIENTE = dr["Cliente"].ToString();
                            // pedido.METODO_PAGO_NOMBRE = dr["Metodo_Pago"] != DBNull.Value ? dr["Metodo_Pago"].ToString() : null;

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
    }
}