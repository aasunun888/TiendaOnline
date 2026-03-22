using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using System.Data;
using TiendaOnline.Areas.Administracion.Models;
using TiendaOnline.Areas.Administracion.Models.ProductoModels;
using TiendaOnline.Entidades;


namespace TiendaOnline.Areas.Administracion.Controllers
{
    [Area("Administracion")]
    [Authorize(Roles = "Administrador")]
    public class CategoriasController : Controller
    {
        private readonly AppDbContext _context;

        public CategoriasController(AppDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> Nuevo()
        {
            var vm = new CategoriaCreacionViewModel();
            // Cargar lista de categorías para el select usando EF (async para no bloquear)
            var categorias = await _context.Categorias
                .AsNoTracking()
                .OrderBy(c => c.Nombre)
                .Select(c => new CategoriaSummary { Id = c.Id, Nombre = c.Nombre })
                .ToListAsync();
            vm.Categorias.AddRange(categorias);
            return View("Nuevo", vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> NuevaCategoriaPost([FromForm] CategoriaCreacionViewModel model)
        {
            if (string.IsNullOrWhiteSpace(model.Nombre))
            {
                ModelState.AddModelError("Nombre", "El nombre es obligatorio.");
            }

            // Normalizar para comparar
            var nombreNormalizado = model.Nombre?.Trim();
            if (!string.IsNullOrWhiteSpace(nombreNormalizado))
            {
                var existe = await _context.Categorias
                    .AsNoTracking()
                    .AnyAsync(c => c.Nombre.ToLower() == nombreNormalizado.ToLower());
                if (existe)
                {
                    ModelState.AddModelError("Nombre", "Ya existe una categoría con ese nombre.");
                }
            }

            if (!ModelState.IsValid)
            {
                // Volver a poblar la lista de categorías para la vista
                var vm = new CategoriaCreacionViewModel
                {
                    Nombre = model.Nombre ?? ""
                };

                var categorias = await _context.Categorias
                    .AsNoTracking()
                    .OrderBy(c => c.Nombre)
                    .Select(c => new CategoriaSummary { Id = c.Id, Nombre = c.Nombre })
                    .ToListAsync();

                vm.Categorias.AddRange(categorias);
                return View("Nuevo", vm);
            }

            try
            {
                // Crear y guardar la nueva categoría
                var nueva = new Categoria
                {
                    Nombre = nombreNormalizado!
                };

                _context.Categorias.Add(nueva);
                await _context.SaveChangesAsync();

                TempData["AdminSuccess"] = "Categoría creada correctamente.";
            }
            catch (Exception ex)
            {
                TempData["AdminError"] = "Error al crear categoría: " + ex.Message;
                // Redirigir a la vista de creación para evitar quedarse en un estado inconsistente
                return RedirectToAction("Nuevo");
            }

            return RedirectToAction("Index", "Dashboard", new { area = "Administracion" });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EliminarCategoria(int id)
        {
            try
            {
                //Recoger categoria
                var categoria = await _context.Categorias.FindAsync(id);

                if (categoria == null)
                {
                    TempData["AdminError"] = "La categoria no existe.";
                    return RedirectToAction("Index", "Dashboard", new { area = "Administracion" });
                }
                // Deshabilitar productos asociados 
                var productos = await _context.Productos.Where(p => p.CategoriaId == id).ToListAsync();
                if (productos.Any())
                {
                    foreach (var p in productos)
                    {
                        p.CategoriaId = null;
                        p.Activo = false;
                    }

                    // Guardar cambios antes de eliminar la categoría
                    await _context.SaveChangesAsync();
                }



                // Eliminarla de la BD
                _context.Categorias.Remove(categoria);
                await _context.SaveChangesAsync();


                TempData["AdminSuccess"] = "Categoría eliminada. Los productos asociados se deshabilitaron automáticamente.";
            }
            catch (Exception ex)
            {
                TempData["AdminError"] = "No se pudo eliminar la categoria: " + ex.Message;
            }

            return RedirectToAction("Index", "Dashboard", new { area = "Administracion" });
        }

        /*Al editar permitir que se pueda cambiar el nombre de la categoría, pero no eliminarla. Si se quiere eliminar, que se use el botón de eliminar. Esto es para evitar problemas con productos 
         * asociados que podrían quedar sin categoría o con categorías huérfanas.
         */

        [HttpGet]
        public async Task<IActionResult> EditarCategoria(int Id)
        {
            var vm = new CategoriaCreacionViewModel();

            //Cargar Categoria elegida
            var categoriaElegida = await _context.Categorias.FindAsync(Id);

            if (categoriaElegida == null)
            {
                TempData["AdminError"] = "La categoria no existe.";
                return RedirectToAction("Index", "Dashboard", new { area = "Administracion" });
            }

            //Almacenar en VM
            vm.Id = categoriaElegida.Id;
            vm.Nombre = categoriaElegida.Nombre;

            // Cargar lista de categorías para el select usando EF (async para no bloquear)
            var categorias = await _context.Categorias
                .AsNoTracking()
                .OrderBy(c => c.Nombre)
                .Select(c => new CategoriaSummary { Id = c.Id, Nombre = c.Nombre })
                .ToListAsync();
            vm.Categorias.AddRange(categorias);

            // Cargar productos enlazados a esta categoría
            var productosEnlazados = await _context.Productos
                .AsNoTracking()
                .Where(p => p.CategoriaId == Id)
                .OrderByDescending(p => p.FechaCreacion)
                .Select(p => new ProductoSummary
                {
                    Id = p.Id,
                    Nombre = p.Nombre,
                    Precio = p.Precio,
                    CategoriaId = p.CategoriaId,
                    FechaCreacion = p.FechaCreacion,
                    Activo = p.Activo
                })
                .ToListAsync();
            vm.ProductosEnlazados.AddRange(productosEnlazados);

            return View("Editar", vm);

        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditarCategoriaPost([FromForm] CategoriaCreacionViewModel vm)
        {

            // Validación mínima
            if (vm == null)
            {
                TempData["AdminError"] = "Datos inválidos.";
                return RedirectToAction("Index", "Dashboard", new { area = "Administracion" });
            }

            if (string.IsNullOrWhiteSpace(vm.Nombre))
            {
                TempData["AdminError"] = "El nombre es obligatorio.";
                return RedirectToAction("EditarCategoria", new { Id = vm.Id });
            }

            var nombreNormalizado = vm.Nombre.Trim();

            // Comprobar nombre duplicado en otra categoría
            var existe = await _context.Categorias
                .AsNoTracking()
                .AnyAsync(c => c.Nombre.ToLower() == nombreNormalizado.ToLower() && c.Id != vm.Id);

            if (existe)
            {
                TempData["AdminError"] = "Ya existe otra categoría con ese nombre.";
                return RedirectToAction("EditarCategoria", new { Id = vm.Id });
            }

            try
            {
                var categoria = await _context.Categorias.FindAsync(vm.Id);

                if (categoria == null)
                {
                    TempData["AdminError"] = "La categoria no existe.";
                    return RedirectToAction("Index", "Dashboard", new { area = "Administracion" });
                }

                // Actualizar solo el campo editable
                categoria.Nombre = nombreNormalizado;

                // Guardar cambios 
                await _context.SaveChangesAsync();

                TempData["AdminSuccess"] = "Categoría actualizada.";
            }
            catch (Exception ex)
            {
                TempData["AdminError"] = "No se pudo actualizar la categoria: " + ex.Message;
            }

            // Redirigir al GET para seguir PRG y mostrar mensajes via TempData
            return RedirectToAction("EditarCategoria", new { Id = vm.Id });
        }
    }
}