using Microsoft.AspNetCore.Mvc;
using login.Models;
using login.DTOs;
using login.Database;

namespace login.Controllers;

[ApiController]
[Route("api/[controller]")]
public class LoginController : ControllerBase
{
    private readonly ILogger<LoginController> _logger;
    private static readonly UserModel[] _users = [
        new UserModel(1, "teste1@teste.com", "Teste 1", "aB1@123"),
        new UserModel(2, "teste2@teste.com", "Teste 2", "aB1@123"),
        // Add more if you want...
    ];

    public LoginController(ILogger<LoginController> logger)
    {
        _logger = logger;
    }

    [HttpPost(Name = "Login")]
    public ActionResult<object> Login([FromBody] UserDTO user)
    {
        UserModel? UserFound = _users.FirstOrDefault(x => x.Email == user.Email && x.Password == user.Password);
        if (UserFound != null)
        {
            string Token = TokenService.GenerateToken(UserFound);
            string RefreshToken = TokenService.GenerateRefreshToken(UserFound.Id);

            Tokens.NewSession(UserFound.Email, Token, RefreshToken);

            return new { Token, RefreshToken };
        }

        return Unauthorized();
    }

    [HttpPost("RefreshToken", Name = "RefreshToken")]
    public async Task<ActionResult<object>> RefreshToken(
        [FromBody] RefreshTokenDTO refreshTokenDTO,
        [FromHeader] string? reactivateSession = null
        )
    {
        string jwt = Request.Headers["Authorization"].ToString().Replace("Bearer ", string.Empty);

        // Check if refresh token is valid - if not, return unauthorized
        if (!await TokenService.ValidateRToken(refreshTokenDTO.RefreshToken))
            return Unauthorized("Refresh token is not valid");

        // If reactivateSession is not null, re-activate the session using the refresh token
        // even if the jwt is expired but the refresh token is still valid
        // Check if JWT token is valid - if not, return bad request
        if (reactivateSession == null && !await TokenService.ValidateJWTToken(jwt))
            return BadRequest("JWT token is not valid");

        // Search for session in database based on refresh token
        List<TokenData> session = Tokens.GetTokens(null, 0, null, refreshTokenDTO.RefreshToken);
        if (session == null)
            return BadRequest("Session not found");

        ClaimsPrincipal principal = TokenService.GetPrincipalFromExpiredToken(jwt);
        string NewToken = TokenService.GenerateTokenByClaims(principal.Claims);

        Tokens.NewSession(session[0].Email, NewToken, refreshTokenDTO.RefreshToken, jwt);
        
        return new { Token = NewToken, refreshTokenDTO.RefreshToken };
    }

    [HttpGet(Name = "GetTokens")]
    public ActionResult<object> GetTokens() => Tokens.GetTokens();

    [HttpGet("TesteRefreshToken", Name = "TesteRefreshToken")]
    public async Task<ActionResult<bool>> TesteRefreshToken([FromHeader] string refreshToken) => await TokenService.ValidateRToken(refreshToken);
}
