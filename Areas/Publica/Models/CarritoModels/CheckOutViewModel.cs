using TiendaOnline.Entidades;
using System.Collections.Generic;

namespace TiendaOnline.Areas.Publica.Models.CarritoModels
{
    public class CheckoutViewModel
    {
        public Usuarios Usuario { get; set; } = new Usuarios();
        public Carrito Carrito { get; set; } = new Carrito();
        public decimal Subtotal { get; set; }
        public decimal Total { get; set; }
        public List<string> MetodosPago { get; set; } = new List<string> { "Tarjeta", "Bizum", "PayPal" };
        public string MetodoPagoSeleccionado { get; set; } = "Tarjeta";
    }
}