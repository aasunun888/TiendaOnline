using System.Collections.Generic;

namespace TiendaOnline.Areas.Administracion.Models.ProductoModels
{
    public class ProductoReactivarViewModel
    {
        public int Id { get; set; }
        public string Nombre { get; set; } = "";

        public List<TallaInput> Tallas { get; set; } = new();

        //Añadir categorias para reactivacion
        public int CategoriaId { get; set; } = 0;
        public List<CategoriaSummary> Categorias { get; set; } = new();

        public class TallaInput
        {
            public string Talla { get; set; } = "";
            public int Stock { get; set; } = 0;
        }
    }
}