using System.ComponentModel.DataAnnotations;

namespace Portfol.io.Identity.DTO
{
    public class LoginDto
    {
        [Required]
        public string Username { get; set; } = null!;

        [Required]
        [DataType(DataType.Password)]
        [MinLength(8)]
        public string Password { get; set; } = null!;

        public bool RememberMe { get; set; }
        public string? ReturnUrl { get; set; }
    }
}
