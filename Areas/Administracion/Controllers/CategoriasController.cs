using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using System.Data;
using TiendaOnline.Areas.Administracion.Models;

namespace TiendaOnline.Areas.Administracion.Controllers
{
    [Area("Administracion")]
    [Route("administracion/categorias")]
    [Authorize(Roles = "Administrador")]
    public class CategoriasController : Controller
    {
        private readonly string conexion = "Server=DESKTOP-RODNH5U\\SQLEXPRESS;Database=StreetSize;Trusted_Connection=True;TrustServerCertificate=True;";

        [HttpGet("nuevo")] 
        public IActionResult Nuevo()
        {
            return View("~/Areas/Administracion/Views/Categorias/Nuevo.cshtml", new CategoriaCreacionViewModel());
        }

        [HttpPost("nuevo")]
        [ValidateAntiForgeryToken]
        public IActionResult Nuevo([FromForm] CategoriaCreacionViewModel model)
        {
            if (string.IsNullOrWhiteSpace(model.Nombre))
            {
                ModelState.AddModelError("Nombre", "El nombre es obligatorio.");
                return View("~/Areas/Administracion/Views/Categorias/Nuevo.cshtml", model);
            }

            try
            {
                using var conn = new SqlConnection(conexion);
                conn.Open();
                using var cmd = new SqlCommand("INSERT INTO Categorias (Nombre) VALUES (@Nombre)", conn);
                cmd.Parameters.AddWithValue("@Nombre", model.Nombre);
                cmd.ExecuteNonQuery();
                TempData["AdminSuccess"] = "Categoría creada correctamente.";
            }
            catch (Exception ex)
            {
                TempData["AdminError"] = "Error al crear categoría: " + ex.Message;
            }

            return RedirectToAction("Index", "Dashboard", new { area = "Administracion" });
        }

        [HttpPost("eliminar")]
        [ValidateAntiForgeryToken]
        public IActionResult Eliminar(int id)
        {
            try
            {
                using var conn = new SqlConnection(conexion);
                conn.Open();

                // Comprobar si hay productos asociados
                using (var cmdCheck = new SqlCommand("SELECT COUNT(1) FROM Productos WHERE CategoriaId = @id", conn))
                {
                    cmdCheck.Parameters.AddWithValue("@id", id);
                    var count = (int)cmdCheck.ExecuteScalar();
                    if (count > 0)
                    {
                        TempData["AdminError"] = "No se puede eliminar la categoría: existen productos asociados.";
                        return RedirectToAction("Index", "Dashboard", new { area = "Administracion" });
                    }
                }

                using (var cmd = new SqlCommand("DELETE FROM Categorias WHERE Id = @id", conn))
                {
                    cmd.Parameters.AddWithValue("@id", id);
                    cmd.ExecuteNonQuery();
                }
                TempData["AdminSuccess"] = "Categoría eliminada correctamente.";
            }
            catch (Exception ex)
            {
                TempData["AdminError"] = "No se pudo eliminar la categoría: " + ex.Message;
            }

            return RedirectToAction("Index", "Dashboard", new { area = "Administracion" });
        }
    }
}