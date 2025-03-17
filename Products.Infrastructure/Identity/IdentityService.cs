using Microsoft.AspNetCore.Identity;
using Products.Application.DTOs;
using Products.Application.Interfaces;
using Products.Common;
using Products.Common.Helpers;
using System.Security.Claims;
using Microsoft.EntityFrameworkCore;
using Products.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore.Storage;

namespace Products.Infrastructure.Identity
{
    public class IdentityService : IIdentityService
    {
        private readonly UserManager<IdentityUser> _userManager;
        private readonly SignInManager<IdentityUser> _signInManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly LoggerHelper _loggerHelper;
        private readonly UserDbContext _userDbContext;
        private const string LOGIN_SUCCESSFUL_MESSAGE = "Login successful";
        private const string USER_NOT_FOUND_MESSAGE = "User not found";

        public IdentityService(UserManager<IdentityUser> userManager, 
                             SignInManager<IdentityUser> signInManager,
                             RoleManager<IdentityRole> roleManager,
                             LoggerHelper loggerHelper,
                             UserDbContext context)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _roleManager = roleManager;
            _loggerHelper = loggerHelper;
            _userDbContext = context;
        }

        private async Task<OperationResult<bool>> CreateRoleIfNotExistsAsync(string role)
        {
            if (!await _roleManager.RoleExistsAsync(role))
            {
                var result = await _roleManager.CreateAsync(new IdentityRole(role));
                if (result.Succeeded)
                {
                    _loggerHelper.LogMessage(LogLevelType.Information, $"Role {role} created successfully");
                    return new OperationResult<bool>
                    {
                        IsSuccess = true,
                        Data = true,
                        Message = $"Role {role} created successfully"
                    };
                }
                else
                {
                    _loggerHelper.LogMessage(LogLevelType.Error, $"Failed to create role {role}: {string.Join(", ", result.Errors.Select(e => e.Description))}");
                    return new OperationResult<bool>
                    {
                        IsSuccess = false,
                        Data = false,
                        Message = "Failed to create role",
                        Errors = result.Errors.Select(e => e.Description).ToList()
                    };
                }
            }
            return new OperationResult<bool>
            {
                IsSuccess = true,
                Data = true,
                Message = $"Role {role} already exists"
            };
        }

        public async Task<OperationResult<string>> CreateUserAsync(UserDto userDto, string role)
        {
            using var transaction = await _userDbContext.Database.BeginTransactionAsync();
            try
            {
                // First, ensure the role exists
                var roleResult = await CreateRoleIfNotExistsAsync(role);
                if (!roleResult.IsSuccess)
                {
                    await transaction.RollbackAsync();
                    return new OperationResult<string>
                    {
                        IsSuccess = false,
                        Message = "Failed to create role",
                        Errors = roleResult.Errors
                    };
                }

                var user = new IdentityUser 
                { 
                    UserName = userDto.Email,
                    Email = userDto.Email,
                };

                var result = await _userManager.CreateAsync(user, userDto.Password);

                if (result.Succeeded)
                {

                    await CrateUserClaims(userDto.FirstName, userDto.LastName, user);


                    var roleAssignmentResult = await _userManager.AddToRoleAsync(user, role);
                    if (!roleAssignmentResult.Succeeded)
                    {
                        _loggerHelper.LogMessage(LogLevelType.Warning, $"User created but failed to assign role: {string.Join(", ", roleAssignmentResult.Errors.Select(e => e.Description))}");
                        await transaction.RollbackAsync();
                        return new OperationResult<string>
                        {
                            IsSuccess = false,
                            Message = "Failed to assign role to user",
                            Errors = roleAssignmentResult.Errors.Select(e => e.Description).ToList()
                        };
                    }

                    await transaction.CommitAsync();
                    _loggerHelper.LogMessage(LogLevelType.Information, $"User {userDto.Email} created successfully with role {role}");
                    return new OperationResult<string>
                    {
                        IsSuccess = true,
                        Data = user.Id,
                        Message = "User created successfully"
                    };
                }

                await transaction.RollbackAsync();
                var errorMessage = string.Join(", ", result.Errors.Select(e => e.Description));
                _loggerHelper.LogMessage(LogLevelType.Error, $"Error creating user {userDto.Email}: {errorMessage}");
                return new OperationResult<string>
                {
                    IsSuccess = false,
                    Errors = result.Errors.Select(e => e.Description).ToList()
                };
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                _loggerHelper.LogMessage(LogLevelType.Error, $"Transaction failed while creating user {userDto.Email}: {ex.Message}");
                return new OperationResult<string>
                {
                    IsSuccess = false,
                    Message = "An error occurred while creating the user",
                    Errors = new List<string> { ex.Message }
                };
            }
        }

        private async Task CrateUserClaims(string firstName, string lastName, IdentityUser user)
        {
            if (!string.IsNullOrEmpty(firstName))
            {
                await _userManager.AddClaimAsync(user, new Claim(ClaimTypes.GivenName, firstName));
            }
            if (!string.IsNullOrEmpty(lastName))
            {
                await _userManager.AddClaimAsync(user, new Claim(ClaimTypes.Surname, lastName));
            }
        }

        public async Task<OperationResult<bool>> AssignRoleToUserAsync(string userId, string role)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                _loggerHelper.LogMessage(LogLevelType.Error, $"User with ID {userId} not found when trying to assign role {role}");
                return new OperationResult<bool>
                {
                    IsSuccess = false,
                    Message = USER_NOT_FOUND_MESSAGE
                };
            }

            var result = await _userManager.AddToRoleAsync(user, role);
            if (result.Succeeded)
            {
                _loggerHelper.LogMessage(LogLevelType.Information, $"Role {role} successfully assigned to user {user.UserName}");
            }
            else
            {
                _loggerHelper.LogMessage(LogLevelType.Error, $"Failed to assign role {role} to user {user.UserName}: {string.Join(", ", result.Errors.Select(e => e.Description))}");
            }

            return new OperationResult<bool>
            {
                IsSuccess = result.Succeeded,
                Data = result.Succeeded,
                Message = result.Succeeded ? "Role assigned successfully" : "Failed to assign role",
                Errors = result.Succeeded ? null : result.Errors.Select(e => e.Description).ToList()
            };
        }

        public async Task<OperationResult<UserDto>> LoginAsync(string email, string password)
        {
            var user = await _userManager.FindByNameAsync(email);

            if (user == null)
            {
                _loggerHelper.LogMessage(LogLevelType.Warning, $"Login attempt failed: User {email} not found");
                return new OperationResult<UserDto>
                {
                    IsSuccess = false,
                    Message = USER_NOT_FOUND_MESSAGE
                };
            }

            var result = await _signInManager.PasswordSignInAsync(user, password, false, lockoutOnFailure: false);

            if (result.Succeeded)
            {
                _loggerHelper.LogMessage(LogLevelType.Information, $"User {email} logged in successfully");
                
                var claims = await _userManager.GetClaimsAsync(user);
                var roles = await _userManager.GetRolesAsync(user);

                var userDto = new UserDto
                {
                    Id = user.Id,
                    Email = user.Email,
                    FirstName = claims.FirstOrDefault(c => c.Type == ClaimTypes.GivenName)?.Value,
                    LastName = claims.FirstOrDefault(c => c.Type == ClaimTypes.Surname)?.Value,
                    Roles = roles.ToList()
                };

                return new OperationResult<UserDto>
                {
                    IsSuccess = true,
                    Data = userDto,
                    Message = LOGIN_SUCCESSFUL_MESSAGE
                };
            }
            else
            {
                _loggerHelper.LogMessage(LogLevelType.Warning, $"Failed login attempt for user {email}");
                return new OperationResult<UserDto>
                {
                    IsSuccess = false,
                    Message = "Invalid credentials"
                };
            }
        }

        public async Task<OperationResult<UserDto>> IsSignedIn(ClaimsPrincipal user)
        {
            if (!_signInManager.IsSignedIn(user))
            {
                _loggerHelper.LogMessage(LogLevelType.Warning, "User is not signed in");
                return new OperationResult<UserDto>
                {
                    IsSuccess = false,
                    Message = "User is not signed in"
                };
            }

            var userId = user.FindFirstValue(ClaimTypes.NameIdentifier);
            var identityUser = await _userManager.FindByIdAsync(userId);

            if (identityUser == null)
            {
                _loggerHelper.LogMessage(LogLevelType.Warning, "User not found in database");
                return new OperationResult<UserDto>
                {
                    IsSuccess = false,
                    Message = "User not found"
                };
            }

            var claims = await _userManager.GetClaimsAsync(identityUser);
            var roles = await _userManager.GetRolesAsync(identityUser);

            var userDto = new UserDto
            {
                Id = identityUser.Id,
                Email = identityUser.Email,
                FirstName = claims.FirstOrDefault(c => c.Type == ClaimTypes.GivenName)?.Value,
                LastName = claims.FirstOrDefault(c => c.Type == ClaimTypes.Surname)?.Value,
                Roles = roles.ToList()
            };

            _loggerHelper.LogMessage(LogLevelType.Information, $"User {identityUser.Email} is signed in");
            return new OperationResult<UserDto>
            {
                IsSuccess = true,
                Data = userDto,
                Message = "User is signed in"
            };
        }

        public async Task<OperationResult<bool>> LogOut()
        {
            try
            {
                var userName = _signInManager.Context?.User?.Identity?.Name;
                await _signInManager.SignOutAsync();
                _loggerHelper.LogMessage(LogLevelType.Information, $"User {userName ?? "Unknown"} logged out successfully");

                return new OperationResult<bool>
                {
                    IsSuccess = true,
                    Data = true,
                    Message = "Successful logout"
                };
            }
            catch (Exception ex)
            {
                _loggerHelper.LogMessage(LogLevelType.Error, $"Logout failed: {ex.Message}");
                return new OperationResult<bool>
                {
                    IsSuccess = false,
                    Data = false,
                    Message = "Logout failed",
                    Errors = new List<string> { ex.Message }
                };
            }
        }
        
        public async Task<OperationResult<object>> GetGoogleLoginProperties(string redirectUrl)
        {
            try
            {
                var properties = _signInManager.ConfigureExternalAuthenticationProperties("Google", redirectUrl);
                _loggerHelper.LogMessage(LogLevelType.Information, "Google login properties configured successfully");
                return new OperationResult<object>
                {
                    IsSuccess = true,
                    Data = properties,
                    Message = "Google login properties configured successfully"
                };
            }
            catch (Exception ex)
            {
                _loggerHelper.LogMessage(LogLevelType.Error, $"Failed to configure Google login properties: {ex.Message}");
                return new OperationResult<object>
                {
                    IsSuccess = false,
                    Message = "Failed to configure Google login properties",
                    Errors = new List<string> { ex.Message }
                };
            }
        }

        public async Task<OperationResult<bool>> GetGoogleResponse()
        {
            var info = await _signInManager.GetExternalLoginInfoAsync();
            if (info == null)
            {
                _loggerHelper.LogMessage(LogLevelType.Error, "Error loading external login information from Google");
                return new OperationResult<bool>
                {
                    IsSuccess = false,
                    Data = false,
                    Message = "Error loading external login information"
                };
            }

            var result = await _signInManager.ExternalLoginSignInAsync(info.LoginProvider, info.ProviderKey, isPersistent: false);
            if (result.Succeeded)
            {
                _loggerHelper.LogMessage(LogLevelType.Information, $"User successfully logged in via Google: {info.Principal.FindFirstValue(ClaimTypes.Email)}");
                return new OperationResult<bool>
                {
                    IsSuccess = true,
                    Data = true,
                    Message = LOGIN_SUCCESSFUL_MESSAGE
                };
            }

            return await CreateUserAndSignInWithGoogleInfo(info);
        }

        private async Task<OperationResult<bool>> CreateUserAndSignInWithGoogleInfo(ExternalLoginInfo info)
        {
            var email = info.Principal.FindFirstValue(ClaimTypes.Email);
            var firstName = info.Principal.FindFirstValue(ClaimTypes.GivenName);
            var lastName = info.Principal.FindFirstValue(ClaimTypes.Surname);
            var user = await _userManager.FindByEmailAsync(email);

            using var transaction = await _userDbContext.Database.BeginTransactionAsync();
            try
            {
                if (user == null)
                {
                    user = new IdentityUser
                    {
                        UserName = email,
                        Email = email
                    };

                    var createResult = await _userManager.CreateAsync(user);
                    if (!createResult.Succeeded)
                    {
                        var errorMessage = string.Join(", ", createResult.Errors.Select(e => e.Description));
                        _loggerHelper.LogMessage(LogLevelType.Error, $"Error creating user from Google login - Email: {email}, Errors: {errorMessage}");
                        await transaction.RollbackAsync();
                        return new OperationResult<bool>
                        {
                            IsSuccess = false,
                            Data = false,
                            Message = "Error creating user",
                            Errors = createResult.Errors.Select(e => e.Description).ToList()
                        };
                    }

                    await CrateUserClaims(firstName, lastName, user);
                    _loggerHelper.LogMessage(LogLevelType.Information, $"New user created from Google login: {email}");
                }

                var addLoginResult = await _userManager.AddLoginAsync(user, info);
                if (!addLoginResult.Succeeded)
                {
                    var errorMessage = string.Join(", ", addLoginResult.Errors.Select(e => e.Description));
                    _loggerHelper.LogMessage(LogLevelType.Error, $"Error linking Google account for user {email}: {errorMessage}");
                    await transaction.RollbackAsync();
                    return new OperationResult<bool>
                    {
                        IsSuccess = false,
                        Data = false,
                        Message = "Error linking Google account",
                        Errors = addLoginResult.Errors.Select(e => e.Description).ToList()
                    };
                }

                await _signInManager.SignInAsync(user, isPersistent: false);
                _loggerHelper.LogMessage(LogLevelType.Information, $"User {email} successfully signed in after Google authentication");

                await transaction.CommitAsync();
                return new OperationResult<bool>
                {
                    IsSuccess = true,
                    Data = true,
                    Message = LOGIN_SUCCESSFUL_MESSAGE
                };
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                _loggerHelper.LogMessage(LogLevelType.Error, $"Transaction failed during Google authentication for user {email}: {ex.Message}");
                return new OperationResult<bool>
                {
                    IsSuccess = false,
                    Data = false,
                    Message = "An error occurred during Google authentication",
                    Errors = new List<string> { ex.Message }
                };
            }
        }
    }
}
