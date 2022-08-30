using System.ComponentModel.DataAnnotations;

namespace Portfol.io.Identity.ViewModels
{
    public class RegisterViewModel
    {
        [Required]
        public string Username { get; set; } = null!;

        [Required]
        [EmailAddress]
        public string Email { get; set; } = null!;

        [Required]
        [MinLength(8)]
        [DataType(DataType.Password)]
        public string Password { get; set; } = null!;

        [Required]
        public string RoleId { get; set; } = null!;
        public string? ReturnUrl { get; set; }
    }
}
