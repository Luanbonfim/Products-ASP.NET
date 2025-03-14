using Microsoft.EntityFrameworkCore;
using Products.Application.DTOs;
using Products.Application.Interfaces;
using Products.Common;
using Products.Common.Helpers;
using Products.Domain.Entities;
using Products.Domain.Interfaces;
using Products.Infrastructure.Persistence;

namespace Products.Infrastructure.Repositories
{
    public class ProductRepository : IProductRepository
    {
        private readonly ProductsDbContext _context;
        private readonly IRabbitMqPublisher _publisher;
        private readonly LoggerHelper _logger;
        
        public ProductRepository(
            ProductsDbContext context, 
            IRabbitMqPublisher publisher,
            LoggerHelper logger)
        {
            _context = context;
            _publisher = publisher;
            _logger = logger;
        }

        public async Task<OperationResult<IEnumerable<ProductDto>>> GetAllAsync()
        {
            try
            {
                _logger.LogMessage(LogLevelType.Information, "Fetching all products.");
                var products = await _context.Products.ToListAsync();
                var productDtos = products.Select(p => new ProductDto 
                { 
                    Id = p.Id,
                    Name = p.Name,
                    Price = p.Price 
                });

                return new OperationResult<IEnumerable<ProductDto>>
                {
                    IsSuccess = true,
                    Data = productDtos,
                    Message = "Products retrieved successfully"
                };
            }
            catch (Exception ex)
            {
                _logger.LogMessage(LogLevelType.Error, $"Error retrieving products: {ex.Message}");
                return new OperationResult<IEnumerable<ProductDto>>
                {
                    IsSuccess = false,
                    Message = "Error retrieving products",
                    Errors = new List<string> { ex.Message }
                };
            }
        }

        public async Task<OperationResult<ProductDto>> GetByIdAsync(int id)
        {
            try
            {
                _logger.LogMessage(LogLevelType.Information, $"Fetching product by ID: {id}.");
                var product = await _context.Products.FindAsync(id);
                if (product == null)
                {
                    _logger.LogMessage(LogLevelType.Warning, $"Product with ID {id} not found.");
                    return new OperationResult<ProductDto>
                    {
                        IsSuccess = false,
                        Message = $"Product with ID {id} not found",
                        Errors = new List<string> { "Product not found" }
                    };
                }

                var productDto = new ProductDto
                {
                    Id = product.Id,
                    Name = product.Name,
                    Price = product.Price
                };

                return new OperationResult<ProductDto>
                {
                    IsSuccess = true,
                    Data = productDto,
                    Message = "Product retrieved successfully"
                };
            }
            catch (Exception ex)
            {
                _logger.LogMessage(LogLevelType.Error, $"Error retrieving product: {ex.Message}");
                return new OperationResult<ProductDto>
                {
                    IsSuccess = false,
                    Message = "Error retrieving product",
                    Errors = new List<string> { ex.Message }
                };
            }
        }

        public async Task<OperationResult<ProductDto>> AddAsync(ProductDto productDto)
        {
            try
            {
                _logger.LogMessage(LogLevelType.Information, "Adding new product.");
                var product = new Product(productDto.Name, productDto.Price);
                
                await _context.Products.AddAsync(product);
                await _context.SaveChangesAsync();
                await PublishMessage(product, "PRODUCT WAS ADDED");

                productDto.Id = product.Id;
                return new OperationResult<ProductDto>
                {
                    IsSuccess = true,
                    Data = productDto,
                    Message = "Product added successfully"
                };
            }
            catch (Exception ex)
            {
                _logger.LogMessage(LogLevelType.Error, $"Error adding product: {ex.Message}");
                return new OperationResult<ProductDto>
                {
                    IsSuccess = false,
                    Message = "Error adding product",
                    Errors = new List<string> { ex.Message }
                };
            }
        }

        public async Task<OperationResult<ProductDto>> UpdateAsync(ProductDto productDto)
        {
            try
            {
                _logger.LogMessage(LogLevelType.Information, $"Updating product ID: {productDto.Id}.");
                if (!productDto.Id.HasValue)
                {
                    _logger.LogMessage(LogLevelType.Warning, "Product ID is required for updates.");
                    return new OperationResult<ProductDto>
                    {
                        IsSuccess = false,
                        Message = "Product ID is required for updates",
                        Errors = new List<string> { "Invalid product ID" }
                    };
                }

                var product = await _context.Products.FindAsync(productDto.Id.Value);
                if (product == null)
                {
                    _logger.LogMessage(LogLevelType.Warning, $"Product with ID {productDto.Id.Value} not found.");
                    return new OperationResult<ProductDto>
                    {
                        IsSuccess = false,
                        Message = $"Product with ID {productDto.Id.Value} not found",
                        Errors = new List<string> { "Product not found" }
                    };
                }

                product.SetName(productDto.Name);
                product.SetPrice(productDto.Price);

                _context.Products.Update(product);
                await _context.SaveChangesAsync();
                await PublishMessage(product, "PRODUCT WAS UPDATED");

                return new OperationResult<ProductDto>
                {
                    IsSuccess = true,
                    Data = productDto,
                    Message = "Product updated successfully"
                };
            }
            catch (Exception ex)
            {
                _logger.LogMessage(LogLevelType.Error, $"Error updating product: {ex.Message}");
                return new OperationResult<ProductDto>
                {
                    IsSuccess = false,
                    Message = "Error updating product",
                    Errors = new List<string> { ex.Message }
                };
            }
        }

        public async Task<OperationResult<bool>> DeleteAsync(int id)
        {
            try
            {
                _logger.LogMessage(LogLevelType.Information, $"Deleting product ID: {id}.");
                var product = await _context.Products.FindAsync(id);
                if (product == null)
                {
                    _logger.LogMessage(LogLevelType.Warning, $"Product with ID {id} not found.");
                    return new OperationResult<bool>
                    {
                        IsSuccess = false,
                        Message = $"Product with ID {id} not found",
                        Errors = new List<string> { "Product not found" }
                    };
                }

                _context.Products.Remove(product);
                await _context.SaveChangesAsync();

                await PublishMessage(product, "PRODUCT WAS DELETED");

                return new OperationResult<bool>
                {
                    IsSuccess = true,
                    Data = true,
                    Message = "Product deleted successfully"
                };
            }
            catch (Exception ex)
            {
                _logger.LogMessage(LogLevelType.Error, $"Error deleting product: {ex.Message}");
                return new OperationResult<bool>
                {
                    IsSuccess = false,
                    Message = "Error deleting product",
                    Errors = new List<string> { ex.Message }
                };
            }
        }

        private async Task PublishMessage(Product product, string message)
        {
            await _publisher.PublishMessage(new { Id = product.Id, Name = product.Name, Message = message });
        }
    }
}
