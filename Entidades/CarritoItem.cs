namespace TiendaOnline.Entidades
{
    public class CarritoItem
    {
        public int Id { get; set; }
        public int CarritoId { get; set; }
        public int ProductoId { get; set; }
        public int Cantidad { get; set; }

        public Producto Producto { get; set; } // Relación con Producto BENEFICIO: Permite acceder a los detalles del producto desde el carrito sin llamar a la entidad Producto directamente

        // Relación con TallaProducto
        public int TallaProductoId { get; set; }
        public TallasProducto? TallaProducto { get; set; }
    }
}
