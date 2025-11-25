
using PrototipoFinal.Models;
using PrototipoFinal.ViewsModels;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;

namespace Prototipado.Controllers
{
    [AllowAnonymous]
    public class ProductosController : Controller
    {
        private readonly FitStyleDBEntities db = new FitStyleDBEntities();

        public ActionResult Index()
        {
            var productos = db.Productos
               .Where(p => p.activo)
               .OrderBy(p => p.nombre_producto)
               .Select(p => new DestacadosViewModel
               {
                   IdProducto = p.id_producto,
                   Nombre = p.nombre_producto,
                   Precio = p.precio,
                   Imagen = p.url_imagen_principal,
                   DescripcionCorta = p.descripcion_corta,
                   CategoriaNombre = p.Categorias.nombre_categoria
               })
               .ToList();

            return View(productos);
        }

        public ActionResult Details(int id)
        {
            var prod = db.Productos.Find(id);
            if (prod == null) return HttpNotFound();
            return View(prod);
        }
    }
}
