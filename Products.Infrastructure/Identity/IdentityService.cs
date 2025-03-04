﻿using Microsoft.AspNetCore.Http;
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
            return new OperationResult("User not found", false);
        }

        public async Task<OperationResult> LoginAsync(string email, string password)
        {
            var user = await _userManager.FindByNameAsync(email);

            if (user == null)
            {
                return new OperationResult
                {
                    IsSuccess = false,
                    Message = "User not found"
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
                Message = "Login successful"
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
    }
}
