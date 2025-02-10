using System.ComponentModel.DataAnnotations;

namespace ProductsAPI.Models
{
    public class Product
    {
        public int Id { get; set; }
        [MaxLength(255)]
        public string Name { get; set; }
        public decimal Price { get; set; }
    }
}
