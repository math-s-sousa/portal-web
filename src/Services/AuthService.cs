using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using portal_web.Database;
using System.Data;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

public class AuthService
{
    private readonly LocalMarketContext _context;
    private readonly IConfiguration _configuration;

    public AuthService(LocalMarketContext context, IConfiguration configuration)
    {
        _context = context;
        _configuration = configuration;
    }

    public async Task<string> Register(UserRegisterModel model)
    {
        if (await _context.Usuarios.AnyAsync(u => u.Username == model.Username))
        {
            throw new Exception("Usuário já existe");
        }

        var novoUsuario = new Usuario
        {
            Username = model.Username,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(model.Password),
            Role = "user"
        };

        _context.Usuarios.Add(novoUsuario);
        await _context.SaveChangesAsync();

        return "Usuário registrado com sucesso";
    }

    public async Task<string> Authenticate(UserLoginModel model)
    {
        var user = await _context.Usuarios.FirstOrDefaultAsync(u => u.Username == model.Username);
        if (user == null || !BCrypt.Net.BCrypt.Verify(model.Password, user.PasswordHash))
        {
            throw new Exception("Não autorizado");
        }

        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, model.Username),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            new Claim(ClaimTypes.Role, user.Role) // Inclui a role do usuário no JWT
        };

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: _configuration["Jwt:Issuer"],
            claims: claims,
            expires: DateTime.Now.AddMinutes(30),
            signingCredentials: creds);

        return new JwtSecurityTokenHandler().WriteToken(token);
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