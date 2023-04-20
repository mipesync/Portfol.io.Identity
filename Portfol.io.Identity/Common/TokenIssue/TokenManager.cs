using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;
using Portfol.io.Identity.Common.Enums;
using Portfol.io.Identity.Interfaces;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace Portfol.io.Identity.Common.TokenIssue
{
    public class TokenManager : ITokenManager
    {
        public Task<JwtSecurityToken> CreateAccessTokenAsync(IdentityUser user, Roles role)
        {
            var claims = new List<Claim> { new Claim(ClaimTypes.Name, user.UserName),
                new Claim(ClaimTypes.NameIdentifier, user.Id), new Claim(ClaimTypes.Role, role.ToString()) };

            var jwt = new JwtSecurityToken(
                issuer: JwtOptions.ISSUER,
                audience: JwtOptions.AUDIENCE,
                claims: claims,
                expires: new JwtOptions().EXPIRES,
                signingCredentials: new SigningCredentials(JwtOptions.GetSymmetricSecurityKey(), SecurityAlgorithms.HmacSha256));

            return Task.FromResult(jwt);
        }

        public Task<JwtSecurityToken> CreateRefreshTokenAsync(string userId)
        {
            var claims = new List<Claim> { new Claim(ClaimTypes.NameIdentifier, userId) };

            var jwt = new JwtSecurityToken(
                issuer: JwtOptions.ISSUER,
                audience: JwtOptions.AUDIENCE,
                claims: claims,
                expires: new JwtOptions().RefreshTokenExpires,
                signingCredentials: new SigningCredentials(JwtOptions.GetSymmetricSecurityKey(), SecurityAlgorithms.HmacSha256));

            return Task.FromResult(jwt);
        }

        public Task<ClaimsPrincipal>? GetPrincipalFromExpiredTokenAsync(string? token)
        {
            var tokenValidationParameters = new TokenValidationParameters
            {
                ValidateAudience = false,
                ValidateIssuer = false,
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = JwtOptions.GetSymmetricSecurityKey(),
                ValidateLifetime = false
            };

            var tokenHandler = new JwtSecurityTokenHandler();
            var principal = tokenHandler.ValidateToken(token, tokenValidationParameters, out SecurityToken securityToken);
            if (securityToken is not JwtSecurityToken jwtSecurityToken || !jwtSecurityToken.Header.Alg.Equals(SecurityAlgorithms.HmacSha256, StringComparison.InvariantCultureIgnoreCase))
                throw new SecurityTokenException("Invalid token");

            return Task.FromResult(principal);
        }
    }
}
