﻿using Asp.Versioning;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using Products.Application.DTOs;
using Products.Application.Interfaces;

namespace Products.Controllers
{
    [ApiController]
    [ApiVersion("1.0")]
    [Route("api/v{version:apiVersion}/auth")]
    public class AuthController : ControllerBase
    {
        private readonly IConfiguration _config;
        private readonly IIdentityService _identityService;

        public AuthController(IConfiguration config, IIdentityService identityService)
        {
            _config = config;
            _identityService = identityService;
        }

        [HttpPost("login")]
        [MapToApiVersion("1.0")]
        public async Task<IActionResult> Login([FromBody] LoginDto request)
        {
            if (request == null)
            {
                return BadRequest("Invalid login data.");
            }

            var result = await _identityService.LoginAsync(request.Email, request.Password);

            if (result.IsSuccess)
                return Ok(result.Data);

            return Unauthorized(result.Message);
        }

        [HttpPost("createuser")]
        [MapToApiVersion("1.0")]
        public async Task<IActionResult> CreateUser([FromBody] UserDto user)
        {
            if (user == null)
            {
                return BadRequest("Invalid user data.");
            }

            var result = await _identityService.CreateUserAsync(user, "Admin");

            if (result.IsSuccess)
            {
                return StatusCode(201);
            }

            return BadRequest(result.Errors);
        }

        [HttpPost("logout")]
        [MapToApiVersion("1.0")]
        public async Task<IActionResult> Logout()
        {
            var result = await _identityService.LogOut();

            if (result.IsSuccess)
                return Ok(new { Message = "Logged out successfully" });
            else
                return BadRequest("Internal Server Error");
        }

        [HttpGet("checkauth")]
        [MapToApiVersion("1.0")]
        public async Task<IActionResult> CheckAuth()
        {
            var result = await _identityService.IsSignedIn(HttpContext.User);

            if (result.IsSuccess)
            {
                return Ok(result.Data);
            }

            return Unauthorized(result.Message);
        }

        [HttpGet("google-login")]
        [MapToApiVersion("1.0")]
        public async Task<IActionResult> GoogleLogin([FromQuery] string redirectUrl)
        {
            try
            {
                var redirectActionEndpoint = Url.Action(nameof(GoogleResponse), "auth", new { redirectUrl });

                var properties = await _identityService.GetGoogleLoginProperties(redirectActionEndpoint);

                return new ChallengeResult("Google", (AuthenticationProperties)properties.Data);
            }
            catch (Exception)
            {
                return BadRequest("Internal Server Error");
            }
        }

        [HttpGet("google-response")]
        [MapToApiVersion("1.0")]
        public async Task<IActionResult> GoogleResponse([FromQuery] string redirectUrl)
        {   
            var result = await _identityService.GetGoogleResponse();
            
            if (result.IsSuccess)
            {
                var defaultRedirectUrl = _config.GetValue<string>("Auth:DefaultRedirectUrl");
                return Redirect(redirectUrl ?? defaultRedirectUrl);
            }
            else
            {
                return BadRequest("Error creating user.");
            }
        }
    }
}



