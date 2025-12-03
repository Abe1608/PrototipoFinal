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
        private readonly FitStyleDBEntities db = new FitStyleDBEntities();

        //Obtiene el usuario que se a logueado en el sistema
        private int? GetUsuarioId()
        {
           
            if (!User.Identity.IsAuthenticated)
            {
                
                Session["UserId"] = null;
                return null;
            }

       
            if (Session["UserId"] is int idSesion)
                return idSesion;

           
            var email = User.Identity.Name;

            var usuario = db.Usuarios.SingleOrDefault(u => u.email == email);
            if (usuario != null)
            {
                Session["UserId"] = usuario.id_usuario; 
                return usuario.id_usuario;
            }

            
            return null;
        }


        //Genera un nuevo carrito si no existe uno abierto para el usuario
        //Este es para abrir el carrito en la tabla Carritos
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

        //Obtiene la cantidad de productos en el carrito
        private int GetCartCount(int idUsuario)
        {
            var carrito = db.Carritos
                .SingleOrDefault(c => c.id_usuario == idUsuario && c.estado == "Abierto");

            if (carrito == null) return 0;

            return db.Carrito_Detalle
                .Where(d => d.id_carrito == carrito.id_carrito)
                .Sum(d => (int?)d.cantidad) ?? 0;
        }


        //Metodo para agregar mas productos al carrito
        [HttpPost]
        public JsonResult AgregarMasProducto(int idInventario)
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

        //Metodo para restar productos del carrito
        [HttpPost]
        public JsonResult RestarProducto(int idInventario)
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


        //Metodo para eliminar productos del carrito
        [HttpPost]
        public JsonResult EliminarProducto(int idDetalle)
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
        //Este es para agregar el producto desde la vista de productos
        [HttpPost]
        public ActionResult AgregarPorProducto(int idProducto, int cantidad = 1)
        {
            var idUsuario = GetUsuarioId();
            if (idUsuario == null)
            {
                return Json(new
                {
                    success = false,
                    notLogged = true,
                });
            }

          
            var inventario = db.Inventario_Detalle
                               .FirstOrDefault(i => i.id_producto == idProducto);

            if (inventario == null)
            {
                return Json(new { success = false, message = "No hay inventario disponible." });
            }

            int idInventario = inventario.id_inventario;

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

            var totalItems = db.Carrito_Detalle
                            .Where(d => d.id_carrito == idCarrito)
                            .Sum(d => (int?)d.cantidad) ?? 0;

            return Json(new { success = true, count = totalItems });
        }


        // Método para que el offcanvas del carrito muestre los productos agregados
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
                                 IdInventario = d.id_inventario,
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