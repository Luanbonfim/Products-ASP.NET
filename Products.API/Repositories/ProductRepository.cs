using Products.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Products.Infrastructure.Persistence;

namespace Products.Repositories
{
    public class ProductRepository : IProductRepository
    {
        private readonly ProductsDbContext _context;

        public ProductRepository(ProductsDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Product>> GetAllAsync()
        {
            return await _context.Set<Product>().ToListAsync();
        }

        public async Task<Product> GetByIdAsync(int id)
        {
            return await _context.Set<Product>().FindAsync(id);
        }

        public async Task AddAsync(Product entity)
        {
            await _context.Set<Product>().AddAsync(entity);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(Product entity)
        {
            _context.Set<Product>().Update(entity);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(int id)
        {
            var entity = await _context.Set<Product>().FindAsync(id);
            if (entity != null)
            {
                _context.Set<Product>().Remove(entity);
                await _context.SaveChangesAsync();
            }
        }
    }
}
