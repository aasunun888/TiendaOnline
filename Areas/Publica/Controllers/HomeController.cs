using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Data;
using TiendaOnline.Areas.Publica.Models;
using TiendaOnline.Entidades;

namespace TiendaOnline.Areas.Publica.Controllers
{
    [Area("Publica")]
    public class HomeController : Controller
    {
        private readonly AppDbContext _context;

        public HomeController(AppDbContext context)
        {
            _context = context;
        }

        // GET: HomeController
        [HttpGet]
        public async Task<ActionResult> Index()
        {
            var modelo = new HomeViewModel
            {
                Titulo = "StreetSize",
                Descripcion = "¡Vístete con confianza y destaca en cada ocasión!.",
                ImagenUrl = "/images/51f7191bd47f34a131a3aa1d766b71ef.jpg"
            };

            return View(modelo);
        }

        // Funcion novedades para mostrar productos nuevos, en este caso todos son nuevos
        [HttpGet]
        public ActionResult NovedadesGet()
        {
            ViewBag.Breadcrumb = "Home / Novedades"; //ruta miga de pan

            return View("Novedades");
        }

        /* Carrusel de los 5 productos más vendidos.
         * Calcula agregando las cantidades en PedidoItem y devuelve una PartialView con la lista de productos.
         */
        [HttpGet]
        public async Task<IActionResult> ProductosMasVendidos()
        {
            try
            {
                // Agrupar por ProductoId y sumar cantidades
                var productosTop = await _context.PedidoItem
                    .GroupBy(pi => pi.ProductoId)
                    .Select(p => new { ProductoId = p.Key, TotalVendido = p.Sum(pi => pi.Cantidad) }) //Almacenamos la "key" del group by como ProductoId y la suma de cantidades como TotalVendido
                    .OrderByDescending(x => x.TotalVendido)
                    .Take(5) //Tomamos los 5 más vendidos
                    .ToListAsync();

                List<Producto> productos; // Lista para almacenar los productos que se mostrarán en el carrusel

                if (productosTop == null || productosTop.Count == 0)
                {
                    // Si no hay ventas registradas, fallback a los 5 productos más recientes
                    productos = await _context.Productos
                        .Where(p => p.Activo)
                        .OrderByDescending(p => p.FechaCreacion)
                        .Take(5)
                        .ToListAsync();
                }
                else
                {
                    // Extraer los ids de los productos en el orden del ranking
                    var idsEnOrden = productosTop.Select(t => t.ProductoId).ToList();

                    // Traer los productos que correspondan a esos ids
                    var productosReales = await _context.Productos
                        .Where(p => idsEnOrden.Contains(p.Id) && p.Activo)
                        .ToDictionaryAsync(p => p.Id);

                    // Mantener el orden según el ranking calculado
                    productos = idsEnOrden
                        .Where(id => productosReales.ContainsKey(id))
                        .Select(id => productosReales[id])
                        .ToList();
                }

                // Devuelve una vista shared con la lista (creada abajo)
                return PartialView("_CarruselProductos", productos);
            }
            catch (Exception ex)
            {
                // Log simple por ahora y devolver partial vacía para no romper la UI
                Console.WriteLine($"Error al obtener productos más vendidos: {ex.Message}");
                return PartialView("_CarruselProductos", new List<Producto>());
            }
        }
    }
}
