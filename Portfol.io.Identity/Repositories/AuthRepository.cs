using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.WebUtilities;
using Portfol.io.Identity.Common.Exceptions;
using Portfol.io.Identity.Common.TokenIssue;
using Portfol.io.Identity.Controllers;
using Portfol.io.Identity.Interfaces;
using Portfol.io.Identity.Models;
using Portfol.io.Identity.ViewModels;
using Portfol.io.Identity.ViewModels.ResponseModels.AuthResponseModels;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Text.Encodings.Web;

namespace Portfol.io.Identity.Repositories
{
    public class AuthRepository : IAuthRepository
    {
        private readonly UserManager<AppUser> _userManager;
        private readonly SignInManager<AppUser> _signInManager;
        private readonly ILogger<AuthController> _logger;
        private readonly IEmailSender _emailSender;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly ITokenManager _tokenManager;

        public AuthRepository(SignInManager<AppUser> signInManager, 
            ILogger<AuthController> logger, UserManager<AppUser> userManager, 
            IEmailSender emailSender, RoleManager<IdentityRole> roleManager, 
            ITokenManager tokenManager)
        {
            _signInManager = signInManager;
            _logger = logger;
            _userManager = userManager;
            _emailSender = emailSender;
            _roleManager = roleManager;
            _tokenManager = tokenManager;
        }
        public async Task<ConfirmEmailResponse> ConfirmEmail(string userId, string code, string? returnUrl)
        {
            if (userId == null || code == null)
            {
                throw new BadRequestException("Отсутствуют входные данные");
            }

            var user = await _userManager.FindByIdAsync(userId);

            if (user == null)
            {
                throw new NotFoundException("Пользователь не найден");
            }

            code = Encoding.UTF8.GetString(WebEncoders.Base64UrlDecode(code));

            var result = await _userManager.ConfirmEmailAsync(user, code);

            if (result.Succeeded)
            {
                return new ConfirmEmailResponse
                {
                    message = "Почта была подтверждена",
                    returnUrl = returnUrl
                };
            }
            else throw new WentWrongException("Ошибка подтверждения почты");
        }

        public async Task ForgotPassword(string email, string hostUrl)
        {
            var user = await _userManager.FindByEmailAsync(email);

            if (user == null || !(await _userManager.IsEmailConfirmedAsync(user)))
            {
                throw new BadRequestException("Пользователь не существует или его почта не подтверждена");
            }

            var code = await _userManager.GeneratePasswordResetTokenAsync(user);
            code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(code));
            var callbackUrl = $"{hostUrl}/resetPassword?email={email}&code={code}";

            await _emailSender.SendEmailAsync(
                email,
                "Сбросить пароль",
                $"Для сброса пароля <a href='{HtmlEncoder.Default.Encode(callbackUrl!)}'>нажмите сюда</a>.");
        }

        public async Task<LoginResponse> Login(LoginViewModel model)
        {
            var user = await _userManager.FindByNameAsync(model.Username);

            if (user is null || user.UserName != model.Username)
            {
                throw new NotFoundException("Пользователь не найден");
            }
            if (!user.EmailConfirmed)
            {
                _logger.LogWarning("Email not confimed.");
                throw new ForbiddenException("Email не подтверждён");
            }

            var result = await _signInManager.PasswordSignInAsync(user, model.Password, model.RememberMe, false);

            if (result.Succeeded)
            {
                _logger.LogInformation("User logged in.");

                JwtSecurityToken accessToken = await _tokenManager.CreateAccessTokenAsync(user, user.Role);

                JwtSecurityToken? refreshToken = null;

                user.LockoutEnd = null;
                user.LockoutEnabled = false;

                if (model.RememberMe)
                {
                    refreshToken = await _tokenManager.CreateRefreshTokenAsync(user.Id.ToString());

                    user.RefreshToken = new JwtSecurityTokenHandler().WriteToken(refreshToken);
                    user.RefreshTokenExpiryTime = new JwtOptions().RefreshTokenExpires;
                }

                await _userManager.UpdateAsync(user);

                return new LoginResponse
                {
                    access_token = new JwtSecurityTokenHandler().WriteToken(accessToken),
                    expires = accessToken.ValidTo,
                    refresh_token = refreshToken is null 
                        ? null 
                        : new JwtSecurityTokenHandler().WriteToken(refreshToken),
                    refresh_token_expires = refreshToken is null 
                        ? null 
                        : refreshToken!.ValidTo,
                    returnUrl = model.ReturnUrl
                };
            }
            if (result.IsLockedOut)
            {
                _logger.LogWarning("User account locked out.");

                user.AccessFailedCount = 0;
                await _userManager.UpdateAsync(user);

                throw new ForbiddenException("Аккаунт заблокирован");
            }
            else
            {
                user.AccessFailedCount++;

                if (user.AccessFailedCount >= 5)
                {
                    user.LockoutEnabled = true;
                    user.LockoutEnd = DateTime.UtcNow + TimeSpan.FromMinutes(10);
                }

                await _userManager.UpdateAsync(user);

                throw new BadRequestException("Неудачная попытка входа");
            }
        }

        public async Task<RegisterResponse> Register(RegisterViewModel model)
        {
            var user = new AppUser { 
                UserName = model.Username, 
                Email = model.Email, 
                DateOfCreation = DateTime.UtcNow,
                Role = model.Role
            };

            var getUser = await _userManager.FindByEmailAsync(model.Email);

            if (getUser is not null)
                throw new BadRequestException("Пользователь с таким email уже существует");

            var result = await _userManager.CreateAsync(user, model.Password);

            if (result.Succeeded)
            {
                _logger.LogInformation("User created a new account with password.");

                var userId = await _userManager.GetUserIdAsync(user);
                var code = await _userManager.GenerateEmailConfirmationTokenAsync(user);
                code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(code));
                var callbackUrl = $"{model.HostUrl}/confirmEmail?userId={userId}" +
                    $"&code={code}&returnUrl={model.ReturnUrl}";

                await _emailSender.SendEmailAsync(model.Email, "Подтвердите свою почту",
                    $"Для подтверждения аккаунта <a href='" +
                    $"{HtmlEncoder.Default.Encode(callbackUrl!)}'>нажмите сюда</a>.");

                if (!_userManager.Options.SignIn.RequireConfirmedAccount)
                {
                    await _signInManager.SignInAsync(user, isPersistent: false);
                    var accessToken = await _tokenManager.CreateAccessTokenAsync(user, model.Role);
                    return new RegisterResponse
                    {
                        userId = userId,
                        returnUrl = model.ReturnUrl!,
                        access_token = new JwtSecurityTokenHandler().WriteToken(accessToken),
                        expires = accessToken.ValidTo
                    };
                }
                return new RegisterResponse 
                {
                    userId = userId, 
                    returnUrl = model.ReturnUrl! 
                };
            }
            foreach (var error in result.Errors)
            {
                throw new BadRequestException(error.Description);
            }

            throw new WentWrongException();
        }

        public async Task ResendEmailConfirmation(string email, string? returnUrl, string hostUrl)
        {
            var user = await _userManager.FindByEmailAsync(email);

            if (user == null)
            {
                throw new NotFoundException("Пользователь не найден");
            }

            var userId = await _userManager.GetUserIdAsync(user);
            var code = await _userManager.GenerateEmailConfirmationTokenAsync(user);
            code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(code));
            var callbackUrl = $"{hostUrl}/confirmEmail?userId={userId}&code={code}&returnUrl={returnUrl}";

            await _emailSender.SendEmailAsync(email, "Подтвердите свою почту",
                $"Для подтверждения аккаунта <a href='{HtmlEncoder.Default.Encode(callbackUrl!)}'>нажмите сюда</a>.");
        }

        public async Task ResetPassword(ResetPasswordViewModel model)
        {
            var user = await _userManager.FindByEmailAsync(model.Email);

            if (user == null)
            {
                throw new NotFoundException("Пользователь не найден");
            }

            var encodeCode = Encoding.UTF8.GetString(WebEncoders.Base64UrlDecode(model.Code));

            var result = await _userManager.ResetPasswordAsync(user, encodeCode, model.Password);

            if (result.Succeeded)
            {
                return;
            }
            foreach (var error in result.Errors)
            {
                throw new BadRequestException(error.Description);
            }

            throw new WentWrongException();
        }

        public async Task<RefreshTokenResponse> RefreshToken(string refresh)
        {
            var principal = await _tokenManager.GetPrincipalFromExpiredTokenAsync(refresh)!;

            if (principal is null) 
                throw new BadRequestException("Недействительный токен");

            var userId = principal.Claims.First(u => u.Type == ClaimTypes.NameIdentifier).Value;

            var user = await _userManager.FindByIdAsync(userId);

            if (user is null)
                throw new NotFoundException("Пользователь не найден");

            if (user.RefreshToken != refresh)
                throw new BadRequestException("Неверный токен обновления");

            var newAccessToken = await _tokenManager.CreateAccessTokenAsync(user, user.Role);
            var newRefreshToken = await _tokenManager.CreateRefreshTokenAsync(userId);

            user.RefreshToken = new JwtSecurityTokenHandler().WriteToken(newRefreshToken);
            user.RefreshTokenExpiryTime = new JwtOptions().RefreshTokenExpires;

            await _userManager.UpdateAsync(user);

            return new RefreshTokenResponse
            {
                access_token = new JwtSecurityTokenHandler().WriteToken(newAccessToken),
                expire = newAccessToken.ValidTo,
                refresh_token = new JwtSecurityTokenHandler().WriteToken(newRefreshToken),
                refresh_token_expire = newRefreshToken.ValidTo
            };
        }

        public async Task Revoke(string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);

            if (user is null) 
                throw new NotFoundException("Пользователь не найден");

            user.RefreshToken = null;
            user.RefreshTokenExpiryTime = default(DateTime);

            var result = await _userManager.UpdateAsync(user);

            if (result.Succeeded)
            {
                return;
            }
            foreach (var error in result.Errors)
            {
                throw new BadRequestException(error.Description);
            }

            return;
        }
    }
}
