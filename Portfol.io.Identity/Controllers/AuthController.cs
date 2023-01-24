using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.WebUtilities;
using Portfol.io.Identity.Common.TokenIssue;
using Portfol.io.Identity.Interfaces;
using Portfol.io.Identity.Models;
using Portfol.io.Identity.ViewModels;
using Portfol.io.Identity.ViewModels.ResponseModels;
using Portfol.io.Identity.ViewModels.ResponseModels.AuthResponseModels;
using Swashbuckle.AspNetCore.Annotations;
using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Text;
using System.Text.Encodings.Web;

namespace Portfol.io.Identity.Controllers
{
    [ApiController]
    [Produces("application/json")]
    [Route("api/auth")]
    public class AuthController : BaseController
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

        [HttpGet("oauth-vk")]
        public async Task<IActionResult> OAuthVK()
        {
            var properties = new AuthenticationProperties()
            {
                RedirectUri = "/oauth-vk",
                Items =
                {
                    { "LoginProvider", "VK" },
                },
                AllowRefresh = true
            };

            return Challenge(properties, "VK");
        }

        /// <summary>
        /// Authenticates the user.
        /// </summary>
        /// <remarks> 
        /// Issues an access token and, if "remember me" is true, issues a refresh token. Sample request:
        /// 
        ///     POST: /api/auth/login
        ///     {
        ///         "username": "user",
        ///         "password": "Abcd_123",
        ///         "rememberMe": "true",
        ///         "returnUrl": "http://example.com/catalog"
        ///     }
        /// </remarks>
        /// <param name="model"></param>
        /// <returns>Returns <see cref="LoginResponse"/></returns>
        /// <response code="400">If model is not valid. </response>
        /// <response code="400">If invalid login attempt. </response>
        /// <response code="404">If the user is not found. </response>
        /// <response code="403">If email not confimed. </response>
        /// <response code="403">If user account locked out. </response>
        /// <response code="200">Success</response>

        [HttpPost("login")]
        [SwaggerResponse(statusCode: StatusCodes.Status200OK, type: typeof(LoginResponse))]
        [SwaggerResponse(statusCode: StatusCodes.Status400BadRequest, type: typeof(Error))]
        [SwaggerResponse(statusCode: StatusCodes.Status403Forbidden, type: typeof(Error))]
        [SwaggerResponse(statusCode: StatusCodes.Status404NotFound, type: typeof(Error))]
        public async Task<IActionResult> Login([FromBody] LoginViewModel model)
        {
            if (!ModelState.IsValid) return BadRequest(new Error { Message = "Model is not valid." });
            var user = await _userManager.FindByNameAsync(model.Username);

            if (user is null || user.UserName != model.Username)
            {
                return NotFound(new Error { Message = "User not found." });
            }
            if (!user.EmailConfirmed)
            {
                _logger.LogWarning("Email not confimed.");
                return StatusCode((int)HttpStatusCode.Forbidden, new Error { Message = "Email not confimed." });
            }

            var result = await _signInManager.PasswordSignInAsync(user, model.Password, model.RememberMe, false);

            if (result.Succeeded)
            {
                _logger.LogInformation("User logged in.");

                var accessToken = await _tokenManager.CreateAccessTokenAsync(user, _userManager.GetRolesAsync(user).Result.FirstOrDefault()!);
                var refreshToken = string.Empty;

                user.LockoutEnd = null;
                user.LockoutEnabled = false;

                if (model.RememberMe)
                {
                    refreshToken = await _tokenManager.CreateRefreshTokenAsync();

                    user.RefreshToken = refreshToken;
                    user.RefreshTokenExpiryTime = new JwtOptions().RefreshTokenExpires;
                }

                await _userManager.UpdateAsync(user);

                return Ok(new LoginResponse
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

                return StatusCode((int)HttpStatusCode.Forbidden, new Error { Message = "User account locked out." });
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

                return BadRequest(new Error { Message = "Invalid login attempt." });
            }
        }

        /*[HttpPost]
        public async Task Logout()
        {
            //TODO: Разобраться
            await _signInManager.SignOutAsync();
            _logger.LogInformation("User logged out.");
        }*/

        /// <summary>
        /// Registers a user
        /// </summary>
        /// <remarks>
        /// Password must be at least 8 characters, contains non-alphanumeric, digit and uppercase. 
        /// The RoleId field is taken from the GET: ".../get_roles" request. There are two public roles: employee and employer. 
        /// Sample request:
        /// 
        ///     POST: /api/auth/register
        ///     {
        ///         "username": "user",
        ///         "email": "user@example.com",
        ///         "password": "Abcd_123",
        ///         "roleId": "4C2C522E-F785-4EB4-8ED7-260861453330",
        ///         "returnUrl": "http://example.com/catalog"
        ///     }
        /// </remarks>
        /// <param name="model"></param>
        /// <returns>Returns <see cref="RegisterResponse"/></returns>
        /// <response code="400">If model is not valid. </response>
        /// <response code="400">If user with this email already exists. </response>
        /// <response code="400">If there were errors. </response>
        /// <response code="404">If role not found. </response>
        /// <response code="200">Success</response>
        /// <response code="204">If none of the conditions are met.</response>

        [HttpPost("register")]
        [SwaggerResponse(statusCode: StatusCodes.Status200OK, type: typeof(RegisterResponse))]
        [SwaggerResponse(statusCode: StatusCodes.Status204NoContent, type: null)]
        [SwaggerResponse(statusCode: StatusCodes.Status400BadRequest, type: typeof(Error))]
        [SwaggerResponse(statusCode: StatusCodes.Status404NotFound, type: typeof(Error))]
        public async Task<IActionResult> Register([FromBody] RegisterViewModel model)
        {
            if (!ModelState.IsValid) return BadRequest(new Error { Message = "Model is not valid." });

            var user = new AppUser { UserName = model.Username, Email = model.Email, DateOfCreation = DateTime.UtcNow };

            var role = await _roleManager.FindByIdAsync(model.RoleId);

            if (role is null) return NotFound(new Error { Message = "Role not found."});

            var getUser = await _userManager.FindByEmailAsync(model.Email);

            if (getUser is not null) return BadRequest(new Error { Message = $"A user with this email already exists." });

            var result = await _userManager.CreateAsync(user, model.Password);

            await _userManager.AddToRoleAsync(user, role.Name);

            if (result.Succeeded)
            {
                _logger.LogInformation("User created a new account with password.");

                var userId = await _userManager.GetUserIdAsync(user);
                var code = await _userManager.GenerateEmailConfirmationTokenAsync(user);
                code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(code));
                var callbackUrl = $"{UrlRaw}/confirm_email?userId={userId}&code={code}&returnUrl={model.ReturnUrl}";

                await _emailSender.SendEmailAsync(model.Email, "Confirm your email",
                    $"Please confirm your account by <a href='{HtmlEncoder.Default.Encode(callbackUrl!)}'>clicking here</a>.");

                if (!_userManager.Options.SignIn.RequireConfirmedAccount)
                {
                    await _signInManager.SignInAsync(user, isPersistent: false);
                    var accessToken = await _tokenManager.CreateAccessTokenAsync(user, role.Name);
                    return Ok(new 
                    {
                        userId = userId,
                        returnUrl = model.ReturnUrl!,
                        access_token = new JwtSecurityTokenHandler().WriteToken(accessToken),
                        expires = accessToken.ValidTo
                    });
                }
                return Ok(new RegisterResponse { userId = userId, returnUrl = model.ReturnUrl! });
            }
            foreach (var error in result.Errors)
            {
                return BadRequest(new Error { Message = error.Description });
            }

            return NoContent();
        }

        /// <summary>
        /// When the user is forgot the password.
        /// </summary>
        /// <remarks>
        /// Sends a confirmation email. Sample request: 
        /// 
        ///     GET: /api/auth/forgot_password?email=user@example.com
        /// </remarks>
        /// <param name="email">The user's email address to which the confirmation email will be sent.</param>
        /// <response code="400">If model is not valid. </response>
        /// <response code="400">If user does not exist or is not confirmed. </response>
        /// <response code="200">Success</response>

        [HttpGet("forgot_password")]
        [SwaggerResponse(statusCode: StatusCodes.Status200OK, type: null)]
        [SwaggerResponse(statusCode: StatusCodes.Status400BadRequest, type: typeof(Error))]
        public async Task<IActionResult> ForgotPassword(string email)
        {
            if (!ModelState.IsValid) return BadRequest(new Error { Message = "Model is not valid." });

            var user = await _userManager.FindByEmailAsync(email);

            if (user == null || !(await _userManager.IsEmailConfirmedAsync(user)))
            {
                return BadRequest(new Error { Message = "The user does not exist or is not confirmed." });
            }

            var code = await _userManager.GeneratePasswordResetTokenAsync(user);
            code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(code));
            var actionDesc = ControllerContext.ActionDescriptor;
            var callbackUrl = $"{UrlRaw}/reset_password?email={email}&code={code}";

            await _emailSender.SendEmailAsync(
                email,
                "Reset Password",
                $"Please reset your password by <a href='{HtmlEncoder.Default.Encode(callbackUrl!)}'>clicking here</a>.");

            return Ok();
        }

        /// <summary>
        /// Resets the password.
        /// </summary>
        /// <remarks>
        /// Accepts the code that was sent to the mail. 
        /// The page address must contain ".../reset_password". Example: http://example.com/reset_password 
        /// Sample request:
        /// 
        ///     POST: /api/auth/reset_password
        ///     {
        ///         "email": "user@example.com",
        ///         "code": "your code",
        ///         "password": "Abcd_123"
        ///     }
        /// </remarks>
        /// <param name="model"></param>
        /// <response code="400">If model is not valid. </response>
        /// <response code="400">if there were errors during password reset. </response>
        /// <response code="404">If the user is not found. </response>
        /// <response code="200">Success</response>
        /// <response code="204">If none of the conditions are met.</response>

        [HttpPost("reset_password")]
        [SwaggerResponse(statusCode: StatusCodes.Status200OK, type: null)]
        [SwaggerResponse(statusCode: StatusCodes.Status204NoContent, type: null)]
        [SwaggerResponse(statusCode: StatusCodes.Status400BadRequest, type: typeof(Error))]
        [SwaggerResponse(statusCode: StatusCodes.Status404NotFound, type: typeof(Error))]
        public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordViewModel model)
        {
            if (!ModelState.IsValid) return BadRequest(new Error { Message = "Model is not valid." });

            var user = await _userManager.FindByEmailAsync(model.Email);

            if (user == null)
            {
                return NotFound(new Error { Message = "User not found." });
            }

            var encodeCode = Encoding.UTF8.GetString(WebEncoders.Base64UrlDecode(model.Code));

            var result = await _userManager.ResetPasswordAsync(user, encodeCode, model.Password);

            if (result.Succeeded)
            {
                return Ok();
            }
            foreach (var error in result.Errors)
            {
                return BadRequest(new Error { Message = error.Description });
            }

            return NoContent();
        }

        /// <summary>
        /// Email confirmation.
        /// </summary>
        /// <remarks>
        /// The page address must contain ".../confirm_email". Example: http://example.com/confirm_email
        /// Sample request:
        /// 
        ///     POST: /api/auth/confirm_email?userId=4C2C522E-F785-4EB4-8ED7-260861453330&amp;code=your_code&amp;returnUrl=http://example.com/catalog
        /// </remarks>
        /// <returns>Returns <see cref="ConfirmEmailResponse"/></returns>
        /// <response code="400">if an error occurred while email confirmation.</response>
        /// <response code="404">If the user is not found.</response>
        /// <response code="200">Success</response>
        /// <response code="204">If userId or code is null.</response>

        [HttpPost("confirm_email")]
        [SwaggerResponse(statusCode: StatusCodes.Status200OK, type: typeof(ConfirmEmailResponse))]
        [SwaggerResponse(statusCode: StatusCodes.Status204NoContent, type: null)]
        [SwaggerResponse(statusCode: StatusCodes.Status400BadRequest, type: typeof(Error))]
        [SwaggerResponse(statusCode: StatusCodes.Status404NotFound, type: typeof(Error))]
        public async Task<IActionResult> ConfirmEmail(string userId, string code, string? returnUrl)
        {
            if (userId == null || code == null)
            {
                return NoContent();
            }

            var user = await _userManager.FindByIdAsync(userId);

            if (user == null)
            {
                return NotFound(new Error { Message = "User not found." });
            }

            code = Encoding.UTF8.GetString(WebEncoders.Base64UrlDecode(code));

            var result = await _userManager.ConfirmEmailAsync(user, code);

            if (result.Succeeded)
            {
                return Ok(new ConfirmEmailResponse 
                {
                    message = "Email has been confirmed",
                    returnUrl = returnUrl 
                });
            }
            else return BadRequest(new Error { Message = "Error confirming email" });
        }

        /// <summary>
        /// Resends the confirmation email.
        /// </summary>
        /// <remarks>
        /// Resends the confirmation email if the previous one was not delivered.
        /// Sample request:
        /// 
        ///     GET: /api/auth/reconfirm_email?email=user@example.com&amp;returnUrl=http://example.com/catalog
        /// </remarks>
        /// <param name="email">The user's email address to which the confirmation email will be sent.</param>
        /// <param name="returnUrl">The return URL to which the user will be returned after confirmation of the mail.</param>
        /// <response code="400">If model is not valid.</response>
        /// <response code="404">If the user is not found.</response>
        /// <response code="200">Success</response>

        [HttpGet("reconfirm_email")]
        [SwaggerResponse(statusCode: StatusCodes.Status200OK, type: null)]
        [SwaggerResponse(statusCode: StatusCodes.Status400BadRequest, type: typeof(Error))]
        [SwaggerResponse(statusCode: StatusCodes.Status404NotFound, type: typeof(Error))]
        public async Task<IActionResult> ResendEmailConfirmation(string email, string? returnUrl)
        {
            if (!ModelState.IsValid) return BadRequest(new Error { Message = "Model is not valid." });

            var user = await _userManager.FindByEmailAsync(email);

            if (user == null)
            {
                return NotFound(new Error { Message = "User not found." });
            }

            var userId = await _userManager.GetUserIdAsync(user);
            var code = await _userManager.GenerateEmailConfirmationTokenAsync(user);
            code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(code));
            var callbackUrl = $"{UrlRaw}/confirm_email?userId={userId}&code={code}&returnUrl={returnUrl}";
            await _emailSender.SendEmailAsync(
                email,
                "Confirm your email",
                $"Please confirm your account by <a href='{HtmlEncoder.Default.Encode(callbackUrl!)}'>clicking here</a>.");

            return Ok();
        }

        /// <summary>
        /// Refreshes the access token and refresh token.
        /// </summary>
        /// <remarks>
        /// Sample request:
        /// 
        ///     PUT: /api/auth/refresh_token
        ///     {
        ///         "access_token": "jwt token",
        ///         "refresh_token": "your refresh token"
        ///     }
        /// </remarks>
        /// <returns>Returns <see cref="RefreshTokenResponse"/></returns>
        /// <response code="400">if an error occurred while email confirmation. </response>
        /// <response code="400">if invalid access token. </response>
        /// <response code="400">if invalid access token or refresh token. </response>
        /// <response code="200">Success</response>

        [HttpPut("refresh_token")]
        [SwaggerResponse(statusCode: StatusCodes.Status200OK, type: typeof(RefreshTokenResponse))]
        [SwaggerResponse(statusCode: StatusCodes.Status400BadRequest, type: typeof(Error))]
        public async Task<IActionResult> RefreshToken([FromBody] RefreshTokenViewModel model)
        {
            if (!ModelState.IsValid) return BadRequest(new Error { Message = "Model is not valid." });

            var accessToken = model.AccessToken;
            var refreshToken = model.RefreshToken;

            var principal = await _tokenManager.GetPrincipalFromExpiredTokenAsync(accessToken)!;

            if (principal is null) return BadRequest(new Error { Message = "Invalid access token." });

            var user = await _userManager.FindByNameAsync(principal.Identity!.Name);

            if  (user is null || user.RefreshToken != refreshToken || user.RefreshTokenExpiryTime <= DateTime.UtcNow)
            {
                return BadRequest(new Error { Message = "Invalid access token or refresh token." });
            }

            var roles = await _userManager.GetRolesAsync(user);

            var newAccessToken = await _tokenManager.CreateAccessTokenAsync(user, roles.Last());
            var newRefreshToken = await _tokenManager.CreateRefreshTokenAsync();

            user.RefreshToken = newRefreshToken;
            user.RefreshTokenExpiryTime = new JwtOptions().RefreshTokenExpires;

            await _userManager.UpdateAsync(user);

            return Ok(new RefreshTokenResponse
            {
                access_token = new JwtSecurityTokenHandler().WriteToken(newAccessToken),
                expire = newAccessToken.ValidTo,
                refresh_token = newRefreshToken
            });
        }

        /// <summary>
        /// Refresh token revocation.
        /// </summary>
        /// <remarks>
        /// Sample request:
        /// 
        ///     DELETE: /api/auth/revoke?userId=4C2C522E-F785-4EB4-8ED7-260861453330
        /// </remarks>
        /// <param name="userId">Id of the user to revoke the refresh token from.</param>
        /// <response code="200">Succeess</response>
        /// <response code="204">If none of the conditions are met.</response>
        /// <response code="400">if there were errors during password reset. </response>
        /// <response code="404">If the user is not found. </response>

        [HttpDelete("revoke")]
        [SwaggerResponse(statusCode: StatusCodes.Status200OK, type: null)]
        [SwaggerResponse(statusCode: StatusCodes.Status204NoContent, type: typeof(Error))]
        [SwaggerResponse(statusCode: StatusCodes.Status400BadRequest, type: typeof(Error))]
        [SwaggerResponse(statusCode: StatusCodes.Status404NotFound, type: typeof(Error))]
        public async Task<IActionResult> Revoke(string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);

            if (user is null) return NotFound(new Error { Message = "User not found." });

            user.RefreshToken = null;
            user.RefreshTokenExpiryTime = default(DateTime);

            var result = await _userManager.UpdateAsync(user);

            if (result.Succeeded)
            {
                return Ok();
            }
            foreach (var error in result.Errors)
            {
                return BadRequest(new Error { Message = error.Description });
            }

            return NoContent();
        }
    }
}
