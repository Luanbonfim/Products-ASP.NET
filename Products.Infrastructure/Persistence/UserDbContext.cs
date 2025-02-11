using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Products.Infrastructure.Persistence
{
    public class UserDbContext : IdentityDbContext<IdentityUser, IdentityRole, string>
    {
        public UserDbContext(DbContextOptions<UserDbContext> options)
            : base(options)
        {
        }

        public DbSet<IdentityUser> CustomUsers { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            builder.Entity<IdentityUser>().ToTable("Users");  
            builder.Entity<IdentityRole>().ToTable("Roles");  

            // Example: Configure custom user entity
            builder.Entity<IdentityUser>().HasKey(c => c.Id);  
        }
    }

}
