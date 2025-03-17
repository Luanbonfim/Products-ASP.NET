using System.ComponentModel.DataAnnotations;

namespace Products.Application.DTOs
{
    public class UserDto
    {
        public string? Id { get; set; }

        [Required]
        [EmailAddress]
        public string Email { get; set; }

        [Required]
        public string Password { get; set; }

        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public List<string>? Roles { get; set; } = new List<string>();
    }
}
