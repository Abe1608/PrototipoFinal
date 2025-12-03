using PrototipoFinal.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace PrototipoFinal.ViewModels
{
    public class ProductoDetalleViewModel
    {
       
        public Productos Producto { get; set; }
        public IEnumerable<string> Colores { get; set; }
        public IEnumerable<string> Tallas { get; set; }

    }
}