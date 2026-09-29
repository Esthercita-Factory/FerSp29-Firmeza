using System.ComponentModel.DataAnnotations;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;

namespace Firmeza.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class AuthController : ControllerBase
{
    private readonly UserManager<IdentityUser> _userManager;
    private readonly RoleManager<IdentityRole> _roleManager;
    private readonly IConfiguration _config;

    public AuthController(UserManager<IdentityUser> userManager, RoleManager<IdentityRole> roleManager, IConfiguration config)
    {
        _userManager = userManager;
        _roleManager = roleManager;
        _config = config;
    }

    private async Task EnsureRolesExistAsync()
    {
        if (!await _roleManager.RoleExistsAsync("Admin"))
        {
            await _roleManager.CreateAsync(new IdentityRole("Admin"));
        }
        if (!await _roleManager.RoleExistsAsync("Cliente"))
        {
            await _roleManager.CreateAsync(new IdentityRole("Cliente"));
        }
    }

    /// <summary>
    /// Registra un nuevo usuario en el sistema asignándole un rol (Admin o Cliente).
    /// </summary>
    [HttpPost("register")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Register([FromBody] RegisterRequest request)
    {
        await EnsureRolesExistAsync();

        var user = new IdentityUser { UserName = request.Email, Email = request.Email };
        var result = await _userManager.CreateAsync(user, request.Password);

        if (!result.Succeeded)
            return BadRequest(new { message = "Error al registrar usuario", errors = result.Errors });

        var assignedRole = string.Equals(request.Rol, "Admin", StringComparison.OrdinalIgnoreCase) ? "Admin" : "Cliente";
        await _userManager.AddToRoleAsync(user, assignedRole);

        return Ok(new { message = "Usuario registrado exitosamente", role = assignedRole });
    }

    /// <summary>
    /// Inicia sesión con credenciales válidas y retorna un token JWT con los claims del rol del usuario.
    /// </summary>
    [HttpPost("login")]
    [ProducesResponseType(typeof(AuthResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
        await EnsureRolesExistAsync();

        var user = await _userManager.FindByEmailAsync(request.Email);
        if (user == null || !await _userManager.CheckPasswordAsync(user, request.Password))
            return Unauthorized(new { message = "Credenciales incorrectas" });

        var roles = await _userManager.GetRolesAsync(user);
        string userRole;
        if (!roles.Any())
        {
            userRole = (user.Email?.ToLower().Contains("admin") == true) ? "Admin" : "Cliente";
            await _userManager.AddToRoleAsync(user, userRole);
        }
        else
        {
            userRole = roles.First();
        }

        var tokenHandler = new JwtSecurityTokenHandler();
        var key = Encoding.UTF8.GetBytes(_config["Jwt:Key"]!);
        var durationInHours = Convert.ToDouble(_config["Jwt:DurationInHours"] ?? "4");
        var expiresAt = DateTime.UtcNow.AddHours(durationInHours);

        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id),
            new Claim(ClaimTypes.Email, user.Email!),
            new Claim(ClaimTypes.Role, userRole),
            new Claim(JwtRegisteredClaimNames.Sub, user.Id),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };

        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(claims),
            Expires = expiresAt,
            Issuer = _config["Jwt:Issuer"] ?? "FirmezaAPI",
            Audience = _config["Jwt:Audience"] ?? "FirmezaAPI",
            SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
        };

        var token = tokenHandler.CreateToken(tokenDescriptor);
        return Ok(new AuthResponse(tokenHandler.WriteToken(token), expiresAt, userRole, user.Email!));
    }
}

public record RegisterRequest(
    [Required(ErrorMessage = "El correo electrónico es obligatorio")]
    [EmailAddress(ErrorMessage = "Formato de correo electrónico inválido")]
    string Email,

    [Required(ErrorMessage = "La contraseña es obligatoria")]
    [MinLength(6, ErrorMessage = "La contraseña debe contener al menos 6 caracteres")]
    string Password,

    string? Rol = "Cliente"
);

public record LoginRequest(
    [Required(ErrorMessage = "El correo electrónico es obligatorio")]
    [EmailAddress(ErrorMessage = "Formato de correo electrónico inválido")]
    string Email,

    [Required(ErrorMessage = "La contraseña es obligatoria")]
    string Password
);

public record AuthResponse(string Token, DateTime Expiration, string Rol, string Email);