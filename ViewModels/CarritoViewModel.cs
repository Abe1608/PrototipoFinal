using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace PrototipoFinal.ViewModels
{
    public class CarritoViewModel
    {
        public int IdDetalle { get; set; }
        public int  IdInventario { get; set; }
        public string Nombre { get; set; }
        public decimal Precio { get; set; }
        public int Cantidad { get; set; }
        public string Imagen { get; set; }

        public decimal Subtotal => Precio * Cantidad;
    }
}