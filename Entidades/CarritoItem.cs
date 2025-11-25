namespace TiendaOnline.Entidades
{
    public class CarritoItem
    {
        public int Id { get; set; }
        public int CarritoId { get; set; }
        public int ProductoId { get; set; }
        public int Cantidad { get; set; }

        public Producto Producto { get; set; }

    }
}
