using Datos;
using Entidades;
using System;

namespace LogicaNegocio
{
    public class LogicaCompra
    {
        private readonly DataCompra _data = new DataCompra();

        public bool RegistrarEntrada(int idProveedor, int idUsuario, string factura, DetalleCompra detalle)
        {
            try
            {
                // 1. Validaciones de Negocio
                if (idProveedor <= 0) throw new Exception("Debe seleccionar un proveedor válido.");
                if (string.IsNullOrEmpty(factura)) throw new Exception("El número de factura del proveedor es requerido.");
                if (detalle.ID_PRODUCTO <= 0) throw new Exception("Debe seleccionar un producto.");

                // 2. Validación de IMEIs
                if (string.IsNullOrEmpty(detalle.LISTA_IMEIS))
                    throw new Exception("Debe escanear los IMEIs de las unidades.");

                // Contamos cuántos IMEIs vienen en la cadena (separados por coma)
                string[] imeis = detalle.LISTA_IMEIS.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries);

                if (imeis.Length != detalle.CANTIDAD)
                {
                    throw new Exception($"La cantidad de unidades ({detalle.CANTIDAD}) no coincide con los IMEIs escaneados ({imeis.Length}).");
                }

                // 3. Pasamos a la capa de datos
                return _data.RegistrarCompraCompleta(idProveedor, idUsuario, factura, detalle);
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }
    }
}