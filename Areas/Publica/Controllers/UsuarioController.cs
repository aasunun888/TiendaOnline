using TiendaOnline.Areas.Publica.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR.Protocol;
using System.Data;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using System.Security.Claims;
using Microsoft.Extensions.Logging;
using Microsoft.Data.SqlClient;
using System.Text.RegularExpressions;



namespace TiendaOnline.Areas.Publica.Controllers
{
  
    public class UsuarioController : Controller
    {
        // GET: UsuarioController

        //Index: Perfil del usuario con datos personales y opciones de configuración.
        public ActionResult Index()
        {

            return View();
        }

        #region Login
        [Route("login")]
        [HttpGet]
        // GET: Lo que el usuario ve antes de iniciar sesión.
        public ActionResult LoginView(int id)
        {
            //Comprobar si el usuario ya ha iniciado sesión
            if(User.Identity.IsAuthenticated)
            {
                //Redirigir al usuario a la página principal
                return RedirectToAction("Index", "Home");
            }

            LoginViewModel comprobar = new();

            return View("Login",comprobar);
        }

        [Route("login")]
        [HttpPost]
        // POST: Validacion de las credenciales del usuario.
        /*Funcion asincrona para validación de las credenciales y autenticación del usuario. Si las credenciales son correctas, 
         * se crea una identidad de usuario y se inicia una sesión.
         */
        public async Task <ActionResult> LoginPostAsync([FromForm] string email, [FromForm] string contraseña)
        {
            //Conexión con BBDD
            string conexion = "Server=DESKTOP-RODNH5U\\SQLEXPRESS; Database=TiendaOnline; Trusted_Connection=True; TrustServerCertificate=True;";
            LoginViewModel comprobaciones = new();

            //Validación de campos vacios
            if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(contraseña))
            {
                comprobaciones.parametrosVacios = "Uno de los campos está vacío";

            }

            //Validación de formato de email 
            if (!email.Contains("@") || email.Split('@').Length != 2)
            {

                comprobaciones.emailComprobacion = "El formato de email no es válido.";


            }

            //Validación de seguridad de contraseña (mínimo 8 caracteres, 1 mayúscula y 1 número)
            if (contraseña.Length < 8 || !Regex.IsMatch(contraseña, @"[A-Z]") || !Regex.IsMatch(contraseña, @"\d"))
            {

                comprobaciones.passComprobacion = "La contraseña debe tener al menos 8 caracteres, una mayúscula y un número.";

            }

            //Si hay errores de validación, retornar la vista sin logearse
            if (!comprobaciones.IsValid())
            {
                return View("Login", comprobaciones);
            }

            //Validación de credenciales en la BBDD
            try
            {
                using (SqlConnection conn = new SqlConnection(conexion))
                {
                    conn.Open(); //Abrir conexión

                    string query = "SELECT  Usuarios.Id, Usuarios,Nombre, Roles.Nombre" +
                        "FROM Usuarios " +
                        "JOIN Roles ON Usuario.RolId = Roles.Id" +
                        "WHERE Email = @email AND Contraseña = @contraseña";

                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@Email", email);
                        cmd.Parameters.AddWithValue("@Password", contraseña); //se usará hash para mayor seguridad

                        DataTable tabla = new DataTable();

                        SqlDataAdapter adaptador = new SqlDataAdapter();

                        adaptador.SelectCommand = cmd;

                        adaptador.Fill(tabla);

                        //creacion de variables vacias para rellenar tabla
                        int idUsuario = 0;
                        string rol = "";
                        string nombreUsuario = "";

                        foreach (DataRow fila in tabla.Rows)
                        {
                            idUsuario = (int)fila["Id"];
                            nombreUsuario = (string)fila["Nombre"];
                            rol = (string)fila["Nombre"];
                        }

                        if(idUsuario != 0 && rol != "")
                        {
                            //Crear identidad de usuario y claims basandonos en email,nombre, id y rol
                            ClaimsIdentity identidadUsuario = new ClaimsIdentity([
                                new Claim(ClaimTypes.Email, email),
                                new Claim(ClaimTypes.Name, nombreUsuario),
                                new Claim(ClaimTypes.Sid,$"{idUsuario}"),
                                new Claim(ClaimTypes.Role, rol)
                                ],CookieAuthenticationDefaults.AuthenticationScheme);

                            ClaimsPrincipal usuario = new ClaimsPrincipal(identidadUsuario);

                            await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, usuario, new AuthenticationProperties
                            {
                                IsPersistent = false,
                                ExpiresUtc = DateTime.UtcNow.AddDays(1),

                            });
                            return RedirectToAction("Index", "Home"); //Si el login es exitoso, redirigir al usuario al Index

                        }
                        else
                        {
                            ModelState.AddModelError("Login", "Email o contraseña incorrectos.");
                            return View("Login", comprobaciones); //Si los datos son incorrectos, mostrar mensaje de error en la vista
                        }
                    }
                }
            }catch(Exception e)
            {
                ModelState.AddModelError("dbError", "Error de conexión con la base de datos.");
                return View("Login", comprobaciones);

            }
        }


        [Route("logout")]
        [HttpGet]
        //Funcion asincrona para cerrar la sesión del usuario.
        public async Task<IActionResult> LogOut()
        {

            await HttpContext.SignOutAsync();
            return RedirectToAction("Index", "Home");


        }
        #endregion




    }
}
