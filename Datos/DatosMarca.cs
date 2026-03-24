using System;
using System.Collections.Generic;
using System.Data;
using Microsoft.Data.SqlClient;
using Entidades;
using Datos;

namespace CapaDatos
{
    public class DatosMarca
    {

        private readonly Conexion _con = new Conexion();
        // 1. LISTAR MARCAS
        public List<Marca> Listar()
        {
            List<Marca> lista = new List<Marca>();

            try
            {
                using (SqlConnection conexion = new SqlConnection(_con.GetCadena()))
                {
                    SqlCommand cmd = new SqlCommand("Catalogo.Sp_ListarMarca", conexion);
                    cmd.CommandType = CommandType.StoredProcedure;

                    conexion.Open();

                    using (SqlDataReader dr = cmd.ExecuteReader())
                    {
                        while (dr.Read())
                        {
                            lista.Add(new Marca()
                            {
                                ID_MARCA = Convert.ToInt32(dr["ID_MARCA"]),
                                NOMBRE = dr["NOMBRE"].ToString(),
                                ESTADO = Convert.ToBoolean(dr["Estado"])
                            });
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Error al listar las marcas: " + ex.Message);
            }

            return lista;
        }

        // 2. CREAR MARCA
        public bool Crear(Marca marca)
        {
            bool respuesta = false;

            try
            {
                using (SqlConnection conexion = new SqlConnection(_con.GetCadena()))
                {
                    SqlCommand cmd = new SqlCommand("Catalogo.Sp_Creacion_Marca", conexion);
                    cmd.CommandType = CommandType.StoredProcedure;

                    // Solo mandamos el nombre. El SP asigna el Estado = 1 internamente.
                    cmd.Parameters.AddWithValue("@Nombre", marca.NOMBRE);

                    conexion.Open();
                    int filasAfectadas = cmd.ExecuteNonQuery();

                    if (filasAfectadas > 0) respuesta = true;
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Error al crear la marca: " + ex.Message);
            }

            return respuesta;
        }

        // 3. EDITAR MARCA
        public bool Editar(Marca marca)
        {
            bool respuesta = false;

            try
            {
                using (SqlConnection conexion = new SqlConnection(_con.GetCadena()))
                {
                    SqlCommand cmd = new SqlCommand("Catalogo.Sp_EditarMarca", conexion);
                    cmd.CommandType = CommandType.StoredProcedure;

                    cmd.Parameters.AddWithValue("@Id_Marca", marca.ID_MARCA);
                    cmd.Parameters.AddWithValue("@Nombre", marca.NOMBRE);
                    cmd.Parameters.AddWithValue("@Estado", marca.ESTADO);

                    conexion.Open();
                    int filasAfectadas = cmd.ExecuteNonQuery();

                    if (filasAfectadas > 0) respuesta = true;
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Error al editar la marca: " + ex.Message);
            }

            return respuesta;
        }

        // 4. DESACTIVAR MARCA
        public bool Desactivar(int idMarca)
        {
            try
            {
                using (SqlConnection conexion = new SqlConnection(_con.GetCadena()))
                {
                    SqlCommand cmd = new SqlCommand("Catalogo.Sp_DesactivarMarca", conexion);
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@Id_Marca", idMarca);

                    conexion.Open();

                    // Si el SP falla (RAISERROR), saltará directamente al catch.
                    // Si no falla, la ejecución sigue a la siguiente línea.
                    cmd.ExecuteNonQuery();

                    return true;
                }
            }
            catch (Exception ex)
            {
                // Lanzamos el mensaje puro de SQL para que el Controller lo atrape
                throw new Exception(ex.Message);
            }
        }

        // 5. FILTRAR POR NOMBRE
        public List<Marca> FiltrarPorNombre(string nombre)
        {
            List<Marca> lista = new List<Marca>();

            try
            {
                using (SqlConnection conexion = new SqlConnection(_con.GetCadena()))
                {
                    SqlCommand cmd = new SqlCommand("Catalogo.Sp_FiltrarMarcaPorNombre", conexion);
                    cmd.CommandType = CommandType.StoredProcedure;

                    // Validamos desde C# en caso de que manden nulos, aunque tu SP también lo valida
                    cmd.Parameters.AddWithValue("@Nombre", string.IsNullOrEmpty(nombre) ? "" : nombre);

                    conexion.Open();

                    using (SqlDataReader dr = cmd.ExecuteReader())
                    {
                        while (dr.Read())
                        {
                            lista.Add(new Marca()
                            {
                                ID_MARCA = Convert.ToInt32(dr["ID_MARCA"]),
                                NOMBRE = dr["NOMBRE"].ToString(),
                                ESTADO = Convert.ToBoolean(dr["Estado"])
                            });
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Error al filtrar las marcas: " + ex.Message);
            }

            return lista;
        }
    }
}