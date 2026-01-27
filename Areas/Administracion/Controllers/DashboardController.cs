using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using System.Data;
using TiendaOnline.Areas.Administracion.Models;

namespace TiendaOnline.Areas.Admin.Controllers
{
    [Area("Administracion")]
    [Route("administracion")]
    [Authorize(Roles = "Administrador")]
    public class DashboardController : Controller
    {
        private readonly string conexion = "Server=DESKTOP-RODNH5U\\SQLEXPRESS;Database=StreetSize;Trusted_Connection=True;TrustServerCertificate=True;";

        [HttpGet("")]
        public IActionResult Index()
        {
            var vm = new AdminDashboardViewModel();

            try
            {
                using var conn = new SqlConnection(conexion);
                conn.Open();

                // Productos (resumen)
                using (var cmd = new SqlCommand(@"SELECT TOP 200 Id, Nombre, Precio, CategoriaId, FechaCreacion FROM Productos ORDER BY FechaCreacion DESC", conn))
                using (var da = new SqlDataAdapter(cmd))
                {
                    var dt = new DataTable();
                    da.Fill(dt);
                    foreach (DataRow r in dt.Rows)
                    {
                        vm.Productos.Add(new ProductSummary
                        {
                            Id = r.Field<int>("Id"),
                            Nombre = r["Nombre"] == DBNull.Value ? "" : r.Field<string>("Nombre")!,
                            Precio = r.Field<decimal>("Precio"),
                            CategoriaId = r.Field<int>("CategoriaId"),
                            FechaCreacion = r["FechaCreacion"].Equals(DBNull.Value) ? DateTime.MinValue : r.Field<DateTime>("FechaCreacion")
                        });
                    }
                }

                // Categorías
                using (var cmd = new SqlCommand(@"SELECT Id, Nombre, Descripcion FROM Categorias ORDER BY Nombre", conn))
                using (var da = new SqlDataAdapter(cmd))
                {
                    var dt = new DataTable();
                    da.Fill(dt);
                    foreach (DataRow r in dt.Rows)
                    {
                        vm.Categorias.Add(new CategorySummary
                        {
                            Id = r.Field<int>("Id"),
                            Nombre = r["Nombre"] == DBNull.Value ? "" : r.Field<string>("Nombre")!,
                            Descripcion = r["Descripcion"] == DBNull.Value ? "" : r.Field<string>("Descripcion")!
                        });
                    }
                }

                // Usuarios
                using (var cmd = new SqlCommand(@"SELECT Id, Nombre, Apellido, Email, RolId, Activo, FechaRegistro FROM Usuarios ORDER BY FechaRegistro DESC", conn))
                using (var da = new SqlDataAdapter(cmd))
                {
                    var dt = new DataTable();
                    da.Fill(dt);
                    foreach (DataRow r in dt.Rows)
                    {
                        vm.Usuarios.Add(new UserSummary
                        {
                            Id = r.Field<int>("Id"),
                            Nombre = r["Nombre"] == DBNull.Value ? "" : r.Field<string>("Nombre")!,
                            Apellido = r["Apellido"] == DBNull.Value ? "" : r.Field<string>("Apellido")!,
                            Email = r["Email"] == DBNull.Value ? "" : r.Field<string>("Email")!,
                            RolId = r["RolId"] == DBNull.Value ? 0 : r.Field<int>("RolId"),
                            Activo = r["Activo"] == DBNull.Value ? false : r.Field<bool>("Activo")
                        });
                    }
                }

                // Pedidos (resumen)
                using (var cmd = new SqlCommand(@"SELECT TOP 200 Id, UsuarioId, FechaCreacion, Total FROM Pedidos ORDER BY FechaCreacion DESC", conn))
                using (var da = new SqlDataAdapter(cmd))
                {
                    var dt = new DataTable();
                    da.Fill(dt);
                    foreach (DataRow r in dt.Rows)
                    {
                        vm.Pedidos.Add(new OrderSummary
                        {
                            Id = r.Field<int>("Id"),
                            UsuarioId = r.Field<int>("UsuarioId"),
                            FechaCreacion = r["FechaCreacion"].Equals(DBNull.Value) ? DateTime.MinValue : r.Field<DateTime>("FechaCreacion"),
                            Total = r.Field<decimal>("Total")
                        });
                    }
                }
            }
            catch (Exception ex)
            {
               
                Console.WriteLine(ex.Message);
            }

            return View("~/Areas/Administracion/Views/Index.cshtml", vm);
        }
    }
}