using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Products.Domain.Entities;
using Products.Application.Services.Interfaces;
using Products.Common.Helpers;

namespace Products.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class ProductsController : ControllerBase
    {
        private readonly IProductService _productService;
        private readonly LoggerHelper _loggerHelper;
        public ProductsController(IProductService productService, LoggerHelper loggerHelper)
        {
            _productService = productService;
            _loggerHelper = loggerHelper;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            _loggerHelper.LogMessage(LogLevelType.Information, "Fetching all products.");

            try
            {
                var products = await _productService.GetAllProductsAsync();

                return Ok(products);
            }
            catch (Exception ex)
            {
                return LogError(ex);
            }
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            _loggerHelper.LogMessage(LogLevelType.Information, $"Fetching a product by ID: {id}.");

            try
            {
                var product = await _productService.GetProductByIdAsync(id);

                if (product == null)
                    return NotFound();

                return Ok(product);
            }
            catch (Exception ex)
            {
                return LogError(ex);
            }
        }

        [HttpPost]
        public async Task<IActionResult> Add(Product product)
        {
            _loggerHelper.LogMessage(LogLevelType.Information, $"Adding product ID: {product.Id}.");

            try
            {
                await _productService.AddProductAsync(product);
                return StatusCode(201);
            }
            catch (Exception ex)
            {
                return LogError(ex);
            }
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, Product product)
        {
            _loggerHelper.LogMessage(LogLevelType.Information, $"Updating product ID: {product.Id}.");

            try
            {

                if (id != product.Id) return BadRequest();
                await _productService.UpdateProductAsync(product);

                return NoContent();
            }
            catch (Exception ex)
            {
                return LogError(ex);
            }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                _loggerHelper.LogMessage(LogLevelType.Information, $"Deleting product ID: {id}.");

                await _productService.DeleteProductAsync(id);
                return NoContent();
            }
            catch (Exception ex)
            {
                return LogError(ex);
            }
        }

        private IActionResult LogError(Exception ex)
        {
            _loggerHelper.LogMessage(LogLevelType.Error, ex.Message);

            return StatusCode(500, "Internal server error");
        }
    }
}