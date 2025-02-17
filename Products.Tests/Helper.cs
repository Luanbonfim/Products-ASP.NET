using Microsoft.EntityFrameworkCore;
using Products.Infrastructure.Persistence;

namespace Products.Tests
{
    public static class Helper
    {
        public static DbContextOptions<T> GetInMemoryOptions<T>(string databaseName) where T : DbContext
        {
            return new DbContextOptionsBuilder<T>()
                .UseInMemoryDatabase(databaseName: databaseName)
                .Options;
        }
    }
}
