using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using TestIdentityCore.Domain;
using TestIdentityCore.Repository;

namespace TestIdentityCore.Api;

[Route("api/auth")]
[ApiController]
public class AuthController(
    UserManager<ApplicationUser> userManager,
    SignInManager<ApplicationUser> signInManager,
    IUserRepository userRepository,
    IConfiguration configuration) : ControllerBase
{
    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterRequest registerRequest)
    {
        var user = new ApplicationUser { UserName = registerRequest.Email, Email = registerRequest.Email };
        var result = await userManager.CreateAsync(user, registerRequest.Password);
        if (!result.Succeeded)
        {
            return BadRequest(result.Errors);
        }
        return Ok("User created");
    }
    
    
    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequest model)
    {
        // UserManager<TUser> in ASP.NET Core Identity does not use a traditional repository pattern.
        // Instead, it directly interacts with IUserStore<TUser> and IUserPasswordStore<TUser>,
        // which are implemented by Entity Framework Core's Identity package via UserStore<TUser>,
        // Thus no vulnerability here.
        var user = await userManager.FindByEmailAsync(model.Email);
        if (user == null) return Unauthorized("Invalid credentials");

        var result = await signInManager.CheckPasswordSignInAsync(user, model.Password, false);
        if (!result.Succeeded) return Unauthorized("Invalid credentials");
        
        var roles = await userManager.GetRolesAsync(user);
        var token = GenerateJwtToken(user, roles);
        return Ok(new { Token = token });
    }
    
    [HttpPost("login-unsafe")]
    public async Task<IActionResult> LoginUnsafe([FromBody] LoginRequest model)
    {
        var user = await userRepository.FindByEmailAsyncRawSqlUnsafe(model.Email);
        if (user == null) return Unauthorized("Invalid credentials");

        var result = await signInManager.CheckPasswordSignInAsync(user, model.Password, false);
        if (!result.Succeeded) return Unauthorized("Invalid credentials");
        
        var roles = await userManager.GetRolesAsync(user);
        var token = GenerateJwtToken(user, roles);
        return Ok(new { Token = token });
    }

    public class LoginRequest
    {
        public string Email { get; set; }
        public string Password { get; set; }
    }

    private string GenerateJwtToken(ApplicationUser user, ICollection<string> roles)
    {
        var jwtSettings = configuration.GetSection("JwtSettings");
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings["SecretKey"]));
        
        // Ensure the key length is sufficient
        if (key.Key.Length < 32)
        {
            Console.WriteLine($"Invalid JWT token {key.Key} for {jwtSettings["SecretKey"]}");
            throw new InvalidOperationException("Secret key must be at least 32 bytes.");
        }

        
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim(ClaimTypes.Email, user.Email!),
            new Claim(ClaimTypes.Role, "User")
        };
        claims.AddRange(roles.Select(role => new Claim(ClaimTypes.Role, role)));

        var token = new JwtSecurityToken(
            configuration["Issuer"],
            configuration["Audience"],
            claims,
            expires: DateTime.UtcNow.AddMinutes(30),
            signingCredentials: creds
        );
        
        return new JwtSecurityTokenHandler().WriteToken(token);
    }

}