using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TiendaOnline.Areas.Administracion.Models.Pedidos;
using TiendaOnline.Entidades;

namespace TiendaOnline.Areas.Administracion.Controllers
{
    [Area("Administracion")]
    [Authorize(Roles = "Administrador")]
    public class PedidosController : Controller
    {
        private readonly AppDbContext _context;

        public PedidosController(AppDbContext context)
        {
            _context = context;
        }

        // GET: /administracion/pedidos/detalle/{id}
        [HttpGet("administracion/pedido/detalle/{id}")]
        public async Task<IActionResult> Detalle(int id)
        {
            // Cargar pedido
            var pedido = await _context.Pedidos
                .AsNoTracking()
                .FirstOrDefaultAsync(p => p.Id == id);

            if (pedido == null)
            {
                TempData["AdminError"] = "Pedido no encontrado.";
                return RedirectToAction("Index", "Dashboard", new { area = "Administracion" });
            }

            // Cargar items del pedido con proyección a DTO (evita traer entidades completas si no hace falta)
            // JOIN explícito a Productos y TallasProducto porque la entidad PedidoItem no tiene propiedades de navegación
            var items = await (from pi in _context.PedidoItem.AsNoTracking()
                               where pi.PedidoId == id
                               join p in _context.Productos.AsNoTracking() on pi.ProductoId equals p.Id
                               join tp in _context.TallasProducto.AsNoTracking() on pi.TallaProductoId equals tp.Id into tpj
                               from tp in tpj.DefaultIfEmpty()
                               select new PedidoItemDto
                               {
                                   Id = pi.Id,
                                   PedidoId = pi.PedidoId,
                                   ProductoId = pi.ProductoId,
                                   Cantidad = pi.Cantidad,
                                   Precio = pi.Precio,
                                   ProductoNombre = p.Nombre ?? "",
                                   ImagenUrl = p.ImagenUrl ?? "",
                                   Talla = tp != null ? tp.Talla ?? "" : ""
                               })
                              .ToListAsync();

            var vm = new AdminPedidoDetalleViewModel
            {
                PedidoId = pedido.Id,
                UsuarioId = pedido.UsuarioId,
                FechaCreacion = pedido.FechaCreacion,
                Total = pedido.Total,
                Items = items
            };

            // Vista admin propia para detalle de pedido
            return View("~/Areas/Administracion/Views/Pedido/Detalle.cshtml", vm);
        }
    }
}