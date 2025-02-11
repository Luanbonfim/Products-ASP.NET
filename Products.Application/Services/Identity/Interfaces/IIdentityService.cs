using Products.Common;

namespace Products.Application.Identity.Interfaces
{
    public interface IIdentityService
    {
        Task<OperationResult> CreateUserAsync(string username, string password, string role);
        Task<OperationResult> AssignRoleToUserAsync(string userId, string role);
        Task<OperationResult> LoginAsync(string userName, string password);
    }
}
