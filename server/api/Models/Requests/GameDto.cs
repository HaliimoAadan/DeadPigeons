using System;
using System.ComponentModel.DataAnnotations;

namespace api.Models.Requests
{
    public class CreateGameDto
    {
        [Required]
        public DateTime ExpirationDate { get; set; }
    }
}