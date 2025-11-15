using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using System.Data;
using TiendaOnline.Areas.Publica.Models;
using TiendaOnline.Entidades;

namespace TiendaOnline.Areas.Publica.Controllers
{

    [Area("Publica")]
    [Route("")]

    public class ProductosController : Controller
    {
        // GET: Productos para mostrar la vista de productos
        [Route("Buscar")]
        [HttpGet]
        /*Funcion para empezar a buscar, muestra la vista de busqueda con todos los productos por defecto, luego da la opcion de filtrar la busqueda.
         * Extraer los productos de la base de datos y mostrarlos en la vista de busqueda.
         * TODO Comprobar la funcionalidad de mostrar views con listas de productos desde la base de datos.
         */
        public ActionResult Buscar()
        {
            //Crear lista de productos para mostrar en la vista de busqueda
            ProductoViewModel Producto = new();

            string conexion = "Server = DESKTOP-RODNH5U\\SQLEXPRESS; Database = StreetSize; Trusted_Connection = True; TrustServerCertificate=True;";
            string query = "SELECT * FROM Productos";

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


                        //Almacenar producto en la lista de productos para mostrar en viewsS
                        Producto.ListadoProductos.Add(producto);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }

            ViewBag.Breadcrumb = "Home / Buscar"; //ruta miga de pan

            return View(Producto);


        }
        // POST: Filtrar productos segun formulario en la vista
        /* Función de filtrado cotejando datos en la base de datos, devuelve lista de productos según lo solicitado en el formulario
         * Control de stock desde la base de datos, Control de precio
         */
        [HttpPost("Buscar")]
        public IActionResult FiltrarPost([FromForm] int? categoriaId, [FromForm] string talla, [FromForm] decimal? precioMax )
        {
            ProductoViewModel Producto = new();

            string conexion = "Server=DESKTOP-RODNH5U\\SQLEXPRESS;Database=StreetSize;Trusted_Connection=True;TrustServerCertificate=True;";

            // Consulta con JOIN para acceder a tallas y aplicar filtros
            /*En la consulta se permiten valores nulos debido a que en el formulario se establece la opcion de "todas" la cual tienes valor NULL, si el usuario no elige nada o deja alguna en null,
             * en la base de datos lo dará por válido buscando así solamente la información que el usuario haya elegido tener, si elige talla, saldrá todo lo demás filtrando nada mas la talla, es decir,
             * todos los productos con talla X
             */
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
            return View("Buscar", Producto); // Reutiliza la misma vista

        }

        /*Función para mostrar el detalle de un producto en base a su Id
          retorna la vista con el producto seleccionado
          Parametro id: Id del producto a mostrar
         */
        [Route("productos/detalle/{id}")]
        public ActionResult Detalle(int id)
        {
            Producto productoSeleccionado = null;
            string conexion = "Server=DESKTOP-RODNH5U\\SQLEXPRESS;Database=StreetSize;Trusted_Connection=True;TrustServerCertificate=True;";

            string query = @"SELECT p.Id, p.Nombre, p.Descripcion, p.Precio, p.Color, p.ImagenUrl, 
                        p.CategoriaId, p.FechaCreacion,
                        c.Nombre AS CategoriaNombre
                 FROM Productos p
                 INNER JOIN Categorias c ON p.CategoriaId = c.Id
                 WHERE p.Id = @Id";

            try
            {
                        using (SqlConnection conn = new SqlConnection(conexion))
                        {
                            conn.Open();

                            using (SqlCommand cmd = new SqlCommand(query, conn))
                            {
                                cmd.Parameters.AddWithValue("@Id", id);

                                using (SqlDataReader reader = cmd.ExecuteReader())
                                {
                                    if (reader.Read())
                                    {
                                        productoSeleccionado = new Producto
                                        {
                                            Id = reader.GetInt32(reader.GetOrdinal("Id")),
                                            Nombre = reader["Nombre"] == DBNull.Value ? "" : reader.GetString(reader.GetOrdinal("Nombre")),
                                            Descripcion = reader["Descripcion"] == DBNull.Value ? "" : reader.GetString(reader.GetOrdinal("Descripcion")),
                                            Precio = reader.GetDecimal(reader.GetOrdinal("Precio")),
                                            Color = reader["Color"] == DBNull.Value ? "" : reader.GetString(reader.GetOrdinal("Color")),
                                            ImagenUrl = reader["ImagenUrl"] == DBNull.Value ? "" : reader.GetString(reader.GetOrdinal("ImagenUrl")),
                                            CategoriaId = reader.GetInt32(reader.GetOrdinal("CategoriaId")),
                                            FechaCreacion = reader["FechaCreacion"] == DBNull.Value ? DateTime.Now : reader.GetDateTime(reader.GetOrdinal("FechaCreacion"))
                                        };

                                        productoSeleccionado.CategoriaNombre = reader["CategoriaNombre"].ToString();

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
                        return HttpNotFound(); // Si no existe el producto, devolvemos 404
                    }
            ViewBag.Breadcrumb = $"Home / Producto / {productoSeleccionado.Nombre}"; //ruta miga de pan
            return View("DetalleProducto", productoSeleccionado);
                }

        /*Metodo autogenerado para errores 404*/
        private ActionResult HttpNotFound()
        {
            throw new NotImplementedException();
        }
    }
}
