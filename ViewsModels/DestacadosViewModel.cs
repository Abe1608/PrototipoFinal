using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace PrototipoFinal.ViewsModels
{
    public class DestacadosViewModel
    {
        public int IdProducto { get; set; }
        public string Nombre { get; set; }
        public decimal Precio { get; set; }
        public string Imagen { get; set; }
        public string CategoriaNombre { get; set; }   
        public string DescripcionCorta { get; set; }
    }
}