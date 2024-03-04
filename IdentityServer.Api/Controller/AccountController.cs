using IdentityServer.Api.Models;
using IdentityServer.Api.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace IdentityServer.Api.Controller
{
    [Route("api/[controller]")]
    [ApiController]
    public class AccountController : ControllerBase
    {
        private readonly UserManager<IdentityUser> _userManager;
        private readonly ILogger<AccountController> _logger;
        private readonly TokenService _tokenService;

        public AccountController(UserManager<IdentityUser> userManager
            , ILogger<AccountController> logger
            , TokenService tokenService)
        {
            _userManager = userManager;
            _logger = logger;
            _tokenService = tokenService;
        }

        // POST: api/Account/register
        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterModel model)
        {
            _logger.LogInformation("Register işlemi başladı. Kullanıcı adı: {UserName}", model.UserName);

            var userExists = await _userManager.FindByNameAsync(model.UserName);
            if (userExists != null)
            {
                _logger.LogWarning("Register işlemi başarısız. Kullanıcı adı zaten var: {UserName}", model.UserName);
                return StatusCode(StatusCodes.Status500InternalServerError, new { Status = "Error", Message = "User already exists!" });
            }

            IdentityUser user = new()
            {
                Email = model.Email,
                SecurityStamp = Guid.NewGuid().ToString(),
                UserName = model.UserName
            };

            var result = await _userManager.CreateAsync(user, model.Password);
            if (!result.Succeeded)
            {
                _logger.LogWarning("Register işlemi başarısız {UserName}. Errors: {Errors}", model.UserName, string.Join(", ", result.Errors.Select(e => e.Description)));

                return StatusCode(StatusCodes.Status500InternalServerError, new { Status = "Error", Message = "User creation failed! Please check user details and try again." });
            }

            _logger.LogInformation("Kullanıcı başarıyla oluşturuldu: {UserName}", model.UserName);
            return Ok(new { Status = "Success", Message = "User created successfully!" });
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginModel model)
        {
            var user = await _userManager.FindByNameAsync(model.UserName);
            if (user != null && await _userManager.CheckPasswordAsync(user, model.Password))
            {
                // Burada JWT token oluşturun
                var token = _tokenService.GenerateJwtToken(user);

                _logger.LogInformation("User {UserName} logged in at {Time}.", model.UserName, DateTime.UtcNow);
                return Ok(token);
            }
            else
            {
                _logger.LogWarning("Login failed for user {UserName}.", model.UserName);
                return Unauthorized(new { Message = "Login failed. Please check your username and password." });
            }
        }

    }
}
