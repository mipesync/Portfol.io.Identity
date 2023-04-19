using Microsoft.AspNetCore.Identity;
using Portfol.io.Identity.Common.Enums;

namespace Portfol.io.Identity.Models;

// Add profile data for application users by adding properties to the AppUser class
public class AppUser : IdentityUser
{
    public string? FullName { get; set; }
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public string? MiddleName { get; set; }
    public string? Description { get; set; }
    public string? ProfileImagePath { get; set; }
    public DateTime DateOfBirth { get; set; }
    public DateTime DateOfCreation { get; set; }
    public string? RefreshToken { get; set; }
    public DateTime RefreshTokenExpiryTime { get; set; }
    public Roles Role { get; set; }
}

