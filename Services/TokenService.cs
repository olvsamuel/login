
using System.IdentityModel.Tokens.Jwt;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;

using login.Models;

public class TokenService
{
    private static byte[] _secretTKey = Encoding.UTF8.GetBytes(Settings.Secret.ToString());
    private static byte[] _secretRTKey = Encoding.UTF8.GetBytes(Settings.RTSecret.ToString());

    public static string GenerateToken(UserModel user)
    {
        int tokenDuration = 20;
        var tokenHandler = new JwtSecurityTokenHandler();

        var identityClaims = new ClaimsIdentity();
        identityClaims.AddClaim(new Claim(ClaimTypes.PrimarySid, user.Id.ToString()));
        identityClaims.AddClaim(new Claim(ClaimTypes.Name, user.Name.ToString()));
        identityClaims.AddClaim(new Claim(ClaimTypes.Email, user.Email.ToString().Trim()));


        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Issuer = Settings.Issuer,
            Audience = Settings.JWTAudience,
            TokenType = "jwt",
            Subject = identityClaims,
            Expires = DateTime.UtcNow.AddSeconds(tokenDuration),
            SigningCredentials = new SigningCredentials(
                new SymmetricSecurityKey(_secretTKey), 
                SecurityAlgorithms.HmacSha256Signature
            )
        };
        var token = tokenHandler.CreateToken(tokenDescriptor);
        return tokenHandler.WriteToken(token);
    }

    public static string GenerateTokenByClaims(IEnumerable<Claim> claims)
    {
        int tokenDuration = 20;
        var tokenHandler = new JwtSecurityTokenHandler();

        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(claims),
            Expires = DateTime.UtcNow.AddSeconds(tokenDuration),
            SigningCredentials = new SigningCredentials(
                new SymmetricSecurityKey(_secretTKey), 
                SecurityAlgorithms.HmacSha256Signature
            )
        };
        var token = tokenHandler.CreateToken(tokenDescriptor);
        return tokenHandler.WriteToken(token);
    }

    public static string GenerateRefreshToken(long IdUsuer)
    {
        int tokenDuration = 3600;

        var tokenHandler = new JwtSecurityTokenHandler();
        var identityClaims = new ClaimsIdentity();
        identityClaims.AddClaim(new Claim(ClaimTypes.PrimarySid, IdUsuer.ToString()));

        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Issuer = Settings.Issuer,
            Audience = Settings.JWTRAudience,
            TokenType = "rt+jwt",
            SigningCredentials = new SigningCredentials(
                new SymmetricSecurityKey(_secretRTKey),
                SecurityAlgorithms.HmacSha256Signature
            ),
            Subject = identityClaims,
            Expires = DateTime.UtcNow.AddSeconds(tokenDuration),
        };
        var token = tokenHandler.CreateToken(tokenDescriptor);
        return tokenHandler.WriteToken(token);
    }

    public static ClaimsPrincipal GetPrincipalFromExpiredToken(string token)
    {
        var tokenValidationParameters = new TokenValidationParameters
        {
            ValidateAudience = false,
            ValidateIssuer = false,
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(_secretTKey),
            ValidateLifetime = false
        };

        var tokenHandler = new JwtSecurityTokenHandler();
        var principal = tokenHandler.ValidateToken(
            token, 
            tokenValidationParameters, 
            out var securityToken
        );
        if (securityToken is not JwtSecurityToken jwtSecurityToken ||
            !jwtSecurityToken.Header.Alg.Equals(SecurityAlgorithms.HmacSha256,
                StringComparison.InvariantCultureIgnoreCase))
            throw new SecurityTokenException("Invalid token");

        return principal;
    }

    public static bool ValidateRToken(string token)
    {
        try
        {
            var handler = new JsonWebTokenHandler();

            var result = handler.ValidateToken(token, new TokenValidationParameters()
            {
                ValidateIssuer = true,
                ValidateAudience = false,
                ValidateIssuerSigningKey = true,
                ValidIssuer = Settings.Issuer,
                ValidAudience = Settings.JWTRAudience,
                IssuerSigningKey = new SymmetricSecurityKey(_secretRTKey),
                ClockSkew = TimeSpan.Zero
            });

            if (!result.IsValid)
                return false;

            return true;
        }
        catch (System.Exception e)
        {
            return false;
        }
    }

    public static bool ValidateJWTToken(string token)
    {
        try
        {
            var handler = new JsonWebTokenHandler();
            var result = handler.ValidateToken(token, new TokenValidationParameters()
            {
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateIssuerSigningKey = true,
                ValidIssuer = Settings.Issuer,
                ValidAudience = Settings.JWTAudience,
                IssuerSigningKey = new SymmetricSecurityKey(_secretTKey),
                ClockSkew = TimeSpan.Zero
            });

            if (!result.IsValid)
                return false;

            return true;
        }
        catch (System.Exception)
        {
            return false;
        }
    }

}