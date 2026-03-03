using Entidades;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Datos
{
    public class DataUsuario
    {
        public Conexion con = new Conexion();

        /* =======================================================
           1. LISTADO GENERAL (Sp_ListarUsuario)
        ======================================================= */
        public List<Usuario> Listar()
        {
            var olista = new List<Usuario>();
            using (SqlConnection connection = new SqlConnection(con.GetCadena()))
            {
                using (SqlCommand cmd = new SqlCommand("Seguridad.Sp_ListarUsuario", connection))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    try
                    {
                        connection.Open();
                        using (SqlDataReader reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                olista.Add(MapearUsuario(reader));
                            }
                        }
                    }
                    catch (Exception) { /* Log error */ }
                }
            }
            return olista;
        }

        /* =======================================================
           2. INSERTAR (Sp_InsertarUsuario)
        ======================================================= */
        public bool Insertar(Usuario obj)
        {
            bool respuesta = false;
            using (SqlConnection connection = new SqlConnection(con.GetCadena()))
            {
                using (SqlCommand cmd = new SqlCommand("Seguridad.Sp_InsertarUsuario", connection))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@Nombre", obj.NOMBRE);
                    cmd.Parameters.AddWithValue("@Correo", obj.CORREO);
                    cmd.Parameters.AddWithValue("@CONTRASEÑA", obj.CONTRASEÑA); // SQL aplicará el HASH
                    cmd.Parameters.AddWithValue("@Id_Rol", obj.ID_ROL);

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

        /* =======================================================
           3. EDITAR (Sp_EditarUsuario)
        ======================================================= */
        public bool Editar(Usuario obj)
        {
            bool respuesta = false;
            using (SqlConnection connection = new SqlConnection(con.GetCadena()))
            {
                using (SqlCommand cmd = new SqlCommand("Seguridad.Sp_EditarUsuario", connection))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@Id_Usuario", obj.ID_USUARIO);
                    cmd.Parameters.AddWithValue("@Nombre", obj.NOMBRE);
                    cmd.Parameters.AddWithValue("@Correo", obj.CORREO);
                    cmd.Parameters.AddWithValue("@Id_Rol", obj.ID_ROL);
                    cmd.Parameters.AddWithValue("@Estado", obj.ESTADO);

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

        /* =======================================================
           4. VALIDAR LOGIN (Sp_ValidarUsuario)
        ======================================================= */
        public Usuario Validar(string correo, string clave)
        {
            Usuario objeto = null;
            using (SqlConnection connection = new SqlConnection(con.GetCadena()))
            {
                using (SqlCommand cmd = new SqlCommand("Seguridad.Sp_ValidarUsuario", connection))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@Correo", correo);
                    cmd.Parameters.AddWithValue("@Clave", clave);

                    try
                    {
                        connection.Open();
                        using (SqlDataReader reader = cmd.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                objeto = MapearUsuario(reader);
                            }
                        }
                    }
                    catch (Exception) { objeto = null; }
                }
            }
            return objeto;
        }

        /* =======================================================
           5. CAMBIAR CLAVE (Sp_CambiarClaveUsuario)
        ======================================================= */
        public bool CambiarClave(int idUsuario, string nuevaClave)
        {
            bool respuesta = false;
            using (SqlConnection connection = new SqlConnection(con.GetCadena()))
            {
                using (SqlCommand cmd = new SqlCommand("Seguridad.Sp_CambiarClaveUsuario", connection))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@Id_Usuario", idUsuario);
                    cmd.Parameters.AddWithValue("@NuevaClave", nuevaClave);

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

        /* =======================================================
           MÉTODO DE MAPEO PRIVADO
        ======================================================= */
        private Usuario MapearUsuario(SqlDataReader reader)
        {
            return new Usuario()
            {
                ID_USUARIO = Convert.ToInt32(reader["ID_USUARIO"]),
                NOMBRE = reader["NOMBRE"].ToString(),
                CORREO = reader["CORREO"].ToString(),
                ESTADO = Convert.ToBoolean(reader["ESTADO"]),
                FECHA_BAJA = reader["FECHA_BAJA"] == DBNull.Value ? (DateTime?)null : Convert.ToDateTime(reader["FECHA_BAJA"]),

                // Mapeo del objeto relacionado para el JOIN
                oRol = new Rol()
                {
                    ID_ROL = Convert.ToInt32(reader["ID_ROL"]),
                    NOMBRE_ROL = reader["NOMBRE_ROL"].ToString()
                }
            };
        }
    }
}