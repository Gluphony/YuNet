using Entidades;
using Microsoft.Data.SqlClient;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace Datos
{
    public class DatosPagos
    {
        private readonly Conexion _con = new Conexion();

        // 1. LISTAR ACTIVOS (Para Clientes)
        public List<MetodoPago> ListarMetodosPagosActivos()
        {
            var olista = new List<MetodoPago>();

            using (SqlConnection connection = new SqlConnection(_con.GetCadena()))
            {
                using (SqlCommand cmd = new SqlCommand("Sp_ListarMetodosPagosActivos", connection))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    try
                    {
                        connection.Open();
                        using (SqlDataReader reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                olista.Add(new MetodoPago()
                                {
                                    IdMetodoPago = Convert.ToInt32(reader["Id_Metodo_Pago"]),
                                    NombreMetodo = reader["Nombre_Metodo"].ToString(),
                                    Estado = Convert.ToBoolean(reader["Estado"])
                                });
                            }
                        }
                    }
                    catch (Exception) { /* Log error */ }
                }
            }
            return olista;
        }

        // 2. LISTAR TODO (Para Admin)
        public List<MetodoPago> ListarMetodosPago_Admin()
        {
            var olista = new List<MetodoPago>();

            using (SqlConnection connection = new SqlConnection(_con.GetCadena()))
            {
                using (SqlCommand cmd = new SqlCommand("Sp_ListarMetodosPago_Admin", connection))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    try
                    {
                        connection.Open();
                        using (SqlDataReader reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                olista.Add(new MetodoPago()
                                {
                                    IdMetodoPago = Convert.ToInt32(reader["Id_Metodo_Pago"]),
                                    NombreMetodo = reader["Nombre_Metodo"].ToString(),
                                    Estado = Convert.ToBoolean(reader["Estado"])
                                });
                            }
                        }
                    }
                    catch (Exception) { /* Log error */ }
                }
            }
            return olista;
        }

        // 3. INSERTAR
        public bool InsertarMetodoPago(MetodoPago metodoPago)
        {
            bool respuesta = false;
            using (SqlConnection connection = new SqlConnection(_con.GetCadena()))
            {
                using (SqlCommand cmd = new SqlCommand("Sp_InsertarMetodoPago", connection))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@Nombre_Metodo", metodoPago.NombreMetodo);
                    cmd.Parameters.AddWithValue("@Estado", metodoPago.Estado);

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

        // 4. EDITAR
        public bool EditarMetodoPago(MetodoPago metodoPago)
        {
            bool respuesta = false;
            using (SqlConnection connection = new SqlConnection(_con.GetCadena()))
            {
                using (SqlCommand cmd = new SqlCommand("Sp_EditarMetodoPago", connection))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@Id_Metodo_Pago", metodoPago.IdMetodoPago);
                    cmd.Parameters.AddWithValue("@Nombre_Metodo", metodoPago.NombreMetodo);
                    cmd.Parameters.AddWithValue("@Estado", metodoPago.Estado);

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

        // 5. DESACTIVAR
        public bool DesactivarMetodoPago(int idMetodoPago)
        {
            bool respuesta = false;
            using (SqlConnection connection = new SqlConnection(_con.GetCadena()))
            {
                using (SqlCommand cmd = new SqlCommand("Sp_DesactivarMetodoPago", connection))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@Id_Metodo_Pago", idMetodoPago);

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



