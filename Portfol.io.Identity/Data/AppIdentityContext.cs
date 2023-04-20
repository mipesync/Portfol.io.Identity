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
    }
}
