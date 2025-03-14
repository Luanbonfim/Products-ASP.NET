using System.ComponentModel.DataAnnotations;

namespace Products.Application.DTOs
{
    public class ProductDto
    {
        public int? Id { get; set; }
        
        [Required]
        [StringLength(100)]
        public string Name { get; set; }
        
        [Range(0, double.MaxValue)]
        public decimal Price { get; set; }
    }
} 