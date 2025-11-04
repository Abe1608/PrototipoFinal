
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using Prototipado.Models;

namespace Prototipado.Controllers
{
    [Authorize]
    public class ProductosController : Controller
    {
        internal static readonly List<ProductoVM> Catalogo = new List<ProductoVM>
        {
            new ProductoVM{ Id=1, Nombre="Playera DryFit Unisex", Marca="FitStyle", Precio=14.99m, Stock=10,
                Colores=new[]{"Azul","Negro"}, Tallas=new[]{"S","M","L","XL"}, CategoriaId=1, Destacado=true,
                Imagen="https://images.unsplash.com/photo-1512436991641-6745cdb1723f?q=80&w=1200&auto=format&fit=crop",
                Descripcion="Playera transpirable con tecnología DryFit. Ideal para entrenamiento o uso diario."
            },
            new ProductoVM{ Id=2, Nombre="Leggings Compresión", Marca="FitStyle", Precio=19.99m, Stock=8,
                Colores=new[]{"Negro"}, Tallas=new[]{"XS","S","M","L"}, CategoriaId=1, Destacado=true,
                Imagen="https://images.unsplash.com/photo-1599058945522-28d584b6f0ff?q=80&w=1200&auto=format&fit=crop",
                Descripcion="Tejido elástico con cintura alta y soporte de compresión."
            },
            new ProductoVM{ Id=3, Nombre="Gorra Running Pro", Marca="FitStyle", Precio=9.99m, Stock=15,
                Colores=new[]{"Azul","Blanco"}, Tallas=new[]{"Única"}, CategoriaId=2, Destacado=false,
                Imagen="https://images.unsplash.com/photo-1516826957135-700dedea698c?q=80&w=1200&auto=format&fit=crop",
                Descripcion="Gorra ligera y ventilada para correr bajo el sol."
            },
            new ProductoVM{ Id=4, Nombre="Tenis Entrenamiento X", Marca="FitStyle", Precio=39.99m, Stock=5,
                Colores=new[]{"Negro","Rojo"}, Tallas=new[]{"38","39","40","41"}, CategoriaId=3, Destacado=false,
                Imagen="https://images.unsplash.com/photo-1525966222134-fcfa99b8ae77?q=80&w=1200&auto=format&fit=crop",
                Descripcion="Zapatilla estable para sesiones de gimnasio y cardio."
            },
            new ProductoVM{ Id=5, Nombre="Short Deportivo Breeze", Marca="FitStyle", Precio=12.99m, Stock=12,
                Colores=new[]{"Verde","Negro"}, Tallas=new[]{"S","M","L"}, CategoriaId=1, Destacado=false,
                Imagen="https://images.unsplash.com/photo-1542291026-7eec264c27ff?q=80&w=1200&auto=format&fit=crop",
                Descripcion="Short ultraligero con forro interior y bolsillos laterales."
            }
        };

        public ActionResult Index() { return View(Catalogo); }

        public ActionResult Details(int id)
        {
            var prod = Catalogo.FirstOrDefault(p => p.Id == id);
            if (prod == null) return HttpNotFound();
            return View(prod);
        }
    }
}
