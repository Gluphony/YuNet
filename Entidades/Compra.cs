namespace Entidades
{
    public class Compra
    {
        public int ID_COMPRA { get; set; }
        public int ID_PROVEEDOR { get; set; }
        public int ID_USUARIO { get; set; }
        public DateTime FECHA_COMPRA { get; set; }
        public string? NUMERO_FACTURA_PROV { get; set; }
        public decimal TOTAL_COMPRA { get; set; }
        // ==========================================
        // PROPIEDADES EXTENDIDAS (Para las Vistas HTML)
        // ==========================================
        public string? PROVEEDOR_NOMBRE { get; set; }
        public string? USUARIO_NOMBRE { get; set; }
    }
}

