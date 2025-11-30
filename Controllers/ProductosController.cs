
using PrototipoFinal.Models;
using PrototipoFinal.ViewModels;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;

namespace Prototipado.Controllers
{
    [AllowAnonymous]
    public class ProductosController : Controller
    {
        private readonly FitStyleDBEntities db = new FitStyleDBEntities();

        public ActionResult Index(int? idCategoria)
        {

        var productosQuery = db.Productos
       .Where(p => p.activo); 

            // Si se proporciona idCategoria, filtrar por categoría
            if (idCategoria.HasValue)
            {
                productosQuery = productosQuery
                    .Where(p => p.id_categoria == idCategoria.Value);
            }

            var productos = productosQuery
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
