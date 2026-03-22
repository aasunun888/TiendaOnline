using Microsoft.EntityFrameworkCore;
using TiendaOnline.Entidades;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options)
    {
    }

    // Entidades
    public DbSet<Usuarios> Usuarios { get; set; }
    public DbSet<Roles> Roles { get; set; }
    public DbSet<Producto> Productos { get; set; }
    public DbSet<TallasProducto> TallasProducto { get; set; }
    public DbSet<Categoria> Categorias { get; set; }
    public DbSet<Pedido> Pedidos { get; set; }
    public DbSet<PedidoItem> PedidoItem { get; set; }
    public DbSet<Carrito> Carrito { get; set; }
    public DbSet<CarritoItem> CarritoItem { get; set; }


}
