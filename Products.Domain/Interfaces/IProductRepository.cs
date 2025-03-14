using Products.Application.DTOs;
using Products.Common;

namespace Products.Domain.Interfaces
{
    public interface IProductRepository
    {
        Task<OperationResult<IEnumerable<ProductDto>>> GetAllAsync();
        Task<OperationResult<ProductDto>> GetByIdAsync(int id);
        Task<OperationResult<ProductDto>> AddAsync(ProductDto productDto);
        Task<OperationResult<ProductDto>> UpdateAsync(ProductDto productDto);
        Task<OperationResult<bool>> DeleteAsync(int id);
    }
}
