using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Products.Application.Interfaces;
using Products.Common.Helpers;
using Products.Domain.Entities;

namespace Products.Controllers
{
    [ApiController]
    [Route("api/auth")]
    public class AuthController : ControllerBase
    {
        private readonly IConfiguration _config;
        private readonly IIdentityService _identityService;
        private readonly LoggerHelper _loggerHelper;
        public AuthController(IConfiguration config, IIdentityService identityService, ILogger<AuthController> logger, LoggerHelper loggerHelper)
        {
            _config = config;
            _identityService = identityService;
            _loggerHelper = loggerHelper;
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] User request)
        {
            if (request == null)
            {
                return BadRequest("Invalid login data.");
            }

            var result = await _identityService.LoginAsync(request.Email, request.Password);

            if (result.IsSuccess)
                return Ok();

            return Unauthorized("Invalid credentials.");
        }

        [HttpPost("createuser")]
        //[Authorize]
        public async Task<IActionResult> CreateUser([FromBody] User user)
        {
            if (user == null)
            {
                return BadRequest("Invalid user data.");
            }

            var passwordHasher = new PasswordHasher<User>();
            string hashedPassword = passwordHasher.HashPassword(user, user.Password);

            var result = await _identityService.CreateUserAsync(user.Email, hashedPassword, "Admin");

            if (result.IsSuccess)
            {
                _loggerHelper.LogMessage(LogLevelType.Information, $"User {user.Email} created successfully with role Admin");
                return StatusCode(201);
            }

            _loggerHelper.LogMessage(LogLevelType.Error, $"Error creating user {user.Email}: {string.Join(", ", result.Errors)}");
            return BadRequest(result.Errors);
        }

        [HttpPost("logout")]
        public async Task<IActionResult> Logout()
        {
            var result = await _identityService.LogOut();

            if (result.IsSuccess)
                return Ok(new { Message = "Logged out successfully" });
            else
                return BadRequest("Internal Server Error");
        }

        [HttpGet("checkauth")]
        public async Task<IActionResult> CheckAuth()
        {
            var IsSignedIn = await _identityService.IsSignedIn(HttpContext.User);

            if (IsSignedIn)
            {
                var userName = HttpContext.User.Identity.Name;
                return Ok(new { message = "User is authenticated", userName });
            }
            else
            {
                return Unauthorized(new { message = "User is not authenticated" });
            }
        }
    }
}



