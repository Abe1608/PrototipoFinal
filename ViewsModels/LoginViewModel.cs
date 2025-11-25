using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace PrototipoFinal.ViewsModels
{
    public class LoginViewModel
    {
        [Required, Display(Name = "Correo")]
        public string email { get; set; }

        [Required, DataType(DataType.Password), Display(Name = "Contraseña")]
        public string password_hash { get; set; }

        [Display(Name = "Recordarme")]
        public bool Recordarme { get; set; }

    }
}