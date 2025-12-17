using System.ComponentModel.DataAnnotations;

namespace api.Models.Requests;

public class RegisterPlayerRequestDto
{
    [Required]
    [MinLength(2)]
    public string FirstName { get; set; } = null!;

    [Required]
    [MinLength(2)]
    public string LastName { get; set; } = null!;

    [Required]
    [EmailAddress]
    public string Email { get; set; } = null!;

    [Required]
    public string PhoneNumber { get; set; } = null!;

    [Required]
    [MinLength(8)]
    public string Password { get; set; } = null!;
}