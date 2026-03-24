using CapaDatos;
using Entidades;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LogicaNegocio
{
    public class LogicaMarca
    {
        private readonly DatosMarca _datosMarca = new DatosMarca();

        // 1. LISTAR MARCAS
        public List<Marca> Listar()
        {
            return _datosMarca.Listar();
        }

        // 2. CREAR MARCA
        public bool Crear(Marca marca)
        {
            if (marca == null)
                throw new ArgumentNullException(nameof(marca), "El objeto marca no puede ser nulo.");

            if (string.IsNullOrWhiteSpace(marca.NOMBRE))
                throw new ArgumentException("El nombre de la marca es obligatorio y no puede estar vacío.");

            // Sanitización: Quitamos espacios al inicio y al final para evitar duplicados como "Sony " y "Sony"
            marca.NOMBRE = marca.NOMBRE.Trim();

            // Opcional: Podrías limitar la longitud según tu base de datos
            if (marca.NOMBRE.Length > 50)
                throw new ArgumentException("El nombre de la marca no puede superar los 50 caracteres.");

            return _datosMarca.Crear(marca);
        }

        // 3. EDITAR MARCA
        public bool Editar(Marca marca)
        {
            if (marca == null)
                throw new ArgumentNullException(nameof(marca), "El objeto marca no puede ser nulo.");

            if (marca.ID_MARCA <= 0)
                throw new ArgumentException("El ID de la marca a editar no es válido.");

            if (string.IsNullOrWhiteSpace(marca.NOMBRE))
                throw new ArgumentException("El nombre de la marca es obligatorio y no puede estar vacío.");

            // Sanitización
            marca.NOMBRE = marca.NOMBRE.Trim();

            return _datosMarca.Editar(marca);
        }

        // 4. DESACTIVAR MARCA
        public bool Desactivar(int idMarca)
        {
            if (idMarca <= 0)
                throw new ArgumentException("El ID de la marca no es válido.");

            try
            {
                return _datosMarca.Desactivar(idMarca);
            }
            catch (Exception ex)
            {
                // Aquí atrapamos el RAISERROR del SQL y lo mandamos hacia arriba (al Controller)
                throw new Exception(ex.Message);
            }
        }

        // 5. FILTRAR POR NOMBRE
        public List<Marca> FiltrarPorNombre(string nombre)
        {
            // Sanitizamos la entrada del usuario antes de buscar
            string terminoBusqueda = string.IsNullOrWhiteSpace(nombre) ? "" : nombre.Trim();

            return _datosMarca.FiltrarPorNombre(terminoBusqueda);
        }
    }
}

