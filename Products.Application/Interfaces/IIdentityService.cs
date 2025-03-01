using Products.Common;

namespace Products.Application.Interfaces
{
    public interface IIdentityService
    {
        Task<OperationResult> CreateUserAsync(string username, string password, string role);
        Task<OperationResult> AssignRoleToUserAsync(string userId, string role);
        Task<OperationResult> LoginAsync(string email, string password);
    }
}
