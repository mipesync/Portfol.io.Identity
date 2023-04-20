using System.ComponentModel.DataAnnotations;

namespace Portfol.io.Identity.DTO
{
    public class RefreshTokenDto
    {

        [Required]
        public string RefreshToken { get; set; } = null!;
    }
}
