namespace api;

public record JwtResponse(string Token)
{
    public string Token { get; set; } = Token;
}