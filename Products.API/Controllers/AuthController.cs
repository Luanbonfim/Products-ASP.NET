using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using Products.Application.Identity.Interfaces;
using Products.Common.Helpers;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

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
            {
                var token = GenerateJwtToken(request.Username);

                if (string.IsNullOrEmpty(token))
                {
                    return Unauthorized("Invalid credentials.");
                }

                return Ok(new { Token = token });
            }

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

        private string GenerateJwtToken(string username)
        {
            var jwtSettings = _config.GetSection("Jwt");
            var key = Encoding.UTF8.GetBytes(jwtSettings["Secret"]);

            var claims = new List<Claim>
        {
            new Claim(JwtRegisteredClaimNames.Sub, username),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };

            var token = new JwtSecurityToken(
                issuer: jwtSettings["Issuer"],
                audience: jwtSettings["Audience"],
                claims: claims,
                expires: DateTime.UtcNow.AddHours(1),
                signingCredentials: new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256)
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
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


