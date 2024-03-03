using Microsoft.AspNetCore.Mvc;
using login.Models;
using login.DTOs;

namespace login.Controllers;

[ApiController]
[Route("api/[controller]")]
public class LoginController : ControllerBase
{
    private readonly ILogger<LoginController> _logger;
    private static UserModel[] _users;

    public LoginController(ILogger<LoginController> logger)
    {
        _logger = logger;

        _users = new[]
        {
            new UserModel { Id = 1, Email = "teste1@teste.com", Name = "Teste 1", Password = "123456" },
            new UserModel { Id = 2, Email = "teste2@teste.com", Name = "Teste 2", Password = "123456" }
            // Add more if you want...
        };
    }

    [HttpPost(Name = "Login")]
    public ActionResult<object> Post([FromBody] UserDTO user)
    {
        var UserFound = _users.FirstOrDefault(x => x.Email == user.Email && x.Password == user.Password);
        if (UserFound != null)
        {

            var Token = TokenService.GenerateToken(UserFound);
            var RefreshToken = TokenService.GenerateRefreshToken(UserFound.Id);

            return new { Token = Token, RefreshToken = RefreshToken };
        }

        return Unauthorized();
    }
}
