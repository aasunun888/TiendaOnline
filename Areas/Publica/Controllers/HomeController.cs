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


    public class HomeController : Controller
    {
        // GET: HomeController
        [Route("")]
        public ActionResult Index()
        {
            var modelo = new HomeViewModel
            {
                Titulo = "StreetSize",
                Descripcion = "¡Vístete con confianza y destaca en cada ocasión!.",
                ImagenUrl = "/images/51f7191bd47f34a131a3aa1d766b71ef.jpg"
            };

            return View(modelo);
        }

   

        // Funcion novedades para mostrar productos nuevos, en este caso todos son nuevos
        [Route("novedades")]

        public ActionResult NovedadesGet(int id)
        {
            ViewBag.Breadcrumb = "Home / Novedades"; //ruta miga de pan

            return View();
        }

        // Funcion About Us, informacion de la empresa 
        [Route("about")]

        public ActionResult About()
        {
            ViewBag.Breadcrumb = "Home / Sobre Nosotros"; //ruta miga de pan

            return View();
        }

       
        }
    
}
