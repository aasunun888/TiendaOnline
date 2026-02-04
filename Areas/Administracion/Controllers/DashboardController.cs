using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using System.Data;
using TiendaOnline.Areas.Administracion.Models;

namespace TiendaOnline.Areas.Administracion.Controllers
{
    [Area("Administracion")]
    [Route("administracion")]
    [Authorize(Roles = "Administrador")]
    public class DashboardController : Controller
    {
        private readonly string conexion = "Server=DESKTOP-RODNH5U\\SQLEXPRESS;Database=StreetSize;Trusted_Connection=True;TrustServerCertificate=True;";

        // Acepta parámetros opcionales para búsqueda por cada pestaña
        [HttpGet("")]
        public IActionResult Index([FromQuery] string qProductos = null, [FromQuery] string qCategorias = null, [FromQuery] string qUsuarios = null, [FromQuery] string qPedidos = null)
        {
            var vm = new AdminDashboardViewModel
            {
                BuscarProductos = qProductos ?? "",
                BuscarCategorias = qCategorias ?? "",
                BuscarUsuarios = qUsuarios ?? "",
                BuscarPedidos = qPedidos ?? ""
            };

            try
            {
                using var conn = new SqlConnection(conexion);
                conn.Open();

                // Productos (con búsqueda por id o nombre/descripcion)
                {
                    var sql = @"SELECT TOP 200 Id, Nombre, Precio, CategoriaId, FechaCreacion FROM Productos";
                    using var cmd = new SqlCommand();
                    cmd.Connection = conn;

                    if (!string.IsNullOrWhiteSpace(vm.BuscarProductos)) //Si no esta null
                    {
                        if (int.TryParse(vm.BuscarProductos.Trim(), out int pid)) //Diferenciar si es num(id) o texto(nombre/descripcion)
                        {
                            sql += " WHERE Id = @pId OR Nombre LIKE @pLike OR Descripcion LIKE @pLike"; //Agregar condiciones
                            cmd.Parameters.AddWithValue("@pId", pid);
                            cmd.Parameters.AddWithValue("@pLike", $"%{vm.BuscarProductos}%");
                        }
                        else
                        {
                            sql += " WHERE Nombre LIKE @pLike OR Descripcion LIKE @pLike";
                            cmd.Parameters.AddWithValue("@pLike", $"%{vm.BuscarProductos}%");
                        }
                    }

                    sql += " ORDER BY FechaCreacion DESC";
                    cmd.CommandText = sql;

                    using var da = new SqlDataAdapter(cmd);
                    var dt = new DataTable();
                    da.Fill(dt);
                    foreach (DataRow r in dt.Rows)
                    {
                        vm.Productos.Add(new ProductoSummary
                        {
                            Id = r.Field<int>("Id"),
                            Nombre = r["Nombre"] == DBNull.Value ? "" : r.Field<string>("Nombre")!,
                            Precio = r.Field<decimal>("Precio"),
                            CategoriaId = r.Field<int>("CategoriaId"),
                            FechaCreacion = r["FechaCreacion"].Equals(DBNull.Value) ? DateTime.MinValue : r.Field<DateTime>("FechaCreacion")
                        });
                    }
                }

                // Categorías (búsqueda por id o nombre)
                {
                    var sql = @"SELECT Id, Nombre FROM Categorias";
                    using var cmd = new SqlCommand();
                    cmd.Connection = conn;

                    if (!string.IsNullOrWhiteSpace(vm.BuscarCategorias))//Si no esta null
                    {
                        if (int.TryParse(vm.BuscarCategorias.Trim(), out int cid))//Diferenciar si es num(id) o texto(nombre)
                        {
                            sql += " WHERE Id = @cId OR Nombre LIKE @cLike";
                            cmd.Parameters.AddWithValue("@cId", cid);
                            cmd.Parameters.AddWithValue("@cLike", $"%{vm.BuscarCategorias}%");
                        }
                        else
                        {
                            sql += " WHERE Nombre LIKE @cLike";
                            cmd.Parameters.AddWithValue("@cLike", $"%{vm.BuscarCategorias}%");
                        }
                    }

                    sql += " ORDER BY Nombre";
                    cmd.CommandText = sql;

                    using var da = new SqlDataAdapter(cmd);
                    var dt = new DataTable();
                    da.Fill(dt);
                    foreach (DataRow r in dt.Rows)
                    {
                        vm.Categorias.Add(new CategoriaSummary
                        {
                            Id = r.Field<int>("Id"),
                            Nombre = r["Nombre"] == DBNull.Value ? "" : r.Field<string>("Nombre")!,
                        });
                    }
                }

                // Usuarios (búsqueda por id, nombre, apellido, email)
                {
                    var sql = @"SELECT Id, Nombre, Apellido, Email, RolId, Activo, FechaRegistro FROM Usuarios";
                    using var cmd = new SqlCommand();
                    cmd.Connection = conn;

                    if (!string.IsNullOrWhiteSpace(vm.BuscarUsuarios))//Si no esta null
                    {
                        if (int.TryParse(vm.BuscarUsuarios.Trim(), out int uid))//Diferenciar si es num(id) o texto(nombre)
                        {
                            sql += " WHERE Id = @uId OR Nombre LIKE @uLike OR Apellido LIKE @uLike OR Email LIKE @uLike";
                            cmd.Parameters.AddWithValue("@uId", uid);
                            cmd.Parameters.AddWithValue("@uLike", $"%{vm.BuscarUsuarios}%");
                        }
                        else
                        {
                            sql += " WHERE Nombre LIKE @uLike OR Apellido LIKE @uLike OR Email LIKE @uLike";
                            cmd.Parameters.AddWithValue("@uLike", $"%{vm.BuscarUsuarios}%");
                        }
                    }

                    sql += " ORDER BY FechaRegistro DESC";
                    cmd.CommandText = sql;

                    using var da = new SqlDataAdapter(cmd);
                    var dt = new DataTable();
                    da.Fill(dt);
                    foreach (DataRow r in dt.Rows)
                    {
                        vm.Usuarios.Add(new UsuarioSummary
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

                // Pedidos (búsqueda por id o por usuario (nombre/apellido/email))
                {
                    var sql = @"SELECT p.Id, p.UsuarioId, p.FechaCreacion, p.Total
                                FROM Pedidos p
                                LEFT JOIN Usuarios u ON u.Id = p.UsuarioId";
                    using var cmd = new SqlCommand();
                    cmd.Connection = conn;

                    if (!string.IsNullOrWhiteSpace(vm.BuscarPedidos))//Si no esta null
                    {
                        if (int.TryParse(vm.BuscarPedidos.Trim(), out int oid))//Diferenciar si es num(id) o texto(nombre/apellido/email)
                        {
                            sql += " WHERE p.Id = @oId";
                            cmd.Parameters.AddWithValue("@oId", oid);
                        }
                        else
                        {
                            sql += " WHERE u.Nombre LIKE @oLike OR u.Apellido LIKE @oLike OR u.Email LIKE @oLike";
                            cmd.Parameters.AddWithValue("@oLike", $"%{vm.BuscarPedidos}%");
                        }
                    }

                    sql += " ORDER BY p.FechaCreacion DESC";
                    cmd.CommandText = sql;

                    using var da = new SqlDataAdapter(cmd);
                    var dt = new DataTable();
                    da.Fill(dt);
                    foreach (DataRow r in dt.Rows)
                    {
                        vm.Pedidos.Add(new PedidoSummary
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
                // log
                Console.WriteLine(ex.Message);
            }

            return View("~/Areas/Administracion/Views/Index.cshtml", vm);
        }
    }
}