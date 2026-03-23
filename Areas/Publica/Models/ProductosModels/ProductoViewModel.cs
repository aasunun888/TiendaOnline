using Microsoft.Data.SqlClient;
using System.Data;
using TiendaOnline.Entidades;

namespace TiendaOnline.Areas.Publica.Models.ProductosModels
{
    // ViewModel para representar los productos en la vista pública
    public class ProductoViewModel
    {
        public List<Producto> ListadoProductos { get; set; } = [];
        public int Id { get; set; }
        public string Nombre { get; set; } = "";
        public string Descripcion { get; set; } = "";
        public decimal Precio { get; set; }
        public string Color { get; set; } = "";
        public int CategoriaId { get; set; }
        public string ImagenUrl { get; set; } = "";
        public DateTime FechaCreacion { get; set; }
        public bool Activo { get; set; } = true;

        // Propiedad calculada
        public string CategoriaNombre { get; set; } = "";

        // Relación con tallas
        public List<TallasProducto> TallasProducto { get; set; } = new List<TallasProducto>();

        // Lista de categorías disponibles (llenada desde BD para el select en la vista Buscar)
        public List<Categoria> Categorias { get; set; } = new List<Categoria>();

    }
}