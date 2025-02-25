﻿using Microsoft.EntityFrameworkCore;
using Products.Application.Interfaces;
using Products.Domain.Entities;
using Products.Infrastructure.Persistence;
using System.Security.Policy;

namespace Products.Infrastructure.Repositories
{
    public class ProductRepository : IProductRepository
    {
        private readonly ProductsDbContext _context;
        private readonly IRabbitMqPublisher _publisher;
        public ProductRepository(ProductsDbContext context, IRabbitMqPublisher publisher)
        {
            _context = context;
            _publisher = publisher;
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

            await PublishMessage(entity, "PRODUCT WAS ADDED");
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

        private async Task PublishMessage(Product produt, string message)
        {
            await _publisher.PublishMessage(new { Id = produt.Id, Name = produt.Name, Message = message });
        }
    }
}
