using TiendaOnline.Entidades;

namespace TiendaOnline.Areas.Publica.Models
{
    public class CarritoViewModel
    {
        public List<CarritoItem> Items { get; set; } = new List<CarritoItem>();
    }
}
