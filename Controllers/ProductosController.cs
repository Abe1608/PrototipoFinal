
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

        public ActionResult Index(int? idCategoria, string seccion)
        {
            
            var productosQuery = db.Productos
                                   .Where(p => p.activo);

          
            if (idCategoria.HasValue)
            {
                productosQuery = productosQuery
                    .Where(p => p.id_categoria == idCategoria.Value);
            }

            if (!string.IsNullOrEmpty(seccion))
            {
                productosQuery = productosQuery
                    .Where(p => p.seccion == seccion);
            }

            
            var productos = productosQuery
                .OrderBy(p => p.nombre_producto)
                .Select(p => new DestacadosViewModel
                {
                    IdProducto = p.id_producto,
                    Nombre = p.nombre_producto,
                    DescripcionCorta = p.descripcion_corta,
                    Precio = p.precio,
                    Imagen = p.url_imagen_principal,
                    CategoriaNombre = p.Categorias.nombre_categoria
                })
                .ToList();

            
            var categorias = db.Categorias
                               .Where(c => c.activo)
                               .OrderBy(c => c.nombre_categoria)
                               .Select(c => new SelectListItem
                               {
                                   Value = c.id_categoria.ToString(),
                                   Text = c.nombre_categoria,
                                   Selected = idCategoria.HasValue && c.id_categoria == idCategoria.Value
                               })
                               .ToList();
           
            ViewBag.Categorias = categorias;
            ViewBag.SeccionSeleccionada = seccion;

            return View(productos);
        }

        //Metodo para ver los detalles de un producto
        public ActionResult Details(int id)
        {
           
            var prod = db.Productos.Find(id);
            if (prod == null) return HttpNotFound();


            var variantes = db.Inventario_Detalle
                              .Where(i => i.id_producto == id && i.stock_actual > 0)
                              .ToList();
            var colores = variantes
                .Where(v => v.Colores_Catalogo != null)
                .Select(v => v.Colores_Catalogo.nombre_color)
                .Distinct()
                .ToList();

            var tallas = variantes
                .Where(v => v.Tallas_Catalogo != null)
                .Select(v => v.Tallas_Catalogo.nombre_talla)
                .Distinct()
                .ToList();

            
            var vm = new ProductoDetalleViewModel
            {
                Producto = prod,
                Colores = colores,
                Tallas = tallas
            };

            
            return View(vm);
        }

    }
}
