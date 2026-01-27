using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Logging;
using System.Data;
using System.Security.Claims;
using System.Text.RegularExpressions;
using TiendaOnline.Areas.Publica.Models;

namespace TiendaOnline.Areas.Publica.Controllers
{
    [Area("Publica")]
    [Route("auth")]
    public class UsuarioController : Controller
    {
        private readonly ILogger<UsuarioController> _logger;

        public UsuarioController(ILogger<UsuarioController> logger)
        {
            _logger = logger;
        }

        #region Login
        [Route("login")]
        [HttpGet]
        public ActionResult LoginView(int id)
        {
            if (User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Index", "Home");
            }

            LoginViewModel comprobar = new();
            ViewBag.Breadcrumb = "Home / Iniciar Sesión";
            return View("Login", comprobar);
        }

        // POST: Validacion de las credenciales del usuario.
        [Route("login")]
        [HttpPost("login")]
        public async Task<ActionResult> LoginPostAsync([FromForm] string email, [FromForm] string contraseña)
        {
            string conexion = "Server=DESKTOP-RODNH5U\\SQLEXPRESS; Database=StreetSize; Trusted_Connection=True; TrustServerCertificate=True;";
            LoginViewModel comprobaciones = new();

            if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(contraseña))
            {
                comprobaciones.parametrosVacios = "Uno de los campos está vacío";
            }

            if (!email.Contains("@") || email.Split('@').Length != 2)
            {
                comprobaciones.emailComprobacion = "El formato de email no es válido.";
            }

            if (contraseña.Length < 8 || !Regex.IsMatch(contraseña, @"[A-Z]") || !Regex.IsMatch(contraseña, @"\d"))
            {
                comprobaciones.passComprobacion = "La contraseña debe tener al menos 8 caracteres, una mayúscula y un número.";
            }

            if (!comprobaciones.IsValid())
            {
                return View("Login", comprobaciones);
            }

            try
            {
                using (SqlConnection conn = new SqlConnection(conexion))
                {
                    conn.Open();

                    string query = @"SELECT Usuarios.Id, Usuarios.Nombre, Roles.Nombre as Rol, Usuarios.ContraseñaHash
                                     FROM Usuarios JOIN Roles ON Usuarios.RolId = Roles.Id
                                     WHERE Email = @Email";

                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@Email", email);

                        DataTable tabla = new DataTable();
                        SqlDataAdapter adaptador = new SqlDataAdapter();
                        adaptador.SelectCommand = cmd;
                        adaptador.Fill(tabla);

                        int idUsuario = 0;
                        string rol = "";
                        string nombreUsuario = "";
                        string hashContraseña = "";

                        foreach (DataRow fila in tabla.Rows)
                        {
                            idUsuario = (int)fila["Id"];
                            nombreUsuario = (string)fila["Nombre"];
                            rol = (string)fila["Rol"];
                            hashContraseña = (string)fila["ContraseñaHash"];
                        }

                        var seguridad = new Seguridad();

                        if (idUsuario != 0 && !string.IsNullOrEmpty(rol) && seguridad.Verificar(contraseña, hashContraseña))
                        {
                            // Crear identidad con claims (incluye rol)
                            var claims = new[]
                            {
                                new Claim(ClaimTypes.Email, email),
                                new Claim(ClaimTypes.Name, nombreUsuario),
                                new Claim(ClaimTypes.Sid, $"{idUsuario}"),
                                new Claim(ClaimTypes.Role, rol)
                            };

                            ClaimsIdentity identidadUsuario = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
                            ClaimsPrincipal usuario = new ClaimsPrincipal(identidadUsuario);

                            await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, usuario, new AuthenticationProperties
                            {
                                IsPersistent = false,
                                ExpiresUtc = DateTime.UtcNow.AddDays(1),
                            });

                            // Redirigir según rol
                            if (string.Equals(rol, "Admin", StringComparison.OrdinalIgnoreCase) ||
                                string.Equals(rol, "Administración", StringComparison.OrdinalIgnoreCase) ||
                                string.Equals(rol, "Administrador", StringComparison.OrdinalIgnoreCase))
                            {
                                // Redirigir a dashboard
                                return RedirectToAction("Index","Dashboard", new { area = "Administracion" });
                            }

                            return RedirectToAction("Index", "Home");
                        }
                        else
                        {
                            ModelState.AddModelError("Login", "Email o contraseña incorrectos.");
                            return View("Login", comprobaciones);
                        }
                    }
                }
            }
            catch (Exception e)
            {
                ModelState.AddModelError("dbError", "Error de conexión con la base de datos.");
                _logger?.LogError(e, "Error en LoginPostAsync");
                return View("Login", comprobaciones);
            }
        }

        [Route("logout")]
        [HttpGet]
        public async Task<IActionResult> LogOut()
        {
            await HttpContext.SignOutAsync();
            return RedirectToAction("Index", "Home");
        }
        #endregion

        #region Registro
        [Route("Registrar")]
        [HttpGet]
        public IActionResult RegistroView()
        {
            LoginViewModel CredencialesUsuario = new();
            ViewBag.Breadcrumb = "Home / Registrar";
            return View("Registrar", CredencialesUsuario);
        }

        [Route("Registrar")]
        [HttpPost]
        public IActionResult RegistrarPost([FromForm] string Nombre, [FromForm] string Apellido, [FromForm] string Email, [FromForm] string Contraseña, [FromForm] string Telefono, [FromForm] string Direccion, [FromForm] string Ciudad, [FromForm] string CodigoPostal)
        {
            string conexion = "Server=DESKTOP-RODNH5U\\SQLEXPRESS; Database=StreetSize; Trusted_Connection=True; TrustServerCertificate=True;";
            LoginViewModel Usuario = new();

            if (string.IsNullOrEmpty(Email) || string.IsNullOrEmpty(Contraseña) || string.IsNullOrEmpty(Nombre) || string.IsNullOrEmpty(Apellido))
            {
                Usuario.parametrosVacios = "Alguno de los campos obligatorios se encuentra vacio";
            }
            if (!Email.Contains("@") || Email.Split('@').Length != 2)
            {
                Usuario.emailComprobacion = "El formato de email no es válido.";
            }
            if (Contraseña.Length < 8 || !Regex.IsMatch(Contraseña, @"[A-Z]") || !Regex.IsMatch(Contraseña, @"\d"))
            {
                Usuario.passComprobacion = "La contraseña debe tener al menos 8 caracteres, una mayúscula y un número.";
            }

            if (!Usuario.IsValid())
            {
                return View("Registrar", Usuario);
            }

            //VALIDAR EMAIL EXISTENTE ANTES DE REGISTRAR 
            try
            {
                using (SqlConnection conn = new SqlConnection(conexion))
                {
                    conn.Open(); // Conectar a la bbdd

                    string query = "SELECT Activo FROM Usuarios WHERE Email = @Email;";
                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@Email", Email);

                        // Ejecutar la consulta y obtener el resultado
                        object resultado = cmd.ExecuteScalar();

                        // Comprobar si el usuario ya existe
                        if (resultado != null)
                        {
                            bool activo = Convert.ToBoolean(resultado); // Suponiendo que 'Activo' es un campo booleano en la base de datos(Bit 1 activo 0 inactivo)

                            //Si el usuario existe pero está inactivo, permitir reactivación
                            if (!activo)
                            {
                                //Se realiza el update para reactivar la cuenta
                                string reactivarQuery = "UPDATE Usuarios SET Activo = 1 WHERE Email = @Email;";
                                using (SqlCommand reactivarCmd = new SqlCommand(reactivarQuery, conn))
                                {
                                    reactivarCmd.Parameters.AddWithValue("@Email", Email);
                                    reactivarCmd.ExecuteNonQuery();
                                }

                                Usuario.usuarioExistente = "Tu cuenta ha sido reactivada. ¡Bienvenido de nuevo!"; //TODO Añadir redirección al login y no mostrar el mensaje en el registro
                                return View("Registrar", Usuario);
                            }
                            //Si el usuario ya está activo, mostrar mensaje de error
                            else
                            {
                                // El usuario ya está registrado y activo
                                Usuario.usuarioExistente = "Ya hay una cuenta asociada a ese correo";
                                return View("Registrar", Usuario);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return View("Registrar");
            }
            //FIN VALIDACION EMAIL

            //REGISTRO en base de datos solo si los datos son correctos y no es una cuenta reactivada
            try
            {
                using (SqlConnection conn = new SqlConnection(conexion))
                {
                    conn.Open(); // Conectar a la bbdd

                    string query = "INSERT INTO Usuarios (Nombre,Apellido,Email,ContraseñaHash,RolId,Activo,FechaRegistro) VALUES(@Nombre,@Apellido,@Email,@ContraseñaHash,@RolId,@Activo,@FechaRegistro); SELECT SCOPE_IDENTITY()";
                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        //Instancia de hash de contraseña
                        var seguridad = new Seguridad();

                        cmd.Parameters.AddWithValue("@Nombre", Nombre);
                        cmd.Parameters.AddWithValue("@Apellido", Apellido);
                        cmd.Parameters.AddWithValue("@Email", Email);
                        cmd.Parameters.AddWithValue("@ContraseñaHash", seguridad.Encriptar(Contraseña)); //Encriptar contraseña

                        //Valores por defecto
                        cmd.Parameters.AddWithValue("@Activo", 1); //Activar usuario
                        cmd.Parameters.AddWithValue("@RolId", 1); //Usuario por defecto 'Cliente'
                        cmd.Parameters.AddWithValue("@FechaRegistro", DateTime.Now); //Fecha registro almacenada

                        if (cmd.ExecuteScalar() == null)//ejecutar TODO FALLO AQUI
                        {
                            return View("Registrar", Usuario); //Si los datos son incorrectos

                        }
                        else
                        {
                            return RedirectToAction("LoginView", Usuario); //Si el registro es exitoso, redirigir al usuario al Login
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return View("Registrar");

            }


        }
        #endregion
    }
}

