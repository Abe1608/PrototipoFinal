
using System.Web.Mvc;

namespace Prototipado.Controllers
{
    [Authorize]
    public class CheckoutController : Controller
    {
        [HttpGet]
        public ActionResult Pagar() { return View(); }
    }
}
