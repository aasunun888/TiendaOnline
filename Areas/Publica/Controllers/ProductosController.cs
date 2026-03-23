using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using System.Data;
using TiendaOnline.Areas.Publica.Models.ProductosModels;
using TiendaOnline.Entidades;

namespace TiendaOnline.Areas.Publica.Controllers
{

    [Area("Publica")]
    public class ProductosController : Controller
    {
        private readonly AppDbContext _context;

        public ProductosController(AppDbContext context)
        {
            _context = context;
        }
        
        // GET: Productos para mostrar la vista de productos
        [Route("Buscar")]
        [HttpGet]
        public async Task<ActionResult> Buscar()
        {
            //Crear lista de productos para mostrar en la vista de busqueda
            ProductoViewModel Producto = new();

            string conexion = "Server=DESKTOP-RODNH5U\\SQLEXPRESS; Database = StreetSize; Trusted_Connection = True; TrustServerCertificate=True;";
            string query = "SELECT * FROM Productos WHERE Activo = 1";

            try
            {
                using (SqlConnection conn = new SqlConnection(conexion))
                {
                    conn.Open();//Conectar a la base de datos
                    SqlCommand sqlCommand = conn.CreateCommand(); //Crea un comando en base a la conexión
                    sqlCommand.CommandText = query; //Establece el comando a ejecutar

                    //SqlDataReader reader = sqlCommand.ExecuteReader();
                    DataTable tabla = new DataTable();

                    SqlDataAdapter adapter = new SqlDataAdapter();
                    adapter.SelectCommand = sqlCommand;
                    adapter.Fill(tabla);

                    foreach (DataRow fila in tabla.Rows)
                    {
                        Producto producto = new Producto();

                        producto.Id = fila.Field<int>("Id"); //Recoger la id para identificar el producto
                        producto.Nombre = fila["Nombre"] == DBNull.Value ? "" : fila.Field<string>("Nombre")!;
                        producto.Descripcion = fila["Descripcion"] == DBNull.Value ? "" : fila.Field<string>("Descripcion")!;
                        producto.Precio = fila.Field<decimal>("Precio");
                        producto.Color = fila["Color"] == DBNull.Value ? "" : fila.Field<string>("Color")!;
                        producto.ImagenUrl = fila["ImagenUrl"] == DBNull.Value ? "" : fila.Field<string>("ImagenUrl")!;
                        producto.CategoriaId = fila.Field<int>("CategoriaId"); //Recoger la categoria para implementar filtros e informacion de categoria
                        producto.FechaCreacion = fila["FechaCreacion"].Equals(DBNull.Value) ? DateTime.Now : fila.Field<DateTime>("FechaCreacion"); //Comprobar funcionamiento y reajustar TODO


                        //Almacenar producto en la lista de productos para mostrar en views
                        Producto.ListadoProductos.Add(producto);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }

            // Cargar categorías desde EF: solo categorías con al menos un producto activo asociado
            try
            {
                var categorias = await _context.Categorias
                    .AsNoTracking()
                    .Where(c => _context.Productos.Any(p => p.CategoriaId == c.Id && p.Activo))//Subconsulta para evitar cargar categorias sin productos enlazados
                    .OrderBy(c => c.Nombre)
                    .ToListAsync();

                Producto.Categorias.AddRange(categorias);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error cargando categorías: " + ex.Message);
            }

            ViewBag.Breadcrumb = "Home / Buscar"; //ruta miga de pan

            return View(Producto);


        }
        // POST: Filtrar productos segun formulario en la vista
        /* Función de filtrado cotejando datos en la base de datos, devuelve lista de productos según lo solicitado en el formulario
         * Control de stock desde la base de datos, Control de precio
         */
        [HttpPost("Buscar")]
        public async Task<IActionResult> FiltrarPost([FromForm] int? categoriaId, [FromForm] string talla, [FromForm] decimal? precioMax )
        {
            ProductoViewModel Producto = new();

            string conexion = "Server=DESKTOP-RODNH5U\\SQLEXPRESS;Database=StreetSize;Trusted_Connection=True;TrustServerCertificate=True;";

            string query = @"
                            SELECT DISTINCT p.*
                            FROM Productos p
                            INNER JOIN TallasProducto as tp ON p.Id = tp.ProductoId
                            WHERE (@categoriaId IS NULL OR p.CategoriaId = @categoriaId)
                              AND (@talla = '' OR tp.Talla = @talla)
                              AND (@precioMax IS NULL OR p.Precio <= @precioMax)
                              AND tp.Stock > 0";

            try
            {
                using (SqlConnection conn = new SqlConnection(conexion))
                {
                    conn.Open();
                    SqlCommand cmd = new SqlCommand(query, conn);

                    // Parámetros seguros
                    //Valores que pueden ser null si el usuario no establece una opción
                    cmd.Parameters.AddWithValue("@categoriaId", categoriaId.HasValue ? categoriaId.Value : (object)DBNull.Value); 
                    cmd.Parameters.AddWithValue("@talla", string.IsNullOrEmpty(talla) ? "" : talla);
                    cmd.Parameters.AddWithValue("@precioMax", precioMax.HasValue ? precioMax.Value : (object)DBNull.Value);

                    //Crear tabla para recoger de la DB
                    DataTable tabla = new DataTable();
                    SqlDataAdapter adapter = new SqlDataAdapter(cmd);
                    adapter.Fill(tabla);

                    //Rellenar tabla con la DB
                    foreach (DataRow fila in tabla.Rows)
                    {
                        Producto producto = new Producto
                        {
                            Id = fila.Field<int>("Id"),
                            Nombre = fila["Nombre"] == DBNull.Value ? "" : fila.Field<string>("Nombre")!,
                            Descripcion = fila["Descripcion"] == DBNull.Value ? "" : fila.Field<string>("Descripcion")!,
                            Precio = fila.Field<decimal>("Precio"),
                            Color = fila["Color"] == DBNull.Value ? "" : fila.Field<string>("Color")!,
                            ImagenUrl = fila["ImagenUrl"] == DBNull.Value ? "" : fila.Field<string>("ImagenUrl")!,
                            CategoriaId = fila.Field<int>("CategoriaId"),
                            FechaCreacion = fila["FechaCreacion"].Equals(DBNull.Value) ? DateTime.Now : fila.Field<DateTime>("FechaCreacion")
                        };

                        Producto.ListadoProductos.Add(producto);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }

            // Cargar categorías desde EF para el select (mismas categorías que en GET)
            try
            {
                var categorias = await _context.Categorias
                    .AsNoTracking()
                    .Where(c => _context.Productos.Any(p => p.CategoriaId == c.Id && p.Activo)) //Subconsulta para evitar cargar categorias sin productos enlazados
                    .OrderBy(c => c.Nombre)
                    .ToListAsync();

                Producto.Categorias.AddRange(categorias);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error cargando categorías: " + ex.Message);
            }

            return View("Buscar", Producto); // Reutiliza la misma vista

        }

        /*Función para mostrar el detalle de un producto en base a su Id
          retorna la vista con el producto seleccionado
          Parametro id: Id del producto a mostrar
         */
        [Route("productos/detalle/{id}")]
        public ActionResult Detalle(int id)
        {
            ProductoViewModel productoSeleccionado = null;
            string conexion = "Server=DESKTOP-RODNH5U\\SQLEXPRESS;Database=StreetSize;Trusted_Connection=True;TrustServerCertificate=True;";

            string queryProducto = @"SELECT p.Id, p.Nombre, p.Descripcion, p.Precio, p.Color, p.ImagenUrl, 
                                    p.CategoriaId, p.FechaCreacion,
                                    c.Nombre AS CategoriaNombre
                             FROM Productos p
                             INNER JOIN Categorias c ON p.CategoriaId = c.Id
                             WHERE p.Id = @Id";

            string queryTallas = @"SELECT Id, ProductoId, Talla, Stock
                           FROM TallasProducto
                           WHERE ProductoId = @Id";

            try
            {
                using (var conn = new SqlConnection(conexion))
                {
                    conn.Open();

                    // Leer datos principales del producto (incluye nombre de categoría)
                    using (var cmd = new SqlCommand(queryProducto, conn))
                    {
                        cmd.Parameters.AddWithValue("@Id", id);
                        using (var reader = cmd.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                //Pasar a variables para controlar posibles nulls y evitar excepciones, aunque en este caso no deberían ser nulls
                                int ordNombre = reader.GetOrdinal("Nombre");
                                int ordDescripcion = reader.GetOrdinal("Descripcion");
                                int ordPrecio = reader.GetOrdinal("Precio");
                                int ordColor = reader.GetOrdinal("Color");
                                int ordImagenUrl = reader.GetOrdinal("ImagenUrl");
                                int ordCategoriaId = reader.GetOrdinal("CategoriaId");
                                int ordFechaCreacion = reader.GetOrdinal("FechaCreacion");
                                int ordCategoriaNombre = reader.GetOrdinal("CategoriaNombre");

                                // Mapear datos a ViewModel
                                productoSeleccionado = new ProductoViewModel
                                {
                                    Id = reader.GetInt32(reader.GetOrdinal("Id")),
                                    Nombre = reader.IsDBNull(ordNombre) ? "" : reader.GetString(ordNombre), 
                                    Descripcion = reader.IsDBNull(ordDescripcion) ? "" : reader.GetString(ordDescripcion),
                                    Precio = reader.IsDBNull(ordPrecio) ? 0m : reader.GetDecimal(ordPrecio),
                                    Color = reader.IsDBNull(ordColor) ? "" : reader.GetString(ordColor),
                                    ImagenUrl = reader.IsDBNull(ordImagenUrl) ? "" : reader.GetString(ordImagenUrl),
                                    CategoriaId = reader.IsDBNull(ordCategoriaId) ? 0 : reader.GetInt32(ordCategoriaId),
                                    FechaCreacion = reader.IsDBNull(ordFechaCreacion) ? DateTime.Now : reader.GetDateTime(ordFechaCreacion),
                                    CategoriaNombre = reader.IsDBNull(ordCategoriaNombre) ? "" : reader.GetString(ordCategoriaNombre)
                                };

                                // Asegurar lista de tallas inicializada
                                if (productoSeleccionado.TallasProducto == null)
                                {
                                    productoSeleccionado.TallasProducto = new List<TallasProducto>();
                                }
                            }
                            else
                            {
                                return NotFound();
                            }
                        }
                    }

                    // Obtener tallas y añadirlas a la lista del producto
                    using (var cmdT = new SqlCommand(queryTallas, conn))
                    {
                        cmdT.Parameters.AddWithValue("@Id", id);
                        using (var reader = cmdT.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                var talla = new TallasProducto
                                {
                                    Id = reader.GetInt32(reader.GetOrdinal("Id")),
                                    ProductoId = reader.GetInt32(reader.GetOrdinal("ProductoId")),
                                    Talla = reader.IsDBNull(reader.GetOrdinal("Talla")) ? "" : reader.GetString(reader.GetOrdinal("Talla")),
                                    Stock = reader.IsDBNull(reader.GetOrdinal("Stock")) ? 0 : reader.GetInt32(reader.GetOrdinal("Stock"))
                                };
                                productoSeleccionado.TallasProducto.Add(talla);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }

            if (productoSeleccionado == null)
            {
                return NotFound();
            }

            ViewBag.Breadcrumb = $"Home / Producto / {productoSeleccionado.Nombre}";
            return View("DetalleProducto", productoSeleccionado);
        }

        /* Función para mostrar productos relacionados,
          en base a la categoría del producto seleccionado, mostrar otros productos de la misma categoría.
         */
        [HttpGet]
        public async Task<ActionResult> ProductosRelacionados(int Id)
        {
           
            try
            {
                //Comprobar id existe y es válido
                if (Id <= 0)
                {
                    return PartialView("_CarruselProductos", new List<Producto>());
                }

                // Obtener el producto para sacar la categoría
                var productoBase = await _context.Productos
                    .AsNoTracking()
                    .Where(p => p.Id == Id)
                    .Select(p => new { p.Id, p.CategoriaId })
                    .FirstOrDefaultAsync();

                if (productoBase == null)
                {
                    return PartialView("_CarruselProductos", new List<Producto>());
                }

                List<Producto> relacionados = new List<Producto>();

                if (productoBase.CategoriaId.HasValue)
                {
                    relacionados = await _context.Productos
                        .AsNoTracking()
                        .Where(p => p.Activo && p.CategoriaId == productoBase.CategoriaId && p.Id != Id) //Controlar que traiga productos activos con misma categoria pero que no se repita el mismo
                        .OrderByDescending(p => p.FechaCreacion)
                        .Take(5)
                        .ToListAsync();
                }

                // Fallback: si no hay relacionados, mostrar productos recientes (excluyendo el actual)
                if (relacionados == null || relacionados.Count == 0)
                {
                    relacionados = await _context.Productos
                        .AsNoTracking()
                        .Where(p => p.Activo && p.Id != Id)
                        .OrderByDescending(p => p.FechaCreacion)
                        .Take(5)
                        .ToListAsync();
                }

                return PartialView("_CarruselProductos", relacionados);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error productosRelacionados: {ex.Message}");
                return PartialView("_CarruselProductos", new List<Producto>());
            }
        }

        /*Metodo autogenerado para errores 404*/
        private ActionResult HttpNotFound()
        {
            throw new NotImplementedException();
        }
    }
}
