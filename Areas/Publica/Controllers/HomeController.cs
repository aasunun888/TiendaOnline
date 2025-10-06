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
            var modelo = new HomeViewModel { 
                Titulo = "Bienvenido a la tienda",
                Descripcion = "¿Quieres destacar con estilo y comodidad? Soy Alberto y  " +
                "os hablo de calidad, Street Size!, mi ropa combina la última moda con calidad excepcional, " +
                "diseñada para realzar tu personalidad. Desde estilosas camisetas únicas para llevar flow a todas " +
                "horas hasta prendas casuales que reflejan tu estilo único, ofrezco una amplia gama de opciones. " +
                "Descubre la diferencia con Street Size: ¡viste con confianza y destaca en cada ocasión!",
            };
            return View(modelo);
        }

        // GET: HomeController/Details/5
        public ActionResult Details(int id)
        {
            return View();
        }

        // GET: HomeController/Create
        public ActionResult Create()
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
