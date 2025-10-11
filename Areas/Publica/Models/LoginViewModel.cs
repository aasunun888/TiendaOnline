using Microsoft.AspNetCore.Authorization.Infrastructure;

namespace TiendaOnline.Areas.Publica.Models
{
    public class LoginViewModel
    {
        //Comprobar si los parámetros están vacíos o mal escritos
        public string parametrosVacios { get; set; } = "";
        public string emailComprobacion { get; set; } = "";
        public string passComprobacion { get; set; } = "";

        public string usuarioExistente { get; set; } = "";

        //funcion que devuelve true si los parámetros son correctos
        public bool IsValid()
        {
            if (parametrosVacios != "" || emailComprobacion != "" || passComprobacion != "")
            {
                return false;

            }
            else
            {
                return true;
            }
        }
    }



}
