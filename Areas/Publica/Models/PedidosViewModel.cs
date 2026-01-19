using TiendaOnline.Entidades;

namespace TiendaOnline.Areas.Publica.Models
{
    public class PedidosViewModel
    {
        public List<Pedido> ListadoPedidos { get; set; } = new List<Pedido>();
        public Pedido PedidoSeleccionado { get; set; }
    }
}