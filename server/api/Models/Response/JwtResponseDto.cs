namespace api.Models.Response;

public class JwtResponseDto
{
    public string Token { get; set; }

    public JwtResponseDto(string token)
    {
        Token = token;
    }
}