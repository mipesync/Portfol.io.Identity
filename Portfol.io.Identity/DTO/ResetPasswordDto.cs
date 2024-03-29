﻿using System.ComponentModel.DataAnnotations;

namespace Portfol.io.Identity.DTO
{
    public class ResetPasswordDto
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; } = null!;
        [Required]
        public string Code { get; set; } = null!;
        [Required]
        [MinLength(8)]
        [DataType(DataType.Password)]
        public string Password { get; set; } = null!;
    }
}
