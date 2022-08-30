using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Portfol.io.Identity.Models;

namespace Portfol.io.Identity.Data;

public class AppIdentityContext : IdentityDbContext<AppUser>
{
    public AppIdentityContext(DbContextOptions<AppIdentityContext> options)
        : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.Entity<IdentityRole>().HasData(
            new IdentityRole<string>
            {
                Name = "employee",
                NormalizedName = "EMPLOYEE",
                Id = Guid.NewGuid().ToString(),
                ConcurrencyStamp = Guid.NewGuid().ToString(),
            },
            new IdentityRole<string>
            {
                Name = "employer",
                NormalizedName = "EMPLOYER",
                Id = Guid.NewGuid().ToString(),
                ConcurrencyStamp = Guid.NewGuid().ToString(),
            },
            new IdentityRole<string>
            {
                Name = "support",
                NormalizedName = "SUPPORT",
                Id = Guid.NewGuid().ToString(),
                ConcurrencyStamp = Guid.NewGuid().ToString(),
            },
            new IdentityRole<string>
            {
                Name = "admin",
                NormalizedName = "ADMIN",
                Id = Guid.NewGuid().ToString(),
                ConcurrencyStamp = Guid.NewGuid().ToString(),
            });
    }
}
