using TiendaOnline.Entidades;

namespace TiendaOnline.Areas.Publica.Models.CarritoModels
{
    public class CarritoViewModel
    {
        public List<CarritoItem> Items { get; set; } = new List<CarritoItem>();
    }
}
