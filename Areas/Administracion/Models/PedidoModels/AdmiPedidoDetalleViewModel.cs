
using System;
using System.Collections.Generic;

namespace TiendaOnline.Areas.Administracion.Models.Pedidos
{
    public class AdminPedidoDetalleViewModel
    {
        public int PedidoId { get; set; }
        public int UsuarioId { get; set; }
        public DateTime FechaCreacion { get; set; }
        public decimal Total { get; set; }

        public List<PedidoItemDto> Items { get; set; } = new();
    }

    public class PedidoItemDto
    {
        public int Id { get; set; }
        public int PedidoId { get; set; }
        public int ProductoId { get; set; }
        public string ProductoNombre { get; set; } = "";
        public string ImagenUrl { get; set; } = "";
        public string Talla { get; set; } = "";
        public int Cantidad { get; set; }
        public decimal Precio { get; set; }
    }
}
