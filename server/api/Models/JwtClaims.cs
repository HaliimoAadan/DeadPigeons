namespace api;

public record JwtClaims(string Id)
{
    public string Id { get; set; } = Id;
}