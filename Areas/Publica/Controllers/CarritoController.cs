using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using TiendaOnline.Entidades;
using System.Security.Claims;

namespace TiendaOnline.Areas.Publica.Controllers
{

    [Area("Publica")]
    [Route("")]


   
    public class CarritoController : Controller
    {
        string conexion = "Server=DESKTOP-RODNH5U\\SQLEXPRESS;Database=StreetSize;Trusted_Connection=True;TrustServerCertificate=True;";

        [Route("Carrito/Index")]
        [HttpGet]
        // Index: Mostrar carrito
        public ActionResult Index(int UsuarioId)
        {
            var carrito = ObtenerCarrito(UsuarioId);

            return View(carrito);

        }

        [Route("Carrito/Agregar")]
        [HttpGet]
        // Añadir producto
        public IActionResult Agregar(int productoId)
        {
            //Obtener el Id del usuario desde las claims
            var usuarioIdClaim = User.FindFirstValue(ClaimTypes.Sid);
            if (string.IsNullOrEmpty(usuarioIdClaim))
            {
                // Si no está logueado, redirigir al login (Evita que añada si no está logueado)
                return RedirectToAction("Login", "Usuario");
            }
            int usuarioId = int.Parse(usuarioIdClaim);

            //Obtener carrito del usuario
            var carrito = ObtenerCarrito(usuarioId);

            //Si no existe carrito, lo creo
            if (carrito == null)
            {
                carrito = CrearCarrito(usuarioId);
            }

            //Ver si el producto ya está en el carrito
            var item = carrito.Items.FirstOrDefault(i => i.ProductoId == productoId);
            if (item != null)
            {
                ActualizarCantidad(item.Id, item.Cantidad + 1);
            }
            else
            {
                InsertarItem(carrito.Id, productoId, 1);
            }

            //Redirigir al carrito del usuario
            return RedirectToAction("Index", new { usuarioId });
        }


        [Route("Carrito/Restar")]
        [HttpPost]
        // Restar cantidad
        public IActionResult Restar(int usuarioId, int productoId)
        {
            var carrito = ObtenerCarrito(usuarioId);
            var item = carrito?.Items.FirstOrDefault(i => i.ProductoId == productoId);

            if (item != null)
            {
                if (item.Cantidad > 1)
                    ActualizarCantidad(item.Id, item.Cantidad - 1);
                else
                    EliminarItem(item.Id);
            }

            return RedirectToAction("Index", new { usuarioId });
        }

        [Route("Carrito/Eliminar")]
        [HttpPost]
        // Eliminar producto
        public IActionResult Eliminar(int usuarioId, int productoId)
        {
            var carrito = ObtenerCarrito(usuarioId);
            var item = carrito?.Items.FirstOrDefault(i => i.ProductoId == productoId);

            if (item != null)
                EliminarItem(item.Id);

            return RedirectToAction("Index", new { usuarioId });
        }

        /* METODOS PRIVADOS DE CONEXION CON LA BASE DE DATOS */
        private Carrito ObtenerCarrito(int usuarioId)
        {
            Carrito carrito = null;


            using (var connection = new SqlConnection(conexion))
            {
                connection.Open();

                var cmd = new SqlCommand("SELECT TOP 1 * FROM Carrito WHERE UsuarioId = @usuarioId", connection);
                cmd.Parameters.AddWithValue("@usuarioId", usuarioId);

                using (var reader = cmd.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        carrito = new Carrito
                        {
                            Id = (int)reader["Id"],
                            UsuarioId = (int)reader["UsuarioId"],
                            FechaCreacion = (DateTime)reader["FechaCreacion"]
                        };
                    }
                }

                if (carrito != null)
                {
                    carrito.Items = new List<CarritoItem>();

                    var cmdItems = new SqlCommand("SELECT * FROM CarritoItem WHERE CarritoId = @carritoId", connection);
                    cmdItems.Parameters.AddWithValue("@carritoId", carrito.Id);

                    using (var reader = cmdItems.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            carrito.Items.Add(new CarritoItem
                            {
                                Id = (int)reader["Id"],
                                CarritoId = (int)reader["CarritoId"],
                                ProductoId = (int)reader["ProductoId"],
                                Cantidad = (int)reader["Cantidad"]
                            });
                        }
                    }
                }
            }

            return carrito;
        }

        private Carrito CrearCarrito(int usuarioId)
        {
            using (var connection = new SqlConnection(conexion))
            {
                connection.Open();

                var cmd = new SqlCommand("INSERT INTO Carrito (UsuarioId, FechaCreacion) OUTPUT INSERTED.Id VALUES (@usuarioId, GETDATE())", connection);
                cmd.Parameters.AddWithValue("@usuarioId", usuarioId);

                int id = (int)cmd.ExecuteScalar();

                return new Carrito { Id = id, UsuarioId = usuarioId, FechaCreacion = DateTime.Now };
            }
        }
        private void InsertarItem(int carritoId, int productoId, int cantidad)
        {
            using (var connection = new SqlConnection(conexion))
            {
                connection.Open();

                var cmd = new SqlCommand("INSERT INTO CarritoItem (CarritoId, ProductoId, Cantidad) VALUES (@carritoId, @productoId, @cantidad)", connection);
                cmd.Parameters.AddWithValue("@carritoId", carritoId);
                cmd.Parameters.AddWithValue("@productoId", productoId);
                cmd.Parameters.AddWithValue("@cantidad", cantidad);

                cmd.ExecuteNonQuery();
            }
        }

        private void ActualizarCantidad(int itemId, int cantidad)
        {
            using (var connection = new SqlConnection(conexion))
            {
                connection.Open();

                var cmd = new SqlCommand("UPDATE CarritoItem SET Cantidad = @cantidad WHERE Id = @id", connection);
                cmd.Parameters.AddWithValue("@cantidad", cantidad);
                cmd.Parameters.AddWithValue("@id", itemId);

                cmd.ExecuteNonQuery();
            }
        }
        private void EliminarItem(int itemId)
        {
            using (var connection = new SqlConnection(conexion))
            {
                connection.Open();

                var cmd = new SqlCommand("DELETE FROM CarritoItem WHERE Id = @id", connection);
                cmd.Parameters.AddWithValue("@id", itemId);

                cmd.ExecuteNonQuery();
            }
        }
    }
}
