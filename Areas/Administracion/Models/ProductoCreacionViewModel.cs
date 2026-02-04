using System.Collections.Generic;

namespace TiendaOnline.Areas.Administracion.Models
{
    public class ProductoCreacionViewModel
    {
        public string Nombre { get; set; } = "";
        public string Descripcion { get; set; } = "";
        public decimal Precio { get; set; }
        public string Color { get; set; } = "";
        public string ImagenUrl { get; set; } = "";
        public int CategoriaId { get; set; }
        public List<CategoriaSummary> Categorias { get; set; } = new();
    }
}