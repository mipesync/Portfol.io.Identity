using Microsoft.IdentityModel.Tokens;
using System.Text;

namespace Portfol.io.Identity.Common.TokenIssue
{
    public static class JwtOptions
    {
        public static string ISSUER = "https://localhost:7150";
        public static string AUDIENCE = "PortfolioWebAPI";
        const string KEY = "375ff8a55aca72cf3a11e318d1592d2f0d3995ae";
        public static DateTime EXPIRES = DateTime.UtcNow.Add(TimeSpan.FromMinutes(30));
        public static DateTime RefreshTokenExpires = DateTime.UtcNow.Add(TimeSpan.FromDays(14));
        public static SymmetricSecurityKey GetSymmetricSecurityKey() =>
            new SymmetricSecurityKey(Encoding.UTF8.GetBytes(KEY));
    }
}
