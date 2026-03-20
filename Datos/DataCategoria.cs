using System;
using System.Collections.Generic;
using System.Data;
using Microsoft.Data.SqlClient;
using Entidades;

namespace Datos
{
    public class DataCategoria
    {
        private readonly Conexion _con = new Conexion();

        // 1. LISTAR CATEGORÍAS
        public List<Categoria> Listar()
        {
            List<Categoria> lista = new List<Categoria>();

            try
            {
                using (SqlConnection conexion = new SqlConnection(_con.GetCadena()))
                {
                    SqlCommand cmd = new SqlCommand("Catalogo.Sp_ListarCategoria", conexion);
                    cmd.CommandType = CommandType.StoredProcedure;

                    conexion.Open();

                    using (SqlDataReader dr = cmd.ExecuteReader())
                    {
                        while (dr.Read())
                        {
                            lista.Add(new Categoria()
                            {
                                ID_CATEGORIA = Convert.ToInt32(dr["ID_CATEGORIA"]),
                                NOMBRE_CATEGORIA = dr["NOMBRE_CATEGORIA"].ToString(),
                                ESTADO = Convert.ToBoolean(dr["ESTADO"])
                            });
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                // Como usaste THROW en SQL, el error caerá directamente aquí.
                throw new Exception("Error al listar las categorías: " + ex.Message);
            }

            return lista;
        }

        // 2. CREAR CATEGORÍA
        public bool Crear(Categoria categoria)
        {
            bool respuesta = false;

            try
            {
                using (SqlConnection conexion = new SqlConnection(_con.GetCadena()))
                {
                    SqlCommand cmd = new SqlCommand("Catalogo.Sp_Creacion_Categoria", conexion);
                    cmd.CommandType = CommandType.StoredProcedure;

                    // Solo pasamos el nombre, el SP se encarga del estado = 1 y el ID es identity
                    cmd.Parameters.AddWithValue("@Nombre_Categoria", categoria.NOMBRE_CATEGORIA);

                    conexion.Open();
                    int filasAfectadas = cmd.ExecuteNonQuery();

                    if (filasAfectadas > 0) respuesta = true;
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Error al crear la categoría: " + ex.Message);
            }

            return respuesta;
        }

        // 3. EDITAR CATEGORÍA
        public bool Editar(Categoria categoria)
        {
            bool respuesta = false;

            try
            {
                using (SqlConnection conexion = new SqlConnection(_con.GetCadena()))
                {
                    SqlCommand cmd = new SqlCommand("Catalogo.Sp_EditarCategoria", conexion);
                    cmd.CommandType = CommandType.StoredProcedure;

                    cmd.Parameters.AddWithValue("@Id_Categoria", categoria.ID_CATEGORIA);
                    cmd.Parameters.AddWithValue("@Nombre_Categoria", categoria.NOMBRE_CATEGORIA);
                    cmd.Parameters.AddWithValue("@Estado", categoria.ESTADO);

                    conexion.Open();
                    int filasAfectadas = cmd.ExecuteNonQuery();

                    if (filasAfectadas > 0) respuesta = true;
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Error al editar la categoría: " + ex.Message);
            }

            return respuesta;
        }

        // 4. DESACTIVAR CATEGORÍA
        public bool Desactivar(int idCategoria)
        {
            bool respuesta = false;

            try
            {
                using (SqlConnection conexion = new SqlConnection(_con.GetCadena()))
                {
                    SqlCommand cmd = new SqlCommand("Catalogo.Sp_DesactivarCategoria", conexion);
                    cmd.CommandType = CommandType.StoredProcedure;

                    cmd.Parameters.AddWithValue("@Id_Categoria", idCategoria);

                    conexion.Open();
                    int filasAfectadas = cmd.ExecuteNonQuery();

                    if (filasAfectadas > 0) respuesta = true;
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Error al desactivar la categoría: " + ex.Message);
            }

            return respuesta;
        }

        // 5. FILTRAR POR NOMBRE
        public List<Categoria> FiltrarPorNombre(string nombre)
        {
            List<Categoria> lista = new List<Categoria>();

            try
            {
                using (SqlConnection conexion = new SqlConnection(_con.GetCadena()))
                {
                    SqlCommand cmd = new SqlCommand("Catalogo.Sp_FiltrarCategoriaPorNombre", conexion);
                    cmd.CommandType = CommandType.StoredProcedure;

                    cmd.Parameters.AddWithValue("@Nombre", nombre);

                    conexion.Open();

                    using (SqlDataReader dr = cmd.ExecuteReader())
                    {
                        while (dr.Read())
                        {
                            lista.Add(new Categoria()
                            {
                                ID_CATEGORIA = Convert.ToInt32(dr["ID_CATEGORIA"]),
                                NOMBRE_CATEGORIA = dr["NOMBRE_CATEGORIA"].ToString(),
                                ESTADO = Convert.ToBoolean(dr["ESTADO"])
                            });
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Error al filtrar las categorías: " + ex.Message);
            }

            return lista;
        }
    }
}