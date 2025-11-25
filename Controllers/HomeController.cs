using PrototipoFinal.Models;
using PrototipoFinal.ViewsModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace PrototipoFinal.Controllers
{

    public class HomeController : Controller
    {

        private readonly FitStyleDBEntities db = new FitStyleDBEntities();


        [AllowAnonymous]
        public ActionResult Index()
        {
            var destacados = db.Productos
                .Where(p => p.activo && p.destacado)
                .OrderBy(p => p.id_producto)
                .Take(8)
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

            return View(destacados);
        }


    }
}