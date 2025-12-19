using System.ComponentModel.DataAnnotations;

namespace api.Models.Requests;

public class RegisterRequestDto
{
    [Required]
    public string FirstName { get; set; } = null!;

    [Required]
    public string LastName { get; set; } = null!;

    [Required, EmailAddress]
    public string Email { get; set; } = null!;

    [Required, MinLength(8)]
    public string Password { get; set; } = null!;
    
    [Required, Phone]
    public string PhoneNumber { get; set; } = null!;
}