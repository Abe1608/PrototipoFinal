
using System.ComponentModel.DataAnnotations;

namespace Prototipado.Models
{
    public class RegisterVM
    {
        [Required, Display(Name = "Nombre completo")]
        public string Nombre { get; set; }

        [Required, EmailAddress, Display(Name = "Correo electrónico")]
        public string Email { get; set; }

        [Required, MinLength(6), DataType(DataType.Password), Display(Name = "Contraseña")]
        public string Password { get; set; }

        [Required, Compare("Password"), DataType(DataType.Password), Display(Name = "Confirmar contraseña")]
        public string ConfirmPassword { get; set; }
    }
}
