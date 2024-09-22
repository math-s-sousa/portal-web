using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using portal_web.Database;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly LocalMarketContext _context;
    private readonly IConfiguration _configuration;

    public AuthController(LocalMarketContext context, IConfiguration configuration)
    {
        _context = context;
        _configuration = configuration;
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register(UserRegisterModel model)
    {
        if (await _context.Usuarios.AnyAsync(u => u.Username == model.Username))
        {
            return BadRequest("Usuário já existe");
        }

        var novoUsuario = new Usuario
        {
            Username = model.Username,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(model.Password),
            Role = "user"
        };

        _context.Usuarios.Add(novoUsuario);
        await _context.SaveChangesAsync();

        return Ok("Usuário registrado com sucesso");
    }

    [HttpPost("authenticate")]
    public async Task<IActionResult> Authenticate(UserLoginModel model)
    {
        var user = await _context.Usuarios.FirstOrDefaultAsync(u => u.Username == model.Username);
        if (user == null || !BCrypt.Net.BCrypt.Verify(model.Password, user.PasswordHash))
        {
            return Unauthorized();
        }

        var tokenHandler = new JwtSecurityTokenHandler();
        var key = Encoding.ASCII.GetBytes(_configuration["Jwt:Key"]);
        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(new[]
            {
                new Claim(ClaimTypes.Name, user.Username),
                new Claim(ClaimTypes.Role, user.Role)
            }),
            Expires = DateTime.UtcNow.AddHours(1),
            SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
        };

        var token = tokenHandler.CreateToken(tokenDescriptor);
        var tokenString = tokenHandler.WriteToken(token);

        return Ok(new { Token = tokenString });
    }
}

public class UserRegisterModel
{
    public string Username { get; set; }
    public string Password { get; set; }
}

public class UserLoginModel
{
    public string Username { get; set; }
    public string Password { get; set; }
}