using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Products.Application.DTOs;
using Products.Application.Interfaces;

namespace Products.Controllers
{
    [ApiController]
    [Route("api/auth")]
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
        public async Task<IActionResult> Login([FromBody] LoginDto request)
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

        [HttpGet("google-login")]
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



