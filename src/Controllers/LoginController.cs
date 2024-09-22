using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace portal_web.Controllers
{
    public class LoginController : Controller
    {
        private readonly AuthService _authService;

        public LoginController(AuthService authService)
        {
            _authService = authService;
        }

        public IActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Index(Dictionary<string, string> login)
        {
            try
            {
                string? user, pass;

                user = login.GetValueOrDefault("username");
                pass = login.GetValueOrDefault("password");

                if (user == null || pass == null)
                    throw new Exception("Usuário ou Senha vazios.");
                
                string token = _authService.Authenticate(new() 
                { Username = user, Password = pass }).Result;

                // Configura o cookie com o token JWT
                Response.Cookies.Append("jwt", token, new CookieOptions
                {
                    HttpOnly = true,  // Impede que o JS acesse o cookie
                    Secure = true,    // Apenas em conexões HTTPS
                    SameSite = SameSiteMode.Strict,
                    Expires = DateTime.UtcNow.AddMinutes(30)
                });

                return Ok();
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
    }
}
