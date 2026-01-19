namespace TiendaOnline.Entidades
{
    public class Pedido
    {
        public int Id { get; set; }
        public int UsuarioId { get; set; }
        public DateTime FechaCreacion { get; set; }
        public decimal Total { get; set; }
        public List<PedidoItem> Items { get; set; } = new List<PedidoItem>();
    }
}