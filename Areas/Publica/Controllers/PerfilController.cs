using Microsoft.AspNetCore.Mvc;
using System;
using Microsoft.Data.SqlClient;
using System.Security.Claims;
using TiendaOnline.Entidades;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using TiendaOnline.Areas.Publica.Models.UsuarioModels;

namespace TiendaOnline.Areas.Publica.Controllers
{
    [Area("Publica")]
    [Route("")]
    public class PerfilController : Controller
    {
        private readonly AppDbContext _context;

        public PerfilController(AppDbContext context)
        {
            _context = context;
        }
        
        private readonly string conexion = "Server=DESKTOP-RODNH5U\\SQLEXPRESS;Database=StreetSize;Trusted_Connection=True;TrustServerCertificate=True;";

        // GET: /usuario/perfil
        [Route("usuario/perfil")]

        public async Task<IActionResult> Index()
        {
            if (!ObtenerIdUsuario(out int usuarioId))
                return RedirectToAction("Login", "Usuario");

            var vm = new PerfilViewModel();

            //obtener usuario
            var usuario = await _context.Usuarios.FindAsync(usuarioId);

            vm.Usuario.Id = usuario.Id;
            vm.Usuario.Nombre = usuario.Nombre;
            vm.Usuario.Apellido = usuario.Apellido;
            vm.Usuario.Email = usuario.Email;
            vm.Usuario.Telefono = usuario.Telefono;
            vm.Usuario.Direccion = usuario.Direccion;
            vm.Usuario.Ciudad = usuario.Ciudad;
            vm.Usuario.CodigoPostal = usuario.CodigoPostal;
            vm.Usuario.FechaRegistro = usuario.FechaRegistro;
            vm.Usuario.Activo = usuario.Activo;
        
           
            return View("~/Areas/Publica/Views/Usuario/Perfil.cshtml", vm);
        }

        // POST: /usuario/perfil/actualizar
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Route("usuario/perfil/actualizar")]
        public async Task<IActionResult> Actualizar(PerfilViewModel model)
        {
            if (!ObtenerIdUsuario(out int usuarioId))
                return RedirectToAction("Login", "Usuario");

            if (model == null || model.Usuario == null)
                return BadRequest();

            if (model.Usuario.Id != usuarioId)
                return RedirectToAction("Login", "Usuario");


            //Obtener usuario
            try
            {

                var usuario = await _context.Usuarios.FindAsync(usuarioId);

                //Actualizar campos editables del formulario
                usuario.Nombre = string.IsNullOrWhiteSpace(model.Usuario.Nombre) ? usuario.Nombre : model.Usuario.Nombre.Trim(); //Si no esta vacio, sera el modelo, si no, mantiene el mismo
                usuario.Apellido = model.Usuario.Apellido ?? usuario.Apellido;
                usuario.Telefono = model.Usuario.Telefono;
                usuario.Direccion = model.Usuario.Direccion ?? usuario.Direccion;
                usuario.Ciudad = model.Usuario.Ciudad ?? usuario.Ciudad;
                usuario.CodigoPostal = model.Usuario.CodigoPostal ?? usuario.CodigoPostal;

                TempData["PerfilMensaje"] = "Usuario Actualizado correctamente";


                await _context.SaveChangesAsync();

            }
            catch (Exception e)
            {
                TempData["PerfilError"] = "Error al actualizar usuario por " + e;

            }
            return RedirectToAction("Perfil", "Usuario");

        }

        /*Metodo para desactivar  cuenta de usuario. 
         * Se desactiva el usuario y se eliminan los items del carrito para evitar problemas
         *  El usuario ya no puede iniciar sesión . Sus datos siguen intactos . Puede reactivar si quiere
        
        public async Task<IActionResult> DesacativarCuenta()
        {
            try
            {
                //obtener usuario
                var usuario = await _context.Usuarios.FindAsync(TryGetCurrentUserId(out int usuarioId));

                if(usuario == null)
                {
                    TempData["EliminarError"] = "Usuario no encontrado.";
                    return View("Login");
                }

                //Obtener carrito ID
                var carritoId = await _context.Carrito
                    .Where(c => c.UsuarioId == usuario.Id)
                    .Select(c => c.Id)
                    .FirstOrDefaultAsync();

                //Vaciar sus items de su carrito
                var carrito = await _context.CarritoItem
                    .Where(ci => ci.CarritoId == carritoId)
                    .ToListAsync();
                _context.CarritoItem.RemoveRange(carrito);

                //Desactivar usuario
                await _context.Usuarios
                    .Where(u => u.Id == usuario.Id)
                    .ExecuteUpdateAsync(u => u.SetProperty(p => p.Activo, false));

                await _context.SaveChangesAsync();

            }
            catch(Exception ex)
            {
               TempData["EliminarError"] = "Error al desactivar la cuenta. Por: " + ex;
            }

            return View("Login");
         */

        /*  Eliminar cuenta 
         *  Se borra todos los datos del usuario
         *  Se quedan pedidos anonimizados para tener un historial de estos
         *  Se marca como false el activo del usuario para evitar que pueda iniciar sesión con esa cuenta eliminada
         */
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EliminarCuenta() // <-- POST: /usuario/perfil/eliminar 
        {
            if (!ObtenerIdUsuario(out int usuarioId))
                return RedirectToAction("Login", "Usuario");

            var strategy = _context.Database.CreateExecutionStrategy(); // Estrategia de reintentos para manejar transacciones en EF Core
            Exception savedException = null;

            try
            {
                await strategy.ExecuteAsync(async () =>
                {
                    await using var tran = await _context.Database.BeginTransactionAsync();
                    try
                    {
                        var usuario = await _context.Usuarios.FindAsync(usuarioId);
                        if (usuario == null)
                        {
                            await tran.RollbackAsync();
                            throw new InvalidOperationException("Usuario no encontrado.");
                        }

                        // Anonimizar datos del usuario
                        usuario.Nombre = "Usuario Eliminado";
                        usuario.Apellido = "";
                        usuario.Telefono = "";
                        usuario.Direccion = "";
                        usuario.Ciudad = "";
                        usuario.CodigoPostal = "";
                        usuario.ContraseñaHash = "";
                        usuario.Email = $"deleted_user_{usuario.Id}@deleted.local";
                        usuario.Activo = false;
                        usuario.RolId = 1;

                        // Eliminar carrito e items asociados
                        var carrito = await _context.Carrito.FirstOrDefaultAsync(c => c.UsuarioId == usuarioId);
                        if (carrito != null)
                        {
                            var items = await _context.CarritoItem
                                .Where(ci => ci.CarritoId == carrito.Id)
                                .ToListAsync();
                            if (items.Count > 0)
                                _context.CarritoItem.RemoveRange(items);
                            _context.Carrito.Remove(carrito);
                        }

                        // Guardar cambios y confirmar transacción
                        await _context.SaveChangesAsync();
                        await tran.CommitAsync();
                    }
                    catch (Exception e)
                    {
                        await tran.RollbackAsync();
                        savedException = e;
                        throw;
                    }
                });
            }
            catch (Exception e)
            {
                savedException ??= e;
            }

            // Manejo seguro de HttpContext 
            if (savedException != null)
            {
                TempData["EliminarError"] = "Error al eliminar la cuenta. Por: " + savedException.Message;
            }
            else
            {
                try
                {
                    // Cerrar sesión y limpiar cookies
                    await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
                    await HttpContext.SignOutAsync();
                    // Eliminar cookies de autenticación
                    foreach (var cookie in Request.Cookies.Keys)
                        Response.Cookies.Delete(cookie);
                    HttpContext.User = new ClaimsPrincipal(new ClaimsIdentity());
                }
                catch
                {
                    // no bloquear por fallo en signout
                }
                TempData["PerfilMensaje"] = "Cuenta eliminada correctamente.";
            }

            return RedirectToAction("LoginView", "Usuario");
        }


        // Helper para obtener id de usuario actual desde claims
        private bool ObtenerIdUsuario(out int usuarioId)
        {
            usuarioId = 0;
            var usuarioIdClaim = User.FindFirstValue(ClaimTypes.Sid) ?? User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(usuarioIdClaim) || !int.TryParse(usuarioIdClaim, out usuarioId))
            {
                return false;
            }
            return true;
        }
    }
}
