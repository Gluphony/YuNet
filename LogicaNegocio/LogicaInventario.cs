using Datos;
using Entidades;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LogicaNegocio
{
    public class LogicaInventario
    {
        private readonly DatosInventario _datosInventario = new DatosInventario();

        // 1. LISTAR HISTORIAL DE INVENTARIO POR PRODUCTO
        public List<Inventario> ListarHistorialPorProducto(int idProducto)
        {
            if (idProducto <= 0)
            {
                throw new ArgumentException("El ID del producto no es válido para consultar su historial (Kárdex).");
            }

            return _datosInventario.ListarHistorialPorProducto(idProducto);
        }

        // 2. FILTRAR INVENTARIO POR RANGO DE FECHAS
        public List<Inventario> ListarPorFechas(DateTime fechaInicio, DateTime fechaFin)
        {
            // REGLA DE NEGOCIO CRÍTICA: La fecha de inicio no puede ser posterior a la fecha final.
            if (fechaInicio > fechaFin)
            {
                throw new ArgumentException("Error en el rango: La fecha de inicio no puede ser mayor que la fecha de fin.");
            }

            // Opcional (Pro-Tip de Rendimiento): 
            // Si tu tabla de inventario crece muchísimo, puedes limitar el rango máximo de búsqueda 
            // a, por ejemplo, 6 meses o 1 año máximo, para evitar colapsar la memoria.
            /*
            if ((fechaFin - fechaInicio).TotalDays > 365)
            {
                throw new ArgumentException("Por motivos de rendimiento, el rango de búsqueda no puede superar 1 año.");
            }
            */

            return _datosInventario.ListarPorFechas(fechaInicio, fechaFin);
        }
    }
}


