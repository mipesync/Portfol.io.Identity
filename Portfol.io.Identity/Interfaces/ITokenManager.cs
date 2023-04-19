using Microsoft.AspNetCore.Identity;
using Portfol.io.Identity.Common.Enums;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace Portfol.io.Identity.Interfaces
{
    public interface ITokenManager
    {
        public Task<JwtSecurityToken> CreateAccessTokenAsync(IdentityUser user, Roles role);
        public Task<JwtSecurityToken> CreateRefreshTokenAsync(string userId);
        public Task<ClaimsPrincipal>? GetPrincipalFromExpiredTokenAsync(string? token);
    }
}
