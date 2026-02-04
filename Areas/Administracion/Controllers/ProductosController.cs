using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using System.Data;
using TiendaOnline.Areas.Administracion.Models;

namespace TiendaOnline.Areas.Administracion.Controllers
{
    [Area("Administracion")]
    [Route("administracion/productos")]
    [Authorize(Roles = "Administrador")]
    public class ProductosController : Controller
    {
        private readonly string conexion = "Server=DESKTOP-RODNH5U\\SQLEXPRESS;Database=StreetSize;Trusted_Connection=True;TrustServerCertificate=True;";

        [HttpGet("nuevo")]
        public IActionResult Nuevo()
        {
            var vm = new ProductoCreacionViewModel();
            // Cargar lista de categorías para el select
            using var conn = new SqlConnection(conexion);
            conn.Open();
            using var cmd = new SqlCommand("SELECT Id, Nombre FROM Categorias ORDER BY Nombre", conn);
            using var da = new SqlDataAdapter(cmd);
            var dt = new DataTable();
            da.Fill(dt);
            foreach (DataRow r in dt.Rows)
            {
                vm.Categorias.Add(new CategoriaSummary
                {
                    Id = r.Field<int>("Id"),
                    Nombre = r["Nombre"] == DBNull.Value ? "" : r.Field<string>("Nombre")!
                });
            }
            return View("~/Areas/Administracion/Views/Productos/Nuevo.cshtml", vm);
        }

        [HttpPost("nuevo")]
        [ValidateAntiForgeryToken]
        public IActionResult Nuevo([FromForm] ProductoCreacionViewModel model)
        {
            if (string.IsNullOrWhiteSpace(model.Nombre))
            {
                ModelState.AddModelError("Nombre", "El nombre es obligatorio.");
            }
            if (!ModelState.IsValid)
            {
                // recargar categorias
                using var conn2 = new SqlConnection(conexion);
                conn2.Open();
                using var cmd2 = new SqlCommand("SELECT Id, Nombre FROM Categorias ORDER BY Nombre", conn2);
                using var da2 = new SqlDataAdapter(cmd2);
                var dt2 = new DataTable();
                da2.Fill(dt2);
                model.Categorias.Clear();
                foreach (DataRow r in dt2.Rows)
                    model.Categorias.Add(new CategoriaSummary { Id = r.Field<int>("Id"), Nombre = r.Field<string>("Nombre") ?? "" });
                return View("~/Areas/Administracion/Views/Productos/Nuevo.cshtml", model);
            }

            try
            {
                using var conn = new SqlConnection(conexion);
                conn.Open();
                var sql = @"INSERT INTO Productos (Nombre, Descripcion, Precio, Color, ImagenUrl, CategoriaId, FechaCreacion)
                            VALUES (@Nombre, @Descripcion, @Precio, @Color, @ImagenUrl, @CategoriaId, GETDATE());
                            SELECT CAST(SCOPE_IDENTITY() AS INT);";
                using var cmd = new SqlCommand(sql, conn);
                cmd.Parameters.AddWithValue("@Nombre", model.Nombre);
                cmd.Parameters.AddWithValue("@Descripcion", (object)model.Descripcion ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@Precio", model.Precio);
                cmd.Parameters.AddWithValue("@Color", (object)model.Color ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@ImagenUrl", (object)model.ImagenUrl ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@CategoriaId", model.CategoriaId);
                var idObj = cmd.ExecuteScalar();
                TempData["AdminSuccess"] = "Producto creado correctamente.";
            }
            catch (Exception ex)
            {
                TempData["AdminError"] = "Error al crear producto: " + ex.Message;
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
                using var tran = conn.BeginTransaction();
                try
                {
                    // Borrar tallas asociadas
                    using (var cmd = new SqlCommand("DELETE FROM TallasProducto WHERE ProductoId = @id", conn, tran))
                    {
                        cmd.Parameters.AddWithValue("@id", id);
                        cmd.ExecuteNonQuery();
                    }
                    // Borrar referencias en carrito
                    using (var cmd = new SqlCommand("DELETE CI FROM CarritoItem CI WHERE CI.ProductoId = @id", conn, tran))
                    {
                        cmd.Parameters.AddWithValue("@id", id);
                        cmd.ExecuteNonQuery();
                    }
                    // Borrar referencias en pedidos
                    using (var cmd = new SqlCommand("DELETE PI FROM PedidoItem PI WHERE PI.ProductoId = @id", conn, tran))
                    {
                        cmd.Parameters.AddWithValue("@id", id);
                        cmd.ExecuteNonQuery();
                    }
                    // Borrar producto
                    using (var cmd = new SqlCommand("DELETE FROM Productos WHERE Id = @id", conn, tran))
                    {
                        cmd.Parameters.AddWithValue("@id", id);
                        cmd.ExecuteNonQuery();
                    }

                    tran.Commit();
                    TempData["AdminSuccess"] = "Producto eliminado correctamente.";
                }
                catch
                {
                    tran.Rollback();
                    throw;
                }
            }
            catch (Exception ex)
            {
                TempData["AdminError"] = "No se pudo eliminar el producto: " + ex.Message;
            }

            return RedirectToAction("Index", "Dashboard", new { area = "Administracion" });
        }
    }
}