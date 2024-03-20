namespace login.DTOs;

public class RefreshTokenDTO
{
    public RefreshTokenDTO(string refreshToken)
    {
        if (string.IsNullOrEmpty(refreshToken))
        {
            throw new ArgumentException("Refresh token is required");
        }

        RefreshToken = refreshToken;
    }
    public string RefreshToken { get; set; }
}