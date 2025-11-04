
namespace Prototipado.Models
{
    public class ProductoVM
    {
        public int Id { get; set; }
        public string Nombre { get; set; }
        public string Marca { get; set; }
        public string Descripcion { get; set; }
        public decimal Precio { get; set; }
        public int Stock { get; set; }
        public string Imagen { get; set; }
        public string[] Colores { get; set; }
        public string[] Tallas { get; set; }
        public int CategoriaId { get; set; }
        public bool Destacado { get; set; }
    }
}
