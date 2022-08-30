using Microsoft.AspNetCore.Identity;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace Portfol.io.Identity.Interfaces
{
    public interface ITokenManager
    {
        public JwtSecurityToken CreateAccessToken(IdentityUser user, string role);
        public string CreateRefreshToken();
        public ClaimsPrincipal? GetPrincipalFromExpiredToken(string? token);
    }
}
