using System;
using System.Data;
using System.Data.SqlClient;
using Entidades;

namespace Datos
{
    public class DatosProveedor
    {
        private readonly Conexion _con = new Conexion();

        public DataTable ListarProveedores()
        {
            DataTable tabla = new DataTable();
            using (SqlConnection conexion = new SqlConnection(_con.GetCadena()))
            {
                try
                {
                    SqlCommand cmd = new SqlCommand("Sp_ListarProveedor", conexion);
                    cmd.CommandType = CommandType.StoredProcedure;

                    SqlDataAdapter da = new SqlDataAdapter(cmd);
                    da.Fill(tabla);
                }
                catch (Exception ex)
                {
                    throw new Exception("Error al listar los proveedores: " + ex.Message);
                }
            }
            return tabla;
        }

        public bool InsertarProveedor(Proveedor proveedor)
        {
            bool respuesta = false;
            using (SqlConnection conexion = new SqlConnection(_con.GetCadena()))
            {
                try
                {
                    SqlCommand cmd = new SqlCommand("Sp_InsertarProveedor", conexion);
                    cmd.CommandType = CommandType.StoredProcedure;

                    // Si el string es null, enviamos DBNull.Value a la base de datos
                    cmd.Parameters.AddWithValue("@Razon_Social", proveedor.RAZON_SOCIAL ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@Nombre_Contacto", proveedor.NOMBRE_CONTACTO ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@Telefono", proveedor.TELEFONO ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@Correo", proveedor.CORREO ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@Direccion", proveedor.DIRECCION ?? (object)DBNull.Value);

                    conexion.Open();
                    cmd.ExecuteNonQuery();
                    respuesta = true;
                }
                catch (Exception ex)
                {
                    throw new Exception("Error al insertar el proveedor: " + ex.Message);
                }
            }
            return respuesta;
        }

        public bool EditarProveedor(Proveedor proveedor)
        {
            bool respuesta = false;
            using (SqlConnection conexion = new SqlConnection(_con.GetCadena()))
            {
                try
                {
                    SqlCommand cmd = new SqlCommand("Sp_EditarProveedor", conexion);
                    cmd.CommandType = CommandType.StoredProcedure;

                    cmd.Parameters.AddWithValue("@Id_Proveedor", proveedor.ID_PROVEEDOR);
                    cmd.Parameters.AddWithValue("@Razon_Social", proveedor.RAZON_SOCIAL ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@Nombre_Contacto", proveedor.NOMBRE_CONTACTO ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@Telefono", proveedor.TELEFONO ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@Correo", proveedor.CORREO ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@Direccion", proveedor.DIRECCION ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@Estado", proveedor.ESTADO);

                    conexion.Open();
                    cmd.ExecuteNonQuery();
                    respuesta = true;
                }
                catch (Exception ex)
                {
                    throw new Exception("Error al editar el proveedor: " + ex.Message);
                }
            }
            return respuesta;
        }

        public bool DesactivarProveedor(int idProveedor)
        {
            bool respuesta = false;
            using (SqlConnection conexion = new SqlConnection(_con.GetCadena()))
            {
                try
                {
                    SqlCommand cmd = new SqlCommand("Sp_DesactivarProveedor", conexion);
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@Id_Proveedor", idProveedor);

                    conexion.Open();
                    cmd.ExecuteNonQuery();
                    respuesta = true;
                }
                catch (Exception ex)
                {
                    throw new Exception("Error al desactivar el proveedor: " + ex.Message);
                }
            }
            return respuesta;
        }

        public DataTable ListarProveedoresActivos()
        {
            DataTable tabla = new DataTable();
            using (SqlConnection conexion = new SqlConnection(_con.GetCadena()))
            {
                try
                {
                    SqlCommand cmd = new SqlCommand("Sp_ListarProveedoresActivos", conexion);
                    cmd.CommandType = CommandType.StoredProcedure;

                    SqlDataAdapter da = new SqlDataAdapter(cmd);
                    da.Fill(tabla);
                }
                catch (Exception ex)
                {
                    throw new Exception("Error al listar proveedores activos: " + ex.Message);
                }
            }
            return tabla;
        }

        public DataTable BuscarProveedorPorNombre(string textoBusqueda)
        {
            DataTable tabla = new DataTable();
            using (SqlConnection conexion = new SqlConnection(_con.GetCadena()))
            {
                try
                {
                    SqlCommand cmd = new SqlCommand("Sp_BuscarProveedorPorNombre", conexion);
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@TextoBusqueda", textoBusqueda);

                    SqlDataAdapter da = new SqlDataAdapter(cmd);
                    da.Fill(tabla);
                }
                catch (Exception ex)
                {
                    throw new Exception("Error al buscar el proveedor: " + ex.Message);
                }
            }
            return tabla;
        }
    }
}