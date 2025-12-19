using System.ComponentModel.DataAnnotations;

namespace api.Models.Requests
{
    public class WinningNumbersDto
    {
        [Required]
        [MinLength(3), MaxLength(3, ErrorMessage = "You must select exactly 3 numbers.")]
        public int[] Numbers { get; set; } = null!;
    }
}