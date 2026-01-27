using System;
using System.Collections.Generic;

namespace TiendaOnline.Areas.Administracion.Models
{
    public class AdminDashboardViewModel
    {
        public List<ProductSummary> Productos { get; set; } = new();
        public List<CategorySummary> Categorias { get; set; } = new();
        public List<UserSummary> Usuarios { get; set; } = new();
        public List<OrderSummary> Pedidos { get; set; } = new();
    }

    public class ProductSummary
    {
        public int Id { get; set; }
        public string Nombre { get; set; } = "";
        public decimal Precio { get; set; }
        public int CategoriaId { get; set; }
        public DateTime FechaCreacion { get; set; }
    }

    public class CategorySummary
    {
        public int Id { get; set; }
        public string Nombre { get; set; } = "";
        public string Descripcion { get; set; } = "";
    }

    public class UserSummary
    {
        public int Id { get; set; }
        public string Nombre { get; set; } = "";
        public string Apellido { get; set; } = "";
        public string Email { get; set; } = "";
        public int RolId { get; set; }
        public bool Activo { get; set; }
    }

    public class OrderSummary
    {
        public int Id { get; set; }
        public int UsuarioId { get; set; }
        public DateTime FechaCreacion { get; set; }
        public decimal Total { get; set; }
    }
}