using TiendaOnline.Entidades;

namespace TiendaOnline.Areas.Administracion.Models
{
    public class CategoriaCreacionViewModel
    {

        public int Id { get; set; }
        public string Nombre { get; set; } = "";

        public List<CategoriaSummary> Categorias { get; set; } = new();

        public List<ProductoSummary> ProductosEnlazados { get; set; } = new();
    }
}