using Products.Application.DTOs;
using Products.Common;
using System.Security.Claims;

namespace Products.Application.Interfaces
{
    public interface IIdentityService
    {
        Task<OperationResult<string>> CreateUserAsync(UserDto userDto, string role);
        Task<OperationResult<bool>> AssignRoleToUserAsync(string userId, string role);
        Task<OperationResult<UserDto>> LoginAsync(string email, string password);
        Task<OperationResult<UserDto>> IsSignedIn(ClaimsPrincipal user);
        Task<OperationResult<bool>> LogOut();
        Task<OperationResult<object>> GetGoogleLoginProperties(string redirectUrl);
        Task<OperationResult<bool>> GetGoogleResponse();
    }
}
