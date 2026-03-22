using TiendaOnline.Entidades;

namespace TiendaOnline.Areas.Administracion.Models.ProductoModels
{
    public class ProductoTallasViewModel
    {
        public int ProductoId { get; set; }
        public string Nombre { get; set; } = "";
        public List<TallasProducto> Tallas { get; set; } = new();
    }
}