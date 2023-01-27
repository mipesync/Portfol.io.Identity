using Microsoft.AspNetCore.Identity;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace Portfol.io.Identity.Interfaces
{
    public interface ITokenManager
    {
        public Task<JwtSecurityToken> CreateAccessTokenAsync(IdentityUser user, string role);
        public Task<JwtSecurityToken> CreateRefreshTokenAsync(string userId);
        public Task<ClaimsPrincipal>? GetPrincipalFromExpiredTokenAsync(string? token);
    }
}
