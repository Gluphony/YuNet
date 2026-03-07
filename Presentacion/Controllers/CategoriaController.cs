using Datos;
using Entidades;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LogicaNegocio
{
    public class LogicaCategoria
    {
        private readonly DataCategoria _dataCategoria = new DataCategoria();

        // 1. LISTAR CATEGORÍAS
        public List<Categoria> Listar()
        {
            return _dataCategoria.Listar();
        }

        // 2. CREAR CATEGORÍA
        public bool Crear(Categoria categoria)
        {
            // Regla de Negocio: El nombre no puede estar vacío o lleno de espacios
            if (string.IsNullOrWhiteSpace(categoria.NOMBRE_CATEGORIA))
            {
                throw new ArgumentException("El nombre de la categoría es obligatorio.");
            }

            // Opcional: Limpiar espacios extra antes de enviar a BD
            categoria.NOMBRE_CATEGORIA = categoria.NOMBRE_CATEGORIA.Trim();

            return _dataCategoria.Crear(categoria);
        }

        // 3. EDITAR CATEGORÍA
        public bool Editar(Categoria categoria)
        {
            if (categoria.ID_CATEGORIA <= 0)
            {
                throw new ArgumentException("El ID de la categoría no es válido.");
            }

            if (string.IsNullOrWhiteSpace(categoria.NOMBRE_CATEGORIA))
            {
                throw new ArgumentException("El nombre de la categoría no puede quedar vacío.");
            }

            categoria.NOMBRE_CATEGORIA = categoria.NOMBRE_CATEGORIA.Trim();

            return _dataCategoria.Editar(categoria);
        }

        // 4. DESACTIVAR CATEGORÍA
        public bool Desactivar(int idCategoria)
        {
            if (idCategoria <= 0)
            {
                throw new ArgumentException("El ID de la categoría a desactivar no es válido.");
            }

            return _dataCategoria.Desactivar(idCategoria);
        }

        // 5. FILTRAR POR NOMBRE
        public List<Categoria> FiltrarPorNombre(string nombre)
        {
            // Regla: Si mandan el filtro vacío, mejor devolvemos la lista completa 
            // en lugar de hacer fallar la consulta
            if (string.IsNullOrWhiteSpace(nombre))
            {
                return _dataCategoria.Listar();
            }

            return _dataCategoria.FiltrarPorNombre(nombre.Trim());
        }
    }
}