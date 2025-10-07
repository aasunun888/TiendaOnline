using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using TiendaOnline.Areas.Publica.Models;

namespace TiendaOnline.Areas.Publica.Controllers
{
    public class HomeController : Controller
    {
        // GET: HomeController
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
        public ActionResult NovedadesGet(int id)
        {

            return View();
        }

        // Funcion About Us, informacion de la empresa 
        public ActionResult About()
        {
            return View();
        }

        // POST: HomeController/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(IFormCollection collection)
        {
            try
            {
                return RedirectToAction(nameof(Index));
            }
            catch
            {
                return View();
            }
        }

        // GET: HomeController/Edit/5
        public ActionResult Edit(int id)
        {
            return View();
        }

        // POST: HomeController/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(int id, IFormCollection collection)
        {
            try
            {
                return RedirectToAction(nameof(Index));
            }
            catch
            {
                return View();
            }
        }

        // GET: HomeController/Delete/5
        public ActionResult Delete(int id)
        {
            return View();
        }

        // POST: HomeController/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Delete(int id, IFormCollection collection)
        {
            try
            {
                return RedirectToAction(nameof(Index));
            }
            catch
            {
                return View();
            }
        }
    }
}
