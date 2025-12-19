namespace api.Models;

public class PlayerCreateDto
{
    public string FullName { get; set; } = null!;
    public string PhoneNumber { get; set; } = null!;
    public string Email { get; set; } = null!;
    public bool? IsActive { get; set; } // optional
}

public class PlayerResponseDto
{
    public Guid PlayerId { get; set; }
    public string FullName { get; set; } = null!;
    public string PhoneNumber { get; set; } = null!;
    public string Email { get; set; } = null!;
    public bool IsActive { get; set; }
}

