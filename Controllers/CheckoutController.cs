
using System.Web.Mvc;

namespace Prototipado.Controllers
{
    [Authorize]
    public class CheckoutController : Controller
    {
        [HttpGet]
        public ActionResult Pagar() { return View(); }

        [HttpPost, ValidateAntiForgeryToken]
        public ActionResult Pagar(string nombre, string tarjeta, string vencimiento, string cvv, decimal total)
        {
            TempData["Total"] = total;
            TempData["Nombre"] = nombre;
            return RedirectToAction("Exito");
        }

        public ActionResult Exito() { return View(); }
    }
}
