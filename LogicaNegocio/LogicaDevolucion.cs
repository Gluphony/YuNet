using Datos;
using Entidades;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LogicaNegocio
{
    public class LogicaDevolucion
    {
        private readonly DatosDevoluciones _datosDevoluciones = new DatosDevoluciones();

        // 1. LISTAR DEVOLUCIONES
        public List<Devolucion> Listar()
        {
            // Como no hay parámetros de entrada, no hay mucho que validar antes de ir a la BD.
            // Simplemente pasamos la solicitud a la capa de Datos.
            try
            {
                return _datosDevoluciones.Listar();
            }
            catch (Exception ex)
            {
                // Aquí podrías agregar un log de errores (guardar en un archivo de texto, etc.)
                // Por ahora, relanzamos la excepción para que llegue a la vista.
                throw new Exception("Ocurrió un error en la capa lógica al intentar obtener las devoluciones.", ex);
            }
        }

        /* * ESPACIO RESERVADO PARA EL FUTURO:
         * Si en algún momento decides procesar las devoluciones desde C# en lugar de hacerlo 
         * solo en SQL, aquí irían métodos como:
         * * public bool RegistrarDevolucion(Devolucion devolucion) { ... validaciones estrictas ... }
         */
    }
}