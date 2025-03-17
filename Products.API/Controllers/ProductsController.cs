using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Products.Application.DTOs;
using Products.Domain.Interfaces;

namespace Products.Controllers
{
    [ApiController]
    [ApiVersion("1.0")]
    [Route("api/v{version:apiVersion}/[controller]")]
    [Authorize]
    public class ProductsController : ControllerBase
    {
        private readonly IProductRepository _productRepository;

        public ProductsController(IProductRepository productRepository)
        {
            _productRepository = productRepository;
        }

        [HttpGet]
        [MapToApiVersion("1.0")]
        public async Task<IActionResult> GetAll()
        {
            var result = await _productRepository.GetAllAsync();
            
            if (!result.IsSuccess)
                return StatusCode(500, result.Message);

            return Ok(result.Data);
        }

        [HttpGet("{id}")]
        [MapToApiVersion("1.0")]
        public async Task<IActionResult> GetById(int id)
        {
            var result = await _productRepository.GetByIdAsync(id);

            if (!result.IsSuccess)
                return result.Message.Contains("not found") ? NotFound(result.Message) : StatusCode(500, result.Message);

            return Ok(result.Data);
        }

        [HttpPost]
        [MapToApiVersion("1.0")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Add(ProductDto productDto)
        {
            var result = await _productRepository.AddAsync(productDto);

            if (!result.IsSuccess)
                return StatusCode(500, result.Message);

            return StatusCode(201, result.Data);
        }

        [HttpPut("{id}")]
        [MapToApiVersion("1.0")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Update(int id, ProductDto productDto)
        {
            if (id != productDto.Id)
                return BadRequest("ID mismatch");

            var result = await _productRepository.UpdateAsync(productDto);

            if (!result.IsSuccess)
                return result.Message.Contains("not found") ? NotFound(result.Message) : StatusCode(500, result.Message);

            return NoContent();
        }

        [HttpDelete("{id}")]
        [MapToApiVersion("1.0")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(int id)
        {
            var result = await _productRepository.DeleteAsync(id);

            if (!result.IsSuccess)
                return result.Message.Contains("not found") ? NotFound(result.Message) : StatusCode(500, result.Message);

            return NoContent();
        }
    }
}