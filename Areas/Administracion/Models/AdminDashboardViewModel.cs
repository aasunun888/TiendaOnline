using System;
using System.Collections.Generic;
using TiendaOnline.Entidades;

namespace TiendaOnline.Areas.Administracion.Models
{
    public class AdminDashboardViewModel
    {
            public List<ProductoSummary> Productos { get; set; } = new();
        public List<CategoriaSummary> Categorias { get; set; } = new();
        public List<UsuarioSummary> Usuarios { get; set; } = new();
        public List<PedidoSummary> Pedidos { get; set; } = new();

        // Búsquedas por pestaña
        public string BuscarProductos { get; set; } = "";
        public string BuscarCategorias { get; set; } = "";
        public string BuscarUsuarios { get; set; } = "";
        public string BuscarPedidos { get; set; } = "";
    }

    public class ProductoSummary
    {
        public int Id { get; set; }
        public string Nombre { get; set; } = "";
        public decimal Precio { get; set; }
        public int? CategoriaId { get; set; }
        public DateTime FechaCreacion { get; set; }
        public bool Activo { get; set; }
    }

    public class CategoriaSummary
    {
        public int Id { get; set; }
        public string Nombre { get; set; } = "";

        public int ProductoContador { get; set; }
    }

    public class UsuarioSummary
    {
        public int Id { get; set; }
        public string Nombre { get; set; } = "";
        public string Apellido { get; set; } = "";
        public string Email { get; set; } = "";
        public int RolId { get; set; }

        public string NombreRol { get; set; } = "";
        public bool Activo { get; set; }
    }

    public class PedidoSummary
    {
        public int Id { get; set; }
        public int UsuarioId { get; set; }
        public DateTime FechaCreacion { get; set; }
        public decimal Total { get; set; }

    }
}