using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

using System.ComponentModel.DataAnnotations;

namespace PrototipoFinal.ViewModels
{
    public class PerfilViewModel
    {
        [Required, StringLength(80)]
        [Display(Name = "Nombre completo")]
        public string NombreCompleto { get; set; }

        [StringLength(20)]
        [Display(Name = "Teléfono")]
        public string Telefono { get; set; }

        [Required, Display(Name = "Dirección")]
        public string DireccionLinea1 { get; set; }

        [Display(Name = "Complemento (opcional)")]
        public string DireccionLinea2 { get; set; }

        [Required]
        public string Ciudad { get; set; }

        public string Departamento { get; set; }

        [Required]
        public string Pais { get; set; }

        [Display(Name = "Código postal")]
        public string CodigoPostal { get; set; }
    }
}
