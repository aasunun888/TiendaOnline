namespace TiendaOnline.Entidades
{
    public class PedidoItem
    {
        public int Id { get; set; }
        public int PedidoId { get; set; }
        public int ProductoId { get; set; }
        public int Cantidad { get; set; }
        public decimal Precio { get; set; }
        public string ProductoNombre { get; set; } = "";
        public string ImagenUrl { get; set; } = "";
        public int? TallaProductoId { get; set; }
        public string Talla { get; set; } = "";
    }
}