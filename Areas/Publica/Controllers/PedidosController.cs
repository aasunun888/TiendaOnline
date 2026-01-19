using Microsoft.AspNetCore.Mvc;
using System;
using System.Security.Claims;
using Microsoft.Data.SqlClient;
using System.Data;
using TiendaOnline.Areas.Publica.Models;
using TiendaOnline.Entidades;

namespace TiendaOnline.Areas.Publica.Controllers
{
    [Area("Publica")]
    [Route("")]
    public class PedidosController : Controller
    {
        private readonly string conexion = "Server=DESKTOP-RODNH5U\\SQLEXPRESS;Database=StreetSize;Trusted_Connection=True;TrustServerCertificate=True;";

        // GET: /usuario/pedidos  -> lista de pedidos del usuario
        [Route("usuario/pedidos")]
        public IActionResult Index()
        {
            if (!TryGetCurrentUserId(out int usuarioId))
            {
                // Si no está logueado, redirigir al login
                return RedirectToAction("Login", "Usuario");
            }

            var vm = new PedidosViewModel();

            string query = @"SELECT Id, UsuarioId, FechaCreacion, Total
                             FROM Pedidos
                             WHERE UsuarioId = @UsuarioId
                             ORDER BY FechaCreacion DESC";

            try
            {
                using (SqlConnection conn = new SqlConnection(conexion))
                {
                    conn.Open();
                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@UsuarioId", usuarioId);
                        using (var reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                var pedido = new Pedido
                                {
                                    Id = reader.GetInt32(reader.GetOrdinal("Id")),
                                    UsuarioId = reader.GetInt32(reader.GetOrdinal("UsuarioId")),
                                    FechaCreacion = reader["FechaCreacion"] == DBNull.Value ? DateTime.Now : reader.GetDateTime(reader.GetOrdinal("FechaCreacion")),
                                    Total = reader["Total"] == DBNull.Value ? 0m : reader.GetDecimal(reader.GetOrdinal("Total"))
                                };
                                vm.ListadoPedidos.Add(pedido);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }

            ViewBag.Breadcrumb = "Home / Usuario / Pedidos";
            // Aunque no haya pedidos, devolvemos la vista con vm vacío; la vista mostrará el mensaje correspondiente.
            return View("~/Areas/Publica/Views/Usuario/Pedidos.cshtml", vm);
        }

        // GET: /usuario/pedidos/{id} -> detalle de un pedido (solo si pertenece al usuario)
        [Route("usuario/pedidos/{id}")]
        public IActionResult Detalle(int id)
        {
            if (!TryGetCurrentUserId(out int usuarioId))
                return RedirectToAction("Login", "Usuario");

            var vm = new PedidosViewModel();

            string queryPedido = @"SELECT Id, UsuarioId, FechaCreacion, Total
                                   FROM Pedidos
                                   WHERE Id = @Id AND UsuarioId = @UsuarioId";

            string queryItems = @"SELECT pi.Id, pi.PedidoId, pi.ProductoId, pi.Cantidad, pi.Precio,
                                         p.Nombre AS ProductoNombre, p.ImagenUrl, tp.Talla, pi.TallaProductoId
                                  FROM PedidoItems pi
                                  LEFT JOIN Productos p ON p.Id = pi.ProductoId
                                  LEFT JOIN TallasProducto tp ON tp.Id = pi.TallaProductoId
                                  WHERE pi.PedidoId = @PedidoId";

            try
            {
                using (SqlConnection conn = new SqlConnection(conexion))
                {
                    conn.Open();

                    using (SqlCommand cmd = new SqlCommand(queryPedido, conn))
                    {
                        cmd.Parameters.AddWithValue("@Id", id);
                        cmd.Parameters.AddWithValue("@UsuarioId", usuarioId);

                        using (var reader = cmd.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                vm.PedidoSeleccionado = new Pedido
                                {
                                    Id = reader.GetInt32(reader.GetOrdinal("Id")),
                                    UsuarioId = reader.GetInt32(reader.GetOrdinal("UsuarioId")),
                                    FechaCreacion = reader["FechaCreacion"] == DBNull.Value ? DateTime.Now : reader.GetDateTime(reader.GetOrdinal("FechaCreacion")),
                                    Total = reader["Total"] == DBNull.Value ? 0m : reader.GetDecimal(reader.GetOrdinal("Total"))
                                };
                            }
                            else
                            {
                                return NotFound();
                            }
                        }
                    }

                    using (SqlCommand cmdItems = new SqlCommand(queryItems, conn))
                    {
                        cmdItems.Parameters.AddWithValue("@PedidoId", id);
                        using (var reader = cmdItems.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                var item = new PedidoItem
                                {
                                    Id = reader.GetInt32(reader.GetOrdinal("Id")),
                                    PedidoId = reader.GetInt32(reader.GetOrdinal("PedidoId")),
                                    ProductoId = reader.GetInt32(reader.GetOrdinal("ProductoId")),
                                    Cantidad = reader.GetInt32(reader.GetOrdinal("Cantidad")),
                                    Precio = reader["Precio"] == DBNull.Value ? 0m : reader.GetDecimal(reader.GetOrdinal("Precio")),
                                    ProductoNombre = reader["ProductoNombre"] == DBNull.Value ? "" : reader.GetString(reader.GetOrdinal("ProductoNombre")),
                                    ImagenUrl = reader["ImagenUrl"] == DBNull.Value ? "" : reader.GetString(reader.GetOrdinal("ImagenUrl")),
                                    TallaProductoId = reader["TallaProductoId"] == DBNull.Value ? null : (int?)reader.GetInt32(reader.GetOrdinal("TallaProductoId")),
                                    Talla = reader["Talla"] == DBNull.Value ? "" : reader.GetString(reader.GetOrdinal("Talla"))
                                };
                                vm.PedidoSeleccionado.Items.Add(item);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }

            ViewBag.Breadcrumb = $"Home / Usuario / Pedido / {vm.PedidoSeleccionado?.Id}";
            return View("~/Areas/Publica/Views/Usuario/DetallePedido.cshtml", vm);
        }

        // Helper para obtener id de usuario actual desde claims
        private bool TryGetCurrentUserId(out int usuarioId)
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