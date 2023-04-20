using System.ComponentModel.DataAnnotations;

namespace Portfol.io.Identity.DTO
{
    public class ConfirmChangePhoneDto
    {
        [Required]
        public string UserId { get; set; } = null!;

        [Required]
        [Phone]
        public string PhoneNumber { get; set; } = null!;

        [Required]
        public string Code { get; set; } = null!;
    }
}
