using Microsoft.AspNetCore.Identity;
using Products.Application.Interfaces;
using Products.Common;
using System.Security.Claims;

namespace Products.Infrastructure.Identity
{
    public class IdentityService : IIdentityService
    {
        private readonly UserManager<IdentityUser> _userManager;
        private readonly SignInManager<IdentityUser> _signInManager;
        private const string LOGIN_SUCCESSFUL_MESSAGE = "Login successful";
        private const string USER_NOT_FOUND_MESSAGE = "User not found";

        public IdentityService(UserManager<IdentityUser> userManager, 
                               SignInManager<IdentityUser> signInManager)
        {
            _userManager = userManager;
            _signInManager = signInManager;
        }

        public async Task<OperationResult> CreateUserAsync(string username, string password, string role)
        {
            var user = new IdentityUser { UserName = username };
            var result = await _userManager.CreateAsync(user, password);

            if (result.Succeeded)
            {
                return new OperationResult("User created successfully");
            }

            return new OperationResult(result.Errors.Select(e => e.Description));
        }

        public async Task<OperationResult> AssignRoleToUserAsync(string userId, string role)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user != null)
            { 
                var result = await _userManager.AddToRoleAsync(user, role);
                return result.Succeeded
                    ? new OperationResult("Role assigned successfully")
                    : new OperationResult(result.Errors.Select(e => e.Description));
            }
            return new OperationResult(USER_NOT_FOUND_MESSAGE, false);
        }

        public async Task<OperationResult> LoginAsync(string email, string password)
        {
            var user = await _userManager.FindByNameAsync(email);

            if (user == null)
            {
                return new OperationResult
                {
                    IsSuccess = false,
                    Message = USER_NOT_FOUND_MESSAGE
                };
            }

            var result = await _signInManager.PasswordSignInAsync(user, password, false, lockoutOnFailure: false);

            if (!result.Succeeded)
            {
                return new OperationResult
                {
                    IsSuccess = false,
                    Message = "Invalid credentials"
                };
            }

            return new OperationResult
            {
                IsSuccess = true,
                Message = LOGIN_SUCCESSFUL_MESSAGE
            };
        }

        public async Task<bool> IsSignedIn(ClaimsPrincipal user)
        {
            return await Task.FromResult(_signInManager.IsSignedIn(user));
        }

        public async Task<OperationResult> LogOut()
        {
            try
            {
                await _signInManager.SignOutAsync();

                return new OperationResult("Successful logout", true);
            }
            catch (Exception ex)
            {
                return new OperationResult(ex.Message, false);
            }
        }
        
        public async Task<Object> GetGoogleLoginProperties(string redirectUrl)
        {
            var properties = _signInManager.ConfigureExternalAuthenticationProperties("Google", redirectUrl);

            return properties;
        }

        public async Task<OperationResult> GetGoogleResponse()
        {
            var info = await _signInManager.GetExternalLoginInfoAsync();
            if (info == null)
            {
                return new OperationResult("Error loading external login information.", false);
            }

            // Try to sign in using external login
            var result = await _signInManager.ExternalLoginSignInAsync(info.LoginProvider, info.ProviderKey, isPersistent: false);
            if (result.Succeeded)
            {
                return new OperationResult(LOGIN_SUCCESSFUL_MESSAGE, true);
            }

            // Check if user already exists in Identity (but without Google login linked)
            var email = info.Principal.FindFirstValue(ClaimTypes.Email);
            var user = await _userManager.FindByEmailAsync(email);

            if (user == null)
            {
                // User does not exist, create a new one
                user = new IdentityUser
                {
                    UserName = email,
                    Email = email
                };

                var createResult = await _userManager.CreateAsync(user);
                if (!createResult.Succeeded)
                {
                    return new OperationResult("Error creating user.", false);
                }
            }

            // Link Google login to existing user
            var addLoginResult = await _userManager.AddLoginAsync(user, info);
            if (!addLoginResult.Succeeded)
            {
                return new OperationResult("Error linking Google account.", false);
            }

            // Sign in the user
            await _signInManager.SignInAsync(user, isPersistent: false);
            return new OperationResult(LOGIN_SUCCESSFUL_MESSAGE, true);
        }
    }
}
