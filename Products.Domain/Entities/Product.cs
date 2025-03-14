using System.ComponentModel.DataAnnotations;

namespace Products.Domain.Entities
{
    public class Product
    {
        [Key]
        public int Id { get; private set; }
        public string Name { get; private set; }
        public decimal Price { get; private set; }

        public Product(string name, decimal price)
        {
            SetName(name);
            SetPrice(price);
        }

        // Protected constructor for EF Core
        protected Product() { }

        // Domain behavior and business rules
        public void SetPrice(decimal newPrice)
        {
            if (newPrice < 0)
                throw new ArgumentOutOfRangeException(nameof(newPrice), "Price cannot be negative");
            
            Price = newPrice;
        }

        public void SetName(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentException("Product name cannot be empty", nameof(name));
            
            Name = name;
        }
    }
}
