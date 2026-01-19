using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using System.Data;
using System.Security.Claims;
using TiendaOnline.Areas.Publica.Models;
using TiendaOnline.Entidades;
using System.Linq;

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
        public ActionResult Index()
        {

            //Obtener el Id del usuario desde las claims
            var usuarioIdClaim = User.FindFirstValue(ClaimTypes.Sid);

            //Si no hay claim, redirigir a Login
            if (string.IsNullOrEmpty(usuarioIdClaim))
            {
                return Redirect("/auth/login");
            }

            int usuarioId = int.Parse(usuarioIdClaim);

            //Obtener el carrito del usuario
            var carritoEntidad = ObtenerCarrito(usuarioId);

            // Mapear a CarritoViewModel
            var carritoVM = new CarritoViewModel
            {
                Items = carritoEntidad?.Items ?? new List<CarritoItem>()
            };

            return View(carritoVM);
        }

        [Route("Carrito/Agregar")]
        [HttpPost]
        public IActionResult Agregar(int productoId, int cantidad = 1, string talla = null)
        {
            //Validar usuario
            var usuarioIdClaim = User.FindFirstValue(ClaimTypes.Sid);
            if (string.IsNullOrEmpty(usuarioIdClaim))
                return Redirect("/auth/login");

            int usuarioId = int.Parse(usuarioIdClaim);

            //Validar talla seleccionada
            if (string.IsNullOrEmpty(talla))
            {
                TempData["Error"] = "Debes seleccionar una talla.";
                return Redirect(Request.Headers["Referer"].ToString());
            }

            //Validar que la talla sea una de las fijas (S, M, L, XL)
            var tallasValidas = new[] { "S", "M", "L", "XL" };
            if (!tallasValidas.Contains(talla))
            {
                TempData["Error"] = "La talla seleccionada no es válida.";
                return RedirectToAction("Index");
            }

            int tallaProductoId;
            int stock;

            //Buscar la talla en la tabla TallaProducto
            using (var connection = new SqlConnection(conexion))
            {
                connection.Open();

                var cmd = new SqlCommand(
                    "SELECT Id, Stock FROM TallasProducto WHERE ProductoId = @prodId AND Talla = @talla",
                    connection);
                cmd.Parameters.AddWithValue("@prodId", productoId);
                cmd.Parameters.AddWithValue("@talla", talla);

                using (var reader = cmd.ExecuteReader())
                {
                    if (!reader.Read())
                    {
                        TempData["Error"] = "La talla seleccionada no existe para este producto.";
                        return RedirectToAction("Index");
                    }

                    tallaProductoId = reader.GetInt32(0);
                    stock = reader.GetInt32(1);
                }
            }

            //Validar stock
            if (stock < cantidad)
            {
                TempData["Error"] = "No hay stock suficiente para la talla seleccionada.";
                return Redirect(Request.Headers["Referer"].ToString());
            }

            //Obtener o crear carrito
            var carrito = ObtenerCarrito(usuarioId) ?? CrearCarrito(usuarioId);

            //Buscar si ya existe el item con esa talla
            var item = carrito.Items.FirstOrDefault(i => i.ProductoId == productoId && i.TallaProductoId == tallaProductoId);

            //Insertar o actualizar
            if (item != null)
            {
                ActualizarCantidad(item.Id, item.Cantidad + cantidad);
            }
            else
            {
                InsertarItem(carrito.Id, productoId, cantidad, tallaProductoId);
            }

            // Redirigir al index del carrito para mostrar el carrito actualizado
            return RedirectToAction("Index");
        }

        [Route("Carrito/Restar")]
        [HttpGet]
        public IActionResult Restar(int productoId)
        {
            var usuarioIdClaim = User.FindFirstValue(ClaimTypes.Sid);
            if (string.IsNullOrEmpty(usuarioIdClaim))
            {
                return RedirectToAction("Login", "Usuario");
            }
            int usuarioId = int.Parse(usuarioIdClaim);

            //Obtener carrito
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
        [HttpGet]
        // Eliminar producto
        public IActionResult Eliminar(int productoId)

        {
            //Obtener el Id del usuario desde las claims
            var usuarioIdClaim = User.FindFirstValue(ClaimTypes.Sid);
            if (string.IsNullOrEmpty(usuarioIdClaim))
            {
                // Si no está logueado, redirigir al login (Evita que añada si no está logueado)
                return RedirectToAction("Login", "Usuario");
            }
            int usuarioId = int.Parse(usuarioIdClaim);

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

                // Obtener el carrito del usuario
                var cmdCarrito = new SqlCommand("SELECT TOP 1 * FROM Carrito WHERE UsuarioId = @usuarioId", connection);
                cmdCarrito.Parameters.AddWithValue("@usuarioId", usuarioId);

                var dtCarrito = new DataTable();
                using (var adapter = new SqlDataAdapter(cmdCarrito))
                {
                    adapter.Fill(dtCarrito);
                }

                if (dtCarrito.Rows.Count > 0)
                {
                    var fila = dtCarrito.Rows[0];
                    carrito = new Carrito
                    {
                        Id = (int)fila["Id"],
                        UsuarioId = (int)fila["UsuarioId"],
                        FechaCreacion = (DateTime)fila["FechaCreacion"],
                        Items = new List<CarritoItem>()
                    };

                    //Obtener los items del carrito
                    var cmdItems = new SqlCommand("SELECT * FROM CarritoItem WHERE CarritoId = @carritoId", connection);
                    cmdItems.Parameters.AddWithValue("@carritoId", carrito.Id);

                    var dtItems = new DataTable();
                    using (var adapterItems = new SqlDataAdapter(cmdItems))
                    {
                        adapterItems.Fill(dtItems);
                    }

                    foreach (DataRow filaItem in dtItems.Rows)
                    {
                        var item = new CarritoItem
                        {
                            Id = (int)filaItem["Id"],
                            CarritoId = (int)filaItem["CarritoId"],
                            ProductoId = (int)filaItem["ProductoId"],
                            Cantidad = (int)filaItem["Cantidad"],
                            TallaProductoId = filaItem.Table.Columns.Contains("TallaProductoId") && filaItem["TallaProductoId"] != DBNull.Value ? (int)filaItem["TallaProductoId"] : 0
                        };

                        item.Producto = ObtenerProducto(item.ProductoId, connection);

                        if (item.TallaProductoId > 0)
                        {
                            item.TallaProducto = ObtenerTallaProducto(item.TallaProductoId, connection);
                        }

                        carrito.Items.Add(item);
                    }
                }
            }

            return carrito;
        }



        /*METODO AUXILIAR PARA CREAR UN PRODUCTO Y ALMACENARLO EN EL CARRITO*/
        private Producto ObtenerProducto(int productoId, SqlConnection connection)
            
        {
            var cmd = new SqlCommand("SELECT * FROM Productos WHERE Id = @id", connection);
            cmd.Parameters.AddWithValue("@id", productoId);

            var dtProducto = new DataTable();
            using (var adapter = new SqlDataAdapter(cmd))
            {
                adapter.Fill(dtProducto);
            }

            if (dtProducto.Rows.Count > 0)
            {
                var fila = dtProducto.Rows[0];
                return new Producto
                {
                    Id = (int)fila["Id"],
                    Nombre = (string)fila["Nombre"],
                    Precio = (decimal)fila["Precio"],
                    ImagenUrl = (string)fila["ImagenUrl"],
                    Color = (string)fila["Color"],
                    CategoriaId = (int)fila["CategoriaId"]
                };
            }

            return null;
        }

        private TallasProducto ObtenerTallaProducto(int tallaProductoId, SqlConnection connection)
        {
            var cmd = new SqlCommand("SELECT Id, ProductoId, Talla, Stock FROM TallasProducto WHERE Id = @id", connection);
            cmd.Parameters.AddWithValue("@id", tallaProductoId);

            var dt = new DataTable();
            using (var adapter = new SqlDataAdapter(cmd))
            {
                adapter.Fill(dt);
            }

            if (dt.Rows.Count > 0)
            {
                var fila = dt.Rows[0];
                return new TallasProducto
                {
                    Id = (int)fila["Id"],
                    ProductoId = (int)fila["ProductoId"],
                    Talla = fila["Talla"] == DBNull.Value ? "" : (string)fila["Talla"],
                    Stock = fila["Stock"] == DBNull.Value ? 0 : (int)fila["Stock"]
                };
            }

            return null;
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
        private void InsertarItem(int carritoId, int productoId, int cantidad, int tallaProductoId)
        {
            using (var connection = new SqlConnection(conexion))
            {
                connection.Open();

                var cmd = new SqlCommand(
                    "INSERT INTO CarritoItem (CarritoId, ProductoId, Cantidad, TallaProductoId) " +
                    "VALUES (@carritoId, @productoId, @cantidad, @tallaProductoId)", connection);

                cmd.Parameters.AddWithValue("@carritoId", carritoId);
                cmd.Parameters.AddWithValue("@productoId", productoId);
                cmd.Parameters.AddWithValue("@cantidad", cantidad);
                cmd.Parameters.AddWithValue("@tallaProductoId", tallaProductoId);

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

        // GET: /Carrito/Checkout
        [Route("Carrito/Checkout")]
        [HttpGet]
        public IActionResult Checkout()
        {
            // Obtener usuario actual desde claims
            var usuarioIdClaim = User.FindFirstValue(ClaimTypes.Sid);
            if (string.IsNullOrEmpty(usuarioIdClaim))
                return Redirect("/auth/login");

            int usuarioId = int.Parse(usuarioIdClaim);

            // Obtener datos necesarios
            var usuario = ObtenerUsuario(usuarioId);
            var carritoEntidad = ObtenerCarrito(usuarioId) ?? new Carrito { Items = new List<CarritoItem>() };

            var subtotal = carritoEntidad.Items.Sum(i => (i.Producto?.Precio ?? 0m) * i.Cantidad);
            var total = subtotal; // aquí se pueden sumar impuestos/envío si aplica

            var vm = new CheckoutViewModel
            {
                Usuario = usuario ?? new Usuario(),
                Carrito = carritoEntidad,
                Subtotal = subtotal,
                Total = total
            };

            return View("~/Areas/Publica/Views/Carrito/Checkout.cshtml", vm);
        }

        // POST: /Carrito/Finalizar -> recibe datos del checkout (por ejemplo dirección o método de pago)
        [Route("Carrito/Finalizar")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Finalizar([FromForm] CheckoutViewModel model)
        {
            var usuarioIdClaim = User.FindFirstValue(ClaimTypes.Sid);
            if (string.IsNullOrEmpty(usuarioIdClaim))
                return Redirect("/auth/login");

            int usuarioId = int.Parse(usuarioIdClaim);

            // Validar carrito
            var carrito = ObtenerCarrito(usuarioId);
            if (carrito == null || carrito.Items == null || !carrito.Items.Any())
            {
                TempData["CheckoutError"] = "Tu carrito está vacío.";
                return RedirectToAction("Index");
            }

            // Calcular total
            decimal total = carrito.Items.Sum(i => (i.Producto?.Precio ?? 0m) * i.Cantidad);

            // Actualizar datos de usuario (opcional)
            if (model?.Usuario != null)
            {
                try
                {
                    using (var conn = new SqlConnection(conexion))
                    {
                        conn.Open();
                        var update = @"UPDATE Usuarios
                                       SET Telefono = @Telefono,
                                           Direccion = @Direccion,
                                           Ciudad = @Ciudad,
                                           CodigoPostal = @CodigoPostal
                                       WHERE Id = @Id";
                        using (var cmd = new SqlCommand(update, conn))
                        {
                            cmd.Parameters.AddWithValue("@Telefono", (object)model.Usuario.Telefono ?? DBNull.Value);
                            cmd.Parameters.AddWithValue("@Direccion", (object)model.Usuario.Direccion ?? DBNull.Value);
                            cmd.Parameters.AddWithValue("@Ciudad", (object)model.Usuario.Ciudad ?? DBNull.Value);
                            cmd.Parameters.AddWithValue("@CodigoPostal", (object)model.Usuario.CodigoPostal ?? DBNull.Value);
                            cmd.Parameters.AddWithValue("@Id", usuarioId);
                            cmd.ExecuteNonQuery();
                        }
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                    TempData["CheckoutError"] = "No se pudieron guardar los datos de envío.";
                    return RedirectToAction("Checkout");
                }
            }

            // Crear pedido, insertar items, restar stock y vaciar carrito dentro de una transacción
            int nuevoPedidoId = 0;
            try
            {
                using (var conn = new SqlConnection(conexion))
                {
                    conn.Open();
                    using (var tran = conn.BeginTransaction())
                    {
                        try
                        {
                            // 1) Insertar Pedido y obtener Id
                            var insertPedidoSql = @"INSERT INTO Pedidos (UsuarioId, FechaCreacion, Total)
                                                    VALUES (@UsuarioId, GETDATE(), @Total);
                                                    SELECT CAST(SCOPE_IDENTITY() AS INT);";
                            using (var cmdPedido = new SqlCommand(insertPedidoSql, conn, tran))
                            {
                                cmdPedido.Parameters.AddWithValue("@UsuarioId", usuarioId);
                                cmdPedido.Parameters.AddWithValue("@Total", total);
                                var result = cmdPedido.ExecuteScalar();
                                nuevoPedidoId = result != null ? Convert.ToInt32(result) : 0;
                            }

                            if (nuevoPedidoId == 0)
                                throw new Exception("No se pudo crear el pedido.");

                            // 2) Por cada item: verificar stock, restar stock e insertar PedidoItem
                            foreach (var item in carrito.Items)
                            {
                                if (item.Producto == null)
                                    throw new Exception("Producto del carrito no existe.");

                                // Comprobar stock actual de la talla dentro de la transacción
                                var selectStockSql = "SELECT Stock FROM TallasProducto WHERE Id = @TallaProductoId";
                                int stockActual;
                                using (var cmdStock = new SqlCommand(selectStockSql, conn, tran))
                                {
                                    cmdStock.Parameters.AddWithValue("@TallaProductoId", item.TallaProductoId);
                                    var o = cmdStock.ExecuteScalar();
                                    if (o == null)
                                        throw new Exception($"Talla (Id={item.TallaProductoId}) no encontrada para el producto {item.ProductoId}.");
                                    stockActual = Convert.ToInt32(o);
                                }

                                if (stockActual < item.Cantidad)
                                    throw new Exception($"No hay stock suficiente para {item.Producto.Nombre} (talla {item.TallaProducto?.Talla}).");

                                // Restar stock (asegurando no quedar negativo)
                                var updateStockSql = @"UPDATE TallasProducto
                                                       SET Stock = Stock - @Cantidad
                                                       WHERE Id = @TallaProductoId AND Stock >= @Cantidad";
                                using (var cmdUpdateStock = new SqlCommand(updateStockSql, conn, tran))
                                {
                                    cmdUpdateStock.Parameters.AddWithValue("@Cantidad", item.Cantidad);
                                    cmdUpdateStock.Parameters.AddWithValue("@TallaProductoId", item.TallaProductoId);
                                    var filas = cmdUpdateStock.ExecuteNonQuery();
                                    if (filas == 0)
                                        throw new Exception($"No se pudo actualizar stock para la talla Id={item.TallaProductoId}.");
                                }

                                // Insertar PedidoItem (guardar precio actual)
                                var insertItemSql = @"INSERT INTO PedidoItem (PedidoId, ProductoId, Cantidad, Precio, TallaProductoId)
                                                      VALUES (@PedidoId, @ProductoId, @Cantidad, @Precio, @TallaProductoId)";
                                using (var cmdInsertItem = new SqlCommand(insertItemSql, conn, tran))
                                {
                                    cmdInsertItem.Parameters.AddWithValue("@PedidoId", nuevoPedidoId);
                                    cmdInsertItem.Parameters.AddWithValue("@ProductoId", item.ProductoId);
                                    cmdInsertItem.Parameters.AddWithValue("@Cantidad", item.Cantidad);
                                    cmdInsertItem.Parameters.AddWithValue("@Precio", item.Producto.Precio);
                                    cmdInsertItem.Parameters.AddWithValue("@TallaProductoId", item.TallaProductoId);
                                    cmdInsertItem.ExecuteNonQuery();
                                }
                            }

                            // 3) Vaciar carrito del usuario (borrar CarritoItem)
                            var deleteItemsSql = @"DELETE CI
                                                   FROM CarritoItem CI
                                                   INNER JOIN Carrito C ON CI.CarritoId = C.Id
                                                   WHERE C.UsuarioId = @UsuarioId";
                            using (var cmdDelete = new SqlCommand(deleteItemsSql, conn, tran))
                            {
                                cmdDelete.Parameters.AddWithValue("@UsuarioId", usuarioId);
                                cmdDelete.ExecuteNonQuery();
                            }

                            // Commit si todo OK
                            tran.Commit();
                        }
                        catch
                        {
                            tran.Rollback();
                            throw;
                        }
                    }
                }

                TempData["CheckoutSuccess"] = "Compra realizada correctamente.";
                // Redirigir al detalle del pedido nuevo
                return RedirectToAction("Detalle", "Pedidos", new { area = "Publica", id = nuevoPedidoId });
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                TempData["CheckoutError"] = "Error al procesar el pedido: " + ex.Message;
                return RedirectToAction("Checkout");
            }
        }

        // Helper para obtener datos del usuario desde la BBDD
        private Usuario ObtenerUsuario(int usuarioId)
        {
            using (var conn = new SqlConnection(conexion))
            {
                conn.Open();
                string query = @"SELECT Id, Nombre, Apellido, Email, ContraseñaHash, Telefono, Direccion, Ciudad, CodigoPostal, RolId, FechaRegistro, Activo
                                 FROM Usuarios
                                 WHERE Id = @Id";
                using (var cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@Id", usuarioId);
                    var dt = new DataTable();
                    using (var adapter = new SqlDataAdapter(cmd))
                    {
                        adapter.Fill(dt);
                    }

                    if (dt.Rows.Count > 0)
                    {
                        var r = dt.Rows[0];
                        return new Usuario
                        {
                            Id = r.Field<int>("Id"),
                            Nombre = r["Nombre"] == DBNull.Value ? "" : r.Field<string>("Nombre")!,
                            Apellido = r["Apellido"] == DBNull.Value ? "" : r.Field<string>("Apellido")!,
                            Email = r["Email"] == DBNull.Value ? "" : r.Field<string>("Email")!,
                            ContraseñaHash = r["ContraseñaHash"] == DBNull.Value ? "" : r.Field<string>("ContraseñaHash")!,
                            Telefono = r["Telefono"] == DBNull.Value ? "" : r.Field<string>("Telefono")!,
                            Direccion = r["Direccion"] == DBNull.Value ? "" : r.Field<string>("Direccion")!,
                            Ciudad = r["Ciudad"] == DBNull.Value ? "" : r.Field<string>("Ciudad")!,
                            CodigoPostal = r["CodigoPostal"] == DBNull.Value ? "" : r.Field<string>("CodigoPostal")!,
                            RolId = r["RolId"] == DBNull.Value ? 0 : r.Field<int>("RolId"),
                            FechaRegistro = r["FechaRegistro"] == DBNull.Value ? DateTime.MinValue : r.Field<DateTime>("FechaRegistro"),
                            Activo = r["Activo"] == DBNull.Value ? false : r.Field<bool>("Activo")
                        };
                    }
                }
            }

            return null;
        }
    }
}
