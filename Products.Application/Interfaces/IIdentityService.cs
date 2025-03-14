using Products.Application.DTOs;
using Products.Common;
using System.Security.Claims;

namespace Products.Application.Interfaces
{
    public interface IIdentityService
    {
        Task<OperationResult<string>> CreateUserAsync(UserDto userDto, string role);
        Task<OperationResult<bool>> AssignRoleToUserAsync(string userId, string role);
        Task<OperationResult<bool>> LoginAsync(string email, string password);
        Task<bool> IsSignedIn(ClaimsPrincipal user);
        Task<OperationResult<bool>> LogOut();
        Task<OperationResult<object>> GetGoogleLoginProperties(string redirectUrl);
        Task<OperationResult<bool>> GetGoogleResponse();
    }
}
