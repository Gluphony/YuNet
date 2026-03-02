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
    public class DatosRolesPermisos
    {
        private readonly Conexion _con = new Conexion();

        // 1. ASIGNAR PERMISO A UN ROL
        public bool AsignarPermiso(int idRol, int idPermiso)
        {
            bool respuesta = false;
            using (SqlConnection connection = new SqlConnection(_con.GetCadena()))
            {
                using (SqlCommand cmd = new SqlCommand("Sp_AsignarPermiso", connection))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@Id_Rol", idRol);
                    cmd.Parameters.AddWithValue("@Id_Permiso", idPermiso);

                    try
                    {
                        connection.Open();
                        // ExecuteNonQuery devuelve las filas afectadas. 
                        // Si el SP hizo el INSERT, devolverá 1 (true).
                        // Si entró en el IF EXISTS y no insertó nada, devolverá 0 (false).
                        respuesta = cmd.ExecuteNonQuery() > 0;
                    }
                    catch (Exception ex)
                    {
                        // Aquí podrías loguear el error ex.Message
                        respuesta = false;
                    }
                }
            }
            return respuesta;
        }

        // 2. REVOCAR (ELIMINAR) PERMISO DE UN ROL
        public bool RevocarPermiso(int idRol, int idPermiso)
        {
            bool respuesta = false;
            using (SqlConnection connection = new SqlConnection(_con.GetCadena()))
            {
                using (SqlCommand cmd = new SqlCommand("Sp_RevocarPermiso", connection))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@Id_Rol", idRol);
                    cmd.Parameters.AddWithValue("@Id_Permiso", idPermiso);

                    try
                    {
                        connection.Open();
                        respuesta = cmd.ExecuteNonQuery() > 0;
                    }
                    catch (Exception)
                    {
                        respuesta = false;
                    }
                }
            }
            return respuesta;
        }

        // 3. LISTAR PERMISOS QUE TIENE UN ROL ESPECÍFICO
        public List<Permisos> ListarPermisosPorRol(int idRol)
        {
            var olista = new List<Permisos>();

            using (SqlConnection connection = new SqlConnection(_con.GetCadena()))
            {
                using (SqlCommand cmd = new SqlCommand("Sp_ListarPermisosPorRol", connection))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@Id_Rol", idRol);

                    try
                    {
                        connection.Open();
                        using (SqlDataReader reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                olista.Add(new Permisos()
                                {
                                    // Usamos los nombres de columna definidos en el SELECT de tu SP
                                    ID_PERMISO = Convert.ToInt32(reader["Id_Permiso"]),
                                    CLAVE_PERMISO = reader["Clave_Permiso"].ToString(),
                                    DESCRIPCION = reader["Descripcion"].ToString()
                                });
                            }
                        }
                    }
                    catch (Exception)
                    {
                        // En caso de error devolvemos la lista vacía
                    }
                }
            }
            return olista;
        }
    }
}
