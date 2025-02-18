using Microsoft.AspNetCore.Mvc;
using Products.Application.Interfaces;
using Products.Common.Helpers;

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
        public async Task<IActionResult> Login([FromBody] LoginModel request)
        {
            if (request == null)
            {
                return BadRequest("Invalid login data.");
            }

            var result = await _identityService.LoginAsync(request.Username, request.Password);

            if (result.IsSuccess)
                return Ok();

            return Unauthorized("Invalid credentials.");
        }

        [HttpPost("createuser")]
        //[Authorize]
        public async Task<IActionResult> CreateUser([FromBody] CreateUserRequest request)
        {
            if (request == null)
            {
                return BadRequest("Invalid user data.");
            }

            var result = await _identityService.CreateUserAsync(request.UserName, request.Password, request.Role);

            if (result.IsSuccess)
            {
                _loggerHelper.LogMessage(LogLevelType.Information, $"User {request.UserName} created successfully with role {request.Role}");
                return StatusCode(201);
            }

            _loggerHelper.LogMessage(LogLevelType.Error, $"Error creating user {request.UserName}: {string.Join(", ", result.Errors)}");
            return BadRequest(result.Errors);
        }

        public class LoginModel
        {
            public string Username { get; set; }
            public string Password { get; set; }
        }

        // DTO to hold request data for creating a user
        public class CreateUserRequest
        {
            public string UserName { get; set; }
            public string Role { get; set; }
            public string Password { get; set; }
        }
    }
}



