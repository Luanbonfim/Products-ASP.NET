using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Products.Application.DTOs;
using Products.Application.Interfaces;
using Products.Common.Helpers;
using Products.Domain.Entities;
using Products.Domain.Interfaces;
using Products.Infrastructure.Messaging;
using Products.Infrastructure.Persistence;
using Products.Infrastructure.Repositories;
using Xunit;

namespace Products.Tests
{
    public class ProductRepositoryTests
    {
        private readonly DbContextOptions<ProductsDbContext> _options;
        private readonly IProductRepository _repository;
        private const string USERS_DATABASE_TEST = "TestDatabase";

        public ProductRepositoryTests()
        {
            _options = Helper.GetInMemoryOptions<ProductsDbContext>(USERS_DATABASE_TEST);

            var serviceProvider = SetupTestServiceProvider();
            _repository = serviceProvider.GetRequiredService<IProductRepository>();

            SeedInitialData().GetAwaiter().GetResult();
        }

        [Fact]
        public async Task GetAllAsync_ReturnsProducts()
        {
            // Arrange

            // Act
            var result = await _repository.GetAllAsync();

            // Assert
            Assert.True(result.IsSuccess);
            Assert.True(result.Data.Count() > 0);
            Assert.All(result.Data, item => Assert.IsType<ProductDto>(item));
        }

        [Fact]
        public async Task GetByIdAsync_ReturnsProductById()
        {
            //Arrange
            var products = await _repository.GetAllAsync();
            var firstProduct = products.Data.First();

            //Act
            var result = await _repository.GetByIdAsync(firstProduct?.Id ?? 0);

            // Assert: 
            Assert.True(result.IsSuccess);
            Assert.NotNull(result.Data);
            Assert.IsType<ProductDto>(result.Data);
            Assert.Equal(firstProduct.Id, result.Data.Id);
        }

        [Fact]
        public async Task AddAsync_AddsAProduct()
        {
            //Arrange
            var newProduct = new ProductDto { Name = "Test Product 3", Price = 100 };

            //Act 
            var result = await _repository.AddAsync(newProduct);

            // Assert: 
            Assert.True(result.IsSuccess);
            Assert.NotNull(result.Data);
            Assert.Equal(newProduct.Name, result.Data.Name);
            Assert.Equal(newProduct.Price, result.Data.Price);

            // Verify in database
            using (var context = new ProductsDbContext(_options))
            {
                var addedProduct = await context.Products.FirstOrDefaultAsync(p => p.Id == result.Data.Id);
                Assert.NotNull(addedProduct);
                Assert.Equal(newProduct.Name, addedProduct.Name);
                Assert.Equal(newProduct.Price, addedProduct.Price);
            }
        }

        [Fact]
        public async Task UpdateAsync_UpdatesAnExistingProduct()
        {
            //Arrange 
            var products = await _repository.GetAllAsync();
            var productToUpdate = products.Data.First();
            productToUpdate.Name = "Updated Product";
            productToUpdate.Price = 150;

            //Act
            var result = await _repository.UpdateAsync(productToUpdate);

            // Assert: 
            Assert.True(result.IsSuccess);
            Assert.NotNull(result.Data);
            Assert.Equal(productToUpdate.Name, result.Data.Name);
            Assert.Equal(productToUpdate.Price, result.Data.Price);

            // Verify in database
            using (var context = new ProductsDbContext(_options))
            {
                var updatedProduct = await context.Products.FirstAsync(p => p.Id == productToUpdate.Id);
                Assert.Equal(productToUpdate.Name, updatedProduct.Name);
                Assert.Equal(productToUpdate.Price, updatedProduct.Price);
            }
        }

        [Fact]
        public async Task DeleteAsync_DeletesAnExistingProductById()
        {
            //Arrange 
            int idToDelete;
            using (var context = new ProductsDbContext(_options))
            {
                var productToDelete = await context.Products.FirstAsync();
                idToDelete = productToDelete.Id;
            }

            //Act
            var result = await _repository.DeleteAsync(idToDelete);

            // Assert
            Assert.True(result.IsSuccess);
            
            // Verify in database
            using (var context = new ProductsDbContext(_options))
            {
                var isDeletedProductFound = await context.Products.AnyAsync(p => p.Id == idToDelete);
                Assert.False(isDeletedProductFound);
            }
        }

        private async Task SeedInitialData()
        {
            var productsList = new List<ProductDto>
            {
                new ProductDto { Name = "Test Product", Price = 100 },
                new ProductDto { Name = "Test Product 2", Price = 100 }
            };

            foreach (var product in productsList)
            {
                await _repository.AddAsync(product);
            }
        }

        private ServiceProvider SetupTestServiceProvider()
        {
            var services = new ServiceCollection();

            services.AddDbContext<ProductsDbContext>(options => options.UseInMemoryDatabase(USERS_DATABASE_TEST));

            services.AddSingleton<IRabbitMqPublisher, RabbitMqPublisher>();
            services.AddLogging();
            services.AddSingleton<LoggerHelper>();
            services.AddScoped<IProductRepository, ProductRepository>();

            services.AddSingleton(GetConfiguration());

            var serviceProvider = services.BuildServiceProvider();

            return serviceProvider;
        }

        private IConfiguration GetConfiguration()
        {
            var configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())  
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)  
                .Build();

            return configuration;
        }
    }
}