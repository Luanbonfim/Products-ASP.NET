using Products.Common;
using System.Security.Claims;

namespace Products.Application.Interfaces
{
    public interface IIdentityService
    {
        Task<OperationResult> CreateUserAsync(string username, string password, string role);
        Task<OperationResult> AssignRoleToUserAsync(string userId, string role);
        Task<OperationResult> LoginAsync(string email, string password);
        Task<bool> IsSignedIn(ClaimsPrincipal user);
        Task<OperationResult> LogOut();
        Task<Object> GetGoogleLoginProperties(string redirectUrl);
        Task<OperationResult> GetGoogleResponse();
    }
}
