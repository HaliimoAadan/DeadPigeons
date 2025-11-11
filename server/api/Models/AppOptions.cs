using System.ComponentModel.DataAnnotations;

namespace api;

public class AppOptions
{
    [Required] [MinLength(1)] public string Db { get; set; } = null!;
    [Required] [MinLength(1)] public string JwtSecret { get; set; } = null!;
}