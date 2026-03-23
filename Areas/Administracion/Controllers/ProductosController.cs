using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Data;
using TiendaOnline.Areas.Administracion.Models;
using TiendaOnline.Areas.Administracion.Models.ProductoModels;
using TiendaOnline.Entidades;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace TiendaOnline.Areas.Administracion.Controllers
{
    [Area("Administracion")]
    [Authorize(Roles = "Administrador")]
    public class ProductosController : Controller
    {
        private readonly AppDbContext _context;

        public ProductosController(AppDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> Nuevo()
        {
            var vm = new ProductoCreacionViewModel();
            // Cargar lista de categorías para el select usando EF (async para no bloquear)
            var categorias = await _context.Categorias
                .AsNoTracking()
                .OrderBy(c => c.Nombre)
                .Select(c => new CategoriaSummary { Id = c.Id, Nombre = c.Nombre })
                .ToListAsync();
            vm.Categorias.AddRange(categorias);
            return View("~/Areas/Administracion/Views/Productos/Nuevo.cshtml", vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken] //Seguridad contra falsificación de solicitudes entre sitios (CSRF)
        public async Task<IActionResult> Nuevo([FromForm] ProductoCreacionViewModel model)
        {
            if (string.IsNullOrWhiteSpace(model.Nombre))
            {
                ModelState.AddModelError("Nombre", "El nombre es obligatorio.");
            }
            if (!ModelState.IsValid)
            {
                // recargar categorias usando EF (async)
                model.Categorias.Clear();
                var categorias = await _context.Categorias
                    .AsNoTracking()
                    .OrderBy(c => c.Nombre)
                    .Select(c => new CategoriaSummary { Id = c.Id, Nombre = c.Nombre })
                    .ToListAsync();
                model.Categorias.AddRange(categorias);
                return View("~/Areas/Administracion/Views/Productos/Nuevo.cshtml", model);
            }

            try
            {
                var producto = new Producto
                {
                    Nombre = model.Nombre,
                    Descripcion = model.Descripcion,
                    Precio = model.Precio,
                    Color = model.Color,
                    ImagenUrl = model.ImagenUrl,
                    CategoriaId = model.CategoriaId,
                    FechaCreacion = DateTime.Now
                };

                // Ejecutar la transacción dentro de la ExecutionStrategy para soportar reintentos
                var strategy = _context.Database.CreateExecutionStrategy();
                await strategy.ExecuteAsync(async () =>
                {
                    await using var tran = await _context.Database.BeginTransactionAsync();
                    try
                    {
                        _context.Productos.Add(producto);
                        await _context.SaveChangesAsync();
                        await tran.CommitAsync();
                    }
                    catch
                    {
                        await tran.RollbackAsync();
                        throw;
                    }
                });

                TempData["AdminSuccess"] = "Producto creado correctamente.";
            }
            catch (Exception ex)
            {
                TempData["AdminError"] = "Error al crear producto: " + ex.Message;
            }

            return RedirectToAction("Index", "Dashboard", new { area = "Administracion" });
        }
        /*
         En lugar de eliminar físicamente el producto,
         lo marcamos como inactivo para mantener la integridad de los datos relacionados con pedidos y carritos.
         */
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeshabilitarProducto(int id)
        {
            try
            {
                var producto = await _context.Productos.FindAsync(id);

                if (producto == null)
                {
                    TempData["AdminError"] = "El producto no existe.";
                    return RedirectToAction("Index", "Dashboard", new { area = "Administracion" });
                }

                // Marcar como inactivo
                producto.Activo = false;
               
               


                // Borrar tallas solo si no están en pedidos
                var tallasEnPedidos = await _context.PedidoItem.AnyAsync(p => p.ProductoId == id);

                if (!tallasEnPedidos)
                {
                    var tallas = await _context.TallasProducto.Where(t => t.ProductoId == id).ToListAsync();
                    _context.TallasProducto.RemoveRange(tallas);
                }

                // Borrar carrito
                var carrito = await _context.CarritoItem.Where(c => c.ProductoId == id).ToListAsync();
                _context.CarritoItem.RemoveRange(carrito);

                await _context.SaveChangesAsync();

                TempData["AdminSuccess"] = "Producto desactivado correctamente.";
            }
            catch (Exception ex)
            {
                TempData["AdminError"] = "No se pudo eliminar el producto: " + ex.Message;
            }

            return RedirectToAction("Index", "Dashboard", new { area = "Administracion" });
        }

        // Rehabilitar producto 
        [HttpGet]
        public async Task<IActionResult> HabilitarProducto(int id)
        {
            var producto = await _context.Productos
                .AsNoTracking()
                .FirstOrDefaultAsync(p => p.Id == id);

            if (producto == null)
            {
                TempData["AdminError"] = "Producto no encontrado.";
                return RedirectToAction("Index", "Dashboard", new { area = "Administracion" });
            }

            var categorias = await _context.Categorias
                .AsNoTracking()
                .OrderBy(c => c.Nombre)
                .Select(c => new CategoriaSummary { Id = c.Id, Nombre = c.Nombre })
                .ToListAsync();


            var vm = new ProductoReactivarViewModel
            {
                Id = producto.Id,
                Nombre = producto.Nombre,
                Tallas = await _context.TallasProducto
                            .AsNoTracking()
                            .Where(t => t.ProductoId == id)
                            .Select(t => new ProductoReactivarViewModel.TallaInput { Talla = t.Talla, Stock = t.Stock })
                            .ToListAsync(),
                Categorias = categorias
            };

            // Si la categoría actual del producto aún existe, precargarla; si no, dejar 0 (obligará al admin a seleccionar)
            var categoriaExiste = producto.CategoriaId.HasValue &&
            await _context.Categorias.AnyAsync(c => c.Id == producto.CategoriaId.Value);
            vm.CategoriaId = categoriaExiste ? producto.CategoriaId.Value : 0;


            // Si no hay tallas previas, precargamos una fila vacía para facilitar la entrada
            if (!vm.Tallas.Any())
            {
                vm.Tallas.Add(new ProductoReactivarViewModel.TallaInput());
            }

            return View("~/Areas/Administracion/Views/Productos/Habilitar.cshtml", vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> HabilitarProducto(ProductoReactivarViewModel model)
        {
            try
            {
                var producto = await _context.Productos.FindAsync(model.Id);
                if (producto == null)
                {
                    TempData["AdminError"] = "El producto no existe.";
                    return RedirectToAction("Index", "Dashboard", new { area = "Administracion" });
                }

                // Validar que se ha seleccionado categoría válida (no 0) y que existe
                if (model.CategoriaId == 0 || !await _context.Categorias.AnyAsync(c => c.Id == model.CategoriaId))
                {
                    // Re-popular lista de categorias para volver a mostrar la vista con error
                    model.Categorias = await _context.Categorias
                        .AsNoTracking()
                        .OrderBy(c => c.Nombre)
                        .Select(c => new CategoriaSummary { Id = c.Id, Nombre = c.Nombre })
                        .ToListAsync();

                    ModelState.AddModelError("CategoriaId", "Debe seleccionar una categoría válida antes de reactivar el producto.");
                    return View("Habilitar", model);
                }


                // Eliminar tallas existentes (asegura estado limpio)
                var existentes = await _context.TallasProducto.Where(t => t.ProductoId == model.Id).ToListAsync();
                if (existentes.Any())
                {
                    _context.TallasProducto.RemoveRange(existentes);
                }

                // Insertar tallas nuevas desde el formulario (solo filas válidas)
                var nuevas = model.Tallas
                    .Where(t => !string.IsNullOrWhiteSpace(t.Talla) && t.Stock >= 0)
                    .Select(t => new TallasProducto
                    {
                        ProductoId = model.Id,
                        Talla = t.Talla!.Trim(),
                        Stock = t.Stock
                    })
                    .ToList();

                if (nuevas.Any())
                {
                    await _context.TallasProducto.AddRangeAsync(nuevas);
                }

                // Asignar categoría y reactivar producto
                producto.CategoriaId = model.CategoriaId;
                producto.Activo = true;

                await _context.SaveChangesAsync();

                TempData["AdminSuccess"] = "Producto reactivado correctamente.";
            }
            catch (Exception ex)
            {
                TempData["AdminError"] = "No se pudo reactivar el producto: " + ex.Message;
            }

            return RedirectToAction("Index", "Dashboard", new { area = "Administracion" });
        }



        [HttpGet]
        public async Task<IActionResult> Editar(int id)
        {
            //Traer producto usando EF para mostrar sus datos en el formulario de edición
            var producto = await _context.Productos
                .AsNoTracking()
                .FirstOrDefaultAsync(p => p.Id == id);

            if (producto == null)
            {
                TempData["AdminError"] = "Producto no encontrado.";
                return RedirectToAction("Index", "Dashboard", new { area = "Administracion" });
            }

            var vm = new ProductoEdicionViewModel();

            //Rellenar campos de producto
            vm.Id = producto.Id;
            vm.Nombre = producto.Nombre;
            vm.Descripcion = producto.Descripcion;
            vm.Precio = producto.Precio;
            vm.Color = producto.Color;
            vm.ImagenUrl = producto.ImagenUrl;
            vm.CategoriaId = producto.CategoriaId;
            vm.Activo = producto.Activo;

            //Convertir id en nombre de categoría para mostrar en el select
            var categoriaNombre = await _context.Categorias
                .AsNoTracking()
                .Where(c => c.Id == producto.CategoriaId)
                .Select(c => c.Nombre)
                .FirstOrDefaultAsync() ?? "";

            vm.CategoriaNombre = categoriaNombre;

            //Añadir apartado de tallas
            vm.Tallas = await _context.TallasProducto
                .AsNoTracking()
                .Where(t => t.ProductoId == id)
                .ToListAsync();

            return View("Editar", vm);
        }


        /*Editar POST*/
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditarPost(ProductoEdicionViewModel model)
        {
            try
            {
                //Recoger producto de DB
                var producto = await _context.Productos.FindAsync(model.Id);
                if (producto == null)
                {
                    TempData["AdminError"] = "Producto no encontrado.";
                    return RedirectToAction("Index", "Dashboard", new { area = "Administracion" });
                }

                //Actualizar campos editables del formulario
                producto.Nombre = string.IsNullOrWhiteSpace(model.Nombre) ? producto.Nombre : model.Nombre.Trim(); //Si no esta vacio, sera el modelo, si no, mantiene el mismo
                producto.Descripcion = model.Descripcion ?? producto.Descripcion;
                producto.Precio = model.Precio;
                producto.Color = model.Color ?? producto.Color;
                producto.ImagenUrl = model.ImagenUrl ?? omitirNullImg(producto.ImagenUrl, model.ImagenUrl);

                // Mantenero o actualizar estado activo según el modelo
                producto.Activo = model.Activo;

                await _context.SaveChangesAsync();

                TempData["AdminSuccess"] = "Producto actualizado correctamente.";

            }
            catch (Exception ex)
            {
                TempData["AdminError"] = "No se pudo actualizar el producto: " + ex.Message;
            }
            return RedirectToAction("Editar", new { id = model.Id });

        }

        /*Metodo auxiliar para evitar null en imagen*/
        private static string omitirNullImg(string valorActual, string? nuevo)
        {
            return string.IsNullOrWhiteSpace(nuevo) ? valorActual : nuevo;
        }

        /*Administrar tallas en metodo get para mayor simplicidad, aunque se podría hacer con AJAX para no recargar toda la página al añadir o eliminar tallas.
         */
        [HttpGet]
        public async Task<IActionResult> AdministrarTallas(int id)
        {
            //Recoger producto de DB
            var producto = await _context.Productos
                .AsNoTracking()
                .FirstOrDefaultAsync(p => p.Id == id);

            if (producto == null)
            {
                TempData["AdminError"] = "Producto no encontrado.";
                return RedirectToAction("Index", "Dashboard", new { area = "Administracion" });
            }
            //Crear ViewModel específico para administrar tallas, con datos del producto y sus tallas actuales
            var vm = new ProductoTallasViewModel
            {
                ProductoId = producto.Id,
                Nombre = producto.Nombre,
                Tallas = await _context.TallasProducto
                            .AsNoTracking()
                            .Where(t => t.ProductoId == id)
                            .ToListAsync()
            };

            // Si no hay tallas, dejar una fila vacía para facilidad
            if (!vm.Tallas.Any())
            {
                vm.Tallas.Add(new TallasProducto { Talla = "", Stock = 0 });
            }

            return View("AdministrarTallas", vm);
        }

        // Guardar cambios de tallas (POST)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AdministrarTallas(ProductoTallasViewModel model)
        {
            try
            {
                //LLamda a BD asincrona 
                var producto = await _context.Productos.FindAsync(model.ProductoId);
                if (producto == null)
                {
                    TempData["AdminError"] = "Producto no encontrado.";
                    return RedirectToAction("Index", "Dashboard", new { area = "Administracion" });
                }

                // Eliminar tallas existentes del producto
                var existentes = await _context.TallasProducto.Where(t => t.ProductoId == model.ProductoId).ToListAsync();
                if (existentes.Any())
                {
                    _context.TallasProducto.RemoveRange(existentes);
                }

                // Insertar las tallas enviadas por el formulario (filtrar filas inválidas)
                var nuevas = model.Tallas
                    .Where(t => !string.IsNullOrWhiteSpace(t.Talla) && t.Stock >= 0)
                    .Select(t => new TallasProducto
                    {
                        ProductoId = model.ProductoId,
                        Talla = t.Talla!.Trim(),
                        Stock = t.Stock
                    })
                    .ToList();

                if (nuevas.Any())
                {
                    await _context.TallasProducto.AddRangeAsync(nuevas);
                }

                // Guardar cambios iniciales (eliminación + posibles inserciones)
                await _context.SaveChangesAsync();

                // Comprobar si el producto se quedó sin tallas y deshabilitarlo automáticamente en ese caso
                var totalTallas = await _context.TallasProducto.CountAsync(t => t.ProductoId == model.ProductoId);
                if (totalTallas == 0)
                {
                    producto.Activo = false;
                    await _context.SaveChangesAsync();
                    TempData["AdminSuccess"] = "Tallas actualizadas. El producto se ha deshabilitado porque no quedan tallas.";
                }
                else
                {
                    TempData["AdminSuccess"] = "Tallas actualizadas correctamente.";
                }
            }
            catch (Exception ex)
            {
                TempData["AdminError"] = "No se pudieron actualizar las tallas: " + ex.Message;
            }

            return RedirectToAction("Editar", new { id = model.ProductoId });
        }

    }
}