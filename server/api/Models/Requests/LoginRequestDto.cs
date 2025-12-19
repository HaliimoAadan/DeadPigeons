using System.ComponentModel.DataAnnotations;

namespace api.Models.Requests;

public class LoginRequestDto
{
    [Required, EmailAddress]
    public string Email { get; set; } = null!;

    [Required, MinLength(8)]
    public string Password { get; set; } = null!;
}