using Microsoft.Data.SqlClient;
using System.Data;
using TiendaOnline.Entidades;

namespace TiendaOnline.Areas.Publica.Models
{
    // ViewModel para representar los productos en la vista pública
    public class ProductoViewModel
    {
        public List<Producto> ListadoProductos { get; set; } = [];
        public string Nombre { get; set; } = "";
        public string Descripcion { get; set; } = "";
        public string ImagenUrl { get; set; } = "";
        public double precio { get; set; }
        public string color { get; set; } = "";

     
      

    }
}