using TiendaOnline.Entidades;

namespace TiendaOnline.Areas.Administracion.Models.ProductoModels
{
    public class ProductoEdicionViewModel
    {
        public int Id { get; set; }
        public string Nombre { get; set; } = "";
        public string Descripcion { get; set; } = "";
        public decimal Precio { get; set; }
        public string Color { get; set; } = "";
        public string ImagenUrl { get; set; } = "";
        public int? CategoriaId { get; set; }
        public bool Activo { get; set; } = true;

        public string CategoriaNombre { get; set; } = "";

        //  Añadir tallas 
        public List<TallasProducto> Tallas { get; set; } = new();

    }
}
