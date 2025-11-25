namespace TiendaOnline.Entidades
{
    public class Carrito
    {
        public int Id { get; set; }
        public int UsuarioId { get; set; }
        public DateTime FechaCreacion { get; set; }
        public List<CarritoItem> Items { get; set; } = new List<CarritoItem>();
    }
}
