using Microsoft.AspNetCore.Authorization.Infrastructure;
using Microsoft.AspNetCore.Identity;

namespace TiendaOnline.Areas.Publica.Models
{
    public class LoginViewModel
    {
        //Comprobar si los parámetros están vacíos o mal escritos
        public string parametrosVacios { get; set; } = "";
        public string emailComprobacion { get; set; } = "";
        public string passComprobacion { get; set; } = "";

        public string usuarioExistente { get; set; } = "";

        public string telefonoComprobacion { get; set; } = "";


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


    public class Seguridad
    {
        //Instancia de PasswordHasher para manejar el hashing de contraseñas
        private readonly PasswordHasher<string> _hasher = new();

        //Método para encriptar una contraseña
        public string Encriptar(string contraseña)
        {
            return _hasher.HashPassword(null, contraseña);
        }

        //Método para verificar una contraseña contra un hash almacenado
        public bool Verificar(string contraseñaUsuario, string hashGuardado)
        {
            var resultado = _hasher.VerifyHashedPassword(null, hashGuardado, contraseñaUsuario);
            return resultado == PasswordVerificationResult.Success;
        }
    }


}
