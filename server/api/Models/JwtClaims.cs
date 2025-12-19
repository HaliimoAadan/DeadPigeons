public class JwtClaims
{
    public Guid Id { get; set; }
    public string? Role { get; set; } = default!;
    public string Email { get; set; } = default!;
}