using TiendaOnline.Entidades;

namespace TiendaOnline.Areas.Publica.Models
{
    public class CarritoViewModel
    {
        public List<Producto> carrito { get; set; } = new List<Producto>();

    }
}
