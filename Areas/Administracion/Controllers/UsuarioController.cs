using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using TiendaOnline.Areas.Administracion.Models;
using TiendaOnline.Areas.Administracion.Models.UsuarioModels;
using TiendaOnline.Entidades;

namespace TiendaOnline.Areas.Administracion.Controllers
{

    [Area("Administracion")]
    [Authorize(Roles = "Administrador")]
    public class UsuarioController : Controller
    {
        private readonly AppDbContext _context;

        public UsuarioController(AppDbContext context)
        {
            _context = context;
        }
        [HttpGet]    
        public async Task<ActionResult> Index(int Id)
        {
            var usuario = await _context.Usuarios.FindAsync(Id);
            
            if(usuario == null)
            {
                TempData["AdminError"] = "El usuario no existe.";
                return RedirectToAction("Index", "Dashboard", new { area = "Administracion" });
            }

            var vm = new UsuarioViewModel
            {
                Id = usuario.Id,
                Nombre = usuario.Nombre,
                Apellido = usuario.Apellido,
                Email = usuario.Email,
                Telefono = usuario.Telefono,
                Direccion = usuario.Direccion,
                Ciudad = usuario.Ciudad,
                CodigoPostal = usuario.CodigoPostal,
                RolId = usuario.RolId,
                FechaRegistro = usuario.FechaRegistro,
                Activo = usuario.Activo
            };


            // Cargar pedidos de usuario
            vm.Pedidos = await _context.Pedidos
                .Where(p => p.UsuarioId == Id)
                .OrderByDescending(p => p.FechaCreacion)
                 .Select(p => new PedidoSummary
                 {
                     Id = p.Id,
                     UsuarioId = p.UsuarioId,
                     FechaCreacion = p.FechaCreacion,
                     Total = p.Total
                 })
                .ToListAsync();
            
            
            return View("Index",vm);
        }

        // Cambiar de rol de usuario
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CambiarRol(int id) 
        {
            try
            {
                // obtener rol del usuario
                var rolUsuario = await _context.Usuarios
                    .Where(u => u.Id == id)
                    .Select(u => u.RolId)
                    .FirstOrDefaultAsync();

                // obtener id del usuario actualmente logueado (si existe)
                var usuarioActualLogueado = User.FindFirstValue(ClaimTypes.Sid);
                int? IdActual = null;
                if (!string.IsNullOrEmpty(usuarioActualLogueado) && int.TryParse(usuarioActualLogueado, out var idParseado))
                {
                    IdActual = idParseado;
                }

                // Prevención: si el admin intenta quitarse a sí mismo el rol, no permitirlo
                if (IdActual.HasValue && IdActual.Value == id && rolUsuario == 2)
                {
                    TempData["AdminError"] = "No puedes quitarte el rol de Administrador mientras estés logueado. Pide a otro administrador que lo haga.";
                    return RedirectToAction("Index", new { Id = id });
                }

                // Si es 1 -> pasar a 2, si es 2 -> pasar a 1. Evita asignar el mismo rol dos veces.
                if (rolUsuario == 1)
                {
                    await _context.Usuarios
                        .Where(u => u.Id == id)
                        .ExecuteUpdateAsync(s => s.SetProperty(u => u.RolId, 2));
                    TempData["AdminSuccess"] = "El usuario " + id + " ha sido cambiado a Administrador.";
                }
                else if (rolUsuario == 2)
                {
                    await _context.Usuarios
                        .Where(u => u.Id == id)
                        .ExecuteUpdateAsync(s => s.SetProperty(u => u.RolId, 1));
                    TempData["AdminSuccess"] = "El usuario " + id + " ha sido cambiado a Cliente.";
                }
                else
                {
                    TempData["AdminError"] = "Rol de usuario desconocido.";
                }
            }
            catch (Exception ex)
            {
                TempData["AdminError"] = "Error al cambiar el rol del usuario: " + ex.Message;
            }

            // Volver a la misma página de detalle de usuario
            return RedirectToAction("Index", new { Id = id });
        }

    }
}
