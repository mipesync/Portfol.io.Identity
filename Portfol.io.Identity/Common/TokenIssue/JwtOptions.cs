using Microsoft.IdentityModel.Tokens;
using System.Text;

namespace Portfol.io.Identity.Common.TokenIssue
{
    public class JwtOptions
    {
        public static string ISSUER = "https://localhost:5000";
        public static string AUDIENCE = "PortfolioWebAPI";
        const string KEY = "375ff8a55aca72cf3a11e318d1592d2f0d3995ae";
        //TODO: ПОСМТОРЕТЬ
        public DateTime EXPIRES = DateTime.UtcNow.Add(TimeSpan.FromMinutes(30));
        public DateTime RefreshTokenExpires = DateTime.UtcNow.Add(TimeSpan.FromDays(14));
        public static SymmetricSecurityKey GetSymmetricSecurityKey() =>
            new SymmetricSecurityKey(Encoding.UTF8.GetBytes(KEY));
    }
}
