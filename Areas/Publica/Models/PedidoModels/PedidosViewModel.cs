using TiendaOnline.Entidades;

namespace TiendaOnline.Areas.Publica.Models.PedidoModels
{
    public class PedidosViewModel
    {
        public List<Pedido> ListadoPedidos { get; set; } = new List<Pedido>();
        public Pedido PedidoSeleccionado { get; set; }
    }
}