public class JwtResponse
{
    public string Token { get; set; }
    public JwtResponse(string token) => Token = token;
}