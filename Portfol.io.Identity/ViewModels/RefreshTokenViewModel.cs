using System.ComponentModel.DataAnnotations;

namespace Portfol.io.Identity.ViewModels
{
    public class RefreshTokenViewModel
    {
        [Required]
        public string AccessToken { get; set; } = null!;

        [Required]
        public string RefreshToken { get; set; } = null!;
    }
}
