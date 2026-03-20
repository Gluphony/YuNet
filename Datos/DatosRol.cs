using System;
using System.Data;
using Microsoft.Data.SqlClient;
using Entidades;

namespace Datos
{
    public class DatosRol
    {
        private readonly Conexion _con = new Conexion();

        public DataTable ListarRoles()
        {
            DataTable tabla = new DataTable();
            using (SqlConnection conexion = new SqlConnection(_con.GetCadena()))
            {
                try
                {
                    // Nota el esquema "Seguridad." en el nombre del SP
                    SqlCommand cmd = new SqlCommand("Seguridad.Sp_ListarRol", conexion);
                    cmd.CommandType = CommandType.StoredProcedure;

                    SqlDataAdapter da = new SqlDataAdapter(cmd);
                    da.Fill(tabla);
                }
                catch (Exception ex)
                {
                    throw new Exception("Error al listar los roles: " + ex.Message);
                }
            }
            return tabla;
        }

        public bool InsertarRol(Rol rol)
        {
            bool respuesta = false;
            using (SqlConnection conexion = new SqlConnection(_con.GetCadena()))
            {
                try
                {
                    SqlCommand cmd = new SqlCommand("Seguridad.Sp_InsertarRol", conexion);
                    cmd.CommandType = CommandType.StoredProcedure;

                    cmd.Parameters.AddWithValue("@Rol", rol.NOMBRE_ROL ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@Descripcion", rol.DESCRIPCION ?? (object)DBNull.Value);

                    conexion.Open();
                    cmd.ExecuteNonQuery();
                    respuesta = true;
                }
                catch (Exception ex)
                {
                    throw new Exception("Error al insertar el rol: " + ex.Message);
                }
            }
            return respuesta;
        }

        // Fíjate que agregamos el byte[] para el control de concurrencia
        public bool EditarRol(Rol rol, byte[] versionOriginal)
        {
            bool respuesta = false;
            using (SqlConnection conexion = new SqlConnection(_con.GetCadena()))
            {
                try
                {
                    SqlCommand cmd = new SqlCommand("Seguridad.Sp_EditarRol", conexion);
                    cmd.CommandType = CommandType.StoredProcedure;

                    cmd.Parameters.AddWithValue("@Id_Rol", rol.ID_ROL);
                    cmd.Parameters.AddWithValue("@Rol", rol.NOMBRE_ROL ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@Descripcion", rol.DESCRIPCION ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@Estado", rol.ESTADO);
                    cmd.Parameters.AddWithValue("@Version_Original", versionOriginal);

                    conexion.Open();
                    cmd.ExecuteNonQuery();
                    respuesta = true;
                }
                catch (Exception ex)
                {
                    throw new Exception("Error al editar el rol: " + ex.Message);
                }
            }
            return respuesta;
        }

        public bool EditarRolPorNombre(string nombreActual, string nuevoNombre, string nuevaDescripcion, bool estado)
        {
            bool respuesta = false;
            using (SqlConnection conexion = new SqlConnection(_con.GetCadena()))
            {
                try
                {
                    SqlCommand cmd = new SqlCommand("Seguridad.Sp_EditarRolPorNombre", conexion);
                    cmd.CommandType = CommandType.StoredProcedure;

                    cmd.Parameters.AddWithValue("@NombreRolActual", nombreActual);
                    cmd.Parameters.AddWithValue("@NuevoNombreRol", nuevoNombre ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@NuevaDescripcion", nuevaDescripcion ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@Estado", estado);

                    conexion.Open();
                    cmd.ExecuteNonQuery();
                    respuesta = true;
                }
                catch (Exception ex)
                {
                    throw new Exception("Error al editar el rol por nombre: " + ex.Message);
                }
            }
            return respuesta;
        }

        public bool DesactivarRol(int idRol)
        {
            bool respuesta = false;
            using (SqlConnection conexion = new SqlConnection(_con.GetCadena()))
            {
                try
                {
                    SqlCommand cmd = new SqlCommand("Seguridad.Sp_DesactivarRol", conexion);
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@Id_Rol", idRol);

                    conexion.Open();
                    cmd.ExecuteNonQuery();
                    respuesta = true;
                }
                catch (Exception ex)
                {
                    throw new Exception("Error al desactivar el rol: " + ex.Message);
                }
            }
            return respuesta;
        }
    }
}