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
    public class DataCarrito
    {
        private readonly Conexion _con = new Conexion();

        // 1. OBTENER O CREAR (Devuelve el objeto Carrito actual del usuario)
        public Carrito ObtenerOCrearCarrito(int idUsuario)
        {
            Carrito objeto = null;

            using (SqlConnection connection = new SqlConnection(_con.GetCadena()))
            {
                using (SqlCommand cmd = new SqlCommand("Venta.Sp_ObtenerOCrearCarrito", connection))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@Id_Usuario", idUsuario);

                    try
                    {
                        connection.Open();
                        using (SqlDataReader reader = cmd.ExecuteReader())
                        {
                            if (reader.Read()) // Solo esperamos uno
                            {
                                objeto = new Carrito()
                                {
                                    ID_CARRITO = Convert.ToInt32(reader["ID_CARRITO"]),
                                    ID_USUARIO = Convert.ToInt32(reader["ID_USUARIO"]),
                                    FECHA_CREACION = Convert.ToDateTime(reader["FECHA_CREACION"]),
                                    TOTAL = Convert.ToDecimal(reader["TOTAL"]),
                                    ESTADO = reader["ESTADO"].ToString()
                                };
                            }
                        }
                    }
                    catch (Exception) { objeto = null; }
                }
            }
            return objeto;
        }

        // 2. ACTUALIZAR TOTAL
        public bool ActualizarTotal(int idCarrito, decimal nuevoTotal)
        {
            bool respuesta = false;
            using (SqlConnection connection = new SqlConnection(_con.GetCadena()))
            {
                using (SqlCommand cmd = new SqlCommand("Venta.Sp_ActualizarTotalCarrito", connection))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@Id_Carrito", idCarrito);
                    cmd.Parameters.AddWithValue("@NuevoTotal", nuevoTotal);

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

        // 3. CAMBIAR ESTADO (PENDIENTE -> COMPLETADO)
        public bool CambiarEstado(int idCarrito, string nuevoEstado)
        {
            bool respuesta = false;
            using (SqlConnection connection = new SqlConnection(_con.GetCadena()))
            {
                using (SqlCommand cmd = new SqlCommand("Venta.Sp_CambiarEstadoCarrito", connection))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@Id_Carrito", idCarrito);
                    cmd.Parameters.AddWithValue("@NuevoEstado", nuevoEstado);

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

        // 4. ELIMINAR/VACIAR CARRITO
        public bool VaciarCarrito(int idCarrito)
        {
            bool respuesta = false;
            using (SqlConnection connection = new SqlConnection(_con.GetCadena()))
            {
                using (SqlCommand cmd = new SqlCommand("Venta.Sp_EliminarCarrito", connection))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@Id_Carrito", idCarrito);

                    try
                    {
                        connection.Open();
                        // Este SP hace un DELETE y un UPDATE interno
                        respuesta = cmd.ExecuteNonQuery() > 0;
                    }
                    catch (Exception) { respuesta = false; }
                }
            }
            return respuesta;
        }
    }
}

