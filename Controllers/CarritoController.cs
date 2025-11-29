using PrototipoFinal.Models;
using PrototipoFinal.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace PrototipoFinal.Controllers
{
    public class CarritoController : Controller
    {

        // Metodo para crear o buscar el carrito "Abierto" del usuario
        // Campo de contexto global
        private readonly FitStyleDBEntities db = new FitStyleDBEntities();

        // Devuelve null si no hay usuario
        private int? GetUsuarioId()
        {
            // 1. Si ya lo tenemos en Session, lo usamos
            if (Session["UserId"] is int idSesion)
                return idSesion;

            // 2. Si el usuario está autenticado, lo buscamos por email
            if (User.Identity.IsAuthenticated)
            {
                var email = User.Identity.Name;

                var usuario = db.Usuarios.SingleOrDefault(u => u.email == email);
                if (usuario != null)
                {
                    Session["UserId"] = usuario.id_usuario; // cachear en Session
                    return usuario.id_usuario;
                }
            }

            // 3. No hay usuario
            return null;
        }

        // Recibe el idUsuario por parámetro, NO vuelve a leer Session
        private Carritos CrearNuevoCarrito(int idUsuario)
        {
            var carrito = db.Carritos
                .SingleOrDefault(c => c.id_usuario == idUsuario && c.estado == "Abierto");

            if (carrito == null)
            {
                carrito = new Carritos
                {
                    id_usuario = idUsuario,
                    estado = "Abierto",
                    fecha_creacion = DateTime.Now,
                    ultima_actualizacion = DateTime.Now
                };

                db.Carritos.Add(carrito);
                db.SaveChanges();
            }

            return carrito;
        }

        private int GetCartCount(int idUsuario)
        {
            var carrito = db.Carritos
                .SingleOrDefault(c => c.id_usuario == idUsuario && c.estado == "Abierto");

            if (carrito == null) return 0;

            return db.Carrito_Detalle
                .Where(d => d.id_carrito == carrito.id_carrito)
                .Sum(d => (int?)d.cantidad) ?? 0;
        }

        [HttpPost]
        public JsonResult Aumentar(int idInventario)
        {
            var idUsuario = GetUsuarioId();
            if (idUsuario == null)
            {
                // Lo manejarás en JS: mostrar "Debes iniciar sesión"
                return Json(new { success = false, notLogged = true });
            }

            var carrito = CrearNuevoCarrito(idUsuario.Value);

            var detalle = db.Carrito_Detalle
                .SingleOrDefault(d => d.id_carrito == carrito.id_carrito
                                   && d.id_inventario == idInventario);

            if (detalle == null)
            {
                detalle = new Carrito_Detalle
                {
                    id_carrito = carrito.id_carrito,
                    id_inventario = idInventario,
                    cantidad = 1
                };
                db.Carrito_Detalle.Add(detalle);
            }
            else
            {
                detalle.cantidad += 1;
            }

            carrito.ultima_actualizacion = DateTime.Now;
            db.SaveChanges();

            int count = GetCartCount(idUsuario.Value);

            return Json(new { success = true, count });
        }

        [HttpPost]
        public JsonResult Disminuir(int idInventario)
        {
            var idUsuario = GetUsuarioId();
            if (idUsuario == null)
            {
                return Json(new { success = false, notLogged = true });
            }

            var carrito = CrearNuevoCarrito(idUsuario.Value);

            var detalle = db.Carrito_Detalle
                .SingleOrDefault(d => d.id_carrito == carrito.id_carrito
                                   && d.id_inventario == idInventario);

            if (detalle != null)
            {
                detalle.cantidad -= 1;

                if (detalle.cantidad <= 0)
                {
                    db.Carrito_Detalle.Remove(detalle);
                }

                carrito.ultima_actualizacion = DateTime.Now;
                db.SaveChanges();
            }

            int count = GetCartCount(idUsuario.Value);

            return Json(new { success = true, count });
        }

        [HttpPost]
        public JsonResult EliminarItem(int idDetalle)
        {
            var idUsuario = GetUsuarioId();
            if (idUsuario == null)
            {
                return Json(new { success = false, notLogged = true });
            }

            var detalle = db.Carrito_Detalle
                .SingleOrDefault(d => d.id_carrito_detalle == idDetalle);

            if (detalle != null)
            {
                db.Carrito_Detalle.Remove(detalle);

                var carrito = db.Carritos
                    .SingleOrDefault(c => c.id_carrito == detalle.id_carrito);

                if (carrito != null)
                {
                    carrito.ultima_actualizacion = DateTime.Now;
                }

                db.SaveChanges();
            }

            int count = GetCartCount(idUsuario.Value);

            return Json(new { success = true, count });
        }

        //Metodo para agregar productos al carrito
        [HttpPost]
        public ActionResult AgregarPorProducto(int idProducto, int cantidad = 1)
        {
            var idUsuario = GetUsuarioId();
            if (idUsuario == null)
            {
                return Json(new { success = false, message = "Debes iniciar sesión." });
            }

            // Buscar inventario del producto
            var inventario = db.Inventario_Detalle
                               .FirstOrDefault(i => i.id_producto == idProducto);

            if (inventario == null)
            {
                return Json(new { success = false, message = "No hay inventario disponible." });
            }

            int idInventario = inventario.id_inventario;

            // Carrito del usuario (lo crea si no existe)
            var carrito = CrearNuevoCarrito(idUsuario.Value);
            int idCarrito = carrito.id_carrito;

            var detalle = db.Carrito_Detalle
                            .SingleOrDefault(d => d.id_carrito == idCarrito &&
                                                  d.id_inventario == idInventario);

            if (detalle == null)
            {
                detalle = new Carrito_Detalle
                {
                    id_carrito = idCarrito,
                    id_inventario = idInventario,
                    cantidad = cantidad
                };
                db.Carrito_Detalle.Add(detalle);
            }
            else
            {
                detalle.cantidad += cantidad;
            }

            carrito.ultima_actualizacion = DateTime.UtcNow;

            db.SaveChanges();

            // Total de items para el badge
            var totalItems = db.Carrito_Detalle
                            .Where(d => d.id_carrito == idCarrito)
                            .Sum(d => (int?)d.cantidad) ?? 0;

            return Json(new { success = true, count = totalItems });
        }


        // Método para renderizar el carrito en el offcanvas
        public ActionResult Offcanvas()
       {
        // Si no hay usuario autenticado, carrito vacío
        if (!User.Identity.IsAuthenticated)
        {
            return PartialView(
                "~/Views/Shared/_Carrito.cshtml",
                Enumerable.Empty<CarritoViewModel>()
            );
        }

        using (var db = new FitStyleDBEntities())
        {
            var email = User.Identity.Name;
            var usuario = db.Usuarios.SingleOrDefault(u => u.email == email);

            if (usuario == null)
            {
                return PartialView(
                    "~/Views/Shared/_Carrito.cshtml",
                    Enumerable.Empty<CarritoViewModel>()
                );
            }

            var carrito = db.Carritos
                .FirstOrDefault(c => c.id_usuario == usuario.id_usuario && c.estado == "Abierto");

            if (carrito == null)
            {
                return PartialView(
                    "~/Views/Shared/_Carrito.cshtml",
                    Enumerable.Empty<CarritoViewModel>()
                );
            }

                var items = (from d in db.Carrito_Detalle
                             join inv in db.Inventario_Detalle on d.id_inventario equals inv.id_inventario
                             join p in db.Productos on inv.id_producto equals p.id_producto
                             where d.id_carrito == carrito.id_carrito
                             select new CarritoViewModel
                             {
                                 IdDetalle = d.id_carrito_detalle,
                                 IdInventario = d.id_inventario,   // <--- añade esto si creaste la propiedad
                                 Nombre = p.nombre_producto,
                                 Precio = p.precio,
                                 Cantidad = d.cantidad,
                                 Imagen = p.url_imagen_principal
                             }).ToList();

                return PartialView("~/Views/Shared/_Carrito.cshtml", items);
        }
       }



     }

}