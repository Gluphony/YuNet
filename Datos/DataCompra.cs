using System;
using System.Collections.Generic;
using System.Data;
using Microsoft.Data.SqlClient;
using Entidades;

namespace Datos
{
    public class DataCompra
    {
        private readonly Conexion _con = new Conexion();

        public bool RegistrarCompraCompleta(int idProveedor, int idUsuario, string factura, DetalleCompra detalle)
        {
            bool respuesta = false;

            try
            {
                using (SqlConnection conexion = new SqlConnection(_con.GetCadena()))
                {
                    SqlCommand cmd = new SqlCommand("Inventario.Sp_RegistrarEntradaMercaderia", conexion);
                    cmd.CommandType = CommandType.StoredProcedure;

                    cmd.Parameters.AddWithValue("@Id_Proveedor", idProveedor);
                    cmd.Parameters.AddWithValue("@Id_Usuario", idUsuario);
                    cmd.Parameters.AddWithValue("@Id_Producto", detalle.ID_PRODUCTO);
                    cmd.Parameters.AddWithValue("@Cantidad", detalle.CANTIDAD);
                    cmd.Parameters.AddWithValue("@Costo", detalle.COSTO_UNITARIO);
                    cmd.Parameters.AddWithValue("@FacturaProv", factura);
                    cmd.Parameters.AddWithValue("@ListaImeis", detalle.LISTA_IMEIS);

                    conexion.Open();

                    // ¡EL CAMBIO MÁGICO ESTÁ AQUÍ! 
                    // Usamos SqlDataReader para leer el "SELECT Exito, Mensaje" del SQL
                    using (SqlDataReader dr = cmd.ExecuteReader())
                    {
                        if (dr.Read())
                        {
                            int exito = Convert.ToInt32(dr["Exito"]);
                            string mensaje = dr["Mensaje"].ToString();

                            if (exito == 1)
                            {
                                respuesta = true; // Todo salió perfecto
                            }
                            else
                            {
                                // Si el SP cayó en el CATCH, lanzamos el error real de SQL
                                throw new Exception(mensaje);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                // Si explota, mandamos el error para que SweetAlert lo muestre
                throw new Exception(ex.Message);
            }

            return respuesta;
        }

        // ==============================================================================
        // NUEVO: LISTAR TODAS LAS COMPRAS (Para la tabla principal)
        // ==============================================================================
        public List<Compra> ListarCompras()
        {
            List<Compra> lista = new List<Compra>();

            try
            {
                using (SqlConnection conexion = new SqlConnection(_con.GetCadena()))
                {
                    SqlCommand cmd = new SqlCommand("Inventario.Sp_ListarCompras", conexion);
                    cmd.CommandType = CommandType.StoredProcedure;

                    conexion.Open();

                    using (SqlDataReader dr = cmd.ExecuteReader())
                    {
                        while (dr.Read())
                        {
                            Compra obj = new Compra();

                            obj.ID_COMPRA = Convert.ToInt32(dr["ID_COMPRA"]);
                            obj.FECHA_COMPRA = Convert.ToDateTime(dr["FECHA_COMPRA"]);
                            obj.NUMERO_FACTURA_PROV = dr["NUMERO_FACTURA_PROV"].ToString();
                            obj.TOTAL_COMPRA = Convert.ToDecimal(dr["TOTAL_COMPRA"]);

                            // El alias que le pusimos en SQL con el JOIN
                            obj.PROVEEDOR_NOMBRE = dr["Proveedor"].ToString();

                            lista.Add(obj);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Error al obtener la lista de compras: " + ex.Message);
            }

            return lista;
        }
    }
}