namespace TiendaOnline.Entidades
{
    //Entidad que representa un producto en la tienda en línea
    public class Producto
    {
        public int Id { get; set; }
        public string Nombre { get; set; } = "";
        public string Descripcion { get; set; } = "";
        public decimal Precio { get; set; }
        public string Color { get; set; } = "";
        public int? CategoriaId { get; set; }
          public Categoria? Categoria { get; set; }
        public string ImagenUrl { get; set; } = "";
        public DateTime FechaCreacion { get; set; }

        public bool Activo { get; set; } = true;


        // Relación con tallas
        public List<TallasProducto> TallasProducto { get; set; } = new List<TallasProducto>();
    }
}
