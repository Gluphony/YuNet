using System;
using System.Data;
using System.Data.SqlClient;
using Entidades;

namespace Datos
{
    public class DatosUsuario
    {

        private readonly Conexion _con = new Conexion();

        public DataTable ListarUsuarios()
        {
            DataTable tabla = new DataTable();
            using (SqlConnection conexion = new SqlConnection(_con.GetCadena()))
            {
                try
                {
                    SqlCommand cmd = new SqlCommand("Seguridad.Sp_ListarUsuario", conexion);
                    cmd.CommandType = CommandType.StoredProcedure;

                    SqlDataAdapter da = new SqlDataAdapter(cmd);
                    da.Fill(tabla);
                }
                catch (Exception ex)
                {
                    throw new Exception("Error al listar los usuarios: " + ex.Message);
                }
            }
            return tabla;
        }

        // Fíjate que pedimos la clave como string aparte de la entidad Usuario
        public bool InsertarUsuario(Usuario usuario, string clavePlana)
        {
            bool respuesta = false;
            using (SqlConnection conexion = new SqlConnection(_con.GetCadena()))
            {
                try
                {
                    SqlCommand cmd = new SqlCommand("Seguridad.Sp_InsertarUsuario", conexion);
                    cmd.CommandType = CommandType.StoredProcedure;

                    cmd.Parameters.AddWithValue("@Nombre", usuario.NOMBRE ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@Correo", usuario.CORREO ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@Clave", clavePlana ?? (object)DBNull.Value); // Se pasa como texto, SQL hace el Hash
                    cmd.Parameters.AddWithValue("@Id_Rol", usuario.ID_ROL);

                    conexion.Open();
                    cmd.ExecuteNonQuery();
                    respuesta = true;
                }
                catch (Exception ex)
                {
                    throw new Exception("Error al insertar el usuario: " + ex.Message);
                }
            }
            return respuesta;
        }

        public bool EditarUsuario(Usuario usuario)
        {
            bool respuesta = false;
            using (SqlConnection conexion = new SqlConnection(_con.GetCadena()))
            {
                try
                {
                    // Nota: Este SP no edita la contraseña
                    // Para cambiar clave, deberías hacer un SP exclusivo (ej. Sp_CambiarClave).
                    SqlCommand cmd = new SqlCommand("Seguridad.Sp_EditarUsuario", conexion);
                    cmd.CommandType = CommandType.StoredProcedure;

                    cmd.Parameters.AddWithValue("@Id_Usuario", usuario.ID_USUARIO);
                    cmd.Parameters.AddWithValue("@Nombre", usuario.NOMBRE ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@Correo", usuario.CORREO ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@Id_Rol", usuario.ID_ROL);
                    cmd.Parameters.AddWithValue("@Estado", usuario.ESTADO);

                    conexion.Open();
                    cmd.ExecuteNonQuery();
                    respuesta = true;
                }
                catch (Exception ex)
                {
                    throw new Exception("Error al editar el usuario: " + ex.Message);
                }
            }
            return respuesta;
        }

        public bool DesactivarUsuario(int idUsuario)
        {
            bool respuesta = false;
            using (SqlConnection conexion = new SqlConnection(_con.GetCadena()))
            {
                try
                {
                    SqlCommand cmd = new SqlCommand("Seguridad.Sp_DesactivarUsuario", conexion);
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@Id_Usuario", idUsuario);

                    conexion.Open();
                    cmd.ExecuteNonQuery();
                    respuesta = true;
                }
                catch (Exception ex)
                {
                    throw new Exception("Error al desactivar el usuario: " + ex.Message);
                }
            }
            return respuesta;
        }
    }
}