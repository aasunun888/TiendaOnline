using Microsoft.AspNetCore.Mvc;
using System;
using Microsoft.Data.SqlClient;
using System.Security.Claims;
using TiendaOnline.Areas.Publica.Models;
using TiendaOnline.Entidades;

namespace TiendaOnline.Areas.Publica.Controllers
{
    [Area("Publica")]
    [Route("")]
    public class PerfilController : Controller
    {
        private readonly string conexion = "Server=DESKTOP-RODNH5U\\SQLEXPRESS;Database=StreetSize;Trusted_Connection=True;TrustServerCertificate=True;";

        // GET: /usuario/perfil
        [Route("usuario/perfil")]
        public IActionResult Index()
        {
            if (!TryGetCurrentUserId(out int usuarioId))
                return RedirectToAction("Login", "Usuario");

            var vm = new PerfilViewModel();

            string query = @"SELECT Id, Nombre, Apellido, Email, Telefono, Direccion, Ciudad, CodigoPostal, RolId, FechaRegistro, Activo
                             FROM Usuarios
                             WHERE Id = @Id";

            try
            {
                using (var conn = new SqlConnection(conexion))
                {
                    conn.Open();
                    using (var cmd = new SqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@Id", usuarioId);
                        using (var reader = cmd.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                vm.Usuario = new Usuario
                                {
                                    Id = reader.GetInt32(reader.GetOrdinal("Id")),
                                    Nombre = reader["Nombre"] == DBNull.Value ? "" : reader.GetString(reader.GetOrdinal("Nombre")),
                                    Apellido = reader["Apellido"] == DBNull.Value ? "" : reader.GetString(reader.GetOrdinal("Apellido")),
                                    Email = reader["Email"] == DBNull.Value ? "" : reader.GetString(reader.GetOrdinal("Email")),
                                    Telefono = reader["Telefono"] == DBNull.Value ? "" : reader.GetString(reader.GetOrdinal("Telefono")),
                                    Direccion = reader["Direccion"] == DBNull.Value ? "" : reader.GetString(reader.GetOrdinal("Direccion")),
                                    Ciudad = reader["Ciudad"] == DBNull.Value ? "" : reader.GetString(reader.GetOrdinal("Ciudad")),
                                    CodigoPostal = reader["CodigoPostal"] == DBNull.Value ? "" : reader.GetString(reader.GetOrdinal("CodigoPostal")),
                                    RolId = reader["RolId"] == DBNull.Value ? 0 : reader.GetInt32(reader.GetOrdinal("RolId")),
                                    FechaRegistro = reader["FechaRegistro"] == DBNull.Value ? DateTime.MinValue : reader.GetDateTime(reader.GetOrdinal("FechaRegistro")),
                                    Activo = reader["Activo"] == DBNull.Value ? false : reader.GetBoolean(reader.GetOrdinal("Activo"))
                                };
                            }
                            else
                            {
                                return NotFound();
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }

            return View("~/Areas/Publica/Views/Usuario/Perfil.cshtml", vm);
        }

        // POST: /usuario/perfil/actualizar
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Route("usuario/perfil/actualizar")]
        public IActionResult Actualizar(PerfilViewModel model)
        {
            if (!TryGetCurrentUserId(out int usuarioId))
                return RedirectToAction("Login", "Usuario");

            if (model == null || model.Usuario == null)
                return BadRequest();

            if (model.Usuario.Id != usuarioId)
                return Forbid();

            string update = @"UPDATE Usuarios
                              SET Nombre = @Nombre,
                                  Apellido = @Apellido,
                                  Telefono = @Telefono,
                                  Direccion = @Direccion,
                                  Ciudad = @Ciudad,
                                  CodigoPostal = @CodigoPostal
                              WHERE Id = @Id";

            try
            {
                using (var conn = new SqlConnection(conexion))
                {
                    conn.Open();
                    using (var cmd = new SqlCommand(update, conn))
                    {
                        cmd.Parameters.AddWithValue("@Nombre", (object)model.Usuario.Nombre ?? DBNull.Value);
                        cmd.Parameters.AddWithValue("@Apellido", (object)model.Usuario.Apellido ?? DBNull.Value);
                        cmd.Parameters.AddWithValue("@Telefono", (object)model.Usuario.Telefono ?? DBNull.Value);
                        cmd.Parameters.AddWithValue("@Direccion", (object)model.Usuario.Direccion ?? DBNull.Value);
                        cmd.Parameters.AddWithValue("@Ciudad", (object)model.Usuario.Ciudad ?? DBNull.Value);
                        cmd.Parameters.AddWithValue("@CodigoPostal", (object)model.Usuario.CodigoPostal ?? DBNull.Value);
                        cmd.Parameters.AddWithValue("@Id", usuarioId);

                        cmd.ExecuteNonQuery();
                    }
                }

                TempData["PerfilMensaje"] = "Perfil actualizado correctamente.";
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                TempData["PerfilError"] = "Error al actualizar el perfil.";
            }

            return RedirectToAction("Index");
        }

        // Helper para obtener id de usuario actual desde claims
        private bool TryGetCurrentUserId(out int usuarioId)
        {
            usuarioId = 0;
            var usuarioIdClaim = User.FindFirstValue(ClaimTypes.Sid) ?? User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(usuarioIdClaim) || !int.TryParse(usuarioIdClaim, out usuarioId))
            {
                return false;
            }
            return true;
        }
    }
}
