
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using System.Web.Security;
using Prototipado.Models;

namespace Prototipado.Controllers
{
    public class AccountController : Controller
    {
        private static readonly List<UsuarioVM> _usuarios = new List<UsuarioVM> {
            new UsuarioVM { Nombre = "Admin", Email = "admin@fitstyle.com", Password = "123456" }
        };

        [AllowAnonymous]
        [HttpGet]
        public ActionResult Login(string returnUrl) { ViewBag.ReturnUrl = returnUrl; return View(); }

        [AllowAnonymous, HttpPost, ValidateAntiForgeryToken]
        public ActionResult Login(string email, string password, string returnUrl)
        {
            var user = _usuarios.FirstOrDefault(u => email.Equals(u.Email, StringComparison.OrdinalIgnoreCase) && u.Password == password);
            if (user != null)
            {
                FormsAuthentication.SetAuthCookie(user.Email, false);
                if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl)) return Redirect(returnUrl);
                return RedirectToAction("Index", "Productos");
            }
            ModelState.AddModelError("", "Credenciales inválidas. Intenta de nuevo o regístrate.");
            return View();
        }

        [AllowAnonymous]
        [HttpGet]
        public ActionResult Register(string returnUrl)
        {
            ViewBag.ReturnUrl = returnUrl;
            return View(new RegisterVM());
        }

        [AllowAnonymous]
        [HttpPost, ValidateAntiForgeryToken]
        public ActionResult Register(RegisterVM model, string returnUrl)
        {
            if (!ModelState.IsValid) return View(model);

            if (_usuarios.Any(u => u.Email.Equals(model.Email, StringComparison.OrdinalIgnoreCase)))
            {
                ModelState.AddModelError("Email", "Este correo ya está registrado.");
                return View(model);
            }

            var nuevo = new UsuarioVM { Nombre = model.Nombre, Email = model.Email, Password = model.Password };
            _usuarios.Add(nuevo);
            FormsAuthentication.SetAuthCookie(nuevo.Email, false);

            if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
                return Redirect(returnUrl);

            return RedirectToAction("Index", "Productos");
        }

        [Authorize]
        public ActionResult Logout()
        {
            FormsAuthentication.SignOut();
            return RedirectToAction("Index", "Home");
        }
    }
}
