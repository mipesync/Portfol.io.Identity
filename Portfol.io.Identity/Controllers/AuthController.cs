using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.WebUtilities;
using Portfol.io.Identity.Common.TokenIssue;
using Portfol.io.Identity.Interfaces;
using Portfol.io.Identity.Models;
using Portfol.io.Identity.ViewModels;
using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Text;
using System.Text.Encodings.Web;

namespace Portfol.io.Identity.Controllers
{
    [ApiController]
    [Produces("application/json")]
    [Route("api/[controller]/[action]")]
    public class AuthController : Controller
    {
        private readonly UserManager<AppUser> _userManager;
        private readonly SignInManager<AppUser> _signInManager;
        private readonly ILogger<AuthController> _logger;
        private readonly IEmailSender _emailSender;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly ITokenManager _tokenManager;

        public AuthController(SignInManager<AppUser> signInManager, ILogger<AuthController> logger,
            UserManager<AppUser> userManager, IEmailSender emailSender, RoleManager<IdentityRole> roleManager, ITokenManager tokenManager)
        {
            _signInManager = signInManager;
            _logger = logger;
            _userManager = userManager;
            _emailSender = emailSender;
            _roleManager = roleManager;
            _tokenManager = tokenManager;
        }

        /// <summary>
        /// Authenticates the user
        /// </summary>
        /// <remarks>
        /// Sample request:
        /// POST /login
        /// {
        ///     username: "usename",
        ///     password: "password",
        ///     rememberMe: bool,
        ///     returnUrl: "return_url"
        /// }
        /// </remarks>
        /// <param name="model"></param>
        /// <returns>Returns
        /// {
        ///     "access_token": "jwt bearer with claims: name - username, name_identifier - userId, role",
        ///     "expires": "DataTime",
        ///     "refresh_token": "string",
        ///     "returnUrl": "string"
        /// }
        /// </returns>
        /// <response code="400">If model is not valid. With JSON message.</response>
        /// <response code="400">If invalid login attempt. With JSON message.</response>
        /// <response code="404">If user not found. With JSON message.</response>
        /// <response code="403">If email not confimed. With JSON message.</response>
        /// <response code="403">If user account locked out. With JSON message.</response>
        /// <response code="200">Success</response>

        [HttpPost("login")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Login([FromBody] LoginViewModel model)
        {
            if (!ModelState.IsValid) return BadRequest(new { message = "Model is not valid." });
            var user = await _userManager.FindByNameAsync(model.Username);

            if (user is null || user.UserName != model.Username)
            {
                return NotFound(new { message = "User not found." });
            }
            if (!user.EmailConfirmed)
            {
                _logger.LogWarning("Email not confimed.");
                return StatusCode((int)HttpStatusCode.Forbidden, new {message = "Email not confimed." });
            }

            var result = await _signInManager.PasswordSignInAsync(user, model.Password, model.RememberMe, false);

            if (result.Succeeded)
            {
                _logger.LogInformation("User logged in.");

                var accessToken = _tokenManager.CreateAccessToken(user, _userManager.GetRolesAsync(user).Result.FirstOrDefault()!);
                var refreshToken = string.Empty;

                user.LockoutEnd = null;
                user.LockoutEnabled = false;

                if (model.RememberMe)
                {
                    refreshToken = _tokenManager.CreateRefreshToken();

                    user.RefreshToken = refreshToken;
                    user.RefreshTokenExpiryTime = JwtOptions.RefreshTokenExpires;
                }

                await _userManager.UpdateAsync(user);

                return Ok(new
                {
                    access_token = new JwtSecurityTokenHandler().WriteToken(accessToken),
                    expires = accessToken.ValidTo,
                    refresh_token = refreshToken,
                    returnUrl = model.ReturnUrl
                });
            }
            if (result.IsLockedOut)
            {
                _logger.LogWarning("User account locked out.");

                user.AccessFailedCount = 0;
                await _userManager.UpdateAsync(user);

                return StatusCode((int)HttpStatusCode.Forbidden, new { message = "User account locked out." });
            }
            else
            {
                user.AccessFailedCount++;

                if(user.AccessFailedCount >= 5)
                {
                    user.LockoutEnabled = true;
                    user.LockoutEnd = DateTime.UtcNow + TimeSpan.FromMinutes(10);
                }

                await _userManager.UpdateAsync(user);

                return BadRequest(new { message = "Invalid login attempt." });
            }
        }

        [HttpPost]
        public async Task Logout()
        {
            //TODO: Разобраться
            await _signInManager.SignOutAsync();
            _logger.LogInformation("User logged out.");
        }

        /// <summary>
        /// Registers a user
        /// </summary>
        /// <remarks>
        /// Sample request:
        /// POST /register
        /// {
        ///     username: "usename",
        ///     email: "email",
        ///     password: "password",
        ///     roleId: "roleId",
        ///     returnUrl: "return_url"
        /// }
        /// </remarks>
        /// <param name="model"></param>
        /// <returns>Returns
        /// {
        ///     "userId": "guid",
        ///     "returnUrl": "string"
        /// }
        /// </returns>
        /// <response code="400">If model is not valid. With JSON message.</response>
        /// <response code="400">If user with this email already exists. With JSON message.</response>
        /// <response code="400">if there were errors during user creation. With JSON message.</response>
        /// <response code="404">If role not found. With JSON message.</response>
        /// <response code="200">Success</response>
        /// <response code="204">If none of the conditions are met.</response>

        [HttpPost("register")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Register([FromBody] RegisterViewModel model)
        {
            if (!ModelState.IsValid) return BadRequest(new { message = "Model is not valid." });

            var user = new AppUser { UserName = model.Username, Email = model.Email, DateOfCreation = DateTime.UtcNow };

            var role = await _roleManager.FindByIdAsync(model.RoleId);

            if (role is null) return NotFound(new {message = "Role not found."});

            var getUser = await _userManager.FindByEmailAsync(model.Email);

            if (getUser is not null) return BadRequest(new { message = $"A user with this email already exists." });

            var result = await _userManager.CreateAsync(user, model.Password);

            await _userManager.AddToRoleAsync(user, role.Name);

            if (result.Succeeded)
            {
                _logger.LogInformation("User created a new account with password.");

                var userId = await _userManager.GetUserIdAsync(user);
                var code = await _userManager.GenerateEmailConfirmationTokenAsync(user);
                code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(code));
                var actionDesc = ControllerContext.ActionDescriptor;
                var callbackUrl = Url.ActionLink(nameof(ConfirmEmail), $"{actionDesc.ControllerName}",
                    values: new { userId = userId, code = code, returnUrl = model.ReturnUrl }, fragment: "api/");

                await _emailSender.SendEmailAsync(model.Email, "Confirm your email",
                    $"Please confirm your account by <a href='{HtmlEncoder.Default.Encode(callbackUrl!)}'>clicking here</a>.");

                if (!_userManager.Options.SignIn.RequireConfirmedAccount)
                {
                    await _signInManager.SignInAsync(user, isPersistent: false);
                    var accessToken = _tokenManager.CreateAccessToken(user, role.Name);
                    return Ok(new
                    {
                        userId = userId,
                        returnUrl = model.ReturnUrl!,
                        access_token = new JwtSecurityTokenHandler().WriteToken(accessToken),
                        expires = accessToken.ValidTo
                    }); ;
                }
                return Ok(new { userId = userId, returnUrl = model.ReturnUrl! });
            }
            foreach (var error in result.Errors)
            {
                return BadRequest(new { message = error.Description });
            }

            return NoContent();
        }

        /// <summary>
        /// When the user is forgot the password
        /// </summary>
        /// <remarks>
        /// Sample request:
        /// POST /forgot_password?email="user_email"
        /// </remarks>
        /// <param name="email"></param>
        /// <response code="400">If model is not valid. With JSON message.</response>
        /// <response code="400">If user does not exist or is not confirmed. With JSON message.</response>
        /// <response code="200">Success</response>

        [HttpPost("forgot_password")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> ForgotPassword(string email)
        {
            if (!ModelState.IsValid) return BadRequest(new { message = "Model is not valid." });

            var user = await _userManager.FindByEmailAsync(email);

            if (user == null || !(await _userManager.IsEmailConfirmedAsync(user)))
            {
                return BadRequest(new { message = "The user does not exist or is not confirmed." });
            }

            var code = await _userManager.GeneratePasswordResetTokenAsync(user);
            code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(code));
            var actionDesc = ControllerContext.ActionDescriptor;
            var callbackUrl = Url.ActionLink(nameof(ResetPassword), $"{actionDesc.ControllerName}",
                values: new { email = email, code = code }, fragment: "api/");

            await _emailSender.SendEmailAsync(
                email,
                "Reset Password",
                $"Please reset your password by <a href='{HtmlEncoder.Default.Encode(callbackUrl!)}'>clicking here</a>.");

            return Ok();
        }

        /// <summary>
        /// Password reset
        /// </summary>
        /// <remarks>
        /// Sample request:
        /// POST /reset_password
        /// {
        ///     email: "email",
        ///     code: "code",
        ///     password: "password"
        /// }
        /// </remarks>
        /// <param name="model"></param>
        /// <returns>Returns
        /// {
        ///     "message": "Password has been reset"
        /// }
        /// </returns>
        /// <response code="400">If model is not valid. With JSON message.</response>
        /// <response code="400">if there were errors during password reset. With JSON message.</response>
        /// <response code="404">If user not found. With JSON message.</response>
        /// <response code="200">Success</response>
        /// <response code="204">If none of the conditions are met.</response>

        [HttpPost("reset_password")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordViewModel model)
        {
            if (!ModelState.IsValid) return BadRequest(new { message = "Model is not valid." });

            var user = await _userManager.FindByEmailAsync(model.Email);

            if (user == null)
            {
                return NotFound(new { message = "User not found." });
            }

            var encodeCode = Encoding.UTF8.GetString(WebEncoders.Base64UrlDecode(model.Code));

            var result = await _userManager.ResetPasswordAsync(user, encodeCode, model.Password);

            if (result.Succeeded)
            {
                return Ok(new { message = "Password has been reset" });
            }
            foreach (var error in result.Errors)
            {
                return BadRequest(new { message = error.Description });
            }

            return NoContent();
        }

        /// <summary>
        /// Confirm email
        /// </summary>
        /// <remarks>
        /// Sample request:
        /// POST /confirm_email?userId="guid"&amp;code="email_confirmation_code"&amp;returnUrl="url"
        /// </remarks>
        /// <returns>Returns
        /// {
        ///     "message" = "Email has been confirmed",
        ///     "returnUrl": "..."
        /// }
        /// </returns>
        /// <response code="400">if an error occurred while email confirmation. With JSON message.</response>
        /// <response code="404">If user not found. With JSON message.</response>
        /// <response code="200">Success</response>
        /// <response code="204">If userId or code is null.</response>

        [HttpPost("confirm_email")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> ConfirmEmail(string userId, string code, string? returnUrl)
        {
            if (userId == null || code == null)
            {
                return NoContent();
            }

            var user = await _userManager.FindByIdAsync(userId);

            if (user == null)
            {
                return NotFound($"User not found.");
            }

            code = Encoding.UTF8.GetString(WebEncoders.Base64UrlDecode(code));

            var result = await _userManager.ConfirmEmailAsync(user, code);

            if (result.Succeeded)
            {
                return Ok(new { message = "Email has been confirmed", returnUrl = returnUrl });
            }
            else return BadRequest(new { message = "Error confirming email" });
        }

        /// <summary>
        /// Resend email confirmation
        /// </summary>
        /// <remarks>
        /// Sample request:
        /// POST /reconfirm_email?email="user email"&amp;returnUrl="url"
        /// </remarks>
        /// <response code="400">If model is not valid. With JSON message.</response>
        /// <response code="400">If user with this email already exists. With JSON message.</response>
        /// <response code="200">Success</response>

        [HttpPost("reconfirm_email")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> ResendEmailConfirmation(string email, string? returnUrl)
        {
            if (!ModelState.IsValid) return BadRequest(new { message = "Model is not valid." });

            var user = await _userManager.FindByEmailAsync(email);

            if (user == null)
            {
                return NotFound(new { message = "User not found." });
            }

            var userId = await _userManager.GetUserIdAsync(user);
            var code = await _userManager.GenerateEmailConfirmationTokenAsync(user);
            code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(code));
            var actionDesc = ControllerContext.ActionDescriptor;
            var callbackUrl = Url.ActionLink(nameof(ConfirmEmail), $"{actionDesc.ControllerName}",
                values: new { userId = userId, code = code, returnUrl = returnUrl }, fragment: "api/");
            await _emailSender.SendEmailAsync(
                email,
                "Confirm your email",
                $"Please confirm your account by <a href='{HtmlEncoder.Default.Encode(callbackUrl!)}'>clicking here</a>.");

            return Ok();
        }

        /// <summary>
        /// Refreshes the access token and refresh token
        /// </summary>
        /// <remarks>
        /// Sample request:
        /// POST /refresh_token
        /// {
        ///     access_token = "string",
        ///     refresh_token: "string"
        /// }
        /// </remarks>
        /// <returns>Returns
        /// {
        ///     "access_token" = "jwt token",
        ///     "expire" = "DateTime",
        ///     "refresh_token" = "string"
        /// }
        /// </returns>
        /// <response code="400">if an error occurred while email confirmation. With JSON message.</response>
        /// <response code="400">if invalid access token. With JSON message.</response>
        /// <response code="400">if invalid access token or refresh token. With JSON message.</response>
        /// <response code="200">Success</response>

        [HttpPost("refresh_token")]
        [Authorize]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> RefreshToken([FromBody] RefreshTokenViewModel model)
        {
            if (!ModelState.IsValid) return BadRequest(new { message = "Model is not valid." });

            var accessToken = model.AccessToken;
            var refreshToken = model.RefreshToken;

            var principal = _tokenManager.GetPrincipalFromExpiredToken(accessToken);

            if (principal is null) return BadRequest(new { message = "Invalid access token." });

            var user = await _userManager.FindByNameAsync(principal.Identity!.Name);

            if  (user is null || user.RefreshToken != refreshToken || user.RefreshTokenExpiryTime <= DateTime.UtcNow)
            {
                return BadRequest(new { message = "Invalid access token or refresh token." });
            }

            var roles = await _userManager.GetRolesAsync(user);

            var newAccessToken = _tokenManager.CreateAccessToken(user, roles.Last());
            var newRefreshToken = _tokenManager.CreateRefreshToken();

            user.RefreshToken = newRefreshToken;
            user.RefreshTokenExpiryTime = JwtOptions.RefreshTokenExpires;

            await _userManager.UpdateAsync(user);

            return Ok(new
            {
                access_token = new JwtSecurityTokenHandler().WriteToken(newAccessToken),
                expire = newAccessToken.ValidTo,
                refresh_token = newRefreshToken
            });
        }

        /// <summary>
        /// Refresh token revocation
        /// </summary>
        /// <remarks>
        /// Sample request:
        /// POST /revoke?userId="user id"
        /// </remarks>
        /// <param name="userId"></param>
        /// <returns></returns>
        /// <response code="200">Succeess</response>
        /// <response code="204">If none of the conditions are met.</response>
        /// <response code="400">if there were errors during password reset. With JSON message.</response>
        /// <response code="404">If user not found. With JSON message.</response>

        [HttpPost("revoke")]
        [Authorize]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Revoke(string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);

            if (user is null) return NotFound(new { message = "User not found." });

            user.RefreshToken = null;
            user.RefreshTokenExpiryTime = default(DateTime);

            var result = await _userManager.UpdateAsync(user);

            if (result.Succeeded)
            {
                return Ok();
            }
            foreach (var error in result.Errors)
            {
                return BadRequest(new { message = error.Description });
            }

            return NoContent();
        }
    }
}
