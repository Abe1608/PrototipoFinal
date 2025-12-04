
using PrototipoFinal.Models;
using PrototipoFinal.ViewModels;
using System;
using System.Collections.Generic;
using System.Data.Entity.Validation;
using System.IO;
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
        public ActionResult Login(string returnUrl) 
        { 
           ViewBag.ReturnUrl = returnUrl; return View(); 
        }



        //Metodo para el login de usuarios
        [AllowAnonymous]
        [HttpPost]
        [ValidateAntiForgeryToken]
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


        //Método para registrar un nuevo usuario
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

            Session.Clear();        
            Session.RemoveAll();    
            Session.Abandon();
            return RedirectToAction("Index", "Home");
        }



        [Authorize]
        public ActionResult Perfil()
        {
            using (var db = new FitStyleDBEntities())
            {
                var email = User.Identity.Name;
                var usuario = db.Usuarios.SingleOrDefault(u => u.email == email);
                if (usuario == null) return HttpNotFound();

                // Buscar dirección principal (si existe)
                var dir = usuario.Direcciones_Usuario
                                 .FirstOrDefault(d => d.es_principal);

                var model = new PerfilViewModel
                {
                    NombreCompleto = usuario.nombre_completo,

                    Telefono = dir?.telefono_contacto,          

                    DireccionLinea1 = dir?.linea1,
                    DireccionLinea2 = dir?.linea2,
                    Ciudad = dir?.ciudad,
                    Departamento = dir?.departamento,
                    Pais = dir?.pais,
                    CodigoPostal = dir?.codigo_postal,
                    FotoPerfilUrl = usuario.foto_perfil
                };

                return View(model);
            }
        }

        [Authorize]
        public ActionResult MiPerfil()
        {
            using (var db = new FitStyleDBEntities())
            {
                var email = User.Identity.Name;
                var usuario = db.Usuarios.SingleOrDefault(u => u.email == email);
                if (usuario == null) return HttpNotFound();

                // Dirección principal (puede ser null la primera vez)
                var dir = usuario.Direcciones_Usuario
                                 .FirstOrDefault(d => d.es_principal);

                var model = new PerfilViewModel
                {
                    NombreCompleto = usuario.nombre_completo,
                    Telefono = dir?.telefono_contacto,
                    DireccionLinea1 = dir?.linea1,
                    DireccionLinea2 = dir?.linea2,
                    Ciudad = dir?.ciudad,
                    Departamento = dir?.departamento,
                    Pais = dir?.pais,
                    CodigoPostal = dir?.codigo_postal,
                    
                    FotoPerfilUrl = usuario.foto_perfil
                };

                return View(model);
            }
        }




        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public ActionResult Perfil(PerfilViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            using (var db = new FitStyleDBEntities())
            {
                var email = User.Identity.Name;
                var usuario = db.Usuarios.SingleOrDefault(u => u.email == email);
                if (usuario == null) return HttpNotFound();

                
                usuario.nombre_completo = model.NombreCompleto;

               
                var dir = usuario.Direcciones_Usuario
                                 .FirstOrDefault(d => d.es_principal);

                if (dir == null)
                {
                    dir = new Direcciones_Usuario
                    {
                        id_usuario = usuario.id_usuario,
                        es_principal = true
                    };
                    db.Direcciones_Usuario.Add(dir);
                }

                // Seteamos los datos de la dirección
                dir.linea1 = model.DireccionLinea1;
                dir.linea2 = model.DireccionLinea2;
                dir.ciudad = model.Ciudad;
                dir.departamento = model.Departamento;
                dir.pais = model.Pais;
                dir.codigo_postal = model.CodigoPostal;
                dir.telefono_contacto = model.Telefono;

                if (model.FotoArchivo != null && model.FotoArchivo.ContentLength > 0)
                {
                    var ext = Path.GetExtension(model.FotoArchivo.FileName).ToLower();
                    var permitidas = new[] { ".jpg", ".jpeg", ".png", ".gif" };

                    if (!permitidas.Contains(ext))
                    {
                        ModelState.AddModelError("FotoArchivo", "Solo se permiten imágenes JPG, PNG o GIF.");
                        return View(model);
                    }

                    
                    var uploadDir = Server.MapPath("~/images/uploads/perfiles");
                    if (!Directory.Exists(uploadDir))
                        Directory.CreateDirectory(uploadDir);

                    var fileName = $"user_{usuario.id_usuario}_{DateTime.Now:yyyyMMddHHmmss}{ext}";
                    var filePath = Path.Combine(uploadDir, fileName);

                    
                    model.FotoArchivo.SaveAs(filePath);

                    
                    usuario.foto_perfil = "/images/uploads/perfiles/" + fileName;
                }

                db.SaveChanges();
                TempData["PerfilActualizado"] = "Tus datos se han guardado correctamente.";
                return RedirectToAction("MiPerfil", "Account");
            }
        }


    }
}
