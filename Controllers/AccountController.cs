
using PrototipoFinal.Models;
using System;
using System.Collections.Generic;
using System.Data.Entity.Validation;
using System.Linq;
using System.Web.Mvc;
using System.Web.Security;
using System.Web.Services.Description;


namespace Prototipado.Controllers
{
    public class AccountController : Controller
    {
        private static readonly FitStyleDBEntities db = new FitStyleDBEntities();

        [AllowAnonymous]
        [HttpGet]
        public ActionResult Login(string returnUrl) { ViewBag.ReturnUrl = returnUrl; return View(); }

        [AllowAnonymous, HttpPost, ValidateAntiForgeryToken]
        public ActionResult Login(string email, string password, string returnUrl)
        {
            var user = db.Usuarios.FirstOrDefault(u => email.Equals(u.email, StringComparison.OrdinalIgnoreCase) && u.password_hash == password);
            if (user != null)
            {
                FormsAuthentication.SetAuthCookie(user.email, false);
                if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl)) return Redirect(returnUrl);
                return RedirectToAction("Index", "Productos");
            }
            ModelState.AddModelError("", "Credenciales inválidas. Intenta de nuevo o regístrate.");
            return View();
        }

        [AllowAnonymous]
        [HttpGet]
        public ActionResult Register()
        {
            return View(new Usuarios());
        }

        [AllowAnonymous]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Register(Usuarios model, string Password, string ConfirmPassword)
        {
            if (Password != ConfirmPassword)
            {
                ModelState.AddModelError("", "Las contraseñas no coinciden.");
            }

            if (!ModelState.IsValid)
            {
                return View(model);
            }


            model.password_hash = Password; 
            model.es_admin = false;
            model.fecha_registro = DateTime.UtcNow;

            using (var db = new FitStyleDBEntities())
            {
               
                if (db.Usuarios.Any(u => u.nombre_usuario == model.nombre_usuario))
                {
                    ModelState.AddModelError("nombre_usuario", "El nombre de usuario ya está en uso, elige otro.");
                    return View(model);
                    
                }
                if (db.Usuarios.Any(u => u.email == model.email))
                {
                    ModelState.AddModelError("email", "El correo ya está en uso, elige otro.");
                    return View(model);
                }
                try
                {
                    db.Usuarios.Add(model);
                    db.SaveChanges();
                    
                }
                catch (DbEntityValidationException ex)
                {
                    
                    var detalles = ex.EntityValidationErrors
                        .SelectMany(e => e.ValidationErrors)
                        .Select(e => $"Propiedad: {e.PropertyName} - Error: {e.ErrorMessage}");

                    var mensaje = string.Join(" || ", detalles);

                
                    System.Diagnostics.Debug.WriteLine("ERRORES: " + mensaje);

                  
                    foreach (var e in ex.EntityValidationErrors.SelectMany(v => v.ValidationErrors))
                    {
                        ModelState.AddModelError(e.PropertyName, e.ErrorMessage);
                    }

                    
                    throw new Exception("Error de validación al guardar Usuario: " + mensaje, ex);
                }

                TempData["SuccessMessage"] = "Tu cuenta ha sido creada correctamente.";
            }
            return RedirectToAction("Login", "Account");

        }


        [Authorize]
        public ActionResult Logout()
        {
            FormsAuthentication.SignOut();
            return RedirectToAction("Index", "Home");
        }
    }
}
