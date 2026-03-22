using TiendaOnline.Entidades;

namespace TiendaOnline.Areas.Administracion.Models.UsuarioModels
{
    public class UsuarioViewModel
    {
        public int Id { get; set; }
        public string Nombre { get; set; } = "";
        public string Apellido { get; set; } = "";
        public string Email { get; set; } = "";
        public string Telefono { get; set; } = "";
        public string Direccion { get; set; } = "";
        public string Ciudad { get; set; } = "";
        public string CodigoPostal { get; set; } = "";
        public int RolId { get; set; }
        public DateTime FechaRegistro { get; set; }
        public bool Activo { get; set; }

        //Propiedad para mostar pedidos del usuario
        public List<PedidoSummary> Pedidos { get; set; } = new();
    }
}
